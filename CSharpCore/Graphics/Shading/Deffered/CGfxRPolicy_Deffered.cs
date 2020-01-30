//using System;
//using System.Collections.Generic;
//using System.Text;
//using EngineNS.Graphics.View;

//namespace EngineNS.Graphics.Shading.Deffered
//{
//    public class CGfxRPolicy_Deffered : CGfxRenderPolicy
//    {
//        public EngineNS.CCommandList[] mCmdListDB_Opaque; //command list double buffer for opaque render layer;
//        public EngineNS.CCommandList[] mCmdListDB_LightShading;

//        public CGfxSceneView mOpaqueSceneView;
        
//        private EngineNS.Graphics.Mesh.CGfxMesh mScreenRect;

//        //private CMRTClearColor[] mClrColorArray = new CMRTClearColor[]
//        //{
//        //        new CMRTClearColor(0, 0x00000000),
//        //        new CMRTClearColor(1, 0xFF000000)
//        //};

//        private CRenderPassDesc mRenderPassDesc_GBuffer;
//        private CRenderPassDesc mRenderPassDesc_Shading;

//        public CGfxRPolicy_Deffered()
//        {
//            var rc = EngineNS.CEngine.Instance.RenderContext;

//            mCmdListDB_Opaque = new CCommandList[2];
//            mCmdListDB_LightShading = new CCommandList[2];
            
//            EngineNS.CCommandListDesc clDesc = new EngineNS.CCommandListDesc();
//            mCmdListDB_Opaque[0] = rc.CreateCommandList(clDesc);
//            mCmdListDB_Opaque[1] = rc.CreateCommandList(clDesc);

//            mCmdListDB_LightShading[0] = rc.CreateCommandList(clDesc);
//            mCmdListDB_LightShading[1] = rc.CreateCommandList(clDesc);

//        }
//        public override void Cleanup()
//        {
//            mCmdListDB_Opaque[0].Cleanup();
//            mCmdListDB_Opaque[0] = null;
//            mCmdListDB_Opaque[1].Cleanup();
//            mCmdListDB_Opaque[1] = null;

//            mCmdListDB_LightShading[0].Cleanup();
//            mCmdListDB_LightShading[0] = null;
//            mCmdListDB_LightShading[1].Cleanup();
//            mCmdListDB_LightShading[1] = null;

//            mOpaqueSceneView.Cleanup();
//            mOpaqueSceneView = null;

//            mBaseSceneView.Cleanup();
//            mBaseSceneView = null;
       
//            mSwapChain.Cleanup();
//            mSwapChain = null;

//            base.Cleanup();
//        }

//        public CGfxShadingEnv mGBufferSE;
//        public CGfxShadingEnv mDirLightSE;
        
        
        
//        public override async System.Threading.Tasks.Task<bool> Init(CRenderContext RHICtx, UInt32 width, UInt32 height, CGfxCamera camera, IntPtr WinHandle)
//        {
//            EngineNS.CSwapChainDesc SwapChainDesc;
//            SwapChainDesc.Format = EngineNS.EPixelFormat.PXF_R8G8B8A8_UNORM;
//            SwapChainDesc.Width = width;
//            SwapChainDesc.Height = height;
//            SwapChainDesc.WindowHandle = WinHandle;
//            mSwapChain = RHICtx.CreateSwapChain(SwapChainDesc);
//            //RHICtx.BindCurrentSwapChain(mSwapChain);

//            //game scene view;
//            CGfxSceneViewInfo GameViewInfo = new CGfxSceneViewInfo();
//            GameViewInfo.mDisuseDSV = true;
//            GameViewInfo.Width = width;
//            GameViewInfo.Height = height;
//            GameViewInfo.DepthStencil.Format = EngineNS.EPixelFormat.PXF_D24_UNORM_S8_UINT;
//            GameViewInfo.DepthStencil.Width = width;
//            GameViewInfo.DepthStencil.Height = height;

