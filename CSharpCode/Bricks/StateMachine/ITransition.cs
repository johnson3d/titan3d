using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine
{
    public interface ITransition<S, T>
    {
        public S CenterData { get; set; }
        public IState<S, T> From { get; set; }
        public IState<S, T> To { get; set; }
        public void OnTransition();
        public bool Check(in T context);
    }
}
