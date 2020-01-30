using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.BlendTree.Node
{
    public class BlendTree_CrossfadePose : IBlendTree
    {
        public Pose.CGfxSkeletonPose OutPose { get; set; }
        public IBlendTree FromNode { get; set; }
        public IBlendTree ToNode { get; set; }
        float mWeight = 0.0f;
        public float BlendTime { get; set; } = 0.1f;
        public bool BlendCompelete = false;
        float mCurrentTime = 0.0f; public void InitializePose(Pose.CGfxSkeletonPose pose)
        {
            OutPose = pose.Clone();
            FromNode.InitializePose(pose);
            ToNode.InitializePose(pose);
        }
        public void Evaluate(float timeInSecond)
        {
            if (FromNode == null && ToNode == null)
            {
                mCurrentTime += EngineNS.CEngine.Instance.EngineElapseTimeSecond;
                if (mCurrentTime > BlendTime)
                    mCurrentTime = BlendTime;
                return;
            }
            if (!BlendCompelete)
                FromNode?.Evaluate(timeInSecond);
            ToNode?.Evaluate(timeInSecond);
            if (FromNode != null && ToNode != null)
            {
                mWeight = (mCurrentTime) / BlendTime;
                if (mWeight == 1)
                {
                    Runtime.CGfxAnimationRuntime.CopyPose(OutPose, ToNode.OutPose);
                    BlendCompelete = true;
                }
                else
                {
                    Runtime.CGfxAnimationRuntime.BlendPose(OutPose, FromNode.OutPose, ToNode.OutPose, mWeight);
                }
            }
            else
            {
                if(ToNode == null)
                    Runtime.CGfxAnimationRuntime.CopyPose(OutPose, FromNode.OutPose);
                if (FromNode == null)
                    Runtime.CGfxAnimationRuntime.CopyPose(OutPose, ToNode.OutPose);
            }
            mCurrentTime += EngineNS.CEngine.Instance.EngineElapseTimeSecond;
            if (mCurrentTime > BlendTime)
                mCurrentTime = BlendTime;
        }
        public void Notifying(GamePlay.Component.GComponent component)
        {
            FromNode?.Notifying(component);
            ToNode?.Notifying(component);
        }
        public void ResetTime()
        {
            mCurrentTime = 0.0f;
            mWeight = 0.0f;
            BlendCompelete = false;
        }
    }
}
