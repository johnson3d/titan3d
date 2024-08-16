using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EngineNS.Graphics.Pipeline.Shader
{
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "VS_INPUT")]
    public struct VS_INPUT
    {
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vPosition", Condition = "USE_VS_Position == 1", Binder = "VK_LOCATION(0)", Semantic = "POSITION")]
        public Vector3 vPosition;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vNormal", Condition = "USE_VS_Normal == 1", Binder = "VK_LOCATION(1)", Semantic = "NORMAL")]
        public Vector3 vNormal;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vTangent", Condition = "USE_VS_Tangent == 1", Binder = "VK_LOCATION(2)", Semantic = "TEXCOORD")]
        public Vector4 vTangent;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vColor", Condition = "USE_VS_Color == 1", Binder = "VK_LOCATION(3)", Semantic = "COLOR")]
        public Vector4 vColor;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vUV", Condition = "USE_VS_UV == 1", Binder = "VK_LOCATION(4)", Semantic = "TEXCOORD1")]
        public Vector2 vUV;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vLightMap", Condition = "USE_VS_LightMap == 1", Binder = "VK_LOCATION(5)", Semantic = "TEXCOORD2")]
        public Vector4 vLightMap;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vSkinIndex", Condition = "USE_VS_SkinIndex == 1", Binder = "VK_LOCATION(6)", Semantic = "TEXCOORD3")]
        public Vector4ui vSkinIndex;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vSkinWeight", Condition = "USE_VS_SkinWeight == 1", Binder = "VK_LOCATION(7)", Semantic = "TEXCOORD4")]
        public Vector4 vSkinWeight;

        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vTerrainIndex", Condition = "USE_VS_TerrainIndex == 1", Binder = "VK_LOCATION(8)", Semantic = "TEXCOORD5")]
        public Vector4ui vTerrainIndex;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vTerrainGradient", Condition = "USE_VS_TerrainGradient == 1", Binder = "VK_LOCATION(9)", Semantic = "TEXCOORD6")]
        public Vector4ui vTerrainGradient;

        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vInstPos", Condition = "USE_VS_InstPos == 1", Binder = "VK_LOCATION(10)", Semantic = "TEXCOORD7")]
        public Vector3 vInstPos;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vInstQuat", Condition = "USE_VS_InstQuat == 1", Binder = "VK_LOCATION(11)", Semantic = "TEXCOORD8")]
        public Vector4 vInstQuat;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vInstScale", Condition = "USE_VS_InstScale == 1", Binder = "VK_LOCATION(12)", Semantic = "TEXCOORD9")]
        public Vector4 vInstScale;

        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vF4_1", Condition = "USE_VS_F4_1 == 1", Binder = "VK_LOCATION(13)", Semantic = "TEXCOORD10")]
        public Vector4ui vF4_1;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vF4_2", Condition = "USE_VS_F4_2 == 1", Binder = "VK_LOCATION(14)", Semantic = "TEXCOORD11")]
        public Vector4 vF4_2;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vF4_3", Condition = "USE_VS_F4_3 == 1", Binder = "VK_LOCATION(15)", Semantic = "TEXCOORD12")]
        public Vector4ui vF4_3;

        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vVertexID", Binder = "VK_LOCATION(16)", Semantic = "SV_VertexID")]
        public uint vVertexID;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vInstanceId", Binder = "VK_LOCATION(17)", Semantic = "SV_InstanceID")]
        public uint vInstanceId;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vViewID", Condition = "USE_VS_ViewID == 1 && RHI_TYPE != RHI_DX11", Binder = "VK_LOCATION(18)", Semantic = "SV_ViewID")]
        public uint vViewID;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(
            ShaderName = "vMultiDrawId", Condition = "USE_VS_DrawIndex == 1", Binder = "[[vk::builtin(\"DrawIndex\")]]", Semantic = "DRAWIDX")]
        public uint vMultiDrawId;

        public static EngineNS.NxRHI.EVertexStreamType NameToInputStream(string name)
        {
            switch (name)
            {
                case "vPosition":
                    return EngineNS.NxRHI.EVertexStreamType.VST_Position;
                case "vNormal":
                    return EngineNS.NxRHI.EVertexStreamType.VST_Normal;
                case "vColor":
                    return EngineNS.NxRHI.EVertexStreamType.VST_Color;
                case "vUV":
                    return EngineNS.NxRHI.EVertexStreamType.VST_UV;
                case "vTangent":
                    return EngineNS.NxRHI.EVertexStreamType.VST_Tangent;
                case "vLightMap":
                    return EngineNS.NxRHI.EVertexStreamType.VST_LightMap;
            }
            return EngineNS.NxRHI.EVertexStreamType.VST_Number;
        }
    }

    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "VS_MODIFIER")]
    public struct VS_MODIFIER
    {
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vPosition")]
        public Vector3 vPosition;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vNormal")]
        public Vector3 vNormal;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vTangent")]
        public Vector4 vTangent;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vColor")]
        public Vector4 vColor;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vUV")]
        public Vector2 vUV;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vLightMap")]
        public Vector4 vLightMap;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vSkinIndex")]
        public Vector4ui vSkinIndex;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vSkinWeight")]
        public Vector4 vSkinWeight;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vTerrainIndex")]
        public Vector4ui vTerrainIndex;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vTerrainGradient")]
        public Vector4ui vTerrainGradient;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vInstPos")]
        public Vector3 vInstPos;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vInstQuat")]
        public Vector4 vInstQuat;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vInstScale")]
        public Vector4 vInstScale;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vF4_1")]
        public Vector4ui vF4_1;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vF4_2")]
        public Vector4 vF4_2;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vF4_3")]
        public Vector4 vF4_3;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vVertexID")]
        public uint vVertexID;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vInstanceId")]
        public uint vInstanceId;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vViewID")]
        public uint vViewID;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "vMultiDrawId")]
        public uint vMultiDrawId;

        public static void VSInput_2_VSModifier(Bricks.CodeBuilder.UHLSLCodeGenerator codeBuilder, ref string sourceCode)
        {
            codeBuilder.AddLine($"VS_MODIFIER VS_INPUT_TO_VS_MODIFIER(VS_INPUT input)", ref sourceCode);
            codeBuilder.PushSegment(ref sourceCode);
            {
                codeBuilder.AddLine($"VS_MODIFIER result = (VS_MODIFIER)0;", ref sourceCode);
                var tarFields = typeof(VS_MODIFIER).GetFields();
                foreach (var i in tarFields)
                {
                    var tarName = i.GetCustomAttribute<EngineNS.Editor.ShaderCompiler.TtShaderDefineAttribute>(false).ShaderName;
                    var src = typeof(VS_INPUT).GetField(i.Name);
                    if (src == null)
                        continue;
                    var attr = src.GetCustomAttribute<EngineNS.Editor.ShaderCompiler.TtShaderDefineAttribute>(false);
                    if (attr.Condition != null)
                    {
                        codeBuilder.AddLine($"#if {attr.Condition}", ref sourceCode);
                        codeBuilder.AddLine($"result.{tarName} = input.{attr.ShaderName};", ref sourceCode);
                        codeBuilder.AddLine($"#endif//{attr.Condition}", ref sourceCode);
                    }
                    else
                    {
                        codeBuilder.AddLine($"result.{tarName} = input.{attr.ShaderName};", ref sourceCode);
                    }
                }
                codeBuilder.AddLine($"return result;", ref sourceCode);
            }
            codeBuilder.PopSegment(ref sourceCode);
        }
    };

    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "PS_INPUT")]
    public struct PS_INPUT
    {
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "vPosition", Condition = "USE_PS_Position == 1", Binder = "VK_LOCATION(0)", Semantic = "SV_POSITION")]
        public Vector4 vPosition;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "vNormal", Condition = "USE_PS_Normal == 1", Binder = "VK_LOCATION(1)", Semantic = "NORMAL")]
        public Vector3 vNormal;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "vTangent", Condition = "USE_PS_Tangent == 1", Binder = "VK_LOCATION(5)", Semantic = "TEXCOORD2")]
        public Vector4 vTangent;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "vColor", Condition = "USE_PS_Color == 1", Binder = "VK_LOCATION(2)", Semantic = "COLOR")]
        public Vector4 vColor;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "vUV", Condition = "USE_PS_UV == 1", Binder = "VK_LOCATION(3)", Semantic = "TEXCOORD")]
        public Vector2 vUV;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "vWorldPos", Condition = "USE_PS_WorldPos == 1", Binder = "VK_LOCATION(0)", Semantic = "TEXCOORD1")]
        public Vector3 vWorldPos;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "vLightMap", Condition = "USE_PS_LightMap == 1", Binder = "VK_LOCATION(6)", Semantic = "TEXCOORD3")]
        public Vector4 vLightMap;

        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "psCustomUV0", Condition = "USE_PS_Custom0 == 1", Binder = "VK_LOCATION(7)", Semantic = "TEXCOORD4")]
        public Vector4 psCustomUV0;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "psCustomUV1", Condition = "USE_PS_Custom1 == 1", Binder = "VK_LOCATION(8)", Semantic = "TEXCOORD5")]
        public Vector4 psCustomUV1;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "psCustomUV2", Condition = "USE_PS_Custom2 == 1", Binder = "VK_LOCATION(9)", Semantic = "TEXCOORD6")]
        public Vector4 psCustomUV2;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "psCustomUV3", Condition = "USE_PS_Custom3 == 1", Binder = "VK_LOCATION(10)", Semantic = "TEXCOORD7")]
        public Vector4 psCustomUV3;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "psCustomUV4", Condition = "USE_PS_Custom4 == 1", Binder = "VK_LOCATION(11)", Semantic = "TEXCOORD8")]
        public Vector4 psCustomUV4;

        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "vF4_1", Condition = "USE_PS_F4_1 == 1", Binder = "VK_LOCATION(13)", Semantic = "TEXCOORD10")]
        public Vector4ui vF4_1;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "vF4_2", Condition = "USE_PS_F4_2 == 1", Binder = "VK_LOCATION(14)", Semantic = "TEXCOORD11")]
        public Vector4 vF4_2;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "vF4_3", Condition = "USE_PS_F4_3 == 1", Binder = "VK_LOCATION(15)", Semantic = "TEXCOORD12")]
        public Vector4 vF4_3;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "vSpecialData", Condition = "USE_PS_SpecialData == 1", Binder = "VK_LOCATION(16)", Semantic = "TEXCOORD13")]
        public Vector4ui vSpecialData;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "vInstanceId", Condition = "USE_PS_Instance == 1", Binder = "VK_LOCATION(17)", Semantic = "TEXCOORD14")]
        public uint vInstanceId;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "vViewID", Condition = "USE_PS_ViewID == 1", Binder = "VK_LOCATION(18)", Semantic = "SV_ViewID")]
        public uint vViewID;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "vMultiDrawId", Condition = "USE_PS_DrawIndex == 1", Binder = "[[vk::builtin(\"DrawIndex\")]]", Semantic = "DRAWIDX")]
        public uint vMultiDrawId;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "vPrimitiveId",
            Condition = "ShaderStage == ShaderStage_PS", Semantic = "SV_PrimitiveID")]
        public uint vPrimitiveId;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(Flags = Editor.ShaderCompiler.EShaderDefine.HasGet | Editor.ShaderCompiler.EShaderDefine.HasSet,
            ShaderName = "bIsFrontFace",
            Condition = "ShaderStage == ShaderStage_PS", Semantic = "SV_IsFrontFace")]
        public bool bIsFrontFace;

        public static Graphics.Pipeline.Shader.EPixelShaderInput NameToInput(string name)
        {
            switch (name)
            {
                case "vPosition":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_Position;
                case "vNormal":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_Normal;
                case "vColor":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_Color;
                case "vUV":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_UV;
                case "vWorldPos":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_WorldPos;
                case "vTangent":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_Tangent;
                case "vLightMap":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_LightMap;
                case "psCustomUV0":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_Custom0;
                case "psCustomUV1":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_Custom1;
                case "psCustomUV2":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_Custom2;
                case "psCustomUV3":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_Custom3;
                case "psCustomUV4":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_Custom4;
                case "vF4_1":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_F4_1;
                case "vF4_2":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_F4_2;
                case "vF4_3":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_F4_3;
                case "SpecialData":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_SpecialData;
                case "vInstanceID":
                    return Graphics.Pipeline.Shader.EPixelShaderInput.PST_InstanceID;
            }
            return Graphics.Pipeline.Shader.EPixelShaderInput.PST_Number;
        }

        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Set_vTangent")]
        public void Set_vTangent(Vector3 v)
        {
            vTangent.X = v.X;
            vTangent.Y = v.Y;
            vTangent.Z = v.Z;
        }
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Set_vSpecialDataX")]
        public void Set_vSpecialDataX(uint v)
        {
            vSpecialData.X = v;
        }
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Set_vSpecialDataY")]
        public void Set_vSpecialDataY(uint v)
        {
            vSpecialData.Y = v;
        }
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Set_vSpecialDataZ")]
        public void Set_vSpecialDataZ(uint v)
        {
            vSpecialData.Z = v;
        }
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Set_vSpecialDataW")]
        public void Set_vSpecialDataW(uint v)
        {
            vSpecialData.W = v;
        }

        public static void VSModifier_2_PSInput(Bricks.CodeBuilder.UHLSLCodeGenerator codeBuilder, ref string sourceCode)
        {
            codeBuilder.AddLine($"void Default_VSInput2PSInput(inout PS_INPUT output, VS_MODIFIER input)", ref sourceCode);
            codeBuilder.PushSegment(ref sourceCode);
            {
                var tarFields = typeof(PS_INPUT).GetFields();
                foreach (var i in tarFields)
                {
                    var tarAttr = i.GetCustomAttribute<Editor.ShaderCompiler.TtShaderDefineAttribute>(false);
                    if (tarAttr.ShaderName == "vWorldPos")
                    {
                        codeBuilder.AddLine($"#if {tarAttr.Condition}", ref sourceCode);
                        codeBuilder.AddLine($"output.{tarAttr.ShaderName} = input.vPosition;", ref sourceCode);
                        codeBuilder.AddLine($"#endif//{tarAttr.Condition}", ref sourceCode);
                        continue;
                    }
                    var src = typeof(VS_MODIFIER).GetField(i.Name);
                    if (src == null)
                        continue;
                    var srcAttr = src.GetCustomAttribute<Editor.ShaderCompiler.TtShaderDefineAttribute>(false);
                    if (tarAttr.Condition != null)
                    {
                        if (tarAttr.ShaderName == "vPosition")
                        {
                            codeBuilder.AddLine($"#if {tarAttr.Condition}", ref sourceCode);
                            codeBuilder.AddLine($"output.{tarAttr.ShaderName} = float4(input.{srcAttr.ShaderName}, 1);", ref sourceCode);
                            codeBuilder.AddLine($"#endif//{tarAttr.Condition}", ref sourceCode);
                            continue;
                        }
                        codeBuilder.AddLine($"#if {tarAttr.Condition}", ref sourceCode);
                        codeBuilder.AddLine($"output.{tarAttr.ShaderName} = input.{srcAttr.ShaderName};", ref sourceCode);
                        codeBuilder.AddLine($"#endif//{tarAttr.Condition}", ref sourceCode);
                    }
                    else
                    {
                        codeBuilder.AddLine($"output.{tarAttr.ShaderName} = input.{srcAttr.ShaderName};", ref sourceCode);
                    }
                }
            }
            codeBuilder.PopSegment(ref sourceCode);
        }
    }

    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "MTL_OUTPUT", Order = 1)]
    public struct MTL_OUTPUT
    {
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mAlbedo")]
        public Vector3 mAlbedo;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mNormal")]
        public Vector3 mNormal;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mMetallic")]
        public float mMetallic;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mRough")]
        public float mRough;   //in the editer,we call it smoth,so rough = 1.0f - smoth;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mAbsSpecular")]
        public float mAbsSpecular;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mTransmit")]
        public float mTransmit;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mEmissive")]
        public Vector3 mEmissive;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mFuzz")]
        public float mFuzz;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mIridescence")]
        public float mIridescence;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mDistortion")]
        public float mDistortion;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mAlpha")]
        public float mAlpha;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mAlphaTest")]
        public float mAlphaTest;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mVertexOffset")]
        public Vector3 mVertexOffset;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mSubAlbedo")]
        public Vector3 mSubAlbedo;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mAO")]
        public float mAO;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mMask")]
        public float mMask;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mShadowColor")]
        public Vector3 mShadowColor;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mDeepShadow")]
        public float mDeepShadow;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "mMoodColor")]
        public Vector3 mMoodColor;

        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "GetWorldNormal")]
        public Vector3 GetWorldNormal(PS_INPUT input)
        {
            return Vector3.Zero;
        }
    };

    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FComputeEnv")]
    struct FComputeEnv
    {
        public FComputeEnv(Vector3ui id, Vector3ui groupId, Vector3ui groupThreadId, uint groupIndex)
        {
            Id = id;
            GroupId = groupId;
            GroupThreadId = groupThreadId;
            GroupIndex = groupIndex;
        }
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Id")]
        public Vector3ui Id;//SV_DispatchThreadID
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "GroupId")]
        public Vector3ui GroupId;//SV_GroupID
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "GroupThreadId")]
        public Vector3ui GroupThreadId;//SV_GroupThreadID
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "GroupIndex")]
        public uint GroupIndex;// SV_GroupIndex
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FVSInstanceData")]
    public struct FVSInstanceData
    {
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Position")]
        public Vector3 Position;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "HitProxyId")]
        public uint HitProxyId;

        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Scale")]
        public Vector3 Scale;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Scale_Pad")]
        public uint Scale_Pad;

        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Quat")]
        public Quaternion Quat;

        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "UserData")]
        public Vector4ui UserData;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "UserData2")]
        public Vector4ui UserData2;
        public void SetMatrix(in Matrix mat)
        {
            Position = mat.Translation;
            Scale = mat.Scale;
            Quat = mat.Rotation;
        }
    };
}
