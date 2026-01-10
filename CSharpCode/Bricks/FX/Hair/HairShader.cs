using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.ShaderNode;
using EngineNS.Bricks.CodeBuilder.ShaderNode.Control;


namespace EngineNS.Bricks.FX.Hair
{
    [Rtti.Meta("")]
    [TtMaterialShader]
    public partial class TtHairShader
    {
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "ShiftTangent", Include = "@Engine/Shaders/Bricks/FX/Hair.cginc")]
        [ContextMenu("ShiftTangent", "FX\\Hair\\ShiftTangent", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector3 ShiftTangent(Vector3 T, Vector3 N, float shift)
        {
            Vector3 shiftedT = T + (shift * N);
            return Vector3.Normalize(shiftedT);
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "StrandSpecular", Include = "@Engine/Shaders/Bricks/FX/Hair.cginc")]
        [ContextMenu("StrandSpecular", "FX\\Hair\\StrandSpecular", TtMaterialGraph.MaterialEditorKeyword)]
        public static float StrandSpecular(Vector3 T, Vector3 V, Vector3 L, float exponent)
        {
            Vector3 H = Vector3.Normalize(L + V);
            float ToH = Vector3.Dot(T, H);
            float sinTH = MathF.Sqrt(1.0f - ToH * ToH);
            float dirAtten = MathHelper.Lerp(-1.0f, 0.0f, Vector3.Dot(T, H));
            return dirAtten * MathF.Pow(sinTH, exponent);
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "GetAnisotropicNeubelt", Include = "@Engine/Shaders/Bricks/FX/Hair.cginc")]
        [ContextMenu("GetAnisotropicNeubelt", "FX\\Hair\\GetAnisotropicNeubelt", TtMaterialGraph.MaterialEditorKeyword)]
        public static void GetAnisotropicNeubelt(float roughness, float anisotropic, out float ax, out float az)
        {
            float roughnessSq = roughness * roughness;
            ax = roughnessSq;
            az = MathHelper.Lerp(0, roughnessSq, 1 - anisotropic);
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "GetAnisotropicBurley", Include = "@Engine/Shaders/Bricks/FX/Hair.cginc")]
        [ContextMenu("GetAnisotropicBurley", "FX\\Hair\\GetAnisotropicBurley", TtMaterialGraph.MaterialEditorKeyword)]
        public static void GetAnisotropicBurley(float roughness, float anisotropic, out float ax, out float az)
        {
            float aspect = MathF.Sqrt(1.0f - 0.9f * anisotropic);
            float roughnessSq = roughness * roughness;
            ax = roughnessSq / aspect;
            az = roughnessSq * aspect;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "GetAnisotropicKulla", Include = "@Engine/Shaders/Bricks/FX/Hair.cginc")]
        [ContextMenu("GetAnisotropicKulla", "FX\\Hair\\GetAnisotropicKulla", TtMaterialGraph.MaterialEditorKeyword)]
        public static void GetAnisotropicKulla(float roughness, float anisotropic, out float ax, out float az)
        {
            float aspect = MathF.Sqrt(1.0f - 0.9f * anisotropic);
            float roughnessSq = roughness * roughness;
            ax = roughnessSq / aspect;
            az = roughnessSq * aspect;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "D_Beckmann_aniso", Include = "@Engine/Shaders/Bricks/FX/Hair.cginc")]
        [ContextMenu("D_Beckmann_aniso", "FX\\Hair\\D_Beckmann_aniso", TtMaterialGraph.MaterialEditorKeyword)]
        public static float D_Beckmann_aniso(float ax, float az, float NoH, Vector3 H, Vector3 T, Vector3 B)
        {
            float ToH = Vector3.Dot(T, H);
            float BoH = Vector3.Dot(B, H);
            float NoH_Sq = NoH * NoH;
            float d = -(ToH * ToH / (ax * ax) + BoH * BoH / (az * az)) / (NoH_Sq);
            return MathF.Exp(d) / (MathF.PI * ax * az * NoH_Sq * NoH_Sq);
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "D_GGXaniso", Include = "@Engine/Shaders/Bricks/FX/Hair.cginc")]
        [ContextMenu("D_GGXaniso", "FX\\Hair\\D_GGXaniso", TtMaterialGraph.MaterialEditorKeyword)]
        public static float D_GGXaniso(float RoughnessX, float RoughnessZ, float NoH, Vector3 H, Vector3 T, Vector3 B)
        {
            float ax = RoughnessX * RoughnessX;
            float az = RoughnessZ * RoughnessZ;
            float ToH = Vector3.Dot(T, H);
            float BoH = Vector3.Dot(B, H);
            float d = ToH * ToH / (ax * ax) + BoH * BoH / (az * az) + NoH * NoH;
            return 1 / (MathF.PI * ax * az * d * d);
        }
    }
    
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Bricks.FX.Hair
{
	partial class TtHairShader
	{
		public static unsafe Vector3 macross_ShiftTangent (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 T, Vector3 N, float shift) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = ShiftTangent(T, N, shift);
			return _return_value;
		}
		public static unsafe float macross_StrandSpecular (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 T, Vector3 V, Vector3 L, float exponent) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = StrandSpecular(T, V, L, exponent);
			return _return_value;
		}
		public static unsafe void macross_GetAnisotropicNeubelt (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float roughness, float anisotropic, out float ax, out float az) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			GetAnisotropicNeubelt(roughness, anisotropic, out ax, out az);
		}
		public static unsafe void macross_GetAnisotropicBurley (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float roughness, float anisotropic, out float ax, out float az) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			GetAnisotropicBurley(roughness, anisotropic, out ax, out az);
		}
		public static unsafe void macross_GetAnisotropicKulla (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float roughness, float anisotropic, out float ax, out float az) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			GetAnisotropicKulla(roughness, anisotropic, out ax, out az);
		}
		public static unsafe float macross_D_Beckmann_aniso (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float ax, float az, float NoH, Vector3 H, Vector3 T, Vector3 B) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = D_Beckmann_aniso(ax, az, NoH, H, T, B);
			return _return_value;
		}
		public static unsafe float macross_D_GGXaniso (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float RoughnessX, float RoughnessZ, float NoH, Vector3 H, Vector3 T, Vector3 B) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = D_GGXaniso(RoughnessX, RoughnessZ, NoH, H, T, B);
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross