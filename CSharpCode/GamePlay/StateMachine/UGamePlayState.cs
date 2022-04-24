using EngineNS.Animation.AnimationStateMachine;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.GamePlay.StateMachine.AnimationStateMachine;
using EngineNS.GamePlay.StateMachine.LogicalStateMachine;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.StateMachine
{
    public class UGamePlayState : UTimedState
    {
        public ILogicalState LogicalState { get; set; }
        public IAnimationState AnimationState { get; set; }
        public UGamePlayState(UGamePlayStateMachine context, string name = "GamePlayState") : base(context, name)
        {

        }
        public override void Tick(float elapseSecond)
        {
            LogicalState.Tick(elapseSecond);
            AnimationState.Tick(elapseSecond);
            base.Tick(elapseSecond);
        }

    }
}
