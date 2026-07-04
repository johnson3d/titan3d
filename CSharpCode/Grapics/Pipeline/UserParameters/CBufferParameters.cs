using EngineNS.Editor.ShaderCompiler;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using static EngineNS.GamePlay.Scene.TtMeshNode;

namespace EngineNS.Graphics.Pipeline.UserParameters
{
    public partial class TtCBufferParametersAMeta : IO.IAssetMeta
    {
        public override string TypeExt => TtCBufferParameter.AssetExt;
        public override string GetAssetTypeName() => "CBufferParameter";
        public override async TtTask<IO.IAsset> GetAsset(params object[] args)
        {
            return await TtEngine.Instance.GfxDevice.CBufferManager.GetCBufferParameter(this.AssetName);
        }
        public override Color4b GetBorderColor()
        {
            return new Color4b(0, 120, 200, 255);
        }
    }

    public enum ECBVarType
    {
        Float,
        Float2,
        Float3,
        Float4,
        Int,
        Int2,
        Int3,
        Int4,
        Uint,
        Uint2,
        Uint3,
        Uint4,
    }

    public class TtCBufferParameterDefinition : IO.BaseSerializer
    {
        [Rtti.Meta("")]
        [Category("Option")]
        private ECBVarType mVarType = ECBVarType.Float;
        [Rtti.Meta("")]
        [Category("Option")]
        public ECBVarType VarType
        {
            get => mVarType;
            set
            {
                if (mVarType == value)
                    return;
                mVarType = value;
                DefaultValue = GetDefaultValueForType(value);
            }
        }

        [Rtti.Meta("")]
        [Category("Option")]
        public string ParamName { get; set; } = "Param0";

        [Rtti.Meta("")]
        [Category("Option")]
        public string DefaultValue { get; set; } = "0";

        public static string GetDefaultValueForType(ECBVarType varType)
        {
            switch (varType)
            {
                case ECBVarType.Float: return "0";
                case ECBVarType.Float2: return "0,0";
                case ECBVarType.Float3: return "0,0,0";
                case ECBVarType.Float4: return "0,0,0,0";
                case ECBVarType.Int: return "0";
                case ECBVarType.Int2: return "0,0";
                case ECBVarType.Int3: return "0,0,0";
                case ECBVarType.Int4: return "0,0,0,0";
                case ECBVarType.Uint: return "0";
                case ECBVarType.Uint2: return "0,0";
                case ECBVarType.Uint3: return "0,0,0";
                case ECBVarType.Uint4: return "0,0,0,0";
                default: return "0";
            }
        }

        public string GetHLSLTypeName()
        {
            switch (VarType)
            {
                case ECBVarType.Float: return "float";
                case ECBVarType.Float2: return "float2";
                case ECBVarType.Float3: return "float3";
                case ECBVarType.Float4: return "float4";
                case ECBVarType.Int: return "int";
                case ECBVarType.Int2: return "int2";
                case ECBVarType.Int3: return "int3";
                case ECBVarType.Int4: return "int4";
                case ECBVarType.Uint: return "uint";
                case ECBVarType.Uint2: return "uint2";
                case ECBVarType.Uint3: return "uint3";
                case ECBVarType.Uint4: return "uint4";
                default: return "float";
            }
        }

        public Rtti.TtTypeDesc GetTtTypeDesc()
        {
            switch (VarType)
            {
                case ECBVarType.Float: return Rtti.TtTypeDescGetter<float>.TypeDesc;
                case ECBVarType.Float2: return Rtti.TtTypeDescGetter<Vector2>.TypeDesc;
                case ECBVarType.Float3: return Rtti.TtTypeDescGetter<Vector3>.TypeDesc;
                case ECBVarType.Float4: return Rtti.TtTypeDescGetter<Vector4>.TypeDesc;
                case ECBVarType.Int: return Rtti.TtTypeDescGetter<int>.TypeDesc;
                case ECBVarType.Int2: return Rtti.TtTypeDescGetter<Vector2i>.TypeDesc;
                case ECBVarType.Int3: return Rtti.TtTypeDescGetter<Vector3i>.TypeDesc;
                case ECBVarType.Int4: return Rtti.TtTypeDescGetter<Vector4i>.TypeDesc;
                case ECBVarType.Uint: return Rtti.TtTypeDescGetter<uint>.TypeDesc;
                case ECBVarType.Uint2: return Rtti.TtTypeDescGetter<Vector2ui>.TypeDesc;
                case ECBVarType.Uint3: return Rtti.TtTypeDescGetter<Vector3ui>.TypeDesc;
                case ECBVarType.Uint4: return Rtti.TtTypeDescGetter<Vector4ui>.TypeDesc;
                default: return Rtti.TtTypeDescGetter<float>.TypeDesc;
            }
        }

