using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EngineNS.Graphics.Pipeline.Deferred.MultiViewID
{
    public class TtBasePassShading : Shader.TtGraphicsShadingEnv
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
        public TtBasePassShading()
        {
            CodeName = RName.GetRName("Shaders/ShadingEnv/Deferred/MultiViewID/BasePass.cginc", RName.ERNameType.Engine);
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
        public override string ToString()
        {
            return base.ToString() + $":USE_VS_ViewID=1";
        }
        protected override void EnvShadingDefines(in FPermutationId id, TtShaderDefinitions defines)
        {
            base.EnvShadingDefines(id, defines);
            defines.AddDefine("USE_VS_ViewID", "1");
            //defines.AddDefine("USE_PS_ViewID", "0"); 
        }
        public override void OnBuildDrawCall(TtRenderPolicy policy, TtGraphicDraw drawcall)
        {
            base.OnBuildDrawCall(policy, drawcall);
            drawcall.mCoreObject.ViewInstanceMask = (1 | (1 << 1));
        }
    }

    [Bricks.CodeBuilder.ContextMenu("BassPass", "Deferred\\MultiViewID\\BassPass", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtBasePassNode : Common.TtBasePassNode
    {
        public TtRenderGraphPin VisiblesPinIn = TtRenderGraphPin.CreateInput("Visibles");
        public TtRenderGraphPin GpuCullPinIn = TtRenderGraphPin.CreateInput("GpuCull");
        public TtRenderGraphPin Rt0PinOut = TtRenderGraphPin.CreateInputOutput("MRT0", true, EPixelFormat.PXF_R16G16B16A16_FLOAT);//rgb - metallicty
        public TtRenderGraphPin Rt1PinOut = TtRenderGraphPin.CreateInputOutput("MRT1", true, EPixelFormat.PXF_R16G16B16A16_FLOAT);//rgb - metallicty
        public TtRenderGraphPin DepthStencilPinOut = TtRenderGraphPin.CreateInputOutput("DepthStencil", true, EPixelFormat.PXF_D24_UNORM_S8_UINT);
        public TtBasePassShading mOpaqueShading;
        public NxRHI.TtRenderPass RenderPass;
        public TtLayerDrawBuffers LayerBasePass = new TtLayerDrawBuffers();

        public TtCpuCullingNode CpuCullNode = null;
        public TtGpuCullingNode GpuCullNode = null;
        public NxRHI.FViewPort[] Viewports = new FViewPort[2];
        [Category("Option")]
        [Rtti.Meta]
        public bool ClearMRT
        {
            get;
            set;
        } = true;
        public TtBasePassNode()
        {
            Name = "MulitiViewIDBasePass";
        }
        public override void InitNodePins()
        {
            AddInput(VisiblesPinIn, NxRHI.EBufferType.BFT_NONE);
            AddInput(GpuCullPinIn, NxRHI.EBufferType.BFT_NONE);
            AddInputOutput(Rt0PinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(Rt1PinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(DepthStencilPinOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);

            GpuCullPinIn.IsAllowInputNull = true;
            Rt0PinOut.IsAllowInputNull = true;
            Rt1PinOut.IsAllowInputNull = true;
            DepthStencilPinOut.IsAllowInputNull = true;
        }
        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            if (GBuffers != null)
            {
                GBuffers.SetSize(x, y);
            }

            Rt0PinOut.Attachement.Height = (uint)y;
            Rt0PinOut.Attachement.Width = (uint)x;

            Rt1PinOut.Attachement.Height = (uint)y;
            Rt1PinOut.Attachement.Width = (uint)x;
        }
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            //BasePass.Initialize(rc, debugName + ".BasePass");

            LayerBasePass.Initialize(rc, debugName + ".ForwordPass");

            CreateGBuffers(policy, Rt0PinOut.Attachement.Format);

            mOpaqueShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtBasePassShading>();

            var linker = VisiblesPinIn.FindInLinker();
            if (linker != null)
            {
                CpuCullNode = linker.OutPin.HostNode as TtCpuCullingNode;
            }
            System.Diagnostics.Debug.Assert(CpuCullNode != null);
            linker = GpuCullPinIn.FindInLinker();
            if (linker != null)
            {
                GpuCullNode = linker.OutPin.HostNode as TtGpuCullingNode;
            }
        }
        public virtual unsafe TtGraphicsBuffers CreateGBuffers(TtRenderPolicy policy, EPixelFormat format)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var PassDesc = new NxRHI.FRenderPassDesc();
            PassDesc.NumOfMRT = 2;
            PassDesc.AttachmentMRTs[0].Format = format;
            PassDesc.AttachmentMRTs[0].Samples = 1;
            PassDesc.AttachmentMRTs[0].LoadAction = ClearMRT ? NxRHI.EFrameBufferLoadAction.LoadActionClear : NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            
            PassDesc.AttachmentMRTs[1].Format = format;
            PassDesc.AttachmentMRTs[1].Samples = 1;
            PassDesc.AttachmentMRTs[1].LoadAction = ClearMRT ? NxRHI.EFrameBufferLoadAction.LoadActionClear : NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.AttachmentMRTs[1].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;

            PassDesc.m_AttachmentDepthStencil.Format = DepthStencilPinOut.Attachement.Format;
            PassDesc.m_AttachmentDepthStencil.Samples = 1;
            PassDesc.m_AttachmentDepthStencil.LoadAction = ClearMRT ? NxRHI.EFrameBufferLoadAction.LoadActionClear : NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;

            PassDesc.m_ViewInstanceDesc.ViewInstanceCount = 2;
            PassDesc.m_ViewInstanceDesc.pViewInstanceLocations[0].ViewportArrayIndex = 0;
            PassDesc.m_ViewInstanceDesc.pViewInstanceLocations[0].RenderTargetArrayIndex = 0;

            PassDesc.m_ViewInstanceDesc.pViewInstanceLocations[1].ViewportArrayIndex = 1;
            PassDesc.m_ViewInstanceDesc.pViewInstanceLocations[1].RenderTargetArrayIndex = 0;

            RenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

            GBuffers.Initialize(policy, RenderPass);
            GBuffers.SetRenderTarget(policy, 0, Rt0PinOut);
            GBuffers.SetRenderTarget(policy, 1, Rt1PinOut);
            GBuffers.SetDepthStencil(policy, DepthStencilPinOut);
            GBuffers.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;

            return GBuffers;
        }
        public override void Dispose()
        {
            GBuffers?.Dispose();
            GBuffers = null;

            base.Dispose();
        }
        public override Shader.TtGraphicsShadingEnv GetPassShading(Mesh.TtMesh.TtAtom atom)
        {
            return mOpaqueShading;
        }
        public override void BeforeTickLogic(TtRenderPolicy policy)
        {
            if (policy.DisableHDR)
            {
                if (Rt0PinOut.Attachement.Format != EPixelFormat.PXF_R8G8B8A8_UNORM)
                {
                    Rt0PinOut.Attachement.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                    this.CreateGBuffers(policy, EPixelFormat.PXF_R8G8B8A8_UNORM);
                }
            }
            else
            {
                if (Rt0PinOut.Attachement.Format != EPixelFormat.PXF_R16G16B16A16_FLOAT)
                {
                    Rt0PinOut.Attachement.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
                    this.CreateGBuffers(policy, EPixelFormat.PXF_R16G16B16A16_FLOAT);
                }
            }
        }
        [Category("Option")]
        [Rtti.Meta]
        public List<ERenderLayer> LayerFilters { get; set; } = new List<ERenderLayer> { ERenderLayer.RL_Opaque, ERenderLayer.RL_Translucent, ERenderLayer.RL_Sky };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsFilters(ERenderLayer layer)
        {
            foreach (var i in LayerFilters)
            {
                if (i == layer)
                    return true;
            }
            return false;
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtDeferredBasePassNode), nameof(TickLogic));
                return mScopeTick;
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopePushGpuDraw;
        private static Profiler.TimeScope ScopePushGpuDraw
        {
            get
            {
                if (mScopePushGpuDraw == null)
                    mScopePushGpuDraw = new Profiler.TimeScope(typeof(TtDeferredBasePassNode), "PushGpuDraw");
                return mScopePushGpuDraw;
            }
        } 
        public unsafe override void TickLogic(GamePlay.TtWorld world, TtRenderPolicy policy, bool bClear)
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
                        if (i == (int)ERenderLayer.RL_Opaque)
                            passClears[i].ClearFlags = ERenderPassClearFlags.CLEAR_DEPTH | ERenderPassClearFlags.CLEAR_STENCIL | ERenderPassClearFlags.CLEAR_RT0 | ERenderPassClearFlags.CLEAR_RT1;
                        else
                            passClears[i].ClearFlags = 0;
                    }

                    GBuffers.BuildFrameBuffers(policy);
                    Viewports[0].TopLeftX = 0;
                    Viewports[0].TopLeftY = 0;
                    Viewports[0].Width = GBuffers.Viewport.Width * 0.5f;
                    Viewports[0].Height = GBuffers.Viewport.Height * 0.5f;
                    Viewports[0].MinDepth = GBuffers.Viewport.MinDepth;
                    Viewports[0].MaxDepth = GBuffers.Viewport.MaxDepth;

                    Viewports[1].TopLeftX = GBuffers.Viewport.Width * 0.5f;
                    Viewports[1].TopLeftY = 0;
                    Viewports[1].Width = Viewports[0].Width;
                    Viewports[1].Height = Viewports[0].Height;
                    Viewports[1].MinDepth = GBuffers.Viewport.MinDepth;
                    Viewports[1].MaxDepth = GBuffers.Viewport.MaxDepth;

                    fixed (NxRHI.FViewPort* pVp = &Viewports[0])
                    {
                        LayerBasePass.BuildRenderPass(policy, 2, pVp, passClears, (int)ERenderLayer.RL_Num, GBuffers, GBuffers, "Forword:");
                    }
                }

                LayerBasePass.ExecuteCommands(policy);
            }
        }
        public override void TickSync(TtRenderPolicy policy)
        {
            if (mOpaqueShading == null)
                return;
            LayerBasePass.SwapBuffer();

            //foreach (var i in CpuCullNode.VisParameter.VisibleMeshes)
            //{
            //    var preMatrix = i.Mesh.PerMeshCBuffer.GetMatrix(TtEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.WorldMatrix);
            //    i.Mesh.PerMeshCBuffer.SetMatrix(TtEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.PreWorldMatrix, preMatrix);
            //}
        }
    }
}
