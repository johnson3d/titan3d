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
using System.Drawing;
using System.Reflection;
using EngineNS.Rtti;
using EngineNS.DesignMacross.Description;

namespace EngineNS.DesignMacross.Editor
{
    public class TtDesignMacrossEditor : IO.ISerializer, EngineNS.Editor.IAssetEditor, IRootForm
    {
        public TtClassDescription ClassDescription { get; set; } = new TtClassDescription();
        public TtDeclarationEditPanel DeclarationEditPanel { get; set; } = new TtDeclarationEditPanel();
        public TtDefinitionGraphPanel DefinitionGraphPanel { get; set; } = new TtDefinitionGraphPanel();
        TtCommandHistory CommandHistory { get; set; } = new TtCommandHistory();

        public TtDesignMacrossEditor()
        {

        }
        public EGui.Controls.PropertyGrid.PropertyGrid PGMember = new EGui.Controls.PropertyGrid.PropertyGrid();
        public async Task<bool> Initialize()
        {
            DeclarationEditPanel.ClassDesc = ClassDescription;
            DeclarationEditPanel.Initialize();
            InitializeMainMenu();
            await PGMember.Initialize();
            return true;
        }

        public unsafe void OnDraw()
        {
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (EGui.UIProxy.DockProxy.BeginMainForm($"Macross:{IO.TtFileManager.GetPureName(AssetName != null ? AssetName.Name : "NoName")}", this, ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar))
            {
                DrawToolbar();
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    if (UEngine.Instance.GfxDevice.SlateApplication is EngineNS.Editor.UMainEditorApplication mainEditor)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                OnDrawMainMenu();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm();

            //draw menu
            //draw toolbar

            
            FDesignMacrossEditorRenderingContext rendingContext = new FDesignMacrossEditorRenderingContext();
            rendingContext.EditorInteroperation.DeclarationEditPanel = DeclarationEditPanel;
            rendingContext.EditorInteroperation.DefinitionGraphEditPanel = DefinitionGraphPanel;
            rendingContext.CommandHistory = CommandHistory;

            bool mClassViewShow = true;
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "ClassView", ref mClassViewShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                DeclarationEditPanel.Draw(rendingContext);
            }
            EGui.UIProxy.DockProxy.EndPanel();

            bool mGraphWindowShow = true;
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "GraphWindow", ref mGraphWindowShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                DefinitionGraphPanel.Draw(rendingContext);
            }
            EGui.UIProxy.DockProxy.EndPanel();

            bool mNodePropertyShow = true;
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "NodeProperty", ref mNodePropertyShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                PGMember.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }

        #region CodeGen
        public string GenerateCode()
        {
            List<UClassDeclaration> classDeclarations = ClassDescription.BuildClassDeclarations();

            var codeGenerator = new UCSharpCodeGenerator();
            string code = "";
            foreach (var classDeclaration in classDeclarations)
            {
                codeGenerator.GenerateClassCode(classDeclaration, AssetName, ref code);
            }
            return code;
            
        }
        #endregion CodeGen

        #region IO.ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml) { }
        public void OnPropertyRead(object root, System.Reflection.PropertyInfo prop, bool fromXml) { }
        #endregion IO.ISerializer

        #region DrawToolbar
        struct STToolButtonData
        {
            public bool IsMouseDown;
            public bool IsMouseHover;
        }
        STToolButtonData[] mToolBtnDatas = new STToolButtonData[7];
        protected void DrawToolbar()
        {
            var drawList = ImGuiAPI.GetWindowDrawList();

            int toolBarItemIdx = 0;
            var spacing = EGui.UIProxy.StyleConfig.Instance.ToolbarSeparatorThickness + EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X * 2;
            EGui.UIProxy.Toolbar.BeginToolbar(in drawList);
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "Save"))
            {
                //SaveClassGraph(AssetName);
                //GenerateCode();
                //CompileCode();
            }
            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.UAnyPointer.Default);
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "GenCode", false, -1, 0, spacing))
            {
                GenerateCode();
                //CompileCode();
            }
            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.UAnyPointer.Default);
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "ClassSettings", false, -1, 0, spacing))
            {
                //PGMember.Target = DefClass;
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
        #endregion Toolbar

        #region IRootForm
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        bool mDockInitialized = false;
        protected ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        protected unsafe void ResetDockspace(bool force = false)
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
            uint unionConfigId = 0;
            ImGuiAPI.DockBuilderSplitNode(graphId, ImGuiDir_.ImGuiDir_Right, 0.4f, ref unionConfigId, ref graphId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("GraphWindow", mDockKeyClass), graphId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("NodeProperty", mDockKeyClass), propertyId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("UnionNodeConfig", mDockKeyClass), unionConfigId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("ClassView", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderFinish(id);
        }
        public void Dispose()
        {

        }
        #endregion IRootForm

        #region EngineNS.Editor.IAssetEditor
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible
        {
            get => mVisible;
            set => mVisible = value;
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        public async Task<bool> OpenEditor(EngineNS.Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            //LoadClassGraph(AssetName);
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
        #endregion

        #region DrawMainMenu
        List<EGui.UIProxy.MenuItemProxy> mMenuItems = new List<EGui.UIProxy.MenuItemProxy>();
        void InitializeMainMenu()
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
                                //LoadClassGraph(AssetName);
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Save",
                            Action = (item, data)=>
                            {
                                //SaveClassGraph(AssetName);
                                //GenerateCode();
                                //CompileCode();
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
                    mMenuItems[i].OnDraw(in drawList, in Support.UAnyPointer.Default);

                ImGuiAPI.EndMenuBar();
            }
        } 
        #endregion
    }
}
