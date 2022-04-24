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
    public class UBlendSpaceEvaluateCommand : UAnimationCommand<ULocalSpaceRuntimePose>
    {
        // Array??
        public List<UExtractPoseFromClipCommand> AnimCmds { get; set; } = new List<UExtractPoseFromClipCommand>();
        public List<UAnimatableSkeletonPose> AnimPoses { get; set; } = new List<UAnimatableSkeletonPose>();

        public UBlendSpaceEvaluateCommandDesc Desc { get; set; }
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
                URuntimePoseUtility.CopyTransforms(ref mOutPose, AnimCmds[0].OutPose);
            }
            if (AnimCmds.Count == 2)
            {
                URuntimePoseUtility.BlendPoses(ref mOutPose, AnimCmds[0].OutPose, AnimCmds[1].OutPose, Desc.Weights[1]);
            }
            if (AnimCmds.Count > 2)
            {
                List<ULocalSpaceRuntimePose> localPoses = new List<ULocalSpaceRuntimePose>();
                foreach(var cmd in AnimCmds)
                {
                    localPoses.Add(cmd.OutPose);
                }
                URuntimePoseUtility.BlendPoses(ref mOutPose, localPoses, Desc.Weights);
            }
        }
    }
    public class UBlendSpaceEvaluateCommandDesc : IAnimationCommandDesc
    {
        public List<float> Times { get; set; } = new List<float>();
        public List<float> Weights { get; set; } = new List<float>();
    }
    public class UBlendTree_BlendSpace2D : UBlendTree<ULocalSpaceRuntimePose>
    {
        public Func<Vector3> EvaluateInput { get; set; } = null;

        UBlendSpace2D mBlendSpace = null;
        public UBlendSpace2D BlendSpace
        { 
            get => mBlendSpace; 
            set
            {
                mBlendSpace = value;
            } 
        }

        public string SyncPlayPercentGrop { get; set; } = "";
        UBlendSpaceEvaluateCommand mAnimationCommand = null;
        public override void Initialize()
        {
            mAnimationCommand = new UBlendSpaceEvaluateCommand();
            base.Initialize();
        }
        public override UAnimationCommand<ULocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            var desc = new UBlendSpaceEvaluateCommandDesc();
            mAnimationCommand.Desc = desc;
            context.AddCommand(context.TreeDepth, mAnimationCommand);

            for (int i = 0; i < mRuntimeBlendSamples.Count; ++i)
            {
                var clip = mRuntimeBlendSamples[i].Animation as UAnimationClip;
                System.Diagnostics.Debug.Assert(clip != null);
                UExtractPoseFromClipCommand animEvaluateCmd = new UExtractPoseFromClipCommand(clip);
                mAnimationCommand.AnimCmds.Add(animEvaluateCmd);
            }
            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond)
        {
            mAnimationCommand.Desc.Times.Clear();
            mAnimationCommand.Desc.Weights.Clear();
            for (int i = 0; i < mRuntimeBlendSamples.Count; ++i)
            {
                var clip = mRuntimeBlendSamples[i].Animation as UAnimationClip;
                System.Diagnostics.Debug.Assert(clip != null);

                mAnimationCommand.Desc.Times.Add(PlayPercent * clip.Duration);
                mAnimationCommand.Desc.Weights.Add(mRuntimeBlendSamples[i].TotalWeight);
            }
        }
        protected Vector3 mInput = Vector3.Zero;
        private List<BlendSample> mRuntimeBlendSamples = new List<BlendSample>();
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
