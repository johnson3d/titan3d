using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.SampleSM
{
    public class UStateMachine : IStateMachine
    {
        public string Name { get; set; }
        public IState PreState { get; protected set; } = null;
        public ITransition PreTransition { get; set; } = null;
        public IState CurrentState { get; protected set; }
        public ITransition CurrentTransition { get; set; } = null;
        public IState PostState { get; protected set; }
        public ITransition PostTransition { get; set; } = null;
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
            CurrentState = state;
        }
        public virtual void Update(float elapseSecond)
        {
            if (StateChangeMode == StateChangeMode.NextFrame && PostState != null)
            {
                SwapPostToCurrent();
            }

            if (CurrentState == null)
                return;
            CurrentState.Tick(elapseSecond);

            //Immediately swap to post state
            if (StateChangeMode == StateChangeMode.Immediately && PostState != null)
            {
                SwapPostToCurrent();
                CurrentState.Tick(elapseSecond);
            }
        }
        public virtual void TransitionTo(IState state , ITransition transition)
        {
            System.Diagnostics.Debug.Assert(state == transition.To);
            PostState = state;
            PostTransition = transition;
        }
        public virtual void TransitionTo(ITransition transition)
        {
            PostState = transition.To;
            PostTransition = transition;
        }

        //一帧执行完了之后再更换状态
        protected virtual void SwapPostToCurrent()
        {
            if (PostState == null)
                return;
            PreState = CurrentState;
            PreTransition = CurrentTransition;
            PreState?.Exit();

            CurrentState = PostState;
            CurrentTransition = PostTransition;

            PostState = null;
            PostTransition = null;

            CurrentState.Enter();
            OnStateChange();
        }
        public virtual void OnStateChange()
        {

        }
    }


}
