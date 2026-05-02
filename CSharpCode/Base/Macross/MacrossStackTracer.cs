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
        //public void OnCallMethod(string nodeName, System.Reflection.MethodBase method)
        //{
        //    //生成代码的时候做类似处理
        //    var st = new System.Diagnostics.StackFrame();
        //    var curFrame = TtMacrossStackTracer.CurrentFrame;
        //    foreach (var i in method.GetParameters())
        //    {
        //        //curFrame.SetWatchVariable($"{nodeName}:{i.Name}", null);//null代码生成的时候传入参数名
        //    }
        //}
    }
    public class TtMacrossStackTracer
    {
        public static List<TtMacrossStackTracer> mThreadMacrossStacks = new List<TtMacrossStackTracer>();
        [ThreadStatic]
        private static TtMacrossStackTracer mThreadInstance;
        public Thread.TtContextThread mThreadContext { get; private set; }
        internal static TtMacrossStackTracer ThreadInstance
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
        public TtMacrossStackFrame TopFrame
        {
            get
            {
                if (mFrames.Count == 0)
                    return null;
                return mFrames.Peek();
            }
        }
        //[ThreadStatic]
        static int mStackDepth = 0;
        public static void PushFrame(TtMacrossStackTracer stack, TtMacrossStackFrame frame)
        {
            System.Threading.Interlocked.Increment(ref mStackDepth);
            if (TtMacrossDebugger.Instance.IsEnableDebugger == false)
                return;

            stack?.mFrames.Push(frame);
        }
        public static void PopFrame(TtMacrossStackTracer stack, TtMacrossStackFrame frame)
        {
            System.Threading.Interlocked.Decrement(ref mStackDepth);
            if (TtMacrossDebugger.Instance.IsEnableDebugger == false)
                return;

            if (stack !=null && stack.mFrames.Count > 0)
            {
                var cur = stack.mFrames.Peek();
                //System.Diagnostics.Debug.Assert(cur == frame);
                frame.ClearDebugInfo();
                stack.mFrames.Pop();
            }
        }
    }
    public struct TtMacrossStackGuard : IDisposable
    {
        public TtMacrossStackTracer mStack;
        public TtMacrossStackFrame mFrame;
        public TtMacrossStackGuard(TtMacrossStackTracer stack, TtMacrossStackFrame frame)
        {
            mStack = stack;
            TtMacrossStackTracer.PushFrame(mStack, frame);
            mFrame = frame;
        }
        public TtMacrossStackGuard(TtMacrossStackFrame frame)
        {
            mStack = TtMacrossStackTracer.ThreadInstance;
            TtMacrossStackTracer.PushFrame(mStack, frame);
            mFrame = frame;
        }
        public void Dispose()
        {
            TtMacrossStackTracer.PopFrame(mStack, mFrame);
        }
    }
}

namespace EngineNS.UnitTest
{
    [UnitTest.TtTest]
    class UTest_UMacrossStackTracer
    {
        Macross.TtMacrossStackTracer mStack_UnitTestEntrance = new Macross.TtMacrossStackTracer();
        Macross.TtMacrossStackFrame mFrame_UnitTestEntrance = new Macross.TtMacrossStackFrame() { MacrossName = RName.GetRName("") };
        public unsafe void UnitTestEntrance()
        {
            using(var guard = new Macross.TtMacrossStackGuard(mStack_UnitTestEntrance, mFrame_UnitTestEntrance))
            {
                float v = 3;
                //MathHelper.macross_Abs(mStack_UnitTestEntrance, "static MathHelper.Abs(float v)", v);
                MathHelper.Abs(v);
                mFrame_UnitTestEntrance.SetWatchVariable("v", v);

                //var frame = Macross.TtMacrossStackTracer.CurrentFrame;
                //foreach (var i in frame.mFrameStates)
                //{
                //    var name = i.Key;
                //    float debug_v = (float)i.Value.ToObject();
                //}
            }
        }
    }
}

