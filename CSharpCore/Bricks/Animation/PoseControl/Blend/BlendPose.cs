using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.PoseControl.Blend
{

    internal struct KeyPose
    {
        public float Weight;
        public Pose.CGfxSkeletonPose Pose;
    }
    public class BlendPose : SkeletonPoseControl
    {
        public float Weight = 0.0f;
        List<KeyPose> mPosesList = new List<KeyPose>();
        public bool Add(float weight, Pose.CGfxSkeletonPose pose)
        {
            var key = new KeyPose();
            key.Weight = weight;
            key.Pose = pose;
            mPosesList.Add(key);
            mPosesList.Sort();
            return true;
        }
        public bool reomve(float time)
        {

            return true;
        }
        public override void Update()
        {
            for (int i = 0; i < mPosesList.Count - 1; ++i)
            {
                if (mPosesList[i].Weight >= Weight && mPosesList[i + 1].Weight <= Weight)
                {
                    var duration = mPosesList[i + 1].Weight - mPosesList[i].Weight;
                    var cur = Weight - mPosesList[i].Weight;
                    var realWeight = cur / duration;
                    Runtime.CGfxAnimationRuntime.BlendPose(OutPose, mPosesList[i].Pose, mPosesList[i + 1].Pose, realWeight);
                    return;
                }
            }
        }
    }
}
