using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.View;

namespace EngineNS.Graphics.RenderPolicy
{
    public class CGfxRP_SceneCapture : CGfxRenderPolicy
    {
        CCommandList mCommandList_Final;
        View.CGfxSceneView mCaptureSV;
        CRenderPassDesc mRenderPassDesc_SceneCapture;


        public View.CGfxSceneView CaptureSV => mCaptureSV;
        EngineNS.Graphics.EnvShader.CGfxSceneCaptureSE mCaptureSE;
        Support.CBlobObject mTexData0;
        public Support.CBlobObject TexData0 => mTexData0;
        Support.CBlobObject mTexData1;
        public Support.CBlobObject TexData1 => mTexData1;

        public uint mViewWidth;
        public uint mViewHeight;

        //private CMRTClearColor[] mClearColorArray = new CMRTClearColor[]
        //{
        //    new CMRTClearColor(0, 0xFFFFFFFF),
        //    new CMRTClearColor(1, 0x00000000),
        //};

        public CGfxRP_SceneCapture()
        {
            var RHICtx = CEngine.Instance.RenderContext;

            var cmdListDesc = new CCommandListDesc();
            mCommandList_Final = RHICtx.CreateCommandList(cmdListDesc);
        }
        public override void Cleanup()
        {
            mCommandList_Final.Cleanup();
            mCommandList_Final = null;

            mCaptureSV.Cleanup();
            mCaptureSV = null;

            base.Cleanup();
        }

        public override async Task<bool> Init(CRenderContext RHICtx, uint width, uint height, CGfxCamera camera, IntPtr WinHandle)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            mViewWidth = width;
            mViewHeight = height;

            var viewInfo = new View.CGfxSceneViewInfo();
            viewInfo.mUseDSV = true;
            viewInfo.Width = width;
            viewInfo.Height = height;
            viewInfo.mDSVDesc.Init();
            viewInfo.mDSVDesc.Format = EPixelFormat.PXF_D24_UNORM_S8_UINT;
            viewInfo.mDSVDesc.Width = width;
            viewInfo.mDSVDesc.Height = height;

            var rtDesc = new CRenderTargetViewDesc();
            rtDesc.Init();
            rtDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;// .PXF_R32_UINT;
            rtDesc.Width = width;
            rtDesc.Height = height;
            viewInfo.mRTVDescArray.Add(rtDesc);
            viewInfo.mRTVDescArray.Add(rtDesc);

            mCaptureSV = new View.CGfxSceneView();
            if(false == mCaptureSV.Init(RHICtx, null, viewInfo))
            {
                return false;
            }

            mRenderPassDesc_SceneCapture = new CRenderPassDesc();
            FrameBufferClearColor TempClearColor0 = new FrameBufferClearColor();
            TempClearColor0.r = 1.0f;
            TempClearColor0.g = 1.0f;
            TempClearColor0.b = 1.0f;
            TempClearColor0.a = 1.0f;
            FrameBufferClearColor TempClearColor1 = new FrameBufferClearColor();
            TempClearColor1.r = 0.0f;
            TempClearColor1.g = 0.0f;
            TempClearColor1.b = 0.0f;
            TempClearColor1.a = 0.0f;
            
            mRenderPassDesc_SceneCapture.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_SceneCapture.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_SceneCapture.mFBClearColorRT0 = TempClearColor0;
            mRenderPassDesc_SceneCapture.mFBClearColorRT1 = TempClearColor1;
            mRenderPassDesc_SceneCapture.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_SceneCapture.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_SceneCapture.mDepthClearValue = 1.0f;
            mRenderPassDesc_SceneCapture.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_SceneCapture.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_SceneCapture.mStencilClearValue = 0u;
            
            Camera = camera;
            if(mCaptureSE == null)
            {
                mCaptureSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<EnvShader.CGfxSceneCaptureSE>();
            }

            mTexData0 = new Support.CBlobObject();
            mTexData1 = new Support.CBlobObject();

            return true;
        }

        public override void TickLogic(CGfxSceneView view, CRenderContext RHICtx)
        {
            if (mCaptureSV == null)
                return;
            var rc = EngineNS.CEngine.Instance.RenderContext;
            if (rc == null)
                return;

            mCaptureSV.CookSpecRenderLayerDataToPass(rc, ERenderLayer.RL_Opaque, Camera, mCaptureSE, PrebuildPassIndex.PPI_SceneCapture);
            mCaptureSV.CookSpecRenderLayerDataToPass(rc, ERenderLayer.RL_CustomOpaque, Camera, mCaptureSE, PrebuildPassIndex.PPI_SceneCapture);
            mCaptureSV.CookSpecRenderLayerDataToPass(rc, ERenderLayer.RL_CustomTranslucent, Camera, mCaptureSE, PrebuildPassIndex.PPI_SceneCapture);
            mCaptureSV.CookSpecRenderLayerDataToPass(rc, ERenderLayer.RL_Translucent, Camera, mCaptureSE, PrebuildPassIndex.PPI_SceneCapture);
            mCaptureSV.CookSpecRenderLayerDataToPass(rc, ERenderLayer.RL_Gizmos, Camera, mCaptureSE, PrebuildPassIndex.PPI_SceneCapture);
            mCommandList_Final.BeginCommand();
            mCommandList_Final.BeginRenderPass(mRenderPassDesc_SceneCapture, mCaptureSV.FrameBuffer);
            mCaptureSV.PushSpecRenderLayerDataToRHI(mCommandList_Final, ERenderLayer.RL_Opaque);
            mCaptureSV.PushSpecRenderLayerDataToRHI(mCommandList_Final, ERenderLayer.RL_CustomOpaque);
            mCaptureSV.PushSpecRenderLayerDataToRHI(mCommandList_Final, ERenderLayer.RL_CustomTranslucent);
            mCaptureSV.PushSpecRenderLayerDataToRHI(mCommandList_Final, ERenderLayer.RL_Translucent);
            mCaptureSV.PushSpecRenderLayerDataToRHI(mCommandList_Final, ERenderLayer.RL_Gizmos);

            mCommandList_Final.BuildRenderPass(int.MaxValue, false, true);
            mCommandList_Final.EndRenderPass();
            mCommandList_Final.EndCommand();
        }

        public bool CaptureRGBData = false;
        public bool UseCapture = false;
        public override void TickRender(CSwapChain swapChain)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            if (rc == null)
                return;

            mCommandList_Final.Commit(rc);

            if (UseCapture)
            {
                var tex = mCaptureSV.FrameBuffer.GetSRV_RenderTarget(0);
                tex.GetTexture2DData(rc, mTexData0, 0, (int)mViewWidth, (int)mViewHeight);
                if (CaptureRGBData)
                {
                    tex = mCaptureSV.FrameBuffer.GetSRV_RenderTarget(1);
                    tex.GetTexture2DData(rc, mTexData1, 0, (int)mViewWidth, (int)mViewHeight);
                }
            }
        }

        public CShaderResourceView GetActoridTexture()
        {
            return mCaptureSV.FrameBuffer.GetSRV_RenderTarget(0);
        }
    }
}
