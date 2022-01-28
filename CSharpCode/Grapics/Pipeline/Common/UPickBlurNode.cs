using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UPickBlurShading : Shader.UShadingEnv
    {
        public UPickBlurShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/pick/pick_blur.cginc", RName.ERNameType.Engine);
        }
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position,
                EVertexSteamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(URenderPolicy policy, RHI.CDrawCall drawcall)
        {
            //var cbIndex = drawcall.mCoreObject.FindCBufferIndex("cbPerShadingEnv");
            //if (cbIndex != 0xFFFFFFFF)
            //{
            //    if (PerShadingCBuffer == null)
            //    {
            //        PerShadingCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(gpuProgram, cbIndex);
            //        PerShadingCBuffer.SetMatrix(0, ref Matrix.mIdentity);
            //        var RenderColor = new Color4(1, 1, 1, 1);
            //        PerShadingCBuffer.SetValue(1, ref RenderColor);
            //    }
            //    drawcall.mCoreObject.BindCBufferAll(cbIndex, PerShadingCBuffer.mCoreObject.Ptr);
            //}
        }
        public unsafe override void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, URenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            var Manager = policy.TagObject as URenderPolicy;

            var pickBlurNode = Manager.FindFirstNode<Common.UPickBlurNode>();

            var gpuProgram = drawcall.Effect.ShaderProgram;
            var index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "SourceTexture");
            if (!CoreSDK.IsNullPointer(index))
            {
                var attachBuffer = pickBlurNode.GetAttachBuffer(pickBlurNode.PickedPinIn);
                drawcall.mCoreObject.BindShaderSrv(index, attachBuffer.Srv.mCoreObject);
            }

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_SourceTexture");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);
        }
    }    
    public class UPickBlurNode : USceenSpaceNode
    {
        public Common.URenderGraphPin PickedPinIn = Common.URenderGraphPin.CreateInput("Picked");
        public UPickBlurNode()
        {
            Name = "PickBlurNode";            
        }
        public override void InitNodePins()
        {
            AddInput(PickedPinIn, EGpuBufferViewType.GBVT_Srv);
            
            ResultPinOut.IsAutoResize = false;
            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16G16_FLOAT;
            base.InitNodePins();
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UPickBlurShading>();
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
            ResultPinOut.Attachement.Width = (uint)(x * UHitproxyNode.ScaleFactor);
            ResultPinOut.Attachement.Height = (uint)(y * UHitproxyNode.ScaleFactor);

            base.OnResize(policy, x * UHitproxyNode.ScaleFactor, y * UHitproxyNode.ScaleFactor);
        }
    }
}
