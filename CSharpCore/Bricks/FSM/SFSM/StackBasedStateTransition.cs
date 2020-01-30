using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.FSM.SFSM
{
    public class StackBasedStateTransition : Transition
    {

    }
    public class StateBasedStateTransitionFunction : StackBasedStateTransition
    {
        public override bool CanTransit()
        {
            if (mTransitionConditions.Count == 0)
                return false;
            for (int i = 0; i < mTransitionConditions.Count; ++i)
            {
                if (mTransitionConditions[i].Invoke() == false)
                    return false;
            }
            return true;
        }
        List<Func<bool>> mTransitionConditions = new List<Func<bool>>();

        public void AddTransitionCondition(Func<bool> transitionFunction)
        {
            mTransitionConditions.Add(transitionFunction);
        }
        public void AddTransitionExecuteAction(Action executeAction)
        {
            OnTransitionEexecuteActions.Add(executeAction);
        }
        public void RemoveTransitonCondition(Func<bool> transitionFunction)
        {
            mTransitionConditions.Remove(transitionFunction);
        }
    }
}
