using EngineNS.Bricks.CodeBuilder.ShaderNode.Var;
using EngineNS.Bricks.NodeGraph;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Control
{
    public class TtMaterialShaderAttribute : Attribute
    {
        public string Name;
        public string Include;
    }

    [Rtti.Meta("")]
    [TtMaterialShader]
    public partial class TtCoreMaterialShader
    {
        #region Texture
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "SampleLevel2D")]
        [UserCallNode(CallNodeType = typeof(SampleLevel2DNode))]
        [ContextMenu("SampleLevel2D", "Texture\\SampleLevel2D", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector4 SampleLevel2D(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, float level, out Vector3 rgb, out float a)
        {
            rgb = new Vector3();
            a = 0;
            return new Vector4();
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Sample2D")]
        [UserCallNode(CallNodeType = typeof(Sample2DNode))]
        [ContextMenu("Sample2D", "Texture\\Sample2D", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector4 Sample2D(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, out Vector3 rgb, out float a)
        {
            rgb = new Vector3();
            a = 0f;
            return new Vector4();
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Sample2DBias")]
        [UserCallNode(CallNodeType = typeof(Sample2DBiasNode))]
        [ContextMenu("Sample2DBias", "Texture\\Sample2DBias", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector4 Sample2DBias(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, float bias, out Vector3 rgb, out float a)
        {
            rgb = new Vector3();
            a = 0;
            return new Vector4();
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "SampleArrayLevel2D")]
        [UserCallNode(CallNodeType = typeof(SampleArrayLevel2DNode))]
        [ContextMenu("SampleArrayLevel2D", "Texture\\SampleArrayLevel2D", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector4 SampleArrayLevel2D(Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, float level, out Vector3 rgb, out float a)
        {
            rgb = new Vector3();
            a = 0;
            return new Vector4();
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "SampleArray2D")]
        [UserCallNode(CallNodeType = typeof(SampleArray2DNode))]
        [ContextMenu("SampleArray2D", "Texture\\SampleArray2D", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector4 SampleArray2D(Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, out Vector3 rgb, out float a)
        {
            rgb = new Vector3();
            a = 0;
            return new Vector4();
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "TextureSize")]
        [ContextMenu("TextureSize", "Texture\\TextureSize", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector2 TextureSize(Var.Texture2D texture)
        {
            return Vector2.Zero;
        }
        #endregion

        #region Terrain
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "GetTerrainDiffuse")]
        [ContextMenu("GetTerrainDiffuse", "Terrain\\GetTerrainDiffuse", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector3 GetTerrainDiffuse(Vector2 uv, Graphics.Pipeline.Shader.PS_INPUT input)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "GetTerrainNormal")]
        [ContextMenu("GetTerrainNormal", "Terrain\\GetTerrainNormal", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector3 GetTerrainNormal(Vector2 uv, Graphics.Pipeline.Shader.PS_INPUT input)
        {
            return Vector3.Zero;
        }
        #endregion

        #region Effect
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "GrayColor")]
        [ContextMenu(filterStrings: "GrayColor", "Effect\\GrayColor", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector3 GrayColor(Vector3 color)
        {
            return new Vector3(Vector3.Dot(color, new Vector3(0.3f, 0.6f, 0.1f)));
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "NormalMap")]
        [ContextMenu(filterStrings: "NormalMap", "Effect\\NormalMap", TtMaterialGraph.MaterialEditorKeyword)]
        public static void NormalMap(Vector3 Nt, Vector4 Tw, Vector3 Nw, out Vector3 UnpackedNormal)
        {
            //   Vector3 Bw = new Vector3(0.0h, 0.0h, 0.0h);
            //   if (Tw.w > 0.0)
            //{
            //       Bw = -Vector3.Cross(new Vector3(Tw.X, , Nw);
            //   }
            //   else
            //   {
            //       Bw = Vector3.Cross(Tw.xyz, Nw);
            //   }
            //   half3x3 TBN = half3x3(Tw.xyz, Bw, Nw);

            //   Nt.xy = Nt.xy * 2.0h - 1.0h;
            //   Nt.z = sqrt(saturate(1.0h - dot(Nt.xy, Nt.xy)));

            //   UnpackedNormal = mul(Nt, TBN);
            UnpackedNormal = Vector3.Zero;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Panner")]
        [ContextMenu(filterStrings: "Panner", "Effect\\Panner", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Panner(Vector2 uv, float time, Vector2 speed, Vector2 scale, out Vector2 outUV)
        {
            Vector2 uvTrans = speed * time;
            if (scale.X == 0 || scale.Y == 0)
            {
                outUV = uv;
            }
            else
            {
                Vector2 UVScale = new Vector2(1.0f / scale.X, 1.0f / scale.Y);

                outUV.X = uv.X * UVScale.X + (-0.5f * UVScale.X + 0.5f) + uvTrans.X;
                outUV.Y = uv.Y * UVScale.Y + (-0.5f * UVScale.Y + 0.5f) + uvTrans.Y;
            }
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Rotator")]
        [ContextMenu(filterStrings: "Rotator", "Effect\\Rotator", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Rotator(Vector2 uv, float time, Vector2 center, Vector2 scale, float speed, out Vector2 outUV)
        {
            outUV = Vector2.Zero;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "TransformToWorldPos")]
        [ContextMenu(filterStrings: "TransformToWorldPos", "Effect\\TransformToWorldPos", TtMaterialGraph.MaterialEditorKeyword)]
        public static void TransformToWorldPos(Vector3 localPos, out Vector3 worldPos)
        {
            worldPos = Vector3.Zero;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Distortion")]
        [ContextMenu(filterStrings: "Distortion", "Effect\\Distortion", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Distortion(Vector4 localPos, Vector4 localNorm, Vector4 viewPos, Vector4 projPos, Vector3 localCameraPos, float strength, float transparency, float distortionOffset, out Vector2 distortionUV, out float distortionAlpha)
        {
            distortionUV = Vector2.Zero;
            distortionAlpha = 0;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "RimLight")]
        [ContextMenu(filterStrings: "RimLight", "Effect\\RimLight", TtMaterialGraph.MaterialEditorKeyword)]
        public void RimLight(Vector3 N, Vector3 V, float rimPower, float rimIntensity, out float OutRimFactor)
        {
            OutRimFactor = 0;
            //float NdotV = 1 - dot(N, V);
            //NdotV = pow(NdotV, rimPower);
            //NdotV *= rimIntensity;
            //OutRimFactor = NdotV;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "VecMultiplyQuat")]
        [ContextMenu(filterStrings: "VecMultiplyQuat", "Effect\\VecMultiplyQuat", TtMaterialGraph.MaterialEditorKeyword)]
        public static void VecMultiplyQuat(Vector3 vec, Vector4 quat, out Vector3 outVector)
        {
            outVector = Vector3.Zero;
            //var uv = Vector3.Cross(quat.xyz, vec);
            //var uuv = Vector3.Cross(quat.xyz, uv);
            //uv = uv * ((half)2.0f * quat.w);
            //uuv *= (half)2.0f;

            //outVector = vec + uv + uuv;
        }
        
        #endregion

        #region Math
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Dot3D")]
        [ContextMenu("Dot3D", "Math\\Dot3D", TtMaterialGraph.MaterialEditorKeyword)]
        public static float Dot3D(Vector3 v1, Vector3 v2)
        {
            return Vector3.Dot(v1, v2);
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Cross3D")]
        [ContextMenu("Cross3D", "Math\\Cross3D", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector3 Cross3D(Vector3 v1, Vector3 v2)
        {
            return Vector3.Cross(v1, v2);
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "UnpackNormal")]
        [ContextMenu("UnpackNormal", "Math\\UnpackNormal", TtMaterialGraph.MaterialEditorKeyword)]
        public static void UnpackNormal(Vector3 packedNormal, out Vector3 normal)
        {
            normal = packedNormal * 2.0f - Vector3.One;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "BumpToWorldNormal")]
        [ContextMenu("BumpToWorldNormal", "Math\\BumpToWorldNormal", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector3 BumpToWorldNormal(Vector3 normMap, PS_INPUT input)
        {
            return normMap;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Frac")]
        [ContextMenu("Frac", "Math\\Frac", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Frac(float x, out float ret)
        {
            ret = 0;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Pow")]
        [ContextMenu("Pow", "Math\\Pow", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pow(float v1, float v2, out float ret)
        {
            ret = 0;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Clamp")]
        [ContextMenu(filterStrings: "Clamp", "Math\\Clamp", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Clamp(float x, float min, float max, out float ret)
        {
            ret = 0;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Sin")]
        [ContextMenu(filterStrings: "Sin", "Math\\Sin", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Sin(float x, out float sin)
        {
            sin = (float)Math.Sin(x);
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Cos")]
        [ContextMenu(filterStrings: "Cos", "Math\\Cos", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Cos(float x, out float cos)
        {
            cos = (float)Math.Cos(x);
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "SinRemapped")]
        [ContextMenu(filterStrings: "SinRemapped", "Math\\SinRemapped", TtMaterialGraph.MaterialEditorKeyword)]
        public static float SinRemapped(float SinPhase, float v1, float v2)
        {
            v1 = 0;
            return 0.0f;
        }

        [Rtti.Meta("")]
        [TtMaterialShader(Name = "SinCos")]
        [ContextMenu(filterStrings: "SinCos", "Math\\SinCos", TtMaterialGraph.MaterialEditorKeyword)]
        public static void SinCos(float x, out float sin, out float cos)
        {
            sin = (float)Math.Sin(x);
            cos = (float)Math.Cos(x);
        }

        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Ceil")]
        [ContextMenu(filterStrings: "Ceil", "Math\\Ceil", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Ceil(float x, out float ret)
        {
            ret = 0;
        }

        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Max")]
        [ContextMenu(filterStrings: "Max", "Math\\Max", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Max(float v1, float v2, out float ret)
        {
            ret = Math.Max(v1, v2);
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Min")]
        [ContextMenu(filterStrings: "Min", "Math\\Min", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Min(float v1, float v2, out float ret)
        {
            ret = Math.Min(v1, v2);
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Lerp")]
        [ContextMenu(filterStrings: "Lerp", "Math\\Lerp", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Lerp(float v1, float v2, float s, out float ret)
        {
            ret = v1 + s * (v2 - v1);
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Lerp2D")]
        [ContextMenu(filterStrings: "Lerp2D", "Math\\Lerp2D", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Lerp2D(Vector2 v1, Vector2 v2, Vector2 s, out Vector2 ret)
        {
            ret.X = v1.X + s.X * (v2.X - v1.X);
            ret.Y = v1.Y + s.Y * (v2.Y - v1.Y);
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Lerp3D")]
        [ContextMenu(filterStrings: "Lerp3D", "Math\\Lerp3D", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Lerp3D(Vector3 v1, Vector3 v2, Vector3 s, out Vector3 ret)
        {
            ret.X = v1.X + s.X * (v2.X - v1.X);
            ret.Y = v1.Y + s.Y * (v2.Y - v1.Y);
            ret.Z = v1.Z + s.Z * (v2.Z - v1.Z);
        }

        [Rtti.Meta("")]
        [TtMaterialShader(Name = "SmoothStep3D")]
        [ContextMenu(filterStrings: "SmoothStep3D", "Math\\SmoothStep3D", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector3 Smoothstep3D(Vector3 InColor)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "floor3D")]
        [ContextMenu(filterStrings: "floor3D", "Math\\floor3D", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector3 floor3D(Vector3 InColor)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "PolarCoodP2D")]
        [ContextMenu(filterStrings: "PolarCoodP2D", "Math\\PolarCoodP2D", TtMaterialGraph.MaterialEditorKeyword)]
        public static void PolarCoodP2D(Vector2 uv, out Vector2 polar)
        {
            float a, b, x, y;
            x = uv.X;
            y = 1 - uv.Y;
            a = (float)(0.5 - y * 0.5 / 1 * Math.Sin(x * 2 * 3.1415926 / 1));
            b = (float)(0.5 + y * 0.5 / 1 * Math.Cos(x * 2 * 3.1415926 / 1));
            polar.X = a;
            polar.Y = b;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "PolarCoodD2P")]
        [ContextMenu(filterStrings: "PolarCoodD2P", "Math\\PolarCoodD2P", TtMaterialGraph.MaterialEditorKeyword)]
        public static void PolarCoodD2P(Vector2 uv, out Vector2 polar)
        {
            float pi;
            pi = 3.1415926f;
            float alpha;
            float a, b, x, y, x1, y1, r1;//, r2;
            x = uv.X;
            y = 1 - uv.Y;
            a = y - 0.5f;
            b = 0.5f - x;
            alpha = (float)Math.Atan(b / a);
            if (a <= 0)
                alpha = alpha + pi;
            if (a > 0 && b <= 0)
                alpha = alpha + 2 * pi;
            r1 = (float)(b / Math.Sin(alpha));
            y1 = r1 * 2;
            x1 = alpha * 1 / (2 * pi);
            x = x1;
            y = y1;
            polar.X = x;
            polar.Y = y;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "SphereMask")]
        [ContextMenu(filterStrings: "SphereMask", "Math\\SphereMask", TtMaterialGraph.MaterialEditorKeyword)]
        public static float SphereMask(Vector3 A, Vector3 B, float Radius, float Hardness)
        {
            Radius = 2.0f;
            return 0.0f;
        }

        [Rtti.Meta("")]
        [TtMaterialShader(Name = "RotateAboutAxis")]
        [ContextMenu(filterStrings: "RotateAboutAxis", "Math\\RotateAboutAxis", TtMaterialGraph.MaterialEditorKeyword)]
        public static void RotateAboutAxis(Vector3 rotationAxis, float rotationAngle, Vector3 pivotPos, Vector3 localPos, out Vector3 localOffset)
        {
            localOffset = Vector3.Zero;
        }
        #endregion

        #region Pivot
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Pivot_DecodePosition")]
        [ContextMenu("Pivot_DecodePosition", "Pivot\\DecodePosition", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_DecodePosition(Vector3 rgb, out Vector3 localPos)
        {
            localPos = Vector3.Zero;
        }
        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Pivot_DecodeAxisVector")]
        [ContextMenu("Pivot_DecodeAxisVector", "Pivot\\DecodeAxisVector", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_DecodeAxisVector(Vector3 rgb, out Vector3 localAxis)
        {
            localAxis = Vector3.UnitY;
        }

        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Pivot_UnpackIntAsFloat")]
        [ContextMenu("Pivot_UnpackIntAsFloat", "Pivot\\UnpackIntAsFloat", TtMaterialGraph.MaterialEditorKeyword)]
        public static float Pivot_UnpackIntAsFloat(float N)
        {
            return 0;
        }

        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Pivot_GetPivotIndex")]
        [ContextMenu("Pivot_GetPivotIndex", "Pivot\\GetPivotIndex", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_GetPivotIndex(Vector2 uv, Vector2 texSize, out float index)
        {
            index = 0;
        }

        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Pivot_GetParentPivotData")]
        [ContextMenu("Pivot_GetParentPivotData", "Pivot\\GetParentPivotData", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_GetParentPivotData(float parentIdx, Vector2 texSize, float currentIdx, out Vector2 parentUV, out float isChild)
        {
            parentUV = Vector2.Zero;
            isChild = 0;
        }

        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Pivot_GetHierarchyData")]
        [ContextMenu("Pivot_GetHierarchyData", "Pivot\\GetHierarchyData", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_GetHierarchyData(float pivotDepth, Vector2 pivot1UV, Vector2 pivot2UV, Vector2 pivot3UV, Vector2 pivot4UV, out Vector2 rootUV, out Vector2 mainBranchUV, out Vector2 smallBranchUV, out Vector2 leaveUV, out float mainBranchMask, out float smallBranchMask, out float leaveMask)
        {
            rootUV = mainBranchUV = smallBranchUV = leaveUV = Vector2.Zero;
            mainBranchMask = smallBranchMask = leaveMask = 0.0f;
        }

        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Pivot_WindAnimation")]
        [ContextMenu("Pivot_WindAnimation", "Pivot\\WindAnimation", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_WindAnimation(Vector3 prePos,
            Var.Texture2D posTex, Var.Texture2D xTex, Var.SamplerState samp, Vector2 uv,
            float mask, Var.Texture2D windTex, float scale, float speedX, Vector3 windAxisX, float speedY, Vector3 windAxisY,
            Vector3 localPos, float rot, float rotOffset, float parentRot,
            float axisScale, float axisSpeedScale,
            out Vector3 localVertexOffset, out float rotationAngle)
        {
            localVertexOffset = Vector3.Zero;
            rotationAngle = 0.0f;
        }

        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Pivot_WindAnimation_Sway2")]
        [ContextMenu("Pivot_WindAnimation_Sway2", "Pivot\\WindAnimation_Sway2", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_WindAnimation_Sway2(Vector3 windSwayDirection, float windSwayGustFrequency, float windSwayIntensity, Vector3 localPos, float time, out Vector3 localVertexOffset)
        {
            localVertexOffset = Vector3.Zero;
        }

        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Pivot_WindAnimation_Sway3")]
        [ContextMenu("Pivot_WindAnimation_Sway3", "Pivot\\WindAnimation_Sway3", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_WindAnimation_Sway3(Vector3 windSwayDirection, float windSwayGustFrequency, float windSwayIntensity, Vector3 localPos, float time, float windSwayEffectOffset, float windSwayEffectFalloff, out Vector3 localVertexOffset)
        {
            localVertexOffset = Vector3.Zero;
        }

        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Pivot_WindAnimation_Rustle")]
        [ContextMenu("Pivot_WindAnimation_Rustle", "Pivot\\WindAnimation_Rustle", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_WindAnimation_Rustle(float windSpeed, float windIntensity, Vector3 localPos, float time, out Vector3 localVertexOffset)
        {
            localVertexOffset = Vector3.Zero;
        }

        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Pivot_Gradient")]
        [ContextMenu("Pivot_Gradient", "Pivot\\Pivot_Gradient", TtMaterialGraph.MaterialEditorKeyword)]
        public static float Pivot_Gradient(Vector3 worldPos, float gradientOffset, float gradientFallout)
        {
            return 0;
        }

        [Rtti.Meta("")]
        [TtMaterialShader(Name = "Pivot_LeafNormal")]
        [ContextMenu("Pivot_LeafNormal", "Pivot\\Pivot_LeafNormal", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector3 Pivot_LeafNormal(bool frontFace, Vector3 normal)
        {
            return Vector3.UnitY;
        }
        #endregion
    }
    public partial class TtMaterialMethodManager
    {
        public Dictionary<string, Rtti.TtClassMeta.TtMethodMeta> Methods { get; } = new Dictionary<string, Rtti.TtClassMeta.TtMethodMeta>();
        public void SureMethods()
        {
            if (Methods.Count > 0)
                return;
            foreach (var i in Rtti.TtClassMetaManager.Instance.Metas)
            {
                var hlslAttr = i.Value.ClassType.GetCustomAttribute<TtMaterialShaderAttribute>(false);
                if (hlslAttr == null)
                    continue;
                foreach (var j in i.Value.Methods)
                {
                    var methodAttr = j.GetMethod().GetCustomAttribute<TtMaterialShaderAttribute>();
                    if (hlslAttr == null)
                        continue;
                    System.Diagnostics.Debug.Assert(Methods.TryGetValue(methodAttr.Name, out var m) == false);
                    Methods[methodAttr.Name] = j;
                }
            }
        }
        public Rtti.TtClassMeta.TtMethodMeta GetMethod(string fun)
        {
            SureMethods();
            if(Methods.TryGetValue(fun, out var result))
            {
                return result;
            }
            return null;
        }
        public Rtti.TtClassMeta.TtMethodMeta GetMethodByDeclString(string declStr)
        {
            SureMethods();
            declStr = Rtti.TtClassMeta.RemoveDeclstringDllVersion(declStr);
            foreach (var i in Methods)
            {
                if (i.Value.GetMethodDeclareString(true) == declStr)
                    return i.Value;
            }
            return null;
        }
    }
}

namespace EngineNS
{
    public partial class TtEngine
    {
        Bricks.CodeBuilder.ShaderNode.Control.TtMaterialMethodManager mMaterialMethodManager = new Bricks.CodeBuilder.ShaderNode.Control.TtMaterialMethodManager();
        public Bricks.CodeBuilder.ShaderNode.Control.TtMaterialMethodManager MaterialMethodManager 
        { 
            get
            {
                mMaterialMethodManager.SureMethods();
                return mMaterialMethodManager;
            }
        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Control
{
	partial class TtCoreMaterialShader
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_SampleLevel2D_1619298209 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static Vector4 SampleLevel2D(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, float level, out Vector3 rgb, out float a)");
		public static unsafe Vector4 macross_SampleLevel2D (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, float level, out Vector3 rgb, out float a) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":texture", texture);
					stackframe.SetWatchVariable(nodeName + ":sampler", sampler);
					stackframe.SetWatchVariable(nodeName + ":uv", uv);
					stackframe.SetWatchVariable(nodeName + ":level", level);
				}
			}
			var _return_value = SampleLevel2D(texture, sampler, uv, level, out rgb, out a);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":rgb", rgb);
					stackframe.SetWatchVariable(nodeName + ":a", a);
				}
			}
			macross_break_SampleLevel2D_1619298209.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Sample2D_22126203 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static Vector4 Sample2D(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, out Vector3 rgb, out float a)");
		public static unsafe Vector4 macross_Sample2D (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, out Vector3 rgb, out float a) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":texture", texture);
					stackframe.SetWatchVariable(nodeName + ":sampler", sampler);
					stackframe.SetWatchVariable(nodeName + ":uv", uv);
				}
			}
			var _return_value = Sample2D(texture, sampler, uv, out rgb, out a);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":rgb", rgb);
					stackframe.SetWatchVariable(nodeName + ":a", a);
				}
			}
			macross_break_Sample2D_22126203.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Sample2DBias_1637392118 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static Vector4 Sample2DBias(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, float bias, out Vector3 rgb, out float a)");
		public static unsafe Vector4 macross_Sample2DBias (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, float bias, out Vector3 rgb, out float a) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":texture", texture);
					stackframe.SetWatchVariable(nodeName + ":sampler", sampler);
					stackframe.SetWatchVariable(nodeName + ":uv", uv);
					stackframe.SetWatchVariable(nodeName + ":bias", bias);
				}
			}
			var _return_value = Sample2DBias(texture, sampler, uv, bias, out rgb, out a);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":rgb", rgb);
					stackframe.SetWatchVariable(nodeName + ":a", a);
				}
			}
			macross_break_Sample2DBias_1637392118.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_SampleArrayLevel2D_4198432855 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static Vector4 SampleArrayLevel2D(Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, float level, out Vector3 rgb, out float a)");
		public static unsafe Vector4 macross_SampleArrayLevel2D (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, float level, out Vector3 rgb, out float a) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":texture", texture);
					stackframe.SetWatchVariable(nodeName + ":sampler", sampler);
					stackframe.SetWatchVariable(nodeName + ":uv", uv);
					stackframe.SetWatchVariable(nodeName + ":arrayIndex", arrayIndex);
					stackframe.SetWatchVariable(nodeName + ":level", level);
				}
			}
			var _return_value = SampleArrayLevel2D(texture, sampler, uv, arrayIndex, level, out rgb, out a);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":rgb", rgb);
					stackframe.SetWatchVariable(nodeName + ":a", a);
				}
			}
			macross_break_SampleArrayLevel2D_4198432855.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_SampleArray2D_3585907133 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static Vector4 SampleArray2D(Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, out Vector3 rgb, out float a)");
		public static unsafe Vector4 macross_SampleArray2D (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, out Vector3 rgb, out float a) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":texture", texture);
					stackframe.SetWatchVariable(nodeName + ":sampler", sampler);
					stackframe.SetWatchVariable(nodeName + ":uv", uv);
					stackframe.SetWatchVariable(nodeName + ":arrayIndex", arrayIndex);
				}
			}
			var _return_value = SampleArray2D(texture, sampler, uv, arrayIndex, out rgb, out a);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":rgb", rgb);
					stackframe.SetWatchVariable(nodeName + ":a", a);
				}
			}
			macross_break_SampleArray2D_3585907133.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_TextureSize_3456849711 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static Vector2 TextureSize(Var.Texture2D texture)");
		public static unsafe Vector2 macross_TextureSize (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Var.Texture2D texture) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":texture", texture);
				}
			}
			var _return_value = TextureSize(texture);
			macross_break_TextureSize_3456849711.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_GetTerrainDiffuse_2999334926 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static Vector3 GetTerrainDiffuse(Vector2 uv, Graphics.Pipeline.Shader.PS_INPUT input)");
		public static unsafe Vector3 macross_GetTerrainDiffuse (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector2 uv, Graphics.Pipeline.Shader.PS_INPUT input) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":uv", uv);
					stackframe.SetWatchVariable(nodeName + ":input", input);
				}
			}
			var _return_value = GetTerrainDiffuse(uv, input);
			macross_break_GetTerrainDiffuse_2999334926.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_GetTerrainNormal_2999334926 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static Vector3 GetTerrainNormal(Vector2 uv, Graphics.Pipeline.Shader.PS_INPUT input)");
		public static unsafe Vector3 macross_GetTerrainNormal (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector2 uv, Graphics.Pipeline.Shader.PS_INPUT input) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":uv", uv);
					stackframe.SetWatchVariable(nodeName + ":input", input);
				}
			}
			var _return_value = GetTerrainNormal(uv, input);
			macross_break_GetTerrainNormal_2999334926.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_GrayColor_2932943049 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static Vector3 GrayColor(Vector3 color)");
		public static unsafe Vector3 macross_GrayColor (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 color) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":color", color);
				}
			}
			var _return_value = GrayColor(color);
			macross_break_GrayColor_2932943049.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_NormalMap_2670023119 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void NormalMap(Vector3 Nt, Vector4 Tw, Vector3 Nw, out Vector3 UnpackedNormal)");
		public static unsafe void macross_NormalMap (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 Nt, Vector4 Tw, Vector3 Nw, out Vector3 UnpackedNormal) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":Nt", Nt);
					stackframe.SetWatchVariable(nodeName + ":Tw", Tw);
					stackframe.SetWatchVariable(nodeName + ":Nw", Nw);
				}
			}
			NormalMap(Nt, Tw, Nw, out UnpackedNormal);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":UnpackedNormal", UnpackedNormal);
				}
			}
			macross_break_NormalMap_2670023119.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Panner_3502202686 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Panner(Vector2 uv, float time, Vector2 speed, Vector2 scale, out Vector2 outUV)");
		public static unsafe void macross_Panner (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector2 uv, float time, Vector2 speed, Vector2 scale, out Vector2 outUV) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":uv", uv);
					stackframe.SetWatchVariable(nodeName + ":time", time);
					stackframe.SetWatchVariable(nodeName + ":speed", speed);
					stackframe.SetWatchVariable(nodeName + ":scale", scale);
				}
			}
			Panner(uv, time, speed, scale, out outUV);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":outUV", outUV);
				}
			}
			macross_break_Panner_3502202686.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Rotator_3937462911 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Rotator(Vector2 uv, float time, Vector2 center, Vector2 scale, float speed, out Vector2 outUV)");
		public static unsafe void macross_Rotator (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector2 uv, float time, Vector2 center, Vector2 scale, float speed, out Vector2 outUV) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":uv", uv);
					stackframe.SetWatchVariable(nodeName + ":time", time);
					stackframe.SetWatchVariable(nodeName + ":center", center);
					stackframe.SetWatchVariable(nodeName + ":scale", scale);
					stackframe.SetWatchVariable(nodeName + ":speed", speed);
				}
			}
			Rotator(uv, time, center, scale, speed, out outUV);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":outUV", outUV);
				}
			}
			macross_break_Rotator_3937462911.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_TransformToWorldPos_1746925731 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void TransformToWorldPos(Vector3 localPos, out Vector3 worldPos)");
		public static unsafe void macross_TransformToWorldPos (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 localPos, out Vector3 worldPos) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":localPos", localPos);
				}
			}
			TransformToWorldPos(localPos, out worldPos);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":worldPos", worldPos);
				}
			}
			macross_break_TransformToWorldPos_1746925731.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Distortion_269822995 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Distortion(Vector4 localPos, Vector4 localNorm, Vector4 viewPos, Vector4 projPos, Vector3 localCameraPos, float strength, float transparency, float distortionOffset, out Vector2 distortionUV, out float distortionAlpha)");
		public static unsafe void macross_Distortion (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector4 localPos, Vector4 localNorm, Vector4 viewPos, Vector4 projPos, Vector3 localCameraPos, float strength, float transparency, float distortionOffset, out Vector2 distortionUV, out float distortionAlpha) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":localPos", localPos);
					stackframe.SetWatchVariable(nodeName + ":localNorm", localNorm);
					stackframe.SetWatchVariable(nodeName + ":viewPos", viewPos);
					stackframe.SetWatchVariable(nodeName + ":projPos", projPos);
					stackframe.SetWatchVariable(nodeName + ":localCameraPos", localCameraPos);
					stackframe.SetWatchVariable(nodeName + ":strength", strength);
					stackframe.SetWatchVariable(nodeName + ":transparency", transparency);
					stackframe.SetWatchVariable(nodeName + ":distortionOffset", distortionOffset);
				}
			}
			Distortion(localPos, localNorm, viewPos, projPos, localCameraPos, strength, transparency, distortionOffset, out distortionUV, out distortionAlpha);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":distortionUV", distortionUV);
					stackframe.SetWatchVariable(nodeName + ":distortionAlpha", distortionAlpha);
				}
			}
			macross_break_Distortion_269822995.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_RimLight_1487571022 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->void RimLight(Vector3 N, Vector3 V, float rimPower, float rimIntensity, out float OutRimFactor)");
		public unsafe void macross_RimLight (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 N, Vector3 V, float rimPower, float rimIntensity, out float OutRimFactor) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":N", N);
					stackframe.SetWatchVariable(nodeName + ":V", V);
					stackframe.SetWatchVariable(nodeName + ":rimPower", rimPower);
					stackframe.SetWatchVariable(nodeName + ":rimIntensity", rimIntensity);
				}
			}
			RimLight(N, V, rimPower, rimIntensity, out OutRimFactor);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":OutRimFactor", OutRimFactor);
				}
			}
			macross_break_RimLight_1487571022.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_VecMultiplyQuat_320759171 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void VecMultiplyQuat(Vector3 vec, Vector4 quat, out Vector3 outVector)");
		public static unsafe void macross_VecMultiplyQuat (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 vec, Vector4 quat, out Vector3 outVector) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":vec", vec);
					stackframe.SetWatchVariable(nodeName + ":quat", quat);
				}
			}
			VecMultiplyQuat(vec, quat, out outVector);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":outVector", outVector);
				}
			}
			macross_break_VecMultiplyQuat_320759171.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Dot3D_3561363165 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static float Dot3D(Vector3 v1, Vector3 v2)");
		public static unsafe float macross_Dot3D (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 v1, Vector3 v2) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v1", v1);
					stackframe.SetWatchVariable(nodeName + ":v2", v2);
				}
			}
			var _return_value = Dot3D(v1, v2);
			macross_break_Dot3D_3561363165.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Cross3D_3561363165 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static Vector3 Cross3D(Vector3 v1, Vector3 v2)");
		public static unsafe Vector3 macross_Cross3D (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 v1, Vector3 v2) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v1", v1);
					stackframe.SetWatchVariable(nodeName + ":v2", v2);
				}
			}
			var _return_value = Cross3D(v1, v2);
			macross_break_Cross3D_3561363165.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_UnpackNormal_1896727442 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void UnpackNormal(Vector3 packedNormal, out Vector3 normal)");
		public static unsafe void macross_UnpackNormal (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 packedNormal, out Vector3 normal) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":packedNormal", packedNormal);
				}
			}
			UnpackNormal(packedNormal, out normal);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":normal", normal);
				}
			}
			macross_break_UnpackNormal_1896727442.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_BumpToWorldNormal_2735709164 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static Vector3 BumpToWorldNormal(Vector3 normMap, PS_INPUT input)");
		public static unsafe Vector3 macross_BumpToWorldNormal (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 normMap, PS_INPUT input) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":normMap", normMap);
					stackframe.SetWatchVariable(nodeName + ":input", input);
				}
			}
			var _return_value = BumpToWorldNormal(normMap, input);
			macross_break_BumpToWorldNormal_2735709164.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Frac_2667751931 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Frac(float x, out float ret)");
		public static unsafe void macross_Frac (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float x, out float ret) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
				}
			}
			Frac(x, out ret);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":ret", ret);
				}
			}
			macross_break_Frac_2667751931.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Pow_1037159604 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Pow(float v1, float v2, out float ret)");
		public static unsafe void macross_Pow (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float v1, float v2, out float ret) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v1", v1);
					stackframe.SetWatchVariable(nodeName + ":v2", v2);
				}
			}
			Pow(v1, v2, out ret);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":ret", ret);
				}
			}
			macross_break_Pow_1037159604.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Clamp_1427936477 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Clamp(float x, float min, float max, out float ret)");
		public static unsafe void macross_Clamp (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float x, float min, float max, out float ret) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
					stackframe.SetWatchVariable(nodeName + ":min", min);
					stackframe.SetWatchVariable(nodeName + ":max", max);
				}
			}
			Clamp(x, min, max, out ret);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":ret", ret);
				}
			}
			macross_break_Clamp_1427936477.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Sin_1727491704 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Sin(float x, out float sin)");
		public static unsafe void macross_Sin (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float x, out float sin) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
				}
			}
			Sin(x, out sin);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":sin", sin);
				}
			}
			macross_break_Sin_1727491704.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Cos_3143966799 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Cos(float x, out float cos)");
		public static unsafe void macross_Cos (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float x, out float cos) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
				}
			}
			Cos(x, out cos);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":cos", cos);
				}
			}
			macross_break_Cos_3143966799.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_SinRemapped_292764772 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static float SinRemapped(float SinPhase, float v1, float v2)");
		public static unsafe float macross_SinRemapped (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float SinPhase, float v1, float v2) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":SinPhase", SinPhase);
					stackframe.SetWatchVariable(nodeName + ":v1", v1);
					stackframe.SetWatchVariable(nodeName + ":v2", v2);
				}
			}
			var _return_value = SinRemapped(SinPhase, v1, v2);
			macross_break_SinRemapped_292764772.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_SinCos_1216592905 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void SinCos(float x, out float sin, out float cos)");
		public static unsafe void macross_SinCos (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float x, out float sin, out float cos) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
				}
			}
			SinCos(x, out sin, out cos);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":sin", sin);
					stackframe.SetWatchVariable(nodeName + ":cos", cos);
				}
			}
			macross_break_SinCos_1216592905.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Ceil_2667751931 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Ceil(float x, out float ret)");
		public static unsafe void macross_Ceil (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float x, out float ret) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
				}
			}
			Ceil(x, out ret);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":ret", ret);
				}
			}
			macross_break_Ceil_2667751931.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Max_1037159604 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Max(float v1, float v2, out float ret)");
		public static unsafe void macross_Max (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float v1, float v2, out float ret) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v1", v1);
					stackframe.SetWatchVariable(nodeName + ":v2", v2);
				}
			}
			Max(v1, v2, out ret);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":ret", ret);
				}
			}
			macross_break_Max_1037159604.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Min_1037159604 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Min(float v1, float v2, out float ret)");
		public static unsafe void macross_Min (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float v1, float v2, out float ret) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v1", v1);
					stackframe.SetWatchVariable(nodeName + ":v2", v2);
				}
			}
			Min(v1, v2, out ret);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":ret", ret);
				}
			}
			macross_break_Min_1037159604.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Lerp_2744330567 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Lerp(float v1, float v2, float s, out float ret)");
		public static unsafe void macross_Lerp (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float v1, float v2, float s, out float ret) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v1", v1);
					stackframe.SetWatchVariable(nodeName + ":v2", v2);
					stackframe.SetWatchVariable(nodeName + ":s", s);
				}
			}
			Lerp(v1, v2, s, out ret);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":ret", ret);
				}
			}
			macross_break_Lerp_2744330567.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Lerp2D_212282019 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Lerp2D(Vector2 v1, Vector2 v2, Vector2 s, out Vector2 ret)");
		public static unsafe void macross_Lerp2D (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector2 v1, Vector2 v2, Vector2 s, out Vector2 ret) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v1", v1);
					stackframe.SetWatchVariable(nodeName + ":v2", v2);
					stackframe.SetWatchVariable(nodeName + ":s", s);
				}
			}
			Lerp2D(v1, v2, s, out ret);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":ret", ret);
				}
			}
			macross_break_Lerp2D_212282019.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Lerp3D_1881825247 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Lerp3D(Vector3 v1, Vector3 v2, Vector3 s, out Vector3 ret)");
		public static unsafe void macross_Lerp3D (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 v1, Vector3 v2, Vector3 s, out Vector3 ret) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v1", v1);
					stackframe.SetWatchVariable(nodeName + ":v2", v2);
					stackframe.SetWatchVariable(nodeName + ":s", s);
				}
			}
			Lerp3D(v1, v2, s, out ret);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":ret", ret);
				}
			}
			macross_break_Lerp3D_1881825247.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Smoothstep3D_1143902460 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static Vector3 Smoothstep3D(Vector3 InColor)");
		public static unsafe Vector3 macross_Smoothstep3D (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 InColor) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":InColor", InColor);
				}
			}
			var _return_value = Smoothstep3D(InColor);
			macross_break_Smoothstep3D_1143902460.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_floor3D_1143902460 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static Vector3 floor3D(Vector3 InColor)");
		public static unsafe Vector3 macross_floor3D (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 InColor) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":InColor", InColor);
				}
			}
			var _return_value = floor3D(InColor);
			macross_break_floor3D_1143902460.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_PolarCoodP2D_1242339339 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void PolarCoodP2D(Vector2 uv, out Vector2 polar)");
		public static unsafe void macross_PolarCoodP2D (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector2 uv, out Vector2 polar) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":uv", uv);
				}
			}
			PolarCoodP2D(uv, out polar);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":polar", polar);
				}
			}
			macross_break_PolarCoodP2D_1242339339.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_PolarCoodD2P_1242339339 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void PolarCoodD2P(Vector2 uv, out Vector2 polar)");
		public static unsafe void macross_PolarCoodD2P (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector2 uv, out Vector2 polar) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":uv", uv);
				}
			}
			PolarCoodD2P(uv, out polar);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":polar", polar);
				}
			}
			macross_break_PolarCoodD2P_1242339339.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_SphereMask_315209473 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static float SphereMask(Vector3 A, Vector3 B, float Radius, float Hardness)");
		public static unsafe float macross_SphereMask (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 A, Vector3 B, float Radius, float Hardness) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":A", A);
					stackframe.SetWatchVariable(nodeName + ":B", B);
					stackframe.SetWatchVariable(nodeName + ":Radius", Radius);
					stackframe.SetWatchVariable(nodeName + ":Hardness", Hardness);
				}
			}
			var _return_value = SphereMask(A, B, Radius, Hardness);
			macross_break_SphereMask_315209473.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_RotateAboutAxis_585100803 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void RotateAboutAxis(Vector3 rotationAxis, float rotationAngle, Vector3 pivotPos, Vector3 localPos, out Vector3 localOffset)");
		public static unsafe void macross_RotateAboutAxis (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 rotationAxis, float rotationAngle, Vector3 pivotPos, Vector3 localPos, out Vector3 localOffset) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":rotationAxis", rotationAxis);
					stackframe.SetWatchVariable(nodeName + ":rotationAngle", rotationAngle);
					stackframe.SetWatchVariable(nodeName + ":pivotPos", pivotPos);
					stackframe.SetWatchVariable(nodeName + ":localPos", localPos);
				}
			}
			RotateAboutAxis(rotationAxis, rotationAngle, pivotPos, localPos, out localOffset);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":localOffset", localOffset);
				}
			}
			macross_break_RotateAboutAxis_585100803.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Pivot_DecodePosition_2998404746 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Pivot_DecodePosition(Vector3 rgb, out Vector3 localPos)");
		public static unsafe void macross_Pivot_DecodePosition (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 rgb, out Vector3 localPos) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":rgb", rgb);
				}
			}
			Pivot_DecodePosition(rgb, out localPos);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":localPos", localPos);
				}
			}
			macross_break_Pivot_DecodePosition_2998404746.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Pivot_DecodeAxisVector_748401133 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Pivot_DecodeAxisVector(Vector3 rgb, out Vector3 localAxis)");
		public static unsafe void macross_Pivot_DecodeAxisVector (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 rgb, out Vector3 localAxis) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":rgb", rgb);
				}
			}
			Pivot_DecodeAxisVector(rgb, out localAxis);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":localAxis", localAxis);
				}
			}
			macross_break_Pivot_DecodeAxisVector_748401133.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Pivot_UnpackIntAsFloat_650858670 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static float Pivot_UnpackIntAsFloat(float N)");
		public static unsafe float macross_Pivot_UnpackIntAsFloat (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float N) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":N", N);
				}
			}
			var _return_value = Pivot_UnpackIntAsFloat(N);
			macross_break_Pivot_UnpackIntAsFloat_650858670.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Pivot_GetPivotIndex_291261633 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Pivot_GetPivotIndex(Vector2 uv, Vector2 texSize, out float index)");
		public static unsafe void macross_Pivot_GetPivotIndex (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector2 uv, Vector2 texSize, out float index) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":uv", uv);
					stackframe.SetWatchVariable(nodeName + ":texSize", texSize);
				}
			}
			Pivot_GetPivotIndex(uv, texSize, out index);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":index", index);
				}
			}
			macross_break_Pivot_GetPivotIndex_291261633.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Pivot_GetParentPivotData_2749766402 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Pivot_GetParentPivotData(float parentIdx, Vector2 texSize, float currentIdx, out Vector2 parentUV, out float isChild)");
		public static unsafe void macross_Pivot_GetParentPivotData (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float parentIdx, Vector2 texSize, float currentIdx, out Vector2 parentUV, out float isChild) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":parentIdx", parentIdx);
					stackframe.SetWatchVariable(nodeName + ":texSize", texSize);
					stackframe.SetWatchVariable(nodeName + ":currentIdx", currentIdx);
				}
			}
			Pivot_GetParentPivotData(parentIdx, texSize, currentIdx, out parentUV, out isChild);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":parentUV", parentUV);
					stackframe.SetWatchVariable(nodeName + ":isChild", isChild);
				}
			}
			macross_break_Pivot_GetParentPivotData_2749766402.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Pivot_GetHierarchyData_1987431315 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Pivot_GetHierarchyData(float pivotDepth, Vector2 pivot1UV, Vector2 pivot2UV, Vector2 pivot3UV, Vector2 pivot4UV, out Vector2 rootUV, out Vector2 mainBranchUV, out Vector2 smallBranchUV, out Vector2 leaveUV, out float mainBranchMask, out float smallBranchMask, out float leaveMask)");
		public static unsafe void macross_Pivot_GetHierarchyData (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float pivotDepth, Vector2 pivot1UV, Vector2 pivot2UV, Vector2 pivot3UV, Vector2 pivot4UV, out Vector2 rootUV, out Vector2 mainBranchUV, out Vector2 smallBranchUV, out Vector2 leaveUV, out float mainBranchMask, out float smallBranchMask, out float leaveMask) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":pivotDepth", pivotDepth);
					stackframe.SetWatchVariable(nodeName + ":pivot1UV", pivot1UV);
					stackframe.SetWatchVariable(nodeName + ":pivot2UV", pivot2UV);
					stackframe.SetWatchVariable(nodeName + ":pivot3UV", pivot3UV);
					stackframe.SetWatchVariable(nodeName + ":pivot4UV", pivot4UV);
				}
			}
			Pivot_GetHierarchyData(pivotDepth, pivot1UV, pivot2UV, pivot3UV, pivot4UV, out rootUV, out mainBranchUV, out smallBranchUV, out leaveUV, out mainBranchMask, out smallBranchMask, out leaveMask);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":rootUV", rootUV);
					stackframe.SetWatchVariable(nodeName + ":mainBranchUV", mainBranchUV);
					stackframe.SetWatchVariable(nodeName + ":smallBranchUV", smallBranchUV);
					stackframe.SetWatchVariable(nodeName + ":leaveUV", leaveUV);
					stackframe.SetWatchVariable(nodeName + ":mainBranchMask", mainBranchMask);
					stackframe.SetWatchVariable(nodeName + ":smallBranchMask", smallBranchMask);
					stackframe.SetWatchVariable(nodeName + ":leaveMask", leaveMask);
				}
			}
			macross_break_Pivot_GetHierarchyData_1987431315.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Pivot_WindAnimation_1869733926 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Pivot_WindAnimation(Vector3 prePos, Var.Texture2D posTex, Var.Texture2D xTex, Var.SamplerState samp, Vector2 uv, float mask, Var.Texture2D windTex, float scale, float speedX, Vector3 windAxisX, float speedY, Vector3 windAxisY, Vector3 localPos, float rot, float rotOffset, float parentRot, float axisScale, float axisSpeedScale, out Vector3 localVertexOffset, out float rotationAngle)");
		public static unsafe void macross_Pivot_WindAnimation (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 prePos, Var.Texture2D posTex, Var.Texture2D xTex, Var.SamplerState samp, Vector2 uv, float mask, Var.Texture2D windTex, float scale, float speedX, Vector3 windAxisX, float speedY, Vector3 windAxisY, Vector3 localPos, float rot, float rotOffset, float parentRot, float axisScale, float axisSpeedScale, out Vector3 localVertexOffset, out float rotationAngle) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":prePos", prePos);
					stackframe.SetWatchVariable(nodeName + ":posTex", posTex);
					stackframe.SetWatchVariable(nodeName + ":xTex", xTex);
					stackframe.SetWatchVariable(nodeName + ":samp", samp);
					stackframe.SetWatchVariable(nodeName + ":uv", uv);
					stackframe.SetWatchVariable(nodeName + ":mask", mask);
					stackframe.SetWatchVariable(nodeName + ":windTex", windTex);
					stackframe.SetWatchVariable(nodeName + ":scale", scale);
					stackframe.SetWatchVariable(nodeName + ":speedX", speedX);
					stackframe.SetWatchVariable(nodeName + ":windAxisX", windAxisX);
					stackframe.SetWatchVariable(nodeName + ":speedY", speedY);
					stackframe.SetWatchVariable(nodeName + ":windAxisY", windAxisY);
					stackframe.SetWatchVariable(nodeName + ":localPos", localPos);
					stackframe.SetWatchVariable(nodeName + ":rot", rot);
					stackframe.SetWatchVariable(nodeName + ":rotOffset", rotOffset);
					stackframe.SetWatchVariable(nodeName + ":parentRot", parentRot);
					stackframe.SetWatchVariable(nodeName + ":axisScale", axisScale);
					stackframe.SetWatchVariable(nodeName + ":axisSpeedScale", axisSpeedScale);
				}
			}
			Pivot_WindAnimation(prePos, posTex, xTex, samp, uv, mask, windTex, scale, speedX, windAxisX, speedY, windAxisY, localPos, rot, rotOffset, parentRot, axisScale, axisSpeedScale, out localVertexOffset, out rotationAngle);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":localVertexOffset", localVertexOffset);
					stackframe.SetWatchVariable(nodeName + ":rotationAngle", rotationAngle);
				}
			}
			macross_break_Pivot_WindAnimation_1869733926.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Pivot_WindAnimation_Sway2_8267977 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Pivot_WindAnimation_Sway2(Vector3 windSwayDirection, float windSwayGustFrequency, float windSwayIntensity, Vector3 localPos, float time, out Vector3 localVertexOffset)");
		public static unsafe void macross_Pivot_WindAnimation_Sway2 (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 windSwayDirection, float windSwayGustFrequency, float windSwayIntensity, Vector3 localPos, float time, out Vector3 localVertexOffset) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":windSwayDirection", windSwayDirection);
					stackframe.SetWatchVariable(nodeName + ":windSwayGustFrequency", windSwayGustFrequency);
					stackframe.SetWatchVariable(nodeName + ":windSwayIntensity", windSwayIntensity);
					stackframe.SetWatchVariable(nodeName + ":localPos", localPos);
					stackframe.SetWatchVariable(nodeName + ":time", time);
				}
			}
			Pivot_WindAnimation_Sway2(windSwayDirection, windSwayGustFrequency, windSwayIntensity, localPos, time, out localVertexOffset);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":localVertexOffset", localVertexOffset);
				}
			}
			macross_break_Pivot_WindAnimation_Sway2_8267977.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Pivot_WindAnimation_Sway3_1685228346 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Pivot_WindAnimation_Sway3(Vector3 windSwayDirection, float windSwayGustFrequency, float windSwayIntensity, Vector3 localPos, float time, float windSwayEffectOffset, float windSwayEffectFalloff, out Vector3 localVertexOffset)");
		public static unsafe void macross_Pivot_WindAnimation_Sway3 (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 windSwayDirection, float windSwayGustFrequency, float windSwayIntensity, Vector3 localPos, float time, float windSwayEffectOffset, float windSwayEffectFalloff, out Vector3 localVertexOffset) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":windSwayDirection", windSwayDirection);
					stackframe.SetWatchVariable(nodeName + ":windSwayGustFrequency", windSwayGustFrequency);
					stackframe.SetWatchVariable(nodeName + ":windSwayIntensity", windSwayIntensity);
					stackframe.SetWatchVariable(nodeName + ":localPos", localPos);
					stackframe.SetWatchVariable(nodeName + ":time", time);
					stackframe.SetWatchVariable(nodeName + ":windSwayEffectOffset", windSwayEffectOffset);
					stackframe.SetWatchVariable(nodeName + ":windSwayEffectFalloff", windSwayEffectFalloff);
				}
			}
			Pivot_WindAnimation_Sway3(windSwayDirection, windSwayGustFrequency, windSwayIntensity, localPos, time, windSwayEffectOffset, windSwayEffectFalloff, out localVertexOffset);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":localVertexOffset", localVertexOffset);
				}
			}
			macross_break_Pivot_WindAnimation_Sway3_1685228346.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Pivot_WindAnimation_Rustle_701467676 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static void Pivot_WindAnimation_Rustle(float windSpeed, float windIntensity, Vector3 localPos, float time, out Vector3 localVertexOffset)");
		public static unsafe void macross_Pivot_WindAnimation_Rustle (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float windSpeed, float windIntensity, Vector3 localPos, float time, out Vector3 localVertexOffset) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":windSpeed", windSpeed);
					stackframe.SetWatchVariable(nodeName + ":windIntensity", windIntensity);
					stackframe.SetWatchVariable(nodeName + ":localPos", localPos);
					stackframe.SetWatchVariable(nodeName + ":time", time);
				}
			}
			Pivot_WindAnimation_Rustle(windSpeed, windIntensity, localPos, time, out localVertexOffset);
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":localVertexOffset", localVertexOffset);
				}
			}
			macross_break_Pivot_WindAnimation_Rustle_701467676.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Pivot_Gradient_3777521470 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static float Pivot_Gradient(Vector3 worldPos, float gradientOffset, float gradientFallout)");
		public static unsafe float macross_Pivot_Gradient (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Vector3 worldPos, float gradientOffset, float gradientFallout) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":worldPos", worldPos);
					stackframe.SetWatchVariable(nodeName + ":gradientOffset", gradientOffset);
					stackframe.SetWatchVariable(nodeName + ":gradientFallout", gradientFallout);
				}
			}
			var _return_value = Pivot_Gradient(worldPos, gradientOffset, gradientFallout);
			macross_break_Pivot_Gradient_3777521470.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Pivot_LeafNormal_1940221793 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtCoreMaterialShader->static Vector3 Pivot_LeafNormal(bool frontFace, Vector3 normal)");
		public static unsafe Vector3 macross_Pivot_LeafNormal (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, bool frontFace, Vector3 normal) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":frontFace", frontFace);
					stackframe.SetWatchVariable(nodeName + ":normal", normal);
				}
			}
			var _return_value = Pivot_LeafNormal(frontFace, normal);
			macross_break_Pivot_LeafNormal_1940221793.TryBreak(mcStack);
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross