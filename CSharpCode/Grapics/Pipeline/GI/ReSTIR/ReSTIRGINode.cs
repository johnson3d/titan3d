using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline.GI.ReSTIR
{
    //https://www.semanticscholar.org/paper/ReSTIR-GI-Ouyang-Liu/33ac37def7bbda9d560ee8fe54dc4e1607f5c7c3

    // =========================================================================
    // 1. ShadingEnv - InitialSampling
    //    通过引擎标准的 TtPermutationItem 系统暴露 ENV_USE_HW_RT 维度:
    //      false (默认) -> HLSL 走屏幕空间 ray march
    //      true         -> HLSL 走 Inline RayQuery (DXR 1.1, 需要 SceneTLAS, cs_6_5)
    //    切换由 TtReSTIRGINode.EnableHardwareRT setter 触发, 引擎按 PermutationId
    //    重新拉取/编译 compute effect (与 TtDeferredDirLightingShading.IsEnableRimLight
    //    完全一致的语义), 运行期请勿频繁切换.
    //
    //    SceneTLAS 的 binder 仅在 ENV_USE_HW_RT=1 编译产物里才存在, 因此 OnDrawCall
    //    无脑 FindBinder("SceneTLAS"), 在 SS 变体下 IsValidPointer=false 会自然跳过,
    //    无需在 C# 端按变体值分支.
    // =========================================================================
    public class TtReSTIRInitialSamplingShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtPermutationItem EnableHWRT { get; set; }
        [Category("Option")]
        public bool IsEnableHWRT
        {
            get { return EnableHWRT.GetValue() == (int)EPermutation_Bool.TrueValue; }
            set { EnableHWRT.SetValue(value); this.UpdatePermutation().AddWaitTask(); }
        }

        // ENV_USE_SKY_CUBE: 0 -> miss 时用 cbReSTIR.SkyColor 常量天光
        //                   1 -> miss 时采 EnvMap (TextureCube), 需要 EnvMap pin 接入
        // EnvMap binder 仅在 1 的编译产物里出现, 因此 OnDrawCall 无脑 FindBinder 即可,
        // SS 变体下 IsValidPointer=false 自动跳过 (与 SceneTLAS 同一套保护机制).
        public TtPermutationItem EnableSkyCube { get; set; }
        [Category("Option")]
        public bool IsEnableSkyCube
        {
            get { return EnableSkyCube.GetValue() == (int)EPermutation_Bool.TrueValue; }
            set { EnableSkyCube.SetValue(value); this.UpdatePermutation().AddWaitTask(); }
        }

        public TtReSTIRInitialSamplingShading()
        {
            CodeName = RName.GetRName("Shaders/GI/ReSTIR/ReSTIRInitialSampling.compute", RName.ERNameType.Engine);
            MainName = "CS_Main";

            this.BeginPermutaion();
            EnableHWRT = this.PushPermutation<EPermutation_Bool>("ENV_USE_HW_RT", (int)EPermutation_Bool.BitWidth);
            EnableHWRT.SetValue((int)EPermutation_Bool.FalseValue);
            EnableSkyCube = this.PushPermutation<EPermutation_Bool>("ENV_USE_SKY_CUBE", (int)EPermutation_Bool.BitWidth);
            EnableSkyCube.SetValue((int)EPermutation_Bool.FalseValue);

            UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtReSTIRGINode;
            if (node == null)
                return;

            // 统一用引擎标准 drawcall.BindXxx("name", view) 字符串直通重载,
            // 与 Bricks/Particle/Effector.cs 的写法对齐. 不要走 FindBinder + 底层
            // BindXxx(FShaderBinder, IXxxView) 重载, 否则 TtXxxView 与 IXxxView 类型不匹配.
            drawcall.BindSrv("GBufferRT0", node.GetAttachBuffer(node.GBufferRT0PinIn).Srv);
            drawcall.BindSrv("GBufferRT1", node.GetAttachBuffer(node.GBufferRT1PinIn).Srv);
            drawcall.BindSrv("DepthBuffer", node.GetAttachBuffer(node.DepthPinIn).Srv);
            drawcall.BindSrv("PrevColor", node.GetAttachBuffer(node.PrevColorPinIn).Srv);
            drawcall.BindSampler("Samp_PointClamp", TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            // 4 个 ReSTIR pass 的 cbReSTIR layout 完全一致, 同一 TtCbView 可以跨 shading 复用
            // (CBV 由 binder 字段反射决定 layout, 不绑定到具体 effect 的根签名槽位).
            // 这里仍走 FindBinder, 是因为 GetOrCreateSharedCBuffer 需要 binder 反射出的字段
            // layout 来 CreateCBV; 拿到 cbView 后用底层 BindCBV(binder, TtCbView) 完成绑定.
            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbReSTIR");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateSharedCBuffer(cbBinder));
            drawcall.BindCBV("cbPerCamera", policy.DefaultCamera.PerCameraCBuffer);
            drawcall.BindUav("OutReservoirs", node.GetCurrentReservoirUav());

            // SceneTLAS: 仅在 ENV_USE_HW_RT=1 编译产物的 binder 表里存在.
            // GetGpuBufferSRV 返回底层 NxRHI.ISrView (核心对象), 走 TtComputeDraw 上的
            // BindSrv(FShaderBinder, ISrView) RHI 直通重载 (Drawcall.cs).
            if (node.SceneTLAS != null)
            {
                var tlasBinder = drawcall.FindBinder(EShaderBindType.SBT_SRV, "SceneTLAS");
                if (tlasBinder.IsValidPointer)
                    drawcall.BindSrv(tlasBinder, node.SceneTLAS.mCoreObject.GetGpuBufferSRV());
            }

            // EnvMap: 仅在 ENV_USE_SKY_CUBE=1 编译产物里存在 binder.
            // EnvMap pin 允许悬空 (FindAttachBuffer 返回 null), 此时 permutation
            // 一定是 0 (由节点的 EnableEnvMap setter 强制保证), binder 也不存在,
            // 所以这里两层保护: pin 有 attach + binder 有效, 缺一就跳过.
            var envBuffer = node.FindAttachBuffer(node.EnvMapPinIn);
            if (envBuffer != null)
            {
                var envBinder = drawcall.FindBinder(EShaderBindType.SBT_SRV, "EnvMap");
                if (envBinder.IsValidPointer)
                    drawcall.BindSrv(envBinder, envBuffer.Srv);
                var envSampBinder = drawcall.FindBinder(EShaderBindType.SBT_Sampler, "Samp_EnvMap");
                if (envSampBinder.IsValidPointer)
                    drawcall.BindSampler(envSampBinder, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);
            }
        }
    }

    // =========================================================================
    // 2. ShadingEnv - TemporalReuse
    //    用 motion vector 重采样上一帧 reservoir, 与本帧 initial reservoir 合并
    // =========================================================================
    public class TtReSTIRTemporalReuseShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtReSTIRTemporalReuseShading()
        {
            CodeName = RName.GetRName("Shaders/GI/ReSTIR/ReSTIRTemporalReuse.compute", RName.ERNameType.Engine);
            MainName = "CS_Main";
            UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtReSTIRGINode;
            if (node == null)
                return;

            drawcall.BindSrv("GBufferRT1", node.GetAttachBuffer(node.GBufferRT1PinIn).Srv);
            drawcall.BindSrv("DepthBuffer", node.GetAttachBuffer(node.DepthPinIn).Srv);
            drawcall.BindSrv("MotionVector", node.GetAttachBuffer(node.MotionVectorPinIn).Srv);
            drawcall.BindSampler("Samp_PointClamp", TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbReSTIR");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateSharedCBuffer(cbBinder));
            drawcall.BindCBV("cbPerCamera", policy.DefaultCamera.PerCameraCBuffer);
            drawcall.BindSrv("InCurrentReservoirs", node.GetCurrentReservoirSrv());
            drawcall.BindSrv("InHistoryReservoirs", node.GetHistoryReservoirSrv());
            drawcall.BindUav("OutReservoirs", node.GetTempReservoirUav());
        }
    }

    // =========================================================================
    // 3. ShadingEnv - SpatialReuse
    //    在邻域内做 K 个候选的水库合并
    // =========================================================================
    public class TtReSTIRSpatialReuseShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtReSTIRSpatialReuseShading()
        {
            CodeName = RName.GetRName("Shaders/GI/ReSTIR/ReSTIRSpatialReuse.compute", RName.ERNameType.Engine);
            MainName = "CS_Main";
            UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtReSTIRGINode;
            if (node == null)
                return;

            drawcall.BindSrv("GBufferRT1", node.GetAttachBuffer(node.GBufferRT1PinIn).Srv);
            drawcall.BindSrv("DepthBuffer", node.GetAttachBuffer(node.DepthPinIn).Srv);
            drawcall.BindSampler("Samp_PointClamp", TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbReSTIR");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateSharedCBuffer(cbBinder));
            drawcall.BindCBV("cbPerCamera", policy.DefaultCamera.PerCameraCBuffer);
            // 空间复用读 temporal 输出, 写回 current 槽 (供下一帧作为 history)
            drawcall.BindSrv("InReservoirs", node.GetTempReservoirSrv());
            drawcall.BindUav("OutReservoirs", node.GetCurrentReservoirUav());
        }
    }

    // =========================================================================
    // 4. ShadingEnv - Resolve
    //    根据最终 reservoir 计算 indirect diffuse, 写到一张输出 RT
    // =========================================================================
    public class TtReSTIRResolveShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtReSTIRResolveShading()
        {
            CodeName = RName.GetRName("Shaders/GI/ReSTIR/ReSTIRResolve.compute", RName.ERNameType.Engine);
            MainName = "CS_Main";
            UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtReSTIRGINode;
            if (node == null)
                return;

            drawcall.BindSrv("GBufferRT0", node.GetAttachBuffer(node.GBufferRT0PinIn).Srv);
            drawcall.BindSrv("GBufferRT1", node.GetAttachBuffer(node.GBufferRT1PinIn).Srv);
            drawcall.BindSrv("GBufferRT2", node.GetAttachBuffer(node.GBufferRT2PinIn).Srv);
            drawcall.BindSrv("DepthBuffer", node.GetAttachBuffer(node.DepthPinIn).Srv);
            drawcall.BindSampler("Samp_PointClamp", TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbReSTIR");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateSharedCBuffer(cbBinder));
            drawcall.BindCBV("cbPerCamera", policy.DefaultCamera.PerCameraCBuffer);
            drawcall.BindSrv("InReservoirs", node.GetCurrentReservoirSrv());
            drawcall.BindUav("OutIndirectDiffuse", node.GetAttachBuffer(node.IndirectDiffusePinOut).Uav);
        }
    }

    // 与 HLSL ReSTIRCommon.cginc 中 ReSTIRPackedReservoir 一一对应:
    //   float4 Data0;  // VisiblePosition.xyz, WeightSum
    //   float4 Data1;  // VisibleNormal.xyz,   M
    //   float4 Data2;  // SamplePosition.xyz,  W
    //   float4 Data3;  // SampleNormal.xyz,    Radiance.r
    //   float4 Data4;  // Radiance.gb,         _pad0, _pad1
    // 共 80 字节, structured buffer 不会为 5 个 float4 插入额外 padding
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct FReSTIRPackedReservoir
    {
        public Vector4 Data0;
        public Vector4 Data1;
        public Vector4 Data2;
        public Vector4 Data3;
        public Vector4 Data4;
    }

    // =========================================================================
    // 聚合节点 TtReSTIRGINode
    //   inputs (pin)   : MRT0, MRT1, MRT2, Depth, MotionVector, PrevColor
    //   output (pin)   : IndirectDiffuse (RGBA16F)
    //   inputs (字段)  : SceneTLAS (TtTopAccelerationStructure, 可选)
    //                    由外部 (全场景 TLAS 管理器) 注入; HW RT 变体下绑给 shader.
    //
    //   内部维护 3 份 StructuredBuffer<ReSTIRPackedReservoir>:
    //     [0] 当前帧 (initial 写, temporal 读, spatial 写, resolve 读, 下一帧作为 history)
    //     [1] 上一帧 (history, 只读)  —— 与 [0] 通过 ping-pong 切换
    //     [2] 临时 (temporal 写, spatial 读)
    //
    //   ENV_USE_HW_RT 由 EnableHardwareRT setter 触发引擎 Permutation 系统重建 effect,
    //   是配置型切换 (启动期/质量档调节), 不应每帧切换 (会触发 effect 重新编译/拉取)
    // =========================================================================
    [Bricks.CodeBuilder.ContextMenu("ReSTIRGI", "GI\\ReSTIR\\ReSTIRGI", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtReSTIRGINode : TAuxRenderGraphNode<TtReSTIRGINode>
    {
        // ---------- Pins ----------
        public TtRenderGraphPin GBufferRT0PinIn  = TtRenderGraphPin.CreateInput("MRT0", EBufferType.BFT_SRV);
        public TtRenderGraphPin GBufferRT1PinIn  = TtRenderGraphPin.CreateInput("MRT1", EBufferType.BFT_SRV);
        public TtRenderGraphPin GBufferRT2PinIn  = TtRenderGraphPin.CreateInput("MRT2", EBufferType.BFT_SRV);
        public TtRenderGraphPin DepthPinIn       = TtRenderGraphPin.CreateInput("Depth", EBufferType.BFT_SRV | EBufferType.BFT_DSV);
        public TtRenderGraphPin MotionVectorPinIn = TtRenderGraphPin.CreateInput("MotionVector", EBufferType.BFT_SRV);
        public TtRenderGraphPin PrevColorPinIn   = TtRenderGraphPin.CreateInput("PrevColor", EBufferType.BFT_SRV);
        // EnvMap (TextureCube) 输入 pin, 允许悬空:
        //   - 不接 -> EnableEnvMap 强制 false, shader ENV_USE_SKY_CUBE=0, miss 时用 SkyColor 常量
        //   - 接入 -> 用户主动开 EnableEnvMap = true, shader ENV_USE_SKY_CUBE=1, miss 时采 EnvMap
        public TtRenderGraphPin EnvMapPinIn      = TtRenderGraphPin.CreateInput("EnvMap", EBufferType.BFT_SRV);

        public TtRenderGraphPin IndirectDiffusePinOut = TtRenderGraphPin.CreateOutput(
            "IndirectDiffuse", true, EPixelFormat.PXF_R16G16B16A16_FLOAT,
            EBufferType.BFT_SRV | EBufferType.BFT_UAV);

        // ---------- Tunable ----------
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public float Intensity { get; set; } = 1.0f;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public float MaxRayDistance { get; set; } = 50.0f;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public uint  MaxRayMarchSteps { get; set; } = 32u;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public float ThicknessBias { get; set; } = 0.05f;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public float TemporalMaxM { get; set; } = 20.0f;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public float NormalThreshold { get; set; } = 0.9f;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public float DepthThreshold { get; set; } = 0.05f;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public uint  SpatialSampleCount { get; set; } = 5u;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public float SpatialRadius { get; set; } = 16.0f;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public float MaxRadiance { get; set; } = 10.0f;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public uint  InitialSampleCount { get; set; } = 16u;
        // 天光 fallback 颜色 (linear), 当 ENV_USE_SKY_CUBE=0 时, miss 方向使用此颜色 * SkyIntensity
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public Vector3 SkyColor { get; set; } = new Vector3(0.5f, 0.7f, 1.0f);
        // 天光强度倍率, 设为 0 等于完全禁用 miss 时的天光贡献
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public float SkyIntensity { get; set; } = 1.0f;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public bool  EnableTemporal { get; set; } = true;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public bool  EnableSpatial { get; set; } = true;
        // ENV_USE_HW_RT permutation 开关: 配置型切换, 启动期/质量档调节, 不应每帧切换.
        //
        // 新版 TtShadingEnv.UpdatePermutation 内部会自动 await OnCreateEffect() 重建 effect,
        // 因此此 setter 直接通过 IsEnableHWRT setter 触发 mInitial.UpdatePermutation 即可,
        // 无需节点端再实现一份 RecreateInitialShadingAsync.
        bool mEnableHardwareRT = false;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public bool EnableHardwareRT
        {
            get { return mEnableHardwareRT; }
            set
            {
                if (mEnableHardwareRT == value)
                    return;
                mEnableHardwareRT = value;
                // Initialize 之后 mInitial 才存在; setter 内部会触发 effect 异步重建.
                // Initialize 之前的设值, 会在 Initialize 流程里被自然采纳.
                if (mInitial != null)
                    mInitial.IsEnableHWRT = value;
            }
        }

        // ENV_USE_SKY_CUBE permutation 开关: 配置型切换, 同 EnableHardwareRT.
        // 用户必须先把 EnvMap pin 接入再开此开关, 否则 shader 端虽然走 cube 分支
        // 但 OnDrawCall 因 pin 悬空而跳过 EnvMap 绑定 -> 采到的是上一次绑定的脏数据.
        bool mEnableEnvMap = false;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public bool EnableEnvMap
        {
            get { return mEnableEnvMap; }
            set
            {
                if (mEnableEnvMap == value)
                    return;
                mEnableEnvMap = value;
                if (mInitial != null)
                    mInitial.IsEnableSkyCube = value;
            }
        }

        // 由外部 (例如全场景 TLAS 管理器或 RayTracingNode) 注入的场景 acceleration structure.
        // 节点本身不负责 TLAS 的 build / refit / instance 维护, 只负责"如果有就用".
        public NxRHI.TtTopAccelerationStructure SceneTLAS;

        // ---------- Shadings ----------
        TtReSTIRInitialSamplingShading mInitial;
        TtReSTIRTemporalReuseShading   mTemporal;
        TtReSTIRSpatialReuseShading    mSpatial;
        TtReSTIRResolveShading         mResolve;

        TtComputeDraw mInitialDraw;
        TtComputeDraw mTemporalDraw;
        TtComputeDraw mSpatialDraw;
        TtComputeDraw mResolveDraw;

        // ---------- Reservoir Buffers (Ping-Pong + Temp) ----------
        // 用 TtGpuBuffer<T> 封装 buffer 创建/UAV/SRV/释放, 与 Particle/PGC 等模块统一
        TtGpuBuffer<FReSTIRPackedReservoir>[] mReservoirBuffers = new TtGpuBuffer<FReSTIRPackedReservoir>[3];
        // 0/1 ping-pong, 2 temp
        int mCurrentSlot = 0;
        int mHistorySlot = 1;
        const int kTempSlot = 2;

        // 4 个 ReSTIR shader 的 cbReSTIR layout 完全一致, 共享一份 CBV 即可
        // (参考 Bricks/Terrain/CDLOD/Patch.cs 的 TerrainNode.TerrainCBuffer 跨 patch 共享模式)
        TtCbView mSharedCBuffer;
        uint mFrameIndexLastWritten = uint.MaxValue;
        uint mWidth = 0;
        uint mHeight = 0;
        uint mFrameIndex = 0;

        public TtReSTIRGINode()
        {
            Name = "ReSTIRGINode";
        }

        public override void InitNodePins()
        {
            AddInput(GBufferRT0PinIn);
            AddInput(GBufferRT1PinIn);
            AddInput(GBufferRT2PinIn);
            AddInput(DepthPinIn);
            AddInput(MotionVectorPinIn);
            // PrevColor 是 InitialSampling pass 唯一的间接光源, HLSL 端没有 fallback,
            // 不允许悬空; 接入侧需保证连接 AntiAliasing 节点的 PreColor 输出
            AddInput(PrevColorPinIn);
            // EnvMap 允许悬空 (用 FindAttachBuffer 检测), 不接时 fallback 到 SkyColor 常量
            AddInput(EnvMapPinIn);
            EnvMapPinIn.IsAllowInputNull = true;

            AddOutput(IndirectDiffusePinOut);
        }

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            // InitialSampling 需要在 OnCreateEffect 之前设置 ENV_USE_HW_RT permutation,
            // 才能保证 effect 编译时的宏与 mCurrentPermutationId 一致. 流程: new + 设值
            // + await UpdatePermutation (基类内部会 await OnCreateEffect).
            mInitial = new TtReSTIRInitialSamplingShading();
            mInitial.IsEnableHWRT = mEnableHardwareRT;

            if (EnvMapPinIn.FindInLinker() != null)
            {
                mEnableEnvMap = true;
            }
            else
            {
                mEnableEnvMap = false;
            }
            mInitial.IsEnableSkyCube = mEnableEnvMap;
            await mInitial.UpdatePermutation();

            mTemporal = await TtShadingEnv.CreateShadingEnv<TtReSTIRTemporalReuseShading>();
            mSpatial = await TtShadingEnv.CreateShadingEnv<TtReSTIRSpatialReuseShading>();
            mResolve = await TtShadingEnv.CreateShadingEnv<TtReSTIRResolveShading>();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            mInitialDraw  = rc.CreateComputeDraw();
            mTemporalDraw = rc.CreateComputeDraw();
            mSpatialDraw  = rc.CreateComputeDraw();
            mResolveDraw  = rc.CreateComputeDraw();

            // 必须显式设置 TagObject, 因为各 ShadingEnv.OnDrawCall 通过
            // (drawcall.TagObject as TtReSTIRGINode) 获取本节点以读取 pin/Reservoir 资源
            mInitialDraw.TagObject  = this;
            mTemporalDraw.TagObject = this;
            mSpatialDraw.TagObject  = this;
            mResolveDraw.TagObject  = this;
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mInitialDraw);
            CoreSDK.DisposeObject(ref mTemporalDraw);
            CoreSDK.DisposeObject(ref mSpatialDraw);
            CoreSDK.DisposeObject(ref mResolveDraw);
            CoreSDK.DisposeObject(ref mSharedCBuffer);
            ReleaseReservoirBuffers();
            base.Dispose();
        }

        void ReleaseReservoirBuffers()
        {
            for (int i = 0; i < mReservoirBuffers.Length; i++)
            {
                if (mReservoirBuffers[i] != null)
                {
                    mReservoirBuffers[i].Dispose();
                    mReservoirBuffers[i] = null;
                }
            }
        }

        public override unsafe void OnResize(TtRenderPolicy policy, float x, float y)
        {
            uint w = (uint)MathHelper.Max(1.0f, x);
            uint h = (uint)MathHelper.Max(1.0f, y);
            if (w == mWidth && h == mHeight && mReservoirBuffers[0] != null)
                return;
            mWidth = w;
            mHeight = h;

            IndirectDiffusePinOut.Attachement.Width = w;
            IndirectDiffusePinOut.Attachement.Height = h;

            ReleaseReservoirBuffers();

            uint elementCount = w * h;
            for (int i = 0; i < mReservoirBuffers.Length; i++)
            {
                mReservoirBuffers[i] = new TtGpuBuffer<FReSTIRPackedReservoir>();
                // pInitData 传 null/IntPtr.Zero -> GPU-only structured buffer, 不分配 InitData
                mReservoirBuffers[i].SetSize(elementCount, IntPtr.Zero.ToPointer(),
                    EBufferType.BFT_UAV | EBufferType.BFT_SRV);
            }
        }

        public TtUaView GetCurrentReservoirUav() => mReservoirBuffers[mCurrentSlot].Uav;
        public TtSrView GetCurrentReservoirSrv() => mReservoirBuffers[mCurrentSlot].Srv;
        public TtUaView GetHistoryReservoirUav() => mReservoirBuffers[mHistorySlot].Uav;
        public TtSrView GetHistoryReservoirSrv() => mReservoirBuffers[mHistorySlot].Srv;
        public TtUaView GetTempReservoirUav() => mReservoirBuffers[kTempSlot].Uav;
        public TtSrView GetTempReservoirSrv() => mReservoirBuffers[kTempSlot].Srv;

        // 4 个 pass 共享同一份 CBV. 首次调用时按任一 pass 的 binder 字段反射创建,
        // 之后所有 pass 复用; 每帧只写一次, 同帧多次调用直接返回缓存值.
        // 因为 4 个 shader 的 cbReSTIR layout 完全一致, 共享是安全的.
        public TtCbView GetOrCreateSharedCBuffer(NxRHI.FShaderBinder binder)
        {
            if (mSharedCBuffer == null)
            {
                mSharedCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                mSharedCBuffer.SetValue("ScreenSize", new Vector2ui(mWidth, mHeight));
                mSharedCBuffer.SetValue("FrameIndex", mFrameIndex);
                mSharedCBuffer.SetValue("MaxRayDistance", MaxRayDistance);
                mSharedCBuffer.SetValue("MaxRayMarchSteps", MaxRayMarchSteps);
                mSharedCBuffer.SetValue("ThicknessBias", ThicknessBias);
                mSharedCBuffer.SetValue("TemporalMaxM", TemporalMaxM);
                mSharedCBuffer.SetValue("NormalThreshold", NormalThreshold);
                mSharedCBuffer.SetValue("DepthThreshold", DepthThreshold);
                mSharedCBuffer.SetValue("SpatialSampleCount", SpatialSampleCount);
                mSharedCBuffer.SetValue("SpatialRadius", SpatialRadius);
                mSharedCBuffer.SetValue("Intensity", Intensity);
                mSharedCBuffer.SetValue("MaxRadiance", MaxRadiance);
                mSharedCBuffer.SetValue("InitialSampleCount", InitialSampleCount);
                var skyColor = SkyColor;
                mSharedCBuffer.SetValue("SkyColor", in skyColor);
                mSharedCBuffer.SetValue("SkyIntensity", SkyIntensity);
                mSharedCBuffer.MarkDirty();
                mSharedCBuffer.FlushDirty();
            }

            // 同一帧多次调用 (4 个 pass) 只写入一次, 减少冗余 SetValue
            if (mFrameIndexLastWritten == mFrameIndex)
                return mSharedCBuffer;
            mFrameIndexLastWritten = mFrameIndex;

            var screenSize = new Vector2ui(mWidth, mHeight);
            mSharedCBuffer.SetValue("ScreenSize", in screenSize);
            mSharedCBuffer.SetValue("FrameIndex", mFrameIndex);
            mSharedCBuffer.SetValue("MaxRayDistance", MaxRayDistance);
            mSharedCBuffer.SetValue("MaxRayMarchSteps", MaxRayMarchSteps);
            mSharedCBuffer.SetValue("ThicknessBias", ThicknessBias);
            mSharedCBuffer.SetValue("TemporalMaxM", TemporalMaxM);
            mSharedCBuffer.SetValue("NormalThreshold", NormalThreshold);
            mSharedCBuffer.SetValue("DepthThreshold", DepthThreshold);
            mSharedCBuffer.SetValue("SpatialSampleCount", SpatialSampleCount);
            mSharedCBuffer.SetValue("SpatialRadius", SpatialRadius);
            mSharedCBuffer.SetValue("Intensity", Intensity);
            mSharedCBuffer.SetValue("MaxRadiance", MaxRadiance);
            mSharedCBuffer.SetValue("InitialSampleCount", InitialSampleCount);
            var skyColorVal = SkyColor;
            mSharedCBuffer.SetValue("SkyColor", in skyColorVal);
            mSharedCBuffer.SetValue("SkyIntensity", SkyIntensity);
            
            return mSharedCBuffer;
        }

        public override unsafe void Tick(TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            if (mInitial == null || mWidth == 0 || mHeight == 0)
                return;

            mFrameIndex++;

            // 每帧 ping-pong: 把 (current, history) 交换. Initial 写入新的 current,
            // Temporal 同时读旧的 history (保持上一帧最终结果不被覆盖)
            // CBV 内容在 OnDrawCall -> GetOrCreateCBuffer 中按 pass 写入
            (mCurrentSlot, mHistorySlot) = (mHistorySlot, mCurrentSlot);

            uint w = mWidth;
            uint h = mHeight;

            // 数据流:
            //   Initial:  PrevColor + GBuffer + Depth (+ SceneTLAS@HWRT) -> current[mCurrentSlot]
            //   Temporal: current + history(=mHistorySlot) -> temp
            //   Spatial:  temp -> current (覆盖 Initial 写入, 形成最终 reservoir, 也是下一帧的 history)
            //   Resolve:  current -> IndirectDiffuse RT
            //
            // 当关闭 Temporal 时跳过 Spatial 一并跳过, 因为 Spatial 依赖 temp, temp 由 Temporal 产出
            // 如要单独使用 Spatial, 需要把 Initial 直接写入 temp, 这里为简化只支持 (None | Temporal | Temporal+Spatial)
            mInitial.SetDrawcallDispatch(this, policy, mInitialDraw, w, h, 1, true);

            bool runTemporal = EnableTemporal;
            bool runSpatial  = EnableTemporal && EnableSpatial;
            if (runTemporal)
                mTemporal.SetDrawcallDispatch(this, policy, mTemporalDraw, w, h, 1, true);
            if (runSpatial)
                mSpatial.SetDrawcallDispatch(this, policy, mSpatialDraw, w, h, 1, true);
            mResolve.SetDrawcallDispatch(this, policy, mResolveDraw, w, h, 1, true);

            var cmd = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new TtCmdListScope(cmd, "ReSTIRGI"))
            {
                cmd.PushGpuDraw(mInitialDraw);
                if (runTemporal)
                    cmd.PushGpuDraw(mTemporalDraw);
                if (runSpatial)
                    cmd.PushGpuDraw(mSpatialDraw);
                cmd.PushGpuDraw(mResolveDraw);
                cmd.FlushDraws();
            }
            policy.CommitCommandList(cmd, "ReSTIRGI");
        }
    }
}
