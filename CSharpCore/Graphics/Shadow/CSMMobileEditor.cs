using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Graphics.View;
using EngineNS.Graphics.Tool;
using EngineNS.Graphics.EnvShader;

namespace EngineNS.Graphics.Shadow
{
    public class CGfxCSMMobileEditor
    {
        private int DPLimitter = int.MaxValue;

        private CCommandList[,] mCmdListDB_Shadow;
        private CRenderPassDesc[] mRenderPassDesc_CSM;

        public CGfxSceneView mShadowMapView;
        private UInt32 mResolutionX = 2048;
        private UInt32 mResolutionY = 2048;
        private UInt32 mBorderSize = 2;
        private UInt32 mInnerResolutionX = 2048 - 2 * 2;
        private UInt32 mInnerResolutionY = 2048 - 2 * 2;
        public UInt32 mCsmNum = 4;
        private UInt32 mWholeReslutionX = 2048 * 4;
        private UInt32 mWholeReslutionY = 2048;

        private CGfxCamera[] mShadowCameraArray;
        public Vector3 mDirLightDirection = new Vector3(0.0f, -1.5f, 1.0f);
        
        private CGfxCamera m_refViewerCamera = null; //use this viewer camera info to calculate shadow camera info;
        public float mShadowDistance = 120.0f;
        public float[] mCsmDistanceArray = new float[4];
        public float[] mSumDistanceFarArray = new float[4];
        public Vector4 mSumDistanceFarVec = new Vector4();
        private float mShadowCameraOffset = 100.0f;

        public float mFadeStrength = 0.2f;
        public Vector2 mFadeParam = new Vector2(0.0f, 0.0f);
        public float[] mShadowTransitionScaleArray = new float[4];
        public Vector4 mShadowTransitionScaleVec = new Vector4();
        public Vector4 mShadowMapSizeAndRcp = new Vector4();
        public Matrix[] mViewer2ShadowMtxArray = new Matrix[4];
        private Matrix mOrtho2UVMtx = new Matrix();
        private Matrix[] mUVAdjustedMtxArray = new Matrix[4];
        
        private Vector3[] mCSM_FrustumVtx = new Vector3[8];
        
        //env shader;
        public CGfxSE_SSM mSE_CSMMobile;

        public CGfxCSMMobileEditor()
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;

            mCmdListDB_Shadow = new CCommandList[4, 2];
            EngineNS.CCommandListDesc CmdListDesc = new EngineNS.CCommandListDesc();
            for (UInt32 CmdIdx = 0; CmdIdx < 4; CmdIdx++)
            {
                mCmdListDB_Shadow[CmdIdx, 0] = RHICtx.CreateCommandList(CmdListDesc);
                mCmdListDB_Shadow[CmdIdx, 1] = RHICtx.CreateCommandList(CmdListDesc);
            }
           
            
        }

        public void Cleanup()
        {
            for (UInt32 CmdIdx = 0; CmdIdx < 4; CmdIdx++)
            {
                mCmdListDB_Shadow[CmdIdx, 0].Cleanup();
                mCmdListDB_Shadow[CmdIdx, 0] = null;
                mCmdListDB_Shadow[CmdIdx, 1].Cleanup();
                mCmdListDB_Shadow[CmdIdx, 1] = null;
            }

            mShadowMapView.Cleanup();
            mShadowMapView = null;

            for (UInt32 CamIdx = 0; CamIdx < 4; CamIdx++)
            {
                mShadowCameraArray[CamIdx].Cleanup();
                mShadowCameraArray[CamIdx] = null;
            }
        }

        public async System.Threading.Tasks.Task<bool> Init(CRenderContext RHICtx)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            if (mSE_CSMMobile == null)
            {
                mSE_CSMMobile = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxSE_SSM>();
            }

            if (mSE_CSMMobile == null)
            {
                return false;
            }

            mWholeReslutionX = mResolutionX * mCsmNum;
            mWholeReslutionY = mResolutionY;

