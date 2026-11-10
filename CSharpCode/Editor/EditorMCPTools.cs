using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;

namespace EngineNS.Editor
{
    /// <summary>
    /// 缂栬緫鍣ㄤ晶鐨?MCP 宸ュ叿闆? 鎵撳紑鍦烘櫙 / 鍦板舰 Level goto / Level 鐘舵€佹煡璇€?
    /// 鎵€鏈夊伐鍏峰唴閮ㄩ兘鍒囧洖涓荤嚎绋嬫墽琛? MCP 缃戠粶绾跨▼杞绛夊緟瀹屾垚鏍囧織銆?
    /// </summary>
    public static class TtEditorMCPTools
    {
        class FSyncCall
        {
            public Action Action;
            public Exception Error;
            public volatile bool Done;
        }

        /// <summary>
        /// 鎶婂伐鍏锋姇鍒颁富绾跨▼鎵ц骞剁瓑寰呭畬鎴?鎴栬秴鏃?, 杩斿洖 null 琛ㄧず鎴愬姛, 鍚﹀垯涓洪敊璇弿杩般€?
        /// </summary>
        static string RunOnMainAndWait(Action action, int timeoutMs = 120000)
        {
            var call = new FSyncCall { Action = action };
            TtEngine.Instance.EventPoster.RunOn((state) =>
            {
                try
                {
                    call.Action();
                }
                catch (Exception ex)
                {
                    call.Error = ex;
                }
                call.Done = true;
                return true;
            }, Thread.Async.EAsyncTarget.Main);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (call.Done == false)
            {
                if (sw.ElapsedMilliseconds > timeoutMs)
                    return $"main-thread call timed out after {timeoutMs}ms";
                System.Threading.Thread.Sleep(10);
            }
            if (call.Error != null)
                return call.Error.Message;
            return null;
        }

        static TtMainEditorApplication GetMainEditor()
        {
            return TtEngine.Instance.GfxDevice.SlateApplication as TtMainEditorApplication;
        }

        static Forms.TtSceneEditor FindSceneEditor(string sceneName)
        {
            Forms.TtSceneEditor result = null;
            // OpenedEditors 鍙兘鐢变富绾跨▼閬嶅巻(涓荤嚎绋嬩細鍚屾椂澧炲垹), 椤荤敱涓荤嚎绋嬮棴鍖呰皟鐢?
            var mainEditor = GetMainEditor();
            if (mainEditor == null)
                return null;
            var editors = mainEditor.AssetEditorManager.OpenedEditors;
            for (int i = editors.Count - 1; i >= 0; i--)
            {
                if (editors[i] is Forms.TtSceneEditor se)
                {
                    if (string.IsNullOrEmpty(sceneName) || se.AssetName.ToString().Contains(sceneName, StringComparison.OrdinalIgnoreCase))
                    {
                        result = se;
                        break;
                    }
                }
            }
            return result;
        }

        static Bricks.Terrain.CDLOD.TtTerrainNode FindTerrainNode(GamePlay.Scene.TtNode root)
        {
            if (root == null)
                return null;
            if (root is Bricks.Terrain.CDLOD.TtTerrainNode terrain)
                return terrain;
            foreach (var child in root.Children)
            {
                var found = FindTerrainNode(child);
                if (found != null)
                    return found;
            }
            return null;
        }

        // ---------- renderdoc_capture ----------

        static bool mRenderDocInited = false;
        /// <summary>
        /// 初始化 in-app API。renderdoc.dll 已由外部注入时, 传注入实例的模块路径
        /// (LoadLibrary 命中已加载模块, GetModuleHandle 才能拿到句柄)。
        /// </summary>
        static unsafe bool EnsureRenderDocInited()
        {
            if (mRenderDocInited)
                return true;
#if PWindow
            var doc = IRenderDocTool.GetInstance();
            doc.InitRenderDoc("C:\\Program Files\\RenderDoc\\renderdoc.dll");
            doc.SetGpuDevice(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject);
            doc.SetActiveWindow(GetMainEditor().NativeWindow.HWindow.ToPointer());
#endif
            mRenderDocInited = true;
            return true;
        }

        static string ToolRenderDocCapture(JsonElement args)
        {
            string tag = "mcp";
            if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty("tag", out var t) && t.ValueKind == JsonValueKind.String)
                tag = t.GetString();
            int frames = 1;
            if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty("frames", out var f) && f.ValueKind == JsonValueKind.Number)
                frames = Math.Max(1, f.GetInt32());
            if (GetMainEditor() == null)
                return "main editor application not ready";

