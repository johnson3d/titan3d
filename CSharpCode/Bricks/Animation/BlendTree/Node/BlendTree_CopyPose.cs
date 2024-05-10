using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtCopyPoseCommand<T> : TtAnimationCommand<T> where T : IRuntimePose
    {
        public TtAnimationCommand<T> FromCommand { get; set; } = null;
        public TtCopyPoseCommandDesc Desc { get; set; }
        public override void Execute()
        {
            if (FromCommand == null)
                return;
            TtRuntimePoseUtility.CopyTransforms(ref mOutPose, FromCommand.OutPose);
        }
    }
    public class TtCopyPoseCommandDesc : IAnimationCommandDesc
    {
    }
    public class TtBlendTree_CopyPose<T> : TtBlendTree<T> where T : IRuntimePose
    {
        protected TtCopyPoseCommandDesc Desc = null;
        public IBlendTree<T> FromNode { get; set; }

        TtCopyPoseCommand<T> mAnimationCommand = null;
        public override TtAnimationCommand<T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            mAnimationCommand = new TtCopyPoseCommand<T>();
            mAnimationCommand.Desc = new TtCopyPoseCommandDesc();
            context.AddCommand(context.TreeDepth, mAnimationCommand);

            context.TreeDepth++;
            mAnimationCommand.FromCommand = FromNode.ConstructAnimationCommandTree(mAnimationCommand, ref context);
            return mAnimationCommand;
        }
    }
}
