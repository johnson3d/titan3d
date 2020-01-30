using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.MotionMatching
{
    public class MotionPose
    {
        public static MotionPose CreateMotionPose(AnimNode.AnimationClip clip,long frameTime)
        {
            Pose.CGfxSkeletonPose animationPose = clip.GetAnimationSkeletonPose(frameTime);
            MotionPose motionPose = new MotionPose(clip.Name.GetHashCode(),frameTime,animationPose);
            motionPose.CalculateTrajectory(clip);
            return motionPose;
        }
        public int HashName { get;private set; } = 0;
        public long PlayTime { get; private set; } = 0;
        public long PredictTime { get; set; } = 1*1000;
        public long PredictionDelatTime { get; set; } = (long)((1f / 20f )* 1000);

        Pose.CGfxSkeletonPose mAnimationPose = null;
        public Pose.CGfxSkeletonPose AnimationPose
        {
            get => mAnimationPose;
        }
        List<TrajectoryPoint> mTrajectory = new List<TrajectoryPoint>();
        public List<TrajectoryPoint> Trajectory
        {
            get => mTrajectory;
        }
        public MotionPose(int hashName,long time,Pose.CGfxSkeletonPose animationPose)
        {
            HashName = hashName;
            PlayTime = time;
            mAnimationPose = animationPose;
        }
        public void CalculateTrajectory(AnimNode.AnimationClip clip)
        {
            Skeleton.CGfxMotionState data = new Skeleton.CGfxMotionState();
            Skeleton.CGfxMotionState playTimeData = new Skeleton.CGfxMotionState();
            long totalTime = 0;
            long curTime = PlayTime;
            var pose = clip.GetAnimationSkeletonPose(curTime * 0.001f);
            playTimeData = pose.Root.MotionData;
            while (totalTime <PredictTime && curTime < clip.DurationInMilliSecond)
            {
                var predictPose = clip.GetAnimationSkeletonPose(curTime * 0.001f);
                data = predictPose.Root.MotionData;
                TrajectoryPoint point = new TrajectoryPoint(totalTime,data.Position - playTimeData.Position, data.Velocity);
                mTrajectory.Add(point);
                totalTime += PredictionDelatTime;
                curTime = PlayTime +totalTime;
            }
        }
    }
}
