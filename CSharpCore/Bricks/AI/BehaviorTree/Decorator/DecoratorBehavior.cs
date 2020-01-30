using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.Actor;

namespace EngineNS.Bricks.AI.BehaviorTree.Decorator
{


    public class DecoratorBehavior : Behavior
    {
        public BehaviorEvent BehaviorEvent { get; set; } = null;
        public Behavior Child { get; set; } = null;
        public FlowControlType FlowControl { get; set; } = FlowControlType.None;
        public override BehaviorStatus Update(long timeElapse, GCenterData context)
        {
            return BehaviorStatus.Success;
        }
        public override void RegisterEvent(BehaviorTree tree)
        {
            tree.AddBehaviorEvent(BehaviorEvent);
        }
        public override bool Schedule(Behavior behavior)
        {
            if (behavior == this)
                return true;
            else
                return Child.Schedule(behavior);
        }
        public override Behavior GetRunningBehavior()
        {
            if (mStatus == BehaviorStatus.Running)
            {
                var runningBh = Child.GetRunningBehavior();
                if (runningBh != null)
                    return runningBh;
            }
            return null;
        }
        public override void AllocatePriority(ref int priority)
        {
            Priority = priority;
            priority++;
            Child.AllocatePriority(ref priority);
        }
        public override void Reset()
        {
            base.Reset();
            mStatus = BehaviorStatus.Invalid;
            Child.Reset();
        }
        public virtual bool EventEvaluate(long timeElapse, GamePlay.Actor.GCenterData context)
        {
            return false;
        }
    }
}
