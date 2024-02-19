using EngineNS.Bricks.VXGI;
using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    public class UOpaqueShading : Shader.UGraphicsShadingEnv
    {
        public UPermutationItem DisableAO
        {
            get;
            set;
        }
        public UPermutationItem DisablePointLights
        {
            get;
            set;
        }
        public UPermutationItem DisableShadow
        {
            get;
            set;
        }
        public UOpaqueShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/forword/ForwordOpaque.cginc", RName.ERNameType.Engine);

            this.BeginPermutaion();
            DisableAO = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_AO", (int)Shader.EPermutation_Bool.BitWidth);
            DisablePointLights = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_POINTLIGHTS", (int)Shader.EPermutation_Bool.BitWidth);
            DisableShadow = this.PushPermutation<Shader.EPermutation_Bool>("DISABLE_SHADOW_ALL", (int)Shader.EPermutation_Bool.BitWidth);
            var editorMode = this.PushPermutation<Shader.EPermutation_Bool>("MODE_EDITOR", (int)Shader.EPermutation_Bool.BitWidth);

            DisableAO.SetValue((int)Shader.EPermutation_Bool.FalseValue);
            DisableShadow.SetValue((int)Shader.EPermutation_Bool.FalseValue);
            DisablePointLights.SetValue((int)Shader.EPermutation_Bool.FalseValue);
            editorMode.SetValue((int)Shader.EPermutation_Bool.TrueValue);

            UpdatePermutation();
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
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
                EPixelShaderInput.PST_WorldPos,
                EPixelShaderInput.PST_Normal,
                EPixelShaderInput.PST_UV,
                EPixelShaderInput.PST_Color,
                EPixelShaderInput.PST_Custom0,
                EPixelShaderInput.PST_Custom1,
                EPixelShaderInput.PST_Custom2,
            };
        }
    }
    public class UTranslucentShading : Shader.UGraphicsShadingEnv
    {
        public UTranslucentShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Forword/ForwordTranslucent.cginc", RName.ERNameType.Engine);
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
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
                EPixelShaderInput.PST_WorldPos,
                EPixelShaderInput.PST_UV,
            };
        }
    }
    public class UForwordNode : Common.UBasePassNode
    {
        public TtRenderGraphPin VisiblesPinIn = TtRenderGraphPin.CreateInput("Visibles");
        public TtRenderGraphPin ColorPinInOut = TtRenderGraphPin.CreateInputOutput("Color");
        public TtRenderGraphPin DepthPinInOut = TtRenderGraphPin.CreateInputOutput("Depth");
        public UForwordNode()
        {
            Name = "UForwordNode";
        }
        public override void InitNodePins()
        {
            AddInput(VisiblesPinIn, NxRHI.EBufferType.BFT_NONE);
            AddInputOutput(ColorPinInOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(DepthPinInOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
        }
        public UOpaqueShading mOpaqueShading;
        public UTranslucentShading mTranslucentShading;
        public TtLayerDrawBuffers LayerBasePass = new TtLayerDrawBuffers();
        public NxRHI.URenderPass RenderPass;
        public TtCpuCullingNode CpuCullNode = null;
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var dfPolicy = policy;// as UDeferredPolicy;
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            LayerBasePass.Initialize(rc, debugName + ".ForwordPass");

            CreateGBuffers(policy, ColorPinInOut.Attachement.Format);

            mOpaqueShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UOpaqueShading>();
            mTranslucentShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UTranslucentShading>();

            var linker = VisiblesPinIn.FindInLinker();
            if (linker != null)
            {
                CpuCullNode = linker.OutPin.GetNakedHostNode<TtCpuCullingNode>();
            }
        }
        public virtual unsafe UGraphicsBuffers CreateGBuffers(URenderPolicy policy, EPixelFormat format)
        {
            var PassDesc = new NxRHI.FRenderPassDesc();

            PassDesc.NumOfMRT = 1;
            PassDesc.AttachmentMRTs[0].Format = format;
            PassDesc.AttachmentMRTs[0].Samples = 1;
            PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.m_AttachmentDepthStencil.Format = DepthPinInOut.Attachement.Format;// dfPolicy.BasePassNode.GBuffers.DepthStencil.AttachBuffer.Srv.mCoreObject.GetFormat(); //dsFmt;
            PassDesc.m_AttachmentDepthStencil.Samples = 1;
            PassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            //PassDesc.mFBClearColorRT0 = new Color4f(1, 0, 0, 0);
            //PassDesc.mDepthClearValue = 1.0f;
            //PassDesc.mStencilClearValue = 0u;

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

            GBuffers.Initialize(policy, RenderPass);
            GBuffers.SetRenderTarget(policy, 0, ColorPinInOut);
            GBuffers.SetDepthStencil(policy, DepthPinInOut);
            GBuffers.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;

            return GBuffers;
        }
        public override void Dispose()
        {
            if (mOpaqueShading == null)
                return;
            GBuffers?.Dispose();
            GBuffers = null;

            base.Dispose();
        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            if (mOpaqueShading == null)
                return;
            if (GBuffers != null)
            {
                GBuffers.SetSize(x, y);
            }
        }
        public override Shader.UGraphicsShadingEnv GetPassShading(Mesh.TtMesh.TtAtom atom)
        {
            switch (atom.Material.RenderLayer)
            {
                case ERenderLayer.RL_Translucent:
                case ERenderLayer.RL_Sky:
                    return mTranslucentShading;
                default:
                    return mOpaqueShading;
            }
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            var buffer = this.FindAttachBuffer(ColorPinInOut);
            if (buffer != null)
            {
                if (ColorPinInOut.Attachement.Format != buffer.BufferDesc.Format)
                {
                    this.CreateGBuffers(policy, buffer.BufferDesc.Format);
                    ColorPinInOut.Attachement.Format = buffer.BufferDesc.Format;
                }
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UForwordNode), nameof(TickLogic));
        public unsafe override void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                if (mOpaqueShading == null)
                    return;

                GBuffers?.SetViewportCBuffer(world, policy);
                
                using (new TtLayerDrawBuffers.TtLayerDrawBuffersScope(LayerBasePass))
                {
                    var camera = policy.DefaultCamera;//CpuCullNode.VisParameter.CullCamera;
                    foreach (var i in CpuCullNode.VisParameter.VisibleMeshes)
                    {
                        if (i.DrawMode == FVisibleMesh.EDrawMode.Instance)
                            continue;
                        foreach (var j in i.Mesh.SubMeshes)
                        {
                            foreach (var k in j.Atoms)
                            {
                                if (k == null || k.Material == null)
                                    continue;

                                var layer = k.Material.RenderLayer;
                                if (layer == ERenderLayer.RL_Translucent || layer == ERenderLayer.RL_Sky)
                                {
                                    var cmdlist = LayerBasePass.GetCmdList(layer);
                                    var drawcall = k.GetDrawCall(cmdlist.mCoreObject, GBuffers, policy, this);
                                    if (drawcall != null)
                                    {
                                        drawcall.BindGBuffer(camera, GBuffers);
                                        //GGizmosBuffers.PerViewportCBuffer = GBuffers.PerViewportCBuffer;

                                        cmdlist.PushGpuDraw(drawcall);
                                    }
                                }
                            }
                        }
                    }

                    var passClears = stackalloc NxRHI.FRenderPassClears[(int)ERenderLayer.RL_Num];
                    for (int i = 0; i < (int)ERenderLayer.RL_Num; i++)
                    {
                        passClears[i].SetDefault();
                        passClears[i].SetClearColor(0, new Color4f(0, 0, 0, 0));
                        passClears[i].ClearFlags = 0;
                    }

                    GBuffers.BuildFrameBuffers(policy);
                    LayerBasePass.BuildRenderPass(policy, in GBuffers.Viewport, passClears, (int)ERenderLayer.RL_Num, GBuffers, GBuffers, "Forword:");
                }

                LayerBasePass.ExecuteCommands();
            }   
        }
        public override void TickSync(URenderPolicy policy)
        {
            if (mOpaqueShading == null)
                return;
            LayerBasePass.SwapBuffer();
            //GBuffers?.Camera?.mCoreObject.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, 1);
        }
        public override void FrameBuild(URenderPolicy policy)
        {
            base.FrameBuild(policy);
        }
    }

    public class TtGizmosNode : Common.UBasePassNode
    {
        public TtRenderGraphPin VisiblesPinIn = TtRenderGraphPin.CreateInput("Visibles");
        public TtRenderGraphPin ColorPinInOut = TtRenderGraphPin.CreateInputOutput("Color");
        public TtRenderGraphPin DepthPinInOut = TtRenderGraphPin.CreateInputOutput("Depth");
        public TtRenderGraphPin GizmosDepthPinOut = TtRenderGraphPin.CreateOutput("GizmosDepth", true, EPixelFormat.PXF_D24_UNORM_S8_UINT);

        public UOpaqueShading mOpaqueShading;
        public UTranslucentShading mTranslucentShading;
        public TtLayerDrawBuffers LayerBasePass = new TtLayerDrawBuffers();
        public NxRHI.URenderPass GizmosRenderPass;

        public NxRHI.URenderPass WithDepthRenderPass;
        public UGraphicsBuffers WithDepthGBuffers { get; protected set; } = new UGraphicsBuffers();
        public TtGizmosNode()
        {
            Name = "GizmosNode";
        }
        public override void InitNodePins()
        {
            AddInput(VisiblesPinIn, NxRHI.EBufferType.BFT_NONE);
            AddInputOutput(ColorPinInOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(DepthPinInOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);

            AddOutput(GizmosDepthPinOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
        }
        public TtCpuCullingNode CpuCullNode = null;
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            LayerBasePass.Initialize(rc, debugName + ".GizmosPass");

            CreateGBuffers(policy, ColorPinInOut.Attachement.Format);

            mOpaqueShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UOpaqueShading>();
            mTranslucentShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UTranslucentShading>();

            var linker = VisiblesPinIn.FindInLinker();
            if (linker != null)
            {
                CpuCullNode = linker.OutPin.GetNakedHostNode<TtCpuCullingNode>();
            }
        }
        public virtual unsafe UGraphicsBuffers CreateGBuffers(URenderPolicy policy, EPixelFormat format)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            {
                var PassDesc = new NxRHI.FRenderPassDesc();

                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].Format = format;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
                PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.Format = DepthPinInOut.Attachement.Format;// dfPolicy.BasePassNode.GBuffers.DepthStencil.AttachBuffer.Srv.mCoreObject.GetFormat(); //dsFmt;
                PassDesc.m_AttachmentDepthStencil.Samples = 1;
                PassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
                PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
                PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = new Color4f(1, 0, 0, 0);
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;

                WithDepthRenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

                WithDepthGBuffers.Initialize(policy, WithDepthRenderPass);
                WithDepthGBuffers.SetRenderTarget(policy, 0, ColorPinInOut);
                WithDepthGBuffers.SetDepthStencil(policy, DepthPinInOut);
                WithDepthGBuffers.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;
            }

            {
                var GizmosPassDesc = new NxRHI.FRenderPassDesc();
                GizmosPassDesc.NumOfMRT = 1;
                GizmosPassDesc.AttachmentMRTs[0].Format = format;
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
                GizmosRenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in GizmosPassDesc);

                GBuffers.Initialize(policy, GizmosRenderPass);
                GBuffers.SetRenderTarget(policy, 0, ColorPinInOut);
                GBuffers.SetDepthStencil(policy, GizmosDepthPinOut);
                GBuffers.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;
            }
            
            return GBuffers;
        }
        public override void Dispose()
        {
            if (mOpaqueShading == null)
                return;
            
            GBuffers?.Dispose();
            GBuffers = null;

            WithDepthGBuffers?.Dispose();
            WithDepthGBuffers = null;

            base.Dispose();
        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            if (mOpaqueShading == null)
                return;
            if (GBuffers != null)
            {
                GBuffers.SetSize(x, y);
            }
            if (WithDepthGBuffers != null)
            {
                WithDepthGBuffers.SetSize(x, y);
            }
        }
        public override Shader.UGraphicsShadingEnv GetPassShading(Mesh.TtMesh.TtAtom atom)
        {
            switch (atom.Material.RenderLayer)
            {
                case ERenderLayer.RL_PostTranslucent:
                case ERenderLayer.RL_TranslucentGizmos:
                    return mTranslucentShading;
                default:
                    return mOpaqueShading;
            }
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            var buffer = this.FindAttachBuffer(ColorPinInOut);
            if (buffer != null)
            {
                if (ColorPinInOut.Attachement.Format != buffer.BufferDesc.Format)
                {
                    this.CreateGBuffers(policy, buffer.BufferDesc.Format);
                    ColorPinInOut.Attachement.Format = buffer.BufferDesc.Format;
                }
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UForwordNode), nameof(TickLogic));
        public unsafe override void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                if (mOpaqueShading == null)
                    return;

                GBuffers?.SetViewportCBuffer(world, policy);

                using (new TtLayerDrawBuffers.TtLayerDrawBuffersScope(LayerBasePass))
                {
                    var camera = policy.DefaultCamera;//CpuCullNode.VisParameter.CullCamera;
                    foreach (var i in CpuCullNode.VisParameter.VisibleMeshes)
                    {
                        foreach (var j in i.Mesh.SubMeshes)
                        {
                            if (j == null)
                                continue;
                            foreach (var k in j.Atoms)
                            {
                                if (k == null)
                                    continue;

                                if (k.Material == null)
                                    continue;
                                var layer = k.Material.RenderLayer;
                                if (layer == ERenderLayer.RL_PostOpaque || layer == ERenderLayer.RL_PostTranslucent
                                    || layer == ERenderLayer.RL_TranslucentGizmos || layer == ERenderLayer.RL_Gizmos)
                                {
                                    var cmdlist = LayerBasePass.GetCmdList(layer);
                                    var drawcall = k.GetDrawCall(cmdlist.mCoreObject, GBuffers, policy, this);
                                    if (drawcall != null)
                                    {
                                        drawcall.BindGBuffer(camera, GBuffers);
                                        //GGizmosBuffers.PerViewportCBuffer = GBuffers.PerViewportCBuffer;

                                        cmdlist.PushGpuDraw(drawcall);
                                    }
                                }
                            }
                        }
                    }

                    var passClears = stackalloc NxRHI.FRenderPassClears[(int)ERenderLayer.RL_Num];
                    for (int i = 0; i < (int)ERenderLayer.RL_Num; i++)
                    {
                        passClears[i].SetDefault();
                        passClears[i].SetClearColor(0, new Color4f(0, 0, 0, 0));
                        passClears[i].ClearFlags = 0;
                    }
                    passClears[(int)ERenderLayer.RL_Gizmos].ClearFlags = NxRHI.ERenderPassClearFlags.CLEAR_DEPTH;

                    WithDepthGBuffers.BuildFrameBuffers(policy);
                    GBuffers.BuildFrameBuffers(policy);
                    LayerBasePass.BuildRenderPass(policy, in GBuffers.Viewport, passClears, (int)ERenderLayer.RL_Num, WithDepthGBuffers, GBuffers, "Gizmos:");
                }

                LayerBasePass.ExecuteCommands();
            }
        }
        public override void TickSync(URenderPolicy policy)
        {
            if (mOpaqueShading == null)
                return;
            LayerBasePass.SwapBuffer();
            //GBuffers?.Camera?.mCoreObject.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, 1);
        }
    }
}
