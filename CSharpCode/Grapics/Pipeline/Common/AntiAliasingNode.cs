using EngineNS.GamePlay;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    //https://www.irimsky.top/archives/301/
    public class TtAntiAliasingShading : Shader.TtGraphicsShadingEnv
    {
        public TtAntiAliasingShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/AAShading.cginc", RName.ERNameType.Engine);

            this.BeginPermutaion();

            TypeAA = this.PushPermutation<TtAntiAliasingNode.ETypeAA>("ENV_TypeAA", (int)TtAntiAliasingNode.ETypeAA.TypeCount);

            TypeAA.SetValue((uint)TtAntiAliasingNode.ETypeAA.Taa);

            this.UpdatePermutation().AddWaitTask();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        protected override void EnvShadingDefines(in FPermutationId id, TtShaderDefinitions defines)
        {
        }
        public TtPermutationItem TypeAA
        {
            get;
            set;
        }
        private void OnDrawcallTAA(NxRHI.TtGraphicDraw drawcall, TtRenderPolicy deferredPolicy, TtAntiAliasingNode aaNode)
        {
            if (TypeAA.GetValue() == (int)TtAntiAliasingNode.ETypeAA.Taa)
            {
                // 注意: 这里所有 binder 找不到的分支严禁再触发
                //   TypeAA.SetValue(...).UpdatePermutation().AddWaitTask()
                // 旧实现这么做会在 OnDrawCall 热路径上每帧 stall 主线程等待 effect 重建,
                // 而且会进入死循环 (binder 不存在 → 重建 effect → 仍然不存在 → 再重建).
                // Permutation 应在 ctor / Initialize 阶段一次性设好.
                var index = drawcall.FindBinder("ColorBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_ColorBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                index = drawcall.FindBinder("DepthBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.DepthPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_DepthBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                index = drawcall.FindBinder("MotionBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.MotionVectorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_MotionBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                index = drawcall.FindBinder("PrevColorBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.PreColorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_PrevColorBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

                index = drawcall.FindBinder("PrevDepthBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.PreDepthPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_PrevDepthBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                index = drawcall.FindBinder("cbShadingEnv");
                if (index.IsValidPointer)
                {
                    if (aaNode.CBShadingEnv == null)
                    {
                        aaNode.CBShadingEnv = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                        // 首帧立刻写一次 TaaBlendAlpha, 保证 cb 不是 0 初始化值.
                        aaNode.UpdateShadingCBuffer(deferredPolicy);
                    }
                    drawcall.BindCBV(index, aaNode.CBShadingEnv);
                }
            }
            else
            {
                // FSAA / None 分支同样禁止热路径 UpdatePermutation, 见上面的注释.
                var index = drawcall.FindBinder("ColorBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_ColorBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            }

        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtRenderMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var aaNode = drawcall.TagObject as Common.TtAntiAliasingNode;

            OnDrawcallTAA(drawcall, policy, aaNode);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("AntiAliasing", "Post\\AntiAliasing", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtAntiAliasingNode : TAuxSceenSpaceNode<TtAntiAliasingNode>
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin PreColorPinIn = TtRenderGraphPin.CreateInput("PreColor", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput("Depth", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin PreDepthPinIn = TtRenderGraphPin.CreateInput("PreDepth", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin MotionVectorPinIn = TtRenderGraphPin.CreateInput("MotionVector", NxRHI.EBufferType.BFT_SRV);

        // 节点内部不再持有 history buffer / copy drawcall,
        // 由外部独立的 PrevColor / PrevDepth Aux 节点统一管理.
        [Editor.ShaderCompiler.TtShaderDefine(ShaderName = "ETypeAA")]
        [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Graphics.Pipeline.TtRenderPolicy.ETypeAA@EngineCore", "EngineNS.Graphics.Pipeline.TtRenderPolicy.ETypeAA" })]
        public enum ETypeAA : uint
        {
            None = 0,
            Fsaa,
            Taa,

            TypeCount,
        }
        public TtAntiAliasingNode()
        {
            Name = "TaaNode";
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn);
            AddInput(PreColorPinIn);

            AddInput(DepthPinIn);
            AddInput(PreDepthPinIn);

            AddInput(MotionVectorPinIn);

            base.InitNodePins();
        }
        public override void Dispose()
        {
            base.Dispose();
        }
        public TtAntiAliasingShading mBasePassShading;
        public override TtGraphicsShadingEnv GetPassShading(TtRenderMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtAntiAliasingShading>();

            // 按节点字段一次性把 effect 的 permutation 设成对应分支并重建 PSO.
            // 注意: 这里只 SetValue, *不* await UpdatePermutation; UpdatePermutation 在 RenderPolicy
            // 完整初始化序列里 await 容易导致后续节点的注册顺序错乱 (上一次实测 TaaNode 直接从图里消失).
            // 真正的 effect 重建延迟到首次 TickLogic 时由 mEffectPermutationDirty 触发, 此时 RenderGraph
            // 已经稳定, 重建 effect 不会动节点拓扑.
            mBasePassShading.TypeAA.SetValue((uint)policy.TypeAA);
            await mBasePassShading.UpdatePermutation();
        }

        public NxRHI.TtCbView CBShadingEnv;

        //double[] mOffsetHaltonSequencer;
        //private double[] OffsetHaltonSequencer
        //{
        //    get
        //    {
        //        if (mOffsetHaltonSequencer == null)
        //            mOffsetHaltonSequencer = MathHelper.GenHaltonSequence(5);
        //        return mOffsetHaltonSequencer;
        //    }
        //}
        private Vector2[] OffsetHaltonSequencer = new Vector2[]
        {
            //new Vector2(0.5f, 0.5f),
            //new Vector2(0.75f, 0.5f),
            //new Vector2(0.25f, 0.5f),
            //new Vector2(0.5f, 0.75f),
            //new Vector2(0.5f, 0.25f),

            new Vector2(0.5f, 1.0f / 3),
            new Vector2(0.25f, 2.0f / 3),
            new Vector2(0.75f, 1.0f / 9),
            new Vector2(0.125f, 4.0f / 9),
            new Vector2(0.625f, 7.0f / 9),
            new Vector2(0.375f, 2.0f / 9),
            new Vector2(0.875f, 5.0f / 9),
            new Vector2(0.0625f, 8.0f / 9),
        };
        private int CurrentOffsetIndex = 0;
        public float TaaBlendAlpha { get; set; } = 0.05f;
        // cbShadingEnv 现在只承担 TAA 调参 (TaaBlendAlpha). jitter 由 cbPerCamera.JitterOffset
        // / PreJitterOffset 统一提供, 这里不再写入 JitterUV, 避免两路 jitter 同步出错的历史 bug.
        internal void UpdateShadingCBuffer(TtRenderPolicy policy)
        {
            if (CBShadingEnv == null)
                return;
            CBShadingEnv.SetValue("TaaBlendAlpha", TaaBlendAlpha);
        }

        private void TickSyncTAA(TtRenderPolicy policy)
        {
            // 推进本帧 jitter 到 Camera. SetJitterOffset 不再触发投影矩阵重算
            // (投影矩阵已是纯净 view-projection), jitter 仅以 cbPerCamera.JitterOffset
            // / PreJitterOffset 形式参与 GBuffer VS 的 SV_Position 偏移和 TAA 的反 jitter 采样.
            // 上一帧 jitter 的推进 (mPreJitterOffset = mJitterOffset) 由 native 端
            // ICamera::UpdateConstBufferData 一帧一次维护, 不要在这里做.
            CurrentOffsetIndex++;
            CurrentOffsetIndex = CurrentOffsetIndex % OffsetHaltonSequencer.Length;
            if (policy.TypeAA == ETypeAA.Taa)
            {
                // OffsetHaltonSequencer 里存的是 [0,1) 的 Halton 原始值, 直接交给 Camera;
                // C++ 端 GetJitterUV() 会自己做 (x-0.5)/size 的中心化 + 归一化.
                Vector2 offset = OffsetHaltonSequencer[CurrentOffsetIndex];
                policy.DefaultCamera.JitterOffset = offset;
            }
            else
            {
                policy.DefaultCamera.JitterOffset = new Vector2(0.5f, 0.5f);
            }

            UpdateShadingCBuffer(policy);
        }

        public override void FrameBuild(TtRenderPolicy policy)
        {
            base.FrameBuild(policy);
        }

        public override void BeforeTick(TtRenderPolicy policy)
        {
            if (policy.TypeAA == ETypeAA.None)
            {
                this.MoveAttachment(ColorPinIn, ResultPinOut);
                return;
            }
            var buffer = this.FindAttachBuffer(ColorPinIn);
            if (buffer != null)
            {
                if (ResultPinOut.Attachement.Format != buffer.BufferDesc.Format)
                {
                    this.CreateGBuffers(policy, buffer.BufferDesc.Format);
                    ResultPinOut.Attachement.Format = buffer.BufferDesc.Format;
                }
            }
            // History (PreColor/PreDepth) 由外部独立的 PrevColor/PrevDepth Aux 拷贝节点提供,
            // 节点本身不再维护 ResultBuffer[0]/[1] 与 TickCopyLogic 拷贝逻辑,
            // 否则会和图里的 PrevColor/PrevDepth 节点重复拷贝并把 ImportedBuffer 互相覆盖.
        }
        public override void Tick(TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            switch (policy.TypeAA)
            {
                case ETypeAA.None:
                    break;
                case ETypeAA.Taa:
                case ETypeAA.Fsaa:
                    base.Tick(world, policy, frameCmdList, bClear);
                    break;
            }
        }
        public override void TickSync(TtRenderPolicy policy)
        {
            base.TickSync(policy);

            // gate: 只在 "节点字段 = Taa" 且 "effect permutation 实际编译为 Taa" 时才注入 jitter.
            // 不再依赖 "上一帧 TickLogic 是否跑过" 这种跨方法时序状态:
            //   - 渲染线程 / 逻辑线程的 TickLogic / TickSync 顺序在不同管线下不固定,
            //     用 mTaaTickLogicRanLastFrame 这种 "本帧入口被置 false, 跑完才置 true" 的状态
            //     做 gate 会出现 "TickSync 永远看到 false → jitter 一直被复位为 (0.5,0.5)
            //     → cbShadingEnv.JitterUV ≈ 0 → TAA 永远收敛不了" 的 bug, RenderDoc 抓帧
            //     已确认是这种状态.
            //   - 真正稳定的判定就是 "节点要跑 TAA 且 effect 已经被构建成 TAA permutation",
            //     这两条满足就一定有 TaaNode draw 提交.
            bool taaActive = policy.TypeAA == ETypeAA.Taa
                && mBasePassShading != null
                && mBasePassShading.TypeAA.GetValue() == (uint)ETypeAA.Taa;
            if (!taaActive)
            {
                // 没跑 TAA 时把 Camera 的 JitterOffset 复位为 (0.5, 0.5),
                // 这样 GetJitterUV() 返回 (0,0), JitterProjectionMatrix == ProjectionMatrix,
                // 不会有残留的 jitter 影响其他 pass.
                if (policy.DefaultCamera != null)
                    policy.DefaultCamera.JitterOffset = new Vector2(0.5f, 0.5f);
                return;
            }

            TickSyncTAA(policy);
        }
    }
}
