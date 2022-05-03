using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public partial class UMacrossEditor : IO.ISerializer, Editor.IAssetEditor, Graphics.Pipeline.IRootForm
    {
        public UMacrossEditor()
        {
            mNewMethodMenuState.Reset();
            mOverrideMenuState.Reset();
        }
        public void Cleanup()
        {

        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            InitializeManMenu();
            await PGMember.Initialize();
            InitializeToolbar();
            return true;
        }

        List<EGui.UIProxy.MenuItemProxy> mOverrideMethodMenuItems = new List<EGui.UIProxy.MenuItemProxy>();
        void InitializeOverrideMethodList()
        {
            for(int i=0; i<DefClass.SupperClassNames.Count; i++)
            {
                var superClass = Rtti.UClassMetaManager.Instance.GetMetaFromFullName(DefClass.SupperClassNames[i]);
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
                    var keyWord = UMethodDeclaration.GetKeyword(method);
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
                        var f = UMethodDeclaration.GetMethodDeclaration(method);
                        DefClass.AddMethod(f);

                        var func = UMacrossMethodGraph.NewGraph(this, f);
                        Methods.Add(func);
                        menuItem.Visible = false;
                    };

                    mOverrideMethodMenuItems.Add(menuItem);
                }
            }
        }

        EGui.UIProxy.Toolbar mToolbar = new EGui.UIProxy.Toolbar();
        void InitializeToolbar()
        {
            mToolbar.AddToolbarItems(
                new EGui.UIProxy.ToolbarIconButtonProxy()
                {
                    Name = "Save",
                    Action = () =>
                    {
                        //SaveClassGraph(RName.GetRName("UTest/class_graph.xml"));
                        SaveClassGraph(AssetName);
                        GenerateCode();
                        CompileCode();
                    }
                },
                new EGui.UIProxy.ToolbarSeparator(),
                new EGui.UIProxy.ToolbarIconButtonProxy()
                {
                    Name = "GenCode",
                    Action = () =>
                    {
                        GenerateCode();
                        CompileCode();
                    }
                },
                new EGui.UIProxy.ToolbarSeparator(),
                new EGui.UIProxy.ToolbarIconButtonProxy()
                {
                    Name = "ClassSettings",
                    Action = ()=>
                    {
                        PGMember.Target = DefClass;
                    }
                }
            );
        }
        public Graphics.Pipeline.IRootForm GetRootForm()
        {
            return this;
        }
        public void OnPreRead(object tagObject, object hostObject, bool fromXml) { }
        public void OnPropertyRead(object root, System.Reflection.PropertyInfo prop, bool fromXml) { }
        [Rtti.Meta(Order = 0)]
        public UClassDeclaration DefClass { get; } = new UClassDeclaration();
        //public DefineClass DefClass { get; } = new DefineClass();
        //[Rtti.Meta(Order = 1)]
        public List<UMacrossMethodGraph> Methods { get; } = new List<UMacrossMethodGraph>();
        public RName AssetName { get; set; }
        bool mVisible = true;
        public bool Visible 
        {
            get => mVisible;
            set => mVisible = value;
        }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public EGui.Controls.PropertyGrid.PropertyGrid PGMember = new EGui.Controls.PropertyGrid.PropertyGrid();
        public List<UMacrossMethodGraph> OpenFunctions = new List<UMacrossMethodGraph>();
        public MemberVar DraggingMember;
        public bool IsDraggingMember = false;
        public float LeftWidth = 300;
        bool bFirstDraw = true;
        UCSharpCodeGenerator mCSCodeGen = new UCSharpCodeGenerator();
        public UCSharpCodeGenerator CSCodeGen => mCSCodeGen;


        public void SaveClassGraph(RName rn)
        {
            var xml = new System.Xml.XmlDocument();
            var xmlRoot = xml.CreateElement($"Root", xml.NamespaceURI);
            xml.AppendChild(xmlRoot);
            IO.SerializerHelper.WriteObjectMetaFields(xml, xmlRoot, this);
            var xmlText = IO.FileManager.GetXmlText(xml);
            IO.FileManager.WriteAllText($"{rn.Address}/class_graph.dat", xmlText);

            for(int i=0; i<Methods.Count; i++)
            {
                var funcXml = new System.Xml.XmlDocument();
                var funcXmlRoot = funcXml.CreateElement($"Root", funcXml.NamespaceURI);
                funcXml.AppendChild(funcXmlRoot);
                IO.SerializerHelper.WriteObjectMetaFields(funcXml, funcXmlRoot, Methods[i]);
                var funcXmlText = IO.FileManager.GetXmlText(funcXml);
                IO.FileManager.WriteAllText($"{rn.Address}/{Methods[i].Name}.func", funcXmlText);
            }

            //LoadClassGraph(rn);
        }
        public void LoadClassGraph(RName rn)
        {
            DefClass.Reset();
            Methods.Clear();
            OpenFunctions.Clear();
            PGMember.Target = null;

            {
                // old
                //var xml = IO.FileManager.LoadXml(rn.Address);
                //object pThis = this;
                //IO.SerializerHelper.ReadObjectMetaFields(this, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
            }
            {
                var xml = IO.FileManager.LoadXml($"{rn.Address}/class_graph.dat");
                object pThis = this;
                IO.SerializerHelper.ReadObjectMetaFields(this, xml.LastChild as System.Xml.XmlElement, ref pThis, null);

                DefClass.ClearMethods();
                var funcFiles = IO.FileManager.GetFiles(rn.Address, "*.func", false);
                for (int i = 0; i < funcFiles.Length; i++)
                {
                    try
                    {
                        var funcXml = IO.FileManager.LoadXml(funcFiles[i]);
                        var funcGraph = UMacrossMethodGraph.NewGraph(this);
                        object pFuncGraph = funcGraph;
                        IO.SerializerHelper.ReadObjectMetaFields(null, funcXml.LastChild as System.Xml.XmlElement, ref pFuncGraph, null);
                        for(int methodIdx = 0; methodIdx < funcGraph.MethodDatas.Count; methodIdx++)
                        {
                            DefClass.AddMethod(funcGraph.MethodDatas[methodIdx].MethodDec);
                        }
                        for (var nodeIdx = 0; nodeIdx < funcGraph.Nodes.Count; nodeIdx++)
                        {
                            var node = funcGraph.Nodes[nodeIdx];
                            node.UserData = this;
                            funcGraph.SetDefaultActionForNode(node);
                            var methodNode = node as MethodNode;
                            if (methodNode == null)
                                continue;

                            for (int methodIdx = 0; methodIdx < DefClass.Methods.Count; methodIdx++)
                            {
                                if (methodNode.MethodMeta == MethodNode.GetMethodMeta(DefClass.Methods[methodIdx]))
                                {
                                    methodNode.MethodDesc = DefClass.Methods[methodIdx];
                                }
                            }
                        }
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
                }
            }

            InitializeOverrideMethodList();
        }
        public string GenerateCode()
        {
            try
            {
                foreach (var i in Methods)
                {
                    i.BuildExpression(DefClass);
                }

                string code = "";
                mCSCodeGen.GenerateClassCode(DefClass, ref code);

                SaveCSFile(code);

                EngineNS.UEngine.Instance.MacrossManager.GenerateProjects();
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

        public void CompileCode()
        {
            var csFiles = new List<string>(IO.FileManager.GetFiles(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Game), "*.cs"));
            var projectPath = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Root) + UEngine.Instance.EditorInstance.Config.GameProjectPath;
            csFiles.AddRange(IO.FileManager.GetFiles(projectPath, "*.cs"));
            List<string> arguments = new List<string>();
            for (int i=0; i<csFiles.Count; ++i)
                arguments.Add(CodeCompiler.CSharpCompiler.GetCommandArguments(CodeCompiler.CSharpCompiler.enCommandType.CSFile, csFiles[i]));

            var projectFile = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Root) + UEngine.Instance.EditorInstance.Config.GameProject;
            var projDef = XDocument.Load(projectFile);
            var references = projDef.Element("Project").Elements("ItemGroup").Elements("Reference").Select(refElem => refElem.Value);
            foreach(var reference in references)
            {
                arguments.Add(CodeCompiler.CSharpCompiler.GetCommandArguments(CodeCompiler.CSharpCompiler.enCommandType.RefAssemblyFile, projectPath + reference));
            }
            //var references = projDef.Element(projDef.n) 

            var assemblyFile = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Root) + UEngine.Instance.EditorInstance.Config.GameAssembly;
            arguments.Add(CodeCompiler.CSharpCompiler.GetCommandArguments(CodeCompiler.CSharpCompiler.enCommandType.OutputFile, assemblyFile));
            arguments.Add(CodeCompiler.CSharpCompiler.GetCommandArguments(CodeCompiler.CSharpCompiler.enCommandType.PdbFile, assemblyFile.Replace(".dll", ".pdb")));
            arguments.Add(CodeCompiler.CSharpCompiler.GetCommandArguments(CodeCompiler.CSharpCompiler.enCommandType.Outputkind, OutputKind.DynamicallyLinkedLibrary.ToString()));
            arguments.Add(CodeCompiler.CSharpCompiler.GetCommandArguments(CodeCompiler.CSharpCompiler.enCommandType.OptimizationLevel, OptimizationLevel.Debug.ToString()));

            if(CodeCompiler.CSharpCompiler.CompilerCSharpWithArguments(arguments.ToArray()))
            {
                UEngine.Instance.MacrossModule.ReloadAssembly(assemblyFile);
                var typeDesc = DefClass.TryGetTypeDesc();
                var meta = Rtti.UClassMetaManager.Instance.GetMeta(typeDesc);
                meta.BuildMethods();
                meta.BuildFields();
                var version = meta.BuildCurrentVersion();
                version.SaveVersion();
                meta.SaveClass();

                for (int i = 0; i < Methods.Count; i++)
                {
                    Methods[i].CanvasMenuDirty = true;
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

        void OnDrawMainMenu()
        {
            if (ImGuiAPI.BeginMenuBar())
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                for (int i = 0; i < mMenuItems.Count; i++)
                    mMenuItems[i].OnDraw(in drawList, in Support.UAnyPointer.Default);

                ImGuiAPI.EndMenuBar();
            }
        }
        public unsafe void OnDraw()
        {
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (ImGuiAPI.Begin($"Macross:{IO.FileManager.GetPureName(AssetName!=null? AssetName.Name :"NoName")}", ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None| ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar))
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                mToolbar.OnDraw(in drawList, in Support.UAnyPointer.Default);

                if (ImGuiAPI.IsWindowDocked())
                {
                    DockId = ImGuiAPI.GetWindowDockID();
                }
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.MainWindow as Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }

                OnDrawMainMenu();

                ImGuiAPI.Columns(2, null, true);
                if (bFirstDraw)
                {
                    ImGuiAPI.SetColumnWidth(0, LeftWidth);
                    bFirstDraw = false;
                }
                var curPos = ImGuiAPI.GetCursorScreenPos();
                LeftWidth = ImGuiAPI.GetColumnWidth(0);
                var szLeft = new Vector2(LeftWidth, 0);
                if (ImGuiAPI.BeginChild("LeftWin", in szLeft, true, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
                {
                    OnLeftWindow();
                }
                ImGuiAPI.EndChild();
                ImGuiAPI.NextColumn();

                var colWidth = ImGuiAPI.GetColumnWidth(1);
                var szRight = new Vector2(colWidth, 0);
                if (ImGuiAPI.BeginChild("RightWin", in szRight, true, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
                {
                    OnRightWindow();
                }
                ImGuiAPI.EndChild();

                ImGuiAPI.Columns(1, null, true);
                ImGuiAPI.NextColumn();
            }
            ImGuiAPI.End();

            if (IsDraggingMember == true && ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) == false)
            {
                IsDraggingMember = false;
                DraggingMember = null;
            }
        }
        EGui.UIProxy.MenuItemProxy.MenuState mNewMethodMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        EGui.UIProxy.MenuItemProxy.MenuState mOverrideMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        protected unsafe void OnLeftWindow()
        {
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;// | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap;
            if (ImGuiAPI.CollapsingHeader("ClassView", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
            {
                Vector2 buttonSize = new Vector2(16, 16);
                float buttonOffset = 16;
                var sz = new Vector2(-1, 0);
                //ImGuiAPI.SetNextItemWidth(-1);
                var regionSize = ImGuiAPI.GetContentRegionAvail();

                var membersTreeNodeResult = ImGuiAPI.TreeNodeEx("Members", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap);
                ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
                if(EGui.UIProxy.CustomButton.ToolButton("+", in buttonSize, 0xFF00FF00))
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

                    var mb = new UVariableDeclaration();
                    mb.VariableType = new UTypeReference(typeof(int));
                    mb.VariableName = $"Member_{num}";
                    mb.VisitMode = EVisisMode.Local;
                    DefClass.Properties.Add(mb);
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
                        var memberTreeNodeResult = ImGuiAPI.TreeNodeEx(mem.VariableName, flags);
                        var memberTreeNodeClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
                        ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
                        if(EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "mem_X_" + i))
                        {
                            // todo: 引用删除警告
                            DefClass.Properties.Remove(mem);
                            break;
                        }
                        if (memberTreeNodeResult)
                        {
                            if (memberTreeNodeClicked)
                            {
                                PGMember.Target = mem;
                                DraggingMember = MemberVar.NewMemberVar(DefClass, mem.VariableName);
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
                    var menuData = new Support.UAnyPointer();
                    if(EGui.UIProxy.MenuItemProxy.MenuItem("New Method", null, false, null, ref drawList, ref menuData, ref mNewMethodMenuState))
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

                        var f = new UMethodDeclaration()
                        {
                            MethodName = $"Method_{num}",
                        };
                        DefClass.AddMethod(f);

                        var func = UMacrossMethodGraph.NewGraph(this, f);
                        Methods.Add(func);
                        for(int i=0; i<OpenFunctions.Count; i++)
                        {
                            OpenFunctions[i].CanvasMenuDirty = true;
                        }
                    }
                    if (EGui.UIProxy.MenuItemProxy.BeginMenuItem("Override Method", null, null, ref drawList, ref menuData, ref mOverrideMenuState))
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
                        var methodTreeNodeResult = ImGuiAPI.TreeNodeEx(method.Name, flags);
                        ImGuiAPI.SameLine(0, EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X);
                        var methodTreeNodeDoubleClicked = ImGuiAPI.IsItemDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
                        var methodTreeNodeIsItemClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
                        ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
                        var keyName = $"Delete func {method.Name}?";
                        if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "func_X_" + i))
                        {
                            EGui.UIProxy.MessageBox.Open(keyName);
                            break;
                        }
                        EGui.UIProxy.MessageBox.Draw(keyName, $"Are you sure to delete {method.Name}?", EGui.UIProxy.MessageBox.EButtonType.YesNo,
                        () =>
                        {
                            if (AssetName != null)
                                IO.FileManager.DeleteFile($"{AssetName.Address}/{method.Name}.func");
                            Methods.Remove(method);
                            for(int methodIdx = 0; methodIdx < method.MethodDatas.Count; methodIdx++)
                                DefClass.RemoveMethod(method.MethodDatas[methodIdx].MethodDec);
                            for(int itemIdx = 0; itemIdx < mOverrideMethodMenuItems.Count; itemIdx++)
                            {
                                for(int dataIdx = 0; dataIdx < method.MethodDatas.Count; dataIdx++)
                                {
                                    if(mOverrideMethodMenuItems[itemIdx].MenuName == method.MethodDatas[dataIdx].MethodDec.GetKeyword())
                                    {
                                        mOverrideMethodMenuItems[itemIdx].Visible = true;
                                    }
                                }
                            }

                            OpenFunctions.Remove(method);
                        }, null);

                        if (methodTreeNodeResult)
                        {
                            if (methodTreeNodeDoubleClicked)
                            {
                                if (OpenFunctions.Contains(method) == false)
                                {
                                    method.VisibleInClassGraphTables = true;
                                    method.GraphRenderer.SetGraph(method);
                                    OpenFunctions.Add(method);
                                }
                            }
                            else if (methodTreeNodeIsItemClicked)
                            {
                                PGMember.Target = method;
                            }
                        }
                    }


                    ImGuiAPI.TreePop();
                }
            }
            if (ImGuiAPI.CollapsingHeader("Property", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
            {
                PGMember.OnDraw(true, false, false);
            }
        }
        protected unsafe void OnRightWindow()
        {
            var vMin = ImGuiAPI.GetWindowContentRegionMin();
            var vMax = ImGuiAPI.GetWindowContentRegionMin();
            if (ImGuiAPI.BeginTabBar("OpenFuncTab", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
            {
                var itMax = ImGuiAPI.GetItemRectSize();
                vMin.Y += itMax.Y;
                var sz = vMax - vMin;
                foreach (var i in OpenFunctions)
                {
                    if (ImGuiAPI.BeginTabItem(i.Name, ref i.VisibleInClassGraphTables, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                    {
                        DrawFunctionGraph(i, sz);

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
        }

        public void DrawFunctionGraph(UMacrossMethodGraph func, Vector2 size)
        {
            if (ImGuiAPI.BeginChild("Function", in size, true, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar))
            {
                func.GraphRenderer.OnDraw();                
                //func.OnDraw(null, false);
            }
            ImGuiAPI.EndChild();
        }

        public async Task<bool> OpenEditor(Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            LoadClassGraph(AssetName);
            //LoadClassGraph(RName.GetRName("UTest/class_graph.xml"));
            await Thread.AsyncDummyClass.DummyFunc();
            return true;
        }

        public void OnCloseEditor()
        {

        }

        public void OnEvent(ref SDL2.SDL.SDL_Event e)
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

