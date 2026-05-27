using System;
using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;

namespace EngineNS.Bricks.AdvanceShadow
{
    /// <summary>
    /// Compute shader ShadingEnv for SMRT (Shadow Map Ray Tracing) soft shadows.
    /// Marches along the light ray in the VSM page pool atlas to produce
    /// contact-hardening penumbra based on occluder distance.
    /// </summary>
    public class TtVSMSoftShadowShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtVSMSoftShadowShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/AdvanceShadow/VSMSoftShadow.compute", RName.ERNameType.Engine);
            MainName = "CS_SoftShadow";
            UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtVSMSoftShadowNode;
            if (node == null)
                return;

            // Bind GBuffer depth
            drawcall.BindSrv("DepthBuffer", node.GetDepthSrv());
            drawcall.BindSampler("Samp_DepthBuffer", TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            // Bind shadow atlas
            drawcall.BindSrv("ShadowAtlas", node.GetShadowAtlasSrv());
            drawcall.BindSampler("Samp_ShadowAtlas", TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            // Bind page table
            drawcall.BindSrv("PageTable", node.GetPageTableSrv());

            // Bind output shadow mask UAV
            drawcall.BindUav("ShadowMask", node.GetShadowMaskUav());

            // Bind cbuffer
            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbVSMSoftShadow");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateCBuffer(cbBinder));
        }
    }

    /// <summary>
    /// RenderGraph node that produces a soft shadow mask using SMRT.
    /// Reads GBuffer depth + VSM page pool atlas, outputs a screen-space shadow factor texture.
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("VSMSoftShadow", "Shadow\\VSMSoftShadow", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtVSMSoftShadowNode : TAuxRenderGraphNode<TtVSMSoftShadowNode>
    {
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput("Depth", EBufferType.BFT_SRV);
        public TtRenderGraphPin AdvShadowPinIn = TtRenderGraphPin.CreateInput("AdvShadow", EBufferType.BFT_NONE);
        public TtRenderGraphPin ShadowMaskPinOut = TtRenderGraphPin.CreateOutput("ShadowMask", true, EPixelFormat.PXF_R16_FLOAT, EBufferType.BFT_SRV | EBufferType.BFT_UAV);

        private TtVSMSoftShadowShading mShading;
        private TtComputeDraw mDrawcall;
        private TtCbView mCBuffer;

        // Shadow mask output texture
        private TtTexture mShadowMaskTexture;
        private TtUaView mShadowMaskUav;
        private TtSrView mShadowMaskSrv;

        // Cached references
        private TtSrView mCurrentDepthSrv;
        private TtAdvanceShadowMapNode mAdvShadowNode;
        private TtRenderPolicy mCachedPolicy;

        // Quality settings
        [System.ComponentModel.Category("SMRT")]
        [Rtti.Meta("")]
        public int RayStepCount { get; set; } = 16;

        [System.ComponentModel.Category("SMRT")]
        [Rtti.Meta("")]
        public float MaxRayDistance { get; set; } = 50.0f;

        [System.ComponentModel.Category("SMRT")]
        [Rtti.Meta("")]
        public float LightSourceRadius { get; set; } = 1.0f;

        [System.ComponentModel.Category("SMRT")]
        [Rtti.Meta("")]
        public float ShadowBias { get; set; } = 0.001f;

        public TtVSMSoftShadowNode()
        {
            Name = "VSMSoftShadow";
        }

        public override void InitNodePins()
        {
            AddInput(DepthPinIn);
            AddInput(AdvShadowPinIn);
            AdvShadowPinIn.LinkType = "AdvShadow";
            AddOutput(ShadowMaskPinOut);
        }

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            mShading = await TtShadingEnv.CreateShadingEnv<TtVSMSoftShadowShading>();
            mDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateComputeDraw();
        }

        public override void Dispose()
        {
            mShadowMaskUav?.Dispose();
            mShadowMaskUav = null;
            mShadowMaskSrv?.Dispose();
            mShadowMaskSrv = null;
            CoreSDK.DisposeObject(ref mShadowMaskTexture);
            CoreSDK.DisposeObject(ref mCBuffer);
            base.Dispose();
        }

        public override unsafe void Tick(TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            mCachedPolicy = policy;

            // Resolve connected AdvShadow node
            mAdvShadowNode = policy.FindFirstNode<TtAdvanceShadowMapNode>();
            if (mAdvShadowNode == null || mAdvShadowNode.PagePool == null)
                return;

            // Resolve depth SRV
            var depthAttach = GetAttachBuffer(DepthPinIn);
            if (depthAttach == null)
                return;
            mCurrentDepthSrv = depthAttach.Srv;

            // Ensure shadow mask texture exists
            var camera = policy.DefaultCamera;
            if (camera == null)
                return;

            uint screenWidth = (uint)camera.Width;
            uint screenHeight = (uint)camera.Height;
            EnsureShadowMask(screenWidth, screenHeight);

            // Dispatch
            mShading.SetDrawcallDispatch(this, policy, mDrawcall, screenWidth, screenHeight, 1, true);

            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new TtCmdListScope(cmdlist, "VSMSoftShadow"))
            {
                cmdlist.PushGpuDraw(mDrawcall);
                cmdlist.FlushDraws();
            }
            policy.CommitCommandList(cmdlist, "VSMSoftShadow");

            // Export shadow mask via output pin
            var attachment = ImportAttachment(ShadowMaskPinOut, null);
            if (attachment != null)
            {
                attachment.GpuResource = mShadowMaskTexture;
                attachment.Srv = mShadowMaskSrv;
            }
        }

