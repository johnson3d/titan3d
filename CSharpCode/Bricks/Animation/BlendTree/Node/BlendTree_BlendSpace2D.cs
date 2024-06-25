using EngineNS.Animation.Asset;
using EngineNS.Animation.Asset.BlendSpace;
using EngineNS.Animation.Command;
using EngineNS.Animation.Player;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtBlendSpaceEvaluateCommand : TtAnimationCommand<TtLocalSpaceRuntimePose>
    {
        // Array??
        public List<TtExtractPoseFromClipCommand> AnimCmds { get; set; } = new List<TtExtractPoseFromClipCommand>();
        public List<TtAnimatableSkeletonPose> AnimPoses { get; set; } = new List<TtAnimatableSkeletonPose>();

        public TtBlendSpaceEvaluateCommandDesc Desc { get; set; }
        public void Reset()
        {
            AnimCmds.Clear();
            AnimPoses.Clear();
        }
        public override void Execute()
        {
            if (AnimCmds.Count == 0)
                return;

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
    public class TtBlendTree_BlendSpace2D : TtBlendTree<TtLocalSpaceRuntimePose>
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
        TtBlendSpaceEvaluateCommand mAnimationCommand = null;
        public override void Initialize(ref FAnimBlendTreeContext context)
        {
            mAnimationCommand = new TtBlendSpaceEvaluateCommand();
            base.Initialize(ref context);
        }
        public override TtAnimationCommand<TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            var desc = new TtBlendSpaceEvaluateCommandDesc();
            mAnimationCommand.Desc = desc;
            context.AddCommand(context.TreeDepth, mAnimationCommand);

            for (int i = 0; i < mRuntimeBlendSamples.Count; ++i)
            {
                var clip = mRuntimeBlendSamples[i].Animation as TtAnimationClip;
                System.Diagnostics.Debug.Assert(clip != null);
                TtExtractPoseFromClipCommand animEvaluateCmd = new TtExtractPoseFromClipCommand(clip);
                mAnimationCommand.AnimCmds.Add(animEvaluateCmd);
            }
            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {
            mAnimationCommand.Desc.Times.Clear();
            mAnimationCommand.Desc.Weights.Clear();
            for (int i = 0; i < mRuntimeBlendSamples.Count; ++i)
            {
                var clip = mRuntimeBlendSamples[i].Animation as TtAnimationClip;
                System.Diagnostics.Debug.Assert(clip != null);

                mAnimationCommand.Desc.Times.Add(PlayPercent * clip.Duration);
                mAnimationCommand.Desc.Weights.Add(mRuntimeBlendSamples[i].TotalWeight);
            }
        }
        protected Vector3 mInput = Vector3.Zero;
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
            mBlendSpace.EvaluateRuntimeSamplesByInput(mInput, ref mRuntimeBlendSamples);

            float newDuration = 0;
            for (int i = 0; i < mRuntimeBlendSamples.Count; ++i)
            {
                var anim = mRuntimeBlendSamples[i].Animation;
                newDuration += anim.Duration * mRuntimeBlendSamples[i].TotalWeight;
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
