using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EngineNS.Thread
{
    public class AsyncDummyClass
    {
#pragma warning disable 1998
        public static async System.Threading.Tasks.Task DummyFunc()
        {

        }
#pragma warning restore 1998
    }
    public class ASyncSemaphore
    {
        public Async.PostEvent PostEvent;
        private System.Threading.AutoResetEvent Waiter;
        int mCount = -1;
        public bool IsValid
        {
            get => mCount > 0;
        }
        public static ASyncSemaphore CreateSemaphore(int num, System.Threading.AutoResetEvent waiter = null)
        {
            var se = new ASyncSemaphore(num, waiter);
            return se;
        }
        private ASyncSemaphore(int num, System.Threading.AutoResetEvent waiter=null)
        {
            PostEvent = null;
            Waiter = waiter;
            System.Threading.Interlocked.Exchange(ref mCount, num);
        }
        public void FreeSemaphore()
        {
            PostEvent = null;
            Waiter = null;
            mCount = -1;
        }
        public void Release()
        {
            System.Threading.Interlocked.Decrement(ref mCount);
            if (mCount <= 0)
            {
                if(Waiter!=null)
                {
                    Waiter.Set();
                }
                EqueueContinue();
            }
        }
        private bool ContinueEnqueued = false;
        public void EqueueContinue()
        {
            if (PostEvent == null)
                return;
            lock (PostEvent.ContinueThread.ContinueEvents)
            {
                if (ContinueEnqueued)
                    return;
                if (PostEvent != null)
                {
                    PostEvent.ContinueThread.ContinueEvents.Enqueue(PostEvent);
                    ContinueEnqueued = true;
                }
            }   
        }
        public int GetCount()
        {
            return System.Threading.Interlocked.Exchange(ref mCount, mCount);
        }
        public void Wait(int milliseconds)
        {
            if (Waiter != null)
            {
                Waiter.WaitOne(milliseconds);
            }
        }
        public async System.Threading.Tasks.Task Await()
        {
            if (this == null)
            {
                System.Diagnostics.Debug.Assert(false);
            }
            await UEngine.Instance.EventPoster.AwaitSemaphore(this);
        }
    }
    public class ThreadSafeNumber
    {
        int mCount = -1;
        public ThreadSafeNumber(int num)
        {
            mCount = num;
        }
        public int Add()
        {
            return System.Threading.Interlocked.Increment(ref mCount);
        }
        public int Release()
        {
            return System.Threading.Interlocked.Decrement(ref mCount);
        }
        public int Exchange(int value)
        {
            return System.Threading.Interlocked.Exchange(ref mCount, value);
        }
        public int GetValue()
        {
            return System.Threading.Interlocked.Exchange(ref mCount, mCount);
        }
    }
}
