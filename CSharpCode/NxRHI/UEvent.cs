using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class UEvent : AuxPtrType<NxRHI.IEvent>
    {
        public string Name
        {
            get
            {
                return mCoreObject.GetName();
            }
        }
        public void SetEvent()
        {
            mCoreObject.SetEvent();
        }
        public void ResetEvent()
        {
            mCoreObject.SetEvent();
        }
        public void Wait(uint time = uint.MaxValue)
        {
            mCoreObject.Wait(time);
        }
    }

    public class UFence : AuxPtrType<NxRHI.IFence>
    {
        public string Name
        {
            get
            {
                return mCoreObject.GetName();
            }
        }
        public ulong CompletedValue
        {
            get
            {
                return mCoreObject.GetCompletedValue();
            }
        }
        public ulong ExpectValue
        {
            get
            {
                return mCoreObject.GetExpectValue();
            }
        }
        public void IncreaseExpect(ICmdQueue queue, ulong value, EQueueType type)
        {
            mCoreObject.IncreaseExpect(queue, value, type);
        }
        public void Wait(ulong value, uint time = uint.MaxValue)
        {
            mCoreObject.Wait(value, time);
        }
        public void WaitToExpect(uint time = uint.MaxValue)
        {
            mCoreObject.WaitToExpect(time);
        }
    }
}
