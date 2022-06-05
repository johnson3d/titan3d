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
        public override EVertexStreamType[] GetNeedStreams()
        {
            return null;
        }
        protected override string GetBaseBuilder(Bricks.CodeBuilder.Backends.UHLSLCodeGenerator codeBuilder)
        {
            string codeString = "";
            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_INPUT input)", ref codeString);
            codeBuilder.PushSegment(ref codeString);
            {

            }
            codeBuilder.PopSegment(ref codeString);

            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION", ref codeString);

            SourceCode = new IO.CMemStreamWriter();
            SourceCode.SetText(codeString);
            return codeString;
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
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            var codeString = GetBaseBuilder(codeBuilder);
            
            if (typeof(PermutationType).Name == "UMdf_NoShadow")
            {
                codeBuilder.AddLine("#define DISABLE_SHADOW_MDFQUEUE 1", ref codeString);
            }
            else if (typeof(PermutationType).Name == "UMdf_Shadow")
            {

            }

            SourceCode.SetText(codeString);
        }
    }

    public class UMdfInstanceStaticMesh : Graphics.Pipeline.Shader.UMdfQueue
    {
        public UMdfInstanceStaticMesh()
        {
            mInstanceModifier = new Modifier.UInstanceModifier();
            mInstanceModifier.SetMode(true);
            UpdateShaderCode();
        }
        Modifier.UInstanceModifier mInstanceModifier;
        public Modifier.UInstanceModifier InstanceModifier
        {
            get => mInstanceModifier;
        }
        public void SetInstantMode(bool bSSBO = true)
        {
            mInstanceModifier.SetMode(bSSBO);
        }
        public override EVertexStreamType[] GetNeedStreams()
        {
            return new EVertexStreamType[] { 
                EVertexStreamType.VST_Position,
                EVertexStreamType.VST_Normal,
                EVertexStreamType.VST_InstPos, 
                EVertexStreamType.VST_InstQuat,
                EVertexStreamType.VST_InstScale,
                EVertexStreamType.VST_F4_1};
        }
        protected override string GetBaseBuilder(Bricks.CodeBuilder.Backends.UHLSLCodeGenerator codeBuilder)
        {
            string codeString = "";
            var mdfSourceName = RName.GetRName("shaders/modifier/InstancingModifier.cginc", RName.ERNameType.Engine);
            codeBuilder.AddLine($"#include \"{mdfSourceName.Address}\"", ref codeString);
            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_INPUT input)", ref codeString);
            codeBuilder.PushSegment(ref codeString);
            {
                codeBuilder.AddLine($"DoInstancingModifierVS(output, input);", ref codeString);
            }
            codeBuilder.PopSegment(ref codeString);

            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION", ref codeString);

            var code = Editor.ShaderCompiler.UShaderCodeManager.Instance.GetShaderCodeProvider(mdfSourceName);
            codeBuilder.AddLine($"//Hash for {mdfSourceName}:{UniHash.APHash(code.SourceCode.AsText)}", ref codeString);

            SourceCode = new IO.CMemStreamWriter();
            SourceCode.SetText(codeString);

            return codeString;
        }
        public override void OnDrawCall(URenderPolicy.EShadingType shadingType, CDrawCall drawcall, URenderPolicy policy, UMesh mesh)
        {
            mInstanceModifier?.OnDrawCall(shadingType, drawcall, policy, mesh);
        }
    }

    public class UMdfInstanceStaticMeshPermutation<PermutationType> : UMdfInstanceStaticMesh
    {
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            var codeString = GetBaseBuilder(codeBuilder);

            if (typeof(PermutationType).Name == "UMdf_NoShadow")
            {
                codeBuilder.AddLine("#define DISABLE_SHADOW_MDFQUEUE 1", ref codeString);
            }
            else if (typeof(PermutationType).Name == "UMdf_Shadow")
            {

            }

            SourceCode.SetText(codeString);
        }
    }
}
