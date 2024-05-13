using System;
using System.Collections.Generic;
using EngineNS.Thread;
using MathNet.Numerics.LinearAlgebra.Solvers;

namespace EngineNS.Editor.Forms
{
    public class USceneEditor : Editor.IAssetEditor, ITickable, IRootForm
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public class USceneEditorViewport : EGui.Slate.UWorldViewportSlate
        {
            public EGui.Controls.PropertyGrid.PropertyGrid NodeInspector = new EGui.Controls.PropertyGrid.PropertyGrid();
            public override async System.Threading.Tasks.Task Initialize(USlateApplication application, RName policyName, float zMin, float zMax)
            {
                await base.Initialize(application, policyName, zMin, zMax);
                await NodeInspector.Initialize();
            }
            public override void OnHitproxySelected(Graphics.Pipeline.IProxiable proxy)
            {
                base.OnHitproxySelected(proxy);

                if (proxy == null)
                {
                    this.ShowBoundVolumes(false, null);
                    return;
                }
                var node = proxy as GamePlay.Scene.UNode;
                if (node != null)
                {
                    this.ShowBoundVolumes(true, node);
                }

                NodeInspector.Target = proxy;
            }
        }
        public class USceneEditorOutliner : UWorldOutliner
        {
            public USceneEditorOutliner(EGui.Slate.UWorldViewportSlate viewport, bool regRoot)
                : base(viewport, regRoot)
            {

            }            
            protected override void OnNodeUI_LClick(INodeUIProvider provider)
            {
                base.OnNodeUI_LClick(provider);
            }
        }
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public GamePlay.Scene.UScene Scene;
        public USceneEditorViewport PreviewViewport = new USceneEditorViewport();
        public USceneEditorOutliner mWorldOutliner;
        EGui.Controls.UContentBrowser mContentBrowser = new EGui.Controls.UContentBrowser();

        public EGui.Controls.PropertyGrid.PropertyGrid ScenePropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public EGui.Controls.PropertyGrid.PropertyGrid EditorPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public Graphics.Pipeline.URenderPolicy RenderPolicy { get => PreviewViewport.RenderPolicy; }

