using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Pipeline.Command
{
    public class UBlendSpaceEvaluateCommand : IAnimationCommand
    {
        // Array??
        public List<UPropertiesSettingCommand> AnimCmds { get; set; } = new List<UPropertiesSettingCommand>();
        public List<UAnimatableSkeletonPose> AnimPoses { get; set; } = new List<UAnimatableSkeletonPose>();
        public List<ULocalSpaceRuntimePose> LocalSpaceRuntimePoses { get; set; } = new List<ULocalSpaceRuntimePose>();
        public List<float> Weight = new List<float>();

        public ULocalSpaceRuntimePose FinalPose;
        public void Execute()
        {
            if (AnimCmds.Count == 0)
                return;
            for(int i = 0; i< AnimCmds.Count; ++i)
            {
                AnimCmds[i].Execute();
                var locaPose = LocalSpaceRuntimePoses[i];
                URuntimePoseUtility.CopyPose(ref locaPose, AnimPoses[i]);
            }

            if (AnimCmds.Count == 1)
            {
               URuntimePoseUtility.CopyPose(ref FinalPose, LocalSpaceRuntimePoses[0]);
            }
            if (AnimCmds.Count == 2)
            {
                URuntimePoseUtility.BlendPoses(ref FinalPose, LocalSpaceRuntimePoses[0], LocalSpaceRuntimePoses[1], Weight[1]);
            }
            if (AnimCmds.Count > 2)
            {
                float totalWeigth = Weight[0] + Weight[1];
                float bWeight = Weight[1] / totalWeigth;
                URuntimePoseUtility.BlendPoses(ref FinalPose, LocalSpaceRuntimePoses[0], LocalSpaceRuntimePoses[1], bWeight);
                for (int i = 2; i < AnimCmds.Count; ++i)
                {
                    totalWeigth += Weight[i];
                    bWeight = Weight[i] / totalWeigth;
                    URuntimePoseUtility.BlendPoses(ref FinalPose, FinalPose, LocalSpaceRuntimePoses[i], bWeight);
                }
            }
        }
    }
}
