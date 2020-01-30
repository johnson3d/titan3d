using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Graphics;
using EngineNS.Graphics.EnvShader;
using EngineNS.Graphics.View;

namespace EngineNS.Graphics.PostEffect
{
    
    public class CGfxMobileAO
    {
        private int DPLimitter = int.MaxValue;

        private const UInt32 mRTCut = 4;
        private CCommandList[] mCLDB_AoMask;
        private CCommandList[] mCLDB_BlurH;
        private CCommandList[] mCLDB_BlurV;
        
        public CGfxScreenView mView_AoMask;
        public CGfxScreenView mView_BlurH;
        //public CGfxScreenView mView_BlurV;
        
        private EngineNS.Graphics.Mesh.CGfxMesh mScreenAlignedTriangle;

        private CRenderPassDesc mRenderPassDesc_Ao;

        public CGfxMobileAoMaskSE mSE_AoMask;
        public CGfxMobileAoBlurHSE mSE_BlurH;
        public CGfxMobileAoBlurVSE mSE_BlurV;
        private List<CGfxShadingEnv> mMobileAoSEArrayMask = new List<CGfxShadingEnv>();


        private Vector4 mViewportSizeAndRcp = new Vector4();
        private Vector4 mAoParam = new Vector4();

        public float mRadius = 0.15f;
        public float mBias = 0.02f;
        public float mDarkness = 5.0f;

        public CGfxMobileAO(CRenderContext RHICtx)
        {
            EngineNS.CCommandListDesc CmdListDesc = new EngineNS.CCommandListDesc();

            mCLDB_AoMask = new CCommandList[2];
            mCLDB_AoMask[0] = RHICtx.CreateCommandList(CmdListDesc);
            mCLDB_AoMask[1] = RHICtx.CreateCommandList(CmdListDesc);
            mCLDB_AoMask[0].DebugName = "AoMask";
            mCLDB_AoMask[1].DebugName = "AoMask";

            mCLDB_BlurH = new CCommandList[2];
            mCLDB_BlurH[0] = RHICtx.CreateCommandList(CmdListDesc);
            mCLDB_BlurH[1] = RHICtx.CreateCommandList(CmdListDesc);
            mCLDB_BlurH[0].DebugName = "AoMask_BlurH";
            mCLDB_BlurH[1].DebugName = "AoMask_BlurH";

            mCLDB_BlurV = new CCommandList[2];
            mCLDB_BlurV[0] = RHICtx.CreateCommandList(CmdListDesc);
            mCLDB_BlurV[1] = RHICtx.CreateCommandList(CmdListDesc);
            mCLDB_BlurH[0].DebugName = "AoMask_BlurV";
            mCLDB_BlurH[1].DebugName = "AoMask_BlurV";
        }

        public void Cleanup()
        {
            mCLDB_AoMask[0].Cleanup();
            mCLDB_AoMask[0] = null;
            mCLDB_AoMask[1].Cleanup();
            mCLDB_AoMask[1] = null;

            mCLDB_BlurH[0].Cleanup();
            mCLDB_BlurH[0] = null;
            mCLDB_BlurH[1].Cleanup();
            mCLDB_BlurH[1] = null;

            mCLDB_BlurV[0].Cleanup();
            mCLDB_BlurV[0] = null;
            mCLDB_BlurV[1].Cleanup();
            mCLDB_BlurV[1] = null;
        }

