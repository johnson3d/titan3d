using System;
using System.Collections.Generic;
using System.Text.Json;
using EngineNS.Bricks.Terrain.CDLOD;

namespace EngineNS.Plugins.MCPServer
{
    /// <summary>
    /// 地形编辑的调试通路。
    ///
    /// 设计原则是每个工具都要能【独立判定一层通路】, 这样一次调用就能把故障范围切一半,
    /// 而不是靠肉眼看画面猜。地形编辑从改数据到出画面要穿过四层:
    ///
    ///   1. CPU 源数据   UTerrainLevelData.SourceHeightMap  (float, 权威源)
    ///   2. 物理侧       PxHeightfieldSamples               (short 量化, GetAltitude 读它)
    ///   3. GPU 纹理     HeightMap / NormalMap              (局部上传或整层重建)
    ///   4. 屏幕像素     RVT atlas -> shader                (标脏了才可见)
    ///
    /// 1 和 2 这里直接读, 而且是【分开读】的 —— terrain_sample_height 同时返回两个值,
    /// 一眼就能看出是数据层没改到还是下游同步断了。
    /// 3 和 4 读不到 (GPU 资源没有回读通路), 交给 capture_renderdoc_frame 抓帧后用
    /// renderdoc MCP 的 read_texture_pixels / save_texture 去看。分工就是这么切的。
    /// </summary>
    public partial class TtMCPServerPlugin
    {
        #region locate
        private class FTerrainCtx
        {
            public Editor.Forms.TtSceneEditor SceneEditor;
            public TtTerrainNode Terrain;
            public string Error;
        }

        /// <summary>
        /// 找当前场景编辑器里的地形节点。必须在主线程上调用。
        /// </summary>
        private static FTerrainCtx LocateTerrain()
        {
            var result = new FTerrainCtx();

            var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
            if (mainEditor == null)
            {
                result.Error = "the running SlateApplication is not TtMainEditorApplication; " +
                    "terrain editing tools only work inside the main editor";
                return result;
            }

            var mgr = mainEditor.AssetEditorManager;
            var sceneEditor = mgr.CurrentActiveEditor as Editor.Forms.TtSceneEditor;
            if (sceneEditor == null)
            {
                foreach (var i in mgr.OpenedEditors)
                {
                    sceneEditor = i as Editor.Forms.TtSceneEditor;
                    if (sceneEditor != null)
                        break;
                }
            }
            if (sceneEditor == null)
            {
                result.Error = "no scene editor is open; double click a .scene asset first " +
                    "(e.g. content/tutorials/terrain/terrain_scene.scene)";
                return result;
            }
            result.SceneEditor = sceneEditor;

            var scene = sceneEditor.Scene;
            if (scene == null)
            {
                result.Error = "the scene editor's Scene is null (probably still loading)";
                return result;
            }

            var terrain = scene.FindFirstChild<TtTerrainNode>(null, true);
            if (terrain == null)
            {
                result.Error = $"scene '{scene.AssetName}' contains no TtTerrainNode";
                return result;
            }
            result.Terrain = terrain;
            return result;
        }

        /// <summary>
        /// 解析目标世界坐标。给了 worldX/worldZ 就用它, 否则退回相机位置, 相机也不在地形上时
        /// 退回第一个已加载 level 的中心。
        /// </summary>
        private static bool ResolveTerrainProbe(FTerrainCtx ctx, double worldX, double worldZ, bool hasExplicit,
            out DVector3 pos, out string how)
        {
            pos = DVector3.Zero;
            how = "";
            var terrain = ctx.Terrain;

            if (hasExplicit)
            {
                pos = new DVector3(worldX, 0.0, worldZ);
                how = "explicit";
                return true;
            }

            var policy = ctx.SceneEditor.PreviewViewport?.RenderPolicy;
            if (policy != null && policy.DefaultCamera != null)
            {
                var camPos = policy.DefaultCamera.GetPosition();
                if (terrain.GetLevelDataAtWorld(in camPos, out _, out _) != null)
                {
                    pos = camPos;
                    how = "camera";
                    return true;
                }
            }

            if (terrain.Levels == null)
                return false;
            for (int z = 0; z < terrain.NumOfLevelZ; z++)
            {
                for (int x = 0; x < terrain.NumOfLevelX; x++)
                {
                    if (terrain.Levels[z, x]?.LevelData == null)
                        continue;
                    var basePos = terrain.Placement.AbsTransform.mPosition;
                    pos = new DVector3(
                        basePos.X + ((double)x + 0.5) * terrain.LevelSize,
                        basePos.Y,
                        basePos.Z + ((double)z + 0.5) * terrain.LevelSize);
                    how = $"center of level({x},{z})";
                    return true;
                }
            }
            return false;
        }
        #endregion

