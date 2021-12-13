using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    public class UDeferredOpaque : Shader.UShadingEnv
    {
        public UDeferredOpaque()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Deferred/DeferredOpaque.cginc", RName.ERNameType.Engine);
        }
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position,
                EVertexSteamType.VST_Normal,
                EVertexSteamType.VST_Tangent,
                EVertexSteamType.VST_UV,
                EVertexSteamType.VST_Color};
        }
    }
    public class UDeferredBasePassNode : Common.UBasePassNode
    {
        public UDeferredOpaque mOpaqueShading;
        public UDrawBuffers BasePass = new UDrawBuffers();
        public RHI.CRenderPass RenderPass;
        public async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat rtFmt, EPixelFormat dsFmt, float x, float y)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, "BasePass");

            var PassDesc = new IRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 3;
                PassDesc.AttachmentMRTs[0].Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.AttachmentMRTs[1].Format = EPixelFormat.PXF_R10G10B10A2_UNORM;
                PassDesc.AttachmentMRTs[1].Samples = 1;
                PassDesc.AttachmentMRTs[1].LoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[1].StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.AttachmentMRTs[2].Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                PassDesc.AttachmentMRTs[2].Samples = 1;
                PassDesc.AttachmentMRTs[2].LoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[2].StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.Format = dsFmt;
                PassDesc.m_AttachmentDepthStencil.Samples = 1;
                PassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
                //PassDesc.mDepthClearValue = 1.0f;                
                //PassDesc.mStencilClearValue = 0u;
            }
            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in PassDesc);

            GBuffers.Initialize(RenderPass, policy.Camera, 3, dsFmt, (uint)x, (uint)y);
            GBuffers.CreateGBuffer(0, EPixelFormat.PXF_R8G8B8A8_UNORM, (uint)x, (uint)y);
            GBuffers.CreateGBuffer(1, EPixelFormat.PXF_R10G10B10A2_UNORM, (uint)x, (uint)y);
            GBuffers.CreateGBuffer(2, EPixelFormat.PXF_R8G8B8A8_UNORM, (uint)x, (uint)y);
            GBuffers.UpdateFrameBuffers(x, y);

            GBuffers.TargetViewIdentifier = new UGraphicsBuffers.UTargetViewIdentifier();

            
            mOpaqueShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UDeferredOpaque>();
        }
        public virtual void Cleanup()
        {
            GBuffers?.Cleanup();
            GBuffers = null;
        }
        public override void OnResize(IRenderPolicy policy, float x, float y)
        {
            if (GBuffers != null)
            {
                GBuffers.OnResize(x, y);
            }
        }
        public override void TickLogic(GamePlay.UWorld world, IRenderPolicy policy, bool bClear)
        {
            BasePass.DrawCmdList.mCoreObject.ClearMeshDrawPassArray();
            //BasePass.DrawCmdList.mCoreObject.SetViewport(GBuffers.ViewPort.mCoreObject);
            foreach (var i in policy.VisibleMeshes)
            {
                if (i.Atoms == null)
                    continue;

                for (int j = 0; j < i.Atoms.Length; j++)
                {
                    var layer = i.Atoms[j].Material.RenderLayer;
                    if (layer != ERenderLayer.RL_Opaque)
                        continue;

                    var drawcall = i.GetDrawCall(GBuffers, j, policy, IRenderPolicy.EShadingType.BasePass, this);
                    if (drawcall != null)
                    {
                        drawcall.BindGBuffer(GBuffers);
                        
                        BasePass.DrawCmdList.mCoreObject.PushDrawCall(drawcall.mCoreObject);
                    }
                }
            }

            var cmdlist = BasePass.DrawCmdList.mCoreObject;
            var passClears = new IRenderPassClears();
            passClears.SetDefault();
            passClears.SetClearColor(0, new Color4(1, 0, 0, 0));
            cmdlist.BeginCommand();
            cmdlist.SetViewport(GBuffers.ViewPort.mCoreObject);
            cmdlist.BeginRenderPass(GBuffers.FrameBuffers.mCoreObject, in passClears, ERenderLayer.RL_Opaque.ToString());
            cmdlist.BuildRenderPass(0);
            cmdlist.EndRenderPass();
            cmdlist.EndCommand();
        }
        public override void TickRender(IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.CommitCmdList.mCoreObject.Commit(rc.mCoreObject);
        }
        public override void TickSync(IRenderPolicy policy)
        {
            BasePass.SwapBuffer();
            GBuffers?.Camera?.mCoreObject.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, 1);
        }
    }
}
