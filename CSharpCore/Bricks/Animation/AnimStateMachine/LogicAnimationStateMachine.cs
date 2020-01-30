using EngineNS.Bricks.Animation.Pose;
using EngineNS.Bricks.Animation.PoseControl.Blend;
using EngineNS.Bricks.FSM;
using EngineNS.Bricks.FSM.TFSM;
using EngineNS.GamePlay.Component;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.AnimStateMachine
{
    public enum AnimLayerType
    {
        Default,
        Additive,
        PerBone,
    }
    public class LogicAnimationStateMachine : TimedFiniteStateMachine
    {
        static Dictionary<string, float> SyncPlayPercents = new Dictionary<string, float>();
        public static void AddSyncPlayPercent(string name)
        {
            if (!SyncPlayPercents.ContainsKey(name))
                SyncPlayPercents.Add(name, 0);
        }
        public static void UpdateSyncPlayPercent(string name, float playPercent)
        {
            if (SyncPlayPercents.ContainsKey(name))
            {
                SyncPlayPercents[name] = playPercent;
            }
        }
        public static float GetSyncPlayPercent(string name)
        {
            if (SyncPlayPercents.ContainsKey(name))
            {
                return SyncPlayPercents[name]; ;
            }
            return 0;
        }
        public AnimLayerType LayerType { get; set; } = AnimLayerType.Default;
        CGfxSkeletonPose mPose = null;
        public CGfxSkeletonPose Pose
        {
            get => mPose;
            set
            {
                mPose = value;
                mCrossfadePose.OutPose = value.Clone();
            }
        }
        public new LogicAnimationState PreviousState
        {
            get => mPreviousState as LogicAnimationState;
        }
        public new LogicAnimationState CurrentState
        {
            get => mCurrentState as LogicAnimationState;
        }
        public LogicAnimationStateMachine()
        {

        }
        public LogicAnimationStateMachine(string name) : base(name)
        {

        }
        public override void Initialize()
        {
            base.Initialize();
        }

        AnimCrossfading mAnimCrossfading = null;
        AnimCrossfading mPostAnimCrossfading = null;
        CrossfadePose mCrossfadePose = new CrossfadePose();
        public AnimCrossfading AnimCrossfading
        {
            get => mAnimCrossfading;
            set
            {
                if (value != null && value.FadeTime < 0)
                    return;
                mAnimCrossfading = value;
            }
        }
        public override void SetCurrentStateByTransition(Transition transition)
        {
            var laT = transition as LATimedTransitionFunction;
            mPostAnimCrossfading = laT.AnimCrossfading;
            base.SetCurrentStateByTransition(transition);
        }
        public override void OnStateChange()
        {
            if (mPostAnimCrossfading == null)
            {
                mAnimCrossfading = null;
                return;
            }
            mAnimCrossfading = mPostAnimCrossfading;
            mCrossfadePose.FromPose = PreviousState.Pose;
            mCrossfadePose.ToPose = CurrentState.Pose;
            mCrossfadePose.BlendTime = mAnimCrossfading.FadeTime;
            mCrossfadePose.ResetTime();
            base.OnStateChange();
        }
        public void ProcessCrossfading()
        {
            if (mCrossfadePose.IsFinish == true)
            {
                AnimCrossfading = null;
                return;
            }
            if (!AnimCrossfading.PerformanceFirst)
                PreviousState.TickAnimation();
            mCrossfadePose.Update();
            Animation.Runtime.CGfxAnimationRuntime.CopyPose(Pose, mCrossfadePose.OutPose);

        }
        public override void Tick()
        {
            base.Tick();
            if (AnimCrossfading != null)
            {
                ProcessCrossfading();
            }
            else
            {
                if (CurrentState != null)
                    Animation.Runtime.CGfxAnimationRuntime.CopyPose(Pose, CurrentState.Pose);
            }
        }
        public void TickNotify(GComponent component)
        {
            CurrentState.TickNotify(component);
        }
    }
}
