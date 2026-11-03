using System;
using System.Runtime.InteropServices;

namespace GameBuilder
{
    /// <summary>
    /// 硬退出进程：绕过 CLR 关闭序列。
    /// 引擎 native 后台线程（任务池/F2M/内存流/日志）在 CLR 关闭期间仍会反向进入托管代码，
    /// 触发 "Attempt to execute managed code after the .NET runtime thread state has been destroyed"
    /// 断言并 FailFast(0xC0000602)，让 MSBuild 误判构建失败（MSB3073）。
    /// TerminateProcess 直接结束线程，不执行 DllMain/CLR 关闭，退出码可控。
    /// </summary>
    internal static class NativeExit
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        public static void Now(int exitCode)
        {
            // 控制台/日志先落地，TerminateProcess 之后不会有任何 flush 机会
            try { Console.Out.Flush(); } catch { }
            TerminateProcess(GetCurrentProcess(), (uint)exitCode);
        }
    }
}
