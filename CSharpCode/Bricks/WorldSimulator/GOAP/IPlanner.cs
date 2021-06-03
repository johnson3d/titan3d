using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.WorldSimulator.GOAP
{
    public class IPlanner
    {
        public virtual bool Plan(IActor actor, IEnvironment env, List<IAction> actions)
        {
            //最短路径搜索
            foreach (var i in actor.Actions)
            {
                i.Value.TmpCost = 0;
            }
            var cur = actor.CurrentGoal;
            Stack<IAction> openTab = new Stack<IAction>();
            IGoal goal = actor.GetBestGoal();
            float curCost = 0;
            foreach(var i in goal.Actions)
            {
                var action = actor.GetAction(i.Name);
                if (action == null || action.TmpCost > curCost)
                    continue;
                action.TmpCost = curCost;
                openTab.Push(action);
                //float weights = i.Key.GetWeights();
                //i.Value.Goal;
            }
            return false;
        }
    }
}
