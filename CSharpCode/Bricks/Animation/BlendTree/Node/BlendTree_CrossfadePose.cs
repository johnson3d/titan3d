using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtCrossfadePoseCommand<T> : TtAnimationCommand<T> where T : IRuntimePose
    {
        public TtAnimationCommand<T> FromNode { get; set; }
        public TtAnimationCommand<T> ToNode { get; set; }
        public TtCrossfadePoseCommandDesc Desc { get; set; }
        public override void Execute()
        {
            if (Desc.Weight == 0.0f)
            {
                TtRuntimePoseUtility.CopyPose(ref mOutPose, FromNode.OutPose);
            }
            else if (Desc.Weight == 1.0f)
            {
                TtRuntimePoseUtility.CopyPose(ref mOutPose, ToNode.OutPose);
            }
            else
            {
                TtRuntimePoseUtility.BlendPoses(ref mOutPose, FromNode.OutPose, ToNode.OutPose, Desc.Weight);
            }
        }
    }
    public class TtCrossfadePoseCommandDesc : IAnimationCommandDesc
    {
        public float Weight { get; set; }
    }
    public class TtBlendTree_CrossfadePose<T> : TtBlendTree<T> where T : IRuntimePose
    {
        public IBlendTree<T> FromNode { get; set; }
        public IBlendTree<T> ToNode { get; set; }
        public float BlendTime { get; set; } = 0.1f;
        public bool BlendCompelete = false;
        float mCurrentTime = 0.0f;

        TtCrossfadePoseCommand<T> mAnimationCommand = null;
        public override void Initialize()
        {
            mAnimationCommand = new TtCrossfadePoseCommand<T>();
            base.Initialize();
        }
        public override TtAnimationCommand<T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            var copyCmd = new TtCopyPoseCommand<T>();
            copyCmd.Desc = new TtCopyPoseCommandDesc();
            context.AddCommand(context.TreeDepth, mAnimationCommand);

            context.TreeDepth++;
            copyCmd.FromCommand = ToNode.ConstructAnimationCommandTree(copyCmd, ref context);
            return copyCmd;
        }
        public override void Tick(float elapseSecond, in FAnimBlendTreeTickContext context)
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

    public class TtBlendTree_ClipCrossfadeWithPose<T> : TtBlendTree_CrossfadePose<T> where T : IRuntimePose
    {

    }
}
