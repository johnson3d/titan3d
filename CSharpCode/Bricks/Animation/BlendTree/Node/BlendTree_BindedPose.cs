using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtBindedPoseCommand<T> : TtAnimationCommand<T> where T : IRuntimePose
    {
        public TtBindedPoseCommandDesc Desc { get; set; }

        public override void Execute()
        {

        }
    }
    public class TtBindedPoseCommandDesc : IAnimationCommandDesc
    {

    }
    public class TtBlendTree_BindedPose : TtBlendTree<TtLocalSpaceRuntimePose>
    {
        TtBindedPoseCommand<TtLocalSpaceRuntimePose> mAnimationCommand = null;
        public override void Initialize()
        {
            mAnimationCommand = new TtBindedPoseCommand<TtLocalSpaceRuntimePose>();
        }
        public override TtAnimationCommand<TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            var mAnimationCommand = new TtBindedPoseCommand<TtLocalSpaceRuntimePose>();
            var desc = new TtBindedPoseCommandDesc();
            mAnimationCommand.Desc = desc; 
            context.AddCommand(context.TreeDepth, mAnimationCommand);

            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond)
        {

        }
    }
}
