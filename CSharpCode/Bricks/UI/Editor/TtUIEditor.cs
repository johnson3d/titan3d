using EngineNS;
using EngineNS.GamePlay.Scene;
using EngineNS.Rtti;
using EngineNS.UI.Canvas;
using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EngineNS.UI.Editor
{
    public class TtUIEditor : EngineNS.Editor.IAssetEditor, IRootForm, ITickable
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

        public TtUIHost mUIHost = new TtUIHost();
        public UMeshNode mUINode;
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref UIAsset);
            CoreSDK.DisposeObject(ref PreviewViewport);
            DetailsGrid.Target = null;
        }

        public async Task<bool> Initialize()
        {
            await DetailsGrid.Initialize();

            mUIHost.WindowSize = new SizeF(1920, 1080);

            // test /////////////////////////
            var element = new TtImage(); // new TtUIElement();
            var canvas = new UI.Controls.Containers.TtCanvasControl();
            canvas.AddChild(element);
            UI.Controls.Containers.TtCanvasControl.SetAnchorRectX(element, 50);
            //UI.Controls.Containers.TtCanvasControl.SetAnchorRectZ(element, 1920);
            //UI.Controls.Containers.TtCanvasControl.SetAnchorRectW(element, 1080);
            DetailsGrid.Target = element;
            mUIHost.AddChild(canvas);
            /////////////////////////////////

            return true;
        }
        protected async System.Threading.Tasks.Task Initialize_PreviewMaterialInstance(Graphics.Pipeline.UViewportSlate viewport, USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();

            (viewport as EngineNS.Editor.UPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var scene = PreviewViewport.World.Root.GetNearestParentScene();
            mUINode = await scene.NewNode(PreviewViewport.World, typeof(UMeshNode), new GamePlay.Scene.UMeshNode.UMeshNodeData(),
                EBoundVolumeType.Box, typeof(GamePlay.UPlacement)) as UMeshNode;
            mUINode.NodeData.Name = "UI";
            mUINode.Parent = PreviewViewport.World.Root;
            mUINode.Placement.SetTransform(DVector3.Zero, Vector3.One, Quaternion.Identity);
            mUINode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
            mUINode.IsAcceptShadow = false;
            mUINode.IsCastShadow = false;

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
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public EGui.Controls.PropertyGrid.PropertyGrid DetailsGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public unsafe void OnDraw()
        {
            if (Visible == false || UIAsset == null || UIAsset.Mesh == null)
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
                
                ImGuiAPI.Separator();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm();

            DrawControls();
            DrawDesigner();
            DrawDetails();
            DrawHierachy();
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
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
        void DrawControlItemData(ControlItemData itemData)
        {
            var flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;
            if (itemData.Children.Count == 0)
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;
            var treeNodeResult = ImGuiAPI.TreeNodeEx(itemData.Name, flags);
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
                PreviewViewport.VieportType = Graphics.Pipeline.UViewportSlate.EVieportType.ChildWindow;
                PreviewViewport.OnDraw();
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
        protected unsafe void DrawHierachy()
        {
            var sz = new Vector2(-1);
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Hierachy", ref mHierachyShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
            }
            EGui.UIProxy.DockProxy.EndPanel();
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
            //throw new NotImplementedException();
        }
        public async System.Threading.Tasks.Task<bool> OpenEditor(EngineNS.Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            UIAsset = new TtUIAsset();
            UIAsset.AssetName = name;
            //UIAsset.Mesh = await UI.Canvas.TtCanvas.TestCreate();
            
            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"UI:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewMaterialInstance;
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.SlateApplication, UEngine.Instance.Config.MainRPolicyName, 0, 1);

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

        async Thread.Async.TtTask BuildMesh()
        {
            if(mUIHost.MeshDirty)
            {
                var mesh = await mUIHost.BuildMesh();
                UIAsset.Mesh = mesh;
                mUINode.Mesh = mesh;
            }
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
            _ = BuildMesh();
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
    }
}
