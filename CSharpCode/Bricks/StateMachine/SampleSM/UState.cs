using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EngineNS.Bricks.StateMachine.SampleSM
{
    public class UState : IState 
    {
        public string Name { get; set; } = "State";
        protected UStateMachine mContext = null;
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
        public UState(UStateMachine context, string name = "State")
        {
            mContext = context;
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
        public virtual void Tick(float elapseSecond)
        {
            if (!EnableTick)
                return;
            if (!ShouldUpdate())
                return;
            Update(elapseSecond);
            foreach(var attachment in mAttachments)
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

        public virtual void OnEnter ()
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
