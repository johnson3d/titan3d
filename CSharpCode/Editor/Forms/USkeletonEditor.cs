using EngineNS.Animation.Asset;
using EngineNS.Animation.Pipeline;
using EngineNS.Animation.Player;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using NPOI.SS.Formula.Functions;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor.Forms
{
    struct FBoneLine : IEquatable<FBoneLine>
    {
        public int Start;
        public int End;

        public override bool Equals(object obj)
        {
            return obj is FBoneLine line && Equals(line);
        }

        public bool Equals(FBoneLine other)
        {
            return Start == other.Start;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }

        public static bool operator ==(FBoneLine left, FBoneLine right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FBoneLine left, FBoneLine right)
        {
            return !(left == right);
        }
    }
    public class USkeletonShowNode : TtSceneActorNode
    {
        public static async System.Threading.Tasks.Task<USkeletonShowNode> AddNode(GamePlay.TtWorld world, TtNode parent, TtNodeData data, Type placementType, DVector3 pos, Vector3 scale, Quaternion quat)
        {
            var scene = parent.GetNearestParentScene();
            var node = await scene.NewNode(world, typeof(USkeletonShowNode), data, EBoundVolumeType.Box, placementType) as USkeletonShowNode;
            node.NodeData.Name = node.SceneId.ToString();
            node.Parent = parent;

            node.Placement.SetTransform(in pos, in scale, in quat);

            return node;
        }

        public class USkeletonShowNodeData : TtNodeData
        {
            public TtSkeletonAsset SkeletonAsset { get; set; } = null;
        }
        Dictionary<int, TtMesh> BoneMeshes = new();
        Dictionary<FBoneLine, TtMesh> BoneLineMeshes = new();
        public TtSkeletonAsset SkeletonAsset { get; set; } = null;
        public TtLocalSpaceRuntimePose CurrentPose = null;
        public override Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var nodeData = data as USkeletonShowNodeData;
            SkeletonAsset = nodeData.SkeletonAsset;
            var animPose = SkeletonAsset.Skeleton.CreatePose() as Animation.SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
            CurrentPose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(animPose);
            var runtimePose = TtRuntimePoseUtility.ConvetToMeshSpaceRuntimePose(CurrentPose);
            for (int i = 0; i < SkeletonAsset.Skeleton.Limbs.Count; ++i)
            {
                var index = SkeletonAsset.Skeleton.Limbs[i].Index.Value;
                var meshProvider = Graphics.Mesh.UMeshDataProvider.MakeSphere(0.005f, 5, 5, Color4b.Green.ToArgb());
                var mesh = meshProvider.ToDrawMesh(TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireVtxColorMateria);
                BoneMeshes.Add(index, mesh);
            }
            CreateBoneLineMesh(SkeletonAsset.Skeleton.Root);
            return base.InitializeNode(world, data, bvType, placementType);
        }
        void CreateBoneLineMesh(ILimb limb)
        {
            var start = limb.Index.Value;
            foreach (var child in limb.Children)
            {
                var end = child.Index.Value;
                var meshProvider = Graphics.Mesh.UMeshDataProvider.MakeBox(0, -0.0005f, -0.0005f, 1, 0.001f, 0.001f, Color4b.Green.ToArgb());
                var mesh = meshProvider.ToDrawMesh(TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireVtxColorMateria);
                var boneLine = new FBoneLine() { Start = start, End = end };
                BoneLineMeshes.Add(boneLine, mesh);
                CreateBoneLineMesh(child);
            }
        }
        void ShowBoneLine(ILimb limb, TtMeshSpaceRuntimePose runtimePose, TtWorld.TtVisParameter rp)
        {
            var startIndex = limb.Index.Value;
            var startTransform = runtimePose.Transforms[startIndex];
            foreach(var child in limb.Children)
            {
                var endIndex = child.Index.Value;
                var endTransform = runtimePose.Transforms[endIndex];
                var dir = endTransform.Position.ToSingleVector3() - startTransform.Position.ToSingleVector3();
                var length = dir.Length();
                dir.Normalize();
                Quaternion rotation = Quaternion.GetQuaternion(Vector3.Right, dir);
                var boneLine = new FBoneLine() { Start = startIndex, End = endIndex };
                if(BoneLineMeshes.ContainsKey(boneLine))
                {
                    FTransform transfrom = FTransform.CreateTransform(startTransform.Position, new Vector3(length, 1, 1), rotation);
                    BoneLineMeshes[boneLine].SetWorldTransform(transfrom, rp.World, false);
                    rp.AddVisibleMesh(BoneLineMeshes[boneLine]);
                }
                ShowBoneLine(child, runtimePose, rp);
            }
        }
        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            var runtimePose = TtRuntimePoseUtility.ConvetToMeshSpaceRuntimePose(CurrentPose);
            foreach(var bone in SkeletonAsset.Skeleton.Limbs)
            {
                var transfrom = runtimePose.Transforms[bone.Index.Value];
                BoneMeshes[bone.Index.Value].SetWorldTransform(in transfrom, rp.World, true);
                rp.AddVisibleMesh(BoneMeshes[bone.Index.Value]);
            }
            ShowBoneLine(SkeletonAsset.Skeleton.Root, runtimePose, rp);
            base.OnGatherVisibleMeshes(rp);
        }
    }
    public class USkeletonEditor : Editor.IAssetEditor, ITickable, IRootForm
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public Animation.Asset.TtSkeletonAsset SkeletonAsset;
        public Editor.TtPreviewViewport PreviewViewport = new Editor.TtPreviewViewport();
        public EGui.Controls.PropertyGrid.PropertyGrid AnimationClipPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        ~USkeletonEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            SkeletonAsset = null;
            CoreSDK.DisposeObject(ref PreviewViewport);
            AnimationClipPropGrid.Target = null;
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
            await AnimationClipPropGrid.Initialize();
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
            if (Visible == false || SkeletonAsset == null)
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
                SkeletonAsset.SaveAssetTo(SkeletonAsset.AssetName);
                var unused = TtEngine.Instance.GfxDevice.MaterialMeshManager.ReloadMaterialMesh(SkeletonAsset.AssetName);

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
                    AnimationClipPropGrid.OnDraw(true, false, false);
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

        }
        EngineNS.GamePlay.Scene.TtMeshNode PlaneMeshNode;
        USkeletonShowNode SkeletonShowNode = null;
        protected async System.Threading.Tasks.Task Initialize_PreviewScene(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();

            (viewport as Editor.TtPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            DBoundingSphere sphere;
            sphere.Center = new DVector3(0, 1, 0);
            sphere.Radius = 5;
            policy.DefaultCamera.AutoZoom(in sphere);

            {
                var nodeDta = new USkeletonShowNode.USkeletonShowNodeData();
                nodeDta.SkeletonAsset = SkeletonAsset;
                SkeletonShowNode = await USkeletonShowNode.AddNode(viewport.World, viewport.World.Root, nodeDta, typeof(GamePlay.TtPlacement), DVector3.Zero, Vector3.One, Quaternion.Identity);
            }

            {
                var PlaneMesh = new Graphics.Mesh.TtMesh();
                var tMaterials = new Graphics.Pipeline.Shader.TtMaterial[1];
                tMaterials[0] = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(TtEngine.Instance.Config.MeshPrimitiveEditorConfig.PlaneMaterialName);
                PlaneMesh.Initialize(Graphics.Mesh.UMeshDataProvider.MakePlane(10, 10).ToMesh(), tMaterials,
                    Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                PlaneMeshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), PlaneMesh, new DVector3(0, -0.0001f, 0), Vector3.One, Quaternion.Identity);
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
        TtAnimationClipPreview AnimationClipPreview = null;
        public async Thread.Async.TtTask<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            SkeletonAsset = await TtEngine.Instance.AnimationModule.SkeletonAssetManager.GetSkeletonAsset(name);
            if (SkeletonAsset == null)
                return false;

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"MaterialMesh:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, TtEngine.Instance.Config.SimpleRPolicyName, 0, 1);
            AnimationClipPreview = new TtAnimationClipPreview();
            AnimationClipPreview.SkeletonEditor = this;
            AnimationClipPropGrid.Target = AnimationClipPreview;
            TtEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        TtAnimationClip PreviewAnimationClip = null;
        TtSkeletonAnimationPlayer AnimationPlayer = null;
        public async Task OnPreviewAnimationhChange(TtAnimationClip clip)
        {
            PreviewAnimationClip = clip;
            AnimationPlayer = new TtSkeletonAnimationPlayer(clip);
            var animPose = SkeletonAsset.Skeleton.CreatePose() as Animation.SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
            AnimationPlayer.BindingPose(animPose);
        }

        class TtAnimationClipPreview
        {
            [Browsable(false)]
            public USkeletonEditor SkeletonEditor = null;
            [Browsable(false)]
            public IO.EAssetState AssetState { get; private set; } = IO.EAssetState.Initialized;
            private RName mPreivewAnimation;
            [RName.PGRName(FilterExts = TtAnimationClip.AssetExt)]
            public RName PreivewAnimation
            {
                get
                {
                    return mPreivewAnimation;
                }
                set
                {
                    if (AssetState == IO.EAssetState.Loading)
                        return;
                    mPreivewAnimation = value;
                    AssetState = IO.EAssetState.Loading;
                    System.Action exec = async () =>
                    {
                        var animation = await TtEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(value);
                        if (animation == null)
                        {
                            AssetState = IO.EAssetState.LoadFailed;
                            return;
                        }
                        AssetState = IO.EAssetState.LoadFinished;
                        await SkeletonEditor.OnPreviewAnimationhChange(animation);
                    };
                    exec();
                }
            }
        }

        #endregion IAssetEditor

        #region ITickable
        public void TickLogic(float ellapse)
        {
            var second = ellapse / 1000;
            if (SkeletonShowNode != null && AnimationPlayer != null)
            {
                AnimationPlayer.Update(second);
                AnimationPlayer.Evaluate();
                SkeletonShowNode.CurrentPose = AnimationPlayer.OutPose;
            }
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
            return SkeletonAsset.AssetName.Name;
        }
        #endregion ITickable
    }
}
namespace EngineNS.Animation.Asset
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.USkeletonEditor))]
    public partial class TtSkeletonAsset
    {

    }
}