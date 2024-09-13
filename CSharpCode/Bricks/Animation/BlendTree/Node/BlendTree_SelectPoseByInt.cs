﻿using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtSelectPoseByIntCommand<S, T> : TtAnimationCommand<S, T> where T : IRuntimePose
    {
        public Dictionary<int, IBlendTree<S, T>> mPosesDic = new Dictionary<int, IBlendTree<S, T>>();
        public Dictionary<int, float> mPosesBlendTime = new Dictionary<int, float>();
        public Dictionary<int, float> mPosesBlendWeights = new Dictionary<int, float>();
        public TtSelectPoseByIntCommandDesc Desc { get; set; }

        public override void Execute()
        {
            //List<T> poses = new List<T>();
            //foreach (var pose in WeightedBlendPoses)
            //{
            //    poses.Add(pose.OutPose);
            //}
            //URuntimePoseUtility.BlendPoses(ref mOutPose, poses, Desc.Weights);
        }
    }
    public class TtSelectPoseByIntCommandDesc : IAnimationCommandDesc
    {
        int mLastSelected = 0;
        public int LastSelected { get => mLastSelected; }
        int mCurrentSelect = -1;
        public int CurrentSelect
        {
            get
            {
                return mCurrentSelect;
            }
            set
            {
                mLastSelected = mCurrentSelect;
                mCurrentSelect = value;
            }
        }
        public Func<int> EvaluateSelectedFunc { get; set; } = null;
        float mBlendTime = 0.1f;
        internal float Alpha = 0.0f;
        public float BlendTime
        {
            get
            {
                if (EvaluateBlendTimeFunc != null)
                    return EvaluateBlendTimeFunc.Invoke();
                return mBlendTime;
            }
        }
        public Func<float> EvaluateBlendTimeFunc { get; set; } = null;
    }

    public class TtBlendTree_SelectPoseByInt<S, T> : TtBlendTree<S, T> where T : IRuntimePose
    {
        TtSelectPoseByIntCommandDesc Desc = new TtSelectPoseByIntCommandDesc();
        public Dictionary<int, IBlendTree<S, T>> mPosesDic = new Dictionary<int, IBlendTree<S, T>>();
        public Dictionary<int, float> mPosesBlendTime = new Dictionary<int, float>();
        public Dictionary<int, float> mPosesBlendWeights = new Dictionary<int, float>();
        public int CurrentSelect
        {
            get
            {
                return Desc.CurrentSelect;
            }
            set
            {
                Desc.CurrentSelect = value;
            }
        }
        public void Add(int index, IBlendTree<S, T> pose)
        {
            if (mPosesDic.ContainsKey(index))
            {
                mPosesDic[index] = pose;
            }
            else
            {
                mPosesDic.Add(index, pose);
            }
        }
        TtSelectPoseByIntCommand<S, T> mAnimationCommand = null;
        public override async Thread.Async.TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            mAnimationCommand = new TtSelectPoseByIntCommand<S, T>();
            return true;
        }
        public override TtAnimationCommand<S, T> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            //TODO : just add no zero weight to command tree
            mAnimationCommand.mPosesDic = mPosesDic;
            mAnimationCommand.mPosesBlendTime = mPosesBlendTime;
            mAnimationCommand.mPosesBlendWeights = mPosesBlendWeights;
            var desc = new TtSelectPoseByIntCommandDesc();
            mAnimationCommand.Desc = desc;

            context.AddCommand(context.TreeDepth, mAnimationCommand);

            return mAnimationCommand;
        }

        public override void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {
            base.Tick(elapseSecond, ref context);
            //TODO : alpha evaluate
           

        }
    }
}
