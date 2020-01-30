using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Graphics.View;
using EngineNS.Graphics.EnvShader;
using EngineNS.Graphics.Shadow;
using EngineNS.Graphics.PostEffect;

namespace EngineNS.Graphics.RenderPolicy
{
    public class CGfxRP_OffScreen : CGfxRenderPolicy
    {
        public CGfxFramePass mForwardBasePass;
        public CGfxPostprocessPass mCopyPostprocessPass;

        public CGfxMobileOpaqueSE mOpaqueSE;
        public CGfxMobileCustomTranslucentSE mCustomTranslucentSE;
        public CGfxMobileTranslucentSE mTranslucentSE;
        public CGfxMobileCopySE mCopySE;
        public CGfxSE_MobileSky mSE_MobileSky;
        public CGfxGizmosSE mGizmosSE;

        public CGfxSSM mSSM;
        private CGfxSunShaftMobile mSunShaftMobile;

        //post effect;
        private CGfxMobileBloom mBloomMobile;
        private CGfxMobileAO mMobileAO;

        
        public CGfxRP_OffScreen()
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;

            mSSM = new CGfxSSM();

            //post effect;
            mBloomMobile = new CGfxMobileBloom(RHICtx);
            mSunShaftMobile = new CGfxSunShaftMobile(RHICtx);
            mMobileAO = new CGfxMobileAO(RHICtx);
        }
        public override void Cleanup()
        {
            mForwardBasePass.Cleanup();
            mCopyPostprocessPass.Cleanup();

            BaseSceneView.Cleanup();
            BaseSceneView = null;

            //shadow ssm
            mSSM.Cleanup();

            //post effect
            mBloomMobile.Cleanup();
            mSunShaftMobile.Cleanup();
            mMobileAO.Cleanup();

            base.Cleanup();
        }

