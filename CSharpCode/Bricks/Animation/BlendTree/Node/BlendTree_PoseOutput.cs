using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtLocalSpaceBlendTree_PoseOutput<S> : TtBlendTree<S, TtLocalSpaceRuntimePose>
    {
        protected TtCopyPoseCommandDesc Desc = null;
        public IBlendTree<S, TtLocalSpaceRuntimePose> FromNode { get; set; }

        TtCopyPoseCommand<S, TtLocalSpaceRuntimePose> mAnimationCommand = null;
        public override async Thread.Async.TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            mAnimationCommand = new();
            mAnimationCommand.OutPose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(context.AnimatableSkeletonPose);
            mAnimationCommand.Desc = new TtCopyPoseCommandDesc();
            return true;
        }

        public override void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {
            base.Tick(elapseSecond, ref context);
            FromNode.Tick(elapseSecond, ref context);
        }
        public override TtAnimationCommand<S, TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
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
