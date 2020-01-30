using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.AI.BehaviorTree;

namespace EngineNS.Bricks.MacrossNode.AI.Behavior
{
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class DelayBehavior : GamePlay.AI.BehaviorTree.Leaf.Action.ActionBehavior
    {
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public long WaitTime { get; set; } = 2000;
        private long mLastTime = 0;

        public override BehaviorStatus Update(long timeElapse, GamePlay.Actor.GCenterData context)
        {
            mLastTime += timeElapse;
            if (mLastTime > WaitTime)
                return BehaviorStatus.Success;
            return BehaviorStatus.Running;
        }
        public override void OnInitialize()
        {
            mLastTime = 0;
        }
    }
}
