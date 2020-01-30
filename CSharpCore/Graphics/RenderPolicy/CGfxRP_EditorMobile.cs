//#define Test_Snapshot
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
    public partial class CGfxRP_EditorMobile : CGfxRenderPolicy
    {
        public CGfxFramePass mForwardBasePass;
        public CGfxPostprocessPass mCopyPostprocessPass;

        //env shader;
        public CGfxMobileOpaqueEditorSE mOpaqueSE;
        public CGfxMobileTranslucentSE mTranslucentSE;
        public CGfxMobileCustomTranslucentEditorSE mCustomTranslucentSE;
        public CGfxGizmosSE mGizmosSE;
        public CGfxMobileCopyEditorSE mCopyEditorSE;
        public CGfxSE_MobileSky mSE_MobileSky;

        //hitproxy;
        public CGfxHitProxy mHitProxy;
        public CGfxPickedEffect mPickedEffect;

        public CGfxCSMMobileEditor mCSM;

        //post effect;
        private CGfxMobileBloom mBloomMobile;
        private CGfxSunShaftMobile mSunShaftMobile;
        private CGfxMobileAO mMobileAO;

        public CGfxRP_EditorMobile()
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;

            //shadow ssm;
            mCSM = new CGfxCSMMobileEditor();

            //hitproxy;
            mHitProxy = new CGfxHitProxy();

            //picked effect for editor to use;
            mPickedEffect = new CGfxPickedEffect();

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
            mCSM.Cleanup();

            //hitproxy
            mHitProxy.Cleanup();

            //picked effect;
            mPickedEffect.Cleanup();

            //post effect
            mBloomMobile.Cleanup();
            mSunShaftMobile.Cleanup();
            mMobileAO.Cleanup();

            base.Cleanup();
        }

        public void SetClearColor(ref Color4 color)
        {
            mForwardBasePass.mRenderPassDesc.mFBClearColorRT0.r = color.Red;
            mForwardBasePass.mRenderPassDesc.mFBClearColorRT0.g = color.Green;
            mForwardBasePass.mRenderPassDesc.mFBClearColorRT0.b = color.Blue;
            mForwardBasePass.mRenderPassDesc.mFBClearColorRT0.a = color.Alpha;
        }
        
        public override async System.Threading.Tasks.Task<bool> Init(CRenderContext rc, UInt32 width, UInt32 height, CGfxCamera camera, IntPtr WinHandle)
        {
            if (mOpaqueSE == null)
            {
                mOpaqueSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileOpaqueEditorSE>();
            }

            if (mCustomTranslucentSE == null)
            {
                mCustomTranslucentSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileCustomTranslucentEditorSE>();
            }

            if (mTranslucentSE == null)
            {
                mTranslucentSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileTranslucentSE>();
            }
            
            if (mGizmosSE == null)
            {
                mGizmosSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxGizmosSE>();
            }
            
            mCopyEditorSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileCopyEditorSE>();
            
            if (mSE_MobileSky == null)
            {
                mSE_MobileSky = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxSE_MobileSky>();
            }

            if (mOpaqueSE == null || mCustomTranslucentSE == null || mTranslucentSE == null || mGizmosSE == null || mCopyEditorSE == null || mSE_MobileSky == null)
            {
                return false;
            }

            //shadow csm;
            bool IsReady = await mCSM.Init(rc);
            if (IsReady == false)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"SSM Error", "");
                return false;
            }

            //hitproxy;
            IsReady = mHitProxy.Init(width, height, camera);
            if (IsReady == false)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"HitProxyError", "");
                return false;
            }
            //mHitProxy.mEnabled = true;

            IsReady = await mPickedEffect.Init(width, height, camera);
            if(IsReady == false)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"PickedEffectError", "");
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
                if (false == BaseSceneView.Init(rc, null, BaseViewInfo))
                {
                    return false;
                }
                BaseSceneView.UIHost = await CEngine.Instance.UIManager.RegisterHost("Editor");
                //BaseSceneView.UIHost.IsInputActive = false;
                BaseSceneView.UIHost.WindowSize = new SizeF(width, height);

                mForwardBasePass = new CGfxFramePass(rc, "GameForwordBasePass");
                mForwardBasePass.mBaseSceneView = BaseSceneView;
                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_Opaque, PrebuildPassIndex.PPI_OpaquePbrEditor, mOpaqueSE);
                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_CustomOpaque, PrebuildPassIndex.PPI_OpaquePbrEditor, mOpaqueSE);
                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_Sky, PrebuildPassIndex.PPI_Sky, mSE_MobileSky);
                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_CustomTranslucent, PrebuildPassIndex.PPI_CustomTranslucentPbrEditor, mCustomTranslucentSE);
                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_Translucent, PrebuildPassIndex.PPI_TransparentPbr, mTranslucentSE);
                mForwardBasePass.SetRLayerParameter(ERenderLayer.RL_Gizmos, PrebuildPassIndex.PPI_Gizmos, mGizmosSE);

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
                SwapChain = rc.CreateSwapChain(SwapChainDesc);
                //RHICtx.BindCurrentSwapChain(mSwapChain);

                //final scene view;
                CGfxScreenViewDesc FinalViewInfo = new CGfxScreenViewDesc();
                FinalViewInfo.IsSwapChainBuffer = true;
                FinalViewInfo.UseDepthStencilView = false;
                FinalViewInfo.Width = width;
                FinalViewInfo.Height = height;

                var rtvDesc1 = new CRenderTargetViewDesc();
                rtvDesc1.mCanBeSampled = vBOOL.FromBoolean(false);
                rtvDesc1.Format = SwapChainDesc.Format;
                FinalViewInfo.mRTVDescArray.Add(rtvDesc1);

                mCopyPostprocessPass = new CGfxPostprocessPass();
                await mCopyPostprocessPass.Init(rc, SwapChain, FinalViewInfo, mCopyEditorSE, RName.GetRName("Material/defaultmaterial.instmtl"), "EditorCopyPost");

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
            Camera.SetSceneView(rc, BaseSceneView);

            //post effect;
            IsReady = await mBloomMobile.Init(rc, width, height, BaseSceneView);
            if (IsReady == false)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"BloomError", "");
                return false;
            }

            //sun shaft;
            IsReady = await mSunShaftMobile.Init(rc, width, height);
            if (IsReady == false)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }
            
            //mobile ao;
            IsReady = await mMobileAO.Init(rc, width, height, BaseSceneView);
            if (IsReady == false)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }
            
            //
            mOpaqueSE.mTex_ShadowMap = mCSM.mShadowMapView.FrameBuffer.GetSRV_DepthStencil();
            mOpaqueSE.EnvMapName = CEngine.Instance.Desc.DefaultEnvMap;
            mOpaqueSE.EyeEnvMapName = CEngine.Instance.Desc.DefaultEnvMap;

            mCustomTranslucentSE.EnvMapName = CEngine.Instance.Desc.DefaultEnvMap;
            mCustomTranslucentSE.mTex_ShadowMap = mCSM.mShadowMapView.FrameBuffer.GetSRV_DepthStencil();

            mTranslucentSE.EnvMapName = CEngine.Instance.Desc.DefaultEnvMap;

            mCopyEditorSE.mBaseSceneView = BaseSceneView.FrameBuffer.GetSRV_RenderTarget(0);
            mCopyEditorSE.mBloomTex = mBloomMobile.mUSView8.FrameBuffer.GetSRV_RenderTarget(0);
            mCopyEditorSE.mPickedTex = mPickedEffect.mSV_PickedBlurH.FrameBuffer.GetSRV_RenderTarget(0);
            mCopyEditorSE.VignetteTex = CEngine.Instance.Desc.DefaultVignette;
            mCopyEditorSE.mSunShaftTex = mSunShaftMobile.mView_Blur.FrameBuffer.GetSRV_RenderTarget(0);
            //mCopyEditorSE.mSRV_MobileAo = mMobileAO.mView_BlurV.FrameBuffer.GetSRV_RenderTarget(0);
            mCopyEditorSE.mSRV_MobileAo = mMobileAO.mView_AoMask.FrameBuffer.GetSRV_RenderTarget(0);

            {
                var DirLightColor = new Vector4(1.0f, 1.0f, 1.0f, 3.5f);
                var DirLightDirection = new Vector3(0.0f, -1.5f, 1.0f);
                DirLightDirection.Normalize();
                
                var SkyLightColor = new Vector3(0.12f, 0.12f, 0.25f);

                BaseSceneView.mDirLightDirection_Leak = new Vector4(DirLightDirection, 0.05f);
                BaseSceneView.DirLightColor_Intensity = DirLightColor;
                BaseSceneView.mSkyLightColor = SkyLightColor;

                var Tex2DDesc = mOpaqueSE.EnvMap.TxPicDesc;
                BaseSceneView.mEnvMapMipMaxLevel = Tex2DDesc.MipLevel - 1;
                
                Tex2DDesc = mOpaqueSE.mEyeEnvMap.TxPicDesc;
                BaseSceneView.mEyeEnvMapMaxMipLevel = Tex2DDesc.MipLevel - 1;
                
                var ViewportSizeAndRcp = new Vector4(width, height, 1.0f / width, 1.0f / height);
                BaseSceneView.mViewportSizeAndRcp = ViewportSizeAndRcp;
                mCopyPostprocessPass.mScreenView.ViewportSizeAndRcp = ViewportSizeAndRcp;
            }

            this.OnDrawUI += (CCommandList cmd, Graphics.View.CGfxScreenView view) =>
            {
                if (BaseSceneView.UIHost != null)
                    BaseSceneView.UIHost.Draw(CEngine.Instance.RenderContext, cmd, view);
            };

