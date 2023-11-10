using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.StackBasedSM
{
    public partial class TtStackBasedStateMachine<S, T> : IStateMachine<S, T>
    {
        public S CenterData { get; set; }
        protected Stack<TtStackBasedState<S, T>> StatesStack = new Stack<TtStackBasedState<S, T>>();
        public TtStackBasedStateMachine(string name)
        {

        }

        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ITransition<S, T> PreTransition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ITransition<S, T> CurrentTransition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ITransition<S, T> PostTransition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool EnableTick { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public EStateChangeMode StateChangeMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IState<S, T> PreState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IState<S, T> CurrentState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IState<S, T> PostState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Initialize()
        {
            throw new NotImplementedException();
        }

        public void OnStateChange()
        {
            throw new NotImplementedException();
        }

        public void SetDefaultState(IState<S, T> state)
        {
            throw new NotImplementedException();
        }

        public void Tick(float elapseSecond, in T context)
        {
            throw new NotImplementedException();
        }

        public void TransitionTo(IState<S, T> state, ITransition<S, T> transition)
        {
            throw new NotImplementedException();
        }

        public void TransitionTo(ITransition<S, T> transition)
        {
            throw new NotImplementedException();
        }

        public void Update(float elapseSecond, in T context)
        {
            throw new NotImplementedException();
        }
    }
}
