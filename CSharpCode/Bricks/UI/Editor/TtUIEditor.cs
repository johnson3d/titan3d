using EngineNS;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.Input;
using EngineNS.Bricks.UI.Controls;
using EngineNS.GamePlay.Scene;
using EngineNS.Rtti;
using EngineNS.UI.Canvas;
using EngineNS.UI.Controls;
using EngineNS.UI.Controls.Containers;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EngineNS.UI.Editor
{
    public partial class TtUIEditor : EngineNS.Editor.IAssetEditor, IRootForm, ITickable
    {
        public int GetTickOrder()
        {
            return 0;
        }
        bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; } = uint.MaxValue;
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;        
        public EngineNS.Editor.UPreviewViewport PreviewViewport = new EngineNS.Editor.UPreviewViewport();

        public EditorUIHost mUIHost = new EditorUIHost();
        public TtUINode mUINode;
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref UIAsset);
            CoreSDK.DisposeObject(ref PreviewViewport);
            DetailsGrid.Target = null;
        }

        public async Task<bool> Initialize()
        {
            await DetailsGrid.Initialize();

            mUIHost.Name = "UIEditorHost";
            mUIHost.WindowSize = new SizeF(1920, 1080);

            /*/ test /////////////////////////
            //var element = new TtImage(); // new TtUIElement();
            var button = new TtButton();
            var canvas = new UI.Controls.Containers.TtCanvasControl();
            //var img = new TtImage();
            //img.UIBrush.UVAnimAsset = RName.GetRName("ui/button_hover.uvanim", RName.ERNameType.Engine);
            //button.Children.Add(img);

            var txt = new TtText();
            txt.Text = "abc";
            button.Children.Add(txt);

            ////canvas.AddChild(element);
            ////DetailsGrid.Target = element;

            canvas.Children.Add(button);
            //UI.Controls.Containers.TtCanvasControl.SetAnchorRectX(button, 50);
            //UI.Controls.Containers.TtCanvasControl.SetAnchorRectZ(button, 1016);
            //UI.Controls.Containers.TtCanvasControl.SetAnchorRectW(button, 793);
            UI.Controls.Containers.TtCanvasControl.SetAnchorRectZ(button, 100);
            UI.Controls.Containers.TtCanvasControl.SetAnchorRectW(button, 50);
            DetailsGrid.Target = button;
            mUIHost.Children.Add(canvas);
            ////////////////////////////////*/

            return true;
        }
        protected async System.Threading.Tasks.Task Initialize_PreviewMaterialInstance(Graphics.Pipeline.UViewportSlate viewport, USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();

            (viewport as EngineNS.Editor.UPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var scene = PreviewViewport.World.Root.GetNearestParentScene();
            mUINode = await scene.NewNode(PreviewViewport.World, typeof(TtUINode), new TtUINode.TtUINodeData(),
                EBoundVolumeType.Box, typeof(GamePlay.UPlacement)) as TtUINode;
            mUINode.NodeData.Name = "UI";
            mUINode.Parent = PreviewViewport.World.Root;
            mUINode.Placement.SetTransform(DVector3.Zero, Vector3.One, Quaternion.Identity);
            mUINode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
            mUINode.IsAcceptShadow = false;
            mUINode.IsCastShadow = false;
            mUIHost.SceneNode = mUINode;

            mUIHost.RenderCamera = PreviewViewport.RenderPolicy.DefaultCamera;

            //var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(),
            //            typeof(GamePlay.UPlacement), UIAsset.Mesh,
            //            DVector3.Zero, Vector3.One, Quaternion.Identity);
            //meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
            //meshNode.NodeData.Name = "PreviewObject";
            //meshNode.IsAcceptShadow = false;
            //meshNode.IsCastShadow = true;

            //var aabb = UIAsset.Mesh.MaterialMesh.Mesh.mCoreObject.mAABB;
            float radius = Math.Max(mUIHost.WindowSize.Width, mUIHost.WindowSize.Height);
            BoundingSphere sphere;
            sphere.Center = new Vector3(mUIHost.WindowSize.Width * 0.5f, mUIHost.WindowSize.Height * 0.5f, 0);
            sphere.Radius = radius;
            policy.DefaultCamera.AutoZoom(ref sphere);

            //var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(viewport.World, viewport.World.Root);
            //gridNode.ViewportSlate = this.PreviewViewport;
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

            var designerId = id;
            uint controlsId = 0;
            uint hierachyId = 0;
            uint detailsId = 0;
            ImGuiAPI.DockBuilderSplitNode(designerId, ImGuiDir_.ImGuiDir_Left, 0.2f, ref controlsId, ref designerId);
            ImGuiAPI.DockBuilderSplitNode(controlsId, ImGuiDir_.ImGuiDir_Down, 0.5f, ref hierachyId, ref controlsId);
            ImGuiAPI.DockBuilderSplitNode(designerId, ImGuiDir_.ImGuiDir_Right, 0.2f, ref detailsId, ref designerId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Controls", mDockKeyClass), controlsId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Designer", mDockKeyClass), designerId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Hierachy", mDockKeyClass), hierachyId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Details", mDockKeyClass), detailsId);
            ImGuiAPI.DockBuilderFinish(id);
        }
        public ImGuiMouseCursor_ mMouseCursor;
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public Vector2 WindowContentRegionMin, WindowContentRegionMax;
        public EGui.Controls.PropertyGrid.PropertyGrid DetailsGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public unsafe void OnDraw()
        {
            if (Visible == false || UIAsset == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (EGui.UIProxy.DockProxy.BeginMainForm(UIAsset.AssetName.Name, this, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as EngineNS.Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                WindowContentRegionMin = ImGuiAPI.GetWindowContentRegionMin();
                WindowContentRegionMax = ImGuiAPI.GetWindowContentRegionMax();

                DrawToolBar();

                ImGuiAPI.Separator();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm();

            DrawControls();
            DrawDesigner();
            DrawDetails();
            DrawHierachy();

            ImGuiAPI.SetMouseCursor(mMouseCursor);
        }
        bool mIsSimulateMode = false;
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                UEngine.Instance.UIManager.Save(AssetName, mUIHost.Children[0]);
                //Mesh.SaveAssetTo(Mesh.AssetName);
                //var unused = UEngine.Instance.GfxDevice.MaterialInstanceManager.ReloadMaterialInstance(Mesh.AssetName);

                //USnapshot.Save(Mesh.AssetName, Mesh.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Reload", in btSize))
            {

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
            if(EGui.UIProxy.CustomButton.ToggleButton("Simulate", in btSize, ref mIsSimulateMode))
            {
                PreviewViewport.FreezCameraControl = mIsSimulateMode;
                if (mIsSimulateMode)
                {
                    UEngine.Instance.UIManager.AddUI(AssetName, "UIEditorSimulate", mUIHost);
                }
                else
                {
                    UEngine.Instance.UIManager.RemoveUI(AssetName, "UIEditorSimulate");
                }
            }
        }

        protected class ControlItemData
        {
            public string Name;
            public string Description;
            public string Icon;
            public UTypeDesc UIControlType;

            public ControlItemData Parent;
            public List<ControlItemData> Children = new List<ControlItemData>();
        }
        List<ControlItemData> mUIControls = new List<ControlItemData>();
        public bool IsUIControlsDirty = true;
        protected unsafe virtual void CollectionUIControls()
        {
            mUIControls.Clear();
            foreach(var service in EngineNS.Rtti.UTypeDescManager.Instance.Services.Values)
            {
                foreach(var type in service.Types.Values)
                {
                    var attr = type.GetCustomAttribute<Editor_UIControlAttribute>(false);
                    if (attr == null)
                        continue;

                    var pathSplit = attr.Path.Split('.');
                    InitControlItemData(null, mUIControls, pathSplit, 0, attr, type);
                }
            }
        }
        void InitControlItemData(ControlItemData parent, List<ControlItemData> childList, string[] path, int pathStartIdx, Editor_UIControlAttribute att, Rtti.UTypeDesc type)
        {
            if ((path.Length - pathStartIdx) <= 0)
                return;

            var curName = path[pathStartIdx];
            pathStartIdx++;
            bool find = false;
            for(int i = 0; i < childList.Count; i++)
            {
                if (childList[i].Name == curName)
                {
                    InitControlItemData(childList[i], childList[i].Children, path, pathStartIdx, att, type);
                    find = true;
                    break;
                }
            }
            if(!find)
            {
                var itemData = new ControlItemData()
                {
                    Name = curName,
                    Parent = parent,
                };
                childList.Add(itemData);
                if((path.Length - pathStartIdx) > 0)
                {
                    InitControlItemData(itemData, itemData.Children, path, pathStartIdx, att, type);
                }
                else
                {
                    itemData.Description = att.Description;
                    itemData.UIControlType = type;
                }
            }
        }

        bool mControlsShow = true;
        protected unsafe void DrawControls()
        {
            var sz = new Vector2(-1);
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Controls", ref mControlsShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                for(int i=0; i<mUIControls.Count; i++)
                {
                    DrawControlItemData(mUIControls[i]);
                }
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        protected struct ControlDragData
        {
            public string TypeName;
        }

        unsafe void DrawControlItemData(ControlItemData itemData)
        {
            var flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;
            if (itemData.Children.Count == 0)
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;
            var treeNodeResult = ImGuiAPI.TreeNodeEx(itemData.Name, flags);
            if(itemData.UIControlType != null)
            {
                if(ImGuiAPI.BeginDragDropSource(ImGuiDragDropFlags_.ImGuiDragDropFlags_None))
                {
                    var data = new ControlDragData()
                    {
                        TypeName = UTypeDescManager.Instance.GetTypeStringFromType(itemData.UIControlType),
                    };
                    var handle = GCHandle.Alloc(data);
                    ImGuiAPI.SetDragDropPayload("UIControlDragDrop", GCHandle.ToIntPtr(handle).ToPointer(), (uint)Marshal.SizeOf<ControlDragData>(), ImGuiCond_.ImGuiCond_None);
                    ImGuiAPI.Text(itemData.Name);
                    ImGuiAPI.EndDragDropSource();
                }
            }
            if(treeNodeResult)
            {
                for(int i=0; i<itemData.Children.Count; i++)
                {
                    DrawControlItemData(itemData.Children[i]);
                }
                ImGuiAPI.TreePop();
            }
        }
        bool mDesignerShow = true;
        protected unsafe void DrawDesigner()
        {
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Designer", ref mDesignerShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                var pos = ImGuiAPI.GetWindowPos();
                var size = ImGuiAPI.GetWindowSize();
                var min = ImGuiAPI.GetWindowContentRegionMin();
                var max = ImGuiAPI.GetWindowContentRegionMax();

                var start = pos + min;
                var end = start + (max - min);

                //var drawList = ImGuiAPI.GetForegroundDrawList();
                //drawList.AddRect(start, end, 0xff0000ff, 0, ImDrawFlags_.ImDrawFlags_None, 3.0f);
                //drawList.AddRect(pos, pos + size, 0xffff00ff, 0, ImDrawFlags_.ImDrawFlags_None, 1.0f);
                var mouseStart = start - WindowPos;
                var mouseEnd = end - WindowPos;
                mUIHost.WindowRectangle.X = mouseStart.X;
                mUIHost.WindowRectangle.Y = mouseStart.Y;
                mUIHost.WindowRectangle.Width = (end.X - start.X);
                mUIHost.WindowRectangle.Height = (end.Y - start.Y);

                //PreviewViewport.WindowSize = ImGuiAPI.GetWindowSize();
                PreviewViewport.ViewportType = Graphics.Pipeline.UViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();

                // debug ///////////////////
                var drawList = ImGuiAPI.GetForegroundDrawList();
                drawList.AddText(pos + new Vector2(0, 50), 0xffffffff, 
                    $"x:{UEngine.Instance.UIManager.DebugMousePt.X}\r\n" +
                    $"y:{UEngine.Instance.UIManager.DebugMousePt.Y}\r\n" +
                    $"px:{UEngine.Instance.UIManager.DebugHitPt.X}\r\n" +
                    $"py:{UEngine.Instance.UIManager.DebugHitPt.Y}\r\n" +
                    $"el:{UEngine.Instance.UIManager.DebugPointatElement}", null);
                ////////////////////////////
                UEngine.Instance.UIManager.DebugHitPt = new Vector2(UEngine.Instance.InputSystem.Mouse.EventMouseX, UEngine.Instance.InputSystem.Mouse.EventMouseY);
                if (ImGuiAPI.BeginDragDropTarget())
                {
                    var payload = ImGuiAPI.AcceptDragDropPayload("UIControlDragDrop", ImGuiDragDropFlags_.ImGuiDragDropFlags_None);
                    if(payload != null)
                    {
                        var handle = GCHandle.FromIntPtr((IntPtr)(payload->Data));
                        var data = (ControlDragData)handle.Target;
                        var uiControl = UTypeDescManager.CreateInstance(UTypeDesc.TypeOf(data.TypeName)) as TtUIElement;
                        
                        //var curPos = ImGuiAPI.GetMousePos() - pos;
                        var curPos = new Vector2(UEngine.Instance.InputSystem.Mouse.EventMouseX, UEngine.Instance.InputSystem.Mouse.EventMouseY);

                        var element = mUIHost.GetPointAtElement(in curPos);
                        TtContainer container = mUIHost;
                        if(element != null)
                        {
                            container = element as TtContainer;
                            if (container == null)
                                container = element.Parent;
                        }

                        if (container.CanAddChild(uiControl))
                        {
                            container.Children.Add(uiControl);
                            Vector2 offset;
                            container.GetElementPointAtPos(in curPos, out offset);
                            container.ProcessNewAddChild(uiControl, offset, new Vector2(100, 50));
                        }
                        else
                            ShowMessage($"{container.Name} can't add child");

                        //handle.Free();
                    }
                }

                for(int i = mMessages.Count - 1; i >= 0; i--)
                {
                    mMessages[i].Tick(UEngine.Instance.ElapsedSecond);
                    if(mMessages[i].ShowTimeSecond <= 0)
                    {
                        mMessages.RemoveAt(i);
                    }
                    else
                    {
                        drawList.AddText(pos + new Vector2(0, 50), 0xffffffff, mMessages[i].Message, null);
                    }
                }
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        bool mDetailsShow = true;
        protected unsafe void DrawDetails()
        {
            var sz = new Vector2(-1);
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Details", ref mDetailsShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                DetailsGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        bool mHierachyShow = true;
        bool mShowTemplateControls = false;
        protected unsafe void DrawHierachy()
        {
            var sz = new Vector2(-1);
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Hierachy", ref mHierachyShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                EGui.UIProxy.CheckBox.DrawCheckBox("Show Template Controls", ref mShowTemplateControls);
                
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, EGui.UIProxy.StyleConfig.Instance.TVHeader);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderActive, EGui.UIProxy.StyleConfig.Instance.TVHeaderActive);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, EGui.UIProxy.StyleConfig.Instance.TVHeaderHovered);
                DrawUIElementInHierachy(mUIHost, 0);
                ImGuiAPI.PopStyleColor(3);
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        void DrawHierachyContextMenu(TtUIElement element, string name)
        {
            if(ImGuiAPI.BeginPopupContextWindow("##ContextMenu_" + name, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                if (!mSelectedElements.Contains(element))
                    ProcessSelectElement(element, UEngine.Instance.InputSystem.IsCtrlKeyDown());
                Support.UAnyPointer menuData = new Support.UAnyPointer();
                if (EGui.UIProxy.MenuItemProxy.MenuItem("Delete##" + name, null, false, null, drawList, menuData, ref element.HierachyContextMenuState))
                {
                    for(int i=0; i<mSelectedElements.Count; i++)
                    {
                        if (mSelectedElements[i] == mUIHost)
                            continue;
                        mSelectedElements[i].Parent.Children.Remove(mSelectedElements[i]);
                    }
                    ProcessSelectElement(null, false);
                }
                ImGuiAPI.EndPopup();
            }
        }
        void DrawUIElementInHierachy(TtUIElement element, int idx)
        {
            var flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;
            var name = "[" + element.GetType().Name + "] " + element.Name + "##" + idx++;
            if (mSelectedElements.Contains(element))
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
            var container = element as TtContainer;
            if(container != null)
            {
                if(mShowTemplateControls)
                {
                    var count = VisualTreeHelper.GetChildrenCount(container);
                    if(count == 0)
                        flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;
                    var treeNodeResult = ImGuiAPI.TreeNodeEx(name, flags);
                    if(ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        ProcessSelectElement(element, UEngine.Instance.InputSystem.IsCtrlKeyDown());
                    }
                    DrawHierachyContextMenu(element, name);
                    if (treeNodeResult)
                    {
                        for(int i=0; i<count; i++)
                        {
                            var child = VisualTreeHelper.GetChild(container, i);
                            DrawUIElementInHierachy(child, idx);
                        }
                        ImGuiAPI.TreePop();
                    }
                }
                else
                {
                    if(container.Children.Count == 0)
                        flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;
                    var treeNodeResult = ImGuiAPI.TreeNodeEx(name, flags);
                    if(ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        ProcessSelectElement(element, UEngine.Instance.InputSystem.IsCtrlKeyDown());
                    }
                    DrawHierachyContextMenu(element, name);
                    if (treeNodeResult)
                    {
                        for (int i=0; i< container.Children.Count; i++)
                        {
                            DrawUIElementInHierachy(container.Children[i], idx);
                        }
                        ImGuiAPI.TreePop();
                    }
                }
            }
            else
            {
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;
                if(ImGuiAPI.TreeNodeEx(name, flags))
                {
                    DrawHierachyContextMenu(element, name);
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        ProcessSelectElement(element, UEngine.Instance.InputSystem.IsCtrlKeyDown());
                    }
                    ImGuiAPI.TreePop();
                }
            }
        }

        List<TtUIElement> mSelectedElements = new List<TtUIElement>();
        public List<TtUIElement> SelectedElements => mSelectedElements;
        void ProcessSelectElement(TtUIElement element, bool multi)
        {
            if(element == null)
            {
                mSelectedElements.Clear();
            }
            else
            {
                if(multi)
                {
                    if (mSelectedElements.Contains(element))
                        mSelectedElements.Remove(element);
                    else
                        mSelectedElements.Add(element);
                }
                else
                {
                    mSelectedElements.Clear();
                    mSelectedElements.Add(element);
                }
            }
            DetailsGrid.Target = mSelectedElements;
            _ = ProcessSelectElementDecorator();
        }

        #region IAssetEditor
        public TtUIAsset UIAsset;
        public RName AssetName { get; set; }
        public IRootForm GetRootForm()
        {
            return this;
        }
        public void OnEvent(in Bricks.Input.Event e)
        {
        }
        public async System.Threading.Tasks.Task<bool> OpenEditor(EngineNS.Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            mUIHost.Children.Add(UEngine.Instance.UIManager.Load(AssetName));
            UIAsset = new TtUIAsset();
            UIAsset.AssetName = name;
            //UIAsset.Mesh = await UI.Canvas.TtCanvas.TestCreate();
            
            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"UI:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewMaterialInstance;
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.SlateApplication, UEngine.Instance.Config.MainRPolicyName, 0, 1);
            PreviewViewport.OnEventAction = OnPreviewViewportEvent;

            await InitializeDecorators();

            //DetailsGrid.Target = UIAsset;
            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public void OnCloseEditor()
        {
            UEngine.Instance.TickableManager.RemoveTickable(this);
            Dispose();
        }
        #endregion

        void OnPreviewViewportEvent(in Bricks.Input.Event e)
        {
            DecoratorEventProcess(in e);

            switch(e.Type)
            {
                case Bricks.Input.EventType.MOUSEMOTION:
                    if(!mIsSimulateMode)
                    {
                        var pt = new Vector2(e.MouseButton.X, e.MouseButton.Y);
                        var data = new TtUIElement.RayIntersectData();
                        var element = mUIHost.GetPointAtElement(in pt);
                        if(mCurrentPointAtElement != element && (CurrentDecorator == null || !CurrentDecorator.IsInDecoratorOperation()))
                        {
                            SetCurrentPointAtElement(element);
                        }
                    }
                    break;
                case Bricks.Input.EventType.MOUSEBUTTONDOWN:
                    {
                        var delta = PreviewViewport.WindowPos - PreviewViewport.ViewportPos;
                        var mousePt = new Vector2(e.MouseButton.X - delta.X, e.MouseButton.Y - delta.Y);
                        if(mousePt.X >= PreviewViewport.ClientMin.X && 
                           mousePt.X <= PreviewViewport.ClientMax.X &&
                           mousePt.Y >= PreviewViewport.ClientMin.Y &&
                           mousePt.Y <= PreviewViewport.ClientMax.Y &&
                           (CurrentDecorator == null || !CurrentDecorator.IsInDecoratorOperation()))
                        {
                            ProcessSelectElement(mCurrentPointAtElement, UEngine.Instance.InputSystem.IsCtrlKeyDown());
                        }
                    }
                    break;
            }
        }

        async Thread.Async.TtTask BuildMesh()
        {
            if (mUIHost.MeshDirty)
            {
                var mesh = await mUIHost.BuildMesh();
                UIAsset.Mesh = mesh;
            }
            //mUINode.Mesh = UIAsset.Mesh;
        }

        #region Tickable
        public void TickLogic(float ellapse)
        {
            //var Mesh = UIAsset.Mesh.MaterialMesh.Mesh;
            //if (Mesh != null)
            //{
            //    var materials = UIAsset.Mesh.MaterialMesh.Materials;
            //    UIAsset.Mesh.UpdateMesh(Mesh, materials);
            //}

            PreviewViewport.TickLogic(ellapse);

            if(IsUIControlsDirty)
            {
                CollectionUIControls();
                IsUIControlsDirty = false;
            }
            if(mIsSimulateMode)
            {
                if (UIAsset.Mesh != mUIHost.DrawMesh)
                    UIAsset.Mesh = mUIHost.DrawMesh;
            }
            else
            {
                _ = UpdateDecorator();
                _ = BuildMesh();
            }
        }
        public void TickRender(float ellapse)
        {
            PreviewViewport.TickRender(ellapse);
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {
            PreviewViewport.TickSync(ellapse);
        }
        #endregion

        struct MessageData
        {
            public string Message;
            public float ShowTimeSecond;

            public void Tick(float elapsedTime)
            {
                ShowTimeSecond -= elapsedTime;
            }
        }
        List<MessageData> mMessages = new List<MessageData>();
        void ShowMessage(string message, float showTimeInSecond = 2.0f)
        {
            var data = new MessageData()
            {
                Message = message,
                ShowTimeSecond = showTimeInSecond,
            };
            mMessages.Add(data);
        }
    }
}
