using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.Actor;

namespace EngineNS.Bricks.AI.BehaviorTree.Leaf.Action
{
    public class FinishWithResultBehavior :LeafBehavior
    {
        public BehaviorStatus Result { get; set; } = BehaviorStatus.Success;
        public override BehaviorStatus Update(long timeElapse, GCenterData context)
        {
            return Result;
        }
    }
}
