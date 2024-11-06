using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.ShaderNode;
using EngineNS.Bricks.CodeBuilder.ShaderNode.Control;


namespace EngineNS.Bricks.FX.Hair
{
    [Rtti.Meta]
    [TtMaterialShader]
    public partial class TtHairShader
    {
        [Rtti.Meta]
        [TtMaterialShader(Name = "StrandSpecular", Include = "@Engine/Shaders/Bricks/FX/Hair.cginc")]
        [ContextMenu("StrandSpecular", "FX\\StrandSpecular", TtMaterialGraph.MaterialEditorKeyword)]
        public float StrandSpecular(Vector3 T, Vector3 V, Vector3 L, float exponent)
        {
            return 0;
        }
    }
}
