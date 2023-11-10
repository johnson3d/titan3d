using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.SampleSM
{
    public class TtStateMachine<S, T> : IStateMachine<S, T>
    {
        public S CenterData { get; set; }
        public string Name { get; set; }
        public IState<S, T> PreState { get; set; } = null;
        public ITransition<S, T> PreTransition { get; set; } = null;
        public IState<S, T> CurrentState { get; set; }
        public ITransition<S, T> CurrentTransition { get; set; } = null;
        public IState<S, T> PostState { get; set; }
        public ITransition<S, T> PostTransition { get; set; } = null;
        public bool EnableTick { get; set; } = true;
        public EStateChangeMode StateChangeMode { get; set; } = EStateChangeMode.NextFrame;
        public TtStateMachine()
        {

        }
        public TtStateMachine(string name)
        {
            Name = name;
        }
        public bool IsInitialized = false;
        public virtual bool Initialize()
        {
            throw new NotImplementedException();
        }
        public virtual void Tick(float elapseSecond, in T context)
        {
            if (!EnableTick)
                return;

            if (!IsInitialized)
            {
                Initialize();
                IsInitialized = true;
            }

            Update(elapseSecond, context);
        }

        public void SetDefaultState(IState<S, T> state)
        {
            CurrentState = state;
        }
        public virtual void Update(float elapseSecond, in T context)
        {
            if (StateChangeMode == EStateChangeMode.NextFrame && PostState != null)
            {
                SwapPostToCurrent();
            }

            if (CurrentState == null)
                return;
            CurrentState.Tick(elapseSecond, context);

            //Immediately swap to post state
            if (StateChangeMode == EStateChangeMode.Immediately && PostState != null)
            {
                SwapPostToCurrent();
                CurrentState.Tick(elapseSecond, context);
            }
        }
        public virtual void TransitionTo(IState<S, T> state , ITransition<S, T> transition)
        {
            System.Diagnostics.Debug.Assert(state == transition.To);
            PostState = state;
            PostTransition = transition;
        }
        public virtual void TransitionTo(ITransition<S, T> transition)
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