        public object ParseDefaultValue()
        {
            return TtCBufferParameter.ParseDefaultValueString(DefaultValue, VarType);
        }
    }
    [IO.AssetCreateMenu(MenuName = "Graphics/CBuffer")]
    [TtCBufferParameterImport]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtCBufferParameter : IO.BaseSerializer, IO.IAsset, IShaderCodeProvider
    {
        public class TtCBufferParameterImportAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                PGAssetInitTask = PGAsset.Initialize();
                mAsset = new TtCBufferParameter();
                PGAsset.Target = mAsset;
            }
        }

        public const string AssetExt = ".cbparam";
        public string TypeExt { get => AssetExt; }

        #region IAsset
        public virtual IO.IAssetMeta CreateAMeta()
        {
            return new TtCBufferParametersAMeta();
        }
        public virtual IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        [Rtti.Meta("")]
        [RName.PGRName(ReadOnly = true)]
        [Category("Option")]
        public RName AssetName { get; set; }

        [Rtti.Meta("")]
        public virtual void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();

            UpdateHLSLCode();
            UpdateMethodMeta();

            if (ameta != null)
            {
                ameta.RefAssetRNames.Clear();
                ameta.SaveAMeta(this);
            }

            UpdateShaderDesc();

            var typeStr = Rtti.TtTypeDesc.TypeOf(this.GetType()).TypeString;
            using (var xnd = new IO.TtXndHolder(typeStr, 0, 0))
            {
                using (var attr = xnd.NewAttribute("CBufferParameter", 0, 0))
                {
                    using (var ar = attr.GetWriter(512))
                    {
                        ar.Write(this);
                    }
                    xnd.RootNode.AddAttribute(attr);
                }

                if (mShaderDesc != null)
                {
                    mShaderDesc.mCoreObject.SaveXnd(xnd.RootNode.mCoreObject);
                }

                xnd.SaveXnd(name.Address);
            }

            name.AMeta.AddAssetFile(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address);

            if (ameta != null)
                ameta.SaveRefAssets().AddWaitTask();
        }
        unsafe void LoadShaderDescFromXnd(IO.TtXndNode node)
        {
            var desc = new NxRHI.TtShaderDesc(NxRHI.EShaderType.SDT_ComputeShader);
            var rc = TtEngine.Instance.GfxDevice.RenderContext.mCoreObject;
            if (desc.mCoreObject.LoadXnd(rc, node.mCoreObject))
            {
                mShaderDesc = desc;
            }
        }

        public static TtCBufferParameter LoadXnd(TtCBufferParameterManager manager, IO.TtXndNode node)
        {
            IO.ISerializer result = null;
            var attr = node.TryGetAttribute("CBufferParameter");
            if (attr.NativePointer != IntPtr.Zero)
            {
                using (var ar = attr.GetReader(null))
                {
                    ar.Read(out result, null);
                }
            }

            var cbParam = result as TtCBufferParameter;
            if (cbParam != null)
            {
                cbParam.UpdateHLSLCode();
                cbParam.UpdateMethodMeta();
                cbParam.LoadShaderDescFromXnd(node);
                return cbParam;
            }
            return null;
        }
        public static bool ReloadXnd(TtCBufferParameter cbParam, TtCBufferParameterManager manager, IO.TtXndNode node)
        {
            var attr = node.TryGetAttribute("CBufferParameter");
            if (attr.NativePointer != IntPtr.Zero)
            {
                using (var ar = attr.GetReader(null))
                {
                    try
                    {
                        ar.ReadTo(cbParam, null);
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                }
            }
            cbParam.UpdateHLSLCode();
            cbParam.UpdateMethodMeta();
            cbParam.LoadShaderDescFromXnd(node);
            return true;
        }
        #endregion

        #region IShaderCodeProvider
        public NxRHI.TtShaderCode DefineCode { get; } = new NxRHI.TtShaderCode();
        public NxRHI.TtShaderCode SourceCode { get; } = new NxRHI.TtShaderCode();
        #endregion

        [Rtti.Meta("")]
        [Category("Parameters")]
        public List<TtCBufferParameterDefinition> ParameterDefinitions { get; set; } = new List<TtCBufferParameterDefinition>();

        [Rtti.Meta("")]
        [TtShaderCodeViewer]
        public string HLSLCode { get; set; }

        [Rtti.Meta("")]
        public Rtti.TtClassMeta.TtMethodMeta MethodMeta { get; set; } = new Rtti.TtClassMeta.TtMethodMeta();

        private string mCallNodeName = null;
        [Rtti.Meta("")]
        [Category("Option")]
        public string CallNodeName
        {
            get
            {
                if (mCallNodeName != null)
                    return mCallNodeName;
                return AssetName?.Name;
            }
            set => mCallNodeName = value;
        }

        public string GetCBufferName()
        {
            if (AssetName == null)
                return "CBParam";
            return AssetName.PureName + "_" + UniHash32.APHash(AssetName.ToString());
        }

        public string GetNamespacedFieldName(TtCBufferParameterDefinition param)
        {
            return GetCBufferName() + "_" + param.ParamName;
        }

        public string GenCBufferHLSLCode()
        {
            var sb = new StringBuilder();
            var cbName = GetCBufferName();
            sb.AppendLine($"cbuffer {cbName}");
            sb.AppendLine("{");
            foreach (var param in ParameterDefinitions)
            {
                sb.AppendLine($"    {param.GetHLSLTypeName()} {GetNamespacedFieldName(param)};");
            }
            sb.AppendLine($"    uint CBufferBinderGenDummy;");
            sb.AppendLine("};");
            return sb.ToString();
        }

        public string GenAccessorFunctionCode()
        {
            var cbName = GetCBufferName();
            var funcName = cbName + "_GetValues";

            var sb = new StringBuilder();
            sb.Append($"void {funcName}(in PS_INPUT input");
            foreach (var param in ParameterDefinitions)
            {
                sb.Append($", out {param.GetHLSLTypeName()} {param.ParamName}");
            }
            sb.AppendLine(")");
            sb.AppendLine("{");
            foreach (var param in ParameterDefinitions)
            {
                sb.AppendLine($"    {param.ParamName} = {GetNamespacedFieldName(param)};");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        public void UpdateHLSLCode()
        {
            HLSLCode = GenCBufferHLSLCode() + "\n" + GenAccessorFunctionCode();
        }

        public void UpdateMethodMeta()
        {
            MethodMeta.Parameters = new List<Rtti.TtClassMeta.TtMethodMeta.TtParamMeta>();
            MethodMeta.MethodName = GetCBufferName() + "_GetValues";
            MethodMeta.ReturnType = Rtti.TtTypeDesc.TypeOf(typeof(void));

            // PS_INPUT is always the first parameter
            var inputParam = new Rtti.TtClassMeta.TtMethodMeta.TtParamMeta();
            inputParam.Name = "input";
            inputParam.ParameterType = Rtti.TtTypeDescGetter<PS_INPUT>.TypeDesc;
            inputParam.ArgumentAttribute = Bricks.CodeBuilder.EMethodArgumentAttribute.In;
            inputParam.DefaultValue = new PS_INPUT();
            MethodMeta.Parameters.Add(inputParam);

            // Each parameter definition becomes an "out" parameter
            foreach (var param in ParameterDefinitions)
            {
                var paramMeta = new Rtti.TtClassMeta.TtMethodMeta.TtParamMeta();
                paramMeta.Name = param.ParamName;
                paramMeta.ParameterType = param.GetTtTypeDesc();
                paramMeta.ArgumentAttribute = Bricks.CodeBuilder.EMethodArgumentAttribute.Out;
                paramMeta.DefaultValue = param.ParseDefaultValue();
                MethodMeta.Parameters.Add(paramMeta);
            }
        }

        public void WriteRefHLSLCode(ref string code)
        {
            if (string.IsNullOrEmpty(HLSLCode))
            {
                UpdateHLSLCode();
            }
            if (code.Contains(HLSLCode))
                return;
            code += HLSLCode;
            code += "\n";
        }

        internal static object ParseDefaultValueString(string defaultStr, ECBVarType varType)
        {
            if (string.IsNullOrWhiteSpace(defaultStr))
                return null;

            var parts = defaultStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            switch (varType)
            {
                case ECBVarType.Float:
                    if (float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                        return f;
                    break;
                case ECBVarType.Float2:
                    if (parts.Length >= 2 &&
                        float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var fx2) &&
                        float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var fy2))
                        return new Vector2(fx2, fy2);
                    break;
                case ECBVarType.Float3:
                    if (parts.Length >= 3 &&
                        float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var fx3) &&
                        float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var fy3) &&
                        float.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var fz3))
                        return new Vector3(fx3, fy3, fz3);
                    break;
                case ECBVarType.Float4:
                    if (parts.Length >= 4 &&
                        float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var fx4) &&
                        float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var fy4) &&
                        float.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var fz4) &&
                        float.TryParse(parts[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var fw4))
                        return new Vector4(fx4, fy4, fz4, fw4);
                    break;
                case ECBVarType.Int:
                    if (int.TryParse(parts[0].Trim(), out var i))
                        return i;
                    break;
                case ECBVarType.Int2:
                    if (parts.Length >= 2 &&
                        int.TryParse(parts[0].Trim(), out var ix2) &&
                        int.TryParse(parts[1].Trim(), out var iy2))
                        return new Vector2(ix2, iy2);
                    break;
                case ECBVarType.Int3:
                    if (parts.Length >= 3 &&
                        int.TryParse(parts[0].Trim(), out var ix3) &&
                        int.TryParse(parts[1].Trim(), out var iy3) &&
                        int.TryParse(parts[2].Trim(), out var iz3))
                        return new Vector3(ix3, iy3, iz3);
                    break;
                case ECBVarType.Int4:
                    if (parts.Length >= 4 &&
                        int.TryParse(parts[0].Trim(), out var ix4) &&
                        int.TryParse(parts[1].Trim(), out var iy4) &&
                        int.TryParse(parts[2].Trim(), out var iz4) &&
                        int.TryParse(parts[3].Trim(), out var iw4))
                        return new Vector4(ix4, iy4, iz4, iw4);
                    break;
                case ECBVarType.Uint:
                    if (uint.TryParse(parts[0].Trim(), out var u))
                        return u;
                    break;
                case ECBVarType.Uint2:
                    if (parts.Length >= 2 &&
                        uint.TryParse(parts[0].Trim(), out var ux2) &&
                        uint.TryParse(parts[1].Trim(), out var uy2))
                        return new Vector2(ux2, uy2);
                    break;
                case ECBVarType.Uint3:
                    if (parts.Length >= 3 &&
                        uint.TryParse(parts[0].Trim(), out var ux3) &&
                        uint.TryParse(parts[1].Trim(), out var uy3) &&
                        uint.TryParse(parts[2].Trim(), out var uz3))
                        return new Vector3(ux3, uy3, uz3);
                    break;
                case ECBVarType.Uint4:
                    if (parts.Length >= 4 &&
                        uint.TryParse(parts[0].Trim(), out var ux4) &&
                        uint.TryParse(parts[1].Trim(), out var uy4) &&
                        uint.TryParse(parts[2].Trim(), out var uz4) &&
                        uint.TryParse(parts[3].Trim(), out var uw4))
                        return new Vector4(ux4, uy4, uz4, uw4);
                    break;
            }
            return null;
        }

        #region ShaderBinder Compilation

        public class TtCBufferShaderBinderGenShading : TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg => new Vector3ui(1, 1, 1);

            public TtCBufferShaderBinderGenShading()
            {
                CodeName = RName.GetRName("Shaders/ShadingEnv/CBufferShaderBinderGen.cginc", RName.ERNameType.Engine);
                MainName = "CS_BuildCBufferBinderGen";

                this.IsOnlyBuildMode = true;
                var inc = new CBufferInclude();
                this.ShaderIncludeProvider = inc;
            }
            public class CBufferInclude : TtHLSLInclude
            {
                public TtCBufferParameter Host;
                private NxRHI.TtShaderCode mCachedUserCode;

                private void EnsureCached()
                {
                    if (mCachedUserCode == null)
                    {
                        mCachedUserCode = new NxRHI.TtShaderCode();
                    }
                    mCachedUserCode.TextCode = Host.GenCBufferHLSLCode();
                }

                public override unsafe NxRHI.FShaderCode* GetHLSLCode(string includeName, out bool bIncluded)
                {
                    if (includeName == "@user_cbuffer.cginc")
                    {
                        bIncluded = true;
                        EnsureCached();
                        return mCachedUserCode.mCoreObject;
                    }
                    else
                    {
                        bIncluded = false;
                        return (NxRHI.FShaderCode*)0;
                    }
                }
            }
        }

        NxRHI.TtShaderDesc mShaderDesc;
        public NxRHI.TtShaderDesc ShaderDesc
        {
            get => mShaderDesc;
        }

        public void UpdateShaderDesc()
        {
            var compiler = new TtHLSLCompiler();
            //var shading = TtShadingEnv.CreateShadingEnv<TtCBufferShaderBinderGenShading>().GetResultUntilCompleted();
            var shading = new TtCBufferShaderBinderGenShading();
            (shading.ShaderIncludeProvider as TtCBufferShaderBinderGenShading.CBufferInclude).Host = this;
            shading.BeginPermutaion();
            shading.UpdatePermutation().WaitCompletedAndDispose();

            mShaderDesc = shading.CurrentEffect.ComputeShader.ShaderDesc;
        }

        public NxRHI.FShaderBinder GetShaderBinder()
        {
            if (mShaderDesc == null)
                return new NxRHI.FShaderBinder();
            return mShaderDesc.mCoreObject.Reflector.FindBinder(NxRHI.EShaderBindType.SBT_CBV, GetCBufferName());
        }

        #endregion
    }

    [Bricks.CodeBuilder.ContextMenu("CBuffer", "Graphics\\CBuffer", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtCBufferParameterNode.TtCBufferParameterData), DefaultNamePrefix = "CBuffer")]
    public partial class TtCBufferParameterNode : GamePlay.Scene.TtVolumeBaseNode
    {
        public class TtCBufferParameterData : TtVolumeBaseData
        {
            [Rtti.Meta("")]
            [RName.PGRName(FilterExts = TtCBufferParameter.AssetExt)]
            public RName CBufferName { get; set; }

            [Rtti.Meta("")]
            [Category("ParameterValues")]
            public List<TtCBufferParameterValue> VarValues { get; set; } = new List<TtCBufferParameterValue>();
        }

        public class TtCBufferParameterValue : IO.BaseSerializer
        {
            [Rtti.Meta("")]
            [ReadOnly(true)]
            public string ParamName { get; set; }

            [Rtti.Meta("")]
            [ReadOnly(true)]
            [Category("Option")]
            public ECBVarType VarType { get; set; } = ECBVarType.Float;

            public class TtCBufferValueEditorAttribute : EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute
            {
                public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
                {
                    newValue = info.Value;
                    var valueEntry = info.ObjectInstance as TtCBufferParameterNode.TtCBufferParameterValue;
                    if (valueEntry == null)
                        return false;

                    var valueStr = (info.Value as string) ?? "0";
                    var varType = valueEntry.VarType;
                    bool changed = false;
                    var label = $"##{info.Name}_val";

                    switch (varType)
                    {
                        case ECBVarType.Float:
                            {
                                var v = ParseSingleFloat(valueStr);
                                changed = ImGuiAPI.InputFloat(label, ref v, 0, 0, null, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                                if (changed)
                                    newValue = v.ToString(CultureInfo.InvariantCulture);
                            }
                            break;
                        case ECBVarType.Float2:
                            {
                                var v = ParseVector2(valueStr);
                                changed = ImGuiAPI.InputFloat2(label, (float*)&v, null, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                                if (changed)
                                    newValue = $"{v.X.ToString(CultureInfo.InvariantCulture)},{v.Y.ToString(CultureInfo.InvariantCulture)}";
                            }
                            break;
                        case ECBVarType.Float3:
                            {
                                var v = ParseVector3(valueStr);
                                changed = ImGuiAPI.InputFloat3(label, (float*)&v, null, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                                if (changed)
                                    newValue = $"{v.X.ToString(CultureInfo.InvariantCulture)},{v.Y.ToString(CultureInfo.InvariantCulture)},{v.Z.ToString(CultureInfo.InvariantCulture)}";
                            }
                            break;
                        case ECBVarType.Float4:
                            {
                                var v = ParseVector4(valueStr);
                                changed = ImGuiAPI.InputFloat4(label, (float*)&v, null, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                                if (changed)
                                    newValue = $"{v.X.ToString(CultureInfo.InvariantCulture)},{v.Y.ToString(CultureInfo.InvariantCulture)},{v.Z.ToString(CultureInfo.InvariantCulture)},{v.W.ToString(CultureInfo.InvariantCulture)}";
                            }
                            break;
                        case ECBVarType.Int:
                            {
                                var v = ParseSingleInt(valueStr);
                                changed = ImGuiAPI.InputInt(label, (int*)&v, 0, 0, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                                if (changed)
                                    newValue = v.ToString();
                            }
                            break;
                        case ECBVarType.Int2:
                            {
                                var v = ParseInt2(valueStr);
                                var arr = stackalloc int[2] { v.X, v.Y };
                                changed = ImGuiAPI.InputInt2(label, arr, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                                if (changed)
                                    newValue = $"{arr[0]},{arr[1]}";
                            }
                            break;
                        case ECBVarType.Int3:
                            {
                                var v = ParseInt3(valueStr);
                                var arr = stackalloc int[3] { v.X, v.Y, v.Z };
                                changed = ImGuiAPI.InputInt3(label, arr, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                                if (changed)
                                    newValue = $"{arr[0]},{arr[1]},{arr[2]}";
                            }
                            break;
                        case ECBVarType.Int4:
                            {
                                var v = ParseInt4(valueStr);
                                var arr = stackalloc int[4] { v.X, v.Y, v.Z, v.W };
                                changed = ImGuiAPI.InputInt4(label, arr, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                                if (changed)
                                    newValue = $"{arr[0]},{arr[1]},{arr[2]},{arr[3]}";
                            }
                            break;
                        case ECBVarType.Uint:
                            {
                                var v = ParseSingleUint(valueStr);
                                changed = ImGuiAPI.InputScalar(label, ImGuiDataType_.ImGuiDataType_U32, (void*)&v, null, null, null, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                                if (changed)
                                    newValue = v.ToString();
                            }
                            break;
                        case ECBVarType.Uint2:
                            {
                                var v = ParseUint2(valueStr);
                                var arr = stackalloc uint[2] { v.X, v.Y };
                                changed = ImGuiAPI.InputScalarN(label, ImGuiDataType_.ImGuiDataType_U32, arr, 2, null, null, null, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                                if (changed)
                                    newValue = $"{arr[0]},{arr[1]}";
                            }
                            break;
                        case ECBVarType.Uint3:
                            {
                                var v = ParseUint3(valueStr);
                                var arr = stackalloc uint[3] { v.X, v.Y, v.Z };
                                changed = ImGuiAPI.InputScalarN(label, ImGuiDataType_.ImGuiDataType_U32, arr, 3, null, null, null, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                                if (changed)
                                    newValue = $"{arr[0]},{arr[1]},{arr[2]}";
                            }
                            break;
                        case ECBVarType.Uint4:
                            {
                                var v = ParseUint4(valueStr);
                                var arr = stackalloc uint[4] { v.X, v.Y, v.Z, v.W };
                                changed = ImGuiAPI.InputScalarN(label, ImGuiDataType_.ImGuiDataType_U32, arr, 4, null, null, null, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                                if (changed)
                                    newValue = $"{arr[0]},{arr[1]},{arr[2]},{arr[3]}";
                            }
                            break;
                        default:
                            {
                                var v = ParseSingleFloat(valueStr);
                                changed = ImGuiAPI.InputFloat(label, ref v, 0, 0, null, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                                if (changed)
                                    newValue = v.ToString(CultureInfo.InvariantCulture);
                            }
                            break;
                    }

                    return changed;
                }

                private static float ParseSingleFloat(string v)
                {
                    if (float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                        return result;
                    return 0;
                }
                private static Vector2 ParseVector2(string v)
                {
                    var parts = v.Split(',');
                    float x = 0, y = 0;
                    if (parts.Length >= 1) float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x);
                    if (parts.Length >= 2) float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y);
                    return new Vector2(x, y);
                }
                private static Vector3 ParseVector3(string v)
                {
                    var parts = v.Split(',');
                    float x = 0, y = 0, z = 0;
                    if (parts.Length >= 1) float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x);
                    if (parts.Length >= 2) float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y);
                    if (parts.Length >= 3) float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z);
                    return new Vector3(x, y, z);
                }
                private static Vector4 ParseVector4(string v)
                {
                    var parts = v.Split(',');
                    float x = 0, y = 0, z = 0, w = 0;
                    if (parts.Length >= 1) float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x);
                    if (parts.Length >= 2) float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y);
                    if (parts.Length >= 3) float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z);
                    if (parts.Length >= 4) float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out w);
                    return new Vector4(x, y, z, w);
                }
                private static int ParseSingleInt(string v)
                {
                    if (int.TryParse(v, out var result))
                        return result;
                    return 0;
                }
                private static Vector2i ParseInt2(string v)
                {
                    var parts = v.Split(',');
                    int x = 0, y = 0;
                    if (parts.Length >= 1) int.TryParse(parts[0], out x);
                    if (parts.Length >= 2) int.TryParse(parts[1], out y);
                    return new Vector2i(x, y);
                }
                private static Vector3i ParseInt3(string v)
                {
                    var parts = v.Split(',');
                    int x = 0, y = 0, z = 0;
                    if (parts.Length >= 1) int.TryParse(parts[0], out x);
                    if (parts.Length >= 2) int.TryParse(parts[1], out y);
                    if (parts.Length >= 3) int.TryParse(parts[2], out z);
                    return new Vector3i(x, y, z);
                }
                private static Vector4i ParseInt4(string v)
                {
                    var parts = v.Split(',');
                    int x = 0, y = 0, z = 0, w = 0;
                    if (parts.Length >= 1) int.TryParse(parts[0], out x);
                    if (parts.Length >= 2) int.TryParse(parts[1], out y);
                    if (parts.Length >= 3) int.TryParse(parts[2], out z);
                    if (parts.Length >= 4) int.TryParse(parts[3], out w);
                    return new Vector4i(x, y, z, w);
                }
                private static uint ParseSingleUint(string v)
                {
                    if (uint.TryParse(v, out var result))
                        return result;
                    return 0;
                }
                private static Vector2ui ParseUint2(string v)
                {
                    var parts = v.Split(',');
                    uint x = 0, y = 0;
                    if (parts.Length >= 1) uint.TryParse(parts[0], out x);
                    if (parts.Length >= 2) uint.TryParse(parts[1], out y);
                    return new Vector2ui(x, y);
                }
                private static Vector3ui ParseUint3(string v)
                {
                    var parts = v.Split(',');
                    uint x = 0, y = 0, z = 0;
                    if (parts.Length >= 1) uint.TryParse(parts[0], out x);
                    if (parts.Length >= 2) uint.TryParse(parts[1], out y);
                    if (parts.Length >= 3) uint.TryParse(parts[2], out z);
                    return new Vector3ui(x, y, z);
                }
                private static Vector4ui ParseUint4(string v)
                {
                    var parts = v.Split(',');
                    uint x = 0, y = 0, z = 0, w = 0;
                    if (parts.Length >= 1) uint.TryParse(parts[0], out x);
                    if (parts.Length >= 2) uint.TryParse(parts[1], out y);
                    if (parts.Length >= 3) uint.TryParse(parts[2], out z);
                    if (parts.Length >= 4) uint.TryParse(parts[3], out w);
                    return new Vector4ui(x, y, z, w);
                }
            }

            [Rtti.Meta("")]
            [Category("Option")]
            [TtCBufferValueEditor]
            public string Value { get; set; }

            public override string ToString()
            {
                return $"{ParamName}: {Value}";
            }
        }

        [Rtti.Meta("")]
        public NxRHI.TtCbView CBuffer { get; private set; }
        public TtCBufferParameter CBufferParameter { get; private set; }

        [Category("CBuffer")]
        [RName.PGRName(FilterExts = TtCBufferParameter.AssetExt)]
        public RName CBufferName 
        {
            get
            {
                return GetNodeData<TtCBufferParameterData>().CBufferName;
            }
            set
            {
                var node = GetNodeData<TtCBufferParameterData>();
                node.CBufferName = value;
                CBufferParameter = TtEngine.Instance.GfxDevice.CBufferManager.GetCBufferParameter(value).GetResultUntilCompleted();

                if (CBuffer == null)
                {
                    var binder = CBufferParameter.GetShaderBinder();
                    CBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                }
                SyncVarValuesFromDefinitions(node);
                SyncVarValuesToCBuffer(node);
            }
        }
        [Category("CBuffer")]
        public List<TtCBufferParameterValue> VarValues
        {
            get
            {
                return GetNodeData<TtCBufferParameterData>()?.VarValues;
            }
        }

        protected override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ret = await base.InitializeNode(world, data, bvType, placementType);

            var cbData = data as TtCBufferParameterData;
            if (cbData != null && cbData.CBufferName != null)
            {
                CBufferParameter = await TtEngine.Instance.GfxDevice.CBufferManager.GetCBufferParameter(cbData.CBufferName);

                if (CBufferParameter != null)
                {
                    SyncVarValuesFromDefinitions(cbData);

                    var binder = CBufferParameter.GetShaderBinder();
                    if (binder.IsValidPointer)
                    {
                        CBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);

                        SyncVarValuesToCBuffer(cbData);
                    }
                }
            }
            return ret;
        }

        private void SyncVarValuesFromDefinitions(TtCBufferParameterData cbData)
        {
            if (CBufferParameter == null || cbData == null)
                return;

            var existingMap = new Dictionary<string, TtCBufferParameterValue>();
            foreach (var v in cbData.VarValues)
            {
                if (!string.IsNullOrEmpty(v.ParamName))
                    existingMap[v.ParamName] = v;
            }

            cbData.VarValues.Clear();
            foreach (var def in CBufferParameter.ParameterDefinitions)
            {
                if (existingMap.TryGetValue(def.ParamName, out var existing) && existing.VarType == def.VarType)
                {
                    cbData.VarValues.Add(existing);
                }
                else
                {
                    cbData.VarValues.Add(new TtCBufferParameterValue
                    {
                        ParamName = def.ParamName,
                        VarType = def.VarType,
                        Value = TtCBufferParameterDefinition.GetDefaultValueForType(def.VarType),
                    });
                }
            }
        }

        private void SyncVarValuesToCBuffer(TtCBufferParameterData cbData)
        {
            if (CBufferParameter == null || CBuffer == null || cbData == null)
                return;

            foreach (var param in CBufferParameter.ParameterDefinitions)
            {
                var fieldName = CBufferParameter.GetNamespacedFieldName(param);
                var valueEntry = cbData.VarValues.Find(v => v.ParamName == param.ParamName);
                if (valueEntry != null && !string.IsNullOrEmpty(valueEntry.Value))
                {
                    var parsed = TtCBufferParameter.ParseDefaultValueString(valueEntry.Value, param.VarType);
                    if (parsed != null)
                        SetValueByType(fieldName, parsed);
                }
                else
                {
                    var defaultVal = param.ParseDefaultValue() ?? 0.0f;
                    SetValueByType(fieldName, defaultVal);
                }
            }
        }

        private void SetValueByType(string fieldName, object value)
        {
            if (CBuffer == null)
                return;

            switch (value)
            {
                case float f:
                    CBuffer.SetValue(fieldName, f);
                    break;
                case Vector2 v2:
                    CBuffer.SetValue(fieldName, in v2);
                    break;
                case Vector3 v3:
                    CBuffer.SetValue(fieldName, in v3);
                    break;
                case Vector4 v4:
                    CBuffer.SetValue(fieldName, in v4);
                    break;
                case int i:
                    CBuffer.SetValue(fieldName, i);
                    break;
                case uint u:
                    CBuffer.SetValue(fieldName, u);
                    break;
            }
        }

        public void SetParameterValue(string paramName, object value)
        {
            if (CBufferParameter == null || CBuffer == null)
                return;

            var fieldName = CBufferParameter.GetNamespacedFieldName(
                CBufferParameter.ParameterDefinitions.Find(p => p.ParamName == paramName));

            if (fieldName != null)
            {
                SetValueByType(fieldName, value);
                CBuffer.FlushDirty();
            }
        }

        public void SetParameterValue(int paramIndex, object value)
        {
            if (CBufferParameter == null || CBuffer == null)
                return;
            if (paramIndex < 0 || paramIndex >= CBufferParameter.ParameterDefinitions.Count)
                return;

            var param = CBufferParameter.ParameterDefinitions[paramIndex];
            SetParameterValue(param.ParamName, value);
        }

        public List<TtCBufferParameterDefinition> GetParameterDefinitions()
        {
            return CBufferParameter?.ParameterDefinitions;
        }
    }
    public partial class TtCBufferParameterManager
    {
        public Dictionary<RName, TtCBufferParameter> CBufferParameters { get; } = new Dictionary<RName, TtCBufferParameter>();

        public async TtTask<TtCBufferParameter> GetCBufferParameter(RName rn)
        {
            if (rn == null)
                return null;

            TtCBufferParameter result;
            if (CBufferParameters.TryGetValue(rn, out result))
                return result;

            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var cbParam = TtCBufferParameter.LoadXnd(this, xnd.RootNode);
                        if (cbParam == null)
                            return null;
                        cbParam.AssetName = rn;
                        return cbParam;
                    }
                    return null;
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (result != null)
            {
                CBufferParameters[rn] = result;
                return result;
            }
            return null;
        }

        public async TtTask<bool> ReloadCBufferParameter(RName rn)
        {
            TtCBufferParameter result;
            if (!CBufferParameters.TryGetValue(rn, out result))
                return true;

            var ok = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        return TtCBufferParameter.ReloadXnd(result, this, xnd.RootNode);
                    }
                    return false;
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return ok;
        }
    }
}
#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Graphics.Pipeline.UserParameters
{
	partial class TtCBufferParameter
	{
		public unsafe void macross_SaveAssetTo (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, RName name) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			SaveAssetTo(name);
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross