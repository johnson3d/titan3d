using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.BehaviorTree
{
    public class DecoratorBehavior
    {
        public BehaviorStatus Tick(long timeElapse, GamePlay.Actor.GCenterData context)
        {
            return BehaviorStatus.Failure;
        }
    }
}
