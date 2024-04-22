using EngineNS.Graphics.Mesh;
using System;
using System.Collections.Generic;
using System.Text;

//using EngineNS.Graphics.Canvas;

namespace EngineNS
{
    public partial class UEngineConfig
    {
        [Rtti.Meta]
        public Editor.Forms.UMeshPrimitiveEditorConfig MeshPrimitiveEditorConfig
        {
            get; set;
        } = new Editor.Forms.UMeshPrimitiveEditorConfig();
    }
}

namespace EngineNS.Editor.Forms
{
    public class UMeshPrimitiveEditorConfig : IO.BaseSerializer
    {
        public UMeshPrimitiveEditorConfig()
        {
            MaterialName = RName.GetRName("material/sysdft.material", RName.ERNameType.Engine);
            PlaneMaterialName = RName.GetRName("material/whitecolor.uminst", RName.ERNameType.Engine);
        }
        public RName MaterialName { get; set; }
        public RName PlaneMaterialName { get; set; }
    }
    public class UMeshPrimitiveEditor : Editor.IAssetEditor, ITickable, IRootForm
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        //public Graphics.Canvas.TtCanvas TestCanvas { get; set; }

        public Graphics.Mesh.UMeshPrimitives Mesh;
        public Editor.UPreviewViewport PreviewViewport = new Editor.UPreviewViewport();
        public EGui.Controls.PropertyGrid.PropertyGrid MeshPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public EGui.Controls.PropertyGrid.PropertyGrid EditorPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        EngineNS.GamePlay.Scene.UMeshNode mCurrentMeshNode;
        public float PlaneScale = 5.0f;
        EngineNS.GamePlay.Scene.UMeshNode PlaneMeshNode;
        public bool IsCastShadow
        {
            get
            {
                if (mCurrentMeshNode == null)
                    return false;
                return mCurrentMeshNode.IsCastShadow;
            }
            set
            {
                if (mCurrentMeshNode == null)
                    return;
                mCurrentMeshNode.IsCastShadow = value;
            }
        }
        public bool IsAcceptShadow
        {
            get
            {
                if (mCurrentMeshNode == null)
                    return false;
                return mCurrentMeshNode.IsAcceptShadow;
            }
            set
            {
                if (mCurrentMeshNode == null)
                    return;
                mCurrentMeshNode.IsAcceptShadow = value;
            }
        }

        ~UMeshPrimitiveEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            Mesh = null;
            CoreSDK.DisposeObject(ref PreviewViewport);
            MeshPropGrid.Target = null;
            EditorPropGrid.Target = null;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await MeshPropGrid.Initialize();
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

