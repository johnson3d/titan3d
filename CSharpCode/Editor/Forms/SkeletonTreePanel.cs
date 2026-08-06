using EngineNS.Animation.Asset;
using EngineNS.Animation.SkeletonAnimation.Skeleton;
using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using EngineNS.Graphics.Mesh.PhysicsAsset;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Editor.Forms
{
    /// <summary>
    /// BoneDetails 面板里可编辑的骨骼 Transform 代理。
    /// TtBoneDesc 上的 Position/Rotation/Scale 只是 InitMatrix（网格空间绑定姿势）的只读投影，
    /// 直接给它加 setter 会把 InvInitMatrix / Inv* 缓存和子骨骼的层级关系写坏，
    /// 所以编辑统一走这个代理：写自身 InitMatrix + 同步 Inv* + 把变换增量刚性传递给所有子孙骨骼。
    /// </summary>
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtBoneEditProxy
    {
        readonly ILimb mLimb;
        readonly RName mSkeletonAssetName;

        public TtBoneEditProxy(ILimb limb, RName skeletonAssetName)
        {
            mLimb = limb;
            mSkeletonAssetName = skeletonAssetName;
        }

        TtBoneDesc Desc => mLimb?.Desc as TtBoneDesc;

        [Category("General")]
        [ReadOnly(true)]
        public string Name => Desc?.Name;

        [Category("General")]
        [ReadOnly(true)]
        public string ParentName => Desc?.ParentName;

        /// <summary>
        /// 当前骨骼的跨编辑器路径 "骨架资产名:骨骼名"。
        /// 右侧 [+] 把它加进全局收藏夹, 其他编辑器(如 DMC 的 KawaiiPhysics 骨骼选择)能直接从收藏夹里挑。
        /// </summary>
        [Category("General")]
        [ReadOnly(true)]
        [Infrastructure.TtPGFavoritePath(Channel = Infrastructure.TtEditorFavoritePaths.ChannelBone, AllowPick = false)]
        public string BonePath => Infrastructure.TtEditorFavoritePaths.MakeBonePath(mSkeletonAssetName, Desc?.Name);

        [Category("Transform")]
        public Vector3 Position
        {
            get
            {
                var desc = Desc;
                return desc != null ? desc.InitMatrix.Translation : Vector3.Zero;
            }
            set
            {
                var desc = Desc;
                if (desc == null)
                    return;
                var mat = desc.InitMatrix;
                mat.SetTrans(value);
                ApplyInitMatrix(mat);
            }
        }

        [Category("Transform")]
        public FRotator Rotation
        {
            get
            {
                var desc = Desc;
                return desc != null ? desc.InitMatrix.Rotation.ToEuler() : new FRotator();
            }
            set
            {
                var desc = Desc;
                if (desc == null)
                    return;
                var mat = desc.InitMatrix;
                ApplyInitMatrix(Matrix.Transformation(mat.Scale, Quaternion.FromEuler(value), mat.Translation));
            }
        }

        [Category("Transform")]
        public Vector3 Scale
        {
            get
            {
                var desc = Desc;
                return desc != null ? desc.InitMatrix.Scale : Vector3.One;
            }
            set
            {
                var desc = Desc;
                if (desc == null)
                    return;
                var mat = desc.InitMatrix;
                ApplyInitMatrix(Matrix.Transformation(value, mat.Rotation, mat.Translation));
            }
        }

        /// <summary>
        /// 写入自身 InitMatrix，并把 old⁻¹ * new 增量右乘到所有子孙骨骼上
        /// （行向量约定下 child = childLocal * parent，因此 newChild = oldChild * oldParent⁻¹ * newParent）
        /// </summary>
        void ApplyInitMatrix(Matrix newInitMatrix)
        {
            var desc = Desc;
            if (desc == null)
                return;
            var oldInv = desc.InitMatrix;
            oldInv.Inverse();
            var delta = oldInv * newInitMatrix;
            SetLimbInitMatrix(mLimb, in newInitMatrix);
            foreach (var child in mLimb.Children)
                ApplyDelta(child, in delta);
        }

        static void ApplyDelta(ILimb limb, in Matrix delta)
        {
            var desc = limb.Desc as TtBoneDesc;
            if (desc != null)
            {
                var mat = desc.InitMatrix * delta;
                SetLimbInitMatrix(limb, in mat);
            }
            foreach (var child in limb.Children)
                ApplyDelta(child, in delta);
        }

        /// <summary>
        /// 写 InitMatrix 并同步蒙皮用的 InvInitMatrix / InvPos / InvQuat / InvScale
        /// （与 AssetImporter.MakeBoneDesc 的计算方式保持一致）
        /// </summary>
        static void SetLimbInitMatrix(ILimb limb, in Matrix initMatrix)
        {
            var desc = limb.Desc as TtBoneDesc;
            if (desc == null)
                return;
            desc.InitMatrix = initMatrix;

            var invInitMatrix = initMatrix;
            invInitMatrix.Inverse();
            desc.InvInitMatrix = invInitMatrix;

            DVector3 invPos;
            Vector3 invScale;
            Quaternion invQuat;
            invInitMatrix.Decompose(out invScale, out invQuat, out invPos);
            if (!invQuat.IsNormalized())
                invQuat.Normalize();
            desc.InvScale = invScale;
            desc.InvQuat = invQuat;
            desc.InvPos = invPos.ToSingleVector3();
        }
    }

    /// <summary>
    /// 宿主编辑器实现此接口以接收骨骼树的选中通知。
    /// </summary>
    public interface ISkeletonTreeHost
    {
        void OnBoneSelected(ILimb selectedBone);
        /// <summary>
        /// Shape 被选中时通知宿主，宿主应将 Axis 绑定到 proxyNode 上
        /// </summary>
        void OnShapeSelected(TtCollisionShape shape, GamePlay.Scene.TtNode proxyNode);
    }

    /// <summary>
    /// 可复用的骨骼树 UI 面板，可嵌入 MeshEditor / MeshPrimitiveEditor / SkeletonEditor 等编辑器的 Dock 窗口。
    /// 自带独立的 BoneDetails PropertyGrid，不会冲击宿主编辑器的 EditorDetails。
    /// </summary>
    public class TtSkeletonTreePanel
    {
        ISkeletonTreeHost mHost;
        TtSkinSkeleton mSkeleton;
        TtBoneTreeDrawer mTreeDrawer;
        TtBoneNode mRootNode;
        ILimb mSelectedBone;
        EGui.Controls.PropertyGrid.TtPropertyGrid mBoneDetailsPropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        bool mBoneDetailsInitialized = false;

        // ─── PhysicsAsset toolbar state ─────────────────────────────
        RName mPhysicsAssetRName;
        bool mEditPhysicsAsset = false;
        bool mShowBones = false;
        GamePlay.TtWorld mWorld;
        TtPhysicsAsset mLoadedPhysicsAsset;
        EGui.Controls.TtContentBrowser mPhyAssetBrowser;
        bool mPhyAssetPopupOpen = false;
        internal TtBoneNode mRClickedBone;
        bool mOpenBoneContextMenu = false;
        TtSkeletonShowNode mSkeletonShowNode;
        bool mXRay = true;

        // ─── Shape Axis proxy ───────────────────────────────────────
        TtCollisionShape mSelectedShape;
        GamePlay.Scene.TtNode mShapeProxyNode;
        bool mDraggingShape = false;
        // 记录选中时的初始状态，用增量方式计算（与 TtAxis.RotWithAxis 同策略）
        FTransform mStartProxyWorldTransform;   // 代理节点选中时的世界 Transform
        Vector3 mStartShapeOffset;              // Shape 选中时的局部 Offset
        Quaternion mStartShapeRotation;         // Shape 选中时的局部 Rotation

        public TtCollisionShape SelectedShape => mSelectedShape;
        public ILimb SelectedBone => mSelectedBone;

        // ─── MeshPrimitives AMeta 关联 ──────────────────────────────
        RName mMeshAssetName;

        // ─── 骨骼树过滤 ─────────────────────────────────────────────
        string mBoneFilterStr = "";
        bool mBoneFilterFocused = false;
        string mAppliedBoneFilter = "";
        TtBoneNode mScrollToNode;

        internal bool IsFiltering => !string.IsNullOrEmpty(mAppliedBoneFilter);

        // ─── 视口 HitProxy 单选 ─────────────────────────────────────
        Graphics.Pipeline.TtRenderPolicy mPickPolicy;
        Graphics.Pipeline.IProxiable mLastPickedBoneOrShape;

        /// <summary>
        /// 骨骼 Transform 是否允许在 BoneDetails 中编辑。
        /// 只有 SkeletonEditor（编辑骨架资产本身）打开，Mesh 类编辑器保持只读显示。
        /// </summary>
        public bool AllowBoneTransformEdit { get; set; } = false;

        /// <summary>
        /// 当前骨架所属的 .skt 资产名，用于拼出可跨编辑器引用的骨骼路径。
        /// Mesh 类编辑器用的是 mesh 自带的 PartialSkeleton，没有独立骨架资产，保持 null 即可。
        /// </summary>
        public RName SkeletonAssetName { get; set; }
        Dictionary<ILimb, TtBoneEditProxy> mBoneEditProxies = new Dictionary<ILimb, TtBoneEditProxy>();

        /// <summary>
        /// 统一Undo/Redo接入点：宿主编辑器把自己的历史栈挂进来，BoneDetails 的属性修改即可撤销
        /// </summary>
        public Infrastructure.TtEditorHistory HistoryHost
        {
            get => mBoneDetailsPropGrid.HistoryHost;
            set => mBoneDetailsPropGrid.HistoryHost = value;
        }

        public void SetSkeleton(TtSkinSkeleton skeleton, ISkeletonTreeHost host)
        {
            mSkeleton = skeleton;
            mHost = host;
            mTreeDrawer = new TtBoneTreeDrawer(this);
            mRootNode = skeleton != null ? new TtBoneNode(this, null, skeleton) : null;
            mSelectedBone = null;
            mBoneEditProxies.Clear();
            mScrollToNode = null;
            mBoneDetailsPropGrid.Target = null;
            RefreshBoneFilter();
        }

        /// <summary>
        /// 复用宿主编辑器已创建的骨架显示节点（而不是自己再建一个），使高亮和 HitProxy 单选可用
        /// </summary>
        public void SetSkeletonShowNode(TtSkeletonShowNode showNode)
        {
            mSkeletonShowNode = showNode;
            if (mSkeletonShowNode == null)
                return;
            mShowBones = true;
            mSkeletonShowNode.SetXRay(mXRay);
            if (mPhysicsAssetNode != null)
                mPhysicsAssetNode.SkeletonShowNode = mSkeletonShowNode;
        }

        /// <summary>
        /// 设置当前编辑的 MeshPrimitives 资产名和 World。会从 AMeta 中读取上次保存的 PhysicsAssetRName 并自动加载。
        /// </summary>
        public void SetMeshAssetName(RName meshAssetName, GamePlay.TtWorld world)
        {
            mMeshAssetName = meshAssetName;
            mWorld = world;
            if (meshAssetName == null)
                return;

            var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(meshAssetName) as Graphics.Mesh.TtMeshPrimitivesAMeta;
            if (ameta?.PhysicsAssetRName != null)
            {
                mPhysicsAssetRName = ameta.PhysicsAssetRName;
                LoadPhysicsAssetAsync(mPhysicsAssetRName);
            }
        }

        public bool HasSkeleton => mSkeleton != null && mSkeleton.Limbs.Count > 0;

        public void SetWorld(GamePlay.TtWorld world)
        {
            mWorld = world;
        }

        public void OnDrawTree()
        {
            DrawPhysicsAssetToolbar();
            ImGuiAPI.Separator();
            DrawBoneFilterBar();

            if (mRootNode == null || mTreeDrawer == null)
                return;
            mTreeDrawer.DrawTree(null, mRootNode, 0);
            // 滚动请求只在被请求后的这一帧生效，未画到（被过滤掉）也不再保留
            mScrollToNode = null;

            if (mOpenBoneContextMenu)
            {
                ImGuiAPI.OpenPopup("BoneContextMenu", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                mOpenBoneContextMenu = false;
            }
            DrawBoneContextMenu();
        }

        void DrawBoneFilterBar()
        {
            var drawList = ImGuiAPI.GetWindowDrawList();
            EGui.UIProxy.SearchBarProxy.OnDraw(ref mBoneFilterFocused, in drawList, "Search Bone", ref mBoneFilterStr, -1);
            if (mBoneFilterStr == mAppliedBoneFilter)
                return;
            mAppliedBoneFilter = mBoneFilterStr;
            RefreshBoneFilter();
        }

        void RefreshBoneFilter()
        {
            mRootNode?.RefreshFilter(IsFiltering ? mAppliedBoneFilter.ToLower() : null);
        }

        internal bool IsScrollTarget(TtBoneNode node)
        {
            return mScrollToNode != null && mScrollToNode == node;
        }

        void DrawPhysicsAssetToolbar()
        {
            var btSize = new Vector2(0, 0);

            // ── Row 1: PhysicsAsset 选择 ──
            ImGuiAPI.Text("PhysAsset:");
            ImGuiAPI.SameLine(0, 4);

            var displayName = mPhysicsAssetRName != null ? mPhysicsAssetRName.Name : "(None)";
            ImGuiAPI.SetNextItemWidth(ImGuiAPI.GetContentRegionAvail().X - 80);
            ImGuiAPI.InputText("##PhyAssetName", ref displayName, ImGuiInputTextFlags_.ImGuiInputTextFlags_ReadOnly);
            ImGuiAPI.SameLine(0, 4);

            if (EGui.UIProxy.CustomButton.ToolButton("Browse", in btSize))
            {
                if (mPhyAssetBrowser == null)
                {
                    mPhyAssetBrowser = TtEditor.NewPopupContentBrowser();
                    mPhyAssetBrowser.ExtNames = TtPhysicsAsset.AssetExt;
                }
                mPhyAssetPopupOpen = true;
                ImGuiAPI.OpenPopup("PhyAssetSelector", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }

            // ── Row 2: ShowBones + EditPhysics + Save ──
            if (ImGuiAPI.Checkbox("ShowBones", ref mShowBones))
            {
                ToggleSkeletonShow(mShowBones);
            }
            ImGuiAPI.SameLine(0, 8);

            if (ImGuiAPI.Checkbox("EditPhysics", ref mEditPhysicsAsset))
            {
                if (mPhysicsAssetNode != null)
                    mPhysicsAssetNode.ShowDebug = mEditPhysicsAsset;
            }
            ImGuiAPI.SameLine(0, 8);

            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                if (mLoadedPhysicsAsset != null && mPhysicsAssetRName != null)
                {
                    mLoadedPhysicsAsset.SaveAssetTo(mPhysicsAssetRName);
                }
            }
            ImGuiAPI.SameLine(0, 8);

            if (ImGuiAPI.Checkbox("XRay", ref mXRay))
            {
                mSkeletonShowNode?.SetXRay(mXRay);
            }

            // ── 弹出的资产选择窗口 ──
            if (mPhyAssetPopupOpen)
            {
                DrawPhyAssetSelectorPopup();
            }
        }

        unsafe void DrawPhyAssetSelectorPopup()
        {
            var pivot = Vector2.Zero;
            var popupSize = new Vector2(500, 400);
            ImGuiAPI.SetNextWindowSize(in popupSize, ImGuiCond_.ImGuiCond_Always);
            if (ImGuiAPI.BeginPopup("PhyAssetSelector", ImGuiWindowFlags_.ImGuiWindowFlags_NoResize))
            {
                mPhyAssetBrowser.OnDraw();

                if (mPhyAssetBrowser.SelectedAssets.Count > 0)
                {
                    var selectedRName = mPhyAssetBrowser.SelectedAssets[0].GetAssetName();
                    if (selectedRName != null)
                    {
                        mPhysicsAssetRName = selectedRName;
                        mPhyAssetBrowser.SelectedAssets.Clear();
                        SavePhysicsAssetRNameToAMeta(selectedRName);
                        LoadPhysicsAssetAsync(selectedRName);
                        mPhyAssetPopupOpen = false;
                        ImGuiAPI.CloseCurrentPopup();
                    }
                }

                ImGuiAPI.EndPopup();
            }
            else
            {
                mPhyAssetPopupOpen = false;
            }
        }

        void SavePhysicsAssetRNameToAMeta(RName physicsAssetRName)
        {
            if (mMeshAssetName == null)
                return;
            var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(mMeshAssetName) as Graphics.Mesh.TtMeshPrimitivesAMeta;
            if (ameta == null)
                return;
            ameta.PhysicsAssetRName = physicsAssetRName;
            ameta.SaveAMeta((IO.IAsset)null);
        }

        async void LoadPhysicsAssetAsync(RName rname)
        {
            var asset = await TtPhysicsAsset.LoadAsset(rname);
            if (asset == null)
            {
                asset = new TtPhysicsAsset();
                asset.AssetName = rname;
            }
            mLoadedPhysicsAsset = asset;
            mEditPhysicsAsset = true;
            mShowBones = true;
            await SetPhysicsAsset(asset, mWorld);
            ToggleSkeletonShow(true);
        }

        async void ToggleSkeletonShow(bool show)
        {
            if (show && mSkeleton != null && mWorld != null)
            {
                if (mSkeletonShowNode == null)
                {
                    var sktAsset = new TtSkeletonAsset();
                    sktAsset.Skeleton = mSkeleton;
                    var nodeData = new TtSkeletonShowNode.TtSkeletonShowNodeData();
                    nodeData.SkeletonAsset = sktAsset;
                    mSkeletonShowNode = await TtSkeletonShowNode.AddNode(mWorld, mWorld.Root, nodeData,
                        typeof(GamePlay.TtPlacement), DVector3.Zero, Vector3.One, Quaternion.Identity);
                    mSkeletonShowNode.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.VisibleAlways);
                    mSkeletonShowNode.SetXRay(mXRay);
                    if (mPhysicsAssetNode != null)
                        mPhysicsAssetNode.SkeletonShowNode = mSkeletonShowNode;
                }
                mSkeletonShowNode.UnsetStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible);
            }
            else if (mSkeletonShowNode != null)
            {
                mSkeletonShowNode.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible);
            }
        }

        // ─── Details 面板 Shapes 列表变化监控 ────────────────────
        int mLastShapesCount = -1;

        public void OnDrawBoneDetails()
        {
            if (!mBoneDetailsInitialized)
            {
                mBoneDetailsPropGrid.Initialize().GetResultUntilCompleted();
                mBoneDetailsInitialized = true;
            }
            mBoneDetailsPropGrid.OnDraw(true, false, false);

            // 监控 Shapes 列表变化：当用户通过 Details 面板“+”按钮添加 Shape 时，自动补全 BoneName 并重建 DebugMesh
            if (mBoneDetailsPropGrid.Target is TtBoneBody body && mLoadedPhysicsAsset != null)
            {
                int curCount = body.Shapes.Count;
                if (mLastShapesCount >= 0 && curCount > mLastShapesCount)
                {
                    // 有新 Shape 被添加，补全初始化
                    bool needRebuild = false;
                    for (int i = 0; i < body.Shapes.Count; i++)
                    {
                        var shape = body.Shapes[i];
                        if (string.IsNullOrEmpty(shape.BoneName))
                        {
                            shape.BoneName = body.BoneName;
                            needRebuild = true;
                        }
                    }
                    if (needRebuild)
                    {
                        if (mSkeleton != null)
                            mLoadedPhysicsAsset.RebuildConstraints(mSkeleton);
                        if (mPhysicsAssetNode != null)
                        {
                            mPhysicsAssetNode.BuildFromAsset(mLoadedPhysicsAsset);
                            mPhysicsAssetNode.ShowDebug = mEditPhysicsAsset;
                        }
                    }
                }
                mLastShapesCount = curCount;
            }
            else
            {
                mLastShapesCount = -1;
            }
        }

        internal void SelectBone(ILimb bone)
        {
            SelectBone(bone, false);
        }

        /// <summary>
        /// 选中骨骼。focusInTree 为 true 时（从 3D 视口点选进来）额外展开祖先并把树滚动到该骨骼
        /// </summary>
        internal void SelectBone(ILimb bone, bool focusInTree)
        {
            if (mSelectedBone == bone)
                return;

            // 互斥：选中骨骼时必须清除 Shape 选中
            if (mSelectedShape != null)
            {
                mSelectedShape = null;
                mDraggingShape = false;
                mHost?.OnShapeSelected(null, null);
            }

            if (mRootNode != null)
                ClearSelection(mRootNode);

            mSelectedBone = bone;

            // 树上同步选中，从 3D 视口点选时还需展开祖先并滚动定位
            FocusTreeNode(bone, focusInTree);

            // 已加载 PhysicsAsset 时显示 TtBoneBody（可编辑 Shapes），
            // 开启骨骼编辑时显示可写的 Transform 代理，否则只读展示 TtBoneDesc
            var boneName = bone?.Desc?.Name;
            if (mLoadedPhysicsAsset != null && boneName != null)
            {
                var boneBody = FindOrCreateBoneBody(boneName);
                mBoneDetailsPropGrid.Target = boneBody;
            }
            else if (AllowBoneTransformEdit && bone != null && bone.Desc is TtBoneDesc)
            {
                mBoneDetailsPropGrid.Target = GetBoneEditProxy(bone);
            }
            else
            {
                mBoneDetailsPropGrid.Target = bone?.Desc;
            }

            // 高亮选中骨骼的碰撞体和骨架球体
            mPhysicsAssetNode?.HighlightBone(boneName);
            mSkeletonShowNode?.HighlightBone(boneName);
            // 视口里只保留这一个被拾取的骨骼
            SyncViewportSelection(GetBoneProxy(bone));

            if (bone != null)
                mHost?.OnBoneSelected(bone);
        }

        TtBoneEditProxy GetBoneEditProxy(ILimb bone)
        {
            if (mBoneEditProxies.TryGetValue(bone, out var proxy))
                return proxy;
            proxy = new TtBoneEditProxy(bone, SkeletonAssetName);
            mBoneEditProxies.Add(bone, proxy);
            return proxy;
        }

        /// <summary>
        /// 在树上选中指定骨骼；scrollTo 为 true 时展开祖先链并请求滚动到可见位置
        /// </summary>
        void FocusTreeNode(ILimb bone, bool scrollTo)
        {
            if (mRootNode == null || bone == null)
                return;
            var node = FindNode(mRootNode, bone);
            if (node == null)
                return;
            node.Selected = true;
            if (!scrollTo)
                return;
            for (var parent = node.Parent; parent != null; parent = parent.Parent)
                parent.NeedExpand = true;
            mScrollToNode = node;
        }

        static TtBoneNode FindNode(TtBoneNode node, ILimb limb)
        {
            if (node.Limb == limb)
                return node;
            foreach (var child in node.Children)
            {
                var found = FindNode(child, limb);
                if (found != null)
                    return found;
            }
            return null;
        }

        TtBoneHitProxy GetBoneProxy(ILimb bone)
        {
            if (bone == null || mSkeletonShowNode == null || !bone.Index.IsValid())
                return null;
            return mSkeletonShowNode.GetBoneProxy(bone.Index.Value);
        }

        /// <summary>
        /// 强制单选：只保留 target 一个骨骼/Shape 处于被拾取状态，
        /// 否则视口里会堆积出多个高亮对象（基类 OnHitproxySelected 不会清旧选中）
        /// </summary>
        void SyncViewportSelection(Graphics.Pipeline.IProxiable target)
        {
            if (mPickPolicy == null)
                return;
            var manager = mPickPolicy.PickedProxiableManager;
            for (int i = manager.PickedProxies.Count - 1; i >= 0; i--)
            {
                var picked = manager.PickedProxies[i];
                if (picked == target)
                    continue;
                if (picked is TtBoneHitProxy || picked is TtCollisionShape)
                    manager.Unselected(picked);
            }
            if (target != null)
                manager.Selected(target);
            mLastPickedBoneOrShape = target;
        }

        TtBoneBody FindOrCreateBoneBody(string boneName)
        {
            foreach (var body in mLoadedPhysicsAsset.Bodies)
            {
                if (body.BoneName == boneName)
                    return body;
            }
            var newBody = new TtBoneBody { BoneName = boneName };
            mLoadedPhysicsAsset.Bodies.Add(newBody);
            if (mSkeleton != null)
                mLoadedPhysicsAsset.RebuildConstraints(mSkeleton);
            return newBody;
        }

        static void ClearSelection(TtBoneNode node)
        {
            node.Selected = false;
            foreach (var child in node.Children)
                ClearSelection(child);
        }

        // ─── Shape Selection & Axis Proxy ───────────────────────────

        /// <summary>
        /// 通过 HitProxy 选中一个 Shape，将 Axis 绑定到代理节点
        /// </summary>
        public void SelectShape(TtCollisionShape shape)
        {
            if (shape == mSelectedShape)
                return;
            mSelectedShape = shape;

            if (shape == null)
            {
                mDraggingShape = false;
                SyncViewportSelection(null);
                mHost?.OnShapeSelected(null, null);
                return;
            }

            // 互斥：选中 Shape 时必须清除骨骼选中
            if (mSelectedBone != null)
            {
                if (mRootNode != null)
                    ClearSelection(mRootNode);
                mSelectedBone = null;
                mSkeletonShowNode?.HighlightBone(null);
            }

            // 显示 Shape 属性到 BoneDetails
            mBoneDetailsPropGrid.Target = shape;
            SyncViewportSelection(shape);

            // 创建或复用代理节点
            if (mShapeProxyNode == null && mWorld != null)
            {
                CreateShapeProxyNode();
            }

            if (mShapeProxyNode != null)
            {
                // 把代理节点移到 Shape 当前世界位置
                var worldPos = GetShapeWorldPosition(shape);
                var worldQuat = GetShapeWorldQuaternion(shape);
                mShapeProxyNode.Placement.Position = worldPos;
                mShapeProxyNode.Placement.Quat = worldQuat;
                // 记录初始状态（与 TtAxis 的 StartAbsTransform 同策略）
                mStartProxyWorldTransform = FTransform.CreateTransform(worldPos, Vector3.One, worldQuat);
                mStartShapeOffset = shape.Offset;
                mStartShapeRotation = shape.Rotation;
                mDraggingShape = true;
                mHost?.OnShapeSelected(shape, mShapeProxyNode);
            }
        }

        async void CreateShapeProxyNode()
        {
            if (mWorld == null || mShapeProxyNode != null)
                return;
            var nodeData = new GamePlay.Scene.TtNodeData();
            nodeData.Name = "ShapeAxisProxy";
            mShapeProxyNode = await GamePlay.Scene.TtNode.SpawnNode<GamePlay.Scene.TtNode>(
                mWorld.Root, null, nodeData, GamePlay.Scene.EBoundVolumeType.Box,
                typeof(GamePlay.TtPlacement), mWorld);
            mShapeProxyNode.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.SelfInvisible);
        }

        /// <summary>
        /// 获取 Shape 在世界空间的位置（骨骼 InitMatrix + Shape Offset）
        /// </summary>
        DVector3 GetShapeWorldPosition(TtCollisionShape shape)
        {
            if (mSkeletonShowNode?.SkeletonAsset?.Skeleton != null && !string.IsNullOrEmpty(shape.BoneName))
            {
                var skeleton = mSkeletonShowNode.SkeletonAsset.Skeleton;
                foreach (var limb in skeleton.Limbs)
                {
                    if (limb.Desc?.Name == shape.BoneName)
                    {
                        var initMat = limb.Desc.InitMatrix;
                        var bonePos = initMat.Translation.AsDVector();
                        var boneQuat = Quaternion.RotationMatrix(in initMat);
                        var boneT = FTransform.CreateTransform(bonePos, Vector3.One, boneQuat);
                        var shapeLocal = FTransform.CreateTransform(shape.Offset.AsDVector(), Vector3.One, shape.Rotation);
                        FTransform worldT;
                        FTransform.MultiplyNoParentScale(out worldT, in shapeLocal, in boneT);
                        return worldT.mPosition;
                    }
                }
            }
            return shape.Offset.AsDVector();
        }

        /// <summary>
        /// 获取 Shape 在世界空间的旋转（骨骼 InitMatrix 旋转 * Shape 本地旋转）
        /// </summary>
        Quaternion GetShapeWorldQuaternion(TtCollisionShape shape)
        {
            if (mSkeletonShowNode?.SkeletonAsset?.Skeleton != null && !string.IsNullOrEmpty(shape.BoneName))
            {
                var skeleton = mSkeletonShowNode.SkeletonAsset.Skeleton;
                foreach (var limb in skeleton.Limbs)
                {
                    if (limb.Desc?.Name == shape.BoneName)
                    {
                        var initMat = limb.Desc.InitMatrix;
                        var boneQuat = Quaternion.RotationMatrix(in initMat);
                        var shapeRot = shape.Rotation;
                        return Quaternion.Multiply(in shapeRot, in boneQuat);
                    }
                }
            }
            return shape.Rotation;
        }

        /// <summary>
        /// 获取骨骼的 InitMatrix Transform（与 Shape/Constraint 渲染保持一致）
        /// </summary>
        bool TryGetBoneTransform(string boneName, out FTransform boneTransform)
        {
            boneTransform = FTransform.Identity;
            if (mSkeletonShowNode?.SkeletonAsset?.Skeleton == null || string.IsNullOrEmpty(boneName))
                return false;

            var skeleton = mSkeletonShowNode.SkeletonAsset.Skeleton;
            foreach (var limb in skeleton.Limbs)
            {
                if (limb.Desc?.Name == boneName)
                {
                    var initMat = limb.Desc.InitMatrix;
                    var bonePos = initMat.Translation.AsDVector();
                    var boneQuat = Quaternion.RotationMatrix(in initMat);
                    boneTransform = FTransform.CreateTransform(bonePos, Vector3.One, boneQuat);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 每帧调用：检测 Axis 拖动代理节点导致的位移和旋转，逆变换回骨骼局部坐标写入 Shape。
        /// 采用与 TtAxis.RotWithAxis 一致的矩阵增量策略：
        ///   增量矩阵 = startMat⁻¹ * currentMat
        ///   新 Shape 世界矩阵 = startShapeWorldMat * 增量矩阵
        ///   局部 = bone⁻¹ * 新世界
        /// </summary>
        public void TickShapeProxy()
        {
            if (!mDraggingShape || mSelectedShape == null || mShapeProxyNode == null)
                return;

            var currentPos = mShapeProxyNode.Placement.Position;
            var currentQuat = mShapeProxyNode.Placement.Quat;

            // 无变化则跳过
            if ((currentPos - mStartProxyWorldTransform.mPosition).Length() < 1e-6
                && (currentQuat - mStartProxyWorldTransform.mQuat).LengthSquared() < 1e-12)
                return;

            // 1. 构造初始代理世界矩阵和当前代理世界矩阵
            DMatrix startProxyMat;
            DMatrix.Transformation(in Vector3.One, in mStartProxyWorldTransform.mQuat,
                in mStartProxyWorldTransform.mPosition, out startProxyMat);

            DMatrix currentProxyMat;
            DMatrix.Transformation(in Vector3.One, in currentQuat, in currentPos, out currentProxyMat);

            // 2. 增量矩阵 = startProxy⁻¹ * currentProxy
            var startProxyInv = startProxyMat;
            startProxyInv.Inverse();
            var deltaMat = startProxyInv * currentProxyMat;

            // 3. 计算初始 Shape 世界矩阵
            DMatrix shapeWorldMat;
            if (TryGetBoneTransform(mSelectedShape.BoneName, out var boneT))
            {
                // 初始 Shape 局部 → 世界
                var shapeLocalT = FTransform.CreateTransform(
                    mStartShapeOffset.AsDVector(), Vector3.One, mStartShapeRotation);
                FTransform shapeWorldT;
                FTransform.MultiplyNoParentScale(out shapeWorldT, in shapeLocalT, in boneT);
                DMatrix.Transformation(in Vector3.One, in shapeWorldT.mQuat,
                    in shapeWorldT.mPosition, out shapeWorldMat);

                // 4. 新 Shape 世界矩阵 = 初始世界 * 增量
                var newShapeWorldMat = shapeWorldMat * deltaMat;

                // 5. 逆变换回骨骼局部：bone⁻¹ * newWorld
                DMatrix boneMat;
                DMatrix.Transformation(in Vector3.One, in boneT.mQuat, in boneT.mPosition, out boneMat);
                boneMat.Inverse();
                var localMat = newShapeWorldMat * boneMat;

                DVector3 localPos;
                Vector3 localScale;
                Quaternion localRot;
                localMat.Decompose(out localScale, out localRot, out localPos);

                mSelectedShape.Offset = localPos.ToSingleVector3();
                localRot.Normalize();
                mSelectedShape.Rotation = localRot;
            }
            else
            {
                // 无骨骼时直接用当前值
                mSelectedShape.Offset = currentPos.ToSingleVector3();
                mSelectedShape.Rotation = currentQuat;
            }
        }

        /// <summary>
        /// 每帧调用：把视口 HitProxy 拾取结果收敛为单选，并把最新选中的骨骼/Shape 派发到面板。
        /// 视口基类的 OnHitproxySelected 只会不断 Selected 而不清旧选中，不收敛就会同时出现多个高亮骨骼。
        /// </summary>
        public void TickViewportPicking(Graphics.Pipeline.TtRenderPolicy policy)
        {
            if (policy == null)
                return;
            mPickPolicy = policy;
            var manager = policy.PickedProxiableManager;

            // 取 PickedProxies 中最后一个 bone/shape，即最新点击的
            Graphics.Pipeline.IProxiable latestPicked = null;
            foreach (var proxy in manager.PickedProxies)
            {
                if (proxy is TtBoneHitProxy || proxy is TtCollisionShape)
                    latestPicked = proxy;
            }

            for (int i = manager.PickedProxies.Count - 1; i >= 0; i--)
            {
                var picked = manager.PickedProxies[i];
                if (picked == latestPicked)
                    continue;
                if (picked is TtBoneHitProxy || picked is TtCollisionShape)
                    manager.Unselected(picked);
            }

            if (latestPicked == mLastPickedBoneOrShape)
                return;
            mLastPickedBoneOrShape = latestPicked;

            if (latestPicked is TtBoneHitProxy pickedBone)
                TryHandleHitProxy(pickedBone);
            else if (latestPicked is TtCollisionShape pickedShape)
                SelectShape(pickedShape);
        }

        /// <summary>
        /// 处理 HitProxy 点击事件——被 TickViewportPicking 派发
        /// </summary>
        public bool TryHandleHitProxy(Graphics.Pipeline.IProxiable proxy)
        {
            // 骨骼球 HitProxy 拾取：始终响应，不依赖 EditPhysicsAsset 状态
            if (proxy is TtBoneHitProxy boneProxy)
            {
                SelectBone(boneProxy.Limb, true);
                return true;
            }

            if (!mEditPhysicsAsset || mPhysicsAssetNode == null)
                return false;

            if (proxy is TtCollisionShape shape && mPhysicsAssetNode.Shapes.Contains(shape))
            {
                SelectShape(shape);
                return true;
            }
            return false;
        }

        // ─── Bone Context Menu (right-click Add Shape) ─────────────

        internal void DrawBoneContextMenu()
        {
            if (ImGuiAPI.BeginPopup("BoneContextMenu", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                bool canAddShape = mRClickedBone != null && mLoadedPhysicsAsset != null;

                if (!canAddShape)
                {
                    ImGuiAPI.TextDisabled("Load a PhysicsAsset first");
                }
                else
                {
                    var boneName = mRClickedBone.Limb.Desc?.Name ?? "";
                    ImGuiAPI.TextDisabled($"Bone: {boneName}");
                    ImGuiAPI.Separator();

                    if (ImGuiAPI.Selectable("Add Sphere", false, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                    {
                        AddShapeToBone(boneName, new TtSphereShape());
                    }
                    if (ImGuiAPI.Selectable("Add Capsule", false, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                    {
                        AddShapeToBone(boneName, new TtCapsuleShape());
                    }
                    if (ImGuiAPI.Selectable("Add Box", false, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                    {
                        AddShapeToBone(boneName, new TtBoxShape());
                    }
                    if (ImGuiAPI.Selectable("Add Plane", false, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                    {
                        AddShapeToBone(boneName, new TtPlaneShape());
                    }
                }

                ImGuiAPI.EndPopup();
            }
        }

        void AddShapeToBone(string boneName, TtCollisionShape shape)
        {
            if (mLoadedPhysicsAsset == null)
                return;

            // 找到或创建对应的 BoneBody
            TtBoneBody targetBody = FindOrCreateBoneBody(boneName);
            targetBody.Shapes.Add(shape);

            // Body 变化后自动重建 Constraints
            if (mSkeleton != null)
                mLoadedPhysicsAsset.RebuildConstraints(mSkeleton);

            // 重建 DebugMesh
            if (mPhysicsAssetNode != null)
            {
                mPhysicsAssetNode.BuildFromAsset(mLoadedPhysicsAsset);
                mPhysicsAssetNode.ShowDebug = mEditPhysicsAsset;
            }

            // 新增 Shape 自动选中，Detail 面板自动显示其属性
            SelectShape(shape);
        }

        /// <summary>
        /// 删除当前选中的 Shape
        /// </summary>
        public void DeleteSelectedShape()
        {
            if (mSelectedShape == null || mLoadedPhysicsAsset == null)
                return;

            var shapeToDelete = mSelectedShape;

            // 先取消选中
            SelectShape(null);

            // 从所属 BoneBody 的 Shapes 列表中移除
            foreach (var body in mLoadedPhysicsAsset.Bodies)
            {
                if (body.Shapes.Remove(shapeToDelete))
                    break;
            }

            // 注销 HitProxy
            TtEngine.Instance.GfxDevice.HitproxyManager.UnmapProxy(shapeToDelete);

            // 重建 Constraints 和 DebugMesh
            if (mSkeleton != null)
                mLoadedPhysicsAsset.RebuildConstraints(mSkeleton);
            if (mPhysicsAssetNode != null)
            {
                mPhysicsAssetNode.BuildFromAsset(mLoadedPhysicsAsset);
                mPhysicsAssetNode.ShowDebug = mEditPhysicsAsset;
            }

            // 重置 Detail 面板
            mBoneDetailsPropGrid.Target = null;
        }

        // ─── PhysicsAsset DebugMesh Management ─────────────────────

        TtPhysicsAssetNode mPhysicsAssetNode;

        public TtPhysicsAssetNode PhysicsAssetNode => mPhysicsAssetNode;

        /// <summary>
        /// 设置 PhysicsAsset，创建/复用 TtPhysicsAssetNode 挂到 World 中。
        /// </summary>
        public async Thread.Async.TtTask SetPhysicsAsset(TtPhysicsAsset physicsAsset, GamePlay.TtWorld world)
        {
            if (physicsAsset == null || world == null)
            {
                if (mPhysicsAssetNode != null)
                    mPhysicsAssetNode.ShowDebug = false;
                return;
            }

            if (mPhysicsAssetNode == null)
            {
                mPhysicsAssetNode = await TtPhysicsAssetNode.AddToWorld(world, world.Root);
            }

            mPhysicsAssetNode.SkeletonShowNode = mSkeletonShowNode;
            if (mSkeleton != null)
                physicsAsset.RebuildConstraints(mSkeleton);
            mPhysicsAssetNode.BuildFromAsset(physicsAsset);
            mPhysicsAssetNode.ShowDebug = true;
        }

        // ─── TreeNode wrapper ───────────────────────────────────────

        internal class TtBoneNode : INodeUIProvider
        {
            internal ILimb Limb;
            internal TtBoneNode Parent;
            TtSkeletonTreePanel mPanel;
            List<TtBoneNode> mChildren;

            // 过滤状态由 RefreshFilter 重算：自身命中 / 子孙有命中 / 是否可见
            internal bool FilterSelfMatch = true;
            internal bool FilterSubtreeMatch = true;
            internal bool FilterVisible = true;
            // 3D 点选定位时，祖先链需要在下一帧强制展开
            internal bool NeedExpand = false;

            public TtBoneNode(TtSkeletonTreePanel panel, TtBoneNode parent, ILimb limb)
            {
                mPanel = panel;
                Parent = parent;
                Limb = limb;
            }

            internal List<TtBoneNode> Children
            {
                get
                {
                    if (mChildren == null)
                    {
                        mChildren = new List<TtBoneNode>();
                        foreach (var child in Limb.Children)
                            mChildren.Add(new TtBoneNode(mPanel, this, child));
                    }
                    return mChildren;
                }
            }

            /// <summary>
            /// 重算过滤可见性：自身或任意子孙命中即可见。lowerFilter 为空表示不过滤
            /// </summary>
            internal bool RefreshFilter(string lowerFilter)
            {
                FilterSelfMatch = string.IsNullOrEmpty(lowerFilter)
                    || (!string.IsNullOrEmpty(NodeName) && NodeName.ToLower().Contains(lowerFilter));
                FilterSubtreeMatch = false;
                foreach (var child in Children)
                {
                    if (child.RefreshFilter(lowerFilter))
                        FilterSubtreeMatch = true;
                }
                FilterVisible = FilterSelfMatch || FilterSubtreeMatch;
                return FilterVisible;
            }

            public int NumOfChildUI() => Children.Count;
            public INodeUIProvider GetChildUI(int index) => index < Children.Count ? Children[index] : null;

            public string NodeName => Limb.Desc != null ? Limb.Desc.Name : "Skeleton";
            public bool Selected { get; set; }

            public bool DrawNode(INodeUIProvider parent, TtTreeNodeDrawer tree, int index, int NumOfChild)
            {
                bool filtering = mPanel != null && mPanel.IsFiltering;
                if (filtering && !FilterVisible)
                    return false;

                var flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow
                          | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;
                if (Selected)
                    flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
                // 过滤时子孙全被过滤掉的节点按叶子画，不给无意义的展开箭头
                if (NumOfChild == 0 || (filtering && !FilterSubtreeMatch))
                    flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;

                // 过滤命中的子树强制展开，使命中项直接可见
                if ((filtering && FilterSubtreeMatch) || NeedExpand)
                {
                    ImGuiAPI.SetNextItemOpen(true, ImGuiCond_.ImGuiCond_Always);
                    NeedExpand = false;
                }

                var label = (string.IsNullOrEmpty(NodeName) ? "EmptyName" : NodeName) + "##" + index;
                bool opened = ImGuiAPI.TreeNodeEx(label, flags);

                if (mPanel != null && mPanel.IsScrollTarget(this))
                    ImGuiAPI.SetScrollHereY(0.5f);

                if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    tree.OnNodeUI_LClick(this);
                if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                    tree.OnNodeUI_RClick(this);

                return opened;
            }

            public GamePlay.TtWorld GetWorld() => null;
        }

        // ─── Tree drawer with selection logic ───────────────────────

        class TtBoneTreeDrawer : TtTreeNodeDrawer
        {
            TtSkeletonTreePanel mPanel;

            public TtBoneTreeDrawer(TtSkeletonTreePanel panel)
            {
                mPanel = panel;
            }

            public override void OnNodeUI_LClick(INodeUIProvider provider)
            {
                if (provider is TtBoneNode boneNode)
                {
                    boneNode.Selected = true;
                    mPanel.SelectBone(boneNode.Limb);
                }
            }

            public override void OnNodeUI_RClick(INodeUIProvider provider)
            {
                if (provider is TtBoneNode boneNode)
                {
                    mPanel.mRClickedBone = boneNode;
                    mPanel.mOpenBoneContextMenu = true;
                }
            }
        }
    }
}
