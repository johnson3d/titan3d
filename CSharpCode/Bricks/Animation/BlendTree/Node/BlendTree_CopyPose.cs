using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtCopyPoseCommand<S, T> : TtAnimationCommand<S, T> where T : IRuntimePose
    {
        public TtAnimationCommand<S, T> FromCommand { get; set; } = null;
        public TtCopyPoseCommandDesc Desc { get; set; }
        public override void Execute()
        {
            if (FromCommand == null)
                return;

            TtRuntimePoseUtility.CopyPose(ref mOutPose, FromCommand.OutPose);
        }
    }
    public class TtCopyPoseCommandDesc : IAnimationCommandDesc
    {
    }
    public class TtBlendTree_CopyPose<S, T> : TtBlendTree<S, T> where T : IRuntimePose
    {
        protected TtCopyPoseCommandDesc Desc = null;
        public IBlendTree<S, T> FromNode { get; set; }

        TtCopyPoseCommand<S, T> mAnimationCommand = null;
        public override TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            mAnimationCommand = new TtCopyPoseCommand<S, T>();
            mAnimationCommand.Desc = new TtCopyPoseCommandDesc();
            return base.Initialize(context);
        }
        public override TtAnimationCommand<S, T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            System.Diagnostics.Debug.Assert(FromNode != null);
            if (FromNode == null)
                return null;

            context.AddCommand(context.TreeDepth, mAnimationCommand);

            context.TreeDepth++;
            mAnimationCommand.FromCommand = FromNode.ConstructAnimationCommandTree(mAnimationCommand, ref context);
            return mAnimationCommand;
        }
    }
}
