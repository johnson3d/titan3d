//using EngineNS.Graphics.EnvShader;
//using EngineNS.Graphics.PostEffect;
//using System;
//using System.Collections.Generic;
//using System.Text;

//using EngineNS.Graphics.View;

//namespace EngineNS.Graphics.RenderPolicy
//{
//    public class CGfxRP_UFOMobileNPR : CGfxRenderPolicy
//    {
//        public CCommandList[] mOutlineCmdListDB;
//        private CCommandList[] mCLDB_Forward;
//        public EngineNS.CCommandList[] mCopyCmdListDB;
        
//        public CGfxScreenView mUFOSceneView;

//        private CRenderPassDesc mRenderPassDesc_OutlineNpr;
//        private CRenderPassDesc mRenderPassDesc_Forward;
//        private CRenderPassDesc mRenderPassDesc_Final;

//        private EngineNS.Graphics.Mesh.CGfxMesh mScreenRect;
        

//        public CGfxUFOMobileOutlineNprSE mOutlineNprSE;
//        public CGfxUFOMobileOpaqueNprSE mOpaqueNprSE;
//        public CGfxUFOMobileTranslucentNprSE mTranslucentNprSE;
//        public CGfxUFOMobileCopySE mCopySE;


//        //post effect;
//        private CGfxMobileBloom mBloomMobile;

//        public CGfxRP_UFOMobileNPR()
//        {
//            var RHICtx = EngineNS.CEngine.Instance.RenderContext;

//            mOutlineCmdListDB = new CCommandList[2];
//            mCLDB_Forward = new CCommandList[2];
            
//            mCopyCmdListDB = new CCommandList[2];
            
//            EngineNS.CCommandListDesc clDesc = new EngineNS.CCommandListDesc();
//            mOutlineCmdListDB[0] = RHICtx.CreateCommandList(clDesc);
//            mOutlineCmdListDB[1] = RHICtx.CreateCommandList(clDesc);

//            mCLDB_Forward[0] = RHICtx.CreateCommandList(clDesc);
//            mCLDB_Forward[1] = RHICtx.CreateCommandList(clDesc);
            
//            mCopyCmdListDB[0] = RHICtx.CreateCommandList(clDesc);
//            mCopyCmdListDB[1] = RHICtx.CreateCommandList(clDesc);
            
//            //post effect;
//            mBloomMobile = new CGfxMobileBloom(RHICtx);

//        }

//        public override void Cleanup()
//        {
//            mOutlineCmdListDB[0].Cleanup();
//            mOutlineCmdListDB[0] = null;
//            mOutlineCmdListDB[1].Cleanup();
//            mOutlineCmdListDB[1] = null;

//            mCLDB_Forward[0].Cleanup();
//            mCLDB_Forward[0] = null;
//            mCLDB_Forward[1].Cleanup();
//            mCLDB_Forward[1] = null;
            
//            mCopyCmdListDB[0].Cleanup();
//            mCopyCmdListDB[0] = null;
//            mCopyCmdListDB[1].Cleanup();
//            mCopyCmdListDB[1] = null;

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
//            if (mOutlineNprSE == null)
//            {
//                mOutlineNprSE = CEngine.Instance.ShadingEnvManager.NewGfxShadingEnv<CGfxUFOMobileOutlineNprSE>(RName.GetRName("Shaders/UFOMobileOutlineNPR.shadingenv"));
//            }

//            if (mOpaqueNprSE == null)
//            {
//                mOpaqueNprSE = CEngine.Instance.ShadingEnvManager.NewGfxShadingEnv<CGfxUFOMobileOpaqueNprSE>(RName.GetRName("Shaders/UFOMobileOpaqueNPR.shadingenv"));
//            }

//            if (mTranslucentNprSE == null)
//            {
//                mTranslucentNprSE = CEngine.Instance.ShadingEnvManager.NewGfxShadingEnv<CGfxUFOMobileTranslucentNprSE>(RName.GetRName("Shaders/UFOMobileTranslucentNPR.shadingenv"));
//            }


