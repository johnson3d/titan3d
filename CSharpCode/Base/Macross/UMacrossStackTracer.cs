﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.Macross
{
    public class UMacrossStackFrame : IDisposable
    {
        public RName MacrossName
        {
            get;
            set;
        }
        public Dictionary<string, Support.TtAnyPointer> mFrameStates = new Dictionary<string, Support.TtAnyPointer>();
        public UMacrossStackFrame()
        {

        }
        public UMacrossStackFrame(in RName name)
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
            var curFrame = UMacrossStackTracer.CurrentFrame;
            foreach (var i in method.GetParameters())
            {
                //curFrame.SetWatchVariable($"{nodeName}:{i.Name}", null);//null代码生成的时候传入参数名
            }
        }
    }
    public class UMacrossStackTracer
    {
        public static List<UMacrossStackTracer> mThreadMacrossStacks = new List<UMacrossStackTracer>();
        [ThreadStatic]
        private static UMacrossStackTracer mThreadInstance;
        public Thread.TtContextThread mThreadContext { get; private set; }
        public static UMacrossStackTracer ThreadInstance
        {
            get
            {
                if (mThreadInstance == null)
                {
                    mThreadInstance = new UMacrossStackTracer();
                    mThreadInstance.mThreadContext = Thread.TtContextThread.CurrentContext;
                    lock (mThreadMacrossStacks)
                    {
                        mThreadMacrossStacks.Add(mThreadInstance);
                    }
                }
                return mThreadInstance;
            }
        }
        public Stack<UMacrossStackFrame> mFrames = new Stack<UMacrossStackFrame>();
        public static UMacrossStackFrame CurrentFrame
        {
            get
            {
                if (UMacrossDebugger.Instance.IsEnableDebugger == false)
                    return null;
                if (ThreadInstance.mFrames.Count == 0)
                    return null;
                return ThreadInstance.mFrames.Peek();
            }
        }
        public static void PushFrame(UMacrossStackFrame frame)
        {
            if (UMacrossDebugger.Instance.IsEnableDebugger == false)
                return;
            ThreadInstance.mFrames.Push(frame);
        }
        public static void PopFrame()
        {
            if (UMacrossDebugger.Instance.IsEnableDebugger == false)
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

    public struct UMacrossStackGuard : IDisposable
    {
        public UMacrossStackGuard(UMacrossStackFrame frame)
        {
            UMacrossStackTracer.PushFrame(frame);
        }
        public void Dispose()
        {
            UMacrossStackTracer.PopFrame();
        }
    }
}

namespace EngineNS.UTest
{
    [UTest.UTest]
    class UTest_UMacrossStackTracer
    {
        Macross.UMacrossStackFrame mFrame_UnitTestEntrance = new Macross.UMacrossStackFrame() { MacrossName = RName.GetRName("") };
        public unsafe void UnitTestEntrance()
        {
            using(var guard = new Macross.UMacrossStackGuard(mFrame_UnitTestEntrance))
            {
                float v = 3;
                MathHelper.macross_Abs("static MathHelper.Abs(float v)", v);

                var frame = Macross.UMacrossStackTracer.CurrentFrame;
                foreach (var i in frame.mFrameStates)
                {
                    var name = i.Key;
                    float debug_v = (float)i.Value.ToObject();
                }
                if (Macross.UMacrossDebugger.Instance.CurrrentBreak != null)
                {//如果break，在别的线程可以通过这个获得break的framestate
                    foreach (var i in Macross.UMacrossDebugger.Instance.CurrrentBreak.BreakStackFrame.mFrameStates)
                    {

                    }
                }
            }
        }
    }
}

