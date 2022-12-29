using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.StackBasedSM
{
    public class UStackBasedStateMachine : IStateMachine
    {
        protected Stack<StackBasedState> StatesStack = new Stack<StackBasedState>();
        public UStackBasedStateMachine(string name)
        {

        }
    }
}
