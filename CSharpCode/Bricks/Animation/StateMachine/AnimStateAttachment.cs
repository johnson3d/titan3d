using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.StateMachine.TimedSM;
using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Animation.BlendTree;
using EngineNS.Animation.Command;

namespace EngineNS.Animation.StateMachine
{
    public class TtAnimStateAttachment<S> : Bricks.StateMachine.TimedSM.TtTimedStateAttachment<S>
    {
        public IBlendTree<TtLocalSpaceRuntimePose> Root = null;
        public override void Update(float elapseSecond, in TtStateMachineContext context)
        {
            base.Update(elapseSecond, context);
        }
    }
    
}
