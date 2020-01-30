using EngineNS.Graphics.EnvShader;
using EngineNS.Graphics.PostEffect;
using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Graphics.View;
using EngineNS.Graphics.Tool;
using EngineNS.Graphics.Shadow;

namespace EngineNS.Graphics.RenderPolicy
{
    public class CGfxRP_GameMobile : CGfxRenderPolicy
    {
        public CGfxFramePass mForwardBasePass;
        public CGfxPostprocessPass mCopyPostprocessPass;

        //env shader;
        public CGfxMobileOpaqueSE mOpaqueSE;
        public CGfxMobileCustomTranslucentSE mCustomTranslucentSE;
        public CGfxMobileTranslucentSE mTranslucentSE;
        public CGfxMobileCopySE mCopySE;
        public CGfxSE_MobileSky mSE_MobileSky;

        public CGfxSSM mSSM;
        private CGfxSunShaftMobile mSunShaftMobile;

        //post effect;
        private CGfxMobileBloom mBloomMobile;
        private CGfxMobileAO mMobileAO;

        public CGfxRP_GameMobile()
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;

            //shadow ssm;
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

            SwapChain.Cleanup();
            SwapChain = null;

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
            if (RHICtx == null)
                RHICtx = CEngine.Instance.RenderContext;

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

            if (mSE_MobileSky == null)
            {
                mSE_MobileSky = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxSE_MobileSky>();
            }

            if (mOpaqueSE == null || mCustomTranslucentSE == null || mTranslucentSE == null || mCopySE == null || mSE_MobileSky ==null)
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
                if (RHICtx.ContextCaps.SupportHalfRT == 0)
                {
                    CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EnvShader.CGfxMobileOpaqueSE>().SetMacroDefineValue("ENV_DISABLE_AO", "1");
                    CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EnvShader.CGfxMobileCopySE>().SetMacroDefineValue("ENV_DISABLE_AO", "1"); 
                    CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Bricks.GpuDriven.GpuScene.CGfxMergeInstanceSE>().SetMacroDefineValue("ENV_DISABLE_AO", "1");
                    rtDesc0.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                }
                else
                {
                    CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EnvShader.CGfxMobileOpaqueSE>().SetMacroDefineValue("ENV_DISABLE_AO", "0");
                    CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EnvShader.CGfxMobileCopySE>().SetMacroDefineValue("ENV_DISABLE_AO", "0");
                    CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<Bricks.GpuDriven.GpuScene.CGfxMergeInstanceSE>().SetMacroDefineValue("ENV_DISABLE_AO", "0");
                    rtDesc0.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
                }
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Graphics", $"MobileGame render texture format: {rtDesc0.Format}");
                rtDesc0.Width = width;
                rtDesc0.Height = height;
                BaseViewInfo.mRTVDescArray.Add(rtDesc0);

                BaseSceneView = new CGfxSceneView();
                if (false == BaseSceneView.Init(RHICtx, null, BaseViewInfo))
                {
                    return false;
                }
                BaseSceneView.UIHost = await CEngine.Instance.UIManager.RegisterHost("Game");
                BaseSceneView.UIHost.IsInputActive = true;
                BaseSceneView.UIHost.WindowSize = new SizeF(width, height);

