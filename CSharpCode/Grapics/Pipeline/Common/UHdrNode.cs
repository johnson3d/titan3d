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
        public override EVertexStreamType[] GetNeedStreams()
        {
            return new EVertexStreamType[] { EVertexStreamType.VST_Position,
                EVertexStreamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(URenderPolicy policy, RHI.CDrawCall drawcall)
        {
        }
        public unsafe override void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, URenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            var Manager = policy.TagObject as URenderPolicy;
            var node = Manager.FindFirstNode<UHdrNode>();
            var lightSRV = node.GetAttachBuffer(node.ColorPinIn).Srv;
            var gpuSceneDescSRV = node.GetAttachBuffer(node.GpuScenePinIn).Srv;
            
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
        public Common.URenderGraphPin ColorPinIn = Common.URenderGraphPin.CreateInput("Color");
        public Common.URenderGraphPin GpuScenePinIn = Common.URenderGraphPin.CreateInput("GpuScene");
        public UHdrNode()
        {
            Name = "HdrNode";            
        }
        public override void Cleanup()
        {
            base.Cleanup();
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn, EGpuBufferViewType.GBVT_Srv);
            AddInput(GpuScenePinIn, EGpuBufferViewType.GBVT_Srv);

            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            base.InitNodePins();
            //base Result
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UHdrShading>();
        }
    }
}
