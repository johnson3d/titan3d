using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangHeadTools
{
    public class IBuilderTask
    {
        public enum ETaskState
        {
            Enqueue,
            Executing,
            Finished,
        }
        public ETaskState State = ETaskState.Enqueue;
        public virtual void Execute(TaskThread thead)
        {

        }
    }
    public class TaskThread
    {
        public System.Threading.Thread mThread;
        public Queue<IBuilderTask> Tasks = new Queue<IBuilderTask>();
        private bool mRunning = true;
        public bool IsFinshed { get; private set; } = false;
        public void Start()
        {
            mThread = new System.Threading.Thread(() =>
            {
                this.Proccess();
            });
            mThread.Start();
        }
        public void Stop()
        {
            mRunning = false;
        }
        public virtual void Proccess()
        {
            while (mRunning)
            {
                while (Tasks.Count > 0)
                {
                    var cur = Tasks.Dequeue();
                    cur.State = IBuilderTask.ETaskState.Executing;
                    cur.Execute(this);
                    cur.State = IBuilderTask.ETaskState.Finished;
                }
                System.Threading.Thread.Sleep(10);
            }
            IsFinshed = true;
        }
    }
    public class TaskThreadManager
    {
        public static TaskThreadManager Instance = new TaskThreadManager();
        public List<TaskThread> TaskThreads = new List<TaskThread>();
        public void InitThreads(int n)
        {
            for (int i = 0; i < n; i++)
            {
                var t = new TaskThread();
                t.Start();
                TaskThreads.Add(t);
            }
        }
        public void StartThreads()
        {
            for (int i = 0; i < TaskThreads.Count; i++)
            {
                TaskThreads[i].Start();
            }
        }
        public void DispatchTask(IBuilderTask task)
        {
            TaskThread thread = null;
            int nMinCount = int.MaxValue;
            for (int i = 0; i < TaskThreads.Count; i++)
            {
                if (TaskThreads[i].Tasks.Count < nMinCount)
                {
                    nMinCount = TaskThreads[i].Tasks.Count;
                    thread = TaskThreads[i];
                }
            }
            if (thread != null)
            {
                thread.Tasks.Enqueue(task);
            }
        }
        public void WaitAllThreadFinished()
        {
            while(true)
            {
                int nFinished = 0;
                for (int i = 0; i < TaskThreads.Count; i++)
                {
                    if (TaskThreads[i].IsFinshed)
                    {
                        nFinished++;
                    }
                }
                if (nFinished == TaskThreads.Count)
                {
                    break;
                }
            }
        }
    }
}
