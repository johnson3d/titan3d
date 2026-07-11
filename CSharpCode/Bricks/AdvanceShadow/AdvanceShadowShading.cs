using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
//using Microsoft.Toolkit.HighPerformance.Buffers;

namespace EngineNS.Bricks.AdvanceShadow
{
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FAdvShadowLayerData")]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]//align 16 for cbv
    public struct FAdvShadowLayerData
    {
        public Vector2i mLayerStartAndSide;
        public Vector2 mLayerGridSize;
    }

    public class TtAdvanceShadowShading : Graphics.Pipeline.Shader.TtGraphicsShadingEnv
    {
        public TtAdvanceShadowShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/SSM.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position, NxRHI.EVertexStreamType.VST_UV, };
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
                //EPixelShaderInput.PST_Custom0,//for test
            };
        }
        public override void OnBuildDrawCall(TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall)
        {
            var shadowMapNode = drawcall.TagObject as TtAdvanceShadowMapNode;

            drawcall.mCoreObject.BindPipeline(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, shadowMapNode.DepthRaster.mCoreObject);
        }
        public override void OnDrawCall(ICommandList cmd, TtGraphicDraw drawcall, TtRenderPolicy policy, TtRenderMesh.TtAtom atom)
        {
            var rdgnd = drawcall.TagObject as TtAdvanceShadowMapNode;

            var index = drawcall.FindBinder("cbAdvanceShadow");
            if (index.IsValidPointer)
            {
                drawcall.BindCBV(index, rdgnd.mDirLightingCBV);
            }
            rdgnd.OnDrawCall(this, cmd, drawcall, policy, atom);
        }
    }

    public class TtESMShading : Graphics.Pipeline.Shader.TtGraphicsShadingEnv
    {
        public TtPermutationItem DisableESM { get; set; }

        public TtESMShading()
        {
            CodeName = RName.GetRName("shaders/Bricks/AdvanceShadow/ESM.cginc", RName.ERNameType.Engine);

            this.BeginPermutaion();
            DisableESM = this.PushPermutation<EPermutation_Bool>("DISABLE_ESM", (int)EPermutation_Bool.BitWidth);
            DisableESM.SetValue((int)EPermutation_Bool.FalseValue);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position, NxRHI.EVertexStreamType.VST_UV, };
        }
        public override void OnBuildDrawCall(TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall)
        {
            
        }
        public override void OnDrawCall(ICommandList cmd, TtGraphicDraw drawcall, TtRenderPolicy policy, TtRenderMesh.TtAtom atom)
        {
            var rdgnd = drawcall.TagObject as TtRenderGraphNode;

            rdgnd.OnDrawCall(this, cmd, drawcall, policy, atom);
        }
    }

    [Bricks.CodeBuilder.ContextMenu("AdvanceShadow", "Shadow\\AdvanceShadow", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtAdvanceShadowMapNode : TAuxRenderGraphNode<TtAdvanceShadowMapNode>
    {
        public TtRenderGraphPin DepthPinOut = TtRenderGraphPin.CreateOutput("Depth", false, EPixelFormat.PXF_R16_FLOAT, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);//or D32
        public TtRenderGraphPin SelfNodePinOut = TtRenderGraphPin.CreateOutput("Self", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_NONE);
        bool mIsDepth32 = false;
        [Rtti.Meta("")]
        [Category("Option")]
        public bool IsDepth32
        {
            get => mIsDepth32;
            set
            {
                mIsDepth32 = value;
            }
        }
        [Rtti.Meta("")]
        [Category("Option")]
        public int PageResolution { get; set; } = 128;

        [Rtti.Meta("")]
        [Category("Option")]
        public int PoolDimPages { get; set; } = 16;

        /// <summary>
        /// Maximum age (in frames) before an unrequested cached page is evicted.
        /// </summary>
        [Rtti.Meta("")]
        [Category("Option")]
        public uint PageMaxAge { get; set; } = 30;

        /// <summary>
        /// Enable directional light shadow via Clipmap.
        /// </summary>
        [Rtti.Meta("")]
        [Category("Option")]
        public bool EnableDirLightShadow { get; set; } = true;

        /// <summary>
        /// Enable local light (point/spot/area) shadow via QTree.
        /// </summary>
        [Rtti.Meta("")]
        [Category("Option")]
        public bool EnableLocalLightShadow { get; set; } = false;

        /// <summary>
        /// Enable Exponential Shadow Maps. When false, uses traditional depth comparison.
        /// Disabling ESM is useful for debugging depth precision issues with R16F.
        /// </summary>
        [Rtti.Meta("")]
        [Category("Option")]
        public bool EnableESM
        {
            get => mEnableESM;
            set
            {
                if (mEnableESM == value)
                    return;
                mEnableESM = value;
                if (mEsmShading != null)
                {
                    mEsmShading.DisableESM.SetValue(value ? (uint)EPermutation_Bool.FalseValue : (uint)EPermutation_Bool.TrueValue);
                    mEsmShading.UpdatePermutation().AddWaitTask();
                }
            }
        }
        private bool mEnableESM = true;

        /// <summary>
        /// PCF kernel half-size when ESM is disabled. 1=3x3, 2=5x5, 3=7x7, etc.
        /// </summary>
        [Rtti.Meta("")]
        [Category("Option")]
        public int PcfRadius { get; set; } = 2;

        /// <summary>
        /// Depth bias for PCF shadow comparison to reduce shadow acne.
        /// </summary>
        [Rtti.Meta("")]
        [Category("Option")]
        public float PcfDepthBias { get; set; } = 0.005f;

        // ---- QTree Configuration (moved from TtAdvanceShadowNode) ----
        [Rtti.Meta("")]
        [Category("QTree")]
        public DVector2 BoxCenter { get; set; } = DVector2.Zero;

        [Rtti.Meta("")]
        [Category("QTree")]
        public double BoxExtent { get; set; } = 1024;

        [Rtti.Meta("")]
        [Category("QTree")]
        public int MaxDeepLevel { get; set; } = 8;

        [Rtti.Meta("")]
        [Category("QTree")]
        public float MaxShadowDistance { get; set; } = 500.0f;

        [Rtti.Meta("")]
        [Category("QTree")]
        public int ShadowMapPage { get; set; } = 512;

        [Rtti.Meta("")]
        [Category("QTree")]
        public int MaxDirtyPagePerFrame { get; set; } = 3;

        [Category("QTree")]
        public float EsmConstant
        {
            get => mShadowQTree?.EsmConstant ?? 80.0f;
            set { if (mShadowQTree != null) mShadowQTree.EsmConstant = value; }
        }

        [Category("QTree")]
        public float MaxExp
        {
            get => mShadowQTree?.MaxExp ?? 50.0f;
            set { if (mShadowQTree != null) mShadowQTree.MaxExp = value; }
        }

        [Category("QTree")]
        public float GaussSigma
        {
            get => mShadowQTree?.GaussSigma ?? 1.5f;
            set { if (mShadowQTree != null) mShadowQTree.GaussSigma = value; }
        }

        // ---- Virtual Shadow Map Page Pool (Phase 1) ----
        public TtVSMPagePool PagePool { get; private set; }

        // ---- Phase 4: Clipmap for Directional Light ----
        public TtVSMClipmap Clipmap { get; private set; }

        [Rtti.Meta("")]
        [Category("Clipmap")]
        public int ClipmapLevelCount
        {
            get => mClipmapLevelCount;
            set
            {
                if (mClipmapLevelCount == value)
                    return;
                mClipmapLevelCount = value;
                RebuildClipmap();
            }
        }
        private int mClipmapLevelCount = 8;

        [Rtti.Meta("")]
        [Category("Clipmap")]
        public float ClipmapBaseHalfExtent
        {
            get => mClipmapBaseHalfExtent;
            set
            {
                if (MathF.Abs(mClipmapBaseHalfExtent - value) < 1e-6f)
                    return;
                mClipmapBaseHalfExtent = value;
                RebuildClipmap();
            }
        }
        private float mClipmapBaseHalfExtent = 8.0f;

        [Rtti.Meta("")]
        [Category("Clipmap")]
        public int ClipmapPagesPerDim
        {
            get => mClipmapPagesPerDim;
            set
            {
                if (mClipmapPagesPerDim == value)
                    return;
                mClipmapPagesPerDim = value;
                RebuildClipmap();
            }
        }
        private int mClipmapPagesPerDim = 4;

        // ---- Clipmap Debug (read-only, shown in Inspector) ----
        [Category("Clipmap Debug")]
        public int ClipmapDirtyPages => Clipmap?.DirtyPageCount ?? 0;

        [Category("Clipmap Debug")]
        public int ClipmapMappedPages => Clipmap?.MappedPageCount ?? 0;

        [Category("Clipmap Debug")]
        public int ClipmapCachedPages => Clipmap?.CachedPageCount ?? 0;

        [Category("Clipmap Debug")]
        public int ClipmapTotalVirtualPages => Clipmap?.TotalVirtualPages ?? 0;

        [Category("Clipmap Debug")]
        public int PagePoolAllocated => PagePool?.AllocatedPageCount ?? 0;

        [Category("Clipmap Debug")]
        public int PagePoolFree => PagePool?.FreePageCount ?? 0;

        // ---- Legacy resources kept for single-page depth pass (per-page rendering scratch) ----
        public TtTexture PageDepthTexture;
        public TtSrView PageDepthTextureSRV;
        public TtDepthStencilView PageDepthTextureDSV;

        // ---- Replaced: DepthTextureArray → PagePool.PhysicalPoolTexture ----
        public NxRHI.TtTexture DepthTextureArray;
        public NxRHI.TtSrView DepthTextureArraySRV;
        public TtAdvanceShadowShading mShadowShading;
        public TtESMShading mEsmShading;
        public NxRHI.TtGpuPipeline DepthRaster;
        public TtGraphicsBuffers mGBuffer;
        public TtRenderTargetView[] mRtViews;
        public TtSrView[] mDebuggerSRViews;
        public TtAttachBuffer DepthAttachment = new TtAttachBuffer();

        public TtCbView mDirLightingCBV = null;

        // Per-clipmap-page VP matrix + near/far, uploaded each frame for shadow projection
        public TtCpu2GpuBuffer<FVSMClipmapPageData> ClipmapPageDataBuffer;

        public Graphics.Mesh.TtRenderMesh ESMScreenMesh;
        public Graphics.Mesh.TtRenderMesh BlurScreenMesh;
        public TtGraphicsBuffers mDrawScreenGBuffers { get; protected set; } = new TtGraphicsBuffers();
        public TtAdvanceShadowMapNode()
        {
            Name = "AdvShadowMap";
        }
        public override void Dispose()
        {
            ClipmapPageDataBuffer?.Dispose();
            ClipmapPageDataBuffer = null;

            PagePool?.Dispose();
            PagePool = null;

            if (mRtViews != null)
            {
                foreach (var view in mRtViews)
                {
                    view.Dispose();
                }
                mRtViews = null;
            }
            CoreSDK.DisposeObject(ref DepthTextureArray);
            base.Dispose();
        }
        public override void InitNodePins()
        {
            AddOutput(DepthPinOut);
            AddOutput(SelfNodePinOut);
            SelfNodePinOut.LinkType = "AdvShadow";
        }
        public override TtGraphicsShadingEnv GetPassShading(TtRenderMesh.TtAtom atom = null)
        {
            var esm = atom.SubMesh.Mesh.Tag as TtESMShading;
            if (esm != null)
                return mEsmShading;
            else
                return mShadowShading;
        }
        public TtQTree mShadowQTree = null;
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            mShadowShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtAdvanceShadowShading>();
            mEsmShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtESMShading>();
            {
                mEsmShading.DisableESM.SetValue(this.EnableESM ? (uint)EPermutation_Bool.FalseValue : (uint)EPermutation_Bool.TrueValue);
                await mEsmShading.UpdatePermutation();
            }

            FTextureDesc desc = new FTextureDesc();
            desc.SetDefault();
            desc.BindFlags = EBufferType.BFT_SRV | EBufferType.BFT_DSV;
            desc.Width = (uint)PageResolution;
            desc.Height = (uint)PageResolution;
            desc.MipLevels = 1;
            desc.Format = (DepthPinOut.Attachement.Format == EPixelFormat.PXF_R16_FLOAT) ? EPixelFormat.PXF_D16_UNORM : EPixelFormat.PXF_D32_FLOAT;
            DepthPinOut.Attachement.Width = desc.Width;
            DepthPinOut.Attachement.Height = desc.Height;
            PageDepthTexture = rc.CreateTexture(in desc);
            PageDepthTexture.SetDebugName("AdvShadowDepth");

            FSrvDesc srvDesc = new FSrvDesc();
            srvDesc.SetTexture2D();
            srvDesc.Format = desc.Format;
            srvDesc.Texture2D.MipLevels = desc.MipLevels;
            srvDesc.Texture2D.MostDetailedMip = 0;
            PageDepthTextureSRV = rc.CreateSRV(PageDepthTexture, in srvDesc);

            var dsDesc = new FDsvDesc();
            dsDesc.SetDefault();
            dsDesc.Type = NxRHI.EDsvType.DSV_Texture2D;
            dsDesc.Format = desc.Format;
            dsDesc.Width = desc.Width;
            dsDesc.Height = desc.Height;
            dsDesc.MipLevel = 0;
            PageDepthTextureDSV = rc.CreateDSV(PageDepthTexture, in dsDesc);

            BuildGBuffer(policy, desc.Format);

            var dpRastDesc = new NxRHI.FGpuPipelineDesc();
            dpRastDesc.SetDefault();
            dpRastDesc.m_Rasterizer.m_DepthBias = 1;
            dpRastDesc.m_Rasterizer.m_SlopeScaledDepthBias = 2.0f;
            DepthRaster = TtEngine.Instance.GfxDevice.PipelineManager.GetPipelineState(rc, in dpRastDesc);

            ESMScreenMesh = Graphics.Pipeline.Common.TtSceenSpaceNode.CreateScreenMesh(null);
            ESMScreenMesh.Tag = mEsmShading;

            // Phase 1: Initialize Virtual Shadow Map Page Pool
            // The pool will be fully connected after mShadowQTree is discovered in Tick.
            // We defer actual pool creation to Tick because MaxPageCount depends on QTree config.

            if (this.Enable)
                this.Enable = true;
        }
        /// <summary>
        /// Creates and initializes the PagePool once the QTree is available.
        /// Called lazily from Tick when mShadowQTree is first discovered.
        /// </summary>
        private void InitializePagePool()
        {
            if (PagePool != null || mShadowQTree == null)
                return;

            var config = new FVSMPoolConfig
            {
                PageResolution = this.PageResolution,
                PoolDimPages = this.PoolDimPages,
            };

            // Compute clipmap page count to reserve virtual index space for both QTree + Clipmap
            int qtreeVirtualPages = mShadowQTree.QTreeBuilder.Nodes.Length;
            var clipConfig = FVSMClipmapConfig.CreateDefault();
            clipConfig.LevelCount = this.ClipmapLevelCount;
            clipConfig.BaseHalfExtent = this.ClipmapBaseHalfExtent;
            clipConfig.PagesPerDim = this.ClipmapPagesPerDim;
            clipConfig.PageResolution = this.PageResolution;
            int clipmapVirtualPages = clipConfig.TotalVirtualPages;

            int maxVirtualPages = qtreeVirtualPages + clipmapVirtualPages;
            PagePool = new TtVSMPagePool();
            PagePool.Initialize(maxVirtualPages, in config);

            // Phase 4: Initialize Clipmap for directional light
            InitializeClipmap(qtreeVirtualPages);
        }

        /// <summary>
        /// Initialize the directional light clipmap. Uses the shared PagePool.
        /// </summary>
        /// <param name="virtualPageOffset">Offset to avoid overlap with QTree virtual page indices.</param>
        private void InitializeClipmap(int virtualPageOffset)
        {
            if (Clipmap != null || PagePool == null)
                return;

            var clipConfig = new FVSMClipmapConfig
            {
                LevelCount = this.ClipmapLevelCount,
                BaseHalfExtent = this.ClipmapBaseHalfExtent,
                PagesPerDim = this.ClipmapPagesPerDim,
                PageResolution = this.PageResolution,
            };

            Clipmap = new TtVSMClipmap();
            Clipmap.Initialize(in clipConfig, PagePool);
            Clipmap.VirtualPageOffset = virtualPageOffset;

            // Create per-page data buffer for shader (VP matrix + near/far per clipmap page)
            int totalClipmapPages = clipConfig.TotalVirtualPages;
            ClipmapPageDataBuffer = new TtCpu2GpuBuffer<FVSMClipmapPageData>();
            ClipmapPageDataBuffer.Initialize(NxRHI.EBufferType.BFT_SRV);
            ClipmapPageDataBuffer.SetSize(totalClipmapPages);
        }

        /// <summary>
        /// Destroy and recreate the Clipmap, PagePool, and related GPU buffers
        /// so that runtime changes to ClipmapLevelCount / BaseHalfExtent / PagesPerDim take effect.
        /// Safe to call before first initialization (early-out if QTree not yet available).
        /// </summary>
        private void RebuildClipmap()
        {
            // Nothing to rebuild if the QTree hasn't been created yet;
            // InitializePagePool will pick up the latest property values when it runs.
            if (mShadowQTree == null)
                return;

            // Tear down Clipmap + its per-page data buffer
            ClipmapPageDataBuffer?.Dispose();
            ClipmapPageDataBuffer = null;
            Clipmap?.Dispose();
            Clipmap = null;

            // Tear down PagePool (virtual page count depends on clipmap config)
            PagePool?.Dispose();
            PagePool = null;

            // Force the CBV to be recreated on next draw so it picks up new pool/clipmap params
            mDirLightingCBV = null;

            // Re-initialize both (InitializePagePool calls InitializeClipmap internally)
            InitializePagePool();
        }

        /// <summary>
        /// Create the DepthTextureArray and associated views. Called once after PagePool init.
        /// </summary>
        private void InitializeDepthTextureArray()
        {
            if (DepthTextureArray != null || mShadowQTree == null)
                return;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            FTextureDesc desc = new FTextureDesc();
            desc.SetDefault();
            desc.BindFlags = EBufferType.BFT_SRV | EBufferType.BFT_RTV;
            desc.Width = (uint)PageResolution;
            desc.Height = (uint)PageResolution;
            desc.MipLevels = 1;
            desc.ArraySize = (uint)mShadowQTree.MaxPageCount;
            desc.Format = DepthPinOut.Attachement.Format;
            DepthPinOut.Attachement.Width = desc.Width;
            DepthPinOut.Attachement.Height = desc.Height;
            DepthTextureArray = rc.CreateTexture(in desc);
            DepthTextureArray.SetDebugName("AdvShadowDepthArray");

            FSrvDesc srvDesc = new FSrvDesc();
            srvDesc.SetTexture2DArray();
            srvDesc.Format = desc.Format;
            srvDesc.Texture2DArray.MipLevels = desc.MipLevels;
            srvDesc.Texture2DArray.ArraySize = desc.ArraySize;
            srvDesc.Texture2DArray.FirstArraySlice = 0;
            srvDesc.Texture2DArray.MostDetailedMip = 0;
            DepthTextureArraySRV = rc.CreateSRV(DepthTextureArray, in srvDesc);

            if (mDebuggerSRViews != null)
            {
                foreach (var view in mDebuggerSRViews)
                    view.Dispose();
            }
            mDebuggerSRViews = new TtSrView[mShadowQTree.MaxPageCount];
            for (int i = 0; i < mDebuggerSRViews.Length; i++)
            {
                var srvDesc1 = new FSrvDesc();
                srvDesc1.SetTexture2DArray();
                srvDesc1.Format = desc.Format;
                srvDesc1.Texture2DArray.MostDetailedMip = 0;
                srvDesc1.Texture2DArray.MipLevels = 1;
                srvDesc1.Texture2DArray.FirstArraySlice = (uint)i;
                srvDesc1.Texture2DArray.ArraySize = 1;
                mDebuggerSRViews[i] = rc.CreateSRV(DepthTextureArray, in srvDesc1);
            }

            if (mRtViews != null)
            {
                foreach (var view in mRtViews)
                    view.Dispose();
            }
            mRtViews = new TtRenderTargetView[mShadowQTree.MaxPageCount];
            for (int i = 0; i < mRtViews.Length; i++)
            {
                var rtvDesc = new FRtvDesc();
                rtvDesc.SetTexture2D();
                rtvDesc.Type = NxRHI.ERtvType.RTV_Texture2DArray;
                rtvDesc.Format = desc.Format;
                rtvDesc.Width = desc.Width;
                rtvDesc.Height = desc.Height;
                rtvDesc.Texture2DArray.MipSlice = 0;
                rtvDesc.Texture2DArray.FirstArraySlice = (uint)i;
                rtvDesc.Texture2DArray.ArraySize = 1;
                mRtViews[i] = rc.CreateRTV(DepthTextureArray, in rtvDesc);
            }
        }

        public unsafe void BuildGBuffer(TtRenderPolicy policy, EPixelFormat dsFormat)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            {
                var PassDesc = new NxRHI.FRenderPassDesc();
                PassDesc.NumOfMRT = 0;

                PassDesc.m_AttachmentDepthStencil.Format = dsFormat;
                PassDesc.m_AttachmentDepthStencil.Samples = 1;
                PassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                NxRHI.TtRenderPass RenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

                mGBuffer = new TtGraphicsBuffers();
                mGBuffer.Initialize(policy, RenderPass);
                mGBuffer.TargetViewIdentifier = new TtGraphicsBuffers.TtTargetViewIdentifier();
                mGBuffer.SetSize(PageResolution, PageResolution);
            }
            {
                var PassDesc = new NxRHI.FRenderPassDesc();
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].Format = (dsFormat == EPixelFormat.PXF_D16_UNORM) ? EPixelFormat.PXF_R16_FLOAT : EPixelFormat.PXF_R32_FLOAT;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;

                var RenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);
                mDrawScreenGBuffers.Initialize(policy, RenderPass);
                mDrawScreenGBuffers.TargetViewIdentifier = new TtGraphicsBuffers.TtTargetViewIdentifier();
                mDrawScreenGBuffers.SetSize(PageResolution, PageResolution);
            }
        }
        // Independent shadow caster list for Clipmap (rebuilt each frame)
        private List<GamePlay.Scene.TtNode> mShadowCasters = new List<GamePlay.Scene.TtNode>();
        public GamePlay.TtWorld.TtVisParameter mVisParameter = new GamePlay.TtWorld.TtVisParameter();
        public override unsafe void Tick(GamePlay.TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            // Lazy-init QTree on first Tick
            if (mShadowQTree == null)
            {
                mShadowQTree = new TtQTree();
                var aabb = new DBoundingBox2D(BoxCenter, BoxExtent);
                mShadowQTree.Initialize(MaxDeepLevel, aabb, ShadowMapPage);
                mShadowQTree.MaxShadowDistance = MaxShadowDistance;
            }

            // Collect shadow casters every frame (handles spawn/destroy)
            mShadowCasters.Clear();
            world.Root.IterateNodes(static (node, arg) =>
            {
                var self = arg as TtAdvanceShadowMapNode;
                if (node.IsCastShadow)
                {
                    self.mShadowCasters.Add(node);
                }
                return true;
            }, this);

            if (EnableLocalLightShadow)
            {
                // Update light direction from scene sun
                mShadowQTree.UpdateLightDirection(world.DirectionLight.Direction);
                foreach (var i in mShadowCasters)
                {
                    mShadowQTree.PushShadowNode(i, true);
                }
            
                // Update QTree (LOD selection, dirty page collection)
                var cullingNode = policy.FindFirstNode<Graphics.Pipeline.TtCpuCullingNode>();
                if (cullingNode != null && cullingNode.VisParameter.CullCamera != null)
                {
                    mShadowQTree.UpdateQTree(world, cullingNode.VisParameter.CullCamera, MaxDirtyPagePerFrame);
                }
            }

            // Initialize PagePool + DepthTextureArray on first valid QTree
            InitializePagePool();
            InitializeDepthTextureArray();

            var attachment = ImportAttachment(DepthPinOut, DepthAttachment);
            attachment.GpuResource = DepthTextureArray;

            // PagePool frame begin: evict stale pages and prepare for new allocations
            if (PagePool != null)
            {
                PagePool.BeginFrame();
                PagePool.EvictStalePages(PageMaxAge);
            }

            {
                if (EnableDirLightShadow && Clipmap != null)
                {
                    // Force all pages dirty every frame to guarantee VP matrices in
                    // ClipmapPageBuffer always match the current ClipmapCenter uploaded to shader.
                    // This prevents stale VP from cached pages causing shadow offset/jitter.
                    // Performance note: 128 pages × simple ortho depth is acceptable for now;
                    // future optimization: scrolling clipmap (only invalidate border pages on move).
                    //test!!!!
                    Clipmap.MarkAllPagesDirty();

                    if (policy.DefaultCamera != null)
                    {
                        var camPos = policy.DefaultCamera.GetPosition();
                        Clipmap.Update(this, in camPos, world.DirectionLight.Direction);
                    }
                }

                policy.QueueCmd((TtRCmdQueue queue, ref FRCmdInfo info) =>
                {
                    TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.BeginEvent("AdvShadowDrawShadowMap");
                }, "BeginAdvShadowDrawShadowMap");

                // ---- Directional light path: Clipmap dirty pages ----
                if (EnableDirLightShadow)
                {
                    if (Clipmap != null && Clipmap.DirtyPages.Count > 0)
                    {
                        DrawClipmapDirtyPages(world, policy);
                    }
                }

                // ---- Local lights path: QTree (point/spot/area lights) ----
                if (EnableLocalLightShadow)
                {
                    foreach (var i in mShadowQTree.UpdateShadowMapNodes)
                    {
                        // Allocate page in the pool for this node (ensures virtual→physical mapping exists)
                        if (PagePool != null && i.NodeIndex >= 0)
                        {
                            PagePool.AllocatePage(i.NodeIndex);

                            // Phase 3: Skip rendering for purely static-cached pages.
                            if (PagePool.IsStaticCached(i.NodeIndex) && !i.Leaf.IsDirty)
                                continue;
                        }
                        DrawShadowObjects(world, policy, i, i.ShadowObjects);
                    }
                }

                policy.QueueCmd((TtRCmdQueue queue, ref FRCmdInfo info) =>
                {
                    TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.EndEvent("AdvShadowDrawShadowMap");
                }, "EndAdvShadowDrawShadowMap");
            }

            // After rendering, mark pages as cached and flush page table to GPU
            if (PagePool != null)
            {
                // Mark QTree pages cached
                if (EnableLocalLightShadow)
                {
                    foreach (var i in mShadowQTree.UpdateShadowMapNodes)
                    {
                        if (i.NodeIndex >= 0)
                            PagePool.MarkPageCached(i.NodeIndex);
                    }
                }

                // Mark Clipmap pages cached
                Clipmap?.MarkRenderedPagesCached();

                PagePool.ClearRenderFlags();
            }

            mVisParameter.ClearVisibles();
        }

        /// <summary>
        /// Invalidate clipmap pages that contain dynamic shadow casters.
        /// Scans all QTree nodes with dynamic objects and invalidates overlapping clipmap pages.
        /// </summary>

        // Camera pool for clipmap page rendering (each dirty page needs its own CBuffer)
        
        private TtCamera GetClipmapPageCamera(int index)
        {
            return this.Clipmap.mClipmapPageCameras[index];
        }

        /// <summary>
        /// Build 4 cull planes for a clipmap page, identical to QTree's BuildCullPlanes.
        /// Each plane is formed by extruding a page edge along the light direction.
        /// A caster fully outside any plane cannot cast shadow into this page.
        /// </summary>
        private static unsafe void BuildPageCullPlanes(in Vector3 lightDir, float pageMinX, float pageMaxX, float pageMinZ, float pageMaxZ, DPlane[] pageCullPlanes)
        {
            // Planes 0,2: edges along Z axis → normal = cross(UnitZ, lightDir)
            var norm = Vector3.Cross(in Vector3.UnitZ, in lightDir);
            norm.Normalize();
            var normD = norm.AsDVector();
            pageCullPlanes[0] = new DPlane(new DVector3(pageMinX, 0, pageMinZ), normD);
            pageCullPlanes[2] = new DPlane(new DVector3(pageMaxX, 0, pageMaxZ), normD);

            // Planes 1,3: edges along X axis → normal = cross(UnitX, lightDir)
            norm = Vector3.Cross(in Vector3.UnitX, in lightDir);
            norm.Normalize();
            normD = norm.AsDVector();
            pageCullPlanes[1] = new DPlane(new DVector3(pageMaxX, 0, pageMaxZ), normD);
            pageCullPlanes[3] = new DPlane(new DVector3(pageMinX, 0, pageMinZ), normD);

            // Flip normals so page center is on the inside (negative half-space)
            double centerX = (pageMinX + pageMaxX) * 0.5;
            double centerZ = (pageMinZ + pageMaxZ) * 0.5;
            var center3D = new DVector3(centerX, 0, centerZ);
            for (int i = 0; i < 4; i++)
            {
                if (DPlane.DotCoordinate(pageCullPlanes[i], center3D) > 0)
                {
                    pageCullPlanes[i].Normal = -pageCullPlanes[i].Normal;
                    pageCullPlanes[i].D = -pageCullPlanes[i].D;
                }
            }
        }

        private unsafe void DrawClipmapDirtyPages(GamePlay.TtWorld world, TtRenderPolicy policy)
        {
            mVisParameter.CullType = GamePlay.TtWorld.TtVisParameter.EVisCull.Shadow;
            mVisParameter.World = world;
            mVisParameter.IsGatherVisibleNodes = false;

            // Clear all dirty page entries so pages with no casters won't use stale VP matrices.
            // Shader checks pageData.ZFar <= 0 and returns 1.0 (lit) for invalid pages.
            FVSMClipmapPageData pageData;
            foreach (var dp in Clipmap.DirtyPages)
            {
                int idx = Clipmap.GetLocalPageIndex(in dp);
                pageData.mViewProj = Matrix.Identity;
                pageData.mZNear = 0;
                pageData.mZFar = 0;
                pageData.mPad0 = 0;
                pageData.mPad1 = 0;
                ClipmapPageDataBuffer.UpdateData(idx, in pageData);
            }

            foreach (var dirtyPage in Clipmap.DirtyPages)
            {
                // 1) Compute page bounds in light-view XY space for precise culling
                Clipmap.GetPageLightViewBounds(in dirtyPage, out float pageLVMinX, out float pageLVMaxX, out float pageLVMinY, out float pageLVMaxY);

                // 2) Gather casters: test caster AABB overlap in light-view XY space
                mVisParameter.ClearVisibles();
                GatherShadowCastersForPageLV(out var mergedAABB, pageLVMinX, pageLVMaxX, pageLVMinY, pageLVMaxY);

                if (mVisParameter.VisibleMeshes.Count == 0)
                    continue;

                int localIndex = Clipmap.GetLocalPageIndex(in dirtyPage);
                ref var pageTableEntry = ref Clipmap.GetPageTableEntry(dirtyPage.VirtualPageIndex);
                pageTableEntry.MarkDynamicDirty();

                // 3) Build page camera with tight depth range from caster AABB
                var pageCamera = GetClipmapPageCamera(localIndex);
                Clipmap.BuildPageCamera(in dirtyPage, pageCamera, world, in mergedAABB, out float tightNear, out float tightFar);
                mVisParameter.CullCamera = pageCamera;

                // 4) Fill per-page data for shader
                // Per CodingGuidelines §1.7: StructuredBuffer upload requires manual Transpose.
                // Engine matrices are row-major C#; HLSL float4x4 in structured buffer is column-major.

                pageData.mViewProj = Matrix.Transpose(pageCamera.GetViewProjection());
                pageData.mZNear = tightNear;
                pageData.mZFar = tightFar;
                pageData.mPad0 = 0;
                pageData.mPad1 = 0;
                ClipmapPageDataBuffer.UpdateData(localIndex, in pageData);

                // 5) Render depth into DepthTextureArray at physical page slot
                int pageIndex = dirtyPage.VirtualPageIndex;
                int physicalIndex = GetPhysicalPageIndex(pageIndex);
                if (physicalIndex < 0 || physicalIndex >= (mRtViews?.Length ?? 0))
                    continue;

                var cmdlist = TtCommandList.GetCmdList();
                using (new NxRHI.TtCmdListScope(cmdlist, "ClipmapPage"))
                {
                    mGBuffer.SetDepthStencil(PageDepthTextureDSV);
                    mGBuffer.FlushModify();
                    DrawClipmapDepth(cmdlist, world, policy, pageCamera);

                    mDrawScreenGBuffers.SetRenderTarget(0, mRtViews[physicalIndex]);
                    mDrawScreenGBuffers.FlushModify();
                    DrawClipmapESM(cmdlist, world, policy, pageCamera);
                }
                policy.CommitCommandList(cmdlist, "ClipmapPage");
            }

            // Upload per-page data (VP + near/far) to GPU immediately after all pages are filled
            ClipmapPageDataBuffer.Flush2GPU(null);
        }

        /// <summary>
        /// Gather shadow casters by testing overlap in light-view XY space.
        /// This is precise for orthographic projection: a caster can only appear in a page
        /// if its AABB, projected onto the light-view XY plane, overlaps the page's XY range.
        /// </summary>
        private unsafe void GatherShadowCastersForPageLV(out DBoundingBox mergedAABB,
            float pageLVMinX, float pageLVMaxX, float pageLVMinY, float pageLVMaxY)
        {
            mergedAABB = new DBoundingBox();
            mergedAABB.InitEmptyBox();

            ref var wToLV = ref Clipmap.WorldToLightViewRotation;

            for (int i = 0; i < mShadowCasters.Count; i++)
            {
                var caster = mShadowCasters[i];
                ref var aabb = ref caster.BoundVolume.AbsAABB;

                // Project caster AABB to light-view space and compute LV XY bounds
                float casterLVMinX = float.MaxValue, casterLVMaxX = float.MinValue;
                float casterLVMinY = float.MaxValue, casterLVMaxY = float.MinValue;
                for (int c = 0; c < 8; c++)
                {
                    var corner = aabb.GetCorner(c);
                    var cornerF = new Vector3((float)corner.X, (float)corner.Y, (float)corner.Z);
                    // LV_X = dot(corner, column0), LV_Y = dot(corner, column1)
                    float lvX = cornerF.X * wToLV.M11 + cornerF.Y * wToLV.M21 + cornerF.Z * wToLV.M31;
                    float lvY = cornerF.X * wToLV.M12 + cornerF.Y * wToLV.M22 + cornerF.Z * wToLV.M32;

                    if (lvX < casterLVMinX) casterLVMinX = lvX;
                    if (lvX > casterLVMaxX) casterLVMaxX = lvX;
                    if (lvY < casterLVMinY) casterLVMinY = lvY;
                    if (lvY > casterLVMaxY) casterLVMaxY = lvY;
                }

                // AABB overlap test in light-view XY
                if (casterLVMaxX < pageLVMinX || casterLVMinX > pageLVMaxX ||
                    casterLVMaxY < pageLVMinY || casterLVMinY > pageLVMaxY)
                    continue;

                caster.OnGatherVisibleMeshes(mVisParameter);
                mergedAABB.Merge(in aabb);
            }
        }

        /// <summary>
        /// Given a world-space position, find which clipmap page it belongs to and return its virtual page index.
        /// Searches from the finest (level 0) to coarsest level, returning the first hit.
        /// Returns -1 if the position is outside all clipmap levels or Clipmap is not initialized.
        /// </summary>
        /// <param name="worldPosition">World-space position to query.</param>
        /// <returns>Virtual page index in the page table, or -1 if not covered.</returns>
        public int GetVirtualPageIndexAtWorldPosition(in Vector3 worldPosition)
        {
            if (Clipmap == null)
                return -1;

            return Clipmap.GetVirtualPageIndexAtWorldPosition(in worldPosition);
        }

        /// <summary>
        /// Get the physical page linear index for a virtual page, used to index mRtViews.
        /// </summary>
        private int GetPhysicalPageIndex(int virtualPageIndex)
        {
            if (PagePool == null || !PagePool.IsPageMapped(virtualPageIndex))
                return -1;

            var viewport = PagePool.GetPageViewport(virtualPageIndex);
            int pageRes = PagePool.Config.PageResolution;
            int pageX = (int)(viewport.TopLeftX / pageRes);
            int pageY = (int)(viewport.TopLeftY / pageRes);
            return pageY * PagePool.Config.PoolDimPages + pageX;
        }

        /// <summary>
        /// Depth pass for a single clipmap page.
        /// </summary>
        private void DrawClipmapDepth(NxRHI.TtCommandList cmdlist, GamePlay.TtWorld world, TtRenderPolicy policy, TtCamera pageCamera)
        {
            mBasePassRecorder.ResetGpuDraws();
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
                        var drawcall = k.GetDrawCall(cmdlist.mCoreObject, mGBuffer, policy, this);
                        if (drawcall != null)
                        {
                            drawcall.BindCBV(drawcall.Effect.BindIndexer.cbPerViewport, mGBuffer.PerViewportCBuffer);
                            drawcall.BindCBV(drawcall.Effect.BindIndexer.cbPerCamera, pageCamera.PerCameraCBuffer);
                            mBasePassRecorder.PushGpuDraw(drawcall);
                        }
                    }
                }
            }

            FViewPort viewport = new FViewPort();
            viewport.MinDepth = mGBuffer.Viewport.MinDepth;
            viewport.MaxDepth = mGBuffer.Viewport.MaxDepth;
            viewport.TopLeftX = 0;
            viewport.TopLeftY = 0;
            viewport.Width = mGBuffer.Viewport.Width;
            viewport.Height = mGBuffer.Viewport.Height;

            cmdlist.SetViewport(in viewport);
            var scissor = new NxRHI.FScissorRect();
            scissor.MinX = 0;
            scissor.MinY = 0;
            scissor.MaxX = (int)viewport.Width;
            scissor.MaxY = (int)viewport.Height;
            cmdlist.SetScissor(in scissor);

            var passClear = new NxRHI.FRenderPassClears();
            passClear.SetDefault();
            passClear.ClearFlags = ERenderPassClearFlags.CLEAR_DEPTH;
            passClear.SetClearColor(0, new Color4f(1, 1, 0, 0));

            cmdlist.BeginPass(mGBuffer.FrameBuffers, in passClear, "ClipmapDepth");
            cmdlist.AppendDraws(mBasePassRecorder);
            cmdlist.FlushDraws();
            cmdlist.EndPass();
            mBasePassRecorder.ResetGpuDraws();
        }

        /// <summary>
        /// ESM pass for a single clipmap page. Reads scratch depth → outputs to DepthTextureArray slice.
        /// </summary>
        private void DrawClipmapESM(NxRHI.TtCommandList cmdlist, GamePlay.TtWorld world, TtRenderPolicy policy, TtCamera pageCamera)
        {
            cmdlist.SetViewport(in mDrawScreenGBuffers.Viewport);
            var scissor = new NxRHI.FScissorRect();
            scissor.MinX = 0;
            scissor.MinY = 0;
            scissor.MaxX = (int)mDrawScreenGBuffers.Viewport.Width;
            scissor.MaxY = (int)mDrawScreenGBuffers.Viewport.Height;
            cmdlist.SetScissor(in scissor);

            var passClears = new NxRHI.FRenderPassClears();
            passClears.SetDefault();
            passClears.ClearFlags = ERenderPassClearFlags.CLEAR_NONE;
            passClears.SetClearColor(0, new Color4f(0, 0, 0, 0));

            cmdlist.BeginPass(mDrawScreenGBuffers.FrameBuffers, in passClears, "ClipmapESM");
            if (ESMScreenMesh != null)
            {
                foreach (var i in ESMScreenMesh.SubMeshes)
                {
                    foreach (var j in i.Atoms)
                    {
                        var drawcall = j.GetDrawCall(cmdlist.mCoreObject, mDrawScreenGBuffers, policy, this);
                        if (drawcall == null)
                            continue;
                        drawcall.TagObject = this;
                        drawcall.BindCBV(drawcall.Effect.BindIndexer.cbPerViewport, mDrawScreenGBuffers.PerViewportCBuffer);
                        drawcall.BindCBV(drawcall.Effect.BindIndexer.cbPerCamera, pageCamera.PerCameraCBuffer);
                        cmdlist.PushGpuDraw(drawcall);
                    }
                }
            }
            cmdlist.FlushDraws();
            cmdlist.EndPass();
        }

        #region Local light shadow drawing (QTree)
        private void DrawShadowObjects(GamePlay.TtWorld world, TtRenderPolicy policy, TtQNode node, List<TtShadowObject> shadowObjects)
        {
            if (node.Leaf.IsDirty == false)
                return;
            node.Leaf.UpdateShadowMatrix(world);
            mVisParameter.ClearVisibles();
            mVisParameter.CullCamera = node.Leaf.ShadowCamera;
            foreach (var j in shadowObjects)
            {
                j.SceneNode.OnGatherVisibleMeshes(mVisParameter);
            }
            var cmdlist = TtCommandList.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmdlist, "AdvShadow"))
            {
                mGBuffer.SetDepthStencil(PageDepthTextureDSV);
                mGBuffer.FlushModify();
                DrawDepth(cmdlist, world, policy);
            
                mDrawScreenGBuffers.SetRenderTarget(0, mRtViews[node.PageIndex]);
                mDrawScreenGBuffers.FlushModify();
                DrawESM(node, cmdlist, world, policy);
                //DrawGaussion(cmdlist, world, policy);
            }
            policy.CommitCommandList(cmdlist, "AdvShadow");
        }
        NxRHI.TtCmdRecorder mBasePassRecorder = new NxRHI.TtCmdRecorder();
        private void DrawDepth(NxRHI.TtCommandList cmdlist, GamePlay.TtWorld world, TtRenderPolicy policy)
        {
            mBasePassRecorder.ResetGpuDraws();
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
                        var drawcall = k.GetDrawCall(cmdlist.mCoreObject, mGBuffer, policy, this);

                        if (drawcall != null)
                        {
                            drawcall.BindGBuffer(mVisParameter.CullCamera, mGBuffer);

                            mBasePassRecorder.PushGpuDraw(drawcall);
                        }
                    }
                }
            }

            FViewPort Viewport = new FViewPort();
            Viewport.MinDepth = mGBuffer.Viewport.MinDepth;
            Viewport.MaxDepth = mGBuffer.Viewport.MaxDepth;
            Viewport.TopLeftX = 0;
            Viewport.TopLeftY = 0;
            Viewport.Width = (mGBuffer.Viewport.Width);// GBuffers.Viewport.Width;
            Viewport.Height = mGBuffer.Viewport.Height;

            cmdlist.SetViewport(in Viewport);
            var scissor = new NxRHI.FScissorRect();
            scissor.MinX = 0;
            scissor.MinY = 0;
            scissor.MaxX = (int)Viewport.Width;
            scissor.MaxY = (int)Viewport.Height;
            cmdlist.SetScissor(in scissor);

            var passClear = new NxRHI.FRenderPassClears();
            {
                passClear.SetDefault();
                passClear.ClearFlags = ERenderPassClearFlags.CLEAR_DEPTH;
                passClear.SetClearColor(0, new Color4f(1, 1, 0, 0));
            }
            cmdlist.BeginPass(mGBuffer.FrameBuffers, in passClear, "AdvShadowDepth");
            cmdlist.AppendDraws(mBasePassRecorder);
            cmdlist.FlushDraws();
            cmdlist.EndPass();
            mBasePassRecorder.ResetGpuDraws();
        }
        private void DrawESM(TtQNode node, NxRHI.TtCommandList cmdlist, GamePlay.TtWorld world, TtRenderPolicy policy)
        {
            {
                cmdlist.SetViewport(in mDrawScreenGBuffers.Viewport);
                var scissor = new NxRHI.FScissorRect();
                scissor.MinX = 0;
                scissor.MinY = 0;
                scissor.MaxX = (int)mDrawScreenGBuffers.Viewport.Width;
                scissor.MaxY = (int)mDrawScreenGBuffers.Viewport.Height;
                cmdlist.SetScissor(in scissor);
                var passClears = new NxRHI.FRenderPassClears();
                passClears.SetDefault();
                passClears.ClearFlags = ERenderPassClearFlags.CLEAR_NONE;
                passClears.SetClearColor(0, new Color4f(0, 0, 0, 0));
                cmdlist.BeginPass(mDrawScreenGBuffers.FrameBuffers, in passClears, "ESM");
                if (ESMScreenMesh != null)
                {
                    foreach (var i in ESMScreenMesh.SubMeshes)
                    {
                        foreach (var j in i.Atoms)
                        {
                            var drawcall = j.GetDrawCall(cmdlist.mCoreObject, mDrawScreenGBuffers, policy, this);
                            if (drawcall == null)
                                continue;
                            drawcall.TagObject = this;
                            drawcall.BindCBV(drawcall.Effect.BindIndexer.cbPerViewport, mDrawScreenGBuffers.PerViewportCBuffer);
                            drawcall.BindCBV(drawcall.Effect.BindIndexer.cbPerCamera, node.Leaf.ShadowCamera.PerCameraCBuffer);

                            cmdlist.PushGpuDraw(drawcall);
                        }
                    }
                }
                cmdlist.FlushDraws();
                cmdlist.EndPass();
            }
        }
        #endregion

        public override void OnDrawCall(TtGraphicsShadingEnv shading, ICommandList cmd, TtGraphicDraw drawcall, TtRenderPolicy policy, TtRenderMesh.TtAtom atom)
        {
            if (shading == mEsmShading)
            {
                var index = drawcall.FindBinder("DepthBuffer");
                if (index.IsValidPointer)
                {
                    drawcall.BindSRV(index, PageDepthTextureSRV);
                }
                index = drawcall.FindBinder("Samp_DepthBuffer");
                if (index.IsValidPointer)
                {
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
                }
                index = drawcall.FindBinder("cbAdvanceShadow");
                if (index.IsValidPointer)
                {
                    drawcall.BindCBV(index, mDirLightingCBV);
                }
            }
        }
        public unsafe void OnDirLightingDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Graphics.Mesh.TtRenderMesh.TtAtom atom)
        {
            if (mShadowQTree == null)
                return;

            var index = drawcall.FindBinder("GShadowMapArray");
            if (index.IsValidPointer)
            {
                drawcall.BindSRV(index, DepthTextureArraySRV);
            }
            index = drawcall.FindBinder("Samp_GShadowMap");
            if (index.IsValidPointer)
            {
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);
            }
            index = drawcall.FindBinder("Samp_GShadowMapArray");
            if (index.IsValidPointer)
            {
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);
            }
            index = drawcall.FindBinder("QTreeNodeBuffer");
            if (index.IsValidPointer)
            {
                mShadowQTree.AdvShadowNodeDatas.Flush2GPU(cmd);
                drawcall.BindSRV(index, mShadowQTree.AdvShadowNodeDatas.Srv);
            }

            // Bind the PageTable buffer for virtual→physical page lookup in projection shader
            if (PagePool != null)
            {
                index = drawcall.FindBinder("VSMPageTable");
                if (index.IsValidPointer)
                {
                    PagePool.FlushPageTableToGpu(cmd);
                    drawcall.BindSRV(index, PagePool.PageTableBuffer.Srv);
                }
            }

            // Bind per-clipmap-page VP + near/far buffer (data already written in DrawClipmapDirtyPages)
            if (ClipmapPageDataBuffer != null)
            {
                index = drawcall.FindBinder("ClipmapPageBuffer");
                if (index.IsValidPointer)
                {
                    ClipmapPageDataBuffer.Flush2GPU(cmd);
                    drawcall.BindSRV(index, ClipmapPageDataBuffer.Srv);
                }
            }

            index = drawcall.FindBinder("cbAdvanceShadow");
            if (index.IsValidPointer)
            {
                if (mDirLightingCBV == null)
                {
                    mDirLightingCBV = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                    mDirLightingCBV.SetValue("BoxMin", mShadowQTree.Root.AABB.Minimum.AsSingleVector());
                    mDirLightingCBV.SetValue("BoxMax", mShadowQTree.Root.AABB.Maximum.AsSingleVector());
                    mDirLightingCBV.SetValue("NodeCount", mShadowQTree.QNodes.Length);
                    mDirLightingCBV.SetValue("PageCount", mShadowQTree.MaxPageCount);
                    mDirLightingCBV.SetValue("MaxShadowDistance", mShadowQTree.MaxShadowDistance);
                    mDirLightingCBV.SetValue("MaxDeepLevel", mShadowQTree.MaxDeepLevel);

                    var indexLayerData = index.FindField("LayerData");
                    for (int i = 0; i < mShadowQTree.QTreeBuilder.Layers.Length; i++)
                    {
                        var layer = mShadowQTree.QTreeBuilder.Layers[i];
                        FAdvShadowLayerData layerData;
                        layerData.mLayerStartAndSide.X = layer.LayerStartIndex;
                        layerData.mLayerStartAndSide.Y = layer.Side;
                        layerData.mLayerGridSize = layer.GridSize.AsSingleVector();

                        mDirLightingCBV.SetValue(indexLayerData, i, in layerData); 
                    }
                    mDirLightingCBV.MarkDirty();
                    mDirLightingCBV.FlushDirty();
                }
                
                mDirLightingCBV.SetValue("EsmConstant", mShadowQTree.EsmConstant);
                mDirLightingCBV.SetValue("MaxExp", mShadowQTree.MaxExp);
                mDirLightingCBV.SetValue("GaussSigma", mShadowQTree.GaussSigma);

                // Page pool parameters
                if (PagePool != null)
                {
                    mDirLightingCBV.SetValue("PoolDimPages", PagePool.Config.PoolDimPages);
                }
                // Update per-frame clipmap data (light-view space center + rotation matrix).
                // Use SNAPSHOT values taken at shadow-render time, not live Clipmap state,
                // because another viewport's TickLogic may have overwritten Clipmap since then.
                if (Clipmap != null)
                {
                    mDirLightingCBV.SetValue("ClipmapPageResolution", Clipmap.Config.PageResolution);
                    mDirLightingCBV.SetValue("ClipmapLevelCount", Clipmap.Config.LevelCount);
                    mDirLightingCBV.SetValue("ClipmapBaseHalfExtent", Clipmap.Config.BaseHalfExtent);
                    mDirLightingCBV.SetValue("ClipmapPagesPerDim", Clipmap.Config.PagesPerDim);
                    mDirLightingCBV.SetValue("ClipmapVirtualPageOffset", Clipmap.VirtualPageOffset);

                    mDirLightingCBV.SetValue("ClipmapCenter", Clipmap.SnappedLightViewCenter);

                    // Shader does dot(worldPos, RowN.xyz) to get light-view coords.
                    // So each row uploaded must be the axis vector (right/up/forward).
                    // In C# matrix (columns = axes): right=(M11,M21,M31), up=(M12,M22,M32), fwd=(M13,M23,M33)
                    ref var m = ref Clipmap.WorldToLightViewRotation;
                    mDirLightingCBV.SetValue("WorldToLightViewRow0", new Vector4(m.M11, m.M21, m.M31, 0)); // right axis
                    mDirLightingCBV.SetValue("WorldToLightViewRow1", new Vector4(m.M12, m.M22, m.M32, 0)); // up axis
                    mDirLightingCBV.SetValue("WorldToLightViewRow2", new Vector4(m.M13, m.M23, m.M33, 0)); // forward axis
                }

                mDirLightingCBV.SetValue("UseESM", mEnableESM ? 1 : 0);
                mDirLightingCBV.SetValue("PcfRadius", PcfRadius);
                mDirLightingCBV.SetValue("PcfDepthBias", PcfDepthBias);
                if (mDirLightingCBV.IsDirty)
                    mDirLightingCBV.FlushDirty(false);

                drawcall.BindCBV(index, mDirLightingCBV);
            }
        }
    }
}
