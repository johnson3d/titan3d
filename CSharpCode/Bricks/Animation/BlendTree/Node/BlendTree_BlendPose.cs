using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtBlendPoseCommand<T> : TtAnimationCommand<T> where T : IRuntimePose
    {
        public List<TtAnimationCommand<T>> WeightedBlendPoses = new List<TtAnimationCommand<T>>();
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

    public class TtBlendTree_BlendPose<T> : TtBlendTree<T> where T : IRuntimePose
    {
        public List<IBlendTree<T>> WeightedTree = new List<IBlendTree<T>>();

        public List<float> Weights { get; set; }

        TtBlendPoseCommand<T> mAnimationCommand = null;
        public override void Initialize()
        {
            mAnimationCommand = new TtBlendPoseCommand<T>();
        }
        public override TtAnimationCommand<T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            mAnimationCommand = new TtBlendPoseCommand<T>();
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
        public override void Tick(float elapseSecond, in FAnimBlendTreeTickContext context)
        {
            
        }

    }
}
