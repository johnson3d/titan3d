using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.BehaviorTree.Leaf
{
    public abstract class LeafBehavior : Behavior
    {
        public override BehaviorStatus Update(long timeElapse, GamePlay.UCenterData context) { return BehaviorStatus.Running; }
    }
}
