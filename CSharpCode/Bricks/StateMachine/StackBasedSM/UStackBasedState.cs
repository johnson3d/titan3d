using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.StackBasedSM
{
    public class StackBasedState : UState
    {
        public StackBasedState(UStackBasedStateMachine context, string name = "State") : base(context, name)
        {
        }
    }
}