        public async System.Threading.Tasks.Task<bool> Init(CRenderContext RHICtx, UInt32 width, UInt32 height, CGfxSceneView BaseSceneView)
        {
            if (RHICtx == null || BaseSceneView == null)
            {
                return false;
            }
            
            if (mSE_AoMask == null)
            {
                mSE_AoMask = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileAoMaskSE>();
            }
            if (mSE_BlurH == null)
            {
                mSE_BlurH = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileAoBlurHSE>();
            }
            if (mSE_BlurV == null)
            {
                mSE_BlurV = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileAoBlurVSE>();
            }
            mMobileAoSEArrayMask.Add(mSE_AoMask);
            mMobileAoSEArrayMask.Add(mSE_BlurV);

            var ScreenAlignedTriangle = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(RHICtx, CEngineDesc.ScreenAlignedTriangleName, true);
            var DefaultMtlInst = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(RHICtx, RName.GetRName("Material/defaultmaterial.instmtl"));
            mScreenAlignedTriangle = CEngine.Instance.MeshManager.CreateMesh(RHICtx, ScreenAlignedTriangle);
            mScreenAlignedTriangle.SetMaterialInstance(RHICtx, 0, DefaultMtlInst, CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);
            
            UInt32 Width = Math.Max(width, 64);
            UInt32 Height = Math.Max(height, 64);
            
            UInt32 Width4 = Width / mRTCut;
            UInt32 Height4 = Height / mRTCut;
            
            //ao mask pass;
            {
                CGfxScreenViewDesc VI_AoMask = new CGfxScreenViewDesc();
                VI_AoMask.UseDepthStencilView = false;
                VI_AoMask.Width = Width4;
                VI_AoMask.Height = Height4;

                CRenderTargetViewDesc RTVDesc0 = new CRenderTargetViewDesc();
                RTVDesc0.mCanBeSampled = vBOOL.FromBoolean(true);
                RTVDesc0.Format = EPixelFormat.PXF_R8_UNORM;
                RTVDesc0.Width = Width4;
                RTVDesc0.Height = Width4;
                VI_AoMask.mRTVDescArray.Add(RTVDesc0);

                mView_AoMask = new CGfxScreenView();
                if (await mView_AoMask.InitForMultiPassMode(RHICtx, VI_AoMask, mMobileAoSEArrayMask, DefaultMtlInst, mScreenAlignedTriangle) == false)
                {
                    return false;
                }
                //mSE_AoMask.mBaseSceneView = BaseSceneView.mFrameBuffer.GetSRV_RenderTarget(0);
            }

            //blur h pass;
            {
                CGfxScreenViewDesc VI_BlurH = new CGfxScreenViewDesc();
                VI_BlurH.UseDepthStencilView = false;
                VI_BlurH.Width = Width4;
                VI_BlurH.Height = Height4;

                CRenderTargetViewDesc RTVDesc0 = new CRenderTargetViewDesc();
                RTVDesc0.mCanBeSampled = vBOOL.FromBoolean(true);
                RTVDesc0.Format = EPixelFormat.PXF_R8_UNORM;
                RTVDesc0.Width = Width4;
                RTVDesc0.Height = Width4;
                VI_BlurH.mRTVDescArray.Add(RTVDesc0);

                mView_BlurH = new CGfxScreenView();
                if (await mView_BlurH.Init(RHICtx, null, VI_BlurH, mSE_BlurH, DefaultMtlInst, mScreenAlignedTriangle) == false)
                {
                    return false;
                }
                mSE_BlurH.mSRV_AoMask= mView_AoMask.FrameBuffer.GetSRV_RenderTarget(0);
            }

            ////blur v pass;
            //{
            //    CGfxScreenViewDesc VI_BlurV = new CGfxScreenViewDesc();
            //    VI_BlurV.UseDepthStencilView = false;
            //    VI_BlurV.Width = Width4;
            //    VI_BlurV.Height = Height4;

            //    CRenderTargetViewDesc RTVDesc0 = new CRenderTargetViewDesc();
            //    RTVDesc0.mCanBeSampled = vBOOL.FromBoolean(true);
            //    RTVDesc0.Format = EPixelFormat.PXF_R8_UNORM;
            //    RTVDesc0.Width = Width4;
            //    RTVDesc0.Height = Width4;
            //    VI_BlurV.mRTVDescArray.Add(RTVDesc0);

            //    mView_BlurV = new CGfxScreenView();
            //    if (await mView_BlurV.Init(RHICtx, null, VI_BlurV, mSE_BlurV, DefaultMtlInst, mScreenAlignedTriangle) == false)
            //    {
            //        return false;
            //    }
            //    mSE_BlurV.mSRV_Src = mView_BlurH.FrameBuffer.GetSRV_RenderTarget(0);
            //}

            mRenderPassDesc_Ao = new CRenderPassDesc();
            FrameBufferClearColor TempClearColor0 = new FrameBufferClearColor();
            TempClearColor0.r = 1.0f;
            TempClearColor0.g = 1.0f;
            TempClearColor0.b = 1.0f;
            TempClearColor0.a = 1.0f;

            mRenderPassDesc_Ao.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_Ao.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_Ao.mFBClearColorRT0 = TempClearColor0;
            mRenderPassDesc_Ao.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_Ao.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_Ao.mDepthClearValue = 1.0f;
            mRenderPassDesc_Ao.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_Ao.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_Ao.mStencilClearValue = 0u;
            
            mViewportSizeAndRcp = new Vector4(Width4, Height4, 1.0f / Width4, 1.0f / Height4);
            mView_AoMask.ViewportSizeAndRcp = mViewportSizeAndRcp;
            mView_BlurH.ViewportSizeAndRcp = mViewportSizeAndRcp;
            //mView_BlurV.ViewportSizeAndRcp = ViewportSizeAndRcp;
            

            return true;
        }
        
