using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Bricks.Terrain.Grass
{
    public class UMdfGrassStaticMesh : Graphics.Pipeline.Shader.UMdfQueue
    {
        public UMdfGrassStaticMesh()
        {
            mGrassModifier = new UGrassModifier();
            mGrassModifier.SetMode(true);
            UpdateShaderCode();
        }
        public override void Dispose()
        {
            mGrassModifier?.Dispose();
            mGrassModifier = null;
            base.Dispose();
        }
        UGrassModifier mGrassModifier;
        public UGrassModifier GrassModifier => mGrassModifier;
        public void SetInstantMode(bool bSSBO = true)
        {
            mGrassModifier.SetMode(bSSBO);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[]
            {
                NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal,
                NxRHI.EVertexStreamType.VST_Color,
            };
        }
        
        protected override string GetBaseBuilder(Bricks.CodeBuilder.Backends.UHLSLCodeGenerator codeBuilder)
        {
            string codeString = "";
            var mdfSourceName = RName.GetRName("shaders/Bricks/Terrain/GrassModifier.cginc", RName.ERNameType.Engine);
            codeBuilder.AddLine($"#include \"{mdfSourceName.Address}\"", ref codeString);
            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_MODIFIER input)", ref codeString);
            codeBuilder.PushSegment(ref codeString);
            {
                codeBuilder.AddLine($"DoGrassModifierVS(output, input);", ref codeString);
            }
            codeBuilder.PopSegment(ref codeString);

            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION", ref codeString);

            var code = Editor.ShaderCompiler.UShaderCodeManager.Instance.GetShaderCodeProvider(mdfSourceName);
            codeBuilder.AddLine($"//Hash for {mdfSourceName}:{UniHash32.APHash(code.SourceCode.TextCode)}", ref codeString);

            SourceCode = new NxRHI.UShaderCode();
            SourceCode.TextCode = codeString;

            return codeString;
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeOnDrawCall = Profiler.TimeScopeManager.GetTimeScope(typeof(UMdfGrassStaticMesh), nameof(OnDrawCall));
        public override void OnDrawCall(Graphics.Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Graphics.Pipeline.URenderPolicy policy, Graphics.Mesh.UMesh mesh, int atom)
        {
            using (new Profiler.TimeScopeHelper(ScopeOnDrawCall))
            {
                mGrassModifier?.OnDrawCall(shadingType, drawcall, policy, mesh);
            }
            base.OnDrawCall(shadingType, drawcall, policy, mesh, atom);
        }
    }

    public class UMdf_Grass_VertexFollowHeight { }
    public class UMdf_Grass_VertexNotFollowHeight : UMdf_Grass_VertexFollowHeight { }

    public class UMdfGrassStaticMeshPermutation<TShadow, TGrassFollowHeight> : UMdfGrassStaticMesh 
        where TShadow : Graphics.Pipeline.Shader.UMdf_Shadow 
        where TGrassFollowHeight : UMdf_Grass_VertexFollowHeight
    {
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            var codeString = GetBaseBuilder(codeBuilder);

            var tempCode = "";
            if (typeof(TShadow).Name == "UMdf_NoShadow")
            {
                codeBuilder.AddLine("#define DISABLE_SHADOW_MDFQUEUE 1", ref tempCode);
            }
            else if (typeof(TShadow).Name == "UMdf_Shadow")
            {

            }

            if(typeof(TGrassFollowHeight).Name == "UMdf_Grass_VertexFollowHeight")
                codeBuilder.AddLine("#define GRASS_VERTEXFOLLOWHEIGHT 1", ref tempCode);
            else if(typeof(TGrassFollowHeight).Name == "UMdf_Grass_VertexNotFollowHeight")
            {

            }
            codeString = tempCode + codeString;
            SourceCode.TextCode = codeString;
        }
    }

}
