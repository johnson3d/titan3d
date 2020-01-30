using EngineNS.Bricks.AI.BehaviorTree.Decorator;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.BehaviorTree
{
    public enum FlowControlType
    {
        None,
        Self,
        LowPriority,
        Both,
    }
    public class BehaviorEvent
    {
        public BehaviorTree BehaviorTree { get; set; }
        public DecoratorBehavior Behavior { get; set; } = null;
        public virtual bool Checking(long timeElapse, GamePlay.Actor.GCenterData context)
        {
            return false;
        }
    }
}
