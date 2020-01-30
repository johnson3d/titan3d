using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Graphics.View;
using EngineNS.Graphics.Tool;
using EngineNS.Graphics.EnvShader;

namespace EngineNS.Graphics.Shadow
{
    public class CGfxSSM
    {
        private int DPLimitter = int.MaxValue;
        public void SetGraphicsProfiler(CGraphicsProfiler profiler)
        {
            mCmdListDB_Shadow[0].SetGraphicsProfiler(profiler);
            mCmdListDB_Shadow[1].SetGraphicsProfiler(profiler);
        }

        private CCommandList[] mCmdListDB_Shadow;
        private CRenderPassDesc mRenderPassDesc_SSM;

        public CGfxSceneView mShadowMapView;
        private UInt32 mResolutionX = 4608; //3072
        private UInt32 mResolutionY = 4608; //4096;
        private UInt32 mBorderSize = 0;//4;
        //private UInt32 mInnerResolutionX = 4608; //5120; //4096 - 4 * 2;
        private UInt32 mInnerResolutionY = 4608; //5120; //4096 - 4 * 2;

        private CGfxCamera mShadowCamera;
        private Vector3 mDirLightCameraPos = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 mDirLightCameraLookAtPos = new Vector3(0.0f, 0.0f, 0.0f);
        public Vector3 mDirLightDirection = new Vector3(0.0f, -1.5f, 1.0f);
        
        private CGfxCamera m_refViewerCamera = null; //use this viewer camera info to calculate shadow camera info;
        public float mShadowDistance = 40.0f;
        private float mShadowCameraOffset = 100.0f;

        public float mFadeStrength = 0.2f;
        public Vector2 mFadeParam = new Vector2(0.0f, 0.0f);
        public float mShadowTransitionScale = 1000.0f;
        public Vector4 mShadowMapSizeAndRcp = new Vector4();
        public Matrix mViewer2ShadowMtx = new Matrix();
        private Matrix mOrtho2UVMtx = new Matrix();

        private Vector3[] mSSM_FrustumVtx = new Vector3[8];
        
        //env shader;
        public CGfxSE_SSM mSE_SSM;
        
        public CGfxSSM()
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;

            mCmdListDB_Shadow = new CCommandList[2];

            EngineNS.CCommandListDesc CmdListDesc = new EngineNS.CCommandListDesc();
            mCmdListDB_Shadow[0] = RHICtx.CreateCommandList(CmdListDesc);
            mCmdListDB_Shadow[1] = RHICtx.CreateCommandList(CmdListDesc);

            mCmdListDB_Shadow[0].DebugName = "SSM_Shadow";
            mCmdListDB_Shadow[1].DebugName = "SSM_Shadow";
        }

        public void Cleanup()
        {
            mCmdListDB_Shadow[0].Cleanup();
            mCmdListDB_Shadow[0] = null;
            mCmdListDB_Shadow[1].Cleanup();
            mCmdListDB_Shadow[1] = null;
            
            mShadowMapView.Cleanup();
            mShadowMapView = null;

            mShadowCamera.Cleanup();
            mShadowCamera = null;

        }

        public async System.Threading.Tasks.Task<bool> Init(CRenderContext RHICtx)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            if (mSE_SSM == null)
            {
                mSE_SSM = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxSE_SSM>();
            }

            if (mSE_SSM == null)
            {
                return false;
            }

            CGfxSceneViewInfo ShadowMapViewInfo = new CGfxSceneViewInfo();
            ShadowMapViewInfo.mUseDSV = true;
            ShadowMapViewInfo.Width = mResolutionX;
            ShadowMapViewInfo.Height = mResolutionY;
            // TODO��OpenGL ES���ʹ�� D24S8��ʽ�� ����depth texture�Ľ����Զ��0�� D16��D32��ʽ����ȷ�ġ�
            ShadowMapViewInfo.mDSVDesc.Init();
            ShadowMapViewInfo.mDSVDesc.Format = EngineNS.EPixelFormat.PXF_D16_UNORM;
            //ShadowMapViewInfo.mDSVDesc.Format = EngineNS.EPixelFormat.PXF_D32_FLOAT;
            //ShadowMapViewInfo.mDSVDesc.Format = EngineNS.EPixelFormat.PXF_D24_UNORM_S8_UINT;
            ShadowMapViewInfo.mDSVDesc.Width = mResolutionX;
            ShadowMapViewInfo.mDSVDesc.Height = mResolutionY;

