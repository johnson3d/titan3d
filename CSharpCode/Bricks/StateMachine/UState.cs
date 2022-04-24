using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine
{
    public class UState : IState 
    {
        public string Name { get; set; } = "State";
        protected UStateMachine mContext = null;
        protected List<UTransition> mTransitions = new List<UTransition>();
        public List<UTransition> Transitions
        {
            get => mTransitions;
        }
        public bool AddTransition(UTransition transition)
        {
            if (mTransitions.Contains(transition))
                return false;
            mTransitions.Add(transition);
            return true;
        }
        public bool RemoveTransition(UTransition transition)
        {
            if (!mTransitions.Contains(transition))
                return false;
            mTransitions.Remove(transition);
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
        public virtual void Tick(float elapseSecond)
        {
            if (!EnableTick)
                return;
            Update(elapseSecond);
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
