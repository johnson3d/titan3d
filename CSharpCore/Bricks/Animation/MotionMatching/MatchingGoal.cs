using EngineNS;
using EngineNS.GamePlay.Component;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.MotionMatching
{
    public class MatchingGoal
    {
        public GPlacementComponent HostPlacement;
        public Support.NativeQueueForArray<Vector3> Trajectory;
        public Pose.CGfxSkeletonPose CurrentPose;
        public Support.NativeQueueForArray<Vector3> MacthingBones;
    }
}
