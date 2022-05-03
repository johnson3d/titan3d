using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDL2;

namespace EngineNS.Editor
{
    public partial class UMainEditorApplication : Graphics.Pipeline.USlateApplication, ITickable
    {
        public UAssetEditorManager AssetEditorManager { get; } = new UAssetEditorManager();
        
        public UMainEditorApplication()
        {
            mCpuProfiler = new Editor.Forms.UCpuProfiler();
            mMetaViewer = new Editor.UMetaViewEditor();            
            mMainInspector = new Forms.UInspector();
            WorldViewportSlate = new UEditorWorldViewportSlate(true);
            mWorldOutliner = new Editor.Forms.UWorldOutliner();

            mBrickManager = new Bricks.ProjectGen.UBrickManager();
            mEditorSettings = new Forms.UEditorSettings();
            mEditorSettings.ViewportSlate = this.WorldViewportSlate;
            //mWorldOutliner.TestUWorldOutliner(this);
        }
        private bool IsVisible = true;
        public Editor.Forms.UWorldOutliner mWorldOutliner;
        public Editor.Forms.UCpuProfiler mCpuProfiler;
        public Editor.Forms.UInspector mMainInspector;
        public Editor.UMetaViewEditor mMetaViewer = null;
        public Bricks.ProjectGen.UBrickManager mBrickManager = null;
        public Editor.Forms.UEditorSettings mEditorSettings;

