using EngineNS.Bricks.VXGI;
using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;
using System.Runtime.CompilerServices;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    public class USdfOpaqueShading : Shader.UGraphicsShadingEnv
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
        public USdfOpaqueShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/forword/ForwordOpaque.cginc", RName.ERNameType.Engine);

            this.BeginPermutaion();
            DisableAO = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_AO", (int)Shader.EPermutation_Bool.BitWidth);
            DisablePointLights = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_POINTLIGHTS", (int)Shader.EPermutation_Bool.BitWidth);
            DisableShadow = this.PushPermutation<Shader.EPermutation_Bool>("DISABLE_SHADOW_ALL", (int)Shader.EPermutation_Bool.BitWidth);
            var editorMode = this.PushPermutation<Shader.EPermutation_Bool>("MODE_EDITOR", (int)Shader.EPermutation_Bool.BitWidth);

            DisableAO.SetValue((int)Shader.EPermutation_Bool.TrueValue);
            DisableShadow.SetValue((int)Shader.EPermutation_Bool.TrueValue);
            DisablePointLights.SetValue((int)Shader.EPermutation_Bool.TrueValue);
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
    public class USdfTranslucentShading : Shader.UGraphicsShadingEnv
    {
        public USdfTranslucentShading()
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
    [Bricks.CodeBuilder.ContextMenu("SdfForword", "Deferred\\SdfForword", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class USdfForwordNode : Common.UBasePassNode
    {
        public TtRenderGraphPin VisiblesPinIn = TtRenderGraphPin.CreateInput("Visibles");
        public TtRenderGraphPin ColorPinInOut = TtRenderGraphPin.CreateInputOutput("Color");
        public TtRenderGraphPin DepthPinInOut = TtRenderGraphPin.CreateInputOutput("Depth");
        public TtRenderGraphPin VoxelPinOut = TtRenderGraphPin.CreateOutput("Voxel", true, EPixelFormat.PXF_R8G8B8A8_UNORM);

        public USdfForwordNode()
        {
            Name = "USdfForwordNode";
        }
        public override void InitNodePins()
        {
            AddInput(VisiblesPinIn, NxRHI.EBufferType.BFT_NONE);
            AddInputOutput(ColorPinInOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(DepthPinInOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
            AddOutput(VoxelPinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        }
        public USdfOpaqueShading mOpaqueShading;
        public USdfTranslucentShading mTranslucentShading;
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

            mOpaqueShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<USdfOpaqueShading>();
            mTranslucentShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<USdfTranslucentShading>();

            var linker = VisiblesPinIn.FindInLinker();
            if (linker != null)
            {
                CpuCullNode = linker.OutPin.GetNakedHostNode<TtCpuCullingNode>();
            }
        }
        public virtual unsafe TtGraphicsBuffers CreateGBuffers(URenderPolicy policy, EPixelFormat format)
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
                case ERenderLayer.RL_Opaque:
                    return mOpaqueShading;
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
        [Rtti.Meta]
        public List<ERenderLayer> LayerFilters { get; set; } = new List<ERenderLayer> { ERenderLayer.RL_Opaque, ERenderLayer.RL_Translucent, ERenderLayer.RL_Sky };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsFilters(ERenderLayer layer)
        {
            foreach(var i in LayerFilters)
            {
                if (i == layer)
                    return true;
            }
            return false;
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(USdfForwordNode), nameof(TickLogic));
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
                                if (IsFilters(layer))
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

                LayerBasePass.ExecuteCommands(policy);
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

}
