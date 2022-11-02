using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Skeleton;
using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.Animation.SkeletonAnimation.Runtime.Pose
{
    public interface IRuntimePose
    {
        public int HashCode { get; }
        public List<ILimbDesc> Descs { get; set; }
        public List<FTransform> Transforms { get; set; }
    }
    public class ULocalSpaceRuntimePose : IRuntimePose  //or struct
    {
        public int HashCode => 0;
        public List<ILimbDesc> Descs { get; set; } = new List<ILimbDesc>();    //or array
        public List<FTransform> Transforms { get; set; } = new List<FTransform>();//or array
    }
    public class UMeshSpaceRuntimePose : IRuntimePose
    {
        public int HashCode => 0;
        public List<ILimbDesc> Descs { get; set; } = new List<ILimbDesc>();//or array
        public List<FTransform> Transforms { get; set; } = new List<FTransform>();//or array
    }

    /// <summary>
    /// LocalSpace Position MeshSpace Rotation
    /// </summary>
    public class ULPosMRotRuntimePose : IRuntimePose  //or struct
    {
        public int HashCode => 0;
        public List<ILimbDesc> Descs { get; set; } = new List<ILimbDesc>();    //or array
        public List<FTransform> Transforms { get; set; } = new List<FTransform>();//or array
    }


    public class URuntimePoseUtility
    {
        #region Get
        public static FTransform GetTransform(string limbName, IRuntimePose pose)
        {
            var index = GetIndex(limbName, pose);
            if (index.Value < 0)
            {
                return FTransform.Identity;
            }
            return pose.Transforms[index.Value];
        }
        public static FTransform GetTransform(uint limbNameHash, IRuntimePose pose)
        {
            var index = GetIndex(limbNameHash, pose);
            if (index.Value < 0)
            {
                return FTransform.Identity;
            }
            return pose.Transforms[index.Value];
        }
        public static ILimbDesc GetDesc(string limbName, IRuntimePose pose)
        {
            var index = GetIndex(limbName, pose);
            if (index.Value < 0)
            {
                return null;
            }
            return pose.Descs[index.Value];
        }
        public static ILimbDesc GetDesc(uint limbNameHash, IRuntimePose pose)
        {
            var index = GetIndex(limbNameHash, pose);
            if (index.Value < 0)
            {
                return null;
            }
            return pose.Descs[index.Value];
        }
        public static IndexInSkeleton GetIndex(string limbName, IRuntimePose pose)
        {
            for (int i = 0; i < pose.Descs.Count; ++i)
            {
                if (limbName == pose.Descs[i].Name)
                {
                    return new IndexInSkeleton(i);
                }
            }
            return new IndexInSkeleton(-1);
        }
        public static IndexInSkeleton GetIndex(uint limbNameHash, IRuntimePose pose)
        {
            for (int i = 0; i < pose.Descs.Count; ++i)
            {
                if (limbNameHash == pose.Descs[i].NameHash)
                {
                    return new IndexInSkeleton(i);
                }
            }
            return new IndexInSkeleton(-1);
        }
        public static List<IndexInSkeleton> GetChildren(uint limbNameHash, IRuntimePose pose)
        {
            List<IndexInSkeleton> childs = new List<IndexInSkeleton>();
            for (int i = 0; i < pose.Descs.Count; ++i)
            {
                if (limbNameHash == pose.Descs[i].ParentHash)
                {
                    childs.Add(new IndexInSkeleton(i));
                }
            }
            return childs;
        }
        public static IndexInSkeleton GetRoot(IRuntimePose pose)
        {
            for (int i = 0; i < pose.Descs.Count; ++i)
            {
                if (string.IsNullOrEmpty(pose.Descs[i].ParentName))
                {
                    return new IndexInSkeleton(i);
                }
            }
            return new IndexInSkeleton(-1);
        }
        public static ILimbDesc GetRootDesc(IRuntimePose pose)
        {
            for (int i = 0; i < pose.Descs.Count; ++i)
            {
                if (string.IsNullOrEmpty(pose.Descs[i].ParentName))
                {
                    return pose.Descs[i];
                }
            }
            return null;
        }
        #endregion get

        #region Create RuntimePose
        public static ULocalSpaceRuntimePose CreateLocalSpaceRuntimePose(AnimatablePose.UAnimatableSkeletonPose skeletonPose)
        {
            ULocalSpaceRuntimePose pose = new ULocalSpaceRuntimePose();
            for (int i = 0; i < skeletonPose.LimbPoses.Count; ++i)
            {
                pose.Transforms.Add(skeletonPose.LimbPoses[i].Transtorm);
                pose.Descs.Add(skeletonPose.LimbPoses[i].Desc);
            }
            return pose;
        }
        public static UMeshSpaceRuntimePose CreateMeshSpaceRuntimePose(AnimatablePose.UAnimatableSkeletonPose skeletonPose)
        {
            ULocalSpaceRuntimePose localSpacePose = CreateLocalSpaceRuntimePose(skeletonPose);
            return ConvetToMeshSpaceRuntimePose(localSpacePose);
        }
        #endregion Create RuntimePose

        #region ConvertRuntimePose
        public static UMeshSpaceRuntimePose ConvetToMeshSpaceRuntimePose(ULocalSpaceRuntimePose localSpacePose)
        {
            UMeshSpaceRuntimePose temp = new UMeshSpaceRuntimePose();
            ConvetToMeshSpaceRuntimePose(ref temp, localSpacePose);
            return temp;
        }
        public static void ConvetToMeshSpaceRuntimePose(ref UMeshSpaceRuntimePose desMeshSpacePose, ULocalSpaceRuntimePose srcLocalSpacePose)
        {
            desMeshSpacePose.Transforms.Clear();
            desMeshSpacePose.Descs.Clear();
            desMeshSpacePose.Transforms.AddRange(srcLocalSpacePose.Transforms);
            desMeshSpacePose.Descs.AddRange(srcLocalSpacePose.Descs);

            var rootHash = GetRootDesc(desMeshSpacePose).NameHash;
            ConvertToMeshSpaceTransformRecursively(ref desMeshSpacePose, rootHash, srcLocalSpacePose);
        }
        static void ConvertToMeshSpaceTransformRecursively(ref UMeshSpaceRuntimePose outPose, uint parentHash, ULocalSpaceRuntimePose srcPose)
        {
            var parentIndex = GetIndex(parentHash, srcPose);
            var childrenIndexs = GetChildren(parentHash, srcPose);
            for (int i = 0; i < childrenIndexs.Count; ++i)
            {
                var childIndex = childrenIndexs[i].Value;
                FTransform temp;
                FTransform.Multiply(out temp, srcPose.Transforms[childIndex], outPose.Transforms[parentIndex.Value]);
                outPose.Transforms[childIndex] = temp;
                ConvertToMeshSpaceTransformRecursively(ref outPose, srcPose.Descs[childIndex].NameHash, srcPose);
            }
        }
        public static ULocalSpaceRuntimePose ConvetToLocalSpaceRuntimePose(UMeshSpaceRuntimePose meshSpacePose)
        {
            ULocalSpaceRuntimePose temp = new ULocalSpaceRuntimePose();
            ConvetToLocalSpaceRuntimePose(ref temp, meshSpacePose);
            return temp;
        }
        public static void ConvetToLocalSpaceRuntimePose(ref ULocalSpaceRuntimePose desLocalSpacePose, UMeshSpaceRuntimePose srcMeshSpacePose)
        {
            desLocalSpacePose.Transforms.Clear();
            desLocalSpacePose.Descs.Clear();
            desLocalSpacePose.Transforms.AddRange(srcMeshSpacePose.Transforms);
            desLocalSpacePose.Descs.AddRange(srcMeshSpacePose.Descs);

            var rootHash = GetRootDesc(desLocalSpacePose).NameHash;
            ConvertToLocalSpaceTransformRecursively(ref desLocalSpacePose, rootHash, srcMeshSpacePose);
        }
        static void ConvertToLocalSpaceTransformRecursively(ref ULocalSpaceRuntimePose outPose, uint parentHash, UMeshSpaceRuntimePose srcPose)
        {
            var parentIndex = GetIndex(parentHash, srcPose);
            var childrenIndexs = GetChildren(parentHash, srcPose);
            for (int i = 0; i < childrenIndexs.Count; ++i)
            {
                var childIndex = childrenIndexs[i].Value;
                ConvertToLocalSpaceTransformRecursively(ref outPose, srcPose.Descs[childIndex].NameHash, srcPose);
                FTransform temp;
                FTransform.Multiply(out temp, srcPose.Transforms[childIndex], srcPose.Transforms[parentIndex.Value].Inverse());
                outPose.Transforms[childIndex] = temp;
            }
        }

        public static void ConvetToLocalSpaceRuntimePose(ref ULocalSpaceRuntimePose desLocalSpacePose, UAnimatableSkeletonPose skeletonPose)
        {
            desLocalSpacePose.Transforms.Clear();
            desLocalSpacePose.Descs.Clear();
            for (int i = 0; i < skeletonPose.LimbPoses.Count; ++i)
            {
                desLocalSpacePose.Transforms.Add(skeletonPose.LimbPoses[i].Transtorm);
                desLocalSpacePose.Descs.Add(skeletonPose.LimbPoses[i].Desc);
            }
        }

        public static ULPosMRotRuntimePose ConvetToLPosMRotRuntimePose(ULocalSpaceRuntimePose localSpacePose)
        {
            ULPosMRotRuntimePose temp = new ULPosMRotRuntimePose();
            ConvetToLPosMRotRuntimePose(ref temp, localSpacePose);
            return temp;
        }
        public static void ConvetToLPosMRotRuntimePose(ref ULPosMRotRuntimePose desLPosMRotSpacePose, ULocalSpaceRuntimePose srcLocalSpacePose)
        {
            desLPosMRotSpacePose.Transforms.Clear();
            desLPosMRotSpacePose.Descs.Clear();
            desLPosMRotSpacePose.Transforms.AddRange(srcLocalSpacePose.Transforms);
            desLPosMRotSpacePose.Descs.AddRange(srcLocalSpacePose.Descs);

            var rootHash = GetRootDesc(desLPosMRotSpacePose).NameHash;
            ConvetToLPosMRotTransformRecursively(ref desLPosMRotSpacePose, rootHash, srcLocalSpacePose);
        }
        static void ConvetToLPosMRotTransformRecursively(ref ULPosMRotRuntimePose outPose, uint parentHash, ULocalSpaceRuntimePose srcPose)
        {
            var parentIndex = GetIndex(parentHash, srcPose);
            var childrenIndexs = GetChildren(parentHash, srcPose);
            for (int i = 0; i < childrenIndexs.Count; ++i)
            {
                var childIndex = childrenIndexs[i].Value;
                FTransform temp;
                temp = srcPose.Transforms[childIndex];
                temp.Quat = temp.Quat * srcPose.Transforms[parentIndex.Value].Quat;
                outPose.Transforms[childIndex] = temp;
                ConvetToLPosMRotTransformRecursively(ref outPose, srcPose.Descs[childIndex].NameHash, srcPose);
            }
        }

        public static ULocalSpaceRuntimePose ConvetToLocalSpaceRuntimePose(ULPosMRotRuntimePose lPosMRotRuntimePose)
        {
            ULocalSpaceRuntimePose temp = new ULocalSpaceRuntimePose();
            ConvetToLocalSpaceRuntimePose(ref temp, lPosMRotRuntimePose);
            return temp;
        }
        public static void ConvetToLocalSpaceRuntimePose(ref ULocalSpaceRuntimePose desLocalSpacePose, ULPosMRotRuntimePose srcLPosMRotRuntimePose)
        {
            desLocalSpacePose.Transforms.Clear();
            desLocalSpacePose.Descs.Clear();
            desLocalSpacePose.Transforms.AddRange(srcLPosMRotRuntimePose.Transforms);
            desLocalSpacePose.Descs.AddRange(srcLPosMRotRuntimePose.Descs);

            var rootHash = GetRootDesc(desLocalSpacePose).NameHash;
            ConvertToLocalSpaceTransformRecursively(ref desLocalSpacePose, rootHash, srcLPosMRotRuntimePose);
        }
        static void ConvertToLocalSpaceTransformRecursively(ref ULocalSpaceRuntimePose outPose, uint parentHash, ULPosMRotRuntimePose srcPose)
        {
            var parentIndex = GetIndex(parentHash, srcPose);
            var childrenIndexs = GetChildren(parentHash, srcPose);
            for (int i = 0; i < childrenIndexs.Count; ++i)
            {
                var childIndex = childrenIndexs[i].Value;
                ConvertToLocalSpaceTransformRecursively(ref outPose, srcPose.Descs[childIndex].NameHash, srcPose);
                FTransform temp;
                temp = srcPose.Transforms[childIndex];
                temp.Quat = temp.Quat * srcPose.Transforms[parentIndex.Value].Quat.Inverse();
                outPose.Transforms[childIndex] = temp;
            }
        }
        #endregion ConvertRuntimePose

        public static T CopyPose<T>(T pose) where T : IRuntimePose
        {
            T temp = System.Activator.CreateInstance<T>();
            temp.Transforms.AddRange(pose.Transforms);
            temp.Descs.AddRange(pose.Descs);
            return temp;
        }
        public static void CopyPose<T>(ref T descPose, T srcPose) where T : IRuntimePose
        {
            descPose.Transforms.Clear();
            descPose.Descs.Clear();
            descPose.Transforms.AddRange(srcPose.Transforms);
            descPose.Descs.AddRange(srcPose.Descs);
        }
        public static void CopyTransforms<T>(ref T descPose, T srcPose) where T : IRuntimePose
        {
            descPose.Transforms.Clear();
            descPose.Transforms.AddRange(srcPose.Transforms);
        }
        public static void CopyTransforms(ref ULocalSpaceRuntimePose descPose, AnimatablePose.UAnimatableSkeletonPose srcPose)
        {
            for (int i = 0; i < srcPose.LimbPoses.Count; ++i)
            {
                descPose.Transforms[i] = srcPose.LimbPoses[i].Transtorm;
            }
        }

        public static void BlendPoses<T>(ref T outPose, T aPose, T bPose, float alpha) where T : IRuntimePose
        {
            System.Diagnostics.Debug.Assert(aPose.Transforms.Count == bPose.Transforms.Count);
            for (int i = 0; i < aPose.Transforms.Count; ++i)
            {
                var lerpedPos = DVector3.Lerp(aPose.Transforms[i].Position, bPose.Transforms[i].Position, alpha);
                var lerpedRot = Quaternion.Slerp(aPose.Transforms[i].Quat, bPose.Transforms[i].Quat, alpha);
                outPose.Transforms[i] = FTransform.CreateTransform(lerpedPos, Vector3.One, lerpedRot);
            }
        }
        public static void BlendPoses<T>(ref T outPose, List<T> poses, List<float> weights) where T : IRuntimePose
        {
            System.Diagnostics.Debug.Assert(poses.Count > 0);
            int boneCount = poses[0].Transforms.Count;
            DVector3 pos = DVector3.Zero;
            Quaternion rot = Quaternion.Identity;
            for (int boneIndex = 0; boneIndex < boneCount; ++boneIndex)
            {
                for (int poseIndex = 0; poseIndex < poses.Count; ++poseIndex)
                {
                    pos += poses[poseIndex].Transforms[boneIndex].Position * weights[poseIndex];
                    rot *= poses[poseIndex].Transforms[boneIndex].Quat * weights[poseIndex];
                }
                outPose.Transforms[boneIndex] = FTransform.CreateTransform(pos, Vector3.One, rot);
            }
        }
        public static void ZeroPose<T>(ref T descPose) where T : IRuntimePose
        {
            for (int i = 0; i < descPose.Transforms.Count; ++i)
            {
                descPose.Transforms[i] = FTransform.Identity;
            }
        }
        public static void ZeroPosePosition<T>(ref T descPose) where T : IRuntimePose
        {
            for (int i = 0; i < descPose.Transforms.Count; ++i)
            {
                var transform = descPose.Transforms[i];
                transform.Position = DVector3.Zero;
                descPose.Transforms[i] = transform;
            }
        }
        public static void AddPoses(ref ULocalSpaceRuntimePose outPose, ULocalSpaceRuntimePose lPose, ULocalSpaceRuntimePose rPose)
        {
            System.Diagnostics.Debug.Assert(lPose.Transforms.Count == rPose.Transforms.Count);
            System.Diagnostics.Debug.Assert(lPose.Transforms.Count == outPose.Transforms.Count);
            for (int i = 0; i < outPose.Transforms.Count; ++i)
            {
                FTransform lTransform = lPose.Transforms[i];
                FTransform rTransform = rPose.Transforms[i];
                FTransform outTransform = FTransform.Identity;
                FTransform.Multiply(out outTransform, lTransform, rTransform);
                outPose.Transforms[i] = outTransform;
            }
        }
        public static void AddPoses(ref ULPosMRotRuntimePose outPose, ULPosMRotRuntimePose basePose, ULPosMRotRuntimePose additivePose, float alpha)
        {
            System.Diagnostics.Debug.Assert(basePose.Transforms.Count == additivePose.Transforms.Count);
            System.Diagnostics.Debug.Assert(basePose.Transforms.Count == outPose.Transforms.Count);
            for (int i = 0; i < outPose.Transforms.Count; ++i)
            {
                FTransform baseBone = basePose.Transforms[i];
                FTransform additiveBone = additivePose.Transforms[i];
                var quat = additiveBone.Quat * baseBone.Quat;
                var pos = baseBone.Position + additiveBone.Position;
                var lerpedPos = DVector3.Lerp(baseBone.Position, pos, alpha);
                var lerpedRot = Quaternion.Slerp(baseBone.Quat, quat, alpha);
                lerpedRot.Normalize();
                outPose.Transforms[i] = FTransform.CreateTransform(lerpedPos, Vector3.One, lerpedRot);
            }
        }
        public static void MinusPoses(ref ULPosMRotRuntimePose outPose, ULPosMRotRuntimePose minusPose, ULPosMRotRuntimePose minuendPose)
        {
            System.Diagnostics.Debug.Assert(minusPose.Transforms.Count == minuendPose.Transforms.Count);
            System.Diagnostics.Debug.Assert(minuendPose.Transforms.Count == outPose.Transforms.Count);
            for (int i = 0; i < outPose.Transforms.Count; ++i)
            {
                FTransform minusBone = minusPose.Transforms[i];
                FTransform minuendBone = minuendPose.Transforms[i];
                var position = minuendBone.Position - minusBone.Position;
                var rotation = minuendBone.Quat * minusBone.Quat.Inverse();
                rotation.Normalize();

                outPose.Transforms[i] = FTransform.CreateTransform(position, Vector3.One, rotation);
            }
        }
    }


}
