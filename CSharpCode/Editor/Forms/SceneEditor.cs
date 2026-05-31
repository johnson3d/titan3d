using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace EngineNS.Editor.Forms
{
    public class TtSceneEditorOutliner : TtWorldOutliner
    {
        public TtSceneEditorOutliner(EGui.Slate.TtWorldViewportSlate viewport, bool regRoot)
            : base(viewport, regRoot)
        {

        }
    }
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtSceneEditor : Editor.IAssetEditor, ITickable, IRootForm
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public class TtSceneEditorViewport : EGui.Slate.TtWorldViewportSlate
        {
            static TtSceneEditorViewport()
            {
                // 注册 SceneEditor 专用的交互模式。
                // TtWorldViewportSlate.Initialize 末尾会调用 ReCreateInteractiveModes(),
                // QueryModeInfo 沿类型继承链收集所有注册的 mode, 这里注册的 mode
                // 会自动被实例化并设为 CurrentIntercativeMode。
                TtEngine.Instance.InteractiveModeManager
                    .RegisterMode<TtSceneEditorViewport, TtSceneEditorInteractiveMode>();
            }

            public TtSceneEditor HostEditor;

            public TtSceneEditorViewport()
            {
                // SceneEditor 把 InteractiveMode 切换 Combo 画在自己的工具栏上
                // (TtSceneEditor.DrawInteractiveModeCombo), 所以禁用基类
                // 在视口 UI 区自动画的同款 Combo, 避免出现两个重复的下拉框。
                ShowInteractiveModeCombo = false;
            }

            public override void OnHitproxySelected(Graphics.Pipeline.IProxiable proxy)
            {
                // 点击视口空白处 (proxy == null) 时, 拾取没命中任何对象。
                // 通行编辑器约定 (Unity / Unreal / Godot) 是: 空白处单击属于易误触操作,
                // 不应清空 outliner 里的当前选择, 否则用户在 outliner 里选了一个 node、
                // 不小心在视口空白点一下, 选中和 NodeDetail (NodeInspector.Target) 都会
                // 跟着丢失, 体验很差。
                //
                // 历史 bug: 这里直接把 null 传给后续的 OnHitproxySelectedMulti(clearPre=true, null)
                // 时, proxies 是 [null] 而不是空数组, 长度判 == 0 不成立 -> 走 else 分支
                // -> clearPre 把 mWorldOutliner.SelectedNodes 全部清掉 -> NodeInspector.Target
                // 也被赋成空列表, 这就是用户看到的 "点空白选中丢失" 的现象。
                //
                // 另外 Ctrl 路径下原代码会直接访问 proxy.Selected, proxy 为 null 时 NRE。
                // 在这里统一早返回, 保持当前选择不变。如果后续要加 "空白点击取消选择"
                // 的语义, 应另走显式路径 (Esc / 右键菜单), 不要再绑回易误触的左键空白点。
                if (proxy == null)
                    return;

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
                    var uNode = prox as TtNode;
                    if (uNode == null)
                        continue;
                    uNode.Selected = false;
                    HostEditor.mWorldOutliner.SelectedNodes.Remove(uNode);
                }
                Axis?.SetSelectedNodes(HostEditor.mWorldOutliner.SelectedNodes);
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
                        var node = proxies[i] as TtNode;
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
                        var uNode = proxies[i] as TtNode;
                        if (uNode == null)
                            continue;
                        uNode.Selected = true;
                        HostEditor.mWorldOutliner.SelectedNodes.Add(uNode);
                    }
                }
                else
                {
                    List<TtNode> needAddNodes = new List<TtNode>();
                    foreach(var i in proxies)
                    {
                        var n = i as TtNode;
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
                    var needDelNodes = new List<TtNode>();
                    foreach (var i in HostEditor.mWorldOutliner.SelectedNodes)
                    {
                        bool bFind = false;
                        foreach (var j in proxies)
                        {
                            var n = j as TtNode;
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
                        var uNode = node as TtNode;
                        if (uNode == null)
                            continue;
                        uNode.Selected = false;
                        HostEditor.mWorldOutliner.SelectedNodes.Remove(uNode);
                    }
                    foreach(var node in needAddNodes)
                    {
                        var uNode = node as TtNode;
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
                Axis?.SetSelectedNodes(HostEditor.mWorldOutliner.SelectedNodes);
            }

            // 视口里按 Esc 清空选择 (Unity / Godot / Blender 通行约定)。
            // 同时清渲染层 PickedProxiableManager 让高亮也跟着去掉。
            // 走 HostEditor.DeselectAll 统一入口, 避免和 outliner 那侧的清空逻辑漂移。
            public override bool OnEvent(in Bricks.Input.Event e)
            {
                if (e.Type == Bricks.Input.EventType.KEYDOWN &&
                    e.Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_ESCAPE &&
                    e.Keyboard.Repeat == 0 &&
                    IsViewportSlateFocused)
                {
                    HostEditor?.DeselectAll();
                    return true;
                }
                return base.OnEvent(in e);
            }
        }

        // 统一的"清空所有选择"入口: 同时清理
        //   1) outliner 选中列表 (mWorldOutliner.SelectedNodes) + 节点的 Selected 标记
        //   2) 渲染层高亮 (PickedProxiableManager.ClearSelected, 这样视口里的描边/Bound 也消失)
        //   3) 节点详情面板 (NodeInspector.Target = null, 否则面板还指着已经"逻辑取消选中"的旧列表)
        // 触发入口目前有两个: Esc 键 (TtSceneEditorViewport.OnEvent) 与 Outliner 树空白处单击
        // (TtWorldOutliner.DrawAsChildWindow / OnDraw)。
        public void DeselectAll()
        {
            if (mWorldOutliner != null)
            {
                var selected = mWorldOutliner.SelectedNodes;
                if (selected != null)
                {
                    for (int i = 0; i < selected.Count; i++)
                    {
                        if (selected[i] != null)
                            selected[i].Selected = false;
                    }
                    selected.Clear();
                }
            }

            var policy = PreviewViewport?.RenderPolicy as Graphics.Pipeline.TtRenderPolicy;
            if (policy != null && policy.PickedProxiableManager != null)
            {
                policy.PickedProxiableManager.ClearSelected();
            }

            // ShowBoundVolumes(true, false, null) 把所有节点上的 Bound 高亮关掉。
            // PreviewViewport 自身有这个方法 (基类 TtViewportSlate 提供)。
            PreviewViewport?.ShowBoundVolumes(true, false, null);

            if (NodeInspector != null)
                NodeInspector.Target = null;

            PreviewViewport?.Axis?.SetSelectedNodes(mWorldOutliner?.SelectedNodes);
        }
        [Category("Option")]
        [ReadOnly(true)]
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible 
        { 
            get => mVisible; 
            set => mVisible = value; 
        }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public virtual IO.IAsset GetAsset()
        {
            return Scene;
        }
        public GamePlay.Scene.TtScene Scene;
        public TtSceneEditorViewport PreviewViewport = new TtSceneEditorViewport();
        public TtWorldOutliner mWorldOutliner;
        EGui.Controls.TtContentBrowser mContentBrowser = new EGui.Controls.TtContentBrowser();

        public EGui.Controls.PropertyGrid.TtPropertyGrid NodeInspector = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        public EGui.Controls.PropertyGrid.TtPropertyGrid ScenePropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        public EGui.Controls.PropertyGrid.TtPropertyGrid EditorPropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        [Category("Option")]
        public Graphics.Pipeline.TtRenderPolicy RenderPolicy { get => PreviewViewport.RenderPolicy; }
        [Category("Option")]
        [DisplayName("Editor UI Font Size")]
        [EGui.Controls.PropertyGrid.TtValueRange(TtEngineConfig.MinEditorUIFontSize, TtEngineConfig.MaxEditorUIFontSize)]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.5f)]
        [EGui.Controls.PropertyGrid.TtValueFormat("%.1f")]
        public float EditorUIFontSize
        {
            get => TtEngine.Instance.Config.EditorUIFontSize;
            set => TtEngine.Instance.Config.EditorUIFontSize = value;
        }

        bool mIsDrawing = false;
        bool IsDrawing
        {
            get => mIsDrawing;
            set
            {
                //if(mIsDrawing && !value)
                //{
                //    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
                //    mainEditor?.RemoveFromMainMenu(mMenuItems);
                //}
                //else if(!mIsDrawing && value)
                //{
                //    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
                //    mainEditor?.AppendToMainMenu(mMenuItems);
                //}
                mIsDrawing = value;
            }
        }
        List<EGui.UIProxy.MenuItemProxy> mMenuItems = new List<EGui.UIProxy.MenuItemProxy>();
        public void InitMainMenu()
        {
            var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
            mMenuItems.Clear();
            mMenuItems.Add(new EGui.UIProxy.MenuItemProxy()
            {
                MenuName = "View",
                IsTopMenuItem = true,
                SubMenus = new List<EGui.UIProxy.IUIProxyBase>()
                    {
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "DisableAO",
                            Selected = false,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                PreviewViewport.RenderPolicy.DisableAO = !PreviewViewport.RenderPolicy.DisableAO;
                                item.Selected = PreviewViewport.RenderPolicy.DisableAO;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "DisableHDR",
                            Selected = false,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
                            {
                                PreviewViewport.RenderPolicy.DisableHDR = !this.PreviewViewport.RenderPolicy.DisableHDR;
                                item.Selected = PreviewViewport.RenderPolicy.DisableHDR;
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "DisablePointLight",
                            Selected = false,
                            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data)=>
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
                    //mMacrossShow,
                    mContentBrowserShow,
                    mPlaceItemPanelShow,
                }
            });
            //mainEditor.AppendToMainMenu(mMenuItems.ToArray());
        }

        protected void DrawMainMenuBar()
        {
            if (ImGuiAPI.BeginMenuBar())
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                for (int i = 0; i < mMenuItems.Count; i++)
                    mMenuItems[i].OnDraw(in drawList, in Support.TtAnyPointer.Default);
                ImGuiAPI.EndMenuBar();
            }
        }

        protected class PlaceItemData
        {
            public string Name;
            public string FilterStrings;
            public Rtti.TtClassMeta NodeClassMeta;

            public List<PlaceItemData> Children = new List<PlaceItemData>();

            public bool MatchFilter(string filter)
            {
                if (string.IsNullOrEmpty(filter))
                    return true;
                if (Name != null && Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (FilterStrings != null && FilterStrings.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                // 分类节点：只要有任一子节点匹配就显示
                for (int i = 0; i < Children.Count; i++)
                {
                    if (Children[i].MatchFilter(filter))
                        return true;
                }
                return false;
            }
        }

        // 按第一级菜单路径分类，key = 分类名, value = 该分类下所有可放置节点
        List<PlaceItemData> mPlaceCategories = new List<PlaceItemData>();
        // 扁平列表，搜索时使用
        List<PlaceItemData> mAllPlaceItems = new List<PlaceItemData>();

        void InitPlaceItems()
        {
            mPlaceCategories.Clear();
            mAllPlaceItems.Clear();

            var categoryMap = new Dictionary<string, PlaceItemData>();

            var typeDesc = Rtti.TtTypeDescGetter<TtNode>.TypeDesc;
            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeDesc);
            var subClasses = meta.SubClasses;
            foreach (var classMeta in subClasses)
            {
                var atts = classMeta.ClassType.SystemType.GetCustomAttributes(typeof(Bricks.CodeBuilder.ContextMenuAttribute), inherit: false);
                if (atts.Length == 0)
                    continue;
                var att = atts[0] as Bricks.CodeBuilder.ContextMenuAttribute;
                if (!att.HasKeyString(TtNode.EditorKeyword))
                    continue;

                // MenuPaths[0] 是分类（如 "Graphics"），最后一个是节点显示名
                string categoryName = att.MenuPaths.Length > 1 ? att.MenuPaths[0] : "General";
                string nodeName = att.MenuPaths[att.MenuPaths.Length - 1];

                if (!categoryMap.TryGetValue(categoryName, out var category))
                {
                    category = new PlaceItemData() { Name = categoryName };
                    categoryMap[categoryName] = category;
                    mPlaceCategories.Add(category);
                }

                var item = new PlaceItemData()
                {
                    Name = nodeName,
                    FilterStrings = att.FilterStrings,
                    NodeClassMeta = classMeta,
                };
                category.Children.Add(item);
                mAllPlaceItems.Add(item);
            }
        }
        public TtSceneEditor()
        {
            mWorldOutliner = new TtSceneEditorOutliner(PreviewViewport, false);
            PreviewViewport.HostEditor = this;
        }
        ~TtSceneEditor()
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
        protected async Thread.Async.TtTask<bool> Initialize_PreviewScene(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            //await viewport.RenderPolicy.Initialize(null);

            (viewport as EGui.Slate.TtWorldViewportSlate).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var gridNode = await GamePlay.Scene.TtGridNode.AddGridNode(viewport.World, viewport.World.Root);
            gridNode.ViewportSlate = this.PreviewViewport;
            return true;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public Graphics.Pipeline.TtCpuCullingNode CpuCullNode = null;
        public GamePlay.TtWorld.TtVisParameter.EVisCullFilter CullFilters 
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
        public async virtual Thread.Async.TtTask<bool> OpenEditor(TtMainEditorApplication mainEditor, RName name, object arg, bool saveLayout)
        {
            await PreviewViewport.InitWorld();

            AssetName = name;
            Scene = await name.GetAsset<TtScene>(PreviewViewport.World);// TtEngine.Instance.SceneManager.CreateScene(PreviewViewport.World, name);
            if (Scene == null)
                return false;
            var rpolicy = Scene.RPolicyName;
            if (rpolicy == null)
                rpolicy = TtEngine.Instance.Config.MainRPolicyName;
            //PreviewViewport.PreviewAsset = name;
            PreviewViewport.Title = $"Scene:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            // SceneEditor 走飞行式滚轮: 相机和 LookAt 同步推进,
            // 避免相机逼近 LookAt 点后滚轮被 EditorCameraController 卡住推不动。
            PreviewViewport.CameralWheelMoveWithLookAt = true;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, rpolicy, 0, 1);
            
            // ReCreateInteractiveModes 沿继承链收集 mode, 因为 TtWorldViewportSlate 自己
            // 也注册了 TtWorldViewportInteractiveMode, 所以列表里同时有它和我们注册的
            // TtSceneEditorInteractiveMode; 而基类 ReCreateInteractiveModes 默认把列表
            // 最后一个设为 CurrentIntercativeMode (顺序不可控)。这里显式把缺省 mode
            // 切回 SceneEditor 专属的 TtSceneEditorInteractiveMode。
            PreviewViewport.SetDefaultInteractiveMode<TtSceneEditorInteractiveMode>();

            var camPos = new DVector3(10, 10, 10);
            PreviewViewport.CameraController.Camera?.LookAtLH(in camPos, in DVector3.Zero, in Vector3.Up);

            PreviewViewport.Axis.RootNode.Parent = Scene;
            PreviewViewport.World.Root = Scene;

            var gridNode = await GamePlay.Scene.TtGridNode.AddGridNode(Scene.World, Scene);
            gridNode.ViewportSlate = this.PreviewViewport;

            ScenePropGrid.Target = Scene;
            EditorPropGrid.Target = this;

            mWorldOutliner.Title = $"Outliner:{name}";

            TtEngine.Instance.TickableManager.AddTickable(this);

            InitializeMacrossEditor();

            CpuCullNode = PreviewViewport.RenderPolicy.FindNode<Graphics.Pipeline.TtCpuCullingNode>("CpuCulling");

            CullFilters = TtWorld.TtVisParameter.EVisCullFilter.UtilityEditor | TtWorld.TtVisParameter.EVisCullFilter.LightDebug;
            //System.Diagnostics.Debug.Assert(CpuCullNode != null);
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
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Left, 0.8f, ref middleId, ref rightId);
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Down, 0.5f, ref rightDownId, ref rightUpId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir.ImGuiDir_Down, 0.3f, ref downId, ref middleId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir.ImGuiDir_Left, 0.2f, ref leftId, ref middleId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("SceneDetails", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("NodeDetails", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Editor Settings", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Camera Settings", mDockKeyClass), rightUpId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Outliner", mDockKeyClass), rightUpId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Preview", mDockKeyClass), middleId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Macross", mDockKeyClass), middleId);
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
            IsDrawing = EGui.UIProxy.DockProxy.BeginMainForm(GetWindowsName(), this, ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar);
            if (IsDrawing)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawMainMenuBar();
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
                            var corners = mWorldOutliner.SelectedNodes[i].RefAABB.GetCorners();
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

            DrawOutliner();

            DrawNodeDetails();
            DrawEditorSettings();
            DrawSceneDetails();
            
            DrawCameraSettings();

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
            TtSnapshot.Save(AssetName, GetAsset().GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV());
        }
        protected virtual void Reload()
        {
            if (Scene != null)
                Scene.Parent = null;

            System.Action action = async () =>
            {
                var saved = Scene;
                Scene = await AssetName.GetAsset<TtScene>(PreviewViewport.World); // TtEngine.Instance.SceneManager.CreateScene(PreviewViewport.World, AssetName);
                Scene.Parent = PreviewViewport.World.Root;

                saved.Dispose();
            };
            action();
        }
        protected virtual void DrawToolBar()
        {
            var drawList = ImGuiAPI.GetWindowDrawList();
            EGui.UIProxy.Toolbar.BeginToolbar(drawList);
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Open Macross", in btSize,
                EGui.UIProxy.StyleConfig.Instance.ToolButtonTextColor,
                EGui.UIProxy.StyleConfig.Instance.ToolButtonTextColor_Press,
                EGui.UIProxy.StyleConfig.Instance.ToolButtonTextColor_Hover,
                EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGColor,
                EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGActiveColor,
                EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGHoverColor
                ))
            {
                Editor.TtAssetEditorManager.TryOpenEditor(Scene.MacrossEditor, AssetName, null, true).AddWaitTask();
            }
            ImGuiAPI.SameLine(0, -1);
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

            // InteractiveMode 切换下拉框: 与 TtViewportSlate.OnDrawViewportUI 内置的
            // ##InteractiveMode Combo 行为一致, 但显示在 SceneEditor 的工具栏上,
            // 让用户在编辑器主视图就能切换 mode。
            DrawInteractiveModeCombo();

            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList);
            //ImGuiAPI.BeginGroup();

            if (CpuCullNode != null)
            {
                for (int i = 0; i < (int)GamePlay.TtWorld.TtVisParameter.EVisCullFilter.FilterTypeCount; i++)
                {
                    var type = (GamePlay.TtWorld.TtVisParameter.EVisCullFilter)(1 << i);
                    ImGuiAPI.SameLine(0, -1);
                    bool checkValue = (CullFilters & type) != 0;
                    var name = type.ToString();
                    if (name == "FilterTypeCount")
                    {
                        name = GamePlay.TtWorld.TtVisParameter.FilterTypeCountAs;
                    }
                    if (EGui.UIProxy.CustomButton.ToggleButton(name, in btSize, ref checkValue))
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
            }
            else
            {
                ImGuiAPI.Text("No CpuCullingNode!");
            }
            //ImGuiAPI.EndGroup();
            EGui.UIProxy.Toolbar.EndToolbar();
        }
        // 在工具栏画 InteractiveMode 切换下拉框。
        // 列表内容来自 PreviewViewport.InteractiveModes (由 ReCreateInteractiveModes 填充),
        // 选中项写回 PreviewViewport.CurrentIntercativeMode, 与 TtViewportSlate 内置的
        // 视口角 Combo 共享同一份状态, 任意一处切换都立即生效。
        protected virtual void DrawInteractiveModeCombo()
        {
            var modes = PreviewViewport?.InteractiveModes;
            if (modes == null || modes.Count == 0)
            {
                ImGuiAPI.SameLine(0, -1);
                ImGuiAPI.Text("Mode: -");
                return;
            }

            ImGuiAPI.SameLine(0, -1);
            ImGuiAPI.Text("Mode:");
            ImGuiAPI.SameLine(0, -1);

            var current = PreviewViewport.CurrentIntercativeMode;
            var currentName = current != null ? current.GetType().Name : "None";
            const float comboWidth = 200.0f;
            ImGuiAPI.SetNextItemWidth(comboWidth);
            if (ImGuiAPI.BeginCombo("##SceneEditorInteractiveMode", currentName, ImGuiComboFlags_.ImGuiComboFlags_None))
            {
                for (int i = 0; i < modes.Count; i++)
                {
                    var mode = modes[i];
                    var modeName = mode.GetType().Name;
                    var isSelected = (mode == current);
                    if (ImGuiAPI.Selectable(modeName, isSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                    {
                        PreviewViewport.CurrentIntercativeMode = mode;
                    }
                }
                ImGuiAPI.EndCombo();
            }
        }
        EGui.UIProxy.MenuItemProxy mDrawSceneDetailsShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "SceneDetails",
            Selected = true,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data) =>
            {
                item.Selected = !item.Selected;
            },
        };
        EGui.UIProxy.MenuItemProxy mDrawNodeDetailsShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "NodeDetails",
            Selected = true,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data) =>
            {
                item.Selected = !item.Selected;
            },
        };
        EGui.UIProxy.MenuItemProxy mEditorSettingsShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "Editor Settings",
            Selected = true,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data) =>
            {
                item.Selected = !item.Selected;
            },
        };
        EGui.UIProxy.MenuItemProxy mCameraSettingsShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "Camera Settings",
            Selected = false,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data) =>
            {
                item.Selected = !item.Selected;
            },
        };
        EGui.UIProxy.MenuItemProxy mOutlinerShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "Outliner",
            Selected = true,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data) =>
            {
                item.Selected = !item.Selected;
            },
        };
        EGui.UIProxy.MenuItemProxy mPreviewShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "Preview",
            Selected = true,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data) =>
            {
                item.Selected = !item.Selected;
            },
        };
        //EGui.UIProxy.MenuItemProxy mMacrossShow = new EGui.UIProxy.MenuItemProxy()
        //{
        //    MenuName = "Macross",
        //    Selected = true,
        //    Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data) =>
        //    {
        //        item.Selected = !item.Selected;
        //    },
        //};
        EGui.UIProxy.MenuItemProxy mContentBrowserShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "Content Browser",
            Selected = true,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data) =>
            {
                item.Selected = !item.Selected;
            },
        };
        EGui.UIProxy.MenuItemProxy mPlaceItemPanelShow = new EGui.UIProxy.MenuItemProxy()
        {
            MenuName = "Place Items",
            Selected = false,
            Action = (EGui.UIProxy.MenuItemProxy item, Support.TtAnyPointer data) =>
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

                var camPos = PreviewViewport.CameraController.Camera.GetPosition();
                var saved = camPos;
                ImGuiAPI.InputDouble($"X", ref camPos.X, 0.1, 10.0, "%.2f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                ImGuiAPI.InputDouble($"Y", ref camPos.Y, 0.1, 10.0, "%.2f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                ImGuiAPI.InputDouble($"Z", ref camPos.Z, 0.1, 10.0, "%.2f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                if (saved != camPos)
                {
                    var lookAt = PreviewViewport.CameraController.Camera.GetLookAt();
                    var up = PreviewViewport.CameraController.Camera.GetUp();
                    PreviewViewport.CameraController.Camera.LookAtLH(in camPos, lookAt - saved + camPos, up);
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
            if (TtContentBrowser.IsInDragDropMode)
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
                var dragData = (TtContentBrowser.DragDropData)handle.Target;
                for (int i = 0; i < dragData.Metas.Length; i++)
                {
                    dragData.Metas[i].DraggingInViewport = draggingInViewport;
                    if (draggingInViewport)
                    {
                        dragData.Metas[i].OnDragging(PreviewViewport).AddWaitTask();
                    }
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
                    // Place Items 面板拖入
                    var payload = ImGuiAPI.AcceptDragDropPayload("PlaceItemNodeDragDrop", ImGuiDragDropFlags_.ImGuiDragDropFlags_None);
                    if (payload != null)
                    {
                        var handle = GCHandle.FromIntPtr((IntPtr)(payload->Data));
                        var dragData = handle.Target as PlaceItemData;
                        if (dragData != null)
                        {
                            PlaceNodeFromItem(dragData).AddWaitTask();
                        }
                    }
                    // Content Browser 拖入
                    payload = ImGuiAPI.AcceptDragDropPayload("ContentBrowserAssetDragDrop", ImGuiDragDropFlags_.ImGuiDragDropFlags_None);
                    if (payload != null)
                    {
                        var handle = GCHandle.FromIntPtr((IntPtr)(payload->Data));
                        var dragData = (TtContentBrowser.DragDropData)handle.Target;
                        for (int i = 0; i < dragData.Metas.Length; i++)
                        {
                            dragData.Metas[i].OnDragTo(PreviewViewport).AddWaitTask();
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
        string mPlaceItemFilterStr = "";
        bool mPlaceItemFilterFocused = false;
        GCHandle mPlaceItemDragHandle;

        protected unsafe void DrawPlaceItemPanel()
        {
            if (!mPlaceItemPanelShow.Selected)
                return;
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Place Items", ref mPlaceItemPanelShow.Selected, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                // 搜索栏
                var drawList = ImGuiAPI.GetWindowDrawList();
                EGui.UIProxy.SearchBarProxy.OnDraw(ref mPlaceItemFilterFocused, in drawList, "Search...", ref mPlaceItemFilterStr, -1);

                var filterLower = mPlaceItemFilterStr.ToLower();
                bool hasFilter = !string.IsNullOrEmpty(filterLower);

                if (hasFilter)
                {
                    // 搜索模式：显示所有匹配的扁平列表
                    for (int i = 0; i < mAllPlaceItems.Count; i++)
                    {
                        var item = mAllPlaceItems[i];
                        if (!item.MatchFilter(filterLower))
                            continue;
                        DrawPlaceItemLeaf(item);
                    }
                }
                else
                {
                    // Tab 分类模式
                    if (ImGuiAPI.BeginTabBar("##PlaceItemTabs", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
                    {
                        // "All" 标签页
                        if (ImGuiAPI.BeginTabItem("All", null, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                        {
                            for (int catIdx = 0; catIdx < mPlaceCategories.Count; catIdx++)
                            {
                                var category = mPlaceCategories[catIdx];
                                if (ImGuiAPI.CollapsingHeader(category.Name, ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                                {
                                    for (int j = 0; j < category.Children.Count; j++)
                                        DrawPlaceItemLeaf(category.Children[j]);
                                }
                            }
                            ImGuiAPI.EndTabItem();
                        }
                        // 每个分类一个 Tab
                        for (int catIdx = 0; catIdx < mPlaceCategories.Count; catIdx++)
                        {
                            var category = mPlaceCategories[catIdx];
                            if (ImGuiAPI.BeginTabItem(category.Name, null, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                            {
                                for (int j = 0; j < category.Children.Count; j++)
                                    DrawPlaceItemLeaf(category.Children[j]);
                                ImGuiAPI.EndTabItem();
                            }
                        }
                        ImGuiAPI.EndTabBar();
                    }
                }
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        unsafe void DrawPlaceItemLeaf(PlaceItemData item)
        {
            var flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf
                      | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen
                      | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;

            ImGuiAPI.TreeNodeEx(item.Name, flags);

            // 拖拽源
            if (ImGuiAPI.BeginDragDropSource(ImGuiDragDropFlags_.ImGuiDragDropFlags_None))
            {
                if (mPlaceItemDragHandle.IsAllocated)
                    mPlaceItemDragHandle.Free();
                mPlaceItemDragHandle = GCHandle.Alloc(item);
                ImGuiAPI.SetDragDropPayload("PlaceItemNodeDragDrop", GCHandle.ToIntPtr(mPlaceItemDragHandle).ToPointer(), (uint)sizeof(IntPtr), ImGuiCond_.ImGuiCond_None);
                ImGuiAPI.Text(item.Name);
                ImGuiAPI.EndDragDropSource();
            }
        }

        /// <summary>
        /// 获取放置节点时的 parent:
        /// 如果 Outliner 中选中了 TtHubNode 则作为 parent，否则使用 Scene
        /// </summary>
        TtNode GetPlaceNodeParent()
        {
            if (mWorldOutliner.SelectedNodes.Count > 0)
            {
                // 取最后选中的那个节点，如果它是 HubNode 就用它
                for (int i = mWorldOutliner.SelectedNodes.Count - 1; i >= 0; i--)
                {
                    if (mWorldOutliner.SelectedNodes[i] is TtHubNode hub)
                        return hub;
                }
            }
            return Scene;
        }

        /// <summary>
        /// 根据鼠标拖放位置做射线检测，计算放置坐标。
        /// 优先 LineCheck 场景几何体，fallback 到 Y=0 地面平面，最后 fallback 到相机前方。
        /// </summary>
        unsafe DVector3 CalcPlacePosition()
        {
            var camera = PreviewViewport.CameraController.Camera;
            if (camera == null)
                return DVector3.Zero;

            var start = camera.GetPosition();
            Vector3 dir = Vector3.Zero;

            // 屏幕鼠标坐标 → 视口局部坐标
            var mouseScreen = new Vector2(
                TtEngine.Instance.InputSystem.Mouse.EventMouseX,
                TtEngine.Instance.InputSystem.Mouse.EventMouseY) - PreviewViewport.ViewportPos;
            var mouseLocal = PreviewViewport.Window2Viewport(mouseScreen);

            camera.GetPickRay(ref dir, mouseLocal.X, mouseLocal.Y,
                PreviewViewport.ClientSize.X, PreviewViewport.ClientSize.Y);

            var end = start + dir.AsDVector() * 1000.0;

            // 1) 射线 vs 场景几何
            VHitResult hitResult = new VHitResult();
            List<TtNode> candidates = null;
            if (PreviewViewport.World.CollideOctree.OctreeHitTest(in start, in end, ref candidates, &hitResult))
                return hitResult.Position;

            // 2) 射线 vs Y=0 地面平面
            var ray = new DRay() { Position = start, Direction = dir };
            var groundPlane = new DPlane(DVector3.Zero, DVector3.Up);
            if (DRay.Intersects(in ray, in groundPlane, out double distance))
                return start + dir.AsDVector() * distance;

            // 3) fallback: 相机前方固定距离
            return start + dir.AsDVector() * 10.0;
        }

        /// <summary>
        /// 从 PlaceItemData 创建节点并放置到场景中
        /// </summary>
        async TtTask PlaceNodeFromItem(PlaceItemData item)
        {
            if (item?.NodeClassMeta == null || Scene == null)
                return;

            var parent = GetPlaceNodeParent();
            var nodeType = item.NodeClassMeta.ClassType.SystemType;
            var newNode = await TtNode.SpawnNode(parent, nodeType, null, null, EBoundVolumeType.Box, typeof(TtPlacement));
            if (newNode == null)
                return;

            string prefix = "Node";
            var attr = TtNode.GetNodeAttribute(nodeType);
            if (attr != null)
                prefix = attr.DefaultNamePrefix;
            newNode.NodeData.Name = $"{prefix}_{newNode.SceneId}";

            // 放置在相机前方
            newNode.Placement.Position = CalcPlacePosition();
        }

        public void OnEvent(in Bricks.Input.Event e)
        {
            //throw new NotImplementedException();
        }
        #region Tickable
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtSceneEditor), nameof(TickLogic));
                return mScopeTick;
            }
        }
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

        public string GetWindowsName()
        {
            return AssetName.Name;
        }
        #endregion
    }

    public class TtPrefabEditorOutliner : TtWorldOutliner
    {
        public TtPrefabEditorOutliner(EGui.Slate.TtWorldViewportSlate viewport, bool regRoot)
            : base(viewport, regRoot)
        {

        }
        protected override void DrawBaseMenu(GamePlay.Scene.TtNode node)
        {
            base.DrawBaseMenu(node);
        }
        public override unsafe void DrawAsChildWindow(in Vector2 size)
        {
            base.DrawAsChildWindow(in size);
        }
    }
    public class TtPrefabEditor : TtSceneEditor
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
        public async override Thread.Async.TtTask<bool> OpenEditor(TtMainEditorApplication mainEditor, RName name, object arg, bool saveLayout)
        {
            AssetName = name;
            //PreviewViewport.PreviewAsset = name;
            PreviewViewport.Title = $"Prefab:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            // PrefabEditor 同 SceneEditor, 走飞行式滚轮 (同步推进 LookAt),
            // 避免相机逼近 LookAt 点后滚轮被 EditorCameraController 卡住推不动。
            PreviewViewport.CameralWheelMoveWithLookAt = true;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, TtEngine.Instance.Config.MainRPolicyName, 0, 1);

            Prefab = await name.GetAsset<TtPrefab>(PreviewViewport.World);// TtEngine.Instance.PrefabManager.CreatePrefab(PreviewViewport.World, name);
            if (Prefab == null)
                return false;
            var rpolicy = Prefab.RPolicyName;
            if (rpolicy == null)
                rpolicy = TtEngine.Instance.Config.MainRPolicyName;

            Prefab.Root.Parent = PreviewViewport.World.Root;
            Prefab.Root.NodeName = name.Name;

            ScenePropGrid.Target = Prefab;
            EditorPropGrid.Target = this;

            mWorldOutliner.Title = $"Outliner:{name}";

            TtEngine.Instance.TickableManager.AddTickable(this);

            CpuCullNode = PreviewViewport.RenderPolicy.FindNode<Graphics.Pipeline.TtCpuCullingNode>("CpuCulling");
            //System.Diagnostics.Debug.Assert(CpuCullNode != null);
            return true;
        }

        protected override void Save()
        {
            Prefab.SaveAssetTo(AssetName);
            //TtEngine.Instance.TaskCollector.AddWaitTask(TtEngine.Instance.PrefabManager.ReloadPrefab(AssetName));
        }
        protected override void Reload()
        {
            if (Prefab != null)
                Prefab.Root.Parent = null;

            System.Action action = async () =>
            {
                var saved = Prefab;
                Prefab = await AssetName.GetAsset<TtPrefab>(PreviewViewport.World);// TtEngine.Instance.PrefabManager.CreatePrefab(PreviewViewport.World,AssetName);
                Prefab.Root.Parent = PreviewViewport.World.Root;
            };
            action();
        }
    }
}

namespace EngineNS.GamePlay.Scene
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.TtSceneEditor))]
    public partial class TtScene
    {
    }

    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.TtPrefabEditor))]
    public partial class TtPrefab
    {
    }
}
