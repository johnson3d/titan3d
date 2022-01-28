using EngineNS.Graphics.Pipeline;
using EngineNS.RHI;
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
        public override EVertexSteamType[] GetNeedStreams()
        {
            return null;
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
        public override Rtti.UTypeDesc GetPermutation(List<string> features)
        {
            if (features.Contains("UMdf_NoShadow"))
            {
                return Rtti.UTypeDescGetter<UMdfStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc;
            }
            else
            {
                return Rtti.UTypeDescGetter<UMdfStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_Shadow>>.TypeDesc;
            }
        }
    }

    public class UMdfStaticMeshPermutation<PermutationType> : UMdfStaticMesh
    {
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();
            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_INPUT input)");
            codeBuilder.PushBrackets();
            {

            }
            codeBuilder.PopBrackets();

            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION");
            
            if (typeof(PermutationType).Name == "UMdf_NoShadow")
            {
                codeBuilder.AddLine("#define DISABLE_SHADOW_MDFQUEUE 1");
            }
            else if (typeof(PermutationType).Name == "UMdf_Shadow")
            {

            }

            SourceCode = new IO.CMemStreamWriter();
            SourceCode.SetText(codeBuilder.ClassCode);
        }
    }

    public class UMdfInstanceStaticMesh : Graphics.Pipeline.Shader.UMdfQueue
    {
        public UMdfInstanceStaticMesh()
        {
            mInstanceModifier = new Modifier.UInstanceModifier();
            UpdateShaderCode();
        }
        public Modifier.UInstanceModifier mInstanceModifier;
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position, 
                EVertexSteamType.VST_InstPos, 
                EVertexSteamType.VST_InstQuat,
                EVertexSteamType.VST_InstScale,
                EVertexSteamType.VST_F4_1};
        }
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();
            codeBuilder.AddLine($"#include \"{RName.GetRName("Shaders/Modifier/InstancingModifier.cginc", RName.ERNameType.Engine).Address}\"");
            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_INPUT input)");
            codeBuilder.PushBrackets();
            {
                codeBuilder.AddLine($"DoInstancingModifierVS(output, input);"); 
            }
            codeBuilder.PopBrackets();

            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION");

            SourceCode = new IO.CMemStreamWriter();
            SourceCode.SetText(codeBuilder.ClassCode);
        }
        public override void OnDrawCall(URenderPolicy.EShadingType shadingType, CDrawCall drawcall, URenderPolicy policy, UMesh mesh)
        {
            mInstanceModifier?.OnDrawCall(shadingType, drawcall, policy, mesh);
        }
    }
}
