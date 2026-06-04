using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline.Common.Post
{
    /// <summary>
    /// Compute ShadingEnv for SSAO horizon search (pass 1).
    /// Outputs raw AO to an intermediate texture.
    /// </summary>
    public class TtSSAOComputeShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtSSAOComputeShading()
        {
            CodeName = RName.GetRName("Shaders/Compute/ScreenSpace/SSAO.compute", RName.ERNameType.Engine);
            MainName = "CS_SSAO";

            this.BeginPermutaion();
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtSSAONode;
            if (node == null)
                return;

            var depthBuffer = node.GetAttachBuffer(node.DepthPinIn);
            if (depthBuffer?.Srv != null)
            {
                drawcall.BindSrv("DepthBuffer", depthBuffer.Srv);
                drawcall.BindSampler("Samp_DepthBuffer",
                    TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            }

            var gbufferRT1 = node.GetAttachBuffer(node.GBufferRT1PinIn);
            if (gbufferRT1?.Srv != null)
            {
                drawcall.BindSrv("GBufferRT1", gbufferRT1.Srv);
                drawcall.BindSampler("Samp_GBufferRT1",
                    TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            }

            // Write raw AO to intermediate output pin UAV
            var rawAOAttach = node.GetAttachBuffer(node.RawAOPinOut);
            if (rawAOAttach?.Uav != null)
                drawcall.BindUav("AOOutput", rawAOAttach.Uav);

            drawcall.BindCBV("cbPerCamera", policy.DefaultCamera.PerCameraCBuffer);

            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbSSAO");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateSSAOCBuffer(cbBinder));
        }
    }

    /// <summary>
    /// Compute ShadingEnv for SSAO spatial filter (pass 2).
    /// Edge-aware bilateral blur on the raw AO, outputs to final AO texture.
    /// </summary>
    public class TtSSAOFilterComputeShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtSSAOFilterComputeShading()
        {
            CodeName = RName.GetRName("Shaders/Compute/ScreenSpace/SSAOFilter.compute", RName.ERNameType.Engine);
            MainName = "CS_SSAOFilter";

            this.BeginPermutaion();
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtSSAONode;
            if (node == null)
                return;

            // Read raw AO from intermediate output pin SRV
            var rawAOAttach = node.GetAttachBuffer(node.RawAOPinOut);
            if (rawAOAttach?.Srv != null)
            {
                drawcall.BindSrv("SSAORawInput", rawAOAttach.Srv);
                drawcall.BindSampler("Samp_SSAORawInput",
                    TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            }

            // Depth for edge-aware weighting
            var depthBuffer = node.GetAttachBuffer(node.DepthPinIn);
            if (depthBuffer?.Srv != null)
            {
                drawcall.BindSrv("FilterDepthBuffer", depthBuffer.Srv);
                drawcall.BindSampler("Samp_FilterDepthBuffer",
                    TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            }

            // Write filtered AO to final output UAV
            var outputAttach = node.GetAttachBuffer(node.AOPinOut);
            if (outputAttach?.Uav != null)
                drawcall.BindUav("SSAOFilteredOutput", outputAttach.Uav);

            drawcall.BindCBV("cbPerCamera", policy.DefaultCamera.PerCameraCBuffer);

            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbSSAOFilter");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateFilterCBuffer(cbBinder));
        }
    }

    /// <summary>
    /// SSAO RenderGraph Node — two-pass pipeline:
    ///   Pass 1: Horizon-based AO compute → raw intermediate texture
    ///   Pass 2: Edge-aware spatial filter → final AO output
    /// Both passes run at half resolution (configurable).
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("SSAO", "Post\\SSAO",
        Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtSSAONode : TAuxRenderGraphNode<TtSSAONode>
    {
        // ---- Pins ----
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput(
            "Depth", EBufferType.BFT_SRV);

        public TtRenderGraphPin GBufferRT1PinIn = TtRenderGraphPin.CreateInput(
            "GBufferRT1", EBufferType.BFT_SRV);

        public TtRenderGraphPin RawAOPinOut = TtRenderGraphPin.CreateOutput(
            "RawAO", false, EPixelFormat.PXF_R8_UNORM,
            EBufferType.BFT_SRV | EBufferType.BFT_UAV);

        public TtRenderGraphPin AOPinOut = TtRenderGraphPin.CreateOutput(
            "AO", false, EPixelFormat.PXF_R8_UNORM,
            EBufferType.BFT_SRV | EBufferType.BFT_UAV);

        // ---- Parameters ----
        [Rtti.Meta("")]
        [Category("SSAO")]
        public float Radius { get; set; } = 0.5f;

        [Rtti.Meta("")]
        [Category("SSAO")]
        public float Intensity { get; set; } = 1.0f;

        [Rtti.Meta("")]
        [Category("SSAO")]
        public float Bias { get; set; } = 0.02f;

        [Rtti.Meta("")]
        [Category("SSAO")]
        public int NumDirections { get; set; } = 4;

        [Rtti.Meta("")]
        [Category("SSAO")]
        public int NumSteps { get; set; } = 4;

        [Rtti.Meta("")]
        [Category("SSAO")]
        public float FalloffDistance { get; set; } = 10.0f;

        [Rtti.Meta("")]
        [Category("SSAO")]
        public bool EnableTemporalRotation { get; set; } = false;

        [Rtti.Meta("")]
        [Category("SSAO")]
        public bool HalfResolution { get; set; } = true;

        [Rtti.Meta("")]
        [Category("SSAO Filter")]
        public float FilterSharpness { get; set; } = 100.0f;

        // ---- Internal: pass 1 (AO compute) ----
        TtSSAOComputeShading mSSAOShading;
        TtComputeDraw mSSAODrawCall;
        TtCbView mSSAOCBuffer;

        // ---- Internal: pass 2 (spatial filter) ----
        TtSSAOFilterComputeShading mFilterShading;
        TtComputeDraw mFilterDrawCall;
        TtCbView mFilterCBuffer;


        public TtSSAONode()
        {
            Name = "SSAONode";
        }

        public override void InitNodePins()
        {
            AddInput(DepthPinIn);
            AddInput(GBufferRT1PinIn);
            AddOutput(RawAOPinOut);
            AddOutput(AOPinOut);
        }

        public override async Thread.Async.TtTask Initialize(
            TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            mSSAOShading = await TtShadingEnv.CreateShadingEnv<TtSSAOComputeShading>();
            mFilterShading = await TtShadingEnv.CreateShadingEnv<TtSSAOFilterComputeShading>();

            var renderContext = TtEngine.Instance.GfxDevice.RenderContext;
            mSSAODrawCall = renderContext.CreateComputeDraw();
            mSSAODrawCall.TagObject = this;

            mFilterDrawCall = renderContext.CreateComputeDraw();
            mFilterDrawCall.TagObject = this;
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mSSAODrawCall);
            CoreSDK.DisposeObject(ref mFilterDrawCall);
            CoreSDK.DisposeObject(ref mSSAOCBuffer);
            CoreSDK.DisposeObject(ref mFilterCBuffer);
            base.Dispose();
        }

        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            uint divisor = HalfResolution ? 2u : 1u;
            uint newWidth = Math.Max(1u, (uint)x / divisor);
            uint newHeight = Math.Max(1u, (uint)y / divisor);
            RawAOPinOut.Attachement.Width = newWidth;
            RawAOPinOut.Attachement.Height = newHeight;
            AOPinOut.Attachement.Width = newWidth;
            AOPinOut.Attachement.Height = newHeight;
        }

        public TtCbView GetOrCreateSSAOCBuffer(FShaderBinder binder)
        {
            if (mSSAOCBuffer == null)
            {
                mSSAOCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                FillSSAOCBufferValues(mSSAOCBuffer);
                mSSAOCBuffer.MarkDirty();
                mSSAOCBuffer.FlushDirty();
                return mSSAOCBuffer;
            }
            FillSSAOCBufferValues(mSSAOCBuffer);
            return mSSAOCBuffer;
        }

        public TtCbView GetOrCreateFilterCBuffer(FShaderBinder binder)
        {
            if (mFilterCBuffer == null)
            {
                mFilterCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                FillFilterCBufferValues(mFilterCBuffer);
                mFilterCBuffer.MarkDirty();
                mFilterCBuffer.FlushDirty();
                return mFilterCBuffer;
            }
            FillFilterCBufferValues(mFilterCBuffer);
            return mFilterCBuffer;
        }

        void FillSSAOCBufferValues(TtCbView cb)
        {
            var screenSize = new Vector2(RawAOPinOut.Attachement.Width, RawAOPinOut.Attachement.Height);
            cb.SetValue("ScreenSize", in screenSize);
            cb.SetValue("AORadius", Radius);
            cb.SetValue("AOIntensity", Intensity);
            cb.SetValue("AOBias", Bias);
            cb.SetValue("NumDirections", NumDirections);
            cb.SetValue("NumSteps", NumSteps);
            cb.SetValue("FalloffDistance", FalloffDistance);
            cb.SetValue("EnableTemporalRotation", EnableTemporalRotation ? 1u : 0u);
            cb.SetValue("FrameIndex", (uint)TtEngine.Instance.FrameCount);
        }

        void FillFilterCBufferValues(TtCbView cb)
        {
            var filterScreenSize = new Vector2(RawAOPinOut.Attachement.Width, RawAOPinOut.Attachement.Height);
            cb.SetValue("FilterScreenSize", in filterScreenSize);
            cb.SetValue("FilterSharpness", FilterSharpness);
        }

        TtAttachBuffer mFallbackAO = new TtAttachBuffer();

        public override void FrameBuild(TtRenderPolicy policy)
        {
            if (policy.EnableAO == false)
            {
                this.ImportAttachment(AOPinOut, mFallbackAO);
                mFallbackAO.Srv = TtEngine.Instance.GfxDevice.TextureManager.WhiteTextureSRV;
            }
        }

        public override unsafe void Tick(GamePlay.TtWorld world,
            TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (policy.EnableAO == false)
                return;

            if (mSSAOShading == null || !mSSAOShading.IsReady)
                return;
            if (mFilterShading == null || !mFilterShading.IsReady)
                return;
            if (mSSAODrawCall == null || mFilterDrawCall == null)
                return;

            var rawAOAttach = GetAttachBuffer(RawAOPinOut);
            if (rawAOAttach?.Uav == null)
                return;

            var outputAttach = GetAttachBuffer(AOPinOut);
            if (outputAttach?.Uav == null)
                return;

            uint width = RawAOPinOut.Attachement.Width;
            uint height = RawAOPinOut.Attachement.Height;
            if (width == 0 || height == 0)
                return;

            var cmd = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmd, "SSAO"))
            {
                // Pass 1: Horizon-based AO → raw intermediate texture
                mSSAOShading.SetDrawcallDispatch(this, policy, mSSAODrawCall,
                    width, height, 1, true);
                cmd.PushGpuDraw(mSSAODrawCall);
                cmd.FlushDraws();

                // Pass 2: Spatial filter → final AO output
                mFilterShading.SetDrawcallDispatch(this, policy, mFilterDrawCall,
                    width, height, 1, true);
                cmd.PushGpuDraw(mFilterDrawCall);
                cmd.FlushDraws();
            }
            policy.CommitCommandList(cmd, "SSAO");
        }
    }
}
