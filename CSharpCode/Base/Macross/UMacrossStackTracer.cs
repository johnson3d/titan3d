using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.Macross
{
    public class TtMacrossStackFrame : IDisposable
    {
        public RName MacrossName
        {
            get;
            set;
        }
        public Dictionary<string, Support.TtAnyPointer> mFrameStates = new Dictionary<string, Support.TtAnyPointer>();
        public TtMacrossStackFrame()
        {

        }
        public TtMacrossStackFrame(in RName name)
        {
            MacrossName = name;
        }
        public void ClearDebugInfo()
        {
            foreach (var i in mFrameStates)
            {
                i.Value.Dispose();
            }
            mFrameStates.Clear();
        }
        public void Dispose()
        {

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWatchVariable<T>(string name, T value) where T : unmanaged
        {
            Support.TtAnyPointer tmp;
            if (mFrameStates.TryGetValue(name, out tmp))
            {
                tmp.SetValue(value);
            }
            else
            {
                tmp.SetValue(value);
            }
            mFrameStates[name] = tmp;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetWatchVariable(string name, void* value, bool dummy = false)
        {
            Support.TtAnyPointer tmp;
            if (mFrameStates.TryGetValue(name, out tmp))
            {
                tmp.SetValue(value);
            }
            else
            {
                tmp.SetValue(value);
            }
            mFrameStates[name] = tmp;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWatchVariable<T>(string name, T value, bool dummy = false) where T : struct
        {
            Support.TtAnyPointer tmp;
            if (mFrameStates.TryGetValue(name, out tmp))
            {
                tmp.SetValue(value);
            }
            else
            {
                tmp.SetValue(value);
            }
            mFrameStates[name] = tmp;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWatchVariable(string name, object value)
        {
            Support.TtAnyPointer tmp = new Support.TtAnyPointer();
            tmp.SetValue(value);
            mFrameStates[name] = tmp;
        }
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public unsafe void SetWatchVariable(string name, void* value)
        //{
        //    Support.UAnyPointer tmp = new Support.UAnyPointer();
        //    tmp.Value.SetPointer((IntPtr)value);
        //    mFrameStates[name] = tmp;
        //}
        public bool HasWatchVariable(string name)
        {
            return mFrameStates.ContainsKey(name);
        }
        public object GetWatchVariable(string name)
        {
            Support.TtAnyPointer tmp;
            if (mFrameStates.TryGetValue(name, out tmp) == false)
            {
                return null;
            }
            if (tmp.Value.ValueType != Support.TtAnyValue.EValueType.Unknown)
            {
                return tmp.Value.ToObject();
            }
            else
            {
                return tmp.RefObject;
            }
        }
        public void OnCallMethod(string nodeName, System.Reflection.MethodBase method)
        {
            //生成代码的时候做类似处理
            var st = new System.Diagnostics.StackFrame();
            var curFrame = TtMacrossStackTracer.CurrentFrame;
            foreach (var i in method.GetParameters())
            {
                //curFrame.SetWatchVariable($"{nodeName}:{i.Name}", null);//null代码生成的时候传入参数名
            }
        }
    }
    public class TtMacrossStackTracer
    {
        public static List<TtMacrossStackTracer> mThreadMacrossStacks = new List<TtMacrossStackTracer>();
        [ThreadStatic]
        private static TtMacrossStackTracer mThreadInstance;
        public Thread.TtContextThread mThreadContext { get; private set; }
        public static TtMacrossStackTracer ThreadInstance
        {
            get
            {
                if (mThreadInstance == null)
                {
                    mThreadInstance = new TtMacrossStackTracer();
                    mThreadInstance.mThreadContext = Thread.TtContextThread.CurrentContext;
                    lock (mThreadMacrossStacks)
                    {
                        mThreadMacrossStacks.Add(mThreadInstance);
                    }
                }
                return mThreadInstance;
            }
        }
        public Stack<TtMacrossStackFrame> mFrames = new Stack<TtMacrossStackFrame>();
        public static TtMacrossStackFrame CurrentFrame
        {
            get
            {
                if (TtMacrossDebugger.Instance.IsEnableDebugger == false)
                    return null;
                if (ThreadInstance.mFrames.Count == 0)
                    return null;
                return ThreadInstance.mFrames.Peek();
            }
        }
        public static void PushFrame(TtMacrossStackFrame frame)
        {
            if (TtMacrossDebugger.Instance.IsEnableDebugger == false)
                return;
            ThreadInstance.mFrames.Push(frame);
        }
        public static void PopFrame()
        {
            if (TtMacrossDebugger.Instance.IsEnableDebugger == false)
                return;
            if (ThreadInstance.mFrames.Count > 0)
            {
                var cur = ThreadInstance.mFrames.Peek();
                cur.ClearDebugInfo();
                ThreadInstance.mFrames.Pop();
            }
            else
            {
                System.Diagnostics.Debugger.Break();
            }
        }
    }
    public struct TtMacrossStackGuard : IDisposable
    {
        public TtMacrossStackGuard(TtMacrossStackFrame frame)
        {
            TtMacrossStackTracer.PushFrame(frame);
        }
        public void Dispose()
        {
            TtMacrossStackTracer.PopFrame();
        }
    }
}

namespace EngineNS.UTest
{
    [UTest.UTest]
    class UTest_UMacrossStackTracer
    {

/* 项目“Engine.Window”的未合并的更改
在此之前:
        Macross.UMacrossStackFrame mFrame_UnitTestEntrance = new Macross.UMacrossStackFrame() { MacrossName = RName.GetRName("") };
        public unsafe void UnitTestEntrance()
在此之后:
        Macross.TtMacrossStackFrame mFrame_UnitTestEntrance = new Macross.TtMacrossStackFrame() { MacrossName = RName.GetRName("") };
        public unsafe void UnitTestEntrance()
*/

/* 项目“Engine.Window”的未合并的更改
在此之前:
        Macross.TtMacrossStackFrame mFrame_UnitTestEntrance = new Macross.TtMacrossStackFrame() { MacrossName = RName.GetRName("") };
        public unsafe void UnitTestEntrance()
在此之后:
        Macross.UMacrossStackFrame mFrame_UnitTestEntrance = new Macross.UMacrossStackFrame() { MacrossName = RName.GetRName("") };
        public unsafe void UnitTestEntrance()
*/
        Macross.TtMacrossStackFrame mFrame_UnitTestEntrance = new Macross.TtMacrossStackFrame() { MacrossName = RName.GetRName("") };
        public unsafe void UnitTestEntrance()
        {
            using(var guard = new Macross.TtMacrossStackGuard(mFrame_UnitTestEntrance))
            {
                float v = 3;
                MathHelper.macross_Abs("static MathHelper.Abs(float v)", v);

                var frame = Macross.TtMacrossStackTracer.CurrentFrame;
                foreach (var i in frame.mFrameStates)
                {
                    var name = i.Key;
                    float debug_v = (float)i.Value.ToObject();
                }
                if (Macross.TtMacrossDebugger.Instance.CurrrentBreak != null)
                {//如果break，在别的线程可以通过这个获得break的framestate
                    foreach (var i in Macross.TtMacrossDebugger.Instance.CurrrentBreak.BreakStackFrame.mFrameStates)
                    {

                    }
                }
            }
        }
    }
}

