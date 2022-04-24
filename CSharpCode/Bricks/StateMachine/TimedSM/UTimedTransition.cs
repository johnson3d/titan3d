using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public class UTimedTransition : UTransition
    {
        public bool TransitionOnFinish = false;
        public long Start { get; set; } = 0;
        public long Duration { get; set; } = 0;
        public override bool Check()
        {
            if (!(From is UTimedState))
            {
                return CheckCondition();
            }
            else
            {
                var from = From as UTimedState;
                if (TransitionOnFinish)
                {
                    if (from.WrapMode == TimedSM_ClockWrapMode.Clamp)
                    {
                        if (from.StateTime == from.StateTimeDuration)
                        {
                            return CheckCondition();
                        }
                    }
                }
                else
                {
                    if (Duration == 0)
                        return CheckCondition();
                    if (from.StateTime >= Start && from.StateTime <= Start + Duration)
                    {
                        return CheckCondition();
                    }
                }
                return false;
            }
        }
        public virtual bool CheckCondition()
        {
            return true;
        }
    }
    public class UTimedTransitionFunction : UTimedTransition
    {
        public override bool CheckCondition()
        {
            if (TransitionFunction == null)
                return false;
            return TransitionFunction.Invoke();
        }

        public Func<bool> TransitionFunction { get; set; }
    }
}
