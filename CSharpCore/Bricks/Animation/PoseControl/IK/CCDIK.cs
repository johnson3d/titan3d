using EngineNS.Bricks.Animation.Pose;
using EngineNS.Bricks.Animation.Skeleton;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.PoseControl.IK
{
    public class CCDIK : SkeletonPoseControl
    {
        Pose.CGfxSkeletonPose mInPose = null;
        public Pose.CGfxSkeletonPose InPose
        {
            get => mInPose;
            set
            {
                mInPose = value;
                LocalSpacePose = value.Clone();
                MeshSpacePose = value.Clone();
            }
        }
        Pose.CGfxSkeletonPose LocalSpacePose { get; set; } = null;
        Pose.CGfxSkeletonPose MeshSpacePose { get; set; } = null;
        public bool Enable { get; set; } = true;
        public float Alpha { get; set; } = 1;
        public uint Iteration { get; set; } = 15;
        public float LimitAngle { get; set; } = 180;

        string mEndEffecterBoneName;
        public string EndEffecterBoneName
        {
            get => mEndEffecterBoneName;
            set
            {
                mEndEffecterBoneName = value;
            }
        }
        string mRootBoneName;
        public string RootBoneName
        {
            get => mRootBoneName;
            set
            {
                mRootBoneName = value;
                if (MeshSpacePose == null)
                    return;
                BuildBoneChain(MeshSpacePose.FindBonePose(mEndEffecterBoneName), MeshSpacePose.FindBonePose(value));
            }
        }
        Vector3 mTargetPosition;
        public Vector3 TargetPosition
        {
            get => mTargetPosition;
            set
            {
                mTargetPosition = value;
            }
        }
        string mTargetBoneName;
        public string TargetBoneName
        {
            get => mTargetBoneName;
            set
            {
                mTargetBoneName = value;
            }
        }
        List<string> mBoneChain = new List<string>();
        void BuildBoneChain(CGfxBonePose bone, CGfxBonePose rootBone)
        {
            if (bone == null)
                return;
            if (rootBone == null)
                return;
            var parentBone = OutPose.FindBonePose(bone.ParentNameHash);
            if (parentBone == null)
                return;
            mBoneChain.Add(parentBone.Name);
            if (parentBone.NameHash == rootBone.NameHash)
            {
                return;
            }
            BuildBoneChain(parentBone, rootBone);
        }
        void Solve(CGfxBonePose localJoint, CGfxBonePose meshSpaceJoint, Vector3 meshSpaceTarget, CGfxBonePose meshSpaceEnd, float limitAngle, int iterNum)
        {
            var j2e = meshSpaceEnd.Transform.Position - meshSpaceJoint.Transform.Position;
            var j2t = meshSpaceTarget - meshSpaceJoint.Transform.Position;
            Quaternion invRot = meshSpaceJoint.Transform.Rotation.Inverse();
            var localJ2E = invRot * j2e;
            var localJ2T = invRot * j2t;
            localJ2E.Normalize();
            localJ2T.Normalize();

            float deltaAngle = MathHelper.Acos(Vector3.Dot(localJ2T, localJ2E));
            if (float.IsNaN(deltaAngle) || deltaAngle < 0.0001f)
                return;

            deltaAngle = MathHelper.FClamp(deltaAngle, -limitAngle, limitAngle);

            var axis = Vector3.Cross(localJ2E, localJ2T);
            axis.Normalize();
            Quaternion detaRotation;
            detaRotation = Quaternion.RotationAxis(axis, deltaAngle);
            Vector3 rot;
            detaRotation.GetYawPitchRoll(out rot.Y, out rot.X, out rot.Z);
            //var locRot = joint->Transform.Rotation * detaRotation;
            var locRot = detaRotation * localJoint.Transform.Rotation;
            //joint->Transform.Position = mTargetPosition;
            var constraint = localJoint.ReferenceBone.BoneDesc.Constraint;
            if ((constraint.ConstraintType & ConstraintType.Rotation) == ConstraintType.Rotation)
            {
                locRot.GetYawPitchRoll(out rot.Y, out rot.X, out rot.Z);
                rot.X = MathHelper.FClamp(rot.X, constraint.MinRotation.X, constraint.MaxRotation.X);
                rot.Y = MathHelper.FClamp(rot.Y, constraint.MinRotation.Y, constraint.MaxRotation.Y);
                rot.Z = MathHelper.FClamp(rot.Z, constraint.MinRotation.Z, constraint.MaxRotation.Z);
                locRot = Quaternion.RotationYawPitchRoll(rot.Y, rot.X, rot.Z);
            }
            var localTrans = localJoint.Transform;
            localTrans.Rotation = Quaternion.Slerp(localJoint.Transform.Rotation, locRot, Alpha);
            localJoint.Transform = localTrans;
        }
        void CalculatePose(int currentBoneIndex)
        {
            Runtime.CGfxAnimationRuntime.CopyPoseAndConvertMeshSpace(MeshSpacePose, LocalSpacePose);
            ////第一根骨
            //var bone = mBoneChain[currentBoneIndex];
            //var parB = OutPose.FindBonePose(bone.ReferenceBone.BoneDesc.ParentHash);
            //if (parB != null)
            //    GfxBoneTransform::Transform(&bone->AbsTransform, &bone->Transform, &parB->AbsTransform);
            //else
            //    bone->AbsTransform = bone->Transform;
            ////中间骨
            //for (int i = currentBoneIndex; i > 0; --i)
            //{
            //    var parentBone = mBoneChain[i];
            //    var bone = mBoneChain[i - 1];
            //    GfxBoneTransform::Transform(&bone->AbsTransform, &bone->Transform, &parentBone->AbsTransform);
            //}
            ////endEffecter骨
            //GfxBoneTransform::Transform(&mEndEffecterBone->AbsTransform, &mEndEffecterBone->Transform, &mBoneChain[0]->AbsTransform);
        }

        public override void Update()
        {
            if (!Enable)
                return;
            Runtime.CGfxAnimationRuntime.CopyPose(LocalSpacePose, mInPose);
            Runtime.CGfxAnimationRuntime.CopyPoseAndConvertMeshSpace(MeshSpacePose, LocalSpacePose);
            if (!string.IsNullOrEmpty(mTargetBoneName))
                mTargetPosition = MeshSpacePose.FindBonePose(mTargetBoneName).Transform.Position;
            for (uint i = 0; i < Iteration; ++i)
            {
                for (int j = 0; j < mBoneChain.Count; ++j)
                {
                    var jointName = mBoneChain[j];
                    Solve(LocalSpacePose.FindBonePose(jointName), MeshSpacePose.FindBonePose(jointName), mTargetPosition, MeshSpacePose.FindBonePose(mEndEffecterBoneName), LimitAngle, (int)i);
                    CalculatePose(j);
                }
                if ((mTargetPosition - MeshSpacePose.FindBonePose(mEndEffecterBoneName).Transform.Position).LengthSquared() < 0.00001f)
                    break;
            }
            Runtime.CGfxAnimationRuntime.CopyPose(OutPose, LocalSpacePose);
        }
    }
}
