using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class UCrossfadePoseCommand<T> : UAnimationCommand<T> where T : IRuntimePose
    {
        public UAnimationCommand<T> FromNode { get; set; }
        public UAnimationCommand<T> ToNode { get; set; }
        public UCrossfadePoseCommandDesc Desc { get; set; }
        public override void Execute()
        {
            if (Desc.Weight == 0.0f)
            {
                URuntimePoseUtility.CopyPose(ref mOutPose, FromNode.OutPose);
            }
            else if (Desc.Weight == 1.0f)
            {
                URuntimePoseUtility.CopyPose(ref mOutPose, ToNode.OutPose);
            }
            else
            {
                URuntimePoseUtility.BlendPoses(ref mOutPose, FromNode.OutPose, ToNode.OutPose, Desc.Weight);
            }
        }
    }
    public class UCrossfadePoseCommandDesc : IAnimationCommandDesc
    {
        public float Weight { get; set; }
    }
    public class UBlendTree_CrossfadePose<T> : UBlendTree<T> where T : IRuntimePose
    {
        public IBlendTree<T> FromNode { get; set; }
        public IBlendTree<T> ToNode { get; set; }
        public float BlendTime { get; set; } = 0.1f;
        public bool BlendCompelete = false;
        float mCurrentTime = 0.0f;

        UCrossfadePoseCommand<T> mAnimationCommand = null;
        public override void Initialize()
        {
            mAnimationCommand = new UCrossfadePoseCommand<T>();
            base.Initialize();
        }
        public override UAnimationCommand<T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            var copyCmd = new UCopyPoseCommand<T>();
            copyCmd.Desc = new UCopyPoseCommandDesc();
            context.AddCommand(context.TreeDepth, mAnimationCommand);

            context.TreeDepth++;
            copyCmd.FromCommand = ToNode.ConstructAnimationCommandTree(copyCmd, ref context);
            return copyCmd;
        }
        public override void Tick(float elapseSecond)
        {
            mCurrentTime += elapseSecond;
            mAnimationCommand.Desc.Weight = Math.Min(mCurrentTime / BlendTime, 1.0f);
        }

        public void ResetTime()
        {
            mCurrentTime = 0.0f;
            BlendCompelete = false;
        }
    }
}
