using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class UMobileFSPolicy : IRenderPolicy
    {
        public UPassDrawBuffers BasePass = new UPassDrawBuffers();
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

            GBuffers.SwapChainIndex = -1;
            GBuffers.Initialize(1, EPixelFormat.PXF_D24_UNORM_S8_UINT, (uint)x, (uint)y);
            GBuffers.CreateGBuffer(0, EPixelFormat.PXF_R16G16B16A16_FLOAT, (uint)x, (uint)y);
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
            //mBasePassShading.EnvMapSRV = EnvMapSRV;
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
        public override Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh, int atom)
        {
            switch (type)
            {
                case EShadingType.BasePass:
                    return mBasePassShading;
            }
            return null;
        }
        public override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Mesh.UMesh mesh, int atom)
        {
            base.OnDrawCall(shadingType, drawcall, mesh, atom);
            if (shadingType == EShadingType.BasePass)
                mBasePassShading.OnDrawCall(shadingType, drawcall, this, mesh);
        }
        public unsafe override void TickLogic()
        {
            BasePass.ClearMeshDrawPassArray();
            BasePass.SetViewport(GBuffers.ViewPort);
            //cmdlist.SetScissorRect(ScissorRect.mCoreObject.Ptr);
            foreach (var i in VisibleMeshes)
            {
                if (i.Atoms == null)
                    continue;
                for (int j = 0; j < i.Atoms.Length; j++)
                {
                    var drawcall = i.GetDrawCall(GBuffers, j, this, EShadingType.BasePass);
                    if (drawcall == null)
                        continue;

                    GBuffers.SureCBuffer(drawcall.Effect, "UMobileFSPolicy");
                    drawcall.BindGBuffer(GBuffers);

                    var layer = i.Atoms[j].Material.RenderLayer;
                    BasePass.PushDrawCall(layer, drawcall);
                }
            }

            BasePass.BuildRenderPass(ref PassDesc, GBuffers.FrameBuffers);
        }
        public unsafe override void TickRender()
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Commit(rc);
        }
        public unsafe override void TickSync()
        {
            BasePass.SwapBuffer();
            GBuffers?.Camera?.mCoreObject.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, 1);
        }
    }
}
