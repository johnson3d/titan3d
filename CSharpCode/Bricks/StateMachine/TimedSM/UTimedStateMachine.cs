using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public enum TimedSM_ClockWrapMode
    {
        Clamp,
        Repeat,
        Forever,
    }
    public class UTimedSMClock
    {
        float mTime = 0;
        public float TimeInSecond
        {
            get => mTime;
        }
        float mDuration = 1;
        public float DurationInSecond
        {
            get => mDuration;
            set => mDuration = value;
        }
        public TimedSM_ClockWrapMode WrapMode
        {
            get; set;
        } = TimedSM_ClockWrapMode.Repeat;
        public float TimeSacle { get; set; } = 1.0f;
        public bool Pause { get; set; } = false;

        public void Advance(float elapseTime)
        {
            if (Pause)
                return;
            mTime += elapseTime * TimeSacle;
            if (mTime > mDuration)
            {
                switch (WrapMode)
                {
                    case TimedSM_ClockWrapMode.Clamp:
                        {
                            mTime = mDuration;
                        }
                        break;
                    case TimedSM_ClockWrapMode.Repeat:
                        {
                            mTime = 0;
                        }
                        break;
                    case TimedSM_ClockWrapMode.Forever:
                        {
                            //just continue time++
                        }
                        break;
                }
            }
        }
        public void Reste()
        {
            mTime = 0;
        }
    }

    public class UTimedStateMachine : IStateMachine
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
        public UTimedSMClock Clock { get; protected set; } = new UTimedSMClock();
        public UTimedStateMachine()
        {

        }
        public UTimedStateMachine(string name)
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
            Clock.Advance(elapseSecond);
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
        public virtual void TransitionTo(IState state, ITransition transition)
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
