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
        public unsafe override void OnBuildDrawCall(RHI.CDrawCall drawcall)
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
        public unsafe override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, IRenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            var Manager = policy.TagObject as Mobile.UMobileEditorFSPolicy;

            var gpuProgram = drawcall.Effect.ShaderProgram;
            var index = drawcall.mCoreObject.FindSRVIndex("SourceTexture");
            drawcall.mCoreObject.BindSRVAll(index, Manager.PickedNode.PickedBuffer.GBufferSRV[0].mCoreObject);
        }
    }    
    public class UPickBlurNode : USceenSpaceNode
    {
        public override async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat rtFmt, EPixelFormat dsFmt, float x, float y)
        {
            await base.Initialize(policy, shading, rtFmt, dsFmt, x, y);
        }
    }
}
