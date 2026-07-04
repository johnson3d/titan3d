using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Graphics.Mesh.PhysicsAsset
{
    /// <summary>
    /// 碰撞形状基类，描述相对于挂载点（骨骼或根节点）的几何体。
    /// 实现 IProxiable 以支持编辑器 HitProxy 拾取。
    /// 派生类定义具体形状参数并实现 CreateDebugMeshProvider 用于编辑器可视化。
    /// </summary>
    [Rtti.Meta("")]
    public class TtCollisionShape : IO.BaseSerializer, IProxiable
    {
        /// <summary>
        /// 相对于挂载点的偏移
        /// </summary>
        [Rtti.Meta("")]
        public Vector3 Offset { get; set; } = Vector3.Zero;

        /// <summary>
        /// 相对于挂载点的旋转
        /// </summary>
        [Rtti.Meta("")]
        public Quaternion Rotation { get; set; } = Quaternion.Identity;

        // ─── IProxiable ─────────────────────────────────────────

        public TtHitProxy HitProxy { get; set; }
        public TtHitProxy.EHitproxyType HitproxyType { get; set; } = TtHitProxy.EHitproxyType.Root;
        public bool Selected { get; set; }

        /// <summary>
        /// 编辑器用的调试渲染 Mesh，由 TtPhysicsAssetNode 管理生命周期
        /// </summary>
        public TtRenderMesh DebugMesh { get; set; }

        /// <summary>
        /// 所属骨骼名（运行时由 TtPhysicsAssetNode 设置）
        /// </summary>
        public string BoneName { get; set; }

        public void SetDebugColor(Color4b color)
        {
            if (DebugMaterial == null)
                return;
            DebugMaterial.SetColor4("clr4_0", color.ToColor4Float());
        }
        public TtMaterialInstance DebugMaterial;
        public unsafe void BuildDebugMesh(Color4b color)
        {
            var oldTransform = DebugMesh?.mTransform ?? FTransform.Identity;
            DebugMaterial = TtEngine.Instance.GfxDevice.MaterialInstanceManager.WhiteColorMaterial.CloneMaterialInstance();
            DebugMaterial.RenderLayer = ERenderLayer.RL_Translucent;
            var blend = DebugMaterial.Blend;
            blend.RenderTarget[0].BlendEnable = 1;
            DebugMaterial.Blend = blend;
            DebugMesh = CreateDebugMeshProvider(0xffffffff).ToDrawMesh(DebugMaterial);
            SetDebugColor(color);
            // 旧矩阵写入新 mesh 的 CBuffer，防止第一帧闪到原点
            DebugMesh.mTransform = oldTransform;
            DebugMesh.DirectSetWorldMatrix(oldTransform.ToMatrixWithScale(in DVector3.Zero));
            DebugMesh.PerMeshCBuffer?.FlushDirty();
            OnHitProxyChanged();
        }

        public void OnHitProxyChanged()
        {
            if (DebugMesh == null)
                return;
            if (HitProxy == null || HitproxyType == TtHitProxy.EHitproxyType.None)
            {
                DebugMesh.IsDrawHitproxy = false;
                return;
            }
            DebugMesh.IsDrawHitproxy = true;
            var value = HitProxy.ConvertHitProxyIdToVector4();
            DebugMesh.SetHitproxy(in value);
        }

        public void GetHitProxyDrawMesh(List<TtRenderMesh> meshes)
        {
            if (DebugMesh != null)
                meshes.Add(DebugMesh);
        }

        /// <summary>
        /// 当 DebugMesh 已存在时，属性变化后自动重建
        /// </summary>
        protected void RebuildDebugMeshIfActive()
        {
            if (DebugMesh != null)
                BuildDebugMesh(TtPhysicsAssetNode.ColorNormal);
        }

        /// <summary>
        /// 创建用于编辑器调试显示的 MeshProvider。
        /// </summary>
        public virtual TtMeshDataProvider CreateDebugMeshProvider(uint color = 0x8000FF00)
        {
            return TtMeshDataProvider.MakeBox(-0.01f, -0.01f, -0.01f, 0.02f, 0.02f, 0.02f, color);
        }
    }

    [Rtti.Meta("")]
    public class TtSphereShape : TtCollisionShape
    {
        float mRadius = 0.05f;
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.005f)]
        public float Radius
        {
            get => mRadius;
            set { mRadius = Math.Max(0.001f, value); RebuildDebugMeshIfActive(); }
        }

        public override TtMeshDataProvider CreateDebugMeshProvider(uint color = 0x8000FF00)
        {
            return TtMeshDataProvider.MakeSphere(Radius, 16, 16, color);
        }
    }

    [Rtti.Meta("")]
    public class TtCapsuleShape : TtCollisionShape
    {
        float mRadius = 0.05f;
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.005f)]
        public float Radius
        {
            get => mRadius;
            set { mRadius = Math.Max(0.001f, value); RebuildDebugMeshIfActive(); }
        }

        float mHalfHeight = 0.1f;
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.005f)]
        public float HalfHeight
        {
            get => mHalfHeight;
            set { mHalfHeight = Math.Max(0.001f, value); RebuildDebugMeshIfActive(); }
        }

        public override TtMeshDataProvider CreateDebugMeshProvider(uint color = 0x8000FF00)
        {
            return TtMeshDataProvider.MakeCapsule(Radius, HalfHeight * 2.0f, 16, 16, 1,
                TtMeshDataProvider.ECapsuleUvProfile.Aspect, color);
        }
    }

    [Rtti.Meta("")]
    public class TtBoxShape : TtCollisionShape
    {
        Vector3 mHalfExtent = new Vector3(0.05f, 0.05f, 0.05f);
        [Rtti.Meta("")]
        public Vector3 HalfExtent
        {
            get => mHalfExtent;
            set { mHalfExtent = value; RebuildDebugMeshIfActive(); }
        }

        public override TtMeshDataProvider CreateDebugMeshProvider(uint color = 0x8000FF00)
        {
            var e = HalfExtent;
            return TtMeshDataProvider.MakeBox(-e.X, -e.Y, -e.Z, e.X * 2, e.Y * 2, e.Z * 2, color);
        }
    }

    [Rtti.Meta("")]
    public class TtPlaneShape : TtCollisionShape
    {
        [Rtti.Meta("")]
        public Vector3 PlaneNormal { get; set; } = Vector3.Up;

        public override TtMeshDataProvider CreateDebugMeshProvider(uint color = 0x8000FF00)
        {
            return TtMeshDataProvider.MakeBox(-0.5f, -0.001f, -0.5f, 1.0f, 0.002f, 1.0f, color);
        }
    }

    /// <summary>
    /// 单根骨骼（或根节点）上的碰撞体定义。
    /// BoneName 为空时表示挂载在 Actor 根节点上，适用于静态网格。
    /// </summary>
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtBoneBody : IO.BaseSerializer
    {
        [Rtti.Meta("")]
        [Category("Bone")]
        [ReadOnly(true)]
        public string BoneName { get; set; }

        [Rtti.Meta("")]
        public uint BoneNameHash { get; set; }

        [Rtti.Meta("")]
        [Category("Bone")]
        [ReadOnly(true)]
        public List<TtCollisionShape> Shapes { get; set; } = new List<TtCollisionShape>();

        [Rtti.Meta("")]
        [Category("Bone")]
        public float Mass { get; set; } = 1.0f;

        [Rtti.Meta("")]
        [Category("Bone")]
        public float LinearDamping { get; set; } = 0.01f;

        [Rtti.Meta("")]
        [Category("Bone")]
        public float AngularDamping { get; set; } = 0.05f;

        /// <summary>
        /// 该 Body 到最近祖先 Body 的关节约束（由 RebuildConstraints 自动关联，不序列化）
        /// </summary>
        [Category("Constraint")]
        [ReadOnly(true)]
        public TtBoneConstraint Constraint { get; set; }
    }

    /// <summary>
    /// 物理约束类型
    /// </summary>
    public enum EConstraintType
    {
        BallSocket,//忽略所有限制参数，3 轴自由旋转
        Hinge,//只有 Swing1 生效（绕单轴的旋转范围），Swing2 和 Twist 被忽略（隐式锁死为 0）
        Limited,//Swing1、Swing2、Twist 三个限制全部生效
        Locked,//全部忽略，0 自由度
    }

    /// <summary>
    /// 两个 BoneBody 之间的物理约束（用于 Ragdoll 等）。
    /// BoneNameA 为空时锚定到世界/根节点。
    /// </summary>
    [Rtti.Meta("")]
    public class TtBoneConstraint : IO.BaseSerializer
    {
        [Rtti.Meta("")]
        [Category("Constraint")]
        [ReadOnly(true)]
        public string BoneNameA { get; set; }

        [Rtti.Meta("")]
        [Category("Constraint")]
        [ReadOnly(true)]
        public string BoneNameB { get; set; }

        EConstraintType mConstraintType = EConstraintType.Limited;
        float mSwing1LimitDeg = 45.0f;
        float mSwing2LimitDeg = 45.0f;
        float mTwistLimitDeg = 30.0f;

        [Rtti.Meta("")]
        [Category("Constraint")]
        public EConstraintType ConstraintType
        {
            get => mConstraintType;
            set { mConstraintType = value; RebuildDebugMeshIfActive(); }
        }

        /// <summary>
        /// Swing1 锥角限制（度），对应骨骼的一个横向摆动轴
        /// </summary>
        [Rtti.Meta("")]
        [Category("Constraint")]
        public float Swing1LimitDeg
        {
            get => mSwing1LimitDeg;
            set { mSwing1LimitDeg = value; RebuildDebugMeshIfActive(); }
        }

        /// <summary>
        /// Swing2 锥角限制（度），对应骨骼的另一个横向摆动轴。
        /// 与 Swing1 不同时形成椭圆锥约束。
        /// </summary>
        [Rtti.Meta("")]
        [Category("Constraint")]
        public float Swing2LimitDeg
        {
            get => mSwing2LimitDeg;
            set { mSwing2LimitDeg = value; RebuildDebugMeshIfActive(); }
        }

        /// <summary>
        /// Twist 扭转限制（度）
        /// </summary>
        [Rtti.Meta("")]
        [Category("Constraint")]
        public float TwistLimitDeg
        {
            get => mTwistLimitDeg;
            set { mTwistLimitDeg = value; RebuildDebugMeshIfActive(); }
        }

        void RebuildDebugMeshIfActive()
        {
            if (DebugMesh != null)
                BuildDebugMesh(TtPhysicsAssetNode.ColorConstraint, 0.5f);
        }

        // ─── Debug Visualization ────────────────────────────────
        [System.ComponentModel.Browsable(false)]
        internal TtRenderMesh DebugMesh { get; set; }
        [System.ComponentModel.Browsable(false)]
        internal Graphics.Pipeline.Shader.TtMaterialInstance DebugMaterial { get; set; }

        internal void SetDebugColor(Color4b color)
        {
            if (DebugMaterial == null)
                return;
            DebugMaterial.SetColor4("clr4_0", color.ToColor4Float());
        }

        internal unsafe void BuildDebugMesh(Color4b color, float coneLength = 0.1f)
        {
            if (ConstraintType == EConstraintType.Locked || ConstraintType == EConstraintType.BallSocket)
            {
                DebugMesh = null;
                return;
            }

            var oldTransform = DebugMesh?.mTransform ?? FTransform.Identity;

            float swing1Rad = Swing1LimitDeg * MathHelper.Deg2Rad;
            float swing2Rad = Swing2LimitDeg * MathHelper.Deg2Rad;

            if (ConstraintType == EConstraintType.Hinge)
                swing2Rad = 0.01f; // 极小值表示锁死

            swing1Rad = Math.Max(swing1Rad, 0.01f);
            swing2Rad = Math.Max(swing2Rad, 0.01f);

            var provider = TtMeshDataProvider.MakeEllipticalConeWireframe(
                coneLength, swing1Rad, swing2Rad, 32, 0xffffffff);

            DebugMaterial = TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria.CloneMaterialInstance();
            DebugMesh = provider.ToDrawMesh(DebugMaterial);
            SetDebugColor(color);
            DebugMesh.mTransform = oldTransform;
            DebugMesh.DirectSetWorldMatrix(oldTransform.ToMatrixWithScale(in DVector3.Zero));
            DebugMesh.PerMeshCBuffer?.FlushDirty();
        }
    }

    // ─── Asset Meta ─────────────────────────────────────────────

    [Rtti.Meta("")]
    public partial class TtPhysicsAssetAMeta : IO.IAssetMeta
    {
        public override string TypeExt => TtPhysicsAsset.AssetExt;

        public override async Thread.Async.TtTask<IO.IAsset> GetAsset(params object[] args)
        {
            return await TtPhysicsAsset.LoadAsset(GetAssetName());
        }

        public override async Thread.Async.TtTask<IO.IAsset> CreateAsset(params object[] args)
        {
            return await TtPhysicsAsset.LoadAsset(GetAssetName());
        }

        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return true;
        }

        public override string GetAssetTypeName()
        {
            return "PhysicsAsset";
        }
    }

    // ─── Asset ──────────────────────────────────────────────────

    [IO.CommonCreateAttribute]
    [IO.AssetCreateMenu(MenuName = "Physics/PhysicsAsset")]
    [Rtti.Meta("")]
    public partial class TtPhysicsAsset : IO.BaseSerializer, IO.IAsset
    {
        public const string AssetExt = ".phyasset";
        public string TypeExt => AssetExt;

        [Rtti.Meta("")]
        public RName AssetName { get; set; }

        [Rtti.Meta("")]
        public List<TtBoneBody> Bodies { get; set; } = new List<TtBoneBody>();

        [Rtti.Meta("")]
        public List<TtBoneConstraint> Constraints { get; set; } = new List<TtBoneConstraint>();

        /// <summary>
        /// 根据 Bodies 和骨骼层级自动重建 Constraints。
        /// 每个 BoneBody 自动生成到最近祖先 BoneBody 的 Constraint（UE 风格）。
        /// 已存在的 Constraint 保留其用户编辑过的参数，仅增删差异部分。
        /// </summary>
        public void RebuildConstraints(Animation.SkeletonAnimation.Skeleton.TtSkinSkeleton skeleton)
        {
            if (skeleton == null)
                return;

            // 构建 BoneName → BoneBody 的查找表
            var bodyLookup = new System.Collections.Generic.HashSet<string>();
            foreach (var body in Bodies)
            {
                if (!string.IsNullOrEmpty(body.BoneName))
                    bodyLookup.Add(body.BoneName);
            }

            // 构建 BoneName → ILimb 的查找表
            var limbLookup = new System.Collections.Generic.Dictionary<string, Animation.SkeletonAnimation.Skeleton.Limb.ILimb>();
            foreach (var limb in skeleton.Limbs)
            {
                if (limb.Desc?.Name != null)
                    limbLookup[limb.Desc.Name] = limb;
            }

            // 构建需要的 Constraint 配对集合：(parentBodyBone, childBodyBone)
            var neededPairs = new System.Collections.Generic.HashSet<(string, string)>();
            foreach (var body in Bodies)
            {
                if (string.IsNullOrEmpty(body.BoneName))
                    continue;
                if (!limbLookup.TryGetValue(body.BoneName, out var limb))
                    continue;

                // 沿骨骼层级往上搜索，找到第一个拥有 BoneBody 的祖先
                var ancestorBone = FindNearestAncestorBody(limb, skeleton, bodyLookup, limbLookup);
                if (ancestorBone != null)
                {
                    neededPairs.Add((ancestorBone, body.BoneName));
                }
            }

            // 保留已有的、仍然需要的 Constraint（保留用户编辑过的参数）
            var existingLookup = new System.Collections.Generic.Dictionary<(string, string), TtBoneConstraint>();
            foreach (var constraint in Constraints)
            {
                var key = (constraint.BoneNameA ?? "", constraint.BoneNameB ?? "");
                existingLookup[key] = constraint;
            }

            var newConstraints = new System.Collections.Generic.List<TtBoneConstraint>();
            foreach (var pair in neededPairs)
            {
                if (existingLookup.TryGetValue(pair, out var existing))
                {
                    newConstraints.Add(existing);
                }
                else
                {
                    newConstraints.Add(new TtBoneConstraint
                    {
                        BoneNameA = pair.Item1,
                        BoneNameB = pair.Item2,
                    });
                }
            }

            Constraints = newConstraints;

            // 反向关联：把 Constraint 挂到对应的子 BoneBody 上
            var bodyByName = new System.Collections.Generic.Dictionary<string, TtBoneBody>();
            foreach (var body in Bodies)
            {
                if (!string.IsNullOrEmpty(body.BoneName))
                    bodyByName[body.BoneName] = body;
                body.Constraint = null;
            }
            foreach (var constraint in Constraints)
            {
                if (!string.IsNullOrEmpty(constraint.BoneNameB)
                    && bodyByName.TryGetValue(constraint.BoneNameB, out var childBody))
                {
                    childBody.Constraint = constraint;
                }
            }
        }

        /// <summary>
        /// 从一个 limb 往上搜索骨骼层级，找到第一个拥有 BoneBody 的祖先骨骼名
        /// </summary>
        static string FindNearestAncestorBody(
            Animation.SkeletonAnimation.Skeleton.Limb.ILimb limb,
            Animation.SkeletonAnimation.Skeleton.TtSkinSkeleton skeleton,
            System.Collections.Generic.HashSet<string> bodyLookup,
            System.Collections.Generic.Dictionary<string, Animation.SkeletonAnimation.Skeleton.Limb.ILimb> limbLookup)
        {
            var parentIdx = limb.ParentIndex;
            while (parentIdx.IsValid() && parentIdx.Value >= 0 && parentIdx.Value < skeleton.Limbs.Count)
            {
                var parentLimb = skeleton.Limbs[parentIdx.Value];
                var parentName = parentLimb.Desc?.Name;
                if (parentName != null && bodyLookup.Contains(parentName))
                    return parentName;
                parentIdx = parentLimb.ParentIndex;
            }
            return null;
        }

        public IAssetMeta CreateAMeta()
        {
            return new TtPhysicsAssetAMeta();
        }

        public IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void SaveAssetTo(RName name)
        {
            AssetName = name;
            var typeStr = Rtti.TtTypeDesc.TypeOf(this.GetType()).TypeString;
            var xnd = new IO.TtXndHolder(typeStr, 0, 0);
            using (var attr = xnd.NewAttribute("PhysicsAsset", 0, 0))
            {
                using (var ar = attr.GetWriter(512))
                {
                    ar.Write(this);
                }
                xnd.RootNode.AddAttribute(attr);
            }

            xnd.SaveXnd(name.Address);
            name.AMeta.AddAssetFile(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address);
        }

        public void UpdateAMetaReferences(IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }

        public static async Thread.Async.TtTask<TtPhysicsAsset> LoadAsset(RName name)
        {
            return await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
                {
                    if (xnd == null)
                        return null;
                    return LoadXnd(xnd.RootNode);
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);
        }

        public static TtPhysicsAsset LoadXnd(IO.TtXndNode node)
        {
            unsafe
            {
                var attr = node.TryGetAttribute("PhysicsAsset");
                if ((IntPtr)attr.CppPointer == IntPtr.Zero)
                    return null;

                using (var ar = attr.GetReader(null))
                {
                    IO.ISerializer result = null;
                    try
                    {
                        ar.Read(out result, null);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                    return result as TtPhysicsAsset;
                }
            }
        }
    }
}
