using EngineNS.Graphics.EnvShader;
using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Graphics.View;

namespace EngineNS.Graphics.RenderPolicy
{
    public class CGfxRP_Snapshot : CGfxRenderPolicy
    {
        private CCommandList[] mCLDB_Snapshot;
        private CCommandList[] mCLDB_Copy;

        private CRenderPassDesc mRenderPassDesc_Snapshot;
        private CRenderPassDesc mRenderPassDesc_Final;

        public CGfxScreenView mFinalView;
        private EngineNS.Graphics.Mesh.CGfxMesh mScreenAlignedTriangle;

        public CGfxSnapshotSE mSnapshotSE;
        public CGfxMobileCopySE mCopySE;
        public CGfxRP_Snapshot()
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;

            mCLDB_Snapshot = new CCommandList[2];
            EngineNS.CCommandListDesc CL_Desc = new EngineNS.CCommandListDesc();
            mCLDB_Snapshot[0] = RHICtx.CreateCommandList(CL_Desc);
            mCLDB_Snapshot[1] = RHICtx.CreateCommandList(CL_Desc);

            mCLDB_Copy = new CCommandList[2];
            mCLDB_Copy[0] = RHICtx.CreateCommandList(CL_Desc);
            mCLDB_Copy[1] = RHICtx.CreateCommandList(CL_Desc);
        }

        public override void Cleanup()
        {
            mCLDB_Snapshot[0].Cleanup();
            mCLDB_Snapshot[0] = null;
            mCLDB_Snapshot[1].Cleanup();
            mCLDB_Snapshot[1] = null;

            mCLDB_Copy[0].Cleanup();
            mCLDB_Copy[0] = null;
            mCLDB_Copy[1].Cleanup();
            mCLDB_Copy[1] = null;
            
            BaseSceneView.Cleanup();
            BaseSceneView = null;

            mFinalView = null;
            
            base.Cleanup();
        }
        
        public override async System.Threading.Tasks.Task<bool> Init(CRenderContext RHICtx, UInt32 width, UInt32 height, CGfxCamera camera, IntPtr WinHandle)
        {
            CGfxSceneViewInfo SnapshotViewInfo = new CGfxSceneViewInfo();
            SnapshotViewInfo.mUseDSV = true;
            SnapshotViewInfo.Width = width;
            SnapshotViewInfo.Height = height;
            SnapshotViewInfo.mDSVDesc.Init();
            SnapshotViewInfo.mDSVDesc.Format = EngineNS.EPixelFormat.PXF_D24_UNORM_S8_UINT;
            SnapshotViewInfo.mDSVDesc.Width = width;
            SnapshotViewInfo.mDSVDesc.Height = height;

            CRenderTargetViewDesc rtDesc0 = new CRenderTargetViewDesc();
            rtDesc0.Init();
            rtDesc0.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            rtDesc0.Width = width;
            rtDesc0.Height = height;
            SnapshotViewInfo.mRTVDescArray.Add(rtDesc0);

            BaseSceneView = new CGfxSceneView();
            if (false == BaseSceneView.Init(RHICtx, null, SnapshotViewInfo))
            {
                return false;
            }
            
            Camera = camera;
            //mCamera.SetSceneView(RHICtx, mBaseSceneView);

            mRenderPassDesc_Snapshot = new CRenderPassDesc();
            FrameBufferClearColor TempClearColor = new FrameBufferClearColor();
            TempClearColor.r = 0.0f;
            TempClearColor.g = 0.0f;
            TempClearColor.b = 0.0f;
            TempClearColor.a = 0.0f;
            mRenderPassDesc_Snapshot.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_Snapshot.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_Snapshot.mFBClearColorRT0 = TempClearColor;
            mRenderPassDesc_Snapshot.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_Snapshot.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_Snapshot.mDepthClearValue = 1.0f;
            mRenderPassDesc_Snapshot.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_Snapshot.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_Snapshot.mStencilClearValue = 0u;

            mRenderPassDesc_Final = new CRenderPassDesc();
            TempClearColor.r = 0.0f;
            TempClearColor.g = 1.0f;
            TempClearColor.b = 0.0f;
            TempClearColor.a = 0.0f;
            mRenderPassDesc_Final.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_Final.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_Final.mFBClearColorRT0 = TempClearColor;
            mRenderPassDesc_Final.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_Final.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_Final.mDepthClearValue = 1.0f;
            mRenderPassDesc_Final.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_Final.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_Final.mStencilClearValue = 0u;

            if (mSnapshotSE == null)
            {
                mSnapshotSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxSnapshotSE>();
            }
            if (mCopySE == null)
            {
                mCopySE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileCopySE>();
            }

            var ufoViewInfo = new CGfxScreenViewDesc();
            ufoViewInfo.IsSwapChainBuffer = false;
            ufoViewInfo.UseDepthStencilView = false;
            ufoViewInfo.Width = width;
            ufoViewInfo.Height = height;

            var rtDesc1 = new CRenderTargetViewDesc();
            rtDesc1.Init();
            rtDesc1.mCanBeSampled = vBOOL.FromBoolean(true);
            rtDesc1.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            rtDesc1.Width = width;
            rtDesc1.Height = height;
            ufoViewInfo.mRTVDescArray.Add(rtDesc1);

            var screenAlignedTriangle = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(RHICtx, CEngineDesc.ScreenAlignedTriangleName, true);
            mScreenAlignedTriangle = CEngine.Instance.MeshManager.CreateMesh(RHICtx, screenAlignedTriangle);
            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(RHICtx, RName.GetRName("Material/defaultmaterial.instmtl"));
            mScreenAlignedTriangle.SetMaterialInstance(RHICtx, 0, mtl, CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);
            //await mScreenAlignedTriangle.AwaitEffects();
            mFinalView = new CGfxScreenView();
            await mFinalView.Init(RHICtx, null, ufoViewInfo, mCopySE, mtl, mScreenAlignedTriangle);

            var viewportSizeAndRcp = new Vector4(width, height, 1.0f / width, 1.0f / height);
            mFinalView.ViewportSizeAndRcp = viewportSizeAndRcp;
            
            return true;
        }

