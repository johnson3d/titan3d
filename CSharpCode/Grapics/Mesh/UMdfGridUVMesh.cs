using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Graphics.Mesh
{
    public class UMdfGridUVMesh : Graphics.Pipeline.Shader.UMdfQueue
    {
        public NxRHI.UCbView PerGridUVMeshCBuffer { get; set; }
        public void SetUVMinAndMax(in Vector2 min, in Vector2 max)
        {
            if (PerGridUVMeshCBuffer == null)
                return;
            PerGridUVMeshCBuffer.SetValue("UVMin", in min);
            PerGridUVMeshCBuffer.SetValue("UVMax", in max);
        }
        public UMdfGridUVMesh()
        {
            UpdateShaderCode();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,
                NxRHI.EVertexStreamType.VST_LightMap};
        }
        public override void CopyFrom(UMdfQueue mdf)
        {
            base.CopyFrom(mdf);
            PerGridUVMeshCBuffer = (mdf as UMdfGridUVMesh).PerGridUVMeshCBuffer;
        }
        protected override string GetBaseBuilder(Bricks.CodeBuilder.Backends.UHLSLCodeGenerator codeBuilder)
        {
            var codeString = "";
            var mdfSourceName = RName.GetRName("shaders/modifier/GridUVModifier.cginc", RName.ERNameType.Engine);
            codeBuilder.AddLine($"#include \"{mdfSourceName.Address}\"", ref codeString);
            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_MODIFIER input)", ref codeString);
            codeBuilder.PushSegment(ref codeString);
            {
                codeBuilder.AddLine("DoGridUVModifierVS(output, input);", ref codeString);
            }
            codeBuilder.PopSegment(ref codeString);

            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION", ref codeString);

            var code = Editor.ShaderCompiler.UShaderCodeManager.Instance.GetShaderCodeProvider(mdfSourceName);
            codeBuilder.AddLine($"//Hash for {mdfSourceName}:{UniHash32.APHash(code.SourceCode.TextCode)}", ref codeString);

            SourceCode = new NxRHI.UShaderCode();
            SourceCode.TextCode = codeString;
            return codeString;
        }
        public override void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Pipeline.URenderPolicy policy, Mesh.UMesh mesh, int atom)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh, atom);
            unsafe
            {
                var binder = drawcall.FindBinder("cbGridUVMesh");
                if (binder.IsValidPointer == false)
                {
                    return;
                }
                if (PerGridUVMeshCBuffer == null)
                {
                    PerGridUVMeshCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                }
                drawcall.BindCBuffer(binder, PerGridUVMeshCBuffer);
            }
        }
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            var codeString = GetBaseBuilder(codeBuilder);

            SourceCode.TextCode = codeString;
        }
    }
}
