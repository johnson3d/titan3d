using System;
using System.Collections.Generic;
using SDL2;

namespace EngineNS.Editor.Forms
{
    public class USceneEditor : Editor.IAssetEditor, ITickable, Graphics.Pipeline.IRootForm
    {
        public class USceneEditorViewport : EGui.Slate.UWorldViewportSlate
        {
            public EGui.Controls.PropertyGrid.PropertyGrid NodeInspector = new EGui.Controls.PropertyGrid.PropertyGrid();
            public override async System.Threading.Tasks.Task Initialize(Graphics.Pipeline.USlateApplication application, RName policyName, float zMin, float zMax)
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
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public GamePlay.Scene.UScene Scene;
        public USceneEditorViewport PreviewViewport = new USceneEditorViewport();
        public USceneEditorOutliner mWorldOutliner;
        
        public EGui.Controls.PropertyGrid.PropertyGrid ScenePropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        List<EGui.UIProxy.MenuItemProxy> mMenuItems = new List<EGui.UIProxy.MenuItemProxy>();
        public void InitMainMenu()
        {
            mMenuItems = new List<EGui.UIProxy.MenuItemProxy>()
            {
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
                },
            };
        }
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
        }
        public USceneEditor()
        {
            mWorldOutliner = new USceneEditorOutliner(PreviewViewport, false);
        }
        ~USceneEditor()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            Scene = null;
            PreviewViewport?.Cleanup();
            PreviewViewport = null;
            ScenePropGrid.Target = null;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await ScenePropGrid.Initialize();

            InitMainMenu();
            return true;
        }
        public Graphics.Pipeline.IRootForm GetRootForm()
        {
            return this;
        }
        protected async System.Threading.Tasks.Task Initialize_PreviewScene(Graphics.Pipeline.UViewportSlate viewport, Graphics.Pipeline.USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            //await viewport.RenderPolicy.Initialize(null);

            await viewport.World.InitWorld();

            (viewport as EGui.Slate.UWorldViewportSlate).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(viewport.World, viewport.World.Root);
            gridNode.ViewportSlate = this.PreviewViewport;
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

            await Scene.InitializeNode(PreviewViewport.World, Scene.NodeData, GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UPlacement));
            Scene.Parent = PreviewViewport.World.Root;

            ScenePropGrid.Target = Scene;

            mWorldOutliner.Title = $"Outliner:{name}";

            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public void OnCloseEditor()
        {
            //UEngine.Instance.EventProcessorManager.UnregProcessor(PreviewViewport);
            PreviewViewport.NodeInspector.Target = null;
            UEngine.Instance.TickableManager.RemoveTickable(this);
            Cleanup();
        }
        public float LeftWidth = 0;
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public unsafe void OnDraw()
        {
            if (Visible == false || Scene == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (ImGuiAPI.Begin(AssetName.Name, ref mVisible, 
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings |
                ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar))
            {
                DrawMainMenu();

                if (ImGuiAPI.IsWindowDocked())
                {
                    DockId = ImGuiAPI.GetWindowDockID();
                }
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
                ImGuiAPI.Columns(2, null, true);
                if (LeftWidth == 0)
                {
                    ImGuiAPI.SetColumnWidth(0, 300);
                }
                LeftWidth = ImGuiAPI.GetColumnWidth(0);
                var min = ImGuiAPI.GetWindowContentRegionMin();
                var max = ImGuiAPI.GetWindowContentRegionMin();

                DrawLeft(ref min, ref max);
                ImGuiAPI.NextColumn();

                DrawRight(ref min, ref max);
                ImGuiAPI.NextColumn();

                ImGuiAPI.Columns(1, null, true);
            }
            ImGuiAPI.End();
        }
        protected void DrawToolBar()
        {
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
            ImGuiAPI.SameLine(0, 15);
            ImGuiAPI.BeginGroup();
            for (int i = 0; i < (int)GamePlay.UWorld.UVisParameter.EVisCullFilter.FilterTypeCount; i++)
            {
                var type = (GamePlay.UWorld.UVisParameter.EVisCullFilter)(1 << i);
                ImGuiAPI.SameLine(0, -1);
                bool checkValue = (PreviewViewport.VisParameter.CullFilters & type) != 0;
                var name = type.ToString();
                if (name == "FilterTypeCount")
                {
                    name = GamePlay.UWorld.UVisParameter.FilterTypeCountAs;
                }
                if (EngineNS.EGui.UIProxy.CheckBox.DrawCheckBox(name, ref checkValue))
                {
                    if (checkValue)
                    {
                        PreviewViewport.VisParameter.CullFilters |= type;
                    }
                    else
                    {
                        PreviewViewport.VisParameter.CullFilters &= (~type);
                    }
                }
            }
            ImGuiAPI.EndGroup();
        }
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            var sz = new Vector2(-1);
            if (ImGuiAPI.BeginChild("LeftView", in sz, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.BeginTabBar("SceneTab", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
                {
                    if (ImGuiAPI.BeginTabItem("Scene", null, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                    {
                        if (ImGuiAPI.CollapsingHeader("Camera", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
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
                        if (ImGuiAPI.CollapsingHeader("Scene", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                        {
                            ScenePropGrid.OnDraw(true, false, false);
                        }
                        mWorldOutliner.DrawAsChildWindow(ref sz);

                        ImGuiAPI.EndTabItem();
                    }
                    if (ImGuiAPI.BeginTabItem("Node", null, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                    {
                        PreviewViewport.NodeInspector.OnDraw(true, false, false);
                        ImGuiAPI.EndTabItem();
                    }
                    ImGuiAPI.EndTabBar();
                }   
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            PreviewViewport.VieportType = Graphics.Pipeline.UViewportSlate.EVieportType.ChildWindow;
            PreviewViewport.OnDraw();
        }

        public void OnEvent(ref SDL.SDL_Event e)
        {
            //throw new NotImplementedException();
        }
        #region Tickable
        public void TickLogic(int ellapse)
        {
            PreviewViewport.TickLogic(ellapse);
        }
        public void TickRender(int ellapse)
        {
            
        }
        public void TickSync(int ellapse)
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