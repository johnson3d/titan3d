using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Particle.Editor
{
    public partial class UParticleEditor : EngineNS.Editor.IAssetEditor, IO.ISerializer, ITickable, IRootForm
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
        public async System.Threading.Tasks.Task<bool> Initialize()
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
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

            bool drawing = true;
            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (EGui.UIProxy.DockProxy.BeginMainForm(AssetName.Name, this, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
            {
                if (ImGuiAPI.IsWindowDocked())
                {
                    DockId = ImGuiAPI.GetWindowDockID();
                }
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as EngineNS.Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
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
            else
            {
                drawing = false;
            }
            EGui.UIProxy.DockProxy.EndMainForm();

            if (drawing)
            {
                if (PreviewDockId != 0)
                {
                    PreviewViewport.DockId = PreviewDockId;
                    PreviewViewport.DockCond = ImGuiCond_.ImGuiCond_Always;
                    PreviewViewport.VieportType = Graphics.Pipeline.UViewportSlate.EVieportType.Window;
                    PreviewViewport.OnDraw();
                }
            }
        }
        protected void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                //EngineNS.Editor.USnapshot.Save(NebulaParticle.AssetName, NebulaParticle.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Compile", in btSize))
            {
                
            }
        }
        uint PreviewDockId = 0;
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            if (PreviewDockId == 0)
                PreviewDockId = ImGuiAPI.GetID($"{AssetName}");

            var size = new Vector2(-1, -1);
            if (ImGuiAPI.BeginChild("LeftWindow", in size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.CollapsingHeader("Preview", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    PreviewViewport.Visible = true;
                    ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                    var winClass = new ImGuiWindowClass();
                    winClass.UnsafeCallConstructor();
                    var sz = ImGuiAPI.GetWindowSize();
                    sz.Y = sz.X;
                    ImGuiAPI.DockSpace(PreviewDockId, in sz, dockspace_flags, in winClass);
                    winClass.UnsafeCallDestructor();
                }
                else
                {
                    PreviewViewport.Visible = false;
                }
                if (ImGuiAPI.CollapsingHeader("ParticleStruct", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    ParticleStructBuilder.OnDraw();
                }
                if (ImGuiAPI.CollapsingHeader("NodeProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    NodePropGrid.OnDraw(true, false, false);
                }
                
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            
            var size = new Vector2(-1, -1);
            if (ImGuiAPI.BeginChild("RightWindow", in size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                GraphRenderer.OnDraw();
            }
            ImGuiAPI.EndChild();
        }
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
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);

        public UParticleGraph ParticleGraph { get; } = new UParticleGraph();
        public UGraphRenderer GraphRenderer { get; } = new UGraphRenderer();
        public CodeBuilder.UClassLayoutBuilder ParticleStructBuilder { get; } = new CodeBuilder.UClassLayoutBuilder();
        bool IsStarting = false;
        protected async System.Threading.Tasks.Task Initialize_PreviewMaterial(Graphics.Pipeline.UViewportSlate viewport, USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            ParticleGraph.NebulaEditor = this;
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();

            (viewport as EngineNS.Editor.UPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var nebulaData = new Bricks.Particle.Simple.USimpleNebulaNode.USimpleNebulaNodeData();
            nebulaData.NebulaName = AssetName;
            var meshNode = new Bricks.Particle.Simple.USimpleNebulaNode();
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
            //var mesh = new Graphics.Mesh.UMesh();
            //var rect = Graphics.Mesh.UMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
            //var rectMesh = rect.ToMesh();
            //var ok = mesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            //if (ok)
            //{
            //    //mesh.SetWorldMatrix(ref Matrix.mIdentity);
            //    //viewport.RenderPolicy.VisibleMeshes.Add(mesh);

            //    var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
            //    meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
            //    meshNode.NodeData.Name = "PreviewObject";
            //    meshNode.IsAcceptShadow = false;
            //    meshNode.IsCastShadow = true;
            //}

            var aabb = meshNode.AABB;
            var radius = (float)aabb.GetMaxSide();
            BoundingSphere sphere;
            sphere.Center = aabb.GetCenter().ToSingleVector3();
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
        public async System.Threading.Tasks.Task<bool> OpenEditor(EngineNS.Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            if (IsStarting)
                return false;

            ParticleGraph.ResetGraph();
            ParticleGraph.OnChangeGraph = (UNodeGraph graph) =>
            {
                GraphRenderer.SetGraph(graph);
            };
            IsStarting = true;

            AssetName = name;
            IsStarting = false;

            await NodePropGrid.Initialize();

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"NebulaPreview:{AssetName}";
            PreviewViewport.OnInitialize = Initialize_PreviewMaterial;
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