            //CRenderTargetViewDesc rtDesc0 = new CRenderTargetViewDesc();
            //rtDesc0.Init();
            //rtDesc0.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            //rtDesc0.Width = mResolutionX;
            //rtDesc0.Height = mResolutionY;
            //ShadowMapViewInfo.mRTVDescArray.Add(rtDesc0);

            mShadowMapView = new CGfxSceneView();
            if (false == mShadowMapView.Init(RHICtx, null, ShadowMapViewInfo))
            {
                return false;
            }

            mRenderPassDesc_SSM = new CRenderPassDesc();
            FrameBufferClearColor TempClearColor = new FrameBufferClearColor();
            TempClearColor.r = 1.0f;
            TempClearColor.g = 1.0f;
            TempClearColor.b = 1.0f;
            TempClearColor.a = 1.0f;
            mRenderPassDesc_SSM.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_SSM.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_SSM.mFBClearColorRT0 = TempClearColor;
            mRenderPassDesc_SSM.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_SSM.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_SSM.mDepthClearValue = 1.0f;
            mRenderPassDesc_SSM.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_SSM.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_SSM.mStencilClearValue = 0u;

            mShadowCamera = new CGfxCamera();
            if (mShadowCamera.Init(RHICtx, false) == false)
            {
                return false;
            }
            //mShadowCamera.SetSceneView(RHICtx, mShadowMapView);
            
            mShadowMapView.ResizeViewport(mBorderSize, mBorderSize, mInnerResolutionY, mInnerResolutionY);

            mShadowMapSizeAndRcp.X = mResolutionX;
            mShadowMapSizeAndRcp.Y = mResolutionY;
            mShadowMapSizeAndRcp.Z = 1.0f / mResolutionX;
            mShadowMapSizeAndRcp.W = 1.0f / mResolutionY;

            //// OpenGL
            //mOrtho2UVMtx.M11 = 0.5f;
            //mOrtho2UVMtx.M22 = 0.5f;
            //mOrtho2UVMtx.M33 = 0.5f;
            //mOrtho2UVMtx.M41 = 0.5f;
            //mOrtho2UVMtx.M42 = 0.5f;
            //mOrtho2UVMtx.M43 = 0.5f;
            //mOrtho2UVMtx.M44 = 1.0f;

            if (CEngine.Instance.RenderSystem.RHIType == ERHIType.RHT_OGL)
            {
                //gles;
                mOrtho2UVMtx.M11 = 0.5f;
                mOrtho2UVMtx.M22 = 0.5f;
                mOrtho2UVMtx.M33 = 0.5f;
                mOrtho2UVMtx.M44 = 1.0f;
                mOrtho2UVMtx.M41 = 0.5f;
                mOrtho2UVMtx.M42 = 0.5f;
                mOrtho2UVMtx.M43 = 0.5f;
            }
            else
            {
                // D3D 
                mOrtho2UVMtx.M11 = 0.5f;
                mOrtho2UVMtx.M22 = -0.5f;
                mOrtho2UVMtx.M33 = 1.0f;
                mOrtho2UVMtx.M44 = 1.0f;
                mOrtho2UVMtx.M41 = 0.5f;
                mOrtho2UVMtx.M42 = 0.5f;
            }


