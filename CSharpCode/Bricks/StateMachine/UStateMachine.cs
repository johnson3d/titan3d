using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine
{
    public enum StateChangeMode
    {
        Immediately,
        NextFrame,
    }

    public class UStateMachine
    {
        public string Name { get; set; }
        protected IState mPreState = null;
        public IState PreState { get => mPreState; }
        public UTransition PreTransition { get; set; } = null;
        protected IState mCurrentState = null;
        public IState CurrentState { get => mCurrentState; }
        public UTransition CurrentTransition { get; set; } = null;
        protected IState mPostState = null;
        public UTransition PostTransition { get; set; } = null;
        public bool EnableTick { get; set; } = true;
        public StateChangeMode StateChangeMode { get; set; } = StateChangeMode.NextFrame;
        public UStateMachine()
        {

        }
        public UStateMachine(string name)
        {
            Name = name;
        }
        public bool IsInitialized = false;
        public virtual void Initialize()
        {

        }
        public virtual void Tick(float elapseSecond)
        {
            if (!EnableTick)
                return;

            if (!IsInitialized)
            {
                Initialize();
                IsInitialized = true;
            }

            Update(elapseSecond);
        }

        public void SetDefaultState(IState state)
        {
            mCurrentState = state;
        }
        public virtual void Update(float elapseSecond)
        {
            if (StateChangeMode == StateChangeMode.NextFrame && mPostState != null)
            {
                SwapPostToCurrent();
            }

            if (CurrentState == null)
                return;
            CurrentState.Tick(elapseSecond);

            //Immediately swap to post state
            if (StateChangeMode == StateChangeMode.Immediately && mPostState != null)
            {
                SwapPostToCurrent();
                mCurrentState.Tick(elapseSecond);
            }
        }
        public virtual void TransitionTo(IState state ,UTransition transition)
        {
            System.Diagnostics.Debug.Assert(state == transition.To);
            mPostState = state;
            PostTransition = transition;
        }
        public virtual void TransitionTo(UTransition transition)
        {
            mPostState = transition.To;
            PostTransition = transition;
        }

        //一帧执行完了之后再更换状态
        protected virtual void SwapPostToCurrent()
        {
            if (mPostState == null)
                return;
            mPreState = mCurrentState;
            PreTransition = CurrentTransition;
            mPreState?.Exit();

            mCurrentState = mPostState;
            CurrentTransition = PostTransition;

            mPostState = null;
            PostTransition = null;

            CurrentState.Enter();
            OnStateChange();
        }
        public virtual void OnStateChange()
        {

        }
    }


}
