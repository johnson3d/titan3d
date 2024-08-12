using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.Graphics.Pipeline.Shader;
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
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var ESMNode = drawcall.TagObject as UExponentialShadowNode;

            var index = drawcall.FindBinder("GShadowMap");
            //var attachBuffer = ESMNode.GetAttachBuffer(ESMNode.ShadowMapPinIn);
            //drawcall.BindSRV(VNameString.FromString("GShadowMap"), attachBuffer.Srv);
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
    [Bricks.CodeBuilder.ContextMenu("ESM", "Shadow\\ESM", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class UExponentialShadowNode : USceenSpaceNode
    {
        public TtRenderGraphPin ShadowMapPinIn = TtRenderGraphPin.CreateInput("ShadowMap");
        //public TtRenderGraphPin ExponentialShadowMapPinOut = TtRenderGraphPin.CreateOutput("ExponentialShadowMap", false, EPixelFormat.PXF_R32G32B32A32_FLOAT); //EPixelFormat.. Debug
        public UExponentialShadowNode()
        {
            Name = "ExponentialShadowMap";
        }

        public override void InitNodePins()
        {
            AddInput(ShadowMapPinIn, NxRHI.EBufferType.BFT_SRV);

            ResultPinOut.IsAutoResize = false;
            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16_FLOAT;
            base.InitNodePins();
        }
        public UExponentialShadowShading mBasePassShading;
        public override UGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await UEngine.Instance.ShadingEnvManager.GetShadingEnv<UExponentialShadowShading>();
        }
        public override void OnLinkIn(TtRenderGraphLinker linker)
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
