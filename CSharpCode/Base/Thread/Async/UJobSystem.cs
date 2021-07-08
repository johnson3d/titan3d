using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread.Async
{
    public interface IJob
    {
        IJobSystem JobSystem { get; set; }
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
        Async.PostEvent PostEvent { get; set; }
    }
    //public class UJob : IJob
    //{
    //    public IJobSystem JobSystem { get; set; }        
    //    public void DoWork()
    //    {
            
    //    }
    //}
    public class UJobThread<T> : IJobThread where T : IJob
    {
        public List<T> Jobs = new List<T>();
        public void DoWorks()
        {
            for (int i = 0; i < Jobs.Count; i++)
            {
                Jobs[i].DoWork();
                Jobs[i].JobSystem.ReleaseTask();
            }
            Jobs.Clear();
        }
    }
    public class UJobSystem<T> : IJobSystem where T : IJob
    {
        public UJobThread<T>[] JobThreads = new UJobThread<T>[UEngine.Instance.ContextThreadManager.PooledThreadNum];
        private int NumOfRemainTasks = 0;
        public List<T> Jobs = new List<T>();
        private System.Threading.AutoResetEvent mFinishEvent = new System.Threading.AutoResetEvent(false);
        public Async.PostEvent PostEvent { get; set; }
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
                    PostEvent.ContinueThread.ContinueEvents.Enqueue(PostEvent);
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
        private UJobThread<T> GetJobThread()
        {
            CurThread++;
            if (CurThread >= JobThreads.Length)
            {
                CurThread = 0;
            }
            if (JobThreads[CurThread] == null)
                JobThreads[CurThread] = new UJobThread<T>();
            return JobThreads[CurThread];
        }
        public void AddJob(ref T job)
        {
            job.JobSystem = this;
            System.Threading.Interlocked.Increment(ref NumOfRemainTasks);
            var thread = GetJobThread();
            thread.Jobs.Add(job);
        }
        public System.Action<IJobSystem> OnFinished;
        public void StartJobs()
        {
            mFinishEvent.Reset();
            for(int i=0; i< JobThreads.Length; i++)
            {
                UEngine.Instance.ContextThreadManager.ContextPools[i].AddJobThread(JobThreads[i]);
            }
            UEngine.Instance.ContextThreadManager.mTPoolTrigger.Set();
        }
        public void Wait()
        {
            mFinishEvent.WaitOne();
        }
        public async System.Threading.Tasks.Task Await()
        {
            await UEngine.Instance.ContextThreadManager.AwaitJobSystem(this);
        }
    }
}

namespace EngineNS.UTest
{
    public struct UTestJob : Thread.Async.IJob
    {
        public Thread.Async.IJobSystem JobSystem { get; set; }
        public UTest_UJobSystem Data;
        public void DoWork()
        {
            System.Threading.Interlocked.Increment(ref Data.NumSum);
        }
    }
    [UTest.UTest]
    public class UTest_UJobSystem
    {
        public static bool IsFinal = false;
        ~UTest_UJobSystem()
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
            var jobSystem = new Thread.Async.UJobSystem<UTestJob>();
            for (int i = 0; i < 1000; i++)
            {
                var job = new UTestJob();
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
