using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline.Common.Post
{
    // =========================================================================
    // ShadingEnv - Spatial Denoise (à-trous wavelet bilateral filter)
    // 每个迭代通过 StepSize (1, 2, 4, 8, ...) 控制滤波半径
    // =========================================================================
    public class TtSpatialDenoiseShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtSpatialDenoiseShading()
        {
            CodeName = RName.GetRName("Shaders/ShadingEnv/Post/SpatialDenoise.compute", RName.ERNameType.Engine);
            MainName = "CS_Main";
            UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtDenoiseNode;
            if (node == null)
                return;

            // 所有 BindXxx 都必须在 OnDrawCall 里完成. 原因:
            //   SetDrawcallDispatch 内部才把 effect/PSO 装进 drawcall 并触发本回调,
            //   在它之前 drawcall 还没有合法的 binder 表, BindXxx 要么绑不上,
            //   要么会被随后 SetDrawcallDispatch 内部的初始化覆盖/失效.
            //   因此 ColorInput / DenoiseOutput 这些"按迭代变化"的 SRV/UAV 也由
            //   节点字段 (mCurrentInputSrv / mCurrentOutputUav) 在 Tick 阶段暂存,
            //   这里再读出来绑定. 与 ReSTIRGINode 4 个 pass 的写法一致.
            drawcall.BindSrv("ColorInput", node.GetCurrentInputSrv());
            drawcall.BindUav("DenoiseOutput", node.GetCurrentOutputUav());
            drawcall.BindSrv("GBufferRT1", node.GetAttachBuffer(node.NormalPinIn).Srv);
            drawcall.BindSrv("DepthBuffer", node.GetAttachBuffer(node.DepthPinIn).Srv);
            drawcall.BindSampler("Samp_PointClamp", TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            // CBV 在这里创建/获取: SetDrawcallDispatch 已经走完, effect 必然就绪,
            // FindBinder 才能拿到有效的字段反射 layout. (参考 TtReSTIRGINode.GetOrCreateSharedCBuffer)
            //
            // 注意: 4 个迭代的 cbDenoise 内容不同 (StepSize = 1, 2, 4, 8, ...),
            // 因此每个 drawcall 必须用独立的 TtCbView, 不能共享.
            // 共享会导致同帧 4 次 SetValue 互相覆盖, GPU 实际看到的全是最后一次的值
            // (cbuffer dirty 是延迟 flush 的, 同帧多次写互相覆盖).
            // 详见 documents/coding/CodingGuidelines.md §1.3.
            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbDenoise");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateCBufferForIteration(cbBinder, node.GetCurrentIterationIndex()));
        }
    }

    // =========================================================================
    // ShadingEnv - Temporal Denoise (motion-vector reprojection + history blend)
    // 在 SpatialDenoise 4 iter 之后单 pass 跑, 用上一帧的 final denoise 输出做时域反馈,
    // 是 SSR / 低 spp ReSTIR 模式下消除帧间闪烁的关键 pass.
    // =========================================================================
    public class TtTemporalDenoiseShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtTemporalDenoiseShading()
        {
            CodeName = RName.GetRName("Shaders/ShadingEnv/Post/TemporalDenoise.compute", RName.ERNameType.Engine);
            MainName = "CS_Main";
            UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtDenoiseNode;
            if (node == null)
                return;

            // 所有 BindXxx 都必须在 OnDrawCall 里完成. 详见 §3.6.
            // ColorInput   = 本帧 spatial 输出 (mPingPongSrv 之一, 由 Tick 暂存到 mTemporalInputSrv)
            // HistoryColor = 上一帧 final 输出 (mHistorySrv[历史 slot])
            // 输出写到 mTemporalOutputUav (= 当前 history slot 的 UAV, 同时也是 ResultPinOut 的来源)
            drawcall.BindSrv("ColorInput", node.GetTemporalInputSrv());
            drawcall.BindSrv("HistoryColor", node.GetHistorySrvForRead());
            drawcall.BindSrv("GBufferRT1", node.GetAttachBuffer(node.NormalPinIn).Srv);
            drawcall.BindSrv("DepthBuffer", node.GetAttachBuffer(node.DepthPinIn).Srv);
            drawcall.BindSrv("MotionVector", node.GetAttachBuffer(node.MotionVectorPinIn).Srv);
            drawcall.BindUav("DenoiseOutput", node.GetTemporalOutputUav());
            drawcall.BindSampler("Samp_PointClamp", TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            drawcall.BindSampler("Samp_LinearClamp", TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbTemporalDenoise");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateTemporalCBuffer(cbBinder));
        }
    }

    // =========================================================================
    // TtDenoiseNode - Edge-aware spatial denoise (à-trous wavelet, N iterations)
    //                 + optional temporal reprojection blend (motion-vector based)
    //   inputs  : Color (待降噪的 HDR 图), Normal (GBuffer RT1), Depth, MotionVector (可选)
    //   output  : Result (降噪后的 HDR 图)
    //
    //   pipeline:
    //     spatial à-trous (IterationCount 次, ping-pong)
    //       -> 写到 mPingPongTextures[lastSpatialOutSlot]   (不再直接写 ResultPinOut)
    //     [若 EnableTemporal && MotionVector pin 已连]
    //       -> temporal blend (mPingPong[lastSpatial], mHistory[读 slot]) -> mHistory[写 slot]
    //       -> 把 mHistory[写 slot] 的内容拷到 ResultPinOut
    //     [否则]
    //       -> 直接把 mPingPong[lastSpatial] 拷到 ResultPinOut
    //
    //   history ping-pong: mHistoryTextures[2], 每帧 swap, 写 slot = frame % 2.
    // =========================================================================
    [Bricks.CodeBuilder.ContextMenu("Denoise", "Post\\Denoise", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtDenoiseNode : TAuxRenderGraphNode<TtDenoiseNode>
    {
        // ---------- Pins ----------
        public TtRenderGraphPin ColorPinIn  = TtRenderGraphPin.CreateInput("Color", EBufferType.BFT_SRV);
        public TtRenderGraphPin NormalPinIn = TtRenderGraphPin.CreateInput("Normal", EBufferType.BFT_SRV);
        public TtRenderGraphPin DepthPinIn  = TtRenderGraphPin.CreateInput("Depth", EBufferType.BFT_SRV | EBufferType.BFT_DSV);
        // MotionVector 允许悬空: 不接 -> EnableTemporal 自动失效, 仅做空间降噪.
        // 接入 (通常连 GBufferMRT3 或 Deferred 输出的 MotionVector) -> 启用 temporal reprojection.
        public TtRenderGraphPin MotionVectorPinIn = TtRenderGraphPin.CreateInput("MotionVector", EBufferType.BFT_SRV);

        public TtRenderGraphPin ResultPinOut = TtRenderGraphPin.CreateOutput(
            "Result", true, EPixelFormat.PXF_R16G16B16A16_FLOAT,
            EBufferType.BFT_SRV | EBufferType.BFT_UAV);

        // ---------- Tunable ----------
        [Category("Denoise")]
        [Rtti.Meta("")]
        public int IterationCount { get; set; } = 4;
        [Category("Denoise")]
        [Rtti.Meta("")]
        public float PhiColor { get; set; } = 1.0f;
        // PhiNormal 默认 32: cos15°^32 ≈ 0.33, 保留 ±15° 内邻域有显著权重.
        // 之前 128 太严苛 (cos10°^128 ≈ 0.14, cos15°^128 ≈ 0.012), 曲面上几乎所有
        // 邻域采样都被剔除, 等于完全不滤波 -> 萤火虫无法被平均掉.
        [Category("Denoise")]
        [Rtti.Meta("")]
        public float PhiNormal { get; set; } = 32.0f;
        [Category("Denoise")]
        [Rtti.Meta("")]
        public float PhiDepth { get; set; } = 1.0f;
        // Firefly clamp: ReSTIR resolve 偶发 outlier 可达几百~几千, à-trous 只会把它
        // 摊薄成"喷溅"而不是消除. shader 端在 center/sample 取色后做 luminance clamp,
        // 把超过 MaxLuminance 的像素亮度按比例压回. 默认 5.0 (经验值).
        [Category("Denoise")]
        [Rtti.Meta("")]
        public float MaxLuminance { get; set; } = 5.0f;

        // ---------- Temporal ----------
        // EnableTemporal: 总开关. 即使打开, 若 MotionVector pin 未连入也会自动 fallback 到纯 spatial.
        [Category("Temporal")]
        [Rtti.Meta("")]
        public bool EnableTemporal { get; set; } = true;
        // TemporalAlpha: 当前帧权重 (历史权重 = 1 - alpha).
        // 越小越稳但 ghosting 越强. 0.1 = 10% 当前帧 + 90% 历史, 是 SSR ReSTIR 推荐值.
        [Category("Temporal")]
        [Rtti.Meta("")]
        public float TemporalAlpha { get; set; } = 0.1f;
        // 历史一致性阈值, 与 ReSTIR 同名字段语义一致.
        [Category("Temporal")]
        [Rtti.Meta("")]
        public float TemporalNormalThreshold { get; set; } = 0.9f;
        [Category("Temporal")]
        [Rtti.Meta("")]
        public float TemporalDepthThreshold { get; set; } = 0.05f;

        // ---------- Internal ----------
        TtSpatialDenoiseShading mShading;
        TtTemporalDenoiseShading mTemporalShading;
        TtComputeDraw[] mDrawCalls;
        TtComputeDraw mTemporalDraw;
        // 用 TtCopyDraw 把 history[writeSlot] 拷到 ResultPinOut. 这是引擎里通用的
        // texture-to-texture 拷贝方式 (参考 CSharpCode/Grapics/Pipeline/Common/CopyNode.cs).
        NxRHI.TtCopyDraw mResultCopyDraw;

        // ping-pong 中间 RT: 用 TtAttachBuffer 和引擎的 attachement 管理系统
        // [0] 和 [1] 交替作为输入/输出; 最终结果写到 ResultPinOut
        TtTexture[] mPingPongTextures = new TtTexture[2];
        TtSrView[]  mPingPongSrv = new TtSrView[2];
        TtUaView[]  mPingPongUav = new TtUaView[2];

        // 每个迭代独立一份 cbuffer. 不能共享, 否则同帧 SetValue 互相覆盖.
        // 详见 documents/coding/CodingGuidelines.md §1.3.
        TtCbView[] mDenoiseCBuffers;
        // Temporal pass 单独一份 cbuffer (只有 1 个 drawcall 用, 不存在共享问题).
        TtCbView mTemporalCBuffer;

        // History ping-pong textures: 存"上一帧 / 本帧"的 final 输出.
        //   读 slot = (frameCounter ^ 1) & 1   (上一帧写入的 slot)
        //   写 slot = frameCounter & 1
        // Temporal pass 从读 slot 采样, 写到写 slot, 然后再拷贝写 slot 到 ResultPinOut.
        TtTexture[] mHistoryTextures = new TtTexture[2];
        TtSrView[]  mHistorySrv = new TtSrView[2];
        TtUaView[]  mHistoryUav = new TtUaView[2];
        uint mFrameCounter = 0;
        bool mDisableHistoryNextFrame = true;   // 首帧 / resize 后强制屏蔽历史

        // 当前迭代的下标 / StepSize / Input SRV / Output UAV. 由 Tick 在 SetDrawcallDispatch
        // 之前设置, OnDrawCall 里再读出来绑定. (因为所有 Bind 必须在 OnDrawCall 里做,
        // SetDrawcallDispatch 之前 drawcall 没有合法的 binder 表.)
        int mCurrentIteration = 0;
        int mCurrentStepSize = 1;
        TtSrView mCurrentInputSrv;
        TtUaView mCurrentOutputUav;
        // Temporal pass 用的 SRV/UAV 暂存 (Tick 阶段写, OnDrawCall 阶段读).
        TtSrView mTemporalInputSrv;       // = 最后一轮 spatial 的输出 SRV
        TtUaView mTemporalOutputUav;      // = 当前帧 history 写 slot 的 UAV
        TtSrView mHistoryReadSrv;         // = 上一帧 history 写入的 slot 的 SRV
        int mTemporalDisableHistoryThisFrame = 1; // 当帧 disable 标志, 写入 cbTemporalDenoise.DisableHistory
        uint mWidth = 0;
        uint mHeight = 0;

        public int GetCurrentIterationIndex() => mCurrentIteration;
        public TtSrView GetCurrentInputSrv() => mCurrentInputSrv;
        public TtUaView GetCurrentOutputUav() => mCurrentOutputUav;
        public TtSrView GetTemporalInputSrv() => mTemporalInputSrv;
        public TtUaView GetTemporalOutputUav() => mTemporalOutputUav;
        public TtSrView GetHistorySrvForRead() => mHistoryReadSrv;

        public TtDenoiseNode()
        {
            Name = "DenoiseNode";
        }

        public override void InitNodePins()
        {
            AddInput(ColorPinIn);
            AddInput(NormalPinIn);
            AddInput(DepthPinIn);
            // MotionVector 允许悬空: Tick 阶段用 GetAttachBuffer + null 检测自动 fallback 到纯 spatial
            AddInput(MotionVectorPinIn);
            MotionVectorPinIn.IsAllowInputNull = true;
            AddOutput(ResultPinOut);
        }

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mShading = await TtShadingEnv.CreateShadingEnv<TtSpatialDenoiseShading>();
            mTemporalShading = await TtShadingEnv.CreateShadingEnv<TtTemporalDenoiseShading>();

            int maxIter = Math.Max(IterationCount, 1);
            mDrawCalls = new TtComputeDraw[maxIter];
            mDenoiseCBuffers = new TtCbView[maxIter];
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            for (int i = 0; i < maxIter; i++)
            {
                mDrawCalls[i] = rc.CreateComputeDraw();
                mDrawCalls[i].TagObject = this;
            }

            mTemporalDraw = rc.CreateComputeDraw();
            mTemporalDraw.TagObject = this;

            // CopyDraw 用于在 temporal pass 之后把 history[writeSlot] 内容拷到 ResultPinOut.
            // 单一 CopyDraw 实例每帧复用 (Mode/Src/Dest 在每次 Tick 重新设置).
            mResultCopyDraw = rc.CreateCopyDraw();
        }

        public override void Dispose()
        {
            if (mDrawCalls != null)
            {
                for (int i = 0; i < mDrawCalls.Length; i++)
                    CoreSDK.DisposeObject(ref mDrawCalls[i]);
            }
            CoreSDK.DisposeObject(ref mTemporalDraw);
            CoreSDK.DisposeObject(ref mResultCopyDraw);
            if (mDenoiseCBuffers != null)
            {
                for (int i = 0; i < mDenoiseCBuffers.Length; i++)
                    CoreSDK.DisposeObject(ref mDenoiseCBuffers[i]);
            }
            CoreSDK.DisposeObject(ref mTemporalCBuffer);
            ReleasePingPongBuffers();
            ReleaseHistoryBuffers();
            base.Dispose();
        }

        void ReleasePingPongBuffers()
        {
            for (int i = 0; i < 2; i++)
            {
                if (mPingPongUav[i] != null) { mPingPongUav[i].Dispose(); mPingPongUav[i] = null; }
                if (mPingPongSrv[i] != null) { mPingPongSrv[i].Dispose(); mPingPongSrv[i] = null; }
                if (mPingPongTextures[i] != null) { mPingPongTextures[i].Dispose(); mPingPongTextures[i] = null; }
            }
        }

        void ReleaseHistoryBuffers()
        {
            for (int i = 0; i < 2; i++)
            {
                if (mHistoryUav[i] != null) { mHistoryUav[i].Dispose(); mHistoryUav[i] = null; }
                if (mHistorySrv[i] != null) { mHistorySrv[i].Dispose(); mHistorySrv[i] = null; }
                if (mHistoryTextures[i] != null) { mHistoryTextures[i].Dispose(); mHistoryTextures[i] = null; }
            }
        }

        public override unsafe void OnResize(TtRenderPolicy policy, float x, float y)
        {
            uint w = (uint)MathHelper.Max(1.0f, x);
            uint h = (uint)MathHelper.Max(1.0f, y);
            if (w == mWidth && h == mHeight && mPingPongTextures[0] != null)
                return;
            mWidth = w;
            mHeight = h;

            ResultPinOut.Attachement.Width = w;
            ResultPinOut.Attachement.Height = h;

            ReleasePingPongBuffers();
            ReleaseHistoryBuffers();
            // Resize 后历史失效, 强制下一帧 disable history (否则会从 1x1 / 旧分辨率残留采样)
            mDisableHistoryNextFrame = true;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            for (int i = 0; i < 2; i++)
            {
                // ----------------------------------------------------------------
                // FTextureDesc: SetDefault() 之后必须显式指定 MipLevels 和 ArraySize.
                // 否则 D3D12 创建带 UAV 的纹理会报 DXGI_ERROR_INVALID_CALL
                // (UAV 资源必须明确 mip 数, 不允许 0=auto mip chain).
                // 参考: CSharpCode/Bricks/AdvanceShadow/AdvanceShadowShading.cs
                // ----------------------------------------------------------------
                var texDesc = new NxRHI.FTextureDesc();
                texDesc.SetDefault();
                texDesc.Width = w;
                texDesc.Height = h;
                texDesc.MipLevels = 1;
                texDesc.ArraySize = 1;
                texDesc.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
                texDesc.BindFlags = EBufferType.BFT_SRV | EBufferType.BFT_UAV;

                mPingPongTextures[i] = rc.CreateTexture(in texDesc);
                mPingPongTextures[i].SetDebugName($"DenoisePingPong_{i}");

                // SRV: SetTexture2D() 后必须填 MipLevels / MostDetailedMip,
                // 否则 D3D12 报 DXGI_ERROR_INVALID_CALL.
                var srvDesc = new NxRHI.FSrvDesc();
                srvDesc.SetTexture2D();
                srvDesc.Format = texDesc.Format;
                srvDesc.Texture2D.MipLevels = 1;
                srvDesc.Texture2D.MostDetailedMip = 0;
                mPingPongSrv[i] = rc.CreateSRV(mPingPongTextures[i], in srvDesc);

                // UAV: SetTexture2D() 后必须填 MipSlice.
                var uavDesc = new NxRHI.FUavDesc();
                uavDesc.SetTexture2D();
                uavDesc.Format = texDesc.Format;
                uavDesc.Texture2D.MipSlice = 0;
                mPingPongUav[i] = rc.CreateUAV(mPingPongTextures[i], in uavDesc);
            }

            // History textures: 与 ping-pong 完全相同的格式/尺寸, 共 2 张做时域 ping-pong.
            for (int i = 0; i < 2; i++)
            {
                var texDesc = new NxRHI.FTextureDesc();
                texDesc.SetDefault();
                texDesc.Width = w;
                texDesc.Height = h;
                texDesc.MipLevels = 1;
                texDesc.ArraySize = 1;
                texDesc.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
                texDesc.BindFlags = EBufferType.BFT_SRV | EBufferType.BFT_UAV;

                mHistoryTextures[i] = rc.CreateTexture(in texDesc);
                mHistoryTextures[i].SetDebugName($"DenoiseHistory_{i}");

                var srvDesc = new NxRHI.FSrvDesc();
                srvDesc.SetTexture2D();
                srvDesc.Format = texDesc.Format;
                srvDesc.Texture2D.MipLevels = 1;
                srvDesc.Texture2D.MostDetailedMip = 0;
                mHistorySrv[i] = rc.CreateSRV(mHistoryTextures[i], in srvDesc);

                var uavDesc = new NxRHI.FUavDesc();
                uavDesc.SetTexture2D();
                uavDesc.Format = texDesc.Format;
                uavDesc.Texture2D.MipSlice = 0;
                mHistoryUav[i] = rc.CreateUAV(mHistoryTextures[i], in uavDesc);
            }
        }

        // 在 OnDrawCall 阶段被调用: effect 已就绪, binder 反射出来的 cbuffer layout 才有效.
        // 每个迭代独立一份 cbuffer (按下标 iteration 索引), 不共享.
        // 首次创建必须按 documents/coding/CodingGuidelines.md §1.1: SetValue 全部字段 +
        // MarkDirty + FlushDirty, 否则首帧 GPU 读到未初始化的 cbDenoise, PhiNormal=0 让
        // pow 退化, PhiDepth=0 让除零产 NaN, 污染整个 ping-pong 链路.
        public TtCbView GetOrCreateCBufferForIteration(NxRHI.FShaderBinder binder, int iteration)
        {
            ref TtCbView slot = ref mDenoiseCBuffers[iteration];
            if (slot == null)
            {
                slot = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                var screenSize = new Vector2ui(mWidth, mHeight);
                slot.SetValue("ScreenSize", in screenSize);
                slot.SetValue("StepSize", mCurrentStepSize);
                slot.SetValue("PhiColor", PhiColor);
                slot.SetValue("PhiNormal", PhiNormal);
                slot.SetValue("PhiDepth", PhiDepth);
                slot.SetValue("MaxLuminance", MaxLuminance);
                slot.MarkDirty();
                slot.FlushDirty();
                return slot;
            }

            // 后续: 直接 SetValue, 引擎 frame end 自动 flush.
            // 每个迭代写入自己专属的 cbuffer, 不会和其他迭代互相覆盖.
            var screenSizeVal = new Vector2ui(mWidth, mHeight);
            slot.SetValue("ScreenSize", in screenSizeVal);
            slot.SetValue("StepSize", mCurrentStepSize);
            slot.SetValue("PhiColor", PhiColor);
            slot.SetValue("PhiNormal", PhiNormal);
            slot.SetValue("PhiDepth", PhiDepth);
            slot.SetValue("MaxLuminance", MaxLuminance);
            return slot;
        }

        // Temporal pass 的 cbuffer. 单独 1 份 (只有 1 个 drawcall 用).
        public TtCbView GetOrCreateTemporalCBuffer(NxRHI.FShaderBinder binder)
        {
            if (mTemporalCBuffer == null)
            {
                mTemporalCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                var screenSize = new Vector2ui(mWidth, mHeight);
                mTemporalCBuffer.SetValue("ScreenSize", in screenSize);
                mTemporalCBuffer.SetValue("TemporalAlpha", TemporalAlpha);
                mTemporalCBuffer.SetValue("NormalThreshold", TemporalNormalThreshold);
                mTemporalCBuffer.SetValue("DepthThreshold", TemporalDepthThreshold);
                mTemporalCBuffer.SetValue("DisableHistory", mTemporalDisableHistoryThisFrame);
                mTemporalCBuffer.SetValue("MaxLuminance", MaxLuminance);
                mTemporalCBuffer.MarkDirty();
                mTemporalCBuffer.FlushDirty();
                return mTemporalCBuffer;
            }
            var screenSizeVal = new Vector2ui(mWidth, mHeight);
            mTemporalCBuffer.SetValue("ScreenSize", in screenSizeVal);
            mTemporalCBuffer.SetValue("TemporalAlpha", TemporalAlpha);
            mTemporalCBuffer.SetValue("NormalThreshold", TemporalNormalThreshold);
            mTemporalCBuffer.SetValue("DepthThreshold", TemporalDepthThreshold);
            mTemporalCBuffer.SetValue("DisableHistory", mTemporalDisableHistoryThisFrame);
            mTemporalCBuffer.SetValue("MaxLuminance", MaxLuminance);
            return mTemporalCBuffer;
        }

        public override unsafe void Tick(TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            if (mShading == null || mWidth == 0 || mHeight == 0)
                return;

            int iterations = Math.Clamp(IterationCount, 1, mDrawCalls.Length);

            // 确保 draw call 数组够用 (用户运行时调大了 IterationCount)
            if (iterations > mDrawCalls.Length)
                iterations = mDrawCalls.Length;

            uint w = mWidth;
            uint h = mHeight;

            // ---------- 决定是否走 temporal pass ----------
            // 条件: EnableTemporal 打开 + MotionVector pin 已连入 + temporal shading 已就绪.
            // GetAttachBuffer 在 pin 悬空时会返回 null, 用它做 null 检测最稳.
            var motionAttach = GetAttachBuffer(MotionVectorPinIn);
            bool runTemporal = EnableTemporal
                               && mTemporalShading != null
                               && mTemporalDraw != null
                               && motionAttach != null
                               && motionAttach.Srv != null;

            // ---------- Spatial 迭代链 ----------
            //   iter 0: ColorPinIn (输入) -> pingpong[0] (输出)
            //   iter 1: pingpong[0]       -> pingpong[1]
            //   ...
            // 最后一轮的输出去向:
            //   - runTemporal: 写到 pingpong[lastSlot] (作为 temporal 的输入), temporal 再写到 history[写slot]
            //   - !runTemporal: 直接写到 ResultPinOut (与改造前等价)

            var inputColorSrv = GetAttachBuffer(ColorPinIn).Srv;
            var finalOutputUav = GetAttachBuffer(ResultPinOut).Uav;

            int lastSpatialOutSlot = (iterations - 1) % 2;

            var cmd = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new TtCmdListScope(cmd, "Denoise"))
            {
                for (int i = 0; i < iterations; i++)
                {
                    int stepSize = 1 << i;
                    var drawcall = mDrawCalls[i];

                    TtSrView currentInputSrv = (i == 0)
                        ? inputColorSrv
                        : mPingPongSrv[(i - 1) % 2];
                    bool isLastIteration = (i == iterations - 1);
                    // 末轮输出: 走 temporal 时写到 pingpong (给 temporal 当输入); 否则直接写 ResultPinOut.
                    TtUaView currentOutputUav;
                    if (isLastIteration)
                        currentOutputUav = runTemporal ? mPingPongUav[lastSpatialOutSlot] : finalOutputUav;
                    else
                        currentOutputUav = mPingPongUav[i % 2];

                    mCurrentIteration = i;
                    mCurrentInputSrv = currentInputSrv;
                    mCurrentOutputUav = currentOutputUav;
                    mCurrentStepSize = stepSize;

                    // SetDrawcallDispatch 必须先于任何 BindXxx 调用 (此处之前没有 BindXxx).
                    // 它会装 effect/PSO 进 drawcall, 然后触发 OnDrawCall, 在 OnDrawCall
                    // 里才能拿到合法的 binder 表完成所有资源绑定.
                    mShading.SetDrawcallDispatch(this, policy, drawcall, w, h, 1, true);
                    cmd.PushGpuDraw(drawcall);
                }

                // ---------- Temporal blend pass ----------
                if (runTemporal)
                {
                    // history ping-pong: 当帧写 slot = frame & 1, 读 slot = (frame ^ 1) & 1.
                    // 首帧 mFrameCounter=0: 写 slot 0, 读 slot 1 (空, 由 DisableHistory=1 屏蔽掉读).
                    int writeSlot = (int)(mFrameCounter & 1u);
                    int readSlot  = writeSlot ^ 1;

                    mTemporalInputSrv = mPingPongSrv[lastSpatialOutSlot];
                    mTemporalOutputUav = mHistoryUav[writeSlot];
                    mHistoryReadSrv = mHistorySrv[readSlot];
                    mTemporalDisableHistoryThisFrame = mDisableHistoryNextFrame ? 1 : 0;

                    mTemporalShading.SetDrawcallDispatch(this, policy, mTemporalDraw, w, h, 1, true);
                    cmd.PushGpuDraw(mTemporalDraw);

                    // 把 history[writeSlot] 拷到 ResultPinOut, 让下游节点能拿到.
                    // (history 本身要保留作为下一帧的读 slot, 不能直接当 output.)
                    // 引擎通用 texture-to-texture 拷贝: TtCopyDraw + CDM_Texture2Texture + BindSrc/BindDest.
                    // 参考 CSharpCode/Grapics/Pipeline/Common/CopyNode.cs 的 TtCopyNode.Tick.
                    var resultAttach = GetAttachBuffer(ResultPinOut);
                    if (resultAttach != null && resultAttach.GpuResource != null
                        && mHistoryTextures[writeSlot] != null && mResultCopyDraw != null)
                    {
                        mResultCopyDraw.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Texture;
                        mResultCopyDraw.BindSrc(mHistoryTextures[writeSlot]);
                        mResultCopyDraw.BindDest(resultAttach.GpuResource);
                        cmd.PushGpuDraw(mResultCopyDraw);
                    }

                    mFrameCounter++;
                    mDisableHistoryNextFrame = false;
                }
                else
                {
                    // 关闭 temporal 时, history 链路下次再开启需要重建 (避免读到很久之前的脏数据).
                    mDisableHistoryNextFrame = true;
                }

                cmd.FlushDraws();
            }
            policy.CommitCommandList(cmd, "Denoise");
        }
    }
}
