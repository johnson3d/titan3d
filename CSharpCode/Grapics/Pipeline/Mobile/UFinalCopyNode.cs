using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class UFinalCopyShading : Shader.UShadingEnv
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
        public UFinalCopyShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Mobile/MobileCopyEditor.cginc", RName.ERNameType.Engine);

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
        public unsafe override void OnBuildDrawCall(URenderPolicy policy, RHI.CDrawCall drawcall)
        {
        }
        public unsafe override void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, URenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            var Manager = policy.TagObject as URenderPolicy;

            var node = Manager.FindFirstNode<UFinalCopyNode>();
            var gpuProgram = drawcall.Effect.ShaderProgram;
            var index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "gBaseSceneView");
            if (!CoreSDK.IsNullPointer(index))
            {
                drawcall.mCoreObject.BindShaderSrv(index, node.GetAttachBuffer(node.ColorPinIn).Srv.mCoreObject);
            }
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_gBaseSceneView");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "gPickedTex");
            if (!CoreSDK.IsNullPointer(index))
            {
                drawcall.mCoreObject.BindShaderSrv(index, node.GetAttachBuffer(node.PickPinIn).Srv.mCoreObject);
            }
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_gPickedTex");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GVignette");
            if (!CoreSDK.IsNullPointer(index))
            {
                drawcall.mCoreObject.BindShaderSrv(index, node.GetAttachBuffer(node.VignettePinIn).Srv.mCoreObject);
            }
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_GVignette");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);
        }
    }
    public class UFinalCopyNode : Common.USceenSpaceNode
    {
        public Common.URenderGraphPin ColorPinIn = Common.URenderGraphPin.CreateInput("Color");
        //public Common.URenderGraphPin DepthPinIn = Common.URenderGraphPin.CreateInput("Depth");

        public Common.URenderGraphPin PickPinIn = Common.URenderGraphPin.CreateInput("Pick");
        public Common.URenderGraphPin VignettePinIn = Common.URenderGraphPin.CreateInput("Vignette");
        public UFinalCopyNode()
        {
            Name = "UFinalCopyNode";
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn, EGpuBufferViewType.GBVT_Srv);
            //AddInput(DepthPinIn);
            AddInput(PickPinIn, EGpuBufferViewType.GBVT_Srv);
            AddInput(VignettePinIn, EGpuBufferViewType.GBVT_Srv);

            base.InitNodePins();
            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            //result by base
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UFinalCopyShading>();
        }
    }
}
