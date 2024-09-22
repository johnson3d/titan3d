﻿using EngineNS.Editor;
using Microsoft.CodeAnalysis;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public partial class UMacrossEditor : IO.ISerializer, Editor.IAssetEditor, IRootForm, NodeGraph.IGraphEditor, IMacrossMethodHolder
    {
        public UMacrossEditor()
        {
            mNewMethodMenuState.Reset();
            mOverrideMenuState.Reset();
        }
        public void Dispose()
        {

        }
        public bool IsGenShader { get; set; } = false;
        bool mInitialized = false;
        public virtual async Thread.Async.TtTask<bool> Initialize()
        {
            if (mInitialized)
                return true;
            mInitialized = true;
            InitializeManMenu();
            await PGMember.Initialize();
            await mUnionNodeConfigRenderer.Initialize();
            mCodeEditor.mCoreObject.SetLanguage("C#");
            mCodeEditor.mCoreObject.ApplyLangDefine();
            mCodeEditor.mCoreObject.ApplyErrorMarkers();
            return true;
        }

        List<EGui.UIProxy.MenuItemProxy> mOverrideMethodMenuItems = new List<EGui.UIProxy.MenuItemProxy>();
        void InitializeOverrideMethodList()
        {
            for(int i=0; i<DefClass.SupperClassNames.Count; i++)
            {
                var superClass = Rtti.TtClassMetaManager.Instance.GetMetaFromFullName(DefClass.SupperClassNames[i]);
                //var superClass = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(DefClass.SupperClassNames[i]);
                if (superClass == null)
                    continue;

                mOverrideMethodMenuItems.Clear();
                var methods = superClass.Methods; //.SystemType.GetMethods();
                for(int methodIdx=0; methodIdx < methods.Count; methodIdx++)
                {
                    if (!methods[methodIdx].IsVirtual)
                        continue;
                    var method = methods[methodIdx];

                    bool hasMethod = false;
                    var keyWord = TtMethodDeclaration.GetKeyword(method);
                    for(int mI = 0; mI < Methods.Count; mI++)
                    {
                        for(int dataIdx =0; dataIdx < Methods[mI].MethodDatas.Count; dataIdx++)
                        {
                            if(keyWord == Methods[mI].MethodDatas[dataIdx].MethodDec.GetKeyword())
                            {
                                hasMethod = true;
                                break;
                            }
                        }
                        if (hasMethod)
                            break;
                    }
                    if (hasMethod)
                        continue;

                    var menuItem = new EGui.UIProxy.MenuItemProxy();
                    //menuItem.MenuName = method.Name + "(";
                    //var parameters = method.GetParameters();
                    //for(int paramIdx = 0; paramIdx < parameters.Length; paramIdx++)
                    //{
                    //    menuItem.MenuName += parameters[paramIdx].ParameterType.Name + ",";
                    //}
                    //menuItem.MenuName = menuItem.MenuName.TrimEnd(',');
                    //menuItem.MenuName += ")";
                    menuItem.MenuName = keyWord;

                    menuItem.Action = (proxy, data) =>
                    {
                        var f = TtMethodDeclaration.GetMethodDeclaration(method);
                        var func = AddMethod(f);
                        func.AssetName = this.AssetName;
                        menuItem.Visible = false;
                    };

                    mOverrideMethodMenuItems.Add(menuItem);
                }
            }
        }

        public IRootForm GetRootForm()
        {
            return this;
        }
        public void OnPreRead(object tagObject, object hostObject, bool fromXml) { }
        public void OnPropertyRead(object root, System.Reflection.PropertyInfo prop, bool fromXml) { }
        [Rtti.Meta(Order = 0)]
        public TtClassDeclaration DefClass { get; } = new TtClassDeclaration();
        //public DefineClass DefClass { get; } = new DefineClass();
        //[Rtti.Meta(Order = 1)]
        public List<UMacrossMethodGraph> Methods { get; } = new List<UMacrossMethodGraph>();
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible 
        {
            get => mVisible;
            set => mVisible = value;
        }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public EGui.Controls.PropertyGrid.PropertyGrid PGMember { get; set; } = new EGui.Controls.PropertyGrid.PropertyGrid();
        public List<UMacrossMethodGraph> OpenFunctions = new List<UMacrossMethodGraph>();
        public MemberVar DraggingMember { get; set; }
        public bool IsDraggingMember { get; set; } = false;
        public MethodLocalVar DraggingLocalVar { get; set; }
        public bool IsDraggingLocalVar { get; set; } = false;
        public float LeftWidth = 300;
        //bool bFirstDraw = true;
        UCSharpCodeGenerator mCSCodeGen = new UCSharpCodeGenerator();
        public UCSharpCodeGenerator CSCodeGen => mCSCodeGen;

        UHLSLCodeGenerator mHlslCodeGen = new UHLSLCodeGenerator();
        public UHLSLCodeGenerator HlslCodeGen => mHlslCodeGen;

        public TtCodeGeneratorBase CodeGen
        {
            get
            {
                if (IsGenShader)
                    return HlslCodeGen;
                else
                    return CSCodeGen;
            }
        }

        public void SaveClassGraph(RName rn)
        {
            var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName) as UMacrossAMeta;
            if (ameta != null)
            {
                if(DefClass.SupperClassNames.Count > 0)
                {
                    var baseType = Rtti.TtTypeDesc.TypeOfFullName(DefClass.SupperClassNames[0]);
                    if(baseType != null && ameta.BaseTypeStr != baseType.TypeString)
                    {
                        var tmp = new TtMacross();
                        tmp.AssetName = rn;
                        tmp.SelectedType = baseType;
                        tmp.UpdateAMetaReferences(ameta);
                        ameta.SaveAMeta(tmp);
                    }
                    else if(baseType == null && !string.IsNullOrEmpty(ameta.BaseTypeStr))
                    {
                        var tmp = new TtMacross();
                        tmp.AssetName = rn;
                        tmp.SelectedType = null;
                        tmp.UpdateAMetaReferences(ameta);
                        ameta.SaveAMeta(tmp);
                    }
                }
            }

            if (!IO.TtFileManager.DirectoryExists(rn.Address))
                IO.TtFileManager.CreateDirectory(rn.Address);
            
            var xml = new System.Xml.XmlDocument();
            var xmlRoot = xml.CreateElement($"Root", xml.NamespaceURI);
            xml.AppendChild(xmlRoot);
            IO.SerializerHelper.WriteObjectMetaFields(xml, xmlRoot, this);
            var xmlText = IO.TtFileManager.GetXmlText(xml);
            var graphDataFileName = $"{rn.Address}/class_graph.dat";
            IO.TtFileManager.WriteAllText(graphDataFileName, xmlText);
            TtEngine.Instance.SourceControlModule.AddFile(graphDataFileName);

            for(int i=0; i<Methods.Count; i++)
            {
                var funcXml = new System.Xml.XmlDocument();
                var funcXmlRoot = funcXml.CreateElement($"Root", funcXml.NamespaceURI);
                funcXml.AppendChild(funcXmlRoot);
                IO.SerializerHelper.WriteObjectMetaFields(funcXml, funcXmlRoot, Methods[i]);
                var funcXmlText = IO.TtFileManager.GetXmlText(funcXml);
                string declName = "";
                foreach (var k in Methods[i].MethodDatas)
                {
                    declName += k.MethodDec.GetKeyword();
                }
                var methodFileName = $"{rn.Address}/{Methods[i].Name}_{Hash160.CreateHash160(declName)}.fn";
                IO.TtFileManager.WriteAllText(methodFileName, funcXmlText);
                TtEngine.Instance.SourceControlModule.AddFile(methodFileName);
            }

            //LoadClassGraph(rn);
        }
        public void LoadClassGraph(RName rn)
        {
            AssetName = rn;
            DefClass.Reset();
            Methods.Clear();
            OpenFunctions.Clear();
            PGMember.Target = null;

            //Rtti.UTypeDescManager.Instance.Services .InterateTypes 

            {
                // old
                //var xml = IO.FileManager.LoadXml(rn.Address);
                //object pThis = this;
                //IO.SerializerHelper.ReadObjectMetaFields(this, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
            }
            {
                var xml = IO.TtFileManager.LoadXml($"{rn.Address}/class_graph.dat");
                if (xml == null)
                    return;
                object pThis = this;
                IO.SerializerHelper.ReadObjectMetaFields(this, xml.LastChild as System.Xml.XmlElement, ref pThis, null);

                var nsName = IO.TtFileManager.GetBaseDirectory(rn.Name).TrimEnd('/').Replace("/", ".");
                if (Regex.IsMatch(nsName, "[A-Za-z0-9_]"))
                    DefClass.Namespace = new TtNamespaceDeclaration("NS_" + nsName);
                else
                {
                    DefClass.Namespace = new TtNamespaceDeclaration("NS_" + ((UInt32)nsName.GetHashCode()).ToString());
                    Profiler.Log.WriteLine<Profiler.TtMacrossCategory>(Profiler.ELogTag.Warning, $"Get namespace failed, {rn.Name} has invalid char!");
                }    
                DefClass.ClearMethods();
                var funcFiles = IO.TtFileManager.GetFiles(rn.Address, "*.fn", false);
                if (funcFiles.Length == 0)
                {
                    funcFiles = IO.TtFileManager.GetFiles(rn.Address, "*.func", false);
                }
                for (int i = 0; i < funcFiles.Length; i++)
                {
                    try
                    {
                        var funcXml = IO.TtFileManager.LoadXml(funcFiles[i]);
                        var funcGraph = UMacrossMethodGraph.NewGraph(this);
                        object pFuncGraph = funcGraph;
                        IO.SerializerHelper.ReadObjectMetaFields(null, funcXml.LastChild as System.Xml.XmlElement, ref pFuncGraph, null);
                        for(int methodIdx = 0; methodIdx < funcGraph.MethodDatas.Count; methodIdx++)
                        {
                            //if (funcGraph.MethodDatas[methodIdx].IsDelegate)
                            //    continue;
                            DefClass.AddMethod(funcGraph.MethodDatas[methodIdx].MethodDec);
                        }
                        funcGraph.AssetName = this.AssetName;
                        Methods.Add(funcGraph);
                    }
                    catch (System.Exception e)
                    {
                        Profiler.Log.WriteException(e);
                    }
                }
                for(int i=0; i<Methods.Count; i++)
                {
                    Methods[i].CanvasMenuDirty = true;

                    for (var nodeIdx = 0; nodeIdx < Methods[i].Nodes.Count; nodeIdx++)
                    {
                        var node = Methods[i].Nodes[nodeIdx];
                        node.UserData = this;
                        Methods[i].SetDefaultActionForNode(node);
                        var methodNode = node as MethodNode;
                        if (methodNode == null)
                            continue;

                        for (int methodIdx = 0; methodIdx < DefClass.Methods.Count; methodIdx++)
                        {
                            var meta = MethodNode.GetMethodMeta(DefClass.Methods[methodIdx]);
                            var metaWithoutNS = meta;
                            if(DefClass.Namespace != null)
                            {
                                metaWithoutNS = meta.Replace(DefClass.Namespace.Namespace + ".", "");
                            }
                            if (methodNode.MethodMeta == meta || methodNode.MethodMeta == metaWithoutNS)
                            {
                                methodNode.MethodDesc = DefClass.Methods[methodIdx];
                            }
                        }
                        methodNode.SetDelegateGraph(this);
                    }
                }
            }

            InitializeOverrideMethodList();

            foreach (var i in DefClass.SupperClassNames)
            {
                var type = Rtti.TtTypeDesc.TypeOfFullName(i);
                var macrossAttr = type.GetCustomAttribute<Macross.TtMacrossAttribute>(false);
                if (macrossAttr != null)
                {
                    this.IsGenShader = macrossAttr.IsGenShader;
                    return;
                }
            }
        }
        public Action<TtClassDeclaration> BeforeGenerateCode;
        public string GenerateCode()
        {
            try
            {
                BeforeGenerateCode?.Invoke(DefClass);

                foreach (var i in Methods)
                {
                    i.BuildExpression(DefClass);
                }

                string code = "";
                if (IsGenShader)
                {
                    foreach(var i in this.Methods)
                    {
                        if (i.IsUseCustumCode)
                        {
                            code += i.CustumCode;
                            continue;
                        }
                        foreach(var j in i.MethodDatas)
                        {
                            var mtd = DefClass.FindMethod(j.GetMethodName());
                            if (mtd.OverrideMethod != null)
                            {
                                var meta = mtd.OverrideMethod.GetFirstCustomAttribute<Rtti.MetaAttribute>(false);
                                if (meta.ShaderName == null)
                                {
                                    code += $"//warning:{mtd.MethodName} is not a shader funtion";
                                }
                                continue;
                            }
                            string funcCode = "";
                            var data = new TtCodeGeneratorData(DefClass.Namespace, DefClass, mHlslCodeGen, null);
                            var methodDecGen = data.CodeGen.GetCodeObjectGen(j.MethodDec.GetType());
                            methodDecGen.GenCodes(j.MethodDec, ref funcCode, ref data);
                            code += funcCode;
                        }
                    }
                    //mHlslCodeGen.GenerateClassCode(DefClass, AssetName, ref code);
                    code += "\n";
                    code += $"#define USER_EMITTER\n";
                    SaveHlslFile(code);
                    mCodeEditor.mCoreObject.SetText(code);
                    TextEditorTitle = AssetName.PureName;

                    code = "";
                    mCSCodeGen.GenerateClassCode(DefClass, AssetName, ref code);
                    SaveCSFile(code);
                }
                else
                {
                    mCSCodeGen.GenerateClassCode(DefClass, AssetName, ref code);
                    SaveCSFile(code);
                    mCodeEditor.mCoreObject.SetText(code);
                    TextEditorTitle = AssetName.PureName;
                }
                DefClass.ResetRuntimeData();
                //GenerateAssemblyDescCreateInstanceCode();

                EngineNS.TtEngine.Instance.MacrossManager.GenerateProjects();
                return code;
            }
            catch (NodeGraph.GraphException ex)
            {
                if (ex.ErrorNode != null)
                {
                    ex.ErrorNode.HasError = true;
                    ex.ErrorNode.CodeExcept = ex;
                }
                Profiler.Log.WriteException(ex);
                return "";
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return "";
            }
        }
        public string GenerateMethodCode(TtCodeGeneratorBase gen, int methodIndex)
        {
            if (methodIndex < 0 || methodIndex >= DefClass.Methods.Count)
                return "";

            string funcCode = "";
            try
            {
                var mtd = DefClass.Methods[methodIndex];
                if (gen == mHlslCodeGen && mtd.OverrideMethod != null)
                {
                    var meta = mtd.OverrideMethod.GetFirstCustomAttribute<Rtti.MetaAttribute>(false);
                    if (meta.ShaderName == null)
                        return $"//warning:{mtd.MethodName} is not a shader funtion";
                }

                BeforeGenerateCode?.Invoke(DefClass);
                foreach (var i in Methods)
                {
                    //if (i.MethodDatas[0].MethodDec == mtd)
                    {
                        i.BuildExpression(DefClass);
                    }
                }
                
                var data = new TtCodeGeneratorData(DefClass.Namespace, DefClass, gen, null);
                var methodDecGen = data.CodeGen.GetCodeObjectGen(mtd.GetType());
                methodDecGen.GenCodes(mtd, ref funcCode, ref data);
            }
            catch (NodeGraph.GraphException ex)
            {
                if (ex.ErrorNode != null)
                {
                    ex.ErrorNode.HasError = true;
                    ex.ErrorNode.CodeExcept = ex;
                }
                Profiler.Log.WriteException(ex);
                return "";
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return "";
            }
            

            return funcCode;
        }

        //void GenerateAssemblyDescCreateInstanceCode()
        //{
        //    var projFolder = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource) + IO.TtFileManager.GetParentPathName(TtEngine.Instance.EditorInstance.Config.GameProject);
        //    var assemblyFileName = projFolder + "/Assembly.cs";
        //    string assemblyDescCodes = "";
        //    using(var sr = new System.IO.StreamReader(assemblyFileName, Encoding.UTF8, true))
        //    {
        //        assemblyDescCodes = sr.ReadToEnd();
        //    }

        //    var startKeyword = "#region MacrossGenerated Start";
        //    var idx = assemblyDescCodes.IndexOf(startKeyword);
        //    var startIdx = idx + startKeyword.Length + 1;
        //    var keyStr = $"if (name == RName.GetRName(\"{this.AssetName.Name}\", {this.AssetName.RNameType.GetType().FullName.Replace("+", ".")}.{this.AssetName.RNameType.ToString()}))";
        //    var keyIdx = assemblyDescCodes.IndexOf(keyStr, idx);
        //    var tab = "                ";
        //    var str = tab + keyStr + "\r\n";
        //    str += tab + "{\r\n";
        //    str += $"{tab}    return new {DefClass.GetFullName()}();\r\n";
        //    str += tab + "}\r\n";
        //    if (keyIdx < 0)
        //    {
        //        assemblyDescCodes = assemblyDescCodes.Insert(startIdx, str);
        //    }
        //    else
        //    {
        //        keyIdx -= tab.Length;
        //        var endIdx = assemblyDescCodes.IndexOf("}\r\n", keyIdx) + "}\r\n".Length;
        //        assemblyDescCodes = assemblyDescCodes.Remove(keyIdx, endIdx - keyIdx);
        //        assemblyDescCodes = assemblyDescCodes.Insert(keyIdx, str);
        //    }
        //    using(var sw = new System.IO.StreamWriter(assemblyFileName, false, Encoding.UTF8))
        //    {
        //        sw.Write(assemblyDescCodes);
        //    }
        //}
        //public static bool RemoveAssemblyDescCreateInstanceCode(string name, RName.ERNameType type)
        //{
        //    try
        //    {
        //        var projFolder = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource) + IO.TtFileManager.GetParentPathName(TtEngine.Instance.EditorInstance.Config.GameProject);
        //        var assemblyFileName = projFolder + "/Assembly.cs";
        //        string assemblyDescCodes = "";
        //        using (var sr = new System.IO.StreamReader(assemblyFileName, Encoding.UTF8, true))
        //        {
        //            assemblyDescCodes = sr.ReadToEnd();
        //        }
        //        if(string.IsNullOrEmpty(assemblyDescCodes))
        //        {
        //            Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Macross", "Remove macross create instance code failed, Assembly file is empty!");
        //            return false;
        //        }
        //        var startKeyword = "#region MacrossGenerated Start";
        //        var idx = assemblyDescCodes.IndexOf(startKeyword);
        //        var startIdx = idx + startKeyword.Length + 1;
        //        var keyStr = $"if (name == RName.GetRName(\"{name}\", {type.GetType().FullName.Replace("+", ".")}.{type.ToString()}))";
        //        var keyIdx = assemblyDescCodes.IndexOf(keyStr, idx);
        //        var tab = "                ";
        //        if (keyIdx >= 0)
        //        {
        //            keyIdx -= tab.Length;
        //            var endIdx = assemblyDescCodes.IndexOf("}\r\n", keyIdx) + "}\r\n".Length;
        //            assemblyDescCodes = assemblyDescCodes.Remove(keyIdx, endIdx - keyIdx);
        //            using (var sw = new System.IO.StreamWriter(assemblyFileName, false, Encoding.UTF8))
        //            {
        //                sw.Write(assemblyDescCodes);
        //            }
        //            return true;
        //        }
        //    }
        //    catch(System.Exception ex)
        //    {
        //        Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Macross", ex.ToString());
        //        return false;
        //    }
        //    return false;
        //}

        public Action<UMacrossEditor> AfterCompileCode;
        public void CompileCode()
        {
            TtEngine.Instance.MacrossManager.ClearGameProjectTemplateBuildFiles();
            var assemblyFile = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource) + TtEngine.Instance.EditorInstance.Config.GameAssembly;
            if (TtEngine.Instance.MacrossModule.CompileCode(assemblyFile))
            {
                TtEngine.Instance.MacrossModule.ReloadAssembly(assemblyFile);
                var typeDesc = DefClass.TryGetTypeDesc();
                if(typeDesc != null)
                {
                    var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeDesc);
                    meta.BuildMethods();
                    meta.BuildFields();
                    var version = meta.BuildCurrentVersion();
                    version.SaveVersion();
                    meta.SaveClass();

                    for (int i = 0; i < Methods.Count; i++)
                    {
                        Methods[i].CanvasMenuDirty = true;
                    }

                    AfterCompileCode?.Invoke(this);
                }
            }
        }

        void SaveCSFile(string code)
        {
            var fileName = AssetName.Address + "/" + DefClass.ClassName + ".cs";
            using(var sr = new System.IO.StreamWriter(fileName, false, Encoding.UTF8))
            {
                sr.Write(code);
            }
            TtEngine.Instance.SourceControlModule.AddFile(fileName);
        }
        void SaveHlslFile(string code)
        {
            var fileName = AssetName.Address + "/" + DefClass.ClassName + ".shader";
            using (var sr = new System.IO.StreamWriter(fileName, false, Encoding.UTF8))
            {
                sr.Write(code);
            }
            TtEngine.Instance.SourceControlModule.AddFile(fileName);
        }

        List<EGui.UIProxy.MenuItemProxy> mMenuItems = new List<EGui.UIProxy.MenuItemProxy>();
        void InitializeManMenu()
        {
            mMenuItems = new List<EGui.UIProxy.MenuItemProxy>()
            {
                new EGui.UIProxy.MenuItemProxy()
                {
                    MenuName = "File",
                    IsTopMenuItem = true,
                    SubMenus = new List<EGui.UIProxy.IUIProxyBase>()
                    {
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Reload",
                            Action = (item, data)=>
                            {
                                //LoadClassGraph(RName.GetRName("UTest/class_graph.xml"));
                                LoadClassGraph(AssetName);
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Save",
                            Action = (item, data)=>
                            {
                                //SaveClassGraph(RName.GetRName("UTest/class_graph.xml"));
                                SaveClassGraph(AssetName);
                                GenerateCode();
                                CompileCode();
                            },
                        },
                    },
                }
            };
        }

        protected void OnDrawMainMenu()
        {
            if (ImGuiAPI.BeginMenuBar())
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                for (int i = 0; i < mMenuItems.Count; i++)
                    mMenuItems[i].OnDraw(in drawList, in Support.TtAnyPointer.Default);

                ImGuiAPI.EndMenuBar();
            }
        }

        public bool DockInitialized = false;
        protected ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        protected unsafe void ResetDockspace(bool force = false)
        {
            var pos = ImGuiAPI.GetCursorPos();
            var id = ImGuiAPI.GetID(AssetName.Name + "_Dockspace");
            mDockKeyClass.ClassId = id;
            ImGuiAPI.DockSpace(id, Vector2.Zero, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, mDockKeyClass);
            if (DockInitialized && !force)
                return;
            ImGuiAPI.DockBuilderRemoveNode(id);
            ImGuiAPI.DockBuilderAddNode(id, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None);
            ImGuiAPI.DockBuilderSetNodePos(id, pos);
            ImGuiAPI.DockBuilderSetNodeSize(id, Vector2.One);
            DockInitialized = true;

            var graphId = id;
            uint leftId = 0;
            ImGuiAPI.DockBuilderSplitNode(graphId, ImGuiDir.ImGuiDir_Left, 0.2f, ref leftId, ref graphId);
            uint propertyId = 0;
            ImGuiAPI.DockBuilderSplitNode(graphId, ImGuiDir.ImGuiDir_Right, 0.2f, ref propertyId, ref graphId);
            uint unionConfigId = 0;
            ImGuiAPI.DockBuilderSplitNode(graphId, ImGuiDir.ImGuiDir_Right, 0.4f, ref unionConfigId, ref graphId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("GraphWindow", mDockKeyClass), graphId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("CodeEditor", mDockKeyClass), graphId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("NodeProperty", mDockKeyClass), propertyId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("UnionNodeConfig", mDockKeyClass), unionConfigId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("ClassView", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderFinish(id);
        }

        public struct STToolButtonData
        {
            public bool IsMouseDown;
            public bool IsMouseHover;
        }
        STToolButtonData[] mToolBtnDatas = new STToolButtonData[7];
        public Action<ImDrawList> DrawToolbarAction;
        protected void DrawToolbar()
        {
            var drawList = ImGuiAPI.GetWindowDrawList();
            if(DrawToolbarAction != null)
            {
                DrawToolbarAction.Invoke(drawList);
                return;
            }

            int toolBarItemIdx = 0;
            var spacing = EGui.UIProxy.StyleConfig.Instance.ToolbarSeparatorThickness + EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X * 2;
            EGui.UIProxy.Toolbar.BeginToolbar(in drawList);
            if(EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList, 
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "Save"))
            {
                var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName) as UMacrossAMeta;
                if (ameta != null)
                {
                    TtMacross.UpdateAMetaReferences(this, ameta);
                    ameta.Description = $"MacrossType:{ameta.BaseTypeStr}\n";
                    ameta.SaveAMeta((IO.IAsset)null);
                }
                SaveClassGraph(AssetName);
                GenerateCode();
                CompileCode();
            }
            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.TtAnyPointer.Default);
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawCheckBox(in drawList, null, "EditorDebug", ref mCSCodeGen.IsEditorDebug))
            {
                
            }
            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.TtAnyPointer.Default);
            if(EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "GenCode", false, -1, 0, spacing))
            {
                GenerateCode();
                CompileCode();
            }
            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.TtAnyPointer.Default);
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "MethodCustum", false, -1, 0, spacing))
            {
                if (CurrentTextoutMethod != null)
                    CurrentTextoutMethod.CustumCode = mCodeEditor.Text;
            }
            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.TtAnyPointer.Default);
            if(EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "ClassSettings", false, -1, 0, spacing))
            {
                PGMember.Target = DefClass;
            }
            toolBarItemIdx++;
            // test ////////////////
            //if(EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList, in Support.UAnyPointer.Default,
            //    ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "DebugTest"))
            //{
            //    var result = TtEngine.Instance.EventPoster.Post(() =>
            //    {
            //        var tt = TtEngine.Instance.MacrossModule.NewInnerObject<Macross.UMacrossTestClass>(AssetName);
            //        var methodInfo = tt.GetType().GetMethod("Method_0");
            //        //tt.VirtualFunc3(5);
            //        methodInfo?.Invoke(tt, null);

            //        return true;
            //    }, Thread.Async.EAsyncTarget.Logic);
            //}
            ////////////////////////
            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.TtAnyPointer.Default);
            if(Macross.TtMacrossDebugger.Instance.CurrrentBreak != null)
            {
                if(EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                    ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "Run", false, -1, 0, spacing))
                {
                    Macross.TtMacrossDebugger.Instance.Run();
                }
            }
                
            EGui.UIProxy.Toolbar.EndToolbar();
        }

        public string GetWindowsName()
        {
            return string.IsNullOrEmpty(FormName) ? $"Macross:{IO.TtFileManager.GetPureName(AssetName != null ? AssetName.Name : "NoName")}" : FormName;
        }

        public IRootForm RootForm = null;
        public string FormName = null;
        public virtual unsafe void OnDraw()
        {
            //ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            var result = EGui.UIProxy.DockProxy.BeginMainForm(
                GetWindowsName(),
                (RootForm != null) ? RootForm : this,
                ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar);
            if (result)
            {
                DrawToolbar();

                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }

                OnDrawMainMenu(); 
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(result);

            DrawClassView();
            DrawTextEditor();
            DrawGraph();
            DrawPropertyGrid();
            DrawUnionNodeConfig();

            if (IsDraggingMember == true && ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) == false)
            {
                IsDraggingMember = false;
                DraggingMember = null;
            }
            if (IsDraggingLocalVar == true && ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) == false)
            {
                IsDraggingLocalVar = false;
                DraggingLocalVar = null;
            }
        }

        public void RemoveMethod(TtMethodDeclaration methodDesc)
        {
            for(var methodIdx = Methods.Count - 1; methodIdx >= 0; methodIdx--)
            {
                var method = Methods[methodIdx];
                for(int dataIdx = method.MethodDatas.Count - 1; dataIdx >= 0; dataIdx--)
                {
                    if(method.MethodDatas[dataIdx].MethodDec.Equals(methodDesc))
                    {
                        method.MethodDatas.RemoveAt(dataIdx);
                    }
                }
                if(method.MethodDatas.Count == 0)
                {
                    RemoveMethod(method);
                }
            }
        }

        public Func<UMacrossMethodGraph, bool> OnRemoveMethod;
        public void RemoveMethod(UMacrossMethodGraph method)
        {
            if (OnRemoveMethod != null)
            {
                if (!OnRemoveMethod.Invoke(method))
                    return;
            }

            if (AssetName != null)
            {
                var funcFileName = $"{AssetName.Address}/{method.Name}.func";
                IO.TtFileManager.DeleteFile(funcFileName);
                TtEngine.Instance.SourceControlModule.RemoveFile(funcFileName);
            }
            Methods.Remove(method);
            for (int methodIdx = 0; methodIdx < method.MethodDatas.Count; methodIdx++)
                DefClass.RemoveMethod(method.MethodDatas[methodIdx].MethodDec);
            for (int itemIdx = 0; itemIdx < mOverrideMethodMenuItems.Count; itemIdx++)
            {
                for (int dataIdx = 0; dataIdx < method.MethodDatas.Count; dataIdx++)
                {
                    if (mOverrideMethodMenuItems[itemIdx].MenuName == method.MethodDatas[dataIdx].MethodDec.GetKeyword())
                    {
                        mOverrideMethodMenuItems[itemIdx].Visible = true;
                    }
                }
            }

            OpenFunctions.Remove(method);

        }

        public UMacrossMethodGraph AddMethod(TtMethodDeclaration methodDesc)
        {
            var metaAtt = new TtAttribute()
            {
                AttributeType = new TtTypeReference(typeof(Rtti.MetaAttribute)),
            };
            if(!methodDesc.Attributes.Contains(metaAtt))
                methodDesc.Attributes.Add(metaAtt);
            DefClass.AddMethod(methodDesc);

            var func = UMacrossMethodGraph.NewGraph(this, methodDesc);
            Methods.Add(func);
            for (int i = 0; i < OpenFunctions.Count; i++)
            {
                OpenFunctions[i].CanvasMenuDirty = true;
            }
            return func;
        }

        public Func<TtVariableDeclaration, bool> OnAddMember;
        public Func<TtVariableDeclaration, bool> OnRemoveMember;

        public Func<TtVariableDeclaration, UMacrossMethodGraph, bool> OnAddMethodLocalVar;
        public Func<TtVariableDeclaration, UMacrossMethodGraph, bool> OnRemoveMethodLocalVar;

        bool mClassViewShow = true;
        EGui.UIProxy.MenuItemProxy.MenuState mNewMethodMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        EGui.UIProxy.MenuItemProxy.MenuState mOverrideMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        protected unsafe void DrawClassView()
        {
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap;
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "ClassView", ref mClassViewShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                Vector2 buttonSize = new Vector2(16, 16);
                float buttonOffset = 16;
                var sz = new Vector2(-1, 0);
                //ImGuiAPI.SetNextItemWidth(-1);
                var regionSize = ImGuiAPI.GetContentRegionAvail();

                var membersTreeNodeResult = ImGuiAPI.TreeNodeEx("Members", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap);
                ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
                if (EGui.UIProxy.CustomButton.ToolButton("+", in buttonSize, 0xFF00FF00))
                {
                    //ImGuiAPI.OpenPopup("MacrossMemTypeSelPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                    Type selectedType = typeof(int);
                    if(selectedType != null)
                    { 
                        var num = 0;
                        while (true)
                        {
                            bool bFind = false;
                            for (int i = 0; i < DefClass.Properties.Count; i++)
                            {
                                if (DefClass.Properties[i].VariableName == $"Member_{num}")
                                {
                                    num++;
                                    bFind = true;
                                    break;
                                }
                            }
                            if (!bFind)
                                break;
                        }

                        var mb = new TtVariableDeclaration();
                        mb.VariableType = new TtTypeReference(selectedType);
                        mb.VariableName = $"Member_{num}";
                        mb.VisitMode = EVisisMode.Local;
                        mb.InitValue = new TtPrimitiveExpression(Rtti.TtTypeDesc.TypeOf(selectedType), selectedType.IsValueType ? Rtti.TtTypeDescManager.CreateInstance(selectedType) : null);
                        mb.Comment = new TtCommentStatement("");
                        bool result = true;
                        if (OnAddMember != null)
                            result = OnAddMember.Invoke(mb);
                        if(result)
                            DefClass.Properties.Add(mb);
                    }
                }
                if (membersTreeNodeResult)
                {
                    if (DraggingMember != null && IsDraggingMember == false && ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, 10))
                    {
                        IsDraggingMember = true;
                    }
                    var memRegionSize = ImGuiAPI.GetContentRegionAvail();
                    for(int i=0; i<DefClass.Properties.Count; i++)
                    {
                        var mem = DefClass.Properties[i];
                        var memberTreeNodeResult = ImGuiAPI.TreeNodeEx(mem.DisplayName, flags);
                        var memberTreeNodeClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
                        ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
                        if(EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "mem_X_" + i))
                        {
                            // todo: 引用删除警告
                            bool result = true;
                            if (OnRemoveMember != null)
                                result = OnRemoveMember.Invoke(mem);
                            if(result)
                                DefClass.Properties.Remove(mem);
                            break;
                        }
                        if (memberTreeNodeResult)
                        {
                            if (memberTreeNodeClicked)
                            {
                                PGMember.Target = mem;
                                DraggingMember = MemberVar.NewMemberVar(DefClass, mem.VariableName,
                                    TtEngine.Instance.InputSystem.IsKeyDown(Input.Keycode.KEY_LCTRL) ? false : true);
                                DraggingMember.UserData = this;
                                IsDraggingMember = false;
                            }
                        }
                    }
                    ImGuiAPI.TreePop();
                }
                var methodsTreeNodeResult = ImGuiAPI.TreeNodeEx("Methods", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap);
                ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
                if(EGui.UIProxy.CustomButton.ToolButton("+", in buttonSize, 0xFF00FF00))
                {
                    ImGuiAPI.OpenPopup("MacrossMethodSelectPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                }
                if(ImGuiAPI.BeginPopup("MacrossMethodSelectPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var drawList = ImGuiAPI.GetWindowDrawList();
                    var menuData = new Support.TtAnyPointer();
                    if(EGui.UIProxy.MenuItemProxy.MenuItem("New Method", null, false, null, in drawList, in menuData, ref mNewMethodMenuState))
                    {
                        var num = 0;
                        while(true)
                        {
                            bool bFind = false;
                            for (int i = 0; i < DefClass.Methods.Count; i++)
                            {
                                if (DefClass.Methods[i].MethodName == $"Method_{num}")
                                {
                                    num++;
                                    bFind = true;
                                    break;
                                }
                            }
                            if (!bFind)
                                break;
                        }

                        var f = new TtMethodDeclaration()
                        {
                            MethodName = $"Method_{num}",
                        };
                        AddMethod(f);
                    }
                    if (EGui.UIProxy.MenuItemProxy.BeginMenuItem("Override Method", null, null, in drawList, in menuData, ref mOverrideMenuState))
                    {
                        for(int i=0; i < mOverrideMethodMenuItems.Count; i++)
                        {
                            mOverrideMethodMenuItems[i].OnDraw(in drawList, in menuData);
                        }
                        EGui.UIProxy.MenuItemProxy.EndMenuItem();
                    }

                    ImGuiAPI.EndPopup();
                }
                if (methodsTreeNodeResult)
                {
                    var funcRegionSize = ImGuiAPI.GetContentRegionAvail();
                    for(int i=Methods.Count - 1; i>=0; i--)
                    {
                        var method = Methods[i];
                        if (method.IsDelegateGraph())
                            continue;
                        var displayName = method.DisplayName;

                        if (method.IsUseCustumCode)
                        {
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, 0xFF0000FF);
                        }
                        var methodTreeNodeResult = ImGuiAPI.TreeNodeEx(displayName, flags);
                        ImGuiAPI.SameLine(0, EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X);
                        if (method.IsUseCustumCode)
                        {
                            ImGuiAPI.PopStyleColor(1);
                        }
                        var methodTreeNodeDoubleClicked = ImGuiAPI.IsItemDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
                        var methodTreeNodeIsItemClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
                        ImGuiAPI.SameLine(regionSize.X - buttonSize.X * 3 - buttonOffset, -1.0f);
                        if (EGui.UIProxy.CustomButton.ToolButton("g", in buttonSize, 0xFF00FF00, "func_G_" + i))
                        {
                            mCodeEditor.mCoreObject.SetText(GenerateMethodCode(this.CodeGen, i));
                            TextEditorTitle = "Gen:" + method.DisplayName;
                            CurrentTextoutMethod = method;
                            break;
                        }
                        ImGuiAPI.SameLine(regionSize.X - buttonSize.X * 2 - buttonOffset, -1.0f);
                        if (EGui.UIProxy.CustomButton.ToolButton("c", in buttonSize, 0xFF00FF00, "func_C_" + i))
                        {
                            if (Methods[i].CustumCode != null)
                                mCodeEditor.mCoreObject.SetText(Methods[i].CustumCode);
                            else
                                mCodeEditor.mCoreObject.SetText("//No CustumCode");
                            TextEditorTitle = "Custum:" + method.DisplayName;
                            CurrentTextoutMethod = method;
                            break;
                        }
                        ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
                        var keyName = $"Delete func {displayName}?";
                        if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "func_X_" + i))
                        {
                            EGui.UIProxy.MessageBox.Open(keyName);
                            break;
                        }
                        EGui.UIProxy.MessageBox.Draw(keyName, $"Are you sure to delete {displayName}?", EGui.UIProxy.MessageBox.EButtonType.YesNo,
                        () =>
                        {
                            RemoveMethod(method);
                        }, null);

                        if (methodTreeNodeResult)
                        {
                            if (methodTreeNodeDoubleClicked)
                            {
                                OpenMethodGraph(method);
                            }
                            else if (methodTreeNodeIsItemClicked)
                            {
                                PGMember.Target = method;
                            }
                        }
                    }


                    ImGuiAPI.TreePop();
                }

                if (CurrentOpenMethod != null)
                {
                    DrawMethodLocalVars(in regionSize, in buttonSize, buttonOffset, flags);
                }
                
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        private unsafe void DrawMethodLocalVars(in Vector2 regionSize, in Vector2 buttonSize, float buttonOffset, ImGuiTreeNodeFlags_ flags)
        {
            var localVarsTreeNodeResult = ImGuiAPI.TreeNodeEx("LocalVars", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap);
            ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
            if (EGui.UIProxy.CustomButton.ToolButton("+", in buttonSize, 0xFF00FF00))
            {
                Type selectedType = typeof(int);
                if (selectedType != null)
                {
                    var num = 0;
                    while (true)
                    {
                        bool bFind = false;
                        for (int i = 0; i < CurrentOpenMethod.LocalVars.Count; i++)
                        {
                            if (CurrentOpenMethod.LocalVars[i].VariableName == $"Local_{num}")
                            {
                                num++;
                                bFind = true;
                                break;
                            }
                        }
                        if (!bFind)
                            break;
                    }

                    var mb = new TtVariableDeclaration();
                    mb.VariableType = new TtTypeReference(selectedType);
                    mb.VariableName = $"Local_{num}";
                    mb.VisitMode = EVisisMode.Local;
                    mb.InitValue = new TtPrimitiveExpression(Rtti.TtTypeDesc.TypeOf(selectedType), selectedType.IsValueType ? Rtti.TtTypeDescManager.CreateInstance(selectedType) : null);
                    mb.Comment = new TtCommentStatement("");
                    bool result = true;
                    if (OnAddMethodLocalVar != null)
                        result = OnAddMethodLocalVar.Invoke(mb, CurrentOpenMethod);
                    if (result)
                        CurrentOpenMethod.LocalVars.Add(mb);
                }
            }

            if (localVarsTreeNodeResult)
            {
                if (DraggingLocalVar != null && IsDraggingLocalVar == false && ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, 10))
                {
                    IsDraggingLocalVar = true;
                }
                var memRegionSize = ImGuiAPI.GetContentRegionAvail();
                for (int i = 0; i < CurrentOpenMethod.LocalVars.Count; i++)
                {
                    var mem = CurrentOpenMethod.LocalVars[i];
                    var memberTreeNodeResult = ImGuiAPI.TreeNodeEx(mem.DisplayName, flags);
                    var memberTreeNodeClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
                    ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
                    if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "mem_X_" + i))
                    {
                        // todo: 引用删除警告
                        bool result = true;
                        if (OnRemoveMethodLocalVar != null)
                            result = OnRemoveMethodLocalVar.Invoke(mem, CurrentOpenMethod);
                        if (result)
                            CurrentOpenMethod.LocalVars.Remove(mem);
                        break;
                    }
                    if (memberTreeNodeResult)
                    {
                        if (memberTreeNodeClicked)
                        {
                            PGMember.Target = mem;
                            DraggingLocalVar = MethodLocalVar.NewMethodLocalVar(CurrentOpenMethod, mem.VariableName,
                                TtEngine.Instance.InputSystem.IsKeyDown(Input.Keycode.KEY_LCTRL) ? false : true);
                            DraggingLocalVar.UserData = this;
                            IsDraggingLocalVar = false;
                        }
                    }
                }
                ImGuiAPI.TreePop();
            }
        }
        public void OpenMethodGraph(UMacrossMethodGraph method)
        {
            mSettingCurrentFuncIndex = OpenFunctions.IndexOf(method);
            if (mSettingCurrentFuncIndex < 0)
            {
                method.VisibleInClassGraphTables = true;
                method.GraphRenderer.SetGraph(method);
                mSettingCurrentFuncIndex = OpenFunctions.Count;
                OpenFunctions.Add(method);
            }
            UMainEditorApplication.NeedFocusPanelName = EGui.UIProxy.DockProxy.GetDockWindowName("GraphWindow", mDockKeyClass);
        }
        bool mNodePropertyShow = true;
        protected void DrawPropertyGrid()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "NodeProperty", ref mNodePropertyShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                PGMember.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        NodeGraph.UnionNodeConfigRenderer mUnionNodeConfigRenderer = new NodeGraph.UnionNodeConfigRenderer();
        bool mUnionNodeConfigShow = false;
        protected void DrawUnionNodeConfig()
        {
            if (!mUnionNodeConfigShow)
                return;
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "UnionNodeConfig", ref mUnionNodeConfigShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                mUnionNodeConfigRenderer.DrawConfigPanel();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        public void SetConfigUnionNode(NodeGraph.IUnionNode node)
        {
            mUnionNodeConfigRenderer?.SetUnionNode(node);
            mUnionNodeConfigShow = (node != null);
        }
        EGui.TtCodeEditor mCodeEditor = new EGui.TtCodeEditor();
        bool ShowTextEditor = true;
        string TextEditorTitle = "TextEditor";
        UMacrossMethodGraph CurrentTextoutMethod = null;
        protected void DrawTextEditor()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "CodeEditor", ref ShowTextEditor, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                var winPos = ImGuiAPI.GetWindowPos();
                var vpMin = ImGuiAPI.GetWindowContentRegionMin();
                var vpMax = ImGuiAPI.GetWindowContentRegionMax();
                ImGuiAPI.TextColored(Color4f.FromColor4b(Color4b.LightGoldenrodYellow), TextEditorTitle);
                mCodeEditor.mCoreObject.Render(AssetName.Name, in Vector2.Zero, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        Macross.TtMacrossBreak mBreakerStore = null;
        bool mGraphWindowShow = true;
        int mSettingCurrentFuncIndex = -1;
        UMacrossMethodGraph CurrentOpenMethod = null;
        protected unsafe void DrawGraph()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "GraphWindow", ref mGraphWindowShow, ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollWithMouse);
            if (show)
            {
                var vMin = ImGuiAPI.GetWindowContentRegionMin();
                var vMax = ImGuiAPI.GetWindowContentRegionMax();
                CurrentOpenMethod = null;
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_WindowBg, EGui.UIProxy.StyleConfig.Instance.SecondPanelBackground);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, EGui.UIProxy.StyleConfig.Instance.SecondPanelBackground);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_TableHeaderBg, EGui.UIProxy.StyleConfig.Instance.SecondPanelBackground);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_TabActive, EGui.UIProxy.StyleConfig.Instance.SecondPanelBackground);
                if (ImGuiAPI.BeginTabBar("OpenFuncTab", ImGuiTabBarFlags_.ImGuiTabBarFlags_DrawSelectedOverline | ImGuiTabBarFlags_.ImGuiTabBarFlags_Reorderable))
                {
                    var itMax = ImGuiAPI.GetItemRectSize();
                    vMin.Y += itMax.Y;
                    var sz = vMax - vMin;
                    bool breakerChanged = false;
                    if (mBreakerStore != Macross.TtMacrossDebugger.Instance.CurrrentBreak)
                    {
                        mBreakerStore = Macross.TtMacrossDebugger.Instance.CurrrentBreak;
                        breakerChanged = true;
                    }
                    
                    for (int i = 0; i < OpenFunctions.Count; i++)
                    {
                        var func = OpenFunctions[i];
                        if(breakerChanged)
                        {
                            for(int linkerIdx=0; linkerIdx < func.Linkers.Count; linkerIdx++)
                            {
                                func.Linkers[linkerIdx].InDebuggerLine = false;
                            }
                            if (mBreakerStore != null)
                                func.GraphRenderer.BreakerName = mBreakerStore.BreakName;
                            else
                                func.GraphRenderer.BreakerName = "";
                        }

                        var flag = ImGuiTabItemFlags_.ImGuiTabItemFlags_None;
                        if(mSettingCurrentFuncIndex == i)
                        {
                            flag |= ImGuiTabItemFlags_.ImGuiTabItemFlags_SetSelected;
                            mSettingCurrentFuncIndex = -1;
                        }
                        if (ImGuiAPI.BeginTabItem(func.DisplayName, ref func.VisibleInClassGraphTables, flag))
                        {
                            DrawFunctionGraph(func, sz);
                            CurrentOpenMethod = func;

                            ImGuiAPI.EndTabItem();
                        }
                    }
                    for (int i = 0; i < OpenFunctions.Count; i++)
                    {
                        if (OpenFunctions[i].VisibleInClassGraphTables == false)
                        {
                            OpenFunctions.RemoveAt(i);
                            i--;
                        }
                    }
                    ImGuiAPI.EndTabBar();
                }
                ImGuiAPI.PopStyleColor(4);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        public void DrawFunctionGraph(UMacrossMethodGraph func, Vector2 size)
        {
            if (ImGuiAPI.BeginChild("Function", in size, ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar))
            {
                ((UMacrossMethodGraph)(func.GraphRenderer.Graph)).UpdateSelectPG();
                func.GraphRenderer.OnDraw();                
                //func.OnDraw(null, false);
            }
            ImGuiAPI.EndChild();
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async Thread.Async.TtTask<bool> OpenEditor(Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            LoadClassGraph(AssetName);
            //LoadClassGraph(RName.GetRName("UTest/class_graph.xml"));
            await Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }

        public void OnCloseEditor()
        {

        }

        public void OnEvent(in Bricks.Input.Event e)
        {

        }
    }
}

namespace EngineNS.UTest
{
    [UTest]
    public class UTest_ClassGraph
    {
        public static UTest_ClassGraph Instance = new UTest_ClassGraph();
        public Bricks.CodeBuilder.MacrossNode.UMacrossEditor mClassGraph = new Bricks.CodeBuilder.MacrossNode.UMacrossEditor();
        public void UT_Draw()
        {
            mClassGraph.OnDraw();
        }
        public void UnitTestEntrance()
        {

        }
    }
}

namespace EngineNS.Macross
{
    public partial class UMacrossModule
    {
        public bool CompileCode(string assemblyFile)
        {
            bool success = false;
            TryCompileCode(assemblyFile, ref success);
            return success;
        }
        partial void TryCompileCode(string assemblyFile, ref bool success)
        {
            var csFilesPath = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game);
            var projectFile = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource) + TtEngine.Instance.EditorInstance.Config.GameProject;
            success = CompileGameProject(csFilesPath, projectFile, assemblyFile);
        }
        public static bool CompileGameProject(string csFilesPath, string projectFile, string assemblyFile)
        {
            csFilesPath = EngineNS.IO.TtFileManager.GetValidDirectory(csFilesPath);
            var csFiles = new List<string>(EngineNS.IO.TtFileManager.GetFiles(csFilesPath, "*.cs"));
            List<string> arguments = new List<string>();
            for (int i = 0; i < csFiles.Count; ++i)
                arguments.Add(EngineNS.CodeCompiler.CSharpCompiler.GetCommandArguments(EngineNS.CodeCompiler.CSharpCompiler.enCommandType.CSFile, csFiles[i]));

            var createInstanceCode = $@"
namespace EngineNS.Rtti
{{
    public partial class AssemblyEntry
    {{
        public partial class GameAssemblyDesc
        {{
            public override object CreateInstance(EngineNS.RName name)
            {{
                var nameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(name.ToString());
                switch(nameHash)
                {{";
            foreach (var csFile in csFiles)
            {
                var relativeFile = EngineNS.IO.TtFileManager.GetValidFileName(csFile).Replace(csFilesPath, "");
                var name = EngineNS.IO.TtFileManager.GetBaseDirectory(relativeFile, 1).TrimEnd('/').ToLower();
                var relativePath = EngineNS.IO.TtFileManager.GetBaseDirectory(relativeFile, 2).TrimEnd('/').ToLower();
                var fileName = EngineNS.IO.TtFileManager.GetPureName(relativeFile).ToLower(); 
                var hashCode = Standart.Hash.xxHash.xxHash64.ComputeHash($"{name}:{EngineNS.RName.ERNameType.Game}");
                createInstanceCode += $@"
                case {hashCode}:
                    return new NS_{relativePath.Replace("/", ".")}.{fileName}();";
            }
            createInstanceCode += $@"
                }}
                return null;
            }}
        }}
    }}
}}";
            var projectPath = EngineNS.IO.TtFileManager.GetBaseDirectory(projectFile, 1);
            var objDir = projectPath + "obj";
            if(!EngineNS.IO.TtFileManager.DirectoryExists(objDir))
            {
                EngineNS.IO.TtFileManager.CreateDirectory(objDir);
            }
            var genCodeFileName = objDir + "/_gencode.cs";
            using (var extCodeWriter = new StreamWriter(genCodeFileName, false, System.Text.Encoding.UTF8))
            {
                extCodeWriter.Write(createInstanceCode);
            }
            arguments.Add(EngineNS.CodeCompiler.CSharpCompiler.GetCommandArguments(EngineNS.CodeCompiler.CSharpCompiler.enCommandType.CSFile, genCodeFileName));

            var projCSFiles = EngineNS.IO.TtFileManager.GetFiles(projectPath, "*.cs");
            foreach (var csFile in projCSFiles)
            {
                if (csFile.Contains(projectPath + "obj"))
                    continue;
                arguments.Add(EngineNS.CodeCompiler.CSharpCompiler.GetCommandArguments(EngineNS.CodeCompiler.CSharpCompiler.enCommandType.CSFile, csFile));
            }

            var projDef = XDocument.Load(projectFile);
            if (projDef != null)
            {
                var references = projDef.Element("Project").Elements("ItemGroup").Elements("Reference").Select(refElem => refElem.Value);
                foreach (var reference in references)
                {
                    arguments.Add(EngineNS.CodeCompiler.CSharpCompiler.GetCommandArguments(EngineNS.CodeCompiler.CSharpCompiler.enCommandType.RefAssemblyFile, projectPath + reference));
                }
            }

            //var references = projDef.Element(projDef.n) 
            arguments.Add(EngineNS.CodeCompiler.CSharpCompiler.GetCommandArguments(EngineNS.CodeCompiler.CSharpCompiler.enCommandType.OutputFile, assemblyFile));
            arguments.Add(EngineNS.CodeCompiler.CSharpCompiler.GetCommandArguments(EngineNS.CodeCompiler.CSharpCompiler.enCommandType.PdbFile, assemblyFile.Replace(".dll", ".tpdb")));
            arguments.Add(EngineNS.CodeCompiler.CSharpCompiler.GetCommandArguments(EngineNS.CodeCompiler.CSharpCompiler.enCommandType.Outputkind, Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary.ToString()));
            arguments.Add(EngineNS.CodeCompiler.CSharpCompiler.GetCommandArguments(EngineNS.CodeCompiler.CSharpCompiler.enCommandType.OptimizationLevel, Microsoft.CodeAnalysis.OptimizationLevel.Debug.ToString()));
            arguments.Add(EngineNS.CodeCompiler.CSharpCompiler.GetCommandArguments(EngineNS.CodeCompiler.CSharpCompiler.enCommandType.AllowUnsafe, "true"));

            var retVal = EngineNS.CodeCompiler.CSharpCompiler.CompilerCSharpWithArguments(arguments.ToArray());
            return retVal;
        }
    }
}