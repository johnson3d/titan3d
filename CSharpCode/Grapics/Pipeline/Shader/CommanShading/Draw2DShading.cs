using EngineNS.Graphics.Mesh;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader.CommanShading
{
    public class USlateGUIShading : TtGraphicsShadingEnv
    {
        public USlateGUIShading()
        {
            CodeName = RName.GetRName("shaders/slate/slategui.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
                EPixelShaderInput.PST_UV,
                EPixelShaderInput.PST_Color,
            };
        }
        public unsafe override void OnBuildDrawCall(TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall)
        {
            //var gpuProgram = drawcall.Effect.GraphicsEffect;
            //var cbIndex = drawcall.mCoreObject.FindCBufferIndex("cbPerShadingEnv");
            //if (cbIndex != 0xFFFFFFFF)
            //{
            //    if (PerShadingCBuffer == null)
            //    {
            //        PerShadingCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(gpuProgram, cbIndex);
            //        PerShadingCBuffer.SetMatrix(0, in Matrix.Identity);
            //        var RenderColor = new Color4f(1, 1, 1, 1);
            //        PerShadingCBuffer.SetValue(1, in RenderColor);
            //    }
            //    drawcall.mCoreObject.BindCBufferAll(cbIndex, PerShadingCBuffer.mCoreObject);
            //}
        }
    }
    public class UDrawViewportShading : TtGraphicsShadingEnv
    {
        public UDrawViewportShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/DrawViewportShading.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
                EPixelShaderInput.PST_Normal,
                EPixelShaderInput.PST_UV,
                EPixelShaderInput.PST_Color,
            };
        }
        public unsafe override void OnBuildDrawCall(TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall)
        {
            
        }
    }
}
