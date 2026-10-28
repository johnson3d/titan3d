using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EngineNS.Editor.AndroidBuild
{
    // 真实进程宿主：把 stdout/stderr 追加进共享缓冲，面板每帧取快照绘制。
    internal sealed class TtAndroidProcessHost : ITtAndroidProcessHost
    {
        private readonly Process mProcess;
        private readonly object mLogLock = new object();
        private readonly List<string> mLines = new List<string>();
        private bool mKilled;

        public TtAndroidProcessHost(Process process)
        {
            mProcess = process;
        }

        internal void HookOutput()
        {
            mProcess.OutputDataReceived += OnLine;
            mProcess.ErrorDataReceived += OnLine;
            mProcess.BeginOutputReadLine();
            mProcess.BeginErrorReadLine();
        }

        private void OnLine(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;
            lock (mLogLock)
            {
                mLines.Add(e.Data);
                // 防御长构建把内存吃爆：只保留最近 8000 行。
                if (mLines.Count > 8000)
                    mLines.RemoveRange(0, mLines.Count - 8000);
            }
        }

        public bool HasExited
        {
            get { return mKilled || mProcess.HasExited; }
        }

        public int GetExitCode()
        {
            if (mKilled)
                return -1;
            try
            {
                return mProcess.ExitCode;
            }
            catch
            {
                return -1;
            }
        }

        public string GetOutput()
        {
            lock (mLogLock)
            {
                var builder = new StringBuilder();
                foreach (var line in mLines)
                    builder.AppendLine(line);
                return builder.ToString();
            }
        }

        public IReadOnlyList<string> GetLines()
        {
            lock (mLogLock)
            {
                return mLines.ToArray();
            }
        }

        public void Kill()
        {
            mKilled = true;
            try
            {
                if (!mProcess.HasExited)
                    mProcess.Kill();
            }
            catch
            {
                // 进程可能恰好自行退出，忽略。
            }
        }

        public void Dispose()
        {
            mProcess.Dispose();
        }
    }

    // 用 Process 启动命令行（"exe" args...），工作目录为仓库根，保证脚本的相对路径可用。
    public sealed class TtAndroidProcessLauncher : ITtAndroidProcessLauncher
    {
        public ITtAndroidProcessHost Launch(string commandLine, string workingDirectory)
        {
            var exe = commandLine;
            var args = string.Empty;
            if (commandLine.StartsWith("\""))
            {
                var end = commandLine.IndexOf('"', 1);
                exe = commandLine.Substring(1, end - 1);
                args = commandLine.Substring(end + 1);
            }
            var info = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args.Trim(),
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            var process = Process.Start(info);
            var host = new TtAndroidProcessHost(process);
            host.HookOutput();
            return host;
        }
    }
}
