using EngineNS.Graphics.Pipeline;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;

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
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return null;
        }
        protected override string GetBaseBuilder(Bricks.CodeBuilder.Backends.UHLSLCodeGenerator codeBuilder)
        {
            string codeString = "";
            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_MODIFIER input)", ref codeString);
            codeBuilder.PushSegment(ref codeString);
            {

            }
            codeBuilder.PopSegment(ref codeString);

            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION", ref codeString);

            SourceCode = new NxRHI.UShaderCode();
            SourceCode.TextCode = codeString;
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

            SourceCode.TextCode = codeString;
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
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] {
                NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal,
                NxRHI.EVertexStreamType.VST_InstPos,
                NxRHI.EVertexStreamType.VST_InstQuat,
                NxRHI.EVertexStreamType.VST_InstScale,
                NxRHI.EVertexStreamType.VST_F4_1};
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_SpecialData,
            };
        }
        protected override string GetBaseBuilder(Bricks.CodeBuilder.Backends.UHLSLCodeGenerator codeBuilder)
        {
            string codeString = "";
            var mdfSourceName = RName.GetRName("shaders/modifier/InstancingModifier.cginc", RName.ERNameType.Engine);
            codeBuilder.AddLine($"#include \"{mdfSourceName.Address}\"", ref codeString);
            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_MODIFIER input)", ref codeString);
            codeBuilder.PushSegment(ref codeString);
            {
                codeBuilder.AddLine($"DoInstancingModifierVS(output, input);", ref codeString);
            }
            codeBuilder.PopSegment(ref codeString);

            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION", ref codeString);

            var code = Editor.ShaderCompiler.UShaderCodeManager.Instance.GetShaderCodeProvider(mdfSourceName);
            codeBuilder.AddLine($"//Hash for {mdfSourceName}:{UniHash32.APHash(code.SourceCode.TextCode)}", ref codeString);

            SourceCode = new NxRHI.UShaderCode();
            SourceCode.TextCode = codeString;

            return codeString;
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, UMesh mesh, int atom)
        {
            base.OnDrawCall(cmd, shadingType, drawcall, policy, mesh, atom);
            mInstanceModifier?.OnDrawCall(cmd, shadingType, drawcall, policy, mesh);
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

            SourceCode.TextCode = codeString;
        }
    }
}
