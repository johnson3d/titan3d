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
        public override EVertexStreamType[] GetNeedStreams()
        {
            return new EVertexStreamType[] { EVertexStreamType.VST_Position};
        }
        public override void OnBuildDrawCall(URenderPolicy policy, RHI.CDrawCall drawcall)
        {
            var shadowMapNode = policy.FindFirstNode<UShadowMapNode>();
            if (shadowMapNode == null)
                return;

            drawcall.mCoreObject.GetPipeline().BindRasterizerState(shadowMapNode.DepthRaster.mCoreObject);
        }
    }
    public class UShadowMapNode : Common.URenderGraphNode
    {
        public Common.URenderGraphPin DepthPinOut = Common.URenderGraphPin.CreateOutput("Depth", false, EPixelFormat.PXF_D24_UNORM_S8_UINT);
        public UShadowMapNode()
        {
            Name = "ShadowMap";
        }
        public override void InitNodePins()
        {
            AddOutput(DepthPinOut, EGpuBufferViewType.GBVT_Dsv | EGpuBufferViewType.GBVT_Srv);
        }
        public GamePlay.UWorld.UVisParameter mVisParameter = new GamePlay.UWorld.UVisParameter();
        public CCamera ShadowCamera;
        public UGraphicsBuffers GBuffers { get; protected set; } = new UGraphicsBuffers();
        public UDrawBuffers BasePass = new UDrawBuffers();
        public RHI.CRenderPass RenderPass;
        public RHI.CRasterizerState DepthRaster;

        private UInt32 mResolutionX = 4608; //3072
        private UInt32 mResolutionY = 4608; //4096;
        //private UInt32 mBorderSize = 0;//4;
        //private UInt32 mInnerResolutionX = 4608; //5120; //4096 - 4 * 2;
        private UInt32 mInnerResolutionY = 4608; //5120; //4096 - 4 * 2;

        //private Vector3 mDirLightCameraPos = new Vector3(0.0f, 0.0f, 0.0f);
        //private Vector3 mDirLightCameraLookAtPos = new Vector3(0.0f, 0.0f, 0.0f);
        public Vector3 mDirLightDirection = new Vector3(0.0f, -1.5f, 1.0f);

        public float mShadowDistance = 40.0f;
        private float mShadowCameraOffset = 100.0f;

        public float mFadeStrength = 0.2f;
        public Vector2 mFadeParam = new Vector2(0.0f, 0.0f);
        public float UniformDepthBias { get; set; } = 2.0f;
        public float mShadowTransitionScale = 1000.0f;
        public Vector4 mShadowMapSizeAndRcp = new Vector4();
        public Matrix mViewer2ShadowMtx = new Matrix();
        private Matrix mOrtho2UVMtx = new Matrix();

        private Vector3[] mSSM_FrustumVtx = new Vector3[8];
        public UShadowShading mShadowShading;

        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            mShadowShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<Shadow.UShadowShading>();

            var PassDesc = new IRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 0;
                PassDesc.AttachmentMRTs[0].Format = EPixelFormat.PXF_UNKNOWN;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionDontCare;
                PassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionDontCare;
                PassDesc.m_AttachmentDepthStencil.Format = DepthPinOut.Attachement.Format;
                PassDesc.m_AttachmentDepthStencil.Samples = 1;
                PassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = TempClearColor;
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
            }
            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in PassDesc);

            ShadowCamera = new CCamera();
            {
                ShadowCamera.mCoreObject.PerspectiveFovLH(3.14f / 4f, 1, 1, 0.3f, 1000.0f);
                var eyePos = new DVector3(0, 0, -10);
                ShadowCamera.mCoreObject.LookAtLH(in eyePos, in DVector3.Zero, in Vector3.Up);
            }
            policy.AddCamera("DirLightShadow", ShadowCamera);
            GBuffers.Initialize(policy, RenderPass);
            //GBuffers.CreateGBuffer(0, EPixelFormat.PXF_UNKNOWN, (uint)x, (uint)y);            
            GBuffers.SetDepthStencil(policy, DepthPinOut);

            GBuffers.TargetViewIdentifier = new UGraphicsBuffers.UTargetViewIdentifier();
            GBuffers.OnResize(mInnerResolutionY, mInnerResolutionY);

            BasePass = new UDrawBuffers();
            BasePass.Initialize(rc, debugName);

            mShadowMapSizeAndRcp.X = mResolutionX;
            mShadowMapSizeAndRcp.Y = mResolutionY;
            mShadowMapSizeAndRcp.Z = 1.0f / mResolutionX;
            mShadowMapSizeAndRcp.W = 1.0f / mResolutionY;

            DepthPinOut.Attachement.Width = mResolutionX;
            DepthPinOut.Attachement.Height = mResolutionY;

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
            else if (UEngine.Instance.GfxDevice.RenderContext.RHIType == ERHIType.RHT_VULKAN)
            {
                //Vulkan;
                mOrtho2UVMtx.M11 = 0.5f;
                mOrtho2UVMtx.M22 = -0.5f;
                mOrtho2UVMtx.M33 = 1.0f;
                mOrtho2UVMtx.M44 = 1.0f;
                mOrtho2UVMtx.M14 = 0.5f;
                mOrtho2UVMtx.M24 = 0.5f;
                mOrtho2UVMtx.M34 = 0.0f;
            }
            else
            {
                // D3D 
                mOrtho2UVMtx.M11 = 0.5f;
                mOrtho2UVMtx.M22 = -0.5f;
                mOrtho2UVMtx.M33 = 1.0f;
                mOrtho2UVMtx.M44 = 1.0f;
                mOrtho2UVMtx.M14 = 0.5f;
                mOrtho2UVMtx.M24 = 0.5f;
                mOrtho2UVMtx.M34 = 0.0f;
            }

            mVisParameter.VisibleMeshes = new List<Mesh.UMesh>();

            var dpRastDesc = new IRasterizerStateDesc();
            dpRastDesc.SetDefault();
            dpRastDesc.DepthBias = 1;
            dpRastDesc.SlopeScaledDepthBias = 2;
            DepthRaster = UEngine.Instance.GfxDevice.RasterizerStateManager.GetPipelineState(UEngine.Instance.GfxDevice.RenderContext, in dpRastDesc);
        }
        public override void Cleanup()
        {
            GBuffers?.Cleanup();
            GBuffers = null;

            base.Cleanup();
        }
        private static float Clamp(float X, float Min, float Max)
        {
            return X < Min ? Min : X < Max ? X : Max;
        }
        public override unsafe void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            mDirLightDirection = world.DirectionLight.Direction;

            var ViewerCamera = policy.DefaultCamera;
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

            var position = ViewerCamera.mCoreObject.GetLocalPosition();
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
            var FrustumSphereCenter = position + direction * ViewerPosOffset;
            double FrustumSphereRadius = 1.0;
            for (UInt32 idx = 0; idx < 8; idx++)
            {
                FrustumSphereRadius = Math.Max(FrustumSphereRadius, Vector3.DistanceSquared(FrustumSphereCenter, mSSM_FrustumVtx[idx]));
            }
            FrustumSphereRadius = (float)Math.Ceiling((float)Math.Sqrt(FrustumSphereRadius));
            //FrustumSphereRadius = Math.Min((float)Math.Sqrt(FrustumSphereRadius), MaxFrustumDiagonal * 0.5f);

            var ShadowCameraPos = FrustumSphereCenter - mDirLightDirection * ((float)FrustumSphereRadius + mShadowCameraOffset);

            var shadowCamera = ShadowCamera.mCoreObject;
            shadowCamera.LookAtLH(ShadowCameraPos.AsDVector(), FrustumSphereCenter.AsDVector(), in Vector3.UnitY);
            float FrustumSphereDiameter = (float)FrustumSphereRadius * 2.0f;
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
            Matrix.Transpose(in vp, out result);

            mViewer2ShadowMtx = mOrtho2UVMtx * result;

            float PerObjCustomDepthBias = 1.0f;
            float DepthBiasClipSpace = UniformDepthBias / (ShadowCameraZFar - ShadowCameraZNear) * (FrustumSphereDiameter / mResolutionY) * PerObjCustomDepthBias;
            //float DepthBiasClipSpace = UniformDepthBias / (ShadowCameraZFar - ShadowCameraZNear);

            var cBuffer = GBuffers.PerViewportCBuffer;
            if (cBuffer != null)
            {
                var tmp = new Vector2(0.0f, 1.0f / ShadowCameraZFar);
                cBuffer.SetValue(cBuffer.PerViewportIndexer.gDepthBiasAndZFarRcp, in tmp);
            }

            mShadowTransitionScale = 1.0f / (DepthBiasClipSpace + 0.00001f);

            float FadeStartDistance = mShadowDistance - mShadowDistance * mFadeStrength;
            mFadeParam.X = 1.0f / (mShadowDistance - FadeStartDistance + 0.0001f);
            mFadeParam.Y = -FadeStartDistance * mFadeParam.X;

            //render Shadow Caster
            mVisParameter.CullType = GamePlay.UWorld.UVisParameter.EVisCull.Shadow;
            mVisParameter.World = world;
            mVisParameter.CullCamera = ShadowCamera;
            world.GatherVisibleMeshes(mVisParameter);

            var cmdlist = BasePass.DrawCmdList;
            cmdlist.ClearMeshDrawPassArray();            
            foreach (var i in mVisParameter.VisibleMeshes)
            {
                if (i.HostNode == null || i.HostNode.IsCastShadow == false)
                    continue;
                if (i.Atoms == null)
                    continue;

                for (int j = 0; j < i.Atoms.Length; j++)
                {
                    var drawcall = i.GetDrawCall(GBuffers, j, policy, URenderPolicy.EShadingType.DepthPass, this);
                    if (drawcall != null)
                    {
                        drawcall.BindGBuffer(ShadowCamera, GBuffers);

                        cmdlist.PushDrawCall(drawcall.mCoreObject);
                    }
                }
            }

            if(cmdlist.BeginCommand())
            {
                cmdlist.SetViewport(GBuffers.ViewPort.mCoreObject);

                var passClear = new IRenderPassClears();
                //passClear.SetDefault();
                passClear.m_DepthClearValue = 1.0f;
                passClear.m_StencilClearValue = 0;
                cmdlist.BeginRenderPass(policy, GBuffers, in passClear, "ShadowDepth");
                //if (bClear)
                //    cmdlist.BeginRenderPass(policy, GBuffers, in passClear, "ShadowDepth");
                //else
                //    cmdlist.BeginRenderPass(policy, GBuffers, "ShadowDepth");
                cmdlist.BuildRenderPass(0);
                cmdlist.EndRenderPass();
                cmdlist.EndCommand();
            }
        }

        public override unsafe void TickRender(URenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var cmdlist = BasePass.CommitCmdList.mCoreObject;
            cmdlist.Commit(rc.mCoreObject);
        }
        public override void TickSync(URenderPolicy policy)
        {
            BasePass.SwapBuffer();
            ShadowCamera.mCoreObject.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, 1);
        }
    }
}