//            if (mCopySE == null)
//            {
//                mCopySE = CEngine.Instance.ShadingEnvManager.NewGfxShadingEnv<CGfxUFOMobileCopySE>(RName.GetRName("Shaders/UFOMobileCopy.shadingenv"));
//            }

//            var RectMesh = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(RHICtx, CEngineDesc.FullScreenRectName, true);

//            mScreenRect = CEngine.Instance.MeshManager.CreateMesh(RHICtx, RectMesh);
//            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(RHICtx, RName.GetRName("Material/defaultmaterial.instmtl"));
//            mScreenRect.SetMaterialInstance(RHICtx, 0,
//                mtl,
//                CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);
//            await mScreenRect.AwaitEffects();

//            EngineNS.CSwapChainDesc SwapChainDesc;
//            SwapChainDesc.Format = EngineNS.EPixelFormat.PXF_R8G8B8A8_UNORM;
//            SwapChainDesc.Width = width;
//            SwapChainDesc.Height = height;
//            SwapChainDesc.WindowHandle = WinHandle;
//            mSwapChain = RHICtx.CreateSwapChain(SwapChainDesc);
//            //RHICtx.BindCurrentSwapChain(mSwapChain);

//            //base scene view;
//            CGfxSceneViewInfo BaseViewInfo = new CGfxSceneViewInfo();
//            BaseViewInfo.mUseDSV = true;
//            BaseViewInfo.Width = width;
//            BaseViewInfo.Height = height;
//            BaseViewInfo.mDSVDesc.Init();
//            BaseViewInfo.mDSVDesc.Format = EngineNS.EPixelFormat.PXF_D24_UNORM_S8_UINT;
//            BaseViewInfo.mDSVDesc.Width = width;
//            BaseViewInfo.mDSVDesc.Height = height;

//            CRenderTargetViewDesc rtDesc0 = new CRenderTargetViewDesc();
//            rtDesc0.Init();
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
//            UFOViewInfo.mDSVDesc.Init();
//            UFOViewInfo.mDSVDesc.Format = EngineNS.EPixelFormat.PXF_D24_UNORM_S8_UINT;
//            UFOViewInfo.mDSVDesc.Width = width;
//            UFOViewInfo.mDSVDesc.Height = height;

//            var RTVDesc1 = new EngineNS.CRenderTargetViewDesc();
//            RTVDesc1.mCanBeSampled = vBOOL.FromBooleam(false);
//            UFOViewInfo.mRTVDescArray.Add(RTVDesc1);
//            mUFOSceneView = new CGfxScreenView();
//            await mUFOSceneView.Init(RHICtx, mSwapChain, UFOViewInfo, mCopySE, mtl, mScreenRect);

//            mCamera = camera;
//            //mCamera.SetSceneView(RHICtx, mBaseSceneView);

//            mRenderPassDesc_OutlineNpr = new CRenderPassDesc();
//            FrameBufferClearColor TempClearColor = new FrameBufferClearColor();
//            TempClearColor.r = 0.0f;
//            TempClearColor.g = 0.0f;
//            TempClearColor.b = 0.0f;
//            TempClearColor.a = 0.0f;
//            mRenderPassDesc_OutlineNpr.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_OutlineNpr.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_OutlineNpr.mFBClearColorRT0 = TempClearColor;
//            mRenderPassDesc_OutlineNpr.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_OutlineNpr.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_OutlineNpr.mDepthClearValue = 1.0f;
//            mRenderPassDesc_OutlineNpr.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_OutlineNpr.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_OutlineNpr.mStencilClearValue = 0u;