        public delegate void FAfterTickLogic(CGfxSceneView view, CRenderContext rc, CCommandList cmd, object arg);
        public FAfterTickLogic OnAfterTickLogic;
        public object OnAfterTickLogicArgument = null;
        public override void TickLogic(CGfxSceneView view, CRenderContext rc)
        {
            if (BaseSceneView == null)
            {
                return;
            }

            Camera.PushVisibleSceneMesh2RenderLayer();

            //int CurrDPNumber = 0;
            //opaque render pass;
            BaseSceneView.CookSpecRenderLayerDataToPass(rc, ERenderLayer.RL_Opaque, Camera, mSnapshotSE, PrebuildPassIndex.PPI_Snapshot);
            //custom opaque render pass;
            BaseSceneView.CookSpecRenderLayerDataToPass(rc, ERenderLayer.RL_CustomOpaque, Camera, mSnapshotSE, PrebuildPassIndex.PPI_Snapshot);
            //sky render pass;
            BaseSceneView.CookSpecRenderLayerDataToPass(rc, ERenderLayer.RL_Sky, Camera, mSnapshotSE, PrebuildPassIndex.PPI_Snapshot);
            //custom translucent render pass;
            BaseSceneView.CookSpecRenderLayerDataToPass(rc, ERenderLayer.RL_CustomTranslucent, Camera, mSnapshotSE, PrebuildPassIndex.PPI_Snapshot);
            //translucent render pass;
            BaseSceneView.CookSpecRenderLayerDataToPass(rc, ERenderLayer.RL_Translucent, Camera, mSnapshotSE, PrebuildPassIndex.PPI_Snapshot);
            //gizmos render pass;
            BaseSceneView.CookSpecRenderLayerDataToPass(rc, ERenderLayer.RL_Gizmos, Camera, mSnapshotSE, PrebuildPassIndex.PPI_Snapshot);

            var CmdList = mCLDB_Snapshot[0];
            BaseSceneView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Opaque);
            BaseSceneView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_CustomOpaque);
            BaseSceneView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Sky);
            BaseSceneView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_CustomTranslucent);
            BaseSceneView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Translucent);
            BaseSceneView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Gizmos);

            CmdList.BeginCommand();
            CmdList.BeginRenderPass(mRenderPassDesc_Snapshot, BaseSceneView.FrameBuffer);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();

            if (OnAfterTickLogic != null)
            {
                OnAfterTickLogic(view, rc, CmdList, OnAfterTickLogicArgument);
            }

            CmdList.EndCommand();

            CmdList = mCLDB_Copy[0];
            if(mScreenAlignedTriangle != null)
            {
                mFinalView.CookViewportMeshToPass(rc, mCopySE, Camera, mScreenAlignedTriangle);
                mFinalView.PushPassToRHI(CmdList);
                this.RiseOnDrawUI(CmdList, mFinalView);
            }

            //CmdList.BeginCommand();
            //CmdList.BeginRenderPass(mRenderPassDesc_Final, mFinalView.FrameBuffer);
            //LatestPass = CmdList.BuildRenderPass(int.MaxValue, GraphicsDebug);
            //CmdList.EndRenderPass();
            //CmdList.EndCommand();
        }
        public override void TickRender(CSwapChain swapChain)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;

            var CmdList = mCLDB_Snapshot[1];
            CmdList.Commit(rc);

            CmdList = mCLDB_Copy[1];
            CmdList.BeginCommand();
            CmdList.BeginRenderPass(mRenderPassDesc_Final, mFinalView.FrameBuffer);
            LatestPass = CmdList.BuildRenderPass(int.MaxValue, GraphicsDebug);
            CmdList.EndRenderPass();

            CmdList.EndCommand();
            CmdList.Commit(rc);

            base.TickRender(null);

            rc.FlushImmContext();
        }
        public override void TickSync()
        {
            var temp = mCLDB_Snapshot[0];
            mCLDB_Snapshot[0] = mCLDB_Snapshot[1];
            mCLDB_Snapshot[1] = temp;

            temp = mCLDB_Copy[0];
            mCLDB_Copy[0] = mCLDB_Copy[1];
            mCLDB_Copy[1] = temp;
            
            CEngine.SDK_RResourceSwapChain_TickSwap(CEngine.Instance.RenderContext.CoreObject);

            base.TickSync();

            Camera.ClearAllRenderLayerData();
        }
        public override void OnResize(CRenderContext RHICtx, CSwapChain SwapChain, UInt32 width, UInt32 height)
        {
            Camera.PerspectiveFovLH(Camera.mDefaultFoV, (float)width, (float)height);

            BaseSceneView.OnResize(RHICtx, null, width, height);
        }
    }
}
