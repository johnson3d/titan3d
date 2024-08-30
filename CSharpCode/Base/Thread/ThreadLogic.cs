using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class TtThreadLogic : TtContextThread
    {
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtThreadLogic), nameof(Tick));
                return mScopeTick;
            }
        } 
        public TtThreadLogic()
        {
            this.Interval = 0;
            LogicEndEvents[(int)EEndEvent.Normal] = LogicEnd;
            LogicEndEvents[(int)EEndEvent.MacrossDebug] = MacrossDebug;
        }
        internal enum EEndEvent : int
        {
            Normal = 0,
            MacrossDebug,
            Number,
        }
        public System.Threading.AutoResetEvent LogicBegin { get; } = new System.Threading.AutoResetEvent(false);
        public System.Threading.AutoResetEvent LogicEnd { get; } = new System.Threading.AutoResetEvent(false);
        public System.Threading.EventWaitHandle MacrossDebug { get; } = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);
        public System.Threading.EventWaitHandle[] LogicEndEvents { get; } = new System.Threading.EventWaitHandle[(int)EEndEvent.Number];
        public bool IsTicking
        {
            get;
            private set;
        }= false;
        public override void Tick()
        {
            LogicBegin.WaitOne();
            LogicBegin.Reset();
            IsTicking = true;

            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                TickAwaitEvent();
                TtEngine.Instance.TryTickLogic();
            }

            TtEngine.Instance.GfxDevice?.RenderSwapQueue?.QueueCmd(static (NxRHI.ICommandList im_cmd, ref NxRHI.FRCmdInfo info) =>
            {

            }, "#TickLogicEnd#");
            IsTicking = false;
            LogicEnd.Set();
        }
        protected override void OnThreadStart()
        {
            
        }
        protected override void OnThreadExited()
        {

        }
        public override bool StartThread(string name, FOnThreadTick action)
        {
            LogicBegin.Reset();
            LogicEnd.Reset();
            return base.StartThread(name, action);
        }
        public override void StopThread(Action waitAction)
        {
            base.StopThread(()=>
            {
                TtEngine.Instance.StartFrame();
            });
        }
    }
}
