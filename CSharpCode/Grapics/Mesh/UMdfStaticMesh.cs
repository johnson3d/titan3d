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
                mCoreObject.PushModifier(StaticModifier.mCoreObject.NativeSuper);
            }

            UpdateShaderCode();
        }
        public override System.Type GetBaseMdfQueue()
        {
            return typeof(UMdfStaticMesh);
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

    public class UMdfStaticMesh_NoShadow : UMdfStaticMesh
    {
        public UMdfStaticMesh_NoShadow()
        {

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
            codeBuilder.AddLine("#define DISABLE_SHADOW_MDFQUEUE 1");

            SourceCode = new IO.CMemStreamWriter();
            SourceCode.SetText(codeBuilder.ClassCode);
        }
    }
}