            var before = TtEngine.Instance.GfxDevice.RenderQueue.LastCaptureFile;
            string initErr = null;
            var err = RunOnMainAndWait(() =>
            {
                try
                {
                    EnsureRenderDocInited();
                    var q = TtEngine.Instance.GfxDevice.RenderQueue;
                    q.OpenRenderDocAfterCapture = false; // 自动化抓帧不弹 UI
                    q.CaptureTagName = tag;
                    q.RemainingCaptureFrames = frames - 1;
                    q.CaptureRenderDocFrame = true;
                }
                catch (Exception ex)
                {
                    initErr = ex.ToString();
                }
            }, 5000);
            if (err != null)
                return err;
            if (initErr != null)
                return initErr;

            // 抓帧是跨帧异步的, 轮询等 LastCaptureFile 变化 (最多 10s)
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 10000)
            {
                var now = TtEngine.Instance.GfxDevice.RenderQueue.LastCaptureFile;
                if (now != null && now != before)
                    return $"captured: cache/renderdoc/{now}";
                System.Threading.Thread.Sleep(50);
            }
            return "capture timeout (no new rdc within 10s)";
        }

        // ---------- open_scene ----------

        static volatile int mSceneOpenState = 0; // 0=idle 1=running 2=done 3=failed
        static string mSceneOpenError = null;

        static async Thread.Async.TtTask OpenSceneAsync(string sceneName)
        {
            try
            {
                var rname = RName.GetRName(sceneName);
                var ok = await TtAssetEditorManager.TryOpenEditor(typeof(Forms.TtSceneEditor), rname, null, false);
                if (ok == false)
                {
                    mSceneOpenError = "TryOpenEditor returned false";
                    mSceneOpenState = 3;
                    return;
                }
                mSceneOpenState = 2;
            }
            catch (Exception ex)
            {
                mSceneOpenError = ex.ToString();
                mSceneOpenState = 3;
            }
        }

        static string ToolOpenScene(JsonElement args)
        {
            string name = null;
            if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String)
                name = n.GetString();
            if (string.IsNullOrEmpty(name))
                return "missing required arg: name";
            if (GetMainEditor() == null)
                return "main editor application not ready";

            // 已打开同名场景则直接成功(主线程查询)
            string existed = null;
            RunOnMainAndWait(() =>
            {
                var se0 = FindSceneEditor(name);
                if (se0 != null)
                    existed = se0.AssetName.ToString();
            }, 10000);
            if (existed != null)
                return $"scene already open: {existed}";

            mSceneOpenError = null;
            mSceneOpenState = 1;
            TtEngine.Instance.EventPoster.RunOn((state) =>
            {
                var _ = OpenSceneAsync(name);
                return true;
            }, Thread.Async.EAsyncTarget.Main);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (mSceneOpenState == 1)
            {
                if (sw.ElapsedMilliseconds > 120000)
                    return $"open_scene timed out: {name}";
                System.Threading.Thread.Sleep(50);
            }
            if (mSceneOpenState != 2)
                return $"open_scene failed: {mSceneOpenError ?? "unknown"}";

            // 鍐嶇瓑涓€鎷嶈 Scene 鎸傚埌 viewport world 涓?
            System.Threading.Thread.Sleep(500);
            var se = FindSceneEditor(name);
            if (se == null)
                return $"scene editor created but not found: {name}";
            return $"scene opened: {se.AssetName}";
        }

        /// <summary>
        /// 涓荤嚎绋嬫煡鎵惧満鏅紪杈戝櫒閲岀殑 TtTerrainNode; 澶辫触鏃惰繑鍥為敊璇弿杩般€?
        /// </summary>
        static Bricks.Terrain.CDLOD.TtTerrainNode FindTerrainOnMain(string sceneName, out string error)
        {
            Bricks.Terrain.CDLOD.TtTerrainNode terrain = null;
            string err = null;   // lambda 不能捕获 out 参数, 先用局部变量接
            err = RunOnMainAndWait(() =>
            {
                var se = FindSceneEditor(sceneName);
                if (se == null) { err = "no open scene editor found"; return; }
                var world = se.PreviewViewport.World;
                if (world?.Root == null) { err = "scene editor world not ready"; return; }
                terrain = FindTerrainNode(world.Root);
                if (terrain == null)
                    err = "no TtTerrainNode in scene";
            });
            error = err;
            if (err != null)
                return null;
            return terrain;
        }

        // ---------- goto_level ----------

        static string ToolGotoLevel(JsonElement args)
        {
            int levelX = 0, levelZ = 0;
            string sceneName = null;
            if (args.ValueKind == JsonValueKind.Object)
            {
                if (args.TryGetProperty("levelX", out var x)) levelX = x.GetInt32();
                if (args.TryGetProperty("levelZ", out var z)) levelZ = z.GetInt32();
                if (args.TryGetProperty("scene", out var s) && s.ValueKind == JsonValueKind.String) sceneName = s.GetString();
            }
            // 编辑器 world/terrain 查找必须在主线程
            Bricks.Terrain.CDLOD.TtTerrainNode terrain = null;
            string err = null;
            err = RunOnMainAndWait(() =>
            {
                var se = FindSceneEditor(sceneName);
                if (se == null) { err = "no open scene editor found"; return; }
                var world = se.PreviewViewport.World;
                if (world?.Root == null) { err = "scene editor world not ready"; return; }
                terrain = FindTerrainNode(world.Root);
                if (terrain == null)
                    err = "no TtTerrainNode in scene";
            });
            if (err != null)
                return err;
            if (terrain == null)
                return "no TtTerrainNode in scene";

            bool gotoOk = false;
            err = RunOnMainAndWait(() =>
            {
                gotoOk = Bricks.Terrain.CDLOD.TtTerrainLevelGridEditor.GotoLevel(terrain, levelX, levelZ);
            });
            if (err != null)
                return $"goto_level failed: {err}";
            if (gotoOk == false)
                return $"goto_level({levelX},{levelZ}) returned false (bad index or node not ready), see TerrainLevelGrid logs";
            return $"goto_level({levelX},{levelZ}) ok";
        }

        // ---------- get_level_states ----------

        static string ToolGetLevelStates(JsonElement args)
        {
            string sceneName = null;
            if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty("scene", out var s) && s.ValueKind == JsonValueKind.String)
                sceneName = s.GetString();
            var terrain = FindTerrainOnMain(sceneName, out var findError);
            if (terrain == null)
                return findError;
            if (terrain.Levels == null)
                return "terrain Levels array is null";

            var lines = new List<string>
            {
                $"terrain={terrain.NodeId} levels={terrain.Levels.GetLength(0)}x{terrain.Levels.GetLength(1)} center=({terrain.ActiveCenterLevelIndex.X},{terrain.ActiveCenterLevelIndex.Y})",
            };
            for (int z = 0; z < terrain.Levels.GetLength(0); z++)
            {
                for (int x = 0; x < terrain.Levels.GetLength(1); x++)
                {
                    var level = terrain.Levels[z, x];
                    if (level == null)
                        continue;
                    var state = terrain.GetLevelDataState(level);
                    lines.Add($"({x},{z}) state={state} data={(level.LevelData != null ? "Y" : "N")}");
                }
            }
            return string.Join("\n", lines);
        }

        /// <summary>
        /// 鍦ㄧ紪杈戝櫒 InitializeApplication 闃舵璋冪敤: 娉ㄥ唽缂栬緫鍣ㄥ伐鍏峰苟鍚姩 MCP 鏈嶅姟銆?
        /// </summary>
        // ---------- level_height_stats ----------

        /// <summary>
        /// Read per-level CPU authoring heightmap (SourceHeightMap) stats, to tell
        /// whether a hand-painted hill disappeared at the data layer or only at the render path.
        /// </summary>
        static string ToolLevelHeightStats(JsonElement args)
        {
            int levelX = 0, levelZ = 0;
            string sceneName = null;
            if (args.ValueKind == JsonValueKind.Object)
            {
                if (args.TryGetProperty("levelX", out var x)) levelX = x.GetInt32();
                if (args.TryGetProperty("levelZ", out var z)) levelZ = z.GetInt32();
                if (args.TryGetProperty("scene", out var s) && s.ValueKind == JsonValueKind.String) sceneName = s.GetString();
            }
            var terrain = FindTerrainOnMain(sceneName, out var findError);
            if (terrain == null)
                return findError;
            if (terrain.Levels == null)
                return "terrain Levels array is null";
            if (levelZ < 0 || levelZ >= terrain.Levels.GetLength(0) || levelX < 0 || levelX >= terrain.Levels.GetLength(1))
                return $"level index ({levelX},{levelZ}) out of range";
            var level = terrain.Levels[levelZ, levelX];
            if (level == null)
                return $"level ({levelX},{levelZ}) is null";
            var ld = level.LevelData;
            if (ld == null)
                return $"level ({levelX},{levelZ}) LevelData is null, state={terrain.GetLevelDataState(level)}";
            var buf = ld.SourceHeightMap;
            if (buf == null)
                return $"level ({levelX},{levelZ}) SourceHeightMap is null (hollow shell)";

            float min = float.MaxValue, max = float.MinValue;
            double sum = 0;
            long count = 0;
            int argmaxX = -1, argmaxY = -1;
            for (int y = 0; y < buf.Height; y++)
            {
                for (int xx = 0; xx < buf.Width; xx++)
                {
                    float h = buf.GetPixel<float>(xx, y);
                    if (h < min) min = h;
                    if (h > max) { max = h; argmaxX = xx; argmaxY = y; }
                    sum += h;
                    count++;
                }
            }
            // texel -> world: level 原点 = (levelX*1024, levelZ*1024), GridSize=1 时 texel 即米
            var gridSize = terrain.GridSize;
            float worldMaxX = levelX * 1024f + argmaxX * gridSize;
            float worldMaxZ = levelZ * 1024f + argmaxY * gridSize;
            return $"level({levelX},{levelZ}) texels={count} min={min:0.###} max={max:0.###} mean={(sum / count):0.###} " +
                $"argmaxTexel=({argmaxX},{argmaxY}) argmaxWorld=({worldMaxX:0.#},{worldMaxZ:0.#}) " +
                $"encodeRange=[{ld.HeightMapMinHeight:0.#},{ld.HeightMapMaxHeight:0.#}] state={terrain.GetLevelDataState(level)} session={ld.IsInEditSession}";
        }

        // ---------- rvt_layer_probe ----------

        static float HalfToFloat(ushort h)
        {
            uint sign = (uint)(h >> 15) & 0x1;
            uint exp = (uint)(h >> 10) & 0x1f;
            uint man = (uint)(h & 0x3ff);
            uint bits;
            if (exp == 0)
            {
                if (man == 0) bits = sign << 31;
                else
                {
                    int e = -1;
                    do { man <<= 1; e++; } while ((man & 0x400) == 0);
                    man &= 0x3ff;
                    bits = (sign << 31) | ((uint)(127 - 15 - e) << 23) | (man << 13);
                }
            }
            else if (exp == 31)
                bits = (sign << 31) | (0xff << 23) | (man << 13);
            else
                bits = (sign << 31) | ((exp - 15 + 127) << 23) | (man << 13);
            return BitConverter.UInt32BitsToSingle(bits);
        }

        /// <summary>
        /// 回读 GPU 上 HeightmapRVT atlas 中指定 level 的那一层, 与 CPU 侧 SourceHeightMap
        /// 的 min/max 对比。GPU 层为空/全 0 而 CPU 有高度 => atlas copy 链丢失;
        /// 两者一致 => 问题在采样/槽表/解码链。
        /// </summary>
        static string ToolRvtLayerProbe(JsonElement args)
        {
            int levelX = 0, levelZ = 0;
            string sceneName = null;
            if (args.ValueKind == JsonValueKind.Object)
            {
                if (args.TryGetProperty("levelX", out var x)) levelX = x.GetInt32();
                if (args.TryGetProperty("levelZ", out var z)) levelZ = z.GetInt32();
                if (args.TryGetProperty("scene", out var s) && s.ValueKind == JsonValueKind.String) sceneName = s.GetString();
            }
            Bricks.Terrain.CDLOD.TtTerrainNode terrain = null;
            string err = null;
            err = RunOnMainAndWait(() =>
            {
                var se = FindSceneEditor(sceneName);
                if (se == null) { err = "no open scene editor found"; return; }
                var world = se.PreviewViewport.World;
                if (world?.Root == null) { err = "scene editor world not ready"; return; }
                terrain = FindTerrainNode(world.Root);
                if (terrain == null)
                    err = "no TtTerrainNode in scene";
            });
            if (err != null)
                return err;
            if (terrain.Levels == null)
                return "terrain Levels array is null";
            var level = terrain.Levels[levelZ, levelX];
            if (level?.LevelData == null)
                return $"level ({levelX},{levelZ}) has no LevelData";

            var srv = level.LevelData.HeightMapSRV;
            var rvt = srv?.Rvt;
            if (rvt == null)
                return "HeightMapSRV has no RVT registration (never RegRVT'd)";
            if (rvt.Slot == null)
                return $"rvt UniqueTexID={rvt.UniqueTexID} has NO slot (currently inactive)";
            var atlas = terrain.Terrain.HeightmapRVT.TextureSlotAllocator.TextureArray;
            if (atlas == null)
                return "atlas TextureArray is null";

            int arrayIndex = (int)rvt.Slot.ArrayIndex;
            uint mipLevels = (uint)terrain.Terrain.HeightmapRVT.TextureSlotAllocator.TexDesc.MipLevels;
            uint subResource = (uint)arrayIndex * mipLevels;

            float gpuMin = float.MaxValue, gpuMax = float.MinValue;
            string probeError = null;
            err = RunOnMainAndWait(() =>
            {
                try
                {
                    var blob = new Support.TtBlobObject();
                    if (NxRHI.TtSrView.FetchTextureSubresource(atlas.mCoreObject, (int)subResource, blob, out var fp) == false)
                    {
                        probeError = "FetchTextureSubresource failed";
                        return;
                    }
                    unsafe
                    {
                        var pData = (byte*)blob.DataPointer;
                        uint rowPitch = fp.RowPitch;
                        pData += sizeof(uint) + sizeof(uint);
                        int w = (int)fp.Width;
                        int h = (int)fp.Height;
                        for (int y = 0; y < h; y++)
                        {
                            ushort* row = (ushort*)(pData + (long)y * rowPitch);
                            for (int xx = 0; xx < w; xx++)
                            {
                                float v = HalfToFloat(row[xx]);
                                if (v < gpuMin) gpuMin = v;
                                if (v > gpuMax) gpuMax = v;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    probeError = ex.Message;
                }
            });
            if (err != null)
                return $"probe failed: {err}";
            if (probeError != null)
                return $"probe failed: {probeError}";

            var buf = level.LevelData.SourceHeightMap;
            float cpuMin = float.MaxValue, cpuMax = float.MinValue;
            for (int y = 0; y < buf.Height; y++)
                for (int xx = 0; xx < buf.Width; xx++)
                {
                    float v = buf.GetPixel<float>(xx, y);
                    if (v < cpuMin) cpuMin = v;
                    if (v > cpuMax) cpuMax = v;
                }

            // 诊断槽表维护链 (仅 CPU 侧, 不做 GPU 回读 —— DYNAMIC buffer CreateReadable 会崩进程)
            string slotInfo = null;
            RunOnMainAndWait(() =>
            {
                var slotBuf = terrain.Terrain.HeightmapRVT.TextureSlotBuffer;
                int n = (int)System.Math.Min(slotBuf.DataArray.Count, 8u);
                var sb = new System.Text.StringBuilder();
                sb.Append($"gpuBuffer={(slotBuf.GpuBuffer != null ? "OK" : "NULL")} srv={(slotBuf.Srv != null ? "OK" : "NULL")} cap={slotBuf.DataArray.Count}");
                for (int i = 0; i < n; i++)
                    sb.Append($" [{i}]={slotBuf.DataArray[i]}");
                slotInfo = sb.ToString();
            });

            string verdict;
            if (gpuMax < 0.001f && cpuMax > 0.5f)
                verdict = "GPU ATLAS LAYER IS EMPTY while CPU has heights => atlas copy chain lost the data (upload timing or copy source)";
            else if (Math.Abs(gpuMax - cpuMax) < 0.1f)
                verdict = "GPU atlas layer matches CPU data => sampling/slot/decode chain is the suspect";
            else
                verdict = $"GPU atlas differs from CPU => stale atlas content";
            return $"level({levelX},{levelZ}) slot={arrayIndex} texID={rvt.UniqueTexID} gpu[min={gpuMin:0.###},max={gpuMax:0.###}] cpu[min={cpuMin:0.###},max={cpuMax:0.###}] :: {verdict} | {slotInfo}";
        }

        public static void RegisterAndStartup()
        {
            Bricks.AIGC.TtEngineMCPServer.RegisterTool("open_scene", ToolOpenScene);
                        Bricks.AIGC.TtEngineMCPServer.RegisterTool("renderdoc_capture", ToolRenderDocCapture);
            Bricks.AIGC.TtEngineMCPServer.RegisterTool("goto_level", ToolGotoLevel);
            Bricks.AIGC.TtEngineMCPServer.RegisterTool("get_level_states", ToolGetLevelStates);
            Bricks.AIGC.TtEngineMCPServer.RegisterTool("level_height_stats", ToolLevelHeightStats);
            Bricks.AIGC.TtEngineMCPServer.RegisterTool("rvt_layer_probe", ToolRvtLayerProbe);
            Bricks.AIGC.TtEngineMCPServer.Startup(18118);   // 8818 被系统 http.sys 占用
        }
    }
}
