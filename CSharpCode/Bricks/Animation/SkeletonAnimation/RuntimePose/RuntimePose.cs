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
        public List<ILimbDesc> Descs { get; set; }
        public List<FTransform> Transforms { get; set; }
    }
    public class ULocalSpaceRuntimePose : IRuntimePose  //or struct
    {
        public List<ILimbDesc> Descs { get; set; } = new List<ILimbDesc>();    //or array
        public List<FTransform> Transforms { get; set; } = new List<FTransform>();//or array

    }
    public class UMeshSpaceRuntimePose : IRuntimePose
    {
        public List<ILimbDesc> Descs { get; set; } = new List<ILimbDesc>();//or array
        public List<FTransform> Transforms { get; set; } = new List<FTransform>();//or array

    }

    public class URuntimePoseUtility
    {
        public static IndexInSkeleton GetIndex(uint NameHash, IRuntimePose pose)
        {
            for (int i = 0; i < pose.Descs.Count; ++i)
            {
                if (NameHash == pose.Descs[i].NameHash)
                {
                    return new IndexInSkeleton(i);
                }
            }
            return new IndexInSkeleton(- 1);
        }
        public static List<IndexInSkeleton> GetChildren(uint NameHash, IRuntimePose pose)
        {
            List<IndexInSkeleton> childs = new List<IndexInSkeleton>();
            for (int i = 0; i < pose.Descs.Count; ++i)
            {
                if (NameHash == pose.Descs[i].ParentHash)
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
            UMeshSpaceRuntimePose pose = new UMeshSpaceRuntimePose();
            for (int i = 0; i < skeletonPose.LimbPoses.Count; ++i)
            {
                pose.Transforms.Add(skeletonPose.LimbPoses[i].Transtorm);
                pose.Descs.Add(skeletonPose.LimbPoses[i].Desc);
            }
            var rootHash = GetRootDesc(pose).NameHash;
            ConvertToMeshSpaceTransformRecursively(rootHash, ref pose);
            return pose;
        }
        public static UMeshSpaceRuntimePose ConvetToMeshSpaceRuntimePose(ULocalSpaceRuntimePose localSpacePose)
        {
            UMeshSpaceRuntimePose temp = new UMeshSpaceRuntimePose();
            temp.Transforms.AddRange(localSpacePose.Transforms);
            temp.Descs.AddRange(localSpacePose.Descs);

            var rootHash = GetRootDesc(temp).NameHash;
            ConvertToMeshSpaceTransformRecursively(rootHash, ref temp);
            return temp;
        }
        public static void ConvetToMeshSpaceRuntimePose(ref UMeshSpaceRuntimePose meshSpacePose, ULocalSpaceRuntimePose localSpacePose)
        {
            meshSpacePose.Transforms.Clear();
            meshSpacePose.Descs.Clear();
            meshSpacePose.Transforms.AddRange(localSpacePose.Transforms);
            meshSpacePose.Descs.AddRange(localSpacePose.Descs);

            var rootHash = GetRootDesc(meshSpacePose).NameHash;
            ConvertToMeshSpaceTransformRecursively(rootHash, ref meshSpacePose);
        }
        static void ConvertToMeshSpaceTransformRecursively(uint parentHash, ref UMeshSpaceRuntimePose InOutPose)
        {
            var parentIndex = GetIndex(parentHash, InOutPose);
            var childrenIndexs = GetChildren(parentHash, InOutPose);
            for (int i = 0; i < childrenIndexs.Count; ++i)
            {
                var childIndex = childrenIndexs[i].Value;
                FTransform temp;
                FTransform.Multiply(out temp, InOutPose.Transforms[childIndex], InOutPose.Transforms[parentIndex.Value]);
                InOutPose.Transforms[childIndex] = temp;
                ConvertToMeshSpaceTransformRecursively(InOutPose.Descs[childIndex].NameHash, ref InOutPose);
            }
        }

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
        public static void CopyPose(ref ULocalSpaceRuntimePose descPose, AnimatablePose.UAnimatableSkeletonPose srcPose) 
        {
            for (int i = 0; i < srcPose.LimbPoses.Count; ++i)
            {
                descPose.Transforms[i] = srcPose.LimbPoses[i].Transtorm;
            }
        }

        public static void BlendPoses<T>(ref T outPose, T aPose, T bPose, float alpha) where T : IRuntimePose
        {
            System.Diagnostics.Debug.Assert(aPose.Transforms.Count == bPose.Transforms.Count);
            for(int i = 0; i< aPose.Transforms.Count; ++i)
            {
                var lerpedPos = DVector3.Lerp(aPose.Transforms[i].Position, bPose.Transforms[i].Position, alpha);
                var lerpedRot = Quaternion.Slerp(aPose.Transforms[i].Quat, bPose.Transforms[i].Quat, alpha);
                outPose.Transforms[i] = FTransform.CreateTransform(lerpedPos, Vector3.One, lerpedRot);
            }
        }
    }

   
}