        public override async System.Threading.Tasks.Task<bool> Init(CRenderContext RHICtx, UInt32 width, UInt32 height, CGfxCamera camera, IntPtr WinHandle)
        {
            if (mOpaqueSE == null)
            {
                mOpaqueSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileOpaqueSE>();
            }

            if (mCustomTranslucentSE == null)
            {
                mCustomTranslucentSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileCustomTranslucentSE>();
            }

            if (mTranslucentSE == null)
            {
                mTranslucentSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileTranslucentSE>();
            }

            if (mCopySE == null)
            {
                mCopySE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileCopySE>();
            }

            if (mGizmosSE == null)
            {
                mGizmosSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxGizmosSE>();
            }

            if (mSE_MobileSky == null)
            {
                mSE_MobileSky = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxSE_MobileSky>();
            }

            if (mOpaqueSE == null || mCustomTranslucentSE == null || mTranslucentSE == null || mCopySE == null || mGizmosSE == null || mSE_MobileSky == null)
            {
                return false;
            }

            {
                //base scene view;
                CGfxSceneViewInfo BaseViewInfo = new CGfxSceneViewInfo();
                BaseViewInfo.mUseDSV = true;
                BaseViewInfo.Width = width;
                BaseViewInfo.Height = height;
                BaseViewInfo.mDSVDesc.Init();
                BaseViewInfo.mDSVDesc.Format = EngineNS.EPixelFormat.PXF_D24_UNORM_S8_UINT;
                BaseViewInfo.mDSVDesc.Width = width;
                BaseViewInfo.mDSVDesc.Height = height;

                CRenderTargetViewDesc rtDesc0 = new CRenderTargetViewDesc();
                rtDesc0.Init();
                //rtDesc0.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                rtDesc0.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
                rtDesc0.Width = width;
                rtDesc0.Height = height;
                BaseViewInfo.mRTVDescArray.Add(rtDesc0);

                BaseSceneView = new CGfxSceneView();
                if (false == BaseSceneView.Init(RHICtx, null, BaseViewInfo))
                {
                    return false;
                }
                BaseSceneView.UIHost = new UISystem.UIHost();
                await BaseSceneView.UIHost.Initialize(RHICtx, new UISystem.UIHostInitializer());
                BaseSceneView.UIHost.WindowSize = new SizeF(width, height);

                mForwardBasePass = new CGfxFramePass(RHICtx, "OffSrcFWBasePass");
                mForwardBasePass.mBaseSceneView = BaseSceneView;

                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_Opaque, PrebuildPassIndex.PPI_OpaquePbr, mOpaqueSE);
                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_CustomOpaque, PrebuildPassIndex.PPI_OpaquePbr, mOpaqueSE);
                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_Sky, PrebuildPassIndex.PPI_Sky, mSE_MobileSky);
                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_CustomTranslucent, PrebuildPassIndex.PPI_CustomTranslucentPbr, mCustomTranslucentSE);
                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_Translucent, PrebuildPassIndex.PPI_Gizmos, mTranslucentSE);
                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_Gizmos, PrebuildPassIndex.PPI_TransparentPbr, mGizmosSE);

                FrameBufferClearColor TempClearColor = new FrameBufferClearColor();
                TempClearColor.r = 0.0f;
                TempClearColor.g = 0.0f;
                TempClearColor.b = 0.0f;
                TempClearColor.a = 0.0f;
                mForwardBasePass.mRenderPassDesc.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
                mForwardBasePass.mRenderPassDesc.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
                mForwardBasePass.mRenderPassDesc.mFBClearColorRT0 = TempClearColor;
                mForwardBasePass.mRenderPassDesc.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
                mForwardBasePass.mRenderPassDesc.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
                mForwardBasePass.mRenderPassDesc.mDepthClearValue = 1.0f;
                mForwardBasePass.mRenderPassDesc.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
                mForwardBasePass.mRenderPassDesc.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
                mForwardBasePass.mRenderPassDesc.mStencilClearValue = 0u;
            }

            {
                //ufo scene view;
                CGfxScreenViewDesc UFOViewInfo = new CGfxScreenViewDesc();
                UFOViewInfo.IsSwapChainBuffer = false;
                UFOViewInfo.UseDepthStencilView = false;
                UFOViewInfo.Width = width;
                UFOViewInfo.Height = height;

                var rtDesc1 = new CRenderTargetViewDesc();
                rtDesc1.Init();
                rtDesc1.mCanBeSampled = vBOOL.FromBoolean(true);
                rtDesc1.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                rtDesc1.Width = width;
                rtDesc1.Height = height;
                UFOViewInfo.mRTVDescArray.Add(rtDesc1);
                
                mCopyPostprocessPass = new CGfxPostprocessPass();
                await mCopyPostprocessPass.Init(RHICtx, SwapChain, UFOViewInfo, mCopySE, RName.GetRName("Material/defaultmaterial.instmtl"), "OffSrcCopyPost");

                FrameBufferClearColor TempClearColor = new FrameBufferClearColor();
                TempClearColor.r = 0.0f;
                TempClearColor.g = 0.0f;
                TempClearColor.b = 0.0f;
                TempClearColor.a = 0.0f;
                mCopyPostprocessPass.mRenderPassDesc.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
                mCopyPostprocessPass.mRenderPassDesc.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
                mCopyPostprocessPass.mRenderPassDesc.mFBClearColorRT0 = TempClearColor;
                mCopyPostprocessPass.mRenderPassDesc.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
                mCopyPostprocessPass.mRenderPassDesc.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
                mCopyPostprocessPass.mRenderPassDesc.mDepthClearValue = 1.0f;
                mCopyPostprocessPass.mRenderPassDesc.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
                mCopyPostprocessPass.mRenderPassDesc.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
                mCopyPostprocessPass.mRenderPassDesc.mStencilClearValue = 0u;
            }

            Camera = camera;
            Camera.SetSceneView(RHICtx, BaseSceneView);

            bool IsReady = await mSSM.Init(RHICtx);
            if (IsReady == false)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"SSM Error", "");
                return false;
            }

            IsReady = await mBloomMobile.Init(RHICtx, width, height, BaseSceneView);
            if (IsReady == false)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"BloomError", "");
                return false;
            }

            //sun shaft;
            IsReady = await mSunShaftMobile.Init(RHICtx, width, height);
            if (IsReady == false)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }

            //mobile ao;
            IsReady = await mMobileAO.Init(RHICtx, width, height, BaseSceneView);
            if (IsReady == false)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }

            //
            mOpaqueSE.mTex_ShadowMap = mSSM.mShadowMapView.FrameBuffer.GetSRV_DepthStencil();
            mOpaqueSE.EnvMapName = CEngine.Instance.Desc.DefaultEnvMap;
            mOpaqueSE.EyeEnvMapName = CEngine.Instance.Desc.DefaultEyeEnvMap;

            mCustomTranslucentSE.EnvMapName = CEngine.Instance.Desc.DefaultEnvMap;
            mCustomTranslucentSE.mTex_ShadowMap = mSSM.mShadowMapView.FrameBuffer.GetSRV_DepthStencil();

            mTranslucentSE.EnvMapName = CEngine.Instance.Desc.DefaultEnvMap;

            mCopySE.mBaseSceneView = BaseSceneView.FrameBuffer.GetSRV_RenderTarget(0);
            if (CEngine.EnableBloom == true)
                mCopySE.mBloomTex = mBloomMobile.mUSView8.FrameBuffer.GetSRV_RenderTarget(0);
            else
                mCopySE.mBloomTex = BaseSceneView.FrameBuffer.GetSRV_RenderTarget(0);
            mCopySE.VignetteTex = CEngine.Instance.Desc.DefaultVignette;
            mCopySE.mSunShaftTex = mSunShaftMobile.mView_Blur.FrameBuffer.GetSRV_RenderTarget(0);
            //mCopySE.mSRV_MobileAo = mMobileAO.mView_BlurV.FrameBuffer.GetSRV_RenderTarget(0);
            mCopySE.mSRV_MobileAo = mMobileAO.mView_AoMask.FrameBuffer.GetSRV_RenderTarget(0);

            {
                var Tex2DDesc = mOpaqueSE.EnvMap.TxPicDesc;
                BaseSceneView.mEnvMapMipMaxLevel = Tex2DDesc.MipLevel - 1;

                Tex2DDesc = mOpaqueSE.EyeEnvMap.TxPicDesc;
                BaseSceneView.mEyeEnvMapMaxMipLevel = Tex2DDesc.MipLevel - 1;

                var ViewportSizeAndRcp = new Vector4(width, height, 1.0f / width, 1.0f / height);
                mCopyPostprocessPass.mScreenView.ViewportSizeAndRcp = ViewportSizeAndRcp;
            }

            return true;
        }

        public override void TickLogic(CGfxSceneView view, CRenderContext RHICtx)
        {
            if (BaseSceneView == null)
            {
                return;
            }

            Camera.PushVisibleSceneMesh2RenderLayer();

            Vector3 DirLightDir = new Vector3(BaseSceneView.mDirLightDirection_Leak.X, BaseSceneView.mDirLightDirection_Leak.Y, BaseSceneView.mDirLightDirection_Leak.Z);
            mSSM.TickLogic(RHICtx, Camera, DirLightDir);

            BaseSceneView.mFadeParam = mSSM.mFadeParam;
            BaseSceneView.mShadowTransitionScale = mSSM.mShadowTransitionScale;
            BaseSceneView.mShadowMapSizeAndRcp = mSSM.mShadowMapSizeAndRcp;
            BaseSceneView.mViewer2ShadowMtx = mSSM.mViewer2ShadowMtx;
            BaseSceneView.mShadowDistance = mSSM.mShadowDistance;


            mForwardBasePass.TickLogic(Camera, view, RHICtx, DPLimitter, GraphicsDebug);

            //post effect;
            mMobileAO.TickLogic(RHICtx, Camera, BaseSceneView);
            if (CEngine.EnableBloom == true)
            {
                mBloomMobile.TickLogic(RHICtx, Camera);
            }
            mSunShaftMobile.TickLogic(RHICtx, Camera, DirLightDir, BaseSceneView);

            {
                mSunShaftMobile.mSunPosNdc.Z = 0.25f * mSunShaftMobile.mSunShaftAtten;
                if (mSunShaftMobile.mStopSunShaftUpdate == true)
                {
                    //disable sun shaft;
                    mSunShaftMobile.mSunPosNdc.W = 0.0f;
                }
                else
                {
                    mSunShaftMobile.mSunPosNdc.W = 1.0f;
                }
                mCopyPostprocessPass.mScreenView.SunPosNDC = mSunShaftMobile.mSunPosNdc;
                mCopyPostprocessPass.mScreenView.DirLightColor_Intensity = BaseSceneView.DirLightColor_Intensity;
                
                mCopyPostprocessPass.TickLogic(Camera, view, RHICtx);

                this.RiseOnDrawUI(mCopyPostprocessPass.CommitingCMDs, mCopyPostprocessPass.mScreenView);
            }
        }
        public override void TickRender(CSwapChain swapChain)
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;

            //shadow ssm;
            mSSM.TickRender(RHICtx);

            mForwardBasePass.TickRender(RHICtx);

            //post effect;
            mMobileAO.TickRender(RHICtx);
            if (CEngine.EnableBloom == true)
            {
                mBloomMobile.TickRender(RHICtx);
            }

            mSunShaftMobile.TickRender(RHICtx);

            mCopyPostprocessPass.TickRender(RHICtx);

            base.TickRender(null);
        }
        public override void TickSync()
        {
            mSSM.TickSync();

            mForwardBasePass.TickSync();

            mMobileAO.TickSync();
            if (CEngine.EnableBloom == true)
            {
                mBloomMobile.TickSync();
            }
            mSunShaftMobile.TickSync();

            mCopyPostprocessPass.TickSync();

            base.TickSync();

            Camera.ClearAllRenderLayerData();
        }
        public override void OnResize(CRenderContext RHICtx, CSwapChain SwapChain, UInt32 width, UInt32 height)
        {
            Camera.PerspectiveFovLH(Camera.mDefaultFoV, (float)width, (float)height);

            mCopyPostprocessPass.mScreenView.OnResize(RHICtx, SwapChain, width, height);
            BaseSceneView.OnResize(RHICtx, null, width, height);

            //post effect;
            mMobileAO.OnResize(RHICtx, width, height, BaseSceneView);
            mBloomMobile.OnResize(RHICtx, width, height, BaseSceneView);
            mSunShaftMobile.OnResize(RHICtx, width, height);

            mCopySE.mBaseSceneView = BaseSceneView.FrameBuffer.GetSRV_RenderTarget(0);
            if (CEngine.EnableBloom == true)
            {
                mCopySE.mBloomTex = mBloomMobile.mUSView8.FrameBuffer.GetSRV_RenderTarget(0);
            }
            else
                mCopySE.mBloomTex = BaseSceneView.FrameBuffer.GetSRV_RenderTarget(0);
            mCopySE.mSunShaftTex = mSunShaftMobile.mView_Blur.FrameBuffer.GetSRV_RenderTarget(0);
            mCopySE.mSRV_MobileAo = mMobileAO.mView_AoMask.FrameBuffer.GetSRV_RenderTarget(0);

            var ViewportSizeAndRcp = new Vector4(width, height, 1.0f / width, 1.0f / height);
            mCopyPostprocessPass.mScreenView.ViewportSizeAndRcp = ViewportSizeAndRcp;
        }
    }
}
