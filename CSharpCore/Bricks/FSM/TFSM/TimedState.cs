using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.FSM.TFSM
{
    public class TimedState : State
    {
        TFSMClock mClock = new TFSMClock();
        public TFSMClock Clock { get => mClock; }
        public TFSMClockWrapMode WrapMode
        {
            get => mClock.WrapMode;
            set => mClock.WrapMode = value;
        }
        public TimedState(TimedFiniteStateMachine mContext,string name = "TimedState"):base(mContext,name)
        {

        }
        public override void Tick()
        {
            if (!EnableTick)
                return;
            mClock.Advance();
            base.Tick();
        }
    }
}
