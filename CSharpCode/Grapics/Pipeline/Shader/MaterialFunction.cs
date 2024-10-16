using EngineNS.Bricks.CodeBuilder.ShaderNode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.NodeGraph;
using Microsoft.Toolkit.HighPerformance.Helpers;
using static Org.BouncyCastle.Math.Primes;

namespace EngineNS.Graphics.Pipeline.Shader
{
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
        protected override Color4b GetBorderColor()
        {
            return TtEngine.Instance.EditorInstance.Config.MaterialFunctionBoderColor;
        }
        public override async Thread.Async.TtTask SaveRefAssets()
        {
            //Stop Editor Operate
            TtEngine.Instance.StopOperation($"{this.AssetName}: SaveRefAssets");
            var holders = new List<IO.IAssetMeta>();
            TtEngine.Instance.AssetMetaManager.GetAssetHolder(this, holders);
            foreach (var i in holders)
            {
                var mtl = i as TtMaterialAMeta;
                if (mtl == null)
                    continue;
                var holdAsset = await mtl.LoadAsset();
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
    }
    [TtMaterialFunction.MaterialFunctionImport]
    [IO.AssetCreateMenu(MenuName = "Graphics/MaterialFunction")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtMaterialFunction : IO.ISerializer, IO.IAsset, IShaderCodeProvider
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
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }
        public virtual void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {

        }
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
        public virtual void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();

            ameta.RefAssetRNames.AddRange(this.RefMaterialFunctions);
        }
        [Rtti.Meta]
        public virtual void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }

            var MaterialGraph = new Bricks.CodeBuilder.ShaderNode.TtMaterialFunctionGraph();
            var xml = IO.TtFileManager.LoadXmlFromString(this.GraphXMLString);
            if (xml != null)
            {
                object pThis = MaterialGraph;
                IO.SerializerHelper.ReadObjectMetaFields(this, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
            }
            
            HLSLCode = GenMateralFunctionGraphCode(new UHLSLCodeGenerator(), MaterialGraph);

            var typeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(this.GetType());
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

            TtEngine.Instance.SourceControlModule.AddFile(name.Address);

            TtEngine.Instance.GfxDevice.MaterialFunctionManager.RegMaterialFunctionName(AssetName);

            _ = ameta.SaveRefAssets();
        }
        public string GenMateralFunctionGraphCode(UHLSLCodeGenerator mHLSLCodeGen,
            Bricks.CodeBuilder.ShaderNode.TtMaterialFunctionGraph MaterialGraph)
        {
            var lstInput = new List<IMaterialFunctionInput>();
            RefMaterialFunctions.Clear();
            foreach (var i in MaterialGraph.Nodes)
            {
                var f = i as Bricks.CodeBuilder.ShaderNode.Control.TtCallMaterialFunctionNode;
                if (f != null)
                {
                    RefMaterialFunctions.Add(f.FunctionName);
                }
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
                UserData = this,
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
        [Rtti.Meta]
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
        [Rtti.Meta]
        public List<RName> RefMaterialFunctions { get; set; } = new List<RName>();
        #endregion

        [Rtti.Meta]
        [ReadOnly(true)]
        public string GraphXMLString { get; set; }
        [Rtti.Meta]
        [ReadOnly(true)]
        public string HLSLCode { get; set; }
        [Rtti.Meta]
        public Rtti.TtClassMeta.TtMethodMeta MethodMeta { get; set; } = new Rtti.TtClassMeta.TtMethodMeta();
        private string mCallNodeName = null;
        [Rtti.Meta]
        [Category("Option")]
        public string CallNodeName
        {
            get
            {
                if (mCallNodeName != null)
                    return mCallNodeName;
                return AssetName.Name;
            }
            set
            {
                mCallNodeName = value;
            }
        }

        public void WriteRefHLSLCode(ref string code)
        {
            foreach (var i in RefMaterialFunctions)
            {
                var refFunc = TtEngine.Instance.GfxDevice.MaterialFunctionManager.GetMaterialFunctionSync(i);
                if (refFunc != null)
                {
                    refFunc.WriteRefHLSLCode(ref code);
                }
            }
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
        public async Thread.Async.TtTask<TtMaterialFunction> CreateMaterialFunction(RName rn)
        {
            TtMaterialFunction result;
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
        public TtMaterialFunction GetMaterialFunctionSync(RName rn)
        {
            if (rn == null)
                return null;

            TtMaterialFunction result;
            if (MaterialFunctions.TryGetValue(rn, out result))
                return result;

            using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
            {
                if (xnd != null)
                {
                    result = TtMaterialFunction.LoadXnd(this, xnd.RootNode);
                }
            }

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

            var effects = TtEngine.Instance.GfxDevice.EffectManager.Effects;
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