        [Bricks.AIGC.TtMCPTool("terrain_get_info",
            "Reports terrain editing environment and per-level state for the terrain in the open scene. " +
            "Read this FIRST before any terrain edit debugging: it gives the world-space scale " +
            "(gridSize / levelSize) needed to pick a brush radius that is actually visible, and tells " +
            "you which levels are editable.",
            returnDescription: "{playMode: string, isTerrainEditSupported: boolean, featureUseRVT: boolean - " +
            "when true a local upload is invisible unless the RVT page is marked dirty, sceneName: string, " +
            "terrain: {nodeName, numOfLevelX, numOfLevelZ, levelSize, gridSize, patchSide, patchSize, " +
            "heightMapTexels, placement:{x,y,z}}, brushHint: {suggestedRadius, suggestedStrength, note} - " +
            "radius is in WORLD units not texels, camera: {x,y,z, isOverTerrain}, loadedLevelCount, " +
            "editableLevelCount, levels: [{x, z, isEditable, sourceHeightMap, sourcePixelsAlive - false " +
            "means the buffer was disposed and is a hollow shell, heightMin, heightMax, isInEditSession, " +
            "hasPhyActor, heightfield}], error}")]
        public static string TerrainGetInfo(
            [Bricks.AIGC.TtMCPParameter("Maximum number of levels to detail")] double levelLimit = 8)
        {
            object payload = null;
            int limit = Math.Max(1, (int)levelLimit);

            var ok = TtMainThreadDispatcher.Invoke(() =>
            {
                var ctx = LocateTerrain();

                var common = new Dictionary<string, object>
                {
                    ["playMode"] = TtEngine.Instance.PlayMode.ToString(),
                    ["isTerrainEditSupported"] = UTerrainLevelData.IsTerrainEditSupported,
                    ["featureUseRVT"] = TtEngine.Instance.Config.Feature_UseRVT,
                };
                if (ctx.Terrain == null)
                {
                    common["error"] = ctx.Error;
                    payload = common;
                    return;
                }

                var terrain = ctx.Terrain;
                common["sceneName"] = ctx.SceneEditor.Scene.AssetName?.ToString();

                var placement = terrain.Placement.AbsTransform.mPosition;
                common["terrain"] = new
                {
                    nodeName = terrain.NodeName,
                    numOfLevelX = terrain.NumOfLevelX,
                    numOfLevelZ = terrain.NumOfLevelZ,
                    levelSize = terrain.LevelSize,
                    gridSize = terrain.GridSize,
                    patchSide = terrain.PatchSide,
                    patchSize = terrain.PatchSize,
                    heightMapTexels = terrain.GridSize > 0.0f ? (int)(terrain.LevelSize / terrain.GridSize) : 0,
                    placement = new { x = placement.X, y = placement.Y, z = placement.Z },
                };

                // 半径单位是世界单位而不是 texel, 很容易给小了 —— 一个 level 宽 levelSize,
                // 半径给到 levelSize 的 1/10 才在屏幕上有明显一块。
                common["brushHint"] = new
                {
                    suggestedRadius = terrain.LevelSize * 0.1f,
                    suggestedStrength = terrain.LevelSize * 0.1f,
                    note = $"Radius/Strength are in WORLD units. One level spans {terrain.LevelSize} units " +
                           $"and one height texel is {terrain.GridSize} units. A radius of 20 covers only " +
                           $"{(terrain.LevelSize > 0 ? 40.0f / terrain.LevelSize * 100.0f : 0.0f):F1}% of a " +
                           "level's width and is easy to miss on screen.",
                };

                var policy = ctx.SceneEditor.PreviewViewport?.RenderPolicy;
                if (policy?.DefaultCamera != null)
                {
                    var camPos = policy.DefaultCamera.GetPosition();
                    common["camera"] = new
                    {
                        x = camPos.X,
                        y = camPos.Y,
                        z = camPos.Z,
                        isOverTerrain = terrain.GetLevelDataAtWorld(in camPos, out _, out _) != null,
                    };
                }

                if (terrain.Levels == null)
                {
                    common["error"] = "terrain.Levels is null, the terrain is not initialized yet";
                    payload = common;
                    return;
                }

                int loaded = 0;
                int editable = 0;
                var levels = new List<object>();
                for (int z = 0; z < terrain.NumOfLevelZ; z++)
                {
                    for (int x = 0; x < terrain.NumOfLevelX; x++)
                    {
                        var ld = terrain.Levels[z, x]?.LevelData;
                        if (ld == null)
                            continue;
                        loaded++;
                        if (ld.IsEditable)
                            editable++;
                        if (levels.Count >= limit)
                            continue;

                        levels.Add(new
                        {
                            x,
                            z,
                            isEditable = ld.IsEditable,
                            sourceHeightMap = ld.SourceHeightMap != null
                                ? $"{ld.SourceHeightMap.Width}x{ld.SourceHeightMap.Height}" : null,
                            sourcePixelsAlive = ld.SourceHeightMap?.SuperPixels != null,
                            heightMin = ld.HeightMapMinHeight,
                            heightMax = ld.HeightMapMaxHeight,
                            isInEditSession = ld.IsInEditSession,
                            hasPhyActor = ld.PhyActor != null,
                            heightfield = $"{ld.HeightfieldWidth}x{ld.HeightfieldHeight}",
                        });
                    }
                }
                common["loadedLevelCount"] = loaded;
                common["editableLevelCount"] = editable;
                common["levels"] = levels;
                if (loaded > 0 && editable == 0)
                {
                    common["error"] = "levels are loaded but none is editable. Either PlayMode is not " +
                        "Editor, or SourceHeightMap was disposed (check sourcePixelsAlive).";
                }
                payload = common;
            });

            if (ok == false)
                return FailJson("timed out waiting for the engine main thread");
            return JsonSerializer.Serialize(payload);
        }

