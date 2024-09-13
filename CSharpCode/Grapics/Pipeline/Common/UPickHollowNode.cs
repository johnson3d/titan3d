using EngineNS.GamePlay;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UPickHollowShading : Shader.TtGraphicsShadingEnv
    {
        public UPickHollowShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/pick/pick_hollow.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall)
        {
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var pickHollowNode = drawcall.TagObject as Common.UPickHollowNode;
            
            var index = drawcall.FindBinder("gPickedSetUpTex");
            if (index.IsValidPointer)
            {
                var attachBuffer = pickHollowNode.GetAttachBuffer(pickHollowNode.PickedPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_gPickedSetUpTex");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = drawcall.FindBinder("gPickedBlurTex");
            if (index.IsValidPointer)
            {
                var attachBuffer = pickHollowNode.GetAttachBuffer(pickHollowNode.BlurPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_gPickedBlurTex");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("PickHollow", "Pick\\PickHollow", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class UPickHollowNode : TtSceenSpaceNode
    {
        public TtRenderGraphPin PickedPinIn = TtRenderGraphPin.CreateInput("Picked");
        public TtRenderGraphPin BlurPinIn = TtRenderGraphPin.CreateInput("Blur");
        public UPickHollowNode()
        {
            Name = "PickHollowNode";
        }
        public override void InitNodePins()
        {
            AddInput(PickedPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(BlurPinIn, NxRHI.EBufferType.BFT_SRV);
            
            ResultPinOut.IsAutoResize = false;
            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16G16_FLOAT;
            base.InitNodePins();
        }
        public UPickHollowShading mBasePassShading;
        public override TtGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<UPickHollowShading>();
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
        public override void TickLogic(TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            var PickedManager = policy.GetOptionData("PickedManager") as TtPickedProxiableManager;
            if (PickedManager != null && PickedManager.PickedProxies.Count == 0)
                return;
            base.TickLogic(world, policy, bClear);
        }
    }

    public class TtPickHollowBlendShading : Shader.TtGraphicsShadingEnv
    {
        public TtPickHollowBlendShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/pick/pick_hollow_blend.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall)
        {
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var node = drawcall.TagObject as TtPickHollowBlendNode;

            var index = drawcall.FindBinder("ColorBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = node.GetAttachBuffer(node.ColorPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_ColorBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("DepthBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = node.GetAttachBuffer(node.DepthPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_DepthBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("GPickedTex");
            if (index.IsValidPointer)
            {
                var attachBuffer = node.GetAttachBuffer(node.PickedPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GPickedTex");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
        }
    }

    [Bricks.CodeBuilder.ContextMenu("PickHollowBlend", "Pick\\PickHollowBlend", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtPickHollowBlendNode : TtSceenSpaceNode
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color");
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInputOutput("Depth");
        public TtRenderGraphPin PickedPinIn = TtRenderGraphPin.CreateInput("Picked");
        public TtPickHollowBlendNode()
        {
            Name = "PickHollowBlendNode";
        }
        public override void InitNodePins()
        {
            base.InitNodePins();

            AddInput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(DepthPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(PickedPinIn, NxRHI.EBufferType.BFT_SRV);
        }
        public TtPickHollowBlendShading mBasePassShading;
        public override TtGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtPickHollowBlendShading>();
        }
        public override void OnLinkIn(TtRenderGraphLinker linker)
        {
            //ResultPinOut.Attachement.Format = PickedPinIn.Attachement.Format;
        }
        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {

        }
        public override void BeforeTickLogic(TtRenderPolicy policy)
        {
            var PickedManager = policy.GetOptionData("PickedManager") as TtPickedProxiableManager;
            if (PickedManager != null && PickedManager.PickedProxies.Count == 0)
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
        public override void TickLogic(TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            var PickedManager = policy.GetOptionData("PickedManager") as TtPickedProxiableManager;
            if (PickedManager != null && PickedManager.PickedProxies.Count == 0)
                return;
            base.TickLogic(world, policy, bClear);
        }
    }
}
