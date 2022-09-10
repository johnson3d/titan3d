using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader.CommanShading
{
    public class USlateGUIShading : UShadingEnv
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
        public unsafe override void OnBuildDrawCall(URenderPolicy policy, NxRHI.UGraphicDraw drawcall)
        {
            //var gpuProgram = drawcall.Effect.ShaderEffect;
            //var cbIndex = drawcall.mCoreObject.FindCBufferIndex("cbPerShadingEnv");
            //if (cbIndex != 0xFFFFFFFF)
            //{
            //    if (PerShadingCBuffer == null)
            //    {
            //        PerShadingCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(gpuProgram, cbIndex);
            //        PerShadingCBuffer.SetMatrix(0, in Matrix.Identity);
            //        var RenderColor = new Color4(1, 1, 1, 1);
            //        PerShadingCBuffer.SetValue(1, in RenderColor);
            //    }
            //    drawcall.mCoreObject.BindCBufferAll(cbIndex, PerShadingCBuffer.mCoreObject);
            //}
        }
    }
    public class UCopy2DShading : UShadingEnv
    {
        public UCopy2DShading()
        {
            CodeName = RName.GetRName("shaders/shadingenv/sys/copy2d.shadingenv", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(URenderPolicy policy, NxRHI.UGraphicDraw drawcall)
        {
            var gpuProgram = drawcall.mCoreObject.GetShaderEffect();
            var cbIndex = gpuProgram.FindBinder("cbPerShadingEnv");
            if (cbIndex.IsValidPointer)
            {
                if (PerShadingCBuffer == null)
                {
                    PerShadingCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(cbIndex.GetShaderBinder(NxRHI.EShaderType.SDT_Unknown));
                    PerShadingCBuffer.SetValue(cbIndex.FindField("RenderMatrix"), in Matrix.Identity);
                    var RenderColor = new Color4(1, 1, 1, 1);
                    PerShadingCBuffer.SetValue(cbIndex.FindField("RenderColor"), in RenderColor);
                }
                drawcall.BindCBuffer(cbIndex, PerShadingCBuffer);
            }
        }
        public unsafe void OnDrawCall(NxRHI.UGraphicDraw drawcall, UCopy2DPolicy policy, Mesh.UMesh mesh)
        {
            var index = drawcall.ShaderEffect.FindBinder("SourceTexture");
            if (index.IsValidPointer)
            {
                drawcall.BindSRV(index, policy.ViewPolicy.GetFinalShowRSV());
            }

            index = drawcall.ShaderEffect.FindBinder("Samp_SourceTexture");
            if (index.IsValidPointer)
            {
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
            }
            //var rsv = mesh.Tag as NxRHI.USrView;
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
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(URenderPolicy policy, NxRHI.UGraphicDraw drawcall)
        {
            var gpuProgram = drawcall.ShaderEffect;
            var cbIndex = gpuProgram.FindBinder("cbPerShadingEnv");
            if (cbIndex.IsValidPointer)
            {
                if (PerShadingCBuffer == null)
                {
                    PerShadingCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(cbIndex.GetShaderBinder(NxRHI.EShaderType.SDT_Unknown));
                    PerShadingCBuffer.SetValue(cbIndex.FindField("RenderMatrix"), in Matrix.Identity);
                    var RenderColor = new Color4(1, 1, 1, 1);
                    PerShadingCBuffer.SetValue(cbIndex.FindField("RenderColor"), in RenderColor);
                }
                drawcall.BindCBuffer(cbIndex, PerShadingCBuffer);
            }
        }
    }
}