        [Bricks.AIGC.TtMCPTool("terrain_sample_height",
            "Samples terrain height at a world position through TWO independent paths and returns both: " +
            "the CPU authoring buffer (SourceHeightMap, float) and the physics heightfield " +
            "(PxHeightfieldSamples, short-quantized, what GetAltitude and raycasts see). " +
            "Comparing them isolates layers: if cpuHeight changed but physicsAltitude did not, the edit " +
            "never propagated downstream; if both changed but the screen did not, the problem is the GPU " +
            "upload or RVT dirty marking.",
            returnDescription: "{worldPos:{x,z}, probeSource: string, level:{x,z}, localX, localZ, " +
            "texel:{x,y}, cpuHeight: number - SourceHeightMap value, level-relative, " +
            "physicsAltitude: number - absolute world Y from PxHeightfieldSamples, " +
            "encodeRange:{min,max}, isInEditSession, error}")]
        public static string TerrainSampleHeight(
            [Bricks.AIGC.TtMCPParameter("World X. Leave both X and Z at 0 to probe under the camera")] double worldX = 0,
            [Bricks.AIGC.TtMCPParameter("World Z. Leave both X and Z at 0 to probe under the camera")] double worldZ = 0)
        {
            object payload = null;
            bool hasExplicit = worldX != 0.0 || worldZ != 0.0;

            var ok = TtMainThreadDispatcher.Invoke(() =>
            {
                var ctx = LocateTerrain();
                if (ctx.Terrain == null)
                {
                    payload = new { error = ctx.Error };
                    return;
                }
                if (ResolveTerrainProbe(ctx, worldX, worldZ, hasExplicit, out var pos, out var how) == false)
                {
                    payload = new { error = "no loaded level found; move the camera near the terrain " +
                        "so streaming loads it, or pass explicit worldX/worldZ" };
                    return;
                }

                var terrain = ctx.Terrain;
                var ld = terrain.GetLevelDataAtWorld(in pos, out var localX, out var localZ);
                if (ld == null)
                {
                    payload = new { worldPos = new { x = pos.X, z = pos.Z }, probeSource = how,
                        error = "the probe position is not inside any loaded level" };
                    return;
                }

                payload = BuildTerrainSample(terrain, ld, pos, localX, localZ, how);
            });

            if (ok == false)
                return FailJson("timed out waiting for the engine main thread");
            return JsonSerializer.Serialize(payload);
        }