            CGfxSceneViewInfo ShadowMapViewInfo = new CGfxSceneViewInfo();
            ShadowMapViewInfo.mUseDSV = true;
            ShadowMapViewInfo.Width = mWholeReslutionX;
            ShadowMapViewInfo.Height = mWholeReslutionY;
            ShadowMapViewInfo.mDSVDesc.Init();
            ShadowMapViewInfo.mDSVDesc.Format = EngineNS.EPixelFormat.PXF_D16_UNORM;
            ShadowMapViewInfo.mDSVDesc.Width = mWholeReslutionX;
            ShadowMapViewInfo.mDSVDesc.Height = mWholeReslutionY;
            mShadowMapView = new CGfxSceneView();
            if (false == mShadowMapView.Init(RHICtx, null, ShadowMapViewInfo))
            {
                return false;
            }

            mRenderPassDesc_CSM = new CRenderPassDesc[4];
            FrameBufferClearColor TempClearColor = new FrameBufferClearColor();
            TempClearColor.r = 1.0f;
            TempClearColor.g = 1.0f;
            TempClearColor.b = 1.0f;
            TempClearColor.a = 1.0f;
            mRenderPassDesc_CSM[0].mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_CSM[0].mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_CSM[0].mFBClearColorRT0 = TempClearColor;
            mRenderPassDesc_CSM[0].mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_CSM[0].mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_CSM[0].mDepthClearValue = 1.0f;
            mRenderPassDesc_CSM[0].mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            mRenderPassDesc_CSM[0].mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            mRenderPassDesc_CSM[0].mStencilClearValue = 0u;

            for (UInt32 RPDIdx = 1; RPDIdx < 4; RPDIdx++)
            {
                mRenderPassDesc_CSM[RPDIdx].mFBLoadAction_Color = FrameBufferLoadAction.LoadActionLoad;
                mRenderPassDesc_CSM[RPDIdx].mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
                mRenderPassDesc_CSM[RPDIdx].mFBClearColorRT0 = TempClearColor;
                mRenderPassDesc_CSM[RPDIdx].mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionLoad;
                mRenderPassDesc_CSM[RPDIdx].mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
                mRenderPassDesc_CSM[RPDIdx].mDepthClearValue = 1.0f;
                mRenderPassDesc_CSM[RPDIdx].mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionLoad;
                mRenderPassDesc_CSM[RPDIdx].mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
                mRenderPassDesc_CSM[RPDIdx].mStencilClearValue = 0u;
            }

            mShadowCameraArray = new CGfxCamera[4];
            for (UInt32 CamIdx = 0; CamIdx < 4; CamIdx++)
            {
                mShadowCameraArray[CamIdx] = new CGfxCamera();
                if (mShadowCameraArray[CamIdx].Init(RHICtx, false) == false)
                {
                    return false;
                }
            }
            
            mShadowMapView.ResizeViewport(mBorderSize, mBorderSize, mInnerResolutionY, mInnerResolutionY);

            mShadowMapSizeAndRcp.X = mWholeReslutionX;
            mShadowMapSizeAndRcp.Y = mWholeReslutionY;
            mShadowMapSizeAndRcp.Z = 1.0f / mWholeReslutionX;
            mShadowMapSizeAndRcp.W = 1.0f / mWholeReslutionY;
            
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
            
            for (UInt32 UVAdjustIdx = 0; UVAdjustIdx < mCsmNum; UVAdjustIdx++)
            {
                mUVAdjustedMtxArray[UVAdjustIdx].M11 = (float)mInnerResolutionX / (float)mWholeReslutionX;
                mUVAdjustedMtxArray[UVAdjustIdx].M22 = (float)mInnerResolutionY / (float)mWholeReslutionY;
                mUVAdjustedMtxArray[UVAdjustIdx].M33 = 1.0f;
                mUVAdjustedMtxArray[UVAdjustIdx].M44 = 1.0f;
                mUVAdjustedMtxArray[UVAdjustIdx].M41 = (float)(mResolutionX * UVAdjustIdx + mBorderSize) / (float)mWholeReslutionX;
                mUVAdjustedMtxArray[UVAdjustIdx].M42 = (float)mBorderSize / (float)mWholeReslutionY;
            }

