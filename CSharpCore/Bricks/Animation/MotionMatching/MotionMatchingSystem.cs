using EngineNS.Bricks.Animation.AnimNode;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.MotionMatching
{
    public class MotionMatchingSystem
    {
        public static MotionMatchingSystem mMotionMatchingSystem;
        public static MotionMatchingSystem Instance
        {
            get
            {
                if (mMotionMatchingSystem == null)
                {
                    mMotionMatchingSystem = new MotionMatchingSystem();
                }
                return mMotionMatchingSystem;
            }
        }

        public MotionPose GetBestMatchingPose(MotionField field, MatchingGoal goal)
        {
            MotionPose bestMotionPose = null;
            float bestCost = float.MaxValue;
            //CGfxAnimationSequence beastAnim = null;
            for (int i = 0; i < field.MotionPoses.Count; ++i)
            {
                var cost = MatchingCost(field.MotionPoses[i], goal);
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestMotionPose = field.MotionPoses[i];
                }
            }

            //foreach (var anim in field.Motions)
            //{
            //    var skeleton = CEngine.Instance.SkeletonAssetManager.GetSkeleton(CEngine.Instance.RenderContext, anim.Value.SkeletonAssetName);
            //    var pose = skeleton.BoneTab.Clone();
            //    var cost = TrajectoryCost(anim.Value, goal, ref pose);
            //    if (cost < bestCost)
            //    {
            //        bestCost = cost;
            //        bestPose = pose;

            //        beastAnim = anim.Value;
            //    }
            //}
            //if (CurrentAnim == beastAnim)
            //{
            //    AnimTime += CEngine.Instance.EngineElapseTime;
            //    CurrentAnim.SkeletonAction.GetAnimaPose(AnimTime, ref bestPose);
            //}
            //else
            //{
            //    CurrentAnim = beastAnim;
            //    CurrentAnim.SkeletonAction.GetAnimaPose(0, ref bestPose);
            //    AnimTime = 0;
            //    EngineNS.Profiler.Log.WriteLine(Profiler.ELogTag.Info, "haha", CurrentAnim.Name.PureName());
            //}

            return bestMotionPose;
        }
        public float MatchingCost(MotionPose pose, MatchingGoal goal)
        {
            var trajectoryCost = TrajectoryCost(pose, goal);
            var feetPoseCose = FeetPoseCost(pose, goal);
            return trajectoryCost + feetPoseCose;
        }
        Pose.CGfxBonePose curLFootBone = null;
        Pose.CGfxBonePose curRFootBone = null;
        Pose.CGfxBonePose preLFootBone = null;
        Pose.CGfxBonePose preRFootBone = null;
        public float FeetPoseCost(MotionPose pose, MatchingGoal goal)
        {
            if (goal.CurrentPose == null)
                return 0;
            float cost = 0;
            if (curLFootBone == null)
                curLFootBone = goal.CurrentPose.FindBonePose("foot_l");
            if (curRFootBone == null)
                curRFootBone = goal.CurrentPose.FindBonePose("root_r");
            if (preLFootBone == null)
                preLFootBone = pose.AnimationPose.FindBonePose("foot_l");
            if (preRFootBone == null)
                preRFootBone = pose.AnimationPose.FindBonePose("foot_r");
            Skeleton.CGfxMotionState preData = new Skeleton.CGfxMotionState();
            Skeleton.CGfxMotionState curData = new Skeleton.CGfxMotionState();
            preLFootBone.GetMotionData(ref preData);
            preLFootBone.GetMotionData(ref curData);
            cost += (curData.Position - preData.Position).Length();
            cost += (curData.Velocity - preData.Velocity).Length();
            preRFootBone.GetMotionData(ref preData);
            preRFootBone.GetMotionData(ref curData);
            cost += (curData.Position - preData.Position).Length();
            cost += (curData.Velocity - preData.Velocity).Length();
            return cost;
        }
        int LoopCount = 10;
        public float TrajectoryCost(MotionPose pose, MatchingGoal goal)
        {
            float cost = 0;
            if (pose.Trajectory.Count < LoopCount)
                return float.MaxValue;
            var count = Math.Min(goal.Trajectory.Count, pose.Trajectory.Count);
            for (int i = 0; i < LoopCount; ++i)
            {
                var p = pose.Trajectory[i].Position;
                p.Y = 0;
                var hostLoc = goal.HostPlacement.Location;
                hostLoc.Y = goal.Trajectory[i].Y;
                cost += (goal.HostPlacement.Rotation * p + hostLoc - goal.Trajectory[i]).Length();
                if (i + 1 >= goal.Trajectory.Count)
                {
                    cost += pose.Trajectory[i].Velocity.Length();
                }
                else
                    cost += (goal.HostPlacement.Rotation * pose.Trajectory[i].Velocity - (goal.Trajectory[i + 1] - goal.Trajectory[i]) * 30).Length();
            }
            return cost;
        }
        public float TrajectoryCost(AnimNode.AnimationClip anim, MatchingGoal goal, ref Pose.CGfxSkeletonPose pose)
        {
            var duration = anim.Duration;
            var count = Math.Min(anim.Duration / 25, goal.Trajectory.Count);
            var skeleton = CEngine.Instance.SkeletonAssetManager.GetSkeleton(CEngine.Instance.RenderContext,RName.GetRName(anim.GetElementProperty(ElementPropertyType.EPT_Skeleton)));
            float cost = 0;

            for (int i = 0; i < count; ++i)
            {
                Skeleton.CGfxMotionState data = new Skeleton.CGfxMotionState();
                //anim.GetMotionData(i * 25, pose.Root.BoneDesc.NameHash, ref data);
                var p = data.Position;
                p.Y = 0;
                var hostLoc = goal.HostPlacement.Location;
                hostLoc.Y = goal.Trajectory[i].Y;
                cost += (goal.HostPlacement.Rotation * p + hostLoc - goal.Trajectory[i]).Length();
                //if(i + 1 >= goal.Trajectory.Length)
                //{
                //    cost += data.Velocity.Length();
                //}
                //else
                //    cost += (goal.HostPlacement.Rotation * data.Velocity - (goal.Trajectory[i + 1] - goal.Trajectory[i]) / 25 * 1000.0f).Length();
            }

            return cost;
        }
    }
}