        private static object BuildTerrainSample(TtTerrainNode terrain, UTerrainLevelData ld,
            DVector3 pos, float localX, float localZ, string how)
        {
            float cpuHeight = float.NaN;
            int texX = 0, texY = 0;
            if (ld.IsEditable)
            {
                texX = Math.Clamp((int)(localX / terrain.GridSize), 0, ld.SourceHeightMap.Width - 1);
                texY = Math.Clamp((int)(localZ / terrain.GridSize), 0, ld.SourceHeightMap.Height - 1);
                cpuHeight = ld.SourceHeightMap.GetPixel<float>(texX, texY);
            }

            return new
            {
                worldPos = new { x = pos.X, z = pos.Z },
                probeSource = how,
                localX,
                localZ,
                texel = new { x = texX, y = texY },
                isEditable = ld.IsEditable,
                cpuHeight,
                // GetAltitude 走 PxHeightfieldSamples, 是完全独立的第二条路径。
                physicsAltitude = terrain.GetAltitude(pos.X, pos.Z, false),
                encodeRange = new { min = ld.HeightMapMinHeight, max = ld.HeightMapMaxHeight },
                isInEditSession = ld.IsInEditSession,
            };
        }

        [Bricks.AIGC.TtMCPTool("terrain_apply_brush",
            "Applies one height brush stroke on the engine main thread and reports exactly what changed, " +
            "so the result can be judged without looking at the screen. " +
            "KEY READING: if heightDelta is ~0 the data layer never applied the stroke (a coordinate or " +
            "weight bug, nothing to do with the GPU). If heightDelta matches strength but the screen does " +
            "not change, the problem is the partial GPU upload or RVT dirty marking. " +
            "Also note wasInEditSession: when it is false this stroke triggered a FULL level rebuild, so a " +
            "visible change does NOT prove the partial-upload path works. Call twice and judge the second.",
            returnDescription: "{applied: boolean, tool, radius, strength, worldPos:{x,z}, probeSource, " +
            "level:{x,z}, texel:{x,y}, wasInEditSession: boolean - false means a full rebuild happened, " +
            "before:{cpuHeight, physicsAltitude, encodeRange}, after:{cpuHeight, physicsAltitude, " +
            "encodeRange}, heightDelta: number, physicsDelta: number, verdict: string - " +
            "plain reading of the numbers, error}")]
        public static string TerrainApplyBrush(
            [Bricks.AIGC.TtMCPParameter("Brush tool: Raise, Lower, Smooth or Flatten")] string tool = "Raise",
            [Bricks.AIGC.TtMCPParameter("Brush radius in WORLD units (not texels). See terrain_get_info brushHint")] double radius = 200,
            [Bricks.AIGC.TtMCPParameter("Height delta per stroke in world units for Raise/Lower; 0-1 blend weight for Smooth/Flatten. " +
                "Leave at -1 to take the per-tool default (300 for Raise/Lower, 1.0 for Smooth/Flatten)")] double strength = -1,
            [Bricks.AIGC.TtMCPParameter("Edge softness 0-1. 0 is a hard edge")] double falloff = 0.5,
            [Bricks.AIGC.TtMCPParameter("Target height for the Flatten tool, level-relative")] double targetHeight = 0,
            [Bricks.AIGC.TtMCPParameter("World X. Leave both X and Z at 0 to stroke under the camera")] double worldX = 0,
            [Bricks.AIGC.TtMCPParameter("World Z. Leave both X and Z at 0 to stroke under the camera")] double worldZ = 0,
            [Bricks.AIGC.TtMCPParameter("Rebuild the physics heightfield. Set false to keep the stroke cheap")] bool rebuildPhysics = true)
        {
            if (Enum.TryParse<ETerrainBrushTool>(tool, true, out var brushTool) == false)
                return FailJson($"unknown tool '{tool}', expected Raise, Lower, Smooth or Flatten");

            object payload = null;
            bool hasExplicit = worldX != 0.0 || worldZ != 0.0;

            var param = FTerrainBrushParam.Default;
            param.Tool = brushTool;
            param.Radius = (float)radius;
            // Strength 的语义按工具分流: Raise/Lower 是世界单位的高度增量, Flatten/Smooth 是
            // clamp 到 0~1 的混合权重。用同一个默认值 300 会让 Flatten/Smooth 处处饱和成 1,
            // falloff 完全失效, 变成硬边一刀切。
            bool isBlendTool = brushTool == ETerrainBrushTool.Flatten || brushTool == ETerrainBrushTool.Smooth;
            if (strength < 0.0)
                strength = isBlendTool ? 1.0 : 300.0;
            param.Strength = (float)strength;
            param.Falloff = (float)falloff;
            param.TargetHeight = (float)targetHeight;

            var ok = TtMainThreadDispatcher.Invoke(() =>
            {
                var ctx = LocateTerrain();
                if (ctx.Terrain == null)
                {
                    payload = new { applied = false, error = ctx.Error };
                    return;
                }
                if (ResolveTerrainProbe(ctx, worldX, worldZ, hasExplicit, out var pos, out var how) == false)
                {
                    payload = new { applied = false, error = "no loaded level found; move the camera near " +
                        "the terrain so streaming loads it, or pass explicit worldX/worldZ" };
                    return;
                }

                var terrain = ctx.Terrain;
                var ld = terrain.GetLevelDataAtWorld(in pos, out var localX, out var localZ);
                if (ld == null)
                {
                    payload = new { applied = false, probeSource = how,
                        error = "the stroke position is not inside any loaded level" };
                    return;
                }
                if (ld.IsEditable == false)
                {
                    payload = new { applied = false, probeSource = how,
                        error = "level is not editable, the stroke would be silently ignored. " +
                                "Run terrain_get_info to see why (PlayMode / sourcePixelsAlive)." };
                    return;
                }

                int texX = Math.Clamp((int)(localX / terrain.GridSize), 0, ld.SourceHeightMap.Width - 1);
                int texY = Math.Clamp((int)(localZ / terrain.GridSize), 0, ld.SourceHeightMap.Height - 1);

                float beforeCpu = ld.SourceHeightMap.GetPixel<float>(texX, texY);
                double beforePhy = terrain.GetAltitude(pos.X, pos.Z, false);
                float beforeMin = ld.HeightMapMinHeight;
                float beforeMax = ld.HeightMapMaxHeight;
                bool wasInSession = ld.IsInEditSession;

                terrain.ApplyHeightBrushAtWorld(in pos, in param, true, rebuildPhysics);

                float afterCpu = ld.SourceHeightMap.GetPixel<float>(texX, texY);
                double afterPhy = terrain.GetAltitude(pos.X, pos.Z, false);

                float delta = afterCpu - beforeCpu;
                string verdict;
                if (Math.Abs(delta) < 0.0001f)
                {
                    verdict = "DATA LAYER DID NOT CHANGE. The brush never touched the center texel. " +
                        "Likely the radius is smaller than one grid cell, or the coordinate/weight math " +
                        "in ApplyHeightBrush is wrong. This is a CPU-side bug, unrelated to the GPU.";
                }
                else if (wasInSession == false)
                {
                    verdict = "Data layer OK, but this stroke also triggered a FULL LEVEL REBUILD " +
                        "(BeginEditSession refroze the encode range). A visible change now does NOT prove " +
                        "the partial-upload path works. Call terrain_apply_brush again and judge that one.";
                }
                else
                {
                    verdict = "Data layer OK and this was a PURE PARTIAL UPLOAD (already in an edit " +
                        "session, no full rebuild). If the screen still does not change, the fault is in " +
                        "the partial GPU upload or the RVT dirty marking. Capture a frame with " +
                        "capture_renderdoc_frame and inspect the height texture to narrow it down.";
                }

                payload = new
                {
                    applied = true,
                    tool = brushTool.ToString(),
                    radius = param.Radius,
                    strength = param.Strength,
                    worldPos = new { x = pos.X, z = pos.Z },
                    probeSource = how,
                    localX,
                    localZ,
                    texel = new { x = texX, y = texY },
                    wasInEditSession = wasInSession,
                    before = new
                    {
                        cpuHeight = beforeCpu,
                        physicsAltitude = beforePhy,
                        encodeRange = new { min = beforeMin, max = beforeMax },
                    },
                    after = new
                    {
                        cpuHeight = afterCpu,
                        physicsAltitude = afterPhy,
                        encodeRange = new { min = ld.HeightMapMinHeight, max = ld.HeightMapMaxHeight },
                    },
                    heightDelta = delta,
                    physicsDelta = afterPhy - beforePhy,
                    rebuiltPhysics = rebuildPhysics,
                    verdict,
                };
            });

            if (ok == false)
                return FailJson("timed out waiting for the engine main thread");
            return JsonSerializer.Serialize(payload);
        }

