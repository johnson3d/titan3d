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
		private static EngineNS.Macross.TtMacrossBreak macross_break_Lut3S_1833416868 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.FX.Skin.TtSkinShader->static void Lut3S(CodeBuilder.ShaderNode.Var.Texture2D LutTex, float NoL, float Curvature, out Vector3 OutColor)");
		public static unsafe void macross_Lut3S (string nodeName, CodeBuilder.ShaderNode.Var.Texture2D LutTex, float NoL, float Curvature, out Vector3 OutColor) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":LutTex", LutTex);
					stackframe.SetWatchVariable(nodeName + ":NoL", NoL);
					stackframe.SetWatchVariable(nodeName + ":Curvature", Curvature);
				}
			}
			Lut3S(LutTex, NoL, Curvature, out OutColor);
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":OutColor", OutColor);
				}
			}
			macross_break_Lut3S_1833416868.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_CalcCurvature_264513993 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.FX.Skin.TtSkinShader->static float CalcCurvature(CodeBuilder.ShaderNode.Var.Texture2D normMap, Graphics.Pipeline.Shader.PS_INPUT input, float norBias)");
		public static unsafe float macross_CalcCurvature (string nodeName, CodeBuilder.ShaderNode.Var.Texture2D normMap, Graphics.Pipeline.Shader.PS_INPUT input, float norBias) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":normMap", normMap);
					stackframe.SetWatchVariable(nodeName + ":input", input);
					stackframe.SetWatchVariable(nodeName + ":norBias", norBias);
				}
			}
			var _return_value = CalcCurvature(normMap, input, norBias);
			macross_break_CalcCurvature_264513993.TryBreak();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross