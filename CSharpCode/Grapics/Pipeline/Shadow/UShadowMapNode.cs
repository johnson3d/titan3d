using EngineNS.Bricks.VXGI;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Graphics.Pipeline.Shadow
{
    public class UShadowShading : Shader.UGraphicsShadingEnv
    {
        public UShadowShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/SSM.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position, NxRHI.EVertexStreamType.VST_UV,};
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
            };
        }
        public override void OnBuildDrawCall(URenderPolicy policy, NxRHI.UGraphicDraw drawcall)
        {
            var shadowMapNode = policy.FindFirstNode<UShadowMapNode>();
            if (shadowMapNode == null)
                return;

            drawcall.mCoreObject.BindPipeline(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, shadowMapNode.DepthRaster.mCoreObject);
        }
    }
    public class UShadowMapNode : Common.URenderGraphNode
    {
        public Common.URenderGraphPin ColorPinOut = Common.URenderGraphPin.CreateOutput("Color", false, EPixelFormat.PXF_B8G8R8A8_UNORM);
        public Common.URenderGraphPin DepthPinOut = Common.URenderGraphPin.CreateOutput("Depth", false, EPixelFormat.PXF_D24_UNORM_S8_UINT);
        public UShadowMapNode()
        {
            Name = "ShadowMap";
        }
        public override void InitNodePins()
        {
            AddOutput(ColorPinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddOutput(DepthPinOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
        }
        public GamePlay.UWorld.UVisParameter mVisParameter = new GamePlay.UWorld.UVisParameter();
        // public CCamera ShadowCamera;
        private UCamera[] mShadowCameraArray;
        public UCamera ViewerCamera;
        public UGraphicsBuffers[] GBuffersArray;
        public UDrawBuffers[] CSMPass = new UDrawBuffers[4];
        
        public NxRHI.UGpuPipeline DepthRaster;

        private UInt32 mResolutionX = 1024; //3072
        protected UInt32 mResolutionY = 1024; //4096;
        private UInt32 mBorderSize = 2;//4;
        private UInt32 mInnerResolutionX = 1024 - 2 * 2; //5120; //4096 - 4 * 2;
        private UInt32 mInnerResolutionY = 1024 - 2 * 2; //5120; //4096 - 4 * 2;
        private UInt32 mWholeReslutionX = 1024 * 4;
        private UInt32 mWholeReslutionY = 1024;

        //private Vector3 mDirLightCameraPos = new Vector3(0.0f, 0.0f, 0.0f);
        //private Vector3 mDirLightCameraLookAtPos = new Vector3(0.0f, 0.0f, 0.0f);
        public Vector3 mDirLightDirection = new Vector3(0.0f, -1.5f, 1.0f);

        public float mShadowDistance = 1200.0f;
        private float mShadowCameraOffset = 100.0f;
        public UInt32 mCsmNum = 4;
        public float[] mSumDistanceFarArray = new float[4];
        public Vector4 mSumDistanceFarVec = new Vector4();

        public float mFadeStrength = 0.2f;
        public Vector2 mFadeParam = new Vector2(0.0f, 0.0f);
        public float[] mShadowTransitionScaleArray = new float[4];
        public Vector4 mShadowTransitionScaleVec = new Vector4();
        
        public float UniformDepthBias { get; set; } = 1.0f;
        public float mShadowTransitionScale = 1000.0f;
        public Vector4 mShadowMapSizeAndRcp = new Vector4();
        public Matrix[] mViewer2ShadowMtxArray = new Matrix[4];
        public Matrix mViewer2ShadowMtx = new Matrix();
        private Matrix mOrtho2UVMtx = new Matrix();
        private Matrix[] mUVAdjustedMtxArray = new Matrix[4];

        private Vector3[] mSSM_FrustumVtx = new Vector3[8];
        public UShadowShading mShadowShading;

        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            mShadowShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<Shadow.UShadowShading>();

           

            // ShadowCamera = new CCamera();
            // {
            //     ShadowCamera.mCoreObject.PerspectiveFovLH(3.14f / 4f, 1, 1, 0.3f, 1000.0f);
            //     var eyePos = new DVector3(0, 0, -10);
            //     ShadowCamera.mCoreObject.LookAtLH(in eyePos, in DVector3.Zero, in Vector3.Up);
            // }

            mShadowCameraArray = new UCamera[4];
            for (UInt32 CamIdx = 0; CamIdx < mCsmNum; CamIdx++)
            {
                mShadowCameraArray[CamIdx] = new UCamera();
                mShadowCameraArray[CamIdx] .mCoreObject.PerspectiveFovLH(3.14f / 4f, 1, 1, 0.3f, 1000.0f);
                var eyePos = new DVector3(0, 0, -10);
                mShadowCameraArray[CamIdx] .mCoreObject.LookAtLH(in eyePos, in DVector3.Zero, in Vector3.Up);
            }

            GBuffersArray = new UGraphicsBuffers[4];
            {
                var PassDesc = new NxRHI.FRenderPassDesc();
                unsafe
                {
                    PassDesc.NumOfMRT = 1;
                    PassDesc.AttachmentMRTs[0].Format = EPixelFormat.PXF_B8G8R8A8_UNORM;
                    PassDesc.AttachmentMRTs[0].Samples = 1;
                    PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                    PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                    PassDesc.m_AttachmentDepthStencil.Format = DepthPinOut.Attachement.Format;
                    PassDesc.m_AttachmentDepthStencil.Samples = 1;
                    PassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                    PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                    PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                    PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                    //PassDesc.mFBClearColorRT0 = TempClearColor;
                    //PassDesc.mDepthClearValue = 1.0f;
                    //PassDesc.mStencilClearValue = 0u;
                }
                NxRHI.URenderPass RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

                GBuffersArray[0] = new UGraphicsBuffers();
                GBuffersArray[0].Initialize(policy, RenderPass);
                //GBuffers.CreateGBuffer(0, EPixelFormat.PXF_UNKNOWN, (uint)x, (uint)y);            
                GBuffersArray[0].SetRenderTarget(policy, 0, ColorPinOut);
                GBuffersArray[0].SetDepthStencil(policy, DepthPinOut);

                GBuffersArray[0].TargetViewIdentifier = new UGraphicsBuffers.UTargetViewIdentifier();
                GBuffersArray[0].SetSize(mResolutionX, mResolutionY);

                var PassDescTwo = new NxRHI.FRenderPassDesc();
                unsafe
                {
                    PassDescTwo.NumOfMRT = 1;
                    PassDescTwo.AttachmentMRTs[0].Format = EPixelFormat.PXF_B8G8R8A8_UNORM;
                    PassDescTwo.AttachmentMRTs[0].Samples = 1;
                    PassDescTwo.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionLoad;
                    PassDescTwo.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                    PassDescTwo.m_AttachmentDepthStencil.Format = DepthPinOut.Attachement.Format;
                    PassDescTwo.m_AttachmentDepthStencil.Samples = 1;
                    PassDescTwo.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionLoad;
                    PassDescTwo.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                    PassDescTwo.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionLoad;
                    PassDescTwo.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                    //PassDesc.mFBClearColorRT0 = TempClearColor;
                    //PassDesc.mDepthClearValue = 1.0f;
                    //PassDesc.mStencilClearValue = 0u;
                }
                NxRHI.URenderPass RenderPassTwo = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDescTwo);

                for (UInt32 CamIdx = 1; CamIdx < mCsmNum; CamIdx++)
                {
                    GBuffersArray[CamIdx] = new UGraphicsBuffers();

                    GBuffersArray[CamIdx].Initialize(policy, RenderPassTwo);
                    //GBuffers.CreateGBuffer(0, EPixelFormat.PXF_UNKNOWN, (uint)x, (uint)y);
                    GBuffersArray[CamIdx].SetRenderTarget(policy, 0, ColorPinOut);
                    GBuffersArray[CamIdx].SetDepthStencil(policy, DepthPinOut);

                    GBuffersArray[CamIdx].TargetViewIdentifier = new UGraphicsBuffers.UTargetViewIdentifier();
                    GBuffersArray[CamIdx].SetSize(mResolutionX, mResolutionY);
                }
            }

            for (UInt32 CamIdx = 0; CamIdx < mCsmNum; CamIdx++)
            {
                CSMPass[CamIdx] = new UDrawBuffers();
                CSMPass[CamIdx].Initialize(rc, debugName);
            }

            mShadowMapSizeAndRcp.X = mWholeReslutionX;
            mShadowMapSizeAndRcp.Y = mWholeReslutionY;
            mShadowMapSizeAndRcp.Z = 1.0f / mWholeReslutionX;
            mShadowMapSizeAndRcp.W = 1.0f / mWholeReslutionY;

            ColorPinOut.Attachement.Width = mWholeReslutionX;
            ColorPinOut.Attachement.Height = mWholeReslutionY;
            DepthPinOut.Attachement.Width = mWholeReslutionX;
            DepthPinOut.Attachement.Height = mWholeReslutionY;

            if (UEngine.Instance.GfxDevice.RenderContext.RhiType == NxRHI.ERhiType.RHI_GL)
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
            else if (UEngine.Instance.GfxDevice.RenderContext.RhiType == NxRHI.ERhiType.RHI_VK)
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

            var dpRastDesc = new NxRHI.FGpuPipelineDesc();
            dpRastDesc.SetDefault();
            dpRastDesc.m_Rasterizer.m_DepthBias = 0;
            dpRastDesc.m_Rasterizer.m_SlopeScaledDepthBias = 1;
            DepthRaster = UEngine.Instance.GfxDevice.PipelineManager.GetPipelineState(UEngine.Instance.GfxDevice.RenderContext, in dpRastDesc);


            for (UInt32 UVAdjustIdx = 0; UVAdjustIdx < mCsmNum; UVAdjustIdx++)
            {
                mUVAdjustedMtxArray[UVAdjustIdx].M11 = (float)mInnerResolutionX / (float)mWholeReslutionX;
                mUVAdjustedMtxArray[UVAdjustIdx].M22 = (float)mInnerResolutionY / (float)mWholeReslutionY;
                mUVAdjustedMtxArray[UVAdjustIdx].M33 = 1.0f;
                mUVAdjustedMtxArray[UVAdjustIdx].M44 = 1.0f;
                mUVAdjustedMtxArray[UVAdjustIdx].M41 = (float)(mResolutionX * UVAdjustIdx + mBorderSize) / (float)mWholeReslutionX;
                mUVAdjustedMtxArray[UVAdjustIdx].M42 = (float)mBorderSize / (float)mWholeReslutionY;
            }


            float r = 0.9f;
            float f = mShadowDistance;
            float n = 1.0f;

            mSumDistanceFarArray[0] = r * n * (float)Math.Pow(f/ n, 0.25f) + (1.0f - r) * (n + 0.25f * (f - n));// (float)Math.Pow(mShadowDistance, 0.25f);// 
            mSumDistanceFarArray[1] = r * n * (float)Math.Pow(f / n, 0.5f) + (1.0f - r) * (n + 0.5f * (f - n));// (float)Math.Pow(mShadowDistance, 0.5f);//
            mSumDistanceFarArray[2] = r * n * (float)Math.Pow(f / n, 0.75f) + (1.0f - r) * (n + 0.75f * (f - n));// (float)Math.Pow(mShadowDistance, 0.75f);//
            mSumDistanceFarArray[3] = mShadowDistance;//

            mSumDistanceFarVec.X = mSumDistanceFarArray[0];
            mSumDistanceFarVec.Y = mSumDistanceFarArray[1];
            mSumDistanceFarVec.Z = mSumDistanceFarArray[2];
            mSumDistanceFarVec.W = mSumDistanceFarArray[3];
        }
        public override void Dispose()
        {
            if (GBuffersArray != null)
            {
                for (UInt32 CamIdx = 1; CamIdx < mCsmNum; CamIdx++)
                {
                    CoreSDK.DisposeObject(ref GBuffersArray[CamIdx]);
                    //GBuffers.CreateGBuffer(0, EPixelFormat.PXF_UNKNOWN, (uint)x, (uint)y);
                }
                GBuffersArray = null;
            }

            base.Dispose();
        }
        private static float Clamp(float X, float Min, float Max)
        {
            return X < Min ? Min : X < Max ? X : Max;
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UShadowMapNode), nameof(TickLogic));
        [ThreadStatic]
        private static Profiler.TimeScope ScopePushGpuDraw = Profiler.TimeScopeManager.GetTimeScope(typeof(UShadowMapNode), "PushGpuDraw");
        public override unsafe void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                mDirLightDirection = world.DirectionLight.Direction;

                ViewerCamera = policy.DefaultCamera;

                //ViewCamera.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext);
                //calculate viewer camera frustum bounding sphere and shadow camera data;
                for (UInt32 CsmIdx = 0; CsmIdx < mCsmNum; CsmIdx++)
                {
                    float HalfFoV = ViewerCamera.mCoreObject.mFov * 0.5f;
                    float zNear = 0.0f;
                    if (CsmIdx == 0)
                    {
                        zNear = ViewerCamera.ZNear;
                    }
                    else
                    {
                        zNear = mSumDistanceFarArray[CsmIdx - 1];
                    }
                    float NearUpLength = zNear * (float)Math.Tan(HalfFoV);
                    float NearRightLength = NearUpLength * ViewerCamera.mCoreObject.mAspect;
                    Vector3 NearUpOffset = NearUpLength * ViewerCamera.mCoreObject.GetUp();
                    Vector3 NearRightOffset = NearRightLength * ViewerCamera.mCoreObject.GetRight();


                    float ShadowDistance = mSumDistanceFarArray[CsmIdx];
                    float FarUpLength = ShadowDistance * (float)Math.Tan(HalfFoV);
                    float FarRightLength = FarUpLength * ViewerCamera.mCoreObject.mAspect;
                    Vector3 FarUpOffset = FarUpLength * ViewerCamera.mCoreObject.GetUp();
                    Vector3 FarRightOffset = FarRightLength * ViewerCamera.mCoreObject.GetRight();

                    var position = ViewerCamera.mCoreObject.GetLocalPosition();
                    var direction = ViewerCamera.mCoreObject.GetDirection();
                    mSSM_FrustumVtx[0] = position + direction * zNear + NearRightOffset + NearUpOffset;//nrt;
                    mSSM_FrustumVtx[1] = position + direction * zNear + NearRightOffset - NearUpOffset;//nrb;
                    mSSM_FrustumVtx[2] = position + direction * zNear - NearRightOffset + NearUpOffset;//nlt;
                    mSSM_FrustumVtx[3] = position + direction * zNear - NearRightOffset - NearUpOffset;//nlb;
                    mSSM_FrustumVtx[4] = position + direction * ShadowDistance + FarRightOffset + FarUpOffset;//frt;
                    mSSM_FrustumVtx[5] = position + direction * ShadowDistance + FarRightOffset - FarUpOffset;//frb;
                    mSSM_FrustumVtx[6] = position + direction * ShadowDistance - FarRightOffset + FarUpOffset;//flt;
                    mSSM_FrustumVtx[7] = position + direction * ShadowDistance - FarRightOffset - FarUpOffset;//flb;

                    float TanHalfFoVHeight = (float)Math.Tan(HalfFoV);
                    float TanHalfFoVWidth = TanHalfFoVHeight * ViewerCamera.mCoreObject.mAspect;
                    float FrustumLength = ShadowDistance - zNear;

                    float NearHalfWidth = zNear * TanHalfFoVWidth;
                    float NearHalfHeight = zNear * TanHalfFoVHeight;
                    float NearHalfDiagonalSqr = NearHalfWidth * NearHalfWidth + NearHalfHeight * NearHalfHeight;
                    float FarHalfWidth = ShadowDistance * TanHalfFoVWidth;
                    float FarHalfHeight = ShadowDistance * TanHalfFoVHeight;
                    float FarHalfDiagonalSqr = FarHalfWidth * FarHalfWidth + FarHalfHeight * FarHalfHeight;

                    float OptimalOffset = FrustumLength * 0.5f + (NearHalfDiagonalSqr - FarHalfDiagonalSqr) / (2.0f * FrustumLength);
                    float ViewerPosOffset = Clamp(ShadowDistance - OptimalOffset, zNear, ShadowDistance);
                    var FrustumSphereCenter = position + direction * ViewerPosOffset;
                    double FrustumSphereRadius = 1.0;
                    for (UInt32 idx = 0; idx < 8; idx++)
                    {
                        FrustumSphereRadius = Math.Max(FrustumSphereRadius, Vector3.DistanceSquared(FrustumSphereCenter, mSSM_FrustumVtx[idx]));
                    }
                    //FrustumSphereRadius = (float)Math.Ceiling((float)Math.Sqrt(FrustumSphereRadius));
                    float MaxFrustumDiagonal = Vector3.Distance(mSSM_FrustumVtx[3], mSSM_FrustumVtx[4]);
                    FrustumSphereRadius = Math.Min((float)Math.Sqrt(FrustumSphereRadius), MaxFrustumDiagonal * 0.5f);

                    var ShadowCameraPos = FrustumSphereCenter - mDirLightDirection * ((float)FrustumSphereRadius + mShadowCameraOffset);

                    var shadowCamera = mShadowCameraArray[CsmIdx].mCoreObject;

                    shadowCamera.LookAtLH(ShadowCameraPos.AsDVector(), FrustumSphereCenter.AsDVector(), in Vector3.UnitY);
                    float FrustumSphereDiameter = (float)FrustumSphereRadius * 2.0f;
                    float ShadowCameraZFar = mShadowCameraOffset + FrustumSphereDiameter;
                    float ShadowCameraZNear = 0.0f;
                    shadowCamera.MakeOrtho(FrustumSphereDiameter, FrustumSphereDiameter, ShadowCameraZNear, ShadowCameraZFar);
                    Vector4 WorldCenterNDC = Vector4.Transform(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), shadowCamera.GetViewProjection());
                    float TexelPosX = (WorldCenterNDC.X * 0.5f + 0.5f) * mInnerResolutionX;
                    float TexelPosAdjustedNdcX = ((float)Math.Floor(TexelPosX) / mInnerResolutionX - 0.5f) / 0.5f;
                    float TexelOffsetNdcX = TexelPosAdjustedNdcX - WorldCenterNDC.X;
                    float TexelPosY = (WorldCenterNDC.Y * (-0.5f) + 0.5f) * mInnerResolutionY;
                    float TexelPosAdjustedNdcY = ((float)Math.Floor(TexelPosY) / mInnerResolutionY - 0.5f) / (-0.5f);
                    float TexelOffsetNdcY = TexelPosAdjustedNdcY - WorldCenterNDC.Y;

                    //render Shadow Caster
                    mVisParameter.CullType = GamePlay.UWorld.UVisParameter.EVisCull.Shadow;
                    mVisParameter.World = world;
                    mVisParameter.CullCamera = mShadowCameraArray[CsmIdx];

                    if (mVisParameter.VisibleNodes == null)
                    {
                        mVisParameter.VisibleNodes = new List<GamePlay.Scene.UNode>();
                    }

                    world.GatherVisibleMeshes(mVisParameter);

                    BoundingBox AABB = new BoundingBox();
                    AABB.InitEmptyBox();
                    foreach (var i in mVisParameter.VisibleNodes)
                    {
                        //if (i.IsCastShadow == false)
                        //    continue;

                        BoundingBox MeshAABB = i.AABB.ToSingleAABB();
                        if (!MeshAABB.IsEmpty())
                        {
                            AABB.Merge(MeshAABB.Minimum);
                            AABB.Merge(MeshAABB.Maximum);
                        }
                    }

                    bool NeedOriShadowPro = true;

                    if (!AABB.IsEmpty())
                    {
                        Matrix LookAtLHMat = Matrix.LookAtLH(ShadowCameraPos, FrustumSphereCenter, Vector3.UnitY);
                        //Matrix LookAtLHMat = Matrix.LookAtLH(-mDirLightDirection, Vector3.Zero, Vector3.UnitY);
                        Matrix ShadowProj = new Matrix();
                        Matrix.Transpose(in LookAtLHMat, out ShadowProj);
                        Vector3[] AABBCorners = AABB.GetCorners();
                        //Vector3* NewPoints = stackalloc Vector3[8];
                        Vector3[] NewPoints = new Vector3[8];
                       
                        for (int i = 0; i < 8; i++)
                        {
                            Vector4 TempPoints = Vector3.Transform(AABBCorners[i], in ShadowProj);
                            float w = 1.0f;// TempPoints.W == 0.0f ? 1.0f : 1.0f / TempPoints.W;
                            NewPoints[i].X = TempPoints.X * w;
                            NewPoints[i].Y = TempPoints.Y * w;
                            NewPoints[i].Z = TempPoints.Z * w;
                        }

                        BoundingBox ShadowBound = BoundingBox.FromPoints(NewPoints);

                        if ((ShadowBound.Maximum.X - ShadowBound.Minimum.X) + 10.0f  < FrustumSphereDiameter && (ShadowBound.Maximum.Z - ShadowBound.Minimum.Z)+ 10.0f < FrustumSphereDiameter)
                        {
                            //shadowCamera.LookAtLH(-mDirLightDirection.AsDVector(), Vector3.Zero.AsDVector(), in Vector3.UnitY);

                            NeedOriShadowPro = false;
                            TexelOffsetNdcX = (ShadowBound.Maximum.X + ShadowBound.Minimum.X) / (ShadowBound.Maximum.X - ShadowBound.Minimum.X);
                            TexelOffsetNdcY = (ShadowBound.Maximum.Z + ShadowBound.Minimum.Z) / (ShadowBound.Maximum.Z - ShadowBound.Minimum.Z);
                            //shadowCamera.DoOrthoProjectionForShadow(ShadowBound.Maximum.X - ShadowBound.Minimum.X, ShadowBound.Maximum.Z - ShadowBound.Minimum.Z, ShadowCameraZNear, ShadowCameraZFar, TexelOffsetNdcX, TexelOffsetNdcY);
                            shadowCamera.DoOrthoProjectionForShadow((ShadowBound.Maximum.X - ShadowBound.Minimum.X) +10.0f, (ShadowBound.Maximum.Z - ShadowBound.Minimum.Z) + 10.0f, ShadowCameraZNear, ShadowCameraZFar, TexelOffsetNdcX, TexelOffsetNdcY);
                        }
                       
                    }
                    
                    if(NeedOriShadowPro)
                    {
                        shadowCamera.DoOrthoProjectionForShadow(FrustumSphereDiameter, FrustumSphereDiameter, ShadowCameraZNear, ShadowCameraZFar, TexelOffsetNdcX, TexelOffsetNdcY);
                    }
                   
                    //shadowCamera.DoOrthoProjectionForShadow(FrustumSphereDiameter, FrustumSphereDiameter, ShadowCameraZNear, ShadowCameraZFar, TexelOffsetNdcX, TexelOffsetNdcY);
                    // Matrix vp = shadowCamera.GetViewProjection();
                    // Matrix result;
                    // Matrix.Transpose(in vp, out result);

                    // mViewer2ShadowMtx = mOrtho2UVMtx * result;

                    Matrix UVAdjustedMtx;
                    Matrix.Transpose(in mUVAdjustedMtxArray[CsmIdx], out UVAdjustedMtx);

                    Matrix vp = shadowCamera.GetViewProjection();
                    Matrix ViewProjection;
                    Matrix.Transpose(in vp, out ViewProjection);

                    mViewer2ShadowMtxArray[CsmIdx] = UVAdjustedMtx * (mOrtho2UVMtx * ViewProjection);//mShadowCameraArray[CsmIdx].CameraData.ViewProjection * mOrtho2UVMtx * mUVAdjustedMtxArray[CsmIdx];

                    float PerObjCustomDepthBias = 1.0f;
                    float DepthBiasClipSpace = UniformDepthBias / (ShadowCameraZFar - ShadowCameraZNear) * (FrustumSphereDiameter / mInnerResolutionY) * PerObjCustomDepthBias;
                    //float DepthBiasClipSpace = UniformDepthBias / (ShadowCameraZFar - ShadowCameraZNear);

                    var coreBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
                    var cBuffer = GBuffersArray[CsmIdx].PerViewportCBuffer;
                    if (cBuffer != null)
                    {
                        var tmp = new Vector2(0.0f, 1.0f / ShadowCameraZFar);
                        cBuffer.SetValue(coreBinder.CBPerViewport.gDepthBiasAndZFarRcp, in tmp);
                    }

                    //mShadowTransitionScale = 1.0f / (DepthBiasClipSpace + 0.00001f);
                    mShadowTransitionScaleArray[CsmIdx] = 1.0f / (DepthBiasClipSpace + 0.00001f);

                    float FadeStartDistance = ShadowDistance - ShadowDistance * mFadeStrength;
                    mFadeParam.X = 1.0f / (ShadowDistance - FadeStartDistance + 0.0001f);
                    mFadeParam.Y = -FadeStartDistance * mFadeParam.X;

                    mShadowCameraArray[CsmIdx].UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext);
                    CSMPass[CsmIdx].SwapBuffer();

                    var cmdlist = CSMPass[CsmIdx].DrawCmdList;
                    cmdlist.BeginCommand();
                    using (new Profiler.TimeScopeHelper(ScopePushGpuDraw))
                    {
                        foreach (var i in mVisParameter.VisibleMeshes)
                        {
                            if (i.IsCastShadow == false)
                                continue;
                            if (i.Atoms == null)
                                continue;

                            for (int j = 0; j < i.Atoms.Count; j++)
                            {
                                var drawcall = i.GetDrawCall(cmdlist.mCoreObject, GBuffersArray[CsmIdx], j, policy, URenderPolicy.EShadingType.DepthPass, this);

                                if (drawcall != null)
                                {
                                    drawcall.BindGBuffer(mShadowCameraArray[CsmIdx], GBuffersArray[CsmIdx]);

                                    cmdlist.PushGpuDraw(drawcall);
                                }
                            }
                        }
                    }

                    {
                        FViewPort Viewport = new FViewPort();
                        Viewport.MinDepth = GBuffersArray[CsmIdx].Viewport.MinDepth;
                        Viewport.MaxDepth = GBuffersArray[CsmIdx].Viewport.MaxDepth;
                        Viewport.TopLeftX = GBuffersArray[CsmIdx].Viewport.TopLeftX + (GBuffersArray[CsmIdx].Viewport.Width * CsmIdx);
                        Viewport.TopLeftY = GBuffersArray[CsmIdx].Viewport.TopLeftY;

                        Viewport.Width = (GBuffersArray[CsmIdx].Viewport.Width);// GBuffers.Viewport.Width;
                        Viewport.Height = GBuffersArray[CsmIdx].Viewport.Height;

                        cmdlist.SetViewport(in Viewport);

                        var passClear = new NxRHI.FRenderPassClears();
                        //if (CsmIdx == 0)
                        {
                            passClear.SetDefault();
                            passClear.SetClearColor(0, new Color4f(1, 0, 0, 0));
                        }

                        GBuffersArray[CsmIdx].BuildFrameBuffers(policy);
                        string PassName = "ShadowDepth";
                        if (CsmIdx == 1)
                        {
                            PassName = "ShadowDepth1";
                        }
                        else if (CsmIdx == 2)
                        {
                            PassName = "ShadowDepth2";
                        }
                        else if (CsmIdx == 3)
                        {
                            PassName = "ShadowDepth3";
                        }
                        cmdlist.BeginPass(GBuffersArray[CsmIdx].FrameBuffers, in passClear, PassName);
                        //if (bClear)
                        //    cmdlist.BeginRenderPass(policy, GBuffers, in passClear, "ShadowDepth");
                        //else
                        //    cmdlist.BeginRenderPass(policy, GBuffers, "ShadowDepth");
                        cmdlist.FlushDraws();
                        cmdlist.EndPass();
                    }

                    cmdlist.EndCommand();
                    UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmdlist);
                }

                mShadowTransitionScaleVec.X = mShadowTransitionScaleArray[0];
                mShadowTransitionScaleVec.Y = mShadowTransitionScaleArray[1];
                mShadowTransitionScaleVec.Z = mShadowTransitionScaleArray[2];
                mShadowTransitionScaleVec.W = mShadowTransitionScaleArray[3];
               
            }   
        }

        public override void TickSync(URenderPolicy policy)
        {
            //base.SwapBuffer();
            //ShadowCamera.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext);
        }
    }
}
