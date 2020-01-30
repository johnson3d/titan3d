using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class ThreadLogic : ContextThread
    {
        public ThreadLogic()
        {
            Async.ContextThreadManager.Instance.ContextLogic = this;
            this.Interval = 0;
        }
        public System.Threading.AutoResetEvent mLogicBegin = new System.Threading.AutoResetEvent(false);
        public System.Threading.AutoResetEvent mLogicEnd = new System.Threading.AutoResetEvent(false);
        public bool IsTicking
        {
            get;
            private set;
        }= false;
        public override void Tick()
        {
            mLogicBegin.WaitOne();
            mLogicBegin.Reset();
            IsTicking = true;

            TickAwaitEvent();
            CEngine.Instance.TryTickLogic();

            IsTicking = false;
            mLogicEnd.Set();
        }
        protected override void OnThreadStart()
        {
            Thread_StartLogicThread();
        }
        protected override void OnThreadExited()
        {

        }
        public override bool StartThread(string name, FOnThreadTick action)
        {
            mLogicBegin.Reset();
            mLogicEnd.Reset();
            return base.StartThread(name, action);
        }
        public override void StopThread(Action waitAction)
        {
            base.StopThread(()=>
            {
                CEngine.Instance.StartFrame();
            });
        }
    }
}
