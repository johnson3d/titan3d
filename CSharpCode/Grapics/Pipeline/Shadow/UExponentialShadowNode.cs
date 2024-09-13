using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shadow
{
    public class TtExponentialShadowShading : Shader.TtGraphicsShadingEnv
    {
        public TtExponentialShadowShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/ESM.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall)
        {
            //var cbIndex = drawcall.mCoreObject.FindCBufferIndex("cbPerShadingEnv");
            //if (cbIndex != 0xFFFFFFFF)
            //{
            //    if (PerShadingCBuffer == null)
            //    {
            //        PerShadingCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(gpuProgram, cbIndex);
            //        PerShadingCBuffer.SetMatrix(0, ref Matrix.mIdentity);
            //        var RenderColor = new Color4f(1, 1, 1, 1);
            //        PerShadingCBuffer.SetValue(1, ref RenderColor);
            //    }
            //    drawcall.mCoreObject.BindCBufferAll(cbIndex, PerShadingCBuffer.mCoreObject.Ptr);
            //}
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var ESMNode = drawcall.TagObject as TtExponentialShadowNode;

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
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("ESM", "Shadow\\ESM", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Shadow.UExponentialShadowNode@EngineCore", "EngineNS.Graphics.Pipeline.Shadow.UExponentialShadowNode" })]
    public class TtExponentialShadowNode : TtSceenSpaceNode
    {
        public TtRenderGraphPin ShadowMapPinIn = TtRenderGraphPin.CreateInput("ShadowMap");
        //public TtRenderGraphPin ExponentialShadowMapPinOut = TtRenderGraphPin.CreateOutput("ExponentialShadowMap", false, EPixelFormat.PXF_R32G32B32A32_FLOAT); //EPixelFormat.. Debug
        public TtExponentialShadowNode()
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
        public TtExponentialShadowShading mBasePassShading;
        public override TtGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtExponentialShadowShading>();
        }
        public override void OnLinkIn(TtRenderGraphLinker linker)
        {
            //ResultPinOut.Attachement.Format = PickedPinIn.Attachement.Format;
        }
        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {

        }
        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            float scaleFactor = 1.0f;
            var hitProxyNode = policy.FindFirstNode<TtHitproxyNode>();
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
