using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.GamePlay;
using EngineNS.Graphics.Mesh;
using EngineNS.NxRHI;

namespace EngineNS.Graphics.Pipeline.Common.Post
{
    public class TtAdditiveShading : Shader.UGraphicsShadingEnv
    {
        public TtAdditiveShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Post/AdditiveShading.cginc", RName.ERNameType.Engine);

            this.UpdatePermutation();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        protected override void EnvShadingDefines(in FPermutationId id, UShaderDefinitions defines)
        {
            defines.AddDefine("ENV_ADD_COLOR", "1");
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, URenderPolicy.EShadingType shadingType, UGraphicDraw drawcall, URenderPolicy policy, TtMesh.TtAtom atom)
        {
            var aaNode = drawcall.TagObject as TtAdditiveNode;
            if (aaNode == null)
            {
                var pipelinePolicy = policy.TagObject as URenderPolicy;
                aaNode = pipelinePolicy.FindFirstNode<TtAdditiveNode>();
            }

            var index = drawcall.FindBinder("Color1Buffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.Color1PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_Color1Buffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            index = drawcall.FindBinder("Color2Buffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.Color2PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_Color2Buffer");
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
    public class TtAdditiveNode : USceenSpaceNode
    {
        public Common.URenderGraphPin Color1PinIn = Common.URenderGraphPin.CreateInput("Color1");
        public Common.URenderGraphPin Color2PinIn = Common.URenderGraphPin.CreateInput("Color2");
        public TtAdditiveNode()
        {
            Name = "AdditiveNode";

            mAdditiveStruct.SetDefault();
        }
        public override void InitNodePins()
        {
            AddInput(Color1PinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(Color2PinIn, NxRHI.EBufferType.BFT_SRV);

            base.InitNodePins();
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtAdditiveShading>();
        }
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        struct FAdditiveStruct
        {
            public void SetDefault()
            {
                Factor1 = 1.0f;
                Factor2 = 1.0f;
            }
            public float Factor1;
            public float Factor2;
        }
        FAdditiveStruct mAdditiveStruct = new FAdditiveStruct();
        [Category("Option")]
        [Rtti.Meta]
        public float Factor1
        {
            get => mAdditiveStruct.Factor1;
            set => mAdditiveStruct.Factor1 = value;
        }
        [Category("Option")]
        [Rtti.Meta]
        public float Factor2
        {
            get => mAdditiveStruct.Factor2;
            set => mAdditiveStruct.Factor2 = value;
        }
        public NxRHI.UCbView CBShadingEnv;
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            base.TickLogic(world, policy, bClear);
            if (CBShadingEnv != null)
            {
                CBShadingEnv.SetValue("AdditiveStruct", in mAdditiveStruct);
            }
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            var buffer = this.FindAttachBuffer(Color1PinIn);
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

    public class TtAdditiveLumShading : TtAdditiveShading
    {
        public override string ToString()
        {
            return base.ToString() + "[Lum]";
        }
        protected override void EnvShadingDefines(in FPermutationId id, UShaderDefinitions defines)
        {
            defines.AddDefine("ENV_ADD_LUMINANCE", "1");
        }
    }
    public class TtAdditiveLumNode : TtAdditiveNode
    {
        public TtAdditiveLumNode()
        {
            Name = "AdditiveLumNode";
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtAdditiveLumShading>();
        }
    }
}
