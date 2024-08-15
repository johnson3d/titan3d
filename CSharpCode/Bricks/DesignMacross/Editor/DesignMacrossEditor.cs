using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Description;
using System.Text;
using System.Diagnostics;

namespace EngineNS.DesignMacross.Editor
{
    public class TtDesignMacrossEditor : IO.ISerializer, EngineNS.Editor.IAssetEditor, IRootForm
    {
        private UDesignMacross mDesignMacross = null;
        //private TtClassDescription mClassDescription = new TtClassDescription();
        //public TtClassDescription ClassDescription { get => mClassDescription; set => mClassDescription = value; }
        public TtOutlineEditPanel DeclarationEditPanel { get; set; } = new TtOutlineEditPanel();
        public TtGraphEditPanel DefinitionGraphPanel { get; set; } = new TtGraphEditPanel();
        TtCommandHistory CommandHistory { get; set; } = new TtCommandHistory();

        TtGraphElementStyleCollection GraphElementCollection = new TtGraphElementStyleCollection();

        public Dictionary<Guid, IGraphElement> DescriptionsElement { get; set; } = new Dictionary<Guid, IGraphElement>();
        public TtDesignMacrossEditor()
        {

        }
        public EGui.Controls.PropertyGrid.PropertyGrid PGMember = new EGui.Controls.PropertyGrid.PropertyGrid();
        public async Thread.Async.TtTask<bool> Initialize()
        {
            DeclarationEditPanel.Initialize();
            InitializeMainMenu();
            await PGMember.Initialize();
            return true;
        }

        public unsafe void OnDraw()
        {
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            var result = EGui.UIProxy.DockProxy.BeginMainForm($"Macross:{IO.TtFileManager.GetPureName(AssetName != null ? AssetName.Name : "NoName")}", this, ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar);
            if (result)
            {
                DrawToolbar();
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    if (TtEngine.Instance.GfxDevice.SlateApplication is EngineNS.Editor.UMainEditorApplication mainEditor)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                OnDrawMainMenu();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(result);

            //draw menu
            //draw toolbar

            DescriptionsElement.Clear();
            FDesignMacrossEditorRenderingContext rendingContext = new FDesignMacrossEditorRenderingContext();
            rendingContext.EditorInteroperation.OutlineEditPanel = DeclarationEditPanel;
            rendingContext.EditorInteroperation.GraphEditPanel = DefinitionGraphPanel;
            rendingContext.EditorInteroperation.PGMember = PGMember;
            rendingContext.CommandHistory = CommandHistory;
            rendingContext.GraphElementStyleManager = GraphElementCollection;
            rendingContext.DescriptionsElement = DescriptionsElement;
            rendingContext.DesignedClassDescription = mDesignMacross.DesignedClassDescription;
            bool mClassViewShow = true;
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "ClassView", ref mClassViewShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                DeclarationEditPanel.Draw(rendingContext);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);

            bool mGraphWindowShow = true;
            show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "GraphWindow", ref mGraphWindowShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                DefinitionGraphPanel.Draw(rendingContext);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);

            bool mNodePropertyShow = true;
            show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "NodeProperty", ref mNodePropertyShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                PGMember.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);

            if (TtEngine.Instance.InputSystem.IsKeyPressed(EngineNS.Bricks.Input.Keycode.KEY_z))
            {
                if (bIsZKeyDown)
                {
                    bIsZKeyHasDown = true;
                }
                bIsZKeyDown = true;
            }
            else
            {
                bIsZKeyDown = false;
                bIsZKeyHasDown = false;
            }

            if (TtEngine.Instance.InputSystem.IsKeyPressed(EngineNS.Bricks.Input.Keycode.KEY_y))
            {
                if (bIsYKeyDown)
                {
                    bIsYKeyHasDown = true;
                }
                bIsYKeyDown = true;
            }
            else
            {
                bIsYKeyDown = false;
                bIsYKeyHasDown = false;
            }

            if (bIsZKeyDown && !bIsZKeyHasDown && TtEngine.Instance.InputSystem.IsCtrlKeyDown())
            {
                CommandHistory.Undo();
            }

