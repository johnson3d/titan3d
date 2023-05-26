using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EngineNS.Thread
{
    public class TtAsyncDummyClass

    {
#pragma warning disable 1998
        public static async Async.TtTask DummyFunc()
        {
            //if (UEngine.Instance == null)
            //    return;
            //await UEngine.Instance.EventPoster.Post((state) =>
            //{
            //    return true;
            //});
            return;
        }
#pragma warning restore 1998
    }
    public class TtSemaphore
    {
        public Async.TtAsyncTaskStateBase PostEvent;
        private System.Threading.AutoResetEvent Waiter;
        private int mCount = -1;
        private bool ContinueEnqueued = false;
        public object Tag1;
        public object Tag2;
        public bool IsValid
        {
            get => mCount > 0;
        }
        public static TtSemaphore CreateSemaphore(int num, System.Threading.AutoResetEvent waiter = null)
        {
            var se = new TtSemaphore(num, waiter);
            return se;
        }
        private TtSemaphore(int num, System.Threading.AutoResetEvent waiter=null)
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
        public void EqueueContinue()
        {
            if (PostEvent == null)
                return;
            if (ContinueEnqueued)
                return;
            if (PostEvent != null)
            {
                PostEvent.ContinueThread.EnqueueContinue(PostEvent);
                ContinueEnqueued = true;
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
    public class TtThreadSafeNumber
    {
        int mCount = -1;
        public TtThreadSafeNumber(int num)
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
            private List<Thread.TtSemaphore> Smph = new List<Thread.TtSemaphore>();
            public T Result;
            public Thread.TtSemaphore AddSemaphore()
            {
                lock (Smph)
                {
                    var tmp = Thread.TtSemaphore.CreateSemaphore(1);
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
