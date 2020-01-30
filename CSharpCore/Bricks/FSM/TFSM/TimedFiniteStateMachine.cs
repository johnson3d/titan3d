using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.FSM.TFSM
{
    public enum TFSMClockWrapMode
    {
        TFSMWM_Clamp,
        TFSMWM_Repeat,
        TFSMWM_Forever,
    }
    public class TFSMClock
    {
        long mTime = 0;
        public long TimeInMillionsecond
        {
            get => mTime;
            set
            {
                if (value < 0)
                    mTime = 0;
                else if (value > DurationInMillionsecond)
                    mTime = DurationInMillionsecond;
                else
                    mTime = value;
            }
        }
        public float TimeInSecond
        {
            get => (float)mTime * 0.001f;
            set
            {
                if (value < 0)
                    mTime = 0;
                else if (value > DurationInSecond)
                    mTime = DurationInMillionsecond;
                else
                    mTime = (long)(value * 1000);
            }
        }
        long mDuration = 1000;
        public long DurationInMillionsecond
        {
            get => mDuration;
            set => mDuration = value;
        }
        public float DurationInSecond
        {
            get => (float)mDuration * 0.001f;
            set => mDuration = (long)(value * 1000);
        }
        public TFSMClockWrapMode WrapMode
        {
            get; set;
        } = TFSMClockWrapMode.TFSMWM_Repeat;
        public float TimeSacle { get; set; } = 1.0f;
        public bool Pause { get; set; } = false;
        public void Advance()
        {
            Advance(CEngine.Instance.EngineElapseTime);
        }
        public void Advance(long elapseTime)
        {
            if (Pause)
                return;
            mTime += (long)((float)elapseTime * TimeSacle);
            if (mTime > mDuration)
            {
                switch (WrapMode)
                {
                    case TFSMClockWrapMode.TFSMWM_Clamp:
                        {
                            mTime = mDuration;
                        }
                        break;
                    case TFSMClockWrapMode.TFSMWM_Repeat:
                        {
                            mTime = 0;
                        }
                        break;
                    case TFSMClockWrapMode.TFSMWM_Forever:
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

    public class TimedFiniteStateMachine : FiniteStateMachine
    {
        TFSMClock mClock = new TFSMClock();
        public TFSMClock Clock { get => mClock; }
        public TimedFiniteStateMachine()
        {

        }
        public TimedFiniteStateMachine(string name) :base(name)
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