            if (TtEngine.Instance.InputSystem.IsCtrlKeyDown() && bIsYKeyDown && !bIsYKeyHasDown)
            {
                CommandHistory.Redo();
            }
        }

        bool bIsZKeyHasDown = false;
        bool bIsZKeyDown = false;
        bool bIsYKeyHasDown = false;
        bool bIsYKeyDown = false;

        #region Save Load

        void SaveElements(RName rn)
        {
            var xml = new System.Xml.XmlDocument();
            var xmlRoot = xml.CreateElement($"Root", xml.NamespaceURI);
            xml.AppendChild(xmlRoot);
            IO.SerializerHelper.WriteObjectMetaFields(xml, xmlRoot, GraphElementCollection);
            var xmlText = IO.TtFileManager.GetXmlText(xml);
            IO.TtFileManager.WriteAllText($"{rn.Address}/GraphElementStyles.dat", xmlText);
        }

        void LoadElements(RName rn)
        {
            var xml = IO.TtFileManager.LoadXml($"{rn.Address}/GraphElementStyles.dat");
            if (xml == null)
                return;
            object pGraphElementCollection = GraphElementCollection;
            IO.SerializerHelper.ReadObjectMetaFields(GraphElementCollection, xml.LastChild as System.Xml.XmlElement, ref pGraphElementCollection, null);
        }
        #endregion Save Load

        #region CodeGen
        List<UClassDeclaration> ClassDeclarationsForGenerateCompileCode = new List<UClassDeclaration>();
        public string GenerateCode()
        {
            ClassDeclarationsForGenerateCompileCode.Clear();
            FClassBuildContext classBuildContext = new FClassBuildContext();
            classBuildContext.MainClassDescription = mDesignMacross.DesignedClassDescription;
            ClassDeclarationsForGenerateCompileCode = mDesignMacross.DesignedClassDescription.BuildClassDeclarations(ref classBuildContext);

            var codeGenerator = new UCSharpCodeGenerator();
            string code = "";
            foreach (var classDeclaration in ClassDeclarationsForGenerateCompileCode)
            {
                codeGenerator.GenerateClassCode(classDeclaration, AssetName, ref code);
            }
            var fileName = AssetName.Address + "/" + mDesignMacross.DesignedClassDescription.ClassName + ".cs";
            using (var sr = new System.IO.StreamWriter(fileName, false, Encoding.UTF8))
            {
                sr.Write(code);
            }
            EngineNS.TtEngine.Instance.MacrossManager.GenerateProjects();
            return code;
        }
        public void CompileCode()
        {
            TtEngine.Instance.MacrossManager.ClearGameProjectTemplateBuildFiles();
            var assemblyFile = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource) + TtEngine.Instance.EditorInstance.Config.GameAssembly;
            if (TtEngine.Instance.MacrossModule.CompileCode(assemblyFile))
            {
                TtEngine.Instance.MacrossModule.ReloadAssembly(assemblyFile);
                foreach (var classDeclaration in ClassDeclarationsForGenerateCompileCode)
                {
                    var typeDesc = classDeclaration.TryGetTypeDesc();
                    var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeDesc);
                    meta.BuildMethods();
                    meta.BuildFields();
                    var version = meta.BuildCurrentVersion();
                    version.SaveVersion();
                    meta.SaveClass();
                }

            }
            else
            {
                Debug.Assert(false);
            }
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
                mDesignMacross.Save(AssetName);
                SaveElements(AssetName);
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
                //PGMember.Target = DefClass;
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
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async Thread.Async.TtTask<bool> OpenEditor(EngineNS.Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            mDesignMacross = new UDesignMacross();
            mDesignMacross.Load(AssetName);
            //LoadClassDescription(AssetName);
            LoadElements(AssetName);
            DeclarationEditPanel.ClassDesc = mDesignMacross.DesignedClassDescription;
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
                                mDesignMacross.Save(AssetName);
                                SaveElements(AssetName);
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
                    mMenuItems[i].OnDraw(in drawList, in Support.UAnyPointer.Default);

                ImGuiAPI.EndMenuBar();
            }
        }
        #endregion
    }
}
