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
    }
    public class UPickedNode : URenderGraphNode
    {
        public UPickedProxiableManager PickedManager;
        public UPickSetupShading PickedShading = null;
        public UGraphicsBuffers PickedBuffer { get; protected set; } = new UGraphicsBuffers();
        public UDrawBuffers BasePass = new UDrawBuffers();
        public RenderPassDesc PassDesc = new RenderPassDesc();

        public async override System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat fmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            PickedShading = shading as UPickSetupShading;

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);
            BasePass.SetDebugName("UPickedProxiableManager");

            PickedBuffer.SwapChainIndex = -1;
            PickedBuffer.Initialize(1, dsFmt, (uint)x, (uint)y);
            PickedBuffer.CreateGBuffer(0, fmt, (uint)x, (uint)y);
            PickedBuffer.TargetViewIdentifier = policy.GetBasePassNode().GBuffers.TargetViewIdentifier;
            PickedBuffer.Camera = policy.GetBasePassNode().GBuffers.Camera;

            PassDesc.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
            PassDesc.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mDepthClearValue = 1.0f;
            PassDesc.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mStencilClearValue = 0u;

            PickedManager = policy.PickedProxiableManager;
        }
        public unsafe void Cleanup()
        {
            PickedBuffer?.Cleanup();
            PickedBuffer = null;
        }
        public void OnResize(float x, float y)
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
                i.GetDrawMesh(mPickedMeshes);
            }
            foreach (var mesh in mPickedMeshes)
            {
                if (mesh.Atoms == null)
                    continue;

                for (int j = 0; j < mesh.Atoms.Length; j++)
                {
                    var drawcall = mesh.GetDrawCall(PickedBuffer, j, policy, Graphics.Pipeline.IRenderPolicy.EShadingType.Picked);
                    if (drawcall != null)
                    {
                        PickedBuffer.SureCBuffer(drawcall.Effect, "UPickedProxiableManager");
                        if (PickedBuffer.PerViewportCBuffer != null)
                            drawcall.mCoreObject.BindCBufferAll(drawcall.Effect.CBPerViewportIndex, PickedBuffer.PerViewportCBuffer.mCoreObject);
                        if (PickedBuffer.Camera.PerCameraCBuffer != null)
                            drawcall.mCoreObject.BindCBufferAll(drawcall.Effect.CBPerCameraIndex, PickedBuffer.Camera.PerCameraCBuffer.mCoreObject);

                        cmdlist.PushDrawCall(drawcall.mCoreObject);
                    }
                }
            }

            cmdlist.BeginCommand();
            cmdlist.BeginRenderPass(ref PassDesc, PickedBuffer.FrameBuffers.mCoreObject, "Picked");
            cmdlist.BuildRenderPass(0);
            cmdlist.EndRenderPass();
            cmdlist.EndCommand();
        }
        public unsafe void TickRender(IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var cmdlist = BasePass.CommitCmdList.mCoreObject;
            cmdlist.Commit(rc.mCoreObject);
        }
        public unsafe void TickSync(IRenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
