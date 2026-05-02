using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class TtFinalCopyShading : Shader.TtGraphicsShadingEnv
    {
        public TtPermutationItem DisableAO
        {
            get;
            set;
        }
        public TtPermutationItem DisablePointLights
        {
            get;
            set;
        }
        public TtPermutationItem DisableShadow
        {
            get;
            set;
        }
        public TtPermutationItem DisableSunshaft
        {
            get;
            set;
        }
        public TtPermutationItem DisableBloom
        {
            get;
            set;
        }
        public TtPermutationItem DisableHdr
        {
            get;
            set;
        }
        public TtFinalCopyShading()
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

            this.UpdatePermutation().AddWaitTask();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public void SetDisableAO(bool value)
        {
            DisableAO.SetValue(value);
            UpdatePermutation().AddWaitTask();
        }
        public void SetDisableSunShaft(bool value)
        {
            DisableSunshaft.SetValue(value);
            UpdatePermutation().AddWaitTask();
        }
        public void SetDisableBloom(bool value)
        {
            DisableBloom.SetValue(value);
            UpdatePermutation().AddWaitTask();
        }
        public void SetDisableHDR(bool value)
        {
            DisableHdr.SetValue(value);
            UpdatePermutation().AddWaitTask();
        }
        public unsafe override void OnBuildDrawCall(TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall)
        {
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtRenderMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var Manager = policy.TagObject as TtRenderPolicy;

            var node = Manager.FindFirstNode<TtFinalCopyNode>();
            var index = drawcall.FindBinder("gBaseSceneView");
            if (index.IsValidPointer)
            {
                drawcall.BindSRV(index, node.GetAttachBuffer(node.ColorPinIn).Srv);
            }
            index = drawcall.FindBinder("Samp_gBaseSceneView");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = drawcall.FindBinder("gPickedTex");
            if (index.IsValidPointer)
            {
                drawcall.BindSRV(index, node.GetAttachBuffer(node.PickPinIn).Srv);
            }
            index = drawcall.FindBinder("Samp_gPickedTex");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = drawcall.FindBinder("GVignette");
            if (index.IsValidPointer)
            {
                drawcall.BindSRV(index, node.GetAttachBuffer(node.VignettePinIn).Srv);
            }
            index = drawcall.FindBinder("Samp_GVignette");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("FinalCopy", "Mobile\\FinalCopy", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtFinalCopyNode : Common.TAuxSceenSpaceNode<TtFinalCopyNode>
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color", NxRHI.EBufferType.BFT_SRV);
        //public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput("Depth");

        public TtRenderGraphPin PickPinIn = TtRenderGraphPin.CreateInput("Pick", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin VignettePinIn = TtRenderGraphPin.CreateInput("Vignette", NxRHI.EBufferType.BFT_SRV);
        public TtFinalCopyNode()
        {
            Name = "UFinalCopyNode";
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn);
            //AddInput(DepthPinIn);
            AddInput(PickPinIn);
            AddInput(VignettePinIn);

            base.InitNodePins();
            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            //result by base
        }
        public TtFinalCopyShading mBasePassShading;
        public override TtGraphicsShadingEnv GetPassShading(TtRenderMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtFinalCopyShading>();
        }
    }
}
