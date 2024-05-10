using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.SkeletonAnimation.Runtime.Control
{
    public class TtCCDIK : TtRuntimePoseControl
    {
        Pose.TtLocalSpaceRuntimePose mInPose = null;
        public Pose.TtLocalSpaceRuntimePose InPose
        {
            get => mInPose;
            set
            {
                mInPose = value;
                mLocalSpacePose = TtRuntimePoseUtility.CopyPose(InPose);
                mMeshSpacePose = TtRuntimePoseUtility.ConvetToMeshSpaceRuntimePose(mLocalSpacePose);
            }
        }
        Pose.TtLocalSpaceRuntimePose mLocalSpacePose = null;
        Pose.TtMeshSpaceRuntimePose mMeshSpacePose = null;
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
                if (mMeshSpacePose == null)
                    return;
                BuildBoneChain(TtRuntimePoseUtility.GetDesc(mEndEffecterBoneName, mMeshSpacePose), TtRuntimePoseUtility.GetDesc(value, mMeshSpacePose));
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
        void BuildBoneChain(ILimbDesc bone, ILimbDesc rootBone)
        {
            if (bone == null)
                return;
            if (rootBone == null)
                return;
            var parentBone = rootBone;
            if (parentBone == null)
                return;
            mBoneChain.Add(parentBone.Name);
            if (parentBone.NameHash == rootBone.NameHash)
            {
                return;
            }
            BuildBoneChain(parentBone, rootBone);
        }
        //void Solve(ILimbDesc localJoint, ILimbDesc meshSpaceJoint, Vector3 meshSpaceTarget, ILimbDesc meshSpaceEnd, float limitAngle, int iterNum)
        //{

        //    var j2e = meshSpaceEnd.Transform.Position - meshSpaceJoint.Transform.Position;
        //    var j2t = meshSpaceTarget - meshSpaceJoint.Transform.Position;
        //    Quaternion invRot = meshSpaceJoint.Transform.Rotation.Inverse();
        //    var localJ2E = invRot * j2e;
        //    var localJ2T = invRot * j2t;
        //    localJ2E.Normalize();
        //    localJ2T.Normalize();

        //    float deltaAngle = MathHelper.Acos(Vector3.Dot(localJ2T, localJ2E));
        //    if (float.IsNaN(deltaAngle) || deltaAngle < 0.0001f)
        //        return;

        //    deltaAngle = MathHelper.FClamp(deltaAngle, -limitAngle, limitAngle);

        //    var axis = Vector3.Cross(localJ2E, localJ2T);
        //    axis.Normalize();
        //    Quaternion detaRotation;
        //    detaRotation = Quaternion.RotationAxis(axis, deltaAngle);
        //    Vector3 rot;
        //    detaRotation.GetYawPitchRoll(out rot.Y, out rot.X, out rot.Z);
        //    //var locRot = joint->Transform.Rotation * detaRotation;
        //    var locRot = detaRotation * localJoint.Transform.Rotation;
        //    //joint->Transform.Position = mTargetPosition;
        //    var constraint = localJoint.ReferenceBone.BoneDesc.Constraint;
        //    if ((constraint.ConstraintType & ConstraintType.Rotation) == ConstraintType.Rotation)
        //    {
        //        locRot.GetYawPitchRoll(out rot.Y, out rot.X, out rot.Z);
        //        rot.X = MathHelper.FClamp(rot.X, constraint.MinRotation.X, constraint.MaxRotation.X);
        //        rot.Y = MathHelper.FClamp(rot.Y, constraint.MinRotation.Y, constraint.MaxRotation.Y);
        //        rot.Z = MathHelper.FClamp(rot.Z, constraint.MinRotation.Z, constraint.MaxRotation.Z);
        //        locRot = Quaternion.RotationYawPitchRoll(rot.Y, rot.X, rot.Z);
        //    }
        //    var localTrans = localJoint.Transform;
        //    localTrans.Rotation = Quaternion.Slerp(localJoint.Transform.Rotation, locRot, Alpha);
        //    localJoint.Transform = localTrans;
        //}
        //void CalculatePose(int currentBoneIndex)
        //{
        //    Runtime.CGfxAnimationRuntime.CopyPoseAndConvertMeshSpace(MeshSpacePose, LocalSpacePose);
        //    ////第一根骨
        //    //var bone = mBoneChain[currentBoneIndex];
        //    //var parB = OutPose.FindBonePose(bone.ReferenceBone.BoneDesc.ParentHash);
        //    //if (parB != null)
        //    //    GfxBoneTransform::Transform(&bone->AbsRenderTransform, &bone->Transform, &parB->AbsRenderTransform);
        //    //else
        //    //    bone->AbsRenderTransform = bone->Transform;
        //    ////中间骨
        //    //for (int i = currentBoneIndex; i > 0; --i)
        //    //{
        //    //    var parentBone = mBoneChain[i];
        //    //    var bone = mBoneChain[i - 1];
        //    //    GfxBoneTransform::Transform(&bone->AbsRenderTransform, &bone->Transform, &parentBone->AbsRenderTransform);
        //    //}
        //    ////endEffecter骨
        //    //GfxBoneTransform::Transform(&mEndEffecterBone->AbsRenderTransform, &mEndEffecterBone->Transform, &mBoneChain[0]->AbsRenderTransform);
        //}

        //public override void Update()
        //{
        //    if (!Enable)
        //        return;
        //    URuntimePoseUtility.CopyPose(ref mLocalSpacePose, mInPose);
        //    URuntimePoseUtility.ConvetToMeshSpaceRuntimePose(ref mMeshSpacePose, mLocalSpacePose);
        //    if (!string.IsNullOrEmpty(mTargetBoneName))
        //        mTargetPosition = URuntimePoseUtility.GetTransform(mTargetBoneName, mMeshSpacePose).Position.ToSingleVector3();
        //    for (uint i = 0; i < Iteration; ++i)
        //    {
        //        for (int j = 0; j < mBoneChain.Count; ++j)
        //        {
        //            var jointName = mBoneChain[j];
        //            Solve(LocalSpacePose.FindBonePose(jointName), MeshSpacePose.FindBonePose(jointName), mTargetPosition, MeshSpacePose.FindBonePose(mEndEffecterBoneName), LimitAngle, (int)i);
        //            CalculatePose(j);
        //        }
        //        if ((mTargetPosition - MeshSpacePose.FindBonePose(mEndEffecterBoneName).Transform.Position).LengthSquared() < 0.00001f)
        //            break;
        //    }
        //    Runtime.CGfxAnimationRuntime.CopyPose(OutPose, LocalSpacePose);
        //}
    }
}
