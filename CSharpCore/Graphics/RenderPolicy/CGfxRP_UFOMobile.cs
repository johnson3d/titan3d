//using EngineNS.Graphics.EnvShader;
//using EngineNS.Graphics.PostEffect;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using EngineNS.Graphics.View;

//namespace EngineNS.Graphics.RenderPolicy
//{
//    public class CGfxRP_UFOMobile : CGfxRenderPolicy
//    {
//        private CCommandList[] mCLDB_Forward;
//        private CCommandList[] mCLDB_Copy;

//        public CGfxScreenView mUFOSceneView;

//        private CRenderPassDesc mRenderPassDesc_UFOMobileForward;
//        private CRenderPassDesc mRenderPassDesc_UFOMobileFinal;

//        private EngineNS.Graphics.Mesh.CGfxMesh mScreenRect;
        
//        public CGfxUFOMobileOpaqueSE mOpaqueSE;
//        public CGfxUFOMobileTranslucentSE mTranslucentSE;
//        public CGfxUFOMobileCopySE mCopySE;

//        //post effect;
//        private CGfxMobileBloom mBloomMobile;

//        public CGfxRP_UFOMobile()
//        {
//            var RHICtx = EngineNS.CEngine.Instance.RenderContext;

//            mCLDB_Forward = new CCommandList[2];
//            mCLDB_Copy = new CCommandList[2];
            
//            EngineNS.CCommandListDesc clDesc = new EngineNS.CCommandListDesc();
//            mCLDB_Forward[0] = RHICtx.CreateCommandList(clDesc);
//            mCLDB_Forward[1] = RHICtx.CreateCommandList(clDesc);

//            mCLDB_Copy[0] = RHICtx.CreateCommandList(clDesc);
//            mCLDB_Copy[1] = RHICtx.CreateCommandList(clDesc);
            
//            //post effect;
//            mBloomMobile = new CGfxMobileBloom(RHICtx);
//        }

//        public override void Cleanup()
//        {
//            mCLDB_Forward[0].Cleanup();
//            mCLDB_Forward[0] = null;
//            mCLDB_Forward[1].Cleanup();
//            mCLDB_Forward[1] = null;

//            mCLDB_Copy[0].Cleanup();
//            mCLDB_Copy[0] = null;
//            mCLDB_Copy[1].Cleanup();
//            mCLDB_Copy[1] = null;

//            mBaseSceneView.Cleanup();
//            mBaseSceneView = null;
            
//            mSwapChain.Cleanup();
//            mSwapChain = null;
            
//            //post effect
//            mBloomMobile.Cleanup();
            
//            base.Cleanup();
//        }

        
//        public override async System.Threading.Tasks.Task<bool> Init(CRenderContext RHICtx, UInt32 width, UInt32 height, CGfxCamera camera, IntPtr WinHandle)
//        {
//            if (mOpaqueSE == null)
//            {
//                mOpaqueSE = CEngine.Instance.ShadingEnvManager.NewGfxShadingEnv<CGfxUFOMobileOpaqueSE>(
//                        RName.GetRName("ShadingEnv/Deprecated.senv"), RName.GetRName("Shaders/UFOMobileOpaque.shadingenv"));
//            }

//            if (mTranslucentSE == null)
//            {
//                mTranslucentSE = CEngine.Instance.ShadingEnvManager.NewGfxShadingEnv<CGfxUFOMobileTranslucentSE>(
//                        RName.GetRName("ShadingEnv/Deprecated.senv"), RName.GetRName("Shaders/UFOMobileTranslucent.shadingenv"));
//            }


//            if (mCopySE == null)
//            {
//                mCopySE = CEngine.Instance.ShadingEnvManager.NewGfxShadingEnv<CGfxUFOMobileCopySE>(
//                        RName.GetRName("ShadingEnv/Deprecated.senv"), RName.GetRName("Shaders/UFOMobileCopy.shadingenv"));
//            }

