using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public enum ETimedSM_ClockWrapMode
    {
        Clamp,
        Repeat,
        Forever,
    }
    public class TtTimedSMClock
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
        public ETimedSM_ClockWrapMode WrapMode
        {
            get; set;
        } = ETimedSM_ClockWrapMode.Repeat;
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
                    case ETimedSM_ClockWrapMode.Clamp:
                        {
                            mTime = mDuration;
                        }
                        break;
                    case ETimedSM_ClockWrapMode.Repeat:
                        {
                            mTime = 0;
                        }
                        break;
                    case ETimedSM_ClockWrapMode.Forever:
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

    public class TtTimedStateMachine<T> : IStateMachine<T>
    {
        public string Name { get; set; }
        public IState<T> PreState { get; set; } = null;
        public ITransition<T> PreTransition { get; set; } = null;
        public IState<T> CurrentState { get; set; }
        public ITransition<T> CurrentTransition { get; set; } = null;
        public IState<T> PostState { get; set; }
        public ITransition<T> PostTransition { get; set; } = null;
        public bool EnableTick { get; set; } = true;
        public EStateChangeMode StateChangeMode { get; set; } = EStateChangeMode.NextFrame;
        public TtTimedSMClock Clock { get; set; } = new TtTimedSMClock();
        public TtTimedStateMachine()
        {

        }
        public TtTimedStateMachine(string name)
        {
            Name = name;
        }
        public bool IsInitialized = false;
        public virtual bool Initialize()
        {
            return false;
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
            Clock.Advance(elapseSecond);
            Update(elapseSecond, context);
        }

        public void SetDefaultState(IState<T> state)
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

            if(CurrentState.TryCheckTransitions(out var transitions))
            {
                foreach (var transition in transitions)
                {
                    if(transition.To is IStatesHub<T> hub)
                    {
                        if(hub.TryCheckTransitions(out var hubTransitions))
                        {
                            TransitionTo(hubTransitions[0]);
                            break;
                        }
                    }
                    else
                    {
                        TransitionTo(transition);
                        break;
                    }
                }
            }
            //Immediately swap to post state current frame
            if (StateChangeMode == EStateChangeMode.Immediately && PostState != null)
            {
                SwapPostToCurrent();
                CurrentState.Tick(elapseSecond, context);
            }
        }
        public virtual void TransitionTo(IState<T> state, ITransition<T> transition)
        {
            System.Diagnostics.Debug.Assert(state == transition.To);
            PostState = state;
            PostTransition = transition;
        }
        public virtual void TransitionTo(ITransition<T> transition)
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

    public struct FStateMachineContext
    {
       
    }

    public class TtTimedStateMachine: TtTimedStateMachine<FStateMachineContext>
    {

    }

   
}
