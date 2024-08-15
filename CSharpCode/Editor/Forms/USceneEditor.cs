using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using EngineNS.EGui.Controls;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using EngineNS.Rtti;
using EngineNS.Thread;
using EngineNS.Thread.Async;
using MathNet.Numerics.LinearAlgebra.Solvers;

namespace EngineNS.Editor.Forms
{
    public class USceneEditorOutliner : UWorldOutliner
    {
        public USceneEditorOutliner(EGui.Slate.TtWorldViewportSlate viewport, bool regRoot)
            : base(viewport, regRoot)
        {

        }
    }
    public class USceneEditor : Editor.IAssetEditor, ITickable, IRootForm
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public class USceneEditorViewport : EGui.Slate.TtWorldViewportSlate
        {
            public USceneEditor HostEditor;
            
            public override void OnHitproxySelected(Graphics.Pipeline.IProxiable proxy)
            {
                if(TtEngine.Instance.InputSystem.IsCtrlKeyDown())
                {
                    if (proxy.Selected)
                        OnHitproxyUnSelectedMulti(proxy);
                    else
                        OnHitproxySelectedMulti(false, proxy);
                }
                else
                {
                    OnHitproxySelectedMulti(true, proxy);
                }

                //var edtorPolicy = this.RenderPolicy as Graphics.Pipeline.URenderPolicy;
                //if (edtorPolicy != null)
                //{
                //    if (proxy == null)
                //    {
                //        //if (this.IsHoverGuiItem == false)
                //        edtorPolicy.PickedProxiableManager.ClearSelected();
                //    }
                //    else
                //    {
                //        edtorPolicy.PickedProxiableManager.Selected(proxy);
                //    }
                //}

                //var node = proxy as GamePlay.Scene.UNode;
                //mAxis.SetSelectedNodes(node);


                //if (proxy == null)
                //{
                //    this.ShowBoundVolumes(true, false, null);
                //    //return;
                //}
                //var node = proxy as GamePlay.Scene.UNode;
                //if (node != null)
                //{
                //    this.ShowBoundVolumes(true, true, node);
                //}

                //if(HostEditor.mWorldOutliner.SelectedNodes.Contains)
                //NodeInspector.Target = HostEditor.mWorldOutliner.SelectedNodes;// proxy;
            }
            public override void OnHitproxyUnSelectedMulti(params IProxiable[] proxies)
            {
                base.OnHitproxyUnSelectedMulti(proxies);

                foreach(var prox in proxies)
                {
                    var uNode = prox as UNode;
                    if (uNode == null)
                        continue;
                    uNode.Selected = false;
                    HostEditor.mWorldOutliner.SelectedNodes.Remove(uNode);
                }
            }
            public override void OnHitproxySelectedMulti(bool clearPre, params IProxiable[] proxies)
            {
                base.OnHitproxySelectedMulti(clearPre, proxies);

                if (proxies == null || proxies.Length == 0)
                    ShowBoundVolumes(true, false, null);
                else
                {
                    ShowBoundVolumes(true, false, null);
                    for(int i=0; i<proxies.Length; i++)
                    {
                        var node = proxies[i] as UNode;
                        if (node == null)
                            continue;
                        if (node.Selected)
                            continue;

                        this.ShowBoundVolumes(false, true, node);
                    }
                }

                if(clearPre)
                {
                    for(int i=0; i<HostEditor.mWorldOutliner.SelectedNodes.Count; i++)
                    {
                        HostEditor.mWorldOutliner.SelectedNodes[i].Selected = false;
                    }
                    HostEditor.mWorldOutliner.SelectedNodes.Clear();
                    for(int i=0; i<proxies.Length; i++)
                    {
                        var uNode = proxies[i] as UNode;
                        if (uNode == null)
                            continue;
                        uNode.Selected = true;
                        HostEditor.mWorldOutliner.SelectedNodes.Add(uNode);
                    }
                }
                else
                {
                    List<UNode> needAddNodes = new List<UNode>();
                    foreach(var i in proxies)
                    {
                        var n = i as UNode;
                        if (n == null)
                            continue;
                        bool bFind = false;
                        foreach(var j in HostEditor.mWorldOutliner.SelectedNodes)
                        {
                            if (n == j)
                            {
                                bFind = true;
                                break;
                            }
                        }
                        if (bFind == false)
                        {
                            needAddNodes.Add(n);
                        }
                    }
                    var needDelNodes = new List<UNode>();
                    foreach (var i in HostEditor.mWorldOutliner.SelectedNodes)
                    {
                        bool bFind = false;
                        foreach (var j in proxies)
                        {
                            var n = j as UNode;
                            if (n == null)
                                continue;
                            if (n == j)
                            {
                                bFind = true;
                                break;
                            }
                        }
                        if (bFind == false)
                        {
                            needDelNodes.Add(i);
                        }
                    }
                    //var needDelNodes = HostEditor.mWorldOutliner.SelectedNodes.Except(proxies);
                    //System.Diagnostics.Debug.Assert(HostEditor.mWorldOutliner.SelectedNodes != needDelNodes);
                    
                    foreach (var node in needDelNodes)
                    {
                        var uNode = node as UNode;
                        if (uNode == null)
                            continue;
                        uNode.Selected = false;
                        HostEditor.mWorldOutliner.SelectedNodes.Remove(uNode);
                    }
                    foreach(var node in needAddNodes)
                    {
                        var uNode = node as UNode;
                        if (uNode == null)
                            continue;
                        uNode.Selected = true;
                        HostEditor.mWorldOutliner.SelectedNodes.Add(uNode);
                    }
                    //foreach(var node in proxies)
                    //{
                    //    var uNode = node as UNode;
                    //    if (uNode == null)
                    //        continue;
                    //    uNode.Selected = true;
                    //    HostEditor.mWorldOutliner.SelectedNodes.Add(uNode);
                    //}
                }
                HostEditor.NodeInspector.Target = HostEditor.mWorldOutliner.SelectedNodes; //proxies;
            }
        }
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public virtual IO.IAsset GetAsset()
        {
            return Scene;
        }
        public GamePlay.Scene.UScene Scene;
        public USceneEditorViewport PreviewViewport = new USceneEditorViewport();
        public UWorldOutliner mWorldOutliner;
        EGui.Controls.UContentBrowser mContentBrowser = new EGui.Controls.UContentBrowser();