//            var rtDesc0 = new EngineNS.CRenderTargetViewDesc();
//            rtDesc0.mCanBeSampled = 0;
//            GameViewInfo.mRTVDescArray.Add(rtDesc0);
//            mBaseSceneView = new CGfxSceneView();
//            mBaseSceneView.Init(RHICtx, mSwapChain, GameViewInfo);

//            //opaque scene view;
//            CGfxSceneViewInfo OpaqueViewInfo = new CGfxSceneViewInfo();
//            OpaqueViewInfo.mDisuseDSV = false;
//            OpaqueViewInfo.Width = width;
//            OpaqueViewInfo.Height = height;
//            OpaqueViewInfo.DepthStencil.Format = EngineNS.EPixelFormat.PXF_D24_UNORM_S8_UINT;
//            OpaqueViewInfo.DepthStencil.Width = width;
//            OpaqueViewInfo.DepthStencil.Height = height;

//            CRenderTargetViewDesc rtDesc1 = new CRenderTargetViewDesc();
//            rtDesc1.SetDefault();
//            rtDesc1.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
//            rtDesc1.Width = width;
//            rtDesc1.Height = height;
//            OpaqueViewInfo.mRTVDescArray.Add(rtDesc1);

//            CRenderTargetViewDesc rtDesc2 = new CRenderTargetViewDesc();
//            rtDesc2.SetDefault();
//            rtDesc2.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
//            rtDesc2.Width = width;
//            rtDesc2.Height = height;
//            OpaqueViewInfo.mRTVDescArray.Add(rtDesc2);

//            mOpaqueSceneView = new CGfxSceneView();
//            if(false== mOpaqueSceneView.Init(RHICtx, null, OpaqueViewInfo))
//            {
//                return false;
//            }
            
//            mCamera = camera;
//            //mCamera.SetSceneView(RHICtx, mGameSceneView);

//            mRenderPassDesc_GBuffer = new CRenderPassDesc();
//            FrameBufferClearColor ClearColorRT0 = new FrameBufferClearColor();
//            ClearColorRT0.r = 0.0f;
//            ClearColorRT0.g = 0.0f;
//            ClearColorRT0.b = 0.0f;
//            ClearColorRT0.a = 0.0f;
//            FrameBufferClearColor ClearColorRT1 = new FrameBufferClearColor();
//            ClearColorRT1.r = 0.0f;
//            ClearColorRT1.g = 0.0f;
//            ClearColorRT1.b = 0.0f;
//            ClearColorRT1.a = 1.0f;

//            mRenderPassDesc_GBuffer.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_GBuffer.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_GBuffer.mFBClearColorRT0 = ClearColorRT0;
//            mRenderPassDesc_GBuffer.mFBClearColorRT1 = ClearColorRT1;
//            mRenderPassDesc_GBuffer.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_GBuffer.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_GBuffer.mDepthClearValue = 1.0f;
//            mRenderPassDesc_GBuffer.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_GBuffer.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_GBuffer.mStencilClearValue = 0u;

//            mRenderPassDesc_Shading = new CRenderPassDesc();
//            FrameBufferClearColor ClearColorShading = new FrameBufferClearColor();
//            ClearColorShading.r = 0.0f;
//            ClearColorShading.g = 0.0f;
//            ClearColorShading.b = 0.0f;
//            ClearColorShading.a = 0.0f;
//            mRenderPassDesc_Shading.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_Shading.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_Shading.mFBClearColorRT0 = ClearColorRT0;
//            mRenderPassDesc_Shading.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_Shading.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_Shading.mDepthClearValue = 1.0f;
//            mRenderPassDesc_Shading.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
//            mRenderPassDesc_Shading.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
//            mRenderPassDesc_Shading.mStencilClearValue = 0u;

//            //mGBufferSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv(RName.GetRName("ShadingEnv/DSBase2RT.senv"));
//            if (mGBufferSE == null)
//            {
//                mGBufferSE = CEngine.Instance.ShadingEnvManager.NewGfxShadingEnv<CGfxShadingEnv>(
//                        RName.GetRName("ShadingEnv/DSBase2RT.senv"), RName.GetRName("Shaders/DSBase2RT.shadingenv"));
//            }

