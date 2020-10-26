using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Editor.Runner
{
    public class ProcessExecuter
    {
        #region WindowAPI
        [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
        public class SECURITY_ATTRIBUTES
        {
            public int nLength;
            public string lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public int lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public int wShowWindow;
            public int cbReserved2;
            public byte lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern bool CreateProcess(
            string lpApplicationName, string lpCommandLine,
            SECURITY_ATTRIBUTES lpProcessAttributes,
            SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            int dwCreationFlags,
            StringBuilder lpEnvironment,
            StringBuilder lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            ref PROCESS_INFORMATION lpProcessInformation
            );
        [DllImport("Kernel32.dll")]
        public static extern uint WaitForSingleObject(System.IntPtr hHandle, uint dwMilliseconds);
        [DllImport("Kernel32.dll")]
        public static extern bool CloseHandle(System.IntPtr hObject);
        [DllImport("Kernel32.dll")]
        static extern bool GetExitCodeProcess(System.IntPtr hProcess, ref uint lpExitCode);
        #endregion
        public static bool RunProcessSync(string cmd, List<string> args, List<string> outInfos)
        {
            string finalArgs = cmd + " ";
            foreach (var i in args)
            {
                finalArgs += i + " ";
            }

            STARTUPINFO sInfo = new STARTUPINFO();
            PROCESS_INFORMATION pInfo = new PROCESS_INFORMATION();

            unsafe
            {
                if (!CreateProcess(null, finalArgs, null, null, false, 0, null, null, ref sInfo, ref pInfo))
                {
                    //throw new Exception("调用失败");
                    return false;
                }
            }

            uint iCode = 0;
            WaitForSingleObject(pInfo.hProcess, int.MaxValue);
            GetExitCodeProcess(pInfo.hProcess, ref iCode);
            CloseHandle(pInfo.hProcess);
            CloseHandle(pInfo.hThread);
            return true;
        }
        public static async System.Threading.Tasks.Task<bool> RunProcess(string cmd, List<string> args, List<string> outInfos)
        {
            if (CEngine.Instance.FileManager.FileExists(cmd) == false)
                return false;
            await CEngine.Instance.EventPoster.Post(() =>
            {
                var p = new System.Diagnostics.Process();
                p.EnableRaisingEvents = true;
                p.StartInfo.FileName = cmd;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                string finalArgs = "";
                foreach (var i in args)
                {
                    finalArgs += i + " ";
                }
                p.StandardInput.WriteLine(finalArgs + " &exit");
                p.StandardInput.AutoFlush = true;
                outInfos.Add(p.StandardOutput.ReadToEnd());
                outInfos.Add(p.StandardError.ReadToEnd());
                p.WaitForExit();
                p.Close();
                return true;
            }, Thread.Async.EAsyncTarget.AsyncEditorSlow);

            return true;
        }
        public static async System.Threading.Tasks.Task<bool> RunEditorCMD(List<string> args, List<string> outInfos)
        {
            string cmd = CEngine.Instance.FileManager.Bin + "EditorCMD.exe";
            return await RunProcess(cmd, args, outInfos);
        }
        public static async System.Threading.Tasks.Task<bool> RunCook(RName entry, EPlatformType platform, int shaderModel,
            bool bCookShader, bool bRecompileShader, bool bGenVSProj, bool bCopyInfo, List<string> outInfos)
        {
            List<string> args = new List<string>();
            args.Add("cook");
            args.Add($"entry={entry.Name}");
            args.Add($"platform={PlatformType2Name(platform)}");
            args.Add($"shadermodel={shaderModel.ToString()}");

            if(bCookShader)
                args.Add("cookshader");
            if (bRecompileShader)
                args.Add("recompile");
            if (bGenVSProj)
                args.Add("genvsproj");
            if (bCopyInfo)
                args.Add("copyrinfo");

            return await RunEditorCMD(args, outInfos);
        }
        public static string PlatformType2Name(EPlatformType platform)
        {
            switch (platform)
            {
                case EPlatformType.PLATFORM_DROID:
                    return "android";
                case EPlatformType.PLATFORM_IOS:
                    return "ios";
                case EPlatformType.PLATFORM_WIN:
                    return "windows";
                default:
                    return "unknown";
            }
        }
    }
}
