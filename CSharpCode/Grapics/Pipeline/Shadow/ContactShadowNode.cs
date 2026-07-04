using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline.Shadow
{
    // TODO: Consider inlining contact shadow ray march into DeferredDirLighting.cginc
    // instead of a separate compute pass. UE does it inline — zero extra bandwidth
    // (no intermediate texture write/read), zero sync cost, per-light toggle.
    // Current independent pass is fine for multi-light sharing but wastes a full-screen
    // depth read + R8 write/read + dispatch barrier for the common single-directional-light case.

    /// <summary>
    /// Compute ShadingEnv for Contact Shadow screen-space ray march.
    /// All resource binding happens here (§1.2 compliant).
    /// </summary>
    public class TtContactShadowComputeShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        // Permutation: ENV_USE_HZB (0=linear fallback, 1=HZB accelerated)
        public TtPermutationItem EnableHzbAccel { get; set; }
        // Permutation: ENV_USE_NORMAL_BIAS (0=no normal bias, 1=bias ray origin along surface normal)
        public TtPermutationItem EnableNormalBias { get; set; }

        public TtContactShadowComputeShading()
        {
            CodeName = RName.GetRName("Shaders/Compute/ScreenSpace/ContactShadow.compute", RName.ERNameType.Engine);
            MainName = "CS_ContactShadow";

            this.BeginPermutaion();
            EnableHzbAccel = this.PushPermutation<EPermutation_Bool>("ENV_USE_HZB", (int)EPermutation_Bool.BitWidth);
            EnableHzbAccel.SetValue((int)EPermutation_Bool.FalseValue);
            EnableNormalBias = this.PushPermutation<EPermutation_Bool>("ENV_USE_NORMAL_BIAS", (int)EPermutation_Bool.BitWidth);
            EnableNormalBias.SetValue((int)EPermutation_Bool.FalseValue);
            UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtContactShadowNode;
            if (node == null)
                return;

            // Bind depth buffer from input pin
            var depthBuffer = node.GetAttachBuffer(node.DepthPinIn);
            if (depthBuffer?.Srv != null)
            {
                drawcall.BindSrv("DepthBuffer", depthBuffer.Srv);
                drawcall.BindSampler("Samp_DepthBuffer",
                    TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            }

            // Bind GBufferRT1 for normal-based ray origin bias (ENV_USE_NORMAL_BIAS == 1)
            if (node.IsNormalBiasConnected)
            {
                var gbufferRT1 = node.GetAttachBuffer(node.GBufferRT1PinIn);
                if (gbufferRT1?.Srv != null)
                {
                    drawcall.BindSrv("GBufferRT1", gbufferRT1.Srv);
                    drawcall.BindSampler("Samp_GBufferRT1",
                        TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
                }
            }

            // Bind GBufferRT3 for RenderFlags (AcceptShadow check)
            var gbufferRT3 = node.GetAttachBuffer(node.GBufferRT3PinIn);
            if (gbufferRT3?.Srv != null)
            {
                drawcall.BindSrv("GBufferRT3", gbufferRT3.Srv);
                drawcall.BindSampler("Samp_GBufferRT3",
                    TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            }

            // Bind HZB texture when available (ENV_USE_HZB == 1)
            if (node.IsHzbConnected)
            {
                var hzbAttach = node.GetAttachBuffer(node.HzbPinIn);
                if (hzbAttach?.Srv != null)
                {
                    drawcall.BindSrv("HzbTexture", hzbAttach.Srv);
                    drawcall.BindSampler("Samp_HzbTexture",
                        TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
                }
            }

            // Bind output UAV from RenderGraph-managed attachment
            var outputAttach = node.GetAttachBuffer(node.ShadowMaskPinOut);
            if (outputAttach?.Uav != null)
                drawcall.BindUav("ContactShadowMask", outputAttach.Uav);

            // Bind cbPerCamera (for ViewPrjMtx / ViewPrjInvMtx / LinearFromDepth)
            drawcall.BindCBV("cbPerCamera", policy.DefaultCamera.PerCameraCBuffer);

            // Bind cbContactShadow
            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbContactShadow");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateCBuffer(cbBinder));
        }
    }

    /// <summary>
    /// Contact Shadow RenderGraph Node.
    /// Dispatches a compute shader that ray-marches in screen space along the
    /// light direction to produce a per-pixel shadow mask for close-range detail
    /// shadows that traditional shadow maps miss.
    /// Output texture is managed by RenderGraph via ShadowMaskPinOut attachment.
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("ContactShadow", "Shadow\\ContactShadow",
        Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtContactShadowNode : TAuxRenderGraphNode<TtContactShadowNode>
    {
        // ---- Pins ----
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput(
            "Depth", EBufferType.BFT_SRV);

        public TtRenderGraphPin GBufferRT1PinIn = TtRenderGraphPin.CreateInput(
            "GBufferRT1", EBufferType.BFT_SRV);

        public TtRenderGraphPin GBufferRT3PinIn = TtRenderGraphPin.CreateInput(
            "GBufferRT3", EBufferType.BFT_SRV);

        public TtRenderGraphPin HzbPinIn = TtRenderGraphPin.CreateInput(
            "Hzb", EBufferType.BFT_SRV);

        public TtRenderGraphPin ShadowMaskPinOut = TtRenderGraphPin.CreateOutput(
            "ShadowMask", false, EPixelFormat.PXF_R8_UNORM,
            EBufferType.BFT_SRV | EBufferType.BFT_UAV);

        /// <summary>
        /// Whether HZB pin is connected and has valid data.
        /// Used by ShadingEnv.OnDrawCall to decide whether to bind HZB resources.
        /// </summary>
        internal bool IsHzbConnected;

        /// <summary>
        /// Whether GBufferRT1 pin is connected for normal-biased ray origin.
        /// </summary>
        internal bool IsNormalBiasConnected;

        // ---- Parameters ----
        [Rtti.Meta("")]
        [Category("Contact Shadow")]
        public float ContactShadowLength { get; set; } = 0.5f;

        [Rtti.Meta("")]
        [Category("Contact Shadow")]
        public int NumSteps { get; set; } = 12;

        [Rtti.Meta("")]
        [Category("Contact Shadow")]
        public float DepthBias { get; set; } = 0.001f;

        [Rtti.Meta("")]
        [Category("Contact Shadow")]
        public float FadeDistance { get; set; } = 50.0f;

        [Rtti.Meta("")]
        [Category("Contact Shadow")]
        public float FadeLength { get; set; } = 20.0f;

        [Rtti.Meta("")]
        [Category("Contact Shadow")]
        public float ShadowIntensity { get; set; } = 0.8f;

        // ---- Internal ----
        TtContactShadowComputeShading mShading;
        TtComputeDraw mDrawCall;
        TtCbView mCBuffer;

        public TtContactShadowNode()
        {
            Name = "ContactShadowNode";
        }

        public override void InitNodePins()
        {
            AddInput(DepthPinIn);
            GBufferRT1PinIn.IsAllowInputNull = true;
            AddInput(GBufferRT1PinIn);
            AddInput(GBufferRT3PinIn);
            HzbPinIn.IsAllowInputNull = true;
            AddInput(HzbPinIn);
            AddOutput(ShadowMaskPinOut);
        }

        public override async Thread.Async.TtTask Initialize(
            TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mShading = await TtShadingEnv.CreateShadingEnv<TtContactShadowComputeShading>();

            var renderContext = TtEngine.Instance.GfxDevice.RenderContext;
            mDrawCall = renderContext.CreateComputeDraw();
            mDrawCall.TagObject = this;

            // Graph 结构在 Initialize 后就固定了, Pin 连接状态不会运行时变化.
            IsHzbConnected = (HzbPinIn.FindInLinker() != null);
            IsNormalBiasConnected = (GBufferRT1PinIn.FindInLinker() != null);

            bool needUpdate = false;
            if (IsHzbConnected)
            {
                mShading.EnableHzbAccel.SetValue(true);
                needUpdate = true;
            }
            if (IsNormalBiasConnected)
            {
                mShading.EnableNormalBias.SetValue(true);
                needUpdate = true;
            }
            if (needUpdate)
                mShading.UpdatePermutation().AddWaitTask();
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mDrawCall);
            CoreSDK.DisposeObject(ref mCBuffer);
            base.Dispose();
        }

        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            ShadowMaskPinOut.Attachement.Width = (uint)x;
            ShadowMaskPinOut.Attachement.Height = (uint)y;
        }

        // Cached world reference, set in Tick, consumed in OnDrawCall
        internal GamePlay.TtWorld CachedWorld;

        /// <summary>
        /// §1.1 compliant: first CreateCBV fills all fields + MarkDirty + FlushDirty.
        /// </summary>
        public TtCbView GetOrCreateCBuffer(FShaderBinder binder)
        {
            if (mCBuffer == null)
            {
                mCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                FillCBufferValues(mCBuffer, CachedWorld);
                mCBuffer.MarkDirty();
                mCBuffer.FlushDirty();
                return mCBuffer;
            }
            FillCBufferValues(mCBuffer, CachedWorld);
            return mCBuffer;
        }

        void FillCBufferValues(TtCbView cb, GamePlay.TtWorld world)
        {
            var lightDir = GetDirLightDirection(world);
            cb.SetValue("LightDirection", in lightDir);
            cb.SetValue("NumSteps", NumSteps);

            var screenSize = new Vector2(
                ShadowMaskPinOut.Attachement.Width,
                ShadowMaskPinOut.Attachement.Height);
            cb.SetValue("ScreenSize", in screenSize);
            cb.SetValue("ContactShadowLength", ContactShadowLength);
            cb.SetValue("DepthBias", DepthBias);
            cb.SetValue("FadeDistance", FadeDistance);
            cb.SetValue("FadeLength", FadeLength);
            cb.SetValue("ShadowIntensity", ShadowIntensity);

            cb.SetValue("FrameIndex", (uint)TtEngine.Instance.FrameCount);
        }

        Vector3 GetDirLightDirection(GamePlay.TtWorld world)
        {
            if (world?.DirectionLight != null)
            {
                var toLight = -world.DirectionLight.Direction;
                var length = toLight.Length();
                if (length > 0.001f)
                    return toLight / length;
            }
            return new Vector3(0, 1, 0);
        }
        TtAttachBuffer mFallbackShadowMask = new TtAttachBuffer();
        public override void FrameBuild(TtRenderPolicy policy)
        {
            if (policy.ContactShadowMode != Deferred.EContactShadowMode.InputNode)
            {
                this.ImportAttachment(ShadowMaskPinOut, mFallbackShadowMask);
                mFallbackShadowMask.Srv = TtEngine.Instance.GfxDevice.TextureManager.WhiteTextureSRV;
            }
        }
        public override unsafe void Tick(GamePlay.TtWorld world,
            TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (policy.ContactShadowMode != Deferred.EContactShadowMode.InputNode)
                return;

            if (mShading == null || !mShading.IsReady)
                return;
            if (mDrawCall == null)
                return;

            var outputAttach = GetAttachBuffer(ShadowMaskPinOut);
            if (outputAttach?.Uav == null)
                return;

            uint width = ShadowMaskPinOut.Attachement.Width;
            uint height = ShadowMaskPinOut.Attachement.Height;
            if (width == 0 || height == 0)
                return;

            CachedWorld = world;

            // SetDrawcallDispatch expects pixel dimensions, not group counts.
            // It internally does Roundup(x, DispatchArg.X) to compute group count.
            var cmd = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmd, "ContactShadow"))
            {
                mShading.SetDrawcallDispatch(this, policy, mDrawCall,
                    width, height, 1, true);
                cmd.PushGpuDraw(mDrawCall);
                cmd.FlushDraws();
            }
            policy.CommitCommandList(cmd, "ContactShadow");
        }
    }
}
