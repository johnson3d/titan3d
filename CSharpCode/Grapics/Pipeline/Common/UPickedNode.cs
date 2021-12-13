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
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position,
                EVertexSteamType.VST_Normal,
                EVertexSteamType.VST_Tangent,};
        }
    }
    public class UPickedNode : URenderGraphNode
    {
        public UPickedProxiableManager PickedManager;
        public UPickSetupShading PickedShading = null;
        public UGraphicsBuffers PickedBuffer { get; protected set; } = new UGraphicsBuffers();
        public UDrawBuffers BasePass = new UDrawBuffers();
        public RHI.CRenderPass RenderPass;

        public async override System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat fmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            PickedShading = shading as UPickSetupShading;

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);
            BasePass.SetDebugName("UPickedProxiableManager");

            var PassDesc = new IRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].Format = fmt;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.Format = dsFmt;
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

            PickedBuffer.Initialize(RenderPass, policy.Camera, 1, dsFmt, (uint)x, (uint)y);
            PickedBuffer.CreateGBuffer(0, fmt, (uint)x, (uint)y);
            PickedBuffer.UpdateFrameBuffers(x, y);
            PickedBuffer.TargetViewIdentifier = policy.GetBasePassNode().GBuffers.TargetViewIdentifier;

            PickedManager = policy.PickedProxiableManager;
        }
        public unsafe void Cleanup()
        {
            PickedBuffer?.Cleanup();
            PickedBuffer = null;
        }
        public override void OnResize(IRenderPolicy policy, float x, float y)
        {
            if (PickedBuffer != null)
                PickedBuffer.OnResize(x, y);
        }
        List<Mesh.UMesh> mPickedMeshes = new List<Mesh.UMesh>();
        public override unsafe void TickLogic(GamePlay.UWorld world, IRenderPolicy policy, bool bClear)
        {
            var cmdlist = BasePass.DrawCmdList.mCoreObject;
            cmdlist.ClearMeshDrawPassArray();
            cmdlist.SetViewport(PickedBuffer.ViewPort.mCoreObject);

            mPickedMeshes.Clear();
            foreach (var i in PickedManager.PickedProxies)
            {
                i.GetHitProxyDrawMesh(mPickedMeshes);
            }
            foreach (var mesh in mPickedMeshes)
            {
                if (mesh.Atoms == null)
                    continue;

                for (int j = 0; j < mesh.Atoms.Length; j++)
                {
                    var drawcall = mesh.GetDrawCall(PickedBuffer, j, policy, Graphics.Pipeline.IRenderPolicy.EShadingType.Picked, this);
                    if (drawcall != null)
                    {
                        if (PickedBuffer.PerViewportCBuffer != null)
                            drawcall.mCoreObject.BindShaderCBuffer(drawcall.Effect.CBPerViewportIndex, PickedBuffer.PerViewportCBuffer.mCoreObject);
                        if (PickedBuffer.Camera.PerCameraCBuffer != null)
                            drawcall.mCoreObject.BindShaderCBuffer(drawcall.Effect.CBPerCameraIndex, PickedBuffer.Camera.PerCameraCBuffer.mCoreObject);

                        cmdlist.PushDrawCall(drawcall.mCoreObject);
                    }
                }
            }

            if(cmdlist.BeginCommand())
            {
                var passClears = new IRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4(1, 0, 1, 0));
                if (cmdlist.BeginRenderPass(PickedBuffer.FrameBuffers.mCoreObject, in passClears, "Picked"))
                {
                    cmdlist.BuildRenderPass(0);
                    cmdlist.EndRenderPass();
                }
                cmdlist.EndCommand();
            }
        }
        public unsafe override void TickRender(IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var cmdlist = BasePass.CommitCmdList.mCoreObject;
            cmdlist.Commit(rc.mCoreObject);
        }
        public unsafe override void TickSync(IRenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
