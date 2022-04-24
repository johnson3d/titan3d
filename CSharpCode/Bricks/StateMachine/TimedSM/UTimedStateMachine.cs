using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.TimedSM
{
    public enum TimedSM_ClockWrapMode
    {
        Clamp,
        Repeat,
        Forever,
    }
    public class UTimedSMClock
    {
        float mTime = 0;
        public float TimeInSecond
        {
            get => mTime;
        }
        float mDuration = 1;
        public float DurationInSecond
        {
            get => mDuration;
            set => mDuration = value;
        }
        public TimedSM_ClockWrapMode WrapMode
        {
            get; set;
        } = TimedSM_ClockWrapMode.Repeat;
        public float TimeSacle { get; set; } = 1.0f;
        public bool Pause { get; set; } = false;

        public void Advance(float elapseTime)
        {
            if (Pause)
                return;
            mTime += elapseTime * TimeSacle;
            if (mTime > mDuration)
            {
                switch (WrapMode)
                {
                    case TimedSM_ClockWrapMode.Clamp:
                        {
                            mTime = mDuration;
                        }
                        break;
                    case TimedSM_ClockWrapMode.Repeat:
                        {
                            mTime = 0;
                        }
                        break;
                    case TimedSM_ClockWrapMode.Forever:
                        {
                            //just continue time++
                        }
                        break;
                }
            }
        }
        public void Reste()
        {
            mTime = 0;
        }
    }

    public class UTimedStateMachine : UStateMachine
    {
        UTimedSMClock mClock = new UTimedSMClock();
        public UTimedSMClock Clock { get => mClock; }
        public UTimedStateMachine()
        {

        }
        public UTimedStateMachine(string name) :base(name)
        {

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
