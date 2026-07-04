using EngineNS.Bricks.CodeBuilder.ShaderNode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.NodeGraph;
using EngineNS.Thread.Async;
using EngineNS.IO;

namespace EngineNS.Graphics.Pipeline.Shader
{
    public enum EMaterialFunctionEditMode
    {
        Graph,
        RawHLSL,
    }

    public partial class TtMaterialFunctionAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtMaterialFunction.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "MaterialFunction";
        }
        public override Color4b GetBorderColor()
        {
            return TtEngine.Instance.EditorInstance.Config.MaterialFunctionBoderColor;
        }
        public override async Thread.Async.TtTask SaveRefAssets()
        {
            //Stop Editor Operate
            TtEngine.Instance.BlockOperation($"{this.AssetName}: SaveRefAssets");
            var holders = new List<IO.IAssetMeta>();
            TtEngine.Instance.AssetMetaManager.GetAssetHolder(this, holders);
            foreach (var i in holders)
            {
                var mtl = i as TtMaterialAMeta;
                if (mtl == null)
                    continue;
                var holdAsset = await mtl.GetAsset();
                holdAsset.SaveAssetTo(mtl.GetAssetName());
            }
            //await TtEngine.Instance.EventPoster.Post((state) =>
            //{
            //    System.Threading.Thread.Sleep(10000);
            //    return true;
            //}, Thread.Async.EAsyncTarget.AsyncIO);
            //Resume Editor Operate
            TtEngine.Instance.ResumeOperation();
        }
        public override async TtTask<IAsset> GetAsset(params object[] args)
        {
            return await TtEngine.Instance.GfxDevice.MaterialFunctionManager.GetMaterialFunction(this.AssetName);
        }
    }
    [TtMaterialFunction.MaterialFunctionImport]
    [IO.AssetCreateMenu(MenuName = "Graphics/MaterialFunction")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtMaterialFunction : IO.BaseSerializer, IO.IAsset, IShaderCodeProvider
    {
        public const string AssetExt = ".mtlfunc";
        public string TypeExt { get => AssetExt; }
        #region Import
        public class MaterialFunctionImportAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                await base.DoCreate(dir, type, ext);

                var material = (mAsset as TtMaterialFunction);
            }
        }
        #endregion
        #region ISerializer
        
        #endregion
        #region IAsset
        public virtual IO.IAssetMeta CreateAMeta()
        {
            var result = new TtMaterialFunctionAMeta();
            return result;
        }
        public virtual IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public virtual void UpdateAMetaReferences(IO.IAssetMeta ameta, TtMaterialFunctionGraph MaterialGraph)
        {
            ameta.RefAssetRNames.Clear();

            foreach (var i in MaterialGraph.Nodes)
            {
                var f = i as Bricks.CodeBuilder.ShaderNode.Control.TtCallMaterialFunctionNode;
                if (f != null)
                {
                    ameta.AddReferenceAsset(f.FunctionName);
                }
            }
        }
        [Rtti.Meta("")]
        public virtual void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();

            if (EditMode == EMaterialFunctionEditMode.Graph)
            {
                var MaterialGraph = new Bricks.CodeBuilder.ShaderNode.TtMaterialFunctionGraph();
                var xml = IO.TtFileManager.LoadXmlFromString(this.GraphXMLString);
                if (xml != null)
                {
                    object pThis = MaterialGraph;
                    IO.SerializerHelper.ReadObjectMetaFields(this, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
                }
                if (ameta != null)
                {
                    UpdateAMetaReferences(ameta, MaterialGraph);
                    ameta.SaveAMeta(this);
                }

                HLSLCode = GenMateralFunctionGraphCode(new UHLSLCodeGenerator(), MaterialGraph, new TtMaterial());
            }
            else
            {
                // RawHLSL mode: parse MethodMeta from HLSLCode and update references from RefMaterialFunctions
                ParseMethodMetaFromHLSL();

                if (ameta != null)
                {
                    ameta.RefAssetRNames.Clear();
                    foreach (var refFunc in RefMaterialFunctions)
                    {
                        if (refFunc != null)
                            ameta.AddReferenceAsset(refFunc);
                    }
                    ameta.SaveAMeta(this);
                }
            }

            var typeStr = Rtti.TtTypeDesc.TypeOf(this.GetType()).TypeString;
            using (var xnd = new IO.TtXndHolder(typeStr, 0, 0))
            {
                using (var attr = xnd.NewAttribute("MaterialFunction", 0, 0))
                {
                    using (var ar = attr.GetWriter(512))
                    {
                        ar.Write(this);
                    }
                    xnd.RootNode.AddAttribute(attr);
                }

                xnd.SaveXnd(name.Address);
            }

            name.AMeta.AddAssetFile(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address);

            TtEngine.Instance.GfxDevice.MaterialFunctionManager.RegMaterialFunctionName(AssetName);

            if (ameta != null)
                ameta.SaveRefAssets().AddWaitTask();
        }
        public string GenMateralFunctionGraphCode(UHLSLCodeGenerator mHLSLCodeGen,
            Bricks.CodeBuilder.ShaderNode.TtMaterialFunctionGraph MaterialGraph, TtMaterial material)
        {
            var lstInput = new List<IMaterialFunctionInput>();
            foreach (var i in MaterialGraph.Nodes)
            {
                var v = i as IMaterialFunctionInput;
                if (v == null)
                    continue;
                lstInput.Add(v);
            }
            lstInput.Sort((x, y) =>
            {
                return x.VarName.CompareTo(y.VarName);
            });
            var lstOutput = new List<IMaterialFunctionOutput>();
            foreach (var i in MaterialGraph.Nodes)
            {
                var v = i as IMaterialFunctionOutput;
                if (v == null)
                    continue;
                lstOutput.Add(v);
            }
            lstOutput.Sort((x, y) =>
            {
                return x.VarName.CompareTo(y.VarName);
            });
            MethodMeta.Parameters = new List<Rtti.TtClassMeta.TtMethodMeta.TtParamMeta>();
            {
                var t = new Rtti.TtClassMeta.TtMethodMeta.TtParamMeta();
                t.Name = "input";
                t.ParameterType = Rtti.TtTypeDescGetter<PS_INPUT>.TypeDesc;
                t.ArgumentAttribute = Bricks.CodeBuilder.EMethodArgumentAttribute.In;
                t.DefaultValue = new PS_INPUT();
                MethodMeta.Parameters.Add(t);
            }
            foreach (var i in lstInput)
            {
                var t = new Rtti.TtClassMeta.TtMethodMeta.TtParamMeta();
                t.Name = i.VarName;
                t.ParameterType = i.InputType;
                t.ArgumentAttribute = Bricks.CodeBuilder.EMethodArgumentAttribute.In;
                t.DefaultValue = i.GetDefaultValueObject();
                MethodMeta.Parameters.Add(t);
            }
            foreach (var i in lstOutput)
            {
                var t = new Rtti.TtClassMeta.TtMethodMeta.TtParamMeta();
                t.Name = i.VarName;
                t.ParameterType = i.OutputType;
                t.ArgumentAttribute = Bricks.CodeBuilder.EMethodArgumentAttribute.Out;
                t.DefaultValue = i.GetDefaultValueObject();
                MethodMeta.Parameters.Add(t);
            }
            MethodMeta.ReturnType = Rtti.TtTypeDesc.TypeOf(typeof(void));
            MethodMeta.MethodName = AssetName.PureName + "_" + UniHash32.APHash(this.AssetName.ToString());

            var MaterialClass = new TtClassDeclaration();

            var gen = mHLSLCodeGen.GetCodeObjectGen(Rtti.TtTypeDescGetter<TtMethodDeclaration>.TypeDesc);
            BuildCodeStatementsData data = new BuildCodeStatementsData()
            {
                ClassDec = MaterialClass,
                NodeGraph = MaterialGraph,
                UserData = material,
                CodeGen = mHLSLCodeGen,
            };
            TtMethodDeclaration MtlFunction = new TtMethodDeclaration();
            MtlFunction.MethodName = this.MethodMeta.MethodName;
            MtlFunction.Arguments.Clear();
            foreach (var i in this.MethodMeta.Parameters)
            {
                MtlFunction.Arguments.Add(
                new TtMethodArgumentDeclaration()
                {
                    OperationType = i.ArgumentAttribute,
                    VariableType = new TtTypeReference(i.ParameterType),
                    VariableName = i.Name,
                    InitValue = new TtPrimitiveExpression(Rtti.TtTypeDesc.TypeOf(i.DefaultValue.GetType()), i.DefaultValue),
                });
            }
            data.CurrentStatements = MtlFunction.MethodBody.Sequence;
            data.MethodDec = MtlFunction;

            foreach (var i in MaterialGraph.Nodes)
            {
                var v = i as IMaterialFunctionOutput;
                if (v == null)
                    continue;
                foreach (var j in v.OutPins)
                {
                    if (j.HasLinker())
                    {
                        var linker = MaterialGraph.FindInLinkerSingle(j);
                        var opPin = MaterialGraph.GetOppositePin(j);
                        var pinNode = MaterialGraph.GetOppositePinNode(j);
                        pinNode.BuildStatements(opPin, ref data);
                        var exp = MaterialGraph.GetOppositePinExpression(j, ref data);
                        var assign = new TtAssignOperatorStatement()
                        {
                            From = exp,
                            To = new TtVariableReferenceExpression(v.GetSetter(i, j), null),
                        };
                        MtlFunction.MethodBody.Sequence.Add(assign);
                    }
                }
            }

            string code = "";
            var incGen = mHLSLCodeGen.GetCodeObjectGen(Rtti.TtTypeDescGetter<TtIncludeDeclaration>.TypeDesc);
            TtCodeGeneratorData genData = new TtCodeGeneratorData()
            {
                Method = null,
                CodeGen = mHLSLCodeGen,
                UserData = this,
            };
            //Material.IncludeFiles.Clear();
            foreach (var i in MaterialClass.PreIncludeHeads)
            {
                incGen.GenCodes(i, ref code, ref genData);
                //Material.IncludeFiles.Add(i.FilePath);
            }
            genData = new TtCodeGeneratorData()
            {
                Method = MtlFunction,
                CodeGen = mHLSLCodeGen,
                UserData = this,
            };
            gen.GenCodes(MtlFunction, ref code, ref genData);
            
            return code;
        }
        public static TtMaterialFunction LoadXnd(TtMaterialFunctionManager manager, IO.TtXndNode node)
        {
            IO.ISerializer result = null;
            var attr = node.TryGetAttribute("MaterialFunction");
            if (attr.NativePointer != IntPtr.Zero)
            {
                using (var ar = attr.GetReader(null))
                {
                    ar.Read(out result, null);
                }
            }

            var material = result as TtMaterialFunction;
            if (material != null)
            {
                //material.UpdateShaderCode(false);
                return material;
            }
            return null;
        }
        public static bool ReloadXnd(TtMaterialFunction material, TtMaterialFunctionManager manager, IO.TtXndNode node)
        {
            var attr = node.TryGetAttribute("MaterialFunction");
            if (attr.NativePointer != IntPtr.Zero)
            {
                using (var ar = attr.GetReader(null))
                {
                    try
                    {
                        ar.ReadTo(material, null);
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                }
            }
            return true;
        }
        [Rtti.Meta("")]
        [RName.PGRName(ReadOnly = true)]
        [Category("Option")]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion
        #region IShaderCodeProvider
        public NxRHI.TtShaderCode DefineCode { get; } = new NxRHI.TtShaderCode();
        public NxRHI.TtShaderCode SourceCode { get; } = new NxRHI.TtShaderCode();
        #endregion

        [Rtti.Meta("")]
        [Category("Option")]
        public EMaterialFunctionEditMode EditMode { get; set; } = EMaterialFunctionEditMode.Graph;

        [Rtti.Meta("")]
        [ReadOnly(true)]
        public string GraphXMLString { get; set; }
        [Rtti.Meta("")]
        public string HLSLCode { get; set; }
        [Rtti.Meta("")]
        public Rtti.TtClassMeta.TtMethodMeta MethodMeta { get; set; } = new Rtti.TtClassMeta.TtMethodMeta();

        [Rtti.Meta("")]
        [Category("Option")]
        public List<RName> RefMaterialFunctions { get; set; } = new List<RName>();
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
            set
            {
                mCallNodeName = value;
            }
        }

        private static readonly Dictionary<string, (Rtti.TtTypeDesc Type, object Default)> HLSLTypeMap = new Dictionary<string, (Rtti.TtTypeDesc, object)>(StringComparer.OrdinalIgnoreCase)
        {
            { "float", (Rtti.TtTypeDescGetter<float>.TypeDesc, 0.0f) },
            { "float2", (Rtti.TtTypeDescGetter<Vector2>.TypeDesc, Vector2.Zero) },
            { "float3", (Rtti.TtTypeDescGetter<Vector3>.TypeDesc, Vector3.Zero) },
            { "float4", (Rtti.TtTypeDescGetter<Vector4>.TypeDesc, Vector4.Zero) },
            { "int", (Rtti.TtTypeDescGetter<int>.TypeDesc, 0) },
            { "uint", (Rtti.TtTypeDescGetter<uint>.TypeDesc, 0u) },
            { "PS_INPUT", (Rtti.TtTypeDescGetter<PS_INPUT>.TypeDesc, new PS_INPUT()) },
            { "SamplerState", (Rtti.TtTypeDescGetter<Bricks.CodeBuilder.ShaderNode.Var.SamplerState>.TypeDesc, null) },
            { "Texture2D", (Rtti.TtTypeDescGetter<Bricks.CodeBuilder.ShaderNode.Var.Texture2D>.TypeDesc, null) },
        };

        /// <summary>
        /// Parse the first function signature in HLSLCode and fill MethodMeta accordingly.
        /// Supports: void FuncName(in/out/inout Type paramName, ...)
        /// </summary>
        public string GetExpectedFunctionName()
        {
            if (AssetName == null)
                return null;
            return AssetName.PureName + "_" + UniHash32.APHash(AssetName.ToString());
        }
        /// <summary>
        /// Extract @default(...) annotations from comment lines in the original HLSL.
        /// Maps parameter name to the raw default value string.
        /// Supported syntax: "in float Foo, // @default(1.5)" or "in float3 Bar, // @default(0.1, 0.2, 0.3)"
        /// </summary>
        private static Dictionary<string, string> ExtractDefaultAnnotations(string hlslCode)
        {
            var defaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            // Match lines like: "in float3 ParamName,   // @default(0.1, 0.2, 0.3)"
            // or the last param without trailing comma: "out float ParamName)  // @default(0.0)"
            var pattern = @"(?:in|out|inout)\s+\w+\s+(\w+)\s*[,)]\s*//\s*@default\(([^)]*)\)";
            var matches = System.Text.RegularExpressions.Regex.Matches(hlslCode, pattern);
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                var paramName = m.Groups[1].Value;
                var defaultVal = m.Groups[2].Value.Trim();
                defaults[paramName] = defaultVal;
            }
            return defaults;
        }

        /// <summary>
        /// Parse a @default(...) string value into a typed object based on the HLSL type.
        /// </summary>
        private static object ParseDefaultValueString(string defaultStr, string typeName)
        {
            if (string.IsNullOrWhiteSpace(defaultStr))
                return null;

            var parts = defaultStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            switch (typeName.ToLowerInvariant())
            {
                case "float":
                    if (float.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f))
                        return f;
                    break;
                case "float2":
                    if (parts.Length >= 2 &&
                        float.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var x2) &&
                        float.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var y2))
                        return new Vector2(x2, y2);
                    break;
                case "float3":
                    if (parts.Length >= 3 &&
                        float.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var x3) &&
                        float.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var y3) &&
                        float.TryParse(parts[2].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var z3))
                        return new Vector3(x3, y3, z3);
                    break;
                case "float4":
                    if (parts.Length >= 4 &&
                        float.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var x4) &&
                        float.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var y4) &&
                        float.TryParse(parts[2].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var z4) &&
                        float.TryParse(parts[3].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var w4))
                        return new Vector4(x4, y4, z4, w4);
                    break;
                case "int":
                    if (int.TryParse(parts[0].Trim(), out var i))
                        return i;
                    break;
                case "uint":
                    if (uint.TryParse(parts[0].Trim(), out var u))
                        return u;
                    break;
            }
            return null;
        }

        public void ParseMethodMetaFromHLSL()
        {
            if (string.IsNullOrEmpty(HLSLCode))
                return;

            // Extract @default(...) annotations from comments BEFORE stripping them
            var defaultAnnotations = ExtractDefaultAnnotations(HLSLCode);

            // Strip all comments BEFORE regex matching, so that parentheses
            // inside comments (e.g. "// range (0.02 ~ 0.1)") don't break
            // the function signature extraction regex.
            var strippedCode = System.Text.RegularExpressions.Regex.Replace(HLSLCode, @"//[^\n]*", "");
            strippedCode = System.Text.RegularExpressions.Regex.Replace(strippedCode, @"/\*.*?\*/", "", System.Text.RegularExpressions.RegexOptions.Singleline);

            // Match pattern: void FuncName ( params )
            var funcMatch = System.Text.RegularExpressions.Regex.Match(strippedCode,
                @"void\s+(\w+)\s*\(([^)]*)\)");
            if (!funcMatch.Success)
                return;

            var functionName = funcMatch.Groups[1].Value;
            var paramsStr = funcMatch.Groups[2].Value.Trim();

            // Auto-correct function name to include asset hash for uniqueness
            var expectedName = GetExpectedFunctionName();
            if (expectedName != null && functionName != expectedName)
            {
                HLSLCode = HLSLCode.Replace(functionName, expectedName);
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Info,
                    $"MaterialFunction.ParseMethodMetaFromHLSL: Renamed '{functionName}' -> '{expectedName}'");
                functionName = expectedName;
            }

            MethodMeta.MethodName = functionName;
            MethodMeta.ReturnType = Rtti.TtTypeDesc.TypeOf(typeof(void));
            MethodMeta.Parameters = new List<Rtti.TtClassMeta.TtMethodMeta.TtParamMeta>();

            if (string.IsNullOrWhiteSpace(paramsStr))
                return;

            var paramParts = SplitHLSLParameters(paramsStr);
            foreach (var paramStr in paramParts)
            {
                var trimmed = paramStr.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                var paramMeta = ParseSingleParameter(trimmed);
                if (paramMeta != null)
                {
                    // Override default value from @default(...) annotation if present
                    if (defaultAnnotations.TryGetValue(paramMeta.Name, out var defaultStr))
                    {
                        // Determine HLSL type name from the trimmed parameter string
                        var tokens = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        var typeIdx = (tokens[0].ToLowerInvariant() == "in" || tokens[0].ToLowerInvariant() == "out" || tokens[0].ToLowerInvariant() == "inout") ? 1 : 0;
                        var hlslTypeName = tokens[typeIdx];
                        var parsedDefault = ParseDefaultValueString(defaultStr, hlslTypeName);
                        if (parsedDefault != null)
                        {
                            paramMeta.DefaultValue = parsedDefault;
                        }
                    }
                    MethodMeta.Parameters.Add(paramMeta);
                }
                else
                {
                    Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Error, $"MaterialFunction.ParseMethodMetaFromHLSL: Unrecognized parameter '{trimmed}', skipped.");
                }
            }
        }

        private static List<string> SplitHLSLParameters(string paramsStr)
        {
            var result = new List<string>();
            int depth = 0;
            int start = 0;
            for (int i = 0; i < paramsStr.Length; i++)
            {
                var ch = paramsStr[i];
                if (ch == '(' || ch == '<') depth++;
                else if (ch == ')' || ch == '>') depth--;
                else if (ch == ',' && depth == 0)
                {
                    result.Add(paramsStr.Substring(start, i - start));
                    start = i + 1;
                }
            }
            result.Add(paramsStr.Substring(start));
            return result;
        }

        private static Rtti.TtClassMeta.TtMethodMeta.TtParamMeta ParseSingleParameter(string paramStr)
        {
            // Possible formats:
            //   in PS_INPUT input
            //   out float3 result
            //   inout float4 color
            //   float3 normal  (default is "in")
            var tokens = paramStr.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2)
                return null;

            var direction = Bricks.CodeBuilder.EMethodArgumentAttribute.In;
            int typeIndex = 0;

            var firstToken = tokens[0].ToLowerInvariant();
            if (firstToken == "in")
            {
                direction = Bricks.CodeBuilder.EMethodArgumentAttribute.In;
                typeIndex = 1;
            }
            else if (firstToken == "out")
            {
                direction = Bricks.CodeBuilder.EMethodArgumentAttribute.Out;
                typeIndex = 1;
            }
            else if (firstToken == "inout")
            {
                direction = Bricks.CodeBuilder.EMethodArgumentAttribute.Ref;
                typeIndex = 1;
            }

            if (typeIndex + 1 >= tokens.Length)
                return null;

            var typeName = tokens[typeIndex];
            var paramName = tokens[typeIndex + 1];

            if (!HLSLTypeMap.TryGetValue(typeName, out var typeInfo))
                return null;

            var meta = new Rtti.TtClassMeta.TtMethodMeta.TtParamMeta();
            meta.Name = paramName;
            meta.ParameterType = typeInfo.Type;
            meta.ArgumentAttribute = direction;
            meta.DefaultValue = typeInfo.Default;
            return meta;
        }

        private Bricks.CodeBuilder.ShaderNode.TtMaterialFunctionGraph LoadGraph()
        {
            var MaterialGraph = new Bricks.CodeBuilder.ShaderNode.TtMaterialFunctionGraph();
            var xml = IO.TtFileManager.LoadXmlFromString(this.GraphXMLString);
            if (xml != null)
            {
                object pThis = MaterialGraph;
                IO.SerializerHelper.ReadObjectMetaFields(this, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
            }
            return MaterialGraph;
        }
        public void WriteRefHLSLCode(ref string code)
        {
            if (EditMode == EMaterialFunctionEditMode.Graph)
            {
                var MaterialGraph = LoadGraph();
                foreach (var i in MaterialGraph.Nodes)
                {
                    var f = i as Bricks.CodeBuilder.ShaderNode.Control.TtCallMaterialFunctionNode;
                    if (f == null)
                        continue;
                    var refFunc = f.FunctionName.GetAsset<Graphics.Pipeline.Shader.TtMaterialFunction>().GetResultUntilCompleted();
                    if (refFunc != null)
                    {
                        refFunc.WriteRefHLSLCode(ref code);
                    }
                }
            }
            else
            {
                foreach (var refName in RefMaterialFunctions)
                {
                    if (refName == null)
                        continue;
                    var refFunc = refName.GetAsset<Graphics.Pipeline.Shader.TtMaterialFunction>().GetResultUntilCompleted();
                    if (refFunc != null)
                    {
                        refFunc.WriteRefHLSLCode(ref code);
                    }
                }
            }

            if (string.IsNullOrEmpty(HLSLCode))
                return;
            if (code.Contains(HLSLCode))
                return;
            code += HLSLCode;
            code += "\n";
        }
    }
    public partial class TtMaterialFunctionManager
    {
        private List<RName> mMaterialFunctionNames = null;
        public List<RName> MaterialFunctionNames
        {
            get
            {
                if (mMaterialFunctionNames == null)
                {
                    mMaterialFunctionNames = new List<RName>();
                    lock (mMaterialFunctionNames)
                    {   
                        var root = EngineNS.TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game);
                        var files = IO.TtFileManager.GetFiles(root, "*" + TtMaterialFunction.AssetExt, true);
                        foreach (var file in files)
                        {
                            var rn = IO.TtFileManager.GetRelativePath(root, file);
                            mMaterialFunctionNames.Add(RName.GetRName(rn, RName.ERNameType.Game));
                        }
                    }
                }
                return mMaterialFunctionNames;
            }
        }
        public void RegMaterialFunctionName(RName rn)
        {
            lock (MaterialFunctionNames)
            {
                if (MaterialFunctionNames.Contains(rn) == false)
                {
                    MaterialFunctionNames.Add(rn);
                }
            }
        }

        public Dictionary<RName, TtMaterialFunction> MaterialFunctions { get; } = new Dictionary<RName, TtMaterialFunction>();
        public static async Thread.Async.TtTask<TtMaterialFunction> CreateMaterialFunction(RName rn)
        {
            TtMaterialFunction result;
            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var material = TtMaterialFunction.LoadXnd(null, xnd.RootNode);
                        if (material == null)
                            return null;

                        material.AssetName = rn;
                        return material;
                    }
                    else
                    {
                        return null;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            return result;
        }
        public async Thread.Async.TtTask<TtMaterialFunction> GetMaterialFunction(RName rn)
        {
            if (rn == null)
                return null;

            TtMaterialFunction result;
            if (MaterialFunctions.TryGetValue(rn, out result))
                return result;

            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var material = TtMaterialFunction.LoadXnd(this, xnd.RootNode);
                        if (material == null)
                            return null;

                        material.AssetName = rn;
                        return material;
                    }
                    else
                    {
                        return null;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (result != null)
            {
                MaterialFunctions[rn] = result;
                return result;
            }

            return null;
        }
        public async Thread.Async.TtTask<bool> ReloadMaterialFuntion(RName rn)
        {
            TtMaterialFunction result;
            if (MaterialFunctions.TryGetValue(rn, out result) == false)
                return true;

            var ok = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        return TtMaterialFunction.ReloadXnd(result, this, xnd.RootNode);
                    }
                    else
                    {
                        return false;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            var effects = TtEngine.Instance.GfxDevice.EffectManager.GraphicsEffects;
            foreach (var i in effects)
            {
                if (i.Value.Desc.MaterialName == rn)
                {
                    //if (i.Value.Desc.MaterialHash != result.MaterialHash)
                    //{
                    //    await i.Value.RefreshEffect(result);
                    //}
                }
            }

            return ok;
        }
    }
    
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Graphics.Pipeline.Shader
{
	partial class TtMaterialFunction
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