using EngineNS.GamePlay;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public partial class TtScreenSpaceUIShading : Shader.UGraphicsShadingEnv
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
    public partial class TtScreenSpaceUINode : USceenSpaceNode
    {
        public Common.URenderGraphPin ColorPinInOut = Common.URenderGraphPin.CreateInputOutput("Color");
        public Common.URenderGraphPin DepthPinInOut = Common.URenderGraphPin.CreateInputOutput("Depth");

        public TtScreenSpaceUIShading mScreenSpaceShading;

        public TtScreenSpaceUINode()
        {
            Name = "ScreenSpaceUINode";
        }
        public override void InitNodePins()
        {
            AddInputOutput(ColorPinInOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(DepthPinInOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
        }
        public override async Task Initialize(URenderPolicy policy, string debugName)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            ScreenDrawPolicy = new Shader.CommanShading.UBasePassPolicy();
            await ScreenDrawPolicy.Initialize(null);
            ScreenDrawPolicy.TagObject = policy;

            CreateGBuffers(policy, ColorPinInOut.Attachement.Format);

            BasePass.Initialize(rc, debugName + ".BasePass");
            DebugName = debugName;

            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtScreenSpaceUIShading>();
        }
        public override unsafe UGraphicsBuffers CreateGBuffers(URenderPolicy policy, EPixelFormat format)
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

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

            GBuffers.Initialize(policy, RenderPass);
            GBuffers.SetRenderTarget(policy, 0, ColorPinInOut);
            GBuffers.SetDepthStencil(policy, DepthPinInOut);
            GBuffers.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;

            return GBuffers;
        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            if(GBuffers != null && RenderPass != null)
            {
                GBuffers.SetSize(x * OutputScaleFactor, y * OutputScaleFactor);
            }
            UEngine.Instance.UIManager.ScreenSize = new SizeF(x, y);
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(TtScreenSpaceUINode), nameof(TickLogic));
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                var cmdlist = BasePass.DrawCmdList;
                cmdlist.BeginCommand();

                if(UEngine.Instance.UIManager.ScreenSpaceUIHost != null)
                {
                    var host = UEngine.Instance.UIManager.ScreenSpaceUIHost;
                    host.UpdateCameraOffset(world);
                    foreach (var i in host.DrawMesh.SubMeshes)
                    {
                        foreach (var j in i.Atoms)
                        {
                            var drawCall = j.GetDrawCall(cmdlist.mCoreObject, GBuffers, ScreenDrawPolicy, Graphics.Pipeline.URenderPolicy.EShadingType.BasePass, this);
                            if (drawCall == null)
                                continue;
                            drawCall.TagObject = this;
                            drawCall.BindCBuffer(drawCall.Effect.BindIndexer.cbPerViewport, GBuffers.PerViewportCBuffer);
                            drawCall.BindCBuffer(drawCall.Effect.BindIndexer.cbPerCamera, policy.DefaultCamera.PerCameraCBuffer);
                            cmdlist.PushGpuDraw(drawCall);
                        }
                    }
                }

                {
                    cmdlist.SetViewport(in GBuffers.Viewport);
                    var passClears = new NxRHI.FRenderPassClears();
                    passClears.SetDefault();
                    passClears.SetClearColor(0, new Color4f(0, 0, 0, 0));
                    passClears.ClearFlags = ERenderPassClearFlags.CLEAR_NONE;
                    GBuffers.BuildFrameBuffers(policy);
                    cmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, DebugName);
                    cmdlist.FlushDraws();
                    cmdlist.EndPass();
                }

                cmdlist.EndCommand();
                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmdlist);
            }
        }
    }
}
