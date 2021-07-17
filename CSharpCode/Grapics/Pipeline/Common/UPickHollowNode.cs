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
        public unsafe override void OnBuildDrawCall(RHI.CDrawCall drawcall)
        {
        }
        public unsafe override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, IRenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            var Manager = policy.TagObject as Mobile.UMobileEditorFSPolicy;
            var gpuProgram = drawcall.Effect.ShaderProgram;
            var index = drawcall.mCoreObject.FindSRVIndex("gPickedSetUpTex");
            drawcall.mCoreObject.BindSRVAll(index, Manager.PickedNode.PickedBuffer.GBufferSRV[0].mCoreObject);

            index = drawcall.mCoreObject.FindSRVIndex("gPickedBlurTex");
            drawcall.mCoreObject.BindSRVAll(index, Manager.PickBlurNode.GBuffers.GBufferSRV[0].mCoreObject);
        }
    }
    public class UPickHollowNode : USceenSpaceNode
    {
        public override async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat rtFmt, EPixelFormat dsFmt, float x, float y)
        {
            await base.Initialize(policy, shading, rtFmt, dsFmt, x, y);
        }
    }
}
