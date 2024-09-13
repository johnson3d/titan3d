using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtBindedPoseCommand<S, T> : TtAnimationCommand<S, T> where T : IRuntimePose
    {
        public TtBindedPoseCommandDesc Desc { get; set; }

        public override void Execute()
        {

        }
    }
    public class TtBindedPoseCommandDesc : IAnimationCommandDesc
    {

    }
    public class TtBlendTree_BindedPose<S> : TtBlendTree<S, TtLocalSpaceRuntimePose>
    {
        TtBindedPoseCommand<S, TtLocalSpaceRuntimePose> mAnimationCommand = null;
        public override async Thread.Async.TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            mAnimationCommand = new TtBindedPoseCommand<S, TtLocalSpaceRuntimePose>();
            return true;
        }
        public override TtAnimationCommand<S, TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            var mAnimationCommand = new TtBindedPoseCommand<S, TtLocalSpaceRuntimePose>();
            var desc = new TtBindedPoseCommandDesc();
            mAnimationCommand.Desc = desc; 
            context.AddCommand(context.TreeDepth, mAnimationCommand);

            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {

        }
    }
}
