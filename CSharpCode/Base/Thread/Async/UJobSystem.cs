using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread.Async
{
    public enum EJobState
    { 
        WaitStart,
        Working,
        Finished,
    }
    public interface IJob
    {
        IJobSystem JobSystem { get; set; }
        EJobState JobState { get; set; }
        void DoWork();
    }
    public interface IJobThread
    {
        void DoWorks();
    }
    public interface IJobSystem
    {
        void ReleaseTask();
        void FinishJobs();
        bool IsFinshed();
        Async.TtAsyncTaskStateBase PostEvent { get; set; }
    }
    //public class UJob : IJob
    //{
    //    public IJobSystem JobSystem { get; set; }        
    //    public void DoWork()
    //    {
            
    //    }
    //}
    public class TtJobThread<T> : IJobThread where T : IJob
    {
        public List<T> Jobs = new List<T>();
        public void DoWorks()
        {
            for (int i = 0; i < Jobs.Count; i++)
            {
                try
                {
                    Jobs[i].DoWork();
                    Jobs[i].JobState = EJobState.Finished;
                }
                catch(Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
                finally
                {
                    Jobs[i].JobSystem.ReleaseTask();
                }
            }
            Jobs.Clear();
        }
    }
    public class TtJobSystem<T> : IJobSystem where T : IJob
    {
        public TtJobThread<T>[] JobThreads = new TtJobThread<T>[TtEngine.Instance.ContextThreadManager.PooledThreadNum];
        private int NumOfRemainTasks = 0;
        public List<T> Jobs = new List<T>();
        private System.Threading.AutoResetEvent mFinishEvent = new System.Threading.AutoResetEvent(false);
        public Async.TtAsyncTaskStateBase PostEvent { get; set; }
        public void ReleaseTask()
        {
            var num = System.Threading.Interlocked.Decrement(ref NumOfRemainTasks);
            if (num == 0)
            {
                mFinishEvent.Set();
                if (OnFinished != null)
                {
                    OnFinished(this);
                }
                if (PostEvent != null)
                {
                    PostEvent.ContinueThread.EnqueueContinue(PostEvent);
                }
            }
        }
        public void FinishJobs()
        {
            mFinishEvent.Set();
        }
        public bool IsFinshed()
        {
            return NumOfRemainTasks == 0;
        }
        private int CurThread = 0;
        private TtJobThread<T> GetJobThread()
        {
            CurThread++;
            if (CurThread >= JobThreads.Length)
            {
                CurThread = 0;
            }
            if (JobThreads[CurThread] == null)
                JobThreads[CurThread] = new TtJobThread<T>();
            return JobThreads[CurThread];
        }
        public void AddJob(ref T job)
        {
            job.JobSystem = this;
            System.Threading.Interlocked.Increment(ref NumOfRemainTasks);
            var thread = GetJobThread();
            thread.Jobs.Add(job);
            job.JobState = EJobState.Working;
        }
        public System.Action<IJobSystem> OnFinished;
        public void StartJobs()
        {
            mFinishEvent.Reset();
            for(int i=0; i< JobThreads.Length; i++)
            {
                TtEngine.Instance.ContextThreadManager.ContextPools[i].AddJobThread(JobThreads[i]);
            }
            TtEngine.Instance.ContextThreadManager.mTPoolTrigger.Set();
        }
        public void Wait()
        {
            mFinishEvent.WaitOne();
        }
        public async System.Threading.Tasks.Task Await()
        {
            await TtEngine.Instance.ContextThreadManager.AwaitJobSystem(this);
        }
    }
}

namespace EngineNS.UTest
{
    public struct TtTestJob : Thread.Async.IJob
    {
        public Thread.Async.IJobSystem JobSystem { get; set; }
        public Thread.Async.EJobState JobState { get; set; }
        public TtTest_UJobSystem Data;
        public void DoWork()
        {
            System.Threading.Interlocked.Increment(ref Data.NumSum);
        }
    }
    [UTest.UTest]
    public class TtTest_UJobSystem
    {
        public static bool IsFinal = false;
        ~TtTest_UJobSystem()
        {
            NumSum = 0;
            IsFinal = true;
        }
        public int NumSum = 0;
        public void UnitTestEntrance()
        {
            var noused = AsyncTest();
        }
        public async System.Threading.Tasks.Task AsyncTest()
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            var jobSystem = new Thread.Async.TtJobSystem<TtTestJob>();
            for (int i = 0; i < 1000; i++)
            {
                var job = new TtTestJob();
                job.Data = this;
                jobSystem.AddJob(ref job);
            }
            jobSystem.OnFinished = (jobSystem) =>
            {
                UnitTestManager.TAssert(this.NumSum == 1000, "?");
            };
            jobSystem.StartJobs();

            //await jobSystem.Await();
            jobSystem.Wait();
            foreach (var i in jobSystem.JobThreads)
            {
                //UnitTestManager.TAssert(i.Jobs.Count == 0, "?");
                //这个断言还真不一定能保证，foreach 后才clear的
            }
            UnitTestManager.TAssert(this.NumSum == 1000, "?");
        }
    }
}