        bool mIsDrawing = true;
        bool IsDrawing
        {
            get => mIsDrawing;
            set
            {
                if(mIsDrawing && !value)
                {
                    var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                    mainEditor?.RemoveFromMainMenu(mMenuItems.ToArray());
                }
                else if(!mIsDrawing && value)
                {
                    var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                    mainEditor?.AppendToMainMenu(mMenuItems.ToArray());
                }
                mIsDrawing = value;
            }
        }
        List<EGui.UIProxy.MenuItemProxy> mMenuItems = new List<EGui.UIProxy.MenuItemProxy>();
        public void InitMainMenu()
        {
            var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
            EGui.UIProxy.MenuItemProxy menu0 = null;
            menu0 = new EGui.UIProxy.MenuItemProxy()
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
                                PreviewViewport.RenderPolicy.DisableShadow = !PreviewViewport.RenderPolicy.DisableShadow;
                                item.Selected = PreviewViewport.RenderPolicy.DisableShadow;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "DisableAO",
                            Selected = false,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                PreviewViewport.RenderPolicy.DisableAO = !PreviewViewport.RenderPolicy.DisableAO;
                                item.Selected = PreviewViewport.RenderPolicy.DisableAO;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "DisableHDR",
                            Selected = false,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            {
                                PreviewViewport.RenderPolicy.DisableHDR = !this.PreviewViewport.RenderPolicy.DisableHDR;
                                item.Selected = PreviewViewport.RenderPolicy.DisableHDR;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "TypeAA",
                            SubMenus = new List<EGui.UIProxy.IUIProxyBase>()
                            {
                                new EGui.UIProxy.MenuItemProxy()
                                {
                                    MenuName = "None",
                                    //Selected = PreviewViewport.RenderPolicy.TypeAA == Graphics.Pipeline.URenderPolicy.ETypeAA.None,
                                    Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                                    {
                                        PreviewViewport.RenderPolicy.TypeAA = Graphics.Pipeline.URenderPolicy.ETypeAA.None;
                                        item.Selected = PreviewViewport.RenderPolicy.TypeAA == Graphics.Pipeline.URenderPolicy.ETypeAA.None;
                                        var typeAA = EGui.UIProxy.MenuItemProxy.FindSubMenu(menu0, new string[]{"TypeAA" });
                                        if (typeAA != null)
                                        {
                                            var tm = typeAA.SubMenus[1] as EGui.UIProxy.MenuItemProxy;
                                            tm.Selected = false;
                                            tm = typeAA.SubMenus[2] as EGui.UIProxy.MenuItemProxy;
                                            tm.Selected = false;
                                        }
                                    },
                                },
                                new EGui.UIProxy.MenuItemProxy()
                                {
                                    MenuName = "FSAA",
                                    //Selected = PreviewViewport.RenderPolicy.TypeAA == Graphics.Pipeline.URenderPolicy.ETypeAA.Fsaa,
                                    Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                                    {
                                        PreviewViewport.RenderPolicy.TypeAA = Graphics.Pipeline.URenderPolicy.ETypeAA.Fsaa;
                                        item.Selected = PreviewViewport.RenderPolicy.TypeAA == Graphics.Pipeline.URenderPolicy.ETypeAA.Fsaa;
                                        var typeAA = EGui.UIProxy.MenuItemProxy.FindSubMenu(menu0, new string[]{"TypeAA" });
                                        if (typeAA != null)
                                        {
                                            var tm = typeAA.SubMenus[0] as EGui.UIProxy.MenuItemProxy;
                                            tm.Selected = false;
                                            tm = typeAA.SubMenus[2] as EGui.UIProxy.MenuItemProxy;
                                            tm.Selected = false;
                                        }
                                    },
                                },
                                new EGui.UIProxy.MenuItemProxy()
                                {
                                    MenuName = "TAA",
                                    //Selected = PreviewViewport.RenderPolicy.TypeAA == Graphics.Pipeline.URenderPolicy.ETypeAA.Taa,
                                    Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                                    {
                                        PreviewViewport.RenderPolicy.TypeAA = Graphics.Pipeline.URenderPolicy.ETypeAA.Taa;
                                        item.Selected = PreviewViewport.RenderPolicy.TypeAA == Graphics.Pipeline.URenderPolicy.ETypeAA.Taa;
                                        var typeAA = EGui.UIProxy.MenuItemProxy.FindSubMenu(menu0, new string[]{"TypeAA" });
                                        if (typeAA != null)
                                        {
                                            var tm = typeAA.SubMenus[0] as EGui.UIProxy.MenuItemProxy;
                                            tm.Selected = false;
                                            tm = typeAA.SubMenus[1] as EGui.UIProxy.MenuItemProxy;
                                            tm.Selected = false;
                                        }
                                    },
                                },
                            }
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
                                PreviewViewport.RenderPolicy.DisablePointLight = !PreviewViewport.RenderPolicy.DisablePointLight;
                                item.Selected = PreviewViewport.RenderPolicy.DisablePointLight;
                            },
                        },
                    },
            };
            var menu1 = new EGui.UIProxy.MenuItemProxy()
            {
                MenuName = "Illumination",
                IsTopMenuItem = true,
                SubMenus = new List<EGui.UIProxy.IUIProxyBase>()
                    {
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "VoxelDebugger",
                            Selected = true,
                            //Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            //{
                            //    var vxNode = this.WorldViewportSlate.RenderPolicy.FindFirstNode<Bricks.VXGI.UVoxelsNode>();
                            //    if(vxNode!=null)
                            //    {
                            //        vxNode.DebugVoxels = !vxNode.DebugVoxels;
                            //        item.Selected = vxNode.DebugVoxels;
                            //    }
                            //},
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "ResetVoxels",

                            //Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data)=>
                            //{
                            //    var vxNode = this.WorldViewportSlate.RenderPolicy.FindFirstNode<Bricks.VXGI.UVoxelsNode>();
                            //    if(vxNode!=null)
                            //    {
                            //        vxNode.SetEraseBox(in vxNode.VxSceneBox);
                            //    }
                            //},
                        },
                    },
            };
            mMenuItems.Clear();
            mMenuItems.Add(menu0);
            mMenuItems.Add(menu1);
            mainEditor.AppendToMainMenu(mMenuItems.ToArray());
        }
        public USceneEditor()
        {
            mWorldOutliner = new USceneEditorOutliner(PreviewViewport, false);
        }
        ~USceneEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            Scene = null;
            CoreSDK.DisposeObject(ref PreviewViewport);
            ScenePropGrid.Target = null;
            EditorPropGrid.Target = null;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await ScenePropGrid.Initialize();
            await EditorPropGrid.Initialize();

            mContentBrowser.DrawInWindow = false;
            await mContentBrowser.Initialize();

            InitMainMenu();
            return true;
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        protected async System.Threading.Tasks.Task Initialize_PreviewScene(Graphics.Pipeline.UViewportSlate viewport, USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            //await viewport.RenderPolicy.Initialize(null);

            await viewport.World.InitWorld();

            (viewport as EGui.Slate.UWorldViewportSlate).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(viewport.World, viewport.World.Root);
            gridNode.ViewportSlate = this.PreviewViewport;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public Graphics.Pipeline.TtCpuCullingNode CpuCullNode = null;
        public GamePlay.UWorld.UVisParameter.EVisCullFilter CullFilters 
        { 
            get
            {
                return CpuCullNode.VisParameter.CullFilters;
            }
            set
            {
                CpuCullNode.VisParameter.CullFilters = value;
            }
        }
        public async System.Threading.Tasks.Task<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            //PreviewViewport.PreviewAsset = name;
            PreviewViewport.Title = $"Scene:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.SlateApplication, UEngine.Instance.Config.MainRPolicyName, 0, 1);

            Scene = await UEngine.Instance.SceneManager.GetScene(PreviewViewport.World, name);
            if (Scene == null)
                return false;

            Scene.Parent = PreviewViewport.World.Root;

            ScenePropGrid.Target = Scene;
            EditorPropGrid.Target = this;

            mWorldOutliner.Title = $"Outliner:{name}";

            UEngine.Instance.TickableManager.AddTickable(this);

            CpuCullNode = PreviewViewport.RenderPolicy.FindNode<Graphics.Pipeline.TtCpuCullingNode>("CpuCulling");
            System.Diagnostics.Debug.Assert(CpuCullNode != null);
            return true;
        }
        public void OnCloseEditor()
        {
            //UEngine.Instance.EventProcessorManager.UnregProcessor(PreviewViewport);
            PreviewViewport.NodeInspector.Target = null;
            UEngine.Instance.TickableManager.RemoveTickable(this);
            Dispose();
        }
        bool mDockInitialized = false;
        protected void ResetDockspace(bool force = false)
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

            var rightId = id;
            uint middleId = 0;
            uint downId = 0;
            uint leftId = 0;
            uint rightUpId = 0;
            uint rightDownId = 0;
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir_.ImGuiDir_Left, 0.8f, ref middleId, ref rightId);
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir_.ImGuiDir_Down, 0.5f, ref rightDownId, ref rightUpId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir_.ImGuiDir_Down, 0.3f, ref downId, ref middleId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir_.ImGuiDir_Left, 0.2f, ref leftId, ref middleId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Details", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Editor Settings", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Camera Settings", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Outliner", mDockKeyClass), rightUpId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Preview", mDockKeyClass), middleId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Content Browser", mDockKeyClass), downId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Place Items", mDockKeyClass), leftId);

            ImGuiAPI.DockBuilderFinish(id);
        }
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public unsafe void OnDraw()
        {
            if (Visible == false || Scene == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            IsDrawing = EGui.UIProxy.DockProxy.BeginMainForm(AssetName.Name, this, ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
            if (IsDrawing)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
                //var sz = new Vector2(-1);
                //ImGuiAPI.BeginChild("Client", ref sz, false, ImGuiWindowFlags_.)
                ImGuiAPI.Separator();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm();

            DrawEditorSettings();
            DrawCameraSettings();
            DrawDetails();
            DrawOutliner();
            DrawPreview();
            DrawContentBrowser();
            DrawPlaceItemPanel();
        }
        protected void DrawToolBar()
        {
            var drawList = ImGuiAPI.GetWindowDrawList();
            EGui.UIProxy.Toolbar.BeginToolbar(drawList);
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                Scene.SaveAssetTo(AssetName);
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Snapshot", in btSize))
            {
                USnapshot.Save(AssetName, Scene.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV());
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Reload", in btSize))
            {
                if (Scene != null)
                    Scene.Parent = null;

                System.Action action = async () =>
                {
                    var saved = Scene;
                    Scene = await GamePlay.Scene.UScene.LoadScene(PreviewViewport.World, AssetName);
                    Scene.Parent = PreviewViewport.World.Root;

                    saved.Cleanup();
                };
                action();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Undo", in btSize))
            {

            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Redo", in btSize))
            {

            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Test", in btSize))
            {
                Scene.ClearChildren();
                var task = EngineNS.Editor.UMainEditorApplication.TestCreateScene(PreviewViewport, PreviewViewport.World, Scene);
            }
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList);
            //ImGuiAPI.BeginGroup();
            
            for (int i = 0; i < (int)GamePlay.UWorld.UVisParameter.EVisCullFilter.FilterTypeCount; i++)
            {
                var type = (GamePlay.UWorld.UVisParameter.EVisCullFilter)(1 << i);
                ImGuiAPI.SameLine(0, -1);
                bool checkValue = (CullFilters & type) != 0;
                var name = type.ToString();
                if (name == "FilterTypeCount")
                {
                    name = GamePlay.UWorld.UVisParameter.FilterTypeCountAs;
                }
                if(EGui.UIProxy.CustomButton.ToggleButton(name, in btSize, ref checkValue))
                {
                    if (checkValue)
                    {
                        CullFilters |= type;
                    }
                    else
                    {
                        CullFilters &= (~type);
                    }
                }
            }
            //ImGuiAPI.EndGroup();
            EGui.UIProxy.Toolbar.EndToolbar();
        }
        bool mDrawDetailsShow = true;
        protected void DrawDetails()
        {
            var sz = new Vector2(-1);
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Details", ref mDrawDetailsShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ScenePropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        bool mEditorSettingsShow = true;
        protected void DrawEditorSettings()
        {
            var sz = new Vector2(-1);
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Editor Settings", ref mEditorSettingsShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                EditorPropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        bool mCameraSettingsShow = true;
        protected void DrawCameraSettings()
        {
            var sz = new Vector2(-1);
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Camera Settings", ref mCameraSettingsShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                float v = PreviewViewport.CameraMoveSpeed;
                ImGuiAPI.SliderFloat("KeyMove", ref v, 1.0f, 150.0f, "%.3f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                PreviewViewport.CameraMoveSpeed = v;

                v = PreviewViewport.CameraMouseWheelSpeed;
                ImGuiAPI.InputFloat("WheelMove", ref v, 0.1f, 1.0f, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                PreviewViewport.CameraMouseWheelSpeed = v;

                var zN = PreviewViewport.CameraController.Camera.mCoreObject.mZNear;
                var zF = PreviewViewport.CameraController.Camera.mCoreObject.mZFar;
                ImGuiAPI.SliderFloat("ZNear", ref zN, 0.1f, 100.0f, "%.1f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                ImGuiAPI.SliderFloat("ZFar", ref zF, 10.0f, 10000.0f, "%.1f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                if (zN != PreviewViewport.CameraController.Camera.mCoreObject.mZNear ||
                    zF != PreviewViewport.CameraController.Camera.mCoreObject.mZFar)
                {
                    PreviewViewport.CameraController.Camera.SetZRange(zN, zF);
                }

                var camPos = PreviewViewport.CameraController.Camera.mCoreObject.GetPosition();
                var saved = camPos;
                ImGuiAPI.InputDouble($"X", ref camPos.X, 0.1, 10.0, "%.2f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                ImGuiAPI.InputDouble($"Y", ref camPos.Y, 0.1, 10.0, "%.2f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                ImGuiAPI.InputDouble($"Z", ref camPos.Z, 0.1, 10.0, "%.2f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                if (saved != camPos)
                {
                    var lookAt = PreviewViewport.CameraController.Camera.mCoreObject.GetLookAt();
                    var up = PreviewViewport.CameraController.Camera.mCoreObject.GetUp();
                    PreviewViewport.CameraController.Camera.mCoreObject.LookAtLH(in camPos, lookAt - saved + camPos, up);
                }
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        bool mOutlinerShow = true;
        protected void DrawOutliner()
        {
            var sz = new Vector2(-1);
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Outliner", ref mOutlinerShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                PreviewViewport.NodeInspector.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        bool mPreviewShow = true;
        protected unsafe void DrawPreview()
        {
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Preview", ref mPreviewShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                PreviewViewport.ViewportType = Graphics.Pipeline.UViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        bool mContentBrowserShow = true;
        protected unsafe void DrawContentBrowser()
        {
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Content Browser", ref mContentBrowserShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                mContentBrowser.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        bool mPlaceItemPanelShow = true;
        protected void DrawPlaceItemPanel()
        {
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Place Items", ref mPlaceItemPanelShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }

        public void OnEvent(in Bricks.Input.Event e)
        {
            //throw new NotImplementedException();
        }
        #region Tickable
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(USceneEditor), nameof(TickLogic));
        public void TickLogic(float ellapse)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                PreviewViewport.TickLogic(ellapse);
            }
        }
        public void TickRender(float ellapse)
        {
            
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {
            PreviewViewport.TickSync(ellapse);
        }
        #endregion
    }
}

namespace EngineNS.GamePlay.Scene
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.USceneEditor))]
    public partial class UScene
    {
    }

    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.USceneEditor))]
    public partial class UPartitionScene
    {
    }
}