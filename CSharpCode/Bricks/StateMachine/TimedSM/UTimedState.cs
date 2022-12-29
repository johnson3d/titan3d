using EngineNS.Bricks.StateMachine.SampleSM;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public class UTimedState : IState
    {
        public string Name { get; set; } = "TimedState";
        protected UTimedStateMachine mContext = null;
        protected List<ITransition> mTransitions = new List<ITransition>();
        public List<ITransition> Transitions
        {
            get => mTransitions;
        }
        public bool AddTransition(ITransition transition)
        {
            if (mTransitions.Contains(transition))
                return false;
            mTransitions.Add(transition);
            return true;
        }
        public bool RemoveTransition(ITransition transition)
        {
            if (!mTransitions.Contains(transition))
                return false;
            mTransitions.Remove(transition);
            return true;
        }
        protected List<IAttachmentRule> mAttachments = new List<IAttachmentRule>();
        public List<IAttachmentRule> Attachments
        {
            get => mAttachments;
        }
        public bool AddAttachment(IAttachmentRule attachment)
        {
            if (mAttachments.Contains(attachment))
                return false;
            mAttachments.Add(attachment);
            return true;
        }

        public bool RemoveAttachment(IAttachmentRule attachment)
        {
            if (!mAttachments.Contains(attachment))
                return false;
            mAttachments.Remove(attachment);
            return true;
        }
        UTimedSMClock mClock = new UTimedSMClock();
        public TimedSM_ClockWrapMode WrapMode
        {
            get => mClock.WrapMode;
            set => mClock.WrapMode = value;
        }
        public float StateTime { get => mClock.TimeInSecond; }
        public float StateTimeDuration { get => mClock.DurationInSecond; set => mClock.DurationInSecond = value; }
        public float StateMachineTime
        {
            get
            {
                return (mContext as UTimedStateMachine).Clock.TimeInSecond;
            }
        }
        public UTimedState(UTimedStateMachine context,string name = "TimedState")
        {
            System.Diagnostics.Debug.Assert(context is UTimedStateMachine);

        }
        public bool IsInitialized = false;
        public virtual void Initialize()
        {

        }
        public virtual void Enter()
        {
            if (!IsInitialized)
            {
                Initialize();
                IsInitialized = true;
            }
            OnEnter();
        }

        public virtual void Exit()
        {
            OnExit();
        }
        public bool EnableTick { get; set; } = true;
        public bool ShouldUpdate()
        {
            return true;
        }
        public virtual void Tick(float elapseSecond)
        {
            if (!EnableTick)
                return;
            mClock.Advance(elapseSecond);
            if (!ShouldUpdate())
                return;
            Update(elapseSecond);
            foreach (var attachment in mAttachments)
            {
                if (attachment.Check())
                {
                    attachment.Tick(elapseSecond);
                }
            }
            for (int i = 0; i < mTransitions.Count; ++i)
            {
                if (mTransitions[i].Check())
                {
                    mContext.TransitionTo(mTransitions[i]);
                    break;
                }
            }
        }
        public virtual void Update(float elapseSecond)
        {
            OnUpdate();
        }
        public virtual void OnEnter()
        {

        }
        public virtual void OnExit()
        {

        }
        public virtual void OnUpdate()
        {

        }
    }
}
