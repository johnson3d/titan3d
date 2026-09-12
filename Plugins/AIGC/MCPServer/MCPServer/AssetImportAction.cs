using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EngineNS.Plugins.MCPServer
{
    /// <summary>
    /// 资产导入 (Asset Import) 诊断与执行 MCP 工具。
    ///
    /// 为什么需要这一组: 导入是"源文件 -> 引擎资产"的单向漏斗, 中途丢数据不会报错, 只是资产里
    /// 少了点东西, 表现出来就是"模型没动/没骨骼/尺寸不对"。查 morph 不动那次就吃了这个亏 ——
    /// 引擎日志只说 "Imported 1 morph target(s)", 完全看不出源文件里还带着一条 morph 权重曲线,
    /// 而 AnimationChunkGenerater 压根没有 morph 分支, 曲线被静默丢掉, 只能靠读导入器源码发现。
    /// inspect_source_file 把这类"引擎会丢什么"直接写进返回值, 免得每次都去翻源码。
    ///
    /// 排查顺序:
    ///   1) inspect_source_file  —— 源文件里到底有什么 (Assimp 视角), 以及导入器会丢掉哪些;
    ///   2) inspect_mesh_asset   —— 导入产物对不对 (顶点流 / 骨骼 / morph / AABB 尺寸);
    ///   3) get_asset_meta_info  —— 产物之间的引用关系, 以及磁盘文件是否齐全;
    ///   4) import_mesh_source / import_animation_source —— 改完导入器后立刻重导验证。
    ///
    /// 单位: 引擎内部一律米 (CodingGuidelines.md §6), 而 DCC 导出的 FBX 基本都是厘米, 所以
    /// TtMeshImportSetting.UnitScale 默认 0.01。导入后 AABB 差两个数量级, 先怀疑这个值。
    /// </summary>
    public partial class TtMCPServerPlugin
    {
        #region asset-import helpers

        // AABB 尺寸的"看着正常"区间(米)。超出只是提示单位可疑 —— 地形和巨物确实可以很大,
        // 螺丝钉确实可以很小, 所以这里只报 suspicion 不报 error。
        private const float ImportSaneMaxSizeMeters = 200.0f;
        private const float ImportSaneMinSizeMeters = 0.005f;

        // 与导入器 AssetImporter.MorphNormalDeltaThreshold 保持一致: 诊断必须和导入器同口径,
        // 否则这个工具会报出一个与实际所走分支不一致的结论。
        private const float MorphNormalDifferEpsilon = 1e-3f;

        /// <summary> 值得关心的顶点流。剩下那些 (Terrain/Inst/F4_x) 不由模型导入产生。 </summary>
        private static readonly NxRHI.EVertexStreamType[] InspectedVertexStreams = new[]
        {
            NxRHI.EVertexStreamType.VST_Position,
            NxRHI.EVertexStreamType.VST_Normal,
            NxRHI.EVertexStreamType.VST_Tangent,
            NxRHI.EVertexStreamType.VST_Color,
            NxRHI.EVertexStreamType.VST_UV,
            NxRHI.EVertexStreamType.VST_ExtraUV,
            NxRHI.EVertexStreamType.VST_SkinIndex,
            NxRHI.EVertexStreamType.VST_SkinWeight,
        };

        /// <summary>
        /// 探测某个顶点流是否存在。GetVertexBuffer 内部是 GetGeomtryMesh()->GetVertexArray()->GetVB(),
        /// 任一环为空都会在原生侧炸, 所以必须整体 try 住。IBuffer 是原生指针的值类型包装,
        /// 判空要用 IsValidPointer 而不是 != null。
        /// </summary>
        private static bool HasVertexStream(Graphics.Mesh.TtMeshPrimitives primitives, NxRHI.EVertexStreamType type)
        {
            try
            {
                return primitives.GetVertexBuffer(type).IsValidPointer;
            }
            catch
            {
                return false;
            }
        }

        private static List<string> CollectVertexStreams(Graphics.Mesh.TtMeshPrimitives primitives)
        {
            var streams = new List<string>();
            for (int i = 0; i < InspectedVertexStreams.Length; i++)
            {
                if (HasVertexStream(primitives, InspectedVertexStreams[i]))
                    streams.Add(InspectedVertexStreams[i].ToString());
            }
            return streams;
        }

        /// <summary>
        /// 读 .vms 的 AABB。FMeshPrimitives.mAABB 的属性访问器是 internal (只对引擎程序集开放),
        /// 插件是独立程序集, 只能走公开的 UnsafeAsLayout 直接读结构体字段。
        /// </summary>
        private static unsafe bool TryGetMeshAabb(Graphics.Mesh.TtMeshPrimitives primitives, out BoundingBox box)
        {
            box = default;
            try
            {
                var layout = primitives.mCoreObject.UnsafeAsLayout;
                if (layout == null)
                    return false;
                box = layout->mAABB;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static Dictionary<string, object> DescribeAabb(in BoundingBox box)
        {
            var size = box.GetSize();
            float maxDim = Math.Max(size.X, Math.Max(size.Y, size.Z));
            string sanity;
            if (maxDim > ImportSaneMaxSizeMeters)
                sanity = $"Largest dimension is {maxDim:F2} m, which is unusually big. If this should be a prop-sized mesh, UnitScale was probably left at 1.0 while the source file is in centimeters.";
            else if (maxDim < ImportSaneMinSizeMeters)
                sanity = $"Largest dimension is {maxDim:F5} m, which is unusually small. UnitScale was probably applied twice (0.01 on an already-metric source).";
            else
                sanity = "Size looks plausible for a metric asset.";

            return new Dictionary<string, object>
            {
                ["minMeters"] = new[] { box.Minimum.X, box.Minimum.Y, box.Minimum.Z },
                ["maxMeters"] = new[] { box.Maximum.X, box.Maximum.Y, box.Maximum.Z },
                ["sizeMeters"] = new[] { size.X, size.Y, size.Z },
                ["maxDimensionMeters"] = maxDim,
                ["sanity"] = sanity,
            };
        }

        /// <summary>
        /// 源 shape 的法线是否真的与基础网格不同 (与导入器
        /// AssetImporter.SourceNormalsDifferFromBase 同一判据。它是 private, 而拿不到它就
        /// 只能靠推断导入器会走哪条分支 —— 正是上次误判 morph 法线根因的原因)。
        /// </summary>
        private static bool SourceMorphNormalsDifferFromBase(Assimp.Mesh mesh, Assimp.MeshAnimationAttachment attachment)
        {
            if (mesh.HasNormals == false || attachment.HasNormals == false)
                return false;

            var baseNormals = mesh.Normals;
            var morphNormals = attachment.Normals;
            if (baseNormals == null || morphNormals == null)
                return false;

            var limit = System.Math.Min(baseNormals.Count, morphNormals.Count);
            for (int v = 0; v < limit; v++)
            {
                var dx = morphNormals[v].X - baseNormals[v].X;
                var dy = morphNormals[v].Y - baseNormals[v].Y;
                var dz = morphNormals[v].Z - baseNormals[v].Z;
                if (dx * dx + dy * dy + dz * dz > MorphNormalDifferEpsilon * MorphNormalDifferEpsilon)
                    return true;
            }
            return false;
        }

        private static Dictionary<string, object> DescribeSourceMesh(Assimp.Mesh mesh, int index)
        {
            var morphNames = new List<string>();
            var morphShapes = new List<Dictionary<string, object>>();
            if (mesh.HasMeshAnimationAttachments)
            {
                for (int i = 0; i < mesh.MeshAnimationAttachments.Count; i++)
                {
                    var attachment = mesh.MeshAnimationAttachments[i];
                    morphNames.Add(attachment.Name ?? "");

                    // 为何要拆出 hasNormals 与 normalsDifferFromBase 两个字段: 它们区分三种源文件,
                    // 三者导出的资产表现完全一样(光照不跟着形变变), 但根因不同:
                    //   false / false —— 源没写 shape 法线, 导入器重算;
                    //   true  / false —— 源写了, 但只是 base 法线的副本(Blender 未勾选重算),
                    //                    delta 恒为 0; 早期只看 HasNormals 时这种会落进死角;
                    //   true  / true  —— 源带了真正的 shape 法线, 直接用。
                    morphShapes.Add(new Dictionary<string, object>
                    {
                        ["name"] = attachment.Name ?? "",
                        ["vertexCount"] = attachment.VertexCount,
                        ["hasNormals"] = attachment.HasNormals,
                        ["normalsDifferFromBase"] = SourceMorphNormalsDifferFromBase(mesh, attachment),
                        // 切线这边几乎总是 false: Assimp 的 CalculateTangentSpace 只管 aiMesh,
                        // 不给 aiAnimMesh 算切线。报出来是为了能确认导入器走的是重算路径
                        // —— 重算需要 base 网格同时有 hasFaces 与 uvChannelCount > 0。
                        ["hasTangents"] = attachment.HasTangentBasis,
                    });
                }
            }

            return new Dictionary<string, object>
            {
                ["index"] = index,
                ["name"] = mesh.Name ?? "",
                ["primitiveType"] = mesh.PrimitiveType.ToString(),
                ["materialIndex"] = mesh.MaterialIndex,
                ["vertexCount"] = mesh.VertexCount,
                ["faceCount"] = mesh.FaceCount,
                ["hasFaces"] = mesh.HasFaces,
                ["boneCount"] = mesh.BoneCount,
                ["hasNormals"] = mesh.HasNormals,
                ["hasTangents"] = mesh.HasTangentBasis,
                ["uvChannelCount"] = mesh.TextureCoordinateChannelCount,
                ["colorChannelCount"] = mesh.VertexColorChannelCount,
                ["morphTargetCount"] = mesh.MeshAnimationAttachmentCount,
                ["morphNames"] = morphNames,
                ["morphShapes"] = morphShapes,
                ["morphMethod"] = mesh.MorphMethod.ToString(),
            };
        }

        /// <summary>
        /// 描述一条动画, 并按 AnimationChunkGenerater.Generate 的真实分支判断它会不会被导入:
        /// 只有 HasNodeAnimations 走 GenerateNodeAnimation, 另外两个分支是空 stub。
        /// </summary>
        private static Dictionary<string, object> DescribeSourceAnimation(Assimp.Animation anim, int index)
        {
            double tps = anim.TicksPerSecond;
            double seconds = tps > 0 ? anim.DurationInTicks / tps : 0;

            var morphChannelNames = new List<string>();
            if (anim.MeshMorphAnimationChannelCount > 0 && anim.MeshMorphAnimationChannels != null)
            {
                for (int i = 0; i < anim.MeshMorphAnimationChannels.Count; i++)
                {
                    var ch = anim.MeshMorphAnimationChannels[i];
                    morphChannelNames.Add($"{ch.Name ?? ""} ({ch.MeshMorphKeyCount} keys)");
                }
            }

            string importedAs;
            string dropReason = null;
            if (anim.HasNodeAnimations)
            {
                importedAs = "nodeAnimation";
            }
            else if (anim.HasMeshAnimations)
            {
                importedAs = "none";
                dropReason = "Vertex-based (mesh) animation: the 'HasMeshAnimations' branch of AnimationChunkGenerater.Generate is an empty stub, so no chunk is produced.";
            }
            else if (anim.MeshMorphAnimationChannelCount > 0)
            {
                importedAs = "none";
                dropReason = "Morph weight curves only: the 'MeshMorphAnimationChannelCount' branch of AnimationChunkGenerater.Generate is an empty stub, so no chunk is produced.";
            }
            else
            {
                importedAs = "none";
                dropReason = "Animation carries no channel of any kind.";
            }

            return new Dictionary<string, object>
            {
                ["index"] = index,
                ["name"] = anim.Name ?? "",
                ["durationInTicks"] = anim.DurationInTicks,
                ["ticksPerSecond"] = tps,
                ["durationSeconds"] = seconds,
                ["nodeChannelCount"] = anim.NodeAnimationChannelCount,
                ["meshChannelCount"] = anim.MeshAnimationChannelCount,
                ["meshMorphChannelCount"] = anim.MeshMorphAnimationChannelCount,
                ["morphChannels"] = morphChannelNames,
                ["importedAs"] = importedAs,
                ["dropReason"] = dropReason,
            };
        }

        /// <summary>
        /// 汇总"引擎导入器会静默丢掉什么"。这些结论来自 AssetImporter.cs 的实际分支, 不是猜的;
        /// 改了导入器就要回来更新这里, 否则工具会撒谎。
        /// </summary>
        private static List<string> BuildImportSupportWarnings(Assimp.Scene scene, float unitScaleFactor)
        {
            var warnings = new List<string>();

            int morphCurveAnims = 0, vertexAnims = 0, noChannelAnims = 0;
            if (scene.HasAnimations)
            {
                for (int i = 0; i < scene.Animations.Count; i++)
                {
                    var anim = scene.Animations[i];
                    if (anim.HasNodeAnimations)
                        continue;
                    if (anim.HasMeshAnimations)
                        vertexAnims++;
                    else if (anim.MeshMorphAnimationChannelCount > 0)
                        morphCurveAnims++;
                    else
                        noChannelAnims++;
                }
            }

            int morphShapes = 0, skinnedMeshes = 0;
            if (scene.Meshes != null)
            {
                for (int i = 0; i < scene.Meshes.Count; i++)
                {
                    morphShapes += scene.Meshes[i].MeshAnimationAttachmentCount;
                    if (scene.Meshes[i].HasBones)
                        skinnedMeshes++;
                }
            }

            if (morphCurveAnims > 0)
            {
                warnings.Add($"{morphCurveAnims} animation(s) carry ONLY morph weight curves. AnimationChunkGenerater.Generate has an empty stub for that branch, so these curves are dropped and the following Debug.Assert(chunk != null) trips in debug builds. Morph shapes still import; their weights stay driver-less.");
            }
            if (vertexAnims > 0)
            {
                warnings.Add($"{vertexAnims} animation(s) are vertex-based (mesh) animation, which the importer does not support (empty stub branch). They will be dropped.");
            }
            if (noChannelAnims > 0)
            {
                warnings.Add($"{noChannelAnims} animation(s) carry no channel at all and will be dropped.");
            }
            if (morphShapes > 0)
            {
                warnings.Add($"{morphShapes} morph target shape(s) will be imported into the .vms 'MorphTargets' attribute. Nothing drives their weights at runtime -- use the MeshPrimitiveEditor MorphTargets sliders or set_morph_weight.");
            }
            if (skinnedMeshes > 0)
            {
                warnings.Add($"{skinnedMeshes} mesh(es) are skinned. Keep AsStaticMesh=false, otherwise skin weights and the embedded PartialSkeleton are discarded.");
            }
            if (unitScaleFactor > 0 && Math.Abs(unitScaleFactor - 1.0f) < 0.001f)
            {
                warnings.Add("Source UnitScaleFactor is 1.0, i.e. the file does not declare centimeters. The importer still defaults UnitScale to 0.01; if this source is already metric, pass unitScale=1.0 or the asset comes out 100x too small.");
            }

            return warnings;
        }

        #endregion

        [Bricks.AIGC.TtMCPTool("inspect_source_file",
            "Read a model/animation source file (fbx, gltf, obj, dae ...) WITHOUT importing anything, and report what " +
            "Assimp sees plus which parts the engine importer will silently drop. Use this before blaming an asset: it " +
            "distinguishes 'the source never had the data' from 'the importer threw the data away'. Uses the same " +
            "PostProcessSteps as the real importer so counts match what an import would produce.",
            returnDescription: "{found, file:{path, sizeBytes, format, formatVersion, generator, upAxis, unitScaleFactor, handledByPlugin}, " +
            "totals:{meshNodeCount, meshCount, materialCount, animationCount, skinnedMeshCount, morphShapeCount}, " +
            "meshNodes:[{index, name, subMeshCount, totalVertices}], " +
            "meshes:[{index, name, primitiveType, materialIndex, vertexCount, faceCount, hasFaces, boneCount, hasNormals, hasTangents, uvChannelCount, colorChannelCount, morphTargetCount, morphNames, morphShapes:[{name, vertexCount, hasNormals, normalsDifferFromBase, hasTangents}], morphMethod}], " +
            "animations:[{index, name, durationInTicks, ticksPerSecond, durationSeconds, nodeChannelCount, meshChannelCount, meshMorphChannelCount, morphChannels, importedAs, dropReason}], " +
            "importWarnings, importDefaults, truncated}")]
        public static string InspectSourceFile(
            [Bricks.AIGC.TtMCPParameter("Filesystem path of the source file, e.g. 'E:/art/box_morph.fbx'. Not an engine RName.")] string filePath,
            [Bricks.AIGC.TtMCPParameter("Maximum number of meshes to list in detail")] double maxMeshes = 24,
            [Bricks.AIGC.TtMCPParameter("Maximum number of animations to list in detail")] double maxAnimations = 24)
        {
            try
            {
                LogToolCall("inspect_source_file", $"filePath={filePath}");

                if (string.IsNullOrEmpty(filePath))
                    return JsonSerializer.Serialize(new { found = false, error = "filePath is required." });
                if (System.IO.File.Exists(filePath) == false)
                    return JsonSerializer.Serialize(new { found = false, error = $"Source file not found: {filePath}" });

                long sizeBytes = 0;
                try { sizeBytes = new System.IO.FileInfo(filePath).Length; } catch { }

                // 插件接管的源格式 (目前是 Blender) 先转中间文件再走 Assimp, 直接调插件会真的去
                // 拉起 Blender 并写中间文件 —— 探测工具不该有这种副作用, 所以只报告不执行。
                if (Bricks.AssetImpExp.TtAssetSourceImportPlugin.HasPluginForSource(filePath))
                {
                    return JsonSerializer.Serialize(new
                    {
                        found = true,
                        file = new
                        {
                            path = filePath,
                            sizeBytes,
                            handledByPlugin = true,
                        },
                        note = "This extension is handled by an asset source import plugin (it converts to an intermediate file first). " +
                               "Inspecting it here would launch the external tool and write intermediate files, so no detail was read. " +
                               "Run the real import, then use inspect_mesh_asset on the result.",
                    });
                }

                var setting = Bricks.AssetImpExp.TtAssetImporter.CreateMeshImporter(filePath);
                var scene = setting?.AssetImporter?.AiScene;
                if (setting == null || scene == null)
                    return JsonSerializer.Serialize(new { found = false, error = $"Assimp failed to read '{filePath}'. The format may be unsupported or the file corrupt." });

                int meshLimit = Math.Max(1, (int)maxMeshes);
                int animLimit = Math.Max(1, (int)maxAnimations);

                var meshNodeInfos = new List<Dictionary<string, object>>();
                var meshNodes = Bricks.AssetImpExp.AssimpSceneUtil.FindMeshNodes(scene);
                for (int i = 0; i < meshNodes.Count; i++)
                {
                    var node = meshNodes[i];
                    int totalVertices = 0, subMeshCount = 0;
                    foreach (var meshIdx in node.MeshIndices)
                    {
                        if (meshIdx < 0 || meshIdx >= scene.Meshes.Count)
                            continue;
                        var m = scene.Meshes[meshIdx];
                        // 导入器同样跳过 Line/Point (见 PopulatePreviewEntries), 这里保持一致
                        if (m.PrimitiveType == Assimp.PrimitiveType.Line || m.PrimitiveType == Assimp.PrimitiveType.Point)
                            continue;
                        subMeshCount++;
                        totalVertices += m.VertexCount;
                    }
                    meshNodeInfos.Add(new Dictionary<string, object>
                    {
                        ["index"] = i,
                        ["name"] = node.Name ?? "",
                        ["subMeshCount"] = subMeshCount,
                        ["totalVertices"] = totalVertices,
                    });
                }

                var meshInfos = new List<Dictionary<string, object>>();
                int skinnedMeshCount = 0, morphShapeCount = 0;
                if (scene.Meshes != null)
                {
                    for (int i = 0; i < scene.Meshes.Count; i++)
                    {
                        if (scene.Meshes[i].HasBones)
                            skinnedMeshCount++;
                        morphShapeCount += scene.Meshes[i].MeshAnimationAttachmentCount;
                        if (meshInfos.Count < meshLimit)
                            meshInfos.Add(DescribeSourceMesh(scene.Meshes[i], i));
                    }
                }

                var animInfos = new List<Dictionary<string, object>>();
                if (scene.Animations != null)
                {
                    for (int i = 0; i < scene.Animations.Count && animInfos.Count < animLimit; i++)
                        animInfos.Add(DescribeSourceAnimation(scene.Animations[i], i));
                }

                return JsonSerializer.Serialize(new
                {
                    found = true,
                    file = new
                    {
                        path = filePath,
                        sizeBytes,
                        format = setting.FileFormat ?? "",
                        formatVersion = setting.FileFormatVersion ?? "",
                        generator = setting.Generator ?? "",
                        upAxis = setting.UpAxis ?? "",
                        unitScaleFactor = setting.UnitScaleFactor,
                        meshesHaveScale = setting.MeshesHaveScale,
                        meshesHaveTranslation = setting.MeshesHaveTranslation,
                        handledByPlugin = false,
                    },
                    totals = new
                    {
                        meshNodeCount = meshNodes.Count,
                        meshCount = scene.MeshCount,
                        materialCount = scene.Materials?.Count ?? 0,
                        animationCount = scene.AnimationCount,
                        skinnedMeshCount,
                        morphShapeCount,
                    },
                    meshNodes = meshNodeInfos,
                    meshes = meshInfos,
                    animations = animInfos,
                    importWarnings = BuildImportSupportWarnings(scene, setting.UnitScaleFactor),
                    importDefaults = new
                    {
                        unitScale = 0.01f,
                        rule = "Mesh is imported in local space; node scale/translation is only baked when applyTransformToVertex=true.",
                    },
                    truncated = (scene.MeshCount > meshInfos.Count) || (scene.AnimationCount > animInfos.Count),
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { found = false, error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("inspect_mesh_asset",
            "Inspect an imported mesh asset (.ums material mesh or .vms mesh primitives): vertex streams actually " +
            "present, skin/morph payload, sub-mesh layout, and the AABB in meters with a unit-sanity verdict. This is " +
            "the 'did the import keep my data' check -- a missing VST_SkinWeight stream or a 100x AABB is visible here " +
            "long before it is visible in the viewport.",
            returnDescription: "{found, assetName, assetKind, aabb:{minMeters, maxMeters, sizeMeters, maxDimensionMeters, sanity}, " +
            "mdfQueueType, skeleton, subMeshCount, subMeshes:[{index, meshName, vertexCount, primitiveCount, atomCount, vertexStreams, hasSkinSkeleton, boneCount, morphTargetCount, morphNames, materials}], warnings}")]
        public static string InspectMeshAsset(
            [Bricks.AIGC.TtMCPParameter("Asset RName, e.g. 'tutorials/animation/blendshape/sk_box_morph_1.vms' or '..._1.ums'")] string assetName)
        {
            try
            {
                LogToolCall("inspect_mesh_asset", $"assetName={assetName}");

                if (string.IsNullOrEmpty(assetName))
                    return JsonSerializer.Serialize(new { found = false, error = "assetName is required." });

                var rn = RName.GetRName(assetName);
                var warnings = new List<string>();
                bool isUms = assetName.EndsWith(Graphics.Mesh.TtMaterialMesh.AssetExt, StringComparison.OrdinalIgnoreCase);

                var subMeshInfos = new List<Dictionary<string, object>>();
                Dictionary<string, object> aabb = null;
                string mdfQueueType = "", skeleton = "";

                if (isUms)
                {
                    var materialMesh = RunSync(() => TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(rn));
                    if (materialMesh == null)
                        return JsonSerializer.Serialize(new { found = false, error = $"Failed to load material mesh '{assetName}'." });

                    mdfQueueType = materialMesh.MdfQueueType?.ToString() ?? "";
                    skeleton = materialMesh.Skeleton?.ToString() ?? "";
                    var box = materialMesh.AABB;
                    aabb = DescribeAabb(in box);

                    for (int i = 0; i < materialMesh.SubMeshes.Count; i++)
                    {
                        var sub = materialMesh.SubMeshes[i];
                        var info = DescribeMeshPrimitivesForImport(sub.Mesh, i, warnings);
                        info["meshName"] = sub.MeshName?.ToString() ?? "";
                        var materials = new List<string>();
                        if (sub.Materials != null)
                        {
                            for (int m = 0; m < sub.Materials.Count; m++)
                                materials.Add(sub.Materials[m]?.AssetName?.ToString() ?? "");
                        }
                        info["materials"] = materials;
                        subMeshInfos.Add(info);
                    }

                    if (string.IsNullOrEmpty(mdfQueueType))
                        warnings.Add("MdfQueueType is empty, so TtRenderMesh.Initialize picks a queue automatically from the asset payload (skin/morph detection).");
                }
                else
                {
                    var primitives = RunSync(() => TtEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(rn));
                    if (primitives == null)
                        return JsonSerializer.Serialize(new { found = false, error = $"Failed to load mesh primitives '{assetName}'." });

                    subMeshInfos.Add(DescribeMeshPrimitivesForImport(primitives, 0, warnings));
                    if (TryGetMeshAabb(primitives, out var box))
                        aabb = DescribeAabb(in box);
                }

                return JsonSerializer.Serialize(new
                {
                    found = true,
                    assetName = rn.ToString(),
                    assetKind = isUms ? "materialMesh(.ums)" : "meshPrimitives(.vms)",
                    aabb,
                    mdfQueueType,
                    skeleton,
                    subMeshCount = subMeshInfos.Count,
                    subMeshes = subMeshInfos,
                    warnings,
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { found = false, error = ex.Message });
            }
        }

        /// <summary>
        /// 描述一个 .vms 的导入结果, 并把"看着不对"的组合塞进 warnings。
        /// 判据都来自导入器的实际行为: 有 skin 权重就该有 PartialSkeleton, 有 morph 就该顶点数对齐。
        /// </summary>
        private static Dictionary<string, object> DescribeMeshPrimitivesForImport(
            Graphics.Mesh.TtMeshPrimitives primitives, int index, List<string> warnings)
        {
            var info = new Dictionary<string, object> { ["index"] = index };
            if (primitives == null)
            {
                info["error"] = "sub-mesh has no TtMeshPrimitives (the referenced .vms failed to load)";
                warnings.Add($"Sub-mesh {index} has no mesh primitives; the .ums references a .vms that cannot be loaded.");
                return info;
            }

            var streams = CollectVertexStreams(primitives);
            int vertexCount = (int)primitives.VertexNumber;
            var skinSkeleton = primitives.PartialSkeleton;
            var morphSet = primitives.MorphTargets;

            var morphNames = new List<string>();
            if (morphSet?.Targets != null)
            {
                for (int i = 0; i < morphSet.Targets.Count; i++)
                    morphNames.Add(morphSet.Targets[i]?.Name ?? "");
            }

            info["assetName"] = primitives.AssetName?.ToString() ?? "";
            info["vertexCount"] = vertexCount;
            info["primitiveCount"] = (int)primitives.PrimitiveNumber;
            info["atomCount"] = (int)primitives.NumAtom;
            info["vertexStreams"] = streams;
            info["hasSkinSkeleton"] = skinSkeleton != null;
            info["boneCount"] = skinSkeleton?.Limbs?.Count ?? 0;
            info["morphTargetCount"] = morphNames.Count;
            info["morphNames"] = morphNames;
            info["morphVertexCount"] = morphSet?.VertexCount ?? 0;

            bool hasSkinStream = streams.Contains(NxRHI.EVertexStreamType.VST_SkinWeight.ToString());
            if (hasSkinStream && skinSkeleton == null)
                warnings.Add($"Sub-mesh {index} has skin weights but no PartialSkeleton; skinning cannot resolve bones. Reimport with AsStaticMesh=false.");
            if (skinSkeleton != null && hasSkinStream == false)
                warnings.Add($"Sub-mesh {index} carries a skeleton but no VST_SkinWeight stream, so it will render rigid.");
            if (streams.Contains(NxRHI.EVertexStreamType.VST_Normal.ToString()) == false)
                warnings.Add($"Sub-mesh {index} has no normal stream; lighting will be wrong. The source file probably had no normals.");
            if (morphNames.Count > 0 && morphSet.VertexCount != vertexCount)
                warnings.Add($"Sub-mesh {index} morph vertex count ({morphSet.VertexCount}) != mesh vertex count ({vertexCount}); TtMorphModifier discards the morph set at Initialize.");

            return info;
        }

        [Bricks.AIGC.TtMCPTool("get_asset_meta_info",
            "Report the .ameta record of an asset: type, guid, which assets it references, which assets reference it, " +
            "and whether its files exist on disk. Use it to see the shape of an import's output (a .ums pointing at " +
            ".vms/.skt/.uminst) instead of guessing from directory listings.",
            returnDescription: "{found, assetName, typeExt, typeName, assetId, originSourceAddress, originSourceExists, " +
            "refAssetCount, refAssets, referencedByCount, referencedBy, " +
            "files:[{path, hash}], onDisk:{assetPath, assetExists, assetSizeBytes, assetLastWriteUtc, metaExists}}")]
        public static string GetAssetMetaInfo(
            [Bricks.AIGC.TtMCPParameter("Asset RName, e.g. 'tutorials/animation/blendshape/sk_box_morph_1.ums'")] string assetName,
            [Bricks.AIGC.TtMCPParameter("Also scan every known asset for references to this one (slower on big projects)")] bool includeReferencers = true)
        {
            try
            {
                LogToolCall("get_asset_meta_info", $"assetName={assetName}, includeReferencers={includeReferencers}");

                if (string.IsNullOrEmpty(assetName))
                    return JsonSerializer.Serialize(new { found = false, error = "assetName is required." });

                var rn = RName.GetRName(assetName);
                var meta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(rn);
                if (meta == null)
                    return JsonSerializer.Serialize(new { found = false, error = $"No .ameta registered for '{assetName}'. The asset was never imported, or the metadata was not loaded." });

                var refAssets = new List<string>();
                if (meta.RefAssetRNames != null)
                {
                    for (int i = 0; i < meta.RefAssetRNames.Count; i++)
                        refAssets.Add(meta.RefAssetRNames[i]?.ToString() ?? "");
                }

                var files = new List<Dictionary<string, object>>();
                if (meta.AssetFiles != null)
                {
                    for (int i = 0; i < meta.AssetFiles.Count; i++)
                    {
                        files.Add(new Dictionary<string, object>
                        {
                            ["path"] = meta.AssetFiles[i]?.Path ?? "",
                            ["hash"] = meta.AssetFiles[i]?.Hash ?? "",
                        });
                    }
                }

                // 反查引用方: 遍历字典要在主线程做, 主线程随时可能因为资源加载往里插条目,
                // 从工作线程边改边遍历会抛 InvalidOperationException。
                var referencedBy = new List<string>();
                if (includeReferencers)
                {
                    var ok = TtMainThreadDispatcher.Invoke(() =>
                    {
                        foreach (var other in TtEngine.Instance.AssetMetaManager.Assets.Values)
                        {
                            if (other == null || other.RefAssetRNames == null || other.AssetName == rn)
                                continue;
                            for (int i = 0; i < other.RefAssetRNames.Count; i++)
                            {
                                if (other.RefAssetRNames[i] == rn)
                                {
                                    referencedBy.Add(other.AssetName?.ToString() ?? "");
                                    break;
                                }
                            }
                        }
                    });
                    if (!ok)
                        referencedBy.Add("<timed out waiting for the engine main thread>");
                }

                string address = "";
                bool assetExists = false, metaExists = false;
                long assetSize = 0;
                string lastWrite = "";
                try
                {
                    address = rn.Address;
                    var fi = new System.IO.FileInfo(address);
                    assetExists = fi.Exists;
                    if (assetExists)
                    {
                        assetSize = fi.Length;
                        lastWrite = fi.LastWriteTimeUtc.ToString("o");
                    }
                    metaExists = System.IO.File.Exists(address + IO.IAssetMeta.MetaExt);
                }
                catch { }

                // 导入源溯源: 目前只有 vms 记了这个 (TtMeshPrimitivesAMeta.OriginSourceAddress),
                // 其他资产类型没有对应字段, 报空串而不是省略字段, 免得调用方以为是取值失败。
                // 顺带报文件是否还在: 路径是当时导入机器上的绝对路径, 换机器后会失效。
                string originSource = "";
                bool originSourceExists = false;
                if (meta is Graphics.Mesh.TtMeshPrimitivesAMeta meshMeta)
                {
                    originSource = meshMeta.OriginSourceAddress ?? "";
                    try { originSourceExists = originSource.Length > 0 && System.IO.File.Exists(originSource); }
                    catch { }
                }

                return JsonSerializer.Serialize(new
                {
                    found = true,
                    assetName = rn.ToString(),
                    typeExt = meta.TypeExt ?? "",
                    typeName = meta.GetAssetTypeName() ?? "",
                    assetId = meta.AssetId.ToString(),
                    originSourceAddress = originSource,
                    originSourceExists,
                    refAssetCount = refAssets.Count,
                    refAssets,
                    referencedByCount = referencedBy.Count,
                    referencedBy,
                    files,
                    onDisk = new
                    {
                        assetPath = address,
                        assetExists,
                        assetSizeBytes = assetSize,
                        assetLastWriteUtc = lastWrite,
                        metaExists,
                    },
                    note = assetExists ? null : "The .ameta is registered but the asset file is missing on disk; the asset will fail to load.",
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { found = false, error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("import_mesh_source",
            "WRITES ASSETS. Import a model source file into the project, producing .vms (and .ums/.uminst/.skt as " +
            "applicable) under targetDir, then report which assets appeared. Not undoable and it overwrites " +
            "same-named assets, so pass a targetDir you are willing to have written to. Intended for verifying " +
            "importer changes without driving the editor UI by hand.",
            returnDescription: "{imported, sourceFile, targetDir, settings:{unitScale, asStaticMesh, mergeMeshes, generateUMS, applyTransformToVertex, joinIdenticalVertices}, newAssetCount, newAssets, warnings, note}")]
        public static string ImportMeshSource(
            [Bricks.AIGC.TtMCPParameter("Filesystem path of the source model file")] string filePath,
            [Bricks.AIGC.TtMCPParameter("Destination directory as an engine RName, e.g. 'tutorials/animation/blendshape/'")] string targetDir,
            [Bricks.AIGC.TtMCPParameter("Source-to-meter scale. 0.01 for centimeter sources (FBX default), 1.0 for metric sources")] double unitScale = 0.01,
            [Bricks.AIGC.TtMCPParameter("Import as static mesh: drops skin weights and the embedded skeleton")] bool asStaticMesh = false,
            [Bricks.AIGC.TtMCPParameter("Merge every mesh node into one mesh")] bool mergeMeshes = false,
            [Bricks.AIGC.TtMCPParameter("Also generate the .ums material mesh")] bool generateUMS = true,
            [Bricks.AIGC.TtMCPParameter("Bake node scale/translation into vertices instead of importing in local space")] bool applyTransformToVertex = false,
            [Bricks.AIGC.TtMCPParameter("Let Assimp weld identical vertices")] bool joinIdenticalVertices = true,
            [Bricks.AIGC.TtMCPParameter("How long to wait for the import on the engine main thread, in seconds")] double timeoutSeconds = 180)
        {
            try
            {
                LogToolCall("import_mesh_source", $"filePath={filePath}, targetDir={targetDir}, unitScale={unitScale}");

                if (string.IsNullOrEmpty(filePath))
                    return JsonSerializer.Serialize(new { imported = false, error = "filePath is required." });
                if (string.IsNullOrEmpty(targetDir))
                    return JsonSerializer.Serialize(new { imported = false, error = "targetDir is required; this tool writes assets and will not guess a destination." });
                if (System.IO.File.Exists(filePath) == false)
                    return JsonSerializer.Serialize(new { imported = false, error = $"Source file not found: {filePath}" });

                var setting = Bricks.AssetImpExp.TtAssetImporter.CreateMeshImporter(filePath);
                if (setting == null)
                    return JsonSerializer.Serialize(new { imported = false, error = $"Assimp failed to read '{filePath}'." });

                setting.SourceFile = filePath;
                setting.UnitScale = (float)unitScale;
                setting.AsStaticMesh = asStaticMesh;
                setting.MergeMeshes = mergeMeshes;
                setting.GenerateUMS = generateUMS;
                setting.ApplyTransformToVertex = applyTransformToVertex;
                setting.JoinIdenticalVertices = joinIdenticalVertices;
                // PreviewEntries 留空 = 全选 (IsMeshNodeSelected/IsMaterialSelected 在空列表时返回 true)

                // JoinIdenticalVertices 不是 ImportAndSaveMesh 会读的开关: 焊接必须在 Assimp
                // 解析阶段由 PostProcessSteps 完成, 所以 UI 流程 (TtImportAssetsAttribute.DoImport)
                // 是先带上这个 flag 重新解析一遍文件, 再进导入。这里必须照做, 否则这个参数是空转的 ——
                // 曾经漏了这一步, 导入出来的网格顶点没焊接(一个球从 726 个顶点变成 1200 面 x 3),
                // 资产大了几倍, 而且每个顶点只属于一个三角形, 导致 morph 法线重算退化成面法线。
                if (joinIdenticalVertices)
                {
                    var sceneFlags = Bricks.AssetImpExp.TtAssetImporter.DefaultSceneFlags
                        | Assimp.PostProcessSteps.JoinIdenticalVertices;
                    setting.AssetImporter.ReImport(sceneFlags);
                    if (setting.AssetImporter.AiScene == null)
                        return JsonSerializer.Serialize(new { imported = false, error = $"Assimp failed to re-read '{filePath}' with JoinIdenticalVertices; pass joinIdenticalVertices=false to import the unwelded mesh." });
                }

                var warnings = BuildImportSupportWarnings(setting.AssetImporter.AiScene, setting.UnitScaleFactor);
                var dir = RName.GetRName(targetDir);

                bool importOk = false;
                var before = new HashSet<string>();
                var newAssets = new List<string>();
                int timeoutMs = Math.Max(5000, (int)(timeoutSeconds * 1000));

                // 整个导入放在主线程: ImportAndSaveMesh 会建 GPU buffer 并注册 meta。
                // TtTask 的同步等待必须用 GetResultUntilCompleted, 它内部走 WaitTask 由本线程
                // (此刻正是主线程) 把任务泵完; GetAwaiter().GetResult() 会直接死锁。
                var ok = TtMainThreadDispatcher.Invoke(() =>
                {
                    foreach (var kv in TtEngine.Instance.AssetMetaManager.RNameAssets)
                        before.Add(kv.Key.ToString());

                    importOk = Graphics.Mesh.TtMeshPrimitives.ImportAttribute
                        .ImportAndSaveMesh(dir, setting).GetResultUntilCompleted();

                    foreach (var kv in TtEngine.Instance.AssetMetaManager.RNameAssets)
                    {
                        var name = kv.Key.ToString();
                        if (before.Contains(name) == false)
                            newAssets.Add(name);
                    }
                }, timeoutMs);

                if (!ok)
                    return JsonSerializer.Serialize(new { imported = false, error = $"Timed out after {timeoutMs} ms waiting for the import on the engine main thread. It may still be running; check get_recent_logs." });

                return JsonSerializer.Serialize(new
                {
                    imported = importOk,
                    sourceFile = filePath,
                    targetDir = dir.ToString(),
                    settings = new
                    {
                        unitScale = setting.UnitScale,
                        asStaticMesh = setting.AsStaticMesh,
                        mergeMeshes = setting.MergeMeshes,
                        generateUMS = setting.GenerateUMS,
                        applyTransformToVertex = setting.ApplyTransformToVertex,
                        joinIdenticalVertices = setting.JoinIdenticalVertices,
                    },
                    newAssetCount = newAssets.Count,
                    newAssets,
                    warnings,
                    note = importOk
                        ? "Import reported success. Verify the payload with inspect_mesh_asset -- newAssets only lists assets that were not registered before, so overwritten assets do not show up here."
                        : "ImportAndSaveMesh returned false. Check get_recent_logs for the IO-category error.",
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { imported = false, error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("import_animation_source",
            "WRITES ASSETS. Import the animations of a source file into .animclip assets under targetDir. Only " +
            "node (skeletal) animation is supported by the engine importer -- morph weight curves and vertex " +
            "animation are dropped, and a file whose animations carry no node channel will trip Debug.Assert in the " +
            "importer. Check inspect_source_file first. Not undoable.",
            returnDescription: "{imported, sourceFile, targetDir, animationCount, settings:{unitScale, ignoreScale}, newAssetCount, newAssets, warnings, note}")]
        public static string ImportAnimationSource(
            [Bricks.AIGC.TtMCPParameter("Filesystem path of the source animation file")] string filePath,
            [Bricks.AIGC.TtMCPParameter("Destination directory as an engine RName, e.g. 'tutorials/animation/blendshape/'")] string targetDir,
            [Bricks.AIGC.TtMCPParameter("Source-to-meter scale for translation tracks. 0.01 for centimeter sources")] double unitScale = 0.01,
            [Bricks.AIGC.TtMCPParameter("Drop scale tracks from the imported curves")] bool ignoreScale = true,
            [Bricks.AIGC.TtMCPParameter("How long to wait for the import on the engine main thread, in seconds")] double timeoutSeconds = 180)
        {
            try
            {
                LogToolCall("import_animation_source", $"filePath={filePath}, targetDir={targetDir}, unitScale={unitScale}");

                if (string.IsNullOrEmpty(filePath))
                    return JsonSerializer.Serialize(new { imported = false, error = "filePath is required." });
                if (string.IsNullOrEmpty(targetDir))
                    return JsonSerializer.Serialize(new { imported = false, error = "targetDir is required; this tool writes assets and will not guess a destination." });
                if (System.IO.File.Exists(filePath) == false)
                    return JsonSerializer.Serialize(new { imported = false, error = $"Source file not found: {filePath}" });

                var setting = Bricks.AssetImpExp.TtAssetImporter.CreateAnimationImporter(filePath);
                if (setting == null || setting.AssetImporter?.AiScene == null)
                    return JsonSerializer.Serialize(new { imported = false, error = $"Assimp failed to read '{filePath}'." });

                setting.UnitScale = (float)unitScale;
                setting.IgnoreScale = ignoreScale;

                var scene = setting.AssetImporter.AiScene;
                var warnings = BuildImportSupportWarnings(scene, setting.UnitScaleFactor);
                if (scene.AnimationCount == 0)
                    return JsonSerializer.Serialize(new { imported = false, animationCount = 0, warnings, error = "The source file contains no animation at all; nothing to import." });

                var dir = RName.GetRName(targetDir);
                bool importOk = false;
                var before = new HashSet<string>();
                var newAssets = new List<string>();
                int timeoutMs = Math.Max(5000, (int)(timeoutSeconds * 1000));

                var ok = TtMainThreadDispatcher.Invoke(() =>
                {
                    foreach (var kv in TtEngine.Instance.AssetMetaManager.RNameAssets)
                        before.Add(kv.Key.ToString());

                    importOk = Animation.Asset.TtAnimationClip.ImportAttribute.ImportAndSaveAnimation(dir, setting);

                    foreach (var kv in TtEngine.Instance.AssetMetaManager.RNameAssets)
                    {
                        var name = kv.Key.ToString();
                        if (before.Contains(name) == false)
                            newAssets.Add(name);
                    }
                }, timeoutMs);

                if (!ok)
                    return JsonSerializer.Serialize(new { imported = false, error = $"Timed out after {timeoutMs} ms waiting for the import on the engine main thread. It may still be running; check get_recent_logs." });

                return JsonSerializer.Serialize(new
                {
                    imported = importOk,
                    sourceFile = filePath,
                    targetDir = dir.ToString(),
                    animationCount = scene.AnimationCount,
                    settings = new
                    {
                        unitScale = setting.UnitScale,
                        ignoreScale = setting.IgnoreScale,
                    },
                    newAssetCount = newAssets.Count,
                    newAssets,
                    warnings,
                    note = importOk
                        ? "Import reported success; verify with get_animation_clip_info."
                        : "ImportAndSaveAnimation returned false. Check get_recent_logs for the IO-category error.",
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { imported = false, error = ex.Message });
            }
        }
    }
}
