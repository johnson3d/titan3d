using EngineNS.Bricks.FSM.TFSM;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.AnimStateMachine
{
    public class AnimCrossfading
    {
        public bool PerformanceFirst { get; set; } = false;
        public float FadeTime { get; set; } = 0.1f;
        public AnimCrossfading()
        {

        }
        public AnimCrossfading(bool performanceFirst,float fadeTime)
        {
            PerformanceFirst = performanceFirst;
            FadeTime = fadeTime;
        }
    }
    public class LATimedTransitionFunction : TimedTransition
    {
        public override bool CanTransit()
        {
            if (mTransitionConditions.Count == 0)
                return false;
            for(int i = 0; i< mTransitionConditions.Count;++i)
            {
                if (mTransitionConditions[i]?.Invoke() == false)
                    return false;
            }
            return true;
        }
        public AnimCrossfading AnimCrossfading { get; set; } = null;
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
