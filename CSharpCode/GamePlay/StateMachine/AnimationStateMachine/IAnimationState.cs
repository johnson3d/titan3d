using EngineNS.Animation.BlendTree;
using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.StateMachine;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.StateMachine.AnimationStateMachine
{
    public interface IAnimationState //: IState
    {
        TtAnimationCommand<TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context);
    }
}
