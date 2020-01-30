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
    public class Log
    {
        public delegate void Delegate_OnReportLog(ELogTag tag, string category, string format, params object[] args);
        public static event Delegate_OnReportLog OnReportLog;
        
        private static FDelegate_WriteLogString NativeLogger = NativeWriteLogString;
        private static FDelegate_AssertEvent NativeAssert = NativeAssertEvent;
        private enum ELevelTraceType
        {
            ELTT_info = 0,
            ELTT_Warning,
            ELTT_Error,

            ELTT_Default = 3,

            ELTT_Graphics,
            ELTT_Network,
            ELTT_SceneGraph,
            ELTT_Memory,
            ELTT_Media,
            ELTT_Physics,
            ELTT_Resource,
            ELTT_SystemCore,
            ELTT_VR,
            ELTT_Input,
        };
#if PlatformIOS
        [ObjCRuntime.MonoPInvokeCallback(typeof(FDelegate_WriteLogString))]
#endif
        private static void NativeWriteLogString(IntPtr threadName, IntPtr logStr, ELevelTraceType level, IntPtr file, int line)
        {
            var thread = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(threadName);
            var logContent = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(logStr);
            var logFile = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(file);
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
            Debug_SetWriteLogStringCallback(NativeLogger);
            Debug_SetAssertEvent(NativeAssert);
        }
        public static void FinalLogger()
        {
            Debug_SetWriteLogStringCallback(null);
            Debug_SetAssertEvent(null);
        }

        public static void WriteLine(ELogTag tag, string category, string format, params object[] args)
        {
            var str = System.String.Format(format, args);
            WriteLine(tag, category, str);
            OnReportLog?.Invoke(tag, category, format, args);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static void WriteLine(ELogTag tag, string category, string info)
        {
            System.Diagnostics.Trace.WriteLine(info);
            OnReportLog?.Invoke(tag, category, info, null);
        }
        public static void WriteException(Exception ex, string category = "异常")
        {
            var info = ex.ToString();
            System.Diagnostics.Trace.WriteLine(info);
            OnReportLog?.Invoke(ELogTag.Error, category, info, null);
        }

        #region SDK
        public const string ModuleNC = CoreObjectBase.ModuleNC;
        private delegate void FDelegate_WriteLogString(IntPtr threadName, IntPtr logStr, ELevelTraceType level, IntPtr file, int line);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void Debug_SetWriteLogStringCallback(FDelegate_WriteLogString wls);
        private delegate void FDelegate_AssertEvent(IntPtr str, IntPtr file, int line);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void Debug_SetAssertEvent(FDelegate_AssertEvent evt);
        #endregion
    }
}