//            mRenderPassDesc_Forward = new CRenderPassDesc();
//            mRenderPassDesc_Forward.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionLoad;
//            mRenderPassDesc_Forward.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_Forward.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionLoad;
//            mRenderPassDesc_Forward.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_Forward.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionLoad;
//            mRenderPassDesc_Forward.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            
//            mRenderPassDesc_Final = new CRenderPassDesc();
//            TempClearColor.r = 0.0f;
//            TempClearColor.g = 0.0f;
//            TempClearColor.b = 0.0f;
//            TempClearColor.a = 0.0f;
//            mRenderPassDesc_Final.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_Final.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_Final.mFBClearColorRT0 = TempClearColor;
//            mRenderPassDesc_Final.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_Final.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_Final.mDepthClearValue = 1.0f;
//            mRenderPassDesc_Final.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_Final.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_Final.mStencilClearValue = 0u;

//            //mOpaqueNprSE.mEnvMap = CEngine.Instance.TextureManager.GetShaderRView(RHICtx, RName.GetRName("Texture/envmap0.txpic"), true);

//            //mTranslucentSE.mEnvMap = CEngine.Instance.TextureManager.GetShaderRView(RHICtx, RName.GetRName("Texture/envmap0.txpic"), true);

//            //post effect;
//            await mBloomMobile.Init(RHICtx, width, height, mBaseSceneView);


//            mCopySE.mBaseSceneView = mBaseSceneView.mFrameBuffer.GetSRV_RenderTarget(0);
//            mCopySE.mBloomTex = mBloomMobile.mUSView8.FrameBuffer.GetSRV_RenderTarget(0);


//            {
//                var DirLightColor = new Vector3(1.0f, 1.0f, 0.8f);
//                var DirLightDirection = new Vector3(0.0f, -0.6f, 1.0f);
//                DirLightDirection.Normalize();
//                float DirLightIntensity = 1.0f;

//                var SkyLightColor = new Vector3(0.1f, 0.1f, 0.15f);
//                float SkyLightIntensity = 6.0f;
                
//                mBaseSceneView. = DirLightDirection;
                    
//                mBaseSceneView.mSkyLightColor = SkyLightColor;

//                //CTex2DDesc Tex2DDesc = mOpaqueNprSE.mEnvMap.GetTex2DDesc();
//                //mBaseSceneView.mEnvMapMipMaxLevel = Tex2DDesc.mMipLevels - 1;
                
//                var ViewportSizeAndRcp = new Vector4(width, height, 1.0f / width, 1.0f / height);
//                mBaseSceneView.mViewportSizeAndRcp = ViewportSizeAndRcp;
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
//            CurrDPNumber = 0;

//            ScopeTickLogic.Begin();

//            //outline render pass;
//            mBaseSceneView.CookSpecRenderLayerDataToPass(RHICtx, ERenderLayer.RL_Opaque, mCamera, mOutlineNprSE, PrebuildPassIndex.PPI_OutLineNpr);
//            var CmdList = mOutlineCmdListDB[0];
//            CmdList.BeginCommand();
//            CmdList.BeginRenderPass(mRenderPassDesc_OutlineNpr, mBaseSceneView.mFrameBuffer);
//            mBaseSceneView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Opaque);
//            LatestPass = CmdList.BuildRenderPass(ref DPLimitter, GraphicsDebug);
//            CmdList.EndRenderPass();
//            CmdList.EndCommand();
//            drawCall += CmdList.DrawCall;
//            drawTriangle += CmdList.DrawTriangle;

//            //opaque render pass;
//            mBaseSceneView.CookSpecRenderLayerDataToPass(RHICtx, ERenderLayer.RL_Opaque, mCamera, mOpaqueNprSE, PrebuildPassIndex.PPI_OpaqueNpr);
//            //translucent render pass;
//            mBaseSceneView.CookSpecRenderLayerDataToPass(RHICtx, ERenderLayer.RL_Translucent, mCamera, mTranslucentNprSE, PrebuildPassIndex.PPI_TransparentNpr);
//            //custom translucent render pass;
//            mBaseSceneView.CookSpecRenderLayerDataToPass(RHICtx, ERenderLayer.RL_CustomTranslucent, mCamera, mTranslucentNprSE, PrebuildPassIndex.PPI_TransparentNpr);
            
