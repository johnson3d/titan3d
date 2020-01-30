using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Graphics;
using EngineNS.Graphics.EnvShader;
using EngineNS.Graphics.View;

namespace EngineNS.Graphics.PostEffect
{
    
    public class CGfxMobileBloom
    {
        private int DPLimitter = int.MaxValue;

        private CCommandList[] mCmdListDB_Bloom;
        
        public CGfxScreenView mView_BlurH8;
        public CGfxScreenView mView_BlurV8;

        public CGfxScreenView mDSView16;
        public CGfxScreenView mDSView32;
        public CGfxScreenView mDSView64;

        public CGfxScreenView mUSView32;
        public CGfxScreenView mUSView16;
        public CGfxScreenView mUSView8;
        
        private EngineNS.Graphics.Mesh.CGfxMesh mScreenAlignedTriangle;

        private CRenderPassDesc mRenderPassDesc_Bloom;
        
        public CGfxMobileBloomBlurHSE mSE_BlurH;
        public CGfxMobileBloomBlurVSE mSE_BlurV;
        public CGfxMobileBloomDSSE mDownSampSE;
        public CGfxMobileBloomUSSE mUpSampSE;
        
        public CGfxMobileBloom(CRenderContext RHICtx)
        {
            mCmdListDB_Bloom = new CCommandList[2];
            
            EngineNS.CCommandListDesc CmdListDesc = new EngineNS.CCommandListDesc();
            mCmdListDB_Bloom[0] = RHICtx.CreateCommandList(CmdListDesc);
            mCmdListDB_Bloom[1] = RHICtx.CreateCommandList(CmdListDesc);

            mCmdListDB_Bloom[0].DebugName = "Bloom";
            mCmdListDB_Bloom[1].DebugName = "Bloom";
        }

        public void Cleanup()
        {
            mCmdListDB_Bloom[0].Cleanup();
            mCmdListDB_Bloom[0] = null;
            mCmdListDB_Bloom[1].Cleanup();
            mCmdListDB_Bloom[1] = null;
        }

