using EngineNS.Animation.Asset;
using EngineNS.Animation.Pipeline;
using EngineNS.Animation.Player;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
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
    /// <summary>
    /// 骨骼球体的 HitProxy 代理对象，使编辑器视口中的骨骼球可被点击拾取。
    /// </summary>
    public class TtBoneHitProxy : Graphics.Pipeline.IProxiable
    {
        public int BoneIndex;
        public string BoneName;
        public ILimb Limb;

        public Graphics.Pipeline.TtHitProxy HitProxy { get; set; }
        public Graphics.Pipeline.TtHitProxy.EHitproxyType HitproxyType { get; set; } = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
        public bool Selected { get; set; }

        public Graphics.Mesh.TtRenderMesh Mesh { get; set; }

        public void OnHitProxyChanged()
        {
            if (Mesh == null) return;
            if (HitProxy == null || HitproxyType == Graphics.Pipeline.TtHitProxy.EHitproxyType.None)
            {
                Mesh.IsDrawHitproxy = false;
                return;
            }
            Mesh.IsDrawHitproxy = true;
            var value = HitProxy.ConvertHitProxyIdToVector4();
            Mesh.SetHitproxy(in value);
        }

        public void GetHitProxyDrawMesh(List<Graphics.Mesh.TtRenderMesh> meshes)
        {
            if (Mesh != null)
                meshes.Add(Mesh);
        }
    }

    //[Rtti.Meta("", NameAlias = new string[] { "EngineNS.Editor.Forms.USkeletonShowNode@EngineCore", "EngineNS.Editor.Forms.USkeletonShowNode" })]
    public class TtSkeletonShowNode : TtVisual
    {
        // 从配置读取颜色，支持用户自定义
        static TtMeshPrimitiveEditorConfig GetEditorConfig()
        {
            return TtEngine.Instance?.ConfigManager?.GetConfig<TtMeshPrimitiveEditorConfig>();
        }
        static Color4b BoneSphereColor => GetEditorConfig()?.BoneSphereColor ?? Color4b.Green;
        static Color4b BoneSphereHighlightColor => GetEditorConfig()?.BoneSphereHighlightColor ?? Color4b.FromArgb(0xFF, 0xFF, 0x00, 0x00);
        static Color4b BoneLineColor => GetEditorConfig()?.BoneLineColor ?? Color4b.Green;
        public static async Thread.Async.TtTask<TtSkeletonShowNode> AddNode(GamePlay.TtWorld world, TtNode parent, TtNodeData data, Type placementType, DVector3 pos, Vector3 scale, Quaternion quat)
        {
            var scene = parent.GetNearestParentScene();
            var node = await GamePlay.Scene.TtNode.SpawnNode<TtSkeletonShowNode>(parent, null, data, EBoundVolumeType.Box, placementType);
            node.NodeData.Name = node.NodeId.ToString();

            node.Placement.SetTransform(in pos, in scale, in quat);

            return node;
        }

        public class TtSkeletonShowNodeData : TtNodeData
        {
            public TtSkeletonAsset SkeletonAsset { get; set; } = null;
        }
        Dictionary<int, TtRenderMesh> BoneMeshes = new();
        Dictionary<FBoneLine, TtRenderMesh> BoneLineMeshes = new();
        Dictionary<int, TtBoneHitProxy> BoneProxies = new();

        /// <summary>
        /// 查找与 HitProxy 拾取结果对应的 TtBoneHitProxy，用于外部处理骨骼选中
        /// </summary>
        public TtBoneHitProxy FindBoneProxy(Graphics.Pipeline.IProxiable proxy)
        {
            if (proxy is TtBoneHitProxy boneProxy && BoneProxies.ContainsValue(boneProxy))
                return boneProxy;
            return null;
        }
        public TtSkeletonAsset SkeletonAsset { get; set; } = null;
        public TtLocalSpaceRuntimePose CurrentPose = null;
        bool mXRay = false;

        Graphics.Pipeline.Shader.TtMaterial ShowMaterial = null;
        Graphics.Pipeline.Shader.TtMaterial GetBoneMaterial()
        {
            if (ShowMaterial == null)
            {
                ShowMaterial = TtEngine.Instance.GfxDevice.MaterialInstanceManager.VtxColorMaterial;
            }
            return ShowMaterial;
        }

        /// <summary>
        /// 开启/关闭 X-Ray 模式（关闭深度测试，骨架始终绘制在最前面）
        /// </summary>
        public void SetXRay(bool xray)
        {
            mXRay = xray;
            var mat = GetBoneMaterial();
            var ds = mat.DepthStencil;
            ds.m_DepthEnable = xray ? 0 : 1;
            mat.DepthStencil = ds;
        }

        protected override Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var nodeData = data as TtSkeletonShowNodeData;
            SkeletonAsset = nodeData.SkeletonAsset;
            var animPose = SkeletonAsset.Skeleton.CreateSkeletonPose();
            CurrentPose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(animPose);
            //var runtimePose = TtRuntimePoseUtility.ConvetToMeshSpaceRuntimePose(CurrentPose);
            var wireMat = GetBoneMaterial();
            for (int i = 0; i < SkeletonAsset.Skeleton.Limbs.Count; ++i)
            {
                var limb = SkeletonAsset.Skeleton.Limbs[i];
                var index = limb.Index.Value;
                var meshProvider = Graphics.Mesh.TtMeshDataProvider.MakeSphere(0.005f, 5, 5, BoneSphereColor.ToB8G8R8A8());
                var mesh = meshProvider.ToDrawMesh(wireMat);
                BoneMeshes.Add(index, mesh);

                // 为每个骨骼球创建 HitProxy 代理
                var boneProxy = new TtBoneHitProxy()
                {
                    BoneIndex = index,
                    BoneName = limb.Desc?.Name,
                    Limb = limb,
                    Mesh = mesh
                };
                TtEngine.Instance.GfxDevice.HitproxyManager.MapProxy(boneProxy);
                BoneProxies.Add(index, boneProxy);
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
                var meshProvider = Graphics.Mesh.TtMeshDataProvider.MakeBox(0, -0.0005f, -0.0005f, 1, 0.001f, 0.001f, BoneLineColor.ToR8G8B8A8());
                var mesh = meshProvider.ToDrawMesh(GetBoneMaterial());
                var boneLine = new FBoneLine() { Start = start, End = end };
                BoneLineMeshes.Add(boneLine, mesh);
                CreateBoneLineMesh(child);
            }
        }
        void ShowBoneLineWhitInitMatrix(ILimb limb, TtWorld.TtVisParameter rp)
        {
            var startIndex = limb.Index.Value;
            var startTranslation = limb.Desc.InitMatrix.Translation;
            foreach (var child in limb.Children)
            {
                var endIndex = child.Index.Value;
                var endTranslation = child.Desc.InitMatrix.Translation;
                var dir = endTranslation - startTranslation;
                var length = dir.Length();
                dir.Normalize();
                Quaternion rotation = Quaternion.GetQuaternion(Vector3.Right, dir);
                var boneLine = new FBoneLine() { Start = startIndex, End = endIndex };
                if (BoneLineMeshes.ContainsKey(boneLine))
                {
                    FTransform transfrom = FTransform.CreateTransform(startTranslation.AsDVector(), new Vector3(length, 1, 1), rotation);
                    BoneLineMeshes[boneLine].SetWorldTransform(transfrom, rp.World, false);
                    rp.AddVisibleMesh(BoneLineMeshes[boneLine]);
                }
                ShowBoneLineWhitInitMatrix(child, rp);
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
        int mHighlightedBoneIndex = -1;

        /// <summary>
        /// 高亮指定骨骼（放大球体并变色），传 null 取消高亮
        /// </summary>
        public void HighlightBone(string boneName)
        {
            int newIndex = -1;
            if (boneName != null && SkeletonAsset?.Skeleton != null)
            {
                foreach (var limb in SkeletonAsset.Skeleton.Limbs)
                {
                    if (limb.Desc?.Name == boneName)
                    {
                        newIndex = limb.Index.Value;
                        break;
                    }
                }
            }

            if (mHighlightedBoneIndex == newIndex)
                return;

            var mat = GetBoneMaterial();

            // 恢复旧的
            if (mHighlightedBoneIndex >= 0 && BoneMeshes.ContainsKey(mHighlightedBoneIndex))
            {
                var mp = Graphics.Mesh.TtMeshDataProvider.MakeSphere(0.005f, 5, 5, BoneSphereColor.ToR8G8B8A8());
                BoneMeshes[mHighlightedBoneIndex] = mp.ToDrawMesh(mat);
                // 同步更新 HitProxy 代理的网格引用
                if (BoneProxies.TryGetValue(mHighlightedBoneIndex, out var oldProxy))
                {
                    oldProxy.Mesh = BoneMeshes[mHighlightedBoneIndex];
                    oldProxy.OnHitProxyChanged();
                }
            }

            // 高亮新的
            if (newIndex >= 0 && BoneMeshes.ContainsKey(newIndex))
            {
                var mp = Graphics.Mesh.TtMeshDataProvider.MakeSphere(0.01f, 8, 8, BoneSphereHighlightColor.ToR8G8B8A8());
                BoneMeshes[newIndex] = mp.ToDrawMesh(mat);
                // 同步更新 HitProxy 代理的网格引用
                if (BoneProxies.TryGetValue(newIndex, out var newProxy))
                {
                    newProxy.Mesh = BoneMeshes[newIndex];
                    newProxy.OnHitProxyChanged();
                }
            }

            mHighlightedBoneIndex = newIndex;
        }

        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            if (CurrentPose == null)
                return;

            var runtimePose = TtRuntimePoseUtility.ConvetToMeshSpaceRuntimePose(CurrentPose);
            foreach(var bone in SkeletonAsset.Skeleton.Limbs)
            {
                var translation = bone.Desc.InitMatrix.Translation;
                var transfrom = FTransform.CreateTransform(translation.AsDVector(), Vector3.One, Quaternion.Identity);
                BoneMeshes[bone.Index.Value].SetWorldTransform(in transfrom, rp.World, true);
                rp.AddVisibleMesh(BoneMeshes[bone.Index.Value]);
            }
            //ShowBoneLine(SkeletonAsset.Skeleton.Root, runtimePose, rp);
            ShowBoneLineWhitInitMatrix(SkeletonAsset.Skeleton.Root, rp);
            base.OnGatherVisibleMeshes(rp);
        }
    }
    public class TtSkeletonEditor : Editor.IAssetEditor, ITickable, IRootForm, ISkeletonTreeHost
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public Animation.Asset.TtSkeletonAsset SkeletonAsset;
        public Editor.TtPreviewViewport PreviewViewport = new Editor.TtPreviewViewport();
        public EGui.Controls.PropertyGrid.TtPropertyGrid AnimationClipPropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        TtSkeletonTreePanel SkeletonTreePanel = new TtSkeletonTreePanel();
        ~TtSkeletonEditor()
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
            uint leftUpId = 0;
            uint leftDownId = 0;
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Left, 0.2f, ref leftId, ref rightId);
            ImGuiAPI.DockBuilderSplitNode(leftId, ImGuiDir.ImGuiDir_Down, 0.5f, ref leftDownId, ref leftUpId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Skeleton", mDockKeyClass), leftUpId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("BoneDetails", mDockKeyClass), leftDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Left", mDockKeyClass), leftDownId);
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
                ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
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
            EGui.UIProxy.DockProxy.EndMainForm(result);

            DrawSkeleton();
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
        public void OnBoneSelected(ILimb selectedBone)
        {
        }

        public void OnShapeSelected(Graphics.Mesh.PhysicsAsset.TtCollisionShape shape, GamePlay.Scene.TtNode proxyNode)
        {
        }

        bool mShowSkeletonPanel = true;
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
        TtSkeletonShowNode SkeletonShowNode = null;
        protected async Thread.Async.TtTask<bool> Initialize_PreviewScene(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();

            (viewport as Editor.TtPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            DBoundingSphere sphere;
            sphere.Center = new DVector3(0, 1, 0);
            sphere.Radius = 5;
            policy.DefaultCamera.AutoZoom(in sphere);

            {
                var nodeDta = new TtSkeletonShowNode.TtSkeletonShowNodeData();
                nodeDta.SkeletonAsset = SkeletonAsset;
                SkeletonShowNode = await TtSkeletonShowNode.AddNode(viewport.World, viewport.World.Root, nodeDta, typeof(GamePlay.TtPlacement), DVector3.Zero, Vector3.One, Quaternion.Identity);
            }

            var planeMaterialName = TtEngine.Instance.ConfigManager.GetConfig<Editor.Forms.TtMeshPrimitiveEditorConfig>().PlaneMaterialName;
            var studioContext = await PreviewViewport.CreateStudioEnvironment(new BoundingBox(3, 3, 3), 5.0f, planeMaterialName);
            PlaneMeshNode = studioContext?.FloorNode;
            return true;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        TtAnimationClipPreview AnimationClipPreview = null;
        public async Thread.Async.TtTask<bool> OpenEditor(TtMainEditorApplication mainEditor, RName name, object arg, bool saveLayout)
        {
            AssetName = name;
            SkeletonAsset = await TtEngine.Instance.AnimationModule.SkeletonAssetManager.GetSkeletonAsset(name);
            if (SkeletonAsset == null)
                return false;

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"MaterialMesh:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, TtEngine.Instance.Config.MainRPolicyName, 0, 1);
            AnimationClipPreview = new TtAnimationClipPreview();
            AnimationClipPreview.SkeletonEditor = this;
            AnimationClipPropGrid.Target = AnimationClipPreview;
            SkeletonTreePanel.SetSkeleton(SkeletonAsset.Skeleton, this);
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
            public TtSkeletonEditor SkeletonEditor = null;
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
                        var animation = await value.GetAsset<Animation.Asset.TtAnimationClip>();
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
    [Editor.TtAssetEditor(EditorType = typeof(Editor.Forms.TtSkeletonEditor))]
    public partial class TtSkeletonAsset
    {

    }
}
