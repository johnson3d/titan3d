using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EngineNS.Bricks.StateMachine.SampleSM
{
    public class TtState<S, T> : IState<S, T>
    {
        public S CenterData { get; set; }
        public string Name { get; set; } = "State";
        protected TtStateMachine<S, T> mStateMachine = null;
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
        public TtState(TtStateMachine<S, T> stateMachine, string name = "State")
        {
            mStateMachine = stateMachine;
            Name = name;
        }
        public bool IsInitialized = false;
        public virtual async Thread.Async.TtTask<bool> Initialize(T context)
        {
            return false;
        }
      
        public virtual void Enter()
        {
            if(!IsInitialized)
            {
                
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
                //if (attachment.Check())?
                {
                    attachment.Tick(elapseSecond, context);
                }
            }
            for (int i = 0; i < mTransitions.Count; ++i)
            {
                if (mTransitions[i].Check(in context))
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

        public bool TryCheckTransitions(in T context, out List<ITransition<S, T>> transitions)
        {
            throw new NotImplementedException();
        }
    }
}
