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
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position,
                EVertexSteamType.VST_Normal,
                EVertexSteamType.VST_Tangent,
                EVertexSteamType.VST_Color,
                EVertexSteamType.VST_LightMap,
                EVertexSteamType.VST_UV,};
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
        public unsafe override void OnBuildDrawCall(URenderPolicy policy, RHI.CDrawCall drawcall)
        {
        }
        public unsafe override void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, URenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            var Manager = policy as Mobile.UMobileEditorFSPolicy;
            if (Manager != null)
            {
                var indexer = drawcall.Effect.ShaderIndexer;
                if (Manager.EnvMapSRV != null)
                {
                    drawcall.mCoreObject.BindShaderSrv(indexer.gEnvMap, Manager.EnvMapSRV.mCoreObject);
                    drawcall.mCoreObject.BindShaderSampler(indexer.Samp_gEnvMap, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);
                }

                if (Manager.mShadowMapNode != null)
                {
                    var depth = Manager.mShadowMapNode.GetAttachBuffer(Manager.mShadowMapNode.DepthPinOut);
                    drawcall.mCoreObject.BindShaderSrv(indexer.gShadowMap, depth.Srv.mCoreObject);
                    drawcall.mCoreObject.BindShaderSampler(indexer.Samp_gShadowMap, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);
                }

                var index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
                if (!CoreSDK.IsNullPointer(index))
                    drawcall.mCoreObject.BindShaderCBuffer(index, Manager.GpuSceneNode.PerGpuSceneCBuffer.mCoreObject);

                index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "TilingBuffer");
                if (!CoreSDK.IsNullPointer(index))
                    drawcall.mCoreObject.BindShaderSrv(index, Manager.ScreenTilingNode.TileSRV.mCoreObject);

                index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GpuScene_PointLights");
                if (!CoreSDK.IsNullPointer(index))
                    drawcall.mCoreObject.BindShaderSrv(index, Manager.GpuSceneNode.PointLights.DataSRV.mCoreObject);
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
    public class UMobileOpaqueNode : Common.UBasePassNode
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
            AddOutput(ColorPinOut, EGpuBufferViewType.GBVT_Srv | EGpuBufferViewType.GBVT_Rtv);
            AddOutput(DepthPinOut, EGpuBufferViewType.GBVT_Srv | EGpuBufferViewType.GBVT_Dsv);
            AddOutput(GizmosDepthPinOut, EGpuBufferViewType.GBVT_Srv | EGpuBufferViewType.GBVT_Dsv);
        }
        public UBasePassOpaque mOpaqueShading;
        public UPassDrawBuffers BasePass = new UPassDrawBuffers();
        public RHI.CRenderPass RenderPass;
        public RHI.CRenderPass GizmosRenderPass;

        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

            var PassDesc = new IRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].IsSwapChain = 0;
                PassDesc.AttachmentMRTs[0].Format = ColorPinOut.Attachement.Format;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.Format = DepthPinOut.Attachement.Format;
                PassDesc.m_AttachmentDepthStencil.Samples = 1;
                PassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
            }
            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in PassDesc);
            var GizmosPassDesc = new IRenderPassDesc();
            unsafe
            {
                GizmosPassDesc.NumOfMRT = 1;
                GizmosPassDesc.AttachmentMRTs[0].IsSwapChain = 0;
                GizmosPassDesc.AttachmentMRTs[0].Format = ColorPinOut.Attachement.Format;
                GizmosPassDesc.AttachmentMRTs[0].Samples = 1;
                GizmosPassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionDontCare;
                GizmosPassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.Format = GizmosDepthPinOut.Attachement.Format;
                GizmosPassDesc.m_AttachmentDepthStencil.Samples = 1;
                GizmosPassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //GizmosPassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
                //GizmosPassDesc.mDepthClearValue = 1.0f;
                //GizmosPassDesc.mStencilClearValue = 0u;
            }
            GizmosRenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in GizmosPassDesc);

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
        protected void SetCBuffer(GamePlay.UWorld world, RHI.CConstantBuffer cBuffer, UMobileFSPolicy mobilePolicy)
        {
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gFadeParam, in mobilePolicy.mShadowMapNode.mFadeParam);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowTransitionScale, in mobilePolicy.mShadowMapNode.mShadowTransitionScale);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowMapSizeAndRcp, in mobilePolicy.mShadowMapNode.mShadowMapSizeAndRcp);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gViewer2ShadowMtx, in mobilePolicy.mShadowMapNode.mViewer2ShadowMtx);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowDistance, in mobilePolicy.mShadowMapNode.mShadowDistance);

            var dirLight = world.DirectionLight;
            //dirLight.mDirection = MathHelper.RandomDirection();
            var dir = dirLight.mDirection;
            var gDirLightDirection_Leak = new Vector4(dir.X, dir.Y, dir.Z, dirLight.mSunLightLeak);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gDirLightDirection_Leak, in gDirLightDirection_Leak);
            var gDirLightColor_Intensity = new Vector4(dirLight.mSunLightColor.X, dirLight.mSunLightColor.Y, dirLight.mSunLightColor.Z, dirLight.mSunLightIntensity);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gDirLightColor_Intensity, in gDirLightColor_Intensity);

            cBuffer.SetValue(cBuffer.PerViewportIndexer.mSkyLightColor, in dirLight.mSkyLightColor);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.mGroundLightColor, in dirLight.mGroundLightColor);

            float EnvMapMaxMipLevel = 1.0f;
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gEyeEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
        }
        public override void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            var mobilePolicy = policy as UMobileFSPolicy;
            var cBuffer = GBuffers.PerViewportCBuffer;
            if (mobilePolicy != null)
            {
                if (cBuffer != null)
                    SetCBuffer(world, cBuffer, mobilePolicy);
            }

            BasePass.ClearMeshDrawPassArray();
            BasePass.SetViewport(GBuffers.ViewPort);

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

            var passClears = new IRenderPassClears();
            passClears.SetDefault();
            passClears.SetClearColor(0, new Color4(1, 0, 0, 0));
            cmdlist.BeginRenderPass(policy, GBuffers, in passClears, ERenderLayer.RL_Opaque.ToString());
            cmdlist.BuildRenderPass(0);
            cmdlist.EndRenderPass();
            cmdlist.EndCommand();
        }
        public override void TickRender(URenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Commit(rc);
        }
        public override void TickSync(URenderPolicy policy)
        {
            BasePass.SwapBuffer();
            policy.DefaultCamera.mCoreObject.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, 1);
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
            AddInputOutput(AlbedoPinInOut, EGpuBufferViewType.GBVT_Srv | EGpuBufferViewType.GBVT_Rtv);
            AddInputOutput(DepthPinInOut, EGpuBufferViewType.GBVT_Srv | EGpuBufferViewType.GBVT_Dsv);
            
            AddOutput(GizmosDepthPinOut, EGpuBufferViewType.GBVT_Srv | EGpuBufferViewType.GBVT_Dsv);
        }
        public override void FrameBuild()
        {
            
        }
        public UBasePassTranslucent mTranslucentShading;
        public UPassDrawBuffers BasePass = new UPassDrawBuffers();
        public RHI.CRenderPass RenderPass;
        public RHI.CRenderPass GizmosRenderPass;
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

            var PassDesc = new IRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].IsSwapChain = 0;
                PassDesc.AttachmentMRTs[0].Format = AlbedoPinInOut.Attachement.Format;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionDontCare;
                PassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.Format = DepthPinInOut.Attachement.Format;
                PassDesc.m_AttachmentDepthStencil.Samples = 1;
                PassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionDontCare;
                PassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionDontCare;
                PassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
            }
            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in PassDesc);

            var GizmosPassDesc = new IRenderPassDesc();
            unsafe
            {
                GizmosPassDesc.NumOfMRT = 1;
                GizmosPassDesc.AttachmentMRTs[0].IsSwapChain = 0;
                GizmosPassDesc.AttachmentMRTs[0].Format = AlbedoPinInOut.Attachement.Format;
                GizmosPassDesc.AttachmentMRTs[0].Samples = 1;
                GizmosPassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionDontCare;
                GizmosPassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.Format = GizmosDepthPinOut.Attachement.Format;
                GizmosPassDesc.m_AttachmentDepthStencil.Samples = 1;
                GizmosPassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //GizmosPassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
                //GizmosPassDesc.mDepthClearValue = 1.0f;
                //GizmosPassDesc.mStencilClearValue = 0u;
            }
            GizmosRenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in GizmosPassDesc);

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
        protected void SetCBuffer(GamePlay.UWorld world, RHI.CConstantBuffer cBuffer, UMobileFSPolicy mobilePolicy)
        {
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gFadeParam, in mobilePolicy.mShadowMapNode.mFadeParam);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowTransitionScale, in mobilePolicy.mShadowMapNode.mShadowTransitionScale);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowMapSizeAndRcp, in mobilePolicy.mShadowMapNode.mShadowMapSizeAndRcp);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gViewer2ShadowMtx, in mobilePolicy.mShadowMapNode.mViewer2ShadowMtx);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowDistance, in mobilePolicy.mShadowMapNode.mShadowDistance);

            var dirLight = world.DirectionLight;
            //dirLight.mDirection = MathHelper.RandomDirection();
            var dir = dirLight.mDirection;
            var gDirLightDirection_Leak = new Vector4(dir.X, dir.Y, dir.Z, dirLight.mSunLightLeak);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gDirLightDirection_Leak, in gDirLightDirection_Leak);
            var gDirLightColor_Intensity = new Vector4(dirLight.mSunLightColor.X, dirLight.mSunLightColor.Y, dirLight.mSunLightColor.Z, dirLight.mSunLightIntensity);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gDirLightColor_Intensity, in gDirLightColor_Intensity);

            cBuffer.SetValue(cBuffer.PerViewportIndexer.mSkyLightColor, in dirLight.mSkyLightColor);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.mGroundLightColor, in dirLight.mGroundLightColor);

            float EnvMapMaxMipLevel = 1.0f;
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gEyeEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
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
        public override void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            var mobilePolicy = policy as UMobileFSPolicy;
            var cBuffer = GBuffers.PerViewportCBuffer;
            if (mobilePolicy != null)
            {
                if (cBuffer != null)
                    SetCBuffer(world, cBuffer, mobilePolicy);
            }

            BasePass.ClearMeshDrawPassArray();
            BasePass.SetViewport(GBuffers.ViewPort);

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
            var passClears = new IRenderPassClears();
            passClears.SetDefault();
            passClears.SetClearColor(0, new Color4(1, 0, 0, 0));
            BasePass.BuildTranslucentRenderPass(policy, in passClears, GBuffers, GGizmosBuffers);
        }
        public override void TickRender(URenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Commit(rc);
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
