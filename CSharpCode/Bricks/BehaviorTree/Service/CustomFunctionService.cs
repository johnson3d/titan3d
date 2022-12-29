using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay;

namespace EngineNS.BehaviorTree.Service
{
    public class CustomFunctionService : ServiceBehavior
    {
        public Action<long, GamePlay.UCenterData> Func { get; set; } = null;
        public override BehaviorStatus Update(long timeElapse, UCenterData context)
        {
            Func?.Invoke(timeElapse,context);
            return BehaviorStatus.Success;
        }
    }
}
