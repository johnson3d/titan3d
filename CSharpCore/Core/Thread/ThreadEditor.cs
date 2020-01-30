using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class ThreadEditor : ContextThread
    {
        public ThreadEditor()
        {
            Async.ContextThreadManager.Instance.ContextEditor = this;
        }
        public override void Tick()
        {
            TickFlag = 3;
#if PWindow
            var saved = System.Threading.SynchronizationContext.Current;
            System.Threading.SynchronizationContext.SetSynchronizationContext(null);
            this.TickAwaitEvent();
            System.Threading.SynchronizationContext.SetSynchronizationContext(saved);
#else
            this.TickAwaitEvent();
#endif
            TickFlag = 0;
        }
    }
}
