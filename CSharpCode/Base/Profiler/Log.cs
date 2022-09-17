using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Runtime.InteropServices;


namespace EngineNS.Profiler
{
    [Flags]
    public enum ELogTag
    {
        [Description("消息")]
        Info = 1,
        [Description("警告")]
        Warning = 1 << 1,
        [Description("错误")]
        Error = 1 << 2,
        [Description("严重")]
        Fatal = 1 << 3,
        All = Info | Warning | Error | Fatal,
    }
    public unsafe partial class Log
    {
        static Log()
        {
            OnReportLog += OnReportLog_WriteConsole;
        }
        public delegate void Delegate_OnReportLog(ELogTag tag, string category, string memberName, string sourceFilePath, int sourceLineNumber, string info);
        public static event Delegate_OnReportLog OnReportLog;
        public static void OnReportLog_WriteConsole(ELogTag tag, string category, string memberName, string sourceFilePath, int sourceLineNumber, string info)
        {
            info = $"{sourceFilePath}({sourceLineNumber},0):{info}";
            System.Diagnostics.Trace.WriteLine(info);
        }
        private static CoreSDK.FDelegate_FWriteLogString NativeLogger = NativeWriteLogString;
#if PMacIOS
        [ObjCRuntime.MonoPInvokeCallback(typeof(FDelegate_WriteLogString))]
#endif
        private static unsafe void NativeWriteLogString(void* threadName, void* logStr, ELevelTraceType level, void* file, int line)
        {
            var thread = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)threadName);
            var logContent = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)logStr);
            var logFile = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)file);            
            ELogTag tag = ELogTag.Info;
            switch (level)
            {
                case ELevelTraceType.ELTT_Error:
                    tag = ELogTag.Error;
                    break;
                case ELevelTraceType.ELTT_Warning:
                    tag = ELogTag.Warning;
                    break;
                case ELevelTraceType.ELTT_Graphics:
                    if (logContent.StartsWith("Vk[Error]"))
                    {
                        tag = ELogTag.Error;
                        break;
                    }
                    break;
            }

            OnReportLog?.Invoke(tag, level.ToString(), null, logFile, line, logContent);
        }
        //private static void NativeAssertEvent(IntPtr str, IntPtr file, int line)
        //{
        //    var info = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(str);
        //    var src = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(file);
        //    System.Diagnostics.Debug.Assert(false,info + ":" + src + ":" + line);
        //}
        public static void InitLogger()
        {
            CoreSDK.SetWriteLogStringCallback(NativeLogger);
        }
        public static void FinalLogger()
        {
            CoreSDK.SetWriteLogStringCallback(null);
        }
        [Rtti.Meta]
        public static void WriteLine(ELogTag tag, string category, string info, 
                [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            OnReportLog?.Invoke(tag, category, memberName, sourceFilePath, sourceLineNumber, info);
        }
        public static void WriteException(Exception ex, string category = "异常",
                [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            var info = ex.ToString();
            OnReportLog?.Invoke(ELogTag.Error, category, memberName, sourceFilePath, sourceLineNumber, info);
        }
    }
}
