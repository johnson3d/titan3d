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
    public interface IStateMachine<T>
    {
        public string Name { get; set; }
        public IState<T> PreState { get; set; }
        public ITransition<T> PreTransition { get; set; }
        public IState<T> CurrentState { get; set; }
        public ITransition<T> CurrentTransition { get; set; }
        public IState<T> PostState { get; set; }
        public ITransition<T> PostTransition { get; set; }
        public bool EnableTick { get; set; } 
        public EStateChangeMode StateChangeMode { get; set; } 
        public bool Initialize();
        public void Tick(float elapseSecond, in T context);

        public void SetDefaultState(IState<T> state);
        public void Update(float elapseSecond, in T context);
        public void TransitionTo(IState<T> state, ITransition<T> transition);
        public void TransitionTo(ITransition<T> transition);

        public void OnStateChange();
    }
}
