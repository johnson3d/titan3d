using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Bricks.Terrain.Grass
{
    public class UMdfGrassStaticMesh : Graphics.Pipeline.Shader.TtMdfQueue1<TtGrassModifier>
    {
        public TtGrassModifier GrassModifier => this.Modifiers[0] as TtGrassModifier;
    }

    public class UMdf_Grass_VertexFollowHeight { }
    public class UMdf_Grass_VertexNotFollowHeight : UMdf_Grass_VertexFollowHeight { }

    public class UMdfGrassStaticMeshPermutation<TGrassFollowHeight> : UMdfGrassStaticMesh 
        where TGrassFollowHeight : UMdf_Grass_VertexFollowHeight
    {
        protected override void PreBuildMdfFunctions(ref string codeString, Bricks.CodeBuilder.Backends.UHLSLCodeGenerator codeBuilder)
        {
            if (typeof(TGrassFollowHeight) == typeof(UMdf_Grass_VertexFollowHeight))
            {
                codeBuilder.AddLine("#define GRASS_VERTEXFOLLOWHEIGHT 1", ref codeString);
            }
            else if (typeof(TGrassFollowHeight) == typeof(UMdf_Grass_VertexNotFollowHeight))
            {

            }
        }
    }

}
