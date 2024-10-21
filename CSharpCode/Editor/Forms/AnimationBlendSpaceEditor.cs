using EngineNS.Animation.Asset;
using EngineNS.Animation.Asset.BlendSpace;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using EngineNS.Thread.Async;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor.Forms
{
    public class TtAnimationBlendSpaceEditor : Editor.IAssetEditor, ITickable, IRootForm
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public TtBlendSpace2D BlendSpace;
        public Editor.TtPreviewViewport PreviewViewport = new Editor.TtPreviewViewport();
        public EGui.Controls.PropertyGrid.PropertyGrid AnimationPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        ~TtAnimationBlendSpaceEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            BlendSpace = null;
            CoreSDK.DisposeObject(ref PreviewViewport);
            AnimationPropGrid.Target = null;
        }
        #region IAssetEditor
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public IRootForm GetRootForm()
        {
            return this;
        }

        public async Thread.Async.TtTask<bool> Initialize()
        {
            await AnimationPropGrid.Initialize();
            return true;
        }

        public void OnCloseEditor()
        {
            TtEngine.Instance.TickableManager.RemoveTickable(this);
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
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Left, 0.2f, ref leftId, ref rightId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Left", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Right", mDockKeyClass), rightId);
            ImGuiAPI.DockBuilderFinish(id);
        }
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public void OnDraw()
        {
            if(Visible == false || BlendSpace == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm(GetWindowsName(), this, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
            if (result)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
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
            EGui.UIProxy.DockProxy.EndMainForm(result);

            DrawLeft();
            DrawRight();
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                BlendSpace.SaveAssetTo(BlendSpace.AssetName);
                var unused = TtEngine.Instance.GfxDevice.MaterialMeshManager.ReloadMaterialMesh(BlendSpace.AssetName);

                //USnapshot.Save(AnimationClip.AssetName, AnimationClip.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
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
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Left", ref mLeftShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                if (ImGuiAPI.CollapsingHeader("Property", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    AnimationPropGrid.OnDraw(true, false, false);
                }
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool mRightShow = true;
        protected unsafe void DrawRight()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Right", ref mRightShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                PreviewViewport.ViewportType = Graphics.Pipeline.TtViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        public void OnEvent(in Bricks.Input.Event e)
        {
            //throw new NotImplementedException();
        }
        EngineNS.GamePlay.Scene.TtMeshNode mCurrentMeshNode;
        public float PlaneScale = 5.0f;
        EngineNS.GamePlay.Scene.TtMeshNode PlaneMeshNode;
        protected async System.Threading.Tasks.Task<bool> Initialize_PreviewScene(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();

            (viewport as Editor.TtPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var aabb = new BoundingBox(3,3,3);
            float radius = aabb.GetMaxSide();
            DBoundingSphere sphere;
            sphere.Center = aabb.GetCenter().AsDVector() + new DVector3(0, 1, 0);
            sphere.Radius = radius;
            policy.DefaultCamera.AutoZoom(in sphere);

            {
                var PlaneMesh = new Graphics.Mesh.TtMesh();
                var tMaterials = new Graphics.Pipeline.Shader.TtMaterial[1];
                tMaterials[0] = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(TtEngine.Instance.Config.MeshPrimitiveEditorConfig.PlaneMaterialName);
                PlaneMesh.Initialize(Graphics.Mesh.TtMeshDataProvider.MakePlane(10, 10).ToMesh(), tMaterials,
                    Rtti.TtTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                PlaneMeshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), PlaneMesh, new DVector3(0, -0.0001f, 0), Vector3.One, Quaternion.Identity);
                PlaneMeshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
                PlaneMeshNode.NodeData.Name = "Plane";
                PlaneMeshNode.IsAcceptShadow = true;
                PlaneMeshNode.IsCastShadow = false;
            }

            var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(viewport.World, viewport.World.Root);
            gridNode.ViewportSlate = this.PreviewViewport;

            return true;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        TtAnimationBlendSpacePreview AnimationPreview = null;
        public async Thread.Async.TtTask<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            BlendSpace = await TtEngine.Instance.AnimationModule.BlendSpaceClipManager.GetAnimation(name);
            if (BlendSpace == null)
                return false;

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"MaterialMesh:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, TtEngine.Instance.Config.SimpleRPolicyName, 0, 1);
            AnimationPreview = new TtAnimationBlendSpacePreview();
            AnimationPreview.Editor = this;
            AnimationPreview.Animation = BlendSpace;
            AnimationPropGrid.Target = AnimationPreview;
            TtEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public async TtTask OnPreviewMeshChange(TtMaterialMesh materialMesh)
        {
            if(mCurrentMeshNode != null)
            {
                mCurrentMeshNode.Parent = null;
            }

            var meshData = new EngineNS.GamePlay.Scene.TtMeshNode.TtMeshNodeData();
            meshData.MeshName = materialMesh.AssetName;
            meshData.MdfQueueType = EngineNS.Rtti.TtTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.UMdfSkinMesh));
            meshData.AtomType = EngineNS.Rtti.TtTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.TtMesh.TtAtom));
            var mesh = new Graphics.Mesh.TtMesh();
            mesh.Initialize(materialMesh, Rtti.TtTypeDescGetter<Graphics.Mesh.UMdfSkinMesh>.TypeDesc);

            var meshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(PreviewViewport.World, PreviewViewport.World.Root, meshData, typeof(GamePlay.TtPlacement), mesh,
                        DVector3.Zero, Vector3.One, Quaternion.Identity);
            meshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
            meshNode.NodeData.Name = "PreviewObject";
            meshNode.IsAcceptShadow = true;
            meshNode.IsCastShadow = true;
            
            mCurrentMeshNode = meshNode;

            //await EngineNS.Animation.SceneNode.TtAnimStateMachinePlayNode.Add(PreviewViewport.World, mCurrentMeshNode, new GamePlay.Scene.UNodeData(),
            //                EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UIdentityPlacement));

            var sapnd = new TtBlendSpaceAnimPreviewNode.TtBlendSpaceAnimPreviewNodeData();
            sapnd.Name = "PlayBlendSpaceAnim";
            sapnd.AnimatinName = AssetName;
            sapnd.Preview = AnimationPreview;
            await TtBlendSpaceAnimPreviewNode.AddBlendSpace2DAnimPreviewNode(PreviewViewport.World, mCurrentMeshNode, sapnd,
                            EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtIdentityPlacement));
        }
        [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
        class TtAnimationBlendSpacePreview
        {
            [Browsable(false)]
            public TtAnimationBlendSpaceEditor Editor = null;
            [Browsable(false)]
            public IO.EAssetState AssetState { get; private set; } = IO.EAssetState.Initialized;
            [Browsable(false)]
            private RName mPreivewMeshName;
            [Category("Option")]
            [RName.PGRName(FilterExts = TtMaterialMesh.AssetExt)]
            public RName PreivewMesh
            {
                get
                {
                    return mPreivewMeshName;
                }
                set
                {
                    if (AssetState == IO.EAssetState.Loading)
                        return;
                    mPreivewMeshName = value;
                    AssetState = IO.EAssetState.Loading;
                    System.Action exec = async () =>
                    {
                        var mesh = await TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(value);
                        if (mesh == null)
                        {
                            AssetState = IO.EAssetState.LoadFailed;
                            return;
                        }
                        AssetState = IO.EAssetState.LoadFinished;
                        await Editor.OnPreviewMeshChange(mesh);
                    };
                    exec();
                }
            }
            [Category("Option")]
            public TtBlendSpace2D Animation { get; set; } = new();
            [Category("Option")]
            public Vector2 PreviewInput { get; set; } = Vector2.Zero;
        }

        class TtBlendSpaceAnimPreviewNode : GamePlay.Scene.TtLightWeightNodeBase
        {
            public class TtBlendSpaceAnimPreviewNodeData : TtNodeData
            {
                public RName AnimatinName { get; set; }
                public TtAnimationBlendSpacePreview Preview { get; set; }
            }
            public Animation.Player.TtBlendSpace2DPlayer Player { get; set; }

            public override TtNode Parent
            {
                get => base.Parent;
                set
                {
                    base.Parent = value;

                }
            }
            public Vector3 Input = Vector3.Zero;

            public override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
            {
                SetStyle(ENodeStyles.Invisible);
                if (!await base.InitializeNode(world, data, bvType, placementType))
                {
                    return false;
                }

                var animPlayNodeData = data as TtBlendSpaceAnimPreviewNodeData;
                var bs2D = await TtEngine.Instance.AnimationModule.BlendSpaceClipManager.GetAnimation(animPlayNodeData.AnimatinName);
                Player = new Animation.Player.TtBlendSpace2DPlayer(bs2D);

                return true;
            }
            public void BindingTo(TtMeshNode meshNode)
            {
                System.Diagnostics.Debug.Assert(meshNode != null);

                var pose = meshNode?.Mesh?.MaterialMesh?.SubMeshes[0].Mesh?.PartialSkeleton?.CreatePose() as Animation.SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
                var skinMDfQueue = meshNode.Mesh.MdfQueue as Graphics.Mesh.UMdfSkinMesh;

                skinMDfQueue.SkinModifier.RuntimePose = Animation.SkeletonAnimation.Runtime.Pose.TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(pose);
                Player.BindingPose(pose);
                Player.RuntimePose = skinMDfQueue.SkinModifier.RuntimePose;

            }
            [ThreadStatic]
            private static Profiler.TimeScope mScopeTick;
            private static Profiler.TimeScope ScopeTick
            {
                get
                {
                    if (mScopeTick == null)
                        mScopeTick = new Profiler.TimeScope(typeof(TtBlendSpaceAnimPreviewNode), nameof(TickLogic));
                    return mScopeTick;
                }
            }
            public override void TickLogic(TtNodeTickParameters args)
            {
                using (new Profiler.TimeScopeHelper(ScopeTick))
                {
                    var animPlayNodeData = NodeData as TtBlendSpaceAnimPreviewNodeData;
                    var previewInput = animPlayNodeData.Preview.PreviewInput;
                    Player.Input = new Vector3(previewInput.X, previewInput.Y, 0);
                    Player.Update(args.World.DeltaTimeSecond);
                    Player.Evaluate();
                }
            }

            public static async System.Threading.Tasks.Task<TtBlendSpaceAnimPreviewNode> AddBlendSpace2DAnimPreviewNode(GamePlay.TtWorld world, TtNode parent, TtNodeData data, EBoundVolumeType bvType, Type placementType)
            {
                System.Diagnostics.Debug.Assert(parent is TtMeshNode);
                var node = new TtBlendSpaceAnimPreviewNode();
                await node.InitializeNode(world, data, bvType, placementType);
                node.BindingTo(parent as TtMeshNode);
                node.Parent = parent;
                return node;
            }
        }

        #endregion IAssetEditor

        #region ITickable
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

        public string GetWindowsName()
        {
            return BlendSpace.AssetName.Name;
        }
        #endregion ITickable
    }
}
namespace EngineNS.Animation.Asset.BlendSpace
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.TtAnimationBlendSpaceEditor))]
    public partial class TtBlendSpace2D
    {

    }
}