using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Editor.Forms
{
    public class TtMeshEditor : TtLightEnvironemnt, Editor.IAssetEditor, IRootForm, ISkeletonTreeHost
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

        public Graphics.Mesh.TtMaterialMesh Mesh;
        public Editor.TtPreviewViewport PreviewViewport = new Editor.TtPreviewViewport();
        [Category("Option")]
        public TtRenderPolicy RenderPolicy 
        { 
            get => PreviewViewport.RenderPolicy; 
        }
        public EGui.Controls.PropertyGrid.TtPropertyGrid MeshPropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        public EGui.Controls.PropertyGrid.TtPropertyGrid EditorPropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        [Category("Option")]
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
        [Category("Option")]
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
        public bool IsShowGrid
        {
            get
            {
                if (GridNode == null)
                    return false;
                return !GridNode.HasStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible);
            }
            set
            {
                if (mCurrentMeshNode == null)
                    return;
                if(value==true)
                    GridNode.UnsetStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible);
                else
                    GridNode.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible);
            }
        }

        TtDebugShowTool DebugShowTool;
        bool mShowNormal = false;
        bool mShowTangent = false;

        #region Color Sdf Preview
        DistanceField.TtSdfAsset MeshSdfAsset = new DistanceField.TtSdfAsset();
        public EngineNS.Editor.USdfPreviewViewport sdfViewport = new EngineNS.Editor.USdfPreviewViewport();
        protected async Thread.Async.TtTask<bool> Initialize_SdfViewport(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            var subMesh = Mesh.GetMeshPrimitives(0);
            if (subMesh == null)
                return false;
            var noExtName = subMesh.AssetName.NoExtName;// subMesh.AssetName.Name.Substring(0, subMesh.AssetName.Name.Length - subMesh.AssetName.ExtName.Length);
            var rn = RName.GetRName(noExtName + DistanceField.TtSdfAsset.AssetExt, Mesh.AssetName.RNameType);
            MeshSdfAsset = await rn.GetAsset<DistanceField.TtSdfAsset>();

            if (MeshSdfAsset == null)
            {
                await policy.Initialize(null);
                return false;
            }
            var SdfMip = MeshSdfAsset.Mips[0];
            var sdfVoxelDimensions = SdfMip.GetVoxelDimensions();


            var boxSize = MeshSdfAsset.LocalSpaceMeshBounds.GetSize();
            var boxCenter = MeshSdfAsset.LocalSpaceMeshBounds.GetCenter();
            var boxExtent = MeshSdfAsset.LocalSpaceMeshBounds.GetExtent();

            var sdfCamera = new TtCamera();
            var center = mCurrentMeshNode.Location + boxCenter.AsDVector();
            var eye = center - new DVector3(0, 0, boxExtent.Z);
            sdfCamera.LookAtLH(eye, center, Vector3.Up);
            sdfCamera.MakeOrtho(boxSize.X, boxSize.Y, 0, boxSize.Z);

            await policy.Initialize(sdfCamera);
            policy.OnResize(sdfVoxelDimensions.X, sdfVoxelDimensions.Y);

            return true;
        }
        #endregion


        ~TtMeshEditor()
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
        EngineNS.GamePlay.Scene.TtMeshNode PlaneMeshNode;
        EngineNS.GamePlay.Scene.TtMeshNode mCurrentMeshNode;
        //EngineNS.GamePlay.Scene.TtMeshNode mArrowMeshNode;
        EngineNS.GamePlay.Scene.TtGridNode GridNode;
        public Graphics.Mesh.Modifier.TtSkinModifier SkinModifier;
        protected async Thread.Async.TtTask<bool> Initialize_PreviewMesh(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();
            viewport.World.DirectionLight.Direction = new Vector3(0, 0, 1);

            (viewport as Editor.TtPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var mesh = new Graphics.Mesh.TtRenderMesh();
            List<Graphics.Mesh.TtMeshPrimitives> MeshPrimitivesList = new List<Graphics.Mesh.TtMeshPrimitives>();
            foreach (var j in Mesh.SubMeshes)
            {
                if (j.Mesh == null)
                    continue;
                MeshPrimitivesList.Add(j.Mesh);
            }

            var ok = mesh.Initialize(Mesh);
            if (ok)
            {
                var meshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
                meshNode.NodeData.Name = "PreviewObject";
                meshNode.IsAcceptShadow = true;
                meshNode.IsCastShadow = true;
                mCurrentMeshNode = meshNode;

                SkinModifier = mesh.MdfQueue.FindModifier<Graphics.Mesh.Modifier.TtSkinModifier>();
                if (SkinModifier!=null && Mesh.Skeleton!=null)
                {
                    SkinModifier.Skeleton = (await Mesh.Skeleton.GetAsset<Animation.Asset.TtSkeletonAsset>()).Skeleton;
                    SkeletonTreePanel.SetSkeleton(SkinModifier.Skeleton, this);
                    var meshPrimRName = Mesh.SubMeshes.Count > 0 ? Mesh.SubMeshes[0].Mesh?.AssetName : null;
                    SkeletonTreePanel.SetMeshAssetName(meshPrimRName, viewport.World);
                    var animatablePose = SkinModifier.Skeleton?.CreateSkeletonPose();
                    var animatedPose = Animation.SkeletonAnimation.Runtime.Pose.TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(animatablePose);
                    meshNode.RuntimePose = animatedPose;
                }
            }

            DebugShowTool = new TtDebugShowTool();
            await DebugShowTool.Initialize(MeshPrimitivesList, PreviewViewport.World);

            var aabb = mesh.MaterialMesh.AABB;
            float radius = aabb.GetMaxSide();
            DBoundingSphere sphere;
            sphere.Center = aabb.GetCenter().AsDVector();
            sphere.Radius = radius;
            policy.DefaultCamera.AutoZoom(in sphere);

            var studioContext = await PreviewViewport.CreateStudioEnvironment(aabb, PlaneScale);
            PlaneMeshNode = studioContext?.FloorNode;
            GridNode = studioContext?.GridNode;

            await InitializeLightEnv(PreviewViewport, studioContext?.Radius ?? radius);

            return true;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async Thread.Async.TtTask<bool> OpenEditor(TtMainEditorApplication mainEditor, RName name, object arg, bool saveLayout)
        {
            AssetName = name;
            Mesh = arg as Graphics.Mesh.TtMaterialMesh;
            if (Mesh == null)
            {
                Mesh = await name.CreateAsset<Graphics.Mesh.TtMaterialMesh>();
                Mesh.AssetName = AssetName;
                if (Mesh == null)
                    return false;
            }

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"MaterialMesh:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewMesh;
            var SimpleRPolicyName = TtEngine.Instance.Config.MainRPolicyName;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, SimpleRPolicyName, 0, 1);
            //await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, TtEngine.Instance.Config.MainRPolicyName, 0, 1);
            PreviewViewport.World.DirectionLight.SunLightIntensity = 1.0f;

            #region sdf
            var sdfRPolicyName = RName.GetRName("graphics/sdf.rpolicy", RName.ERNameType.Engine);
            //sdfViewport.Title = $"MaterialMesh:{name}";
            sdfViewport.OnInitialize = Initialize_SdfViewport;
            await sdfViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, sdfRPolicyName, 0, 1);
            var mesh = new Graphics.Mesh.TtRenderMesh();
            var ok = mesh.Initialize(Mesh);
            if (ok)
            {
                var meshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(sdfViewport.World, sdfViewport.World.Root, new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
                meshNode.NodeData.Name = "PreviewObject";
                meshNode.IsAcceptShadow = false;
                meshNode.IsCastShadow = false;
            }
            #endregion

            MeshPropGrid.Target = Mesh;
            EditorPropGrid.Target = this;
            TtEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public void OnCloseEditor()
        {
            TtEngine.Instance.TickableManager.RemoveTickable(this);
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
            IsDrawing = EGui.UIProxy.DockProxy.BeginMainForm(GetWindowsName(), this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (IsDrawing)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
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
            DrawSkeleton();
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
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Left, 0.8f, ref middleId, ref rightId);
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Down, 0.5f, ref rightDownId, ref rightUpId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir.ImGuiDir_Down, 0.3f, ref downId, ref middleId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir.ImGuiDir_Left, 0.2f, ref leftId, ref middleId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Skeleton", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Preview", mDockKeyClass), middleId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("sdfPreview", mDockKeyClass), middleId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("EditorDetails", mDockKeyClass), rightUpId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("MeshDetails", mDockKeyClass), rightUpId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("BoneDetails", mDockKeyClass), rightUpId);

            ImGuiAPI.DockBuilderFinish(id);
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                Mesh.SaveAssetTo(Mesh.AssetName);
                var unused = TtEngine.Instance.GfxDevice.MaterialMeshManager.ReloadMaterialMesh(Mesh.AssetName);

                //USnapshot.Save(Mesh.AssetName, Mesh.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
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
                var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
                ameta.AutoGenSnapshot().AddWaitTask();
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
                MeshPropGrid.OnDraw(true, false, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar, 
                    ImGuiChildFlags_.ImGuiChildFlags_AlwaysAutoResize | ImGuiChildFlags_.ImGuiChildFlags_AutoResizeX | ImGuiChildFlags_.ImGuiChildFlags_AutoResizeY);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        TtSkeletonTreePanel SkeletonTreePanel = new TtSkeletonTreePanel();
        bool mShowSkeletonPanel = true;

        public void OnBoneSelected(ILimb selectedBone)
        {
        }

        public void OnShapeSelected(Graphics.Mesh.PhysicsAsset.TtCollisionShape shape, GamePlay.Scene.TtNode proxyNode)
        {
        }

        bool mShowBoneDetails = true;
        protected void DrawSkeleton()
        {
            if (!SkeletonTreePanel.HasSkeleton)
                return;
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Skeleton", ref mShowSkeletonPanel, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                SkeletonTreePanel.OnDrawTree();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);

            var showDetails = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "BoneDetails", ref mShowBoneDetails, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (showDetails)
            {
                SkeletonTreePanel.OnDrawBoneDetails();
            }
            EGui.UIProxy.DockProxy.EndPanel(showDetails);
        }
        bool ShowPreview = true;
        protected unsafe void DrawPreview()
        {
            #region mesh
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Preview", ref ShowPreview, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                PreviewViewport.ViewportType = Graphics.Pipeline.TtViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();
            }
            this.PreviewViewport.Visible = show;
            EGui.UIProxy.DockProxy.EndPanel(show);
            #endregion

            #region sdf
            var showSdf = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "sdfPreview", ref ShowPreview, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (showSdf)
            {
                sdfViewport.ViewportType = Graphics.Pipeline.TtViewportSlate.EViewportType.ChildWindow;
                sdfViewport.OnDraw();
            }
            sdfViewport.Visible = true;
            EGui.UIProxy.DockProxy.EndPanel(showSdf);
            #endregion
        }
        #endregion

        public void OnEvent(in Bricks.Input.Event e)
        {

        }
        #region Tickable
        public override void TickLogic(float ellapse)
        {
            PreviewViewport.TickLogic(ellapse);
            sdfViewport.TickLogic(ellapse);
        }
        public override void TickRender(float ellapse)
        {
            PreviewViewport.TickRender(ellapse);

            if (IsDrawing == false)
                return;

            base.TickRender(ellapse);
        }
        public override void TickBeginFrame(float ellapse)
        {

        }
        public override void TickSync(float ellapse)
        {
            PreviewViewport.TickSync(ellapse);
            sdfViewport.TickSync(ellapse);
        }

        public string GetWindowsName()
        {
            return AssetName.Name;
        }
        #endregion
    }
}

namespace EngineNS.Graphics.Mesh
{
    [Editor.TtAssetEditor(EditorType = typeof(Editor.Forms.TtMeshEditor))]
    public partial class TtMaterialMesh
    {
        
    }
}
