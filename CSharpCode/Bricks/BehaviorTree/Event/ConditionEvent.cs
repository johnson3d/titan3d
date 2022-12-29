using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.BehaviorTree.Event
{
    public class ConditionEvent : BehaviorEvent
    {
        public bool ConditionValue = false;
        public override bool Checking(long timeElapse, GamePlay.UCenterData context)
        {
            var result = Behavior.EventEvaluate(timeElapse, context);
            if (result != ConditionValue)
            {
                ConditionValue = result;
                return true;
            }
            else
                return false;
        }
    }
}
