using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class UMobileFSPolicy : IRenderPolicy
    {
        public UDrawBuffers BasePass = new UDrawBuffers();
        public RenderPassDesc PassDesc = new RenderPassDesc();
        public RHI.CShaderResourceView EnvMapSRV;
        public override RHI.CShaderResourceView GetFinalShowRSV()
        {
            return GBuffers.GBufferSRV[0];
        }

        public override async System.Threading.Tasks.Task Initialize(float x, float y)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc);
            
            GBuffers.Initialize(1, EPixelFormat.PXF_D24_UNORM_S8_UINT, (uint)x, (uint)y);
            GBuffers.CreateGBuffer(0, EPixelFormat.PXF_R8G8B8A8_UNORM, (uint)x, (uint)y);
            GBuffers.SwapChainIndex = -1;
            GBuffers.TargetViewIdentifier = new UGraphicsBuffers.UTargetViewIdentifier();

            PassDesc.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
            PassDesc.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mDepthClearValue = 1.0f;
            PassDesc.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mStencilClearValue = 0u;

            mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<Pipeline.Mobile.UBasePassOpaque>();
            EnvMapSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(RName.GetRName("utest/texture/default_envmap.srv"));
        }
        public override void OnResize(float x, float y)
        {
            base.OnResize(x, y);
            if (GBuffers != null)
            {
                GBuffers.OnResize(x, y);
            }
        }
        public override void Cleanup()
        {
            base.Cleanup();
        }
        protected UBasePassOpaque mBasePassShading;
        public override Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh)
        {
            if (mesh.Tag != null)
            {
                if (type == EShadingType.BasePass)
                {
                    return mesh.Tag.GetPassShading(type, mesh);
                }
            }
            switch (type)
            {
                case EShadingType.BasePass:
                    if (mBasePassShading == null)
                        mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<Pipeline.Mobile.UBasePassOpaque>();
                    return mBasePassShading;
            }
            return null;
        }
        public override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Mesh.UMesh mesh)
        {
            if (mesh.Tag != null)
            {
                var shading = mesh.Tag.GetPassShading(shadingType, mesh) as Mobile.UBasePassShading;
                if(shading!=null)
                    shading.OnDrawCall(shadingType, drawcall, this, mesh);
            }
            else
            {
                if (shadingType == EShadingType.BasePass)
                    mBasePassShading.OnDrawCall(shadingType, drawcall, this, mesh);
            }
        }
        public unsafe override void TickLogic()
        {
            var cmdlist = BasePass.DrawCmdList.mCoreObject;
            cmdlist.ClearMeshDrawPassArray();
            cmdlist.SetViewport(GBuffers.ViewPort.mCoreObject);
            //cmdlist.SetScissorRect(ScissorRect.mCoreObject.Ptr);
            foreach (var i in VisibleMeshes)
            {
                if (i.Atoms == null)
                    continue;
    
                for (int j = 0; j < i.Atoms.Length; j++)
                {
                    var drawcall = i.GetDrawCall(GBuffers, (uint)j, this, EShadingType.BasePass);
                    if (drawcall == null)
                        continue;

                    GBuffers.SureCBuffer(drawcall.Effect, "UMobileFSPolicy");
                    if (GBuffers.PerViewportCBuffer != null)
                        drawcall.mCoreObject.BindCBufferAll(drawcall.Effect.CBPerViewportIndex, GBuffers.PerViewportCBuffer.mCoreObject);
                    if (GBuffers.Camera.PerCameraCBuffer != null)
                        drawcall.mCoreObject.BindCBufferAll(drawcall.Effect.CBPerCameraIndex, GBuffers.Camera.PerCameraCBuffer.mCoreObject);
                    
                    cmdlist.PushDrawCall(drawcall.mCoreObject);
                }
            }
            cmdlist.BeginCommand();
            cmdlist.BeginRenderPass(ref PassDesc, GBuffers.FrameBuffers.mCoreObject);
            int DPLimitter = int.MaxValue;
            //IDrawCall* tmp = (IDrawCall*)0;
            cmdlist.BuildRenderPass(0, DPLimitter, (IDrawCall**)0);
            cmdlist.EndRenderPass();
            cmdlist.EndCommand();
        }
        public unsafe override void TickRender()
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var cmdlist = BasePass.CommitCmdList.mCoreObject;
            cmdlist.Commit(rc.mCoreObject);
        }
        public unsafe override void TickSync()
        {
            BasePass.SwapBuffer();
            GBuffers?.Camera?.mCoreObject.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, 1);
        }
    }
}