        public async System.Threading.Tasks.Task<bool> Init(CRenderContext RHICtx, UInt32 width, UInt32 height, CGfxSceneView BaseSceneView)
        {
            if (RHICtx == null || BaseSceneView == null)
            {
                return false;
            }
            
            if (mSE_BlurH == null)
            {
                mSE_BlurH = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileBloomBlurHSE>();
            }
            if (mSE_BlurV == null)
            {
                mSE_BlurV = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileBloomBlurVSE>();
            }

            if (mDownSampSE == null)
            {
                mDownSampSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileBloomDSSE>();
            }

            if (mUpSampSE == null)
            {
                mUpSampSE = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileBloomUSSE>();
            }

            var ScreenAlignedTriangle = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(RHICtx, CEngineDesc.ScreenAlignedTriangleName, true);
            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(RHICtx, RName.GetRName("Material/defaultmaterial.instmtl"));
            mScreenAlignedTriangle = CEngine.Instance.MeshManager.CreateMesh(RHICtx, ScreenAlignedTriangle);
            mScreenAlignedTriangle.SetMaterialInstance(RHICtx, 0, mtl, CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);
            //await mScreenAlignedTriangle.AwaitEffects();

            UInt32 Width = Math.Max(width, 64);
            UInt32 Height = Math.Max(height, 64);
            
            UInt32 Width8 = Width / 8;
            UInt32 Height8 = Height / 8;

            UInt32 Width16 = Width / 16;
            UInt32 Height16 = Height / 16;

            UInt32 Width32 = Width / 32;
            UInt32 Height32 = Height / 32;

            UInt32 Width64 = Width / 64;
            UInt32 Height64 = Height / 64;

            EPixelFormat chooseFormat = EPixelFormat.PXF_R8G8B8A8_UNORM; //EPixelFormat.PXF_R11G11B10_FLOAT;
            if (RHICtx.ContextCaps.SupportHalfRT == 0)
            {
                chooseFormat = EPixelFormat.PXF_R8G8B8A8_UNORM;
            }
            else
            {
                chooseFormat = EPixelFormat.PXF_R16G16B16A16_FLOAT;
            }
            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Graphics", $"MobileBloom render texture format: {chooseFormat}");
            //blur h pass;
            {
                CGfxScreenViewDesc VI_BlurH = new CGfxScreenViewDesc();
                VI_BlurH.UseDepthStencilView = false;
                VI_BlurH.Width = Width8;
                VI_BlurH.Height = Height8;

                CRenderTargetViewDesc RTVDesc0 = new CRenderTargetViewDesc();
                RTVDesc0.mCanBeSampled = vBOOL.FromBoolean(true);
                RTVDesc0.Format = chooseFormat;
                RTVDesc0.Width = Width8;
                RTVDesc0.Height = Height8;
                VI_BlurH.mRTVDescArray.Add(RTVDesc0);

                mView_BlurH8 = new CGfxScreenView();
                if (await mView_BlurH8.Init(RHICtx, null, VI_BlurH, mSE_BlurH, mtl, mScreenAlignedTriangle) == false)
                {
                    return false;
                }
                mSE_BlurH.mBaseSceneView = BaseSceneView.FrameBuffer.GetSRV_RenderTarget(0);
            }
            
            //blur v pass;
            {
                CGfxScreenViewDesc VI_BlurV = new CGfxScreenViewDesc();
                VI_BlurV.UseDepthStencilView = false;
                VI_BlurV.Width = Width8;
                VI_BlurV.Height = Height8;

                var RTVDesc0 = new EngineNS.CRenderTargetViewDesc();
                RTVDesc0.mCanBeSampled = vBOOL.FromBoolean(true);
                RTVDesc0.Format = chooseFormat;
                RTVDesc0.Width = Width8;
                RTVDesc0.Height = Height8;
                VI_BlurV.mRTVDescArray.Add(RTVDesc0);

                mView_BlurV8 = new CGfxScreenView();
                if (await mView_BlurV8.Init(RHICtx, null, VI_BlurV, mSE_BlurV, mtl, mScreenAlignedTriangle) == false)
                {
                    return false;
                }
                mSE_BlurV.mSrcTex = mView_BlurH8.FrameBuffer.GetSRV_RenderTarget(0);
            }

            //ds16;
            {
                CGfxScreenViewDesc DSViewInfo16 = new CGfxScreenViewDesc();
                DSViewInfo16.UseDepthStencilView = false;
                DSViewInfo16.Width = Width16;
                DSViewInfo16.Height = Height16;

                var RTVDesc0 = new EngineNS.CRenderTargetViewDesc();
                RTVDesc0.mCanBeSampled = vBOOL.FromBoolean(true);
                RTVDesc0.Format = chooseFormat;
                RTVDesc0.Width = Width16;
                RTVDesc0.Height = Height16;
                DSViewInfo16.mRTVDescArray.Add(RTVDesc0);

                mDSView16 = new CGfxScreenView();
                if (await mDSView16.Init(RHICtx, null, DSViewInfo16, mDownSampSE, mtl, mScreenAlignedTriangle) == false)
                {
                    return false;
                }
            }

            //ds32;
            {
                CGfxScreenViewDesc DSViewInfo32 = new CGfxScreenViewDesc();
                DSViewInfo32.UseDepthStencilView = false;
                DSViewInfo32.Width = Width32;
                DSViewInfo32.Height = Height32;

                var RTVDesc0 = new EngineNS.CRenderTargetViewDesc();
                RTVDesc0.mCanBeSampled = vBOOL.FromBoolean(true);
                RTVDesc0.Format = chooseFormat;
                RTVDesc0.Width = Width32;
                RTVDesc0.Height = Height32;
                DSViewInfo32.mRTVDescArray.Add(RTVDesc0);

                mDSView32 = new CGfxScreenView();
                if (await mDSView32.Init(RHICtx, null, DSViewInfo32, mDownSampSE, mtl, mScreenAlignedTriangle) == false)
                {
                    return false;
                }
            }

            //ds64;
            {
                CGfxScreenViewDesc DSViewInfo64 = new CGfxScreenViewDesc();
                DSViewInfo64.UseDepthStencilView = false;
                DSViewInfo64.Width = Width64;
                DSViewInfo64.Height = Height64;

                var RTVDesc0 = new EngineNS.CRenderTargetViewDesc();
                RTVDesc0.mCanBeSampled = vBOOL.FromBoolean(true);
                RTVDesc0.Format = chooseFormat;
                RTVDesc0.Width = Width64;
                RTVDesc0.Height = Height64;
                DSViewInfo64.mRTVDescArray.Add(RTVDesc0);

                mDSView64 = new CGfxScreenView();
                if (await mDSView64.Init(RHICtx, null, DSViewInfo64, mDownSampSE, mtl, mScreenAlignedTriangle) == false)
                {
                    return false;
                }
            }

            //us32;
            {
                CGfxScreenViewDesc USViewInfo32 = new CGfxScreenViewDesc();
                USViewInfo32.UseDepthStencilView = false;
                USViewInfo32.Width = Width32;
                USViewInfo32.Height = Height32;

                var RTVDesc0 = new CRenderTargetViewDesc();
                RTVDesc0.mCanBeSampled = vBOOL.FromBoolean(true);
                RTVDesc0.Format = chooseFormat;
                RTVDesc0.Width = Width32;
                RTVDesc0.Height = Height32;
                USViewInfo32.mRTVDescArray.Add(RTVDesc0);

                mUSView32 = new CGfxScreenView();
                if (await mUSView32.Init(RHICtx, null, USViewInfo32, mUpSampSE, mtl, mScreenAlignedTriangle) == false)
                {
                    return false;
                }
            }

            //us16;
            {
                CGfxScreenViewDesc USViewInfo16 = new CGfxScreenViewDesc();
                USViewInfo16.UseDepthStencilView = false;
                USViewInfo16.Width = Width16;
                USViewInfo16.Height = Height16;

                var RTVDesc0 = new CRenderTargetViewDesc();
                RTVDesc0.mCanBeSampled = vBOOL.FromBoolean(true);
                RTVDesc0.Format = chooseFormat;
                RTVDesc0.Width = Width16;
                RTVDesc0.Height = Height16;
                USViewInfo16.mRTVDescArray.Add(RTVDesc0);

                mUSView16 = new CGfxScreenView();
                if (await mUSView16.Init(RHICtx, null, USViewInfo16, mUpSampSE, mtl, mScreenAlignedTriangle) == false)
                {
                    return false;
                }
            }

            //us8;
            {
                CGfxScreenViewDesc USViewInfo8 = new CGfxScreenViewDesc();
                USViewInfo8.UseDepthStencilView = false;
                USViewInfo8.Width = Width8;
                USViewInfo8.Height = Height8;

                var RTVDesc0 = new CRenderTargetViewDesc();
                RTVDesc0.mCanBeSampled = vBOOL.FromBoolean(true);
                RTVDesc0.Format = chooseFormat;
                RTVDesc0.Width = Width8;
                RTVDesc0.Height = Height8;
                USViewInfo8.mRTVDescArray.Add(RTVDesc0);

                mUSView8 = new CGfxScreenView();
                if (await mUSView8.Init(RHICtx, null, USViewInfo8, mUpSampSE, mtl, mScreenAlignedTriangle) == false)
                {
                    return false;
                }
            }

            mRenderPassDesc_Bloom = new CRenderPassDesc();
            FrameBufferClearColor TempClearColor0 = new FrameBufferClearColor();
            TempClearColor0.r = 0.0f;
            TempClearColor0.g = 0.0f;
            TempClearColor0.b = 0.0f;
            TempClearColor0.a = 0.0f;

            mRenderPassDesc_Bloom.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_Bloom.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_Bloom.mFBClearColorRT0 = TempClearColor0;
            mRenderPassDesc_Bloom.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_Bloom.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_Bloom.mDepthClearValue = 1.0f;
            mRenderPassDesc_Bloom.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_Bloom.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_Bloom.mStencilClearValue = 0u;
            
            {
                var ViewportSizeAndRcpBlurH = new Vector4(width, height, 1.0f / width, 1.0f / height);
                mView_BlurH8.ViewportSizeAndRcp = ViewportSizeAndRcpBlurH;
            }
            {
                var ViewportSizeAndRcpBlurV = new Vector4(Width8, Height8, 1.0f / Width8, 1.0f / Height8);
                mView_BlurV8.ViewportSizeAndRcp = ViewportSizeAndRcpBlurV;
            }
            //down
            {
                var ViewportSizeAndRcpD16 = new Vector4(Width8, Height8, 1.0f / Width8, 1.0f / Height8);
                mDSView16.ViewportSizeAndRcp = ViewportSizeAndRcpD16;
            }
            {
                var ViewportSizeAndRcpD32 = new Vector4(Width16, Height16, 1.0f / Width16, 1.0f / Height16);
                mDSView32.ViewportSizeAndRcp = ViewportSizeAndRcpD32;
            }
            {
                var ViewportSizeAndRcpD64 = new Vector4(Width32, Height32, 1.0f / Width32, 1.0f / Height32);
                mDSView64.ViewportSizeAndRcp = ViewportSizeAndRcpD64;
            }
            //up
            {
                var ViewportRcpU32 = new Vector4(1.0f / Width32, 1.0f / Height32, 1.0f / Width64, 1.0f / Height64);
                mUSView32.ViewportSizeAndRcp = ViewportRcpU32;
            }
            {
                var ViewportRcpU16 = new Vector4(1.0f / Width16, 1.0f / Height16, 1.0f / Width32, 1.0f / Height32);
                mUSView16.ViewportSizeAndRcp = ViewportRcpU16;
            }
            {
                var ViewportRcpU8 = new Vector4(1.0f / Width8, 1.0f / Height8, 1.0f / Width16, 1.0f / Height16);
                mUSView8.ViewportSizeAndRcp = ViewportRcpU8;
            }
            
            return true;
        }