        [Bricks.AIGC.TtMCPTool("terrain_dump_height_region",
            "Dumps statistics and a downsampled grid of the CPU height buffer over a texel region. " +
            "Call before and after an edit to see the shape of the change instead of a single sample - " +
            "this catches strokes that landed in the wrong place rather than not at all.",
            returnDescription: "{level:{x,z}, region:{minX,minY,maxX,maxY}, texelCount, min, max, mean, " +
            "grid: number[][] - Downsampled heights, row major, north-west origin, gridStep, error}")]
        public static string TerrainDumpHeightRegion(
            [Bricks.AIGC.TtMCPParameter("Level index X")] double levelX = 0,
            [Bricks.AIGC.TtMCPParameter("Level index Z")] double levelZ = 0,
            [Bricks.AIGC.TtMCPParameter("Region min texel X. Negative means the whole level")] double minX = -1,
            [Bricks.AIGC.TtMCPParameter("Region min texel Y")] double minY = -1,
            [Bricks.AIGC.TtMCPParameter("Region max texel X")] double maxX = -1,
            [Bricks.AIGC.TtMCPParameter("Region max texel Y")] double maxY = -1,
            [Bricks.AIGC.TtMCPParameter("Side length of the returned downsampled grid")] double gridSide = 16)
        {
            object payload = null;
            int lx = (int)levelX;
            int lz = (int)levelZ;
            int side = Math.Clamp((int)gridSide, 2, 64);

            var ok = TtMainThreadDispatcher.Invoke(() =>
            {
                var ctx = LocateTerrain();
                if (ctx.Terrain == null)
                {
                    payload = new { error = ctx.Error };
                    return;
                }
                var terrain = ctx.Terrain;
                if (terrain.Levels == null ||
                    lx < 0 || lx >= terrain.NumOfLevelX || lz < 0 || lz >= terrain.NumOfLevelZ)
                {
                    payload = new { error = $"level index ({lx},{lz}) is out of range " +
                        $"[0,{terrain.NumOfLevelX})x[0,{terrain.NumOfLevelZ})" };
                    return;
                }
                var ld = terrain.Levels[lz, lx]?.LevelData;
                if (ld == null)
                {
                    payload = new { error = $"level ({lx},{lz}) is not loaded" };
                    return;
                }
                if (ld.IsEditable == false)
                {
                    payload = new { error = $"level ({lx},{lz}) has no readable CPU height buffer " +
                        "(IsEditable is false); run terrain_get_info for the reason" };
                    return;
                }

                var buf = ld.SourceHeightMap;
                int x0 = minX < 0 ? 0 : Math.Clamp((int)minX, 0, buf.Width - 1);
                int y0 = minY < 0 ? 0 : Math.Clamp((int)minY, 0, buf.Height - 1);
                int x1 = maxX < 0 ? buf.Width - 1 : Math.Clamp((int)maxX, 0, buf.Width - 1);
                int y1 = maxY < 0 ? buf.Height - 1 : Math.Clamp((int)maxY, 0, buf.Height - 1);
                if (x1 < x0 || y1 < y0)
                {
                    payload = new { error = "the region is empty after clamping to the buffer" };
                    return;
                }

                float min = float.MaxValue;
                float max = float.MinValue;
                double sum = 0.0;
                long count = 0;
                for (int y = y0; y <= y1; y++)
                {
                    for (int x = x0; x <= x1; x++)
                    {
                        float h = buf.GetPixel<float>(x, y);
                        if (h < min) min = h;
                        if (h > max) max = h;
                        sum += h;
                        count++;
                    }
                }

                // 降采样成固定边长的网格, 免得把上百万个 texel 塞进 JSON。
                int w = x1 - x0 + 1;
                int h2 = y1 - y0 + 1;
                var grid = new float[side][];
                for (int gy = 0; gy < side; gy++)
                {
                    grid[gy] = new float[side];
                    for (int gx = 0; gx < side; gx++)
                    {
                        int sx = x0 + (int)((double)gx / (side - 1) * (w - 1));
                        int sy = y0 + (int)((double)gy / (side - 1) * (h2 - 1));
                        grid[gy][gx] = buf.GetPixel<float>(sx, sy);
                    }
                }

                payload = new
                {
                    level = new { x = lx, z = lz },
                    region = new { minX = x0, minY = y0, maxX = x1, maxY = y1 },
                    texelCount = count,
                    min,
                    max,
                    mean = count > 0 ? sum / count : 0.0,
                    grid,
                    gridStep = new { x = (double)w / side, y = (double)h2 / side },
                    isInEditSession = ld.IsInEditSession,
                };
            });

            if (ok == false)
                return FailJson("timed out waiting for the engine main thread");
            return JsonSerializer.Serialize(payload);
        }

