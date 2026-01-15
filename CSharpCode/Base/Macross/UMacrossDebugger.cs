using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace EngineNS.Macross
{
    public class TtMacrossBreak
    {
        public Func<IMacrossObject, bool> BreakCondition = null;
        public string BreakName;
        internal bool Enable;
        public TtMacrossStackTracer BreakStack;
        public TtMacrossStackFrame BreakFrame;
        public TtMacrossBreak(string name, bool enable = false)
        {
            Enable = enable;
            BreakName = name;
            TtMacrossDebugger.Instance.AddBreak(this);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsConditionTrue(IMacrossObject owner)
        {
            if (BreakCondition != null)
            {
                return BreakCondition(owner);
            }
            else if (TtMacrossDebugger.Instance.BreakCondition != null)
            {
                return TtMacrossDebugger.Instance.BreakCondition(owner, this);
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryBreak(TtMacrossStackTracer stack = null, IMacrossObject owner = null)
        {
            if (Enable && IsConditionTrue(owner))
            {
                if (TtMacrossDebugger.Instance.CurrrentBreak != null)
                    return;
                TtMacrossDebugger.Instance.CurrrentBreak = this;
                BreakStack = stack;
                BreakFrame = BreakStack?.TopFrame;

                if (Thread.TtContextThread.CurrentContext.ThreadId == TtEngine.Instance.ThreadMain.ThreadId)
                {//主线程上打开断点，只能启动RunLoop来让编辑器处理继续工作
                    TtEngine.Instance.RunEditorLoop_MainThread(() =>
                    {
                        if (Enable == false)
                        {
                            TtEngine.Instance.mIsRunLoop = false;
                        }
                    });
                    return;
                }
                else if (Thread.TtContextThread.CurrentContext.ThreadId == TtEngine.Instance.ThreadLogic.ThreadId)
                {//在逻辑线程上打开断点，关键是给编辑器线程发送信号，让它处理断点
                    TryBreakLogicTread(stack);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);

                    lock (TtMacrossDebugger.Instance)
                    {
                        TtMacrossDebugger.Instance.BreakEvent.Reset();
                    }
                    TtMacrossDebugger.Instance.BreakEvent.WaitOne();
                }
            }
        }
        private void TryBreakLogicTread(TtMacrossStackTracer stack)
        {
            lock (TtMacrossDebugger.Instance)
            {
                TtMacrossDebugger.Instance.BreakEvent.Reset();
                TtEngine.Instance.ThreadLogic.MacrossDebug.Set();
            }
            TtMacrossDebugger.Instance.BreakEvent.WaitOne();
        }
    }

    public class TtMacrossDebugger
    {
        internal static TtMacrossDebugger Instance = new TtMacrossDebugger();
        public Func<IMacrossObject, TtMacrossBreak, bool> BreakCondition = TestBreakCondition;
        public static bool TestBreakCondition(IMacrossObject owner, TtMacrossBreak brk)
        {
            if (owner.MacrossGetter.Name==null)
            {//必须是有名字的Macross对象才允许断点
                if (brk.BreakName != null)
                {

                }
                var game = owner as GamePlay.TtMacrossGame;
                if (game != null)
                {
                    //for example:test instance name...
                }
            }
            return true;
        }
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
            lock (this)
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
            lock (this)
            {
                if (CurrrentBreak == null)
                    return null;
                var result = CurrrentBreak;
                CurrrentBreak = null;

                TtEngine.Instance.ThreadLogic.MacrossDebug.Reset();
                BreakEvent.Set();
                TtEngine.Instance.mIsRunLoop = false;

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
            lock (this)
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
            lock (this)
            {
                brk.Enable = false;
                Breaks.Remove(brk.BreakName);
                mBreakEnableStore.Remove(brk.BreakName);
            }
        }
        public void RemoveAllBreaks()
        {
            lock (this)
            {
                SetBreakStateAll(false);                
                Breaks.Clear();
            }
        }
        public void SetBreakStateAll(bool enable)
        {
            ClearDestroyedBreaks();

            lock (this)
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

namespace EngineNS
{
    partial class TtEngine
    {
        public Macross.TtMacrossDebugger MacrossDebugger
        {
            get => Macross.TtMacrossDebugger.Instance; 
            set => Macross.TtMacrossDebugger.Instance = value; 
        }
    }
}