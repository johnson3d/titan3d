using EngineNS;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.Input;
using EngineNS.EGui.UIProxy;
using EngineNS.GamePlay.Scene;
using EngineNS.Rtti;
using EngineNS.UI.Canvas;
using EngineNS.UI.Controls;
using EngineNS.UI.Controls.Containers;
using Microsoft.VisualBasic;
using NPOI.SS.Formula.UDF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Permissions;
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

        public EditorUIHost mUIHost;
        public TtUINode mUINode;
        Vector2 mNewCreateUISize = new Vector2(100, 50);
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref UIAsset);
            CoreSDK.DisposeObject(ref PreviewViewport);
            DetailsGrid.Target = null;
        }

        public async Task<bool> Initialize()
        {
            await DetailsGrid.Initialize();
            DetailsGrid.HostEditor = this;

            if (mUIHost == null)
                mUIHost = new EditorUIHost(this);
            mUIHost.Name = "UIEditorHost";
            var newRect = UEngine.Instance.UIManager.Config.DefaultDesignRect;
            mDesignResolution = new Vector2i((int)newRect.Width, (int)newRect.Height);
            mUIHost.WindowSize = newRect.Size;
            mUIHost.SetDesignRect(in newRect, true);
            mUIHost.ViewportSlate = PreviewViewport;

            PreviewViewport.CameraMouseWheelSpeed = 50.0f;

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
            mUIHost.AddToNode(mUINode);

            mUIHost.RenderCamera = PreviewViewport.RenderPolicy.DefaultCamera;

            //var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(),
            //            typeof(GamePlay.UPlacement), UIAsset.Mesh,
            //            DVector3.Zero, Vector3.One, Quaternion.Identity);
            //meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
            //meshNode.NodeData.Name = "PreviewObject";
            //meshNode.IsAcceptShadow = false;
            //meshNode.IsCastShadow = true;

            //var aabb = UIAsset.Mesh.MaterialMesh.Mesh.mCoreObject.mAABB;
            if(mUIHost.IsScreenSpace)
            {

            }
            else
            { 
                float radius = Math.Max(mUIHost.WindowSize.Width, mUIHost.WindowSize.Height);
                BoundingSphere sphere;
                sphere.Center = new Vector3(mUIHost.WindowSize.Width * 0.5f, mUIHost.WindowSize.Height * 0.5f, 0);
                sphere.Radius = radius;
                policy.DefaultCamera.AutoZoom(ref sphere);
            }

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
        public enum enDrawType : byte
        {
            Designer,
            Macross,
        }
        enDrawType mDrawType = enDrawType.Designer;
        public enDrawType DrawType
        {
            get => mDrawType;
            set
            {
                mDrawType = value;
                switch(mDrawType)
                {
                    case enDrawType.Designer:
                        mDockInitialized = false;
                        break;
                    case enDrawType.Macross:
                        UIAsset.MacrossEditor.DockInitialized = false;
                        break;
                }
            }
        }

        public unsafe void OnDraw()
        {
            if (Visible == false || UIAsset == null)
                return;

            switch(mDrawType)
            {
                case enDrawType.Macross:
                    OnDrawMacrossWindow();
                    break;
                case enDrawType.Designer:
                    OnDrawDesignerWindow();
                    break;
            }
        }

        public unsafe void OnDrawDesignerWindow()
        {
            //mDragTips = mDragItemName;
            //mDragState = EDragState.None;

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

            DrawDesigner();
            DrawDetails();
            DrawHierachy();
            DrawControls();
            if (!mIsDragDroping)
            {
                mDragTips = mDragItemName;
                mDragState = EDragState.None;
            }

            ImGuiAPI.SetMouseCursor(mMouseCursor);
        }
        bool mIsSimulateMode = false;
        void Save()
        {
            UEngine.Instance.UIManager.Save(AssetName, mUIHost.Children[0]);
            UIAsset.MacrossEditor.SaveClassGraph(AssetName);
            UIAsset.MacrossEditor.GenerateCode();
            UIAsset.MacrossEditor.CompileCode();
        }
        protected unsafe void DrawToolBar()
        {
            var drawList = ImGuiAPI.GetWindowDrawList();
            EGui.UIProxy.Toolbar.BeginToolbar(drawList);
            var btSize = Vector2.Zero;
            if(EGui.UIProxy.CustomButton.ToolButton("Show Graph", in btSize,
                EGui.UIProxy.StyleConfig.Instance.ToolButtonTextColor,
                EGui.UIProxy.StyleConfig.Instance.ToolButtonTextColor_Press,
                EGui.UIProxy.StyleConfig.Instance.ToolButtonTextColor_Hover,
                EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGColor,
                EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGActiveColor,
                EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGHoverColor
                ))
            {
                DrawType = enDrawType.Macross;
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                Save();
                //Mesh.SaveAssetTo(Mesh.AssetName);
                //var unused = UEngine.Instance.GfxDevice.MaterialInstanceManager.ReloadMaterialInstance(Mesh.AssetName);

                //USnapshot.Save(Mesh.AssetName, Mesh.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Reload", in btSize))
            {

            }
            ImGuiAPI.SameLine(0, -1);
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.UAnyPointer.Default);
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Undo", in btSize))
            {

            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Redo", in btSize))
            {

            }
            ImGuiAPI.SameLine(0, -1);
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.UAnyPointer.Default);
            ImGuiAPI.SameLine(0, -1);
            if(EGui.UIProxy.CustomButton.ToggleButton("Simulate", in btSize, ref mIsSimulateMode))
            {
                PreviewViewport.FreezCameraControl = mIsSimulateMode;
                UEngine.Instance.UIManager.KeyboardFocus(null, null);
                UEngine.Instance.UIManager.CaptureMouse(null, null);
                if (mIsSimulateMode)
                {
                    if (mUIHost.Children.Count > 0)
                    {
                        if (mUIHost.Children[0].MacrossGetter == null)
                        {
                            mUIHost.Children[0].MacrossGetter = Macross.UMacrossGetter<TtUIMacrossBase>.NewInstance();
                            mUIHost.Children[0].MacrossGetter.Name = AssetName;
                        }
                        var mc = mUIHost.Children[0].MacrossGetter.Get();
                        mc.HostElement = mUIHost.Children[0];
                        mc.Initialize();
                    }

                    UEngine.Instance.UIManager.AddUI(AssetName, "UIEditorSimulate", mUIHost);
                }
                else
                {
                    UEngine.Instance.UIManager.RemoveUI(AssetName, "UIEditorSimulate");
                }
            }
            EGui.UIProxy.Toolbar.EndToolbar();
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
        protected struct ControlCreateDragData
        {
            public string TypeName;
        }
        protected struct ControlMoveDragData
        {
            public TtUIElement[] Elements;
        }

        string mDragTips;
        string mDragItemName;
        bool mIsDragDroping = false;
        enum EDragState : byte
        {
            None,
            Add,
            InsertBefore,
            InsertAfter,
            Failed,
        }
        EDragState mDragState = EDragState.None;
        unsafe void DrawControlItemData(ControlItemData itemData)
        {
            var flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;
            if (itemData.Children.Count == 0)
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;
            var treeNodeResult = ImGuiAPI.TreeNodeEx(itemData.Name, flags);
            if(itemData.UIControlType != null)
            {
                if(ImGuiAPI.BeginDragDropSource(ImGuiDragDropFlags_.ImGuiDragDropFlags_SourceNoDisableHover))
                {
                    var data = new ControlCreateDragData()
                    {
                        TypeName = UTypeDescManager.Instance.GetTypeStringFromType(itemData.UIControlType),
                    };
                    var handle = GCHandle.Alloc(data);
                    ImGuiAPI.SetDragDropPayload("UIControlCreateDragDrop", GCHandle.ToIntPtr(handle).ToPointer(), (uint)Marshal.SizeOf<ControlCreateDragData>(), ImGuiCond_.ImGuiCond_None);
                    mDragItemName = itemData.Name;
                    switch (mDragState)
                    {
                        case EDragState.Add:
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, StyleConfig.Instance.PassStringColor);
                            break;
                        case EDragState.Failed:
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, StyleConfig.Instance.ErrorStringColor);
                            break;
                        case EDragState.InsertAfter:
                        case EDragState.InsertBefore:
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, 0xFF00FFFF);
                            break;
                        default:
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, StyleConfig.Instance.TextColor);
                            break;
                    }
                    ImGuiAPI.Text(mDragTips);
                    ImGuiAPI.PopStyleColor(1);
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
        public static string GetElementShowName(TtUIElement element)
        {
            if (element == null)
                return "";
            return element.GetEditorShowName();
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
                var dragDropPayload = ImGuiAPI.GetDragDropPayload();
                if (dragDropPayload != null)
                {
                    var curPos = new Vector2(UEngine.Instance.InputSystem.Mouse.EventMouseX, UEngine.Instance.InputSystem.Mouse.EventMouseY);
                    Vector2 pointOffset;
                    var element = mUIHost.GetPointAtElement(in curPos, out pointOffset);
                    if(element != null)
                    {
                        var container = element as TtContainer;
                        if (container == null)
                            container = element.Parent;
                        var handle = GCHandle.FromIntPtr((IntPtr)(dragDropPayload->Data));
                        if(handle.Target is ControlCreateDragData)
                        {
                            mIsDragDroping = mIsDragDroping || true;
                            var data = (ControlCreateDragData)handle.Target;
                            var ctrlType = UTypeDesc.TypeOf(data.TypeName);
                            var name = GetElementShowName(container);
                            if (container.CanAddChild(ctrlType))
                            {
                                mDragTips = $"Add {mDragItemName} to {name}";
                                mDragState = EDragState.Add;
                                if (ImGuiAPI.BeginDragDropTarget())
                                {
                                    //var curPos = ImGuiAPI.GetMousePos() - pos;
                                    DropToCreateUIControl(container, 0, mNewCreateUISize,
                                        (container) =>
                                        {
                                            Vector2 offset;
                                            container.GetElementPointAtPos(in curPos, out offset);
                                            return offset;
                                        });
                                    ImGuiAPI.EndDragDropTarget();
                                }
                            }
                            else
                            {
                                mDragTips = $"{name} can't add {mDragItemName}!";
                                mDragState = EDragState.Failed;
                            }
                        }
                        //else
                        //{
                        //    mDragTips = mDragItemName;
                        //    mDragState = EDragState.None;
                        //}
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
                //if (ImGuiAPI.BeginChild($"Details_VIValues", in sz, false, ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar))
                {
                    if (mSelectedElements.Count > 1)
                    {
                        string name = "Multiple Values";
                        ImGuiAPI.InputText("##Details_Name", ref name, ImGuiInputTextFlags_.ImGuiInputTextFlags_ReadOnly);
                        bool firstValue = mSelectedElements[0].IsVariable;
                        bool sameValue = true;
                        for(int i=1; i<mSelectedElements.Count; i++)
                        {
                            if (firstValue != mSelectedElements[i].IsVariable)
                            {
                                sameValue = false;
                                break;
                            }
                        }
                        int checkValue = -1;
                        if(sameValue)
                        {
                            checkValue = firstValue ? 1 : 0;
                        }
                        ImGuiAPI.SameLine(0, -1);
                        if (ImGuiAPI.CheckBoxTristate("Is Variable##Details", ref checkValue))
                        {
                            if(checkValue == 1)
                            {
                                for(int i=0; i<mSelectedElements.Count; i++)
                                {
                                    // add variables to class
                                    var element = mSelectedElements[i];
                                    element.IsVariable = true;
                                    if(string.IsNullOrEmpty(element.Name))
                                    {
                                        element.Name = GetValidName(element);
                                    }
                                    var varName = GetUIElementMacrossVariableName(element);
                                    var elementVariable = new UVariableDeclaration()
                                    {
                                        VariableType = new UTypeReference(element.GetType()),
                                        VariableName = varName,
                                        GetDisplayNameFunc = element.GetVariableDisplayName,
                                        InitValue = new UNullValueExpression(),
                                        VisitMode = EVisisMode.Public,
                                    };
                                    if (UIAsset.MacrossEditor.DefClass.FindMember(varName) == null)
                                        UIAsset.MacrossEditor.DefClass.Properties.Add(elementVariable);
                                }
                            }
                            else if(checkValue == 0)
                            {
                                for(int i=0; i<mSelectedElements.Count; i++)
                                {
                                    // remove variable from class
                                    UIAsset.MacrossEditor.DefClass.RemoveMember(GetUIElementMacrossVariableName(mSelectedElements[i]));
                                    mSelectedElements[i].IsVariable = false;
                                }
                            }
                        }
                    }
                    else if(mSelectedElements.Count == 1)
                    {
                        var element = mSelectedElements[0];
                        string name = element.Name;
                        if(ImGuiAPI.InputText("##Details_Name", ref name, ImGuiInputTextFlags_.ImGuiInputTextFlags_AutoSelectAll | ImGuiInputTextFlags_.ImGuiInputTextFlags_EnterReturnsTrue))
                            element.Name = name;
                        ImGuiAPI.SameLine(0, -1);
                        bool isVariable = element.IsVariable;
                        if(ImGuiAPI.Checkbox("Is Variable##Details", ref isVariable))
                        {
                            element.IsVariable = isVariable;
                            if(isVariable)
                            {
                                // add variable to class
                                var varName = GetUIElementMacrossVariableName(element);
                                var elementVariable = new UVariableDeclaration()
                                {
                                    VariableType = new UTypeReference(element.GetType()),
                                    VariableName = varName,
                                    GetDisplayNameFunc = element.GetVariableDisplayName,
                                    InitValue = new UNullValueExpression(),
                                    VisitMode = EVisisMode.Public,
                                };
                                if(UIAsset.MacrossEditor.DefClass.FindMember(varName) == null)
                                    UIAsset.MacrossEditor.DefClass.Properties.Add(elementVariable);
                            }
                            else
                            {
                                // remove variable from class
                                UIAsset.MacrossEditor.DefClass.RemoveMember(GetUIElementMacrossVariableName(element));
                            }
                        }
                    }
                    else
                    {
                        ImGuiAPI.Text("Nothing selected");
                    }
                }
                //ImGuiAPI.EndChild();

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
                int idx = 0;
                DrawUIElementInHierachy(mUIHost, ref idx);
                ImGuiAPI.PopStyleColor(3);
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        void DrawHierachyContextMenu(TtUIElement element, string name)
        {
            if(ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
            {
                if (!mSelectedElements.Contains(element))
                    ProcessSelectElement(element, UEngine.Instance.InputSystem.IsCtrlKeyDown());
                ImGuiAPI.OpenPopup("##ContextMenu_" + name, ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            if(ImGuiAPI.BeginPopup("##ContextMenu_" + name, ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize | ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                Support.UAnyPointer menuData = new Support.UAnyPointer();
                string menuName = "Delete ";
                if (mSelectedElements.Count == 1)
                    menuName += name;
                else
                    menuName += $"{mSelectedElements.Count} items";
                if (EGui.UIProxy.MenuItemProxy.MenuItem(menuName, null, false, null, drawList, menuData, ref element.HierachyContextMenuState))
                {
                    for(int i=0; i<mSelectedElements.Count; i++)
                    {
                        if (mSelectedElements[i] == mUIHost)
                            continue;
                        if (mSelectedElements[i].TemplateParent != null)
                            continue;
                        mSelectedElements[i].Parent.Children.Remove(mSelectedElements[i]);
                    }
                    ProcessSelectElement(null, false);
                }
                ImGuiAPI.EndPopup();
            }
        }
        unsafe void DropToCreateUIControl(TtUIElement element, sbyte type, in Vector2 size, Func<TtContainer, Vector2> getOffsetFunc)
        {
            TtContainer parent;
            switch(type)
            {
                case -1:
                case 1:
                    parent = element.Parent;
                    break;
                default:
                    parent = element as TtContainer;
                    break;
            }
            if (parent == null)
                return;
            var payload = ImGuiAPI.AcceptDragDropPayload("UIControlCreateDragDrop", ImGuiDragDropFlags_.ImGuiDragDropFlags_None);
            if (payload != null)
            {
                var handle = GCHandle.FromIntPtr((IntPtr)(payload->Data));
                var data = (ControlCreateDragData)handle.Target;

                var ctrlType = UTypeDesc.TypeOf(data.TypeName);
                if (parent.CanAddChild(ctrlType))
                {
                    var uiControl = UTypeDescManager.CreateInstance(ctrlType) as TtUIElement;
                    uiControl.Name = GetValidName(uiControl);
                    switch(type)
                    {
                        case -1:
                            {
                                var idx = parent.Children.IndexOf(element);
                                parent.Children.Insert(idx, uiControl);
                            }
                            break;
                        case 0:
                            parent.Children.Add(uiControl);
                            break;
                        case 1:
                            {
                                var idx = parent.Children.IndexOf(element);
                                if (idx + 1 >= parent.Children.Count)
                                    parent.Children.Add(uiControl);
                                else
                                    parent.Children.Insert(idx + 1, uiControl);
                            }
                            break;
                    }
                    Vector2 offset = getOffsetFunc.Invoke(parent);
                    parent.ProcessNewAddChild(uiControl, offset, size);
                }

                //handle.Free();
            }
        }
        unsafe bool CheckMoveDropValid(TtUIElement element, out sbyte type)
        {
            type = 0;
            var dragDropPayload = ImGuiAPI.GetDragDropPayload();
            if (dragDropPayload == null)
            {
                //mDragTips = mDragItemName;
                //mDragState = EDragState.None;
                return false;
            }
            if (!dragDropPayload->IsDataType("UIControlMoveDragDrop"))
            {
                //mDragTips = mDragItemName;
                //mDragState = EDragState.None;
                return false;
            }
            //if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_AllowWhenBlockedByActiveItem))
            {
                var mousePos = ImGuiAPI.GetMousePos();
                var itemMin = ImGuiAPI.GetItemRectMin();
                var itemSize = ImGuiAPI.GetItemRectSize();
                var delta = (mousePos - itemMin).Y / itemSize.Y;
                if (delta < 0.2)
                    type = -1;
                else if (delta > 0.8)
                    type = 1;
                var name = GetElementShowName(element);
                var container = element as TtContainer;
                if (element.TemplateParent != null)
                {
                    mDragTips = $"Can't move {mDragItemName} into template element!";
                    mDragState = EDragState.Failed;
                    return false;
                }
                var handle = GCHandle.FromIntPtr((IntPtr)(dragDropPayload->Data));
                var data = (ControlMoveDragData)handle.Target;
                if(type != 0)
                    container = element.Parent;
                bool result = true;
                if (container != null)
                {
                    for (int i = 0; i < data.Elements.Length; i++)
                    {
                        if (element == data.Elements[i])
                        {
                            result = false;
                            break;
                        }
                        if (data.Elements[i].FindElement(element.Id) != null)
                        {
                            result = false;
                            break;
                        }
                        result = result && container.CanAddChild(Rtti.UTypeDesc.TypeOf(data.Elements[i].GetType()));
                    }
                }
                else
                    result = false;
                if (result)
                {
                    switch(type)
                    {
                        case -1:
                            mDragState = EDragState.InsertBefore;
                            mDragTips = $"Move {mDragItemName} before {name}";
                            break;
                        case 1:
                            mDragState = EDragState.InsertAfter;
                            mDragTips = $"Move {mDragItemName} after {name}";
                            break;
                        case 0:
                            mDragState = EDragState.Add;
                            mDragTips = $"Move {mDragItemName} into {name}";
                            break;
                    }
                    return true;
                }
                else
                {
                    switch (type)
                    {
                        case -1:
                            mDragTips = $"Can't move {mDragItemName} before {name}";
                            break;
                        case 1:
                            mDragTips = $"Can't move {mDragItemName} after {name}";
                            break;
                        case 0:
                            mDragTips = $"Can't move {mDragItemName} into {name}";
                            break;
                    }
                    mDragState = EDragState.Failed;
                    return false;
                } 
            }
            //return false;
        }
        unsafe bool CheckCreateDropValid(TtUIElement element, out sbyte type)
        {
            type = 0;
            var dragDropPayload = ImGuiAPI.GetDragDropPayload();
            if (dragDropPayload == null)
            {
                //mDragTips = mDragItemName;
                //mDragState = EDragState.None;
                return false;
            }
            if (!dragDropPayload->IsDataType("UIControlCreateDragDrop"))
            {
                //mDragTips = mDragItemName;
                //mDragState = EDragState.None;
                return false;
            }
            //if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_AllowWhenBlockedByActiveItem))
            {
                var mousePos = ImGuiAPI.GetMousePos();
                var itemMin = ImGuiAPI.GetItemRectMin();
                var itemSize = ImGuiAPI.GetItemRectSize();
                var delta = (mousePos - itemMin).Y / itemSize.Y;
                if (delta < 0.2)
                    type = -1;
                else if (delta > 0.8)
                    type = 1;
                var name = GetElementShowName(element);
                var container = element as TtContainer;
                if(element.TemplateParent != null)
                {
                    mDragTips = $"Can't add {mDragItemName} in template element!";
                    mDragState = EDragState.Failed;
                    return false;
                }
                var handle = GCHandle.FromIntPtr((IntPtr)(dragDropPayload->Data));
                var data = (ControlCreateDragData)handle.Target;
                var ctrlType = UTypeDesc.TypeOf(data.TypeName);
                if (type != 0)
                    container = element.Parent;
                if (container != null && container.CanAddChild(ctrlType))
                {
                    switch (type)
                    {
                        case -1:
                            mDragState = EDragState.InsertBefore;
                            mDragTips = $"Insert {mDragItemName} before {name}";
                            break;
                        case 1:
                            mDragState = EDragState.InsertAfter;
                            mDragTips = $"Insert {mDragItemName} after {name}";
                            break;
                        case 0:
                            mDragState = EDragState.Add;
                            mDragTips = $"Add {mDragItemName} to {name}";
                            break;
                    }
                    return true;
                }
                else
                {
                    switch (type)
                    {
                        case -1:
                            mDragTips = $"Can't insert {mDragItemName} before {name}";
                            break;
                        case 1:
                            mDragTips = $"Can't insert {mDragItemName} after {name}";
                            break;
                        case 0:
                            mDragTips = $"{name} can't add {mDragItemName}!";
                            break;
                    }
                    mDragState = EDragState.Failed;
                    return false;
                }
            }
        }
        unsafe void DrawUIElementInHierachy(TtUIElement element, ref int idx)
        {
            if (element.TemplateParent != null)
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, StyleConfig.Instance.TextDisableColor);
            else if (element is TtTemplateContainer)
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, 0xffbb7200);
            else
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, StyleConfig.Instance.TextColor);

            var flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;
            var name = GetElementShowName(element) + "##" + idx++;
            if (mSelectedElements.Contains(element))
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
            var container = element as TtContainer;
            Vector2 itemMin = Vector2.Zero, itemMax = Vector2.Zero;
            int childrenCount = 0;
            if(container != null)
            {
                if(mShowTemplateControls)
                {
                    childrenCount = VisualTreeHelper.GetChildrenCount(container, VisualTreeHelper.EFlag.PassContentsPresenter);
                }
                else
                {
                    childrenCount = container.Children.Count;
                }
            }

            if (childrenCount == 0)
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;
            var treeNodeResult = ImGuiAPI.TreeNodeEx(name, flags);
            itemMin = ImGuiAPI.GetItemRectMin();
            itemMax = ImGuiAPI.GetItemRectMax();

            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
            {
                ProcessSelectElement(element, UEngine.Instance.InputSystem.IsCtrlKeyDown());
            }
            if (ImGuiAPI.BeginDragDropTarget())
            {
                mIsDragDroping = mIsDragDroping || true;
                sbyte dropType = 0;
                if(CheckCreateDropValid(element, out dropType))
                {
                    DropToCreateUIControl(element, dropType, mNewCreateUISize,
                        (container) =>
                        {
                            return new Vector2(0, 0);
                        });
                }
                DrawInsertLine(dropType, itemMin, itemMax);

                if (CheckMoveDropValid(element, out dropType))
                {
                    TtContainer parent;
                    switch (dropType)
                    {
                        case -1:
                        case 1:
                            parent = element.Parent;
                            break;
                        default:
                            parent = element as TtContainer;
                            break;
                    }
                    if (parent != null)
                    {
                        var payload = ImGuiAPI.AcceptDragDropPayload("UIControlMoveDragDrop", ImGuiDragDropFlags_.ImGuiDragDropFlags_None);
                        if(payload != null)
                        {
                            var handle = GCHandle.FromIntPtr((IntPtr)(payload->Data));
                            var data = (ControlMoveDragData)handle.Target;
                            for(int i=0; i<data.Elements.Length; i++)
                            {
                                switch(dropType)
                                {
                                    case -1:
                                        {
                                            data.Elements[i].Parent.Children.Remove(data.Elements[i]);
                                            var childIdx = parent.Children.IndexOf(element);
                                            parent.Children.Insert(childIdx, data.Elements[i]);
                                        }
                                        break;
                                    case 0:
                                        parent.Children.Add(data.Elements[i]);
                                        break;
                                    case 1:
                                        {
                                            data.Elements[i].Parent.Children.Remove(data.Elements[i]);
                                            var childIdx = parent.Children.IndexOf(element);
                                            if (childIdx + 1 >= parent.Children.Count)
                                                parent.Children.Add(data.Elements[i]);
                                            else
                                                parent.Children.Insert(childIdx + 1, data.Elements[i]);
                                        }
                                        break;
                                }
                                // calculate offset
                                Vector2 offset;
                                data.Elements[i].GetOffsetFromElement(parent, out offset);
                                parent.ProcessNewAddChild(data.Elements[i], offset, mNewCreateUISize);
                            }
                        }
                    }
                }
                DrawInsertLine(dropType, itemMin, itemMax);
                ImGuiAPI.EndDragDropTarget();
            }
            //else
            //{
            //    mDragTips = mDragItemName;
            //    mDragState = EDragState.None;
            //}
            if (ImGuiAPI.BeginDragDropSource(ImGuiDragDropFlags_.ImGuiDragDropFlags_SourceNoDisableHover))
            {
                var data = new ControlMoveDragData();
                data.Elements = new TtUIElement[mSelectedElements.Count];
                mSelectedElements.CopyTo(data.Elements, 0);
                var handle = GCHandle.Alloc(data);
                ImGuiAPI.SetDragDropPayload("UIControlMoveDragDrop", GCHandle.ToIntPtr(handle).ToPointer(), (uint)Marshal.SizeOf<ControlMoveDragData>(), ImGuiCond_.ImGuiCond_None);
                mDragItemName = (mSelectedElements.Count == 1) ? GetElementShowName(mSelectedElements[0]) : (mSelectedElements.Count + " items");
                switch (mDragState)
                {
                    case EDragState.Add:
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, StyleConfig.Instance.PassStringColor);
                        break;
                    case EDragState.Failed:
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, StyleConfig.Instance.ErrorStringColor);
                        break;
                    case EDragState.InsertAfter:
                    case EDragState.InsertBefore:
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, 0xFF00FFFF);
                        break;
                    default:
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, StyleConfig.Instance.TextColor);
                        break;
                }
                ImGuiAPI.Text(mDragTips);
                ImGuiAPI.PopStyleColor(1);
                ImGuiAPI.EndDragDropSource();
            }
            DrawHierachyContextMenu(element, name);
            if (treeNodeResult)
            {
                if(container != null)
                {
                    container.TourContentsPresenterContainers(TourContentsPresenterContainersAction, ref idx);
                    if(mShowTemplateControls)
                    {
                        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(container, VisualTreeHelper.EFlag.PassContentsPresenter); i++)
                        {
                            var child = VisualTreeHelper.GetChild(container, i, VisualTreeHelper.EFlag.PassContentsPresenter);
                            DrawUIElementInHierachy(child, ref idx);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < container.Children.Count; i++)
                        {
                            DrawUIElementInHierachy(container.Children[i], ref idx);
                        }
                    }
                }
                ImGuiAPI.TreePop();
            }

            ImGuiAPI.PopStyleColor(1);
        }
    
        void TourContentsPresenterContainersAction(TtUIElement element, ref int idx)
        {
            DrawUIElementInHierachy(element, ref idx);
        }
        void DrawInsertLine(sbyte dropType, in Vector2 itemMin, in Vector2 itemMax)
        {
            if(mDragState != EDragState.Failed)
            {
                switch(dropType)
                {
                    case -1:
                        {
                            var drawList = ImGuiAPI.GetWindowDrawList();
                            drawList.AddLine(itemMin, new Vector2(itemMax.X, itemMin.Y), 0xFF00FFFF, 1.0f);
                        }
                        break;
                    case 1:
                        {
                            var drawList = ImGuiAPI.GetWindowDrawList();
                            drawList.AddLine(new Vector2(itemMin.X, itemMax.Y), new Vector2(itemMax.X, itemMax.Y), 0xFF00FFFF, 1.0f);
                        }
                        break;
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
            else if(element.IsSelectedable)
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
        void ResetCamera()
        {
            if (mUIHost.IsScreenSpace)
            {

            }
            else
            {
                BoundingSphere sphere;
                sphere.Center = new Vector3(mUIHost.WindowSize.Width * 0.5f, mUIHost.WindowSize.Height * 0.5f, 0);
                sphere.Radius = Math.Max(mUIHost.WindowSize.Width, mUIHost.WindowSize.Height);
                mUIHost.RenderCamera.LookAtLH(-DVector3.UnitZ, DVector3.Zero, Vector3.UnitY);
                mUIHost.RenderCamera.AutoZoom(ref sphere);
            }
        }
        bool mIsWireFrame = false;
        string mDimensionToolButtonName = "3D";
        void DrawViewportUIAction(in Vector2 startDrawPos)
        {
            if (AssetName != null)
            {
                if (EGui.UIProxy.CustomButton.ToolButton("S", in Vector2.Zero))
                    EngineNS.Editor.USnapshot.Save(AssetName, UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName), PreviewViewport.RenderPolicy.GetFinalShowRSV());
                ImGuiAPI.SameLine(0, -1);
            }
            if (EGui.UIProxy.CustomButton.ToolButton("Reset Camera", in Vector2.Zero))
            {
                ResetCamera();
            }
            ImGuiAPI.SameLine(0, -1);
            if(EGui.UIProxy.CustomButton.ToolButton("Focus", in Vector2.Zero))
            {
                if(mSelectedElements.Count > 0)
                {
                    if(mUIHost.IsScreenSpace)
                    {

                    }
                    else
                    {
                        Vector2 rectMin = Vector2.MaxValue;
                        Vector2 rectMax = Vector2.MinValue;
                        for(int i=0; i<mSelectedElements.Count; i++)
                        {
                            Vector2 offset;
                            var designRect = mSelectedElements[i].DesignRect;
                            mSelectedElements[i].GetOffsetFromElement(mUIHost, out offset);
                            var min = new Vector2(designRect.Left, designRect.Top) + offset;
                            if (min.X < rectMin.X)
                                rectMin.X = min.X;
                            if (min.Y < rectMin.Y)
                                rectMin.Y = min.Y;
                            var max = new Vector2(designRect.Right, designRect.Bottom) + offset;
                            if(max.X > rectMax.X)
                                rectMax.X = max.X;
                            if (max.Y > rectMax.Y)
                                rectMax.Y = max.Y;
                        }
                        var size = rectMax - rectMin;
                        rectMax.Y = mUIHost.DesignRect.Height - rectMax.Y;
                        rectMin.Y = mUIHost.DesignRect.Height - rectMin.Y;
                        BoundingSphere sphere;
                        sphere.Center = new Vector3((rectMin.X + rectMax.X) * 0.5f, (rectMin.Y + rectMax.Y) * 0.5f, 0.0f);
                        sphere.Radius = ((size.X > size.Y) ? size.X : size.Y);
                        mUIHost.RenderCamera.AutoZoom(ref sphere, 0.3f);
                    }
                }
                else
                {
                    ResetCamera();
                }
            }
            ImGuiAPI.SameLine(0, -1);
            if(EGui.UIProxy.CustomButton.ToggleButton("Wireframe", in Vector2.Zero, ref mIsWireFrame))
            {
                async Thread.Async.TtTask WireFrameProcess()
                {
                    if (mIsWireFrame)
                    {
                        var mtl = await UEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("material/wireframe_red.uminst", RName.ERNameType.Engine));
                        mUIHost.DrawMesh.UpdateMaterial(mtl);
                    }
                    else
                        mUIHost.MeshDirty = true;
                }

                _ = WireFrameProcess();
            }
            /*ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton(mDimensionToolButtonName, in Vector2.Zero))
            {
                switch (mDimensionToolButtonName)
                {
                    case "2D":
                        {
                            mDimensionToolButtonName = "3D";
                            PreviewViewport.PopHUD();
                            mUIHost.ViewportSlate = PreviewViewport;
                            mUINode.AddUIHost(mUIHost);
                            mUIHost.MeshDirty = true;
                            mUIHost.WindowSize = mUIHost.DesiredWindowSize;
                        }
                        break;
                    case "3D":
                        {
                            mDimensionToolButtonName = "2D";
                            PreviewViewport.PushHUD(mUIHost);
                            mUINode.RemoveUIHost(mUIHost);
                            mUIHost.MeshDirty = true;
                            mUIHost.WindowSize = new SizeF(PreviewViewport.ClientSize);
                        }
                        break;
                }
                mUIHost.MeshDirty = true;
            }*/
            ImGuiAPI.SameLine(0, -1);
            var str = $"Design:{mDesignResolution.X}x{mDesignResolution.Y}";
            if(EGui.UIProxy.CustomButton.ToolButton(str, in Vector2.Zero))
            {
                ImGuiAPI.OpenPopup("##DesignResolution", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            if(ImGuiAPI.BeginPopup("##DesignResolution", ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar))
            {
                unsafe
                {
                    ImGuiAPI.Text("Custom:");
                    fixed(Vector2i* resPtr = &mDesignResolution)
                    {
                        if(ImGuiAPI.InputInt2("##CustomDesignResolution", (int*)resPtr, ImGuiInputTextFlags_.ImGuiInputTextFlags_None))
                        {
                            if((mDesignResolution.X > 0) && (mDesignResolution.Y > 0))
                            {
                                if(mUIHost.IsScreenSpace)
                                    mUIHost.DesignWindowSize = new SizeF(mDesignResolution.X, mDesignResolution.Y);
                                else
                                    mUIHost.WindowSize = new SizeF(mDesignResolution.X, mDesignResolution.Y);
                            }
                        }
                    }
                }
                ImGuiAPI.EndPopup();
            }
        }
        Vector2i mDesignResolution;
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async Task<bool> OpenEditor(EngineNS.Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            mUIHost.Children.Add(UEngine.Instance.UIManager.Load(AssetName));
            UIAsset = new TtUIAsset();
            UIAsset.AssetName = name;
            //UIAsset.Mesh = await UI.Canvas.TtCanvas.TestCreate();
            await InitMacrossEditor();

            PreviewViewport.OnDrawViewportUIAction = DrawViewportUIAction;
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
                        Vector2 pointOffset;
                        var element = mUIHost.GetPointAtElement(in pt, out pointOffset);
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
                           mousePt.Y >= (PreviewViewport.ClientMin.Y + 25) && // 避开工具栏
                           mousePt.Y <= PreviewViewport.ClientMax.Y &&
                           (CurrentDecorator == null || !CurrentDecorator.IsInDecoratorOperation()))
                        {
                            ProcessSelectElement(mCurrentPointAtElement, UEngine.Instance.InputSystem.IsCtrlKeyDown());
                        }

                        if(e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_MIDDLE)
                        {
                            CalculateCameraMovingSpeed(e.MouseButton.X, e.MouseButton.Y);
                        }
                    }
                    break;
                case EventType.MOUSEWHEEL:
                    break;
            }
        }

        void CalculateCameraMovingSpeed(in float mousePointX, in float mousePointY)
        {
            if (mUIHost.IsScreenSpace)
                return;

            var hostRect = mUIHost.DesignRect;
            var transMatrix = mUIHost.AbsRenderTransform.ToMatrixWithScale(in DVector3.Zero);
            var v0 = new Vector3(hostRect.Left, hostRect.Top, 0.0f);
            v0 = Vector3.TransformCoordinate(in v0, in transMatrix);
            var v1 = new Vector3(hostRect.Right, hostRect.Top, 0.0f);
            v1 = Vector3.TransformCoordinate(in v1, in transMatrix);
            var v2 = new Vector3(hostRect.Left, hostRect.Bottom, 0.0f);
            v2 = Vector3.TransformCoordinate(in v2, in transMatrix);
            var plane = new Plane(v0, v1, v2);
            var delta = mUIHost.ViewportSlate.WindowPos - mUIHost.ViewportSlate.ViewportPos;
            float distance;
            Vector3 dir = Vector3.Zero;
            mUIHost.RenderCamera.GetPickRay(ref dir, mousePointX - delta.X, mousePointY - delta.Y, mUIHost.ViewportSlate.ClientSize.Width, mUIHost.ViewportSlate.ClientSize.Height);
            if (dir == Vector3.Zero)
                return;
            var cameraPos = PreviewViewport.CameraController.Camera.GetPosition().ToSingleVector3();
            Ray.Intersects(new Ray(in cameraPos, in dir), in plane, out distance);
            var angle = PreviewViewport.CameraController.Camera.Fov * 0.5f;
            var uiPos = mUIHost.AbsRenderTransform.Position;
            PreviewViewport.CameraMoveSpeed = distance * 0.048f;
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

        Support.UBitset mNameIndexBits = new Support.UBitset(512);
        struct ValidNameData
        {
            public Support.UBitset Bits;
            public TtUIElement Element;
            public string CheckName;
            public bool FindEqual;
        }
        bool NameIndexBitsSets(TtUIElement element, ref ValidNameData data)
        {
            if (element != data.Element && element.Name == data.CheckName)
                data.FindEqual = true;
            if(!string.IsNullOrEmpty(element.Name))
            {
                var idx = element.Name.LastIndexOf("_");
                if(idx >= 0)
                {
                    UInt32 id;
                    if (UInt32.TryParse(element.Name.Substring(idx + 1), out id))
                    {
                        if(id < data.Bits.BitCount)
                            data.Bits.SetBit(id);
                    }
                }
            }
            return false;
        }
        string GetValidName(TtUIElement element)
        {
            var name = element.Name;
            if (string.IsNullOrEmpty(name))
                name = element.GetType().Name;
            var data = new ValidNameData()
            {
                Bits = mNameIndexBits,
                Element = element,
                CheckName = name,
                FindEqual = false,
            };
            for (uint i = 0; i < 512; i++)
                mNameIndexBits.UnsetBit(i);
            mUIHost.QueryElements(NameIndexBitsSets, ref data);
            if (!data.FindEqual)
                return name;
            for(uint i=0; i<512; i++)
            {
                if (!mNameIndexBits.IsSet(i))
                    return name + "_" + i;
            }
            return "";
        }
    }
}
