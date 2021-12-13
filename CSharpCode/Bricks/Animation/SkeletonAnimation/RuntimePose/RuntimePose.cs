using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.SkeletonAnimation.RuntimePose
{
    public struct LocalSpacePose
    {
        public List<FTransform> LocalSpacePoses;
        public List<int> ParentIndexs;
    }
    public struct MeshSpacePose
    {
       public List<FTransform> LimbPoses;
       public List<int> ParentIndexs;
    }

    public class URuntimePoseUtility
    {
        public static int GetIndexInSkeletonPose(AnimatablePose.IAnimatableLimbPose limb, AnimatablePose.UAnimatableSkeletonPose skeletonPose)
        {
            for(int i = 0; i< skeletonPose.LimbPoses.Count; ++i)
            {
                if(limb == skeletonPose.LimbPoses[i])
                {
                    return i;
                }
            }
            return -1;
        }
        public static MeshSpacePose ConvetToMeshSpacePose(AnimatablePose.UAnimatableSkeletonPose skeletonPose)
        {
            LocalSpacePose locap = new LocalSpacePose();
            locap.LocalSpacePoses = new List<FTransform>();
            MeshSpacePose temp = new MeshSpacePose();
            temp.LimbPoses = new List<FTransform>();
            for(int i = 0;i < skeletonPose.LimbPoses.Count; ++i)
            {
                locap.LocalSpacePoses.Add(skeletonPose.LimbPoses[i].Transtorm);
                temp.LimbPoses.Add(skeletonPose.LimbPoses[i].Transtorm);
            }
            var limb = skeletonPose.Root;
            ConvertToMeshSpaceTransformRecursively(locap,limb,skeletonPose,ref temp);
            return temp;
        }
        public static void ConvertToMeshSpaceTransformRecursively(LocalSpacePose localSpacePose, AnimatablePose.IAnimatableLimbPose parent, AnimatablePose.UAnimatableSkeletonPose skeletonPose, ref MeshSpacePose meshSpacePose)
        {
           
            for (int i = 0; i < parent.Children.Count; ++i)
            {
                var parentIndex = GetIndexInSkeletonPose(parent, skeletonPose);
                var childIndex = GetIndexInSkeletonPose(parent.Children[i], skeletonPose);
                FTransform temp;
                FTransform.Multiply(out temp, meshSpacePose.LimbPoses[childIndex] , meshSpacePose.LimbPoses[parentIndex]);
                meshSpacePose.LimbPoses[childIndex] = temp;
                ConvertToMeshSpaceTransformRecursively(localSpacePose, parent.Children[i], skeletonPose,ref meshSpacePose);
            }
        }
    }

   
}
