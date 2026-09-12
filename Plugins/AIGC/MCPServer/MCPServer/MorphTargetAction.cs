using System;
using System.Collections.Generic;
using System.Text.Json;
using EngineNS.GamePlay.Scene;

namespace EngineNS.Plugins.MCPServer
{
    /// <summary>
    /// Morph target (BlendShape) 调试 MCP 工具。
    ///
    /// 为什么单独一组工具: 骨骼动画那套 (get_final_local_pose / get_mesh_runtime_pose) 只能看到
    /// 骨骼 pose, 而 morph 走的是完全独立的通路 —— 资产里的稀疏 delta + TtMorphModifier 的权重表,
    /// 权重变化不经过 pose 也不经过任何 AnimPlayer, 用 pose 工具查 morph 一定是 0 命中, 会把
    /// "morph 没生效"误判成"资产没数据"。
    ///
    /// 排查 morph 不生效的固定顺序 (缺一环都表现为"网格纹丝不动"):
    ///   1) list_morph_targets  —— 资产里到底有没有 shape 数据, delta 幅度是否非零;
    ///   2) get_morph_state     —— 运行时 MdfQueue 是否挂了 TtMorphModifier, 顶点数是否对得上;
    ///   3) sample_morph_weights—— 权重是否随时间变化 (区分"没人驱动"与"驱动了但形变没出来");
    ///   4) set_morph_weight    —— 手动打到 1.0, 用来确认形变通路本身是通的。
    /// </summary>
    public partial class TtMCPServerPlugin
    {
        #region morph helpers

        private static Graphics.Mesh.Modifier.TtMorphModifier GetMorphModifier(TtMeshNode node)
        {
            return node?.RenderMesh?.MdfQueue?.FindModifier<Graphics.Mesh.Modifier.TtMorphModifier>();
        }

