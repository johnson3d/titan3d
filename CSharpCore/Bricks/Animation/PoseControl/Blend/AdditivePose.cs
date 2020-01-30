using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.PoseControl.Blend
{
    public class AdditivePose : SkeletonPoseControl
    {
        public Pose.CGfxSkeletonPose BasePose { get; set; }
        public Pose.CGfxSkeletonPose ReferencePose { get; set; }
        public Pose.CGfxSkeletonPose AddPose { get; set; }
        public float Alpha = 1.0f;
        public override void Update()
        {
            if (BasePose == null || ReferencePose == null || AddPose == null)
                return;
            var addPose = AddPose.Clone();
            var refPose = ReferencePose.Clone();
            var basePose = BasePose.Clone();
            var tempPose = BasePose.Clone();
            Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToMeshSpace(addPose);
            Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToMeshSpace(refPose);
            Bricks.Animation.Runtime.CGfxAnimationRuntime.MinusPose(tempPose, refPose, addPose);
            Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToMeshSpace(basePose);
            Bricks.Animation.Runtime.CGfxAnimationRuntime.AddPose(OutPose, basePose, tempPose, 1);
            Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToLocalSpace(OutPose);
        }
    }
}
