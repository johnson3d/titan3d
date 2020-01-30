using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.FSM
{
    public enum StateChangeMode
    {
        Immediately,
        NextFrame,
    }
    public class FiniteStateMachine
    {
        public string Name { get; set; }
        protected IState mPreviousState = null;
        public IState PreviousState { get => mPreviousState; }
        protected IState mCurrentState = null;
        public IState CurrentState { get => mCurrentState; }
        protected IState mPostState = null;
        public bool EnableTick { get; set; } = true;
        public StateChangeMode StateChangeMode { get; set; } = StateChangeMode.NextFrame;
        public FiniteStateMachine()
        {

        }
        public FiniteStateMachine(string name)
        {
            Name = name;
        }
        public bool IsInitialized = false;
        public virtual void Initialize()
        {

        }
        public virtual void Tick()
        {
            if (!IsInitialized)
            {
                Initialize();
                IsInitialized = true;
            }
            if (!EnableTick)
                return;
            SwapPostToCurrent();
            if (CurrentState == null)
                return;
            CurrentState.Tick();
        }
        public void SetCurrentState(IState state)
        {
            mPostState = state;
            if (StateChangeMode == StateChangeMode.Immediately)
            {
                SwapPostToCurrent();
                mCurrentState.Tick();
            }
        }
        public List<Action> OnTransitionEexecuteActions { get; set; } = null;
        public virtual void SetCurrentStateByTransition(Transition transition)
        {
            mPostState = transition.To;
            OnTransitionEexecuteActions = transition.OnTransitionEexecuteActions;
            if (StateChangeMode == StateChangeMode.Immediately)
            {
                SwapPostToCurrent();
                mCurrentState.Tick();
            }
        }
        public void SetCurrentStateImmediately(IState state)
        {
            mPreviousState = mCurrentState;
            mPreviousState?.Exit();
            mCurrentState = state;
            mPostState = null;
            CurrentState.Enter();
        }
        //一帧执行完了之后再更换状态
        protected virtual void SwapPostToCurrent()
        {
            if (mPostState == null)
                return;
            mPreviousState = mCurrentState;
            mPreviousState?.Exit();
            mCurrentState = mPostState;
            mPostState = null;
            CurrentState.Enter();
            OnStateChange();
            if (OnTransitionEexecuteActions != null)
            {
                for (int i = 0; i < OnTransitionEexecuteActions.Count; ++i)
                {
                    OnTransitionEexecuteActions[i]?.Invoke();
                }
            }
        }
        public virtual void OnStateChange()
        {

        }
    }


}
