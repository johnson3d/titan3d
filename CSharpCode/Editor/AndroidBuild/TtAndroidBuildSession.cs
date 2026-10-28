namespace EngineNS.Editor.AndroidBuild
{
    // 进程抽象：便于单测注入 fake，也隔离 System.Diagnostics 依赖。
    public interface ITtAndroidProcessHost
    {
        bool HasExited { get; }
        int GetExitCode();
        string GetOutput();
        void Kill();
    }

    public interface ITtAndroidProcessLauncher
    {
        ITtAndroidProcessHost Launch(string commandLine, string workingDirectory);
    }

    public enum TtAndroidBuildPhase
    {
        Idle,
        Check,
        Cook,
        Build,
        Done,
        Failed,
    }

    // 构建（检查 → 可选 cook → APK）阶段机。Poll 由面板每帧驱动，本身不建线程。
    public sealed class TtAndroidBuildSession
    {
        private readonly ITtAndroidProcessLauncher mLauncher;
        private ITtAndroidProcessHost mCurrent;
        private TtAndroidBuildRequest mRequest;

        public TtAndroidBuildPhase Phase { get; private set; } = TtAndroidBuildPhase.Idle;
        public string LastError { get; private set; } = string.Empty;
        public string ApkPath { get; private set; } = string.Empty;
        // Build 阶段内当前子步骤（[TITAN-STAGE] 标记）：Native/Managed/Content/Apk，空表示还没进入标记。Check/Cook 阶段为空。
        public string CurrentBuildStage { get; private set; } = string.Empty;
        public bool IsRunning => mCurrent != null && !mCurrent.HasExited;

        // 面板用：取当前进程日志（无构建时为 null）。
        public ITtAndroidProcessHost CurrentHost => mCurrent;

        public TtAndroidBuildSession(ITtAndroidProcessLauncher launcher)
        {
            mLauncher = launcher;
        }

        public void Start(TtAndroidBuildRequest request)
        {
            if (mCurrent != null && !mCurrent.HasExited)
                return;
            var errors = request.Validate();
            if (errors.Count > 0)
            {
                Phase = TtAndroidBuildPhase.Failed;
                LastError = string.Join("\n", errors.ToArray());
                return;
            }
            Phase = TtAndroidBuildPhase.Check;
            LastError = string.Empty;
            ApkPath = string.Empty;
            CurrentBuildStage = string.Empty;
            mRequest = request;
            mCurrent = mLauncher.Launch(TtAndroidBuildCommandLine.Check(request, "Managed"), request.RepositoryRoot);
        }

        public void Poll()
        {
            if (mCurrent == null)
                return;
            // 运行中：扫描输出里的 [TITAN-STAGE] 标记，面板据此刻步骤进度条。
            var stage = TtAndroidBuildLogParser.FindLastStageMarker(mCurrent.GetOutput());
            if (stage != null)
                CurrentBuildStage = stage;
            if (!mCurrent.HasExited)
                return;
            var request = mRequest;
            var exitCode = mCurrent.GetExitCode();
            if (exitCode != 0)
            {
                Phase = TtAndroidBuildPhase.Failed;
                LastError = "阶段 " + Phase.ToString() + " 失败，退出码 " + exitCode + "。";
                mCurrent = null;
                return;
            }
            switch (Phase)
            {
                case TtAndroidBuildPhase.Check:
                    mCurrent = null;
                    if (request.CookFirst)
                    {
                        Phase = TtAndroidBuildPhase.Cook;
                        mCurrent = mLauncher.Launch(TtAndroidBuildCommandLine.Cook(request), request.RepositoryRoot);
                    }
                    else
                    {
                        Phase = TtAndroidBuildPhase.Build;
                        mCurrent = mLauncher.Launch(TtAndroidBuildCommandLine.Apk(request), request.RepositoryRoot);
                    }
                    break;
                case TtAndroidBuildPhase.Cook:
                    Phase = TtAndroidBuildPhase.Build;
                    mCurrent = mLauncher.Launch(TtAndroidBuildCommandLine.Apk(request), request.RepositoryRoot);
                    break;
                case TtAndroidBuildPhase.Build:
                    ApkPath = TtAndroidBuildLogParser.ExtractApkPath(mCurrent.GetOutput()) ?? string.Empty;
                    if (string.IsNullOrEmpty(ApkPath))
                    {
                        Phase = TtAndroidBuildPhase.Failed;
                        LastError = "APK 构建退出码为 0，但输出中没有 APK= 路径，请查看日志。";
                    }
                    else
                    {
                        Phase = TtAndroidBuildPhase.Done;
                    }
                    mCurrent = null;
                    break;
                default:
                    mCurrent = null;
                    break;
            }
        }

        public void Cancel()
        {
            if (mCurrent != null && !mCurrent.HasExited)
                mCurrent.Kill();
            mCurrent = null;
            Phase = TtAndroidBuildPhase.Failed;
            LastError = "已由用户取消。";
        }
    }
}
