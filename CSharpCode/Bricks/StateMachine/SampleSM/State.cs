using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EngineNS.Bricks.StateMachine.SampleSM
{
    public class TtState<T> : IState<T>
    {
        public string Name { get; set; } = "State";
        protected TtStateMachine<T> mStateMachine = null;
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
        public TtState(TtStateMachine<T> stateMachine, string name = "State")
        {
            mStateMachine = stateMachine;
            Name = name;
        }
        public bool IsInitialized = false;
        public virtual void Initialize()
        {

        }
      
        public virtual void Enter()
        {
            if(!IsInitialized)
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
            if (!ShouldUpdate())
                return;
            Update(elapseSecond, context);
            foreach(var attachment in mAttachments)
            {
                if (attachment.Check())
                {
                    attachment.Tick(elapseSecond, context);
                }
            }
            for (int i = 0; i < mTransitions.Count; ++i)
            {
                if (mTransitions[i].Check())
                {
                    mStateMachine.TransitionTo(mTransitions[i]);
                    break;
                }
            }
        }
        public virtual void Update(float elapseSecond, in T context)
        {
            OnUpdate();
        }

        public virtual void OnEnter ()
        {

        }
        public virtual void OnExit()
        {

        }
        public virtual void OnUpdate()
        {

        }

        public bool TryCheckTransitions(out List<ITransition<T>> transitions)
        {
            throw new NotImplementedException();
        }
    }
}
