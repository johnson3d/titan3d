using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline.GI.ProbeVolume
{
    // =========================================================================
    // ShadingEnv - ProbeVolumeUpdate
    //   驱动 ProbeVolumeUpdate.compute, 每帧对一批 probe 做球面采样刷新 SH
    // =========================================================================
    public class TtProbeVolumeUpdateShading : TtComputeShadingEnv
    {
        // ProbeVolumeUpdate.compute 使用 1D dispatch: (ProbeCount, 1, 1)
        // DispatchArg 定义 numthreads, 这里用 64x1x1 (每个线程处理一个 probe)
        public override Vector3ui DispatchArg => new Vector3ui(64, 1, 1);

        // ENV_USE_SKY_CUBE: 0 -> miss 时用 cbProbeUpdate.SkyColor 常量天光
        //                   1 -> miss 时采 EnvMap (TextureCube), 需要 EnvMap pin 接入
        public TtPermutationItem EnableSkyCube { get; set; }
        [Category("Option")]
        public bool IsEnableSkyCube
        {
            get { return EnableSkyCube.GetValue() == (int)EPermutation_Bool.TrueValue; }
            set { EnableSkyCube.SetValue(value); this.UpdatePermutation().AddWaitTask(); }
        }

        public TtProbeVolumeUpdateShading()
        {
            CodeName = RName.GetRName("Shaders/GI/ProbeVolume/ProbeVolumeUpdate.compute", RName.ERNameType.Engine);
            MainName = "CS_Main";

            this.BeginPermutaion();
            EnableSkyCube = this.PushPermutation<EPermutation_Bool>("ENV_USE_SKY_CUBE", (int)EPermutation_Bool.BitWidth);
            EnableSkyCube.SetValue((int)EPermutation_Bool.FalseValue);

            UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtProbeVolumeNode;
            if (node == null)
                return;

            drawcall.BindSrv("PrevColor", node.GetAttachBuffer(node.PrevColorPinIn).Srv);
            drawcall.BindSrv("DepthBuffer", node.GetAttachBuffer(node.DepthPinIn).Srv);
            drawcall.BindSampler("Samp_PointClamp", TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            drawcall.BindCBV("cbPerCamera", policy.DefaultCamera.PerCameraCBuffer);

            // InProbeInfos (SRV, StructuredBuffer<ProbeInfo>)
            if (node.ProbeInfoBuffer != null)
            {
                var probeInfoBinder = drawcall.FindBinder(EShaderBindType.SBT_SRV, "InProbeInfos");
                if (probeInfoBinder.IsValidPointer)
                    drawcall.BindSrv(probeInfoBinder, node.ProbeInfoBuffer.Srv);
            }

            // InOutProbeSH (UAV, RWStructuredBuffer<PackedProbeSH>)
            if (node.ProbeSHBuffer != null)
            {
                var probeSHBinder = drawcall.FindBinder(EShaderBindType.SBT_UAV, "InOutProbeSH");
                if (probeSHBinder.IsValidPointer)
                    drawcall.BindUav(probeSHBinder, node.ProbeSHBuffer.Uav);
            }

            // ProbeTetrahedra / ProbeBvhNodes (SRV, probe-to-probe 传播所需)
            if (node.TetraBuffer != null)
            {
                var tetraBinder = drawcall.FindBinder(EShaderBindType.SBT_SRV, "ProbeTetrahedra");
                if (tetraBinder.IsValidPointer)
                    drawcall.BindSrv(tetraBinder, node.TetraBuffer.Srv);
            }
            if (node.BvhBuffer != null)
            {
                var bvhBinder = drawcall.FindBinder(EShaderBindType.SBT_SRV, "ProbeBvhNodes");
                if (bvhBinder.IsValidPointer)
                    drawcall.BindSrv(bvhBinder, node.BvhBuffer.Srv);
            }

            // EnvMap: 仅在 ENV_USE_SKY_CUBE=1 编译产物里存在 binder.
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

            // cbProbeUpdate
            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbProbeUpdate");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateCBuffer(cbBinder));
        }
    }

    // =========================================================================
    // TtProbeVolumeNode
    //   RenderGraph 节点, 负责:
    //   - 持有 TtProbeVolumeData (probe 位置 / 四面体 / BVH 的 CPU 端数据)
    //   - 维护 4 个 GPU StructuredBuffer (ProbeInfo, ProbeSH, Tetrahedra, BVH)
    //   - 每帧时间摊分 dispatch ProbeVolumeUpdate.compute 刷新一批 probe 的 SH
    //   - 输出 ProbeSH / Tetrahedra / BVH 的 SRV pin 供 ReSTIR 等下游节点消费
    //
    //   inputs (pin):  PrevColor, Depth
    //   outputs (pin): ProbeSH (SRV), ProbeTetra (SRV), ProbeBVH (SRV), ProbeInfo (SRV)
    //   这些输出不是 texture pin, 而是由节点直接暴露 TtGpuBuffer<T>.Srv 给下游
    //   (下游通过 FindAttachBuffer 拿不到 structured buffer, 所以改用直接字段引用)
    // =========================================================================
    [Bricks.CodeBuilder.ContextMenu("ProbeVolume", "GI\\ProbeVolume\\ProbeVolume", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtProbeVolumeNode : TAuxRenderGraphNode<TtProbeVolumeNode>
    {
        // ---------- Input Pins ----------
        public TtRenderGraphPin PrevColorPinIn = TtRenderGraphPin.CreateInput("PrevColor", EBufferType.BFT_SRV);
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput("Depth", EBufferType.BFT_SRV | EBufferType.BFT_DSV);
        // EnvMap (TextureCube) 输入 pin, 允许悬空:
        //   - 不接 -> EnableEnvMap 强制 false, shader ENV_USE_SKY_CUBE=0, miss 时用 SkyColor 常量
        //   - 接入 -> 用户主动开 EnableEnvMap = true, shader ENV_USE_SKY_CUBE=1, miss 时采 EnvMap
        public TtRenderGraphPin EnvMapPinIn = TtRenderGraphPin.CreateInput("EnvMap", EBufferType.BFT_SRV);

        // ---------- Tunable ----------
        [Category("ProbeVolume")]
        [Rtti.Meta("")]
        public float ProbeSpacing { get; set; } = 4.0f;

        [Category("ProbeVolume")]
        [Rtti.Meta("")]
        public uint RaysPerProbe { get; set; } = 64u;

        [Category("ProbeVolume")]
        [Rtti.Meta("")]
        public uint ProbesPerFrame { get; set; } = 32u;

        [Category("ProbeVolume")]
        [Rtti.Meta("")]
        public float HistoryBlendAlpha { get; set; } = 0.05f;

        [Category("ProbeVolume")]
        [Rtti.Meta("")]
        public Vector3 SkyColor { get; set; } = new Vector3(0.5f, 0.7f, 1.0f);

        [Category("ProbeVolume")]
        [Rtti.Meta("")]
        public float SkyIntensity { get; set; } = 1.0f;

        [Category("ProbeVolume")]
        [Rtti.Meta("")]
        public float MaxRayDistance { get; set; } = 50.0f;

        // ENV_USE_SKY_CUBE permutation 开关: 配置型切换, 同 ReSTIRGINode.EnableEnvMap.
        // EnvMapPinIn 悬空时, Initialize 会把这个值刷成 false.
        bool mEnableEnvMap = false;
        [Category("ProbeVolume")]
        [Rtti.Meta("")]
        public bool EnableEnvMap
        {
            get { return mEnableEnvMap; }
            set
            {
                if (mEnableEnvMap == value)
                    return;
                mEnableEnvMap = value;
                if (mUpdateShading != null)
                    mUpdateShading.IsEnableSkyCube = value;
            }
        }

        // ---------- GPU Buffers (下游节点直接引用) ----------
        public TtGpuBuffer<FProbeInfo> ProbeInfoBuffer { get; private set; }
        public TtGpuBuffer<FPackedProbeSH> ProbeSHBuffer { get; private set; }
        public TtGpuBuffer<FTetrahedronGpu> TetraBuffer { get; private set; }
        public TtGpuBuffer<FBvhNodeGpu> BvhBuffer { get; private set; }

        // ---------- CPU 端数据 ----------
        TtProbeVolumeData mVolumeData;

        // ---------- Shading & Draw ----------
        TtProbeVolumeUpdateShading mUpdateShading;
        TtComputeDraw mUpdateDraw;

        // ---------- CBuffer ----------
        TtCbView mCBuffer;
        uint mFrameIndex = 0;
        uint mCurrentProbeOffset = 0;

        public TtProbeVolumeNode()
        {
            Name = "ProbeVolumeNode";
        }

        public override void InitNodePins()
        {
            AddInput(PrevColorPinIn);
            AddInput(DepthPinIn);
            AddInput(EnvMapPinIn);
            EnvMapPinIn.IsAllowInputNull = true;
        }

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            mUpdateShading = new TtProbeVolumeUpdateShading();

            // EnvMap pin 接入则自动开; 悬空则强制关.
            if (EnvMapPinIn.FindInLinker() != null)
                mEnableEnvMap = true;
            else
                mEnableEnvMap = false;
            mUpdateShading.IsEnableSkyCube = mEnableEnvMap;

            await mUpdateShading.UpdatePermutation();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            mUpdateDraw = rc.CreateComputeDraw();
            mUpdateDraw.TagObject = this;

            ProbeInfoBuffer = new TtGpuBuffer<FProbeInfo>();
            ProbeSHBuffer = new TtGpuBuffer<FPackedProbeSH>();
            TetraBuffer = new TtGpuBuffer<FTetrahedronGpu>();
            BvhBuffer = new TtGpuBuffer<FBvhNodeGpu>();
        }

        // 外部调用: 设置场景 AABB, 自动生成网格 probe 并构建四面体/BVH, 上传 GPU
        public void SetupFromSceneAABB(in BoundingBox sceneAABB)
        {
            if (mVolumeData != null)
                mVolumeData.Dispose();

            mVolumeData = new TtProbeVolumeData();
            mVolumeData.GenerateGridProbes(in sceneAABB, ProbeSpacing);
            mVolumeData.Build();

            if (mVolumeData.IsBuilt)
            {
                mVolumeData.UploadToGpu(ProbeInfoBuffer, ProbeSHBuffer, TetraBuffer, BvhBuffer);
                mCurrentProbeOffset = 0;
            }
        }

        // 外部调用: 自定义 probe 位置列表
        public void SetupFromPositions(System.Collections.Generic.IList<Vector3> positions)
        {
            if (mVolumeData != null)
                mVolumeData.Dispose();

            mVolumeData = new TtProbeVolumeData();
            mVolumeData.SetProbes(positions);
            mVolumeData.Build();

            if (mVolumeData.IsBuilt)
            {
                mVolumeData.UploadToGpu(ProbeInfoBuffer, ProbeSHBuffer, TetraBuffer, BvhBuffer);
                mCurrentProbeOffset = 0;
            }
        }

        public bool IsReady => mVolumeData != null && mVolumeData.IsBuilt;
        public uint TotalProbeCount => mVolumeData != null ? (uint)mVolumeData.ProbeCount : 0u;

        // 4 个 pass 共用的 CBV 创建/更新 (在 OnDrawCall 中调用)
        public TtCbView GetOrCreateCBuffer(NxRHI.FShaderBinder binder)
        {
            uint totalProbes = TotalProbeCount;
            uint batchCount = Math.Min(ProbesPerFrame, totalProbes);

            uint bvhNodeCount = mVolumeData != null ? mVolumeData.GetBvhNodeCount() : 0u;

            if (mCBuffer == null)
            {
                mCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                mCBuffer.SetValue("ProbeStartIndex", mCurrentProbeOffset);
                mCBuffer.SetValue("ProbeCount", batchCount);
                mCBuffer.SetValue("RaysPerProbe", RaysPerProbe);
                mCBuffer.SetValue("FrameIndex", mFrameIndex);
                mCBuffer.SetValue("HistoryBlendAlpha", HistoryBlendAlpha);
                var skyColor = SkyColor;
                mCBuffer.SetValue("SkyColor", in skyColor);
                mCBuffer.SetValue("SkyIntensity", SkyIntensity);
                mCBuffer.SetValue("MaxRayDistance", MaxRayDistance);
                mCBuffer.SetValue("BvhNodeCount", bvhNodeCount);
                mCBuffer.MarkDirty();
                mCBuffer.FlushDirty();
                return mCBuffer;
            }

            mCBuffer.SetValue("ProbeStartIndex", mCurrentProbeOffset);
            mCBuffer.SetValue("ProbeCount", batchCount);
            mCBuffer.SetValue("RaysPerProbe", RaysPerProbe);
            mCBuffer.SetValue("FrameIndex", mFrameIndex);
            mCBuffer.SetValue("HistoryBlendAlpha", HistoryBlendAlpha);
            var skyColorVal = SkyColor;
            mCBuffer.SetValue("SkyColor", in skyColorVal);
            mCBuffer.SetValue("SkyIntensity", SkyIntensity);
            mCBuffer.SetValue("MaxRayDistance", MaxRayDistance);
            mCBuffer.SetValue("BvhNodeCount", bvhNodeCount);

            return mCBuffer;
        }

        public override unsafe void Tick(TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            if (mUpdateShading == null || !IsReady)
                return;

            mFrameIndex++;

            // 时间摊分: 每帧处理 ProbesPerFrame 个 probe, 循环遍历所有 probe
            uint totalProbes = TotalProbeCount;
            uint batchCount = Math.Min(ProbesPerFrame, totalProbes);
            if (batchCount == 0)
                return;

            // 推进偏移, 环形遍历
            if (mCurrentProbeOffset + batchCount > totalProbes)
                mCurrentProbeOffset = 0;

            // Dispatch: (batchCount, 1, 1), numthreads=(64,1,1)
            // SetDrawcallDispatch 第 3-5 参数是像素/元素维度,
            // 引擎内部会除以 DispatchArg (numthreads) 得到 group count
            mUpdateShading.SetDrawcallDispatch(this, policy, mUpdateDraw, batchCount, 1, 1, true);

            var cmd = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new TtCmdListScope(cmd, "ProbeVolumeUpdate"))
            {
                cmd.PushGpuDraw(mUpdateDraw);
                cmd.FlushDraws();
            }
            policy.CommitCommandList(cmd, "ProbeVolumeUpdate");

            // 推进偏移到下一批
            mCurrentProbeOffset += batchCount;
            if (mCurrentProbeOffset >= totalProbes)
                mCurrentProbeOffset = 0;
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mUpdateDraw);
            CoreSDK.DisposeObject(ref mCBuffer);

            if (ProbeInfoBuffer != null) { ProbeInfoBuffer.Dispose(); ProbeInfoBuffer = null; }
            if (ProbeSHBuffer != null) { ProbeSHBuffer.Dispose(); ProbeSHBuffer = null; }
            if (TetraBuffer != null) { TetraBuffer.Dispose(); TetraBuffer = null; }
            if (BvhBuffer != null) { BvhBuffer.Dispose(); BvhBuffer = null; }

            if (mVolumeData != null) { mVolumeData.Dispose(); mVolumeData = null; }

            base.Dispose();
        }
    }
}
