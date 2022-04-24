using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.BlendTree.Node
{
    public class UBindedPoseCommand<T> : UAnimationCommand<T> where T : IRuntimePose
    {
        public UBindedPoseCommandDesc Desc { get; set; }

        public override void Execute()
        {

        }
    }
    public class UBindedPoseCommandDesc : IAnimationCommandDesc
    {

    }
    public class UBlendTree_BindedPose : UBlendTree<ULocalSpaceRuntimePose>
    {
        UBindedPoseCommand<ULocalSpaceRuntimePose> mAnimationCommand = null;
        public override void Initialize()
        {
            mAnimationCommand = new UBindedPoseCommand<ULocalSpaceRuntimePose>();
        }
        public override UAnimationCommand<ULocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            var mAnimationCommand = new UBindedPoseCommand<ULocalSpaceRuntimePose>();
            var desc = new UBindedPoseCommandDesc();
            mAnimationCommand.Desc = desc; 
            context.AddCommand(context.TreeDepth, mAnimationCommand);

            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond)
        {

        }
    }
}
