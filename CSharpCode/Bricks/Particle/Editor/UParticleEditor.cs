using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Particle.Editor
{
    public partial class UParticleEditor : EngineNS.Editor.IAssetEditor, IO.ISerializer, ITickable, IRootForm, IGraphEditor
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public static LinkDesc NewInOutPinDesc(string linkType = "Value")
        {
            var result = new LinkDesc();
            result.Icon.Size = new Vector2(20, 20);
            result.ExtPadding = 0;
            result.LineThinkness = 3;
            result.LineColor = 0xFFFF0000;
            result.CanLinks.Add(linkType);
            return result;
        }
        public UParticleEditor()
        {
            PreviewViewport = new EngineNS.Editor.UPreviewViewport();
        }
        ~UParticleEditor()
        {
            Dispose();
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref PreviewViewport);
        }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public UNebulaParticle NebulaParticle;
        protected ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public IRootForm GetRootForm()
        {
            return this;
        }
        #region DrawUI
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public bool IsDrawing { get; set; }
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            IsDrawing = EGui.UIProxy.DockProxy.BeginMainForm(AssetName.Name, this, ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
            if (IsDrawing)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as EngineNS.Editor.UMainEditorApplication;
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

            DrawPreview();
            DrawParticleStructBuilder();
            DrawNodeDetails();
            DrawGraph();
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

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Preview", mDockKeyClass), rightUpId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("NodeDetails", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("ParticleStruct", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Graph", mDockKeyClass), middleId);

            ImGuiAPI.DockBuilderFinish(id);
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
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

        bool ShowEditorPropGrid = true;
        protected void DrawParticleStructBuilder()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "ParticleStruct", ref ShowEditorPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                ParticleStructBuilder.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowMeshPropGrid = true;
        protected void DrawNodeDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "NodeDetails", ref ShowMeshPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                NodePropGrid.OnDraw(true, false, false);
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
        bool ShowGraph = true;
        protected unsafe void DrawGraph()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Graph", ref ShowGraph, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                GraphRenderer.OnDraw();
            }
            this.PreviewViewport.Visible = show;
            EGui.UIProxy.DockProxy.EndPanel(show);

        }
        #endregion
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }
        public void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {

        }
        #endregion
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
        #region IAssetEditor
        public RName AssetName { get; set; }
        public EngineNS.Editor.UPreviewViewport PreviewViewport;
        public EGui.Controls.PropertyGrid.PropertyGrid NodePropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public float LeftWidth = 0;
        
        public TtParticleGraph ParticleGraph { get; } = new TtParticleGraph();
        public UGraphRenderer GraphRenderer { get; } = new UGraphRenderer();
        public CodeBuilder.UClassLayoutBuilder ParticleStructBuilder { get; } = new CodeBuilder.UClassLayoutBuilder();
        bool IsStarting = false;
        protected async System.Threading.Tasks.Task Initialize_PreviewParticle(Graphics.Pipeline.UViewportSlate viewport, USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            ParticleGraph.NebulaEditor = this;
            viewport.RenderPolicy = policy;

            (viewport as EngineNS.Editor.UPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var nebulaData = new Bricks.Particle.TtNebulaNode.TtNebulaNodeData();
            nebulaData.NebulaName = AssetName;
            var meshNode = new Bricks.Particle.TtNebulaNode();
            await meshNode.InitializeNode(viewport.World, nebulaData, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
            meshNode.Parent = viewport.World.Root;
            meshNode.Placement.Position = DVector3.Zero;
            meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
            meshNode.NodeData.Name = "NebulaParticle";
            meshNode.IsAcceptShadow = false;
            meshNode.IsCastShadow = false;

            NebulaParticle = meshNode.NebulaParticle;

            //var materials = new Graphics.Pipeline.Shader.UMaterial[1];
            //materials[0] = UEngine.Instance.GfxDevice.MaterialManager.PxDebugMaterial;
            //if (materials[0] == null)
            //    return;
            //var mesh = new Graphics.Mesh.TtMesh();
            //var rect = Graphics.Mesh.UMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
            //var rectMesh = rect.ToMesh();
            //var ok = mesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            //if (ok)
            //{
            //    //mesh.DirectSetWorldMatrix(ref Matrix.mIdentity);
            
            //    var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
            //    meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
            //    meshNode.NodeData.Name = "PreviewObject";
            //    meshNode.IsAcceptShadow = false;
            //    meshNode.IsCastShadow = true;
            //}

            var aabb = meshNode.AABB;
            var radius = (float)aabb.GetMaxSide();
            DBoundingSphere sphere;
            sphere.Center = aabb.GetCenter();
            sphere.Radius = radius;
            policy.DefaultCamera.AutoZoom(in sphere);
            //this.RenderPolicy.GBuffers.SunLightColor = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SunLightDirection = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SkyLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.GroundLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.UpdateViewportCBuffer();

            var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(viewport.World, viewport.World.Root);
            gridNode.ViewportSlate = this.PreviewViewport;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async Thread.Async.TtTask<bool> OpenEditor(EngineNS.Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            if (IsStarting)
                return false;

            ParticleGraph.ResetGraph();
            ParticleGraph.OnChangeGraph = (UNodeGraph graph) =>
            {
                GraphRenderer.SetGraph(graph);
            };
            ParticleGraph.Editor = this;
            IsStarting = true;

            AssetName = name;
            IsStarting = false;

            await NodePropGrid.Initialize();

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"NebulaPreview:{AssetName}";
            PreviewViewport.OnInitialize = Initialize_PreviewParticle;
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.SlateApplication, UEngine.Instance.Config.MainRPolicyName, 0, 1);

            UEngine.Instance.TickableManager.AddTickable(this);

            GraphRenderer.SetGraph(this.ParticleGraph);
            return true;
        }
        public void OnCloseEditor()
        {
            UEngine.Instance.TickableManager.RemoveTickable(this);
            Dispose();
        }
        public void OnEvent(in Bricks.Input.Event e)
        {
            //PreviewViewport.OnEvent(ref e);
        }
        #endregion
    }
}
