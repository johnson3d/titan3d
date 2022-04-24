using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;

namespace EngineNS.Animation.BlendTree.Node
{
    public class UAdditivePoseCommand : UAnimationCommand<ULocalSpaceRuntimePose>
    {
        public UAnimationCommand<ULPosMRotRuntimePose> AdditiveNode { get; set; }
        public UAnimationCommand<ULocalSpaceRuntimePose> BaseNode { get; set; }
        ULPosMRotRuntimePose mLPosMRotBasePose = null;
        ULPosMRotRuntimePose mLPosMRotResultPose = null;
        public UAdditivePoseCommandDesc Desc { get; set; }
        public override void Execute()
        {
            if (AdditiveNode == null && BaseNode != null)
            {
                URuntimePoseUtility.CopyPose(ref mOutPose, BaseNode.OutPose);
                return;
            }
            if (AdditiveNode != null && BaseNode == null)
            {
                URuntimePoseUtility.ConvetToLocalSpaceRuntimePose(ref mOutPose, AdditiveNode.OutPose);
            }
            URuntimePoseUtility.ConvetToLPosMRotRuntimePose(ref mLPosMRotBasePose, BaseNode.OutPose);
            URuntimePoseUtility.AddPoses(ref mLPosMRotResultPose, mLPosMRotBasePose, AdditiveNode.OutPose, Desc.Alpha);
            URuntimePoseUtility.ConvetToLocalSpaceRuntimePose(ref mOutPose, mLPosMRotResultPose);
        }
    }
    public class UAdditivePoseCommandDesc : IAnimationCommandDesc
    {
        public float Alpha { get; set; }
    }
    public class BlendTree_AdditivePose : UBlendTree<ULocalSpaceRuntimePose>
    {
        public IBlendTree<ULPosMRotRuntimePose> AdditiveNode { get; set; }
        public IBlendTree<ULocalSpaceRuntimePose> BaseNode { get; set; }
        public Func<float> EvaluateAlpha { get; set; } = null;

        UAdditivePoseCommand mAnimationCommand = null;
        public override UAnimationCommand<ULocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            mAnimationCommand = new UAdditivePoseCommand();
            mAnimationCommand.Desc = new UAdditivePoseCommandDesc();
            context.AddCommand(context.TreeDepth, mAnimationCommand);

            context.TreeDepth++;
            mAnimationCommand.AdditiveNode = AdditiveNode.ConstructAnimationCommandTree(mAnimationCommand, ref context);
            mAnimationCommand.BaseNode = BaseNode.ConstructAnimationCommandTree(mAnimationCommand, ref context);
            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond)
        {
            mAnimationCommand.Desc.Alpha = EvaluateAlpha.Invoke();
        }
    }
}
