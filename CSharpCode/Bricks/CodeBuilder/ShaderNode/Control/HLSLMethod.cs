using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Control
{
    [Rtti.Meta]
    public partial class HLSLMethod
    {
        [Rtti.Meta]
        [UserCallNode(CallNodeType = typeof(SampleLevel2DNode))]
        [ContextMenu("samplelevel2d", "Sample\\Level2D", UMaterialGraph.MaterialEditorKeyword)]
        public static Vector4 SampleLevel2D(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, float level, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        [UserCallNode(CallNodeType = typeof(Sample2DNode))]
        public static Vector4 Sample2D(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        [UserCallNode(CallNodeType = typeof(SampleArrayLevel2DNode))]
        public static Vector4 SampleArrayLevel2D(Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, float level, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        [UserCallNode(CallNodeType = typeof(SampleArray2DNode))]
        public static Vector4 SampleArray2D(Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        public static Vector3 GetTerrainDiffuse(Vector2 uv, Graphics.Pipeline.Shader.UMaterial.PSInput input)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta]
        public static Vector3 GetTerrainNormal(Vector2 uv, Graphics.Pipeline.Shader.UMaterial.PSInput input)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta]
        public static void Clamp(float x, float min, float max, out float ret)
        {
            ret = 0;
        }
        [Rtti.Meta]
        public static void SinCos(float x, out float sin, out float cos)
        {
            sin = (float)Math.Sin(x);
            cos = (float)Math.Cos(x);
        }
        [Rtti.Meta]
        public static void Max(float v1, float v2, out float ret)
        {
            ret = Math.Max(v1, v2);
        }
        [Rtti.Meta]
        public static void Min(float v1, float v2, out float ret)
        {
            ret = Math.Min(v1, v2);
        }
        [Rtti.Meta]
        public static void Lerp(float v1, float v2, float s, out float ret)
        {
            ret = v1 + s * (v2 - v1);
        }
        [Rtti.Meta]
        public static void Lerp2D(Vector2 v1, Vector2 v2, Vector2 s, out Vector2 ret)
        {
            ret.X = v1.X + s.X * (v2.X - v1.X);
            ret.Y = v1.Y + s.Y * (v2.Y - v1.Y);
        }
        [Rtti.Meta]
        public static void Lerp3D(Vector3 v1, Vector3 v2, Vector3 s, out Vector3 ret)
        {
            ret.X = v1.X + s.X * (v2.X - v1.X);
            ret.Y = v1.Y + s.Y * (v2.Y - v1.Y);
            ret.Z = v1.Z + s.Z * (v2.Z - v1.Z);
        }
        [Rtti.Meta]
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
        public static void Distortion(Vector4 localPos, Vector4 localNorm, Vector4 viewPos, Vector4 projPos, Vector3 localCameraPos, float strength, float transparency, float distortionOffset, out Vector2 distortionUV, out float distortionAlpha)
        {
            distortionUV = Vector2.Zero;
            distortionAlpha = 0;
        }
        [Rtti.Meta]
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
        public static Vector3 smoothstep3D(Vector3 InColor)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta]
        public static Vector3 floor3D(Vector3 InColor)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta]
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
        public static void PolarCoodD2P(Vector2 uv, out Vector2 polar)
        {
            float pi;
            pi = 3.1415926f;
            float alpha;
            float a, b, x, y, x1, y1, r1, r2;
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
        [RName.PGRName(FilterExts = RHI.CShaderResourceView.AssetExt)]
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
                    TextureSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                exec();
            }
        }
        ISamplerStateDesc mSampler;
        [Rtti.Meta]
        public ISamplerStateDesc Sampler { get => mSampler; set => mSampler = value; }
        private RHI.CShaderResourceView TextureSRV;
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
        public override void BuildStatements(ref NodeGraph.BuildCodeStatementsData data)
        {
            var material = data.UserData as UMaterial;
            var texturePinIn = FindPinIn("texture");
            if (texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
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
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + TextureVarName;
                if (material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = Sampler;
                    material.UsedSamplerStates.Add(tmp);
                }
            }
            base.BuildStatements(ref data);
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
        [RName.PGRName(FilterExts = RHI.CShaderResourceView.AssetExt)]
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
                    TextureSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                exec();
            }
        }
        ISamplerStateDesc mSampler;
        [Rtti.Meta]
        public ISamplerStateDesc Sampler { get => mSampler; set => mSampler = value; }
        private RHI.CShaderResourceView TextureSRV;
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
        public override void BuildStatements(ref NodeGraph.BuildCodeStatementsData data)
        {
            var material = data.UserData as UMaterial;
            var texturePinIn = FindPinIn("texture");
            if(texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
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
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + TextureVarName;
                if (material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = Sampler;
                    material.UsedSamplerStates.Add(tmp);
                }
            }
            base.BuildStatements(ref data);
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
        [RName.PGRName(FilterExts = RHI.CShaderResourceView.AssetExt)]
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
                    TextureSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                exec();
            }
        }
        ISamplerStateDesc mSampler;
        [Rtti.Meta]
        public ISamplerStateDesc Sampler { get => mSampler; set => mSampler = value; }
        private RHI.CShaderResourceView TextureSRV;
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
        public override void BuildStatements(ref NodeGraph.BuildCodeStatementsData data)
        {
            var material = data.UserData as UMaterial;
            var texturePinIn = FindPinIn("texture");
            if (texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
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
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + TextureVarName;
                if (material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = Sampler;
                    material.UsedSamplerStates.Add(tmp);
                }
            }
            base.BuildStatements(ref data);
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
        [RName.PGRName(FilterExts = RHI.CShaderResourceView.AssetExt)]
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
                    TextureSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                exec();
            }
        }
        ISamplerStateDesc mSampler;
        [Rtti.Meta]
        public ISamplerStateDesc Sampler { get => mSampler; set => mSampler = value; }
        private RHI.CShaderResourceView TextureSRV;
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
        public override void BuildStatements(ref NodeGraph.BuildCodeStatementsData data)
        {
            var material = data.UserData as UMaterial;
            var texturePinIn = FindPinIn("texture");
            if (texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
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
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + TextureVarName;
                if (material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = Sampler;
                    material.UsedSamplerStates.Add(tmp);
                }
            }
            base.BuildStatements(ref data);
        }
    }
}
