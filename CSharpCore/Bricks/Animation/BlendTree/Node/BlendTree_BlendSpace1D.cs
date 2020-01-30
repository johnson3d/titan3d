using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Animation.BlendTree.Node
{
    public class BlendTree_BlendSpace1D : IBlendTree
    {
        public async static Task<BlendTree_BlendSpace1D> Create(RName name)
        {
            var bs = new BlendTree_BlendSpace1D();
            bs.BlendSpace = await Animation.AnimNode.BlendSpace1D.Create(name);
            return bs;
        }
        public static BlendTree_BlendSpace1D CreateSync(RName name)
        {
            var bs = new BlendTree_BlendSpace1D();
            bs.BlendSpace = Animation.AnimNode.BlendSpace1D.CreateSync(name);
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
                if (BlendSpace != null)
                    BlendSpace.Pose = mOutPose;
            }
        }
        public string SyncPlayPercentGrop { get; set; } = "";
        AnimNode.BlendSpace1D mBlendSpace = null;
        public AnimNode.BlendSpace1D BlendSpace
        {
            get => mBlendSpace;
            set
            {
                mBlendSpace = value;
            }
        }
        public void InitializePose(Pose.CGfxSkeletonPose pose)
        {
            OutPose = pose.Clone();
        }
        public void Evaluate(float timeInSecond)
        {
            if (EvaluateInput != null)
                mBlendSpace.Input = EvaluateInput.Invoke();
            if (SyncPlayPercentGrop != "")
            {
                mBlendSpace.CurrentTime = Bricks.Animation.AnimStateMachine.LogicAnimationStateMachine.GetSyncPlayPercent(SyncPlayPercentGrop);
            }
            mBlendSpace.Tick(CEngine.Instance.EngineElapseTimeSecond);
            if (SyncPlayPercentGrop != "")
            {
                AnimStateMachine.LogicAnimationStateMachine.UpdateSyncPlayPercent(SyncPlayPercentGrop, mBlendSpace.PlayPercent);
            }
        }
        public void Notifying(GamePlay.Component.GComponent component)
        {
            mBlendSpace.TickNofity(component);
        }
    }
}
