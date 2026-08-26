using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh.Modifier
{
    /// <summary>
    /// Morph target (BlendShape) 顶点变形 modifier。
    ///
    /// 职责: 把 mesh 资产里的稀疏 morph delta 按当前权重累加成一张逐顶点稠密表上传 GPU,
    /// VS 端 DoMorphModifierVS 按 vVertexID 索引读取并叠加到 vert 上。
    ///
    /// 执行顺序: 必须排在 skin modifier 之前 (TtMdfQueue2&lt;TtMorphModifier, TtSkinModifier&gt;)。
    /// morph delta 是 mesh space 的偏移, 若在骨骼混合之后叠加, delta 就不会跟随骨骼旋转,
    /// 表现为形变方向不随角色朝向。
    ///
    /// 与其他 modifier 的差异: 本类没有 native 对应物 (纯 C# 实现 IMeshModifier)。引擎 C# 侧
    /// 从不访问 modifier 的 mCoreObject, 因此无需为 morph 增加一个空的 native modifier。
    ///
    /// 状态按 SubMesh 分开持有: 一个 MdfQueue 服务整个 TtMaterialMesh, 而每个 SubMesh 的
    /// TtMeshPrimitives 有各自独立的顶点编号(即各自独立的 vVertexID 空间), 稠密表不能混用。
    /// </summary>
    public class TtMorphModifier : Pipeline.Shader.IMeshModifier
    {
        /// <summary> 权重绝对值低于此值视为未激活, 不参与累加。 </summary>
        const float WeightEpsilon = 1e-4f;

        /// <summary> HLSL 端 StructuredBuffer&lt;FMorphDelta&gt; 的变量名, 两端必须一致。 </summary>
        const string MorphDeltasShaderName = "MorphDeltas";

        /// <summary> 单个 SubMesh 的 morph 运行时状态。 </summary>
        class TtMorphMeshState : IDisposable
        {
            public TtMorphTargetSet MorphSet;
            public int VertexCount;

            /// <summary> 与 MorphSet.Targets 等长的权重表; MorphSet 为空时为 null。 </summary>
            public float[] Weights;

            /// <summary> 逐顶点稠密累加表, 长度恒为 VertexCount, 创建后不再扩容。 </summary>
            public Pipeline.TtCpu2GpuBuffer<FMorphDelta> DeltaBuffer;

            /// <summary>
            /// 初值为 true: 首次必须 flush 一次, 保证 GPU 端拿到确定的零值而不是未初始化内存
            /// (同 CodingGuidelines.md §1.1 对 cbuffer 的要求, 这里是 structured buffer 版)。
            /// </summary>
            public bool Dirty = true;

            /// <summary>
            /// 上一次累加中被写过的顶点索引。下次重算时只把这些位置清零, 避免每帧全量 clear
            /// (全量 clear 是 O(VertexCount), 而实际被 morph 影响的顶点通常只有几百个)。
            /// 允许有重复项, 重复清零无副作用。
            /// </summary>
            public List<uint> TouchedVertices = new List<uint>();

            public void Dispose()
            {
                if (DeltaBuffer != null)
                {
                    DeltaBuffer.Dispose();
                    DeltaBuffer = null;
                }
                MorphSet = null;
                Weights = null;
                TouchedVertices?.Clear();
            }
        }

        TtMorphMeshState[] mStates = null;

        public string ModifierNameVS { get => "DoMorphModifierVS"; }
        public string ModifierNamePS { get => null; }

        public RName SourceName
        {
            get
            {
                return RName.GetRName("shaders/modifier/MorphModifier.cginc", RName.ERNameType.Engine);
            }
        }

        public unsafe NxRHI.FShaderCode* GetHLSLCode(string includeName, string includeOriName)
        {
            return (NxRHI.FShaderCode*)0;
        }

        public string GetUniqueText()
        {
            return "";
        }

        public NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal };
        }

        public Graphics.Pipeline.Shader.EPixelShaderInput[] GetPSNeedInputs()
        {
            return null;
        }

        public void Dispose()
        {
            if (mStates != null)
            {
                for (int i = 0; i < mStates.Length; i++)
                {
                    mStates[i]?.Dispose();
                }
                mStates = null;
            }
        }

        public void Initialize(Graphics.Mesh.TtMaterialMesh materialMesh)
        {
            Dispose();

            if (materialMesh == null || materialMesh.SubMeshes == null)
                return;

            mStates = new TtMorphMeshState[materialMesh.SubMeshes.Count];
            for (int i = 0; i < materialMesh.SubMeshes.Count; i++)
            {
                var meshPrimitives = materialMesh.SubMeshes[i]?.Mesh;
                if (meshPrimitives == null)
                    continue;

                var state = new TtMorphMeshState();
                state.VertexCount = (int)meshPrimitives.VertexNumber;
                state.MorphSet = meshPrimitives.MorphTargets;

                if (state.MorphSet != null && state.MorphSet.IsValid)
                {
                    if (state.MorphSet.VertexCount != state.VertexCount)
                    {
                        // mesh 被重导入过而 morph 数据没跟上: 顶点编号已经对不上, 强行使用会让
                        // delta 落到错误的顶点上。丢弃 morph 数据比画错更安全。
                        Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning, "Mesh",
                            $"TtMorphModifier: morph vertex count mismatch on {meshPrimitives.AssetName} " +
                            $"(morph={state.MorphSet.VertexCount}, mesh={state.VertexCount}), morph data ignored");
                        state.MorphSet = null;
                    }
                    else
                    {
                        state.Weights = new float[state.MorphSet.Targets.Count];
                    }
                }
                else
                {
                    state.MorphSet = null;
                }

                if (state.MorphSet == null)
                {
                    // 没有可用 morph 数据时依然分配并绑定零值稠密表: shader 端无条件按 vVertexID
                    // 取值, 不绑或长度不足会读到未定义数据。
                    Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Info, "Mesh",
                        $"TtMorphModifier: no morph data on {meshPrimitives.AssetName}, " +
                        $"consider using a non-morph MdfQueue to save memory");
                }

                if (state.VertexCount > 0)
                {
                    state.DeltaBuffer = new Pipeline.TtCpu2GpuBuffer<FMorphDelta>();
                    state.DeltaBuffer.Initialize(NxRHI.EBufferType.BFT_SRV);
                    state.DeltaBuffer.SetSize(state.VertexCount);
                    // TtNativeArray.SetSize 不保证清零, 显式写一遍零值
                    var zero = new FMorphDelta();
                    for (int v = 0; v < state.VertexCount; v++)
                    {
                        state.DeltaBuffer.UpdateData(v, in zero);
                    }
                }

                mStates[i] = state;
            }
        }

        #region 权重接口

        /// <summary>
        /// 收集所有 SubMesh 上出现过的 morph 名(去重), 供编辑器列出可调节项。
        /// </summary>
        public void CollectMorphNames(List<string> outNames)
        {
            if (outNames == null || mStates == null)
                return;
            for (int i = 0; i < mStates.Length; i++)
            {
                var set = mStates[i]?.MorphSet;
                if (set == null)
                    continue;
                for (int t = 0; t < set.Targets.Count; t++)
                {
                    var name = set.Targets[t].Name;
                    if (outNames.Contains(name) == false)
                        outNames.Add(name);
                }
            }
        }

        /// <summary>
        /// 按名字设权重。同名 morph 出现在多个 SubMesh 上时(例如头和身体各有一份"张嘴")会一起生效。
        /// 找不到该名字则什么都不做。
        /// </summary>
        public void SetMorphWeight(string name, float weight)
        {
            if (mStates == null || string.IsNullOrEmpty(name))
                return;
            for (int i = 0; i < mStates.Length; i++)
            {
                var state = mStates[i];
                if (state == null || state.MorphSet == null || state.Weights == null)
                    continue;
                var index = state.MorphSet.FindTargetIndex(name);
                if (index < 0)
                    continue;
                if (state.Weights[index] == weight)
                    continue;
                state.Weights[index] = weight;
                state.Dirty = true;
            }
        }

        /// <summary> 取权重, 找不到返回 0。 </summary>
        public float GetMorphWeight(string name)
        {
            if (mStates == null || string.IsNullOrEmpty(name))
                return 0.0f;
            for (int i = 0; i < mStates.Length; i++)
            {
                var state = mStates[i];
                if (state == null || state.MorphSet == null || state.Weights == null)
                    continue;
                var index = state.MorphSet.FindTargetIndex(name);
                if (index >= 0)
                    return state.Weights[index];
            }
            return 0.0f;
        }

        /// <summary> 所有权重归零。 </summary>
        public void ResetMorphWeights()
        {
            if (mStates == null)
                return;
            for (int i = 0; i < mStates.Length; i++)
            {
                var state = mStates[i];
                if (state == null || state.Weights == null)
                    continue;
                for (int w = 0; w < state.Weights.Length; w++)
                {
                    if (state.Weights[w] == 0.0f)
                        continue;
                    state.Weights[w] = 0.0f;
                    state.Dirty = true;
                }
            }
        }

        /// <summary>
        /// 从另一个 morph modifier 复制权重状态。供 TtMdfQueueBase.CopyFrom 在切换 MdfQueue
        /// 类型时保住当前表情, 否则切换后表情会突然复位。
        /// </summary>
        public void CopyWeightsFrom(TtMorphModifier other)
        {
            if (other == null || other.mStates == null || mStates == null)
                return;
            var names = new List<string>();
            other.CollectMorphNames(names);
            for (int i = 0; i < names.Count; i++)
            {
                SetMorphWeight(names[i], other.GetMorphWeight(names[i]));
            }
        }

        #endregion

        /// <summary>
        /// 按当前权重重算稠密表。只触碰"上次写过的"与"本次要写的"顶点, 与 VertexCount 无关。
        /// </summary>
        unsafe void AccumulateDeltas(TtMorphMeshState state)
        {
            var buffer = state.DeltaBuffer;
            if (buffer == null)
                return;

            var zero = new FMorphDelta();
            for (int i = 0; i < state.TouchedVertices.Count; i++)
            {
                buffer.UpdateData((int)state.TouchedVertices[i], in zero);
            }
            state.TouchedVertices.Clear();

            if (state.MorphSet == null || state.Weights == null)
                return;

            var targets = state.MorphSet.Targets;
            for (int t = 0; t < targets.Count; t++)
            {
                var weight = state.Weights[t];
                if (weight > -WeightEpsilon && weight < WeightEpsilon)
                    continue;

                var deltas = targets[t].Deltas;
                if (deltas == null)
                    continue;

                for (int d = 0; d < deltas.Length; d++)
                {
                    var vertexIndex = (int)deltas[d].VertexIndex;
                    if (vertexIndex < 0 || vertexIndex >= state.VertexCount)
                        continue;

                    var accumulated = buffer.DataArray[vertexIndex];
                    accumulated.mDeltaPosition += deltas[d].DeltaPosition * weight;
                    accumulated.mDeltaNormal += deltas[d].DeltaNormal * weight;
                    buffer.UpdateData(vertexIndex, in accumulated);

                    state.TouchedVertices.Add(deltas[d].VertexIndex);
                }
            }
        }

        public void OnBuildDrawCall(Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall, Graphics.Mesh.TtRenderMesh.TtAtom atom)
        {

        }

        public unsafe void OnDrawCall(Graphics.Pipeline.Shader.TtMdfQueueBase mdfQueue, NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy, Graphics.Mesh.TtRenderMesh.TtAtom atom)
        {
            if (mStates == null || atom == null || atom.SubMesh == null)
                return;

            var meshIndex = atom.SubMesh.MeshIndex;
            if (meshIndex < 0 || meshIndex >= mStates.Length)
                return;

            var state = mStates[meshIndex];
            if (state == null || state.DeltaBuffer == null)
                return;

            if (state.Dirty)
            {
                state.Dirty = false;
                AccumulateDeltas(state);
            }
            // Flush2GPU 内部有 Dirty 守卫, 无变化时是空操作; 首帧会在这里创建 GpuBuffer 与 Srv,
            // 因此必须在 BindSrv 之前调用。SetSize 只在 Initialize 做过一次, 之后不会重建 buffer,
            // 所以下面绑定的 Srv 不会被后续 flush 失效。
            state.DeltaBuffer.Flush2GPU(cmd);

            if (state.DeltaBuffer.Srv != null)
            {
                // TtGraphicDraw 上的方法名是 BindSRV (与 TtComputeDraw 的 BindSrv 不同),
                // 优先用 string 重载 (skill §3.2)。
                drawcall.BindSRV(MorphDeltasShaderName, state.DeltaBuffer.Srv);
            }
        }
    }
}