            mCsmDistanceArray[0] = mShadowDistance * 0.08f;
            mCsmDistanceArray[1] = mShadowDistance * 0.15f;
            mCsmDistanceArray[2] = mShadowDistance * 0.30f;
            mCsmDistanceArray[3] = mShadowDistance * 0.47f;

            mSumDistanceFarArray[0] = mCsmDistanceArray[0];
            mSumDistanceFarArray[1] = mCsmDistanceArray[0] + mCsmDistanceArray[1];
            mSumDistanceFarArray[2] = mCsmDistanceArray[0] + mCsmDistanceArray[1] + mCsmDistanceArray[2];
            mSumDistanceFarArray[3] = mCsmDistanceArray[0] + mCsmDistanceArray[1] + mCsmDistanceArray[2] + mCsmDistanceArray[3];

            mSumDistanceFarVec.X = mSumDistanceFarArray[0];
            mSumDistanceFarVec.Y = mSumDistanceFarArray[1];
            mSumDistanceFarVec.Z = mSumDistanceFarArray[2];
            mSumDistanceFarVec.W = mSumDistanceFarArray[3];

            return true;
        }        
	    static float Clamp( float X, float Min, float Max )
	    {
		    return X<Min? Min : X<Max? X : Max;
	    }

        public void TickLogic(CRenderContext RHICtx, CGfxCamera ViewerCamera, Vector3 DirLightDir)
        {
            ViewerCamera.mShadowDistance = mShadowDistance;
            m_refViewerCamera = ViewerCamera;
            mDirLightDirection = DirLightDir;
            
            for (UInt32 CsmIdx = 0; CsmIdx < mCsmNum; CsmIdx++)
            {
                //calculate viewer camera frustum bounding sphere and shadow camera data;
                float HalfFoV = ViewerCamera.FoV * 0.5f;
                
                float FrustumNear = 0.0f;
                if (CsmIdx == 0)
                {
                    FrustumNear = ViewerCamera.ZNear;
                }
                else
                {
                    FrustumNear = mSumDistanceFarArray[CsmIdx - 1];
                }
                
                float FrustumFar = mSumDistanceFarArray[CsmIdx];
                float FrustumLength = FrustumFar - FrustumNear;

                float TanHalfFoVHeight = (float)Math.Tan(HalfFoV);
                float TanHalfFoVWidth = TanHalfFoVHeight * ViewerCamera.Aspect;
               
                float NearHalfHeight = FrustumNear * TanHalfFoVHeight;
                float NearHalfWidth = FrustumNear * TanHalfFoVWidth;
                Vector3 NearUpOffset = NearHalfHeight * ViewerCamera.CameraData.Up;
                Vector3 NearRightOffset = NearHalfWidth * ViewerCamera.CameraData.Right;
                
                float FarHalfHeight = FrustumFar * TanHalfFoVHeight;
                float FarHalfWidth = FrustumFar * TanHalfFoVWidth;
                Vector3 FarUpOffset = FarHalfHeight * ViewerCamera.CameraData.Up;
                Vector3 FarRightOffset = FarHalfWidth * ViewerCamera.CameraData.Right;

                mCSM_FrustumVtx[0] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * FrustumNear + NearRightOffset + NearUpOffset;//nrt;
                mCSM_FrustumVtx[1] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * FrustumNear + NearRightOffset - NearUpOffset;//nrb;
                mCSM_FrustumVtx[2] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * FrustumNear - NearRightOffset + NearUpOffset;//nlt;
                mCSM_FrustumVtx[3] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * FrustumNear - NearRightOffset - NearUpOffset;//nlb;
                mCSM_FrustumVtx[4] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * FrustumFar + FarRightOffset + FarUpOffset;//frt;
                mCSM_FrustumVtx[5] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * FrustumFar + FarRightOffset - FarUpOffset;//frb;
                mCSM_FrustumVtx[6] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * FrustumFar - FarRightOffset + FarUpOffset;//flt;
                mCSM_FrustumVtx[7] = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * FrustumFar - FarRightOffset - FarUpOffset;//flb;

                //Vector3 FrustumCenter = (mCSM_FrustumVtx[1] + mCSM_FrustumVtx[2] + mCSM_FrustumVtx[7] + mCSM_FrustumVtx[4]) / 4.0f;
                //Vector3 ViewerCameraPos = ViewerCamera.CameraData.Position;
                //float DistanceFrustum2Camera = Vector3.Distance(ref FrustumCenter, ref ViewerCameraPos);
                //Vector3 FrustumSphereCenter = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * DistanceFrustum2Camera;

                float NearHalfDiagonalSqr = NearHalfWidth * NearHalfWidth + NearHalfHeight * NearHalfHeight;
                float FarHalfDiagonalSqr = FarHalfWidth * FarHalfWidth + FarHalfHeight * FarHalfHeight;
                float OptimalOffset = FrustumLength * 0.5f + (NearHalfDiagonalSqr - FarHalfDiagonalSqr) / (2.0f * FrustumLength);
                float ViewerPosOffset = Clamp(FrustumFar - OptimalOffset, FrustumNear, FrustumFar);
                Vector3 FrustumSphereCenter = ViewerCamera.CameraData.Position + ViewerCamera.CameraData.Direction * ViewerPosOffset;

                float FrustumSphereRadius = 1.0f;
                for (UInt32 FSR_Idx = 0; FSR_Idx < 8; FSR_Idx++)
                {
                    FrustumSphereRadius = Math.Max(FrustumSphereRadius, Vector3.DistanceSquared(FrustumSphereCenter, mCSM_FrustumVtx[FSR_Idx]));
                }
                //FrustumSphereRadius = (float)Math.Ceiling((float)Math.Sqrt(FrustumSphereRadius));
                FrustumSphereRadius = (float)Math.Sqrt(FrustumSphereRadius);

                Vector3 ShadowCameraPos = FrustumSphereCenter - mDirLightDirection * (FrustumSphereRadius + mShadowCameraOffset);

                mShadowCameraArray[CsmIdx].LookAtLH(ShadowCameraPos, FrustumSphereCenter, Vector3.UnitY, true);
                float FrustumSphereDiameter = FrustumSphereRadius * 2.0f;
                float ShadowCameraZFar = mShadowCameraOffset + FrustumSphereDiameter;
                float ShadowCameraZNear = 0.0f;
                mShadowCameraArray[CsmIdx].MakeOrtho(FrustumSphereDiameter, FrustumSphereDiameter, ShadowCameraZNear, ShadowCameraZFar);

                Vector4 WorldCenterNDC = Vector4.Transform(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), mShadowCameraArray[CsmIdx].CameraData.ViewProjection);
                float TexelPosX = (WorldCenterNDC.X * 0.5f + 0.5f) * mInnerResolutionX;
                float TexelPosAdjustedNdcX = ((float)Math.Floor(TexelPosX) / mInnerResolutionX - 0.5f) / 0.5f;
                float TexelOffsetNdcX = TexelPosAdjustedNdcX - WorldCenterNDC.X;
                float TexelPosY = (WorldCenterNDC.Y * (-0.5f) + 0.5f) * mInnerResolutionY;
                float TexelPosAdjustedNdcY = ((float)Math.Floor(TexelPosY) / mInnerResolutionY - 0.5f) / (-0.5f);
                float TexelOffsetNdcY = TexelPosAdjustedNdcY - WorldCenterNDC.Y;
                mShadowCameraArray[CsmIdx].DoOrthoProjectionForShadow(FrustumSphereDiameter, FrustumSphereDiameter, ShadowCameraZNear, ShadowCameraZFar, TexelOffsetNdcX, TexelOffsetNdcY);

                //data for other code to use;
                {
                    mViewer2ShadowMtxArray[CsmIdx] = mShadowCameraArray[CsmIdx].CameraData.ViewProjection * mOrtho2UVMtx * mUVAdjustedMtxArray[CsmIdx];

                    float UniformDepthBias = 2.0f;
                    float PerObjCustomDepthBias = 1.0f;
                    float DepthBiasClipSpace = UniformDepthBias / (ShadowCameraZFar - ShadowCameraZNear) * (FrustumSphereDiameter / mInnerResolutionY) * PerObjCustomDepthBias;
                    //mShadowMapView.mDepthBiasAndZFarRcp = new Vector2(DepthBiasClipSpace, 1.0f / ShadowCameraZFar);
                    mShadowMapView.mDepthBiasAndZFarRcp = new Vector2(0.0f, 1.0f / ShadowCameraZFar);

                    mShadowTransitionScaleArray[CsmIdx] = 1.0f / (DepthBiasClipSpace + 0.00001f);

                    float FadeStartDistance = mShadowDistance - mShadowDistance * mFadeStrength;
                    mFadeParam.X = 1.0f / (mShadowDistance - FadeStartDistance + 0.0001f);
                    mFadeParam.Y = -FadeStartDistance * mFadeParam.X;
                }

                ViewerCamera.PushVisibleMesh2ShadowLayer(mShadowCameraArray[CsmIdx].CullingFrustum);

                mShadowMapView.ResizeViewport(mResolutionX * CsmIdx + mBorderSize, mBorderSize, mInnerResolutionY, mInnerResolutionY);
                mShadowMapView.CookShadowLayerData2Pass(RHICtx, m_refViewerCamera, mShadowCameraArray[CsmIdx], mSE_CSMMobile);
                var CmdList = mCmdListDB_Shadow[CsmIdx, 0];
                mShadowMapView.PushSpecRenderLayerDataToRHI(CmdList, ERenderLayer.RL_Shadow);
                
                CmdList.BeginCommand();
                CmdList.BeginRenderPass(mRenderPassDesc_CSM[CsmIdx], mShadowMapView.FrameBuffer);
                CmdList.BuildRenderPass(DPLimitter);
                CmdList.EndRenderPass();
                CmdList.EndCommand();

                ViewerCamera.ClearSpecRenderLayerData(ERenderLayer.RL_Shadow);

                DrawCall = CmdList.DrawCall;
                DrawTriangle = CmdList.DrawTriangle;
            }

            mShadowTransitionScaleVec.X = mShadowTransitionScaleArray[0];
            mShadowTransitionScaleVec.Y = mShadowTransitionScaleArray[1];
            mShadowTransitionScaleVec.Z = mShadowTransitionScaleArray[2];
            mShadowTransitionScaleVec.W = mShadowTransitionScaleArray[3];
        }

        public int DrawCall;
        public int DrawTriangle;

        public void TickRender(CRenderContext RHICtx)
        {
            for (UInt32 CmdIdx = 0; CmdIdx < mCsmNum; CmdIdx++)
            {
                mCmdListDB_Shadow[CmdIdx, 1].Commit(RHICtx);
            }
        }
        public void BeforeFrame()
        {
            
        }
        public void TickSync()
        {
            for (UInt32 CmdIdx = 0; CmdIdx < mCsmNum; CmdIdx++)
            {
                var Temp = mCmdListDB_Shadow[CmdIdx ,0];
                mCmdListDB_Shadow[CmdIdx, 0] = mCmdListDB_Shadow[CmdIdx, 1];
                mCmdListDB_Shadow[CmdIdx, 1] = Temp;

                mShadowCameraArray[CmdIdx].SwapBuffer();
            }
            
        }

    }
}
