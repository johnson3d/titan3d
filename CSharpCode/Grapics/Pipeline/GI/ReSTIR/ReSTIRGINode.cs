using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.Graphics.Pipeline.GI.ProbeVolume;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.ComponentModel;
using System.Reflection;

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

        // ENV_USE_HZB: 0 -> 屏幕空间线性 ray march (与原行为一致, 不依赖 hzb 输入)
        //              1 -> Hi-Z 加速 ray march, 需要 Hzb pin 接入
        // Hzb / Samp_HzbPoint binder 仅在 1 的编译产物里出现, OnDrawCall 用 FindBinder
        // + IsValidPointer 双重保护; HzbPinIn 悬空时, C# 端会强制保持 0.
        // 仅 ENV_USE_HW_RT == 0 的 SS 变体下生效, HW RT 路径不走 hzb.
        public TtPermutationItem EnableHzbAccel { get; set; }
        [Category("Option")]
        public bool IsEnableHzbAccel
        {
            get { return EnableHzbAccel.GetValue() == (int)EPermutation_Bool.TrueValue; }
            set { EnableHzbAccel.SetValue(value); this.UpdatePermutation().AddWaitTask(); }
        }

        // ENV_USE_PROBE_FALLBACK: 0 -> miss 时仅用天光
        //                         1 -> miss 时先查 probe volume irradiance, 查询失败再天光
        // ProbeVolume buffer 仅在 1 的编译产物里存在 binder, OnDrawCall 用 FindBinder
        // + IsValidPointer 双重保护; ProbeVolumeNode 为 null 时, C# 端会强制保持 0.
        public TtPermutationItem EnableProbeFallback { get; set; }
        [Category("Option")]
        public bool IsEnableProbeFallback
        {
            get { return EnableProbeFallback.GetValue() == (int)EPermutation_Bool.TrueValue; }
            set { EnableProbeFallback.SetValue(value); this.UpdatePermutation().AddWaitTask(); }
        }

        // ENV_USE_UNLIT_SKIP: 0 -> 不检测 Unlit 材质 (MRT3 pin 悬空时强制为 0)
        //                     1 -> 从 GBufferRT3 提取 ShadingMode, Unlit 像素提前退出
        // GBufferRT3 binder 仅在 1 的编译产物里存在, OnDrawCall 用 FindAttachBuffer
        // + binder 保护; MRT3 pin 悬空时, C# 端会强制保持 0.
        public TtPermutationItem EnableUnlitSkip { get; set; }
        [Category("Option")]
        public bool IsEnableUnlitSkip
        {
            get { return EnableUnlitSkip.GetValue() == (int)EPermutation_Bool.TrueValue; }
            set { EnableUnlitSkip.SetValue(value); this.UpdatePermutation().AddWaitTask(); }
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
            EnableHzbAccel = this.PushPermutation<EPermutation_Bool>("ENV_USE_HZB", (int)EPermutation_Bool.BitWidth);
            EnableHzbAccel.SetValue((int)EPermutation_Bool.FalseValue);
            EnableProbeFallback = this.PushPermutation<EPermutation_Bool>("ENV_USE_PROBE_FALLBACK", (int)EPermutation_Bool.BitWidth);
            EnableProbeFallback.SetValue((int)EPermutation_Bool.FalseValue);
            EnableUnlitSkip = this.PushPermutation<EPermutation_Bool>("ENV_USE_UNLIT_SKIP", (int)EPermutation_Bool.BitWidth);
            EnableUnlitSkip.SetValue((int)EPermutation_Bool.FalseValue);

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

            // GBufferRT3: 仅在 ENV_USE_UNLIT_SKIP=1 编译产物里存在 binder.
            // MRT3 pin 悬空时, permutation 一定是 0, binder 不存在, FindAttachBuffer 返回 null.
            var mrt3Buffer = node.FindAttachBuffer(node.GBufferRT3PinIn);
            if (mrt3Buffer != null)
                drawcall.BindSrv("GBufferRT3", mrt3Buffer.Srv);
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

            // Hzb: 仅在 ENV_USE_HZB=1 编译产物里存在 binder.
            // HzbPinIn 允许悬空 (FindAttachBuffer 返回 null), 此时 permutation 一定是 0
            // (由节点的 EnableHzbAccel setter 强制保证), binder 不存在, 与 EnvMap 同一套保护.
            var hzbBuffer = node.FindAttachBuffer(node.HzbPinIn);
            if (hzbBuffer != null)
            {
                var hzbBinder = drawcall.FindBinder(EShaderBindType.SBT_SRV, "Hzb");
                if (hzbBinder.IsValidPointer)
                    drawcall.BindSrv(hzbBinder, hzbBuffer.Srv);
                var hzbSampBinder = drawcall.FindBinder(EShaderBindType.SBT_Sampler, "Samp_HzbPoint");
                if (hzbSampBinder.IsValidPointer)
                    drawcall.BindSampler(hzbSampBinder, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            }

            // ProbeVolume: 仅在 ENV_USE_PROBE_FALLBACK=1 编译产物里存在 binder.
            // ProbeVolumeNode 为 null 或未 Ready 时, permutation 一定是 0
            // (由节点的 EnableProbeFallback setter 强制保证), binder 不存在, 与 EnvMap 同一套保护.
            var probeNode = node.ProbeVolumeSource;
            if (probeNode != null && probeNode.IsReady)
            {
                var probeInfoBinder = drawcall.FindBinder(EShaderBindType.SBT_SRV, "ProbeInfos");
                if (probeInfoBinder.IsValidPointer)
                    drawcall.BindSrv(probeInfoBinder, probeNode.ProbeInfoBuffer.Srv);
                var probeSHBinder = drawcall.FindBinder(EShaderBindType.SBT_SRV, "ProbeSH");
                if (probeSHBinder.IsValidPointer)
                    drawcall.BindSrv(probeSHBinder, probeNode.ProbeSHBuffer.Srv);
                var tetraBinder = drawcall.FindBinder(EShaderBindType.SBT_SRV, "ProbeTetrahedra");
                if (tetraBinder.IsValidPointer)
                    drawcall.BindSrv(tetraBinder, probeNode.TetraBuffer.Srv);
                var bvhBinder = drawcall.FindBinder(EShaderBindType.SBT_SRV, "ProbeBvhNodes");
                if (bvhBinder.IsValidPointer)
                    drawcall.BindSrv(bvhBinder, probeNode.BvhBuffer.Srv);
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

        // ENV_USE_UNLIT_SKIP: 与 InitialSampling 同一套 permutation, 同步开关.
        // GBufferRT3 binder 仅在 1 的编译产物里存在.
        public TtPermutationItem EnableUnlitSkip { get; set; }
        [Category("Option")]
        public bool IsEnableUnlitSkip
        {
            get { return EnableUnlitSkip.GetValue() == (int)EPermutation_Bool.TrueValue; }
            set { EnableUnlitSkip.SetValue(value); this.UpdatePermutation().AddWaitTask(); }
        }

        public TtReSTIRSpatialReuseShading()
        {
            CodeName = RName.GetRName("Shaders/GI/ReSTIR/ReSTIRSpatialReuse.compute", RName.ERNameType.Engine);
            MainName = "CS_Main";

            this.BeginPermutaion();
            EnableUnlitSkip = this.PushPermutation<EPermutation_Bool>("ENV_USE_UNLIT_SKIP", (int)EPermutation_Bool.BitWidth);
            EnableUnlitSkip.SetValue((int)EPermutation_Bool.FalseValue);

            UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtReSTIRGINode;
            if (node == null)
                return;

            drawcall.BindSrv("GBufferRT1", node.GetAttachBuffer(node.GBufferRT1PinIn).Srv);
            drawcall.BindSrv("DepthBuffer", node.GetAttachBuffer(node.DepthPinIn).Srv);

            // GBufferRT3: 仅在 ENV_USE_UNLIT_SKIP=1 编译产物里存在 binder.
            var mrt3Buffer = node.FindAttachBuffer(node.GBufferRT3PinIn);
            if (mrt3Buffer != null)
                drawcall.BindSrv("GBufferRT3", mrt3Buffer.Srv);
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
        public TtRenderGraphPin GBufferRT3PinIn  = TtRenderGraphPin.CreateInput("MRT3", EBufferType.BFT_SRV);
        public TtRenderGraphPin DepthPinIn       = TtRenderGraphPin.CreateInput("Depth", EBufferType.BFT_SRV | EBufferType.BFT_DSV);
        public TtRenderGraphPin MotionVectorPinIn = TtRenderGraphPin.CreateInput("MotionVector", EBufferType.BFT_SRV);
        public TtRenderGraphPin PrevColorPinIn   = TtRenderGraphPin.CreateInput("PrevColor", EBufferType.BFT_SRV);
        // EnvMap (TextureCube) 输入 pin, 允许悬空:
        //   - 不接 -> EnableEnvMap 强制 false, shader ENV_USE_SKY_CUBE=0, miss 时用 SkyColor 常量
        //   - 接入 -> 用户主动开 EnableEnvMap = true, shader ENV_USE_SKY_CUBE=1, miss 时采 EnvMap
        public TtRenderGraphPin EnvMapPinIn      = TtRenderGraphPin.CreateInput("EnvMap", EBufferType.BFT_SRV);
        // Hzb (Texture2D<float2>, multi-mip min/max depth) 输入 pin, 允许悬空:
        //   - 不接 -> EnableHzbAccel 强制 false, shader ENV_USE_HZB=0, 屏幕空间 ray march 走线性步长 fallback
        //   - 接入 -> 自动开 EnableHzbAccel = true, shader ENV_USE_HZB=1, 走 hi-z 加速 march
        // 仅 SS 路径 (ENV_USE_HW_RT=0) 下生效, HW RT 路径不消费 hzb.
        public TtRenderGraphPin HzbPinIn         = TtRenderGraphPin.CreateInput("Hzb", EBufferType.BFT_SRV);

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

        // ENV_USE_HZB permutation 开关: 配置型切换, 同 EnableEnvMap.
        // 与 EnvMap 同样的"pin 悬空 -> 强制关"保护: HzbPinIn 没接时, Initialize / Tick
        // 会把这个值刷成 false, 即使外部尝试 set true 也无效 (setter 会被 Tick 覆盖回去).
        // 用户主动接入 HzbPinIn -> Initialize 阶段自动设为 true, 后续可以通过 setter
        // 在质量档之间切换 (off/on), 但不要每帧切换 (会触发 effect 重新编译).
        bool mEnableHzbAccel = false;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public bool EnableHzbAccel
        {
            get { return mEnableHzbAccel; }
            set
            {
                if (mEnableHzbAccel == value)
                    return;

                if (mHzbNode == null)
                {
                    mEnableHzbAccel = false;
                    return;
                }
                mEnableHzbAccel = value;
                if (mInitial != null)
                    mInitial.IsEnableHzbAccel = value;
            }
        }
        TtHzbNode mHzbNode = null;
        // ENV_USE_PROBE_FALLBACK permutation 开关: 配置型切换, 同 EnableEnvMap.
        // ProbeVolumeSource 为 null 或未 Ready 时, setter 会被 Tick 覆盖回 false.
        // 用户主动注入 ProbeVolumeSource -> 自动设为 true.
        bool mEnableProbeFallback = false;
        [Category("ReSTIR")]
        [Rtti.Meta("")]
        public bool EnableProbeFallback
        {
            get { return mEnableProbeFallback; }
            set
            {
                if (mEnableProbeFallback == value)
                    return;
                mEnableProbeFallback = value;
                if (mInitial != null)
                    mInitial.IsEnableProbeFallback = value;
            }
        }

        // 由外部注入的 ProbeVolume 节点引用, 提供 probe SH / 四面体 / BVH 的 GPU buffer.
        // 节点本身不负责 probe 的生成和刷新, 只负责"如果有就用".
        // 注入后会自动开启 ENV_USE_PROBE_FALLBACK permutation.
        public TtProbeVolumeNode ProbeVolumeSource;

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
        const int ResizeSuppressFrameCount = 4;
        int mSuppressAfterResizeFrameCount = 0;

        public TtReSTIRGINode()
        {
            Name = "ReSTIRGINode";
        }

        public override void InitNodePins()
        {
            AddInput(GBufferRT0PinIn);
            AddInput(GBufferRT1PinIn);
            AddInput(GBufferRT2PinIn);
            // MRT3 允许悬空 (用 FindAttachBuffer 检测):
            //   不接 -> ENV_USE_UNLIT_SKIP=0, 不做 Unlit 材质跳过 (节省 GBufferRT3 读取开销)
            //   接入 -> ENV_USE_UNLIT_SKIP=1, 从 GBufferRT3 提取 ShadingMode 跳过 Unlit 像素
            AddInput(GBufferRT3PinIn);
            GBufferRT3PinIn.IsAllowInputNull = true;
            AddInput(DepthPinIn);
            AddInput(MotionVectorPinIn);
            // PrevColor 是 InitialSampling pass 唯一的间接光源, HLSL 端没有 fallback,
            // 不允许悬空; 接入侧需保证连接 AntiAliasing 节点的 PreColor 输出
            AddInput(PrevColorPinIn);
            // EnvMap 允许悬空 (用 FindAttachBuffer 检测), 不接时 fallback 到 SkyColor 常量
            AddInput(EnvMapPinIn);
            EnvMapPinIn.IsAllowInputNull = true;
            // Hzb 允许悬空 (用 FindAttachBuffer 检测), 不接时 hi-z 加速被强制关闭, march 走线性 fallback
            AddInput(HzbPinIn);
            HzbPinIn.IsAllowInputNull = true;

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

            // Hzb pin 接入则自动开 hi-z 加速; 悬空则强制关 (即便外部曾 set true).
            // 与 EnvMap 同一套"悬空 -> 强制关"防御.
            var linker = HzbPinIn.FindInLinker();
            if (linker != null)
            {
                mHzbNode = linker.OutPin.HostNode as TtHzbNode;
                EnableHzbAccel = true;
            }
            else
            {
                mHzbNode = null;
                EnableHzbAccel = false;
            }

            // ProbeVolume: 外部注入 ProbeVolumeSource 后自动开启; 未注入时强制关.
            mInitial.IsEnableProbeFallback = (ProbeVolumeSource != null && ProbeVolumeSource.IsReady);
            mEnableProbeFallback = mInitial.IsEnableProbeFallback;

            // MRT3 pin 接入则自动开 Unlit 跳过; 悬空则强制关.
            bool hasMRT3 = GBufferRT3PinIn.FindInLinker() != null;
            mInitial.IsEnableUnlitSkip = hasMRT3;

            await mInitial.UpdatePermutation();

            mTemporal = await TtShadingEnv.CreateShadingEnv<TtReSTIRTemporalReuseShading>();
            mSpatial = new TtReSTIRSpatialReuseShading();
            mSpatial.IsEnableUnlitSkip = hasMRT3;
            await mSpatial.UpdatePermutation();
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
            // 屏幕空间 GI 在 viewport resize 拖动中会短暂拿到不稳定的屏幕空间命中,
            // 容易把模型轮廓当成间接光投到背景上。尺寸稳定后再恢复 ReSTIR 输出。
            mSuppressAfterResizeFrameCount = ResizeSuppressFrameCount;

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
        float GetEffectiveIntensity()
        {
            return mSuppressAfterResizeFrameCount > 0 ? 0.0f : Intensity;
        }

        // HZB mip0 = screenSize/2, 最大 mip = floor(log2(max(mip0W, mip0H))).
        // 使用整数位运算避免浮点精度问题 (如 512.0 经 Math.Log2 得到 8.999… 被截断为 8).
        uint CalcHzbMaxMip()
        {
            uint hzbMip0Max = Math.Max(mWidth / 2, mHeight / 2);
            if (hzbMip0Max < 2)
                return 0;
            // BitOperations.Log2 等价于 31 - LeadingZeroCount, 即 floor(log2(n))
            return (uint)System.Numerics.BitOperations.Log2(hzbMip0Max);
        }

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
                mSharedCBuffer.SetValue("Intensity", GetEffectiveIntensity());
                mSharedCBuffer.SetValue("MaxRadiance", MaxRadiance);
                mSharedCBuffer.SetValue("InitialSampleCount", InitialSampleCount);
                var skyColor = SkyColor;
                mSharedCBuffer.SetValue("SkyColor", in skyColor);
                mSharedCBuffer.SetValue("SkyIntensity", SkyIntensity);
                mSharedCBuffer.SetValue("HzbMaxMip", CalcHzbMaxMip() - 2);
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
            mSharedCBuffer.SetValue("Intensity", GetEffectiveIntensity());
            mSharedCBuffer.SetValue("MaxRadiance", MaxRadiance);
            mSharedCBuffer.SetValue("InitialSampleCount", InitialSampleCount);
            var skyColorVal = SkyColor;
            mSharedCBuffer.SetValue("SkyColor", in skyColorVal);
            mSharedCBuffer.SetValue("SkyIntensity", SkyIntensity);

            var hzbBuffer = FindAttachBuffer(HzbPinIn);
            if (hzbBuffer != null)
            {
                mSharedCBuffer.SetValue("HzbMaxMip", hzbBuffer.Texture.mCoreObject.Desc.MipLevels - 1);
            }
            
            return mSharedCBuffer;
        }
        TtAttachBuffer mFallbackBlack = new TtAttachBuffer();
        public override void FrameBuild(TtRenderPolicy policy)
        {
            if (policy.EnableGI == false)
            {
                this.ImportAttachment(IndirectDiffusePinOut, mFallbackBlack);
                mFallbackBlack.Srv = TtEngine.Instance.GfxDevice.TextureManager.BlackTextureSRV;
            }
        }
        public override unsafe void Tick(TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            if (policy.EnableGI == false)
                return;
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

            var cmd = TtCommandList.GetCmdList();
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
            if (mSuppressAfterResizeFrameCount > 0)
                mSuppressAfterResizeFrameCount--;
        }
    }
}
