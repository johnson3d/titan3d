using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public class TtTimedStateTransition<S, T> : ITransition<S, T>
    {
        public S CenterData { get; set; }
        public bool TransitionOnFinish = false;
        public long Start { get; set; } = 0;
        public long Duration { get; set; } = 0;
        public IState<S, T> From { get; set; }
        public IState<S, T> To { get; set; }

        public bool Check(in T context)
        {
            if (!(From is TtTimedState<S, T>))
            {
                return CheckCondition(context);
            }
            else
            {
                var from = From as TtTimedState<S, T>;
                if (TransitionOnFinish)
                {
                    if (from.WrapMode == ETimedSM_ClockWrapMode.Clamp)
                    {
                        if (from.StateTime == from.Duration)
                        {
                            return CheckCondition(context);
                        }
                    }
                }
                else
                {
                    if (Duration == 0)
                        return CheckCondition(context);
                    if (from.StateTime >= Start && from.StateTime <= Start + Duration)
                    {
                        return CheckCondition(context);
                    }
                }
                return false;
            }
        }
        public virtual bool CheckCondition(in T context)
        {
            return true;
        }

        public void OnTransition()
        {
           
        }
    }
    //public class UTimedStateTransitionFunction<T> : TtTimedStateTransition<T>
    //{
    //    public override bool CheckCondition(in T context)
    //    {
    //        if (TransitionFunction == null)
    //            return false;
    //        return TransitionFunction.Invoke();
    //    }

    //    public Func<bool> TransitionFunction { get; set; }
    //}

    public class TtTimedStateTransition<S> : TtTimedStateTransition<S, TtStateMachineContext>
    {

    }

    public class TtTimedStateTransition: TtTimedStateTransition<TtDefaultCenterData, TtStateMachineContext>
    {

    }
}
