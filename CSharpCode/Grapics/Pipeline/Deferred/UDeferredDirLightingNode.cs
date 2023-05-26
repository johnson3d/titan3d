using EngineNS.Graphics.Pipeline.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    public partial class UDeferredDirLightingShading : Shader.UGraphicsShadingEnv
    {
        #region Permutation
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
        #endregion
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
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(URenderPolicy policy, NxRHI.UGraphicDraw drawcall)
        {
        }
        public unsafe override void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            var deferredPolicy = policy.TagObject as URenderPolicy;
            var dirLightingNode = deferredPolicy.FindFirstNode<UDeferredDirLightingNode>();

            var index = drawcall.FindBinder("cbPerGpuScene");
            if (index.IsValidPointer)
            {
                //drawcall.mCoreObject.BindShaderCBuffer(index, Manager.GetGpuSceneNode().PerGpuSceneCBuffer.mCoreObject);
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.GpuScenePinIn);
                drawcall.BindCBuffer(index, attachBuffer.CBuffer);
            }

            #region MRT
            index = drawcall.FindBinder("GBufferRT0");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt0PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT0");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            index = drawcall.FindBinder("GBufferRT1");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt1PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT1");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("GBufferRT2");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt2PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT2");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("GBufferRT3");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt3PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT3");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("DepthBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.DepthStencilPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_DepthBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            #endregion

            #region shadow
            index = drawcall.FindBinder("GShadowMap");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.ShadowMapPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GShadowMap");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);
            #endregion

            #region effect
            index = drawcall.FindBinder("gEnvMap");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.EnvMapPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_gEnvMap");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = drawcall.FindBinder("GVignette");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.VignettePinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GVignette");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = drawcall.FindBinder("GPickedTex");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.PickPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GPickedTex");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
            #endregion

            #region MultiLights
            index = drawcall.FindBinder("TilingBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.TileScreenPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }

            index = drawcall.FindBinder("GpuScene_PointLights");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.PointLightsPinIn);
                if (attachBuffer.Srv != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }
            #endregion

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
    public partial class UDeferredDirLightingNode : Common.USceenSpaceNode
    {
        public Common.URenderGraphPin Rt0PinIn = Common.URenderGraphPin.CreateInput("MRT0");
        public Common.URenderGraphPin Rt1PinIn = Common.URenderGraphPin.CreateInput("MRT1");
        public Common.URenderGraphPin Rt2PinIn = Common.URenderGraphPin.CreateInput("MRT2");
        public Common.URenderGraphPin Rt3PinIn = Common.URenderGraphPin.CreateInputOutput("MRT3");
        public Common.URenderGraphPin DepthStencilPinIn = Common.URenderGraphPin.CreateInputOutput("DepthStencil");
        
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
            AddInput(Rt0PinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(Rt1PinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(Rt2PinIn, NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(Rt3PinIn, NxRHI.EBufferType.BFT_SRV);
            //Rt3PinIn.IsAllowInputNull = true;
            AddInputOutput(DepthStencilPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(ShadowMapPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(EnvMapPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(VignettePinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(PickPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(TileScreenPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(PointLightsPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(GpuScenePinIn, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
            base.InitNodePins();
        }
        public override void FrameBuild(Graphics.Pipeline.URenderPolicy policy)
        {
            base.FrameBuild(policy);
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UDeferredDirLightingShading>();
        }
        private void SetCBuffer(GamePlay.UWorld world, NxRHI.UCbView cBuffer, URenderPolicy mobilePolicy)
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
            var gDirLightColor_Intensity = new Vector4(dirLight.SunLightColor.X, dirLight.SunLightColor.Y, dirLight.SunLightColor.Z, dirLight.mSunLightIntensity);
            cBuffer.SetValue(coreBinder.CBPerViewport.gDirLightColor_Intensity, in gDirLightColor_Intensity);

            cBuffer.SetValue(coreBinder.CBPerViewport.mSkyLightColor, in dirLight.mSkyLightColor);
            cBuffer.SetValue(coreBinder.CBPerViewport.mGroundLightColor, in dirLight.mGroundLightColor);

            float EnvMapMaxMipLevel = 1.0f;
            cBuffer.SetValue(coreBinder.CBPerViewport.gEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
            cBuffer.SetValue(coreBinder.CBPerViewport.gEyeEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UDeferredDirLightingNode), nameof(TickLogic));
        public override void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                var cBuffer = GBuffers.PerViewportCBuffer;
                if (cBuffer != null)
                    SetCBuffer(world, cBuffer, policy);
                base.TickLogic(world, policy, bClear);
            }
        }
        public override void TickSync(URenderPolicy policy)
        {
            base.TickSync(policy);
        }
    }
}
