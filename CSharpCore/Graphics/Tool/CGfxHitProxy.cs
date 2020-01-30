using EngineNS.Graphics.EnvShader;
using EngineNS.Graphics.PostEffect;
using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Graphics.View;
using EngineNS.Support;

namespace EngineNS.Graphics.Tool
{
    public class CGfxHitProxy
    {
        private CCommandList[] mCLDB_HitProxy;
        
        private CRenderPassDesc mRenderPassDesc_HitProxy;

        private CGfxSceneView mHitProxyView;

        private CGfxCamera mCamera;

        //private CMRTClearColor[] mClearColorArray = new CMRTClearColor[]
        //{
        //        new CMRTClearColor(0, 0x00000000),
        //};

        //env shader;
        private CGfxHitProxySE mHitProxySE;
        private CGfxHitProxyAxisSE mHitProxyAxisSE;

        private CBlobObject mHitProxyCPUTexture;

        private UInt32 mHitProxyID = 0;
        private const UInt32 mHitCheckRegion = 2;
        private const UInt32 mCheckRegionSamplePointCount = 25;
        private UInt32[] mHitProxyIdArray = new UInt32[mCheckRegionSamplePointCount];
        private const UInt32 mCheckRegionCenter = 13;
        private const UInt32 mSkipID = 0;

        private UInt32 mViewportSizeX;
        private UInt32 mViewportSizeY;

        private Int32 mTakeElapseFrame = 5;
        private bool mUpdateSelf = false;

        public bool mEnabled = true;
        private bool mCpuTexHasData = false;


        #region Get_Set_Method
        private Int32 Clamp( Int32 ValueIn,  Int32 MinValue, Int32 MaxValue)
	    {
		    return ValueIn < MinValue ? MinValue : ValueIn < MaxValue ? ValueIn : MaxValue;
	    }
        private struct PixelDesc
        {
            public int Width;
            public int Height;
            public int Stride;
            public EPixelFormat Format;
            public PixelDesc(int width, int height, int stride, EPixelFormat format)
            {
                Width = width;
                Height = height;
                Stride = stride;
                Format = format;
            }
        }
        public UInt32 GetHitProxyID(UInt32 MouseX, UInt32 MouseY)
        {
            if (mHitProxyCPUTexture == null || mHitProxyCPUTexture.Size==0 || mCpuTexHasData == false)
            {
                return 0;
            }

            Int32 RegionMinX = (Int32)(MouseX - mHitCheckRegion);
            Int32 RegionMinY = (Int32)(MouseY - mHitCheckRegion);
            Int32 RegionMaxX = (Int32)(MouseX + mHitCheckRegion);
            Int32 RegionMaxY = (Int32)(MouseY + mHitCheckRegion);

            RegionMinX = Clamp(RegionMinX, 0, (Int32)mViewportSizeX - 1);
            RegionMinY = Clamp(RegionMinY, 0, (Int32)mViewportSizeY - 1);
            RegionMaxX = Clamp(RegionMaxX, 0, (Int32)mViewportSizeX - 1);
            RegionMaxY = Clamp(RegionMaxY, 0, (Int32)mViewportSizeY - 1);

            Int32 RegionSizeX = RegionMaxX - RegionMinX + 1;
            Int32 RegionSizeY = RegionMaxY - RegionMinY + 1;

            if (RegionSizeX > 0 && RegionSizeY > 0)
            {
                unsafe
                {
                    lock(mHitProxyCPUTexture)
                    {
                        byte* pPixelData = (byte*)mHitProxyCPUTexture.Data.ToPointer();
                        var pBitmapDesc = (PixelDesc*)pPixelData;
                        pPixelData += sizeof(PixelDesc);

                        IntColor* HitProxyIdArray = (IntColor*)pPixelData;
                        if (HitProxyIdArray == null)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"HitProxyError: Null Ptr!!!", "");
                            return 0;
                        }

                        int max = pBitmapDesc->Width * pBitmapDesc->Height;
                        if (mViewportSizeX != pBitmapDesc->Width)
                            return 0;
                        if (mViewportSizeY != pBitmapDesc->Height)
                            return 0;

                        UInt32 HitProxyArrayIdx = 0;
                        for (Int32 PointY = RegionMinY; PointY < RegionMaxY; PointY++)
                        {
                            IntColor* TempHitProxyIdCache = &HitProxyIdArray[PointY * mViewportSizeX];
                            for (Int32 PointX = RegionMinX; PointX < RegionMaxX; PointX++)
                            {
                                mHitProxyIdArray[HitProxyArrayIdx] = CEngine.Instance.HitProxyManager.ConvertCpuTexColorToHitProxyId(TempHitProxyIdCache[PointX]);
                                HitProxyArrayIdx++;
                            }
                        }

                        if (mHitProxyIdArray[mCheckRegionCenter] == mSkipID)
                        {
                            for (UInt32 idx = 0; idx < mCheckRegionSamplePointCount; idx++)
                            {
                                if (mHitProxyIdArray[idx] == mSkipID)
                                {
                                    mHitProxyID = mSkipID;
                                    continue;
                                }

                                mHitProxyID = mHitProxyIdArray[idx];
                            }
                        }
                        else
                        {
                            mHitProxyID = mHitProxyIdArray[mCheckRegionCenter];
                        }
                    }
                }
            }
         
