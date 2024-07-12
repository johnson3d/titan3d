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

        UDebugShowTool DebugShowTool;
        bool mShowNormal = false;
        bool mShowTangent = false;

        #region Color Sdf Preview
        DistanceField.TtSdfAsset MeshSdfAsset = new DistanceField.TtSdfAsset();
        public EngineNS.Editor.USdfPreviewViewport sdfViewport = new EngineNS.Editor.USdfPreviewViewport();
        protected async System.Threading.Tasks.Task Initialize_SdfViewport(Graphics.Pipeline.UViewportSlate viewport, USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            var subMesh = Mesh.GetMeshPrimitives(0);            
            var noExtName = subMesh.AssetName.Name.Substring(0, subMesh.AssetName.Name.Length - subMesh.AssetName.ExtName.Length);
            var rn = RName.GetRName(noExtName + DistanceField.TtSdfAsset.AssetExt, Mesh.AssetName.RNameType);
            MeshSdfAsset = await UEngine.Instance.SdfAssetManager.GetSdfAsset(rn);

            if (MeshSdfAsset == null)
            {
                await policy.Initialize(null);
                return;
            }
            var SdfMip = MeshSdfAsset.Mips[0];
            var sdfVoxelDimensions = SdfMip.GetVoxelDimensions();


            var boxSize = MeshSdfAsset.LocalSpaceMeshBounds.GetSize();
            var boxCenter = MeshSdfAsset.LocalSpaceMeshBounds.GetCenter();
            var boxExtent = MeshSdfAsset.LocalSpaceMeshBounds.GetExtent();

            var sdfCamera = new UCamera();
            var center = mCurrentMeshNode.Location + boxCenter.AsDVector();
            var eye = center - new DVector3(0, 0, boxExtent.Z);
            sdfCamera.LookAtLH(eye, center, Vector3.Up);
            sdfCamera.MakeOrtho(boxSize.X, boxSize.Y, 0, boxSize.Z);

            await policy.Initialize(sdfCamera);
            policy.OnResize(sdfVoxelDimensions.X, sdfVoxelDimensions.Y);
        }
        #endregion


        ~UMeshEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            Mesh = null;
            CoreSDK.DisposeObject(ref PreviewViewport);
            CoreSDK.DisposeObject(ref sdfViewport);
            MeshPropGrid.Target = null;
            EditorPropGrid.Target = null;
        }
        public async Thread.Async.TtTask<bool> Initialize()
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

            List<Graphics.Mesh.UMeshPrimitives> MeshPrimitivesList = new List<Graphics.Mesh.UMeshPrimitives>();
            foreach (var j in Mesh.SubMeshes)
            {
                if (j.Mesh == null)
                    continue;
                MeshPrimitivesList.Add(j.Mesh);
            }
            DebugShowTool = new UDebugShowTool();
            await DebugShowTool.Initialize(MeshPrimitivesList, PreviewViewport.World);

            var mesh = new Graphics.Mesh.TtMesh();

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
            var arrowMesh = new Graphics.Mesh.TtMesh();
            ok = arrowMesh.Initialize(arrowMaterialMesh, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                mArrowMeshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), arrowMesh, DVector3.UnitX*3, Vector3.One, Quaternion.Identity);
                mArrowMeshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                mArrowMeshNode.NodeData.Name = "PreviewArrow";
                mArrowMeshNode.IsAcceptShadow = false;
                mArrowMeshNode.IsCastShadow = false;
            }

            var aabb = mesh.MaterialMesh.AABB;
            mCurrentMeshRadius = aabb.GetMaxSide();
            DBoundingSphere sphere;
            sphere.Center = aabb.GetCenter().AsDVector();
            sphere.Radius = mCurrentMeshRadius;
            policy.DefaultCamera.AutoZoom(in sphere);

            {
                var meshCenter = aabb.GetCenter();
                var meshSize = aabb.GetSize() * PlaneScale;
                var maxLength = MathHelper.Max(meshSize.X, meshSize.Z);
                meshSize.X = meshSize.Z = maxLength;
                meshSize.Y = aabb.GetSize().Y * 0.05f;
                var boxStart = meshCenter - meshSize * 0.5f;
                boxStart.Y -= aabb.GetSize().Y * 0.5f + meshSize.Y * 0.5f + 0.001f;
                var box = Graphics.Mesh.UMeshDataProvider.MakePlane(meshSize.X, meshSize.Z).ToMesh();

                var PlaneMesh = new Graphics.Mesh.TtMesh();
                var tMaterials = new Graphics.Pipeline.Shader.UMaterial[1];
                tMaterials[0] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("material/whitecolor.uminst", RName.ERNameType.Engine));
                PlaneMesh.Initialize(box, tMaterials,
                    Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                PlaneMeshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), PlaneMesh, new DVector3(0, -0.0001f, 0), Vector3.One, Quaternion.Identity);
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
        public async Thread.Async.TtTask<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
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

            #region sdf
            var sdfRPolicyName = RName.GetRName("graphics/sdf.rpolicy", RName.ERNameType.Engine);
            //sdfViewport.Title = $"MaterialMesh:{name}";
            sdfViewport.OnInitialize = Initialize_SdfViewport;
            await sdfViewport.Initialize(UEngine.Instance.GfxDevice.SlateApplication, sdfRPolicyName, 0, 1);
            var mesh = new Graphics.Mesh.TtMesh();
            var ok = mesh.Initialize(Mesh, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(sdfViewport.World, sdfViewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
                meshNode.NodeData.Name = "PreviewObject";
                meshNode.IsAcceptShadow = false;
                meshNode.IsCastShadow = false;
                meshNode.IsSceneManaged = false;
            }
            #endregion

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
        #region DrawUI
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public bool IsDrawing { get; set; }
        public unsafe void OnDraw()
        {
            if (Visible == false || Mesh == null)
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

            DrawPreview();
            DrawEditorDetails();
            DrawMeshDetails();
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

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Preview", mDockKeyClass), middleId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("sdfPreview", mDockKeyClass), middleId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("MeshDetails", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("EditorDetails", mDockKeyClass), rightDownId);

            ImGuiAPI.DockBuilderFinish(id);
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
            if (EGui.UIProxy.CustomButton.ToolButton("ApplySubMeshes", in btSize))
            {
                Mesh.UpdateSubMeshes();
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
            if (ImGuiAPI.ToggleButton("N", ref mShowNormal, in btSize, 0))
            {
                DebugShowTool.ShowNormal = mShowNormal;
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.ToggleButton("T", ref mShowTangent, in btSize, 0))
            {
                DebugShowTool.ShowTangent = mShowTangent;
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.ToggleButton("TestAuto", ref mShowTangent, in btSize, 0))
            {
                var ameta = UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
                _ = ameta.AutoGenSnap();
            }
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
        bool ShowMeshPropGrid = true;
        protected void DrawMeshDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "MeshDetails", ref ShowMeshPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                MeshPropGrid.OnDraw(true, false, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowPreview = true;
        protected unsafe void DrawPreview()
        {
            #region sdf
            var showSdf = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "sdfPreview", ref ShowPreview, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (showSdf)
            {
                sdfViewport.ViewportType = Graphics.Pipeline.UViewportSlate.EViewportType.ChildWindow;
                sdfViewport.OnDraw();
            }
            sdfViewport.Visible = true;
            EGui.UIProxy.DockProxy.EndPanel(showSdf);
            #endregion

            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Preview", ref ShowPreview, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                PreviewViewport.ViewportType = Graphics.Pipeline.UViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();
            }
            this.PreviewViewport.Visible = show;
            EGui.UIProxy.DockProxy.EndPanel(show);

        }
        #endregion

        public void OnEvent(in Bricks.Input.Event e)
        {

        }
        #region Tickable
        public void TickLogic(float ellapse)
        {
            PreviewViewport.TickLogic(ellapse);
            sdfViewport.TickLogic(ellapse);
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

            if (IsDrawing == false)
                return;

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
            sdfViewport.TickSync(ellapse);
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