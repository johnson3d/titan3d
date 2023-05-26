using EngineNS.GamePlay;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    //https://www.irimsky.top/archives/301/
    public class TtAntiAliasingShading : Shader.UGraphicsShadingEnv
    {
        public TtAntiAliasingShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/AAShading.cginc", RName.ERNameType.Engine);

            TypeAA = this.PushPermutation<Graphics.Pipeline.URenderPolicy.ETypeAA>("ENV_TypeAA", (int)Graphics.Pipeline.URenderPolicy.ETypeAA.TypeCount);

            TypeAA.SetValue((int)Graphics.Pipeline.URenderPolicy.ETypeAA.None);

            this.UpdatePermutation();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        protected override void EnvShadingDefines(in FPermutationId id, UShaderDefinitions defines)
        {
            defines.AddDefine("ETypeAA_None", (int)Graphics.Pipeline.URenderPolicy.ETypeAA.None);
            defines.AddDefine("TETypeAA_Fsaa", (int)Graphics.Pipeline.URenderPolicy.ETypeAA.Fsaa);
            defines.AddDefine("TETypeAA_Taa", (int)Graphics.Pipeline.URenderPolicy.ETypeAA.Taa);
        }
        public UPermutationItem TypeAA
        {
            get;
            set;
        }
        private void OnDrawcallTAA(NxRHI.UGraphicDraw drawcall, URenderPolicy deferredPolicy, TtAntiAliasingNode aaNode)
        {
            if (deferredPolicy.TypeAA == URenderPolicy.ETypeAA.Taa)
            {
                var index = drawcall.FindBinder("ColorBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                else
                {
                    TypeAA.SetValue((uint)URenderPolicy.ETypeAA.Taa);
                    this.UpdatePermutation();
                }
                index = drawcall.FindBinder("Samp_ColorBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

                index = drawcall.FindBinder("DepthBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.DepthPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_DepthBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                index = drawcall.FindBinder("MotionBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.MotionVectorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_MotionBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                index = drawcall.FindBinder("PrevColorBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.PreColorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                else
                {
                    TypeAA.SetValue((uint)URenderPolicy.ETypeAA.Taa);
                    this.UpdatePermutation();
                }
                index = drawcall.FindBinder("Samp_PrevColorBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

                index = drawcall.FindBinder("PrevDepthBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.PreDepthPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_PrevDepthBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);


                index = drawcall.FindBinder("cbShadingEnv");
                if (index.IsValidPointer)
                {
                    if (aaNode.CBShadingEnv == null)
                    {
                        aaNode.CBShadingEnv = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                        var jitterUV = deferredPolicy.DefaultCamera.mCoreObject.GetJitterUV();
                        aaNode.CBShadingEnv.SetValue("JitterUV", in jitterUV);
                    }
                    drawcall.BindCBuffer(index, aaNode.CBShadingEnv);
                }
            }
            else
            {
                var index = drawcall.FindBinder("ColorBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                else
                {
                    TypeAA.SetValue((uint)URenderPolicy.ETypeAA.Taa);
                    this.UpdatePermutation();
                }
                index = drawcall.FindBinder("Samp_ColorBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            }
        }
        public unsafe override void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            var pipelinPolicy = policy.TagObject as URenderPolicy;

            var aaNode = drawcall.TagObject as Common.TtAntiAliasingNode;
            if (aaNode == null)
                aaNode = pipelinPolicy.FindFirstNode<Common.TtAntiAliasingNode>();

            OnDrawcallTAA(drawcall, pipelinPolicy, aaNode);
        }
    }
    public class TtAntiAliasingNode : USceenSpaceNode
    {
        public Common.URenderGraphPin ColorPinIn = Common.URenderGraphPin.CreateInput("Color");
        public Common.URenderGraphPin PreColorPinIn = Common.URenderGraphPin.CreateInput("PreColor");
        public Common.URenderGraphPin DepthPinIn = Common.URenderGraphPin.CreateInput("Depth");
        public Common.URenderGraphPin PreDepthPinIn = Common.URenderGraphPin.CreateInput("PreDepth");
        public Common.URenderGraphPin MotionVectorPinIn = Common.URenderGraphPin.CreateInput("MotionVector");
        public TtAntiAliasingNode()
        {
            Name = "TaaNode";
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(PreColorPinIn, NxRHI.EBufferType.BFT_SRV);

            AddInput(DepthPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(PreDepthPinIn, NxRHI.EBufferType.BFT_SRV);

            AddInput(MotionVectorPinIn, NxRHI.EBufferType.BFT_SRV);

            base.InitNodePins();
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtAntiAliasingShading>();
        }

        public NxRHI.UCbView CBShadingEnv;

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
        private void TickSyncTAA(URenderPolicy policy)
        {
            if (CBShadingEnv != null)
            {
                var jitterUV = policy.DefaultCamera.mCoreObject.GetJitterUV();
                jitterUV.Y = -jitterUV.Y;
                CBShadingEnv.SetValue("JitterUV", in jitterUV);
                //if (CurrentOffsetIndex % 2 == 1)
                //    TaaBlendAlpha = 0.0f;
                //else
                //    TaaBlendAlpha = 1.0f;
                CBShadingEnv.SetValue("TaaBlendAlpha", TaaBlendAlpha);
                CBShadingEnv.FlushDirty(false);
            }

            CurrentOffsetIndex++;
            CurrentOffsetIndex = CurrentOffsetIndex % OffsetHaltonSequencer.Length;
            if (policy.TypeAA == URenderPolicy.ETypeAA.Taa)
            {
                Vector2 offset = OffsetHaltonSequencer[CurrentOffsetIndex];
                policy.DefaultCamera.JitterOffset = offset;
            }
            else
            {
                policy.DefaultCamera.JitterOffset = new Vector2(0.5f, 0.5f);
            }
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            if (policy.TypeAA == URenderPolicy.ETypeAA.None)
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
        }
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            if (policy.TypeAA == URenderPolicy.ETypeAA.None)
            {
                return;
            }
            base.TickLogic(world, policy, bClear);
        }
        public override void TickSync(URenderPolicy policy)
        {
            if (policy.TypeAA == URenderPolicy.ETypeAA.None)
            {
                return;
            }
            base.TickSync(policy);

            TickSyncTAA(policy);
        }
    }
}
