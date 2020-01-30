using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.Actor;

namespace EngineNS.Bricks.AI.BehaviorTree.Service
{
    public class CustomFunctionService : ServiceBehavior
    {
        public Action<long, GamePlay.Actor.GCenterData> Func { get; set; } = null;
        public override BehaviorStatus Update(long timeElapse, GCenterData context)
        {
            Func?.Invoke(timeElapse,context);
            return BehaviorStatus.Success;
        }
    }
}
