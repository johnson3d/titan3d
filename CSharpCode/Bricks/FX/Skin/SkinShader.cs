using System;
using System.Collections.Generic;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.ShaderNode;
using EngineNS.Bricks.CodeBuilder.ShaderNode.Control;

namespace EngineNS.Bricks.FX.Skin
{
    [Rtti.Meta("")]
    [TtMaterialShader]
    public partial class TtSkinShader
    {
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Lut3S")]
        [ContextMenu(filterStrings: "Lut3S", "FX\\Skin\\Lut3S", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Lut3S(CodeBuilder.ShaderNode.Var.Texture2D LutTex, float NoL, float Curvature, out Vector3 OutColor)
        {
            OutColor = Vector3.Zero;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "CalcCurvature")]
        [ContextMenu(filterStrings: "CalcCurvature", "FX\\Skin\\CalcCurvature", TtMaterialGraph.MaterialEditorKeyword)]
        public static float CalcCurvature(CodeBuilder.ShaderNode.Var.Texture2D normMap, Graphics.Pipeline.Shader.PS_INPUT input, float norBias)
        {
            return 0;
        }
    }
}



#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Bricks.FX.Skin
{
	partial class TtSkinShader
	{
		public static unsafe void macross_Lut3S (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, CodeBuilder.ShaderNode.Var.Texture2D LutTex, float NoL, float Curvature, out Vector3 OutColor) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			Lut3S(LutTex, NoL, Curvature, out OutColor);
		}
		public static unsafe float macross_CalcCurvature (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, CodeBuilder.ShaderNode.Var.Texture2D normMap, Graphics.Pipeline.Shader.PS_INPUT input, float norBias) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = CalcCurvature(normMap, input, norBias);
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross