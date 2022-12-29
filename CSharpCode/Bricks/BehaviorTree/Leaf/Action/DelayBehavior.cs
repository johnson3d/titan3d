using EngineNS.BehaviorTree;
using System;
using System.Collections.Generic;
using System.Text;


namespace EngineNS.BehaviorTree.Leaf.Action
{
    ////[Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class DelayBehavior : ActionBehavior
    {
        // //[Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public long WaitTime { get; set; } = 2000;
        public Func<long> WaitTimeEvaluateFunc { get; set; } = null;
        private long mLastTime = 0;

        public override BehaviorStatus Update(long timeElapse, GamePlay.UCenterData context)
        {
            if (WaitTimeEvaluateFunc != null)
                WaitTime = WaitTimeEvaluateFunc.Invoke();
            mLastTime += timeElapse;
            if (mLastTime > WaitTime)
            {
                return BehaviorStatus.Success;
            }
            return BehaviorStatus.Running;
        }
        public override void OnInitialize()
        {
            mLastTime = 0;
        }
        public override void OnTerminate(BehaviorStatus status)
        {
            base.OnTerminate(status);
            mLastTime = 0;
        }
        public override void Reset()
        {
            base.Reset();
            mLastTime = 0;
        }
    }
}
