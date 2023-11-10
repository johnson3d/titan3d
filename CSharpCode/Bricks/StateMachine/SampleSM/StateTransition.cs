using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.SampleSM
{
    public class TtStateTransition<S, T> : ITransition<S, T>
    {
        public S CenterData { get; set; }
        public IState<S, T> From { get; set; } = null;
        public IState<S, T> To { get; set; } = null;
        public virtual void OnTransition() { }
        public virtual bool Check(in T context) { return true; }
    }
}
