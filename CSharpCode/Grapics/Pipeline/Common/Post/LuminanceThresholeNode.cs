using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.GamePlay;
using EngineNS.Graphics.Mesh;
using EngineNS.NxRHI;

namespace EngineNS.Graphics.Pipeline.Common.Post
{
    public class TtLuminanceThresholeShading : Shader.UGraphicsShadingEnv
    {
        public TtLuminanceThresholeShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Post/LuminanceThresholeShading.cginc", RName.ERNameType.Engine);

            this.UpdatePermutation();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        protected override void EnvShadingDefines(in FPermutationId id, UShaderDefinitions defines)
        {
            defines.AddDefine("ENV_OUT_COLOR", "1");
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, URenderPolicy.EShadingType shadingType, UGraphicDraw drawcall, URenderPolicy policy, TtMesh.TtAtom atom)
        {
            var aaNode = drawcall.TagObject as TtLuminanceThresholeNode;
            if (aaNode == null)
            {
                var pipelinePolicy = policy.TagObject as URenderPolicy;
                aaNode = pipelinePolicy.FindFirstNode<TtLuminanceThresholeNode>();
            }

            var index = drawcall.FindBinder("ColorBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_ColorBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            index = drawcall.FindBinder("cbShadingEnv");
            if (index.IsValidPointer)
            {
                if (aaNode.CBShadingEnv == null)
                {
                    aaNode.CBShadingEnv = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                }
                drawcall.BindCBuffer(index, aaNode.CBShadingEnv);
            }

            base.OnDrawCall(cmd, shadingType, drawcall, policy, atom);
        }
    }
    public class TtLuminanceThresholeNode : USceenSpaceNode
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color");
        public TtLuminanceThresholeNode()
        {
            Name = "LuminanceThresholeNode";

            mLuminanceThresholeStruct.SetDefault();
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);

            base.InitNodePins();
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtLuminanceThresholeShading>();
        }
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        struct FLuminanceThresholeStruct
        {
            public void SetDefault()
            {
                Threshole = 1.0f;
            }
            public float Threshole;
        }
        FLuminanceThresholeStruct mLuminanceThresholeStruct = new FLuminanceThresholeStruct();
        [Category("Option")]
        [Rtti.Meta]
        public float Threshole
        {
            get => mLuminanceThresholeStruct.Threshole;
            set => mLuminanceThresholeStruct.Threshole = value;
        }
        public NxRHI.UCbView CBShadingEnv;
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            base.TickLogic(world, policy, bClear);
            if (CBShadingEnv != null)
            {
                CBShadingEnv.SetValue("LuminanceThresholeStruct", in mLuminanceThresholeStruct);
            }
        }
        public override void TickSync(URenderPolicy policy)
        {
            base.TickSync(policy);
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
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
    }

    public class TtLuminanceThresholeOutLumShading : TtLuminanceThresholeShading
    {
        public override string ToString()
        {
            return base.ToString() + "[OutLum]";
        }
        protected override void EnvShadingDefines(in FPermutationId id, UShaderDefinitions defines)
        {
            defines.AddDefine("ENV_OUT_LUMINANCE", "1");
        }
    }
    public class TtLuminanceThresholeOutLumNode : TtLuminanceThresholeNode
    {
        public TtLuminanceThresholeOutLumNode()
        {
            Name = "LuminanceThresholeOutLumNode";
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtLuminanceThresholeOutLumShading>();
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            var buffer = this.FindAttachBuffer(ColorPinIn);
            if (buffer != null)
            {
                //ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16_FLOAT;
                if (ResultPinOut.Attachement.Format != EPixelFormat.PXF_R16_FLOAT)
                {
                    this.CreateGBuffers(policy, EPixelFormat.PXF_R16_FLOAT);
                    ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16_FLOAT;
                }
            }
        }
    }
}
