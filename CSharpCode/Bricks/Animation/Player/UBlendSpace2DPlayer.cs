using EngineNS.Animation.Animatable;
using EngineNS.Animation.Asset;
using EngineNS.Animation.Asset.BlendSpace;
using EngineNS.Animation.Pipeline;
using EngineNS.Animation.Pipeline.Command;
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
        public float Duration
        {
            get => DurationInMilliSecond * 0.001f;
            set => DurationInMilliSecond = (long)(value * 1000);
        }
        public long DurationInMilliSecond
        {
            get;
            protected set;
        }
        public uint KeyFrames
        {
            get;
            protected set;
        }
        public float PlayRate { get; set; } = 1.0f;
        public float Fps
        {
            get;
            protected set;
        }
        public float CurrentTime
        {
            get => mCurrentTimeInMilliSecond * 0.001f;
            set
            {
                mCurrentTimeInMilliSecond = (long)(value * 1000);
            }
        }
        long mCurrentTimeInMilliSecond = 0;
        public long CurrentTimeInMilliSecond
        {
            get => mCurrentTimeInMilliSecond;
            set
            {
                mCurrentTimeInMilliSecond = value;
            }
        }
        public float PlayPercent { get; set; } = 0.0f;

        //播放了多长时间，包括循环
        protected long mLastTime = 0;
        protected uint mCurrentLoopTimes = 0;
        public uint CurrentLoopTimes
        {
            get => mCurrentLoopTimes;
            set => mCurrentLoopTimes = value;
        }
        protected uint mLoopTimes = 0;
        public uint LoopTimes
        {
            get => mLoopTimes;
            set => mLoopTimes = value;
        }
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
        Int64 beforeTime = 0;
        protected void TimeAdvance(float elapseTimeSecond)
        {
            mLastTime += (long)((elapseTimeSecond * PlayRate) * 1000);
            CurrentLoopTimes = (uint)(mLastTime / (DurationInMilliSecond + 1));
            beforeTime = CurrentTimeInMilliSecond;
            CurrentTimeInMilliSecond = mLastTime % (DurationInMilliSecond + 1);
            if (IsLoop == false && beforeTime > CurrentTimeInMilliSecond)
            {
                CurrentTimeInMilliSecond = DurationInMilliSecond;
                Finish = true;
            }
            PlayPercent = CurrentTime / Duration;
        }
        private List<BlendSample> mRuntimeBlendSamples = new List<BlendSample>();
        public UBlendSpace2DPlayer(Asset.BlendSpace.UBlendSpace2D blendSpace2D)
        {
            System.Diagnostics.Debug.Assert(blendSpace2D != null);
            BlendSpace2D = blendSpace2D;
        }

        public void BindingPose(UAnimatableSkeletonPose bindedPose)
        {
            System.Diagnostics.Debug.Assert(bindedPose != null);
            BindedPose = bindedPose;
            LocalPose = URuntimePoseUtility.CreateLocalSpaceRuntimePose(BindedPose);


        }

        public void Update(float elapse)
        {
            UpdateRuntimeSamples();
            TimeAdvance(elapse);
        }

        public void Evaluate()
        {
            //make command  now is expensive of clonePose and BindingSetter need to optimize
            UBlendSpaceEvaluateCommand bsEvaluateCmd = new UBlendSpaceEvaluateCommand();
            for (int i = 0; i < mRuntimeBlendSamples.Count; ++i)
            {
                UAnimatableSkeletonPose clipPose = BindedPose.Clone() as UAnimatableSkeletonPose;
                var clip = mRuntimeBlendSamples[i].Animation as UAnimationClip;
                System.Diagnostics.Debug.Assert(clip != null);
                var setter = UAnimationPropertiesSetter.Binding(clip, clipPose);
                UPropertiesSettingCommand animEvaluateCmd = new UPropertiesSettingCommand()
                {
                    AnimationPropertiesSetter = setter,
                    Time = PlayPercent * clip.Duration
                };
                bsEvaluateCmd.AnimCmds.Add(animEvaluateCmd);
                bsEvaluateCmd.AnimPoses.Add(clipPose);
                bsEvaluateCmd.Weight.Add(mRuntimeBlendSamples[i].TotalWeight);
                bsEvaluateCmd.LocalSpaceRuntimePoses.Add(SkeletonAnimation.Runtime.Pose.URuntimePoseUtility.CreateLocalSpaceRuntimePose(BindedPose));
            }
            bsEvaluateCmd.FinalPose = URuntimePoseUtility.CreateLocalSpaceRuntimePose(BindedPose);
            //if(IsImmediate)
            bsEvaluateCmd.Execute();
            URuntimePoseUtility.ConvetToMeshSpaceRuntimePose(ref RuntimePose, bsEvaluateCmd.FinalPose);
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
            mLastTime = mCurrentTimeInMilliSecond;
            LoopTimes = 0;
            KeyFrames = (uint)(Duration * 30);

        }
    }
}
