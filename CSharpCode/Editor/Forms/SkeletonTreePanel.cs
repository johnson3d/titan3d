using EngineNS.Animation.Asset;
using EngineNS.Animation.SkeletonAnimation.Skeleton;
using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using EngineNS.Graphics.Mesh.PhysicsAsset;
using System.Collections.Generic;

namespace EngineNS.Editor.Forms
{
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

        public void SetSkeleton(TtSkinSkeleton skeleton, ISkeletonTreeHost host)
        {
            mSkeleton = skeleton;
            mHost = host;
            mTreeDrawer = new TtBoneTreeDrawer(this);
            mRootNode = skeleton != null ? new TtBoneNode(skeleton) : null;
            mSelectedBone = null;
            mBoneDetailsPropGrid.Target = null;
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

            if (mRootNode == null || mTreeDrawer == null)
                return;
            mTreeDrawer.DrawTree(null, mRootNode, 0);

            if (mOpenBoneContextMenu)
            {
                ImGuiAPI.OpenPopup("BoneContextMenu", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                mOpenBoneContextMenu = false;
            }
            DrawBoneContextMenu();
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

            // 已加载 PhysicsAsset 时显示 TtBoneBody（可编辑 Shapes），否则显示 TtBoneDesc
            var boneName = bone?.Desc?.Name;
            if (mLoadedPhysicsAsset != null && boneName != null)
            {
                var boneBody = FindOrCreateBoneBody(boneName);
                mBoneDetailsPropGrid.Target = boneBody;
            }
            else
            {
                mBoneDetailsPropGrid.Target = bone?.Desc;
            }

            // 高亮选中骨骼的碰撞体和骨架球体
            mPhysicsAssetNode?.HighlightBone(boneName);
            mSkeletonShowNode?.HighlightBone(boneName);

            if (bone != null)
                mHost?.OnBoneSelected(bone);
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
        /// 处理 HitProxy 点击事件——被 MeshPrimitiveEditor 的 OnHitproxySelected 调用
        /// </summary>
        public bool TryHandleHitProxy(Graphics.Pipeline.IProxiable proxy)
        {
            // 骨骼球 HitProxy 拾取：始终响应，不依赖 EditPhysicsAsset 状态
            if (proxy is TtBoneHitProxy boneProxy)
            {
                SelectBone(boneProxy.Limb);
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
            List<TtBoneNode> mChildren;

            public TtBoneNode(ILimb limb)
            {
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
                            mChildren.Add(new TtBoneNode(child));
                    }
                    return mChildren;
                }
            }

            public int NumOfChildUI() => Children.Count;
            public INodeUIProvider GetChildUI(int index) => index < Children.Count ? Children[index] : null;

            public string NodeName => Limb.Desc != null ? Limb.Desc.Name : "Skeleton";
            public bool Selected { get; set; }

            public bool DrawNode(INodeUIProvider parent, TtTreeNodeDrawer tree, int index, int NumOfChild)
            {
                var flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow
                          | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;
                if (Selected)
                    flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
                if (NumOfChild == 0)
                    flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;

                var label = (string.IsNullOrEmpty(NodeName) ? "EmptyName" : NodeName) + "##" + index;
                bool opened = ImGuiAPI.TreeNodeEx(label, flags);

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
