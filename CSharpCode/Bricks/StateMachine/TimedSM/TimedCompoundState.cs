using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public partial class TtTimedCompoundState<S, T> : ICompoundStates<S, T>
    {
        public S CenterData { get; set; }
        public string Name { get; set; } = "TimedCompoundState";
        protected TtTimedStateMachine<S, T> mStateMachine = null;
        public TtTimedStateMachine<S, T> StateMachine { get=>mStateMachine; set=> mStateMachine = value; }
        protected List<ITransition<S, T>> mTransitions = new List<ITransition<S, T>>();
        public List<ITransition<S, T>> Transitions
        {
            get => mTransitions;
        }
        public bool AddTransition(ITransition<S, T> transition)
        {
            if (mTransitions.Contains(transition))
                return false;
            mTransitions.Add(transition);
            return true;
        }
        public bool RemoveTransition(ITransition<S, T> transition)
        {
            if (!mTransitions.Contains(transition))
                return false;
            mTransitions.Remove(transition);
            return true;
        }
        protected List<IAttachment<S, T>> mAttachments = new List<IAttachment<S, T>>();
        public List<IAttachment<S, T>> Attachments
        {
            get => mAttachments;
        }
        public bool AddAttachment(IAttachment<S, T> attachment)
        {
            if (mAttachments.Contains(attachment))
                return false;
            mAttachments.Add(attachment);
            return true;
        }

        public bool RemoveAttachment(IAttachment<S, T> attachment)
        {
            if (!mAttachments.Contains(attachment))
                return false;
            mAttachments.Remove(attachment);
            return true;
        }
       public float StateMachineTime
        {
            get
            {
                return (mStateMachine as TtTimedStateMachine<S, T>).Clock.TimeInSecond;
            }
        }
        public TtTimedCompoundState(string name = "TimedCompoundState ")
        {


        }
        public TtTimedCompoundState(TtTimedStateMachine<S, T> stateMachine, string name = "TimedCompoundState ")
        {


        }
        public bool IsInitialized = false;
        public virtual async Thread.Async.TtTask<bool> Initialize(T context)
        {
            return false;
        }
        public virtual void Enter()
        {
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
        public virtual void Tick(float elapseSecond, in T context)
        {
            if (!EnableTick)
                return;
        }
        public bool TryCheckTransitions(in T context, out List<ITransition<S, T>> transitions)
        {
            transitions = new List<ITransition<S, T>>();
            for (int i = 0; i < mTransitions.Count; ++i)
            {
                if (mTransitions[i].Check(in context))
                {
                    transitions.Add(mTransitions[i]);
                }
            }
            if (transitions.Count > 0)
                return true;
            return false;
        }
        public virtual void Update(float elapseSecond, in T context)
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

    public class TtTimedCompoundState<S> : TtTimedCompoundState<S,TtStateMachineContext>
    {
        public TtTimedCompoundState(string name = "TimedCompoundState ") : base(name)
        {


        }
        public TtTimedCompoundState(TtTimedStateMachine<S, TtStateMachineContext> stateMachine, string name = "TimedCompoundState ") : base(stateMachine, name)
        {
        }
    }
    public class TtTimedCompoundState : TtTimedCompoundState<TtDefaultCenterData, TtStateMachineContext>
    {
        public TtTimedCompoundState(string name = "TimedCompoundState ") : base(name)
        {


        }
        public TtTimedCompoundState(TtTimedStateMachine<TtDefaultCenterData, TtStateMachineContext> stateMachine, string name = "TimedCompoundState ") : base(stateMachine, name)
        {
        }
    }
}