#if Test_Snapshot
            await TestSnapRender(rc);
#endif
            return true;
        }
#if Test_Snapshot
        Editor.GSnapshotCreator mTestSnapshort;
        private async System.Threading.Tasks.Task<bool> TestSnapRender(CRenderContext rc)
        {
            var mCurMesh = await CEngine.Instance.MeshManager.CreateMeshAsync(rc, RName.GetRName("editor/basemesh/box.gms"));
            if (mCurMesh == null)
            {
                return false;
            }
            mTestSnapshort = new Editor.GSnapshotCreator();
            mTestSnapshort.SkyName = EngineNS.RName.GetRName("Mesh/sky.gms");
            mTestSnapshort.FloorName = EngineNS.RName.GetRName(@"editor/floor.gms");
            var eye = new EngineNS.Vector3();
            eye.SetValue(1.6f, 1.5f, -3.6f);
            var at = new EngineNS.Vector3();
            at.SetValue(0.0f, 0.0f, 0.0f);
            var up = new EngineNS.Vector3();
            up.SetValue(0.0f, 1.0f, 0.0f);
            await mTestSnapshort.InitEnviroment();
            mTestSnapshort.Camera.LookAtLH(eye, at, up);

            var actor = EngineNS.GamePlay.Actor.GActor.NewMeshActorDirect(mCurMesh);

            mCurMesh.PreUse(true);//就这个地方用，别的地方别乱用，效率不好
            mTestSnapshort.World.AddActor(actor);
            mTestSnapshort.World.GetScene(RName.GetRName("SnapshorCreator")).AddActor(actor);
            mTestSnapshort.FocusActor = actor;
            actor.Placement.Location = new Vector3(0, 0, 0);
            OnFetchFinished = (InSrv) =>
            {
                var blob = new EngineNS.Support.CBlobObject();
                unsafe
                {
                    void* pData;
                    uint rowPitch;
                    uint depthPitch;
                    if (InSrv.Map(CEngine.Instance.RenderContext.ImmCommandList, 0, &pData, &rowPitch, &depthPitch))
                    {
                        InSrv.BuildImageBlob(blob, pData, rowPitch);
                        InSrv.Unmap(CEngine.Instance.RenderContext.ImmCommandList, 0);
                    }
                }
                bool bSave = false;
                if (bSave)
                {
                    var blbArray = new EngineNS.Support.CBlobObject[] { blob };
                    CShaderResourceView.SaveSnap(@"D:\OpenSource\titan3d\Content\editor\basemesh\box.gms.snap", blbArray);
                }
            };
            mTestSnapshort.mRP_Snapshot.OnAfterTickLogic = (InView, InRc, InCmd, InArg) =>
            {
                CTexture2D ReadableTex = null;
                InCmd.CreateReadableTexture2D(ref ReadableTex, mTestSnapshort.mRP_Snapshot.BaseSceneView.FrameBuffer.GetSRV_RenderTarget(0), mTestSnapshort.mRP_Snapshot.BaseSceneView.FrameBuffer);
                
                EngineNS.CEngine.Instance.GpuFetchManager.RegFetchTexture2D(ReadableTex, OnFetchFinished);
            };
            return true;
        }
        
        EngineNS.Graphics.CGfxGpuFetchManager.FOnGpuFinished OnFetchFinished;
