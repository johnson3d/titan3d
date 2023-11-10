using EngineNS.Animation.AnimationStateMachine;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.GamePlay.StateMachine.AnimationStateMachine;
using EngineNS.GamePlay.StateMachine.LogicalStateMachine;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.StateMachine
{
    public class TtGamePlayState<T> : TtTimedState<T>
    {
        public ILogicalState LogicalState { get; set; }
        public IAnimationState AnimationState { get; set; }
        public TtGamePlayState(TtGamePlayStateMachine<T> context, string name = "GamePlayState") : base(context, name)
        {

        }
        public override void Tick(float elapseSecond, in TtStateMachineContext context)
        {
            //LogicalState.Tick(elapseSecond, context);
            //AnimationState.Tick(elapseSecond, context);
            base.Tick(elapseSecond, context);
        }

    }
}
