using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.BehaviorTree.Composite
{
    //[Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Createable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class RandomSelectBehavior : SelectBehavior
    {
        public override BehaviorStatus Update(long timeElapse, GamePlay.UCenterData context)
        {
            if (mRunningChild != null)
            {
                var status = mRunningChild.Tick(timeElapse, context);
                if (status != BehaviorStatus.Running)
                {
                    mRunningChild = null;
                }
                else
                {
                   // return BehaviorStatus.Running;
                }
                if (status != BehaviorStatus.Failure)
                {
                    //Reset();
                    return status;
                }
            }
            else
            {
                if (mIndex >= mChildrenList.Count)
                    mIndex = MathHelper.RandomRange(0, mChildrenList.Count - 1);
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
                    mIndex = MathHelper.RandomRange(0, mChildrenList.Count - 1);
                    if (status != BehaviorStatus.Failure)
                    {
                       // Reset();
                        return status;
                    }
                }
            }
            return BehaviorStatus.Failure;
        }
    }
}
