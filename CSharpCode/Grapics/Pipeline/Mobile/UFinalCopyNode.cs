using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class UFinalCopyShading : Shader.UShadingEnv
    {
        public UFinalCopyShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Mobile/MobileCopyEditor.cginc", RName.ERNameType.Engine);

            var disable_AO = new MacroDefine();//0
            disable_AO.Name = "ENV_DISABLE_AO";
            disable_AO.Values.Add("0");
            disable_AO.Values.Add("1");
            MacroDefines.Add(disable_AO);

            var disable_Sunshaft = new MacroDefine();//1
            disable_Sunshaft.Name = "ENV_DISABLE_SUNSHAFT";
            disable_Sunshaft.Values.Add("0");
            disable_Sunshaft.Values.Add("1");
            MacroDefines.Add(disable_Sunshaft);

            var disable_Bloom = new MacroDefine();//2
            disable_Bloom.Name = "ENV_DISABLE_BLOOM";
            disable_Bloom.Values.Add("0");
            disable_Bloom.Values.Add("1");
            MacroDefines.Add(disable_Bloom);

            var disable_Hdr = new MacroDefine();//2
            disable_Hdr.Name = "ENV_DISABLE_HDR";
            disable_Hdr.Values.Add("0");
            disable_Hdr.Values.Add("1");
            MacroDefines.Add(disable_Hdr);

            UpdatePermutationBitMask();

            mMacroValues.Add("0");//disable_AO = 0
            mMacroValues.Add("1");//disable_Sunshaft = 1
            mMacroValues.Add("1");//disable_Bloom = 1
            mMacroValues.Add("0");//disable_Hdr = 1

            UpdatePermutation(mMacroValues);
        }
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position,
                EVertexSteamType.VST_UV,};
        }
        public void SetDisableAO(bool value)
        {
            mMacroValues[0] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public void SetDisableSunShaft(bool value)
        {
            mMacroValues[1] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public void SetDisableBloom(bool value)
        {
            mMacroValues[2] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public void SetDisableHDR(bool value)
        {
            mMacroValues[3] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public unsafe override void OnBuildDrawCall(IRenderPolicy policy, RHI.CDrawCall drawcall)
        {
        }
        public unsafe override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, IRenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            var Manager = policy.TagObject as UMobileEditorFSPolicy;

            var gpuProgram = drawcall.Effect.ShaderProgram;
            var index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "gBaseSceneView");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, Manager.GetBasePassNode().GBuffers.GetGBufferSRV(0).mCoreObject);
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_gBaseSceneView");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "gPickedTex");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, Manager.PickHollowNode.GBuffers.GetGBufferSRV(0).mCoreObject);
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_gPickedTex");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GVignette");
            drawcall.mCoreObject.BindShaderSrv(index, Manager.VignetteSRV.mCoreObject);
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_GVignette");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);
        }
    }
    public class UFinalCopyNode : Common.USceenSpaceNode
    {
        public UFinalCopyNode()
        {
            InputGpuBuffers = null;
            InputShaderResourceViews = new Common.URenderGraphSRV[2];
            InputShaderResourceViews[0] = new Common.URenderGraphSRV();
            InputShaderResourceViews[0].Name = "BaseSceneView";

            InputShaderResourceViews[1] = new Common.URenderGraphSRV();
            InputShaderResourceViews[1].Name = "PickedTex";

            OutputGpuBuffers = null;

            OutputShaderResourceViews = new Common.URenderGraphSRV[1];
            OutputShaderResourceViews[0] = new Common.URenderGraphSRV();
            OutputShaderResourceViews[0].Name = "Final";
        }
        public override async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat rtFmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            await base.Initialize(policy, shading, rtFmt, dsFmt, x, y, debugName);

            OutputShaderResourceViews[0].SRV = GBuffers.GetGBufferSRV(0);
        }
    }
}