        [Bricks.AIGC.TtMCPTool("terrain_edit_session",
            "Controls the edit session of one terrain level. " +
            "'begin' freezes the height encode range with headroom so later strokes can use partial GPU " +
            "uploads. 'end' leaves the session. 'flush' uploads pending dirty regions. " +
            "'rebuild' forces a FULL level rebuild from the CPU buffer - use it as a control experiment: " +
            "if a rebuild makes the edit visible but partial uploads do not, the fault is isolated to the " +
            "partial upload or RVT dirty marking path.",
            returnDescription: "{action: string, level:{x,z}, isInEditSession, encodeRange:{min,max}, " +
            "error}")]
        public static string TerrainEditSession(
            [Bricks.AIGC.TtMCPParameter("One of: begin, end, flush, rebuild")] string action = "begin",
            [Bricks.AIGC.TtMCPParameter("Level index X")] double levelX = 0,
            [Bricks.AIGC.TtMCPParameter("Level index Z")] double levelZ = 0,
            [Bricks.AIGC.TtMCPParameter("Headroom in world units for 'begin'")] double headroom = 256,
            [Bricks.AIGC.TtMCPParameter("Rebuild the physics heightfield for 'flush'")] bool rebuildPhysics = true)
        {
            var act = (action ?? "").Trim().ToLowerInvariant();
            if (act != "begin" && act != "end" && act != "flush" && act != "rebuild")
                return FailJson($"unknown action '{action}', expected begin, end, flush or rebuild");

            object payload = null;
            int lx = (int)levelX;
            int lz = (int)levelZ;

            var ok = TtMainThreadDispatcher.Invoke(() =>
            {
                var ctx = LocateTerrain();
                if (ctx.Terrain == null)
                {
                    payload = new { action = act, error = ctx.Error };
                    return;
                }
                var terrain = ctx.Terrain;
                if (terrain.Levels == null ||
                    lx < 0 || lx >= terrain.NumOfLevelX || lz < 0 || lz >= terrain.NumOfLevelZ)
                {
                    payload = new { action = act, error = $"level index ({lx},{lz}) is out of range" };
                    return;
                }
                var ld = terrain.Levels[lz, lx]?.LevelData;
                if (ld == null)
                {
                    payload = new { action = act, error = $"level ({lx},{lz}) is not loaded" };
                    return;
                }
                if (ld.IsEditable == false)
                {
                    payload = new { action = act, error = $"level ({lx},{lz}) is not editable" };
                    return;
                }

                switch (act)
                {
                    case "begin": ld.BeginEditSession((float)headroom); break;
                    case "end": ld.EndEditSession(); break;
                    case "flush": ld.FlushDirty(rebuildPhysics); break;
                    case "rebuild": ld.RebuildHeightMapFromSource(); break;
                }

                payload = new
                {
                    action = act,
                    level = new { x = lx, z = lz },
                    isInEditSession = ld.IsInEditSession,
                    encodeRange = new { min = ld.HeightMapMinHeight, max = ld.HeightMapMaxHeight },
                };
            });

            if (ok == false)
                return FailJson("timed out waiting for the engine main thread");
            return JsonSerializer.Serialize(payload);
        }
    }
}
