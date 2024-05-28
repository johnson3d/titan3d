using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.StateMachine
{
    public class TtAnimCompoundState<S> : Bricks.StateMachine.TimedSM.TtTimedCompoundState<S>
    {
        public TtLocalSpaceRuntimePose OutPose { get; set; }
    }
}
