using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Runtime.InteropServices;


namespace EngineNS.Profiler
{
    public enum ELogTag
    {
        [Description("消息")]
        Info,
        [Description("警告")]
        Warning,
        [Description("错误")]
        Error,
        [Description("严重")]
        Fatal,
    }
    public unsafe partial class Log
    {
        public delegate void Delegate_OnReportLog(ELogTag tag, string category, string format, params object[] args);
        public static event Delegate_OnReportLog OnReportLog;
        
        private static CoreSDK.FDelegate_FWriteLogString NativeLogger = NativeWriteLogString;
#if PlatformIOS
        [ObjCRuntime.MonoPInvokeCallback(typeof(FDelegate_WriteLogString))]
#endif
        private static unsafe void NativeWriteLogString(void* threadName, void* logStr, ELevelTraceType level, void* file, int line)
        {
            var thread = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)threadName);
            var logContent = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)logStr);
            var logFile = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)file);
            switch(level)
            {
                case ELevelTraceType.ELTT_Resource:
                    {
                        System.Diagnostics.Trace.WriteLine($"{logFile}:{line}:[Core Resource]{logContent}");
                    }
                    break;
                default:
                    System.Diagnostics.Trace.WriteLine($"{logFile}:{line}:[Core {level}]{logContent}");
                    break;
            }
        }
        private static void NativeAssertEvent(IntPtr str, IntPtr file, int line)
        {
            var info = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(str);
            var src = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(file);
            System.Diagnostics.Debug.Assert(false,info + ":" + src + ":" + line);
        }
        public static void InitLogger()
        {
            CoreSDK.SetWriteLogStringCallback(NativeLogger);
        }
        public static void FinalLogger()
        {
            CoreSDK.SetWriteLogStringCallback(null);
        }

        public static void WriteLine(ELogTag tag, string category, string format, params object[] args)
        {
            var str = System.String.Format(format, args);
            WriteLine(tag, category, str);
            OnReportLog?.Invoke(tag, category, format, args);
        }
        [Rtti.Meta]
        public static void WriteLine(ELogTag tag, string category, string info, 
                [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            info = $"{sourceFilePath}({sourceLineNumber},0):{info}";
            System.Diagnostics.Trace.WriteLine(info);
            OnReportLog?.Invoke(tag, category, info, null);
        }
        public static void WriteException(Exception ex, string category = "异常",
                [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            var info = ex.ToString();
            info = $"{sourceFilePath}({sourceLineNumber},0):{info}";
            System.Diagnostics.Trace.Write($"{sourceFilePath}:{sourceLineNumber}:");
            System.Diagnostics.Trace.WriteLine(info);
            OnReportLog?.Invoke(ELogTag.Error, category, info, null);
        }
    }
}
