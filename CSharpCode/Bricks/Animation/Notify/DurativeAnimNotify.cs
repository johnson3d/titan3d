using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Notify
{
    [Rtti.Meta]
    public class DurativeAnimNotify : IO.BaseSerializer, IAnimNotify
    {
        Int64 mBeginTriggerTime = 0;
        public Int64 BeginTriggerTime //ms
        {
            get => mBeginTriggerTime;
            set => mBeginTriggerTime = value;
        }
        Int64 mEndTriggerTime = 0;
        public Int64 EndTriggerTime //ms
        {
            get => mEndTriggerTime;
            set
            {
                if (value < mBeginTriggerTime)
                {
                    mEndTriggerTime = mBeginTriggerTime;
                }
                else
                {
                    mEndTriggerTime = value;
                }
            }
        }
        string mName;
        public string Name
        {
            get => mName;
            set => mName = value;
        }
        Guid mId = Guid.NewGuid();
        public Guid ID
        {
            get => mId;
            set => mId = value;
        }

        public event NotifyHandle OnNotifyStart;
        public event NotifyHandle OnNotify;
        public event NotifyHandle OnNotifyStop;

        public bool CanTrigger(long beforeTime, long afterTime)
        {
            System.Diagnostics.Debug.Assert(mBeginTriggerTime <= mEndTriggerTime);
            if (mEndTriggerTime < beforeTime || mBeginTriggerTime > afterTime)
            {
                return false;
            }
            return true;

        }

        internal bool bIsTriggeed = false;
        bool IsTriggerStart(long beforeTime, long afterTime)
        {
            if(beforeTime <= mBeginTriggerTime && afterTime >= mEndTriggerTime)
            {
                return true;
            }
            if (beforeTime <= mBeginTriggerTime && afterTime >= mBeginTriggerTime && beforeTime <= mEndTriggerTime)
            {
                return true;
            }
            return false;
        }
        bool IsTriggerStop(long beforeTime, long afterTime)
        {
            if (beforeTime <= mBeginTriggerTime && afterTime >= mEndTriggerTime)
            {
                return true;
            }
            if (beforeTime <= mEndTriggerTime && afterTime >= mEndTriggerTime && beforeTime >= mBeginTriggerTime)
            {
                return true;
            }
            return false;
        }
        public void Trigger(long beforeTime, long afterTime)
        {
           if(IsTriggerStart(beforeTime, afterTime))
           {
                OnNotifyStart(this);
           }
           if (IsTriggerStop(beforeTime, afterTime))
           {
               OnNotifyStop(this);
           }
            OnNotify(this);
        }
    }
}
