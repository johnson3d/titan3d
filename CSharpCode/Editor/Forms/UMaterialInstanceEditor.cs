using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor.Forms
{
    public class UMaterialInstanceEditor : Editor.IAssetEditor, ITickable, IRootForm
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        protected ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public Graphics.Pipeline.Shader.UMaterialInstance Material;
        public Editor.UPreviewViewport PreviewViewport { get; set; } = new Editor.UPreviewViewport();
        public EGui.Controls.PropertyGrid.PropertyGrid MaterialPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public EGui.Controls.PropertyGrid.PropertyGrid EditorPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public UMaterialInstanceEditorRecorder ActionRecorder = new UMaterialInstanceEditorRecorder();
        public URenderPolicy RenderPolicy { get => PreviewViewport.RenderPolicy; }
        GamePlay.Scene.UMeshNode PreviewNode;
        ~UMaterialInstanceEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            Material = null;
            if (PreviewViewport != null)
            {
                PreviewViewport.Dispose();
                PreviewViewport = null;
            }
            MaterialPropGrid.Target = null;
            EditorPropGrid.Target = null;
            ActionRecorder?.ClearRecords();
            ActionRecorder = null;
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await MaterialPropGrid.Initialize();
            await EditorPropGrid.Initialize();
            return true;
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        protected async System.Threading.Tasks.Task Initialize_PreviewMaterialInstance(Graphics.Pipeline.UViewportSlate viewport, USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();

            (viewport as Editor.UPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var materials = new Graphics.Pipeline.Shader.UMaterial[1];
            materials[0] = Material;
            if (materials[0] == null)
                return;
            var mesh = new Graphics.Mesh.TtMesh();
            var rect = Graphics.Mesh.UMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
            var rectMesh = rect.ToMesh();
            var ok = mesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), mesh,
                    DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                meshNode.NodeData.Name = "PreviewObject";
                meshNode.IsAcceptShadow = false;
                meshNode.IsCastShadow = true;

                PreviewNode = meshNode;
            }

            //CreateAnother(viewport, rectMesh, materials);

            var aabb = mesh.MaterialMesh.AABB;
            float radius = aabb.GetMaxSide();
            BoundingSphere sphere;
            sphere.Center = aabb.GetCenter();
            sphere.Radius = radius;
            policy.DefaultCamera.AutoZoom(ref sphere);
            //this.RenderPolicy.GBuffers.SunLightColor = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SunLightDirection = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SkyLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.GroundLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.UpdateViewportCBuffer();

            var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(viewport.World, viewport.World.Root);
            gridNode.ViewportSlate = this.PreviewViewport;
        }
        async System.Threading.Tasks.Task CreateAnother(Graphics.Pipeline.UViewportSlate viewport, Graphics.Mesh.UMeshPrimitives rectMesh, Graphics.Pipeline.Shader.UMaterial[] materials)
        {
            materials[0] = Material.CloneMaterialInstance();
            materials[0].RenderLayer = ERenderLayer.RL_Translucent;
            var mesh = new Graphics.Mesh.TtMesh();
            var ok = mesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), mesh,
                    DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                meshNode.NodeData.Name = "PreviewObject1";
                meshNode.IsAcceptShadow = false;
                meshNode.IsCastShadow = true;
            }
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async Thread.Async.TtTask<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            LoadingPercent = 0;
            ProgressText = "Load Material";
            AssetName = name;
            Material = await UEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(name);
            if (Material == null)
                return false;
            LoadingPercent = 0.1f;
            ProgressText = "Initialize PreviewViewport";

            ActionRecorder.ClearRecords();
            Material.ActionRecorder = ActionRecorder;

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"Material:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewMaterialInstance;
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.SlateApplication, UEngine.Instance.Config.MainRPolicyName, 0, 1);

            LoadingPercent = 0.8f;

            MaterialPropGrid.Target = Material;
            EditorPropGrid.Target = this;
            UEngine.Instance.TickableManager.AddTickable(this);
            LoadingPercent = 1.0f;
            return true;
        }
        public void OnCloseEditor()
        {
            Material.ActionRecorder = null;
            ActionRecorder.ClearRecords();
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

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("EditorDetails", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("MaterialDetails", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Preview", mDockKeyClass), middleId);

            ImGuiAPI.DockBuilderFinish(id);
        }
        public float LeftWidth = 0;

        float ObjecRotAngle = 0;
        public float ObjectRotateSpeed { get; set; } = 0;
        bool mIsDrawing = true;
        bool IsDrawing
        {
            get => mIsDrawing;
            set
            {
                if (mIsDrawing && !value)
                {
                }
                else if (!mIsDrawing && value)
                {
                    
                }
                mIsDrawing = value;
            }
        }
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public unsafe void OnDraw()
        {
            if (Visible == false || Material == null)
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
            EGui.UIProxy.DockProxy.EndMainForm(IsDrawing);

            DrawMaterialDetails();
            DrawEditorDetails();

            DrawPreview();
        }
        bool ShowMaterialPropGrid = true;
        protected void DrawMaterialDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "MaterialDetails", ref ShowMaterialPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                MaterialPropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowEditorPropGrid = true;
        protected void DrawEditorDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "EditorDetails", ref ShowEditorPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                EditorPropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowPreview = true;
        protected unsafe void DrawPreview()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Preview", ref ShowPreview, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                PreviewViewport.ViewportType = Graphics.Pipeline.UViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();
            }
            this.PreviewViewport.Visible = show;
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                Material.SaveAssetTo(Material.AssetName);
                Material.SerialId++;
                var unused = UEngine.Instance.GfxDevice.MaterialInstanceManager.ReloadMaterialInstance(Material.AssetName);

                //USnapshot.Save(Material.AssetName, Material.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Reload", in btSize))
            {
                
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Undo", in btSize))
            {
                ActionRecorder.Undo();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Redo", in btSize))
            {
                ActionRecorder.Redo();
            }
        }
        
        public void OnEvent(in Bricks.Input.Event e)
        {
            //throw new NotImplementedException();
        }
        #region Tickable
        public void TickLogic(float ellapse)
        {
            PreviewViewport.TickLogic(ellapse);
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

    public class UMaterialInstanceEditorRecorder : GamePlay.Action.UActionRecorder
    {
        public override GamePlay.Action.UAction CurrentAction
        {
            get
            {
                if (mCurrentAction == null)
                {
                    mCurrentAction = this.NewAction();
                }
                return mCurrentAction;
            }
            set => mCurrentAction = value;
        }
        public override void OnChanged(GamePlay.Action.UAction.UPropertyModifier modifier)
        {
            if (mCurrentAction != null)
            {
                mCurrentAction.Name = $"Set:{modifier.PropertyName}";
            }
            this.CloseAction();
        }
    }
}

namespace EngineNS.Graphics.Pipeline.Shader
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.UMaterialInstanceEditor))]
    public partial class UMaterialInstance
    {
    }
}