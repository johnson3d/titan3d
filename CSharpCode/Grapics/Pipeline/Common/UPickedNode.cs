using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UPickSetupShading : Shader.UGraphicsShadingEnv
    {
        public UPickSetupShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/pick/pick_setup.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal,
                NxRHI.EVertexStreamType.VST_Tangent,};
        }
    }
    public class UPickedNode : URenderGraphNode
    {
        public Common.URenderGraphPin PickedPinOut = Common.URenderGraphPin.CreateOutput("Picked", false, EPixelFormat.PXF_R16G16_FLOAT);
        public Common.URenderGraphPin DepthPinOut = Common.URenderGraphPin.CreateOutput("Depth", false, EPixelFormat.PXF_D16_UNORM);
        public UPickedNode()
        {
            Name = "PickedNode";
        }
        public override void InitNodePins()
        {
            AddOutput(PickedPinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_RTV);
            AddOutput(DepthPinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);
        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            float scaleFactor = 1.0f;
            var hitProxyNode = policy.FindFirstNode<UHitproxyNode>();
            if (hitProxyNode != null)
            {
                scaleFactor = hitProxyNode.ScaleFactor;
            }

            PickedPinOut.Attachement.Width = (uint)(x * scaleFactor);
            PickedPinOut.Attachement.Height = (uint)(y * scaleFactor);

            DepthPinOut.Attachement.Width = (uint)(x * scaleFactor);
            DepthPinOut.Attachement.Height = (uint)(y * scaleFactor);

            if (PickedBuffer != null)
                PickedBuffer.SetSize(x * scaleFactor, y * scaleFactor);
        }
        public UPickedProxiableManager PickedManager;
        public UPickSetupShading PickedShading = null;
        public UGraphicsBuffers PickedBuffer { get; protected set; } = new UGraphicsBuffers();
        public NxRHI.URenderPass RenderPass;

        public async override System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            PickedShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UPickSetupShading>();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName + ".BasePass");
            BasePass.SetDebugName("UPickedProxiableManager");

            var PassDesc = new NxRHI.FRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].Format = PickedPinOut.Attachement.Format;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.Format = DepthPinOut.Attachement.Format;
                PassDesc.m_AttachmentDepthStencil.Samples = 1;
                PassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = new Color4f(1, 0, 1, 0);
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
            }
            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc); 

            PickedBuffer.Initialize(policy, RenderPass);
            PickedBuffer.SetRenderTarget(policy, 0, PickedPinOut);
            PickedBuffer.SetDepthStencil(policy, DepthPinOut);
            
            PickedBuffer.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;

            PickedManager = policy.PickedProxiableManager;
        }
        public override void Dispose()
        {
            PickedBuffer?.Dispose();
            PickedBuffer = null;

            base.Dispose();
        }
        List<Mesh.UMesh> mPickedMeshes = new List<Mesh.UMesh>();
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UPickedNode), nameof(TickLogic));
        public override unsafe void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                var cmdlist = BasePass.DrawCmdList;
                cmdlist.BeginCommand();

                mPickedMeshes.Clear();
                foreach (var i in PickedManager.PickedProxies)
                {
                    i.GetHitProxyDrawMesh(mPickedMeshes);
                }
                foreach (var mesh in mPickedMeshes)
                {
                    if (mesh == null || mesh.Atoms == null)
                        continue;

                    for (int j = 0; j < mesh.Atoms.Count; j++)
                    {
                        var drawcall = mesh.GetDrawCall(PickedBuffer, j, policy, Graphics.Pipeline.URenderPolicy.EShadingType.Picked, this);
                        if (drawcall != null)
                        {
                            if (PickedBuffer.PerViewportCBuffer != null)
                                drawcall.BindCBuffer(drawcall.Effect.BindIndexer.cbPerViewport, PickedBuffer.PerViewportCBuffer);
                            if (policy.DefaultCamera.PerCameraCBuffer != null)
                                drawcall.BindCBuffer(drawcall.Effect.BindIndexer.cbPerCamera, policy.DefaultCamera.PerCameraCBuffer);

                            cmdlist.PushGpuDraw(drawcall);
                        }
                    }
                }

                {
                    cmdlist.SetViewport(in PickedBuffer.Viewport);
                    var passClears = new NxRHI.FRenderPassClears();
                    passClears.SetDefault();
                    passClears.SetClearColor(0, new Color4f(1, 0, 1, 0));
                    PickedBuffer.BuildFrameBuffers(policy);
                    cmdlist.BeginPass(PickedBuffer.FrameBuffers, in passClears, "Picked");
                    cmdlist.FlushDraws();
                    cmdlist.EndPass();
                }
                cmdlist.EndCommand();
                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmdlist);
            }   
        }
    }
}
