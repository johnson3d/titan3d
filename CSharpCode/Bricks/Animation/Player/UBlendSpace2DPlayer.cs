using EngineNS.Animation.Animatable;
using EngineNS.Animation.Asset;
using EngineNS.Animation.Asset.BlendSpace;
using EngineNS.Animation.BlendTree.Node;
using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Player
{
    public class UBlendSpace2DPlayer
    {
        public float Time { get; set; } = 0;
        protected Vector3 mInput = Vector3.Zero;
        public Vector3 Input
        {
            get => mInput;
            set
            {
                if (mInput == value)
                    return;
                mInput = value;
            }
        }
        public Asset.BlendSpace.UBlendSpace2D BlendSpace2D { get; set; } = null;
        public SkeletonAnimation.AnimatablePose.UAnimatableSkeletonPose BindedPose { get; set; } = null;
        public UMeshSpaceRuntimePose RuntimePose = null;
        public ULocalSpaceRuntimePose LocalPose = null;
        public float Duration { get; set; }

        public uint KeyFrames { get; protected set; }
        public float PlayRate { get; set; } = 1.0f;
        public float Fps
        {
            get;
            protected set;
        }
        public float CurrentTime { get; set; }
        public float PlayPercent { get; set; } = 0.0f;

        //播放了多长时间，包括循环
        protected float mLastTime = 0;
        protected bool mIsLoop = true;
        public bool IsLoop
        {
            get => mIsLoop;
            set => mIsLoop = value;
        }
        protected bool mPause = false;
        public bool Pause
        {
            get => mPause;
            set => mPause = value;
        }
        protected bool mFinish = false;
        public bool Finish
        {
            get => mFinish;
            set => mFinish = value;
        }
        float beforeTime = 0;
        protected void TimeAdvance(float elapseTimeSecond)
        {
            mLastTime += elapseTimeSecond * PlayRate;
            beforeTime = CurrentTime;
            CurrentTime = mLastTime % Duration;
            if (IsLoop == false && beforeTime > Duration)
            {
                CurrentTime = Duration;
                Finish = true;
            }
            PlayPercent = CurrentTime / Duration;
        }
        private List<BlendSample> mRuntimeBlendSamples = new List<BlendSample>();
        public UBlendSpace2DPlayer(Asset.BlendSpace.UBlendSpace2D blendSpace2D)
        {
            System.Diagnostics.Debug.Assert(blendSpace2D != null);
            BlendSpace2D = blendSpace2D;
            mEvaluateCmd = new UBlendSpaceEvaluateCommand();
            mEvaluateCmd.Desc = new UBlendSpaceEvaluateCommandDesc();
        }

        public void BindingPose(UAnimatableSkeletonPose bindedPose)
        {
            System.Diagnostics.Debug.Assert(bindedPose != null);
            BindedPose = bindedPose;
            LocalPose = URuntimePoseUtility.CreateLocalSpaceRuntimePose(BindedPose);


        }
        UBlendSpaceEvaluateCommand mEvaluateCmd = null;
        public void Update(float elapse)
        {
            UpdateRuntimeSamples();
            TimeAdvance(elapse);

            mEvaluateCmd.Reset();
            for (int i = 0; i < mRuntimeBlendSamples.Count; ++i)
            {
                UAnimatableSkeletonPose clipPose = BindedPose.Clone() as UAnimatableSkeletonPose;
                var clip = mRuntimeBlendSamples[i].Animation as UAnimationClip;
                System.Diagnostics.Debug.Assert(clip != null);
                UExtractPoseFromClipCommand animEvaluateCmd = new UExtractPoseFromClipCommand(ref clipPose,clip);
                animEvaluateCmd.Time = PlayPercent * clip.Duration;

                mEvaluateCmd.AnimCmds.Add(animEvaluateCmd);
                mEvaluateCmd.AnimPoses.Add(clipPose);
                mEvaluateCmd.Desc.Weights.Add(mRuntimeBlendSamples[i].TotalWeight);
                mEvaluateCmd.Desc.Times.Add(PlayPercent * clip.Duration);
            }
            mEvaluateCmd.OutPose = URuntimePoseUtility.CreateLocalSpaceRuntimePose(BindedPose);
        }

        public void Evaluate()
        {
            //make command  now is expensive of clonePose and BindingSetter need to optimize

            
            //if(IsImmediate)
            for(int i = 0;i< mEvaluateCmd.AnimCmds.Count; ++i)
            {
                //TODO: 
                mEvaluateCmd.AnimCmds[i].Execute();
            }
            mEvaluateCmd.Execute();
            URuntimePoseUtility.ConvetToMeshSpaceRuntimePose(ref RuntimePose, mEvaluateCmd.OutPose);
            //else Insert to pipeline
            //AnimationPiple.CommandList.Add();
        }

        public void UpdateRuntimeSamples()
        {
            lock (mRuntimeBlendSamples)
            {
                mRuntimeBlendSamples.Clear();
                BlendSpace2D.EvaluateRuntimeSamplesByInput(mInput, ref mRuntimeBlendSamples);
            }
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

        }
    }
}
