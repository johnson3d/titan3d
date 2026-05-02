using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineNS.Thread
{
    public abstract class TtContextThread
    {
        public abstract Async.EAsyncTarget GetThreadType();
        public static List<WeakReference<TtContextThread>> AllContexts = new List<WeakReference<TtContextThread>>();
        public static int GetTotalContinueEventNumber(TtContextThread thread)
        {
            int count = 0;
            foreach (var i in AllContexts)
            {
                TtContextThread context;
                if (i.TryGetTarget(out context))
                {
                    count += context.GetContinueEventNumber(thread);
                }
            }
            return count;
        }
        public static int GetTotalEventNumber()
        {
            int count = 0;
            foreach (var i in AllContexts)
            {
                TtContextThread context;
                if (i.TryGetTarget(out context))
                {
                    var e = context.TotalEvents;
                    if (e > 0)
                        count += e;
                    else
                        count += 0;//for debug
                }
            }
            return count;
        }
        [ThreadStatic]
        public static TtContextThread CurrentContext;
        //这个Flag是解决主线程同时是:RHIContext，MainContext
        [ThreadStatic]
        public static int TickStage = 0;
        public virtual bool IsTaskPoolThread() { return false; }
        public TtContextThread()
        {
            Interval = 20;
            lock (AllContexts)
            {
                AllContexts.Add(new WeakReference<TtContextThread>(this));
            }
        }
        public void ExitWhenFrameFinished()
        {
            
        }
        public static TtContextThread WaitingThread = null;
        private static int WaitingCount = 0;
        private static void EnterWaitingThread(TtContextThread thread)
        {
            if (WaitingThread!=null)
            {
                System.Diagnostics.Debug.Assert(WaitingThread.ThreadId == thread.ThreadId);
            }
            WaitingThread = thread;
            WaitingCount++;
        }
        private static void LeaveWaitingThread(TtContextThread thread)
        {
            System.Diagnostics.Debug.Assert(WaitingThread.ThreadId == thread.ThreadId);
            WaitingCount--;
            if (WaitingCount == 0)
                WaitingThread = null;
        }
        protected bool mIsRun = false;
        private bool mIsFinished = false;
        public int Interval
        {
            get;
            set;
        }
        protected System.Threading.Thread mThread;
        public string Name;
        public void FromCurrent(string name)
        {
            if (mThreadId == 0)
            {
                mThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                if(CurrentContext==null)
                    CurrentContext = this;
                mIsRun = true;
                this.Name = name;
            }
            this.OnThreadStart();
        }
        public delegate void FOnThreadTick(TtContextThread ctx);
        public FOnThreadTick TickAction = null;
        public virtual bool StartThread(string name, FOnThreadTick action, int stackSize = -1)
        {
            if (mIsRun)
                return false;
            Name = name;
            mIsRun = true;
            mIsFinished = false;
            if (stackSize<0)
            {
                mThread = new System.Threading.Thread(ThreadMain);
            }
            else
            {
                mThread = new System.Threading.Thread(ThreadMain, stackSize);
            }
            mThread.Name = name;
            TickAction = action;
            mThread.Start();
            return true;
        }
        public virtual void StopThread(System.Action waitAction)
        {
            if (mIsRun == false)
                return;

            mIsRun = false;
            if (mThreadId == System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                System.Diagnostics.Debugger.Break();
                mThread = null;
                return;
            }
            while(mIsFinished==false)
            {
                if (waitAction != null)
                    waitAction();
                System.Threading.Thread.Sleep(5);
                mEnqueueTrigger.Set();
            }
            mThread = null;
        }
        public void FlushAllThreadEvents(bool bFlushStreaming = true)
        {
            EnterWaitingThread(this);

            var IsMainThread = TtContextThread.CurrentContext.ThreadId == TtEngine.Instance.ThreadMain.ThreadId;
            var IsLogicThread = TtContextThread.CurrentContext.ThreadId == TtEngine.Instance.ThreadLogic.ThreadId;
            while (true)
            {
                FContextTickableManager.GetInstance().ThreadTick();
                TickAwaitEvent();
                if (IsMainThread)
                {
                    TtEngine.Instance.ThreadLogic.TickAwaitEvent();
                }
                else if (IsLogicThread)
                {
                    TtEngine.Instance.ThreadMain.TickAwaitEvent();
                }
                TtEngine.Instance.TaskCollector.Tick();
                //if (bFlushStreaming && TtEngine.Instance.GfxDevice.TextureManager.GetNeedStreamingNumber() > 0)
                //{
                //    continue;
                //}
                if (TtContextThread.GetTotalEventNumber() == 0)
                {
                    LeaveWaitingThread(this);
                    return;
                }
            }

            //var t1 = Support.TtTime.HighPrecision_GetTickCount();
            //while (TtContextThread.GetTotalContinueEventNumber(thread) + thread.ContinueNum > 0)
            //{
            //    FContextTickableManager.GetInstance().ThreadTick();
            //    thread.TickAwaitEvent();
            //    TtEngine.Instance.TaskCollector.Tick();
            //}
            //var t2 = Support.TtTime.HighPrecision_GetTickCount();
            //if (t2 - t1 > 20000)
            //{
            //    Profiler.Log.WriteLine<Profiler.TtThreadGategory>(Profiler.ELogTag.Warning, $"FlushAllThreadEvents({thread.Name}) Time = {(t2 - t1)/1000} ms");
            //}
        }
        public void FlushToSemephore(TtSemaphore smp)
        {
            EnterWaitingThread(this);
            //System.Diagnostics.Debug.Assert(TtContextThread.CurrentContext.ThreadId != TtEngine.Instance.ThreadMain.ThreadId);
            var IsMainThread = TtContextThread.CurrentContext.ThreadId == TtEngine.Instance.ThreadMain.ThreadId;
            var IsLogicThread = TtContextThread.CurrentContext.ThreadId == TtEngine.Instance.ThreadLogic.ThreadId;
            var t1 = Support.TtTime.HighPrecision_GetTickCount();
            while (true)
            {
                FContextTickableManager.GetInstance().ThreadTick();
                TickAwaitEvent();
                if (IsMainThread)
                {
                    TtEngine.Instance.ThreadLogic.TickAwaitEvent();
                }
                else if (IsLogicThread)
                {
                    TtEngine.Instance.ThreadMain.TickAwaitEvent();
                }
                TtEngine.Instance.TaskCollector.Tick();
                if (smp.GetCount() == 0)
                {
                    var t2 = Support.TtTime.HighPrecision_GetTickCount();
                    if (t2 - t1 > 20000)
                    {
                        Profiler.Log.WriteLine<Profiler.TtThreadGategory>(Profiler.ELogTag.Warning, $"FlushToSemephore Time = {(t2 - t1) / 1000} ms");
                    }
                    LeaveWaitingThread(this);
                    return;
                }
            }
        }
        public void WaitTask(Thread.Async.ITask task)
        {
            EnterWaitingThread(this);
            var IsMainThread = TtContextThread.CurrentContext.ThreadId == TtEngine.Instance.ThreadMain.ThreadId;
            var IsLogicThread = TtContextThread.CurrentContext.ThreadId == TtEngine.Instance.ThreadLogic.ThreadId;
            var t1 = Support.TtTime.HighPrecision_GetTickCount();
            while (true)
            {
                FContextTickableManager.GetInstance().ThreadTick();
                TickAwaitEvent();
                if (IsMainThread)
                {
                    TtEngine.Instance.ThreadLogic.TickAwaitEvent();
                }
                else if (IsLogicThread)
                {
                    TtEngine.Instance.ThreadRHI.TickAwaitEvent();
                    TtEngine.Instance.ThreadMain.TickAwaitEvent();
                }
                TtEngine.Instance.TaskCollector.Tick();
                if (task.IsCompleted)
                {
                    var t2 = Support.TtTime.HighPrecision_GetTickCount();
                    if (t2 - t1 > 20000)
                    {
                        Profiler.Log.WriteLine<Profiler.TtThreadGategory>(Profiler.ELogTag.Warning, $"WaitTask Time = {(t2 - t1) / 1000} ms");
                    }
                    LeaveWaitingThread(this);
                    return;
                }
            }
        }
        protected int mThreadId = 0;
        public int ThreadId
        {
            get
            {
                return mThreadId;
            }
        }
        public bool IsFinished
        {
            get
            {
                return mIsFinished;
            }
        }
        private void ThreadMain()
        {
            CurrentContext = this;
            mThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            OnThreadStart();
            while (mIsRun || TotalEvents > 0)
            {
                var time1 = Support.TtTime.GetTickCount();
                try
                {
                    FContextTickableManager.GetInstance().ThreadTick();
                    Tick();
                }
                catch(Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
                finally
                {
                    TtContextThread.CurrentContext.ExitWhenFrameFinished();
                }
                var time2 = Support.TtTime.GetTickCount();
                if (time2 - time1 < Interval)
                    System.Threading.Thread.Sleep(Interval - (int)(time2 - time1));
            }
            //ExecuteToEmpty();
            mIsFinished = true;
            OnThreadExited();
            CurrentContext = null;
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtContextThread), nameof(Tick));
                return mScopeTick;
            }
        } 
        public virtual void Tick()
        {
            if (TickAction != null)
            {
                using (new Profiler.TimeScopeHelper(ScopeTick))
                {
                    TickAction(this);
                }
            }
        }
        protected virtual void OnThreadStart()
        {

        }
        protected virtual void OnThreadExited()
        {

        }
        public int PriorityNum
        {
            get { return PriorityEvents.Count; }
        }
        public int AsyncNum
        {
            get { return AsyncEvents.Count; }
        }
        public int ContinueNum
        {
            get { return ContinueEvents.Count; }
        }
        public int GetContinueEventNumber(TtContextThread thread)
        {
            int count = 0;
            lock (AsyncEvents)
            {
                foreach (var i in AsyncEvents)
                {
                    if (i.ContinueThread == thread)
                        count++;
                }
            }
            return count;
        }
        public int TotalEvents
        {
            get
            {
                return PriorityNum + AsyncNum + ContinueNum + TempCount;
            }
        }
        int TempCount = 0;
        [Browsable(false)]
        protected Queue<Async.TtAsyncTaskStateBase> PriorityEvents
        {
            get;
        } = new Queue<Async.TtAsyncTaskStateBase>();
        internal System.Threading.AutoResetEvent mEnqueueTrigger = new System.Threading.AutoResetEvent(false);
        public void EnqueuePriority(Async.TtAsyncTaskStateBase evt)
        {
            System.Diagnostics.Debug.Assert(IsFinished == false);
            lock (PriorityEvents)
            {
                PriorityEvents.Enqueue(evt);
            }
            mEnqueueTrigger.Set();
        }
        [Browsable(false)]
        protected Queue<Async.TtAsyncTaskStateBase> AsyncEvents
        {
            get;
        } = new Queue<Async.TtAsyncTaskStateBase>();
        public void EnqueueAsync(Async.TtAsyncTaskStateBase evt)
        {
            System.Diagnostics.Debug.Assert(IsFinished == false);
            lock (AsyncEvents)
            {
                AsyncEvents.Enqueue(evt);
            }
            mEnqueueTrigger.Set();
        }
        [Browsable(false)]
        protected Queue<Async.TtAsyncTaskStateBase> ContinueEvents
        {
            get;
        } = new Queue<Async.TtAsyncTaskStateBase>();
        public virtual void EnqueueContinue(Async.TtAsyncTaskStateBase evt)
        {
            System.Diagnostics.Debug.Assert(IsFinished == false);
            System.Diagnostics.Debug.Assert(evt.ContinueThread == this);
            lock (ContinueEvents)
            {
                if (this == TtContextThread.CurrentContext)
                {
                    evt.ExecuteContinue();
                }
                else
                {
                    ContinueEvents.Enqueue(evt);
                    mEnqueueTrigger.Set();
                }
            }
        }
        public long LimitTime
        {
            get;
            set;
        } = 5 * 1000;//5 ms
        internal bool TimeOut = false;

        private bool TestTimeOut(long start, long limit, Async.TtAsyncTaskStateBase state)
        {
            var cur = Support.TtTime.HighPrecision_GetTickCount();
            var delta = cur - start;
            if (delta > limit)
            {
                if (cur - cur > 4 * limit)//20 ms
                {
                    if (TtEngine.Instance.EventPoster.IsThread(Async.EAsyncTarget.Logic))
                    {
                        Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"Logic Thread[Async] is Blocked({(cur - start)/1000}ms > {limit / 1000}):\n{state.ToString()}");
                    }
                    else if (TtEngine.Instance.EventPoster.IsThread(Async.EAsyncTarget.Render))
                    {
                        Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"Render Thread[Async] is Blocked({(cur - start) /1000}ms > {limit / 1000}):\n{state.ToString()}");
                        //if (cur.CallStackTrace != null)
                        //{
                        //    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Async", $"StackInof=>{cur.CallStackTrace}");
                        //}
                    }
                }
                
                return true;
            }
            return false;
        }
        public void TickAwaitEvent()
        {
            lock (this)
            {
                Async.TtAsyncTaskStateBase cur;
                var start = Support.TtTime.HighPrecision_GetTickCount();
                while (DoOnePriorityEvent(out cur))
                {
                    cur.Dispose();
                    if (TestTimeOut(start, LimitTime, cur))
                    {
                        TimeOut = true;
                        mEnqueueTrigger.Set();
                        return;
                    }
                }

                while (DoOneAsyncEvent(out cur))
                {
                    if (TestTimeOut(start, LimitTime, cur))
                    {
                        TimeOut = true;
                        mEnqueueTrigger.Set();
                        return;
                    }
                }
                while (DoOneContinueEvent(out cur))
                {

                }
                //while ((t1 = Support.Time.HighPrecision_GetTickCount()) > 0 && DoOneContinueEvent(out cur))
                //{
                //    if (TestTimeOut(start, t1, LimitTime, cur))
                //    {
                //        TimeOut = true;
                //        return;
                //    }
                //}
                TimeOut = false;
            }
        }
        public bool DoOnePriorityEvent(out Async.TtAsyncTaskStateBase oe)
        {
            oe = null;
            Async.TtAsyncTaskStateBase e;
            lock (PriorityEvents)
            {
                if (PriorityEvents.Count == 0)
                    return false;
                e = PriorityEvents.Dequeue();
            }

            try
            {
                var state = e.ExecutePostEvent();
                if (state.TaskState == Async.EAsyncTaskState.Suspended)
                {
                    lock (PriorityEvents)
                    {
                        PriorityEvents.Enqueue(e);
                    }
                }
                else
                {
                    e.ExecuteContinue();
                }
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                e.ExceptionInfo = ex;
            }
            oe = e;
            return true;
        }
        protected bool DoOneAsyncEvent(out Async.TtAsyncTaskStateBase oe)
        {
            oe = null;
            Async.TtAsyncTaskStateBase e;
            lock (AsyncEvents)
            {
                if (AsyncEvents.Count == 0)
                    return false;
                TempCount++;
                e = AsyncEvents.Dequeue();
            }
            try
            {
                var state = e.ExecutePostEvent();
                if (state.TaskState == Async.EAsyncTaskState.Suspended)
                {
                    lock (AsyncEvents)
                    {
                        AsyncEvents.Enqueue(e);
                    }
                    oe = e;
                    TempCount--;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                e.ExceptionInfo = ex;
            }
            if(e.Tag==null)
            {
                if(e.ContinueThread != null)
                {
                    lock (e.ContinueThread.ContinueEvents)
                    {
                        e.ContinueThread.EnqueueContinue(e);
                    }
                }
            }
            TempCount--;

            oe = e;
            return true;
        }
        protected bool DoOneContinueEvent(out Async.TtAsyncTaskStateBase oe)
        {
            oe = null;
            Async.TtAsyncTaskStateBase e;
            lock (ContinueEvents)
            {
                if (ContinueEvents.Count == 0)
                    return false;
                e = ContinueEvents.Dequeue();
            }
            
            try
            {
                e.ExecuteContinue();
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                e.ExceptionInfo = ex;
            }
            oe = e;
            return true;
        }

        public virtual bool IsThisThread()
        {
            if (this.ThreadId == System.Threading.Thread.CurrentThread.ManagedThreadId)
                return true;
            //if (WaitingThread!=null)
            //{
            //    if (TtContextThread.CurrentContext.ThreadId == WaitingThread.ThreadId)
            //    {
            //        return true;
            //    }
            //}
            return false;
        }

        #region Payload
        #endregion
    }
}
