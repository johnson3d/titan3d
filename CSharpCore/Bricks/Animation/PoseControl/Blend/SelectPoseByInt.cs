using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.PoseControl.Blend
{
    public class PoseItemForBlend
    {
        public Pose.CGfxSkeletonPose Pose{ get; set; } = null;
        float mBlendTime = 0.1f;
        public float BlendTime
        {
            get
            {
                if (EvaluateBlendTimeFunc != null)
                    return EvaluateBlendTimeFunc.Invoke();
                return mBlendTime;
            }
        } 
        public Func<float> EvaluateBlendTimeFunc = null;
        public PoseItemForBlend(Pose.CGfxSkeletonPose pose,float blendTime)
        {
            Pose = pose;
            mBlendTime = blendTime;
        }
        public PoseItemForBlend(Pose.CGfxSkeletonPose pose, Func<float> blendTimeFunc)
        {
            Pose = pose;
            EvaluateBlendTimeFunc = blendTimeFunc;
        }

    }
    public class SelectPoseByInt :SkeletonPoseControl
    {
        public Dictionary<int, PoseItemForBlend> mPosesDic = new Dictionary<int, PoseItemForBlend>();
        int mLastSelected = 0;
        int mCurrentSelect = 0;
        public int CurrentSelect
        {
            get
            {
                return mCurrentSelect;
            }
            set
            {
                mLastSelected = mCurrentSelect;
                mCurrentSelect = value;
            }
        }
        public Func<int> EvaluateSelectedFunc = null;
        CrossfadePose mCrossfade = new CrossfadePose();
        public void Add(int index, PoseItemForBlend pose)
        {
            if(mPosesDic.ContainsKey(index))
            {
                mPosesDic[index] = pose;
            }
            else
            {
                mPosesDic.Add(index, pose);
            }
        }
        
        public override void Update()
        {
            if (EvaluateSelectedFunc != null)
                CurrentSelect = EvaluateSelectedFunc.Invoke();
            if (mCrossfade.OutPose == null)
                mCrossfade.OutPose = OutPose;
            if (mLastSelected != CurrentSelect)
            {
                mCrossfade.FromPose = mPosesDic[mLastSelected].Pose;
                mCrossfade.ToPose = mPosesDic[CurrentSelect].Pose;
                mCrossfade.BlendTime = mPosesDic[CurrentSelect].BlendTime;
                mCrossfade.ResetTime();
            }
            mCrossfade.Update();
            //Runtime.CGfxAnimationRuntime.CopyPose(OutPose, mPosesDic[CurrentSelect]);
        }
    }
}
