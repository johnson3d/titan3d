using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.StackBasedSM
{
    public class TtStackBasedStateTransition<S, T> : ITransition<S, T>
    {
        public S CenterData { get; set; }
        public IState<S, T> From { get; set; }
        public IState<S, T> To { get; set; }  

        public bool Check(in T context)
        {
            return false;
        }

        public void OnTransition()
        {
        }
    }

}
