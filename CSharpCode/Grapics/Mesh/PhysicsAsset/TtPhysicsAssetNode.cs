using EngineNS.Animation.SkeletonAnimation.Runtime;
using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using System.Collections.Generic;

namespace EngineNS.Graphics.Mesh.PhysicsAsset
{
    /// <summary>
    /// 物理资产的可视化节点，派生自 TtVisual。
    /// 管理 PhysicsAsset 中所有 TtCollisionShape 的 DebugMesh 渲染和 HitProxy 拾取。
    /// </summary>
    public class TtPhysicsAssetNode : TtVisual
    {
        public class TtPhysicsAssetNodeData : TtNodeData
        {
        }

        public static async Thread.Async.TtTask<TtPhysicsAssetNode> AddToWorld(TtWorld world, TtNode parent)
        {
            var data = new TtPhysicsAssetNodeData();
            var node = await TtNode.SpawnNode<TtPhysicsAssetNode>(parent, null, data, EBoundVolumeType.Box, typeof(TtPlacement));
            node.NodeData.Name = "PhysicsAssetDebug";
            node.Placement.SetTransform(DVector3.Zero, Vector3.One, Quaternion.Identity);
            node.IsAcceptShadow = false;
            node.IsCastShadow = false;
            node.HitproxyType = TtHitProxy.EHitproxyType.None;
            node.SetStyle(ENodeStyles.VisibleAlways);
            return node;
        }

        // 颜色从 TtMeshPrimitiveEditorConfig 读取，支持用户自定义
        static Editor.Forms.TtMeshPrimitiveEditorConfig GetEditorConfig()
        {
            return TtEngine.Instance?.ConfigManager?.GetConfig<Editor.Forms.TtMeshPrimitiveEditorConfig>();
        }
        public static Color4b ColorNormal => GetEditorConfig()?.PhysicsShapeColor ?? Color4b.FromArgb(0x40, 0, 0xff, 0);
        public static Color4b ColorConstraint => GetEditorConfig()?.ConstraintConeColor ?? Color4b.FromArgb(0xC0, 0x40, 0xA0, 0xFF);

        TtPhysicsAsset mPhysicsAsset;
        List<TtCollisionShape> mShapes = new List<TtCollisionShape>();
        string mHighlightedBone;
        bool mShowDebug = true;
        TtBoneConstraint mActiveConstraint;

        /// <summary>
        /// 关联的骨架显示节点，用于获取骨骼 Pose Transform
        /// </summary>
        public Editor.Forms.TtSkeletonShowNode SkeletonShowNode { get; set; }

        public bool ShowDebug
        {
            get => mShowDebug;
            set
            {
                mShowDebug = value;
                if (value)
                    UnsetStyle(ENodeStyles.Invisible);
                else
                    SetStyle(ENodeStyles.Invisible);
            }
        }

        public TtPhysicsAsset PhysicsAsset => mPhysicsAsset;
        public IReadOnlyList<TtCollisionShape> Shapes => mShapes;

        /// <summary>
        /// 从 PhysicsAsset 构建所有 Shape 的 DebugMesh 并注册 HitProxy
        /// </summary>
        public void BuildFromAsset(TtPhysicsAsset asset)
        {
            ClearShapes();
            mPhysicsAsset = asset;
            if (asset == null)
                return;

            foreach (var body in asset.Bodies)
            {
                foreach (var shape in body.Shapes)
                {
                    shape.BoneName = body.BoneName;
                    shape.BuildDebugMesh(ColorNormal);
                    TtEngine.Instance.GfxDevice.HitproxyManager.MapProxy(shape);
                    mShapes.Add(shape);
                }
            }
        }

        /// <summary>
        /// 参数变化后重建单个 Shape 的 DebugMesh
        /// </summary>
        public void RebuildShape(TtCollisionShape shape)
        {
            shape.BuildDebugMesh(ColorNormal);
        }

