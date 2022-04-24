using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;

namespace EngineNS.Animation.BlendTree.Node
{
    public class UBlendTree_AdditiveBlendSpace2D : UBlendTree<ULPosMRotRuntimePose>
    {
        //TODO : UAdditiveBlendSpace2D

        //UAdditiveBlendSpace2D mAdditiveBlendSpace = null;
        //public UAdditiveBlendSpace2D AdditiveBlendSpace
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
