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

    [Rtti.Meta]
    [TtMaterialShader]
    public partial class TtCoreMaterialShader
    {
        #region Texture
        [Rtti.Meta]
        [TtMaterialShader(Name = "SampleLevel2D")]
        [UserCallNode(CallNodeType = typeof(SampleLevel2DNode))]
        [ContextMenu("SampleLevel2D", "Texture\\SampleLevel2D", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector4 SampleLevel2D(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, float level, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "Sample2D")]
        [UserCallNode(CallNodeType = typeof(Sample2DNode))]
        [ContextMenu("Sample2D", "Texture\\Sample2D", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector4 Sample2D(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "SampleArrayLevel2D")]
        [UserCallNode(CallNodeType = typeof(SampleArrayLevel2DNode))]
        [ContextMenu("SampleArrayLevel2D", "Texture\\SampleArrayLevel2D", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector4 SampleArrayLevel2D(Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, float level, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "SampleArray2D")]
        [UserCallNode(CallNodeType = typeof(SampleArray2DNode))]
        [ContextMenu("SampleArray2D", "Texture\\SampleArray2D", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector4 SampleArray2D(Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "TextureSize")]
        [ContextMenu("TextureSize", "Texture\\TextureSize", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector2 TextureSize(Var.Texture2D texture)
        {
            return Vector2.Zero;
        }
        #endregion

        #region Terrain
        [Rtti.Meta]
        [TtMaterialShader(Name = "GetTerrainDiffuse")]
        [ContextMenu("GetTerrainDiffuse", "Terrain\\GetTerrainDiffuse", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector3 GetTerrainDiffuse(Vector2 uv, Graphics.Pipeline.Shader.PS_INPUT input)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "GetTerrainNormal")]
        [ContextMenu("GetTerrainNormal", "Terrain\\GetTerrainNormal", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector3 GetTerrainNormal(Vector2 uv, Graphics.Pipeline.Shader.PS_INPUT input)
        {
            return Vector3.Zero;
        }
        #endregion

        #region Effect
        [Rtti.Meta]
        [TtMaterialShader(Name = "NormalMap")]
        [ContextMenu(filterStrings: "Effect", "Effect\\NormalMap", TtMaterialGraph.MaterialEditorKeyword)]
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
        [Rtti.Meta]
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
        [Rtti.Meta]
        [TtMaterialShader(Name = "Rotator")]
        [ContextMenu(filterStrings: "Rotator", "Effect\\Rotator", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Rotator(Vector2 uv, float time, Vector2 center, Vector2 scale, float speed, out Vector2 outUV)
        {
            outUV = Vector2.Zero;
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "TransformToWorldPos")]
        [ContextMenu(filterStrings: "TransformToWorldPos", "Effect\\TransformToWorldPos", TtMaterialGraph.MaterialEditorKeyword)]
        public static void TransformToWorldPos(Vector3 localPos, out Vector3 worldPos)
        {
            worldPos = Vector3.Zero;
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "Distortion")]
        [ContextMenu(filterStrings: "Distortion", "Effect\\Distortion", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Distortion(Vector4 localPos, Vector4 localNorm, Vector4 viewPos, Vector4 projPos, Vector3 localCameraPos, float strength, float transparency, float distortionOffset, out Vector2 distortionUV, out float distortionAlpha)
        {
            distortionUV = Vector2.Zero;
            distortionAlpha = 0;
        }
        [Rtti.Meta]
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
        [Rtti.Meta]
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
        [Rtti.Meta]
        [TtMaterialShader(Name = "UnpackNormal")]
        [ContextMenu("UnpackNormal", "Math\\UnpackNormal", TtMaterialGraph.MaterialEditorKeyword)]
        public static void UnpackNormal(Vector3 packedNormal, out Vector3 normal)
        {
            normal = packedNormal * 2.0f - Vector3.One;
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "Frac")]
        [ContextMenu("Frac", "Math\\Frac", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Frac(float x, out float ret)
        {
            ret = 0;
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "Pow")]
        [ContextMenu("Pow", "Math\\Pow", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pow(float v1, float v2, out float ret)
        {
            ret = 0;
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "Clamp")]
        [ContextMenu(filterStrings: "Clamp", "Math\\Clamp", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Clamp(float x, float min, float max, out float ret)
        {
            ret = 0;
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "Sin")]
        [ContextMenu(filterStrings: "Sin", "Math\\Sin", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Sin(float x, out float sin)
        {
            sin = (float)Math.Sin(x);
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "Cos")]
        [ContextMenu(filterStrings: "Cos", "Math\\Cos", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Cos(float x, out float cos)
        {
            cos = (float)Math.Cos(x);
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "SinRemapped")]
        [ContextMenu(filterStrings: "SinRemapped", "Math\\SinRemapped", TtMaterialGraph.MaterialEditorKeyword)]
        public static float SinRemapped(float SinPhase, float v1, float v2)
        {
            v1 = 0;
            return 0.0f;
        }

        [Rtti.Meta]
        [TtMaterialShader(Name = "SinCos")]
        [ContextMenu(filterStrings: "SinCos", "Math\\SinCos", TtMaterialGraph.MaterialEditorKeyword)]
        public static void SinCos(float x, out float sin, out float cos)
        {
            sin = (float)Math.Sin(x);
            cos = (float)Math.Cos(x);
        }

        [Rtti.Meta]
        [TtMaterialShader(Name = "Ceil")]
        [ContextMenu(filterStrings: "Ceil", "Math\\Ceil", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Ceil(float x, out float ret)
        {
            ret = 0;
        }

        [Rtti.Meta]
        [TtMaterialShader(Name = "Max")]
        [ContextMenu(filterStrings: "Max", "Math\\Max", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Max(float v1, float v2, out float ret)
        {
            ret = Math.Max(v1, v2);
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "Min")]
        [ContextMenu(filterStrings: "Min", "Math\\Min", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Min(float v1, float v2, out float ret)
        {
            ret = Math.Min(v1, v2);
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "Lerp")]
        [ContextMenu(filterStrings: "Lerp", "Math\\Lerp", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Lerp(float v1, float v2, float s, out float ret)
        {
            ret = v1 + s * (v2 - v1);
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "Lerp2D")]
        [ContextMenu(filterStrings: "Lerp2D", "Math\\Lerp2D", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Lerp2D(Vector2 v1, Vector2 v2, Vector2 s, out Vector2 ret)
        {
            ret.X = v1.X + s.X * (v2.X - v1.X);
            ret.Y = v1.Y + s.Y * (v2.Y - v1.Y);
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "Lerp3D")]
        [ContextMenu(filterStrings: "Lerp3D", "Math\\Lerp3D", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Lerp3D(Vector3 v1, Vector3 v2, Vector3 s, out Vector3 ret)
        {
            ret.X = v1.X + s.X * (v2.X - v1.X);
            ret.Y = v1.Y + s.Y * (v2.Y - v1.Y);
            ret.Z = v1.Z + s.Z * (v2.Z - v1.Z);
        }

        [Rtti.Meta]
        [TtMaterialShader(Name = "SmoothStep3D")]
        [ContextMenu(filterStrings: "SmoothStep3D", "Math\\SmoothStep3D", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector3 Smoothstep3D(Vector3 InColor)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "floor3D")]
        [ContextMenu(filterStrings: "floor3D", "Math\\floor3D", TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector3 floor3D(Vector3 InColor)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta]
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
        [Rtti.Meta]
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
        [Rtti.Meta]
        [TtMaterialShader(Name = "Cross3D")]
        [ContextMenu(filterStrings: "Cross3D", "Math\\Cross3D", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Cross3D(Vector3 v1, Vector3 v2, out Vector3 ret)
        {
            ret = Vector3.Zero;
        }

        [Rtti.Meta]
        [TtMaterialShader(Name = "SphereMask")]
        [ContextMenu(filterStrings: "SphereMask", "Math\\SphereMask", TtMaterialGraph.MaterialEditorKeyword)]
        public static float SphereMask(Vector3 A, Vector3 B, float Radius, float Hardness)
        {
            Radius = 2.0f;
            return 0.0f;
        }

        [Rtti.Meta]
        [TtMaterialShader(Name = "RotateAboutAxis")]
        [ContextMenu(filterStrings: "RotateAboutAxis", "Math\\RotateAboutAxis", TtMaterialGraph.MaterialEditorKeyword)]
        public static void RotateAboutAxis(Vector3 rotationAxis, float rotationAngle, Vector3 pivotPos, Vector3 localPos, out Vector3 localOffset)
        {
            localOffset = Vector3.Zero;
        }
        #endregion

        #region Pivot
        [Rtti.Meta]
        [TtMaterialShader(Name = "Pivot_DecodePosition")]
        [ContextMenu("Pivot_DecodePosition", "Pivot\\DecodePosition", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_DecodePosition(Vector3 rgb, out Vector3 localPos)
        {
            localPos = Vector3.Zero;
        }
        [Rtti.Meta]
        [TtMaterialShader(Name = "Pivot_DecodeAxisVector")]
        [ContextMenu("Pivot_DecodeAxisVector", "Pivot\\DecodeAxisVector", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_DecodeAxisVector(Vector3 rgb, out Vector3 localAxis)
        {
            localAxis = Vector3.UnitY;
        }

        [Rtti.Meta]
        [TtMaterialShader(Name = "Pivot_UnpackIntAsFloat")]
        [ContextMenu("Pivot_UnpackIntAsFloat", "Pivot\\UnpackIntAsFloat", TtMaterialGraph.MaterialEditorKeyword)]
        public static float Pivot_UnpackIntAsFloat(float N)
        {
            return 0;
        }

        [Rtti.Meta]
        [TtMaterialShader(Name = "Pivot_GetPivotIndex")]
        [ContextMenu("Pivot_GetPivotIndex", "Pivot\\GetPivotIndex", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_GetPivotIndex(Vector2 uv, Vector2 texSize, out float index)
        {
            index = 0;
        }

        [Rtti.Meta]
        [TtMaterialShader(Name = "Pivot_GetParentPivotData")]
        [ContextMenu("Pivot_GetParentPivotData", "Pivot\\GetParentPivotData", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_GetParentPivotData(float parentIdx, Vector2 texSize, float currentIdx, out Vector2 parentUV, out float isChild)
        {
            parentUV = Vector2.Zero;
            isChild = 0;
        }

        [Rtti.Meta]
        [TtMaterialShader(Name = "Pivot_GetHierarchyData")]
        [ContextMenu("Pivot_GetHierarchyData", "Pivot\\GetHierarchyData", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_GetHierarchyData(float pivotDepth, Vector2 pivot1UV, Vector2 pivot2UV, Vector2 pivot3UV, Vector2 pivot4UV, out Vector2 rootUV, out Vector2 mainBranchUV, out Vector2 smallBranchUV, out Vector2 leaveUV, out float mainBranchMask, out float smallBranchMask, out float leaveMask)
        {
            rootUV = mainBranchUV = smallBranchUV = leaveUV = Vector2.Zero;
            mainBranchMask = smallBranchMask = leaveMask = 0.0f;
        }

        [Rtti.Meta]
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

        [Rtti.Meta]
        [TtMaterialShader(Name = "Pivot_WindAnimation_Sway2")]
        [ContextMenu("Pivot_WindAnimation_Sway2", "Pivot\\WindAnimation_Sway2", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_WindAnimation_Sway2(Vector3 windSwayDirection, float windSwayGustFrequency, float windSwayIntensity, Vector3 localPos, float time, out Vector3 localVertexOffset)
        {
            localVertexOffset = Vector3.Zero;
        }

        [Rtti.Meta]
        [TtMaterialShader(Name = "Pivot_WindAnimation_Sway3")]
        [ContextMenu("Pivot_WindAnimation_Sway3", "Pivot\\WindAnimation_Sway3", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_WindAnimation_Sway3(Vector3 windSwayDirection, float windSwayGustFrequency, float windSwayIntensity, Vector3 localPos, float time, float windSwayEffectOffset, float windSwayEffectFalloff, out Vector3 localVertexOffset)
        {
            localVertexOffset = Vector3.Zero;
        }

        [Rtti.Meta]
        [TtMaterialShader(Name = "Pivot_WindAnimation_Rustle")]
        [ContextMenu("Pivot_WindAnimation_Rustle", "Pivot\\WindAnimation_Rustle", TtMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_WindAnimation_Rustle(float windSpeed, float windIntensity, Vector3 localPos, float time, out Vector3 localVertexOffset)
        {
            localVertexOffset = Vector3.Zero;
        }

        [Rtti.Meta]
        [TtMaterialShader(Name = "Pivot_Gradient")]
        [ContextMenu("Pivot_Gradient", "Pivot\\Pivot_Gradient", TtMaterialGraph.MaterialEditorKeyword)]
        public static float Pivot_Gradient(Vector3 worldPos, float gradientOffset, float gradientFallout)
        {
            return 0;
        }

        [Rtti.Meta]
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

    public class SampleLevel2DNode : CallNode
    {
        public SampleLevel2DNode()
        {
            PrevSize = new Vector2(100, 100);
            TextureVarName = $"Texture_{(uint)NodeId.GetHashCode()}";
            
            mSampler.SetDefault();
        }
        ~SampleLevel2DNode()
        {
            CoreSDK.DisposeObject(ref CmdParameters);
        }
        //public override void OnMaterialEditorGenCode(UMaterial Material)
        //{
        //    var texNode = this;
        //    var texturePinIn = texNode.FindPinIn("texture");
        //    if (texturePinIn.HasLinker() == false)
        //    {
        //        var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
        //        tmp.Name = texNode.TextureVarName;
        //        tmp.ShaderType = "Texture2D";
        //        if (Material.FindSRV(tmp.Name) == null)
        //        {
        //            tmp.Value = texNode.AssetName;
        //            Material.UsedRSView.Add(tmp);
        //        }
        //    }
        //    var samplerPinIn = texNode.FindPinIn("sampler");
        //    if (samplerPinIn.HasLinker() == false)
        //    {
        //        var tmp = new Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair();
        //        tmp.Name = "Samp_" + texNode.TextureVarName;
        //        if (Material.FindSampler(tmp.Name) == null)
        //        {
        //            tmp.Value = texNode.Sampler;
        //            Material.UsedSamplerStates.Add(tmp);
        //        }
        //    }
        //}
        [Rtti.Meta]
        public string TextureVarName { get; set; }
        [Rtti.Meta]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName AssetName
        {
            get
            {
                if (TextureSRV == null)
                    return null;
                return TextureSRV.AssetName;
            }
            set
            {
                if (value == null)
                {
                    TextureSRV = null;
                    return;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);

                    mSlateEffect = await TtEngine.Instance.GfxDevice.EffectManager.GetEffect(
                        await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<EngineNS.Editor.Forms.USlateTextureViewerShading>(),
                        TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial, new Graphics.Mesh.UMdfStaticMesh());
                };
                exec();
            }
        }
        TtEffect mSlateEffect;
        EngineNS.Editor.Forms.TtTextureViewerCmdParams CmdParameters = null;
        NxRHI.FSamplerDesc mSampler;
        [Rtti.Meta]
        public NxRHI.FSamplerDesc Sampler { 
            get => mSampler; 
            set => mSampler = value; }
        private NxRHI.TtSrView TextureSRV;
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null || mSlateEffect == null)
                return;

            unsafe
            {
                if (CmdParameters == null)
                {
                    var rc = TtEngine.Instance.GfxDevice.RenderContext;

                    var iptDesc = new NxRHI.TtInputLayoutDesc();
                    unsafe
                    {
                        iptDesc.mCoreObject.AddElement("POSITION", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, 0, 0, 0);
                        iptDesc.mCoreObject.AddElement("TEXCOORD", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, (uint)sizeof(Vector2), 0, 0);
                        iptDesc.mCoreObject.AddElement("COLOR", 0, EPixelFormat.PXF_R8G8B8A8_UNORM, 0, (uint)sizeof(Vector2) * 2, 0, 0);
                        //iptDesc.SetShaderDesc(SlateEffect.GraphicsEffect);
                    }
                    iptDesc.mCoreObject.SetShaderDesc(mSlateEffect.DescVS.mCoreObject);
                    var InputLayout = rc.CreateInputLayout(iptDesc); //TtEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, iptDesc);
                    mSlateEffect.ShaderEffect.mCoreObject.BindInputLayout(InputLayout.mCoreObject);

                    var cmdParams = EGui.TtImDrawCmdParameters.CreateInstance<EngineNS.Editor.Forms.TtTextureViewerCmdParams>();
                    var cbBinder = mSlateEffect.ShaderEffect.FindBinder("ProjectionMatrixBuffer");
                    cmdParams.CBuffer = rc.CreateCBV(cbBinder);
                    cmdParams.Drawcall.BindShaderEffect(mSlateEffect);
                    cmdParams.Drawcall.BindCBuffer(cbBinder.mCoreObject, cmdParams.CBuffer);
                    cmdParams.Drawcall.BindSRV(TtNameTable.FontTexture, TextureSRV);
                    cmdParams.Drawcall.BindSampler(TtNameTable.Samp_FontTexture, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                    cmdParams.IsNormalMap = 0;
                    if (TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_UNORM || TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_TYPELESS || TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_SNORM)
                        cmdParams.IsNormalMap = 1;

                    CmdParameters = cmdParams;
                }

                var uv0 = new Vector2(0, 0);
                var uv1 = new Vector2(1, 1);
                cmdlist.AddImage((ulong)CmdParameters.GetHandle(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            }

        }
        //protected override OpExpress OnNoneLinkedParameter(UMaterialGraph funGraph, ICodeGen cGen, int i)
        //{
        //    if (Method.Parameters[i].Name == "texture")
        //    {
        //        var retVar = new DefineVar();
        //        retVar.IsLocalVar = false;
        //        retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParameterType);
        //        retVar.VarName = TextureVarName;
        //        return new OpUseDefinedVar(retVar);
        //    }
        //    else if (Method.Parameters[i].Name == "sampler")
        //    {
        //        var retVar = new DefineVar();
        //        retVar.IsLocalVar = false;
        //        retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParameterType);
        //        retVar.VarName = "Samp_" + TextureVarName;
        //        return new OpUseDefinedVar(retVar);
        //    }
        //    else if (Method.Parameters[i].Name == "uv")
        //    {
        //        var retVar = new DefineVar();
        //        retVar.IsLocalVar = false;
        //        retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParameterType);
        //        retVar.VarName = "input.vUV";
        //        return new OpUseDefinedVar(retVar);
        //    }

        //    return base.OnNoneLinkedParameter(funGraph, cGen, i);
        //}

        protected override TtExpressionBase GetNoneLinkedParameterExp(NodeGraph.PinIn pin, int argIdx, ref NodeGraph.BuildCodeStatementsData data)
        {
            var method = Method;
            if (method.Parameters[argIdx].Name == "texture")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "sampler")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "Samp_" + TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "uv")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "input.vUV"
                };
                return retVal;
            }
            return base.GetNoneLinkedParameterExp(pin, argIdx, ref data);
        }
        public override void BuildStatements(NodePin pin, ref NodeGraph.BuildCodeStatementsData data)
        {
            var material = data.UserData as TtMaterial;
            var texturePinIn = FindPinIn("texture");
            if (texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameRNamePair();
                tmp.Name = TextureVarName;
                tmp.ShaderType = "Texture2D";
                if (material.FindSRV(tmp.Name) == null)
                {
                    tmp.Value = AssetName;
                    material.UsedSrView.Add(tmp);
                }
            }
            var samplerPinIn = FindPinIn("sampler");
            if (samplerPinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + TextureVarName;
                if (material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = Sampler;
                    material.UsedSamplerStates.Add(tmp);
                }
            }
            base.BuildStatements(pin, ref data);
        }
    }

    public class Sample2DNode : CallNode
    {
        public Sample2DNode()
        {
            PrevSize = new Vector2(100, 100);
            TextureVarName = $"Texture_{(uint)NodeId.GetHashCode()}";

            mSampler.SetDefault();
        }
        ~Sample2DNode()
        {
            CoreSDK.DisposeObject(ref CmdParameters);
        }
        //public override void OnMaterialEditorGenCode(UMaterial Material)
        //{
        //    var texNode = this;
        //    var texturePinIn = texNode.FindPinIn("texture");
        //    if (texturePinIn.HasLinker() == false)
        //    {
        //        var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
        //        tmp.Name = texNode.TextureVarName;
        //        tmp.ShaderType = "Texture2D";
        //        if (Material.FindSRV(tmp.Name) == null)
        //        {
        //            tmp.Value = texNode.AssetName;
        //            Material.UsedRSView.Add(tmp);
        //        }
        //    }
        //    var samplerPinIn = texNode.FindPinIn("sampler");
        //    if (samplerPinIn.HasLinker() == false)
        //    {
        //        var tmp = new Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair();
        //        tmp.Name = "Samp_" + texNode.TextureVarName;
        //        if (Material.FindSampler(tmp.Name) == null)
        //        {
        //            tmp.Value = texNode.Sampler;
        //            Material.UsedSamplerStates.Add(tmp);
        //        }
        //    }
        //}
        [Rtti.Meta]
        public string TextureVarName { get; set; }
        [Rtti.Meta]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName AssetName
        {
            get
            {
                if (TextureSRV == null)
                    return null;
                return TextureSRV.AssetName;
            }
            set
            {
                if (value == null)
                {
                    TextureSRV = null;
                    return;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);

                    mSlateEffect = await TtEngine.Instance.GfxDevice.EffectManager.GetEffect(
                        await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<EngineNS.Editor.Forms.USlateTextureViewerShading>(),
                        TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial, new Graphics.Mesh.UMdfStaticMesh());

                };
                exec();
            }
        }
        TtEffect mSlateEffect;
        EngineNS.Editor.Forms.TtTextureViewerCmdParams CmdParameters = null;
        NxRHI.FSamplerDesc mSampler;
        [Rtti.Meta]
        public NxRHI.FSamplerDesc Sampler { get => mSampler; set => mSampler = value; }
        private NxRHI.TtSrView TextureSRV;
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null || mSlateEffect == null)
                return;

            unsafe
            {
                if (CmdParameters == null)
                {
                    var rc = TtEngine.Instance.GfxDevice.RenderContext;

                    var iptDesc = new NxRHI.TtInputLayoutDesc();
                    unsafe
                    {
                        iptDesc.mCoreObject.AddElement("POSITION", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, 0, 0, 0);
                        iptDesc.mCoreObject.AddElement("TEXCOORD", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, (uint)sizeof(Vector2), 0, 0);
                        iptDesc.mCoreObject.AddElement("COLOR", 0, EPixelFormat.PXF_R8G8B8A8_UNORM, 0, (uint)sizeof(Vector2) * 2, 0, 0);
                        //iptDesc.SetShaderDesc(SlateEffect.GraphicsEffect);
                    }
                    iptDesc.mCoreObject.SetShaderDesc(mSlateEffect.DescVS.mCoreObject);
                    var InputLayout = rc.CreateInputLayout(iptDesc); //TtEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, iptDesc);
                    mSlateEffect.ShaderEffect.mCoreObject.BindInputLayout(InputLayout.mCoreObject);

                    var cmdParams = EGui.TtImDrawCmdParameters.CreateInstance<EngineNS.Editor.Forms.TtTextureViewerCmdParams>();
                    var cbBinder = mSlateEffect.ShaderEffect.FindBinder("ProjectionMatrixBuffer");
                    cmdParams.CBuffer = rc.CreateCBV(cbBinder);
                    cmdParams.Drawcall.BindShaderEffect(mSlateEffect);
                    cmdParams.Drawcall.BindCBuffer(cbBinder.mCoreObject, cmdParams.CBuffer);
                    cmdParams.Drawcall.BindSRV(TtNameTable.FontTexture, TextureSRV);
                    cmdParams.Drawcall.BindSampler(TtNameTable.Samp_FontTexture, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                    cmdParams.IsNormalMap = 0;
                    if (TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_UNORM || TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_TYPELESS || TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_SNORM)
                        cmdParams.IsNormalMap = 1;

                    CmdParameters = cmdParams;
                }

                var uv0 = new Vector2(0, 0);
                var uv1 = new Vector2(1, 1);
                cmdlist.AddImage((ulong)CmdParameters.GetHandle(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);

                // support preview A channel
                //var textPos = end - new Vector2(32, 32);
                //cmdlist.AddText(textPos, mShowA ? 0xFFFFFFFF : 0x00FF00FF, "A", null);
                //if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false) && ImGuiAPI.IsMouseHoveringRect(textPos, end, true))
                //{
                //    CmdParameters.ColorMask.W = mShowA ? 1 : 0;
                //    mShowA = !mShowA;
                //}
            }
        }
        //protected override OpExpress OnNoneLinkedParameter(UMaterialGraph funGraph, ICodeGen cGen, int i)
        //{
        //    if (Method.Parameters[i].Name == "texture")
        //    {
        //        var retVar = new DefineVar();
        //        retVar.IsLocalVar = false;
        //        retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParameterType);
        //        retVar.VarName = TextureVarName;
        //        return new OpUseDefinedVar(retVar);
        //    }
        //    else if (Method.Parameters[i].Name == "sampler")
        //    {
        //        var retVar = new DefineVar();
        //        retVar.IsLocalVar = false;
        //        retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParameterType);
        //        retVar.VarName = "Samp_" + TextureVarName;
        //        return new OpUseDefinedVar(retVar);
        //    }
        //    else if (Method.Parameters[i].Name == "uv")
        //    {
        //        var retVar = new DefineVar();
        //        retVar.IsLocalVar = false;
        //        retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParameterType);
        //        retVar.VarName = "input.vUV";
        //        return new OpUseDefinedVar(retVar);
        //    }

        //    return base.OnNoneLinkedParameter(funGraph, cGen, i);
        //}
        protected override TtExpressionBase GetNoneLinkedParameterExp(NodeGraph.PinIn pin, int argIdx, ref NodeGraph.BuildCodeStatementsData data)
        {
            var method = Method;
            if(method.Parameters[argIdx].Name == "texture")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = TextureVarName
                };
                return retVal;
            }
            else if(method.Parameters[argIdx].Name == "sampler")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "Samp_" + TextureVarName
                };
                return retVal;
            }
            else if(method.Parameters[argIdx].Name == "uv")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "input.vUV"
                };
                return retVal;
            }
            return base.GetNoneLinkedParameterExp(pin, argIdx, ref data);
        }
        public override void BuildStatements(NodePin pin, ref NodeGraph.BuildCodeStatementsData data)
        {
            var material = data.UserData as TtMaterial;
            var texturePinIn = FindPinIn("texture");
            if(texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameRNamePair();
                tmp.Name = TextureVarName;
                tmp.ShaderType = "Texture2D";
                if (material.FindSRV(tmp.Name) == null)
                {
                    tmp.Value = AssetName;
                    material.UsedSrView.Add(tmp);
                }
            }
            var samplerPinIn = FindPinIn("sampler");
            if (samplerPinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + TextureVarName;
                if (material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = Sampler;
                    material.UsedSamplerStates.Add(tmp);
                }
            }
            base.BuildStatements(pin, ref data);
        }
    }

    public class SampleArrayLevel2DNode : CallNode
    {
        public SampleArrayLevel2DNode()
        {
            PrevSize = new Vector2(100, 100);
            TextureVarName = $"TextureArray_{(uint)NodeId.GetHashCode()}";

            mSampler.SetDefault();
        }
        ~SampleArrayLevel2DNode()
        {
        }
        //public override void OnMaterialEditorGenCode(UMaterial Material)
        //{
        //    var texNode = this;
        //    var texturePinIn = texNode.FindPinIn("texture");
        //    if (texturePinIn.HasLinker() == false)
        //    {
        //        var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
        //        tmp.Name = texNode.TextureVarName;
        //        tmp.ShaderType = "Texture2DArray";
        //        if (Material.FindSRV(tmp.Name) == null)
        //        {
        //            tmp.Value = texNode.AssetName;
        //            Material.UsedRSView.Add(tmp);
        //        }
        //    }
        //    var samplerPinIn = texNode.FindPinIn("sampler");
        //    if (samplerPinIn.HasLinker() == false)
        //    {
        //        var tmp = new Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair();
        //        tmp.Name = "Samp_" + texNode.TextureVarName;
        //        if (Material.FindSampler(tmp.Name) == null)
        //        {
        //            tmp.Value = texNode.Sampler;
        //            Material.UsedSamplerStates.Add(tmp);
        //        }
        //    }
        //}
        [Rtti.Meta]
        public string TextureVarName { get; set; }
        [Rtti.Meta]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName AssetName
        {
            get
            {
                if (TextureSRV == null)
                    return null;
                return TextureSRV.AssetName;
            }
            set
            {
                if (value == null)
                {
                    TextureSRV = null;
                    return;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                exec();
            }
        }
        NxRHI.FSamplerDesc mSampler;
        [Rtti.Meta]
        public NxRHI.FSamplerDesc Sampler { get => mSampler; set => mSampler = value; }
        private NxRHI.TtSrView TextureSRV;
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null)
                return;

            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            unsafe
            {
                cmdlist.AddImage((ulong)TextureSRV.GetTextureHandle(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            }
        }
        //protected override OpExpress OnNoneLinkedParameter(UMaterialGraph funGraph, ICodeGen cGen, int i)
        //{
        //    if (Method.Parameters[i].Name == "texture")
        //    {
        //        var retVar = new DefineVar();
        //        retVar.IsLocalVar = false;
        //        retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParameterType);
        //        retVar.VarName = TextureVarName;
        //        return new OpUseDefinedVar(retVar);
        //    }
        //    else if (Method.Parameters[i].Name == "sampler")
        //    {
        //        var retVar = new DefineVar();
        //        retVar.IsLocalVar = false;
        //        retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParameterType);
        //        retVar.VarName = "Samp_" + TextureVarName;
        //        return new OpUseDefinedVar(retVar);
        //    }
        //    else if (Method.Parameters[i].Name == "uv")
        //    {
        //        var retVar = new DefineVar();
        //        retVar.IsLocalVar = false;
        //        retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParameterType);
        //        retVar.VarName = "input.vUV";
        //        return new OpUseDefinedVar(retVar);
        //    }

        //    return base.OnNoneLinkedParameter(funGraph, cGen, i);
        //}
        protected override TtExpressionBase GetNoneLinkedParameterExp(NodeGraph.PinIn pin, int argIdx, ref NodeGraph.BuildCodeStatementsData data)
        {
            var method = Method;
            if (method.Parameters[argIdx].Name == "texture")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "sampler")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "Samp_" + TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "uv")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "input.vUV"
                };
                return retVal;
            }
            return base.GetNoneLinkedParameterExp(pin, argIdx, ref data);
        }
        public override void BuildStatements(NodePin pin, ref NodeGraph.BuildCodeStatementsData data)
        {
            var material = data.UserData as TtMaterial;
            var texturePinIn = FindPinIn("texture");
            if (texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameRNamePair();
                tmp.Name = TextureVarName;
                tmp.ShaderType = "Texture2D";
                if (material.FindSRV(tmp.Name) == null)
                {
                    tmp.Value = AssetName;
                    material.UsedSrView.Add(tmp);
                }
            }
            var samplerPinIn = FindPinIn("sampler");
            if (samplerPinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + TextureVarName;
                if (material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = Sampler;
                    material.UsedSamplerStates.Add(tmp);
                }
            }
            base.BuildStatements(pin, ref data);
        }
    }

    public class SampleArray2DNode : CallNode
    {
        public SampleArray2DNode()
        {
            PrevSize = new Vector2(100, 100);
            TextureVarName = $"TextureArray_{(uint)NodeId.GetHashCode()}";

            mSampler.SetDefault();
        }
        ~SampleArray2DNode()
        {
        }
        //public override void OnMaterialEditorGenCode(UMaterial Material)
        //{
        //    var texNode = this;
        //    var texturePinIn = texNode.FindPinIn("texture");
        //    if (texturePinIn.HasLinker() == false)
        //    {
        //        var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
        //        tmp.Name = texNode.TextureVarName;
        //        tmp.ShaderType = "Texture2DArray";
        //        if (Material.FindSRV(tmp.Name) == null)
        //        {
        //            tmp.Value = texNode.AssetName;
        //            Material.UsedRSView.Add(tmp);
        //        }
        //    }
        //    var samplerPinIn = texNode.FindPinIn("sampler");
        //    if (samplerPinIn.HasLinker() == false)
        //    {
        //        var tmp = new Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair();
        //        tmp.Name = "Samp_" + texNode.TextureVarName;
        //        if (Material.FindSampler(tmp.Name) == null)
        //        {
        //            tmp.Value = texNode.Sampler;
        //            Material.UsedSamplerStates.Add(tmp);
        //        }
        //    }
        //}
        [Rtti.Meta]
        public string TextureVarName { get; set; }
        [Rtti.Meta]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName AssetName
        {
            get
            {
                if (TextureSRV == null)
                    return null;
                return TextureSRV.AssetName;
            }
            set
            {
                if (value == null)
                {
                    TextureSRV = null;
                    return;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                exec();
            }
        }
        NxRHI.FSamplerDesc mSampler;
        [Rtti.Meta]
        public NxRHI.FSamplerDesc Sampler { get => mSampler; set => mSampler = value; }
        private NxRHI.TtSrView TextureSRV;
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null)
                return;

            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            unsafe
            {
                cmdlist.AddImage((ulong)TextureSRV.GetTextureHandle(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            }
        }
        //protected override OpExpress OnNoneLinkedParameter(UMaterialGraph funGraph, ICodeGen cGen, int i)
        //{
        //    if (Method.Parameters[i].Name == "texture")
        //    {
        //        var retVar = new DefineVar();
        //        retVar.IsLocalVar = false;
        //        retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParameterType);
        //        retVar.VarName = TextureVarName;
        //        return new OpUseDefinedVar(retVar);
        //    }
        //    else if (Method.Parameters[i].Name == "sampler")
        //    {
        //        var retVar = new DefineVar();
        //        retVar.IsLocalVar = false;
        //        retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParameterType);
        //        retVar.VarName = "Samp_" + TextureVarName;
        //        return new OpUseDefinedVar(retVar);
        //    }
        //    else if (Method.Parameters[i].Name == "uv")
        //    {
        //        var retVar = new DefineVar();
        //        retVar.IsLocalVar = false;
        //        retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParameterType);
        //        retVar.VarName = "input.vUV";
        //        return new OpUseDefinedVar(retVar);
        //    }

        //    return base.OnNoneLinkedParameter(funGraph, cGen, i);
        //}
        protected override TtExpressionBase GetNoneLinkedParameterExp(NodeGraph.PinIn pin, int argIdx, ref NodeGraph.BuildCodeStatementsData data)
        {
            var method = Method;
            if (method.Parameters[argIdx].Name == "texture")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "sampler")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "Samp_" + TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "uv")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "input.vUV"
                };
                return retVal;
            }
            return base.GetNoneLinkedParameterExp(pin, argIdx, ref data);
        }
        public override void BuildStatements(NodePin pin, ref NodeGraph.BuildCodeStatementsData data)
        {
            var material = data.UserData as TtMaterial;
            var texturePinIn = FindPinIn("texture");
            if (texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameRNamePair();
                tmp.Name = TextureVarName;
                tmp.ShaderType = "Texture2D";
                if (material.FindSRV(tmp.Name) == null)
                {
                    tmp.Value = AssetName;
                    material.UsedSrView.Add(tmp);
                }
            }
            var samplerPinIn = FindPinIn("sampler");
            if (samplerPinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + TextureVarName;
                if (material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = Sampler;
                    material.UsedSamplerStates.Add(tmp);
                }
            }
            base.BuildStatements(pin, ref data);
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
