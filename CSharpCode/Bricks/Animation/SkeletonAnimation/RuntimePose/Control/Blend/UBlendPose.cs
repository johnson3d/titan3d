using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.SkeletonAnimation.Runtime.Control
{

    internal struct KeyPose
    {
        public float Weight;
        public Pose.ULocalSpaceRuntimePose Pose;
    }
    public class UBlendPose : URuntimePoseControl
    {
        public float Weight = 0.0f;
        List<KeyPose> mPosesList = new List<KeyPose>();
        public bool Add(float weight, Pose.ULocalSpaceRuntimePose pose)
        {
            var key = new KeyPose();
            key.Weight = weight;
            key.Pose = pose;
            mPosesList.Add(key);
            mPosesList.Sort();
            return true;
        }
        public bool Reomve(float time)
        {

            return true;
        }
        public override void Update(float elapseSecond)
        {
            for (int i = 0; i < mPosesList.Count - 1; ++i)
            {
                if (mPosesList[i].Weight >= Weight && mPosesList[i + 1].Weight <= Weight)
                {
                    var duration = mPosesList[i + 1].Weight - mPosesList[i].Weight;
                    var cur = Weight - mPosesList[i].Weight;
                    var realWeight = cur / duration;
                    URuntimePoseUtility.BlendPoses(ref mOutPose, mPosesList[i].Pose, mPosesList[i + 1].Pose, realWeight);
                    return;
                }
            }
        }
    }
}
