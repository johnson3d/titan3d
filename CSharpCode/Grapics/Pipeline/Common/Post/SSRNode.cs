using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline.Common.Post
{
    /// <summary>
    /// SSR trace pass 的 ShadingEnv。所有绑定在 OnDrawCall 完成 (CodingGuidelines §1.2)。
    /// </summary>
    public class TtSSRTraceShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        // Permutation: ENV_SSR_USE_HZB (0 = 线性步进 fallback, 1 = HZB 加速)
        public TtPermutationItem EnableHzbAccel { get; set; }

        public TtSSRTraceShading()
        {
            CodeName = RName.GetRName("Shaders/Compute/ScreenSpace/SSR.compute", RName.ERNameType.Engine);
            MainName = "CS_SSRTrace";

            this.BeginPermutaion();
            EnableHzbAccel = this.PushPermutation<EPermutation_Bool>("ENV_SSR_USE_HZB", (int)EPermutation_Bool.BitWidth);
            EnableHzbAccel.SetValue((int)EPermutation_Bool.FalseValue);
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtSSRNode;
            if (node == null)
                return;

            var samplerMgr = TtEngine.Instance.GfxDevice.SamplerStateManager;

            var depthBuffer = node.GetAttachBuffer(node.DepthPinIn);
            if (depthBuffer?.Srv != null)
            {
                drawcall.BindSrv("DepthBuffer", depthBuffer.Srv);
                drawcall.BindSampler("Samp_DepthBuffer", samplerMgr.PointState);
            }

            var gbufferRT1 = node.GetAttachBuffer(node.GBufferRT1PinIn);
            if (gbufferRT1?.Srv != null)
            {
                drawcall.BindSrv("GBufferRT1", gbufferRT1.Srv);
                drawcall.BindSampler("Samp_GBufferRT1", samplerMgr.PointState);
            }

            var gbufferRT2 = node.GetAttachBuffer(node.GBufferRT2PinIn);
            if (gbufferRT2?.Srv != null)
            {
                drawcall.BindSrv("GBufferRT2", gbufferRT2.Srv);
                drawcall.BindSampler("Samp_GBufferRT2", samplerMgr.PointState);
            }

            var sceneColor = node.GetAttachBuffer(node.SceneColorPinIn);
            if (sceneColor?.Srv != null)
            {
                drawcall.BindSrv("SceneColor", sceneColor.Srv);
                drawcall.BindSampler("Samp_SceneColor", samplerMgr.LinearClampState);
            }

            if (node.IsHzbConnected)
            {
                var hzb = node.GetAttachBuffer(node.HzbPinIn);
                if (hzb?.Srv != null)
                {
                    drawcall.BindSrv("HzbTexture", hzb.Srv);
                    drawcall.BindSampler("Samp_HzbTexture", samplerMgr.PointState);
                }
            }

            var traceAttach = node.GetAttachBuffer(node.TracePinOut);
            if (traceAttach?.Uav != null)
                drawcall.BindUav("SSRTraceOutput", traceAttach.Uav);

            drawcall.BindCBV("cbPerCamera", policy.DefaultCamera.PerCameraCBuffer);

            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbSSR");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateTraceCBuffer(cbBinder));
        }
    }

    /// <summary>
    /// SSR resolve pass 的 ShadingEnv: 空间复用降噪 + 施加镜面 BRDF。
    /// </summary>
    public class TtSSRResolveShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtSSRResolveShading()
        {
            CodeName = RName.GetRName("Shaders/Compute/ScreenSpace/SSRResolve.compute", RName.ERNameType.Engine);
            MainName = "CS_SSRResolve";

            this.BeginPermutaion();
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtSSRNode;
            if (node == null)
                return;

            var samplerMgr = TtEngine.Instance.GfxDevice.SamplerStateManager;

            var traceAttach = node.GetAttachBuffer(node.TracePinOut);
            if (traceAttach?.Srv != null)
            {
                drawcall.BindSrv("SSRTraceInput", traceAttach.Srv);
                drawcall.BindSampler("Samp_SSRTraceInput", samplerMgr.PointState);
            }

            var depthBuffer = node.GetAttachBuffer(node.DepthPinIn);
            if (depthBuffer?.Srv != null)
            {
                drawcall.BindSrv("ResolveDepthBuffer", depthBuffer.Srv);
                drawcall.BindSampler("Samp_ResolveDepthBuffer", samplerMgr.PointState);
            }

            var gbufferRT0 = node.GetAttachBuffer(node.GBufferRT0PinIn);
            if (gbufferRT0?.Srv != null)
            {
                drawcall.BindSrv("ResolveGBufferRT0", gbufferRT0.Srv);
                drawcall.BindSampler("Samp_ResolveGBufferRT0", samplerMgr.PointState);
            }

            var gbufferRT1 = node.GetAttachBuffer(node.GBufferRT1PinIn);
            if (gbufferRT1?.Srv != null)
            {
                drawcall.BindSrv("ResolveGBufferRT1", gbufferRT1.Srv);
                drawcall.BindSampler("Samp_ResolveGBufferRT1", samplerMgr.PointState);
            }

            var gbufferRT2 = node.GetAttachBuffer(node.GBufferRT2PinIn);
            if (gbufferRT2?.Srv != null)
            {
                drawcall.BindSrv("ResolveGBufferRT2", gbufferRT2.Srv);
                drawcall.BindSampler("Samp_ResolveGBufferRT2", samplerMgr.PointState);
            }

            var resultAttach = node.GetAttachBuffer(node.ReflectionPinOut);
            if (resultAttach?.Uav != null)
                drawcall.BindUav("SSRResolveOutput", resultAttach.Uav);

            drawcall.BindCBV("cbPerCamera", policy.DefaultCamera.PerCameraCBuffer);

            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbSSRResolve");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateResolveCBuffer(cbBinder));
        }
    }

    /// <summary>
    /// 屏幕空间反射 (SSR) 节点, 两个 compute pass:
    ///   Pass 1 (Trace):   每像素投一条反射线, 屏幕空间步进求交, 命中后取 SceneColor
    ///   Pass 2 (Resolve): 按深度/法线加权的邻域滤波 + 施加镜面 BRDF
    ///
    /// 输出 <see cref="ReflectionPinOut"/> 是"可以直接加到场景色上"的能量, 因此接一个
    /// TtAdditiveNode 就能合成, 不需要专门的 SSR 合成节点。
    ///
    /// 与其他反射通路的关系 (Roadmap P1-8 要求明确这一点):
    ///   - SSR 是 <b>非光追设备上的主力反射</b>, 覆盖屏幕内可见的清晰反射。
    ///   - ReSTIR GI 负责的是间接漫反射, 两者不冲突, 可同时开。
    ///   - 屏幕外 / 被遮挡的反射 SSR 取不到 (置信度为 0 就是 0), 由 IBL (gEnvMap) 兜底;
    ///     需要完整反射时走 DXR 路线。SSR 不做任何"猜测补全"。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("SSR", "Post\\SSR",
        Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtSSRNode : TAuxRenderGraphNode<TtSSRNode>
    {
        // ---- Pins ----
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput(
            "Depth", EBufferType.BFT_SRV);

        public TtRenderGraphPin GBufferRT0PinIn = TtRenderGraphPin.CreateInput(
            "GBufferRT0", EBufferType.BFT_SRV);

        public TtRenderGraphPin GBufferRT1PinIn = TtRenderGraphPin.CreateInput(
            "GBufferRT1", EBufferType.BFT_SRV);

        public TtRenderGraphPin GBufferRT2PinIn = TtRenderGraphPin.CreateInput(
            "GBufferRT2", EBufferType.BFT_SRV);

        /// <summary>反射要采样的场景颜色。通常接 DirLighting 的 Result。</summary>
        public TtRenderGraphPin SceneColorPinIn = TtRenderGraphPin.CreateInput(
            "SceneColor", EBufferType.BFT_SRV);

        /// <summary>可选。接上 HZB 后走层级加速步进, 否则退回线性等步长。</summary>
        public TtRenderGraphPin HzbPinIn = TtRenderGraphPin.CreateInput(
            "Hzb", EBufferType.BFT_SRV);

        /// <summary>trace 中间结果: rgb = 命中点颜色, a = 置信度。</summary>
        public TtRenderGraphPin TracePinOut = TtRenderGraphPin.CreateOutput(
            "Trace", false, EPixelFormat.PXF_R16G16B16A16_FLOAT,
            EBufferType.BFT_SRV | EBufferType.BFT_UAV);

        /// <summary>最终反射能量, 直接加到场景色上。</summary>
        public TtRenderGraphPin ReflectionPinOut = TtRenderGraphPin.CreateOutput(
            "Reflection", false, EPixelFormat.PXF_R11G11B10_FLOAT,
            EBufferType.BFT_SRV | EBufferType.BFT_UAV);

        // ---- Parameters ----
        [Rtti.Meta("")]
        [Category("SSR")]
        public float Intensity { get; set; } = 1.0f;

        /// <summary>超过该粗糙度的像素不做 SSR (粗糙反射靠 IBL / GI, SSR 在那儿噪声收益都很差)。</summary>
        [Rtti.Meta("")]
        [Category("SSR")]
        public float MaxRoughness { get; set; } = 0.6f;

        /// <summary>反射线最长世界距离 (米)。</summary>
        [Rtti.Meta("")]
        [Category("SSR")]
        public float MaxDistance { get; set; } = 30.0f;

        [Rtti.Meta("")]
        [Category("SSR")]
        public int NumSteps { get; set; } = 24;

        /// <summary>线性 fallback 的相交厚度容差 (米)。HZB 路径不用这个值。</summary>
        [Rtti.Meta("")]
        [Category("SSR")]
        public float Thickness { get; set; } = 0.5f;

        [Rtti.Meta("")]
        [Category("SSR")]
        public float EdgeFadePower { get; set; } = 1.0f;

        /// <summary>按粗糙度扰动反射方向的强度。0 = 纯镜面 (最锐但粗糙面会失真)。</summary>
        [Rtti.Meta("")]
        [Category("SSR")]
        public float RayJitter { get; set; } = 1.0f;

        /// <summary>半分辨率 trace + resolve。合成时由 Additive 的 LinearClamp 采样自动升采样。</summary>
        [Rtti.Meta("")]
        [Category("SSR")]
        public bool HalfResolution { get; set; } = true;

        [Rtti.Meta("")]
        [Category("SSR Resolve")]
        public int ResolveRadius { get; set; } = 2;

        [Rtti.Meta("")]
        [Category("SSR Resolve")]
        public float ResolveDepthSigma { get; set; } = 8.0f;

        /// <summary>
        /// HZB pin 是否连接。图结构在 Initialize 后就固定, 不会运行时变化。
        /// </summary>
        internal bool IsHzbConnected;

        // ---- Internal ----
        TtSSRTraceShading mTraceShading;
        TtSSRResolveShading mResolveShading;
        TtComputeDraw mTraceDrawCall;
        TtComputeDraw mResolveDrawCall;
        TtCbView mTraceCBuffer;
        TtCbView mResolveCBuffer;

        public TtSSRNode()
        {
            Name = "SSRNode";
        }

        public override void InitNodePins()
        {
            AddInput(DepthPinIn);
            AddInput(GBufferRT0PinIn);
            AddInput(GBufferRT1PinIn);
            AddInput(GBufferRT2PinIn);
            AddInput(SceneColorPinIn);
            HzbPinIn.IsAllowInputNull = true;
            AddInput(HzbPinIn);
            AddOutput(TracePinOut);
            AddOutput(ReflectionPinOut);
        }

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            mTraceShading = await TtShadingEnv.CreateShadingEnv<TtSSRTraceShading>();
            mResolveShading = await TtShadingEnv.CreateShadingEnv<TtSSRResolveShading>();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            mTraceDrawCall = rc.CreateComputeDraw();
            mTraceDrawCall.TagObject = this;
            mResolveDrawCall = rc.CreateComputeDraw();
            mResolveDrawCall.TagObject = this;

            IsHzbConnected = (HzbPinIn.FindInLinker() != null);
            if (IsHzbConnected)
            {
                mTraceShading.EnableHzbAccel.SetValue(true);
                mTraceShading.UpdatePermutation().AddWaitTask();
            }
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mTraceDrawCall);
            CoreSDK.DisposeObject(ref mResolveDrawCall);
            CoreSDK.DisposeObject(ref mTraceCBuffer);
            CoreSDK.DisposeObject(ref mResolveCBuffer);
            base.Dispose();
        }

        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            uint divisor = HalfResolution ? 2u : 1u;
            uint w = System.Math.Max(1u, (uint)x / divisor);
            uint h = System.Math.Max(1u, (uint)y / divisor);
            TracePinOut.Attachement.Width = w;
            TracePinOut.Attachement.Height = h;
            ReflectionPinOut.Attachement.Width = w;
            ReflectionPinOut.Attachement.Height = h;
        }

        /// <summary>§1.1: 首次 CreateCBV 必须填满所有字段 + MarkDirty + FlushDirty。</summary>
        public TtCbView GetOrCreateTraceCBuffer(FShaderBinder binder)
        {
            if (mTraceCBuffer == null)
            {
                mTraceCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                FillTraceCBuffer(mTraceCBuffer);
                mTraceCBuffer.MarkDirty();
                mTraceCBuffer.FlushDirty();
                return mTraceCBuffer;
            }
            FillTraceCBuffer(mTraceCBuffer);
            return mTraceCBuffer;
        }

        void FillTraceCBuffer(TtCbView cb)
        {
            var screenSize = new Vector2(
                TracePinOut.Attachement.Width,
                TracePinOut.Attachement.Height);
            cb.SetValue("SSRScreenSize", in screenSize);
            cb.SetValue("SSRMaxRoughness", MaxRoughness);
            cb.SetValue("SSRMaxDistance", MaxDistance);

            cb.SetValue("SSRNumSteps", NumSteps);
            cb.SetValue("SSRThickness", Thickness);
            cb.SetValue("SSREdgeFadePower", EdgeFadePower);
            cb.SetValue("SSRFrameIndex", (uint)TtEngine.Instance.FrameCount);

            cb.SetValue("SSRRayJitter", RayJitter);
            float pad = 0.0f;
            cb.SetValue("SSRPad0", pad);
            cb.SetValue("SSRPad1", pad);
            cb.SetValue("SSRPad2", pad);
        }

        public TtCbView GetOrCreateResolveCBuffer(FShaderBinder binder)
        {
            if (mResolveCBuffer == null)
            {
                mResolveCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                FillResolveCBuffer(mResolveCBuffer);
                mResolveCBuffer.MarkDirty();
                mResolveCBuffer.FlushDirty();
                return mResolveCBuffer;
            }
            FillResolveCBuffer(mResolveCBuffer);
            return mResolveCBuffer;
        }

        void FillResolveCBuffer(TtCbView cb)
        {
            var screenSize = new Vector2(
                ReflectionPinOut.Attachement.Width,
                ReflectionPinOut.Attachement.Height);
            cb.SetValue("SSRResolveScreenSize", in screenSize);
            cb.SetValue("SSRIntensity", Intensity);
            cb.SetValue("SSRResolveMaxRoughness", MaxRoughness);

            cb.SetValue("SSRResolveRadius", ResolveRadius);
            cb.SetValue("SSRResolveDepthSigma", ResolveDepthSigma);
            float pad = 0.0f;
            cb.SetValue("SSRResolvePad0", pad);
            cb.SetValue("SSRResolvePad1", pad);
        }

        TtAttachBuffer mFallbackReflection = new TtAttachBuffer();

        public override void FrameBuild(TtRenderPolicy policy)
        {
            if (policy.EnableSSR == false)
            {
                // 关掉 SSR 时输出黑图: 下游是 Additive, 加 0 等于什么都没发生
                this.ImportAttachment(ReflectionPinOut, mFallbackReflection);
                mFallbackReflection.Srv = TtEngine.Instance.GfxDevice.TextureManager.BlackTextureSRV;
            }
        }

        public override unsafe void Tick(GamePlay.TtWorld world,
            TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (policy.EnableSSR == false)
                return;
            if (mTraceShading == null || !mTraceShading.IsReady)
                return;
            if (mResolveShading == null || !mResolveShading.IsReady)
                return;
            if (mTraceDrawCall == null || mResolveDrawCall == null)
                return;

            if (GetAttachBuffer(TracePinOut)?.Uav == null)
                return;
            if (GetAttachBuffer(ReflectionPinOut)?.Uav == null)
                return;

            uint width = TracePinOut.Attachement.Width;
            uint height = TracePinOut.Attachement.Height;
            if (width == 0 || height == 0)
                return;

            var cmd = TtCommandList.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmd, "SSR"))
            {
                // Pass 1: 屏幕空间步进 -> Trace (rgb 命中颜色 / a 置信度)
                mTraceShading.SetDrawcallDispatch(this, policy, mTraceDrawCall,
                    width, height, 1, true);
                cmd.PushGpuDraw(mTraceDrawCall);
                cmd.FlushDraws();

                // Pass 2: 空间复用 + BRDF -> Reflection
                mResolveShading.SetDrawcallDispatch(this, policy, mResolveDrawCall,
                    width, height, 1, true);
                cmd.PushGpuDraw(mResolveDrawCall);
                cmd.FlushDraws();
            }
            policy.CommitCommandList(cmd, "SSR");
        }
    }
}
