using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Runner
{
    public class MacrossDebugContext
    {
        public bool CanBreak = false;
        public Thread.Async.EAsyncTarget CurrentAsyncTarget;
        public bool EventRunnerFinished = false;

        public MacrossDebugContext()
        {
        }
    }

    public class MacrossDebugger
    {
        public void StartDebug()
        {
            EngineNS.CEngine.Instance.EventPoster.RunOn()
        }
    }
}