//            //mDirLightSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv(RName.GetRName("ShadingEnv/dsdirlighting.senv"));
//            if (mDirLightSE == null)
//            {
//                mDirLightSE = CEngine.Instance.ShadingEnvManager.NewGfxShadingEnv<CGfxShadingEnv_DirLighting>(
//                        RName.GetRName("ShadingEnv/dsdirlighting.senv"), RName.GetRName("Shaders/DSDirLighting.shadingenv"));
//            }
            
//            var rectMesh = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(RHICtx, CEngineDesc.FullScreenRectName, true);

//            mScreenRect = CEngine.Instance.MeshManager.CreateMesh(RHICtx, rectMesh/*, this*/);
//            await mScreenRect.SetMaterial(RHICtx, 0, await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(RHICtx, RName.GetRName("Material/defaultmaterial.instmtl")), CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);
//            CGfxShadingEnv_DirLighting DirLightSE = (CGfxShadingEnv_DirLighting)mDirLightSE;
//            DirLightSE.mGBuffer0 = mOpaqueSceneView.mFrameBuffer.GetSRV_RenderTarget(0);
//            DirLightSE.mGBuffer1 = mOpaqueSceneView.mFrameBuffer.GetSRV_RenderTarget(1);
//            DirLightSE.mDepthStencil = mOpaqueSceneView.mFrameBuffer.GetTextureDS();

//            {
//                var DirLightDirection = new Vector3(1, -1, 1);
//                float DirLightIntensity = 1;
//                float DirLightSpecularIntensity = 1;
//                var DirLightingAmbient = new Color4(0, 0.3f, 0.6f, 0);
//                var DirLightingDiffuse = new Color4(1, 1, 1, 1);
//                var DirLightingSpecular = new Color4(1, 1, 1, 1);
//                float DirLightShadingSSS = 1;
//                DirLightDirection.Normalize();
//                {
//                    mBaseSceneView. = DirLightDirection;

//                    mBaseSceneView.DirLightSpecularIntensity = DirLightSpecularIntensity;
//                    mBaseSceneView.DirLightingAmbient = DirLightingAmbient;
//                    mBaseSceneView.DirLightingDiffuse = DirLightingDiffuse;
//                    mBaseSceneView.DirLightingSpecular = DirLightingSpecular;
//                    mBaseSceneView.DirLightShadingSSS = DirLightShadingSSS;
//               }
//            }

//            return true;
//        }

//        private void BindDataToShader()
//        {
//            var shaderProgram = mScreenRect.MtlMeshArray[0].GetPass(PrebuildPassIndex.PPI_OpaquePbr).Effect.ShaderProgram;
//            CTextureBindInfo info = new CTextureBindInfo();
//            if (shaderProgram.FindTextureBindInfo(null, "txAlbedoTexture", ref info))
//            {
//                mScreenRect.MtlMeshArray[0].SetTexutre(info.PSBindPoint, mOpaqueSceneView.mFrameBuffer.GetSRV_RenderTarget(0));
//            }

//            if (shaderProgram.FindTextureBindInfo(null, "txNormBloomSpecTexture", ref info))
//            {
//                mScreenRect.MtlMeshArray[0].SetTexutre(info.PSBindPoint, mOpaqueSceneView.mFrameBuffer.GetSRV_RenderTarget(1));
//            }
            
//            if (shaderProgram.FindTextureBindInfo(null, "txDepthStencilTexture", ref info))
//            {
//                mScreenRect.MtlMeshArray[0].SetTexutre(info.PSBindPoint, mOpaqueSceneView.mFrameBuffer.GetTextureDS());
//            }
//        }

