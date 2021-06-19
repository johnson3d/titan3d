using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public class ClassGraph : IO.ISerializer, Editor.IAssetEditor, Graphics.Pipeline.IRootForm
    {
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
                IO.FileManager.WriteAllText($"{rn.Address}/{Functions[i].Function.Name}_{(uint)Functions[i].FunctionName.GetHashCode()}.func", funcXmlText);
            }

            //LoadClassGraph(rn);
        }
        public void LoadClassGraph(RName rn)
        {
            DefClass.Reset();
            Functions.Clear();
            OpenFunctions.Clear();
            NodePropGrid.SingleTarget = null;
            PGMember.SingleTarget = null;

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

        public void OnDrawMainMenu()
        {
            if (ImGuiAPI.BeginMenuBar())
            {
                if (ImGuiAPI.BeginMenu("File", true))
                {
                    if (ImGuiAPI.MenuItem("Reload", null, false, true))
                    {
                        //LoadClassGraph(RName.GetRName("UTest/class_graph.xml"));
                        LoadClassGraph(AssetName);
                    }
                    if (ImGuiAPI.MenuItem("Save", null, false, true))
                    {
                        //SaveClassGraph(RName.GetRName("UTest/class_graph.xml"));
                        SaveClassGraph(AssetName);
                        GenerateCode();
                        CompileCode();
                    }
                    ImGuiAPI.Separator();
                    ImGuiAPI.EndMenu();
                }
                ImGuiAPI.EndMenuBar();
            }
        }
        public unsafe void OnDraw()
        {
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (ImGuiAPI.Begin($"Macross:{IO.FileManager.GetPureName(AssetName!=null? AssetName.Name :"NoName")}", ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None| ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar))
            {
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

                var cltMin = ImGuiAPI.GetWindowContentRegionMin();
                var cltMax = ImGuiAPI.GetWindowContentRegionMax();

                ImGuiAPI.Columns(2, null, true);
                if (bFirstDraw)
                {
                    ImGuiAPI.SetColumnWidth(0, LeftWidth);
                    bFirstDraw = false;
                }
                LeftWidth = ImGuiAPI.GetColumnWidth(0);
                var szLeft = new Vector2(LeftWidth, cltMax.Y - cltMin.Y);
                if (ImGuiAPI.BeginChild("LeftWin", ref szLeft, true, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
                {
                    OnLeftWindow();
                    OnDrawMenuLeftWindow();
                }
                ImGuiAPI.EndChild();
                ImGuiAPI.NextColumn();

                var colWidth = ImGuiAPI.GetColumnWidth(1);
                var szRight = new Vector2(colWidth, cltMax.Y - cltMin.Y);
                if (ImGuiAPI.BeginChild("RightWin", ref szRight, true, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
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
        public enum EMenuType
        {
            None,
            Member,
            Method,
        }
        private void OnDrawMenuLeftWindow()
        {
            if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                bool isShow = false;
                switch (mMenuType)
                {
                    case EMenuType.Member:
                        isShow = LeftWinDrawMenu_Member(); 
                        break;
                    case EMenuType.Method:
                        isShow = LeftWinDrawMenu_Method();
                        break;
                    default:
                        break;
                }
                ImGuiAPI.EndPopup();
                if (isShow == false)
                {
                    mMenuType = EMenuType.None;
                }
            }
        }
        EMenuType mMenuType = EMenuType.None;
        protected unsafe void OnLeftWindow()
        {
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
            if (ImGuiAPI.CollapsingHeader("ClassView", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
            {
                var sz = new Vector2(-1, 0);
                //ImGuiAPI.SetNextItemWidth(-1);
                if (ImGuiAPI.Button("GenCode", ref sz))
                {
                    GenerateCode();
                    CompileCode();
                }
                if (ImGuiAPI.TreeNode("ClassSettings"))
                {
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        PGMember.SingleTarget = DefClass;
                    }
                    ImGuiAPI.TreePop();
                }
                if (ImGuiAPI.TreeNode("Members"))
                {
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                    {
                        mMenuType = EMenuType.Member;
                    }
                    if (DraggingMember != null && IsDraggingMember == false && ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, 10))
                    {
                        IsDraggingMember = true;
                    }
                    foreach (var i in DefClass.Members)
                    {
                        if (ImGuiAPI.TreeNodeEx(i.VarName, flags))
                        {
                            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                PGMember.SingleTarget = i;
                                mMenuType = EMenuType.None;
                                DraggingMember = MemberVar.NewMemberVar(DefClass, i.VarName);
                                DraggingMember.Graph = this;
                                IsDraggingMember = false;
                            }
                        }
                    }
                    ImGuiAPI.TreePop();
                }
                if (ImGuiAPI.TreeNode("Methods"))
                {
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                    {
                        mMenuType = EMenuType.Method;
                    }
                    foreach(var i in Functions)
                    {
                        if (ImGuiAPI.TreeNodeEx(i.ToString(), flags))
                        {
                            if (ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                if (OpenFunctions.Contains(i) == false)
                                {
                                    i.VisibleInClassGraphTables = true;
                                    OpenFunctions.Add(i);
                                }
                            }
                            else if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                PGMember.SingleTarget = i.Function;
                                mMenuType = EMenuType.None;
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
            if (ImGuiAPI.BeginChild("Function", ref size, true, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar))
            {
                func.OnDraw(null, false);
            }
            ImGuiAPI.EndChild();
        }

        #region Menu
        bool LeftWinDrawMenu_Member()
        {
            if (mMenuType == EMenuType.None)
                return false;
            //ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
            if (ImGuiAPI.BeginMenu("Member", true))
            {
                if (ImGuiAPI.MenuItem($"Add Member", null, false, true))
                {
                    var mb = new DefineVar();
                    mb.DefType = typeof(int).FullName;
                    mb.VarName = $"Member_{DefClass.Members.Count}";
                    mb.IsLocalVar = false;
                    DefClass.Members.Add(mb);
                    mMenuType = EMenuType.None;
                }
                ImGuiAPI.EndMenu();
            }
            return true;
        }
        bool LeftWinDrawMenu_Method()
        {
            if (mMenuType == EMenuType.None)
                return false;
            //ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
            if (ImGuiAPI.BeginMenu("Method", true))
            {
                if (ImGuiAPI.MenuItem($"Add Method", null, false, true))
                {
                    var f = new DefineFunction();
                    f.ReturnType = typeof(void).FullName;
                    f.Name = $"Function_{Functions.Count}";
                    DefClass.Functions.Add(f);

                    var func = FunctionGraph.NewGraph(this, f);
                    Functions.Add(func);

                    mMenuType = EMenuType.None;
                }
                ImGuiAPI.EndMenu();
            }
            return true;
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
        #endregion
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

