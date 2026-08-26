using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    /// <summary>
    /// 贴花 ShadingEnv。所有绑定都在 OnDrawCall 完成 (CodingGuidelines §1.2)。
    ///
    /// 不用 ShadingEnv permutation: <c>ENV_DECAL_WRITE_NORMAL</c> 由材质在
    /// <c>TtMaterial.UpdateShaderCode</c> 里注入 (仅 RL_Decal 材质), 走
    /// "effect hash 含材质资产名 + MaterialHash 变化自动 Refresh" 的既有机制,
    /// 因此两种 DecalMode 天然分裂成两个 effect, 单个 ShadingEnv 服务全部贴花。
    ///
    /// 逐贴花的参数通过 <c>atom.RenderMesh.HostNode</c> 回溯到 <see cref="GamePlay.Scene.TtDecalNode"/>
    /// 取得 —— 每个贴花有自己的 box mesh、自己的 atom、自己的 drawcall, 因此逐贴花 cbuffer
    /// 天然满足 §1.3 (同帧多 drawcall 不能共享 CBV)。
    /// </summary>
    public class TtDecalShading : Shader.TtGraphicsShadingEnv
    {
        public TtDecalShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Decal/DecalPS.cginc", RName.ERNameType.Engine);
        }

        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            // 盒子 mesh 有完整的 Position/Normal/Tangent/Color/UV 流 (MeshDataProvider.MakeBox)。
            // 材质图需要 UV; Normal/Tangent 必须声明, 否则 PS_INPUT 没有这两个字段,
            // CalcNormalMap 的 TBN 路径 (USE_PS_Normal/USE_PS_Tangent) 会被编译掉。
            return new NxRHI.EVertexStreamType[] {
                NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,
                NxRHI.EVertexStreamType.VST_Normal,
                NxRHI.EVertexStreamType.VST_Tangent,
            };
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            // 与 GetNeedStreams 对齐 (PS_INPUT 字段需要 VS 能填):
            //   UV      -> 材质图采样 (PS 里被覆盖为投影 UV)
            //   Normal/Tangent -> CalcNormalMap 的 TBN (PS 里被覆盖为贴花切线基)
            //   WorldPos -> 材质图的世界坐标节点 (PS 里被覆盖为被投影表面坐标)
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
                EPixelShaderInput.PST_UV,
                EPixelShaderInput.PST_Normal,
                EPixelShaderInput.PST_Tangent,
                EPixelShaderInput.PST_WorldPos,
            };
        }

        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall,
            TtRenderPolicy policy, TtRenderMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var node = drawcall.TagObject as TtDecalPassNode;
            if (node == null)
                return;

            // ---- 逐 RT 的 blend / 写掩码靠覆盖 PSO 实现 (材质的 DecalMode 决定) ----
            // 先例: Shadow/ShadowMapNode.cs:39, Bricks/AdvanceShadow/AdvanceShadowShading.cs:44
            var decalMode = atom?.Material?.DecalMode ?? Shader.TtMaterial.EDecalMode.ColorAndMaterial;
            var pipeline = node.GetPipeline(decalMode == Shader.TtMaterial.EDecalMode.WithNormal);
            if (pipeline != null)
                drawcall.BindPipeline(pipeline);

            var samplerMgr = TtEngine.Instance.GfxDevice.SamplerStateManager;

            var index = drawcall.FindBinder("DecalDepthBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = node.GetAttachBuffer(node.DepthPinIn);
                if (attachBuffer?.Srv != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_DecalDepthBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, samplerMgr.PointState);

            // rt3 永远不是本 pass 的 RTV, 可以安全当 SRV 读 (取 RenderFlags 判 ShadingMode,
            // 对 Hair 像素跳过法线写入)。注意 rt1 统一 3-RT 后是 RTV, 不能再读 ——
            // 角度淡出改用几何法线, 见 DecalPS.cginc。
            index = drawcall.FindBinder("DecalGBufferRT3");
            if (index.IsValidPointer)
            {
                var attachBuffer = node.GetAttachBuffer(node.Rt3PinIn);
                if (attachBuffer?.Srv != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_DecalGBufferRT3");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, samplerMgr.PointState);

            // 贴花贴图不再在这里绑 —— 颜色 / 法线 / PBR 采样全部由材质图的
            // DX_AUTOBIND 资源接管 (BuildDrawCall 里按 Material.UsedSrView 绑定),
            // 对应 UE 的 MaterialDomain=Deferred Decal。

            // ---- 逐贴花 cbuffer (权重 + 淡出参数) ----
            var decal = atom?.RenderMesh?.HostNode as GamePlay.Scene.TtDecalNode;
            index = drawcall.FindBinder("cbDecal");
            if (index.IsValidPointer && decal != null)
                drawcall.BindCBV(index, decal.GetOrCreateDecalCBuffer(index, node.GlobalIntensity));
        }
    }

    /// <summary>
    /// 延迟贴花 pass —— 直写 GBuffer, 无 DBuffer。
    ///
    /// 逐贴花光栅化投影盒, 按深度重建世界坐标后做盒体剔除, 输出到 GBuffer 的 MRT,
    /// 由 <b>逐 RT 的硬件混合 + 写掩码</b> 完成合成。贴花视觉完全由贴花节点的
    /// <see cref="GamePlay.Scene.TtDecalNode.TtDecalNodeData.DecalMaterial"/> 材质定义
    /// (RenderLayer 必须为 RL_Decal, 对应 UE 的 MaterialDomain=Deferred Decal)。
    ///
    /// 两种 DecalMode 共用同一个 <b>统一 3-RT</b> render pass (rt0/rt1/rt2 -> slot 0/1/2),
    /// 仅 PSO 不同:
    ///
    /// <list type="table">
    /// <item><description>ColorAndMaterial (缺省): slot1 (法线) 写掩码全 0, 只叠 rt0/rt2</description></item>
    /// <item><description>WithNormal: slot1 开写但关混合 (oct 编码不能线性混合),
    /// 淡出在世界空间 lerp 烤进法线值</description></item>
    /// </list>
    ///
    /// 写掩码一律只开 RGB, 保住 rt0.a / rt1.a / rt2.a; rt3 只读不写。
    /// 因此本节点<b>零额外显存</b> —— 这是相对早期 DBuffer 方案的主要收益, 详见
    /// Documents/Roadmap.md §7.1。
    ///
    /// GBuffer 的三张 RT 以 InputOutput pin 穿过本节点, 所以节点被 Enable=false 关掉时
    /// 天然是<b>直通</b>的 (下游读到的就是 BasePass 写的那份), 不会像 DBuffer 方案那样留垃圾。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("Decal", "Deferred\\Decal",
        Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtDecalPassNode : TAuxRenderGraphNode<TtDecalPassNode>
    {
        // ---- Pins ----
        public TtRenderGraphPin VisiblesPinIn = TtRenderGraphPin.CreateInput(
            "Visibles", EBufferType.BFT_NONE);

        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput(
            "Depth", EBufferType.BFT_SRV);

        // GBuffer 的 rt0 / rt1 / rt2 穿过本节点 (读 + 写)
        public TtRenderGraphPin Rt0PinInOut = TtRenderGraphPin.CreateInputOutput(
            "MRT0", EBufferType.BFT_RTV | EBufferType.BFT_SRV);
        public TtRenderGraphPin Rt1PinInOut = TtRenderGraphPin.CreateInputOutput(
            "MRT1", EBufferType.BFT_RTV | EBufferType.BFT_SRV);
        public TtRenderGraphPin Rt2PinInOut = TtRenderGraphPin.CreateInputOutput(
            "MRT2", EBufferType.BFT_RTV | EBufferType.BFT_SRV);

        /// <summary>rt3 只读 (取 RenderFlags 判 ShadingMode, 对 Hair 像素跳过法线写入)</summary>
        public TtRenderGraphPin Rt3PinIn = TtRenderGraphPin.CreateInput(
            "MRT3", EBufferType.BFT_SRV);

        // ---- Parameters ----
        /// <summary>贴花强度的全局缩放, 便于整体压掉而不用改每个贴花。</summary>
        [Rtti.Meta("")]
        [Category("Decal")]
        public float GlobalIntensity { get; set; } = 1.0f;

        // ---- Internal ----
        // 单一 ShadingEnv: DecalMode 的变体分裂由材质 effect hash 天然完成 (见 TtDecalShading 注释)
        TtDecalShading mShading;

        // 统一 3-RT (rt0/rt1/rt2 -> slot 0/1/2): 两种 DecalMode 共用, 区别只在 PSO 写掩码。
        // 统一的代价: rt1 是 RTV 不能再当 SRV 读, 角度淡出改用几何法线 (DecalPS.cginc)
        TtGraphicsBuffers mGBuffers = new TtGraphicsBuffers();
        NxRHI.TtRenderPass mRenderPass;
        public TtGraphicsBuffers.TtTargetViewIdentifier TargetViewId = new TtGraphicsBuffers.TtTargetViewIdentifier();

        NxRHI.TtGpuPipeline mPipelineNoNormal;
        NxRHI.TtGpuPipeline mPipelineWithNormal;

        public TtCpuCullingNode CpuCullNode = null;

        readonly List<GamePlay.Scene.TtDecalNode> mDecals = new List<GamePlay.Scene.TtDecalNode>();

        public TtDecalPassNode()
        {
            Name = "DecalPassNode";
        }

        public NxRHI.TtGpuPipeline GetPipeline(bool writeNormal)
        {
            return writeNormal ? mPipelineWithNormal : mPipelineNoNormal;
        }

        public override void InitNodePins()
        {
            AddInput(VisiblesPinIn);
            AddInput(DepthPinIn);
            AddInputOutput(Rt0PinInOut);
            AddInputOutput(Rt1PinInOut);
            AddInputOutput(Rt2PinInOut);
            AddInput(Rt3PinIn);
        }

        /// <summary>恒返回单一 ShadingEnv: DecalMode 的变体分裂由材质 effect hash 天然完成, 无需多实例。</summary>
        public override TtGraphicsShadingEnv GetPassShading(TtRenderMesh.TtAtom atom = null)
        {
            return mShading;
        }

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            mShading = await TtShadingEnv.CreateShadingEnv<TtDecalShading>();

            CreateRenderPasses(policy);
            CreatePipelines();

            var linker = VisiblesPinIn.FindInLinker();
            if (linker != null)
                CpuCullNode = linker.OutPin.HostNode as TtCpuCullingNode;
        }

        unsafe void CreateRenderPasses(TtRenderPolicy policy)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            // ---- 统一 3-RT: (rt0, rt1, rt2) -> slot 0/1/2 ----
            // 两种 DecalMode 共用; ColorAndMaterial 用写掩码 0 屏蔽 slot1 (见 CreatePipelines)
            var desc = new NxRHI.FRenderPassDesc();
            desc.NumOfMRT = 3;
            desc.AttachmentMRTs[0].Format = Rt0PinInOut.Attachement.Format;
            desc.AttachmentMRTs[0].Samples = 1;
            // 必须 Load: GBuffer 已经被 BasePass 写过了, Clear 会把它擦掉
            desc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionLoad;
            desc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            desc.AttachmentMRTs[1].Format = Rt1PinInOut.Attachement.Format;
            desc.AttachmentMRTs[1].Samples = 1;
            desc.AttachmentMRTs[1].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionLoad;
            desc.AttachmentMRTs[1].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            desc.AttachmentMRTs[2].Format = Rt2PinInOut.Attachement.Format;
            desc.AttachmentMRTs[2].Samples = 1;
            desc.AttachmentMRTs[2].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionLoad;
            desc.AttachmentMRTs[2].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            mRenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in desc);
            mGBuffers.Initialize(policy, mRenderPass);
            mGBuffers.SetRenderTarget(policy, 0, Rt0PinInOut);
            mGBuffers.SetRenderTarget(policy, 1, Rt1PinInOut);
            mGBuffers.SetRenderTarget(policy, 2, Rt2PinInOut);
            mGBuffers.TargetViewIdentifier = TargetViewId;
        }

        /// <summary>SrcAlpha/InvSrcAlpha 的 RGB 混合; 写掩码只开 RGB 以保住各 RT 的 alpha。</summary>
        static NxRHI.FRenderTargetBlendDesc MakeBlendDesc(bool blendEnable)
        {
            var b = new NxRHI.FRenderTargetBlendDesc();
            b.m_BlendEnable = blendEnable ? 1 : 0;
            b.m_SrcBlend = NxRHI.EBlend.BLD_SRC_ALPHA;
            b.m_DestBlend = NxRHI.EBlend.BLD_INV_SRC_ALPHA;
            b.m_BlendOp = NxRHI.EBlendOp.BLDOP_ADD;
            // alpha 被写掩码挡掉, 这里的 alpha 混合因子实际不生效, 填成"保留 dst"最安全
            b.m_SrcBlendAlpha = NxRHI.EBlend.BLD_ZERO;
            b.m_DestBlendAlpha = NxRHI.EBlend.BLD_ONE;
            b.m_BlendOpAlpha = NxRHI.EBlendOp.BLDOP_ADD;
            b.m_RenderTargetWriteMask = 0x07;   // R|G|B, 屏蔽 A
            return b;
        }

        unsafe void CreatePipelines()
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var blendOn = MakeBlendDesc(true);
            var blendOff = MakeBlendDesc(false);

            // ColorAndMaterial 的 slot1: 完全不写 (写掩码 0), 只是为了 MRT 布局与 render pass 一致
            var maskOff = new NxRHI.FRenderTargetBlendDesc();
            maskOff.m_BlendEnable = 0;
            maskOff.m_RenderTargetWriteMask = 0;

            // ---- 缺省 ColorAndMaterial: 3-RT, slot1 屏蔽 ----
            var d0 = new NxRHI.FGpuPipelineDesc();
            d0.SetDefault();
            // 只画背面 + 完全不做深度测试: 覆盖判定纯靠盒体数学 + discard
            d0.m_Rasterizer.m_CullMode = NxRHI.ECullMode.CMD_FRONT;
            d0.m_DepthStencil.m_DepthEnable = 0;
            d0.m_Blend.m_IndependentBlendEnable = 1;
            d0.m_Blend.RenderTarget0 = blendOn;
            d0.m_Blend.RenderTarget1 = maskOff;
            d0.m_Blend.RenderTarget2 = blendOn;
            mPipelineNoNormal = TtEngine.Instance.GfxDevice.PipelineManager.GetPipelineState(rc, in d0);

            // ---- WithNormal: 3-RT, slot1 (法线) 开写但关混合 ----
            var d1 = new NxRHI.FGpuPipelineDesc();
            d1.SetDefault();
            d1.m_Rasterizer.m_CullMode = NxRHI.ECullMode.CMD_FRONT;
            d1.m_DepthStencil.m_DepthEnable = 0;
            d1.m_Blend.m_IndependentBlendEnable = 1;
            d1.m_Blend.RenderTarget0 = blendOn;
            d1.m_Blend.RenderTarget1 = blendOff;   // 法线: 八面体编码不能线性混合
            d1.m_Blend.RenderTarget2 = blendOn;
            mPipelineWithNormal = TtEngine.Instance.GfxDevice.PipelineManager.GetPipelineState(rc, in d1);
        }

        public override void Dispose()
        {
            mGBuffers?.Dispose();
            mGBuffers = null;
            base.Dispose();
        }

        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            mGBuffers?.SetSize(x, y);
        }

        int CollectDecals()
        {
            mDecals.Clear();

            var visibleNodes = CpuCullNode?.VisParameter?.VisibleNodes;
            if (visibleNodes == null)
                return 0;

            foreach (var i in visibleNodes)
            {
                var decal = i as GamePlay.Scene.TtDecalNode;
                if (decal == null)
                    continue;
                var data = decal.DecalData;
                if (data == null)
                    continue;
                data.EnsureMaterial();

                // DecalMode 是材质的属性, 分组交给 PSO 写掩码 (OnDrawCall), 这里单列表统一排序
                mDecals.Add(decal);
            }

            // SortOrder 小的先画, 大的盖在上面
            mDecals.Sort(static (a, b) => a.DecalData.SortOrder.CompareTo(b.DecalData.SortOrder));

            return mDecals.Count;
        }

        unsafe void DrawGroup(NxRHI.TtCommandList cmd, TtRenderPolicy policy,
            TtGraphicsBuffers gbuffers, List<GamePlay.Scene.TtDecalNode> decals, string passName)
        {
            if (decals.Count == 0 || gbuffers == null)
                return;

            cmd.SetViewport(in gbuffers.Viewport);
            var scissor = new NxRHI.FScissorRect();
            scissor.MinX = 0;
            scissor.MinY = 0;
            scissor.MaxX = (int)gbuffers.Viewport.Width;
            scissor.MaxY = (int)gbuffers.Viewport.Height;
            cmd.SetScissor(in scissor);

            var passClears = new NxRHI.FRenderPassClears();
            passClears.SetDefault();
            // 一个 clear 都不能做: 我们是往已经写好的 GBuffer 上叠加
            passClears.ClearFlags = (NxRHI.ERenderPassClearFlags)0;

            gbuffers.BuildFrameBuffers(policy);
            cmd.BeginPass(gbuffers.FrameBuffers, in passClears, passName);

            for (int i = 0; i < decals.Count; i++)
            {
                var mesh = decals[i].ProjectionMesh;
                if (mesh == null)
                    continue;
                foreach (var sub in mesh.SubMeshes)
                {
                    foreach (var atom in sub.Atoms)
                    {
                        if (atom == null || atom.Material == null)
                            continue;
                        var drawcall = atom.GetDrawCall(cmd.mCoreObject, gbuffers, policy, this, true);
                        if (drawcall == null)
                            continue;
                        drawcall.TagObject = this;
                        drawcall.BindCBV(drawcall.Effect.BindIndexer.cbPerViewport, gbuffers.PerViewportCBuffer);
                        drawcall.BindCBV(drawcall.Effect.BindIndexer.cbPerCamera, policy.DefaultCamera.PerCameraCBuffer);
                        cmd.PushGpuDraw(drawcall);
                    }
                }
            }

            cmd.FlushDraws();
            cmd.EndPass();
        }

        public override unsafe void Tick(GamePlay.TtWorld world,
            TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (policy.EnableDecal == false)
                return;
            if (mShading == null)
                return;
            if (mPipelineNoNormal == null || mPipelineWithNormal == null)
                return;

            if (CollectDecals() == 0)
                return;

            var cmd = TtCommandList.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmd, "Decal"))
            {
                // 不需要单独判 shading env 就绪: effect 未完成时 atom.GetDrawCall 会返回 null
                // (与 TtSceenSpaceNode 同一套兼容逻辑)
                DrawGroup(cmd, policy, mGBuffers, mDecals, "Decal");
            }
            policy.CommitCommandList(cmd, "Decal");
        }
    }
}
