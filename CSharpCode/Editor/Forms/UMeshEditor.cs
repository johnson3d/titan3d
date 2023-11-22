using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Graphics.Pipeline;

namespace EngineNS.Editor.Forms
{
    public class UMeshEditor : Editor.IAssetEditor, ITickable, IRootForm
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

        public Graphics.Mesh.UMaterialMesh Mesh;
        public Editor.UPreviewViewport PreviewViewport = new Editor.UPreviewViewport();
        public URenderPolicy RenderPolicy { get => PreviewViewport.RenderPolicy; }
        public EGui.Controls.PropertyGrid.PropertyGrid MeshPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public EGui.Controls.PropertyGrid.PropertyGrid EditorPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();

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
        ~UMeshEditor()
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
        public float PlaneScale = 5.0f;
        EngineNS.GamePlay.Scene.UMeshNode PlaneMeshNode;
        EngineNS.GamePlay.Scene.UMeshNode mCurrentMeshNode;
        EngineNS.GamePlay.Scene.UMeshNode mArrowMeshNode;
        float mCurrentMeshRadius = 1.0f;
        protected async System.Threading.Tasks.Task Initialize_PreviewMesh(Graphics.Pipeline.UViewportSlate viewport, USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();
            viewport.World.DirectionLight.Direction = new Vector3(0, 0, 1);

            (viewport as Editor.UPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var mesh = new Graphics.Mesh.UMesh();

            var ok = mesh.Initialize(Mesh, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                meshNode.NodeData.Name = "PreviewObject";
                meshNode.IsAcceptShadow = true;
                meshNode.IsCastShadow = true;
                mCurrentMeshNode = meshNode;
            }

            var arrowMaterialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(RName.GetRName("mesh/base/arrow.ums", RName.ERNameType.Engine));
            var arrowMesh = new Graphics.Mesh.UMesh();
            ok = arrowMesh.Initialize(arrowMaterialMesh, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                mArrowMeshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), arrowMesh, DVector3.UnitX*3, Vector3.One, Quaternion.Identity);
                mArrowMeshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                mArrowMeshNode.NodeData.Name = "PreviewArrow";
                mArrowMeshNode.IsAcceptShadow = false;
                mArrowMeshNode.IsCastShadow = false;
            }

            var aabb = mesh.MaterialMesh.Mesh.mCoreObject.mAABB;
            mCurrentMeshRadius = aabb.GetMaxSide();
            BoundingSphere sphere;
            sphere.Center = aabb.GetCenter();
            sphere.Radius = mCurrentMeshRadius;
            policy.DefaultCamera.AutoZoom(ref sphere);

            {
                var meshCenter = aabb.GetCenter();
                var meshSize = aabb.GetSize() * PlaneScale;
                meshSize.Y = aabb.GetSize().Y * 0.05f;
                var boxStart = meshCenter - meshSize * 0.5f;
                boxStart.Y -= aabb.GetSize().Y * 0.5f;
                var box = Graphics.Mesh.UMeshDataProvider.MakeBox(boxStart.X, boxStart.Y, boxStart.Z, meshSize.X, meshSize.Y, meshSize.Z).ToMesh();

                var PlaneMesh = new Graphics.Mesh.UMesh();
                var tMaterials = new Graphics.Pipeline.Shader.UMaterial[1];
                tMaterials[0] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("material/whitecolor.uminst", RName.ERNameType.Engine));
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
        public async System.Threading.Tasks.Task<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            Mesh = arg as Graphics.Mesh.UMaterialMesh;
            if (Mesh == null)
            {
                Mesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.CreateMaterialMesh(name);
                if (Mesh == null)
                    return false;
            }

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"MaterialMesh:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewMesh;
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
                var unused = UEngine.Instance.GfxDevice.MaterialMeshManager.ReloadMaterialMesh(Mesh.AssetName);

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
        bool mLeftShow = true;
        protected unsafe void DrawLeft()
        {
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Left", ref mLeftShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.CollapsingHeader("MeshProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    MeshPropGrid.OnDraw(true, false, false);
                }
                if (ImGuiAPI.CollapsingHeader("EditorProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    EditorPropGrid.OnDraw(true, false, false);
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

        }
        #region Tickable
        public void TickLogic(float ellapse)
        {
            PreviewViewport.TickLogic(ellapse);
        }
        [Category("Light")]
        [EGui.Controls.PropertyGrid.PGValueRange(-3.1416f, 3.1416f)]
        [EGui.Controls.PropertyGrid.PGValueChangeStep(3.1416f / 100.0f)]
        public float Yaw { get; set; } = -0.955047f;
        //[Category("Light")]
        //[EGui.Controls.PropertyGrid.PGValueRange(-3.1416f, 3.1416f)]
        //[EGui.Controls.PropertyGrid.PGValueChangeStep(3.1416f / 100.0f)]
        //public float Pitch { get; set; }
        [Category("Light")]
        [EGui.Controls.PropertyGrid.PGValueRange(-3.1416f, 3.1416f)]
        [EGui.Controls.PropertyGrid.PGValueChangeStep(3.1416f / 100.0f)]
        public float Roll { get; set; } = -0.552922f;
        [Category("Light")]
        public GamePlay.TtDirectionLight DirLight
        {
            get
            {
                return PreviewViewport.World.DirectionLight;
            }
        }
        public void TickRender(float ellapse)
        {
            PreviewViewport.TickRender(ellapse);

            if (ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, -1) || ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Right, -1))
            {
                if (UEngine.Instance.InputSystem.IsKeyDown(EngineNS.Bricks.Input.Keycode.KEY_l))
                {
                    var delta = ImGuiAPI.GetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Left, -1);
                    var delta2 = ImGuiAPI.GetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Right, -1);
                    delta.X = Math.Max(delta.X, delta.X);
                    delta.Y = Math.Max(delta.Y, delta.Y);

                    var step = 3.1416f / 500.0f;
                    Yaw -= delta.X * step;
                    Roll += delta.Y * step;
                    ImGuiAPI.ResetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Left);
                    ImGuiAPI.ResetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Right);
                    mArrowMeshNode.Placement.Scale = new Vector3(Math.Min(mCurrentMeshRadius * 0.5f, 2.0f));
                }
            }
            else
            {
                mArrowMeshNode.Placement.Scale = Vector3.Zero;
            }


            var quat = EngineNS.Quaternion.RotationYawPitchRoll(Yaw, 0, Roll);
            PreviewViewport.World.DirectionLight.Direction = quat * Vector3.UnitX;

            var arrowPos = -mCurrentMeshRadius * PreviewViewport.World.DirectionLight.Direction;
            mArrowMeshNode.Placement.Position = new DVector3(arrowPos.X, arrowPos.Y, arrowPos.Z);
            mArrowMeshNode.Placement.Quat = quat;
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
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.UMeshEditor))]
    public partial class UMaterialMesh
    {
        
    }
}