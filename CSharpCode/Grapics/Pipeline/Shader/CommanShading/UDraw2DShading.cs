using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader.CommanShading
{
    public class UCopy2DShading : UShadingEnv
    {
        public UCopy2DShading()
        {
            CodeName = RName.GetRName("shaders/shadingenv/sys/copy2d.shadingenv", RName.ERNameType.Engine);
        }
        public RHI.CConstantBuffer PerShadingCBuffer;
        public unsafe override void OnBuildDrawCall(RHI.CDrawCall drawcall)
        {
            var gpuProgram = drawcall.Effect.ShaderProgram;
            var cbIndex = drawcall.mCoreObject.FindCBufferIndex("cbPerShadingEnv");
            if (cbIndex != 0xFFFFFFFF)
            {
                if (PerShadingCBuffer == null)
                {
                    PerShadingCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(gpuProgram, cbIndex);
                    PerShadingCBuffer.SetMatrix(0, ref Matrix.mIdentity);
                    var RenderColor = new Color4(1, 1, 1, 1);
                    PerShadingCBuffer.SetValue(1, ref RenderColor);
                }
                drawcall.mCoreObject.BindCBufferAll(cbIndex, PerShadingCBuffer.mCoreObject.Ptr);
            }
        }
        public unsafe void OnDrawCall(RHI.CDrawCall drawcall, UCopy2DPolicy policy, Mesh.UMesh mesh)
        {
            var index = drawcall.mCoreObject.FindSRVIndex("SourceTexture");

            drawcall.mCoreObject.BindSRVAll(index, policy.ViewPolicy.GetFinalShowRSV().mCoreObject.Ptr);
            //var rsv = mesh.Tag as RHI.CShaderResourceView;
            //if (rsv != null)
            //{
            //    drawcall.mCoreObject.BindSRVAll(index, rsv.mCoreObject.Ptr); 
            //}
        }
    }
    public class URect2DShading : UShadingEnv
    {
        public URect2DShading()
        {
            CodeName = RName.GetRName("shaders/shadingenv/sys/rect2d.shadingenv", RName.ERNameType.Engine);
        }
        public RHI.CConstantBuffer PerShadingCBuffer;
        public unsafe override void OnBuildDrawCall(RHI.CDrawCall drawcall)
        {
            var gpuProgram = drawcall.Effect.ShaderProgram;
            var cbIndex = drawcall.mCoreObject.FindCBufferIndex("cbPerShadingEnv");
            if (cbIndex != 0xFFFFFFFF)
            {
                if (PerShadingCBuffer == null)
                {
                    PerShadingCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(gpuProgram, cbIndex);
                    PerShadingCBuffer.SetMatrix(0, ref Matrix.mIdentity);
                    var RenderColor = new Color4(1, 1, 1, 1);
                    PerShadingCBuffer.SetValue(1, ref RenderColor);
                }
                drawcall.mCoreObject.BindCBufferAll(cbIndex, PerShadingCBuffer.mCoreObject.Ptr);
            }
        }
    }
}
