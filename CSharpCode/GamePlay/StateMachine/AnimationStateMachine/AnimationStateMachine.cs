using EngineNS.Animation.BlendTree;
using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.StateMachine;
using EngineNS.Bricks.StateMachine.TimedSM;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.StateMachine.AnimationStateMachine
{
    public class TtAnimationStateMachine<T> : TtTimedStateMachine<T>, IAnimationState
    {
        

        public TtAnimationCommand<TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            throw new NotImplementedException();
        }

        public void Enter()
        {
            throw new NotImplementedException();
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }


        public bool ShouldUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
