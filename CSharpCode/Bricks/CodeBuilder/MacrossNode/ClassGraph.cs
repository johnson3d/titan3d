using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public partial class ClassGraph : IO.ISerializer, Editor.IAssetEditor, Graphics.Pipeline.IRootForm
    {
        public ClassGraph()
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
            if (string.IsNullOrEmpty(DefClass.SuperClassName))
                return;
            var superClass = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(DefClass.SuperClassName);
            if (superClass == null)
                return;

            mOverrideMethodMenuItems.Clear();
            var methods = superClass.SystemType.GetMethods();
            for(int i=0; i<methods.Length; i++)
            {
                if (!methods[i].IsVirtual)
                    continue;

                var method = methods[i];
                var menuItem = new EGui.UIProxy.MenuItemProxy();
                menuItem.MenuName = method.Name + "(";
                var parameters = method.GetParameters();
                for(int paramIdx = 0; paramIdx < parameters.Length; paramIdx++)
                {
                    menuItem.MenuName += parameters[paramIdx].ParameterType.Name + ",";
                }
                menuItem.MenuName = menuItem.MenuName.TrimEnd(',');
                menuItem.MenuName += ")";

                menuItem.Action = (proxy, data) =>
                {
                    var f = new DefineFunction();
                    f.IsOverride = true;
                    f.ReturnType = method.ReturnType.FullName;
                    f.Name = method.Name;
                    f.Arguments = new List<DefineFunctionParam>(parameters.Length);
                    for(int paramIdx = 0; paramIdx < parameters.Length; paramIdx++)
                    {
                        var parameter = parameters[paramIdx];
                        var argument = new DefineFunctionParam()
                        {
                            VarName = parameter.Name,
                        };
                        argument.DefType = parameter.ParameterType.FullName.TrimEnd('&');
                        if (parameter.IsIn)
                            argument.OpType = DefineFunctionParam.enOpType.In;
                        else if (parameter.IsOut)
                            argument.OpType = DefineFunctionParam.enOpType.Out;
                        else if (parameter.ParameterType.IsByRef)
                            argument.OpType = DefineFunctionParam.enOpType.Ref;
                        if (parameter.HasDefaultValue && parameter.DefaultValue != null)
                            argument.InitValue = parameter.DefaultValue.ToString();
                        if (parameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
                            argument.IsParamArray = true;
                        f.Arguments.Add(argument);
                    }
                    DefClass.Functions.Add(f);

                    var func = FunctionGraph.NewGraph(this, f);
                    Functions.Add(func);
                };

                mOverrideMethodMenuItems.Add(menuItem);
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
        public EGui.Controls.PropertyGrid.PropertyGrid NodePropGrid { get; } = new EGui.Controls.PropertyGrid.PropertyGrid();
        [Rtti.Meta(Order = 0)]
        public DefineClass DefClass { get; } = new DefineClass();
        //[Rtti.Meta(Order = 1)]
        public List<FunctionGraph> Functions { get; } = new List<FunctionGraph>();
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
        public List<FunctionGraph> OpenFunctions = new List<FunctionGraph>();
        public MemberVar DraggingMember;
        public bool IsDraggingMember = false;
        public float LeftWidth = 300;
        bool bFirstDraw = true;
        
        public void SaveClassGraph(RName rn)
        {
            var xml = new System.Xml.XmlDocument();
            var xmlRoot = xml.CreateElement($"Root", xml.NamespaceURI);
            xml.AppendChild(xmlRoot);
            IO.SerializerHelper.WriteObjectMetaFields(xml, xmlRoot, this);
            var xmlText = IO.FileManager.GetXmlText(xml);
            IO.FileManager.WriteAllText($"{rn.Address}/class_graph.dat", xmlText);

            for(int i=0; i<Functions.Count; i++)
            {
                var funcXml = new System.Xml.XmlDocument();
                var funcXmlRoot = funcXml.CreateElement($"Root", funcXml.NamespaceURI);
                funcXml.AppendChild(funcXmlRoot);
                IO.SerializerHelper.WriteObjectMetaFields(funcXml, funcXmlRoot, Functions[i]);
                var funcXmlText = IO.FileManager.GetXmlText(funcXml);
                IO.FileManager.WriteAllText($"{rn.Address}/{Functions[i].Function.Name}.func", funcXmlText);
            }

            //LoadClassGraph(rn);
        }
        public void LoadClassGraph(RName rn)
        {
            DefClass.Reset();
            Functions.Clear();
            OpenFunctions.Clear();
            NodePropGrid.Target = null;
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

                DefClass.Functions.Clear();
                var funcFiles = IO.FileManager.GetFiles(rn.Address, "*.func", false);
                for (int i = 0; i < funcFiles.Length; i++)
                {
                    try
                    {
                        var funcXml = IO.FileManager.LoadXml(funcFiles[i]);
                        var funcGraph = FunctionGraph.NewGraph(this);
                        object pFuncGraph = funcGraph;
                        IO.SerializerHelper.ReadObjectMetaFields(null, funcXml.LastChild as System.Xml.XmlElement, ref pFuncGraph, null);
                        DefClass.Functions.Add(funcGraph.Function);
                        Functions.Add(funcGraph);
                    }
                    catch (System.Exception e)
                    {
                        Profiler.Log.WriteException(e);
                    }
                }
            }

            InitializeOverrideMethodList();
        }
        public string GenerateCode()
        {
            try
            {
                var gen = new Bricks.CodeBuilder.CSharp.CSGen();
                foreach (var i in Functions)
                {
                    i.BuildCodeExpr(gen);
                }

                gen.BuildClassCode(DefClass);

                SaveCSFile(gen);

                EngineNS.UEngine.Instance.MacrossManager.GenerateProjects();
                return gen.ClassCode;
            }
            catch (GraphException ex)
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
            var csFiles = IO.FileManager.GetFiles(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Game), "*.cs");
            List<string> arguments = new List<string>();
            for (int i=0; i<csFiles.Length; ++i)
                arguments.Add(CodeCompiler.CSharpCompiler.GetCommandArguments(CodeCompiler.CSharpCompiler.enCommandType.CSFile, csFiles[i]));
            arguments.Add(CodeCompiler.CSharpCompiler.GetCommandArguments(CodeCompiler.CSharpCompiler.enCommandType.OutputFile, UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Current) + "net5.0/GameProject.dll"));
            arguments.Add(CodeCompiler.CSharpCompiler.GetCommandArguments(CodeCompiler.CSharpCompiler.enCommandType.PdbFile, UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Current) + "net5.0/GameProject.pdb"));
            arguments.Add(CodeCompiler.CSharpCompiler.GetCommandArguments(CodeCompiler.CSharpCompiler.enCommandType.Outputkind, OutputKind.DynamicallyLinkedLibrary.ToString()));
            arguments.Add(CodeCompiler.CSharpCompiler.GetCommandArguments(CodeCompiler.CSharpCompiler.enCommandType.OptimizationLevel, OptimizationLevel.Debug.ToString()));
            
            arguments.Add(CodeCompiler.CSharpCompiler.GetCommandArguments(CodeCompiler.CSharpCompiler.enCommandType.RefAssemblyFile, typeof(object).Assembly.Location));

            CodeCompiler.CSharpCompiler.CompilerCSharpWithArguments(arguments.ToArray());
        }

        void SaveCSFile(CSharp.CSGen gen)
        {
            var fileName = AssetName.Address + "/" + DefClass.ClassName + ".cs";
            using(var sr = new System.IO.StreamWriter(fileName, false, Encoding.UTF8))
            {
                sr.Write(gen.ClassCode);
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
                    mMenuItems[i].OnDraw(ref drawList, ref Support.UAnyPointer.Default);

                ImGuiAPI.EndMenuBar();
            }
        }
        public unsafe void OnDraw()
        {
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (ImGuiAPI.Begin($"Macross:{IO.FileManager.GetPureName(AssetName!=null? AssetName.Name :"NoName")}", ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None| ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar))
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                mToolbar.OnDraw(ref drawList, ref Support.UAnyPointer.Default);

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
                        for (int i = 0; i < DefClass.Members.Count; i++)
                        {
                            if (DefClass.Members[i].VarName == $"Member_{num}")
                            {
                                num++;
                                bFind = true;
                                break;
                            }
                        }
                        if (!bFind)
                            break;
                    }

                    var mb = new DefineVar();
                    mb.DefType = typeof(int).FullName;
                    mb.VarName = $"Member_{num}";
                    mb.IsLocalVar = false;
                    DefClass.Members.Add(mb);
                }
                if (membersTreeNodeResult)
                {
                    if (DraggingMember != null && IsDraggingMember == false && ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, 10))
                    {
                        IsDraggingMember = true;
                    }
                    var memRegionSize = ImGuiAPI.GetContentRegionAvail();
                    for(int i=0; i<DefClass.Members.Count; i++)
                    {
                        var mem = DefClass.Members[i];
                        var memberTreeNodeResult = ImGuiAPI.TreeNodeEx(mem.VarName, flags);
                        var memberTreeNodeClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
                        ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
                        if(EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "mem_X_" + i))
                        {
                            // todo: 引用删除警告
                            DefClass.Members.Remove(mem);
                            break;
                        }
                        if (memberTreeNodeResult)
                        {
                            if (memberTreeNodeClicked)
                            {
                                PGMember.Target = mem;
                                DraggingMember = MemberVar.NewMemberVar(DefClass, mem.VarName);
                                DraggingMember.Graph = this;
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
                            for (int i = 0; i < DefClass.Functions.Count; i++)
                            {
                                if (DefClass.Functions[i].Name == $"Method_{num}")
                                {
                                    num++;
                                    bFind = true;
                                    break;
                                }
                            }
                            if (!bFind)
                                break;
                        }

                        var f = new DefineFunction();
                        f.ReturnType = typeof(void).FullName;
                        f.Name = $"Method_{num}";
                        DefClass.Functions.Add(f);

                        var func = FunctionGraph.NewGraph(this, f);
                        Functions.Add(func);
                    }
                    if (EGui.UIProxy.MenuItemProxy.BeginMenuItem("Override Method", null, null, ref drawList, ref menuData, ref mOverrideMenuState))
                    {
                        for(int i=0; i < mOverrideMethodMenuItems.Count; i++)
                        {
                            mOverrideMethodMenuItems[i].OnDraw(ref drawList, ref menuData);
                        }
                        EGui.UIProxy.MenuItemProxy.EndMenuItem();
                    }

                    ImGuiAPI.EndPopup();
                }
                if (methodsTreeNodeResult)
                {
                    var funcRegionSize = ImGuiAPI.GetContentRegionAvail();
                    for(int i=Functions.Count - 1; i>=0; i--)
                    {
                        var func = Functions[i];
                        var funcTreeNodeResult = ImGuiAPI.TreeNodeEx(func.ToString(), flags);
                        ImGuiAPI.SameLine(0, EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X);
                        var funcTreeNodeDoubleClicked = ImGuiAPI.IsItemDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
                        var funcTreeNodeIsItemClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
                        ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
                        var keyName = $"Delete func {func.Function.Name}?";
                        if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "func_X_" + i))
                        {
                            EGui.UIProxy.MessageBox.Open(keyName);
                            break;
                        }
                        EGui.UIProxy.MessageBox.Draw(keyName, $"Are you sure to delete {func.Function.Name}?", EGui.UIProxy.MessageBox.EButtonType.YesNo,
                        () =>
                        {
                            if (AssetName != null)
                                IO.FileManager.DeleteFile($"{AssetName.Address}/{func.Function.Name}.func");
                            Functions.Remove(func);
                            DefClass.Functions.Remove(func.Function);
                        }, null);

                        if (funcTreeNodeResult)
                        {
                            if (funcTreeNodeDoubleClicked)
                            {
                                if (OpenFunctions.Contains(func) == false)
                                {
                                    func.VisibleInClassGraphTables = true;
                                    OpenFunctions.Add(func);
                                }
                            }
                            else if (funcTreeNodeIsItemClicked)
                            {
                                PGMember.Target = func.Function;
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
            if (ImGuiAPI.CollapsingHeader("NodeProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
            {
                NodePropGrid.OnDraw(true, false, false);
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
                    fixed (bool* pOpen = &i.VisibleInClassGraphTables)
                    {
                        if (ImGuiAPI.BeginTabItem(i.Function.Name, pOpen, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                        {
                            DrawFunctionGraph(i, sz);

                            ImGuiAPI.EndTabItem();
                        }
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

        public void DrawFunctionGraph(FunctionGraph func, Vector2 size)
        {
            if (ImGuiAPI.BeginChild("Function", in size, true, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar))
            {
                func.OnDraw(null, false);
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
        public Bricks.CodeBuilder.MacrossNode.ClassGraph mClassGraph = new Bricks.CodeBuilder.MacrossNode.ClassGraph();
        public void UT_Draw()
        {
            mClassGraph.OnDraw();
        }
        public void UnitTestEntrance()
        {

        }
    }
}

