using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtBlendTree_AdditiveBlendSpace1D : TtBlendTree<TtLPosMRotRuntimePose>
    {
        //TODO : UAdditiveBlendSpace1D

        //public Func<Vector3> EvaluateInput { get; set; } = null;
        //UAdditiveBlendSpace1D mAdditiveBlendSpace = null;
        //public UAdditiveBlendSpace1D AdditiveBlendSpace
        //{
        //    get => mAdditiveBlendSpace;
        //    set
        //    {
        //        mAdditiveBlendSpace = value;
        //    }
        //}
        //public string SyncPlayPercentGrop { get; set; } = "";
        //public override void InitializePose(ULPosMRotRuntimePose pose)
        //{
        //    mOutPose = URuntimePoseUtility.CopyPose(pose);
        //}
        //public override void Evaluate(float elapseSecond)
        //{
        //    if (EvaluateInput != null)
        //        mAdditiveBlendSpace.Input = EvaluateInput.Invoke();
        //    if (SyncPlayPercentGrop != "")
        //    {
        //        mAdditiveBlendSpace.CurrentTime = Bricks.Animation.AnimStateMachine.LogicAnimationStateMachine.GetSyncPlayPercent(SyncPlayPercentGrop);
        //    }
        //    mAdditiveBlendSpace.Tick(CEngine.Instance.EngineElapseTimeSecond);
        //    if (SyncPlayPercentGrop != "")
        //    {
        //        AnimStateMachine.LogicAnimationStateMachine.UpdateSyncPlayPercent(SyncPlayPercentGrop, mAdditiveBlendSpace.PlayPercent);
        //    }
        //}
        //public void Notifying(GamePlay.Component.GComponent component)
        //{
        //    mAdditiveBlendSpace.TickNofity(component);
        //}
    }
}
