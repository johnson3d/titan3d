using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.BlendTree;
using EngineNS.Bricks.Animation.LogicAnim;
using EngineNS.Bricks.Animation.Pose;
using EngineNS.Bricks.FSM.TFSM;
using EngineNS.GamePlay.Component;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.AnimStateMachine
{
    public class LogicAnimationState : TimedState
    {
        public LogicAnimationStateMachine LAContext
        {
            get { return mContext as LogicAnimationStateMachine; }
        }
        IBlendTree mBlendTreeRoot = null;
        public IBlendTree BlendTreeRoot
        {
            get => mBlendTreeRoot;
            set
            {
                mBlendTreeRoot = value;
                mBlendTreeRoot.InitializePose(Pose);
            }
        }
        public CGfxSkeletonPose Pose { get; set; } = null;
        RName mRefAnimName = RName.EmptyName;
        public RName RefAnimation
        {
            get { return mRefAnimName; }
            set
            {
                mRefAnimName = value;
                if (value == null || value == RName.EmptyName || value.Name == "null")
                    return;
                var clip = LAAnimClip.CreateSync(value);
                if (clip == null)
                    return;
                clip.Bind(Pose.Clone());
                clip.StretchTime(Duration);
                clip.Seek(0);
                RefrenceClip = clip;
            }
        }
        RName mAnimationName = RName.EmptyName;
        public RName Animation
        {
            get { return mAnimationName; }
            set
            {
                mAnimationName = value;
                if (value == null || value == RName.EmptyName || value.Name == "null")
                    return;
                var clip = LAAnimClip.CreateSync(value);
                if (clip == null)
                    return;
                clip.Bind(Pose.Clone());
                clip.StretchTime(Duration);
                clip.Seek(0);
                AnimationClip = clip;
            }
        }
        public bool ResetTime { get; set; } = true;
        public float Duration
        {
            get => Clock.DurationInSecond;
            set
            {
                Clock.DurationInSecond = value;
            }
        }
        public LAAnimClip RefrenceClip { get; set; } = null;
        public LAAnimClip AnimationClip { get; set; } = null;
        public LogicAnimationStateMachine Context
        {
            get => mContext as LogicAnimationStateMachine;
        }
        public LogicAnimationState(LogicAnimationStateMachine stateContext, string name) : base(stateContext, name)
        {

        }
        public Action<LogicAnimationState> InitializeAction { get; set; } = null;
        public override void Initialize()
        {
            InitializeAction?.Invoke(this);
            base.Initialize();
        }
        public override void Enter()
        {
            if (ResetTime)
                Clock.Reste();
            base.Enter();
        }
        public override void Exit()
        {
            base.Exit();
        }
        public Action<float> UpdateAction { get; set; } = null;
        public void TickAnimation()
        {
            Clock.Advance();
            UpdateAnimation(Clock.TimeInSecond);
        }
        public void UpdateAnimation(float time)
        {
            if (BlendTreeRoot == null)
            {
                RefrenceClip?.Seek(time);
                AnimationClip?.Seek(time);
                if (RefrenceClip != null && AnimationClip != null)
                {
                    Bricks.Animation.Runtime.CGfxAnimationRuntime.MinusPose(Pose, RefrenceClip.OutPose, AnimationClip.OutPose);
                }
                else if (AnimationClip != null)
                {
                    Runtime.CGfxAnimationRuntime.CopyPose(Pose, AnimationClip.OutPose);
                }
                else
                {
                    if (Context.LayerType == AnimLayerType.Additive)
                        Runtime.CGfxAnimationRuntime.ZeroPose(Pose);
                    else
                        Runtime.CGfxAnimationRuntime.CopyPose(Pose, LAContext.Pose);
                }
            }
            else
            {
                BlendTreeRoot?.Evaluate(time);
                Runtime.CGfxAnimationRuntime.CopyPose(Pose, BlendTreeRoot.OutPose);
            }
        }
        public override void Update()
        {
            var time = Clock.TimeInSecond;
            UpdateAnimation(time);
            UpdateAction?.Invoke(time);
            base.Update();
        }
        public void TickNotify(GComponent component)
        {
            if (BlendTreeRoot == null)
            {
                RefrenceClip?.TickNotify(component);
                AnimationClip?.TickNotify(component);
              
            }
            else
            {
                BlendTreeRoot?.Notifying(component);
            }
        }
    }
}
