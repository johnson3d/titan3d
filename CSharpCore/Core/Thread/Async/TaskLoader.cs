using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread.Async
{
    public interface IWaitLoader
    {
        void OnWaitLoaderSetFinished(int waitCount);
    }
    public class TaskLoader
    {
        public class WaitContext
        {
            public List<Thread.ASyncSemaphore> WaitCreateds = new List<ASyncSemaphore>();
            private bool mIsFinished = false;
            public bool IsFinished
            {
                get { return mIsFinished; }
            }
            internal void SetFinished(object host)
            {
                var wtl = host as IWaitLoader;
                if(wtl!=null)
                {
                    wtl.OnWaitLoaderSetFinished(WaitCreateds.Count);
                }
                mIsFinished = true;
            }
            public object Result;
        }
        public static async System.Threading.Tasks.Task<WaitContext> Awaitload(WaitContext context)
        {
            if (context==null || context.IsFinished)
                return context;

            var smp = Thread.ASyncSemaphore.CreateSemaphore(1);
            lock (context)
            {
                context.WaitCreateds.Add(smp);
            }
            await smp.Await();
            lock (context)
            {
                context.WaitCreateds.Remove(smp);
            }
            return context;
        }
        public static void Release(ref WaitContext context, object result)
        {
            if (context == null)
                return;
            lock (context)
            {
                context.Result = result;
                context.SetFinished(result);
                for (int i = 0; i < context.WaitCreateds.Count; i++)
                {
                    context.WaitCreateds[i].Release();
                }
                context.WaitCreateds.Clear();
            }
            //context = null;
        }
    }
}