//        //public delegate void FAfterDrawDSDirlighting(CCommandList CmdList, CGfxRPolicy_Deffered policy, CGfxSceneView view);
//        //public FAfterDrawDSDirlighting AfterDrawDSDirlighting;
//        public override void TickLogic(CGfxSceneView view, CRenderContext RHICtx)
//        {
//            if (mBaseSceneView == null)
//            {
//                return;
//            }
//            CurrDPNumber = 0;

//            //gbuffer render pass;
//            mBaseSceneView.CookSpecRenderLayerDataToPass(RHICtx, ERenderLayer.RL_Opaque, mCamera, mGBufferSE, PrebuildPassIndex.PPI_OpaquePbr);
            
//            var CmdList = mCmdListDB_Opaque[0];
//            CmdList.BeginCommand();
//            CmdList.BeginRenderPass(mRenderPassDesc_GBuffer, mOpaqueSceneView.mFrameBuffer);
//            mBaseSceneView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Opaque);
//            CmdList.EndRenderPass();
//            LatestPass = CmdList.EndCommand(GraphicsDebug, ref CurrDPNumber, DPLimitter);

//            //deffered dir lighting render pass;
//            CmdList = mCmdListDB_LightShading[0];
//            CmdList.BeginCommand();
//            CmdList.BeginRenderPass(mRenderPassDesc_Shading, mBaseSceneView.mFrameBuffer);
            
//            if (mScreenRect != null)
//            {
//                var PassArray = mScreenRect.CookToPassArray(RHICtx, mCamera, mDirLightSE, mBaseSceneView);
//                mScreenRect.PushPassArrayToRHI(CmdList, PassArray);
//            }

//            LatestPass = CmdList.EndCommand(GraphicsDebug, ref CurrDPNumber, DPLimitter);
//        }
//        public override void TickRender(CSwapChain swapChain)
//        {
//            var rc = EngineNS.CEngine.Instance.RenderContext;
//            if (rc == null)
//                return;

//            if (mSwapChain != null)
//            {
//                //rc.BindCurrentSwapChain(mSwapChain);

//                var CmdList = mCmdListDB_Opaque[1];
//                CmdList.Commit(rc);

//                CmdList = mCmdListDB_LightShading[1];
//                CmdList.Commit(rc, mSwapChain);

//                //rc.Present(0, 0);
//            }
//            else
//            {
//                var CmdList = mCmdListDB_Opaque[1];
//                CmdList.Commit(rc);

//                CmdList = mCmdListDB_LightShading[1];
//                CmdList.Commit(rc);
//            }

//            base.TickRender(mSwapChain);
//        }
//        public override void TickSync()
//        {
//            var save = mCmdListDB_Opaque[0];
//            mCmdListDB_Opaque[0] = mCmdListDB_Opaque[1];
//            mCmdListDB_Opaque[1] = save;

//            save = mCmdListDB_LightShading[0];
//            mCmdListDB_LightShading[0] = mCmdListDB_LightShading[1];
//            mCmdListDB_LightShading[1] = save;
//            base.TickSync();
//        }
//        public override void OnResize(CRenderContext RHICtx, CSwapChain SwapChain, UInt32 width, UInt32 height)
//        {
//            mSwapChain.OnResize(width, height);

//            //RHICtx.BindCurrentSwapChain(mSwapChain);
            
//            mCamera.PerspectiveFovLH(mCamera.mDefaultFoV, (float)width, (float)height);

//            mBaseSceneView.OnResize(RHICtx, mSwapChain, width, height);
//            mOpaqueSceneView.OnResize(RHICtx, mSwapChain, width, height);
//            CGfxShadingEnv_DirLighting DirLightSE = (CGfxShadingEnv_DirLighting)mDirLightSE;
//            DirLightSE.mGBuffer0 = mOpaqueSceneView.mFrameBuffer.GetSRV_RenderTarget(0);
//            DirLightSE.mGBuffer1 = mOpaqueSceneView.mFrameBuffer.GetSRV_RenderTarget(1);
//            DirLightSE.mDepthStencil = mOpaqueSceneView.mFrameBuffer.GetTextureDS();
//        }

        
//    }
//}
