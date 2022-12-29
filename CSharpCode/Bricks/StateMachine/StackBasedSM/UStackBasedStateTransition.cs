using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.StackBasedSM
{
    public class UStackBasedStateTransition : ITransition
    {
        public IState From { get; set; }
        public IState To { get; set; }  

        public bool Check()
        {
            return false;
        }

        public void OnTransition()
        {
        }
    }

}
