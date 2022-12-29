using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.NodeGraph;
using EngineNS.Bricks.RenderPolicyEditor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace EngineNS.DesignMacross
{
    public class UDesignMacrossEditor : IO.ISerializer, Editor.IAssetEditor, IRootForm
    {
        public UDesignMacrossEditor()
        {
            mNewStateMachineMenuState.Reset();
            mNewBehaviorTreeMenuState.Reset();
        }
        public void Cleanup()
        {

        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            InitializeManMenu();
            await PGMember.Initialize();
            return true;
        }
        // StateMachine,BehviorTree都可以是Graph，在类中以变量形式存在，并继承自接口INodeGraph,
        //在类中没添加一个Graph就是添加个变量而不是函数
        List<EGui.UIProxy.MenuItemProxy> mGraphMenuItems = new List<EGui.UIProxy.MenuItemProxy>();
        public IRootForm GetRootForm()
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
            var ameta = UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
            if (ameta != null)
            {
                var tmp = new UMacross();
                tmp.AssetName = rn;
                tmp.UpdateAMetaReferences(ameta);
                ameta.SaveAMeta();
            }

            var xml = new System.Xml.XmlDocument();
            var xmlRoot = xml.CreateElement($"Root", xml.NamespaceURI);
            xml.AppendChild(xmlRoot);
            IO.SerializerHelper.WriteObjectMetaFields(xml, xmlRoot, this);
            var xmlText = IO.FileManager.GetXmlText(xml);
            IO.FileManager.WriteAllText($"{rn.Address}/class_graph.dat", xmlText);

            for (int i = 0; i < Methods.Count; i++)
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
                if (xml == null)
                    return;
                object pThis = this;
                IO.SerializerHelper.ReadObjectMetaFields(this, xml.LastChild as System.Xml.XmlElement, ref pThis, null);

                var nsName = IO.FileManager.GetBaseDirectory(rn.Name).TrimEnd('/').Replace("/", ".");
                if (Regex.IsMatch(nsName, "[A-Za-z0-9_]"))
                    DefClass.Namespace = new UNamespaceDeclaration("NS_" + nsName);
                else
                {
                    DefClass.Namespace = new UNamespaceDeclaration("NS_" + ((UInt32)nsName.GetHashCode()).ToString());
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Macross", $"Get namespace failed, {rn.Name} has invalid char!");
                }
                DefClass.ClearMethods();
            }
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
                mCSCodeGen.GenerateClassCode(DefClass, AssetName, ref code);
                SaveCSFile(code);
                GenerateAssemblyDescCreateInstanceCode();

                EngineNS.UEngine.Instance.MacrossManager.GenerateProjects();
                return code;
            }
            catch (Bricks.NodeGraph.GraphException ex)
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

        void GenerateAssemblyDescCreateInstanceCode()
        {
            var projFolder = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.EngineSource) + IO.FileManager.GetParentPathName(UEngine.Instance.EditorInstance.Config.GameProject);
            var assemblyFileName = projFolder + "/Assembly.cs";
            string assemblyDescCodes = "";
            using (var sr = new System.IO.StreamReader(assemblyFileName, Encoding.UTF8, true))
            {
                assemblyDescCodes = sr.ReadToEnd();
            }

            var startKeyword = "#region MacrossGenerated Start";
            var idx = assemblyDescCodes.IndexOf(startKeyword);
            var startIdx = idx + startKeyword.Length + 1;
            var keyStr = $"if (name == RName.GetRName(\"{this.AssetName.Name}\", {this.AssetName.RNameType.GetType().FullName.Replace("+", ".")}.{this.AssetName.RNameType.ToString()}))";
            var keyIdx = assemblyDescCodes.IndexOf(keyStr, idx);
            var tab = "                ";
            var str = tab + keyStr + "\r\n";
            str += tab + "{\r\n";
            str += $"{tab}    return new {DefClass.GetFullName()}();\r\n";
            str += tab + "}\r\n";
            if (keyIdx < 0)
            {
                assemblyDescCodes = assemblyDescCodes.Insert(startIdx, str);
            }
            else
            {
                keyIdx -= tab.Length;
                var endIdx = assemblyDescCodes.IndexOf("}\r\n", keyIdx) + "}\r\n".Length;
                assemblyDescCodes = assemblyDescCodes.Remove(keyIdx, endIdx - keyIdx);
                assemblyDescCodes = assemblyDescCodes.Insert(keyIdx, str);
            }
            using (var sw = new System.IO.StreamWriter(assemblyFileName, false, Encoding.UTF8))
            {
                sw.Write(assemblyDescCodes);
            }
        }
        public static bool RemoveAssemblyDescCreateInstanceCode(string name, RName.ERNameType type)
        {
            try
            {
                var projFolder = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.EngineSource) + IO.FileManager.GetParentPathName(UEngine.Instance.EditorInstance.Config.GameProject);
                var assemblyFileName = projFolder + "/Assembly.cs";
                string assemblyDescCodes = "";
                using (var sr = new System.IO.StreamReader(assemblyFileName, Encoding.UTF8, true))
                {
                    assemblyDescCodes = sr.ReadToEnd();
                }
                if (string.IsNullOrEmpty(assemblyDescCodes))
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Macross", "Remove macross create instance code failed, Assembly file is empty!");
                    return false;
                }
                var startKeyword = "#region MacrossGenerated Start";
                var idx = assemblyDescCodes.IndexOf(startKeyword);
                var startIdx = idx + startKeyword.Length + 1;
                var keyStr = $"if (name == RName.GetRName(\"{name}\", {type.GetType().FullName.Replace("+", ".")}.{type.ToString()}))";
                var keyIdx = assemblyDescCodes.IndexOf(keyStr, idx);
                var tab = "                ";
                if (keyIdx >= 0)
                {
                    keyIdx -= tab.Length;
                    var endIdx = assemblyDescCodes.IndexOf("}\r\n", keyIdx) + "}\r\n".Length;
                    assemblyDescCodes = assemblyDescCodes.Remove(keyIdx, endIdx - keyIdx);
                    using (var sw = new System.IO.StreamWriter(assemblyFileName, false, Encoding.UTF8))
                    {
                        sw.Write(assemblyDescCodes);
                    }
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Macross", ex.ToString());
                return false;
            }
            return false;
        }

        public void CompileCode()
        {
            UEngine.Instance.MacrossManager.ClearGameProjectTemplateBuildFiles();
            var assemblyFile = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.EngineSource) + UEngine.Instance.EditorInstance.Config.GameAssembly;
            if (UEngine.Instance.MacrossModule.CompileCode(assemblyFile))
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
            using (var sr = new System.IO.StreamWriter(fileName, false, Encoding.UTF8))
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

        bool mDockInitialized = false;
        ImGuiWindowClass mDockKeyClass;
        unsafe void ResetDockspace(bool force = false)
        {
            var pos = ImGuiAPI.GetCursorPos();
            var id = ImGuiAPI.GetID(AssetName.Name + "_Dockspace");
            mDockKeyClass.ClassId = id;
            ImGuiAPI.DockSpace(id, Vector2.Zero, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, mDockKeyClass);
            if (mDockInitialized && !force)
                return;
            ImGuiAPI.DockBuilderRemoveNode(id);
            ImGuiAPI.DockBuilderAddNode(id, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None);
            ImGuiAPI.DockBuilderSetNodePos(id, pos);
            ImGuiAPI.DockBuilderSetNodeSize(id, Vector2.One);
            mDockInitialized = true;

            var graphId = id;
            uint leftId = 0;
            ImGuiAPI.DockBuilderSplitNode(graphId, ImGuiDir_.ImGuiDir_Left, 0.2f, ref leftId, ref graphId);
            uint propertyId = 0;
            ImGuiAPI.DockBuilderSplitNode(graphId, ImGuiDir_.ImGuiDir_Right, 0.2f, ref propertyId, ref graphId);

            ImGuiAPI.DockBuilderDockWindow(GetDockWindowName("GraphWindow"), graphId);
            ImGuiAPI.DockBuilderDockWindow(GetDockWindowName("NodeProperty"), propertyId);
            ImGuiAPI.DockBuilderDockWindow(GetDockWindowName("Detials"), leftId);
            ImGuiAPI.DockBuilderFinish(id);
        }
        string GetDockWindowName(string name)
        {
            return name + "##" + mDockKeyClass.m_ClassId;
        }
        struct STToolButtonData
        {
            public bool IsMouseDown;
            public bool IsMouseHover;
        }
        STToolButtonData[] mToolBtnDatas = new STToolButtonData[7];
        void DrawToolbar()
        {
            var drawList = ImGuiAPI.GetWindowDrawList();

            int toolBarItemIdx = 0;
            var spacing = EGui.UIProxy.StyleConfig.Instance.ToolbarSeparatorThickness + EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X * 2;
            EGui.UIProxy.Toolbar.BeginToolbar(in drawList);
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "Save"))
            {
                SaveClassGraph(AssetName);
                GenerateCode();
                CompileCode();
            }
            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.UAnyPointer.Default);
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "GenCode", false, -1, 0, spacing))
            {
                GenerateCode();
                CompileCode();
            }
            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.UAnyPointer.Default);
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "ClassSettings", false, -1, 0, spacing))
            {
                PGMember.Target = DefClass;
            }
            toolBarItemIdx++;
            // test ////////////////
            //if(EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList, in Support.UAnyPointer.Default,
            //    ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "DebugTest"))
            //{
            //    var result = UEngine.Instance.EventPoster.Post(() =>
            //    {
            //        var tt = UEngine.Instance.MacrossModule.NewInnerObject<Macross.UMacrossTestClass>(AssetName);
            //        var methodInfo = tt.GetType().GetMethod("Method_0");
            //        //tt.VirtualFunc3(5);
            //        methodInfo?.Invoke(tt, null);

            //        return true;
            //    }, Thread.Async.EAsyncTarget.Logic);
            //}
            ////////////////////////
            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.UAnyPointer.Default);
            if (Macross.UMacrossDebugger.Instance.CurrrentBreak != null)
            {
                if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                    ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "Run", false, -1, 0, spacing))
                {
                    Macross.UMacrossDebugger.Instance.Run();
                }
            }

            EGui.UIProxy.Toolbar.EndToolbar();
        }
        public unsafe void OnDraw()
        {
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (EGui.UIProxy.DockProxy.BeginMainForm($"Macross:{IO.FileManager.GetPureName(AssetName != null ? AssetName.Name : "NoName")}", ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar))
            {
                DrawToolbar();

                //if (ImGuiAPI.IsWindowDocked())
                //{
                //    DockId = ImGuiAPI.GetWindowDockID();
                //}
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }

                OnDrawMainMenu();

                //ImGuiAPI.Columns(2, null, true);
                //if (bFirstDraw)
                //{
                //    ImGuiAPI.SetColumnWidth(0, LeftWidth);
                //    bFirstDraw = false;
                //}
                //var curPos = ImGuiAPI.GetCursorScreenPos();
                //LeftWidth = ImGuiAPI.GetColumnWidth(0);
                //var szLeft = new Vector2(LeftWidth, 0);
                //if (ImGuiAPI.BeginChild("LeftWin", in szLeft, true, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
                //{
                //    OnLeftWindow();
                //}
                //ImGuiAPI.EndChild();
                //ImGuiAPI.NextColumn();

                //var colWidth = ImGuiAPI.GetColumnWidth(1);
                //var szRight = new Vector2(colWidth, 0);
                //if (ImGuiAPI.BeginChild("RightWin", in szRight, true, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
                //{
                //    OnDrawGraph();
                //}
                //ImGuiAPI.EndChild();

                //ImGuiAPI.Columns(1, null, true);
                //ImGuiAPI.NextColumn();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm();

            DrawDetials();
            DrawGraph();
            DrawPropertyGrid();

            if (IsDraggingMember == true && ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) == false)
            {
                IsDraggingMember = false;
                DraggingMember = null;
            }
        }

        public void RemoveMethod(UMacrossMethodGraph method)
        {


        }

        bool mDetialsShow = true;
        EGui.UIProxy.MenuItemProxy.MenuState mNewStateMachineMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        EGui.UIProxy.MenuItemProxy.MenuState mNewBehaviorTreeMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        protected unsafe void DrawDetials()
        {
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;// | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap;
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, GetDockWindowName("Detials"), ref mDetialsShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
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
                    ImGuiAPI.OpenPopup("DesignMacrossMemTypeSelPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                    Type selectedType = typeof(int);
                    if (selectedType != null)
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
                        mb.VariableType = new UTypeReference(selectedType);
                        mb.VariableName = $"Member_{num}";
                        mb.VisitMode = EVisisMode.Local;
                        mb.InitValue = new UPrimitiveExpression(Rtti.UTypeDesc.TypeOf(selectedType), selectedType.IsValueType ? Rtti.UTypeDescManager.CreateInstance(selectedType) : null);
                        mb.Comment = new UCommentStatement("");
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
                    for (int i = 0; i < DefClass.Properties.Count; i++)
                    {
                        var mem = DefClass.Properties[i];
                        var memberTreeNodeResult = ImGuiAPI.TreeNodeEx(mem.VariableName, flags);
                        var memberTreeNodeClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
                        ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
                        if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "mem_X_" + i))
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
                                DraggingMember = MemberVar.NewMemberVar(DefClass, mem.VariableName,
                                    UEngine.Instance.InputSystem.IsKeyDown(Bricks.Input.Keycode.KEY_LCTRL) ? false : true);
                                DraggingMember.UserData = this;
                                IsDraggingMember = false;
                            }
                        }
                    }
                    ImGuiAPI.TreePop();
                }

                var graphTreeNodeResult = ImGuiAPI.TreeNodeEx("Graphs", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap);
                ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
                if (EGui.UIProxy.CustomButton.ToolButton("+", in buttonSize, 0xFF00FF00))
                {
                    ImGuiAPI.OpenPopup("DesignMacrossGraphSelectPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                }
                if (ImGuiAPI.BeginPopup("DesignMacrossGraphSelectPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var drawList = ImGuiAPI.GetWindowDrawList();
                    var menuData = new Support.UAnyPointer();
                    if (EGui.UIProxy.MenuItemProxy.MenuItem("StateMachine Graph", null, false, null, in drawList, in menuData, ref mNewStateMachineMenuState))
                    {

                    }
                    if (EGui.UIProxy.MenuItemProxy.MenuItem("BehaviorTree Graph", null, false, null, in drawList, in menuData, ref mNewBehaviorTreeMenuState))
                    {

                    }
                    ImGuiAPI.EndPopup();
                }
                if(graphTreeNodeResult)
                {
                    if (DraggingMember != null && IsDraggingMember == false && ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, 10))
                    {
                        IsDraggingMember = true;
                    }

                    ImGuiAPI.TreePop();
                }
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        bool mNodePropertyShow = true;
        void DrawPropertyGrid()
        {
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, GetDockWindowName("NodeProperty"), ref mNodePropertyShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                PGMember.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        Macross.UMacrossBreak mBreakerStore = null;
        bool mGraphWindowShow = true;
        protected unsafe void DrawGraph()
        {
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, GetDockWindowName("GraphWindow"), ref mGraphWindowShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                var vMin = ImGuiAPI.GetWindowContentRegionMin();
                var vMax = ImGuiAPI.GetWindowContentRegionMin();
                if (ImGuiAPI.BeginTabBar("OpenFuncTab", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
                {
                    var itMax = ImGuiAPI.GetItemRectSize();
                    vMin.Y += itMax.Y;
                    var sz = vMax - vMin;
                    bool breakerChanged = false;
                    if (mBreakerStore != Macross.UMacrossDebugger.Instance.CurrrentBreak)
                    {
                        mBreakerStore = Macross.UMacrossDebugger.Instance.CurrrentBreak;
                        breakerChanged = true;
                    }
                    for (int i = 0; i < OpenFunctions.Count; i++)
                    {
                        var func = OpenFunctions[i];
                        if (breakerChanged)
                        {
                            for (int linkerIdx = 0; linkerIdx < func.Linkers.Count; linkerIdx++)
                            {
                                func.Linkers[linkerIdx].InDebuggerLine = false;
                            }
                            if (mBreakerStore != null)
                                func.GraphRenderer.BreakerName = mBreakerStore.BreakName;
                            else
                                func.GraphRenderer.BreakerName = "";
                        }

                        if (ImGuiAPI.BeginTabItem(func.Name, ref func.VisibleInClassGraphTables, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                        {
                            DrawFunctionGraph(func, sz);

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
            EGui.UIProxy.DockProxy.EndPanel();
        }

        public void DrawFunctionGraph(UMacrossMethodGraph func, Vector2 size)
        {
            if (ImGuiAPI.BeginChild("Function", in size, true, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar))
            {
                ((UMacrossMethodGraph)(func.GraphRenderer.Graph)).UpdateSelectPG();
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

        public void OnEvent(in Bricks.Input.Event e)
        {

        }
    }
}
