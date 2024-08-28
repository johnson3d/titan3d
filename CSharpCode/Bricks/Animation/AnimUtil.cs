using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation
{
    public class TtAnimUtil
    {
        public static TtAnimatableSkeletonPose CreateAnimatableSkeletonPoseFromeNode(TtNode node)
        {
            if(node is TtMeshNode meshNode)
            {
                var animatablePose = meshNode?.Mesh?.MaterialMesh?.SubMeshes[0].Mesh?.PartialSkeleton?.CreatePose() as SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
                return animatablePose;
            }
            return null;
        }
        public static TtLocalSpaceRuntimePose BindRuntimeSkeletonPoseToNode(TtNode node)
        {
            if (node is TtMeshNode meshNode)
            {
                var animatablePose = meshNode?.Mesh?.MaterialMesh?.SubMeshes[0].Mesh?.PartialSkeleton?.CreatePose() as SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
                var skinMDfQueue = meshNode.Mesh.MdfQueue as Graphics.Mesh.UMdfSkinMesh;
                var animatedPose = SkeletonAnimation.Runtime.Pose.TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(animatablePose);
                skinMDfQueue.SkinModifier.RuntimePose = animatedPose;
                return animatedPose;
            }
            return null;
        }
    }
}
