using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace EngineNS.Macross
{
    public class TtMacrossBreak
    {
        public string BreakName;
        internal bool Enable;
        public TtMacrossStackTracer StackTracer;
        public TtMacrossStackFrame BreakStackFrame;
        public TtMacrossBreak(string name, bool enable = false)
        {
            Enable = enable;
            BreakName = name;
            TtMacrossDebugger.Instance.AddBreak(this);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryBreak()
        {
            if (Enable)
            {
                if (Thread.TtContextThread.CurrentContext.ThreadId == TtEngine.Instance.ThreadMain.ThreadId)
                {//不能在主线程break住，否则没法调试了
                    return;
                }
                TryBreakInner();
            }
        }
        private void TryBreakInner()
        {
            lock (TtMacrossDebugger.Instance)
            {
                if (TtMacrossDebugger.Instance.CurrrentBreak != null)
                    return;
                StackTracer = TtMacrossStackTracer.ThreadInstance;

                BreakStackFrame = TtMacrossStackTracer.CurrentFrame;
                TtMacrossDebugger.Instance.CurrrentBreak = this;
                TtMacrossDebugger.Instance.BreakEvent.Reset();
                TtEngine.Instance.ThreadLogic.MacrossDebug.Set();
            }
            TtMacrossDebugger.Instance.BreakEvent.WaitOne();
        }
    }

    public class TtMacrossDebugger
    {
        public static TtMacrossDebugger Instance = new TtMacrossDebugger();
        internal System.Threading.AutoResetEvent BreakEvent { get; } = new System.Threading.AutoResetEvent(false);
        internal TtMacrossBreak CurrrentBreak;
        public Dictionary<string, WeakReference<TtMacrossBreak>> Breaks = new();
        internal Dictionary<string, bool> mBreakEnableStore = new Dictionary<string, bool>();
        private bool mIsEnableDebugger = true;
        public bool IsEnableDebugger
        {
            get => mIsEnableDebugger;
        }
        public void EnableDebugger(bool enable)
        {
            mIsEnableDebugger = enable;
            if (enable == false)
            {
                SetBreakStateAll(false);
            }
        }
        public void ClearDestroyedBreaks()
        {
            lock (Instance)
            {
                List<string> rvm = null;
                foreach(var i in Breaks)
                {
                    if (i.Value.TryGetTarget(out var tmp) == false)
                    {
                        if (rvm == null)
                            rvm = new List<string>();
                        rvm.Add(i.Key);
                    }
                }
                if (rvm != null)
                {
                    foreach(var i in rvm)
                    {
                        Breaks.Remove(i);
                    }
                }
            }
        }
        public TtMacrossBreak Run()
        {
            lock (Instance)
            {
                if (CurrrentBreak == null)
                    return null;
                var result = CurrrentBreak;
                CurrrentBreak = null;

                TtEngine.Instance.ThreadLogic.MacrossDebug.Reset();
                BreakEvent.Set();

                return result;
            }
        }
        public void SetBreakEnable(string breakName, bool enable)
        {
            var breaker = FindBreak(breakName);
            if(breaker != null)
            {
                breaker.Enable = enable;
                mBreakEnableStore[breakName] = enable;
            }
            else
            {
                mBreakEnableStore[breakName] = enable;
            }
        }
        public void AddBreak(TtMacrossBreak brk)
        {
            lock (Instance)
            {
                Breaks[brk.BreakName] = new WeakReference<TtMacrossBreak>(brk);

                if (mBreakEnableStore.TryGetValue(brk.BreakName, out var eb))
                {
                    brk.Enable = eb;
                }
            }   
        }
        public void RemoveBreak(TtMacrossBreak brk)
        {
            lock (Instance)
            {
                brk.Enable = false;
                Breaks.Remove(brk.BreakName);
                mBreakEnableStore.Remove(brk.BreakName);
            }
        }
        public void RemoveAllBreaks()
        {
            lock (Instance)
            {
                SetBreakStateAll(false);                
                Breaks.Clear();
            }
        }
        public void SetBreakStateAll(bool enable)
        {
            ClearDestroyedBreaks();

            lock (Instance)
            {
                foreach (var i in Breaks.Values)
                {
                    TtMacrossBreak tmp;
                    if (i.TryGetTarget(out tmp))
                    {
                        tmp.Enable = enable;
                    }
                }
            }
        }
        public TtMacrossBreak FindBreak(string name)
        {
            if (Breaks.TryGetValue(name, out var v))
            {
                TtMacrossBreak tmp;
                if (v.TryGetTarget(out tmp))
                {
                    return tmp;
                }
            }
            return null;
        }
    }
}
