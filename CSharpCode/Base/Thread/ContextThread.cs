using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineNS.Thread
{
    public class TtContextThread
    {
        [ThreadStatic]
        public static TtContextThread CurrentContext;
        //这个Flag是解决主线程同时是:RHIContext，MainContext
        [ThreadStatic]
        public static int TickStage = 0;
        public TtContextThread()
        {
            Interval = 20;
        }
        List<object> mMonitorEnterObjects = new List<object>();
        public int MonitorEnter(object obj)
        {
            System.Threading.Monitor.Enter(obj);
            int index = mMonitorEnterObjects.Count;
            mMonitorEnterObjects.Add(obj);
            return index;
        }
        public bool MonitorExit(int index)
        {
            if (index<0 || index >= mMonitorEnterObjects.Count)
                return false;

            System.Threading.Monitor.Exit(mMonitorEnterObjects[index]);
            mMonitorEnterObjects[index] = null;
            return true;
        }
        public void ExitWhenFrameFinished()
        {
            for (int i = 0; i < mMonitorEnterObjects.Count; i++)
            {
                if(mMonitorEnterObjects[i]!=null)
                {
                    System.Threading.Monitor.Exit(mMonitorEnterObjects[i]);
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Thread", $"Locker({mMonitorEnterObjects[i]}) is not released");
                }
            }
            mMonitorEnterObjects.Clear();
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
        public virtual bool StartThread(string name, FOnThreadTick action)
        {
            if (mIsRun)
                return false;
            Name = name;
            mIsRun = true;
            mIsFinished = false;
            mThread = new System.Threading.Thread(ThreadMain);
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
            }
            mThread = null;
        }
        public virtual void ExecuteToEmpty()
        {
            while (TotalEvents > 0)
            {
                FContextTickableManager.GetInstance().ThreadTick();
                TickAwaitEvent();
            }
            //Async.PostEvent cur;
            //while (DoOnePriorityEvent(out cur))
            //{
            //    UEngine.Instance.EventPoster.mRunOnPEAllocator.ReleaseObject(cur);
            //}
            //while (DoOneAsyncEvent(out cur))
            //{

            //}
            //while (DoOneContnueEvent(out cur))
            //{

            //}            
            //lock (RunUntilFinishEvents)
            //{

            //}
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
                var time1 = Support.Time.GetTickCount();
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
                var time2 = Support.Time.GetTickCount();
                if (time2 - time1 < Interval)
                    System.Threading.Thread.Sleep(Interval - (int)(time2 - time1));
            }
            //ExecuteToEmpty();
            mIsFinished = true;
            OnThreadExited();
            CurrentContext = null;
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(TtContextThread), nameof(Tick));
        public virtual void Tick()
        {
            if(TickAction!=null)
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
        public int TotalEvents
        {
            get
            {
                return PriorityNum + AsyncNum + ContinueNum;
            }
        }
        [Browsable(false)]
        protected Queue<Async.TtAsyncTaskStateBase> PriorityEvents
        {
            get;
        } = new Queue<Async.TtAsyncTaskStateBase>();
        public void EnqueuePriority(Async.TtAsyncTaskStateBase evt)
        {
            System.Diagnostics.Debug.Assert(IsFinished == false);
            lock (PriorityEvents)
            {
                PriorityEvents.Enqueue(evt);
            }
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
        }
        [Browsable(false)]
        protected Queue<Async.TtAsyncTaskStateBase> ContinueEvents
        {
            get;
        } = new Queue<Async.TtAsyncTaskStateBase>();
        public void EnqueueContinue(Async.TtAsyncTaskStateBase evt)
        {
            System.Diagnostics.Debug.Assert(IsFinished == false);
            lock (ContinueEvents)
            {
                ContinueEvents.Enqueue(evt);
            }
        }
        [Browsable(false)]
        public List<Async.TtAsyncTaskStateBase> RunUntilFinishEvents
        {
            get;
        } = new List<Async.TtAsyncTaskStateBase>();
        public long LimitTime
        {
            get;
            set;
        } = 5 * 1000;//5 ms
        internal bool TimeOut = false;

        private bool TestTimeOut(long start, long t1, long limit, Async.TtAsyncTaskStateBase state)
        {
            var cur = Support.Time.HighPrecision_GetTickCount();
            var delta = cur - start;
            if (delta > limit)
            {
                if (cur - t1 > 4 * limit)//20 ms
                {
                    if (UEngine.Instance.EventPoster.IsThread(Async.EAsyncTarget.Logic))
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Async", $"Logic Thread[Async] is Blocked({(cur - t1)/1000}ms > {limit / 1000}):\n{state.ToString()}");
                    }
                    else if (UEngine.Instance.EventPoster.IsThread(Async.EAsyncTarget.Render))
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Async", $"Render Thread[Async] is Blocked({(cur - t1)/1000}ms > {limit / 1000}):\n{state.ToString()}");
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
            Async.TtAsyncTaskStateBase cur;
            var start = Support.Time.HighPrecision_GetTickCount();
            long t1 = 0;
            while ((t1 = Support.Time.HighPrecision_GetTickCount())>0 && DoOnePriorityEvent(out cur))
            {
                cur.Dispose();
                if (TestTimeOut(start, t1, LimitTime, cur))
                {
                    TimeOut = true;
                    return;
                }
            }
            
            while ((t1 = Support.Time.HighPrecision_GetTickCount()) > 0 && DoOneAsyncEvent(out cur))
            {
                if (TestTimeOut(start, t1, LimitTime, cur))
                {
                    TimeOut = true;
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
            lock(RunUntilFinishEvents)
            {
                for (int i = RunUntilFinishEvents.Count - 1; i >= 0; i--)
                {
                    t1 = Support.Time.HighPrecision_GetTickCount();
                    var e = RunUntilFinishEvents[i];
                    bool bFinish = e.ExecutePostEventCondition();
                    if (bFinish)
                    {
                        RunUntilFinishEvents.RemoveAt(i);
                        e.Dispose();
                    }

                    if (TestTimeOut(start, t1, LimitTime, e))
                    {
                        TimeOut = true;
                        return;
                    }
                }
            }
            TimeOut = false;
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
                        e.ContinueThread.ContinueEvents.Enqueue(e);
                    }
                }
            }

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

        public bool IsThisThread()
        {
            return (this.ThreadId == System.Threading.Thread.CurrentThread.ManagedThreadId);
        }
    }
}
