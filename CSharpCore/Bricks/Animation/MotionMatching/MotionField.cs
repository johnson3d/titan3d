using EngineNS.Bricks.Animation.AnimNode;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.MotionMatching
{
    public class MotionField
    {
        Dictionary<int, AnimationClip> mMotions = new Dictionary<int, AnimationClip>();
        public Dictionary<int, AnimationClip> Motions
        {
            get => mMotions;
        }

        List<MotionPose> mMotionPoses = new List<MotionPose>();
        public List<MotionPose> MotionPoses
        {
            get => mMotionPoses;
        }

        public void Add(AnimationClip clip)
        {
            var hashCode = clip.Name.GetHashCode();
            if (!mMotions.ContainsKey(hashCode))
            {
                mMotions.Add(hashCode, clip);
                AnalysisMotionPoses(clip);
            }
        }
        public void Remove(int hashCode)
        {
            if (mMotions.ContainsKey(hashCode))
            {
                mMotions.Remove(hashCode);
            }
        }
        public void AnalysisMotionPoses(AnimationClip clip)
        {
            
            var delta = (float)clip.DurationInMilliSecond / (float)clip.KeyFrames;
            for (int i = 0; i < clip.KeyFrames; ++i)
            {
                mMotionPoses.Add(MotionPose.CreateMotionPose(clip, (long)(i * delta)));
            }

        }
    }
}
