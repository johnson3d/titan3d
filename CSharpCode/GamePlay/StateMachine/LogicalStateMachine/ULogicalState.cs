using EngineNS.Bricks.StateMachine.TimedSM;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.StateMachine.LogicalStateMachine
{
    public class ULogicalState : UTimedState, ILogicalState
    {
        
        public ULogicalState(UGamePlayStateMachine context, string name = "LogicalState") : base(context, name)
        {
        }
    }
}
