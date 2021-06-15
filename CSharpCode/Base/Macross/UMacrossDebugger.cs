using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace EngineNS.Macross
{
    public class UMacrossBreak
    {
        public string BreakName;
        internal bool Enable;
        public UMacrossStackTracer StackTracer;
        public UMacrossStackFrame BreakStackFrame;
        public UMacrossBreak(string name, bool enable = false)
        {
            Enable = enable;
            BreakName = name;
            UMacrossDebugger.Instance.AddBreak(this);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryBreak()
        {
            if (Enable)
            {
                if (Thread.ContextThread.CurrentContext.ThreadId == UEngine.Instance.ThreadMain.ThreadId)
                {//不能在主线程break住，否则没法调试了
                    return;
                }
                TryBreakInner();
            }
        }
        private void TryBreakInner()
        {
            lock (UMacrossDebugger.Instance)
            {
                if (UMacrossDebugger.Instance.CurrrentBreak != null)
                    return;
                StackTracer = UMacrossStackTracer.ThreadInstance;
                BreakStackFrame = UMacrossStackTracer.CurrentFrame;
                UMacrossDebugger.Instance.CurrrentBreak = this;
                UMacrossDebugger.Instance.mBreakEvent.Reset();
                UEngine.Instance.ThreadLogic.mMacrossDebug.Set();
            }
            UMacrossDebugger.Instance.mBreakEvent.WaitOne();
        }
    }
    public class UMacrossDebugger
    {
        public static UMacrossDebugger Instance = new UMacrossDebugger();
        internal System.Threading.AutoResetEvent mBreakEvent = new System.Threading.AutoResetEvent(false);
        internal UMacrossBreak CurrrentBreak;
        public List<WeakReference<UMacrossBreak>> Breaks = new List<WeakReference<UMacrossBreak>>();
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
                for (int i = 0; i < Breaks.Count; i++)
                {
                    UMacrossBreak tmp;
                    if (Breaks[i].TryGetTarget(out tmp) == false)
                    {
                        Breaks.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        public UMacrossBreak Run()
        {
            lock (Instance)
            {
                if (CurrrentBreak == null)
                    return null;
                var result = CurrrentBreak;
                CurrrentBreak = null;

                UEngine.Instance.ThreadLogic.mMacrossDebug.Reset();
                mBreakEvent.Set();

                return result;
            }
        }
        public void AddBreak(UMacrossBreak brk)
        {
            lock (Instance)
            {
                brk.Enable = true;
                foreach(var i in Breaks)
                {
                    UMacrossBreak tmp;
                    if (i.TryGetTarget(out tmp))
                    {
                        if (tmp == brk)
                            return;
                    }
                }
                Breaks.Add(new WeakReference<UMacrossBreak>(brk));
            }   
        }
        public void RemoveBreak(UMacrossBreak brk)
        {
            lock (Instance)
            {
                brk.Enable = false;
                foreach (var i in Breaks)
                {
                    UMacrossBreak tmp;
                    if (i.TryGetTarget(out tmp))
                    {
                        if (tmp == brk)
                        {
                            Breaks.Remove(i);
                            return;
                        }
                    }
                }
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
                foreach (var i in Breaks)
                {
                    UMacrossBreak tmp;
                    if (i.TryGetTarget(out tmp))
                    {
                        tmp.Enable = enable;
                    }
                }
            }
        }
        public UMacrossBreak FindBreak(string name)
        {
            foreach(var i in Breaks)
            {
                UMacrossBreak tmp;
                if (i.TryGetTarget(out tmp))
                {
                    if (tmp.BreakName == name)
                        return tmp;
                }
            }
            return null;
        }
    }
}
