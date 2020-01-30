using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.FSM.SFSM
{
    public class StackBasedState : State
    {
        public StackBasedState(FiniteStateMachine context, string name = "State") : base(context, name)
        {
        }
    }
}
