using EngineNS.Bricks.CodeBuilder.MacrossNode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Animation.Notify
{
    public delegate void NotifyHandle(IAnimNotify sender);
    [Rtti.Meta("")]
    public class TtTransientAnimNotify : IO.BaseSerializer, IAnimNotify
    {
        Int64 mTriggerTime = 0;
        public Int64 TriggerTime  //ms
        {
            get => mTriggerTime;
            set => mTriggerTime = value;
        }
        public Int64 BeginTime
        {
            get => mTriggerTime;
            set => mTriggerTime = value;
        }
        /// <summary>
        /// 瞬时通知没有长度, 结束时刻永远等于触发时刻, 写入无效
        /// </summary>
        public Int64 EndTime
        {
            get => mTriggerTime;
            set { }
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
        
        public bool CanTrigger(Int64 beforeTime, Int64 afterTime)
        {
            if (beforeTime <= mTriggerTime && afterTime >= mTriggerTime)
            {
                return true;
            }
            return false;
        }
        public virtual void Trigger(long beforeTime, long afterTime)
        {
            OnNotify?.Invoke(this);
        }

        protected RName mNotifyMacross;

        public event NotifyHandle OnNotify;

        public RName NotifyMacross
        {
            get { return mNotifyMacross; }
            set
            {
                if (mNotifyMacross == value)
                    return;
                mNotifyMacross = value;
                //mMcNotifyGetter = CEngine.Instance.MacrossDataManager.NewObjectGetter<McNotify>(value);
            }
        }
        public IAnimNotify Clone()
        {
            return null;
        }
    }
}