//            var RectMesh = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(RHICtx, CEngineDesc.FullScreenRectName, true);

//            mScreenRect = CEngine.Instance.MeshManager.CreateMesh(RHICtx, RectMesh);
//            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(RHICtx, RName.GetRName("Material/defaultmaterial.instmtl"));
//            await mScreenRect.SetMaterial(RHICtx, 0,
//                mtl,
//                CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);

//            EngineNS.CSwapChainDesc SwapChainDesc;
//            SwapChainDesc.Format = EngineNS.EPixelFormat.PXF_R8G8B8A8_UNORM;
//            SwapChainDesc.Width = width;
//            SwapChainDesc.Height = height;
//            SwapChainDesc.WindowHandle = WinHandle;
//            mSwapChain = RHICtx.CreateSwapChain(SwapChainDesc);
//            //RHICtx.BindCurrentSwapChain(mSwapChain);

//            //base scene view;
//            CGfxSceneViewInfo BaseViewInfo = new CGfxSceneViewInfo();
//            BaseViewInfo.mDisuseDSV = false;
//            BaseViewInfo.Width = width;
//            BaseViewInfo.Height = height;
//            BaseViewInfo.DepthStencil.Format = EngineNS.EPixelFormat.PXF_D24_UNORM_S8_UINT;
//            BaseViewInfo.DepthStencil.Width = width;
//            BaseViewInfo.DepthStencil.Height = height;

//            CRenderTargetViewDesc rtDesc0 = new CRenderTargetViewDesc();
//            rtDesc0.SetDefault();
//            //rtDesc0.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
//            rtDesc0.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
//            rtDesc0.Width = width;
//            rtDesc0.Height = height;
//            BaseViewInfo.mRTVDescArray.Add(rtDesc0);

//            mBaseSceneView = new CGfxSceneView();
//            if (false == mBaseSceneView.Init(RHICtx, null, BaseViewInfo))
//            {
//                return false;
//            }

//            //ufo scene view;
//            CGfxScreenViewDesc UFOViewInfo = new CGfxScreenViewDesc();
//            UFOViewInfo.IsSwapChainBuffer = true;
//            UFOViewInfo.UseDepthStencilView = false;
//            UFOViewInfo.Width = width;
//            UFOViewInfo.Height = height;
            
//            var rtDesc1 = new CRenderTargetViewDesc();
//            rtDesc1.mCanBeSampled = 0;
//            UFOViewInfo.mRTVDescArray.Add(rtDesc1);
//            mUFOSceneView = new CGfxScreenView();
//            await mUFOSceneView.Init(RHICtx, mSwapChain, UFOViewInfo, mCopySE, mtl, mScreenRect);

//            mCamera = camera;
//            //mCamera.SetSceneView(RHICtx, mBaseSceneView);

//            mRenderPassDesc_UFOMobileForward = new CRenderPassDesc();
//            FrameBufferClearColor TempClearColor = new FrameBufferClearColor();
//            TempClearColor.r = 0.0f;
//            TempClearColor.g = 0.0f;
//            TempClearColor.b = 0.0f;
//            TempClearColor.a = 0.0f;
//            mRenderPassDesc_UFOMobileForward.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_UFOMobileForward.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_UFOMobileForward.mFBClearColorRT0 = TempClearColor;
//            mRenderPassDesc_UFOMobileForward.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_UFOMobileForward.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_UFOMobileForward.mDepthClearValue = 1.0f;
//            mRenderPassDesc_UFOMobileForward.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_UFOMobileForward.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_UFOMobileForward.mStencilClearValue = 0u;

