using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.BlendTree.Node
{
    public class BlendTree_PoseItemForBlend
    {
        public IBlendTree PoseNode { get; set; } = null;
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
        public Func<float> EvaluateBlendTimeFunc { get; set; } = null;
        public BlendTree_PoseItemForBlend()
        {

        }
        public BlendTree_PoseItemForBlend(IBlendTree poseNode, float blendTime)
        {
            PoseNode = poseNode;
            mBlendTime = blendTime;
        }
        public BlendTree_PoseItemForBlend(IBlendTree poseNode, Func<float> blendTimeFunc)
        {
            PoseNode = poseNode;
            EvaluateBlendTimeFunc = blendTimeFunc;
        }

    }
    public class BlendTree_SelectPoseByInt : IBlendTree
    {
        public Pose.CGfxSkeletonPose OutPose { get; set; }
        public Dictionary<int, BlendTree_PoseItemForBlend> mPosesDic = new Dictionary<int, BlendTree_PoseItemForBlend>();
        int mLastSelected = 0;
        int mCurrentSelect = -1;
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
        public void Add(int index, BlendTree_PoseItemForBlend pose)
        {
            if (mPosesDic.ContainsKey(index))
            {
                mPosesDic[index] = pose;
            }
            else
            {
                mPosesDic.Add(index, pose);
            }
        }
        BlendTree_CrossfadePose mCrossfade = new BlendTree_CrossfadePose();
        public Func<int> EvaluateSelectedFunc { get; set; } = null;
        public void InitializePose(Pose.CGfxSkeletonPose pose)
        {
            OutPose = pose.Clone();
            mCrossfade.OutPose = OutPose;
            using (var it = mPosesDic.GetEnumerator())
            {
                while(it.MoveNext())
                {
                    it.Current.Value.PoseNode.InitializePose(pose);
                }
            }
        }
        public void Evaluate(float timeInSecond)
        {
            if (EvaluateSelectedFunc != null)
                CurrentSelect = EvaluateSelectedFunc.Invoke();
            if (!mPosesDic.ContainsKey(CurrentSelect))
            {
                CurrentSelect = -1;
            }
            if (mLastSelected != CurrentSelect)
            {
                if (mPosesDic.ContainsKey(mLastSelected))
                    mCrossfade.FromNode = mPosesDic[mLastSelected].PoseNode;
                if (mPosesDic.ContainsKey(CurrentSelect))
                {
                    mCrossfade.ToNode = mPosesDic[CurrentSelect].PoseNode;
                    mCrossfade.BlendTime = mPosesDic[CurrentSelect].BlendTime;
                }
                mCrossfade.ResetTime();
            }
            mCrossfade.Evaluate(timeInSecond);
        }
        public void Notifying(GamePlay.Component.GComponent component)
        {
            mCrossfade.ToNode?.Notifying(component);
        }
    }
}
