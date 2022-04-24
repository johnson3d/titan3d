using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    public class UDeferredDirLightingShading : Shader.UShadingEnv
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
        public UPermutationItem DisableSunshaft
        {
            get;
            set;
        }
        public UPermutationItem DisableBloom
        {
            get;
            set;
        }
        public UPermutationItem DisableHdr
        {
            get;
            set;
        }
        public UDeferredDirLightingShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Deferred/DeferredDirLighting.cginc", RName.ERNameType.Engine);

            this.BeginPermutaion();

            DisableAO = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_AO", (int)Shader.EPermutation_Bool.BitWidth);
            DisablePointLights = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_POINTLIGHTS", (int)Shader.EPermutation_Bool.BitWidth);
            DisableShadow = this.PushPermutation<Shader.EPermutation_Bool>("DISABLE_SHADOW_ALL", (int)Shader.EPermutation_Bool.BitWidth);
            DisableSunshaft = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_SUNSHAFT", (int)Shader.EPermutation_Bool.BitWidth);
            DisableBloom = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_BLOOM", (int)Shader.EPermutation_Bool.BitWidth);
            DisableHdr = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_HDR", (int)Shader.EPermutation_Bool.BitWidth);

            DisableAO.SetValue((int)Shader.EPermutation_Bool.FalseValue);
            DisableShadow.SetValue((int)Shader.EPermutation_Bool.FalseValue);
            DisablePointLights.SetValue((int)Shader.EPermutation_Bool.FalseValue);

            DisableSunshaft.SetValue((int)Shader.EPermutation_Bool.TrueValue);
            DisableBloom.SetValue((int)Shader.EPermutation_Bool.TrueValue);
            DisableHdr.SetValue((int)Shader.EPermutation_Bool.TrueValue);

            this.UpdatePermutation();
        }
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position,
                EVertexSteamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(URenderPolicy policy, RHI.CDrawCall drawcall)
        {
        }
        public unsafe override void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, URenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            var Manager = policy.TagObject as URenderPolicy;
            var dirLightingNode = Manager.FindFirstNode<UDeferredDirLightingNode>();

            var gpuProgram = drawcall.Effect.ShaderProgram;
            var index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GBufferRT0");
            if (!CoreSDK.IsNullPointer(index))
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt0PinIn);
                drawcall.mCoreObject.BindShaderSrv(index, attachBuffer.Srv.mCoreObject);
            }
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_GBufferRT0");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GBufferRT1");
            if (!CoreSDK.IsNullPointer(index))
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt1PinIn);
                drawcall.mCoreObject.BindShaderSrv(index, attachBuffer.Srv.mCoreObject);
            }
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_GBufferRT1");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GBufferRT2");
            if (!CoreSDK.IsNullPointer(index))
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt2PinIn);
                drawcall.mCoreObject.BindShaderSrv(index, attachBuffer.Srv.mCoreObject);
            }
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_GBufferRT2");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "DepthBuffer");
            if (!CoreSDK.IsNullPointer(index))
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.DepthStencilPinIn);
                drawcall.mCoreObject.BindShaderSrv(index, attachBuffer.Srv.mCoreObject);
            }
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_DepthBuffer");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GShadowMap");
            if (!CoreSDK.IsNullPointer(index))
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.ShadowMapPinIn);
                drawcall.mCoreObject.BindShaderSrv(index, attachBuffer.Srv.mCoreObject);
            }
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_GShadowMap");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "gEnvMap");
            if (!CoreSDK.IsNullPointer(index))
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.EnvMapPinIn);
                drawcall.mCoreObject.BindShaderSrv(index, attachBuffer.Srv.mCoreObject);
            }
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_gEnvMap");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GVignette");
            if (!CoreSDK.IsNullPointer(index))
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.VignettePinIn);
                drawcall.mCoreObject.BindShaderSrv(index, attachBuffer.Srv.mCoreObject);
            }
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_GVignette");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GPickedTex");
            if (!CoreSDK.IsNullPointer(index))
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.PickPinIn);
                drawcall.mCoreObject.BindShaderSrv(index, attachBuffer.Srv.mCoreObject);
            }
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_GPickedTex");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
            if (!CoreSDK.IsNullPointer(index))
            {
                //drawcall.mCoreObject.BindShaderCBuffer(index, Manager.GetGpuSceneNode().PerGpuSceneCBuffer.mCoreObject);
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.GpuScenePinIn);
                drawcall.mCoreObject.BindShaderCBuffer(index, attachBuffer.CBuffer.mCoreObject);
            }

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "TilingBuffer");
            if (!CoreSDK.IsNullPointer(index))
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.TileScreenPinIn);
                drawcall.mCoreObject.BindShaderSrv(index, attachBuffer.Srv.mCoreObject);
            }

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GpuScene_PointLights");
            if (!CoreSDK.IsNullPointer(index))
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.PointLightsPinIn);
                if (attachBuffer.Srv != null)
                    drawcall.mCoreObject.BindShaderSrv(index, attachBuffer.Srv.mCoreObject);
            }
        }
        public void SetDisableShadow(bool value)
        {
            DisableShadow.SetValue(value);
            UpdatePermutation();
        }
        public void SetDisableAO(bool value)
        {
            DisableAO.SetValue(value);
            UpdatePermutation();
        }
        public void SetDisableSunShaft(bool value)
        {
            DisableSunshaft.SetValue(value);
            UpdatePermutation();
        }
        public void SetDisableBloom(bool value)
        {
            DisableBloom.SetValue(value);
            UpdatePermutation();
        }
        public void SetDisableHDR(bool value)
        {
            DisableHdr.SetValue(value);
            UpdatePermutation();
        }
        public void SetDisablePointLights(bool value)
        {
            DisablePointLights.SetValue(value);
            UpdatePermutation();
        }
    }
    public class UDeferredDirLightingNode : Common.USceenSpaceNode
    {
        public Common.URenderGraphPin Rt0PinIn = Common.URenderGraphPin.CreateInput("MRT0");
        public Common.URenderGraphPin Rt1PinIn = Common.URenderGraphPin.CreateInput("MRT1");
        public Common.URenderGraphPin Rt2PinIn = Common.URenderGraphPin.CreateInput("MRT2");
        public Common.URenderGraphPin DepthStencilPinIn = Common.URenderGraphPin.CreateInput("DepthStencil");
        public Common.URenderGraphPin ShadowMapPinIn = Common.URenderGraphPin.CreateInput("ShadowMap");
        public Common.URenderGraphPin EnvMapPinIn = Common.URenderGraphPin.CreateInput("EnvMap");
        public Common.URenderGraphPin VignettePinIn = Common.URenderGraphPin.CreateInput("Vignette");
        public Common.URenderGraphPin PickPinIn = Common.URenderGraphPin.CreateInput("Pick");
        public Common.URenderGraphPin TileScreenPinIn = Common.URenderGraphPin.CreateInput("TileScreen");
        public Common.URenderGraphPin GpuScenePinIn = Common.URenderGraphPin.CreateInput("GpuScene");
        public Common.URenderGraphPin PointLightsPinIn = Common.URenderGraphPin.CreateInput("PointLights");

        public UDeferredDirLightingNode()
        {
            Name = "UDeferredDirLightingNode";
        }
        public override void InitNodePins()
        {
            AddInput(Rt0PinIn, EGpuBufferViewType.GBVT_Srv);
            AddInput(Rt1PinIn, EGpuBufferViewType.GBVT_Srv);
            AddInput(Rt2PinIn, EGpuBufferViewType.GBVT_Srv);
            AddInput(DepthStencilPinIn, EGpuBufferViewType.GBVT_Srv);
            AddInput(ShadowMapPinIn, EGpuBufferViewType.GBVT_Srv);
            AddInput(EnvMapPinIn, EGpuBufferViewType.GBVT_Srv);
            AddInput(VignettePinIn, EGpuBufferViewType.GBVT_Srv);
            AddInput(PickPinIn, EGpuBufferViewType.GBVT_Srv);
            AddInput(TileScreenPinIn, EGpuBufferViewType.GBVT_Srv);
            AddInput(PointLightsPinIn, EGpuBufferViewType.GBVT_Srv);
            AddInput(GpuScenePinIn, EGpuBufferViewType.GBVT_Srv | EGpuBufferViewType.GBVT_Uav);

            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R10G10B10A2_UNORM;
            base.InitNodePins();
            //Setup by base class
            //pin = AddOutput("LightingResult");
            //{
            //    pin.Attachement.AttachmentName = FHashText.Create($"{Name}->LightingResult");
            //}
        }
        public override void FrameBuild()
        {
            
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UDeferredDirLightingShading>();
        }
        private void SetCBuffer(GamePlay.UWorld world, RHI.CConstantBuffer cBuffer, URenderPolicy mobilePolicy)
        {
            var shadowNode = mobilePolicy.FindFirstNode<Shadow.UShadowMapNode>();
            if (shadowNode != null)
            {
                cBuffer.SetValue(cBuffer.PerViewportIndexer.gFadeParam, in shadowNode.mFadeParam);
                cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowTransitionScale, in shadowNode.mShadowTransitionScale);
                cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowMapSizeAndRcp, in shadowNode.mShadowMapSizeAndRcp);
                cBuffer.SetValue(cBuffer.PerViewportIndexer.gViewer2ShadowMtx, in shadowNode.mViewer2ShadowMtx);
                cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowDistance, in shadowNode.mShadowDistance);
            }

            var dirLight = world.DirectionLight;
            //dirLight.mDirection = MathHelper.RandomDirection();
            var dir = dirLight.Direction;
            var gDirLightDirection_Leak = new Vector4(dir.X, dir.Y, dir.Z, dirLight.mSunLightLeak);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gDirLightDirection_Leak, in gDirLightDirection_Leak);
            var gDirLightColor_Intensity = new Vector4(dirLight.SunLightColor.X, dirLight.SunLightColor.Y, dirLight.SunLightColor.Z, dirLight.mSunLightIntensity);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gDirLightColor_Intensity, in gDirLightColor_Intensity);

            cBuffer.SetValue(cBuffer.PerViewportIndexer.mSkyLightColor, in dirLight.mSkyLightColor);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.mGroundLightColor, in dirLight.mGroundLightColor);

            float EnvMapMaxMipLevel = 1.0f;
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gEyeEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
        }
        public override void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            var cBuffer = GBuffers.PerViewportCBuffer;
            if (cBuffer != null)
                SetCBuffer(world, cBuffer, policy);
            base.TickLogic(world, policy, bClear);
        }
    }
}