                mForwardBasePass = new CGfxFramePass(RHICtx, "GameForwordBasePse");
                mForwardBasePass.mBaseSceneView = BaseSceneView;

                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_Opaque, PrebuildPassIndex.PPI_OpaquePbr, mOpaqueSE);
                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_CustomOpaque, PrebuildPassIndex.PPI_OpaquePbr, mOpaqueSE);
                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_Sky, PrebuildPassIndex.PPI_Sky, mSE_MobileSky);
                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_CustomTranslucent, PrebuildPassIndex.PPI_CustomTranslucentPbr, mCustomTranslucentSE);
                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_Translucent, PrebuildPassIndex.PPI_TransparentPbr, mTranslucentSE);

                FrameBufferClearColor TempClearColor = new FrameBufferClearColor();
                TempClearColor.r = 0.0f;
                TempClearColor.g = 0.0f;
                TempClearColor.b = 0.0f;
                TempClearColor.a = 1.0f;
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
                EngineNS.CSwapChainDesc SwapChainDesc;
                SwapChainDesc.Format = EngineNS.EPixelFormat.PXF_R8G8B8A8_UNORM;
                SwapChainDesc.Width = width;
                SwapChainDesc.Height = height;
                SwapChainDesc.WindowHandle = WinHandle;
                SwapChainDesc.ColorSpace = EColorSpace.COLOR_SPACE_SRGB_NONLINEAR;
                SwapChain = RHICtx.CreateSwapChain(SwapChainDesc);
                //RHICtx.BindCurrentSwapChain(mSwapChain);

                //ufo scene view;
                CGfxScreenViewDesc UFOViewInfo = new CGfxScreenViewDesc();
                UFOViewInfo.IsSwapChainBuffer = true;
                UFOViewInfo.UseDepthStencilView = false;
                UFOViewInfo.Width = width;
                UFOViewInfo.Height = height;

                var rtDesc1 = new CRenderTargetViewDesc();
                rtDesc1.mCanBeSampled = vBOOL.FromBoolean(false);
                UFOViewInfo.mRTVDescArray.Add(rtDesc1);

                mCopyPostprocessPass = new CGfxPostprocessPass();
                await mCopyPostprocessPass.Init(RHICtx, SwapChain, UFOViewInfo, mCopySE, RName.GetRName("Material/defaultmaterial.instmtl"), "GameCopyPost");

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

            //shadow ssm;
            bool IsReady = await mSSM.Init(RHICtx);
            if (IsReady == false)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"SSM Error", "");
                return false;
            }

            //post effect;
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
        public override void SetEnvMap(RName name)
        {
            mOpaqueSE.EnvMapName = name;
            mCustomTranslucentSE.EnvMapName = name;
            mTranslucentSE.EnvMapName = name;
        }
        public override void TickLogic(CGfxSceneView view, CRenderContext RHICtx)
        {
            if (BaseSceneView == null)
            {
                return;
            }
            ScopeTickLogic.Begin();

            int drawCall = 0;
            int drawTriangle = 0;
            UInt32 cmdCount = 0;

            Camera.PushVisibleSceneMesh2RenderLayer();
            Vector3 DirLightDir = new Vector3(BaseSceneView.mDirLightDirection_Leak.X, BaseSceneView.mDirLightDirection_Leak.Y, BaseSceneView.mDirLightDirection_Leak.Z);
            mSSM.TickLogic(RHICtx, Camera, DirLightDir);

            BaseSceneView.mFadeParam = mSSM.mFadeParam;
            BaseSceneView.mShadowTransitionScale = mSSM.mShadowTransitionScale;
            BaseSceneView.mShadowMapSizeAndRcp = mSSM.mShadowMapSizeAndRcp;
            BaseSceneView.mViewer2ShadowMtx = mSSM.mViewer2ShadowMtx;
            BaseSceneView.mShadowDistance = mSSM.mShadowDistance;
            mForwardBasePass.TickLogic(Camera, view, RHICtx, DPLimitter, GraphicsDebug);

            drawCall += mForwardBasePass.CommitingCMDs.DrawCall;
            drawTriangle += mForwardBasePass.CommitingCMDs.DrawTriangle;
            cmdCount += mForwardBasePass.CommitingCMDs.CmdCount;

            //post effect;
            mMobileAO.TickLogic(RHICtx, Camera, BaseSceneView);
            mBloomMobile.TickLogic(RHICtx, Camera);

            mSunShaftMobile.TickLogic(RHICtx, Camera, DirLightDir, BaseSceneView);

            {
                mCopyPostprocessPass.mScreenView.SunPosNDC = mSunShaftMobile.mSunPosNdc;
                mCopyPostprocessPass.mScreenView.DirLightColor_Intensity = BaseSceneView.DirLightColor_Intensity;
                mCopyPostprocessPass.TickLogic(Camera, view, RHICtx);

                RiseOnDrawUI(mCopyPostprocessPass.CommitingCMDs, mCopyPostprocessPass.mScreenView);
            }

            //CmdList.BeginCommand();
            //CmdList.BeginRenderPass(mRenderPassDesc_Final, mFinalView.FrameBuffer);
            //LatestPass = CmdList.BuildRenderPass(ref DPLimitter, GraphicsDebug);
            //CmdList.EndRenderPass();
            //CmdList.EndCommand();

            drawCall += mCopyPostprocessPass.CommitingCMDs.DrawCall;
            drawTriangle += mCopyPostprocessPass.CommitingCMDs.DrawTriangle;
            cmdCount += mCopyPostprocessPass.CommitingCMDs.CmdCount;

            DrawCall = drawCall;
            DrawTriangle = drawTriangle;
            CmdCount = cmdCount;

            ScopeTickLogic.End();
        }
        public override void TickRender(CSwapChain swapChain)
        {
            base.TickRender(SwapChain);

            var RHICtx = EngineNS.CEngine.Instance.RenderContext;
            if (RHICtx == null)
                return;

            //RHICtx.BindCurrentSwapChain(mSwapChain);

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

            swapChain.Present();
        }

        public override void TickSync()
        {
            //shadow ssm;
            mSSM.TickSync();

            mForwardBasePass.TickSync();

            //post effect;
            mMobileAO.TickSync();
            if (CEngine.EnableBloom == true)
            {
                mBloomMobile.TickSync();
            }
            mSunShaftMobile.TickSync();

            mCopyPostprocessPass.TickSync();
            
            base.TickSync();

            //this is the end of frame;
            Camera.ClearAllRenderLayerData();
        }

        public override void OnResize(CRenderContext RHICtx, CSwapChain SwapChain, UInt32 width, UInt32 height)
        {
            if (Camera == null || mBloomMobile.mUSView8 == null)
            {
                return;
            }

            SwapChain.OnResize(width, height);

            //RHICtx.BindCurrentSwapChain(mSwapChain);

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

        public void SetGraphicsProfiler(CGraphicsProfiler profiler)
        {
            mCopyPostprocessPass.SetGraphicsProfiler(profiler);

            mSSM?.SetGraphicsProfiler(profiler);
        }
    }
}
