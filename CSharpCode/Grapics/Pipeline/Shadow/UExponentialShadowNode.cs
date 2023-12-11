using EngineNS.Graphics.Pipeline.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shadow
{
    public class UExponentialShadowShading : Shader.UGraphicsShadingEnv
    {
        public UExponentialShadowShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/ESM.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(URenderPolicy policy, NxRHI.UGraphicDraw drawcall)
        {
            //var cbIndex = drawcall.mCoreObject.FindCBufferIndex("cbPerShadingEnv");
            //if (cbIndex != 0xFFFFFFFF)
            //{
            //    if (PerShadingCBuffer == null)
            //    {
            //        PerShadingCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(gpuProgram, cbIndex);
            //        PerShadingCBuffer.SetMatrix(0, ref Matrix.mIdentity);
            //        var RenderColor = new Color4f(1, 1, 1, 1);
            //        PerShadingCBuffer.SetValue(1, ref RenderColor);
            //    }
            //    drawcall.mCoreObject.BindCBufferAll(cbIndex, PerShadingCBuffer.mCoreObject.Ptr);
            //}
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, shadingType, drawcall, policy, atom);

            var Manager = policy.TagObject as URenderPolicy;

            var ESMNode = Manager.FindFirstNode<UExponentialShadowNode>();

            var index = drawcall.FindBinder("GShadowMap");
            if (index.IsValidPointer)
            {
                var attachBuffer = ESMNode.GetAttachBuffer(ESMNode.ShadowMapPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }

            index = drawcall.FindBinder("Samp_GShadowMap");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
        }
    }
    public class UExponentialShadowNode : USceenSpaceNode
    {
        public Common.URenderGraphPin ShadowMapPinIn = Common.URenderGraphPin.CreateInput("ShadowMap");
        //public Common.URenderGraphPin ExponentialShadowMapPinOut = Common.URenderGraphPin.CreateOutput("ExponentialShadowMap", false, EPixelFormat.PXF_R32G32B32A32_FLOAT); //EPixelFormat.. Debug
        public UExponentialShadowNode()
        {
            Name = "ExponentialShadowMap";
        }

        public override void InitNodePins()
        {
            AddInput(ShadowMapPinIn, NxRHI.EBufferType.BFT_SRV);

            ResultPinOut.IsAutoResize = false;
            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R32G32B32A32_FLOAT;
            base.InitNodePins();
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UExponentialShadowShading>();
        }
        public override void OnLinkIn(URenderGraphLinker linker)
        {
            //ResultPinOut.Attachement.Format = PickedPinIn.Attachement.Format;
        }
        public override void FrameBuild(Graphics.Pipeline.URenderPolicy policy)
        {

        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            float scaleFactor = 1.0f;
            var hitProxyNode = policy.FindFirstNode<UHitproxyNode>();
            if (hitProxyNode != null)
            {
                scaleFactor = hitProxyNode.ScaleFactor;
            }

            ResultPinOut.Attachement.Width = (uint)(x * scaleFactor);
            ResultPinOut.Attachement.Height = (uint)(y * scaleFactor);

            base.OnResize(policy, x * scaleFactor, y * scaleFactor);
        }
    }
}
