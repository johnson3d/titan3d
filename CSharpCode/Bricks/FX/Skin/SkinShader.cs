using System;
using System.Collections.Generic;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.ShaderNode;
using EngineNS.Bricks.CodeBuilder.ShaderNode.Control;

namespace EngineNS.Bricks.FX.Skin
{
    [Rtti.Meta]
    [TtMaterialShader]
    public partial class TtSkinShader
    {
        [Rtti.Meta]
        [TtMaterialShader(Name = "Lut3S")]
        [ContextMenu(filterStrings: "Lut3S", "FX\\Skin\\Lut3S", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Lut3S(CodeBuilder.ShaderNode.Var.Texture2D LutTex, float NoL, float Curvature, out Vector3 OutColor)
        {
            OutColor = Vector3.Zero;
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "CalcCurvature")]
        [ContextMenu(filterStrings: "CalcCurvature", "FX\\Skin\\CalcCurvature", TtMaterialGraph.MaterialEditorKeyword)]
        public static float CalcCurvature(CodeBuilder.ShaderNode.Var.Texture2D normMap, Graphics.Pipeline.Shader.PS_INPUT input, float norBias)
        {
            return 0;
        }
    }
}