//            mRenderPassDesc_UFOMobileFinal = new CRenderPassDesc();
//            TempClearColor.r = 0.0f;
//            TempClearColor.g = 0.0f;
//            TempClearColor.b = 0.0f;
//            TempClearColor.a = 0.0f;
//            mRenderPassDesc_UFOMobileFinal.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_UFOMobileFinal.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_UFOMobileFinal.mFBClearColorRT0 = TempClearColor;
//            mRenderPassDesc_UFOMobileFinal.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_UFOMobileFinal.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_UFOMobileFinal.mDepthClearValue = 1.0f;
//            mRenderPassDesc_UFOMobileFinal.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_UFOMobileFinal.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_UFOMobileFinal.mStencilClearValue = 0u;

//            mOpaqueSE.mEnvMap = CEngine.Instance.TextureManager.GetShaderRView(RHICtx, RName.GetRName("Texture/envmap0.txpic"), true);
//            mOpaqueSE.mEyeEnvMap = CEngine.Instance.TextureManager.GetShaderRView(RHICtx, RName.GetRName("Texture/eyeenvmap0.txpic"), true);

//            mTranslucentSE.mEnvMap = CEngine.Instance.TextureManager.GetShaderRView(RHICtx, RName.GetRName("Texture/envmap0.txpic"), true);


//            //post effect;
//            await mBloomMobile.Init(RHICtx, width, height, mBaseSceneView);
            
//            mCopySE.mBaseSceneView = mBaseSceneView.mFrameBuffer.GetSRV_RenderTarget(0);
//            mCopySE.mBloomTex = mBloomMobile.mUSView8.FrameBuffer.GetSRV_RenderTarget(0);

//            {
//                var DirLightColor = new Vector3(1.0f, 1.0f, 1.0f);
//                var DirLightDirection = new Vector3(0.0f, -1.5f, 1.0f);
//                DirLightDirection.Normalize();
//                float DirLightIntensity = 2.5f;

//                var SkyLightColor = new Vector3(0.1f, 0.1f, 0.2f);
//                float SkyLightIntensity = 1.0f;
                
//                mBaseSceneView. = DirLightDirection;
                    
//                mBaseSceneView.mSkyLightColor = SkyLightColor;
//                mBaseSceneView.mSkyLightIntensity = SkyLightIntensity;

//                CTex2DDesc Tex2DDesc = mOpaqueSE.mEnvMap.GetTex2DDesc();
//                mBaseSceneView.mEnvMapMipMaxLevel = Tex2DDesc.mMipLevels - 1;
                
//                Tex2DDesc = mOpaqueSE.mEyeEnvMap.GetTex2DDesc();
//                mBaseSceneView.mEyeEnvMapMaxMipLevel = Tex2DDesc.mMipLevels - 1;

//                var ViewportSizeAndRcp = new Vector4(width, height, 1.0f / width, 1.0f / height);
//                mUFOSceneView.ViewportSizeAndRcp = ViewportSizeAndRcp;


//            }

//            return true;
//        }
//        public override void TickLogic(CGfxSceneView view, CRenderContext RHICtx)
//        {
//            if (mBaseSceneView == null)
//            {
//                return;
//            }
//            int drawCall = 0;
//            int drawTriangle = 0;
//            int CurrDPNumber = 0;

//            //opaque render pass;
//            mBaseSceneView.CookSpecRenderLayerDataToPass(RHICtx, ERenderLayer.RL_Opaque, mCamera, mOpaqueSE, PrebuildPassIndex.PPI_OpaquePbr);
//            //translucent render pass;
//            mBaseSceneView.CookSpecRenderLayerDataToPass(RHICtx, ERenderLayer.RL_Translucent, mCamera, mTranslucentSE, PrebuildPassIndex.PPI_TransparentPbr);
//            //custom translucent render pass;
//            mBaseSceneView.CookSpecRenderLayerDataToPass(RHICtx, ERenderLayer.RL_CustomTranslucent, mCamera, mTranslucentSE, PrebuildPassIndex.PPI_TransparentPbr);


