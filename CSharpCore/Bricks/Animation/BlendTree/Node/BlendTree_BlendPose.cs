using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.BlendTree.Node
{
    public class BlendTree_BlendPose : IBlendTree
    {
        public Pose.CGfxSkeletonPose OutPose { get; set; }
        public IBlendTree SourceNode { get; set; }
        public IBlendTree DestinationNode { get; set; }
        public Func<float> EvaluateWeight { get; set; } = null;
        public float Weight { get; set; } = 0.0f;
        public void InitializePose(Pose.CGfxSkeletonPose pose)
        {
            OutPose = pose.Clone();
            SourceNode?.InitializePose(pose);
            DestinationNode?.InitializePose(pose);
        }
        public void Evaluate(float timeInSecond)
        {
            SourceNode?.Evaluate(timeInSecond);
            DestinationNode?.Evaluate(timeInSecond);
            if (SourceNode == null)
                Runtime.CGfxAnimationRuntime.CopyPose(OutPose,DestinationNode.OutPose);
            if (DestinationNode == null)
                Runtime.CGfxAnimationRuntime.CopyPose(OutPose, SourceNode.OutPose);
            if (EvaluateWeight != null)
                Weight = EvaluateWeight.Invoke();
            Runtime.CGfxAnimationRuntime.BlendPose(OutPose, SourceNode.OutPose, DestinationNode.OutPose, Weight);
        }
        public void Notifying(GamePlay.Component.GComponent component)
        {
            SourceNode?.Notifying(component);
            DestinationNode?.Notifying(component);
        }
    }
}
