using EngineNS.Bricks.VXGI;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class UBasePassShading : Shader.UShadingEnv
    {
        public UBasePassShading()
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

            this.UpdatePermutation();
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
        public override bool IsValidPermutation(UMdfQueue mdfQueue, Shader.UMaterial mtl)
        {
            if (mtl.LightingMode != Shader.UMaterial.ELightingMode.Stand)
            {
                if(DisableAO.GetValue() == 1 || DisablePointLights.GetValue() == 1)
                    return false;
            }
            return true;
        }
        public unsafe override void OnBuildDrawCall(URenderPolicy policy, NxRHI.UGraphicDraw drawcall)
        {
        }
        public unsafe override void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            var Manager = policy as Mobile.UMobileEditorFSPolicy;
            if (Manager != null)
            {
                var node = Manager.FindFirstNode<UMobileForwordNodeBase>();
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
                        drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

                    index = drawcall.FindBinder("GVignette");
                    if (index.IsValidPointer)
                    {
                        var attachBuffer = node.GetAttachBuffer(node.VignettePinIn);
                        drawcall.BindSRV(index, attachBuffer.Srv);
                    }
                    index = drawcall.FindBinder("Samp_GVignette");
                    if (index.IsValidPointer)
                        drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

                    index = drawcall.FindBinder("GShadowMap");
                    if (index.IsValidPointer)
                    {
                        var attachBuffer = node.GetAttachBuffer(node.ShadowMapPinIn);
                        drawcall.BindSRV(index, attachBuffer.Srv);
                    }
                    index = drawcall.FindBinder("Samp_GShadowMap");
                    if (index.IsValidPointer)
                        drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

                    index = drawcall.FindBinder("cbPerGpuScene");
                    if (index.IsValidPointer)
                        drawcall.BindCBuffer(index, Manager.GetGpuSceneNode().PerGpuSceneCBuffer);

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
    public class UBasePassOpaque : UBasePassShading
    {
        public UBasePassOpaque()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Mobile/MobileOpaque.cginc", RName.ERNameType.Engine);
        }
    }
    public class UBasePassTranslucent : UBasePassShading
    {
        public UBasePassTranslucent()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Mobile/MobileTranslucent.cginc", RName.ERNameType.Engine);
        }
    }
    public class UMobileForwordNodeBase : Common.UBasePassNode
    {
        public Common.URenderGraphPin ShadowMapPinIn = Common.URenderGraphPin.CreateInput("ShadowMap");
        public Common.URenderGraphPin EnvMapPinIn = Common.URenderGraphPin.CreateInput("EnvMap");
        public Common.URenderGraphPin VignettePinIn = Common.URenderGraphPin.CreateInput("Vignette");        
        public Common.URenderGraphPin TileScreenPinIn = Common.URenderGraphPin.CreateInput("TileScreen");
        public Common.URenderGraphPin PointLightsPinIn = Common.URenderGraphPin.CreateInput("PointLights");

        public override void InitNodePins()
        {
            AddInput(ShadowMapPinIn, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);
            AddInput(EnvMapPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(VignettePinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(TileScreenPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(PointLightsPinIn, NxRHI.EBufferType.BFT_SRV);
        }
    }

    public class UMobileOpaqueNode : UMobileForwordNodeBase
    {
        public Common.URenderGraphPin ColorPinOut = Common.URenderGraphPin.CreateOutput("Color", true, EPixelFormat.PXF_R16G16B16A16_FLOAT);
        public Common.URenderGraphPin DepthPinOut = Common.URenderGraphPin.CreateOutput("Depth", true, EPixelFormat.PXF_D24_UNORM_S8_UINT);
        public Common.URenderGraphPin GizmosDepthPinOut = Common.URenderGraphPin.CreateOutput("GizmosDepth", true, EPixelFormat.PXF_D16_UNORM);
        public UMobileOpaqueNode()
        {
            Name = "MobileOpaqueNode";
        }
        public override void InitNodePins()
        {
            base.InitNodePins();

            AddOutput(ColorPinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_RTV);
            AddOutput(DepthPinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);
            AddOutput(GizmosDepthPinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);
        }
        public UBasePassOpaque mOpaqueShading;
        public UPassDrawBuffers BasePass = new UPassDrawBuffers();
        public NxRHI.URenderPass RenderPass;
        public NxRHI.URenderPass GizmosRenderPass;

        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

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
                //PassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
            }
            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);
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
                //GizmosPassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
                //GizmosPassDesc.mDepthClearValue = 1.0f;
                //GizmosPassDesc.mStencilClearValue = 0u;
            }
            GizmosRenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in GizmosPassDesc);

            GBuffers.Initialize(policy, RenderPass);
            GBuffers.SetRenderTarget(policy, 0, ColorPinOut);
            GBuffers.SetDepthStencil(policy, DepthPinOut);
            GBuffers.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;

            GGizmosBuffers.Initialize(policy, GizmosRenderPass);
            GGizmosBuffers.SetRenderTarget(policy, 0, ColorPinOut);
            GBuffers.SetDepthStencil(policy, GizmosDepthPinOut);
            GGizmosBuffers.TargetViewIdentifier = GBuffers.TargetViewIdentifier;

            //mBasePassShading = shading as Pipeline.Mobile.UBasePassOpaque;
            mOpaqueShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UBasePassOpaque>();
        }
        public override void Cleanup()
        {
            GBuffers?.Cleanup();
            GBuffers = null;

            GGizmosBuffers?.Cleanup();
            GGizmosBuffers = null;

            base.Cleanup();
        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            if (GBuffers != null)
            {
                GBuffers.OnResize(x, y);
            }
            if (GGizmosBuffers != null)
            {
                GGizmosBuffers.OnResize(x, y);
            }
        }
        protected void SetCBuffer(GamePlay.UWorld world, NxRHI.UCbView cBuffer, URenderPolicy mobilePolicy)
        {
            var coreBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
            var shadowNode = mobilePolicy.FindFirstNode<Shadow.UShadowMapNode>();
            if (shadowNode != null)
            {
                cBuffer.SetValue(coreBinder.CBPerViewport.gFadeParam, in shadowNode.mFadeParam);
                cBuffer.SetValue(coreBinder.CBPerViewport.gShadowTransitionScale, in shadowNode.mShadowTransitionScale);
                cBuffer.SetValue(coreBinder.CBPerViewport.gShadowMapSizeAndRcp, in shadowNode.mShadowMapSizeAndRcp);
                cBuffer.SetValue(coreBinder.CBPerViewport.gViewer2ShadowMtx, in shadowNode.mViewer2ShadowMtx);
                cBuffer.SetValue(coreBinder.CBPerViewport.gShadowDistance, in shadowNode.mShadowDistance);
                
                cBuffer.SetValue(coreBinder.CBPerViewport.gCsmDistanceArray, in shadowNode.mSumDistanceFarVec);
                cBuffer.SetValue(coreBinder.CBPerViewport.gViewer2ShadowMtxArrayEditor, 0, in shadowNode.mViewer2ShadowMtxArray[0]);
                cBuffer.SetValue(coreBinder.CBPerViewport.gViewer2ShadowMtxArrayEditor, 1, in shadowNode.mViewer2ShadowMtxArray[1]);
                cBuffer.SetValue(coreBinder.CBPerViewport.gViewer2ShadowMtxArrayEditor, 2, in shadowNode.mViewer2ShadowMtxArray[2]);
                cBuffer.SetValue(coreBinder.CBPerViewport.gViewer2ShadowMtxArrayEditor, 3, in shadowNode.mViewer2ShadowMtxArray[3]);

                cBuffer.SetValue(coreBinder.CBPerViewport.gShadowTransitionScaleArrayEditor, in shadowNode.mShadowTransitionScaleVec);
                cBuffer.SetValue(coreBinder.CBPerViewport.gCsmNum, in shadowNode.mCsmNum);
            }

            var dirLight = world.DirectionLight;
            //dirLight.mDirection = MathHelper.RandomDirection();
            var dir = dirLight.Direction;
            var gDirLightDirection_Leak = new Vector4(dir.X, dir.Y, dir.Z, dirLight.mSunLightLeak);
            cBuffer.SetValue(coreBinder.CBPerViewport.gDirLightDirection_Leak, in gDirLightDirection_Leak);
            var gDirLightColor_Intensity = new Vector4(dirLight.mSunLightColor.X, dirLight.mSunLightColor.Y, dirLight.mSunLightColor.Z, dirLight.mSunLightIntensity);
            cBuffer.SetValue(coreBinder.CBPerViewport.gDirLightColor_Intensity, in gDirLightColor_Intensity);

            cBuffer.SetValue(coreBinder.CBPerViewport.mSkyLightColor, in dirLight.mSkyLightColor);
            cBuffer.SetValue(coreBinder.CBPerViewport.mGroundLightColor, in dirLight.mGroundLightColor);

            float EnvMapMaxMipLevel = 1.0f;
            cBuffer.SetValue(coreBinder.CBPerViewport.gEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
            cBuffer.SetValue(coreBinder.CBPerViewport.gEyeEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UMobileOpaqueNode), nameof(TickLogic));
        public override void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                var mobilePolicy = policy;
                var cBuffer = GBuffers.PerViewportCBuffer;
                if (cBuffer != null)
                    SetCBuffer(world, cBuffer, mobilePolicy);

                BasePass.ClearMeshDrawPassArray();
                BasePass.SetViewport(in GBuffers.Viewport);

                foreach (var i in policy.VisibleMeshes)
                {
                    if (i.Atoms == null)
                        continue;

                    for (int j = 0; j < i.Atoms.Length; j++)
                    {
                        var layer = i.Atoms[j].Material.RenderLayer;
                        if (layer != ERenderLayer.RL_Opaque)
                            continue;
                        var drawcall = i.GetDrawCall(GBuffers, j, policy, URenderPolicy.EShadingType.BasePass, this);
                        if (drawcall != null)
                        {
                            drawcall.BindGBuffer(policy.DefaultCamera, GBuffers);
                            //GGizmosBuffers.PerViewportCBuffer = GBuffers.PerViewportCBuffer;

                            BasePass.PushDrawCall(layer, drawcall);
                        }
                    }
                }
                

                var cmdlist = BasePass.PassBuffers[(int)ERenderLayer.RL_Opaque].DrawCmdList;

                var passClears = new NxRHI.FRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4(1, 0, 0, 0));
                GBuffers.BuildFrameBuffers(policy);
                cmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, ERenderLayer.RL_Opaque.ToString());
                cmdlist.FlushDraws();
                cmdlist.EndPass();
                cmdlist.EndCommand();
            }
        }
        public override void TickSync(URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }

    public class UMobileTranslucentNode : Common.UBasePassNode
    {
        public Graphics.Pipeline.Common.URenderGraphPin AlbedoPinInOut = Graphics.Pipeline.Common.URenderGraphPin.CreateInputOutput("Albedo");
        public Graphics.Pipeline.Common.URenderGraphPin DepthPinInOut = Graphics.Pipeline.Common.URenderGraphPin.CreateInputOutput("Depth");

        public Graphics.Pipeline.Common.URenderGraphPin GizmosDepthPinOut = Graphics.Pipeline.Common.URenderGraphPin.CreateOutput("GizmosDepth", true, EPixelFormat.PXF_D16_UNORM);
        public UMobileTranslucentNode()
        {
            Name = "UMobileTranslucentNode";
        }
        public override void InitNodePins()
        {
            base.InitNodePins();

            AddInputOutput(AlbedoPinInOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_RTV);
            AddInputOutput(DepthPinInOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);
            
            AddOutput(GizmosDepthPinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);
        }
        public override void FrameBuild()
        {
            
        }
        public UBasePassTranslucent mTranslucentShading;
        public UPassDrawBuffers BasePass = new UPassDrawBuffers();
        public NxRHI.URenderPass RenderPass;
        public NxRHI.URenderPass GizmosRenderPass;
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

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
                //PassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
            }
            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

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
                //GizmosPassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
                //GizmosPassDesc.mDepthClearValue = 1.0f;
                //GizmosPassDesc.mStencilClearValue = 0u;
            }
            GizmosRenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in GizmosPassDesc);

            GBuffers.Initialize(policy, RenderPass);
            GBuffers.SetRenderTarget(policy, 0, AlbedoPinInOut);
            GBuffers.SetDepthStencil(policy, DepthPinInOut);
            GBuffers.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;
            
            GGizmosBuffers.Initialize(policy, GizmosRenderPass);
            GGizmosBuffers.SetRenderTarget(policy, 0, AlbedoPinInOut);
            GGizmosBuffers.SetDepthStencil(policy, GizmosDepthPinOut);
            GGizmosBuffers.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;

            mTranslucentShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UBasePassTranslucent>();
        }
        public override void Cleanup()
        {
            GBuffers?.Cleanup();
            GBuffers = null;

            GGizmosBuffers?.Cleanup();
            GGizmosBuffers = null;

            base.Cleanup();
        }
        protected void SetCBuffer(GamePlay.UWorld world, NxRHI.UCbView cBuffer, UMobileFSPolicy mobilePolicy)
        {
            var coreBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
            cBuffer.SetValue(coreBinder.CBPerViewport.gFadeParam, in mobilePolicy.mShadowMapNode.mFadeParam);
            cBuffer.SetValue(coreBinder.CBPerViewport.gShadowTransitionScale, in mobilePolicy.mShadowMapNode.mShadowTransitionScale);
            cBuffer.SetValue(coreBinder.CBPerViewport.gShadowMapSizeAndRcp, in mobilePolicy.mShadowMapNode.mShadowMapSizeAndRcp);
            cBuffer.SetValue(coreBinder.CBPerViewport.gViewer2ShadowMtx, in mobilePolicy.mShadowMapNode.mViewer2ShadowMtx);
            cBuffer.SetValue(coreBinder.CBPerViewport.gShadowDistance, in mobilePolicy.mShadowMapNode.mShadowDistance);

            cBuffer.SetValue(coreBinder.CBPerViewport.gCsmDistanceArray, in mobilePolicy.mShadowMapNode.mSumDistanceFarVec);

            cBuffer.SetValue(coreBinder.CBPerViewport.gViewer2ShadowMtxArrayEditor, 0, in mobilePolicy.mShadowMapNode.mViewer2ShadowMtxArray[0]);
            cBuffer.SetValue(coreBinder.CBPerViewport.gViewer2ShadowMtxArrayEditor, 1, in mobilePolicy.mShadowMapNode.mViewer2ShadowMtxArray[1]);
            cBuffer.SetValue(coreBinder.CBPerViewport.gViewer2ShadowMtxArrayEditor, 2, in mobilePolicy.mShadowMapNode.mViewer2ShadowMtxArray[2]);
            cBuffer.SetValue(coreBinder.CBPerViewport.gViewer2ShadowMtxArrayEditor, 3, in mobilePolicy.mShadowMapNode.mViewer2ShadowMtxArray[3]);

            cBuffer.SetValue(coreBinder.CBPerViewport.gShadowTransitionScaleArrayEditor, in mobilePolicy.mShadowMapNode.mShadowTransitionScaleVec);
            cBuffer.SetValue(coreBinder.CBPerViewport.gCsmNum, in mobilePolicy.mShadowMapNode.mCsmNum);

            var dirLight = world.DirectionLight;
            //dirLight.mDirection = MathHelper.RandomDirection();
            var dir = dirLight.Direction;
            var gDirLightDirection_Leak = new Vector4(dir.X, dir.Y, dir.Z, dirLight.mSunLightLeak);
            cBuffer.SetValue(coreBinder.CBPerViewport.gDirLightDirection_Leak, in gDirLightDirection_Leak);
            var gDirLightColor_Intensity = new Vector4(dirLight.mSunLightColor.X, dirLight.mSunLightColor.Y, dirLight.mSunLightColor.Z, dirLight.mSunLightIntensity);
            cBuffer.SetValue(coreBinder.CBPerViewport.gDirLightColor_Intensity, in gDirLightColor_Intensity);

            cBuffer.SetValue(coreBinder.CBPerViewport.mSkyLightColor, in dirLight.mSkyLightColor);
            cBuffer.SetValue(coreBinder.CBPerViewport.mGroundLightColor, in dirLight.mGroundLightColor);

            float EnvMapMaxMipLevel = 1.0f;
            cBuffer.SetValue(coreBinder.CBPerViewport.gEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
            cBuffer.SetValue(coreBinder.CBPerViewport.gEyeEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            if (GBuffers != null)
            {
                GBuffers.OnResize(x, y);
            }
            if (GGizmosBuffers != null)
            {
                GGizmosBuffers.OnResize(x, y);
            }
            base.OnResize(policy, x, y);
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UMobileTranslucentNode), nameof(TickLogic));
        public override void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                var mobilePolicy = policy as UMobileFSPolicy;
                var cBuffer = GBuffers.PerViewportCBuffer;
                if (mobilePolicy != null)
                {
                    if (cBuffer != null)
                        SetCBuffer(world, cBuffer, mobilePolicy);
                }

                BasePass.ClearMeshDrawPassArray();
                BasePass.SetViewport(in GBuffers.Viewport);

                foreach (var i in policy.VisibleMeshes)
                {
                    if (i.Atoms == null)
                        continue;

                    for (int j = 0; j < i.Atoms.Length; j++)
                    {
                        var layer = i.Atoms[j].Material.RenderLayer;
                        if (layer == ERenderLayer.RL_Opaque)
                            continue;
                        var drawcall = i.GetDrawCall(GBuffers, j, policy, URenderPolicy.EShadingType.BasePass, this);
                        if (drawcall != null)
                        {
                            drawcall.BindGBuffer(policy.DefaultCamera, GBuffers);
                            //GGizmosBuffers.PerViewportCBuffer = GBuffers.PerViewportCBuffer;

                            BasePass.PushDrawCall(layer, drawcall);
                        }
                    }
                }
                var passClears = new NxRHI.FRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4(1, 0, 0, 0));
                BasePass.BuildTranslucentRenderPass(policy, in passClears, GBuffers, GGizmosBuffers);
            }   
        }
        public override void TickSync(URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}

namespace EngineNS.UTest
{
    [UTest.UTest]
    public class UTest_ShadingEnv
    {
        public void UnitTestEntrance()
        {
            var env = UEngine.Instance.ShadingEnvManager.GetShadingEnv<Graphics.Pipeline.Mobile.UBasePassOpaque>();

            env.BeginPermutaion();
            var Name1 = env.PushPermutation<EPixelFormat>("Name1", 3);

            Name1.SetValue((int)EPixelFormat.PXF_R16_FLOAT);
            var Str = Name1.GetValueString(in Name1.Value);
            env.UpdatePermutation();

            UnitTestManager.TAssert(Str == "PXF_R16_FLOAT", "");
            UnitTestManager.TAssert(env.CurrentPermutationId.Data == (int)EPixelFormat.PXF_R16_FLOAT, "");
        }
    }
}