            var mtl = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(UEngine.Instance.Config.MeshPrimitiveEditorConfig.MaterialName);
            var materials = new Graphics.Pipeline.Shader.UMaterial[Mesh.mCoreObject.GetAtomNumber()];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = mtl;
            }
            var mesh = new Graphics.Mesh.TtMesh();
            mesh.Initialize(Mesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);

            var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), mesh,
                        DVector3.Zero, Vector3.One, Quaternion.Identity);
            meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
            meshNode.NodeData.Name = "PreviewObject";
            meshNode.IsAcceptShadow = true;
            meshNode.IsCastShadow = true;

            mCurrentMeshNode = meshNode;

            var aabb = mesh.MaterialMesh.AABB;
            float radius = aabb.GetMaxSide();
            BoundingSphere sphere;
            sphere.Center = aabb.GetCenter();
            sphere.Radius = radius;
            policy.DefaultCamera.AutoZoom(ref sphere);

            {
                var meshCenter = aabb.GetCenter();
                var meshSize = aabb.GetSize() * PlaneScale;
                meshSize.Y = aabb.GetSize().Y * 0.05f;
                var boxStart = meshCenter - meshSize * 0.5f;
                boxStart.Y -= aabb.GetSize().Y * 0.5f;
                var box = Graphics.Mesh.UMeshDataProvider.MakeBox(boxStart.X, boxStart.Y, boxStart.Z, meshSize.X, meshSize.Y, meshSize.Z).ToMesh();

                var PlaneMesh = new Graphics.Mesh.TtMesh();
                var tMaterials = new Graphics.Pipeline.Shader.UMaterial[1];
                tMaterials[0] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(UEngine.Instance.Config.MeshPrimitiveEditorConfig.PlaneMaterialName);
                PlaneMesh.Initialize(box, tMaterials,
                    Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                PlaneMeshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), PlaneMesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                PlaneMeshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
                PlaneMeshNode.NodeData.Name = "Plane";
                PlaneMeshNode.IsAcceptShadow = true;
                PlaneMeshNode.IsCastShadow = false;
            }

            var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(viewport.World, viewport.World.Root);
            gridNode.ViewportSlate = this.PreviewViewport;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async System.Threading.Tasks.Task<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            Mesh = arg as Graphics.Mesh.UMeshPrimitives;
            if (Mesh == null)
            {
                Mesh = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.CreateMeshPrimitive(name);
                if (Mesh == null)
                    return false;
            }

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"Mesh:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewMaterialInstance;
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.SlateApplication, UEngine.Instance.Config.MainRPolicyName, 0, 1);

            MeshPropGrid.Target = Mesh;
            EditorPropGrid.Target = this;
            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public void OnCloseEditor()
        {
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
            uint leftId = 0;
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir_.ImGuiDir_Left, 0.2f, ref leftId, ref rightId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Left", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Right", mDockKeyClass), rightId);
            ImGuiAPI.DockBuilderFinish(id);
        }
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public unsafe void OnDraw()
        {
            if (Visible == false || Mesh == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (EGui.UIProxy.DockProxy.BeginMainForm(Mesh.AssetName.Name, this, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
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
            EGui.UIProxy.DockProxy.EndMainForm();

            DrawLeft();
            DrawRight();
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                Mesh.SaveAssetTo(Mesh.AssetName);
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
            if (EGui.UIProxy.CustomButton.ToolButton("BuildCluster", in btSize))
            {
                var meshMeta = Mesh.GetAMeta() as EngineNS.Graphics.Mesh.UMeshPrimitivesAMeta;
                meshMeta.IsClustered = true;
                meshMeta.AddReferenceAsset(RName.GetRName(Mesh.AssetName + ".clusteremesh", Mesh.AssetName.RNameType));
                meshMeta.SaveAMeta();
                Mesh.BuildClusteredMesh();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("LoadCluster", in btSize))
            {
                Mesh.LoadClusterMesh();
            }
        }
        bool mLeftShow = true;
        protected unsafe void DrawLeft()
        {
            var sz = new Vector2(-1);
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Left", ref mLeftShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.CollapsingHeader("MeshProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_CollapsingHeader))
                {
                    MeshPropGrid.OnDraw(true, false, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize);
                    if(ImGuiAPI.Button("Build SDF"))
                    {
                        UMeshDataProvider meshProvider = new UMeshDataProvider();
                        if(meshProvider.InitFrom(Mesh))
                        {
                            var embreeScene = new DistanceField.UEmbreeScene();
                            var embreeManager = new DistanceField.UEmbreeManager();
                            embreeManager.SetupEmbreeScene(Mesh.AssetName.ToString(), meshProvider, 1.0f, embreeScene);
                            embreeManager.DeleteEmbreeScene(embreeScene);
                        }
                    }
                }
                if (ImGuiAPI.CollapsingHeader("EditorProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_CollapsingHeader))
                {
                    EditorPropGrid.OnDraw(true, false, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize);
                }
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        bool mRightShow = true;
        protected unsafe void DrawRight()
        {
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Right", ref mRightShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                PreviewViewport.ViewportType = Graphics.Pipeline.UViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel();
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
}

namespace EngineNS.Graphics.Mesh
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.UMeshPrimitiveEditor))]
    public partial class UMeshPrimitives
    {
    }
}