            return mHitProxyID;
        }

        #endregion

        #region Main_Function
        public CGfxHitProxy()
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;

            mCLDB_HitProxy = new CCommandList[2];
            
            EngineNS.CCommandListDesc clDesc = new EngineNS.CCommandListDesc();
            mCLDB_HitProxy[0] = RHICtx.CreateCommandList(clDesc);
            mCLDB_HitProxy[1] = RHICtx.CreateCommandList(clDesc);
            
        }

        public void Cleanup()
        {
            mCLDB_HitProxy[0].Cleanup();
            mCLDB_HitProxy[0] = null;
            mCLDB_HitProxy[1].Cleanup();
            mCLDB_HitProxy[1] = null;
            
            mHitProxyView.Cleanup();
            mHitProxyView = null;
        }

        public bool Init(UInt32 width, UInt32 height, CGfxCamera camera)
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;

            mViewportSizeX = width;
            mViewportSizeY = height;

            //hit proxy view;
            CGfxSceneViewInfo HitProxyViewInfo = new CGfxSceneViewInfo();
            HitProxyViewInfo.mUseDSV = true;
            HitProxyViewInfo.Width = width;
            HitProxyViewInfo.Height = height;
            HitProxyViewInfo.mDSVDesc.Init();
            HitProxyViewInfo.mDSVDesc.Format = EngineNS.EPixelFormat.PXF_D24_UNORM_S8_UINT;
            HitProxyViewInfo.mDSVDesc.Width = width;
            HitProxyViewInfo.mDSVDesc.Height = height;

            CRenderTargetViewDesc rtDesc0 = new CRenderTargetViewDesc();
            rtDesc0.Init();
            rtDesc0.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            rtDesc0.Width = width;
            rtDesc0.Height = height;
            HitProxyViewInfo.mRTVDescArray.Add(rtDesc0);

            mHitProxyView = new CGfxSceneView();
            if (false == mHitProxyView.Init(RHICtx, null, HitProxyViewInfo))
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"HitProxyError!!!", "");
                return false;
            }
            
            mCamera = camera;

            mRenderPassDesc_HitProxy = new CRenderPassDesc();
            FrameBufferClearColor TempClearColor = new FrameBufferClearColor();
            TempClearColor.r = 0.0f;
            TempClearColor.g = 0.0f;
            TempClearColor.b = 0.0f;
            TempClearColor.a = 0.0f;
            mRenderPassDesc_HitProxy.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_HitProxy.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_HitProxy.mFBClearColorRT0 = TempClearColor;
            mRenderPassDesc_HitProxy.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_HitProxy.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_HitProxy.mDepthClearValue = 1.0f;
            mRenderPassDesc_HitProxy.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_HitProxy.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_HitProxy.mStencilClearValue = 0u;

            if (mHitProxySE == null)
            {
                mHitProxySE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxHitProxySE>();
            }
            if (mHitProxySE == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"HitProxyError!!!", "");
                return false;
            }
            
            if (mHitProxyAxisSE == null)
            {
                mHitProxyAxisSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxHitProxyAxisSE>();
            }
            if (mHitProxyAxisSE == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"HitProxyError!!!", "");
                return false;
            }

            mHitProxyCPUTexture = new CBlobObject();
            return true;
        }

        public static Profiler.TimeScope ScopeTickLogic = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxHitProxy), nameof(TickLogic));
        public void TickLogic()
        {
            if (mUpdateSelf == false || mEnabled == false)
            {
                return;
            }

            var RHICtx = EngineNS.CEngine.Instance.RenderContext;
            if (RHICtx == null || mHitProxyView == null)
            {
                return;
            }
            ScopeTickLogic.Begin();

            //int CurrDPNumber = 0;
            //CEngine.Instance.HitProxyManager.HitProxyIsNewest = false;

            //opaque render pass;
            mHitProxyView.CookSpecRenderLayerDataToPass(RHICtx, ERenderLayer.RL_Opaque, mCamera, mHitProxySE, PrebuildPassIndex.PPI_HitProxy);
            //custom opaque pass;
            mHitProxyView.CookSpecRenderLayerDataToPass(RHICtx, ERenderLayer.RL_CustomOpaque, mCamera, mHitProxySE, PrebuildPassIndex.PPI_HitProxy);
            //custom translucent render pass;
            mHitProxyView.CookSpecRenderLayerDataToPass(RHICtx, ERenderLayer.RL_CustomTranslucent, mCamera, mHitProxySE, PrebuildPassIndex.PPI_HitProxy);
            //translucent render pass;
            mHitProxyView.CookSpecRenderLayerDataToPass(RHICtx, ERenderLayer.RL_Translucent, mCamera, mHitProxySE, PrebuildPassIndex.PPI_HitProxy);
            //gizmos pass;the coordinary should be rendered in this pass;
            mHitProxyView.CookSpecRenderLayerDataToPass(RHICtx, ERenderLayer.RL_Gizmos, mCamera, mHitProxyAxisSE, PrebuildPassIndex.PPI_HitProxy);

            var CmdList = mCLDB_HitProxy[0];
            CmdList.BeginCommand();
            CmdList.BeginRenderPass(mRenderPassDesc_HitProxy, mHitProxyView.FrameBuffer);
            mHitProxyView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Opaque);
            mHitProxyView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_CustomOpaque);
            mHitProxyView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_CustomTranslucent);
            mHitProxyView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Translucent);
            mHitProxyView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Gizmos);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();

            CmdList.CreateReadableTexture2D(ref HitProxyTex2D, mHitProxyView.FrameBuffer.GetSRV_RenderTarget(0), mHitProxyView.FrameBuffer);
            EngineNS.CEngine.Instance.GpuFetchManager.RegFetchTexture2D(HitProxyTex2D, (InSrv) =>
            {
                if (mHitProxyCPUTexture == null)
                {
                    mHitProxyCPUTexture = new CBlobObject();
                }
                if (CEngine.Instance.Desc.RHIType == ERHIType.RHT_OGL)
                {
                    var t1 = EngineNS.Support.Time.HighPrecision_GetTickCount();
                    unsafe
                    {
                        void* pData;
                        uint rowPitch;
                        uint depthPitch;
                        if (InSrv.Map(CEngine.Instance.RenderContext.ImmCommandList, 0, &pData, &rowPitch, &depthPitch))
                        {
                            lock (mHitProxyCPUTexture)
                            {
                                InSrv.BuildImageBlob(mHitProxyCPUTexture, pData, rowPitch);
                            }
                            InSrv.Unmap(CEngine.Instance.RenderContext.ImmCommandList, 0);
                        }
                        var t2 = EngineNS.Support.Time.HighPrecision_GetTickCount();
                        //System.Diagnostics.Debug.WriteLine($"Fetch hitproxy time : {t2 - t1}");
                    }
                }
                else
                {
                    CEngine.Instance.EventPoster.RunOn(() =>
                    {
                        var t1 = EngineNS.Support.Time.HighPrecision_GetTickCount();
                        unsafe
                        {
                            void* pData;
                            uint rowPitch;
                            uint depthPitch;
                            if (InSrv.Map(CEngine.Instance.RenderContext.ImmCommandList, 0, &pData, &rowPitch, &depthPitch))
                            {
                                lock (mHitProxyCPUTexture)
                                {
                                    InSrv.BuildImageBlob(mHitProxyCPUTexture, pData, rowPitch);
                                }
                                InSrv.Unmap(CEngine.Instance.RenderContext.ImmCommandList, 0);
                            }
                            var t2 = EngineNS.Support.Time.HighPrecision_GetTickCount();
                            //System.Diagnostics.Debug.WriteLine($"Fetch hitproxy time : {t2 - t1}");
                        }
                        return null;
                    }, Thread.Async.EAsyncTarget.AsyncEditor);
                }
            });

            CmdList.EndCommand();

            ScopeTickLogic.End();
        }

        CTexture2D HitProxyTex2D = null;
        public void TickRender()
        {
            if (mUpdateSelf == false || mEnabled == false)
            {
                return;
            }

            var RHICtx = EngineNS.CEngine.Instance.RenderContext;
            
            var CmdList = mCLDB_HitProxy[1];
            
            CmdList.Commit(RHICtx);

            mCpuTexHasData = true;
            //CEngine.Instance.HitProxyManager.HitProxyIsNewest = true;

        }

        public void TickSync()
        {
            if (mEnabled == false)
            {
                return;
            }

            if (mUpdateSelf == true)
            {
                var Temp = mCLDB_HitProxy[0];
                mCLDB_HitProxy[0] = mCLDB_HitProxy[1];
                mCLDB_HitProxy[1] = Temp;
            }

            mTakeElapseFrame--;
            if (mTakeElapseFrame >= 0)
            {
                mUpdateSelf = false;
            }
            else
            {
                mUpdateSelf = true;
                mTakeElapseFrame = 8;
            }
        }

        public void OnResize(UInt32 width, UInt32 height)
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;
            if (RHICtx == null)
            {
                return;
            }

            //mCamera.PerspectiveFovLH(mCamera.mDefaultFoV, (float)width, (float)height, -1, -1);

            mHitProxyView.OnResize(RHICtx, null, width, height);

            mViewportSizeX = width;
            mViewportSizeY = height;

            //var HitProxyTex = mHitProxyView.FrameBuffer.GetSRV_RenderTarget(0);
            //HitProxyTex.GetTexture2DData(RHICtx, mHitProxyCPUTexture, 0, (int)mViewportSizeX, (int)mViewportSizeY);
        }
        #endregion

    }
}
