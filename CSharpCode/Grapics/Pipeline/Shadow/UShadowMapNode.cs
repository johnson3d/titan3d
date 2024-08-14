using EngineNS.Bricks.VXGI;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.Graphics.Mesh;
using NPOI.SS.Formula.Functions;

namespace EngineNS.Graphics.Pipeline.Shadow
{
    public class UShadowShading : Shader.TtGraphicsShadingEnv
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
    [Bricks.CodeBuilder.ContextMenu("CSM", "Shadow\\CSM", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class UShadowMapNode : TtRenderGraphNode
    {
        public TtRenderGraphPin[] VisiblePinIn = new TtRenderGraphPin[]{
            TtRenderGraphPin.CreateInput("Visible0"),
            TtRenderGraphPin.CreateInput("Visible1"),
            TtRenderGraphPin.CreateInput("Visible2"),
            TtRenderGraphPin.CreateInput("Visible3"),
        };
        //public TtRenderGraphPin ColorPinOut = TtRenderGraphPin.CreateOutput("Color", false, EPixelFormat.PXF_B8G8R8A8_UNORM);
        public TtRenderGraphPin DepthPinOut = TtRenderGraphPin.CreateOutput("Depth", false, EPixelFormat.PXF_D24_UNORM_S8_UINT);
        public UShadowMapNode()
        {
            Name = "ShadowMap";
        }
        public override void InitNodePins()
        {
            foreach (var i in VisiblePinIn)
            {
                AddInput(i, NxRHI.EBufferType.BFT_NONE);
                i.IsAllowInputNull = true;
            }

            //AddOutput(ColorPinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddOutput(DepthPinOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
        }
        public GamePlay.UWorld.UVisParameter mVisParameter = new GamePlay.UWorld.UVisParameter();
        // public CCamera ShadowCamera;
        private UCamera[] mShadowCameraArray;
        public UCamera ViewerCamera;
        public UCamera CullCamera;
        public TtGraphicsBuffers[] GBuffersArray;
        public UDrawBuffers[] CSMPass = new UDrawBuffers[4];

        TtCpuCullingNode[] CSMCullingNode = new TtCpuCullingNode[4];

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

        public UShadowShading mShadowShading;

        public override TtGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mShadowShading;
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            mShadowShading = await UEngine.Instance.ShadingEnvManager.GetShadingEnv<Shadow.UShadowShading>();

            mShadowCameraArray = new UCamera[4];
            for (UInt32 CamIdx = 0; CamIdx < mCsmNum; CamIdx++)
            {
                mShadowCameraArray[CamIdx] = new UCamera();
                mShadowCameraArray[CamIdx] .mCoreObject.PerspectiveFovLH(3.14f / 4f, 1, 1, 0.3f, 1000.0f);
                var eyePos = new DVector3(0, 0, -10);
                mShadowCameraArray[CamIdx] .mCoreObject.LookAtLH(in eyePos, in DVector3.Zero, in Vector3.Up);

                policy.AddCamera($"CSM_Camera_{CamIdx}", mShadowCameraArray[CamIdx]);
            }

            foreach (var i in VisiblePinIn)
            {
                var linker = i.FindInLinker();
                if (linker != null)
                {
                    CSMCullingNode[0] = linker.OutPin.HostNode as TtCpuCullingNode;
                    CSMCullingNode[0].VisParameter.CullCamera = mShadowCameraArray[0];
                }
            }

            GBuffersArray = new TtGraphicsBuffers[4];
            unsafe
            {
                var PassDesc = new NxRHI.FRenderPassDesc();
                PassDesc.NumOfMRT = 0;
                //PassDesc.AttachmentMRTs[0].Format = EPixelFormat.PXF_B8G8R8A8_UNORM;
                //PassDesc.AttachmentMRTs[0].Samples = 1;
                //PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                //PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.Format = DepthPinOut.Attachement.Format;
                PassDesc.m_AttachmentDepthStencil.Samples = 1;
                PassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = TempClearColor;
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
                NxRHI.URenderPass RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

                GBuffersArray[0] = new TtGraphicsBuffers();
                GBuffersArray[0].Initialize(policy, RenderPass);
                //GBuffers.CreateGBuffer(0, EPixelFormat.PXF_UNKNOWN, (uint)x, (uint)y);            
                //GBuffersArray[0].SetRenderTarget(policy, 0, ColorPinOut);
                GBuffersArray[0].SetDepthStencil(policy, DepthPinOut);

                GBuffersArray[0].TargetViewIdentifier = new TtGraphicsBuffers.TtTargetViewIdentifier();
                GBuffersArray[0].SetSize(mResolutionX, mResolutionY);

                var PassDescTwo = new NxRHI.FRenderPassDesc();
                PassDescTwo.NumOfMRT = 0;
                //PassDescTwo.AttachmentMRTs[0].Format = EPixelFormat.PXF_B8G8R8A8_UNORM;
                //PassDescTwo.AttachmentMRTs[0].Samples = 1;
                //PassDescTwo.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionLoad;
                //PassDescTwo.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDescTwo.m_AttachmentDepthStencil.Format = DepthPinOut.Attachement.Format;
                PassDescTwo.m_AttachmentDepthStencil.Samples = 1;
                PassDescTwo.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionLoad;
                PassDescTwo.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDescTwo.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionLoad;
                PassDescTwo.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                NxRHI.URenderPass RenderPassTwo = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDescTwo);

                for (UInt32 CamIdx = 1; CamIdx < mCsmNum; CamIdx++)
                {
                    GBuffersArray[CamIdx] = new TtGraphicsBuffers();

                    GBuffersArray[CamIdx].Initialize(policy, RenderPassTwo);
                    //GBuffers.CreateGBuffer(0, EPixelFormat.PXF_UNKNOWN, (uint)x, (uint)y);
                    //GBuffersArray[CamIdx].SetRenderTarget(policy, 0, ColorPinOut);
                    GBuffersArray[CamIdx].SetDepthStencil(policy, DepthPinOut);

                    GBuffersArray[CamIdx].TargetViewIdentifier = new TtGraphicsBuffers.TtTargetViewIdentifier();
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

            //ColorPinOut.Attachement.Width = mWholeReslutionX;
            //ColorPinOut.Attachement.Height = mWholeReslutionY;
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
            mOrtho2UVMtx = Matrix.Transpose(in mOrtho2UVMtx);

            var dpRastDesc = new NxRHI.FGpuPipelineDesc();
            dpRastDesc.SetDefault();
            dpRastDesc.m_Rasterizer.m_DepthBias = 1;
            dpRastDesc.m_Rasterizer.m_SlopeScaledDepthBias = 2.0f;
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

            CullCamera = new UCamera();
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
            foreach (var i in CSMCullingNode)
            {
                if (i != null)
                {
                    i.VisParameter.CullType = GamePlay.UWorld.UVisParameter.EVisCull.Shadow;
                    i.VisParameter.World = world;
                }
            }
            DVector3* AABBCorners = stackalloc DVector3[8];
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                mDirLightDirection = world.DirectionLight.Direction;

                ViewerCamera = policy.DefaultCamera;
                
                //ViewCamera.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext);
                //calculate viewer camera frustum bounding sphere and shadow camera data;
                for (UInt32 CsmIdx = 0; CsmIdx < mCsmNum; CsmIdx++)
                {
                    float ShadowDistance = mSumDistanceFarArray[CsmIdx];
                    var shadowCamera = mShadowCameraArray[CsmIdx].mCoreObject;

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
                    //先得到阴影裁剪摄像机，确保CSM区段内看得到的对象都产生投影 
                    var aabb = new DBoundingBox();
                    {
                        CullCamera.PerspectiveFovLH(ViewerCamera.Fov, ViewerCamera.Width, ViewerCamera.Height, ViewerCamera.ZNear, mSumDistanceFarArray[CsmIdx]);
                        CullCamera.LookAtLH(ViewerCamera.GetPosition(), ViewerCamera.GetLookAt(), in Vector3.UnitY);
                        //收集本csm阶段可投影Mesh
                        mVisParameter.CullType = GamePlay.UWorld.UVisParameter.EVisCull.Shadow;
                        mVisParameter.IsBuildAABB = true;
                        mVisParameter.World = world;
                        mVisParameter.CullCamera = CullCamera;
                        world.GatherVisibleMeshes(mVisParameter);

                        aabb = mVisParameter.AABB;
                    }

                    //先按照方向光构建一个阴影摄像机坐标系 
                    var viewSpaceMatrix = new DMatrix();
                    var invViewSpaceMatrix = new DMatrix();
                    var cameralStart = CullCamera.GetMatrixStartPosition();
                    var pFrustumVerts = CullCamera.Frustum.GetFrustumVertices();
                    var frstAABB = new BoundingBox();
                    CullCamera.Frustum.GetAABB(ref frstAABB);
                    var eyeAt = cameralStart + frstAABB.GetCenter();
                    var lookAt = eyeAt + mDirLightDirection;
                    DMatrix.LookAtLH(in eyeAt, in lookAt, in DVector3.UnitY, out viewSpaceMatrix);
                    DMatrix.Invert(in viewSpaceMatrix, out invViewSpaceMatrix);

                    //CSM本阶段能看到的frustum在阴影摄像机空间的shadowCameraBox1
                    var shadowCameraBox1 = new DBoundingBox();
                    {
                        shadowCameraBox1.InitEmptyBox();
                        for (int i = 0; i < 8; i++)
                        {
                            var vert = cameralStart + pFrustumVerts[i];
                            shadowCameraBox1.Merge(DVector3.TransformCoordinate(in vert, in viewSpaceMatrix));
                        }
                    }

                    //对裁剪出来的aabb变换到阴影摄像机空间shadowCameraBox2
                    var shadowCameraBox2 = new DBoundingBox();
                    {
                        shadowCameraBox2.InitEmptyBox();
                        aabb.UnsafeGetCorners(AABBCorners);
                        for (int i = 0; i < 8; i++)
                        {
                            shadowCameraBox2.Merge(DVector3.TransformCoordinate(in AABBCorners[i], in viewSpaceMatrix));
                        }
                    }

                    //shadowCameraBox1和shadowCameraBox2的交集就是最佳的阴影投射区间 
                    var shadowCameraBox = DBoundingBox.IntersectBox(in shadowCameraBox1, in shadowCameraBox2);
                    //将阴影投射区间变换到世界坐标
                    var center = shadowCameraBox.GetCenter();
                    var extent = shadowCameraBox.GetSize() * 0.5;
                    center = DVector3.TransformCoordinate(in center, in invViewSpaceMatrix);
                    var AABB = new DBoundingBox(center - extent, center + extent);

                    //根据AABB制作最终真正的阴影摄像机
                    float BoxExt = 1.2f;
                    var FrustumSphereDiameter = (float)AABB.GetMaxSide() * BoxExt;
                    shadowCamera.LookAtLH(center - mDirLightDirection.AsDVector() * FrustumSphereDiameter * 0.5f, center, in Vector3.UnitY);
                    //这里 0.3 - 1000是为了配合后面PCF的时候mShadowTransitionScale设置的1000做的
                    //因为采用的非线性Depth，作比较的时候做了放大mShadowTransitionScale倍做的比较->return saturate((ShadowmapDepth - SFD.mViewer2ShadowDepth) * SFD.mShadowTransitionScale + 1.0h);
                    var shadowZNear = 0.3f;// (\float)shadowCameraBox.Minimum.Z;
                    var shadowZFar = 1000.0f;
                    shadowCamera.DoOrthoProjectionForShadow(FrustumSphereDiameter, FrustumSphereDiameter, shadowZNear, shadowZFar, 0, 0);

                    Matrix vp = shadowCamera.GetViewProjection();
                    mViewer2ShadowMtxArray[CsmIdx] = vp * mOrtho2UVMtx * mUVAdjustedMtxArray[CsmIdx];//mShadowCameraArray[CsmIdx].CameraData.ViewProjection * mOrtho2UVMtx * mUVAdjustedMtxArray[CsmIdx];

                    float PerObjCustomDepthBias = 1.5f;
                    float DepthBiasClipSpace = UniformDepthBias / (shadowZFar - shadowZNear) * (FrustumSphereDiameter / mInnerResolutionY) * PerObjCustomDepthBias;
                    //float DepthBiasClipSpace = UniformDepthBias / (ShadowCameraZFar - ShadowCameraZNear) * PerObjCustomDepthBias;

                    var coreBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
                    var cBuffer = GBuffersArray[CsmIdx].PerViewportCBuffer;
                    if (cBuffer != null)
                    {
                        var tmp = new Vector2(0.0f, 1.0f / shadowZFar);
                        cBuffer.SetValue(coreBinder.CBPerViewport.gDepthBiasAndZFarRcp, in tmp);
                    }

                    //mShadowTransitionScale = 1.0f / (DepthBiasClipSpace + 0.00001f);
                    mShadowTransitionScaleArray[CsmIdx] = 1.0f / (DepthBiasClipSpace + 0.00001f);
                    mShadowTransitionScale = mShadowTransitionScaleArray[CsmIdx];
                    float FadeStartDistance = ShadowDistance - ShadowDistance * mFadeStrength;
                    mFadeParam.X = 1.0f / (ShadowDistance - FadeStartDistance + 0.0001f);
                    mFadeParam.Y = -FadeStartDistance * mFadeParam.X;

                    mShadowCameraArray[CsmIdx].UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext);
                    CSMPass[CsmIdx].SwapBuffer();

                    var cmdlist = CSMPass[CsmIdx].DrawCmdList;
                    using (new NxRHI.TtCmdListScope(cmdlist))
                    {
                        using (new Profiler.TimeScopeHelper(ScopePushGpuDraw))
                        {
                            foreach (var i in mVisParameter.VisibleMeshes)
                            {
                                if (i.Mesh.IsCastShadow == false)
                                    continue;
                                if (i.DrawMode == FVisibleMesh.EDrawMode.Instance)
                                    continue;
                                foreach (var j in i.Mesh.SubMeshes)
                                {
                                    foreach (var k in j.Atoms)
                                    {
                                        var drawcall = k.GetDrawCall(cmdlist.mCoreObject, GBuffersArray[CsmIdx], policy, this);

                                        if (drawcall != null)
                                        {
                                            drawcall.BindGBuffer(mShadowCameraArray[CsmIdx], GBuffersArray[CsmIdx]);

                                            cmdlist.PushGpuDraw(drawcall);
                                        }
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

                    }

                    policy.CommitCommandList(cmdlist);
                }

                //TODO  Global Value...
                mShadowTransitionScaleVec.X = 1000.0f;// mShadowTransitionScaleArray[0];
                mShadowTransitionScaleVec.Y = 1000.0f;//mShadowTransitionScaleArray[1];
                mShadowTransitionScaleVec.Z = 1000.0f;//mShadowTransitionScaleArray[2];
                mShadowTransitionScaleVec.W = 1000.0f;//mShadowTransitionScaleArray[3];

            }   
        }

        public override void TickSync(URenderPolicy policy)
        {
            //base.SwapBuffer();
            //ShadowCamera.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext);
        }
    }
}