        public void TickLogic(CRenderContext RHICtx, CGfxCamera camera, CGfxSceneView BaseSceneView)
        {
            if (RHICtx == null)
            {
                System.Diagnostics.Debug.Assert(false);
                return;
            }
            
            if (CEngine.EnableMobileAo == true)
            {
                mAoParam.X = mRadius;
                mAoParam.Z = mBias;
                //float temp0 = 1.0f / (mRadius * mRadius * mRadius);
                //mDarkness = temp0 * temp0 * mDarkness;
                mAoParam.W = mDarkness;

                if (CEngine.Instance.RenderSystem.RHIType != ERHIType.RHT_OGL)
                {
                    mAoParam.Y = 1.0f;
                }
                else
                {
                    mAoParam.Y = -1.0f;
                }
                
                //ao mask pass
                var CmdList = mCLDB_AoMask[0];
                CmdList.BeginCommand();
                mSE_AoMask.mBaseSceneView = BaseSceneView.FrameBuffer.GetSRV_RenderTarget(0);
                mView_AoMask.AoParam = mAoParam;
                mView_AoMask.CookViewportMeshToPassInMultiPassMode(RHICtx, mSE_AoMask, 0, camera, mScreenAlignedTriangle);
                mView_AoMask.PushPassToRHIInMultiPassMode(CmdList, 0);
                CmdList.BeginRenderPass(mRenderPassDesc_Ao, mView_AoMask.FrameBuffer);
                CmdList.BuildRenderPass();
                CmdList.EndRenderPass();
                CmdList.EndCommand();

                //ao blur h;
                CmdList = mCLDB_BlurH[0];
                CmdList.BeginCommand();
                //mSE_BlurH.mSRV_AoMask = mView_AoMask.FrameBuffer.GetSRV_RenderTarget(0);
                mView_BlurH.CookViewportMeshToPass(RHICtx, mSE_BlurH, camera, mScreenAlignedTriangle);
                mView_BlurH.PushPassToRHI(CmdList);
                CmdList.BeginRenderPass(mRenderPassDesc_Ao, mView_BlurH.FrameBuffer);
                CmdList.BuildRenderPass();
                CmdList.EndRenderPass();
                CmdList.EndCommand();

                //ao blur v;
                CmdList = mCLDB_BlurV[0];
                CmdList.BeginCommand();
                mSE_BlurV.mSRV_Src = mView_BlurH.FrameBuffer.GetSRV_RenderTarget(0);
                mView_AoMask.CookViewportMeshToPassInMultiPassMode(RHICtx, mSE_BlurV, 1, camera, mScreenAlignedTriangle);
                mView_AoMask.PushPassToRHIInMultiPassMode(CmdList, 1);
                CmdList.BeginRenderPass(mRenderPassDesc_Ao, mView_AoMask.FrameBuffer);
                CmdList.BuildRenderPass();
                CmdList.EndRenderPass();
                CmdList.EndCommand();

                //CmdList = mCLDB_BlurV[0];
                //CmdList.BeginCommand();
                //mView_BlurV.CookViewportMeshToPass(RHICtx, mSE_BlurV, camera, mScreenAlignedTriangle);
                //mView_BlurV.PushPassToRHI(CmdList);
                //CmdList.BeginRenderPass(mRenderPassDesc_Ao, mView_BlurV.FrameBuffer);
                //CmdList.BuildRenderPass(ref DPLimitter);
                //CmdList.EndRenderPass();
                //CmdList.EndCommand();
            }
            else
            {
                //ao blur v;
                var CmdList = mCLDB_BlurV[0];
                CmdList.BeginCommand();
                CmdList.BeginRenderPass(mRenderPassDesc_Ao, mView_AoMask.FrameBuffer);
                CmdList.EndRenderPass();
                CmdList.EndCommand();
                //var CmdList = mCLDB_BlurV[0];
                //CmdList.BeginCommand();
                //CmdList.BeginRenderPass(mRenderPassDesc_Ao, mView_BlurV.FrameBuffer);
                //CmdList.EndRenderPass();
                //CmdList.EndCommand();
            }
            
        }
        public void TickRender(CRenderContext RHICtx)
        {
            if (RHICtx == null)
            {
                System.Diagnostics.Debug.Assert(false);
                return;
            }

            if (CEngine.EnableMobileAo == true)
            {
                var CmdList = mCLDB_AoMask[1];
                CmdList.Commit(RHICtx);

                CmdList = mCLDB_BlurH[1];
                CmdList.Commit(RHICtx);

                CmdList = mCLDB_BlurV[1];
                CmdList.Commit(RHICtx);
            }
            else
            {
                var CmdList = mCLDB_BlurV[1];
                CmdList.Commit(RHICtx);
            }
        }
        public void TickSync()
        {
            if (CEngine.EnableMobileAo == true)
            {
                var Temp = mCLDB_AoMask[0];
                mCLDB_AoMask[0] = mCLDB_AoMask[1];
                mCLDB_AoMask[1] = Temp;

                Temp = mCLDB_BlurH[0];
                mCLDB_BlurH[0] = mCLDB_BlurH[1];
                mCLDB_BlurH[1] = Temp;

                Temp = mCLDB_BlurV[0];
                mCLDB_BlurV[0] = mCLDB_BlurV[1];
                mCLDB_BlurV[1] = Temp;
            }
            else
            {
               var Temp = mCLDB_BlurV[0];
                mCLDB_BlurV[0] = mCLDB_BlurV[1];
                mCLDB_BlurV[1] = Temp;
            }
                
        }
        public  void OnResize(CRenderContext RHICtx, UInt32 width, UInt32 height, CGfxSceneView BaseSceneView)
        {
            if (RHICtx == null || mView_AoMask == null || mView_BlurH == null /*|| mView_BlurV == null*/)
            {
                System.Diagnostics.Debug.Assert(false);
                return;
            }
            
            UInt32 Width = Math.Max(width, 64);
            UInt32 Height = Math.Max(height, 64);
            
            UInt32 Width4 = Width / mRTCut;
            UInt32 Height4 = Height / mRTCut;
            
            mView_AoMask.OnResize(RHICtx, null, Width4, Height4);
            mView_BlurH.OnResize(RHICtx, null, Width4, Height4);
            //mView_BlurV.OnResize(RHICtx, null, Width4, Height4);

            //mSE_AoMask.mBaseSceneView = BaseSceneView.mFrameBuffer.GetSRV_RenderTarget(0);
            mSE_BlurH.mSRV_AoMask = mView_AoMask.FrameBuffer.GetSRV_RenderTarget(0);
            //mSE_BlurV.mSRV_Src = mView_BlurH.FrameBuffer.GetSRV_RenderTarget(0);

            mViewportSizeAndRcp = new Vector4(Width4, Height4, 1.0f / Width4, 1.0f / Height4);
            mView_AoMask.ViewportSizeAndRcp = mViewportSizeAndRcp;
            mView_BlurH.ViewportSizeAndRcp = mViewportSizeAndRcp;
            //mView_BlurV.ViewportSizeAndRcp = ViewportSizeAndRcp;
            
        }

    }

}