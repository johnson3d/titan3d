using System;
using System.Collections.Generic;
using EngineNS.Graphics;
using EngineNS.Graphics.EnvShader;
using EngineNS.Graphics.View;

namespace EngineNS.Graphics.PostEffect
{
    public class CGfxSunShaftMobile
    {
        private int DPLimitter = int.MaxValue;

        private CCommandList[] mCLDB_SunShaft;
        //this command buffer is to diable dx11 deffered ctx warning;
        private CCommandList[] mCLDB_DisableWarning0;
        private CCommandList[] mCLDB_DisableWarning1;

        public CGfxScreenView mView_Mask;
        public CGfxScreenView mView_Blur;
        
        private EngineNS.Graphics.Mesh.CGfxMesh mViewportMesh;
        private CRenderPassDesc mRenderPassDesc_SunShaft;

        private CGfxMobileSunShaftMaskSE mSE_Mask;
        private CGfxMobileSunShaftBlurSE mSE_Blur;
        private List<CGfxShadingEnv> mSunShaftSEArrayMask = new List<CGfxShadingEnv>();
        private List<CGfxShadingEnv> mSunShaftSEArrayBlur = new List<CGfxShadingEnv>();

        public Vector4 mSunPosNdc = new Vector4();
        private float mSunDistance = 10000.0f;
        public float mSunShaftAtten = 1.0f;
        private float mSunPosNdcMin = -1.5f;
        private float mSunPosNdcMax = 1.5f;
        public bool mStopSunShaftUpdate = false;

        public CGfxSunShaftMobile(CRenderContext RHICtx)
        {
            mCLDB_SunShaft = new CCommandList[2];

            EngineNS.CCommandListDesc CmdListDesc = new EngineNS.CCommandListDesc();
            mCLDB_SunShaft[0] = RHICtx.CreateCommandList(CmdListDesc);
            mCLDB_SunShaft[1] = RHICtx.CreateCommandList(CmdListDesc);
            mCLDB_SunShaft[0].DebugName = "SunShaft";
            mCLDB_SunShaft[1].DebugName = "SunShaft";

            mCLDB_DisableWarning0 = new CCommandList[2];
            mCLDB_DisableWarning0[0] = RHICtx.CreateCommandList(CmdListDesc);
            mCLDB_DisableWarning0[1] = RHICtx.CreateCommandList(CmdListDesc);
            mCLDB_DisableWarning0[0].DebugName = "SunShaft_DisableWarning0";
            mCLDB_DisableWarning0[1].DebugName = "SunShaft_DisableWarning0";

            mCLDB_DisableWarning1 = new CCommandList[2];
            mCLDB_DisableWarning1[0] = RHICtx.CreateCommandList(CmdListDesc);
            mCLDB_DisableWarning1[1] = RHICtx.CreateCommandList(CmdListDesc);
            mCLDB_DisableWarning1[0].DebugName = "SunShaft_DisableWarning1";
            mCLDB_DisableWarning1[1].DebugName = "SunShaft_DisableWarning1";
        }

        public void Cleanup()
        {
            mCLDB_SunShaft[0].Cleanup();
            mCLDB_SunShaft[0] = null;
            mCLDB_SunShaft[1].Cleanup();
            mCLDB_SunShaft[1] = null;

            mCLDB_DisableWarning0[0].Cleanup();
            mCLDB_DisableWarning0[0] = null;
            mCLDB_DisableWarning0[1].Cleanup();
            mCLDB_DisableWarning0[1] = null;

            mCLDB_DisableWarning1[0].Cleanup();
            mCLDB_DisableWarning1[0] = null;
            mCLDB_DisableWarning1[1].Cleanup();
            mCLDB_DisableWarning1[1] = null;
        }

