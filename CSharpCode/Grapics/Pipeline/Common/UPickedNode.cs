using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UPickSetupShading : Shader.UShadingEnv
    {
        public UPickSetupShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/pick/pick_setup.cginc", RName.ERNameType.Engine);
        }
        public override EVertexStreamType[] GetNeedStreams()
        {
            return new EVertexStreamType[] { EVertexStreamType.VST_Position,
                EVertexStreamType.VST_Normal,
                EVertexStreamType.VST_Tangent,};
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
            AddOutput(PickedPinOut, EGpuBufferViewType.GBVT_Srv | EGpuBufferViewType.GBVT_Rtv);
            AddOutput(DepthPinOut, EGpuBufferViewType.GBVT_Srv | EGpuBufferViewType.GBVT_Dsv);
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
                PickedBuffer.OnResize(x * scaleFactor, y * scaleFactor);
        }
        public UPickedProxiableManager PickedManager;
        public UPickSetupShading PickedShading = null;
        public UGraphicsBuffers PickedBuffer { get; protected set; } = new UGraphicsBuffers();
        public UDrawBuffers BasePass = new UDrawBuffers();
        public RHI.CRenderPass RenderPass;

        public async override System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            PickedShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UPickSetupShading>();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);
            BasePass.SetDebugName("UPickedProxiableManager");

            var PassDesc = new IRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].Format = PickedPinOut.Attachement.Format;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.Format = DepthPinOut.Attachement.Format;
                PassDesc.m_AttachmentDepthStencil.Samples = 1;
                PassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = new Color4(1, 0, 1, 0);
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
            }
            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in PassDesc); 

            PickedBuffer.Initialize(policy, RenderPass);
            PickedBuffer.SetRenderTarget(policy, 0, PickedPinOut);
            PickedBuffer.SetDepthStencil(policy, DepthPinOut);
            
            PickedBuffer.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;

            PickedManager = policy.PickedProxiableManager;
        }
        public override void Cleanup()
        {
            PickedBuffer?.Cleanup();
            PickedBuffer = null;

            base.Cleanup();
        }
        List<Mesh.UMesh> mPickedMeshes = new List<Mesh.UMesh>();
        public override unsafe void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            var cmdlist = BasePass.DrawCmdList;
            cmdlist.ClearMeshDrawPassArray();
            cmdlist.SetViewport(PickedBuffer.ViewPort.mCoreObject);

            mPickedMeshes.Clear();
            foreach (var i in PickedManager.PickedProxies)
            {
                i.GetHitProxyDrawMesh(mPickedMeshes);
            }
            foreach (var mesh in mPickedMeshes)
            {
                if (mesh == null || mesh.Atoms == null)
                    continue;

                for (int j = 0; j < mesh.Atoms.Length; j++)
                {
                    var drawcall = mesh.GetDrawCall(PickedBuffer, j, policy, Graphics.Pipeline.URenderPolicy.EShadingType.Picked, this);
                    if (drawcall != null)
                    {
                        if (PickedBuffer.PerViewportCBuffer != null)
                            drawcall.mCoreObject.BindShaderCBuffer(drawcall.Effect.ShaderIndexer.cbPerViewport, PickedBuffer.PerViewportCBuffer.mCoreObject);
                        if (policy.DefaultCamera.PerCameraCBuffer != null)
                            drawcall.mCoreObject.BindShaderCBuffer(drawcall.Effect.ShaderIndexer.cbPerCamera, policy.DefaultCamera.PerCameraCBuffer.mCoreObject);

                        cmdlist.PushDrawCall(drawcall.mCoreObject);
                    }
                }
            }

            if(cmdlist.BeginCommand())
            {
                var passClears = new IRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4(1, 0, 1, 0));
                if (cmdlist.BeginRenderPass(policy, PickedBuffer, in passClears, "Picked"))
                {
                    cmdlist.BuildRenderPass(0);
                    cmdlist.EndRenderPass();
                }
                cmdlist.EndCommand();
            }
        }
        public unsafe override void TickRender(URenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var cmdlist = BasePass.CommitCmdList.mCoreObject;
            cmdlist.Commit(rc.mCoreObject);
        }
        public unsafe override void TickSync(URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
