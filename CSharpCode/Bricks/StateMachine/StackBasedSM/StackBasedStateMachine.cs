using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.StackBasedSM
{
    public partial class TtStackBasedStateMachine<T> : IStateMachine<T>
    {
        protected Stack<TtStackBasedState<T>> StatesStack = new Stack<TtStackBasedState<T>>();
        public TtStackBasedStateMachine(string name)
        {

        }

        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ITransition<T> PreTransition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ITransition<T> CurrentTransition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ITransition<T> PostTransition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool EnableTick { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public EStateChangeMode StateChangeMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IState<T> PreState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IState<T> CurrentState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IState<T> PostState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Initialize()
        {
            throw new NotImplementedException();
        }

        public void OnStateChange()
        {
            throw new NotImplementedException();
        }

        public void SetDefaultState(IState<T> state)
        {
            throw new NotImplementedException();
        }

        public void Tick(float elapseSecond, in T context)
        {
            throw new NotImplementedException();
        }

        public void TransitionTo(IState<T> state, ITransition<T> transition)
        {
            throw new NotImplementedException();
        }

        public void TransitionTo(ITransition<T> transition)
        {
            throw new NotImplementedException();
        }

        public void Update(float elapseSecond, in T context)
        {
            throw new NotImplementedException();
        }
    }
}
