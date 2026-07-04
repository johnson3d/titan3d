using EngineNS.Graphics.Mesh.PhysicsAsset;
using System;

namespace EngineNS.Bricks.Animation.KawaiiPhysics
{
    /// <summary>
    /// Forward axis direction of the tip virtual bone.
    /// </summary>
    public enum EKawaiiTailBoneAxis : byte
    {
        X_Positive = 0,
        X_Negative,
        Y_Positive,
        Y_Negative,
        Z_Positive,
        Z_Negative,
    }

    /// <summary>
    /// Chain simulation setup data. Configures a single bone chain for physics simulation.
    /// </summary>
    public class TtKawaiiChainSetup
    {
        [Rtti.Meta]
        public string Name { get; set; } = "Chain";

        [Rtti.Meta]
        public int RootBoneIndex { get; set; } = -1;

        [Rtti.Meta]
        public int EndBoneIndex { get; set; } = -1;

        [Rtti.Meta]
        public float TailBoneLength { get; set; } = 0.0f;

        [Rtti.Meta]
        public EKawaiiTailBoneAxis TailBoneAxis { get; set; } = EKawaiiTailBoneAxis.X_Positive;

        [Rtti.Meta]
        public bool ConstrainBoneLength { get; set; } = true;

        [Rtti.Meta]
        public float BoneLengthConstraintBlend { get; set; } = 1.0f;

        [Rtti.Meta]
        public bool RootCollision { get; set; } = false;

        /// <summary>
        /// LOD threshold. -1 means always active.
        /// </summary>
        [Rtti.Meta]
        public int LODThreshold { get; set; } = -1;
        public TtKawaiiChainSetup()
        {

        }
        public TtKawaiiChainSetup(
            string name = "Chain",
            int rootBoneIndex = -1,
            int endBoneIndex = -1,
            float tailBoneLength = 0.0f,
            EKawaiiTailBoneAxis tailBoneAxis = EKawaiiTailBoneAxis.X_Positive,
            bool constrainBoneLength = true,
            float boneLengthConstraintBlend = 1.0f,
            bool rootCollision = false,
            int lodThreshold = -1)
        {
            Name = name;
            RootBoneIndex = rootBoneIndex;
            EndBoneIndex = endBoneIndex;
            TailBoneLength = tailBoneLength;
            TailBoneAxis = tailBoneAxis;
            ConstrainBoneLength = constrainBoneLength;
            BoneLengthConstraintBlend = boneLengthConstraintBlend;
            RootCollision = rootCollision;
            LODThreshold = lodThreshold;
        }

    }

    /// <summary>
    /// Cloth simulation setup data. Extends chain setup with cloth-specific settings.
    /// </summary>
    public class TtKawaiiClothSetup : TtKawaiiChainSetup
    {
        [Rtti.Meta]
        public bool LoopChains { get; set; } = false;
        public TtKawaiiClothSetup()
        {

        }
        public TtKawaiiClothSetup(
            string name = "Cloth",
            int rootBoneIndex = -1,
            int endBoneIndex = -1,
            float tailBoneLength = 0.0f,
            EKawaiiTailBoneAxis tailBoneAxis = EKawaiiTailBoneAxis.X_Positive,
            bool constrainBoneLength = true,
            float boneLengthConstraintBlend = 1.0f,
            bool rootCollision = false,
            int lodThreshold = -1,
            bool loopChains = false)
            : base(name, rootBoneIndex, endBoneIndex, tailBoneLength, tailBoneAxis, constrainBoneLength, boneLengthConstraintBlend, rootCollision, lodThreshold)
        {
            LoopChains = loopChains;
        }
    }

    /// <summary>
    /// Cosserat rod simulation setup data.
    /// </summary>
    public class TtKawaiiRodSetup
    {
        [Rtti.Meta]
        public string Name { get; set; } = "Rod";

        [Rtti.Meta]
        public int RootBoneIndex { get; set; } = -1;

        [Rtti.Meta]
        public int EndBoneIndex { get; set; } = -1;

        [Rtti.Meta]
        public float StretchShearStiffness { get; set; } = 1.0f;

        [Rtti.Meta]
        public float BendTwistStiffness { get; set; } = 0.05f;

        [Rtti.Meta]
        public float PointAttachStiffness { get; set; } = 0.10f;

        [Rtti.Meta]
        public float OrientAttachStiffness { get; set; } = 0.05f;

        [Rtti.Meta]
        public int LODThreshold { get; set; } = -1;
        public TtKawaiiRodSetup()
        {

        }
        public TtKawaiiRodSetup(
            string name = "Rod",
            int rootBoneIndex = -1,
            int endBoneIndex = -1,
            float stretchShearStiffness = 1.0f,
            float bendTwistStiffness = 0.05f,
            float pointAttachStiffness = 0.10f,
            float orientAttachStiffness = 0.05f,
            int lodThreshold = -1)
        {
            Name = name;
            RootBoneIndex = rootBoneIndex;
            EndBoneIndex = endBoneIndex;
            StretchShearStiffness = stretchShearStiffness;
            BendTwistStiffness = bendTwistStiffness;
            PointAttachStiffness = pointAttachStiffness;
            OrientAttachStiffness = orientAttachStiffness;
            LODThreshold = lodThreshold;
        }
    }

    /// <summary>
    /// Collider definition for bone-driven colliders.
    /// Updated each frame from bone transforms.
    /// Shape geometry is defined by the TtCollisionShape派生类 (TtSphereShape, TtCapsuleShape, etc.)
    /// </summary>
    public class TtKawaiiColliderDef
    {
        /// <summary>
        /// Collision shape (TtSphereShape / TtCapsuleShape / TtBoxShape / TtPlaneShape).
        /// Offset and Rotation in the shape are relative to the attached bone.
        /// </summary>
        [Rtti.Meta]
        public TtCollisionShape Shape { get; set; } = new TtSphereShape() { Radius = 5.0f };

        /// <summary>
        /// Bone index that drives this collider's transform. -1 for world-space static colliders.
        /// </summary>
        [Rtti.Meta]
        public int AttachBoneIndex { get; set; } = -1;
    }
}
