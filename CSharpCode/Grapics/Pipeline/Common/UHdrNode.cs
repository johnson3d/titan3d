using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UHdrShading : Shader.UShadingEnv
    {
        public UHdrShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/screenspace/hdr.cginc", RName.ERNameType.Engine);
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

            var Manager = policy.TagObject as IRenderPolicy;
            var lightSRV = Manager.QuerySRV("LightRT");
            if (lightSRV == null)
                return;
            var gpuSceneDescSRV = Manager.GetGpuSceneNode().GpuSceneDescSRV;
            if (gpuSceneDescSRV == null)
                return;
            var gpuProgram = drawcall.Effect.ShaderProgram;
            var index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GSourceTarget");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, lightSRV.mCoreObject);
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_GSourceTarget");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GpuSceneDescSRV");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, gpuSceneDescSRV.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderCBuffer(index, Manager.GetGpuSceneNode().PerGpuSceneCBuffer.mCoreObject);
        }
    }
    public class UHdrNode : USceenSpaceNode
    {
    }
}
