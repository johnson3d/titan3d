using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.AnimNode
{
    [Rtti.MetaClass]
    public class AdditiveBlendSpace : BlendSpace
    {
        public override Pose.CGfxSkeletonPose Pose
        {
            get => mPose;
            set
            {
                mPose = value;
                //BasePose = value.Clone();
                ReferencePose = value.Clone();
                AdditivePose = value.Clone();
                mTempPose = value.Clone();
                mAddPose = value.Clone();
                mRefPose = value.Clone();
                mBasePose = value.Clone();
                if (ReferenceClip != null)
                {
                    ReferenceClip.Bind(ReferencePose);
                }
                for (int i = 0; i < mSamples.Count; i++)
                {
                    mSamples[i].Animation.Bind(value.Clone());
                }
            }
        }
        public Pose.CGfxSkeletonPose BasePose { get; set; } = null;
        public Pose.CGfxSkeletonPose ReferencePose { get; set; } = null;
        public Pose.CGfxSkeletonPose AdditivePose { get; set; } = null;
        public float Alpha { get; set; } = 1.0f;
        protected RName mReferenceAnimation = RName.EmptyName;
        public AnimationClip ReferenceClip { get; set; }
        [Rtti.MetaData]
        public RName ReferenceAnimation
        {
            get { return mReferenceAnimation; }
            set
            {
                if (value == null || value == RName.EmptyName)
                    return;
                mReferenceAnimation = value;
                ReferenceClip = AnimationClip.CreateSync(value);
                if (Pose != null)
                {
                    ReferencePose = Pose.Clone();
                    ReferenceClip.Bind(ReferencePose);
                }
            }
        }
        public override void Tick()
        {
            if (Pause)
                return;
            lock (CurrentSamples)
            {
                if (CurrentSamples == null || CurrentSamples.Count == 0)
                {
                    EvaluateNewSamples();
                }
                TimeAdvance(CEngine.Instance.EngineElapseTimeSecond);
                if (ReferenceClip != null)
                    ReferenceClip.Seek(PlayPercent * ReferenceClip.Duration);
                for (int i = 0; i < CurrentSamples.Count; ++i)
                {
                    CurrentSamples[i].Animation.Seek(PlayPercent * CurrentSamples[i].Animation.Duration);
                }
                BlendSamples(CurrentSamples);
            }
        }
        public override void Tick(float elapseTimeSecond)
        {
            if (Pause)
                return;
            lock (CurrentSamples)
            {
                if (CurrentSamples == null || CurrentSamples.Count == 0)
                {
                    EvaluateNewSamples();
                }
                TimeAdvance(elapseTimeSecond);
                if (ReferenceClip != null)
                    ReferenceClip.Seek(PlayPercent * ReferenceClip.Duration);
                for (int i = 0; i < CurrentSamples.Count; ++i)
                {
                    CurrentSamples[i].Animation.Seek(PlayPercent * CurrentSamples[i].Animation.Duration);
                }
                BlendSamples(CurrentSamples);
            }
        }
        public override void Evaluate(float playpercent)
        {
            lock (CurrentSamples)
            {
                if (CurrentSamples == null || CurrentSamples.Count == 0)
                {
                    EvaluateNewSamples();
                }
                if (ReferenceClip != null)
                    ReferenceClip.Seek(PlayPercent * ReferenceClip.Duration);
                for (int i = 0; i < CurrentSamples.Count; ++i)
                {
                    CurrentSamples[i].Animation.Seek(playpercent * CurrentSamples[i].Animation.Duration);
                }
                BlendSamples(CurrentSamples);
            }
        }
        public override void TickFroEditor(float playpercent)
        {
            lock (CurrentSamples)
            {
                if (CurrentSamples == null || CurrentSamples.Count == 0)
                {
                    EvaluateNewSamples();
                }
                if (ReferenceClip != null)
                    ReferenceClip.Seek(PlayPercent * ReferenceClip.Duration);
                for (int i = 0; i < CurrentSamples.Count; ++i)
                {
                    CurrentSamples[i].Animation.Seek(playpercent * CurrentSamples[i].Animation.Duration);
                }
                BlendSamples(CurrentSamples);
            }
        }
        public override void TickNofity(GamePlay.Component.GComponent component)
        {
            if (ReferenceClip != null)
                ReferenceClip.TickNofity(component);
            for (int i = 0; i < CurrentSamples.Count; ++i)
            {
                CurrentSamples[i].Animation.TickNofity(component);
            }
        }
        Pose.CGfxSkeletonPose mTempPose;
        Pose.CGfxSkeletonPose mAddPose ;
        Pose.CGfxSkeletonPose mRefPose;
        Pose.CGfxSkeletonPose mBasePose;
        protected override void BlendSamples(List<AnimationSampleData> samples)
        {
            if (samples == null)
                return;
            if (samples.Count == 1)
            {
                Animation.Runtime.CGfxAnimationRuntime.CopyPose(AdditivePose, samples[0].Animation.BindingSkeletonPose);
            }
            if (samples.Count == 2)
            {
                Animation.Runtime.CGfxAnimationRuntime.BlendPose(AdditivePose, samples[0].Animation.BindingSkeletonPose, samples[1].Animation.BindingSkeletonPose, samples[1].TotalWeight);
            }
            if (samples.Count > 2)
            {
                float totalWeigth = samples[0].TotalWeight + samples[1].TotalWeight;
                float bWeight = samples[1].TotalWeight / totalWeigth;
                Animation.Runtime.CGfxAnimationRuntime.BlendPose(AdditivePose, samples[0].Animation.BindingSkeletonPose, samples[1].Animation.BindingSkeletonPose, bWeight);
                for (int i = 2; i < samples.Count; ++i)
                {
                    totalWeigth += samples[i].TotalWeight;
                    bWeight = samples[i].TotalWeight / totalWeigth;
                    Animation.Runtime.CGfxAnimationRuntime.BlendPose(AdditivePose, AdditivePose, samples[i].Animation.BindingSkeletonPose, bWeight);
                }
            }
            if (ReferenceClip == null)
                Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(Pose, AdditivePose);
            else
            {
                Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPoseAndConvertRotationToMeshSpace(mAddPose, AdditivePose);
                Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPoseAndConvertRotationToMeshSpace(mRefPose, ReferencePose);
                //Bricks.Animation.Runtime.CGfxAnimationRuntime.MinusPoseMeshSpace(tempPose, refPose, addPose);
                Bricks.Animation.Runtime.CGfxAnimationRuntime.MinusPose(mTempPose, mRefPose, mAddPose);
                if (BasePose != null)
                {
                    Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPoseAndConvertRotationToMeshSpace(mBasePose, BasePose);
                    Bricks.Animation.Runtime.CGfxAnimationRuntime.AddPose(Pose, mBasePose, mTempPose, 1);
                    Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToLocalSpace(Pose);
                }
                else
                {
                    Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(Pose, mTempPose);
                }
            }
        }

    }
}

