using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.SampleSM
{
    public class TtStateTransition<T> : ITransition<T>
    {
        public IState<T> From { get; set; } = null;
        public IState<T> To { get; set; } = null;
        public virtual void OnTransition() { }
        public virtual bool Check() { return true; }
    }
}