            return true;
        }        
	    static float Clamp( float X, float Min, float Max )
	    {
		    return X<Min? Min : X<Max? X : Max;
	    }
        public static Profiler.TimeScope ScopeTickLogic = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxSSM), nameof(TickLogic));
        public void TickLogic(CRenderContext RHICtx, CGfxCamera ViewerCamera, Vector3 DirLightDir)
        {
            ScopeTickLogic.Begin();

            ViewerCamera.mShadowDistance = mShadowDistance;
            m_refViewerCamera = ViewerCamera;
            mDirLightDirection = DirLightDir;

            //calculate viewer camera frustum bounding sphere and shadow camera data;
            float HalfFoV = ViewerCamera.FoV * 0.5f;
            float NearUpLength = ViewerCamera.ZNear * (float)Math.Tan(HalfFoV);
            float NearRightLength = NearUpLength * ViewerCamera.Aspect;
            Vector3 NearUpOffset = NearUpLength * ViewerCamera.CameraData.Up;
            Vector3 NearRightOffset = NearRightLength * ViewerCamera.CameraData.Right;

            float FarUpLength = mShadowDistance * (float)Math.Tan(HalfFoV);
            float FarRightLength = FarUpLength * ViewerCamera.Aspect;
            Vector3 FarUpOffset = FarUpLength * ViewerCamera.CameraData.Up;
            Vector3 FarRightOffset = FarRightLength * ViewerCamera.CameraData.Right;
            
            mSSM_FrustumVtx[0] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * ViewerCamera.ZNear + NearRightOffset + NearUpOffset;//nrt;
            mSSM_FrustumVtx[1] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * ViewerCamera.ZNear + NearRightOffset - NearUpOffset;//nrb;
            mSSM_FrustumVtx[2] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * ViewerCamera.ZNear - NearRightOffset + NearUpOffset;//nlt;
            mSSM_FrustumVtx[3] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * ViewerCamera.ZNear - NearRightOffset - NearUpOffset;//nlb;
            mSSM_FrustumVtx[4] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * mShadowDistance + FarRightOffset + FarUpOffset;//frt;
            mSSM_FrustumVtx[5] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * mShadowDistance + FarRightOffset - FarUpOffset;//frb;
            mSSM_FrustumVtx[6] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * mShadowDistance - FarRightOffset + FarUpOffset;//flt;
            mSSM_FrustumVtx[7] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * mShadowDistance - FarRightOffset - FarUpOffset;//flb;

            //float MaxFrustumDiagonal = Vector3.Distance(SSM_FrustumVtx[3], SSM_FrustumVtx[4]);

            float TanHalfFoVHeight = (float)Math.Tan(HalfFoV);
            float TanHalfFoVWidth = TanHalfFoVHeight * ViewerCamera.Aspect;
            float FrustumLength = mShadowDistance - ViewerCamera.ZNear;

            float NearHalfWidth = ViewerCamera.ZNear * TanHalfFoVWidth;
            float NearHalfHeight = ViewerCamera.ZNear * TanHalfFoVHeight;
            float NearHalfDiagonalSqr = NearHalfWidth * NearHalfWidth + NearHalfHeight * NearHalfHeight;
            float FarHalfWidth = mShadowDistance * TanHalfFoVWidth;
            float FarHalfHeight = mShadowDistance * TanHalfFoVHeight;
            float FarHalfDiagonalSqr = FarHalfWidth * FarHalfWidth + FarHalfHeight * FarHalfHeight;

            float OptimalOffset = FrustumLength * 0.5f + (NearHalfDiagonalSqr - FarHalfDiagonalSqr) / (2.0f * FrustumLength);
            float ViewerPosOffset = Clamp(mShadowDistance - OptimalOffset, ViewerCamera.ZNear, mShadowDistance);
            Vector3 FrustumSphereCenter = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * ViewerPosOffset;
            float FrustumSphereRadius = 1.0f;
            for (UInt32 idx = 0; idx < 8; idx++)
            {
                FrustumSphereRadius = Math.Max(FrustumSphereRadius, Vector3.DistanceSquared(FrustumSphereCenter, mSSM_FrustumVtx[idx]));
            }
            FrustumSphereRadius = (float)Math.Ceiling((float)Math.Sqrt(FrustumSphereRadius));
            //FrustumSphereRadius = Math.Min((float)Math.Sqrt(FrustumSphereRadius), MaxFrustumDiagonal * 0.5f);

            Vector3 ShadowCameraPos = FrustumSphereCenter - mDirLightDirection * (FrustumSphereRadius + mShadowCameraOffset);

            mShadowCamera.LookAtLH(ShadowCameraPos, FrustumSphereCenter, Vector3.UnitY, true);
            float FrustumSphereDiameter = FrustumSphereRadius * 2.0f;
            float ShadowCameraZFar = mShadowCameraOffset + FrustumSphereDiameter;
            float ShadowCameraZNear = 0.0f;
            mShadowCamera.MakeOrtho(FrustumSphereDiameter, FrustumSphereDiameter, ShadowCameraZNear, ShadowCameraZFar);
            Vector4 WorldCenterNDC = Vector4.Transform(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), mShadowCamera.CameraData.ViewProjection);
            float TexelPosX = (WorldCenterNDC.X * 0.5f + 0.5f) * mResolutionY;
            float TexelPosAdjustedNdcX = ((float)Math.Floor(TexelPosX) / mResolutionY - 0.5f) / 0.5f;
            float TexelOffsetNdcX = TexelPosAdjustedNdcX - WorldCenterNDC.X;
            float TexelPosY = (WorldCenterNDC.Y * (-0.5f) + 0.5f) * mResolutionY;
            float TexelPosAdjustedNdcY = ((float)Math.Floor(TexelPosY) / mResolutionY - 0.5f) / (-0.5f);
            float TexelOffsetNdcY = TexelPosAdjustedNdcY - WorldCenterNDC.Y;
            mShadowCamera.DoOrthoProjectionForShadow(FrustumSphereDiameter, FrustumSphereDiameter, ShadowCameraZNear, ShadowCameraZFar, TexelOffsetNdcX, TexelOffsetNdcY);


            //mShadowCamera.LookAtLH(ShadowCameraPos, FrustumSphereCenter, Vector3.UnitY);
            //float FrustumSphereDiameter = FrustumSphereRadius * 2.0f;
            //float TexelPerUnit = mResolutionY / FrustumSphereDiameter;
            //Matrix TempViewMtx = mShadowCamera.ViewMatrix;
            //TempViewMtx.M11 *= TexelPerUnit;
            //TempViewMtx.M22 *= TexelPerUnit;
            //Matrix TempViewMtxInverse = Matrix.Invert(ref TempViewMtx);
            //Vector4 FrustumSphereCenterViewSpace = Vector4.Transform(new Vector4(FrustumSphereCenter, 1.0f), TempViewMtx);
            //FrustumSphereCenterViewSpace.X = (float)Math.Floor(FrustumSphereCenterViewSpace.X);
            //FrustumSphereCenterViewSpace.Y = (float)Math.Floor(FrustumSphereCenterViewSpace.Y);

            //Vector4 FrustumSphereCenterWS = Vector4.Transform(FrustumSphereCenterViewSpace, TempViewMtxInverse);
            //FrustumSphereCenter.X = FrustumSphereCenterWS.X;
            //FrustumSphereCenter.Y = FrustumSphereCenterWS.Y;
            //FrustumSphereCenter.Z = FrustumSphereCenterWS.Z;

            //ShadowCameraPos = FrustumSphereCenter - mDirLightDirection * (FrustumSphereRadius + mShadowCameraOffset);
            //mShadowCamera.LookAtLH(ShadowCameraPos, FrustumSphereCenter, Vector3.UnitY);
            //float ShadowCameraZFar = mShadowCameraOffset + FrustumSphereDiameter;
            //float ShadowCameraZNear = 0.0f;
            //mShadowCamera.MakeOrtho(FrustumSphereDiameter, FrustumSphereDiameter, ShadowCameraZNear, ShadowCameraZFar);


            mViewer2ShadowMtx = mShadowCamera.CameraData.ViewProjection * mOrtho2UVMtx;

            float UniformDepthBias = 2.0f;
            float PerObjCustomDepthBias = 1.0f;
            float DepthBiasClipSpace = UniformDepthBias / (ShadowCameraZFar - ShadowCameraZNear) * (FrustumSphereDiameter / mResolutionY) * PerObjCustomDepthBias;
            mShadowMapView.mDepthBiasAndZFarRcp = new Vector2(0.0f, 1.0f / ShadowCameraZFar);
            
            mShadowTransitionScale = 1.0f / (DepthBiasClipSpace + 0.00001f);
            
            float FadeStartDistance = mShadowDistance - mShadowDistance * mFadeStrength;
            mFadeParam.X = 1.0f / (mShadowDistance - FadeStartDistance + 0.0001f);
            mFadeParam.Y = -FadeStartDistance * mFadeParam.X;

            ViewerCamera.PushVisibleMesh2ShadowLayer(mShadowCamera.CullingFrustum);
            mShadowMapView.CookShadowLayerData2Pass(RHICtx, m_refViewerCamera, mShadowCamera, mSE_SSM);
            var CmdList = mCmdListDB_Shadow[0];
            mShadowMapView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Shadow);

            CmdList.BeginCommand();
            CmdList.BeginRenderPass(mRenderPassDesc_SSM, mShadowMapView.FrameBuffer);
            CmdList.BuildRenderPass(DPLimitter);
            CmdList.EndRenderPass();
            CmdList.EndCommand();

            ViewerCamera.ClearSpecRenderLayerData(ERenderLayer.RL_Shadow);

            DrawCall = CmdList.DrawCall;
            DrawTriangle = CmdList.DrawTriangle;

            ScopeTickLogic.End();
        }
        public int DrawCall;
        public int DrawTriangle;
        public void TickRender(CRenderContext RHICtx)
        {
            var CmdList = mCmdListDB_Shadow[1];
            CmdList.Commit(RHICtx);
        }
        public void BeforeFrame()
        {
            mShadowCamera.BeforeFrame();
        }
        public void TickSync()
        {
            var Temp = mCmdListDB_Shadow[0];
            mCmdListDB_Shadow[0] = mCmdListDB_Shadow[1];
            mCmdListDB_Shadow[1] = Temp;
            mShadowCamera.SwapBuffer();
        }

    }
}
