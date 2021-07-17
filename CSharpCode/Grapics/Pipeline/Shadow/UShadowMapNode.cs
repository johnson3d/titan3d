using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shadow
{
    public class UShadowShading : Shader.UShadingEnv
    {
        public UShadowShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/SSM.cginc", RName.ERNameType.Engine);
        }
    }
    public class UShadowMapNode : Shader.UShadingEnv
    {
        public GamePlay.UWorld.UVisParameter mVisParameter = new GamePlay.UWorld.UVisParameter();
        public UGraphicsBuffers GBuffers { get; protected set; } = new UGraphicsBuffers();
        public UDrawBuffers BasePass = new UDrawBuffers();
        public RenderPassDesc PassDesc = new RenderPassDesc();

        private UInt32 mResolutionX = 4608; //3072
        private UInt32 mResolutionY = 4608; //4096;
        private UInt32 mBorderSize = 0;//4;
        //private UInt32 mInnerResolutionX = 4608; //5120; //4096 - 4 * 2;
        private UInt32 mInnerResolutionY = 4608; //5120; //4096 - 4 * 2;

        private Vector3 mDirLightCameraPos = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 mDirLightCameraLookAtPos = new Vector3(0.0f, 0.0f, 0.0f);
        public Vector3 mDirLightDirection = new Vector3(0.0f, -1.5f, 1.0f);

        public float mShadowDistance = 40.0f;
        private float mShadowCameraOffset = 100.0f;

        public float mFadeStrength = 0.2f;
        public Vector2 mFadeParam = new Vector2(0.0f, 0.0f);
        public float mShadowTransitionScale = 1000.0f;
        public Vector4 mShadowMapSizeAndRcp = new Vector4();
        public Matrix mViewer2ShadowMtx = new Matrix();
        private Matrix mOrtho2UVMtx = new Matrix();

        private Vector3[] mSSM_FrustumVtx = new Vector3[8];
        public UShadowShading mShadowShading;

        public void Initialize(float x, float y)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            mShadowShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UShadowShading>();

            GBuffers.SwapChainIndex = -1;
            GBuffers.Initialize(0, EPixelFormat.PXF_D16_UNORM, (uint)x, (uint)y);
            //GBuffers.CreateGBuffer(0, EPixelFormat.PXF_UNKNOWN, (uint)x, (uint)y);            
            GBuffers.TargetViewIdentifier = new UGraphicsBuffers.UTargetViewIdentifier();
            GBuffers.OnResize(mInnerResolutionY, mInnerResolutionY);

            BasePass = new UDrawBuffers();
            BasePass.Initialize(rc);

            var TempClearColor = new Color4();
            //TempClearColor.Red = 1.0f;
            //TempClearColor.Green = 1.0f;
            //TempClearColor.Blue = 1.0f;
            //TempClearColor.Alpha = 1.0f;
            PassDesc.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mFBClearColorRT0 = TempClearColor;
            PassDesc.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mDepthClearValue = 1.0f;
            PassDesc.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mStencilClearValue = 0u;

            mShadowMapSizeAndRcp.X = mResolutionX;
            mShadowMapSizeAndRcp.Y = mResolutionY;
            mShadowMapSizeAndRcp.Z = 1.0f / mResolutionX;
            mShadowMapSizeAndRcp.W = 1.0f / mResolutionY;

            if (UEngine.Instance.GfxDevice.RenderContext.RHIType == ERHIType.RHT_OGL)
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
                mOrtho2UVMtx.M33 = 0.5f;
                mOrtho2UVMtx.M44 = 1.0f;
                mOrtho2UVMtx.M14 = 0.5f;
                mOrtho2UVMtx.M24 = 0.5f;
                mOrtho2UVMtx.M34 = 0.5f;

            }

            mVisParameter.VisibleMeshes = new List<Mesh.UMesh>();
        }
        public void Cleanup()
        {
            GBuffers?.Cleanup();
            GBuffers = null;
        }
        private static float Clamp(float X, float Min, float Max)
        {
            return X < Min ? Min : X < Max ? X : Max;
        }
        public unsafe void TickLogic(GamePlay.UWorld world, IRenderPolicy policy, bool bClear)
        {
            mDirLightDirection = world.DirectionLight.mDirection;

            var ViewerCamera = policy.GBuffers.Camera;
            //calculate viewer camera frustum bounding sphere and shadow camera data;
            float HalfFoV = ViewerCamera.mCoreObject.mFov * 0.5f;
            float zNear = ViewerCamera.mCoreObject.mZNear;
            float NearUpLength = zNear * (float)Math.Tan(HalfFoV);
            float NearRightLength = NearUpLength * ViewerCamera.mCoreObject.mAspect;
            Vector3 NearUpOffset = NearUpLength * ViewerCamera.mCoreObject.GetUp();
            Vector3 NearRightOffset = NearRightLength * ViewerCamera.mCoreObject.GetRight();

            float FarUpLength = mShadowDistance * (float)Math.Tan(HalfFoV);
            float FarRightLength = FarUpLength * ViewerCamera.mCoreObject.mAspect;
            Vector3 FarUpOffset = FarUpLength * ViewerCamera.mCoreObject.GetUp();
            Vector3 FarRightOffset = FarRightLength * ViewerCamera.mCoreObject.GetRight();

            var position = ViewerCamera.mCoreObject.GetPosition();
            var direction = ViewerCamera.mCoreObject.GetDirection();
            mSSM_FrustumVtx[0] = position + direction * zNear + NearRightOffset + NearUpOffset;//nrt;
            mSSM_FrustumVtx[1] = position + direction * zNear + NearRightOffset - NearUpOffset;//nrb;
            mSSM_FrustumVtx[2] = position + direction * zNear - NearRightOffset + NearUpOffset;//nlt;
            mSSM_FrustumVtx[3] = position + direction * zNear - NearRightOffset - NearUpOffset;//nlb;
            mSSM_FrustumVtx[4] = position + direction * mShadowDistance + FarRightOffset + FarUpOffset;//frt;
            mSSM_FrustumVtx[5] = position + direction * mShadowDistance + FarRightOffset - FarUpOffset;//frb;
            mSSM_FrustumVtx[6] = position + direction * mShadowDistance - FarRightOffset + FarUpOffset;//flt;
            mSSM_FrustumVtx[7] = position + direction * mShadowDistance - FarRightOffset - FarUpOffset;//flb;

            float TanHalfFoVHeight = (float)Math.Tan(HalfFoV);
            float TanHalfFoVWidth = TanHalfFoVHeight * ViewerCamera.mCoreObject.mAspect;
            float FrustumLength = mShadowDistance - zNear;

            float NearHalfWidth = zNear * TanHalfFoVWidth;
            float NearHalfHeight = zNear * TanHalfFoVHeight;
            float NearHalfDiagonalSqr = NearHalfWidth * NearHalfWidth + NearHalfHeight * NearHalfHeight;
            float FarHalfWidth = mShadowDistance * TanHalfFoVWidth;
            float FarHalfHeight = mShadowDistance * TanHalfFoVHeight;
            float FarHalfDiagonalSqr = FarHalfWidth * FarHalfWidth + FarHalfHeight * FarHalfHeight;

            float OptimalOffset = FrustumLength * 0.5f + (NearHalfDiagonalSqr - FarHalfDiagonalSqr) / (2.0f * FrustumLength);
            float ViewerPosOffset = Clamp(mShadowDistance - OptimalOffset, zNear, mShadowDistance);
            Vector3 FrustumSphereCenter = position + direction * ViewerPosOffset;
            float FrustumSphereRadius = 1.0f;
            for (UInt32 idx = 0; idx < 8; idx++)
            {
                FrustumSphereRadius = Math.Max(FrustumSphereRadius, Vector3.DistanceSquared(FrustumSphereCenter, mSSM_FrustumVtx[idx]));
            }
            FrustumSphereRadius = (float)Math.Ceiling((float)Math.Sqrt(FrustumSphereRadius));
            //FrustumSphereRadius = Math.Min((float)Math.Sqrt(FrustumSphereRadius), MaxFrustumDiagonal * 0.5f);

            Vector3 ShadowCameraPos = FrustumSphereCenter - mDirLightDirection * (FrustumSphereRadius + mShadowCameraOffset);

            var shadowCamera = GBuffers.Camera.mCoreObject;
            shadowCamera.LookAtLH(ref ShadowCameraPos, ref FrustumSphereCenter, ref Vector3.UnitY);
            float FrustumSphereDiameter = FrustumSphereRadius * 2.0f;
            float ShadowCameraZFar = mShadowCameraOffset + FrustumSphereDiameter;
            float ShadowCameraZNear = 0.0f;
            shadowCamera.MakeOrtho(FrustumSphereDiameter, FrustumSphereDiameter, ShadowCameraZNear, ShadowCameraZFar);
            Vector4 WorldCenterNDC = Vector4.Transform(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), shadowCamera.GetViewProjection());
            float TexelPosX = (WorldCenterNDC.X * 0.5f + 0.5f) * mResolutionY;
            float TexelPosAdjustedNdcX = ((float)Math.Floor(TexelPosX) / mResolutionY - 0.5f) / 0.5f;
            float TexelOffsetNdcX = TexelPosAdjustedNdcX - WorldCenterNDC.X;
            float TexelPosY = (WorldCenterNDC.Y * (-0.5f) + 0.5f) * mResolutionY;
            float TexelPosAdjustedNdcY = ((float)Math.Floor(TexelPosY) / mResolutionY - 0.5f) / (-0.5f);
            float TexelOffsetNdcY = TexelPosAdjustedNdcY - WorldCenterNDC.Y;
            shadowCamera.DoOrthoProjectionForShadow(FrustumSphereDiameter, FrustumSphereDiameter, ShadowCameraZNear, ShadowCameraZFar, TexelOffsetNdcX, TexelOffsetNdcY);

            Matrix vp = shadowCamera.GetViewProjection();
            Matrix result;
            Matrix.Transpose(ref vp, out result);

            mViewer2ShadowMtx = mOrtho2UVMtx * result;


            float UniformDepthBias = 2.0f;
            float PerObjCustomDepthBias = 1.0f;
            float DepthBiasClipSpace = UniformDepthBias / (ShadowCameraZFar - ShadowCameraZNear) * (FrustumSphereDiameter / mResolutionY) * PerObjCustomDepthBias;

            var cBuffer = GBuffers.PerViewportCBuffer;
            if (cBuffer != null)
            {
                var tmp = new Vector2(0.0f, 1.0f / ShadowCameraZFar);
                cBuffer.SetValue(cBuffer.PerViewportIndexer.gDepthBiasAndZFarRcp, ref tmp);
            }

            mShadowTransitionScale = 1.0f / (DepthBiasClipSpace + 0.00001f);

            float FadeStartDistance = mShadowDistance - mShadowDistance * mFadeStrength;
            mFadeParam.X = 1.0f / (mShadowDistance - FadeStartDistance + 0.0001f);
            mFadeParam.Y = -FadeStartDistance * mFadeParam.X;

            //render Shadow Caster

            mVisParameter.CullCamera = GBuffers.Camera;
            world.GatherVisibleMeshes(mVisParameter);

            var cmdlist = BasePass.DrawCmdList.mCoreObject;
            cmdlist.ClearMeshDrawPassArray();
            cmdlist.SetViewport(GBuffers.ViewPort.mCoreObject);
            foreach (var i in mVisParameter.VisibleMeshes)
            {
                if (i.HostNode == null || i.HostNode.IsCastShadow == false)
                    continue;
                if (i.Atoms == null)
                    continue;

                for (int j = 0; j < i.Atoms.Length; j++)
                {
                    var drawcall = i.GetDrawCall(GBuffers, j, policy, IRenderPolicy.EShadingType.DepthPass);
                    if (drawcall != null)
                    {
                        GBuffers.SureCBuffer(drawcall.Effect, "UMobileEditorFSPolicy");
                        drawcall.BindGBuffer(GBuffers);

                        cmdlist.PushDrawCall(drawcall.mCoreObject);
                    }
                }
            }

            cmdlist.BeginCommand();
            if (bClear)
                cmdlist.BeginRenderPass(ref PassDesc, GBuffers.FrameBuffers.mCoreObject);
            else
                cmdlist.BeginRenderPass((RenderPassDesc*)0, GBuffers.FrameBuffers.mCoreObject);
            cmdlist.BuildRenderPass(0);
            cmdlist.EndRenderPass();
            cmdlist.EndCommand();
        }

        public unsafe void TickRender()
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var cmdlist = BasePass.CommitCmdList.mCoreObject;
            cmdlist.Commit(rc.mCoreObject);
        }
        public void TickSync()
        {
            BasePass.SwapBuffer();
            GBuffers?.Camera?.mCoreObject.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, 1);
        }
    }
}
