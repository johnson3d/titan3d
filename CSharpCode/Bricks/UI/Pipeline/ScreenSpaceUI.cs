using EngineNS.GamePlay;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public partial class TtScreenSpaceUIShading : Shader.TtGraphicsShadingEnv
    {
        public TtScreenSpaceUIShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/ScreenSpaceUI.cginc", RName.ERNameType.Engine);
            this.UpdatePermutation();
        }
        public override EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[]
            {
                NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Color,
                NxRHI.EVertexStreamType.VST_UV,
                NxRHI.EVertexStreamType.VST_SkinIndex,
            };
        }
    }
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    [Bricks.CodeBuilder.ContextMenu("ScreenUI", "ScreenUI", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public partial class TtScreenSpaceUINode : TAuxSceenSpaceNode<TtScreenSpaceUINode>
    {
        public TtRenderGraphPin ColorPinInOut = TtRenderGraphPin.CreateInputOutput("Color", NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin DepthPinInOut = TtRenderGraphPin.CreateInputOutput("Depth", NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);

        public TtScreenSpaceUIShading mScreenSpaceShading;

        public TtScreenSpaceUINode()
        {
            Name = "ScreenSpaceUINode";
        }
        public override void InitNodePins()
        {
            AddInputOutput(ColorPinInOut);
            AddInputOutput(DepthPinInOut);
        }
        public override TtGraphicsShadingEnv GetPassShading(TtRenderMesh.TtAtom atom)
        {
            return mBasePassShading;
        }
        public TtScreenSpaceUIShading mBasePassShading;
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            CreateGBuffers(policy, ColorPinInOut.Attachement.Format);

            DebugName = debugName;

            mBasePassShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtScreenSpaceUIShading>();
        }
        public override unsafe TtGraphicsBuffers CreateGBuffers(TtRenderPolicy policy, EPixelFormat format)
        {
            var PassDesc = new NxRHI.FRenderPassDesc();

            PassDesc.NumOfMRT = 1;
            PassDesc.AttachmentMRTs[0].Format = format;
            PassDesc.AttachmentMRTs[0].Samples = 1;
            PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.m_AttachmentDepthStencil.Format = DepthPinInOut.Attachement.Format;// dfPolicy.BasePassNode.GBuffers.DepthStencil.AttachBuffer.Srv.mCoreObject.GetFormat(); //dsFmt;
            PassDesc.m_AttachmentDepthStencil.Samples = 1;
            PassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            RenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

            GBuffers.Initialize(policy, RenderPass);
            GBuffers.SetRenderTarget(policy, 0, ColorPinInOut);
            GBuffers.SetDepthStencil(policy, DepthPinInOut);
            GBuffers.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;

            return GBuffers;
        }
        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            if(GBuffers != null && RenderPass != null)
            {
                GBuffers.SetSize(x * OutputScaleFactor, y * OutputScaleFactor);
            }
        }
        public unsafe override void TickLogic(TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmdlist, "ScreenSpaceUI"))
            {
                cmdlist.SetViewport(in GBuffers.Viewport);
                FScissorRect scissor = new FScissorRect();
                scissor.MinX = 0;
                scissor.MinY = 0;
                scissor.MaxX = (int)GBuffers.Viewport.Width;
                scissor.MaxY = (int)GBuffers.Viewport.Height;
                cmdlist.SetScissor(in scissor);
                var passClears = new NxRHI.FRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4f(0, 0, 0, 0));
                passClears.ClearFlags = ERenderPassClearFlags.CLEAR_NONE;
                GBuffers.BuildFrameBuffers(policy);
                cmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, DebugName);
                var hud = policy.ViewportSlate?.HUD;
                if (hud != null)
                {
                    var host = hud;
                    host.UpdateCameraOffset(world);
                    if (host.DrawMesh != null)
                    {
                        foreach (var i in host.DrawMesh.SubMeshes)
                        {
                            foreach (var j in i.Atoms)
                            {
                                var drawCall = j.GetDrawCall(cmdlist.mCoreObject, GBuffers, policy, this);
                                if (drawCall == null)
                                    continue;
                                drawCall.TagObject = this;
                                drawCall.BindCBV(drawCall.Effect.BindIndexer.cbPerViewport, GBuffers.PerViewportCBuffer);
                                drawCall.BindCBV(drawCall.Effect.BindIndexer.cbPerCamera, policy.DefaultCamera.PerCameraCBuffer);
                                cmdlist.PushGpuDraw(drawCall);
                            }
                        }
                    }
                }
                cmdlist.FlushDraws();
                cmdlist.EndPass();
            }

            policy.CommitCommandList(cmdlist, "ScreenSpaceUI");
        }
    }
}
