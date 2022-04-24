using EngineNS.Animation.BlendTree;
using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.StateMachine.TimedSM;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.StateMachine
{
    public class UGamePlayStateMachine : UTimedStateMachine
    {
        public UAnimationCommand<ULocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            return (CurrentState as UGamePlayState).AnimationState.ConstructAnimationCommandTree(parentNode, ref context);
        }
    }


}
