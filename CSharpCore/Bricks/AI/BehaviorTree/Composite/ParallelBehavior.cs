using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.BehaviorTree.Composite
{
    public enum Policy
    {
        RequireOne,
        RequireAll,
    }
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Createable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class ParallelBehavior : CompositeBehavior
    {
        public Policy mSuccessPolicy;
        public Policy mFailurePolicy;
        public override BehaviorStatus Update(long timeElapse, GamePlay.Actor.GCenterData context)        {
            int iSuccessCount = 0, iFailureCount = 0;
            for (int i = 0; i < mChildrenList.Count; ++i)
            {
                var child = mChildrenList[i];
                if (!child.IsTerminated)
                {
                    child.Tick(timeElapse, context);
                }
                if (child.Status == BehaviorStatus.Success)
                {
                    ++iSuccessCount;
                    if (mSuccessPolicy == Policy.RequireOne)
                    {
                        return BehaviorStatus.Success;
                    }
                }
                if (child.Status == BehaviorStatus.Failure)
                {
                    ++iFailureCount;
                    if (mSuccessPolicy == Policy.RequireOne)
                    {
                        return BehaviorStatus.Failure;
                    }
                }
            }
            if (mFailurePolicy == Policy.RequireAll && iFailureCount == mChildrenList.Count)
            {
                return BehaviorStatus.Failure;
            }
            if (mSuccessPolicy == Policy.RequireAll && iFailureCount == mChildrenList.Count)
            {
                return BehaviorStatus.Success;
            }
            return BehaviorStatus.Running;        }
        public override void OnTerminate(BehaviorStatus status)
        {
            for (int i = 0; i < mChildrenList.Count; ++i)
            {
                if (mChildrenList[i].IsRunning)
                    mChildrenList[i].Abort();
            }
        }
    }
}
