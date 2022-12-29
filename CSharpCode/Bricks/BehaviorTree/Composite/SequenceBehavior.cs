using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EngineNS.BehaviorTree.Composite
{
    //[Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Createable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class SequenceBehavior : CompositeBehavior
    {
        protected Behavior mRunningChild;
        protected int mIndex = 0;
        public override void OnInitialize()
        {
            base.OnInitialize();
        }
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
        public override BehaviorStatus Update(long timeElapse, GamePlay.UCenterData context)
        {
            if (mRunningChild != null)
            {
                var status = mRunningChild.Tick(timeElapse, context);
                if (status != BehaviorStatus.Running)
                {
                    mRunningChild = null;
                }
                if (status != BehaviorStatus.Success)
                {
    
                    return status;
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
                        //return BehaviorStatus.Running;
                    }
                    else
                    {
                        mRunningChild = null;
                    }
                    mIndex++;
                    if (status != BehaviorStatus.Success)
                    {
                        return status;
                    }
                }
            }
            return BehaviorStatus.Success;
        }
        public override void Reset()
        {
            mRunningChild = null;
            mIndex = 0;
            base.Reset();
        }
    }
}
