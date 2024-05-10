using EngineNS.Animation.BlendTree;
using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.StateMachine.TimedSM;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.StateMachine
{
    public class TtGamePlayStateMachine<T> : TtTimedStateMachine<T>
    {
        public TtAnimationCommand<TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            return (CurrentState as TtGamePlayState<T>).AnimationState.ConstructAnimationCommandTree(parentNode, ref context);
        }
    }


}
