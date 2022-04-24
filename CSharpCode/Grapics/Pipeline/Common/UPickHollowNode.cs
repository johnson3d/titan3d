using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UPickHollowShading : Shader.UShadingEnv
    {
        public UPickHollowShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/pick/pick_hollow.cginc", RName.ERNameType.Engine);
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
            var pickHollowNode = Manager.FindFirstNode<Common.UPickHollowNode>();
            
            var index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "gPickedSetUpTex");
            if (!CoreSDK.IsNullPointer(index))
            {
                var attachBuffer = pickHollowNode.GetAttachBuffer(pickHollowNode.PickedPinIn);
                drawcall.mCoreObject.BindShaderSrv(index, attachBuffer.Srv.mCoreObject);
            }
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_gPickedSetUpTex");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "gPickedBlurTex");
            if (!CoreSDK.IsNullPointer(index))
            {
                var attachBuffer = pickHollowNode.GetAttachBuffer(pickHollowNode.BlurPinIn);
                drawcall.mCoreObject.BindShaderSrv(index, attachBuffer.Srv.mCoreObject);
            }
            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_gPickedBlurTex");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);
        }
    }
    public class UPickHollowNode : USceenSpaceNode
    {
        public Common.URenderGraphPin PickedPinIn = Common.URenderGraphPin.CreateInput("Picked");
        public Common.URenderGraphPin BlurPinIn = Common.URenderGraphPin.CreateInput("Blur");
        public UPickHollowNode()
        {
            Name = "PickHollowNode";
        }
        public override void InitNodePins()
        {
            AddInput(PickedPinIn, EGpuBufferViewType.GBVT_Srv);
            AddInput(BlurPinIn, EGpuBufferViewType.GBVT_Srv);
            
            ResultPinOut.IsAutoResize = false;
            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16G16_FLOAT;
            base.InitNodePins();
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UPickHollowShading>();
        }
        public override void OnLinkIn(URenderGraphLinker linker)
        {
            //ResultPinOut.Attachement.Format = PickedPinIn.Attachement.Format;
        }
        public override void FrameBuild()
        {
            
        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            float scaleFactor = 1.0f;
            var hitProxyNode = policy.FindFirstNode<UHitproxyNode>();
            if (hitProxyNode != null)
            {
                scaleFactor = hitProxyNode.ScaleFactor;
            }
            ResultPinOut.Attachement.Width = (uint)(x * scaleFactor);
            ResultPinOut.Attachement.Height = (uint)(y * scaleFactor);

            base.OnResize(policy, x * scaleFactor, y * scaleFactor);
        }
    }
}
