using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public class TtTimedStateTransition<T> : ITransition<T>
    {
        public bool TransitionOnFinish = false;
        public long Start { get; set; } = 0;
        public long Duration { get; set; } = 0;
        public IState<T> From { get; set; }
        public IState<T> To { get; set; }

        public bool Check()
        {
            if (!(From is TtTimedState<T>))
            {
                return CheckCondition();
            }
            else
            {
                var from = From as TtTimedState<T>;
                if (TransitionOnFinish)
                {
                    if (from.WrapMode == ETimedSM_ClockWrapMode.Clamp)
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

        public void OnTransition()
        {
           ;
        }
    }
    public class UTimedStateTransitionFunction<T> : TtTimedStateTransition<T>
    {
        public override bool CheckCondition()
        {
            if (TransitionFunction == null)
                return false;
            return TransitionFunction.Invoke();
        }

        public Func<bool> TransitionFunction { get; set; }
    }

    public class TtTimedStateTransition : TtTimedStateTransition<FStateMachineContext>
    {

    }
}
