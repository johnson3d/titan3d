using EngineNS.Bricks.VXGI;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class TtBasePassShading : Shader.TtGraphicsShadingEnv
    {
        public TtBasePassShading()
        {
            this.BeginPermutaion();

            DisableAO = this.PushPermutation<EPermutation_Bool>("ENV_DISABLE_AO", (int)EPermutation_Bool.BitWidth);
            DisablePointLights = this.PushPermutation<EPermutation_Bool>("ENV_DISABLE_POINTLIGHTS", (int)EPermutation_Bool.BitWidth);
            DisableShadow = this.PushPermutation<EPermutation_Bool>("DISABLE_SHADOW_ALL", (int)EPermutation_Bool.BitWidth);
            var mode_editor = this.PushPermutation<EPermutation_Bool>("MODE_EDITOR", (int)EPermutation_Bool.BitWidth);

            //DisableAO.SetValue((int)EPermutation_Bool.FalseValue);
            DisableAO.SetValue(false);
            //DisablePointLights.SetValue((int)EPermutation_Bool.FalseValue);
            DisablePointLights.SetValue(false);
            //DisableShadow.SetValue((int)EPermutation_Bool.FalseValue);
            DisableShadow.SetValue(false);
            //mode_editor.SetValue((int)EPermutation_Bool.FalseValue);
            mode_editor.SetValue(false);

            this.UpdatePermutation().AddWaitTask();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal,
                NxRHI.EVertexStreamType.VST_Tangent,
                NxRHI.EVertexStreamType.VST_Color,
                NxRHI.EVertexStreamType.VST_LightMap,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public TtPermutationItem DisableAO
        {
            get;
            set;
        }
        public TtPermutationItem DisablePointLights
        {
            get;
            set;
        }
        public TtPermutationItem DisableShadow
        {
            get;
            set;
        }
        public override bool IsValidPermutation(TtMdfQueueBase mdfQueue, Shader.TtMaterial mtl)
        {
            return true;
        }
        public unsafe override void OnBuildDrawCall(TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall)
        {
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtRenderMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var Manager = policy as Mobile.TtMobileEditorFSPolicy;
            if (Manager != null)
            {
                var node = Manager.FindFirstNode<TtMobileForwordNodeBase>();
                if (node != null)
                {
                    var index = drawcall.FindBinder("gEnvMap");
                    if (index.IsValidPointer)
                    {
                        var attachBuffer = node.GetAttachBuffer(node.EnvMapPinIn);
                        drawcall.BindSRV(index, attachBuffer.Srv);
                    }
                    index = drawcall.FindBinder("Samp_gEnvMap");
                    if (index.IsValidPointer)
                        drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

                    index = drawcall.FindBinder("GVignette");
                    if (index.IsValidPointer)
                    {
                        var attachBuffer = node.GetAttachBuffer(node.VignettePinIn);
                        drawcall.BindSRV(index, attachBuffer.Srv);
                    }
                    index = drawcall.FindBinder("Samp_GVignette");
                    if (index.IsValidPointer)
                        drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

                    index = drawcall.FindBinder("GShadowMap");
                    if (index.IsValidPointer)
                    {
                        var attachBuffer = node.GetAttachBuffer(node.ShadowMapPinIn);
                        drawcall.BindSRV(index, attachBuffer.Srv);
                    }
                    index = drawcall.FindBinder("Samp_GShadowMap");
                    if (index.IsValidPointer)
                        drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

                    index = drawcall.FindBinder("cbPerGpuScene");
                    if (index.IsValidPointer)
                        drawcall.BindCBV(index, Manager.GetGpuSceneNode().PerGpuSceneCbv);

                    index = drawcall.FindBinder("TilingBuffer");
                    if (index.IsValidPointer)
                    {
                        var attachBuffer = node.GetAttachBuffer(node.TileScreenPinIn);
                        drawcall.BindSRV(index, attachBuffer.Srv);
                    }

                    index = drawcall.FindBinder("GpuScene_PointLights");
                    if (index.IsValidPointer)
                    {
                        var attachBuffer = node.GetAttachBuffer(node.PointLightsPinIn);
                        if (attachBuffer.Srv != null)
                            drawcall.BindSRV(index, attachBuffer.Srv);
                    }
                }
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("BassPass", "Mobile\\BasePass", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtBasePassOpaque : TtBasePassShading
    {
        public TtBasePassOpaque()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Mobile/MobileOpaque.cginc", RName.ERNameType.Engine);
        }
    }
    public class TtBasePassTranslucent : TtBasePassShading
    {
        public TtBasePassTranslucent()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Mobile/MobileTranslucent.cginc", RName.ERNameType.Engine);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Forword", "Mobile\\Forword", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtMobileForwordNodeBase : Common.TtBasePassNode
    {
        public TtRenderGraphPin VisiblesPinIn = TtRenderGraphPin.CreateInput("Visibles", NxRHI.EBufferType.BFT_NONE);
        public TtRenderGraphPin ShadowMapPinIn = TtRenderGraphPin.CreateInput("ShadowMap", NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);
        public TtRenderGraphPin EnvMapPinIn = TtRenderGraphPin.CreateInput("EnvMap", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin VignettePinIn = TtRenderGraphPin.CreateInput("Vignette", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin TileScreenPinIn = TtRenderGraphPin.CreateInput("TileScreen", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin PointLightsPinIn = TtRenderGraphPin.CreateInput("PointLights", NxRHI.EBufferType.BFT_SRV);

        public TtCpuCullingNode CpuCullNode = null;
        public override void InitNodePins()
        {
            AddInput(VisiblesPinIn);
            AddInput(ShadowMapPinIn);
            AddInput(EnvMapPinIn);
            AddInput(VignettePinIn);
            AddInput(TileScreenPinIn);
            AddInput(PointLightsPinIn);
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Opaque", "Mobile\\Opaque", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtMobileOpaqueNode : TtMobileForwordNodeBase
    {
        public TtRenderGraphPin ColorPinOut = TtRenderGraphPin.CreateOutput("Color", true, EPixelFormat.PXF_R16G16B16A16_FLOAT, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_RTV);
        public TtRenderGraphPin DepthPinOut = TtRenderGraphPin.CreateOutput("Depth", true, EPixelFormat.PXF_D24_UNORM_S8_UINT, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);
        public TtRenderGraphPin GizmosDepthPinOut = TtRenderGraphPin.CreateOutput("GizmosDepth", true, EPixelFormat.PXF_D16_UNORM, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);

        public TtGraphicsBuffers GGizmosBuffers { get; protected set; } = new TtGraphicsBuffers();
        public TtMobileOpaqueNode()
        {
            Name = "MobileOpaqueNode";
        }
        public override void InitNodePins()
        {
            base.InitNodePins();

            AddOutput(ColorPinOut);
            AddOutput(DepthPinOut);
            AddOutput(GizmosDepthPinOut);
        }
        public TtBasePassOpaque mOpaqueShading;
        public TtLayerDrawBuffers LayerBasePass = new TtLayerDrawBuffers();
        public NxRHI.TtRenderPass RenderPass;
        public NxRHI.TtRenderPass GizmosRenderPass;

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            LayerBasePass.Initialize(rc, debugName + ".OpaqueBassPass");

            var PassDesc = new NxRHI.FRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].IsSwapChain = 0;
                PassDesc.AttachmentMRTs[0].Format = ColorPinOut.Attachement.Format;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.Format = DepthPinOut.Attachement.Format;
                PassDesc.m_AttachmentDepthStencil.Samples = 1;
                PassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = new Color4f(1, 0, 0, 0);
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
            }
            RenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);
            var GizmosPassDesc = new NxRHI.FRenderPassDesc();
            unsafe
            {
                GizmosPassDesc.NumOfMRT = 1;
                GizmosPassDesc.AttachmentMRTs[0].IsSwapChain = 0;
                GizmosPassDesc.AttachmentMRTs[0].Format = ColorPinOut.Attachement.Format;
                GizmosPassDesc.AttachmentMRTs[0].Samples = 1;
                GizmosPassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
                GizmosPassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.Format = GizmosDepthPinOut.Attachement.Format;
                GizmosPassDesc.m_AttachmentDepthStencil.Samples = 1;
                GizmosPassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                //GizmosPassDesc.mFBClearColorRT0 = new Color4f(1, 0, 0, 0);
                //GizmosPassDesc.mDepthClearValue = 1.0f;
                //GizmosPassDesc.mStencilClearValue = 0u;
            }
            GizmosRenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in GizmosPassDesc);

            GBuffers.Initialize(policy, RenderPass);
            GBuffers.SetRenderTarget(policy, 0, ColorPinOut);
            GBuffers.SetDepthStencil(policy, DepthPinOut);
            GBuffers.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;

            GGizmosBuffers.Initialize(policy, GizmosRenderPass);
            GGizmosBuffers.SetRenderTarget(policy, 0, ColorPinOut);
            GBuffers.SetDepthStencil(policy, GizmosDepthPinOut);
            GGizmosBuffers.TargetViewIdentifier = GBuffers.TargetViewIdentifier;

            //mBasePassShading = shading as Pipeline.Mobile.UBasePassOpaque;
            mOpaqueShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtBasePassOpaque>();

            var linker = VisiblesPinIn.FindInLinker();
            if (linker != null)
            {
                CpuCullNode = linker.OutPin.HostNode as TtCpuCullingNode;
            }
        }
        public override void Dispose()
        {
            GBuffers?.Dispose();
            GBuffers = null;

            GGizmosBuffers?.Dispose();
            GGizmosBuffers = null;

            base.Dispose();
        }
        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            if (GBuffers != null)
            {
                GBuffers.SetSize(x, y);
            }
            if (GGizmosBuffers != null)
            {
                GGizmosBuffers.SetSize(x, y);
            }
        }
        public unsafe override void Tick(GamePlay.TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            var mobilePolicy = policy;
            GBuffers?.SetViewportCBuffer(world, policy);

            using (new TtLayerDrawBuffers.TtLayerDrawBuffersScope(LayerBasePass))
            {
                var passClears = stackalloc NxRHI.FRenderPassClears[(int)ERenderLayer.RL_Num];
                for (int i = 0; i < (int)ERenderLayer.RL_Num; i++)
                {
                    passClears[i].SetDefault();
                    passClears[i].SetClearColor(0, new Color4f(0, 0, 0, 0));
                    passClears[i].ClearFlags = 0;
                }

                GBuffers.BuildFrameBuffers(policy);
                var cmdlist = NxRHI.TtCommandList.GetCmdList();
                using (new NxRHI.TtCmdListScope(cmdlist, "Opaque"))
                {
                    var camera = policy.DefaultCamera;//CpuCullNode.VisParameter.CullCamera;
                    cmdlist.SetViewport(in GBuffers.Viewport);

                    foreach (var i in CpuCullNode.VisParameter.VisibleMeshes)
                    {
                        foreach (var j in i.Mesh.SubMeshes)
                        {
                            foreach (var k in j.Atoms)
                            {
                                var layer = k.Material.RenderLayer;
                                if (layer != ERenderLayer.RL_Opaque)
                                    continue;
                                var recorder = LayerBasePass.GetCmdRecorder(layer);
                                var drawcall = k.GetDrawCall(cmdlist.mCoreObject, GBuffers, policy, this);
                                if (drawcall != null)
                                {
                                    drawcall.BindGBuffer(camera, GBuffers);
                                    //GGizmosBuffers.PerViewportCBuffer = GBuffers.PerViewportCBuffer;

                                    recorder.PushGpuDraw(drawcall);
                                }
                            }
                        }
                    }
                    LayerBasePass.BuildRenderPass(cmdlist, policy, in GBuffers.Viewport, passClears, (int)ERenderLayer.RL_Num, GBuffers, GBuffers, "Mobile:");
                }

                policy.CommitCommandList(cmdlist, "Opaque");
            }

            //var cmdlist = LayerBasePass.PassBuffers[(int)ERenderLayer.RL_Opaque].DrawCmdList;

            //var passClears = new NxRHI.FRenderPassClears();
            //passClears.SetDefault();
            //passClears.SetClearColor(0, new Color4f(1, 0, 0, 0));
            //GBuffers.BuildFrameBuffers(policy);

            //LayerBasePass.BuildRenderPass(policy, in GBuffers.Viewport, )
            //cmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, ERenderLayer.RL_Opaque.ToString());
            //cmdlist.FlushDraws();
            //cmdlist.EndPass();
            //cmdlist.EndCommand();
        }
        public override void TickSync(TtRenderPolicy policy)
        {
            
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Translucent", "Mobile\\Translucent", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtMobileTranslucentNode : Common.TtBasePassNode
    {
        public TtRenderGraphPin VisiblesPinIn = TtRenderGraphPin.CreateInput("Visibles", NxRHI.EBufferType.BFT_NONE);
        public Graphics.Pipeline.TtRenderGraphPin AlbedoPinInOut = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Albedo", NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_RTV);
        public Graphics.Pipeline.TtRenderGraphPin DepthPinInOut = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Depth", NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);

        public Graphics.Pipeline.TtRenderGraphPin GizmosDepthPinOut = Graphics.Pipeline.TtRenderGraphPin.CreateOutput("GizmosDepth", true, EPixelFormat.PXF_D16_UNORM, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);

        public TtGraphicsBuffers GGizmosBuffers { get; protected set; } = new TtGraphicsBuffers();
        public TtMobileTranslucentNode()
        {
            Name = "UMobileTranslucentNode";
        }
        public override void InitNodePins()
        {
            base.InitNodePins();

            AddInputOutput(AlbedoPinInOut);
            AddInputOutput(DepthPinInOut);
            
            AddOutput(GizmosDepthPinOut);
        }
        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            
        }
        public TtBasePassTranslucent mTranslucentShading;
        public TtLayerDrawBuffers LayerBasePass = new TtLayerDrawBuffers();
        public NxRHI.TtRenderPass RenderPass;
        public NxRHI.TtRenderPass GizmosRenderPass;
        public TtCpuCullingNode CpuCullNode = null;
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            LayerBasePass.Initialize(rc, debugName + ".TranslucentBasePass");

            var PassDesc = new NxRHI.FRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].IsSwapChain = 0;
                PassDesc.AttachmentMRTs[0].Format = AlbedoPinInOut.Attachement.Format;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
                PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.Format = DepthPinInOut.Attachement.Format;
                PassDesc.m_AttachmentDepthStencil.Samples = 1;
                PassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
                PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
                PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = new Color4f(1, 0, 0, 0);
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
            }
            RenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

            var GizmosPassDesc = new NxRHI.FRenderPassDesc();
            unsafe
            {
                GizmosPassDesc.NumOfMRT = 1;
                GizmosPassDesc.AttachmentMRTs[0].IsSwapChain = 0;
                GizmosPassDesc.AttachmentMRTs[0].Format = AlbedoPinInOut.Attachement.Format;
                GizmosPassDesc.AttachmentMRTs[0].Samples = 1;
                GizmosPassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
                GizmosPassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.Format = GizmosDepthPinOut.Attachement.Format;
                GizmosPassDesc.m_AttachmentDepthStencil.Samples = 1;
                GizmosPassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                //GizmosPassDesc.mFBClearColorRT0 = new Color4f(1, 0, 0, 0);
                //GizmosPassDesc.mDepthClearValue = 1.0f;
                //GizmosPassDesc.mStencilClearValue = 0u;
            }
            GizmosRenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in GizmosPassDesc);

            GBuffers.Initialize(policy, RenderPass);
            GBuffers.SetRenderTarget(policy, 0, AlbedoPinInOut);
            GBuffers.SetDepthStencil(policy, DepthPinInOut);
            GBuffers.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;
            
            GGizmosBuffers.Initialize(policy, GizmosRenderPass);
            GGizmosBuffers.SetRenderTarget(policy, 0, AlbedoPinInOut);
            GGizmosBuffers.SetDepthStencil(policy, GizmosDepthPinOut);
            GGizmosBuffers.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;

            mTranslucentShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtBasePassTranslucent>();

            var linker = VisiblesPinIn.FindInLinker();
            if (linker != null)
            {
                CpuCullNode = linker.OutPin.HostNode as TtCpuCullingNode;
            }
        }
        public override void Dispose()
        {
            GBuffers?.Dispose();
            GBuffers = null;

            GGizmosBuffers?.Dispose();
            GGizmosBuffers = null;

            base.Dispose();
        }
        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            if (GBuffers != null)
            {
                GBuffers.SetSize(x, y);
            }
            if (GGizmosBuffers != null)
            {
                GGizmosBuffers.SetSize(x, y);
            }
            base.OnResize(policy, x, y);
        }
        public unsafe override void Tick(GamePlay.TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            var mobilePolicy = policy as TtMobileFSPolicy;
            GBuffers?.SetViewportCBuffer(world, policy);

            using (new TtLayerDrawBuffers.TtLayerDrawBuffersScope(LayerBasePass))
            {
                var cmdlist = NxRHI.TtCommandList.GetCmdList();
                using (new NxRHI.TtCmdListScope(cmdlist, "BassPass"))
                {
                    cmdlist.SetViewport(in GBuffers.Viewport);

                    var camera = policy.DefaultCamera;//CpuCullNode.VisParameter.CullCamera;
                    foreach (var i in CpuCullNode.VisParameter.VisibleMeshes)
                    {
                        foreach (var j in i.Mesh.SubMeshes)
                        {
                            foreach (var k in j.Atoms)
                            {
                                if (k.Material == null)
                                    continue;
                                var layer = k.Material.RenderLayer;
                                if (layer == ERenderLayer.RL_Opaque)
                                    continue;
                                var recorder = LayerBasePass.GetCmdRecorder(layer);
                                var drawcall = k.GetDrawCall(cmdlist.mCoreObject, GBuffers, policy, this);
                                if (drawcall != null)
                                {
                                    drawcall.BindGBuffer(camera, GBuffers);
                                    //GGizmosBuffers.PerViewportCBuffer = GBuffers.PerViewportCBuffer;

                                    recorder.PushGpuDraw(drawcall);
                                }
                            }
                        }
                    }
                    var passClears = new NxRHI.FRenderPassClears();
                    passClears.SetDefault();
                    passClears.SetClearColor(0, new Color4f(1, 0, 0, 0));
                    LayerBasePass.BuildTranslucentRenderPass(cmdlist, policy, in passClears, GBuffers, GGizmosBuffers);
                }
                policy.CommitCommandList(cmdlist, "BassPass");
            }
            //var passClears = stackalloc NxRHI.FRenderPassClears[(int)ERenderLayer.RL_Num];
            //for (int i = 0; i < (int)ERenderLayer.RL_Num; i++)
            //{
            //    passClears[i].SetDefault();
            //    passClears[i].SetClearColor(0, new Color4f(0, 0, 0, 0));
            //    passClears[i].ClearFlags = 0;
            //}
            //passClears[(int)ERenderLayer.RL_Background].ClearFlags = NxRHI.ERenderPassClearFlags.CLEAR_ALL;
            //passClears[(int)ERenderLayer.RL_Gizmos].ClearFlags = NxRHI.ERenderPassClearFlags.CLEAR_DEPTH;
            //LayerBasePass.BuildRenderPass(policy, in GBuffers.Viewport, passClears, (int)ERenderLayer.RL_Num, GBuffers, GGizmosBuffers, "Forward:");
        }
        public override void TickSync(TtRenderPolicy policy)
        {
            
        }
    }
}
