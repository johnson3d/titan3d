using EngineNS.Bricks.NodeGraph;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Control
{
    public class TtHLSLProviderAttribute : Attribute
    {
        public string Name;
        public string Include;
    }

    [Rtti.Meta]
    [TtHLSLProvider]
    public partial class TtCoreHLSLMethod
    {
        [Rtti.Meta]
        [TtHLSLProvider(Name = "SampleLevel2D")]
        [UserCallNode(CallNodeType = typeof(SampleLevel2DNode))]
        [ContextMenu("samplelevel2d", "Sample\\Level2D", UMaterialGraph.MaterialEditorKeyword)]
        public static Vector4 SampleLevel2D(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, float level, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "Sample2D")]
        [UserCallNode(CallNodeType = typeof(Sample2DNode))]
        public static Vector4 Sample2D(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "SampleArrayLevel2D")]
        [UserCallNode(CallNodeType = typeof(SampleArrayLevel2DNode))]
        public static Vector4 SampleArrayLevel2D(Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, float level, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "SampleArray2D")]
        [UserCallNode(CallNodeType = typeof(SampleArray2DNode))]
        public static Vector4 SampleArray2D(Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "TextureSize")]
        public static Vector2 TextureSize(Var.Texture2D texture)
        {
            return Vector2.Zero;
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "UnpackNormal")]
        public static void UnpackNormal(Vector3 packedNormal, out Vector3 normal)
        {
            normal = packedNormal * 2.0f - Vector3.One;
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "GetTerrainDiffuse")]
        public static Vector3 GetTerrainDiffuse(Vector2 uv, Graphics.Pipeline.Shader.PS_INPUT input)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "GetTerrainNormal")]
        public static Vector3 GetTerrainNormal(Vector2 uv, Graphics.Pipeline.Shader.PS_INPUT input)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "Frac")]
        public static void Frac(float x, out float ret)
        {
            ret = 0;
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "Pow")]
        public static void Pow(float v1, float v2, out float ret)
        {
            ret = 0;
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "Clamp")]
        public static void Clamp(float x, float min, float max, out float ret)
        {
            ret = 0;
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "Sin")]
        public static void Sin(float x, out float sin)
        {
            sin = (float)Math.Sin(x);
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "Cos")]
        public static void Cos(float x, out float cos)
        {
            cos = (float)Math.Cos(x);
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "SinRemapped")]
        public static float SinRemapped(float SinPhase, float v1, float v2)
        {
            v1 = 0;
            return 0.0f;
        }

        [Rtti.Meta]
        [TtHLSLProvider(Name = "SinCos")]
        public static void SinCos(float x, out float sin, out float cos)
        {
            sin = (float)Math.Sin(x);
            cos = (float)Math.Cos(x);
        }

        [Rtti.Meta]
        [TtHLSLProvider(Name = "Ceil")]
        public static void Ceil(float x, out float ret)
        {
            ret = 0;
        }

        [Rtti.Meta]
        [TtHLSLProvider(Name = "Max")]
        public static void Max(float v1, float v2, out float ret)
        {
            ret = Math.Max(v1, v2);
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "Min")]
        public static void Min(float v1, float v2, out float ret)
        {
            ret = Math.Min(v1, v2);
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "Lerp")]
        public static void Lerp(float v1, float v2, float s, out float ret)
        {
            ret = v1 + s * (v2 - v1);
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "Lerp2D")]
        public static void Lerp2D(Vector2 v1, Vector2 v2, Vector2 s, out Vector2 ret)
        {
            ret.X = v1.X + s.X * (v2.X - v1.X);
            ret.Y = v1.Y + s.Y * (v2.Y - v1.Y);
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "Lerp3D")]
        public static void Lerp3D(Vector3 v1, Vector3 v2, Vector3 s, out Vector3 ret)
        {
            ret.X = v1.X + s.X * (v2.X - v1.X);
            ret.Y = v1.Y + s.Y * (v2.Y - v1.Y);
            ret.Z = v1.Z + s.Z * (v2.Z - v1.Z);
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "NormalMap")]
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
        [TtHLSLProvider(Name = "Panner")]
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
        [TtHLSLProvider(Name = "Rotator")]
        public static void Rotator(Vector2 uv, float time, Vector2 center, Vector2 scale, float speed, out Vector2 outUV)
        {
            outUV = Vector2.Zero;
            /*
             * float angle = time * speed;

	//matrix <float, 4, 4> scaleM = {
	float4x4 scaleM = {
		1.0f, 0.0f, 0.0f, 0.0f, // row 1
		0.0f, 1.0f, 0.0f, 0.0f, // row 2
		0.0f, 0.0f, 1.0f, 0.0f, // row 3
		0.0f, 0.0f, 0.0f, 1.0f, // row 4
	};

	scaleM[0][0] = 1 / scale.x;
	scaleM[1][1] = 1 / scale.y;
	// Skip matrix concat since first matrix update
	scaleM[3][0] = (-0.5f * scaleM[0][0]) + 0.5f;
	scaleM[3][1] = (-0.5f * scaleM[1][1]) + 0.5f;

	//matrix <float, 4, 4> trans = {
	float4x4 trans = {
		1.0f, 0.0f, 0.0f, 0.0f, // row 1
		0.0f, 1.0f, 0.0f, 0.0f, // row 2
		0.0f, 0.0f, 1.0f, 0.0f, // row 3
		0.0f, 0.0f, 0.0f, 1.0f, // row 4
	};

	trans[3][0] = center.x;
	trans[3][1] = center.y;

	//matrix <float, 4, 4> rot = { 
	float4x4 rot = { 
		1.0f, 0.0f, 0.0f, 0.0f, // row 1
		0.0f, 1.0f, 0.0f, 0.0f, // row 2
		0.0f, 0.0f, 1.0f, 0.0f, // row 3
		0.0f, 0.0f, 0.0f, 1.0f, // row 4
	};

	float theta = radians(angle);
	float cosTheta = cos(theta);
	float sinTheta = sin(theta);


	rot[0][0] = cosTheta;
	rot[1][0] = -sinTheta;
	rot[0][1] = sinTheta;
	rot[1][1] = cosTheta;

	rot[3][0] = 0.5f + ((-0.5f * cosTheta) - (-0.5f * sinTheta));
	rot[3][1] = 0.5f + ((-0.5f * sinTheta) + (-0.5f * cosTheta));


	float4 inUV = { uv.x, uv.y, 1.0f, 1.0f };
	inUV = mul(inUV, scaleM);
	inUV = mul(inUV, trans);
	outUV = mul(inUV, rot).xy;
            */
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "TransformToWorldPos")]
        public static void TransformToWorldPos(Vector3 localPos, out Vector3 worldPos)
        {
            worldPos = Vector3.Zero;
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "Distortion")]
        public static void Distortion(Vector4 localPos, Vector4 localNorm, Vector4 viewPos, Vector4 projPos, Vector3 localCameraPos, float strength, float transparency, float distortionOffset, out Vector2 distortionUV, out float distortionAlpha)
        {
            distortionUV = Vector2.Zero;
            distortionAlpha = 0;
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "RimLight")]
        public static void RimLight(Vector3 localPos, Vector3 localNormal, float rimStart, float rimEnd, Vector4 rimColor, float rimMultiply, out Vector3 outColor)
        {
            outColor = Vector3.Zero;
            //var l = localNormal.Length();
            //if (l == 0)
            //{
            //    outColor = Vector4.Zero;
            //    return;
            //}
            //var N = Vector3.Normalize(localNormal);
            //var V = Vector3.Normalize(CameraPositionInModel - localPos);
            //var rim = (half)smoothstep(rimStart, rimEnd, 1 - dot(N, V));

            //outColor = rim * rimMultiply * rimColor;
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "VecMultiplyQuat")]
        public static void VecMultiplyQuat(Vector3 vec, Vector4 quat, out Vector3 outVector)
        {
            outVector = Vector3.Zero;
            //var uv = Vector3.Cross(quat.xyz, vec);
            //var uuv = Vector3.Cross(quat.xyz, uv);
            //uv = uv * ((half)2.0f * quat.w);
            //uuv *= (half)2.0f;

            //outVector = vec + uv + uuv;
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "SmoothStep3D")]
        public static Vector3 Smoothstep3D(Vector3 InColor)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "floor3D")]
        public static Vector3 floor3D(Vector3 InColor)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "PolarCoodP2D")]
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
        [TtHLSLProvider(Name = "PolarCoodD2P")]
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
        //[Rtti.Meta]
        //public static void GetGridUV(Vector2 uv, Vector4 lightmapUV, Vector2 min, Vector2 max, out Vector2 outUV)
        //{
        //    var v = new Vector2(lightmapUV.X, lightmapUV.Y);
        //    var u = new Vector2(lightmapUV.Z, lightmapUV.W);
        //    var min_v = new Vector2(min.Y);
        //    var max_v = new Vector2(max.Y);
        //    var min_u = new Vector2(min.X);
        //    var max_u = new Vector2(max.X);
        //    var t1 = Vector2.Lerp(in min_u, in max_u, in u);
        //    var t2 = Vector2.Lerp(in min_v, in max_v, in v);

        //    var slt = new Vector2[4]
        //        {
        //            new Vector2(t1.X, t2.X),
        //            new Vector2(t1.X, t2.Y),
        //            new Vector2(t1.Y, t2.Y),
        //            new Vector2(t1.Y, t2.X),
        //        };
        //    outUV = slt[(int)uv.X];
        //}

        [Rtti.Meta]
        [TtHLSLProvider(Name = "Cross3D")]
        public static void Cross3D(Vector3 v1, Vector3 v2, out Vector3 ret)
        {
            ret = Vector3.Zero;
        }

        [Rtti.Meta]
        [TtHLSLProvider(Name = "SphereMask")]
        public static float SphereMask(Vector3 A, Vector3 B, float Radius, float Hardness)
        {
            Radius = 2.0f;
            return 0.0f;
        }

        [Rtti.Meta]
        [TtHLSLProvider(Name = "RotateAboutAxis")]
        public static void RotateAboutAxis(Vector3 rotationAxis, float rotationAngle, Vector3 pivotPos, Vector3 localPos, out Vector3 localOffset)
        {
            localOffset = Vector3.Zero;
        }

        [Rtti.Meta]
        [TtHLSLProvider(Name = "Pivot_DecodePosition")]
        [ContextMenu("Pivot_DecodePosition", "Pivot\\DecodePosition", UMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_DecodePosition(Vector3 rgb, out Vector3 localPos)
        {
            localPos = Vector3.Zero;
        }
        [Rtti.Meta]
        [TtHLSLProvider(Name = "Pivot_DecodeAxisVector")]
        [ContextMenu("Pivot_DecodeAxisVector", "Pivot\\DecodeAxisVector", UMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_DecodeAxisVector(Vector3 rgb, out Vector3 localAxis)
        {
            localAxis = Vector3.UnitY;
        }

        [Rtti.Meta]
        [TtHLSLProvider(Name = "Pivot_UnpackIntAsFloat")]
        [ContextMenu("Pivot_UnpackIntAsFloat", "Pivot\\UnpackIntAsFloat", UMaterialGraph.MaterialEditorKeyword)]
        public static float Pivot_UnpackIntAsFloat(float N)
        {
            return 0;
        }

        [Rtti.Meta]
        [TtHLSLProvider(Name = "Pivot_GetPivotIndex")]
        [ContextMenu("Pivot_GetPivotIndex", "Pivot\\GetPivotIndex", UMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_GetPivotIndex(Vector2 uv, Vector2 texSize, out float index)
        {
            index = 0;
        }

        [Rtti.Meta]
        [TtHLSLProvider(Name = "Pivot_GetParentPivotData")]
        [ContextMenu("Pivot_GetParentPivotData", "Pivot\\GetParentPivotData", UMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_GetParentPivotData(float parentIdx, Vector2 texSize, float currentIdx, out Vector2 parentUV, out float isChild)
        {
            parentUV = Vector2.Zero;
            isChild = 0;
        }

        [Rtti.Meta]
        [TtHLSLProvider(Name = "Pivot_GetHierarchyData")]
        [ContextMenu("Pivot_GetHierarchyData", "Pivot\\GetHierarchyData", UMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_GetHierarchyData(float pivotDepth, Vector2 pivot1UV, Vector2 pivot2UV, Vector2 pivot3UV, Vector2 pivot4UV, out Vector2 rootUV, out Vector2 mainBranchUV, out Vector2 smallBranchUV, out Vector2 leaveUV, out float mainBranchMask, out float smallBranchMask, out float leaveMask)
        {
            rootUV = mainBranchUV = smallBranchUV = leaveUV = Vector2.Zero;
            mainBranchMask = smallBranchMask = leaveMask = 0.0f;
        }

        [Rtti.Meta]
        [TtHLSLProvider(Name = "Pivot_WindAnimation")]
        [ContextMenu("Pivot_WindAnimation", "Pivot\\WindAnimation", UMaterialGraph.MaterialEditorKeyword)]
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
        [TtHLSLProvider(Name = "Pivot_WindAnimation_Sway2")]
        [ContextMenu("Pivot_WindAnimation_Sway2", "Pivot\\WindAnimation_Sway2", UMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_WindAnimation_Sway2(Vector3 windSwayDirection, float windSwayGustFrequency, float windSwayIntensity, Vector3 localPos, float time, out Vector3 localVertexOffset)
        {
            localVertexOffset = Vector3.Zero;
        }

        [Rtti.Meta]
        [TtHLSLProvider(Name = "Pivot_WindAnimation_Sway3")]
        [ContextMenu("Pivot_WindAnimation_Sway3", "Pivot\\WindAnimation_Sway3", UMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_WindAnimation_Sway3(Vector3 windSwayDirection, float windSwayGustFrequency, float windSwayIntensity, Vector3 localPos, float time, float windSwayEffectOffset, float windSwayEffectFalloff, out Vector3 localVertexOffset)
        {
            localVertexOffset = Vector3.Zero;
        }

        [Rtti.Meta]
        [TtHLSLProvider(Name = "Pivot_WindAnimation_Rustle")]
        [ContextMenu("Pivot_WindAnimation_Rustle", "Pivot\\WindAnimation_Rustle", UMaterialGraph.MaterialEditorKeyword)]
        public static void Pivot_WindAnimation_Rustle(float windSpeed, float windIntensity, Vector3 localPos, float time, out Vector3 localVertexOffset)
        {
            localVertexOffset = Vector3.Zero;
        }

        [Rtti.Meta]
        [TtHLSLProvider(Name = "Pivot_Gradient")]
        [ContextMenu("Pivot_Gradient", "Pivot\\Pivot_Gradient", UMaterialGraph.MaterialEditorKeyword)]
        public static float Pivot_Gradient(Vector3 worldPos, float gradientOffset, float gradientFallout)
        {
            return 0;
        }

        [Rtti.Meta]
        [TtHLSLProvider(Name = "Pivot_LeafNormal")]
        [ContextMenu("Pivot_LeafNormal", "Pivot\\Pivot_LeafNormal", UMaterialGraph.MaterialEditorKeyword)]
        public static Vector3 Pivot_LeafNormal(bool frontFace, Vector3 normal)
        {
            return Vector3.UnitY;
        }
    }
    public partial class TtHLSLMethodManager
    {
        public Dictionary<string, Rtti.TtClassMeta.TtMethodMeta> Methods { get; } = new Dictionary<string, Rtti.TtClassMeta.TtMethodMeta>();
        public void SureMethods()
        {
            if (Methods.Count > 0)
                return;
            foreach (var i in Rtti.TtClassMetaManager.Instance.Metas)
            {
                var hlslAttr = i.Value.ClassType.GetCustomAttribute<TtHLSLProviderAttribute>(false);
                if (hlslAttr == null)
                    continue;
                foreach (var j in i.Value.Methods)
                {
                    var methodAttr = j.GetMethod().GetCustomAttribute<TtHLSLProviderAttribute>();
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

                    var iptDesc = new NxRHI.UInputLayoutDesc();
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
                cmdlist.AddImage(CmdParameters.GetHandle(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
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

        protected override UExpressionBase GetNoneLinkedParameterExp(NodeGraph.PinIn pin, int argIdx, ref NodeGraph.BuildCodeStatementsData data)
        {
            var method = Method;
            if (method.Parameters[argIdx].Name == "texture")
            {
                var retVal = new UVariableReferenceExpression()
                {
                    VariableName = TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "sampler")
            {
                var retVal = new UVariableReferenceExpression()
                {
                    VariableName = "Samp_" + TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "uv")
            {
                var retVal = new UVariableReferenceExpression()
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
                    material.UsedRSView.Add(tmp);
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

                    var iptDesc = new NxRHI.UInputLayoutDesc();
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
                cmdlist.AddImage(CmdParameters.GetHandle(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);

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
        protected override UExpressionBase GetNoneLinkedParameterExp(NodeGraph.PinIn pin, int argIdx, ref NodeGraph.BuildCodeStatementsData data)
        {
            var method = Method;
            if(method.Parameters[argIdx].Name == "texture")
            {
                var retVal = new UVariableReferenceExpression()
                {
                    VariableName = TextureVarName
                };
                return retVal;
            }
            else if(method.Parameters[argIdx].Name == "sampler")
            {
                var retVal = new UVariableReferenceExpression()
                {
                    VariableName = "Samp_" + TextureVarName
                };
                return retVal;
            }
            else if(method.Parameters[argIdx].Name == "uv")
            {
                var retVal = new UVariableReferenceExpression()
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
                    material.UsedRSView.Add(tmp);
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
                cmdlist.AddImage(TextureSRV.GetTextureHandle().ToPointer(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
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
        protected override UExpressionBase GetNoneLinkedParameterExp(NodeGraph.PinIn pin, int argIdx, ref NodeGraph.BuildCodeStatementsData data)
        {
            var method = Method;
            if (method.Parameters[argIdx].Name == "texture")
            {
                var retVal = new UVariableReferenceExpression()
                {
                    VariableName = TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "sampler")
            {
                var retVal = new UVariableReferenceExpression()
                {
                    VariableName = "Samp_" + TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "uv")
            {
                var retVal = new UVariableReferenceExpression()
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
                    material.UsedRSView.Add(tmp);
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
                cmdlist.AddImage(TextureSRV.GetTextureHandle().ToPointer(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
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
        protected override UExpressionBase GetNoneLinkedParameterExp(NodeGraph.PinIn pin, int argIdx, ref NodeGraph.BuildCodeStatementsData data)
        {
            var method = Method;
            if (method.Parameters[argIdx].Name == "texture")
            {
                var retVal = new UVariableReferenceExpression()
                {
                    VariableName = TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "sampler")
            {
                var retVal = new UVariableReferenceExpression()
                {
                    VariableName = "Samp_" + TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "uv")
            {
                var retVal = new UVariableReferenceExpression()
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
                    material.UsedRSView.Add(tmp);
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
        Bricks.CodeBuilder.ShaderNode.Control.TtHLSLMethodManager mHLSLMethodManager = new Bricks.CodeBuilder.ShaderNode.Control.TtHLSLMethodManager();
        public Bricks.CodeBuilder.ShaderNode.Control.TtHLSLMethodManager HLSLMethodManager 
        { 
            get
            {
                mHLSLMethodManager.SureMethods();
                return mHLSLMethodManager;
            }
        }
    }
}
