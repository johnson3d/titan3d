using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace EngineNS.Bricks.AI.BehaviorTree.Composite
{
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Createable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class SelectBehavior : CompositeBehavior
    {
        protected Behavior mRunningChild;
        protected int mIndex = 0;
        public override bool Schedule(Behavior behavior)
        {
            var bp = behavior.Priority;
            if (Priority > bp)
                return false;
            for (int i = 0; i < mChildrenList.Count; ++i)
            {
                if (mChildrenList[i].Priority <= bp)
                {
                    if (mChildrenList[i].Schedule(behavior))
                    {
                        mRunningChild = mChildrenList[i];
                        mIndex = i;
                        return true;
                    }
                }
                else
                    return false;
            }
            return false;
        }
        public override BehaviorStatus Update(long timeElapse, GamePlay.Actor.GCenterData context)
        {
            if (mRunningChild != null)
            {
                var status = mRunningChild.Tick(timeElapse, context);
                if (status != BehaviorStatus.Running)
                {
                    mRunningChild = null;
                }
                if (status != BehaviorStatus.Failure)
                {
                    return status;
                }
                if (status == BehaviorStatus.Aborted)
                {
                    return BehaviorStatus.Aborted;
                }
            }
            else
            {
                if (mIndex >= mChildrenList.Count)
                    mIndex = 0;
                if (mIndex < mChildrenList.Count)
                {
                    var child = mChildrenList[mIndex];
                    var status = child.Tick(timeElapse, context);
                    if (status == BehaviorStatus.Running)
                    {
                        mRunningChild = child;
                    }
                    else
                    {
                        mRunningChild = null;
                    }
                    mIndex++;
                    if (status != BehaviorStatus.Failure)
                    {
                        return status;
                    }
                    if (status == BehaviorStatus.Aborted)
                    {
                        return BehaviorStatus.Aborted;
                    }
                }
            }
            return BehaviorStatus.Failure;
        }
        public override void Reset()
        {
            mRunningChild = null;
            mIndex = 0;
            base.Reset(); 
        }
    }
}
