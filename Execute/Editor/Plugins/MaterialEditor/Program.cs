using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MaterialEditor
{
    public class Program
    {
        public static readonly string MaterialAssemblyKey = "MaterialEditor.dll";

        public static string GetInitialNewString(string strType)
        {
            string strRet = "";
            switch (strType)
            {
                case "int":
                    strRet = "0";
                    break;

                case "float":
                case "float1":
                case "half":
                case "Half1":
                case "uint":
                case "uint1":
                    strRet = "0";
                    break;
                case "float2":
                    strRet = "float2(0, 0)";
                    break;
                case "half2":
                    strRet = "half2(0, 0)";
                    break;
                case "float3":
                    strRet = "float3(0, 0, 0)";
                    break;
                case "half3":
                    strRet = "half3(0, 0, 0)";
                    break;
                case "float4":
                    strRet = "float4(0, 0, 0, 0)";
                    break;
                case "half4":
                    strRet = "half4(0, 0, 0, 0)";
                    break;
                case "uint2":
                    strRet = "uint2(0, 0)";
                    break;
                case "uint3":
                    strRet = "uint3(0, 0, 0)";
                    break;
                case "uint4":
                    strRet = "uint4(0, 0, 0, 0)";
                    break;
                case "float4x4":
                    strRet = "(float4x4)0";
                    break;
                default:
                    throw new InvalidOperationException("不支持的类型" + strType);
            }

            return strRet;
        }
        public static Type GetTypeFromValueType(string valueType)
        {
            valueType = valueType.ToLower();
            switch (valueType)
            {
                case "texture":
                    return null;
                case "int":
                    return typeof(System.Int32);
                case "float":
                case "float1":
                case "half":
                case "half1":
                    return typeof(System.Single);
                case "float2":
                case "half2":
                    return typeof(EngineNS.Vector2);
                case "float3":
                case "half3":
                    return typeof(EngineNS.Vector3);
                case "float4":
                case "half4":
                    return typeof(EngineNS.Vector4);
                case "float3x3":
                case "half3x3":
                    return null;
                case "float4x4":
                case "half4x4":
                    return typeof(EngineNS.Matrix);
            }

            return null;
        }

        public static EngineNS.EShaderVarType GetShaderVarTypeFromValueType(string valueType)
        {
            valueType = valueType.ToLower();
            switch(valueType)
            {
                case "texture":
                    return EngineNS.EShaderVarType.SVT_Texture;
                case "sampler":
                    return EngineNS.EShaderVarType.SVT_Sampler;
                case "int":
                case "int1":
                    return EngineNS.EShaderVarType.SVT_Int1;
                case "int2":
                    return EngineNS.EShaderVarType.SVT_Int2;
                case "int3":
                    return EngineNS.EShaderVarType.SVT_Int3;
                case "int4":
                    return EngineNS.EShaderVarType.SVT_Int4;
                case "float":
                case "float1":
                case "half":
                case "half1":
                    return EngineNS.EShaderVarType.SVT_Float1;
                case "float2":
                case "half2":
                    return EngineNS.EShaderVarType.SVT_Float2;
                case "float3":
                case "half3":
                    return EngineNS.EShaderVarType.SVT_Float3;
                case "float4":
                case "half4":
                    return EngineNS.EShaderVarType.SVT_Float4;
                case "float3x3":
                case "half3x3":
                    return EngineNS.EShaderVarType.SVT_Matrix3x3;
                case "float4x4":
                case "half4x4":
                    return EngineNS.EShaderVarType.SVT_Matrix4x4;
                default:
                    return EngineNS.EShaderVarType.SVT_Unknown;
            }
        }

        public static string GetTypeValueString(string valueType, object value)
        {
            valueType = valueType.ToLower();
            switch (valueType)
            {
                case "texture":
                    return null;
                case "int":
                    return System.Convert.ToInt32(value).ToString();
                case "float":
                case "float1":
                case "half":
                case "half1":
                    return System.Convert.ToSingle(value).ToString();
                case "float2":
                    {
                        var val = (EngineNS.Vector2)value;
                        if (val == null)
                            return "";
                        return "float2(" + val.X + "," + val.Y + ")";
                    }
                case "half2":
                    {
                        var val = (EngineNS.Vector2)value;
                        if (val == null)
                            return "";
                        return "half2(" + val.X + "," + val.Y + ")";
                    }
                case "float3":
                    {
                        var val = (EngineNS.Vector3)value;
                        if (val == null)
                            return "";
                        return "float3(" + val.X + "," + val.Y + "," + val.Z + ")";
                    }
                case "half3":
                    {
                        var val = (EngineNS.Vector3)value;
                        if (val == null)
                            return "";
                        return "half3(" + val.X + "," + val.Y + "," + val.Z + ")";
                    }
                case "float4":
                    {
                        var val = (EngineNS.Vector4)value;
                        if (val == null)
                            return "";
                        return "float4(" + val.X + "," + val.Y + "," + val.Z + "," + val.W + ")";
                    }
                case "half4":
                    {
                        var val = (EngineNS.Vector4)value;
                        if (val == null)
                            return "";
                        return "half4(" + val.X + "," + val.Y + "," + val.Z + "," + val.W + ")";
                    }
                //case "float3x3":
                //    return "";
                //case "float4x4":
                //    return "";
                default:
                    throw new InvalidOperationException();
            }

            //return "";
        }

        public static Brush GetBrushFromValueType(string valueType, FrameworkElement element)
        {
            return Brushes.White;
            //valueType = valueType.ToLower();
            //switch (valueType)
            //{
            //    case "int":
            //        return element.TryFindResource("ValueLink_int") as Brush;
            //    case "float":
            //    case "float1":
            //        return element.TryFindResource("ValueLink_float1") as Brush;
            //    case "float2":
            //        return element.TryFindResource("ValueLink_float2") as Brush;
            //    case "float3":
            //        return element.TryFindResource("ValueLink_float3") as Brush;
            //    case "float4":
            //        return element.TryFindResource("ValueLink_float4") as Brush;
            //    case "float3x3":
            //        return element.TryFindResource("ValueLink_float3X3") as Brush;
            //    case "float4x4":
            //        return element.TryFindResource("ValueLink_float4X4") as Brush;
            //    case "sampler2D":
            //    case "texture":
            //        return element.TryFindResource("TextureLink") as Brush;
            //    default:
            //        return element.TryFindResource("ValueLink") as Brush;
            //}
        }

        // 判断idx位置的代码段在{与}之中是否包含测试字符串
        public static bool IsSegmentContainString(int idx, string segment, string checkString)
        {
            if (string.IsNullOrEmpty(segment))
                return false;

            if (idx >= segment.Length)
                idx = segment.Length - 1;

            var rightbIdx = segment.LastIndexOf('}', idx);
            var leftbIdx = segment.LastIndexOf('{', idx);
            if (leftbIdx < 0)
                return segment.Contains(checkString);

            if (leftbIdx > rightbIdx)
            {
                var ctIdx = segment.IndexOf(checkString, leftbIdx, idx - leftbIdx);
                if (ctIdx < 0)
                    return false;
                else
                    return true;
            }
            else
            {
                var subStr = segment.Remove(leftbIdx, idx - leftbIdx + 1);
                return IsSegmentContainString(leftbIdx - 1, subStr, checkString);
            }
        }

        public static string GetValidNodeName(CodeGenerateSystem.Base.BaseNodeControl nodeCtrl)
        {
            string postStr = "";
            if (nodeCtrl.HostNodesContainer != null)
            {
                var matCtrl = nodeCtrl.HostNodesContainer.HostControl as MaterialEditorControl;
                if(matCtrl != null)
                {
                    // matCtrl.CurrentMaterial 不应为空，所以这里不判断
                    var hash64 = matCtrl.CurrentMaterial.GetHash64();
                    postStr = hash64.ToString();
                }
            }
            string strValueName;
            if ((string.IsNullOrEmpty(nodeCtrl.NodeName)) || (nodeCtrl.NodeName == CodeGenerateSystem.Program.NodeDefaultName))
                strValueName = EngineNS.Graphics.CGfxMaterialManager.GetValidShaderVarName(EngineNS.Editor.Assist.GetValuedGUIDString(nodeCtrl.Id), postStr);
            else
                strValueName = EngineNS.Graphics.CGfxMaterialManager.GetValidShaderVarName(nodeCtrl.NodeName, postStr);

            return strValueName;
        }
        public static bool IsShaderKeyWorld(string str)
        {
            //var lowerStr = str.ToLower();
            //switch (lowerStr)
            //{
            //    //case "texture":
            //    //case "int":
            //    //case "float":
            //    //case "float2":
            //    //case "float3":
            //    //case "float4":
            //    //case "float3x3":
            //    //case "float4x4":
            //    //case "abs":
            //    //case "acos":
            //    //case "all":
            //    //case "AllMemoryBarrier":
            //    //case "AllMemoryBarrierWithGroupSync":
            //    //case "any":
            //    //case "asdouble":
            //    //case "asfloat":
            //    //case "asin":
            //    //case "asint":
            //    //case "asuint":
            //    //case "atan":
            //    //case "atan2":
            //    //case "ceil":
            //    //case "clamp":
            //    //case "clip":
            //    //case "cos":
            //    //case "cosh":
            //    //case "countbits":
            //    //case "cross":
            //    //case "D3DCOLORtoUBYTE4":
            //    //case "ddx":
            //    //case "ddx_coarse":
            //    //case "ddx_fine":
            //    //case "ddy":
            //    //case "ddy_coarse":
            //    //case "ddy_fine":
            //    //case "degrees":
            //    //case "determinant":
            //    //case "DeviceMemoryBarrier":
            //    //case "DeviceMemoryBarrierWithGroupSync":
            //    //case "distance":
            //    //case "dot":
            //    //case "dst":
            //    //case "EvaluateAttributeAtCentroid":
            //    //case "EvaluateAttributeAtSample":
            //    //case "EvaluateAttributeSnapped":
            //    //case "exp":
            //    //case "exp2":
            //    //case "f16tof32":
            //    //case "f32tof16":
            //    //case "faceforward":
            //    //case "firstbithigh":
            //    //case "firstbitlow":
            //    //case "floor":
            //    //case "fmod":
            //    //case "frac":
            //    //case "frexp":
            //    //case "fwidth":
            //    //case "GetRenderTargetSampleCount":
            //    //case "GetRenderTargetSamplePosition":
            //    //case "GroupMemoryBarrier":
            //    //case "GroupMemoryBarrierWithGroupSync":
            //    //case "InterlockedAdd":
            //    //case "InterlockedAnd":
            //    //case "InterlockedCompareExchange":
            //    //case "InterlockedCompareStore":
            //    //case "InterlockedExchange":
            //    //case "InterlockedMax":
            //    //case "InterlockedMin":
            //    //case "InterlockedOr":
            //    //case "InterlockedXor":
            //    //case "isfinite":
            //    //case "isinf":
            //    //case "isnan":
            //    //case "ldexp":
            //    //case "length":
            //    //case "lerp":
            //    //case "lit":
            //    //case "log":
            //    //case "log10":
            //    //case "log2":
            //    //case "mad":
            //    //case "max":
            //    //case "min":
            //    //case "modf":
            //    //case "mul":
            //    //case "noise":
            //    //case "normalize":
            //    //case "pow":
            //    //case "Process2DQuadTessFactorsAvg":
            //    //case "Process2DQuadTessFactorsMax":
            //    //case "Process2DQuadTessFactorsMin":
            //    //case "ProcessIsolineTessFactors":
            //    //case "ProcessQuadTessFactorsAvg":
            //    //case "ProcessQuadTessFactorsMax":
            //    //case "ProcessQuadTessFactorsMin":
            //    //case "ProcessTriTessFactorsAvg":
            //    //case "ProcessTriTessFactorsMax":
            //    //case "ProcessTriTessFactorsMin":
            //    //case "radians":
            //    //case "rcp":
            //    //case "reflect":
            //    //case "refract":
            //    //case "reversebits":
            //    //case "round":
            //    //case "rsqrt":
            //    //case "saturate":
            //    //case "sign":
            //    //case "sin":
            //    //case "sincos":
            //    //case "sinh":
            //    //case "smoothstep":
            //    //case "sqrt":
            //    //case "step":
            //    //case "tan":
            //    //case "tanh":
            //    //case "tex1D":
            //    //case "tex1Dbias":
            //    //case "tex1Dgrad":
            //    //case "tex1Dlod":
            //    //case "tex1Dproj":
            //    //case "tex2D":
            //    //case "tex2Dbias":
            //    //case "tex2Dgrad":
            //    //case "tex2Dlod":
            //    //case "tex2Dproj":
            //    //case "tex3D":
            //    //case "tex3Dbias":
            //    //case "tex3Dgrad":
            //    //case "tex3Dlod":
            //    //case "tex3Dproj":
            //    //case "texCUBE":
            //    //case "texCUBEbias":
            //    //case "texCUBEgrad":
            //    //case "texCUBElod":
            //    //case "texCUBEproj":
            //    //case "transpose":
            //    //case "trunc":
            //    case "GDiffuseTexture":
            //        return true;
            //}

            return false;
        }
    }
}
