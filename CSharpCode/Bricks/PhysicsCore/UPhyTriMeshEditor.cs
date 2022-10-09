using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public class UPhyTriMeshEditor : Editor.IAssetEditor, ITickable, Graphics.Pipeline.IRootForm
    {
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public UPhyTriMesh TriMesh;
        public Graphics.Mesh.UMaterialMesh ShowMesh;
        public Editor.UPreviewViewport PreviewViewport = new Editor.UPreviewViewport();
        public EGui.Controls.PropertyGrid.PropertyGrid TriMeshPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        ~UPhyTriMeshEditor()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            TriMesh = null;
            TriMeshPropGrid.Target = null;

            PreviewViewport?.Cleanup();
            PreviewViewport = null;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await TriMeshPropGrid.Initialize();
            return true;
        }
        public Graphics.Pipeline.IRootForm GetRootForm()
        {
            return this;
        }
        protected async System.Threading.Tasks.Task Initialize_PreviewMesh(Graphics.Pipeline.UViewportSlate viewport, Graphics.Pipeline.USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            //await viewport.RenderPolicy.Initialize(null);

            await viewport.World.InitWorld();

            (viewport as Editor.UPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            ShowMesh = new Graphics.Mesh.UMaterialMesh();
            var meshPrimitve = TriMesh.ToMeshProvider().ToMesh();
            var matrials = new Graphics.Pipeline.Shader.UMaterial[1];
            matrials[0] = await UEngine.Instance.GfxDevice.MaterialManager.CreateMaterial(RName.GetRName("material/SysDft_Color.material", RName.ERNameType.Engine));
            matrials[0].RenderLayer = Graphics.Pipeline.ERenderLayer.RL_Translucent;
            var rast = matrials[0].Rasterizer;
            rast.FillMode = NxRHI.EFillMode.FMD_WIREFRAME;
            matrials[0].Rasterizer = rast;
            ShowMesh.Initialize(meshPrimitve, matrials);

            var mesh = new Graphics.Mesh.UMesh();
            var ok = mesh.Initialize(ShowMesh, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                meshNode.NodeData.Name = "PreviewObject";
                meshNode.IsAcceptShadow = false;
                meshNode.IsCastShadow = true;
            }

            var aabb = mesh.MaterialMesh.Mesh.mCoreObject.mAABB;
            float radius = aabb.GetMaxSide();
            BoundingSphere sphere;
            sphere.Center = aabb.GetCenter();
            sphere.Radius = radius;
            policy.DefaultCamera.AutoZoom(ref sphere);

            var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(viewport.World, viewport.World.Root);
            gridNode.ViewportSlate = this.PreviewViewport;
        }
        public async System.Threading.Tasks.Task<bool> OpenEditor(Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            TriMesh = await UEngine.Instance.PhyModule.PhyContext.PhyMeshManager.GetMesh(name);
            if (TriMesh == null)
                return false;

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"PxTriMesh:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewMesh;
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.SlateApplication, UEngine.Instance.Config.MainRPolicyName, 0, 1);
            UEngine.Instance.TickableManager.AddTickable(this);

            TriMeshPropGrid.Target = TriMesh;
            return true;
        }
        public void OnCloseEditor()
        {
            Cleanup();
        }
        public float LeftWidth = 0;
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public unsafe void OnDraw()
        {
            if (Visible == false || TriMesh == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (ImGuiAPI.Begin(TriMesh.AssetName.Name, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
            {
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
        protected unsafe void DrawToolBar()
        {
            var btSize = new Vector2(64, 64);
            if (ImGuiAPI.Button("SaveSnap", in btSize))
            {
                //Editor.USnapshot.Save(TriMesh.AssetName, TriMesh.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
            }
        }
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            var sz = new Vector2(-1);
            if (ImGuiAPI.BeginChild("LeftView", in sz, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.CollapsingHeader("MeshProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    TriMeshPropGrid.OnDraw(true, false, false);
                }
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            PreviewViewport.VieportType = Graphics.Pipeline.UViewportSlate.EVieportType.ChildWindow;
            PreviewViewport.OnDraw();
        }

        public void OnEvent(ref SDL2.SDL.SDL_Event e)
        {
            //throw new NotImplementedException();
        }
        #region Tickable
        public void TickLogic(int ellapse)
        {
            PreviewViewport?.TickLogic(ellapse);
        }
        public void TickRender(int ellapse)
        {
            PreviewViewport?.TickRender(ellapse);
        }
        public void TickSync(int ellapse)
        {
            PreviewViewport?.TickSync(ellapse);
        }
        #endregion
    }
}
