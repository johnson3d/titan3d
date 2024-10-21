using EngineNS.Animation.Asset;
using EngineNS.Animation.Asset.BlendSpace;
using EngineNS.Animation.Command;
using EngineNS.Animation.Player;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.StateMachine.TimedSM;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtBlendSpaceEvaluateCommand<S> : TtAnimationCommand<S, TtLocalSpaceRuntimePose>
    {
        // Array??
        public List<TtExtractPoseFromClipCommand<S>> AnimCmds { get; set; } = new List<TtExtractPoseFromClipCommand<S>>();
        public TtAnimatableSkeletonPose AnimatableSkeletonPose { get; set; } = null;

        public TtBlendSpaceEvaluateCommandDesc Desc { get; set; } = new();
        public void Reset()
        {
            AnimCmds.Clear();
            Desc.Weights.Clear();
            Desc.Times.Clear();
        }
        public override void Execute()
        {
            if (AnimCmds.Count == 0)
                return;
            foreach(var animCmd in AnimCmds)
            {
                animCmd.Execute();
            }

            if (AnimCmds.Count == 1)
            {
                TtRuntimePoseUtility.CopyTransforms(ref mOutPose, AnimCmds[0].OutPose);
            }
            if (AnimCmds.Count == 2)
            {
                TtRuntimePoseUtility.BlendPoses(ref mOutPose, AnimCmds[0].OutPose, AnimCmds[1].OutPose, Desc.Weights[1]);
            }
            if (AnimCmds.Count > 2)
            {
                List<TtLocalSpaceRuntimePose> localPoses = new List<TtLocalSpaceRuntimePose>();
                foreach(var cmd in AnimCmds)
                {
                    localPoses.Add(cmd.OutPose);
                }
                TtRuntimePoseUtility.BlendPoses(ref mOutPose, localPoses, Desc.Weights);
            }
        }
    }
    public class TtBlendSpaceEvaluateCommandDesc : IAnimationCommandDesc
    {
        public List<float> Times { get; set; } = new List<float>();
        public List<float> Weights { get; set; } = new List<float>();
    }
    public class TtBlendTree_BlendSpace2D<S> : TtBlendTree<S, TtLocalSpaceRuntimePose>
    {
        public Func<Vector3> EvaluateInput { get; set; } = null;

        TtBlendSpace2D mBlendSpace = null;
        public TtBlendSpace2D BlendSpace
        { 
            get => mBlendSpace; 
            set
            {
                mBlendSpace = value;
            } 
        }

        public string SyncPlayPercentGrop { get; set; } = "";
        TtBlendSpaceEvaluateCommand<S> mAnimationCommand = null;
        public override async Thread.Async.TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            mAnimationCommand = new TtBlendSpaceEvaluateCommand<S>();
            mAnimationCommand.AnimatableSkeletonPose = context.AnimatableSkeletonPose;
            await base.Initialize(context);
            return true;
        }
        public override TtAnimationCommand<S, TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            context.AddCommand(context.TreeDepth, mAnimationCommand);

            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {
            UpdateRuntimeSamples(elapseSecond);

            mAnimationCommand.Reset();

            for (int i = 0; i < mRuntimeBlendSamples.Count; ++i)
            {
                TtAnimatableSkeletonPose clipPose = mAnimationCommand.AnimatableSkeletonPose.Clone() as TtAnimatableSkeletonPose;
                var clip = mRuntimeBlendSamples[i].Animation as TtAnimationClip;
                System.Diagnostics.Debug.Assert(clip != null);
                TtExtractPoseFromClipCommand<S> animEvaluateCmd = new TtExtractPoseFromClipCommand<S>(ref clipPose, clip);
                animEvaluateCmd.Time = PlayPercent * clip.Duration;

                mAnimationCommand.AnimCmds.Add(animEvaluateCmd);
                //mAnimationCommand.AnimPoses.Add(clipPose);
                mAnimationCommand.Desc.Weights.Add(mRuntimeBlendSamples[i].TotalWeight);
                mAnimationCommand.Desc.Times.Add(PlayPercent * clip.Duration);
            }
            mAnimationCommand.OutPose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(mAnimationCommand.AnimatableSkeletonPose);
        }
        public Vector3 Input { get; set; } = Vector3.Zero;
        private List<FBlendSample> mRuntimeBlendSamples = new List<FBlendSample>();
        public float Duration { get; set; }
        public uint KeyFrames { get; protected set; }
        public float PlayRate { get; set; } = 1.0f;
        public float Fps { get; protected set; }
        public float CurrentTime { get; set; }
        public float PlayPercent { get; set; } = 0.0f;
        protected float mLastTime = 0;
        float beforeTime = 0;
        public bool IsLoop { get; set; }
        public void UpdateRuntimeSamples(float elapseTimeSecond)
        {
            mRuntimeBlendSamples.Clear();
            mBlendSpace.EvaluateRuntimeSamplesByInput(Input, ref mRuntimeBlendSamples);

            float newDuration = 0;
            for (int i = 0; i < mRuntimeBlendSamples.Count; ++i)
            {
                var anim = mRuntimeBlendSamples[i].Animation;
                if(anim != null)
                {
                    newDuration += anim.Duration * mRuntimeBlendSamples[i].TotalWeight;
                }
            }
            float scale = 1;
            if (Duration > 0)
                scale = newDuration / Duration;
            Duration = newDuration;
            CurrentTime = CurrentTime * scale;
            mLastTime = CurrentTime;
            KeyFrames = (uint)(Duration * 30);

            CurrentTime += elapseTimeSecond * PlayRate;
            beforeTime = mLastTime;
            CurrentTime = CurrentTime % Duration;
            if (IsLoop == false && beforeTime > Duration)
            {
                CurrentTime = Duration;
            }
            PlayPercent = CurrentTime / Duration;
        }
    }
}
