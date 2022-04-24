using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class UBlendPoseCommand<T> : UAnimationCommand<T> where T : IRuntimePose
    {
        public List<UAnimationCommand<T>> WeightedBlendPoses = new List<UAnimationCommand<T>>();
        public UBlendPoseCommandDesc Desc { get; set; }
        public override void Execute()
        {
            List<T> poses = new List<T>();
            foreach(var pose in WeightedBlendPoses)
            {
                poses.Add(pose.OutPose);
            }
            URuntimePoseUtility.BlendPoses(ref mOutPose, poses, Desc.Weights);
        }
    }
    public class UBlendPoseCommandDesc : IAnimationCommandDesc
    {
        public List<float> Weights { get; set; }
    }

    public class UBlendTree_BlendPose<T> : UBlendTree<T> where T : IRuntimePose
    {
        public List<IBlendTree<T>> WeightedTree = new List<IBlendTree<T>>();

        public List<float> Weights { get; set; }

        UBlendPoseCommand<T> mAnimationCommand = null;
        public override void Initialize()
        {
            mAnimationCommand = new UBlendPoseCommand<T>();
        }
        public override UAnimationCommand<T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            mAnimationCommand = new UBlendPoseCommand<T>();
            var desc = new UBlendPoseCommandDesc();
            desc.Weights = Weights;
            context.AddCommand(context.TreeDepth, mAnimationCommand);

            context.TreeDepth++;
            for (int i = 0; i< WeightedTree.Count; ++i)
            {
                if (Weights[i] > 0)
                {
                    mAnimationCommand.WeightedBlendPoses.Add(WeightedTree[i].ConstructAnimationCommandTree(mAnimationCommand, ref context));
                    desc.Weights.Add(Weights[i]);
                }
            }
            mAnimationCommand.Desc = desc;
            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond)
        {
            
        }

    }
}
