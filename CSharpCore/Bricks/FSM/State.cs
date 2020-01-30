using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.FSM
{
    public class State : IState
    {
        public string Name { get; set; } = "GState";
        protected FiniteStateMachine mContext = null;
        protected List<Transition> mTransitions = new List<Transition>();
        public List<Transition> Transitions
        {
            get => mTransitions;
        }
        public bool AddTransition(Transition transition)
        {
            if (mTransitions.Contains(transition))
                return false;
            mTransitions.Add(transition);
            return true;
        }
        public bool RemoveTransition(Transition transition)
        {
            if (!mTransitions.Contains(transition))
                return false;
            mTransitions.Remove(transition);
            return true;
        }
        public State(FiniteStateMachine context, string name = "State")
        {
            mContext = context;
            Name = name;
        }
        public bool IsInitialized = false;
        public virtual void Initialize()
        {

        }
        public Action OnEnter { get; set; } = null;
        public virtual void Enter()
        {
            if(!IsInitialized)
            {
                Initialize();
                IsInitialized = true;
            }
            OnEnter?.Invoke();
        }
        public Action OnExit { get; set; } = null;
        public virtual void Exit()
        {
            OnExit?.Invoke();
        }
        public bool EnableTick { get; set; } = true;
        public virtual void Tick()
        {
            if (!EnableTick)
                return;
            Update();
            for (int i = 0; i < mTransitions.Count; ++i)
            {
                if (mTransitions[i].CheckCondition())
                {
                    mContext.SetCurrentStateByTransition(mTransitions[i]);
                    break;
                }
            }
        }
        public Action OnUpdate { get; set; } = null;
        public virtual void Update()
        {
            OnUpdate?.Invoke();
        }

    }
}
