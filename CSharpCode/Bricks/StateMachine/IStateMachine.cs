using EngineNS.Bricks.StateMachine.TimedSM;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine
{
    public enum EStateChangeMode
    {
        Immediately,
        NextFrame,
    }
    public interface IStateMachine<S,T>
    {
        public S CenterData { get; set; }
        public string Name { get; set; }
        public IState<S, T> PreState { get; set; }
        public ITransition<S, T> PreTransition { get; set; }
        public IState<S, T> CurrentState { get; set; }
        public ITransition<S, T> CurrentTransition { get; set; }
        public IState<S, T> PostState { get; set; }
        public ITransition<S, T> PostTransition { get; set; }
        public bool EnableTick { get; set; } 
        public EStateChangeMode StateChangeMode { get; set; } 
        public bool Initialize();
        public void Tick(float elapseSecond, in T context);

        public void SetDefaultState(IState<S, T> state);
        public void Update(float elapseSecond, in T context);
        public void TransitionTo(IState<S, T> state, ITransition<S, T> transition);
        public void TransitionTo(ITransition<S, T> transition);

        public void OnStateChange();
    }
}
