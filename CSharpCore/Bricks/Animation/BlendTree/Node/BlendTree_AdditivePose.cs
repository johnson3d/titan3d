using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.BlendTree.Node
{
    public class BlendTree_AdditivePose : IBlendTree
    {
        public Pose.CGfxSkeletonPose OutPose { get; set; }
        public IBlendTree AdditiveNode { get; set; }
        public IBlendTree BaseNode { get; set; }
        public Func<float> EvaluateAlpha { get; set; } = null;
        Pose.CGfxSkeletonPose mMeshSpaceBasePose = null;
        public void InitializePose(Pose.CGfxSkeletonPose pose)
        {
            OutPose = pose.Clone();
            mMeshSpaceBasePose = pose.Clone();
            AdditiveNode?.InitializePose(pose);
            BaseNode?.InitializePose(pose);
        }
        public void Evaluate(float timeInSecond)
        {
            BaseNode?.Evaluate(timeInSecond);
            AdditiveNode?.Evaluate(timeInSecond);
            if (AdditiveNode == null && BaseNode != null)
            {
                Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(OutPose, BaseNode.OutPose);
                return;
            }
            if (AdditiveNode != null && BaseNode == null)
            {
                Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(OutPose, AdditiveNode.OutPose);
            }
            Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPoseAndConvertRotationToMeshSpace(mMeshSpaceBasePose, BaseNode.OutPose);
            if (EvaluateAlpha != null)
            {
                Bricks.Animation.Runtime.CGfxAnimationRuntime.AddPose(OutPose, mMeshSpaceBasePose, AdditiveNode.OutPose, EvaluateAlpha.Invoke());
            }
            else
            {
                Bricks.Animation.Runtime.CGfxAnimationRuntime.AddPose(OutPose, mMeshSpaceBasePose, AdditiveNode.OutPose, 1);

            }
            Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToLocalSpace(OutPose);
        }
        public void Notifying(GamePlay.Component.GComponent component)
        {
            BaseNode?.Notifying(component);
            AdditiveNode?.Notifying(component);
        }
    }
}
