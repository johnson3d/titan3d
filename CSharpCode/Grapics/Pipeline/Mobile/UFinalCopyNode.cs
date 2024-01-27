using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class UFinalCopyShading : Shader.UGraphicsShadingEnv
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
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
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
        public unsafe override void OnBuildDrawCall(URenderPolicy policy, NxRHI.UGraphicDraw drawcall)
        {
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var Manager = policy.TagObject as URenderPolicy;

            var node = Manager.FindFirstNode<UFinalCopyNode>();
            var index = drawcall.FindBinder("gBaseSceneView");
            if (index.IsValidPointer)
            {
                drawcall.BindSRV(index, node.GetAttachBuffer(node.ColorPinIn).Srv);
            }
            index = drawcall.FindBinder("Samp_gBaseSceneView");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = drawcall.FindBinder("gPickedTex");
            if (index.IsValidPointer)
            {
                drawcall.BindSRV(index, node.GetAttachBuffer(node.PickPinIn).Srv);
            }
            index = drawcall.FindBinder("Samp_gPickedTex");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = drawcall.FindBinder("GVignette");
            if (index.IsValidPointer)
            {
                drawcall.BindSRV(index, node.GetAttachBuffer(node.VignettePinIn).Srv);
            }
            index = drawcall.FindBinder("Samp_GVignette");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
        }
    }
    public class UFinalCopyNode : Common.USceenSpaceNode
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color");
        //public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput("Depth");

        public TtRenderGraphPin PickPinIn = TtRenderGraphPin.CreateInput("Pick");
        public TtRenderGraphPin VignettePinIn = TtRenderGraphPin.CreateInput("Vignette");
        public UFinalCopyNode()
        {
            Name = "UFinalCopyNode";
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);
            //AddInput(DepthPinIn);
            AddInput(PickPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(VignettePinIn, NxRHI.EBufferType.BFT_SRV);

            base.InitNodePins();
            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            //result by base
        }
        public UFinalCopyShading mBasePassShading;
        public override UGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UFinalCopyShading>();
        }
    }
}