        #region Resource Access (called by ShadingEnv.OnDrawCall)

        public TtSrView GetDepthSrv() => mCurrentDepthSrv;

        public TtSrView GetShadowAtlasSrv() => mAdvShadowNode?.PagePool?.PhysicalPoolSRV;

        public TtSrView GetPageTableSrv() => mAdvShadowNode?.PagePool?.PageTableBuffer?.Srv;

        public TtUaView GetShadowMaskUav() => mShadowMaskUav;

        public TtCbView GetOrCreateCBuffer(FShaderBinder binder)
        {
            if (mCBuffer == null)
            {
                mCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);

                // §1.1: Fill all fields immediately after CreateCBV
                mCBuffer.SetValue("InvViewProjection", Matrix.Identity);
                mCBuffer.SetValue("ShadowViewProjection", Matrix.Identity);
                mCBuffer.SetValue("LightDirection", Vector3.Down);
                mCBuffer.SetValue("LightSourceRadius", LightSourceRadius);
                mCBuffer.SetValue("ScreenSize", new Vector2(1920, 1080));
                mCBuffer.SetValue("ShadowAtlasSize", new Vector2(2048, 2048));
                mCBuffer.SetValue("MaxRayDistance", MaxRayDistance);
                mCBuffer.SetValue("RayStepCount", RayStepCount);
                mCBuffer.SetValue("ShadowBias", ShadowBias);
                mCBuffer.SetValue("PageResolution", mAdvShadowNode?.PagePool?.Config.PageResolution ?? 128);
                mCBuffer.SetValue("PoolDimPages", mAdvShadowNode?.PagePool?.Config.PoolDimPages ?? 16);
                mCBuffer.MarkDirty();
                mCBuffer.FlushDirty();
            }

            // Update per-frame dynamic values
            var camera = mCachedPolicy?.DefaultCamera;
            if (camera != null)
            {
                var viewProj = camera.GetViewProjection();
                var invViewProj = Matrix.Invert(in viewProj);
                mCBuffer.SetValue("InvViewProjection", invViewProj);
                mCBuffer.SetValue("ScreenSize", new Vector2(camera.Width, camera.Height));
            }

            if (mAdvShadowNode?.mShadowQTree != null)
            {
                mCBuffer.SetValue("LightDirection", mAdvShadowNode.mShadowQTree.LightDirection);
            }

            // Use clipmap level 0 VP if available, otherwise fall back to QTree projection
            if (mAdvShadowNode?.Clipmap != null && mAdvShadowNode.Clipmap.Levels.Length > 0)
            {
                mCBuffer.SetValue("ShadowViewProjection", mAdvShadowNode.Clipmap.Levels[0].ViewProjection);
            }

            if (mAdvShadowNode?.PagePool != null)
            {
                var poolConfig = mAdvShadowNode.PagePool.Config;
                float atlasSize = poolConfig.PoolTextureResolution;
                mCBuffer.SetValue("ShadowAtlasSize", new Vector2(atlasSize, atlasSize));
                mCBuffer.SetValue("PageResolution", poolConfig.PageResolution);
                mCBuffer.SetValue("PoolDimPages", poolConfig.PoolDimPages);
            }

            mCBuffer.SetValue("MaxRayDistance", MaxRayDistance);
            mCBuffer.SetValue("RayStepCount", RayStepCount);
            mCBuffer.SetValue("LightSourceRadius", LightSourceRadius);
            mCBuffer.SetValue("ShadowBias", ShadowBias);

            return mCBuffer;
        }

        #endregion

        #region Private Implementation

        private uint mShadowMaskWidth;
        private uint mShadowMaskHeight;

        private void EnsureShadowMask(uint width, uint height)
        {
            // If texture exists and dimensions match, nothing to do
            if (mShadowMaskTexture != null && mShadowMaskWidth == width && mShadowMaskHeight == height)
                return;

            // Dimensions changed or first creation — release old resources and recreate
            if (mShadowMaskTexture != null)
            {
                mShadowMaskUav?.Dispose();
                mShadowMaskUav = null;
                mShadowMaskSrv?.Dispose();
                mShadowMaskSrv = null;
                CoreSDK.DisposeObject(ref mShadowMaskTexture);
            }

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var desc = new FTextureDesc();
            desc.SetDefault();
            desc.BindFlags = EBufferType.BFT_SRV | EBufferType.BFT_UAV;
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = 1;
            desc.Format = EPixelFormat.PXF_R16_FLOAT;
            mShadowMaskTexture = rc.CreateTexture(in desc);
            mShadowMaskTexture.SetDebugName("VSM_SoftShadowMask");

            var srvDesc = new FSrvDesc();
            srvDesc.SetTexture2D();
            srvDesc.Format = EPixelFormat.PXF_R16_FLOAT;
            srvDesc.Texture2D.MipLevels = 1;
            srvDesc.Texture2D.MostDetailedMip = 0;
            mShadowMaskSrv = rc.CreateSRV(mShadowMaskTexture, in srvDesc);

            var uavDesc = new FUavDesc();
            uavDesc.SetTexture2D();
            uavDesc.Format = EPixelFormat.PXF_R16_FLOAT;
            uavDesc.Texture2D.MipSlice = 0;
            mShadowMaskUav = rc.CreateUAV(mShadowMaskTexture, in uavDesc);

            mShadowMaskWidth = width;
            mShadowMaskHeight = height;
        }

        #endregion
    }
}
