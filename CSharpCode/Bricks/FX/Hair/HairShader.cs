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
        public static float StrandSpecular(Vector3 T, Vector3 V, Vector3 L, float exponent)
        {
            Vector3 H = Vector3.Normalize(L + V);
            float ToH = Vector3.Dot(T, H);
            float sinTH = MathF.Sqrt(1.0f - ToH * ToH);
            float dirAtten = MathHelper.Lerp(-1.0f, 0.0f, Vector3.Dot(T, H));
            return dirAtten * MathF.Pow(sinTH, exponent);
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "ShiftTangent", Include = "@Engine/Shaders/Bricks/FX/Hair.cginc")]
        [ContextMenu("ShiftTangent", "FX\\ShiftTangent", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector3 ShiftTangent(Vector3 T, Vector3 N, float shift)
        {
            Vector3 shiftedT = T + (shift * N);
            return Vector3.Normalize(shiftedT);
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "GetAnisotropicRoughness", Include = "@Engine/Shaders/Bricks/FX/Hair.cginc")]
        [ContextMenu("GetAnisotropicRoughness", "FX\\GetAnisotropicRoughness", TtMaterialGraph.MaterialEditorKeyword)]
        public static void GetAnisotropicRoughness(float roughness, float anisotropic, out float ax, out float az)
        {
            float aspect = MathF.Sqrt(1.0f - 0.9f * anisotropic);
            float roughnessSq = roughness * roughness;
            ax = roughnessSq / aspect;
            az = roughnessSq * aspect;
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "D_GGXaniso", Include = "@Engine/Shaders/Bricks/FX/Hair.cginc")]
        [ContextMenu("D_GGXaniso", "FX\\D_GGXaniso", TtMaterialGraph.MaterialEditorKeyword)]
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