        public static Profiler.TimeScope ScopeTickLogic = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxMobileBloom), nameof(TickLogic));
        public void TickLogic(CRenderContext RHICtx, CGfxCamera camera)
        {
            if (CEngine.EnableBloom == false)
                return;
            if (RHICtx == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"BloomError!!!", "");
                return;
            }

            ScopeTickLogic.Begin();
            //blur h pass
            var CmdList = mCmdListDB_Bloom[0];
            CmdList.BeginCommand();
            CmdList.BeginRenderPass(mRenderPassDesc_Bloom, mView_BlurH8.FrameBuffer);
            mView_BlurH8.CookViewportMeshToPass(RHICtx, mSE_BlurH, camera, mScreenAlignedTriangle);
            mView_BlurH8.PushPassToRHI(CmdList);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();
            
            //blur v pass
            CmdList.BeginRenderPass(mRenderPassDesc_Bloom, mView_BlurV8.FrameBuffer);
            mView_BlurV8.CookViewportMeshToPass(RHICtx, mSE_BlurV, camera, mScreenAlignedTriangle);
            mView_BlurV8.PushPassToRHI(CmdList);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();

            //d16 pass
            CmdList.BeginRenderPass(mRenderPassDesc_Bloom, mDSView16.FrameBuffer);
            mDownSampSE.mSrcTex = mView_BlurV8.FrameBuffer.GetSRV_RenderTarget(0);
            mDSView16.CookViewportMeshToPass(RHICtx, mDownSampSE, camera, mScreenAlignedTriangle);
            mDSView16.PushPassToRHI(CmdList);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();

            //d32 pass
            CmdList.BeginRenderPass(mRenderPassDesc_Bloom, mDSView32.FrameBuffer);
            mDownSampSE.mSrcTex = mDSView16.FrameBuffer.GetSRV_RenderTarget(0);
            mDSView32.CookViewportMeshToPass(RHICtx, mDownSampSE, camera, mScreenAlignedTriangle);
            mDSView32.PushPassToRHI(CmdList);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();
            
            //d64 pass
            CmdList.BeginRenderPass(mRenderPassDesc_Bloom, mDSView64.FrameBuffer);
            mDownSampSE.mSrcTex = mDSView32.FrameBuffer.GetSRV_RenderTarget(0);
            mDSView64.CookViewportMeshToPass(RHICtx, mDownSampSE, camera, mScreenAlignedTriangle);
            mDSView64.PushPassToRHI(CmdList);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();
            
            //u32 pass
            CmdList.BeginRenderPass(mRenderPassDesc_Bloom, mUSView32.FrameBuffer);
            mUpSampSE.mSrcTexUp = mDSView32.FrameBuffer.GetSRV_RenderTarget(0);
            mUpSampSE.mSrcTexDown = mDSView64.FrameBuffer.GetSRV_RenderTarget(0);
            mUSView32.CookViewportMeshToPass(RHICtx, mUpSampSE, camera, mScreenAlignedTriangle);
            mUSView32.PushPassToRHI(CmdList);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();
            
            //u16 pass
            CmdList.BeginRenderPass(mRenderPassDesc_Bloom, mUSView16.FrameBuffer);
            mUpSampSE.mSrcTexUp = mDSView16.FrameBuffer.GetSRV_RenderTarget(0);
            mUpSampSE.mSrcTexDown = mUSView32.FrameBuffer.GetSRV_RenderTarget(0);
            mUSView16.CookViewportMeshToPass(RHICtx, mUpSampSE, camera, mScreenAlignedTriangle);
            mUSView16.PushPassToRHI(CmdList);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();
            
            //u8 pass
            CmdList.BeginRenderPass(mRenderPassDesc_Bloom, mUSView8.FrameBuffer);
            mUpSampSE.mSrcTexUp = mView_BlurV8.FrameBuffer.GetSRV_RenderTarget(0);
            mUpSampSE.mSrcTexDown = mUSView16.FrameBuffer.GetSRV_RenderTarget(0);
            mUSView8.CookViewportMeshToPass(RHICtx, mUpSampSE, camera, mScreenAlignedTriangle);
            mUSView8.PushPassToRHI(CmdList);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();
            CmdList.EndCommand();

            ScopeTickLogic.End();
        }
        public void TickRender(CRenderContext RHICtx)
        {
            if (RHICtx == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"BloomError!!!", "");
                return;
            }

            var CmdList = mCmdListDB_Bloom[1];
            CmdList.Commit(RHICtx);
        }
        public void TickSync()
        {
            var Temp = mCmdListDB_Bloom[0];
            mCmdListDB_Bloom[0] = mCmdListDB_Bloom[1];
            mCmdListDB_Bloom[1] = Temp;
        }
        public  void OnResize(CRenderContext RHICtx, UInt32 width, UInt32 height, CGfxSceneView BaseSceneView)
        {
            if (RHICtx == null || mView_BlurH8 == null || mView_BlurV8 == null || mDSView16 == null || mDSView32 == null || mDSView64 == null || mUSView32 == null
                || mUSView16 == null || mUSView8 == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"BloomError!!!", "");
                return;
            }
            
            UInt32 Width = Math.Max(width, 64);
            UInt32 Height = Math.Max(height, 64);
            
            UInt32 Width8 = Width / 8;
            UInt32 Height8 = Height / 8;

            UInt32 Width16 = Width / 16;
            UInt32 Height16 = Height / 16;

            UInt32 Width32 = Width / 32;
            UInt32 Height32 = Height / 32;

            UInt32 Width64 = Width / 64;
            UInt32 Height64 = Height / 64;
            
            mView_BlurH8.OnResize(RHICtx, null, Width8, Height8);
            mView_BlurV8.OnResize(RHICtx, null, Width8, Height8);
            //down
            mDSView16.OnResize(RHICtx, null, Width16, Height16);
            mDSView32.OnResize(RHICtx, null, Width32, Height32);
            mDSView64.OnResize(RHICtx, null, Width64, Height64);
            //up;
            mUSView32.OnResize(RHICtx, null, Width32, Height32);
            mUSView16.OnResize(RHICtx, null, Width16, Height16);
            mUSView8.OnResize(RHICtx, null, Width8, Height8);
            
            mSE_BlurH.mBaseSceneView = BaseSceneView.FrameBuffer.GetSRV_RenderTarget(0);
            mSE_BlurV.mSrcTex = mView_BlurH8.FrameBuffer.GetSRV_RenderTarget(0);

            //blur h
            {
                var ViewportSizeAndRcpBlurH = new Vector4(Width, Height, 1.0f / Width, 1.0f / Height);
                mView_BlurH8.ViewportSizeAndRcp = ViewportSizeAndRcpBlurH;
            }
            //blur v
            {
                var ViewportSizeAndRcpBlurV = new Vector4(Width8, Height8, 1.0f / Width8, 1.0f / Height8);
                mView_BlurV8.ViewportSizeAndRcp = ViewportSizeAndRcpBlurV;
            }
            {
                var ViewportSizeAndRcpD16 = new Vector4(Width8, Height8, 1.0f / Width8, 1.0f / Height8);
                mDSView16.ViewportSizeAndRcp = ViewportSizeAndRcpD16;
            }
            {
                var ViewportSizeAndRcpD32 = new Vector4(Width16, Height16, 1.0f / Width16, 1.0f / Height16);
                mDSView32.ViewportSizeAndRcp = ViewportSizeAndRcpD32;
            }
            {
                var ViewportSizeAndRcpD64 = new Vector4(Width32, Height32, 1.0f / Width32, 1.0f / Height32);
                mDSView64.ViewportSizeAndRcp = ViewportSizeAndRcpD64;
            }

            //up
            {
                var ViewportRcpU32 = new Vector4(1.0f / Width32, 1.0f / Height32, 1.0f / Width64, 1.0f / Height64);
                mUSView32.ViewportSizeAndRcp = ViewportRcpU32;
            }
            {
                var ViewportRcpU16 = new Vector4(1.0f / Width16, 1.0f / Height16, 1.0f / Width32, 1.0f / Height32);
                mUSView16.ViewportSizeAndRcp = ViewportRcpU16;
            }
            {
                var ViewportRcpU8 = new Vector4(1.0f / Width8, 1.0f / Height8, 1.0f / Width16, 1.0f / Height16);
                mUSView8.ViewportSizeAndRcp = ViewportRcpU8;
            }
        }
        
    }

}