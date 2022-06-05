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
            else
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Thread", $"ASyncSemaphore.Waiter is null");
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

    public class UAwaitSessionManager<K,T>
    {
        public class UAwaitSession
        {
            private List<Thread.ASyncSemaphore> Smph = new List<Thread.ASyncSemaphore>();
            public T Result;
            public Thread.ASyncSemaphore AddSemaphore()
            {
                lock (Smph)
                {
                    var tmp = Thread.ASyncSemaphore.CreateSemaphore(1);
                    Smph.Add(tmp);
                    return tmp;
                }
            }
            public void FinishSession(T rst)
            {
                Result = rst;
                lock (Smph)
                {
                    foreach (var i in Smph)
                    {
                        i.Release();
                    }
                    Smph.Clear();
                }
            }

            public async System.Threading.Tasks.Task<T> Await()
            {
                var tmp = this.AddSemaphore();
                await tmp.Await();
                return this.Result;
            }
        }
        public Dictionary<K, UAwaitSession> mSessions = new Dictionary<K, UAwaitSession>();
        public UAwaitSession GetOrNewSession(K key, out bool isNewSession)
        {
            lock (mSessions)
            {
                UAwaitSession result;
                if (mSessions.TryGetValue(key, out result) == false)
                {
                    isNewSession = true;
                    result = new UAwaitSession();
                    mSessions.Add(key, result);
                }
                else
                {
                    isNewSession = false;
                }
                return result;
            }
        }
        public void FinishSession(K key, UAwaitSession session, T result)
        {
            lock (mSessions)
            {
                mSessions.Remove(key);
            }
            session.FinishSession(result);
        }
        //public async System.Threading.Tasks.Task<T> Await(K key)
        //{
        //    bool isNew;
        //    var session = GetOrNewSession(key, out isNew);
        //    if (isNew)
        //    {
        //        var smp = session.AddSemaphore();
        //        await smp.Await();
        //        return session.Result;
        //    }
        //}
    }
}
