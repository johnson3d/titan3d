using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Thread.Async;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtAdditivePoseCommand<S> : TtAnimationCommand<S, TtLocalSpaceRuntimePose>
    {
        public TtAnimationCommand<S, TtLPosMRotRuntimePose> AdditiveNode { get; set; }
        public TtAnimationCommand<S, TtLocalSpaceRuntimePose> BaseNode { get; set; }
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
    public class TtBlendTree_AdditivePose<S> : TtBlendTree<S, TtLocalSpaceRuntimePose>
    {
        public IBlendTree<S, TtLPosMRotRuntimePose> AdditiveNode { get; set; }
        public IBlendTree<S, TtLocalSpaceRuntimePose> BaseNode { get; set; }
        public Func<float> EvaluateAlpha { get; set; } = null;

        TtAdditivePoseCommand<S> mAnimationCommand = null;
        public override TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            mAnimationCommand = new TtAdditivePoseCommand<S>();
            mAnimationCommand.Desc = new TtAdditivePoseCommandDesc();
            return base.Initialize(context);
        }
        public override TtAnimationCommand<S, TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            System.Diagnostics.Debug.Assert(AdditiveNode != null && BaseNode != null);
            if (AdditiveNode == null || BaseNode == null)
                return null;

            context.AddCommand(context.TreeDepth, mAnimationCommand);

            context.TreeDepth++;
            mAnimationCommand.AdditiveNode = AdditiveNode.ConstructAnimationCommandTree(mAnimationCommand, ref context);
            mAnimationCommand.BaseNode = BaseNode.ConstructAnimationCommandTree(mAnimationCommand, ref context);
            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {
            mAnimationCommand.Desc.Alpha = EvaluateAlpha.Invoke();
        }
    }
}
