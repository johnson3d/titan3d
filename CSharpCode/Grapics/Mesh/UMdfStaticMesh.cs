using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    public class UMdfStaticMesh : Graphics.Pipeline.Shader.UMdfQueue
    {
        public Mesh.Modifier.CStaticModifier StaticModifier { get; set; }
        public UMdfStaticMesh()
        {
            StaticModifier = new Mesh.Modifier.CStaticModifier();
            unsafe
            {
                mCoreObject.PushModifier(StaticModifier.mCoreObject.CastToSuper().CppPointer);
            }

            UpdateShaderCode();
        }
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_INPUT input)");
            codeBuilder.PushBrackets();
            {

            }
            codeBuilder.PopBrackets();

            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION");

            SourceCode = new IO.CMemStreamWriter();
            SourceCode.SetText(codeBuilder.ClassCode);
        }
    }
}
