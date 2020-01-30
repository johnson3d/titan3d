using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Animation.BlendTree.Node
{
    public class BlendTree_AdditiveBlendSpace1D : IBlendTree
    {
        public async static Task<BlendTree_AdditiveBlendSpace1D> Create(RName name)
        {
            var bs = new BlendTree_AdditiveBlendSpace1D();
            bs.AdditiveBlendSpace = await Animation.AnimNode.AdditiveBlendSpace1D.Create(name);
            return bs;
        }
        public static BlendTree_AdditiveBlendSpace1D CreateSync(RName name)
        {
            var bs = new BlendTree_AdditiveBlendSpace1D();
            bs.AdditiveBlendSpace = Animation.AnimNode.AdditiveBlendSpace1D.CreateSync(name);
            return bs;
        }
        public Func<Vector3> EvaluateInput { get; set; } = null;
        protected Pose.CGfxSkeletonPose mOutPose = null;
        public Pose.CGfxSkeletonPose OutPose
        {
            get => mOutPose;
            set
            {
                mOutPose = value;
                if (AdditiveBlendSpace != null)
                    AdditiveBlendSpace.Pose = mOutPose;
            }
        }
        AnimNode.AdditiveBlendSpace1D mAdditiveBlendSpace = null;
        public AnimNode.AdditiveBlendSpace1D AdditiveBlendSpace
        {
            get => mAdditiveBlendSpace;
            set
            {
                mAdditiveBlendSpace = value;
            }
        }
        public string SyncPlayPercentGrop { get; set; } = "";
        public void InitializePose(Pose.CGfxSkeletonPose pose)
        {
            OutPose = pose.Clone();
        }
        public void Evaluate(float timeInSecond)
        {
            if (EvaluateInput != null)
                mAdditiveBlendSpace.Input = EvaluateInput.Invoke();
            if (SyncPlayPercentGrop != "")
            {
                mAdditiveBlendSpace.CurrentTime = Bricks.Animation.AnimStateMachine.LogicAnimationStateMachine.GetSyncPlayPercent(SyncPlayPercentGrop);
            }
            mAdditiveBlendSpace.Tick(CEngine.Instance.EngineElapseTimeSecond);
            if (SyncPlayPercentGrop != "")
            {
                AnimStateMachine.LogicAnimationStateMachine.UpdateSyncPlayPercent(SyncPlayPercentGrop, mAdditiveBlendSpace.PlayPercent);
            }
        }
        public void Notifying(GamePlay.Component.GComponent component)
        {
            mAdditiveBlendSpace.TickNofity(component);
        }
    }
}
