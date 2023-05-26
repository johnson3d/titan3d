using EngineNS.Bricks.StateMachine.TimedSM;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.StateMachine.LogicalStateMachine
{
    public class TtLogicalState<T> : TtTimedState<T>, ILogicalState
    {
        
        public TtLogicalState(TtGamePlayStateMachine<T> stateMachine, string name = "LogicalState") : base(stateMachine, name)
        {
        }
    }
}
