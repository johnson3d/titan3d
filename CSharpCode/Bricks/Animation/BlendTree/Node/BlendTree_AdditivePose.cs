using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtAdditivePoseCommand : TtAnimationCommand<TtLocalSpaceRuntimePose>
    {
        public TtAnimationCommand<TtLPosMRotRuntimePose> AdditiveNode { get; set; }
        public TtAnimationCommand<TtLocalSpaceRuntimePose> BaseNode { get; set; }
        TtLPosMRotRuntimePose mLPosMRotBasePose = null;
        TtLPosMRotRuntimePose mLPosMRotResultPose = null;
        public TtAdditivePoseCommandDesc Desc { get; set; }
        public override void Execute()
        {
            if (AdditiveNode == null && BaseNode != null)
            {
                TtRuntimePoseUtility.CopyPose(ref mOutPose, BaseNode.OutPose);
                return;
            }
            if (AdditiveNode != null && BaseNode == null)
            {
                TtRuntimePoseUtility.ConvetToLocalSpaceRuntimePose(ref mOutPose, AdditiveNode.OutPose);
            }
            TtRuntimePoseUtility.ConvetToLPosMRotRuntimePose(ref mLPosMRotBasePose, BaseNode.OutPose);
            TtRuntimePoseUtility.AddPoses(ref mLPosMRotResultPose, mLPosMRotBasePose, AdditiveNode.OutPose, Desc.Alpha);
            TtRuntimePoseUtility.ConvetToLocalSpaceRuntimePose(ref mOutPose, mLPosMRotResultPose);
        }
    }
    public class TtAdditivePoseCommandDesc : IAnimationCommandDesc
    {
        public float Alpha { get; set; }
    }
    public class TtBlendTree_AdditivePose : TtBlendTree<TtLocalSpaceRuntimePose>
    {
        public IBlendTree<TtLPosMRotRuntimePose> AdditiveNode { get; set; }
        public IBlendTree<TtLocalSpaceRuntimePose> BaseNode { get; set; }
        public Func<float> EvaluateAlpha { get; set; } = null;

        TtAdditivePoseCommand mAnimationCommand = null;
        public override TtAnimationCommand<TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            mAnimationCommand = new TtAdditivePoseCommand();
            mAnimationCommand.Desc = new TtAdditivePoseCommandDesc();
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
