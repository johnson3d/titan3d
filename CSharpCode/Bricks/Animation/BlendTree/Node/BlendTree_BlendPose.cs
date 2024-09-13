using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtBlendPoseCommand<S, T> : TtAnimationCommand<S, T> where T : IRuntimePose
    {
        public List<TtAnimationCommand<S, T>> WeightedBlendPoses = new List<TtAnimationCommand<S, T>>();
        public TtBlendPoseCommandDesc Desc { get; set; }
        public override void Execute()
        {
            List<T> poses = new List<T>();
            foreach(var pose in WeightedBlendPoses)
            {
                poses.Add(pose.OutPose);
            }
            TtRuntimePoseUtility.BlendPoses(ref mOutPose, poses, Desc.Weights);
        }
    }
    public class TtBlendPoseCommandDesc : IAnimationCommandDesc
    {
        public List<float> Weights { get; set; }
    }

    public class TtBlendTree_BlendPose<S, T> : TtBlendTree<S, T> where T : IRuntimePose
    {
        public List<IBlendTree<S, T>> WeightedTree = new List<IBlendTree<S, T>>();

        public List<float> Weights { get; set; }

        TtBlendPoseCommand<S, T> mAnimationCommand = null;
        public override async Thread.Async.TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            mAnimationCommand = new TtBlendPoseCommand<S, T>();
            return true;
        }
        public override TtAnimationCommand<S, T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            mAnimationCommand = new TtBlendPoseCommand<S, T>();
            var desc = new TtBlendPoseCommandDesc();
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
        public override void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {
            
        }

    }
}