//            var CmdList = mCLDB_Forward[0];
//            CmdList.BeginCommand();
//            CmdList.BeginRenderPass(mRenderPassDesc_UFOMobileForward, mBaseSceneView.mFrameBuffer);
//            mBaseSceneView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Opaque);
//            mBaseSceneView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Translucent);
//            mBaseSceneView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_CustomTranslucent);
//            CmdList.EndRenderPass();
//            LatestPass = CmdList.EndCommand(GraphicsDebug, ref CurrDPNumber, DPLimitter);
//            drawCall += CmdList.DrawCall;
//            drawTriangle += CmdList.DrawTriangle;

//            //post effect;
//            mBloomMobile.TickLogic(RHICtx, mCamera);
            
//            //copy2screen render pass;
//            CmdList = mCLDB_Copy[0];
//            CmdList.BeginCommand();
//            CmdList.BeginRenderPass(mRenderPassDesc_UFOMobileFinal, mUFOSceneView.FrameBuffer);
//            if (mScreenRect != null)
//            {
//                mUFOSceneView.CookViewportMeshToPass(RHICtx, mCopySE, mCamera, mScreenRect);
//                mUFOSceneView.PushPassToRHI(CmdList);

//                if (OnDrawUI != null)
//                {
//                    OnDrawUI(CmdList, mUFOSceneView);
//                }
//            }
//            CmdList.EndRenderPass();
//            CmdList.EndCommand(false, ref CurrDPNumber);
//            drawCall += CmdList.DrawCall;
//            drawTriangle += CmdList.DrawTriangle;

//            DrawCall = drawCall;
//            DrawTriangle = drawTriangle;
//        }
//        public override void TickRender(CSwapChain swapChain)
//        {
//            var RHICtx = EngineNS.CEngine.Instance.RenderContext;
//            if (RHICtx == null)
//                return;

//            if (mSwapChain != null)
//            {
//                //RHICtx.BindCurrentSwapChain(mSwapChain);

//                var CmdList = mCLDB_Forward[1];
//                CmdList.Commit(RHICtx);
                
//                //post effect;
//                mBloomMobile.TickRender(RHICtx);

//                CmdList = mCLDB_Copy[1];
//                CmdList.Commit(RHICtx, mSwapChain);
                
//            }
         
//            base.TickRender(mSwapChain);
//        }
//        public override void TickSync()
//        {
//            var Temp = mCLDB_Forward[0];
//            mCLDB_Forward[0] = mCLDB_Forward[1];
//            mCLDB_Forward[1] = Temp;
            
//            //post effect;
//            mBloomMobile.TickSync();
            
//            Temp = mCLDB_Copy[0];
//            mCLDB_Copy[0] = mCLDB_Copy[1];
//            mCLDB_Copy[1] = Temp;
//            base.TickSync();

//            mCamera.ClearAllRenderLayerData();
//        }
//        public override void OnResize(CRenderContext RHICtx, CSwapChain SwapChain, UInt32 width, UInt32 height)
//        {
//            mSwapChain.OnResize(width, height);

//            //RHICtx.BindCurrentSwapChain(mSwapChain);

//            mCamera.PerspectiveFovLH(mCamera.mDefaultFoV, (float)width, (float)height);

//            mUFOSceneView.OnResize(RHICtx, mSwapChain, width, height);
//            mBaseSceneView.OnResize(RHICtx, null, width, height);

//            //post effect;
//            mBloomMobile.OnResize(RHICtx, width, height, mBaseSceneView);

//            mCopySE.mBaseSceneView = mBaseSceneView.mFrameBuffer.GetSRV_RenderTarget(0);
//            mCopySE.mBloomTex = mBloomMobile.mUSView8.FrameBuffer.GetSRV_RenderTarget(0);

//            var ViewportSizeAndRcp = new Vector4(width, height, 1.0f / width, 1.0f / height);
//            mUFOSceneView.ViewportSizeAndRcp = ViewportSizeAndRcp;
//        }


//    }
//}
