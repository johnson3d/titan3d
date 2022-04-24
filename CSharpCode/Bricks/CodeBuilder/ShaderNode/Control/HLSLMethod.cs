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
        public override void OnMaterialEditorGenCode(Bricks.CodeBuilder.HLSL.UHLSLGen gen, UMaterial Material)
        {
            var texNode = this;
            var texturePinIn = texNode.FindPinIn("texture");
            if (texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
                tmp.Name = texNode.TextureVarName;
                tmp.ShaderType = "Texture2D";
                if (Material.FindSRV(tmp.Name) == null)
                {
                    tmp.Value = texNode.AssetName;
                    Material.UsedRSView.Add(tmp);
                }
            }
            var samplerPinIn = texNode.FindPinIn("sampler");
            if (samplerPinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + texNode.TextureVarName;
                if (Material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = texNode.Sampler;
                    Material.UsedSamplerStates.Add(tmp);
                }
            }
        }
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
        protected override OpExpress OnNoneLinkedParameter(UMaterialGraph funGraph, ICodeGen cGen, int i)
        {
            if (Method.Parameters[i].ParamInfo.Name == "texture")
            {
                var retVar = new DefineVar();
                retVar.IsLocalVar = false;
                retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
                retVar.VarName = TextureVarName;
                return new OpUseDefinedVar(retVar);
            }
            else if (Method.Parameters[i].ParamInfo.Name == "sampler")
            {
                var retVar = new DefineVar();
                retVar.IsLocalVar = false;
                retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
                retVar.VarName = "Samp_" + TextureVarName;
                return new OpUseDefinedVar(retVar);
            }
            else if (Method.Parameters[i].ParamInfo.Name == "uv")
            {
                var retVar = new DefineVar();
                retVar.IsLocalVar = false;
                retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
                retVar.VarName = "input.vUV";
                return new OpUseDefinedVar(retVar);
            }

            return base.OnNoneLinkedParameter(funGraph, cGen, i);
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
        public override void OnMaterialEditorGenCode(Bricks.CodeBuilder.HLSL.UHLSLGen gen, UMaterial Material)
        {
            var texNode = this;
            var texturePinIn = texNode.FindPinIn("texture");
            if (texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
                tmp.Name = texNode.TextureVarName;
                tmp.ShaderType = "Texture2D";
                if (Material.FindSRV(tmp.Name) == null)
                {
                    tmp.Value = texNode.AssetName;
                    Material.UsedRSView.Add(tmp);
                }
            }
            var samplerPinIn = texNode.FindPinIn("sampler");
            if (samplerPinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + texNode.TextureVarName;
                if (Material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = texNode.Sampler;
                    Material.UsedSamplerStates.Add(tmp);
                }
            }
        }
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
        protected override OpExpress OnNoneLinkedParameter(UMaterialGraph funGraph, ICodeGen cGen, int i)
        {
            if (Method.Parameters[i].ParamInfo.Name == "texture")
            {
                var retVar = new DefineVar();
                retVar.IsLocalVar = false;
                retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
                retVar.VarName = TextureVarName;
                return new OpUseDefinedVar(retVar);
            }
            else if (Method.Parameters[i].ParamInfo.Name == "sampler")
            {
                var retVar = new DefineVar();
                retVar.IsLocalVar = false;
                retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
                retVar.VarName = "Samp_" + TextureVarName;
                return new OpUseDefinedVar(retVar);
            }
            else if (Method.Parameters[i].ParamInfo.Name == "uv")
            {
                var retVar = new DefineVar();
                retVar.IsLocalVar = false;
                retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
                retVar.VarName = "input.vUV";
                return new OpUseDefinedVar(retVar);
            }

            return base.OnNoneLinkedParameter(funGraph, cGen, i);
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
        public override void OnMaterialEditorGenCode(Bricks.CodeBuilder.HLSL.UHLSLGen gen, UMaterial Material)
        {
            var texNode = this;
            var texturePinIn = texNode.FindPinIn("texture");
            if (texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
                tmp.Name = texNode.TextureVarName;
                tmp.ShaderType = "Texture2DArray";
                if (Material.FindSRV(tmp.Name) == null)
                {
                    tmp.Value = texNode.AssetName;
                    Material.UsedRSView.Add(tmp);
                }
            }
            var samplerPinIn = texNode.FindPinIn("sampler");
            if (samplerPinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + texNode.TextureVarName;
                if (Material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = texNode.Sampler;
                    Material.UsedSamplerStates.Add(tmp);
                }
            }
        }
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
        protected override OpExpress OnNoneLinkedParameter(UMaterialGraph funGraph, ICodeGen cGen, int i)
        {
            if (Method.Parameters[i].ParamInfo.Name == "texture")
            {
                var retVar = new DefineVar();
                retVar.IsLocalVar = false;
                retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
                retVar.VarName = TextureVarName;
                return new OpUseDefinedVar(retVar);
            }
            else if (Method.Parameters[i].ParamInfo.Name == "sampler")
            {
                var retVar = new DefineVar();
                retVar.IsLocalVar = false;
                retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
                retVar.VarName = "Samp_" + TextureVarName;
                return new OpUseDefinedVar(retVar);
            }
            else if (Method.Parameters[i].ParamInfo.Name == "uv")
            {
                var retVar = new DefineVar();
                retVar.IsLocalVar = false;
                retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
                retVar.VarName = "input.vUV";
                return new OpUseDefinedVar(retVar);
            }

            return base.OnNoneLinkedParameter(funGraph, cGen, i);
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
        public override void OnMaterialEditorGenCode(Bricks.CodeBuilder.HLSL.UHLSLGen gen, UMaterial Material)
        {
            var texNode = this;
            var texturePinIn = texNode.FindPinIn("texture");
            if (texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
                tmp.Name = texNode.TextureVarName;
                tmp.ShaderType = "Texture2DArray";
                if (Material.FindSRV(tmp.Name) == null)
                {
                    tmp.Value = texNode.AssetName;
                    Material.UsedRSView.Add(tmp);
                }
            }
            var samplerPinIn = texNode.FindPinIn("sampler");
            if (samplerPinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + texNode.TextureVarName;
                if (Material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = texNode.Sampler;
                    Material.UsedSamplerStates.Add(tmp);
                }
            }
        }
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
        protected override OpExpress OnNoneLinkedParameter(UMaterialGraph funGraph, ICodeGen cGen, int i)
        {
            if (Method.Parameters[i].ParamInfo.Name == "texture")
            {
                var retVar = new DefineVar();
                retVar.IsLocalVar = false;
                retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
                retVar.VarName = TextureVarName;
                return new OpUseDefinedVar(retVar);
            }
            else if (Method.Parameters[i].ParamInfo.Name == "sampler")
            {
                var retVar = new DefineVar();
                retVar.IsLocalVar = false;
                retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
                retVar.VarName = "Samp_" + TextureVarName;
                return new OpUseDefinedVar(retVar);
            }
            else if (Method.Parameters[i].ParamInfo.Name == "uv")
            {
                var retVar = new DefineVar();
                retVar.IsLocalVar = false;
                retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
                retVar.VarName = "input.vUV";
                return new OpUseDefinedVar(retVar);
            }

            return base.OnNoneLinkedParameter(funGraph, cGen, i);
        }
    }
}
