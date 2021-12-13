using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    public class UDeferredDirLightingShading : Shader.UShadingEnv
    {
        public UDeferredDirLightingShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Deferred/DeferredDirLighting.cginc", RName.ERNameType.Engine);

            var disable_Shadow = new MacroDefine();//0
            disable_Shadow.Name = "DISABLE_SHADOW_ALL";
            disable_Shadow.Values.Add("0");
            disable_Shadow.Values.Add("1");
            MacroDefines.Add(disable_Shadow);

            var disable_AO = new MacroDefine();//1
            disable_AO.Name = "ENV_DISABLE_AO";
            disable_AO.Values.Add("0");
            disable_AO.Values.Add("1");
            MacroDefines.Add(disable_AO);

            var disable_Sunshaft = new MacroDefine();//2
            disable_Sunshaft.Name = "ENV_DISABLE_SUNSHAFT";
            disable_Sunshaft.Values.Add("0");
            disable_Sunshaft.Values.Add("1");
            MacroDefines.Add(disable_Sunshaft);

            var disable_Bloom = new MacroDefine();//3
            disable_Bloom.Name = "ENV_DISABLE_BLOOM";
            disable_Bloom.Values.Add("0");
            disable_Bloom.Values.Add("1");
            MacroDefines.Add(disable_Bloom);

            var disable_Hdr = new MacroDefine();//4
            disable_Hdr.Name = "ENV_DISABLE_HDR";
            disable_Hdr.Values.Add("0");
            disable_Hdr.Values.Add("1");
            MacroDefines.Add(disable_Hdr);

            var disable_PointLights = new MacroDefine();//5
            disable_PointLights.Name = "ENV_DISABLE_POINTLIGHTS";
            disable_PointLights.Values.Add("0");
            disable_PointLights.Values.Add("1");
            MacroDefines.Add(disable_PointLights);

            UpdatePermutationBitMask();

            mMacroValues.Add("0");//disable_Shadow_All = 0
            mMacroValues.Add("0");//disable_AO = 0
            mMacroValues.Add("1");//disable_Sunshaft = 1
            mMacroValues.Add("1");//disable_Bloom = 1
            mMacroValues.Add("0");//disable_Hdr = 1
            mMacroValues.Add("0");//disable_PointLights = 0

            UpdatePermutation(mMacroValues);
        }
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position,
                EVertexSteamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(IRenderPolicy policy, RHI.CDrawCall drawcall)
        {
        }
        public unsafe override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, IRenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            var Manager = policy.TagObject as UDeferredPolicy;

            var gpuProgram = drawcall.Effect.ShaderProgram;
            var index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GBufferRT0");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, Manager.GetBasePassNode().GBuffers.GetGBufferSRV(0).mCoreObject);
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_GBufferRT0");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GBufferRT1");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, Manager.GetBasePassNode().GBuffers.GetGBufferSRV(1).mCoreObject);
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_GBufferRT1");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GBufferRT2");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, Manager.GetBasePassNode().GBuffers.GetGBufferSRV(2).mCoreObject);
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_GBufferRT2");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "DepthBuffer");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, Manager.GetBasePassNode().GBuffers.GetDepthStencilSRV().mCoreObject);
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_DepthBuffer");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GShadowMap");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, Manager.mShadowMapNode.GBuffers.GetDepthStencilSRV().mCoreObject);
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_GShadowMap");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "gEnvMap");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, Manager.EnvMapSRV.mCoreObject);
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_gEnvMap");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GVignette");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, Manager.VignetteSRV.mCoreObject);
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_GVignette");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GPickedTex");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, Manager.PickHollowNode.GBuffers.GetGBufferSRV(0).mCoreObject);
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_GPickedTex");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderCBuffer(index, Manager.GetGpuSceneNode().PerGpuSceneCBuffer.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "TilingBuffer");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, Manager.ScreenTilingNode.TileSRV.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GpuScene_PointLights");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, Manager.GpuSceneNode.PointLights.DataSRV.mCoreObject); 
        }
        public void SetDisableShadow(bool value)
        {
            mMacroValues[0] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public void SetDisableAO(bool value)
        {
            mMacroValues[1] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public void SetDisableSunShaft(bool value)
        {
            mMacroValues[2] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public void SetDisableBloom(bool value)
        {
            mMacroValues[3] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public void SetDisableHDR(bool value)
        {
            mMacroValues[4] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public void SetDisablePointLights(bool value)
        {
            mMacroValues[5] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
    }
    public class UDeferredDirLightingNode : Common.USceenSpaceNode
    {
        public UDeferredDirLightingNode()
        {
        }
        public override async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat rtFmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            await base.Initialize(policy, shading, rtFmt, dsFmt, x, y, debugName);
            GBuffers.Camera = policy.GetBasePassNode().GBuffers.Camera;
        }
        private void SetCBuffer(GamePlay.UWorld world, RHI.CConstantBuffer cBuffer, UDeferredPolicy mobilePolicy)
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
        public override void TickLogic(GamePlay.UWorld world, IRenderPolicy policy, bool bClear)
        {
            var mobilePolicy = policy as UDeferredPolicy;
            var cBuffer = GBuffers.PerViewportCBuffer;
            if (mobilePolicy != null)
            {
                if (cBuffer != null)
                    SetCBuffer(world, cBuffer, mobilePolicy);
            }
            base.TickLogic(world, policy, bClear);
        }
    }
}
