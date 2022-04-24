using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public class UTimedState : UState
    {
        UTimedSMClock mClock = new UTimedSMClock();
        public TimedSM_ClockWrapMode WrapMode
        {
            get => mClock.WrapMode;
            set => mClock.WrapMode = value;
        }
        public float StateTime { get => mClock.TimeInSecond; }
        public float StateTimeDuration { get => mClock.DurationInSecond; set => mClock.DurationInSecond = value; }
        public float StateMachineTime
        {
            get
            {
                return (mContext as UTimedStateMachine).Clock.TimeInSecond;
            }
        }
        public UTimedState(UTimedStateMachine context,string name = "TimedState"):base(context, name)
        {
            System.Diagnostics.Debug.Assert(context is UTimedStateMachine);

        }
        public override void Tick(float elapseSecond)
        {
            if (!EnableTick)
                return;
            mClock.Advance(elapseSecond);
            base.Tick(elapseSecond);
        }
    }
}
