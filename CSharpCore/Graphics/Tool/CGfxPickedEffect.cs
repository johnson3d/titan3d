using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Graphics.View;
using EngineNS.Graphics.EnvShader;

namespace EngineNS.Graphics.Tool
{
    public class CGfxPickedEffect
    {
        private int mDPLimitter = int.MaxValue;

        private CCommandList[] mCLDB_Picked;
        //this command buffer is to diable dx11 deffered ctx warning;
        private CCommandList[] mCLDB_DisableWarning;

        private CRenderPassDesc mRenderPassDesc_Picked;

        public CGfxSceneView mSV_PickedSetUp;
        public CGfxScreenView mSV_PickedBlurH;
        public CGfxScreenView mSV_PickedBlurV;
        //public CGfxScreenView mSV_PickedHollow;

        public CGfxPickedSetUpSE mSE_PickedSetUp;
        public CGfxPickedBlurHSE mSE_PickedBlurH;
        public CGfxPickedBlurVSE mSE_PickedBlurV;
        public CGfxPickedHollowSE mSE_PickedHollow;

        private List<CGfxShadingEnv> mBlurAndHollowSEArray = new List<CGfxShadingEnv>();

        private CGfxCamera mCamera;
        private CRenderContext mRHICtx = null;

        private EngineNS.Graphics.Mesh.CGfxMesh mScreenAlignedTriangle;

        public CGfxPickedEffect()
        {
            mRHICtx = CEngine.Instance.RenderContext;

            EngineNS.CCommandListDesc CL_Desc = new EngineNS.CCommandListDesc();
            mCLDB_Picked = new CCommandList[2];
            mCLDB_Picked[0] = mRHICtx.CreateCommandList(CL_Desc);
            mCLDB_Picked[1] = mRHICtx.CreateCommandList(CL_Desc);

            mCLDB_DisableWarning = new CCommandList[2];
            mCLDB_DisableWarning[0] = mRHICtx.CreateCommandList(CL_Desc);
            mCLDB_DisableWarning[1] = mRHICtx.CreateCommandList(CL_Desc);
        }

        public void Cleanup()
        {
            mCLDB_Picked[0].Cleanup();
            mCLDB_Picked[0] = null;
            mCLDB_Picked[1].Cleanup();
            mCLDB_Picked[1] = null;

            mCLDB_DisableWarning[0].Cleanup();
            mCLDB_DisableWarning[0] = null;
            mCLDB_DisableWarning[1].Cleanup();
            mCLDB_DisableWarning[1] = null;

            mSV_PickedSetUp.Cleanup();
            mSV_PickedSetUp = null;
            
            mSV_PickedBlurH = null;
            mSV_PickedBlurV = null;
            //mSV_PickedHollow = null;
        }

