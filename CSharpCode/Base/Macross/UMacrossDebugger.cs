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
        public UMacrossStackFrame BreakStackFrame;
        public UMacrossBreak(string name)
        {
            BreakName = name;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryBreak()
        {
            if (Enable)
            {
                if (Thread.ContextThread.CurrentContext.ThreadId == UEngine.Instance.ThreadMain.ThreadId)
                {
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
                BreakStackFrame = UMacrossStackTracer.CurrentFrame;
                UMacrossDebugger.Instance.CurrrentBreak = this;
                UMacrossDebugger.Instance.mBreakEvent.Reset();
            }
            UMacrossDebugger.Instance.mBreakEvent.WaitOne();
        }
    }
    public class UMacrossDebugger
    {
        public static UMacrossDebugger Instance = new UMacrossDebugger();
        internal System.Threading.AutoResetEvent mBreakEvent = new System.Threading.AutoResetEvent(false);
        internal UMacrossBreak CurrrentBreak;
        public List<UMacrossBreak> Breaks = new List<UMacrossBreak>();
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
                DisableAllBreaks();
            }
        }
        public UMacrossBreak Run()
        {
            lock (Instance)
            {
                if (CurrrentBreak == null)
                    return null;
                var result = CurrrentBreak;
                mBreakEvent = null;
                mBreakEvent.Set();
                return result;
            }
        }
        public void AddBreak(UMacrossBreak brk)
        {
            lock (Instance)
            {
                brk.Enable = true;
                if (Breaks.Contains(brk))
                    return;
                Breaks.Add(brk);
            }   
        }
        public void RemoveBreak(UMacrossBreak brk)
        {
            lock (Instance)
            {
                brk.Enable = false;
                Breaks.Remove(brk);
            }
        }
        public void RemoveAllBreaks()
        {
            lock (Instance)
            {
                foreach (var i in Breaks)
                {
                    i.Enable = false;
                }
                Breaks.Clear();
            }
        }
        public void DisableAllBreaks()
        {
            lock (Instance)
            {
                foreach (var i in Breaks)
                {
                    i.Enable = false;
                }
            }
        }
        public UMacrossBreak FindBreak(string name)
        {
            foreach(var i in Breaks)
            {
                if (i.BreakName == name)
                    return i;
            }
            return null;
        }
    }
}