        public async System.Threading.Tasks.Task<bool> Init(CRenderContext RHICtx, UInt32 width, UInt32 height)
        {
            if (RHICtx == null)
            {
                return false;
            }

            if (mSE_Mask == null)
            {
                mSE_Mask = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileSunShaftMaskSE>();
            }

            if (mSE_Blur == null)
            {
                mSE_Blur = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileSunShaftBlurSE>();
            }
            mSunShaftSEArrayMask.Add(mSE_Mask);
            mSunShaftSEArrayMask.Add(mSE_Blur);

            mSunShaftSEArrayBlur.Add(mSE_Blur);
            mSunShaftSEArrayBlur.Add(mSE_Blur);

            var ScreenAlignedTriangle = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(RHICtx, CEngineDesc.ScreenAlignedTriangleName, true);
            var DefaultMtlInst = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(RHICtx, RName.GetRName("Material/defaultmaterial.instmtl"));
            mViewportMesh = CEngine.Instance.MeshManager.CreateMesh(RHICtx, ScreenAlignedTriangle);
            mViewportMesh.SetMaterialInstance(RHICtx, 0, DefaultMtlInst, CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);
            //await mViewportMesh.AwaitEffects();

            UInt32 Width = Math.Max(width, 4);
            UInt32 Height = Math.Max(height, 4);

            UInt32 Width4 = Width / 4;
            UInt32 Height4 = Height / 4;

            //mask view
            {
                CGfxScreenViewDesc VD_Mask = new CGfxScreenViewDesc();
                VD_Mask.UseDepthStencilView = false;
                VD_Mask.Width = Width4;
                VD_Mask.Height = Height4;

                CRenderTargetViewDesc RTVDesc_Mask = new CRenderTargetViewDesc();
                RTVDesc_Mask.mCanBeSampled = vBOOL.FromBoolean(true);
                RTVDesc_Mask.Format = EPixelFormat.PXF_R8G8_UNORM;
                RTVDesc_Mask.Width = Width4;
                RTVDesc_Mask.Height = Height4;
                VD_Mask.mRTVDescArray.Add(RTVDesc_Mask);

                mView_Mask = new CGfxScreenView();
                if (await mView_Mask.InitForMultiPassMode(RHICtx, VD_Mask, mSunShaftSEArrayMask, DefaultMtlInst, mViewportMesh) == false)
                {
                    System.Diagnostics.Debug.Assert(false);
                    return false;
                }
                
            }

            //blur0 view
            {
                CGfxScreenViewDesc VD_Blur = new CGfxScreenViewDesc();
                VD_Blur.UseDepthStencilView = false;
                VD_Blur.Width = Width4;
                VD_Blur.Height = Height4;

                CRenderTargetViewDesc RTVDesc_Blur = new CRenderTargetViewDesc();
                RTVDesc_Blur.mCanBeSampled = vBOOL.FromBoolean(true);
                RTVDesc_Blur.Format = EPixelFormat.PXF_R8G8_UNORM;
                RTVDesc_Blur.Width = Width4;
                RTVDesc_Blur.Height = Height4;
                VD_Blur.mRTVDescArray.Add(RTVDesc_Blur);

                mView_Blur = new CGfxScreenView();
                if (await mView_Blur.InitForMultiPassMode(RHICtx, VD_Blur, mSunShaftSEArrayBlur, DefaultMtlInst, mViewportMesh) == false)
                {
                    System.Diagnostics.Debug.Assert(false);
                    return false;
                }
            }

            mRenderPassDesc_SunShaft = new CRenderPassDesc();
            FrameBufferClearColor TempClearColor0 = new FrameBufferClearColor();
            TempClearColor0.r = 0.0f;
            TempClearColor0.g = 0.0f;
            TempClearColor0.b = 0.0f;
            TempClearColor0.a = 0.0f;

            mRenderPassDesc_SunShaft.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_SunShaft.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_SunShaft.mFBClearColorRT0 = TempClearColor0;
            mRenderPassDesc_SunShaft.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_SunShaft.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_SunShaft.mDepthClearValue = 1.0f;
            mRenderPassDesc_SunShaft.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_SunShaft.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_SunShaft.mStencilClearValue = 0u;
            
            return true;
        }