        /// <summary>
        /// 高亮指定骨骼的碰撞体，并显示该骨骼关联的 Constraint 椭圆锥
        /// </summary>
        public void HighlightBone(string boneName)
        {
            if (mHighlightedBone == boneName)
                return;
            mHighlightedBone = boneName;

            foreach (var shape in mShapes)
            {
                shape.BuildDebugMesh(ColorNormal);
            }

            // 查找该骨骼关联的 Constraint 并构建椭圆锥 debug mesh
            mActiveConstraint = null;
            if (mPhysicsAsset != null && !string.IsNullOrEmpty(boneName))
            {
                Profiler.Log.WriteLine<Profiler.TtAssetGategory>(Profiler.ELogTag.Info,
                    $"HighlightBone: {boneName}, Constraints count: {mPhysicsAsset.Constraints.Count}");
                foreach (var constraint in mPhysicsAsset.Constraints)
                {
                    if (constraint.BoneNameA == boneName || constraint.BoneNameB == boneName)
                    {
                        mActiveConstraint = constraint;
                        constraint.BuildDebugMesh(ColorConstraint, 0.5f);
                        Profiler.Log.WriteLine<Profiler.TtAssetGategory>(Profiler.ELogTag.Info,
                            $"Found constraint: {constraint.BoneNameA} -> {constraint.BoneNameB}");
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 通过 HitProxy 查找被点击的碰撞形状
        /// </summary>
        public TtCollisionShape FindShapeByHitProxy(TtHitProxy hitProxy)
        {
            if (hitProxy == null)
                return null;
            foreach (var shape in mShapes)
            {
                if (shape.HitProxy == hitProxy)
                    return shape;
            }
            return null;
        }

        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            if (!mShowDebug)
                return;

            // 获取骨骼 Pose 用于定位 Shape
            Animation.SkeletonAnimation.Runtime.Pose.TtMeshSpaceRuntimePose meshPose = null;
            Animation.SkeletonAnimation.Skeleton.TtSkinSkeleton skeleton = null;
            if (SkeletonShowNode?.CurrentPose != null)
            {
                meshPose = Animation.SkeletonAnimation.Runtime.Pose.TtRuntimePoseUtility.ConvetToMeshSpaceRuntimePose(SkeletonShowNode.CurrentPose);
                skeleton = SkeletonShowNode.SkeletonAsset?.Skeleton;
            }

            foreach (var shape in mShapes)
            {
                if (shape.DebugMesh == null)
                    continue;

                // Shape 自身的局部 Transform
                var shapeLocalTransform = FTransform.CreateTransform(
                    shape.Offset.AsDVector(),
                    Vector3.One,
                    shape.Rotation);

                if (skeleton != null && !string.IsNullOrEmpty(shape.BoneName))
                {
                    // 查找骨骼索引
                    int boneIndex = -1;
                    foreach (var limb in skeleton.Limbs)
                    {
                        if (limb.Desc?.Name == shape.BoneName)
                        {
                            boneIndex = limb.Index.Value;
                            break;
                        }
                    }

                    if (boneIndex >= 0 && boneIndex < skeleton.Limbs.Count)
                    {
                        // 使用 InitMatrix 定位，与 TtSkeletonShowNode 的骨骼球保持一致
                        var limbDesc = skeleton.Limbs[boneIndex].Desc;
                        var initMat = limbDesc.InitMatrix;
                        var bonePos = initMat.Translation.AsDVector();
                        var boneQuat = Quaternion.RotationMatrix(in initMat);
                        var boneTransform = FTransform.CreateTransform(bonePos, Vector3.One, boneQuat);
                        FTransform worldTransform;
                        FTransform.MultiplyNoParentScale(out worldTransform, in shapeLocalTransform, in boneTransform);
                        shape.DebugMesh.SetWorldTransform(in worldTransform, rp.World, false);
                    }
                    else
                    {
                        shape.DebugMesh.SetWorldTransform(in shapeLocalTransform, rp.World, false);
                    }
                }
                else
                {
                    shape.DebugMesh.SetWorldTransform(in shapeLocalTransform, rp.World, false);
                }

                rp.AddVisibleMesh(shape.DebugMesh);
            }

            // 渲染选中骨骼的 Constraint 椭圆锥：锥顶在子骨骼，张开方向从父指向子（约束子骨骼的运动范围）
            // 使用 InitMatrix.Translation 定位，与 TtSkeletonShowNode 的骨骼球保持一致
            if (mActiveConstraint?.DebugMesh != null && skeleton != null)
            {
                int childIdx = FindBoneIndex(skeleton, mActiveConstraint.BoneNameB);
                int parentIdx = FindBoneIndex(skeleton, mActiveConstraint.BoneNameA);

                if (childIdx >= 0 && parentIdx >= 0
                    && childIdx < skeleton.Limbs.Count && parentIdx < skeleton.Limbs.Count)
                {
                    var childPos = skeleton.Limbs[childIdx].Desc.InitMatrix.Translation.AsDVector();
                    var parentPos = skeleton.Limbs[parentIdx].Desc.InitMatrix.Translation.AsDVector();

                    // 父→子方向
                    var dir = (childPos - parentPos).ToSingleVector3();
                    if (dir.LengthSquared() > 1e-8f)
                    {
                        dir.Normalize();
                        var coneQuat = Quaternion.GetQuaternion(Vector3.UnitX, dir);
                        var coneTransform = FTransform.CreateTransform(childPos, Vector3.One, coneQuat);
                        mActiveConstraint.DebugMesh.SetWorldTransform(in coneTransform, rp.World, false);
                    }
                    else
                    {
                        var fallback = FTransform.CreateTransform(childPos, Vector3.One, Quaternion.Identity);
                        mActiveConstraint.DebugMesh.SetWorldTransform(in fallback, rp.World, false);
                    }
                    rp.AddVisibleMesh(mActiveConstraint.DebugMesh);
                }
            }

            base.OnGatherVisibleMeshes(rp);
        }

        static int FindBoneIndex(Animation.SkeletonAnimation.Skeleton.TtSkinSkeleton skeleton, string boneName)
        {
            if (string.IsNullOrEmpty(boneName) || skeleton == null)
                return -1;
            foreach (var limb in skeleton.Limbs)
            {
                if (limb.Desc?.Name == boneName)
                    return limb.Index.Value;
            }
            return -1;
        }

        void ClearShapes()
        {
            mShapes.Clear();
            mHighlightedBone = null;
            mActiveConstraint = null;
        }
    }
}
