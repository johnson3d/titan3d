using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.PoseControl.Blend
{
    public class CrossfadePose : SkeletonPoseControl
    {
        public Pose.CGfxSkeletonPose FromPose { get; set; }
        public Pose.CGfxSkeletonPose ToPose { get; set; }
        float mWeight = 0.0f;
        public float BlendTime { get; set; } = 0.1f;
        float mCurrentTime = 0.0f;
        public bool IsFinish = true;
        public override void Update()
        {
            if (BlendTime == 0)
                mWeight = 1;
            else
                mWeight = (mCurrentTime) / BlendTime;
            if (mWeight == 1)
            {
                Runtime.CGfxAnimationRuntime.CopyPose(OutPose, ToPose);
                IsFinish = true;
            }
            else
            {
                Runtime.CGfxAnimationRuntime.BlendPose(OutPose, FromPose, ToPose, mWeight);
            }
            mCurrentTime += EngineNS.CEngine.Instance.EngineElapseTimeSecond;
            if (mCurrentTime > BlendTime)
                mCurrentTime = BlendTime;
        }
        public void ResetTime()
        {
            mCurrentTime = 0.0f;
            mWeight = 0.0f;
            IsFinish = false;
        }
    }
}