        public async System.Threading.Tasks.Task<bool> Init(UInt32 width, UInt32 height, CGfxCamera camera)
        {
            CGfxSceneViewInfo VI_PickedSetUp = new CGfxSceneViewInfo();
            VI_PickedSetUp.mUseDSV = true;
            UInt32 TempWidth = width;
            UInt32 TempHeight = height;
            VI_PickedSetUp.Width = TempWidth;
            VI_PickedSetUp.Height = TempHeight;
            VI_PickedSetUp.mDSVDesc.Init();
            VI_PickedSetUp.mDSVDesc.Format = EngineNS.EPixelFormat.PXF_D24_UNORM_S8_UINT;
            VI_PickedSetUp.mDSVDesc.Width = TempWidth;
            VI_PickedSetUp.mDSVDesc.Height = TempHeight;

            CRenderTargetViewDesc RTVDesc_PickeSetUp = new CRenderTargetViewDesc();
            RTVDesc_PickeSetUp.Init();
            RTVDesc_PickeSetUp.Format = EPixelFormat.PXF_R16G16_FLOAT;
            RTVDesc_PickeSetUp.Width = TempWidth;
            RTVDesc_PickeSetUp.Height = TempHeight;
            VI_PickedSetUp.mRTVDescArray.Add(RTVDesc_PickeSetUp);

            mSV_PickedSetUp = new CGfxSceneView();
            if (false == mSV_PickedSetUp.Init(mRHICtx, null, VI_PickedSetUp))
            {
                return false;
            }

            mSE_PickedSetUp = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxPickedSetUpSE>();
            mSE_PickedBlurH = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxPickedBlurHSE>();
            mSE_PickedBlurV = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxPickedBlurVSE>();
            mSE_PickedHollow = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxPickedHollowSE>();
            mBlurAndHollowSEArray.Add(mSE_PickedBlurH);
            mBlurAndHollowSEArray.Add(mSE_PickedHollow);

            var ScreenAlignedTriangle = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(mRHICtx, CEngineDesc.ScreenAlignedTriangleName, true);
            var DefaultMtlInst = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(mRHICtx, RName.GetRName("Material/defaultmaterial.instmtl"));
            mScreenAlignedTriangle = CEngine.Instance.MeshManager.CreateMesh(mRHICtx, ScreenAlignedTriangle);
            mScreenAlignedTriangle.SetMaterialInstance(mRHICtx, 0, DefaultMtlInst, CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);
            //await mScreenAlignedTriangle.AwaitEffects();
            
            //blur h and hollow;
            {
                CGfxScreenViewDesc VI_BlurH = new CGfxScreenViewDesc();
                VI_BlurH.UseDepthStencilView = false;
                VI_BlurH.Width = TempWidth;
                VI_BlurH.Height = TempHeight;

                var RTVDesc_BlurH = new CRenderTargetViewDesc();
                RTVDesc_BlurH.Init();
                RTVDesc_BlurH.Format = EPixelFormat.PXF_R16G16_FLOAT;
                VI_BlurH.mRTVDescArray.Add(RTVDesc_BlurH);
                mSV_PickedBlurH = new CGfxScreenView();
                if (await mSV_PickedBlurH.InitForMultiPassMode(mRHICtx, VI_BlurH, mBlurAndHollowSEArray, DefaultMtlInst, mScreenAlignedTriangle) == false)
                {
                    return false;
                }
            }
            
            //blur v;
            {
                CGfxScreenViewDesc VI_BlurV = new CGfxScreenViewDesc();
                VI_BlurV.UseDepthStencilView = false;
                VI_BlurV.Width = TempWidth;
                VI_BlurV.Height = TempHeight;

                var RTVDesc_BlurV = new CRenderTargetViewDesc();
                RTVDesc_BlurV.Init();
                RTVDesc_BlurV.Format = EPixelFormat.PXF_R16G16_FLOAT;
                VI_BlurV.mRTVDescArray.Add(RTVDesc_BlurV);
                mSV_PickedBlurV = new CGfxScreenView();
                if (await mSV_PickedBlurV.Init(mRHICtx, null, VI_BlurV, mSE_PickedBlurV, DefaultMtlInst, mScreenAlignedTriangle) == false)
                {
                    return false;
                }
            }
            
            mCamera = camera;

            mRenderPassDesc_Picked = new CRenderPassDesc();
            FrameBufferClearColor TempClearColor = new FrameBufferClearColor();
            TempClearColor.r = 0.0f;
            TempClearColor.g = 1.0f;
            TempClearColor.b = 0.0f;
            TempClearColor.a = 0.0f;
            mRenderPassDesc_Picked.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_Picked.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_Picked.mFBClearColorRT0 = TempClearColor;
            mRenderPassDesc_Picked.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_Picked.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_Picked.mDepthClearValue = 1.0f;
            mRenderPassDesc_Picked.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_Picked.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_Picked.mStencilClearValue = 0u;

            mSE_PickedBlurH.mSRV_PickedSetUp = mSV_PickedSetUp.FrameBuffer.GetSRV_RenderTarget(0);
            mSE_PickedBlurV.mSRV_PickedBlurH = mSV_PickedBlurH.FrameBuffer.GetSRV_RenderTarget(0);
            mSE_PickedHollow.mSRV_PickedSetUp = mSV_PickedSetUp.FrameBuffer.GetSRV_RenderTarget(0);
            mSE_PickedHollow.mSRV_PickedBlur = mSV_PickedBlurV.FrameBuffer.GetSRV_RenderTarget(0);

            var ViewportSizeAndRcp = new Vector4(TempWidth, TempHeight, 1.0f / TempWidth, 1.0f / TempHeight);
            mSV_PickedBlurH.ViewportSizeAndRcp = ViewportSizeAndRcp;
            mSV_PickedBlurV.ViewportSizeAndRcp = ViewportSizeAndRcp;

            return true;
        }
        public static Profiler.TimeScope ScopeTickLogic = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxPickedEffect), nameof(TickLogic));
        public void TickLogic()
        {
            if (mSV_PickedSetUp == null)
            {
                return;
            }

            ScopeTickLogic.Begin();

            mSV_PickedSetUp.CookSpecRenderLayerDataToPass(mRHICtx, ERenderLayer.RL_Opaque, mCamera, mSE_PickedSetUp, PrebuildPassIndex.PPI_PickedEditor);
            mSV_PickedSetUp.CookSpecRenderLayerDataToPass(mRHICtx, ERenderLayer.RL_CustomOpaque, mCamera, mSE_PickedSetUp, PrebuildPassIndex.PPI_PickedEditor);
            mSV_PickedSetUp.CookSpecRenderLayerDataToPass(mRHICtx, ERenderLayer.RL_CustomTranslucent, mCamera, mSE_PickedSetUp, PrebuildPassIndex.PPI_PickedEditor);
            mSV_PickedSetUp.CookSpecRenderLayerDataToPass(mRHICtx, ERenderLayer.RL_Translucent, mCamera, mSE_PickedSetUp, PrebuildPassIndex.PPI_PickedEditor);
            mSV_PickedSetUp.CookSpecRenderLayerDataToPass(mRHICtx, ERenderLayer.RL_Gizmos, mCamera, mSE_PickedSetUp, PrebuildPassIndex.PPI_PickedEditor);

            var CmdList = mCLDB_Picked[0];
            mSV_PickedSetUp.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Opaque);
            mSV_PickedSetUp.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_CustomOpaque);
            mSV_PickedSetUp.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_CustomTranslucent);
            mSV_PickedSetUp.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Translucent);
            mSV_PickedSetUp.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Gizmos);

            CmdList.BeginCommand();
            CmdList.BeginRenderPass(mRenderPassDesc_Picked, mSV_PickedSetUp.FrameBuffer);
            CmdList.BuildRenderPass(mDPLimitter);
            CmdList.EndRenderPass();
            
            mSV_PickedBlurH.CookViewportMeshToPassInMultiPassMode(mRHICtx, mSE_PickedBlurH, 0, mCamera, mScreenAlignedTriangle);
            mSV_PickedBlurH.PushPassToRHIInMultiPassMode(CmdList, 0);
            CmdList.BeginRenderPass(mRenderPassDesc_Picked, mSV_PickedBlurH.FrameBuffer);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();

            mSV_PickedBlurV.CookViewportMeshToPass(mRHICtx, mSE_PickedBlurV, mCamera, mScreenAlignedTriangle);
            mSV_PickedBlurV.PushPassToRHI(CmdList);
            CmdList.BeginRenderPass(mRenderPassDesc_Picked, mSV_PickedBlurV.FrameBuffer);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();
            CmdList.EndCommand();

            CmdList = mCLDB_DisableWarning[0];
            mSV_PickedBlurH.CookViewportMeshToPassInMultiPassMode(mRHICtx, mSE_PickedHollow, 1, mCamera, mScreenAlignedTriangle);
            mSV_PickedBlurH.PushPassToRHIInMultiPassMode(CmdList, 1);
            CmdList.BeginCommand();
            CmdList.BeginRenderPass(mRenderPassDesc_Picked, mSV_PickedBlurH.FrameBuffer);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();
            CmdList.EndCommand();

            ScopeTickLogic.End();
        }
        public void TickRender()
        {
            var CmdList = mCLDB_Picked[1];
            CmdList.Commit(mRHICtx);

            CmdList = mCLDB_DisableWarning[1];
            CmdList.Commit(mRHICtx);
        }
        public void TickSync()
        {
            var Temp = mCLDB_Picked[0];
            mCLDB_Picked[0] = mCLDB_Picked[1];
            mCLDB_Picked[1] = Temp;

            Temp = mCLDB_DisableWarning[0];
            mCLDB_DisableWarning[0] = mCLDB_DisableWarning[1];
            mCLDB_DisableWarning[1] = Temp;
        }
        public void OnResize(UInt32 width, UInt32 height)
        {
            UInt32 TempWidth = width;
            UInt32 TempHeight = height;

            mSV_PickedSetUp.OnResize(mRHICtx, null, width, height);
            mSV_PickedBlurH.OnResize(mRHICtx, null, width, height);
            mSV_PickedBlurV.OnResize(mRHICtx, null, width, height);
            
            mSE_PickedBlurH.mSRV_PickedSetUp = mSV_PickedSetUp.FrameBuffer.GetSRV_RenderTarget(0);
            mSE_PickedBlurV.mSRV_PickedBlurH = mSV_PickedBlurH.FrameBuffer.GetSRV_RenderTarget(0);
            mSE_PickedHollow.mSRV_PickedSetUp = mSV_PickedSetUp.FrameBuffer.GetSRV_RenderTarget(0);
            mSE_PickedHollow.mSRV_PickedBlur = mSV_PickedBlurV.FrameBuffer.GetSRV_RenderTarget(0);

            var ViewportSizeAndRcp = new Vector4(TempWidth, TempHeight, 1.0f / TempWidth, 1.0f / TempHeight);
            mSV_PickedBlurH.ViewportSizeAndRcp = ViewportSizeAndRcp;
            mSV_PickedBlurV.ViewportSizeAndRcp = ViewportSizeAndRcp;
        }
    }
}
