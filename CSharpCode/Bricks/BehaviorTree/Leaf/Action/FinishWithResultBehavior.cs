using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay;

namespace EngineNS.BehaviorTree.Leaf.Action
{
    public class FinishWithResultBehavior :LeafBehavior
    {
        public BehaviorStatus Result { get; set; } = BehaviorStatus.Success;
        public override BehaviorStatus Update(long timeElapse, UCenterData context)
        {
            return Result;
        }
    }
}