        public EGui.Controls.PropertyGrid.PropertyGrid NodeInspector = new EGui.Controls.PropertyGrid.PropertyGrid();
        public EGui.Controls.PropertyGrid.PropertyGrid ScenePropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public EGui.Controls.PropertyGrid.PropertyGrid EditorPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public Graphics.Pipeline.TtRenderPolicy RenderPolicy { get => PreviewViewport.RenderPolicy; }

        bool mIsDrawing = false;
        bool IsDrawing
        {
            get => mIsDrawing;
            set
            {
                if(mIsDrawing && !value)
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                    mainEditor?.RemoveFromMainMenu(mMenuItems);
                }
                else if(!mIsDrawing && value)
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                    mainEditor?.AppendToMainMenu(mMenuItems);
                }
                mIsDrawing = value;
            }
        }
        List<EGui.UIProxy.MenuItemProxy> mMenuItems = new List<EGui.UIProxy.MenuItemProxy>();
        public void InitMainMenu()
        {
            var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
            mMenuItems.Clear();
            mMenuItems.Add(new EGui.UIProxy.MenuItemProxy()
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
                                        PreviewViewport.RenderPolicy.TypeAA = Graphics.Pipeline.TtRenderPolicy.ETypeAA.None;
                                        item.Selected = PreviewViewport.RenderPolicy.TypeAA == Graphics.Pipeline.TtRenderPolicy.ETypeAA.None;
                                        var typeAA = EGui.UIProxy.MenuItemProxy.FindSubMenu(mMenuItems[0], new string[]{"TypeAA" });
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
                                        PreviewViewport.RenderPolicy.TypeAA = Graphics.Pipeline.TtRenderPolicy.ETypeAA.Fsaa;
                                        item.Selected = PreviewViewport.RenderPolicy.TypeAA == Graphics.Pipeline.TtRenderPolicy.ETypeAA.Fsaa;
                                        var typeAA = EGui.UIProxy.MenuItemProxy.FindSubMenu(mMenuItems[0], new string[]{"TypeAA" });
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
                                        PreviewViewport.RenderPolicy.TypeAA = Graphics.Pipeline.TtRenderPolicy.ETypeAA.Taa;
                                        item.Selected = PreviewViewport.RenderPolicy.TypeAA == Graphics.Pipeline.TtRenderPolicy.ETypeAA.Taa;
                                        var typeAA = EGui.UIProxy.MenuItemProxy.FindSubMenu(mMenuItems[0], new string[]{"TypeAA" });
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
            });
            mMenuItems.Add(new EGui.UIProxy.MenuItemProxy()
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
            });
            mMenuItems.Add(new EGui.UIProxy.MenuItemProxy()
            {
                MenuName = "Windows",
                IsTopMenuItem = true,
                SubMenus = new List<EGui.UIProxy.IUIProxyBase>()
                {
                    mDrawSceneDetailsShow,
                    mDrawNodeDetailsShow,
                    mEditorSettingsShow,
                    mCameraSettingsShow,
                    mOutlinerShow,
                    mPreviewShow,
                    mContentBrowserShow,
                    mPlaceItemPanelShow,
                }
            });
            //mainEditor.AppendToMainMenu(mMenuItems.ToArray());
        }

        protected class PlaceItemData
        {
            public string Name;
            public string Description;
            public string Icon;
            public Action<PlaceItemData> DropAction;

            public PlaceItemData Parent;
            public List<PlaceItemData> Children = new List<PlaceItemData>();
        }
        List<PlaceItemData> mPlaceItems = new List<PlaceItemData>();
        void InitPlaceItems()
        {
            mPlaceItems.Clear();
            var shapesItem = new PlaceItemData()
            {
                Name = "Shapes",
            };
            mPlaceItems.Add(shapesItem);
            var planeItem = new PlaceItemData()
            {
                Name = "Plane",
                Parent = shapesItem,
                DropAction = async (data)=>
                {
                    var gridNode = UMeshDataProvider.MakeGridPlane(TtEngine.Instance.GfxDevice.RenderContext, new Vector2(-50, -50), new Vector2(50, 50), 1).ToMesh();

                    var meshNodeData = new UMeshNode.UMeshNodeData();
                    var meshNode = await Scene.NewNode(Scene.World, typeof(UMeshNode), meshNodeData, EBoundVolumeType.Box, typeof(UPlacement)) as UMeshNode;

                },
            };
            shapesItem.Children.Add(planeItem);

        }
        public USceneEditor()
        {
            mWorldOutliner = new USceneEditorOutliner(PreviewViewport, false);
            PreviewViewport.HostEditor = this;
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
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await ScenePropGrid.Initialize();
            await EditorPropGrid.Initialize();
            await NodeInspector.Initialize();

            mContentBrowser.DrawInWindow = false;
            await mContentBrowser.Initialize();

            await mWorldOutliner.Initialize();

            InitMainMenu();
            InitPlaceItems();
            return true;
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        protected async System.Threading.Tasks.Task Initialize_PreviewScene(Graphics.Pipeline.TtViewportSlate viewport, USlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            //await viewport.RenderPolicy.Initialize(null);

            (viewport as EGui.Slate.TtWorldViewportSlate).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

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
        public async virtual Thread.Async.TtTask<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            //PreviewViewport.PreviewAsset = name;
            PreviewViewport.Title = $"Scene:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, TtEngine.Instance.Config.MainRPolicyName, 0, 1);
            var camPos = new DVector3(10, 10, 10);
            PreviewViewport.CameraController.Camera.mCoreObject.LookAtLH(in camPos, in DVector3.Zero, in Vector3.Up);

            Scene = await TtEngine.Instance.SceneManager.CreateScene(PreviewViewport.World, name);
            if (Scene == null)
                return false;

            PreviewViewport.Axis.RootNode.Parent = Scene;
            PreviewViewport.World.Root = Scene;

            var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(Scene.World, Scene);
            gridNode.ViewportSlate = this.PreviewViewport;

            ScenePropGrid.Target = Scene;
            EditorPropGrid.Target = this;

            mWorldOutliner.Title = $"Outliner:{name}";

            TtEngine.Instance.TickableManager.AddTickable(this);

            CpuCullNode = PreviewViewport.RenderPolicy.FindNode<Graphics.Pipeline.TtCpuCullingNode>("CpuCulling");
            System.Diagnostics.Debug.Assert(CpuCullNode != null);
            return true;
        }
        public virtual void OnCloseEditor()
        {
            //TtEngine.Instance.EventProcessorManager.UnregProcessor(PreviewViewport);
            NodeInspector.Target = null;
            TtEngine.Instance.TickableManager.RemoveTickable(this);
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

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("SceneDetails", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("NodeDetails", mDockKeyClass), rightDownId);
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
        public virtual bool IsAssetLoaed { get => Scene != null; }
        public unsafe void OnDraw()
        {
            if (Visible == false || IsAssetLoaed == false)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            IsDrawing = EGui.UIProxy.DockProxy.BeginMainForm(AssetName.Name, this, ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
            if (IsDrawing)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
                //var sz = new Vector2(-1);
                //ImGuiAPI.BeginChild("Client", ref sz, false, ImGuiWindowFlags_.)
                ImGuiAPI.Separator();

                //if (ImGuiAPI.IsWindowHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_ChildWindows))
                {
                    if (TtEngine.Instance.InputSystem.IsKeyPressed(Bricks.Input.Keycode.KEY_f))
                    {
                        DBoundingBox box = DBoundingBox.EmptyBox();
                        for (int i = 0; i < mWorldOutliner.SelectedNodes.Count; i++)
                        {
                            var transform = mWorldOutliner.SelectedNodes[i].Placement.AbsTransform;
                            var corners = mWorldOutliner.SelectedNodes[i].AABB.GetCorners();
                            for (int cornerIdx = 0; cornerIdx < corners.Length; cornerIdx++)
                            {
                                var absPos = transform.TransformPosition(in corners[cornerIdx]);
                                box.Merge(in absPos);
                            }
                        }
                        if (!box.IsEmpty())
                        {
                            DBoundingSphere sphere = new DBoundingSphere(box.GetCenter(), (float)box.GetMaxSide());
                            PreviewViewport.CameraController.Camera.AutoZoom(in sphere, 0.2f);
                        }
                    }
                }
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(IsDrawing);

            DrawEditorSettings();
            DrawCameraSettings();
            DrawSceneDetails();
            DrawNodeDetails();
            DrawOutliner();
            DrawPreview();
            DrawContentBrowser();
            DrawPlaceItemPanel();
        }
        protected virtual void Save()
        {
            this.GetAsset().SaveAssetTo(AssetName);
        }
        protected virtual void Snapshot()
        {
            USnapshot.Save(AssetName, GetAsset().GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV());
        }
        protected virtual void Reload()
        {
            if (Scene != null)
                Scene.Parent = null;

            System.Action action = async () =>
            {
                var saved = Scene;
                Scene = await TtEngine.Instance.SceneManager.CreateScene(PreviewViewport.World, AssetName);
                Scene.Parent = PreviewViewport.World.Root;

                saved.Cleanup();
            };
            action();
        }
        protected virtual void DrawToolBar()
        {
            var drawList = ImGuiAPI.GetWindowDrawList();
            EGui.UIProxy.Toolbar.BeginToolbar(drawList);
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                Save();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Snapshot", in btSize))
            {
                Snapshot();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Reload", in btSize))
            {
                Reload();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Undo", in btSize))
            {

            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Redo", in btSize))
            {

            }
            //ImGuiAPI.SameLine(0, -1);
            //if (EGui.UIProxy.CustomButton.ToolButton("Test", in btSize))
            //{
            //    Scene.ClearChildren();
            //    var task = EngineNS.Editor.UMainEditorApplication.TestCreateScene(PreviewViewport, PreviewViewport.World, Scene);
            //}
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
        EGui.UIProxy.MenuItemProxy mDrawSceneDetailsShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "SceneDetails",
            Selected = false,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data) =>
            {
                item.Selected = !item.Selected;
            },
        };
        EGui.UIProxy.MenuItemProxy mDrawNodeDetailsShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "NodeDetails",
            Selected = true,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data) =>
            {
                item.Selected = !item.Selected;
            },
        };
        EGui.UIProxy.MenuItemProxy mEditorSettingsShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "Editor Settings",
            Selected = false,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data) =>
            {
                item.Selected = !item.Selected;
            },
        };
        EGui.UIProxy.MenuItemProxy mCameraSettingsShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "Camera Settings",
            Selected = false,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data) =>
            {
                item.Selected = !item.Selected;
            },
        };
        EGui.UIProxy.MenuItemProxy mOutlinerShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "Outliner",
            Selected = true,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data) =>
            {
                item.Selected = !item.Selected;
            },
        };
        EGui.UIProxy.MenuItemProxy mPreviewShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "Preview",
            Selected = true,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data) =>
            {
                item.Selected = !item.Selected;
            },
        };
        EGui.UIProxy.MenuItemProxy mContentBrowserShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "Content Browser",
            Selected = true,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data) =>
            {
                item.Selected = !item.Selected;
            },
        };
        EGui.UIProxy.MenuItemProxy mPlaceItemPanelShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "Place Items",
            Selected = false,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.UAnyPointer data) =>
            {
                item.Selected = !item.Selected;
            },
        };
        protected void DrawSceneDetails()
        {
            if (!mDrawSceneDetailsShow.Selected)
                return;
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "SceneDetails", ref mDrawSceneDetailsShow.Selected, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                ScenePropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        protected void DrawNodeDetails()
        {
            if (!mDrawNodeDetailsShow.Selected)
                return;
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "NodeDetails", ref mDrawNodeDetailsShow.Selected, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                NodeInspector.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        protected void DrawEditorSettings()
        {
            if (!mEditorSettingsShow.Selected)
                return;
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Editor Settings", ref mEditorSettingsShow.Selected, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                EditorPropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        protected void DrawCameraSettings()
        {
            if (!mCameraSettingsShow.Selected)
                return;
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Camera Settings", ref mCameraSettingsShow.Selected, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
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
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        protected void DrawOutliner()
        {
            if (!mOutlinerShow.Selected)
                return;
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Outliner", ref mOutlinerShow.Selected, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                mWorldOutliner.DrawAsChildWindow(in sz);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        unsafe void ContentBrowserDragDropPreview()
        {
            if (UContentBrowser.IsInDragDropMode)
            {
                var payload = ImGuiAPI.GetDragDropPayload();
                if (payload == null)
                    return;
                if (!payload->IsDataType("ContentBrowserAssetDragDrop"))
                    return;

                var pos = ImGuiAPI.GetWindowPos();
                var min = ImGuiAPI.GetWindowContentRegionMin() + pos;
                var max = ImGuiAPI.GetWindowContentRegionMax() + pos;
                var draggingInViewport = ImGuiAPI.IsMouseHoveringRect(in min, in max, true);
                var handle = GCHandle.FromIntPtr((IntPtr)(payload->Data));
                var dragData = (UContentBrowser.DragDropData)handle.Target;
                for (int i = 0; i < dragData.Metas.Length; i++)
                {
                    dragData.Metas[i].DraggingInViewport = draggingInViewport;
                    if(draggingInViewport)
                        _ = dragData.Metas[i].OnDragging(PreviewViewport);
                }
            }
        }
        protected unsafe void DrawPreview()
        {
            if (!mPreviewShow.Selected)
                return;
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Preview", ref mPreviewShow.Selected, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                PreviewViewport.ViewportType = Graphics.Pipeline.TtViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();


                ContentBrowserDragDropPreview();

                // dragdrop
                if (ImGuiAPI.BeginDragDropTarget())
                {
                    var payload = ImGuiAPI.AcceptDragDropPayload("ScenePlaneItemDragDrop", ImGuiDragDropFlags_.ImGuiDragDropFlags_None);
                    if (payload != null)
                    {
                        var handle = GCHandle.FromIntPtr((IntPtr)(payload->Data));
                        var dragData = (PlaceItemData)handle.Target;
                        if(dragData != null)
                        {
                            dragData.DropAction?.Invoke(dragData);
                        }
                    }
                    payload = ImGuiAPI.AcceptDragDropPayload("ContentBrowserAssetDragDrop", ImGuiDragDropFlags_.ImGuiDragDropFlags_None);
                    if(payload != null)
                    {
                        var handle = GCHandle.FromIntPtr((IntPtr)(payload->Data));
                        var dragData = (UContentBrowser.DragDropData)handle.Target;
                        for(int i=0; i<dragData.Metas.Length; i++)
                        {
                            _ = dragData.Metas[i].OnDragTo(PreviewViewport);
                        }
                    }
                    ImGuiAPI.EndDragDropTarget();
                }
            }
            this.PreviewViewport.Visible = show;
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        protected unsafe void DrawContentBrowser()
        {
            if (!mContentBrowserShow.Selected)
                return;
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Content Browser", ref mContentBrowserShow.Selected, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                mContentBrowser.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        internal string mSelectedPlaceItemName = null;
        protected void DrawPlaceItemPanel()
        {
            if (!mPlaceItemPanelShow.Selected)
                return;
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Place Items", ref mPlaceItemPanelShow.Selected, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                //if(ImGuiAPI.TreeNodeEx("Shapes", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth))
                //{
                //    var flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;
                //    if (mSelectedPlaceItemName == "Plane")
                //        flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
                //    if(ImGuiAPI.TreeNodeEx("Plane", flags))
                //    {
                //        if(ImGuiAPI.IsItemActivated())
                //        {
                //            mSelectedPlaceItemName = "Plane";
                //        }
                //    }
                //    ImGuiAPI.BeginDragDropSource
                //}
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
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

    public class TtPrefabEditorOutliner : UWorldOutliner
    {
        public TtPrefabEditorOutliner(EGui.Slate.TtWorldViewportSlate viewport, bool regRoot)
            : base(viewport, regRoot)
        {

        }
        protected override void DrawBaseMenu(GamePlay.Scene.UNode node)
        {
            base.DrawBaseMenu(node);
        }
        public override unsafe void DrawAsChildWindow(in Vector2 size)
        {
            base.DrawAsChildWindow(in size);
        }
    }
    public class TtPrefabEditor : USceneEditor
    {
        public GamePlay.Scene.TtPrefab Prefab;
        public override IO.IAsset GetAsset()
        {
            return Prefab;
        }
        public TtPrefabEditor()
        {
            mWorldOutliner = new TtPrefabEditorOutliner(PreviewViewport, false);
        }
        public override bool IsAssetLoaed { get => Prefab != null; }
        public async override Thread.Async.TtTask<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            //PreviewViewport.PreviewAsset = name;
            PreviewViewport.Title = $"Prefab:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, TtEngine.Instance.Config.MainRPolicyName, 0, 1);

            Prefab = await TtEngine.Instance.PrefabManager.CreatePrefab(name);
            if (Prefab == null)
                return false;

            Prefab.Root.Parent = PreviewViewport.World.Root;

            ScenePropGrid.Target = Prefab;
            EditorPropGrid.Target = this;

            mWorldOutliner.Title = $"Outliner:{name}";

            TtEngine.Instance.TickableManager.AddTickable(this);

            CpuCullNode = PreviewViewport.RenderPolicy.FindNode<Graphics.Pipeline.TtCpuCullingNode>("CpuCulling");
            System.Diagnostics.Debug.Assert(CpuCullNode != null);
            return true;
        }

        protected override void Save()
        {
            Prefab.SaveAssetTo(AssetName);
            _ = TtEngine.Instance.PrefabManager.ReloadPrefab(AssetName);
        }
        protected override void Reload()
        {
            if (Prefab != null)
                Prefab.Root.Parent = null;

            System.Action action = async () =>
            {
                var saved = Prefab;
                Prefab = await TtEngine.Instance.PrefabManager.CreatePrefab(AssetName);
                Prefab.Root.Parent = PreviewViewport.World.Root;
            };
            action();
        }
    }
}

namespace EngineNS.GamePlay.Scene
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.USceneEditor))]
    public partial class UScene
    {
    }

    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.TtPrefabEditor))]
    public partial class TtPrefab
    {
    }
}