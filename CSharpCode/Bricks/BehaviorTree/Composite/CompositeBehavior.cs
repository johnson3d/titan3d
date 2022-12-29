using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.BehaviorTree.Composite
{
    public abstract class CompositeBehavior : Behavior
    {
        protected List<Behavior> mChildrenList = new List<Behavior>();
        public void AddChild(Behavior behavior)
        {
            if (mChildrenList.Contains(behavior))
            {

            }
            mChildrenList.Add(behavior);
        }
        public CompositeBehavior SetChildren(List<Behavior> behaviorList)
        {
            mChildrenList = behaviorList;
            return this;
        }
        public void RemoveChild(Behavior behavior)
        {
            mChildrenList.Remove(behavior);
        }
        public void ClearChildren()
        {
            mChildrenList.Clear();
        }
        public override void RegisterEvent(BehaviorTree tree)
        {
            for (int i = 0; i < mChildrenList.Count; ++i)
            {
                mChildrenList[i].RegisterEvent(tree);
            }
        }
        public override Behavior GetRunningBehavior()
        {
            if (mStatus == BehaviorStatus.Running)
            {
                for (int i = 0; i < mChildrenList.Count; ++i)
                {
                    var runningBh = mChildrenList[i].GetRunningBehavior();
                    if (runningBh != null)
                        return runningBh;
                }
            }
            return null;
        }
        public override void Reset()
        {
            base.Reset();
            for (int i = 0; i < mChildrenList.Count; ++i)
            {
                mChildrenList[i].Reset();
            }
        }
        public override void AllocatePriority(ref int priority)
        {
            Priority = priority;
            for (int i = 0; i < mChildrenList.Count; ++i)
            {
                priority++;
                mChildrenList[i].AllocatePriority(ref priority);
            }
        }

        public override BehaviorStatus Update(long timeElapse, GamePlay.UCenterData context) { return BehaviorStatus.Running; }
    }
}