#endif
        public override void SetEnvMap(RName name)
        {
            mOpaqueSE.EnvMapName = name;
            mCustomTranslucentSE.EnvMapName = name;
            mTranslucentSE.EnvMapName = name;
        }
        public static Profiler.TimeScope ScopeDrawUI = Profiler.TimeScopeManager.GetTimeScope(typeof(CEngine), "DrawUI");

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
            Vector3 DirLightDirection =  new Vector3(BaseSceneView.mDirLightDirection_Leak.X, BaseSceneView.mDirLightDirection_Leak.Y, BaseSceneView.mDirLightDirection_Leak.Z);
            mCSM.TickLogic(RHICtx, Camera, DirLightDirection);

            //hitproxy;
            mHitProxy.TickLogic();

            //Picked Effect;
            mPickedEffect.TickLogic();

            BaseSceneView.mFadeParam = mCSM.mFadeParam;
            BaseSceneView.ShadowTransitionScaleArrayEditor = mCSM.mShadowTransitionScaleVec;
            BaseSceneView.mShadowMapSizeAndRcp = mCSM.mShadowMapSizeAndRcp;
            BaseSceneView.Viewer2ShadowMtxArrayEditor0 = mCSM.mViewer2ShadowMtxArray[0];
            BaseSceneView.Viewer2ShadowMtxArrayEditor1 = mCSM.mViewer2ShadowMtxArray[1];
            BaseSceneView.Viewer2ShadowMtxArrayEditor2 = mCSM.mViewer2ShadowMtxArray[2];
            BaseSceneView.Viewer2ShadowMtxArrayEditor3 = mCSM.mViewer2ShadowMtxArray[3];
            BaseSceneView.CsmDistanceArray = mCSM.mSumDistanceFarVec;
            BaseSceneView.CsmNum = (int)mCSM.mCsmNum;

            mForwardBasePass.TickLogic(Camera, view, RHICtx, DPLimitter, GraphicsDebug);

            drawCall += mForwardBasePass.CommitingCMDs.DrawCall;
            drawTriangle += mForwardBasePass.CommitingCMDs.DrawTriangle;
            cmdCount += mForwardBasePass.CommitingCMDs.CmdCount;

            //post effect;
            mMobileAO.TickLogic(RHICtx, Camera, BaseSceneView);
            mBloomMobile.TickLogic(RHICtx, Camera);
            mSunShaftMobile.TickLogic(RHICtx, Camera, DirLightDirection, BaseSceneView);
            
            ////UInt32 ID = mHitProxy.GetHitProxyID(0, 0);

            {
                mCopyPostprocessPass.mScreenView.SunPosNDC = mSunShaftMobile.mSunPosNdc;
                mCopyPostprocessPass.mScreenView.DirLightColor_Intensity = BaseSceneView.DirLightColor_Intensity;
                mCopyPostprocessPass.TickLogic(Camera, view, RHICtx);

                ScopeDrawUI.Begin();
                RiseOnDrawUI(mCopyPostprocessPass.CommitingCMDs, mCopyPostprocessPass.mScreenView);
                ScopeDrawUI.End();
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
        partial void TickRender_Snapshots();
        public override void TickRender(CSwapChain swapChain)
        {
            base.TickRender(SwapChain);

            var RHICtx = EngineNS.CEngine.Instance.RenderContext;
            if (RHICtx == null)
                return;
#if Test_Snapshot
            mTestSnapshort?.RenderTick(null);
#endif
            TickRender_Snapshots();

            //shadow ssm;
            mCSM.TickRender(RHICtx);

            //hit proxy;
            mHitProxy.TickRender();

            //picked effect;
            mPickedEffect.TickRender();

            mForwardBasePass.TickRender(RHICtx);

            //post effect;
            mMobileAO.TickRender(RHICtx);
            mBloomMobile.TickRender(RHICtx);
            mSunShaftMobile.TickRender(RHICtx);

            mCopyPostprocessPass.TickRender(RHICtx);

            SwapChain.Present();
        }
        public override void BeforeFrame()
        {
            mCSM.BeforeFrame();
        }
        public override void TickSync()
        {
            //shadow ssm;
            mCSM.TickSync();

            mHitProxy.TickSync();

            mPickedEffect.TickSync();

            mForwardBasePass.TickSync();

            //post effect;
            mMobileAO.TickSync();
            mBloomMobile.TickSync();
            mSunShaftMobile.TickSync();

            mCopyPostprocessPass.TickSync();
            
            base.TickSync();

            //this is the end of frame;
            Camera.ClearAllRenderLayerData();
        }

        public override void OnResize(CRenderContext RHICtx, CSwapChain SwapChain, UInt32 width, UInt32 height)
        {
            if (Camera == null || mBloomMobile.mUSView8 == null || BaseSceneView == null)
            {
                return;
            }

            SwapChain.OnResize(width, height);

            //RHICtx.BindCurrentSwapChain(mSwapChain);

            Camera.PerspectiveFovLH(Camera.mDefaultFoV, (float)width, (float)height);

            mCopyPostprocessPass.mScreenView.OnResize(RHICtx, SwapChain, width, height);
            BaseSceneView.OnResize(RHICtx, null, width, height);

            //hitproxy;
            mHitProxy.OnResize(width, height);

            //picked effect;
            mPickedEffect.OnResize(width, height);

            //post effect;
            mMobileAO.OnResize(RHICtx, width, height, BaseSceneView);
            mBloomMobile.OnResize(RHICtx, width, height, BaseSceneView);
            mSunShaftMobile.OnResize(RHICtx, width, height);

            mCopyEditorSE.mBaseSceneView = BaseSceneView.FrameBuffer.GetSRV_RenderTarget(0);
            mCopyEditorSE.mBloomTex = mBloomMobile.mUSView8.FrameBuffer.GetSRV_RenderTarget(0);
            mCopyEditorSE.mPickedTex = mPickedEffect.mSV_PickedBlurH.FrameBuffer.GetSRV_RenderTarget(0);
            mCopyEditorSE.mSunShaftTex = mSunShaftMobile.mView_Blur.FrameBuffer.GetSRV_RenderTarget(0);
            //mCopyEditorSE.mSRV_MobileAo = mMobileAO.mView_BlurV.FrameBuffer.GetSRV_RenderTarget(0);
            mCopyEditorSE.mSRV_MobileAo = mMobileAO.mView_AoMask.FrameBuffer.GetSRV_RenderTarget(0);

            var ViewportSizeAndRcp = new Vector4(width, height, 1.0f / width, 1.0f / height);
            BaseSceneView.mViewportSizeAndRcp = ViewportSizeAndRcp;
            mCopyPostprocessPass.mScreenView.ViewportSizeAndRcp = ViewportSizeAndRcp;
        }
    }
}