//            CmdList = mCLDB_Forward[0];
//            CmdList.BeginCommand();
//            CmdList.BeginRenderPass(mRenderPassDesc_Forward, mBaseSceneView.mFrameBuffer);
//            mBaseSceneView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Opaque);
//            mBaseSceneView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Translucent);
//            mBaseSceneView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_CustomTranslucent);
//            LatestPass = CmdList.BuildRenderPass(ref DPLimitter, GraphicsDebug);
//            CmdList.EndRenderPass();
//            CmdList.EndCommand();
//            drawCall += CmdList.DrawCall;
//            drawTriangle += CmdList.DrawTriangle;

//            //post effect;
//            mBloomMobile.TickLogic(RHICtx, mCamera);

//            //copy2screen render pass;
//            CmdList = mCopyCmdListDB[0];
//            CmdList.BeginCommand();
//            CmdList.BeginRenderPass(mRenderPassDesc_Final, mUFOSceneView.FrameBuffer);
//            if (mScreenRect != null)
//            {
//                mUFOSceneView.CookViewportMeshToPass(RHICtx, mCopySE, mCamera, mScreenRect);
//                mUFOSceneView.PushPassToRHI(CmdList);

//                if(OnDrawUI!=null)
//                {
//                    OnDrawUI(CmdList, mUFOSceneView);
//                }
//            }
//            CmdList.BuildRenderPass(ref DPLimitter);
//            CmdList.EndRenderPass();
//            CmdList.EndCommand();
//            drawCall += CmdList.DrawCall;
//            drawTriangle += CmdList.DrawTriangle;
            
//            ScopeTickLogic.End();

//            DrawCall = drawCall;
//            DrawTriangle = drawTriangle;
//        }
//        public override void TickRender(CSwapChain swapChain)
//        {
//            var RHICtx = EngineNS.CEngine.Instance.RenderContext;
//            if (RHICtx == null)
//            {
//                return;
//            }
            
//            if (mSwapChain != null)
//            {
//                //RHICtx.BindCurrentSwapChain(mSwapChain);

//                var CmdList = mOutlineCmdListDB[1];
//                CmdList.Commit(RHICtx);
                
//                CmdList = mCLDB_Forward[1];
//                CmdList.Commit(RHICtx);
                
//                mBloomMobile.TickRender(RHICtx);

//                CmdList = mCopyCmdListDB[1];
//                CmdList.Commit(RHICtx, mSwapChain);

//                //RHICtx.Present(0, 0);
//            }
            
//            base.TickRender(mSwapChain);
//        }
//        public override void TickSync()
//        {
//            var Temp = mOutlineCmdListDB[0];
//            mOutlineCmdListDB[0] = mOutlineCmdListDB[1];
//            mOutlineCmdListDB[1] = Temp;

//            Temp = mCLDB_Forward[0];
//            mCLDB_Forward[0] = mCLDB_Forward[1];
//            mCLDB_Forward[1] = Temp;
            
//            //post effect;
//            mBloomMobile.TickSync();

//            Temp = mCopyCmdListDB[0];
//            mCopyCmdListDB[0] = mCopyCmdListDB[1];
//            mCopyCmdListDB[1] = Temp;
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
            
            

//            var ViewportSizeAndRcp = new Vector4(width, height, 1.0f / width, 1.0f / height);
//            mBaseSceneView.mViewportSizeAndRcp = ViewportSizeAndRcp;
//            mUFOSceneView.ViewportSizeAndRcp = ViewportSizeAndRcp;


//            //post effect;
//            mBloomMobile.OnResize(RHICtx, width, height, mBaseSceneView);


//            mCopySE.mBaseSceneView = mBaseSceneView.mFrameBuffer.GetSRV_RenderTarget(0);
//            mCopySE.mBloomTex = mBloomMobile.mUSView8.FrameBuffer.GetSRV_RenderTarget(0);
//        }


//    }
//}
