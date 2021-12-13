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
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position,
                EVertexSteamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(IRenderPolicy policy, RHI.CDrawCall drawcall)
        {
            //var gpuProgram = drawcall.Effect.ShaderProgram;
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
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position,
                EVertexSteamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(IRenderPolicy policy, RHI.CDrawCall drawcall)
        {
            var gpuProgram = drawcall.Effect.ShaderProgram;
            var cbIndex = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerShadingEnv");
            if (!CoreSDK.IsNullPointer(cbIndex))
            {
                if (PerShadingCBuffer == null)
                {
                    PerShadingCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(gpuProgram, "cbPerShadingEnv");
                    PerShadingCBuffer.SetMatrix(0, in Matrix.Identity);
                    var RenderColor = new Color4(1, 1, 1, 1);
                    PerShadingCBuffer.SetValue(1, in RenderColor);
                }
                drawcall.mCoreObject.BindShaderCBuffer(cbIndex, PerShadingCBuffer.mCoreObject);
            }
        }
        public unsafe void OnDrawCall(RHI.CDrawCall drawcall, UCopy2DPolicy policy, Mesh.UMesh mesh)
        {
            var index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "SourceTexture");
            if (!CoreSDK.IsNullPointer(index))
            {
                drawcall.mCoreObject.BindShaderSrv(index, policy.ViewPolicy.GetFinalShowRSV().mCoreObject);
            }

            index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_SourceTexture");
            if (!CoreSDK.IsNullPointer(index))
            {
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);
            }
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
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position,
                EVertexSteamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(IRenderPolicy policy, RHI.CDrawCall drawcall)
        {
            var gpuProgram = drawcall.Effect.ShaderProgram;
            var cbIndex = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerShadingEnv");
            if (CoreSDK.IsNullPointer(cbIndex) != false)
            {
                if (PerShadingCBuffer == null)
                {
                    PerShadingCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(gpuProgram, "cbPerShadingEnv");
                    PerShadingCBuffer.SetMatrix(0, in Matrix.Identity);
                    var RenderColor = new Color4(1, 1, 1, 1);
                    PerShadingCBuffer.SetValue(1, in RenderColor);
                }
                drawcall.mCoreObject.BindShaderCBuffer(cbIndex, PerShadingCBuffer.mCoreObject);
            }
        }
    }
}
