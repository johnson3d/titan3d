using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.BehaviorTree.Leaf
{
    public abstract class LeafBehavior : Behavior
    {
        public override BehaviorStatus Update(long timeElapse, GamePlay.Actor.GCenterData context) { return BehaviorStatus.Running; }
    }
}