        /// <summary> 该 mesh 节点引用的资产里是否带 morph 数据(与 modifier 是否存在无关)。 </summary>
        private static bool MeshAssetHasMorph(TtMeshNode node)
        {
            var subMeshes = node?.RenderMesh?.MaterialMesh?.SubMeshes;
            if (subMeshes == null)
                return false;
            for (int i = 0; i < subMeshes.Count; i++)
            {
                var set = subMeshes[i]?.Mesh?.MorphTargets;
                if (set != null && set.IsValid)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 定位待检查的 mesh 节点。meshName 为空时挑第一个"资产带 morph"的节点 —— 预览编辑器里
        /// 节点名固定是 "PreviewObject", 让调用方去猜这个名字没有意义。
        /// </summary>
        private static TtMeshNode FindMorphMeshNode(GamePlay.TtWorld world, string meshName)
        {
            if (!string.IsNullOrEmpty(meshName))
                return FindMeshNodeByName(world, meshName);

            if (world?.Root == null)
                return null;
            TtMeshNode found = null;
            world.Root.IterateNodes((n, _) =>
            {
                if (n is TtMeshNode mn && MeshAssetHasMorph(mn))
                {
                    found = mn;
                    return false;
                }
                return true;
            }, null);
            return found;
        }

        private static float Vec3Length(Vector3 v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }

        /// <summary>
        /// 统计一个 morph target 的 delta 幅度。零幅度的 target 是"导入把 shape 吃掉了"的典型信号:
        /// 名字在、条目在、但权重拉满也看不出变化。
        /// </summary>
        private static Dictionary<string, object> SerializeMorphTarget(Graphics.Mesh.TtMorphTarget target)
        {
            var entry = new Dictionary<string, object>
            {
                ["name"] = target?.Name ?? "",
                ["deltaCount"] = target?.DeltaCount ?? 0,
            };
            var deltas = target?.Deltas;
            float maxPos = 0f, maxNormal = 0f, maxTangent = 0f, sumPos = 0f;
            uint minIndex = uint.MaxValue, maxIndex = 0;
            if (deltas != null && deltas.Length > 0)
            {
                for (int i = 0; i < deltas.Length; i++)
                {
                    var lenPos = Vec3Length(deltas[i].DeltaPosition);
                    var lenNrm = Vec3Length(deltas[i].DeltaNormal);
                    var lenTan = Vec3Length(deltas[i].DeltaTangent);
                    if (lenPos > maxPos) maxPos = lenPos;
                    if (lenNrm > maxNormal) maxNormal = lenNrm;
                    if (lenTan > maxTangent) maxTangent = lenTan;
                    sumPos += lenPos;
                    if (deltas[i].VertexIndex < minIndex) minIndex = deltas[i].VertexIndex;
                    if (deltas[i].VertexIndex > maxIndex) maxIndex = deltas[i].VertexIndex;
                }
            }
            else
            {
                minIndex = 0;
            }
            // 长度单位是米 (CodingGuidelines.md §6), 调用方看到 0.01 别当成厘米
            entry["maxDeltaPositionMeters"] = maxPos;
            entry["avgDeltaPositionMeters"] = deltas != null && deltas.Length > 0 ? sumPos / deltas.Length : 0f;
            entry["maxDeltaNormalLength"] = maxNormal;
            // 法线 delta 全零 => DoMorphModifierVS 里 vNormal + 0 == vNormal: 形状形变了而光照
            // 维持基础网格的朝向。这不是渲染 bug, 别去查管线; 但也不要在这里断定根因。
            //
            // 曾经这里写的是"全零 == 源 blendshape 没带法线", 那个断言把排查带进了死胡同:
            // 全零至少有三种成因(源没带 / 源带了但只是 base 副本 / 网格没面拓扑), 而这一层
            // 只看得到资产里的 delta, 三者在这里长得一模一样。现在导入器会在源没有可用
            // 法线时自己重算, 所以全零只能说明重算也没发生, 定性交给 inspect_source_file。
            entry["normalsPresent"] = maxNormal > 0f;
            entry["maxDeltaTangentLength"] = maxTangent;
            // 切线 delta 全零不一定是毛病: 旧资产(Version 1)本就没有这个字段, 读进来就是零向量;
            // 而 v2 下全零则意味着重算没跑(没面拓扑或没 UV0)。两者在这一层长得一模一样,
            // 所以只报事实, 不在这里断定根因。
            entry["tangentsPresent"] = maxTangent > 0f;
            if (maxPos > 0f && maxNormal <= 0f)
            {
                entry["normalNote"] = "Position deltas exist but every normal delta is zero, so shading keeps the base " +
                    "mesh normals no matter the weight. The importer recomputes normals when the source carries none, " +
                    "so this means the recompute did not run: either the asset predates that behaviour (reimport it), " +
                    "or the base mesh has no face topology to recompute from. Run inspect_source_file on the source and " +
                    "read morphShapes[].hasNormals / normalsDifferFromBase plus hasFaces to tell those apart.";
            }
            entry["vertexIndexRange"] = new[] { minIndex, maxIndex };
            return entry;
        }

        #endregion

        [Bricks.AIGC.TtMCPTool("list_morph_targets",
            "Inspect the morph target (BlendShape) data stored in a mesh asset (.vms). Reports the shape data only " +
            "-- it says nothing about whether anything animates the weights at runtime; use get_morph_state and " +
            "sample_morph_weights for that. Delta lengths are in meters.",
            returnDescription: "{found, assetName, hasMorphData, morphVertexCount, meshVertexCount, vertexCountMatches, " +
            "targetCount, targets:[{name, deltaCount, maxDeltaPositionMeters, avgDeltaPositionMeters, maxDeltaNormalLength, " +
            "normalsPresent, normalNote, maxDeltaTangentLength, tangentsPresent, vertexIndexRange}], warnings, note}")]
        public static string ListMorphTargets(
            [Bricks.AIGC.TtMCPParameter("Mesh asset name, e.g. 'tutorials/animation/blendshape/sk_box_morph_1.vms'. Empty picks the morph-carrying mesh in the active world.")] string meshAsset = "")
        {
            try
            {
                LogToolCall("list_morph_targets", $"meshAsset={meshAsset}");

                Graphics.Mesh.TtMeshPrimitives primitives = null;
                string resolvedName = meshAsset;

                if (!string.IsNullOrEmpty(meshAsset))
                {
                    var rn = RName.GetRName(meshAsset);
                    primitives = RunSync(() => TtEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(rn));
                    if (primitives == null)
                        return JsonSerializer.Serialize(new { found = false, error = $"Failed to load mesh asset '{meshAsset}'." });
                }
                else
                {
                    var world = GetActiveWorld();
                    var node = FindMorphMeshNode(world, "");
                    var subMeshes = node?.RenderMesh?.MaterialMesh?.SubMeshes;
                    if (subMeshes != null)
                    {
                        for (int i = 0; i < subMeshes.Count; i++)
                        {
                            var set = subMeshes[i]?.Mesh?.MorphTargets;
                            if (set != null && set.IsValid)
                            {
                                primitives = subMeshes[i].Mesh;
                                break;
                            }
                        }
                    }
                    if (primitives == null)
                        return JsonSerializer.Serialize(new { found = false, error = "No morph-carrying mesh found in the active world; pass meshAsset explicitly." });
                    resolvedName = primitives.AssetName?.ToString() ?? "";
                }

                var morphSet = primitives.MorphTargets;
                int meshVertexCount = (int)primitives.VertexNumber;
                if (morphSet == null)
                {
                    return JsonSerializer.Serialize(new
                    {
                        found = true,
                        assetName = resolvedName,
                        hasMorphData = false,
                        meshVertexCount,
                        targetCount = 0,
                        note = "Asset has no MorphTargets attribute. Either the source file carries no BlendShape, " +
                               "or the mesh was imported before morph support existed -- reimport the source file.",
                    });
                }

                var targets = new List<Dictionary<string, object>>();
                if (morphSet.Targets != null)
                {
                    for (int i = 0; i < morphSet.Targets.Count; i++)
                        targets.Add(SerializeMorphTarget(morphSet.Targets[i]));
                }

                bool matches = morphSet.VertexCount == meshVertexCount;

                // 把“拉满滑杆也看不出变化”的几种真因直接报出来, 不要让调用方自己回去读管线。
                var warnings = new List<string>();
                int normalLessCount = 0;
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].ContainsKey("normalNote"))
                        normalLessCount++;
                }
                if (normalLessCount > 0)
                {
                    warnings.Add($"{normalLessCount} of {targets.Count} target(s) have position deltas but zero normal " +
                        "deltas, so the shape deforms while lighting keeps the base mesh normals. The importer recomputes " +
                        "normals for sources that carry none, so a zero here means that recompute never ran -- reimport " +
                        "the asset, and if it is still zero check inspect_source_file for hasFaces on the base mesh.");
                }

                int tangentLessCount = 0;
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].TryGetValue("tangentsPresent", out var present) && present is bool ok && ok == false)
                        tangentLessCount++;
                }
                if (tangentLessCount > 0)
                {
                    warnings.Add($"{tangentLessCount} of {targets.Count} target(s) carry zero tangent deltas. Since " +
                        "TtMorphTargetSet version 2 the importer recomputes tangents from the morphed positions plus UV0, " +
                        "so a zero means either the asset was saved as version 1 (reimport it) or the base mesh has no " +
                        "face topology / no UV0 to recompute from. Consequence: normal-mapped materials keep the base " +
                        "mesh tangent basis, and DoMorphModifierVS can only re-orthogonalize it against the morphed " +
                        "normal (Gram-Schmidt) -- that fixes the tangent tilt from the surface changing orientation but " +
                        "not the tangent rotation about the normal.");
                }

                return JsonSerializer.Serialize(new
                {
                    found = true,
                    assetName = resolvedName,
                    hasMorphData = morphSet.IsValid,
                    morphVertexCount = morphSet.VertexCount,
                    meshVertexCount,
                    vertexCountMatches = matches,
                    targetCount = targets.Count,
                    targets,
                    warnings,
                    note = matches
                        ? "Shape data present. Nothing in the engine drives these weights automatically -- morph " +
                          "weights are only set by the MeshPrimitiveEditor's MorphTargets sliders or by an explicit " +
                          "TtMorphModifier.SetMorphWeight call."
                        : "Vertex count mismatch: TtMorphModifier will discard the morph data at Initialize time. " +
                          "The mesh was reimported without regenerating morph deltas.",
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { found = false, error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("get_morph_state",
            "Read the runtime morph state of a mesh node: which MdfQueue it uses, whether a TtMorphModifier is " +
            "actually present in the modifier chain, and the current weight of every morph target. A missing " +
            "TtMorphModifier is the usual reason morph silently does nothing even though the asset has shape data.",
            returnDescription: "{found, meshName, mdfQueueType, assetHasMorph, hasMorphModifier, morphCount, " +
            "allWeightsZero, weights:[{name, weight}], subMeshes:[{index, asset, meshVertexCount, morphVertexCount, targetCount}], note}")]
        public static string GetMorphState(
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name. Empty picks the first morph-carrying mesh node.")] string meshName = "")
        {
            try
            {
                LogToolCall("get_morph_state", $"meshName={meshName}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world available." });

                var node = FindMorphMeshNode(world, meshName);
                if (node == null)
                    return JsonSerializer.Serialize(new { found = false, error = string.IsNullOrEmpty(meshName) ? "No morph-carrying mesh node in the active world." : $"Mesh node '{meshName}' not found." });

                string payload = null;
                var ok = TtMainThreadDispatcher.Invoke(() =>
                {
                    var renderMesh = node.RenderMesh;
                    var modifier = GetMorphModifier(node);
                    var assetHasMorph = MeshAssetHasMorph(node);

                    var subMeshInfos = new List<Dictionary<string, object>>();
                    var subMeshes = renderMesh?.MaterialMesh?.SubMeshes;
                    if (subMeshes != null)
                    {
                        for (int i = 0; i < subMeshes.Count; i++)
                        {
                            var primitives = subMeshes[i]?.Mesh;
                            if (primitives == null)
                                continue;
                            var set = primitives.MorphTargets;
                            subMeshInfos.Add(new Dictionary<string, object>
                            {
                                ["index"] = i,
                                ["asset"] = primitives.AssetName?.ToString() ?? "",
                                ["meshVertexCount"] = (int)primitives.VertexNumber,
                                ["morphVertexCount"] = set?.VertexCount ?? 0,
                                ["targetCount"] = set?.Targets?.Count ?? 0,
                            });
                        }
                    }

                    var weights = new List<Dictionary<string, object>>();
                    bool allZero = true;
                    if (modifier != null)
                    {
                        var names = new List<string>();
                        modifier.CollectMorphNames(names);
                        for (int i = 0; i < names.Count; i++)
                        {
                            var w = modifier.GetMorphWeight(names[i]);
                            if (Math.Abs(w) > 1e-4f)
                                allZero = false;
                            weights.Add(new Dictionary<string, object>
                            {
                                ["name"] = names[i],
                                ["weight"] = w,
                            });
                        }
                    }

                    string note;
                    if (modifier == null)
                    {
                        note = assetHasMorph
                            ? "Asset has morph data but the MdfQueue has no TtMorphModifier, so morph is silently " +
                              "ignored. Set MdfQueueType to TtMdfSkinMorphMesh (skinned) or TtMdfMorphMesh (static)."
                            : "Neither the asset nor the MdfQueue carries morph data.";
                    }
                    else if (weights.Count == 0)
                    {
                        note = "TtMorphModifier is present but exposes no morph names -- the modifier dropped the " +
                               "morph set at Initialize (usually a vertex count mismatch; check list_morph_targets).";
                    }
                    else if (allZero)
                    {
                        note = "All weights are 0, so the mesh renders in its base shape. Nothing in the engine " +
                               "drives morph weights over time: there is no morph curve in animation clips and no " +
                               "anim node writes SetMorphWeight. Drag the editor's MorphTargets slider or call " +
                               "set_morph_weight to deform it.";
                    }
                    else
                    {
                        note = "Non-zero weights are applied; the mesh should be deformed.";
                    }

                    payload = JsonSerializer.Serialize(new
                    {
                        found = true,
                        meshName = node.NodeName ?? "",
                        mdfQueueType = renderMesh?.MdfQueue?.GetType().Name ?? "",
                        assetHasMorph,
                        hasMorphModifier = modifier != null,
                        morphCount = weights.Count,
                        allWeightsZero = weights.Count > 0 && allZero,
                        weights,
                        subMeshes = subMeshInfos,
                        note,
                    });
                });
                if (!ok)
                    return JsonSerializer.Serialize(new { found = false, error = "Timed out waiting for the engine main thread." });
                return payload;
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { found = false, error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("set_morph_weight",
            "Set a morph target weight on a live mesh node and read it back. This is a runtime-only preview value: " +
            "it is never saved into the asset and is not undoable. Use it to prove the morph deformation path works " +
            "end to end (asset delta -> dense buffer -> VS).",
            returnDescription: "{applied, meshName, morphName, requestedWeight, actualWeight, availableNames, note}")]
        public static string SetMorphWeight(
            [Bricks.AIGC.TtMCPParameter("Morph target name as reported by get_morph_state")] string morphName,
            [Bricks.AIGC.TtMCPParameter("Weight to apply, normally 0..1")] double weight = 1.0,
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name. Empty picks the first morph-carrying mesh node.")] string meshName = "",
            [Bricks.AIGC.TtMCPParameter("Set true to zero every weight before applying (ignores morphName when weight is 0)")] bool resetOthers = false)
        {
            try
            {
                LogToolCall("set_morph_weight", $"meshName={meshName}, morphName={morphName}, weight={weight}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { applied = false, error = "No active world available." });

                var node = FindMorphMeshNode(world, meshName);
                if (node == null)
                    return JsonSerializer.Serialize(new { applied = false, error = string.IsNullOrEmpty(meshName) ? "No morph-carrying mesh node in the active world." : $"Mesh node '{meshName}' not found." });

                string payload = null;
                // 必须回主线程: SetMorphWeight 改的权重表会被 OnDrawCall 里的 AccumulateDeltas 读,
                // 从 MCP 工作线程直接写是竞态 (见 TtMainThreadDispatcher 的说明)。
                var ok = TtMainThreadDispatcher.Invoke(() =>
                {
                    var modifier = GetMorphModifier(node);
                    if (modifier == null)
                    {
                        payload = JsonSerializer.Serialize(new
                        {
                            applied = false,
                            meshName = node.NodeName ?? "",
                            error = "No TtMorphModifier on this mesh's MdfQueue; nothing to set. See get_morph_state.",
                        });
                        return;
                    }

                    var names = new List<string>();
                    modifier.CollectMorphNames(names);

                    if (resetOthers)
                        modifier.ResetMorphWeights();

                    float applied = 0f;
                    bool nameKnown = false;
                    if (!string.IsNullOrEmpty(morphName))
                    {
                        nameKnown = names.Contains(morphName);
                        modifier.SetMorphWeight(morphName, (float)weight);
                        applied = modifier.GetMorphWeight(morphName);
                    }

                    payload = JsonSerializer.Serialize(new
                    {
                        applied = nameKnown,
                        meshName = node.NodeName ?? "",
                        morphName,
                        requestedWeight = (float)weight,
                        actualWeight = applied,
                        availableNames = names,
                        note = nameKnown
                            ? "Weight written. The dense delta buffer is rebuilt on the next drawcall of this mesh."
                            : "Morph name not found on this mesh, so SetMorphWeight was a no-op (it matches by name, " +
                              "never by index). Pick one of availableNames.",
                    });
                });
                if (!ok)
                    return JsonSerializer.Serialize(new { applied = false, error = "Timed out waiting for the engine main thread." });
                return payload;
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { applied = false, error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("sample_morph_weights",
            "Sample every morph weight of a mesh node repeatedly over a short window and report the maximum change. " +
            "Definitively answers 'is anything animating this BlendShape' -- isAnimating=false means no driver exists " +
            "(expected today: morph weight curves are not imported and no anim node writes them), so a static preview " +
            "is not a rendering bug.",
            returnDescription: "{found, meshName, samples, intervalMs, morphCount, isAnimating, maxWeightChange, " +
            "perMorph:[{name, first, last, maxDelta}], note}")]
        public static string SampleMorphWeights(
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name. Empty picks the first morph-carrying mesh node.")] string meshName = "",
            [Bricks.AIGC.TtMCPParameter("Number of samples to take (2-30)")] double samples = 8,
            [Bricks.AIGC.TtMCPParameter("Milliseconds between samples (10-500)")] double intervalMs = 60,
            [Bricks.AIGC.TtMCPParameter("Weight change above which the morph counts as animating")] double movingThreshold = 0.0001)
        {
            try
            {
                LogToolCall("sample_morph_weights", $"meshName={meshName}, samples={samples}, intervalMs={intervalMs}");
                int sampleCount = Math.Max(2, Math.Min(30, (int)samples));
                int interval = Math.Max(10, Math.Min(500, (int)intervalMs));

                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world available." });

                var node = FindMorphMeshNode(world, meshName);
                if (node == null)
                    return JsonSerializer.Serialize(new { found = false, error = string.IsNullOrEmpty(meshName) ? "No morph-carrying mesh node in the active world." : $"Mesh node '{meshName}' not found." });

                var modifier = GetMorphModifier(node);
                if (modifier == null)
                    return JsonSerializer.Serialize(new { found = false, meshName = node.NodeName ?? "", error = "No TtMorphModifier on this mesh's MdfQueue; there is no weight to sample. See get_morph_state." });

                var names = new List<string>();
                modifier.CollectMorphNames(names);
                if (names.Count == 0)
                    return JsonSerializer.Serialize(new { found = true, meshName = node.NodeName ?? "", morphCount = 0, isAnimating = false, note = "Modifier exposes no morph names; nothing to sample." });

                var first = new float[names.Count];
                var last = new float[names.Count];
                var prev = new float[names.Count];
                var maxDelta = new float[names.Count];
                for (int s = 0; s < sampleCount; s++)
                {
                    // 逐次采样都读主线程当帧的值; 工作线程 Sleep 的间隙引擎照常 tick,
                    // 所以有驱动时这里能看到变化。
                    var ok = TtMainThreadDispatcher.Invoke(() =>
                    {
                        for (int i = 0; i < names.Count; i++)
                        {
                            var w = modifier.GetMorphWeight(names[i]);
                            if (s == 0)
                            {
                                first[i] = w;
                                prev[i] = w;
                            }
                            else
                            {
                                var d = Math.Abs(w - prev[i]);
                                if (d > maxDelta[i]) maxDelta[i] = d;
                                prev[i] = w;
                            }
                            last[i] = w;
                        }
                    });
                    if (!ok)
                        return JsonSerializer.Serialize(new { found = false, error = "Timed out waiting for the engine main thread." });
                    if (s < sampleCount - 1)
                        System.Threading.Thread.Sleep(interval);
                }

                float globalMax = 0f;
                var perMorph = new List<Dictionary<string, object>>();
                for (int i = 0; i < names.Count; i++)
                {
                    if (maxDelta[i] > globalMax) globalMax = maxDelta[i];
                    perMorph.Add(new Dictionary<string, object>
                    {
                        ["name"] = names[i],
                        ["first"] = first[i],
                        ["last"] = last[i],
                        ["maxDelta"] = maxDelta[i],
                    });
                }

                bool isAnimating = globalMax > (float)movingThreshold;
                return JsonSerializer.Serialize(new
                {
                    found = true,
                    meshName = node.NodeName ?? "",
                    samples = sampleCount,
                    intervalMs = interval,
                    morphCount = names.Count,
                    isAnimating,
                    maxWeightChange = globalMax,
                    perMorph,
                    note = isAnimating
                        ? "Weights change between samples, so something is driving the morph."
                        : "Weights are constant across all samples: no driver. The engine imports BlendShape shapes " +
                          "but not their weight curves (AnimationChunkGenerater has no morph branch), and no anim " +
                          "node/player calls SetMorphWeight, so morph never animates on its own.",
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { found = false, error = ex.Message });
            }
        }
    }
}
