using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.Macross
{
    public class UMacrossStackFrame : IDisposable
    {
        public Dictionary<string, object> mFrameStates = new Dictionary<string, object>();
        public void Dispose()
        {

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWatchVariable<T>(string name, T value)
        {
            mFrameStates[name] = value;
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
        [ThreadStatic]
        private static UMacrossStackTracer ThreadInstance = new UMacrossStackTracer();
        public static UMacrossStackTracer GetStackTracer()
        {
            return ThreadInstance;
        }
        public Stack<UMacrossStackFrame> mFrames = new Stack<UMacrossStackFrame>();
        public static UMacrossStackFrame CurrentFrame
        {
            get
            {
                if (ThreadInstance.mFrames.Count == 0)
                    return null;
                return ThreadInstance.mFrames.Peek();
            }
        }
        public static void PushFrame(UMacrossStackFrame frame)
        {
            ThreadInstance.mFrames.Push(frame);
        }
        public static void PopFrame()
        {
            ThreadInstance.mFrames.Pop();
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
        Macross.UMacrossStackFrame mFrame_UnitTestEntrance = new Macross.UMacrossStackFrame();
        public unsafe void UnitTestEntrance()
        {
            using(var guard = new Macross.UMacrossStackGuard(mFrame_UnitTestEntrance))
            {

            }
        }
    }
}