        static float Clamp(float X, float Min, float Max)
        {
            return X < Min ? Min : X < Max ? X : Max;
        }
        public static Profiler.TimeScope ScopeTickLogic = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxSunShaftMobile), nameof(TickLogic));
        public void TickLogic(CRenderContext RHICtx, CGfxCamera camera, Vector3 SunDir, CGfxSceneView BaseSceneView)
        {
            TickLogic_Impl(RHICtx, camera, SunDir, BaseSceneView);

            mSunPosNdc.Z = 0.25f * mSunShaftAtten;
            if (mStopSunShaftUpdate == true)
            {
                //disable sun shaft;
                mSunPosNdc.W = 0.0f;
            }
            else
            {
                mSunPosNdc.W = 1.0f;
            }
        }
        private void TickLogic_Impl(CRenderContext RHICtx, CGfxCamera camera, Vector3 SunDir, CGfxSceneView BaseSceneView)
        {
            if (CEngine.EnableSunShaft == false)
            {
                mStopSunShaftUpdate = true;
                return;
            }
            if (RHICtx == null)
            {
                return;
            }

            ScopeTickLogic.Begin();
            mSunPosNdc.X = -SunDir.X * mSunDistance;
            mSunPosNdc.Y = -SunDir.Y * mSunDistance;
            mSunPosNdc.Z = -SunDir.Z * mSunDistance;
            mSunPosNdc.W = 1.0f;
            
            mSunPosNdc = Vector4.Transform(mSunPosNdc, camera.CameraData.ViewProjection);
            mSunPosNdc.X = Clamp(mSunPosNdc.X / mSunPosNdc.W, mSunPosNdcMin, mSunPosNdcMax);
            mSunPosNdc.Y = Clamp(mSunPosNdc.Y / mSunPosNdc.W, mSunPosNdcMin, mSunPosNdcMax);
            float AttenOffset = Clamp(Math.Abs(mSunPosNdc.X) - 1.0f, 0.0f, 0.5f) + Clamp(Math.Abs(mSunPosNdc.Y) - 1.0f, 0.0f, 0.5f);
            mSunShaftAtten = Clamp(Vector3.Dot(-1.0f * camera.CameraData.Direction, SunDir) * 2.0f - AttenOffset, 0.0f, 1.0f);
            if (mSunShaftAtten <= 0.0f)
            {
                mStopSunShaftUpdate = true;
                return;
            }
            else
            {
                mStopSunShaftUpdate = false;
            }
            
            if (CEngine.Instance.RenderSystem.RHIType != ERHIType.RHT_OGL)
            {
                mSunPosNdc.W = 1.0f;
            }
            else
            {
                mSunPosNdc.W = -1.0f;
            }
            
            var CmdList = mCLDB_SunShaft[0];
            CmdList.BeginCommand();
            //generate mask;
            mSE_Mask.mBaseSceneView = BaseSceneView.FrameBuffer.GetSRV_RenderTarget(0);
            mView_Mask.CookViewportMeshToPassInMultiPassMode(RHICtx, mSE_Mask, 0, camera, mViewportMesh);
            mView_Mask.PushPassToRHIInMultiPassMode(CmdList, 0);
            CmdList.BeginRenderPass(mRenderPassDesc_SunShaft, mView_Mask.FrameBuffer);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();

            //blur0;
            mSunPosNdc.Z = 0.2f;
            mView_Blur.SunPosNDC = mSunPosNdc;
            mSE_Blur.mSunShaftMask = mView_Mask.FrameBuffer.GetSRV_RenderTarget(0);
            mView_Blur.CookViewportMeshToPassInMultiPassMode(RHICtx, mSE_Blur, 0, camera, mViewportMesh);
            mView_Blur.PushPassToRHIInMultiPassMode(CmdList, 0);
            CmdList.BeginRenderPass(mRenderPassDesc_SunShaft, mView_Blur.FrameBuffer);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();

            CmdList.EndCommand();

            CmdList = mCLDB_DisableWarning0[0];
            CmdList.BeginCommand();
            //blur1;
            mSunPosNdc.Z = 0.4f;
            mView_Mask.SunPosNDC = mSunPosNdc;
            mSE_Blur.mSunShaftMask = mView_Blur.FrameBuffer.GetSRV_RenderTarget(0);
            mView_Mask.CookViewportMeshToPassInMultiPassMode(RHICtx, mSE_Blur, 1, camera, mViewportMesh);
            mView_Mask.PushPassToRHIInMultiPassMode(CmdList, 1);
            CmdList.BeginRenderPass(mRenderPassDesc_SunShaft, mView_Mask.FrameBuffer);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();

            CmdList.EndCommand();

            CmdList = mCLDB_DisableWarning1[0];
            CmdList.BeginCommand();
            //blur2;
            mSunPosNdc.Z = 0.7f;
            mView_Blur.SunPosNDC = mSunPosNdc;
            mSE_Blur.mSunShaftMask = mView_Mask.FrameBuffer.GetSRV_RenderTarget(0);
            mView_Blur.CookViewportMeshToPassInMultiPassMode(RHICtx, mSE_Blur, 1, camera, mViewportMesh);
            mView_Blur.PushPassToRHIInMultiPassMode(CmdList, 1);
            CmdList.BeginRenderPass(mRenderPassDesc_SunShaft, mView_Blur.FrameBuffer);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();

            CmdList.EndCommand();

            ScopeTickLogic.End();
        }

        public void TickRender(CRenderContext RHICtx)
        {
            if (RHICtx == null)
            {
                System.Diagnostics.Debug.Assert(false);
                return;
            }

            if (mStopSunShaftUpdate == true)
            {
                return;
            }

            var CmdList = mCLDB_SunShaft[1];
            CmdList.Commit(RHICtx);
            CmdList = mCLDB_DisableWarning0[1];
            CmdList.Commit(RHICtx);
            CmdList = mCLDB_DisableWarning1[1];
            CmdList.Commit(RHICtx);

        }

        public void TickSync()
        {
            if (mStopSunShaftUpdate == true)
            {
                return;
            }

            var Temp = mCLDB_SunShaft[0];
            mCLDB_SunShaft[0] = mCLDB_SunShaft[1];
            mCLDB_SunShaft[1] = Temp;

            Temp = mCLDB_DisableWarning0[0];
            mCLDB_DisableWarning0[0] = mCLDB_DisableWarning0[1];
            mCLDB_DisableWarning0[1] = Temp;

            Temp = mCLDB_DisableWarning1[0];
            mCLDB_DisableWarning1[0] = mCLDB_DisableWarning1[1];
            mCLDB_DisableWarning1[1] = Temp;
        }

        public void OnResize(CRenderContext RHICtx, UInt32 width, UInt32 height)
        {
            if (RHICtx == null || mView_Mask == null || mView_Blur == null)
            {
                System.Diagnostics.Debug.Assert(false);
                return;
            }

            UInt32 Width = Math.Max(width, 4);
            UInt32 Height = Math.Max(height, 4);

            UInt32 Width4 = Width / 4;
            UInt32 Height4 = Height / 4;

            mView_Mask.OnResize(RHICtx, null, Width4, Height4);
            mView_Blur.OnResize(RHICtx, null, Width4, Height4);
        }
    }
}