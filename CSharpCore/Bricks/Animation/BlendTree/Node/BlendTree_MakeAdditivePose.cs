using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.BlendTree.Node
{
public class BlendTree_MakeAdditivePose : IBlendTree
    {
        public Pose.CGfxSkeletonPose OutPose { get; set; }
        public IBlendTree AdditiveNode { get; set; }
        public IBlendTree RefNode { get; set; }
        Pose.CGfxSkeletonPose mAddPose { get; set; }
        Pose.CGfxSkeletonPose mRefPose { get; set; }
        public void InitializePose(Pose.CGfxSkeletonPose pose)
        {
            OutPose = pose.Clone();
            mAddPose = pose.Clone();
            mRefPose = pose.Clone();
            AdditiveNode?.InitializePose(pose);
            RefNode?.InitializePose(pose);
        }
        public void Evaluate(float timeInSecond)
        {
            if (RefNode == null || AdditiveNode == null)
            {
                Bricks.Animation.Runtime.CGfxAnimationRuntime.ZeroPose(OutPose);
                return;
            }
            RefNode.Evaluate(timeInSecond);
            AdditiveNode.Evaluate(timeInSecond);
            Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPoseAndConvertRotationToMeshSpace(mAddPose, AdditiveNode.OutPose);
            Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPoseAndConvertRotationToMeshSpace(mRefPose, RefNode.OutPose);
            Bricks.Animation.Runtime.CGfxAnimationRuntime.MinusPose(OutPose, mRefPose, mAddPose);
        }
        public void Notifying(GamePlay.Component.GComponent component)
        {
            RefNode?.Notifying(component);
            AdditiveNode?.Notifying(component);
        }
    }
}
