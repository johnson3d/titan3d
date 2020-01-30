using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.FSM.TFSM
{
    public class TimedTransition : Transition
    {
        public bool TransitionOnFinish = false;
        public long Start { get; set; } = 0;
        public long Duration { get; set; } = 0;
        internal override bool CheckCondition()
        {
            var from = From as TimedState;
            var to = To as TimedState;
            if (TransitionOnFinish)
            {
                if (from.Clock.WrapMode == TFSMClockWrapMode.TFSMWM_Clamp)
                {
                    if (from.Clock.TimeInMillionsecond == from.Clock.DurationInMillionsecond)
                    {
                        return CanTransit();
                    }
                }
            }
            else
            {
                if (Duration == 0)
                    return CanTransit();
                if (from.Clock.TimeInMillionsecond >= Start && from.Clock.TimeInMillionsecond <= Start + Duration)
                {
                    return CanTransit();
                }
            }
            return false;
        }
    }
    public class TimedTransitionFunction : TimedTransition
    {
        public override bool CanTransit()
        {
            if (TransitionFunction == null)
                return false;
            return TransitionFunction.Invoke();
        }

        public Func<bool> TransitionFunction { get; set; }
    }
}