        public UEditorWorldViewportSlate WorldViewportSlate = null;
        public override EGui.Slate.UWorldViewportSlate GetWorldViewportSlate()
        {
            return WorldViewportSlate;
        }
        public EGui.Controls.ContentBrowser ContentBrowser = new EGui.Controls.ContentBrowser();
        public override void Cleanup()
        {
            Graphics.Pipeline.USlateApplication.ClearRootForms();
            UEngine.Instance?.TickableManager.RemoveTickable(this);
            mWinTest.Cleanup();
            base.Cleanup();
        }
        public override async System.Threading.Tasks.Task<bool> InitializeApplication(RHI.CRenderContext rc, RName rpName, Type rpType)
        {
            await base.InitializeApplication(rc, rpName, rpType);

            await ContentBrowser.Initialize();
            Editor.UMainEditorApplication.RegRootForm(ContentBrowser);

            await mMetaViewer.Initialize();

            await WorldViewportSlate.Initialize(this, rpName, Rtti.UTypeDesc.TypeOf(rpType), 0, 1);

            await mMainInspector.Initialize();

            await mEditorSettings.Initialize();

            mMainInspector.PropertyGrid.PGName = "MainInspector";
            mMainInspector.PropertyGrid.Target = EGui.UIProxy.StyleConfig.Instance;// WorldViewportSlate;

            UEngine.Instance.TickableManager.AddTickable(this);

            EGui.UIProxy.StyleConfig.Instance.ResetStyle();
            /////////////////////////////////
            mWinTest.Initialized();
            //Editor.UMainEditorApplication.RegRootForm(mWinTest);
            /////////////////////////////////

            InitMainMenu();

            return true;
        }
        public void InitMainMenu()
        {
            mMenuItems = new List<EGui.UIProxy.MenuItemProxy>()
            {
                new EGui.UIProxy.MenuItemProxy()
                {
                    MenuName = "File",
                    IsTopMenuItem = true,
                    SubMenus = new List<EGui.UIProxy.IUIProxyBase>()
                    {
                        new EGui.UIProxy.NamedMenuSeparator()
                        {
                            Name = "OPEN",
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Load...",
                            Shortcut = "Ctrl+L",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(140.0f/1024, 265.0f/1024),
                                UVMax = new Vector2(156.0f/1024, 281.0f/1024),
                            },
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                // Do sth
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Save...",
                            Shortcut = "Ctrl+S",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(124.0f/1024, 305.0f/1024),
                                UVMax = new Vector2(140.0f/1024, 321.0f/1024),
                            },
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                // Do sth
                            },
                        },
                    },
                },
                new EGui.UIProxy.MenuItemProxy()
                {
                    MenuName = "Windows",
                    IsTopMenuItem = true,
                    SubMenus = new List<EGui.UIProxy.IUIProxyBase>()
                    {
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "WorldOutliner",
                            Selected = true,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                mWorldOutliner.Visible = !mWorldOutliner.Visible;
                                item.Selected = mWorldOutliner.Visible;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "CpuProfiler",
                            Selected = true,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                mCpuProfiler.Visible = !mCpuProfiler.Visible;
                                var application = UEngine.Instance.GfxDevice.MainWindow as EngineNS.Editor.UMainEditorApplication;
                                mCpuProfiler.DockId = application.CenterDockId;
                                item.Selected = mCpuProfiler.Visible;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "MainInspector",
                            Selected = true,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                mMainInspector.Visible = !mMainInspector.Visible;
                                item.Selected = mMainInspector.Visible;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "MetaViewer",
                            Selected = true,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                mMetaViewer.Visible = !mMetaViewer.Visible;
                                item.Selected = mMetaViewer.Visible;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "BrickManager",
                            Selected = false,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                mBrickManager.Visible = !mBrickManager.Visible;
                                item.Selected = mBrickManager.Visible;
                                if(mBrickManager.Visible)
                                {
                                    var task = mBrickManager.Initialize();
                                }
                            },
                        },
                    },
                },
                new EGui.UIProxy.MenuItemProxy()
                {
                    MenuName = "View",
                    IsTopMenuItem = true,
                    SubMenus = new List<EGui.UIProxy.IUIProxyBase>()
                    {
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "DisableShadow",
                            Selected = false,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                this.WorldViewportSlate.RenderPolicy.DisableShadow = !this.WorldViewportSlate.RenderPolicy.DisableShadow;
                                item.Selected = this.WorldViewportSlate.RenderPolicy.DisableShadow;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "DisableAO",
                            Selected = false,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                this.WorldViewportSlate.RenderPolicy.DisableAO = !this.WorldViewportSlate.RenderPolicy.DisableAO;
                                item.Selected = this.WorldViewportSlate.RenderPolicy.DisableAO;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "DisableHDR",
                            Selected = false,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                this.WorldViewportSlate.RenderPolicy.DisableHDR = !this.WorldViewportSlate.RenderPolicy.DisableHDR;
                                item.Selected = this.WorldViewportSlate.RenderPolicy.DisableHDR;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "DisablePointLight",
                            Selected = false,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                //var prop = this.WorldViewportSlate.RenderPolicy.GetType().GetProperty("DisablePointLight");
                                //if(prop !=null && prop.PropertyType==typeof(bool))
                                //{
                                //    bool value = (bool)prop.GetValue(this.WorldViewportSlate.RenderPolicy);
                                //    value = !value;
                                //    prop.SetValue(this.WorldViewportSlate.RenderPolicy, value);
                                //    item.CheckBox = value;
                                //}
                                this.WorldViewportSlate.RenderPolicy.DisablePointLight = !this.WorldViewportSlate.RenderPolicy.DisablePointLight;
                                item.Selected = this.WorldViewportSlate.RenderPolicy.DisablePointLight;
                            },
                        },
                        #region del code
                        //new EGui.UIProxy.MenuItemProxy()
                        //{
                        //    MenuName = "Forward",
                        //    Selected = false,
                        //    Action = async (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                        //    {
                        //        var saved = this.WorldViewportSlate.RenderPolicy;
                        //        //var policy = new Graphics.Pipeline.Mobile.UMobileEditorFSPolicy();                                
                        //        //await policy.Initialize(saved.DefaultCamera);
                        //        //policy.OnResize(this.WorldViewportSlate.ClientSize.X, this.WorldViewportSlate.ClientSize.Y);
                        //        Graphics.Pipeline.URenderPolicy policy = null;
                        //        var rpAsset = Bricks.RenderPolicyEditor.URenderPolicyAsset.LoadAsset(RName.GetRName("utest/forword.rpolicy"));
                        //        if (rpAsset != null)
                        //        {
                        //            policy = rpAsset.CreateRenderPolicy();
                        //        }
                        //        await policy.Initialize(saved.DefaultCamera);
                        //        policy.OnResize(this.WorldViewportSlate.ClientSize.X, this.WorldViewportSlate.ClientSize.Y);
                        //        policy.AddCamera("MainCamera", saved.DefaultCamera);
                        //        policy.SetDefaultCamera("MainCamera");

                        //        this.WorldViewportSlate.RenderPolicy = policy;
                        //        saved.Cleanup();

                        //        //policy.VoxelsNode.ResetDebugMeshNode(this.WorldViewportSlate.World);
                        //    },
                        //},
                        //new EGui.UIProxy.MenuItemProxy()
                        //{
                        //    MenuName = "Deferred",
                        //    Selected = false,
                        //    Action = async (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                        //    {
                        //        var saved = this.WorldViewportSlate.RenderPolicy;
                        //        //var policy = new Graphics.Pipeline.Deferred.UDeferredPolicy();
                        //        //await policy.Initialize(saved.DefaultCamera);
                        //        //policy.OnResize(this.WorldViewportSlate.ClientSize.X, this.WorldViewportSlate.ClientSize.Y);
                        //        Graphics.Pipeline.URenderPolicy policy = null;
                        //        var rpAsset = Bricks.RenderPolicyEditor.URenderPolicyAsset.LoadAsset(RName.GetRName("utest/deferred.rpolicy"));
                        //        if (rpAsset != null)
                        //        {
                        //            policy = rpAsset.CreateRenderPolicy();
                        //        }
                        //        await policy.Initialize(saved.DefaultCamera);
                        //        policy.OnResize(this.WorldViewportSlate.ClientSize.X, this.WorldViewportSlate.ClientSize.Y);
                        //        policy.AddCamera("MainCamera", saved.DefaultCamera);
                        //        policy.SetDefaultCamera("MainCamera");

                        //        this.WorldViewportSlate.RenderPolicy = policy;
                        //        saved.Cleanup();

                        //        //policy.VoxelsNode.ResetDebugMeshNode(this.WorldViewportSlate.World);
                        //    },
                        //},
                        #endregion
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "ShowLightDebugger",
                            Selected = UEngine.Instance.EditorInstance.Config.IsFilters(GamePlay.UWorld.UVisParameter.EVisCullFilter.LightDebug),
                            Action = async (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                if(UEngine.Instance.EditorInstance.Config.IsFilters(GamePlay.UWorld.UVisParameter.EVisCullFilter.LightDebug))
                                {
                                    UEngine.Instance.EditorInstance.Config.CullFilters &= ~GamePlay.UWorld.UVisParameter.EVisCullFilter.LightDebug;
                                }
                                else
                                {
                                    UEngine.Instance.EditorInstance.Config.CullFilters |= GamePlay.UWorld.UVisParameter.EVisCullFilter.LightDebug;
                                }

                                item.Selected = UEngine.Instance.EditorInstance.Config.IsFilters(GamePlay.UWorld.UVisParameter.EVisCullFilter.LightDebug);
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "ShowPxDebugger",
                            Selected = UEngine.Instance.EditorInstance.Config.IsFilters(GamePlay.UWorld.UVisParameter.EVisCullFilter.PhyxDebug),
                            Action = async (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                if(UEngine.Instance.EditorInstance.Config.IsFilters(GamePlay.UWorld.UVisParameter.EVisCullFilter.PhyxDebug))
                                {
                                    UEngine.Instance.EditorInstance.Config.CullFilters &= ~GamePlay.UWorld.UVisParameter.EVisCullFilter.PhyxDebug;
                                }
                                else
                                {
                                    UEngine.Instance.EditorInstance.Config.CullFilters |= GamePlay.UWorld.UVisParameter.EVisCullFilter.PhyxDebug;
                                }
                                item.Selected = UEngine.Instance.EditorInstance.Config.IsFilters(GamePlay.UWorld.UVisParameter.EVisCullFilter.PhyxDebug);
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "EditPolicy",
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                this.mMainInspector.PropertyGrid.Target = WorldViewportSlate.RenderPolicy;
                            },
                        },
                    },
                },
                new EGui.UIProxy.MenuItemProxy()
                {
                    MenuName = "Illumination",
                    IsTopMenuItem = true,
                    SubMenus = new List<EGui.UIProxy.IUIProxyBase>()
                    {
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "VoxelDebugger",
                            Selected = true,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                var vxNode = this.WorldViewportSlate.RenderPolicy.FindFirstNode<Bricks.VXGI.UVoxelsNode>();
                                if(vxNode!=null)
                                {
                                    vxNode.DebugVoxels = !vxNode.DebugVoxels;
                                    item.Selected = vxNode.DebugVoxels;
                                }
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "ResetVoxels",

                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                var vxNode = this.WorldViewportSlate.RenderPolicy.FindFirstNode<Bricks.VXGI.UVoxelsNode>();
                                if(vxNode!=null)
                                {
                                    vxNode.SetEraseBox(in vxNode.VxSceneBox);
                                }
                            },
                        },
                    },
                },
            };
        }

        #region DrawGui
        bool _showDemoWindow = false;
        public float LeftWidth = 0;
        public float CenterWidth = 0;
        public float RightWidth = 0;

        ////////////////////////
        UIWindowsTest mWinTest = new UIWindowsTest();
        ////////////////////////

        public uint LeftDockId { get; private set; } = 0;
        public uint CenterDockId { get; private set; } = 0;
        public uint RightDockId { get; private set; } = 0;

        public UInt32 ActiveViewportId = UInt32.MaxValue;
        protected unsafe override void OnDrawUI()
        {
            {
                var count = ImGuiAPI.PlatformIO_Viewports_Size(ImGuiAPI.GetPlatformIO());
                for (int i = 0; i < count; i++)
                {
                    var pViewport = ImGuiAPI.PlatformIO_Viewports_Get(ImGuiAPI.GetPlatformIO(), i);
                    var nativeWindow = pViewport->PlatformHandle;
                    var flags = SDL.SDL_GetWindowFlags((IntPtr)nativeWindow);
                    if ((flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) != 0)
                    {
                        ActiveViewportId = pViewport->ID;
                    }
                }
            }
            if (IsVisible == false)
            {
                AssetEditorManager.CloseAll();
                var num = ImGuiAPI.PlatformIO_Viewports_Size(ImGuiAPI.GetPlatformIO());
                if (num == 1)
                {//只剩下被特意隐藏的主Viewport了
                    UEngine.Instance.PostQuitMessage();
                }
                return;
            }

            if (LeftDockId == 0)
            {
                LeftDockId = ImGuiAPI.GetID("LeftDocker");
                CenterDockId = ImGuiAPI.GetID("CenterDocker");
                RightDockId = ImGuiAPI.GetID("RightDocker");

                mCpuProfiler.DockId = CenterDockId;
                mClrProfiler.DockId = CenterDockId;
                mMetaViewer.DockId = RightDockId;
                WorldViewportSlate.DockId = CenterDockId;
                mWorldOutliner.DockId = LeftDockId;
                mMainInspector.DockId = RightDockId;
                ContentBrowser.DockId = CenterDockId;
                mBrickManager.DockId = CenterDockId;
                mEditorSettings.DockId = LeftDockId;
            }

            var io = ImGuiAPI.GetIO();
            if ((io.ConfigFlags & ImGuiConfigFlags_.ImGuiConfigFlags_DockingEnable) != 0)
            {
                ImGuiAPI.DockSpaceOverViewport(ImGuiAPI.GetMainViewport(), ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, null);
                
                //var dockspace_id = ImGuiAPI.GetID("MyDockSpace");
                //ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                //var winClass = new ImGuiWindowClass();
                //winClass.UnsafeCallConstructor();
                //var sz = new Vector2(0.0f, 0.0f);                
                //ImGuiAPI.DockSpace(dockspace_id, ref sz, dockspace_flags, ref winClass);
                //winClass.UnsafeCallDestructor();
            }
            try
            {
                var mainPos = new Vector2(0);
                ImGuiAPI.SetNextWindowPos(in mainPos, ImGuiCond_.ImGuiCond_FirstUseEver, in mainPos);
                var wsz = new Vector2(1290, 800);
                ImGuiAPI.SetNextWindowSize(in wsz, ImGuiCond_.ImGuiCond_FirstUseEver);
                if (ImGuiAPI.Begin("T3D CoreEditor", ref IsVisible, //ImGuiWindowFlags_.ImGuiWindowFlags_NoMove |
                    //ImGuiWindowFlags_.ImGuiWindowFlags_NoResize |
                    ImGuiWindowFlags_.ImGuiWindowFlags_NoCollapse |
                    ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar))
                {
                    wsz = ImGuiAPI.GetWindowSize();
                    DrawMainMenu();
                    DrawToolBar();

                    ImGuiAPI.Separator();
                    ImGuiAPI.Columns(3, null, true);
                    if (LeftWidth == 0)
                    {
                        ImGuiAPI.SetColumnWidth(0, wsz.X * 0.15f);
                    }
                    var min = ImGuiAPI.GetWindowContentRegionMin();
                    var max = ImGuiAPI.GetWindowContentRegionMin();

                    DrawLeft(ref min, ref max);
                    LeftWidth = ImGuiAPI.GetColumnWidth(0);
                    ImGuiAPI.NextColumn();

                    if (RightWidth == 0)
                    {
                        CenterWidth = ImGuiAPI.GetColumnWidth(1);
                        RightWidth = ImGuiAPI.GetColumnWidth(2);
                        ImGuiAPI.SetColumnWidth(1, wsz.X * 0.70f);
                        //if (CenterWidth + RightWidth > 200)
                        //{
                        //    ImGuiAPI.SetColumnWidth(1, CenterWidth + RightWidth - 200);
                        //}
                    }
                    DrawCenter(ref min, ref max);
                    CenterWidth = ImGuiAPI.GetColumnWidth(1);
                    ImGuiAPI.NextColumn();

                    DrawRight(ref min, ref max);
                    RightWidth = ImGuiAPI.GetColumnWidth(2);
                    ImGuiAPI.NextColumn();

                    ImGuiAPI.Columns(1, null, true);
                }
                ImGuiAPI.End();

                WorldViewportSlate.DockId = CenterDockId;

                DrawRootForms();
                mWinTest.OnDraw();

                AssetEditorManager.OnDraw();
            }
            catch
            {

            }

            if (_showDemoWindow)
            {
                // Normally user code doesn't need/want to call this because positions are saved in .ini file anyway.
                // Here we just want to make the demo initial state a bit more friendly!
                var pos = new Vector2(650, 20);
                var pivot = new Vector2(0, 0);
                ImGuiAPI.SetNextWindowPos(in pos, ImGuiCond_.ImGuiCond_FirstUseEver, in pivot);
                ImGuiAPI.ShowDemoWindow(ref _showDemoWindow);
            }
        }
        List<EGui.UIProxy.MenuItemProxy> mMenuItems = new List<EGui.UIProxy.MenuItemProxy>();
        private void DrawMainMenu()
        {
            if (ImGuiAPI.BeginMenuBar())
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                for (int i = 0; i < mMenuItems.Count; i++)
                {
                    mMenuItems[i].OnDraw(in drawList, in Support.UAnyPointer.Default);
                }
                ImGuiAPI.EndMenuBar();
            }
            //if (ImGuiAPI.BeginMenuBar())
            //{
            //    if (ImGuiAPI.BeginMenu("File", true))
            //    {
            //        if (ImGuiAPI.MenuItem("Load", null, false, true))
            //        {
                        
            //        }
            //        if (ImGuiAPI.MenuItem("Save", null, false, true))
            //        {
                        
            //        }
            //        ImGuiAPI.EndMenu();
            //    }
            //    if (ImGuiAPI.BeginMenu("Windows", true))
            //    {
            //        var check = mWorldOutliner.Visible;
            //        ImGuiAPI.Checkbox("##mWorldOutliner", ref check);
            //        ImGuiAPI.SameLine(0, -1);
            //        if ( ImGuiAPI.MenuItem("WorldOutliner", null, false, true))
            //        {
            //            mWorldOutliner.Visible = !mWorldOutliner.Visible;
            //        }
            //        check = mCpuProfiler.Visible;
            //        ImGuiAPI.Checkbox("##mCpuProfiler", ref check);
            //        ImGuiAPI.SameLine(0, -1);
            //        if (ImGuiAPI.MenuItem("CpuProfiler", null, false, true))
            //        {
            //            mCpuProfiler.Visible = !mCpuProfiler.Visible;
            //        }
            //        check = mMainInspector.Visible;
            //        ImGuiAPI.Checkbox("##mMainInspector", ref check);
            //        ImGuiAPI.SameLine(0, -1);
            //        if (ImGuiAPI.MenuItem("MainInspector", null, false, true))
            //        {
            //            mMainInspector.Visible = !mMainInspector.Visible;
            //        }
            //        check = mMetaViewer.Visible;
            //        ImGuiAPI.Checkbox("##mMetaViewer", ref check);
            //        ImGuiAPI.SameLine(0, -1);
            //        if (ImGuiAPI.MenuItem("MetaViewer", null, false, true))
            //        {
            //            mMetaViewer.Visible = !mMetaViewer.Visible;
            //        }
            //        ImGuiAPI.EndMenu();
            //    }
            //    if (ImGuiAPI.BeginMenu("View", true))
            //    {
            //        var check = this.WorldViewportSlate.RenderPolicy.DisableShadow;
            //        ImGuiAPI.Checkbox("##DisableShadow", ref check);
            //        ImGuiAPI.SameLine(0, -1);
            //        if (ImGuiAPI.MenuItem("DisableShadow", null, false, true))
            //        {
            //            this.WorldViewportSlate.RenderPolicy.DisableShadow = !check;
            //        }

            //        check = this.WorldViewportSlate.RenderPolicy.DisableAO;
            //        ImGuiAPI.Checkbox("##DisableAO", ref check);
            //        ImGuiAPI.SameLine(0, -1);
            //        if (ImGuiAPI.MenuItem("DisableAO", null, false, true))
            //        {
            //            this.WorldViewportSlate.RenderPolicy.DisableAO = !check;
            //        }

            //        ImGuiAPI.EndMenu();
            //    }
            //    ImGuiAPI.EndMenuBar();
            //}
        }
        private unsafe void DrawToolBar()
        {
            var btSize = new Vector2(64, 64);
            ImGuiAPI.Button("New", in btSize);
            ImGuiAPI.SameLine(0, -1);
            ImGuiAPI.Button("Save", in btSize);
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.Button("Cap", in btSize))
            {
                IRenderDocTool.GetInstance().InitTool(UEngine.Instance.GfxDevice.RenderContext.mCoreObject);
                IRenderDocTool.GetInstance().SetActiveWindow(this.NativeWindow.HWindow.ToPointer());
                IRenderDocTool.GetInstance().TriggerCapture();
            }
        }
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            var size = new Vector2(-1, -1);
            if (ImGuiAPI.BeginChild("LeftWindow", in size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                var winClass = new ImGuiWindowClass();
                winClass.UnsafeCallConstructor();
                var sz = new Vector2(0.0f, 0.0f);
                ImGuiAPI.DockSpace(LeftDockId, in sz, dockspace_flags, in winClass);
                winClass.UnsafeCallDestructor();
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawCenter(ref Vector2 min, ref Vector2 max)
        {
            var size = new Vector2(-1, -1);
            if (ImGuiAPI.BeginChild("CenterWindow", in size, false, 
                ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                var sz = new Vector2(0.0f, 0.0f);
                //var winClass = new ImGuiWindowClass();
                //winClass.UnsafeCallConstructor();
                //ImGuiAPI.DockSpace(CenterDockId, ref sz, dockspace_flags, ref winClass);
                //winClass.UnsafeCallDestructor();

                ImGuiAPI.DockSpace(CenterDockId, &sz, dockspace_flags, (ImGuiWindowClass*)0);
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            var size = new Vector2(-1, -1);
            if (ImGuiAPI.BeginChild("RightWindow", in size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                var winClass = new ImGuiWindowClass();
                winClass.UnsafeCallConstructor();
                var sz = new Vector2(0.0f, 0.0f);
                ImGuiAPI.DockSpace(RightDockId, in sz, dockspace_flags, in winClass);
                winClass.UnsafeCallDestructor();
            }
            ImGuiAPI.EndChild();
        }
        #endregion

        #region Tick
        public void TickLogic(int ellapse)
        {
            WorldViewportSlate.TickLogic(ellapse);
        }
        public void TickRender(int ellapse)
        {
            WorldViewportSlate.TickRender(ellapse);
        }
        public void TickSync(int ellapse)
        {
            WorldViewportSlate.TickSync(ellapse);

            OnDrawSlate();
        }
        #endregion
    }
}
