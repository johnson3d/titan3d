using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public partial class TtTimedStatesHub<T> : IStatesHub<T>
    {
        public string Name { get; set; } = "TimedStatesHub";
        protected TtTimedStateMachine<T> mStateMachine = null;
        protected List<ITransition<T>> mTransitions = new List<ITransition<T>>();
        public List<ITransition<T>> Transitions
        {
            get => mTransitions;
        }
        public bool AddTransition(ITransition<T> transition)
        {
            if (mTransitions.Contains(transition))
                return false;
            mTransitions.Add(transition);
            return true;
        }
        public bool RemoveTransition(ITransition<T> transition)
        {
            if (!mTransitions.Contains(transition))
                return false;
            mTransitions.Remove(transition);
            return true;
        }
        protected List<IAttachmentRule<T>> mAttachments = new List<IAttachmentRule<T>>();
        public List<IAttachmentRule<T>> Attachments
        {
            get => mAttachments;
        }
        public bool AddAttachment(IAttachmentRule<T> attachment)
        {
            if (mAttachments.Contains(attachment))
                return false;
            mAttachments.Add(attachment);
            return true;
        }

        public bool RemoveAttachment(IAttachmentRule<T> attachment)
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
                return (mStateMachine as TtTimedStateMachine<T>).Clock.TimeInSecond;
            }
        }
        public TtTimedStatesHub(TtTimedStateMachine<T> stateMachine, string name = "TimedStatesHub ")
        {


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
        public virtual void Tick(float elapseSecond, in T context)
        {
            if (!EnableTick)
                return;
        }
        public bool TryCheckTransitions(out List<ITransition<T>> transitions)
        {
            transitions = new List<ITransition<T>>();
            for (int i = 0; i < mTransitions.Count; ++i)
            {
                if (mTransitions[i].Check())
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

    public class TtTimedStatesHub : TtTimedStatesHub<FStateMachineContext>
    {
        public TtTimedStatesHub(TtTimedStateMachine<FStateMachineContext> stateMachine, string name = "TimedStatesHub ") : base(stateMachine, name)
        {
        }
    }
}
