using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.StackBasedSM
{
    public class TtStackBasedStateTransition<T> : ITransition<T>
    {
        public IState<T> From { get; set; }
        public IState<T> To { get; set; }  

        public bool Check()
        {
            return false;
        }

        public void OnTransition()
        {
        }
    }

}
