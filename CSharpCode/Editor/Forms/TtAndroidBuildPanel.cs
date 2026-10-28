using System;
using System.Collections.Generic;
using System.IO;

namespace EngineNS.Editor.Forms
{
    // Android 构建面板：只做「读写配置 + 拼命令行 + 驱动外部进程」，门禁全部留给 BuildScript。
    public class TtAndroidBuildPanel : IRootForm
    {
        public TtAndroidBuildPanel()
        {
            TtEngine.RootFormManager.RegRootForm(this);
        }

        private bool mVisible = false;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public void Dispose() { }

        private string mRepositoryRoot = string.Empty;
        private AndroidBuild.TtAndroidToolchainDocument mToolchain;
        private AndroidBuild.TtAndroidContentSpec mContentSpec;
        private AndroidBuild.TtAndroidBuildSession mSession;

        // 面板上 6 个路径字段 + abi/apiLevel，写入后点「保存配置」落盘。
        private readonly string[] mToolchainKeys = { "ndk", "androidSdk", "cmake", "ninja", "dotnet", "javaHome" };
        private readonly Dictionary<string, string> mToolchainValues = new Dictionary<string, string>();
        private int mApiLevel;
        private string mNewPattern = string.Empty;
        private bool mCookFirst = false;
        private string mCookConfig = "content/config/engineconfig_windows_cook.jscfg";
        private string mCookBlacklist = string.Empty;
        private string mConfiguration = "Debug";
        private string mStatus = string.Empty;

        // 目录选择对话框：一次只挂起一个 key（共享同一个 FileDialog 实例）。
        private string mPendingPickKey = null;
        private bool mInitialized = false;

        // adb 部署：与构建会话共用 Launcher，但独立持有 host，不干扰构建状态机。
        private AndroidBuild.TtAndroidProcessLauncher mDeployLauncher = new AndroidBuild.TtAndroidProcessLauncher();
        private AndroidBuild.TtAndroidProcessHost mDeployHost;
        private string mDeployStatus = string.Empty;

        // 失败转发引擎日志：记上一帧阶段与最近运行的进程 host，用于在 Failed 转变时取输出。
        private AndroidBuild.TtAndroidBuildPhase mLastPhase = AndroidBuild.TtAndroidBuildPhase.Idle;
        private AndroidBuild.ITtAndroidProcessHost mLastRunHost;
        private AndroidBuild.TtAndroidBuildPhase mLastDeployPhase = AndroidBuild.TtAndroidBuildPhase.Idle;

        // 步骤进度：根据 Session 的 Phase 与 Build 阶段内的 [TITAN-STAGE] 标记，
        // 画出每个步骤的 ✓（完成）/ ▶（进行中）/ ·（待执行）/ ✗（失败）与文本进度条。
        private void DrawBuildSteps()
        {
            var phase = mSession.Phase;
            var failed = phase == AndroidBuild.TtAndroidBuildPhase.Failed;
            var names = new List<string> { TtLocalization.Tr("Check Toolchain") };
            var states = new List<int>(); // 0 待执行 1 进行中 2 完成 3 失败
            if (mCookFirst)
                names.Add(TtLocalization.Tr("Cook Assets"));
            names.AddRange(new[] { TtLocalization.Tr("Native Build"), TtLocalization.Tr("Managed Build"), TtLocalization.Tr("Stage Content"), TtLocalization.Tr("Package APK") });

            // Check 步
            if (phase == AndroidBuild.TtAndroidBuildPhase.Check)
                states.Add(failed ? 3 : 1);
            else
                states.Add(2);
            // Cook 步（若有）
            if (mCookFirst)
            {
                if (phase == AndroidBuild.TtAndroidBuildPhase.Cook)
                    states.Add(failed ? 3 : 1);
                else if (phase == AndroidBuild.TtAndroidBuildPhase.Build || phase == AndroidBuild.TtAndroidBuildPhase.Done)
                    states.Add(2);
                else
                    states.Add(0);
            }
            // Build 内四个子步，由 CurrentBuildStage 决定当前位置（-1 = 还没出现标记）。
            var stageOrder = new[] { "Native", "Managed", "Content", "Apk" };
            var current = failed && phase == AndroidBuild.TtAndroidBuildPhase.Failed && string.IsNullOrEmpty(mSession.CurrentBuildStage)
                ? -2 : Array.IndexOf(stageOrder, mSession.CurrentBuildStage);
            for (var i = 0; i < stageOrder.Length; i++)
            {
                if (phase == AndroidBuild.TtAndroidBuildPhase.Done)
                    states.Add(2);
                else if (current == -2)
                    states.Add(failed ? 1 : 0); // Build 未启动就在别处失败
                else if (i < current)
                    states.Add(2);
                else if (i == current)
                    states.Add(failed ? 3 : 1);
                else
                    states.Add(0);
            }

            var marks = new[] { "· ", "▶ ", "✓ ", "✗ " };
            var text = new System.Text.StringBuilder();
            var doneCount = 0;
            for (var i = 0; i < names.Count; i++)
            {
                text.Append(marks[states[i]]).Append(names[i]);
                if (i < names.Count - 1)
                    text.Append("   ");
                if (states[i] == 2)
                    doneCount++;
            }
            ImGuiAPI.Text(text.ToString());
            var fraction = (doneCount + (states.Contains(1) ? 0.5f : 0f)) / names.Count;
            var filled = (int)(fraction * 24);
            ImGuiAPI.Text("[" + new string('■', filled) + new string('·', 24 - filled) + "] "
                + (int)(fraction * 100) + "%" + (failed ? TtLocalization.Tr(" (failed, see log)") : string.Empty));
        }

        // 把构建失败写入引擎 Profiler.Log（LogWatcher 可见），并附上可疑错误行。
        private void ReportBuildFailureToEngineLog(AndroidBuild.ITtAndroidProcessHost host)
        {
            Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Error, "AndroidBuild", mSession.LastError);
            if (host == null)
                return;
            var suspects = new List<string>();
            var tail = new List<string>();
            var lines = host.GetOutput().Replace("\r\n", "\n").Split('\n');
            foreach (var line in lines)
            {
                if (AndroidBuild.TtAndroidBuildLogParser.LooksLikeFailure(line))
                    suspects.Add(line.Trim());
            }
            for (var i = Math.Max(0, lines.Length - 5); i < lines.Length; i++)
                tail.Add(lines[i].Trim());
            if (suspects.Count == 0)
                suspects = tail;
            const int max = 20;
            for (var i = Math.Max(0, suspects.Count - max); i < suspects.Count; i++)
                Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Error, "AndroidBuild", "  " + suspects[i]);
        }

        public async Thread.Async.TtTask<bool> Initialize()
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            if (mInitialized)
                return true;
            mInitialized = true;
            mRepositoryRoot = AndroidBuild.TtAndroidRepository.FindRoot(System.AppContext.BaseDirectory);
            if (string.IsNullOrEmpty(mRepositoryRoot))
                mRepositoryRoot = AndroidBuild.TtAndroidRepository.FindRoot(Directory.GetCurrentDirectory());
            if (string.IsNullOrEmpty(mRepositoryRoot))
            {
                mStatus = TtLocalization.Tr("Repository root not found: make sure the editor runs inside the titanengine tree");
                return true;
            }
            LoadAll();
            return true;
        }

        private void LoadAll()
        {
            var toolchainPath = AndroidBuild.TtAndroidRepository.ToFullPath(mRepositoryRoot, AndroidBuild.TtAndroidRepository.ToolchainConfigRelativePath);
            mToolchain = AndroidBuild.TtAndroidToolchainDocument.Load(toolchainPath);
            mToolchainValues.Clear();
            foreach (var key in mToolchainKeys)
                mToolchainValues[key] = mToolchain.Get(key);
            mApiLevel = mToolchain.GetApiLevel();

            var specPath = AndroidBuild.TtAndroidRepository.ToFullPath(mRepositoryRoot, AndroidBuild.TtAndroidRepository.ContentSpecRelativePath);
            mContentSpec = AndroidBuild.TtAndroidContentSpec.Load(specPath);
        }

        private void SaveAll()
        {
            var toolchainPath = AndroidBuild.TtAndroidRepository.ToFullPath(mRepositoryRoot, AndroidBuild.TtAndroidRepository.ToolchainConfigRelativePath);
            foreach (var pair in mToolchainValues)
                mToolchain.Set(pair.Key, pair.Value);
            mToolchain.SetApiLevel(mApiLevel);
            mToolchain.Save(toolchainPath);

            var specPath = AndroidBuild.TtAndroidRepository.ToFullPath(mRepositoryRoot, AndroidBuild.TtAndroidRepository.ContentSpecRelativePath);
            mContentSpec.Save(specPath);
            mStatus = TtLocalization.Format("Saved: {0} / {1}", AndroidBuild.TtAndroidRepository.ToolchainConfigRelativePath,
                AndroidBuild.TtAndroidRepository.ContentSpecRelativePath);
        }

        private AndroidBuild.TtAndroidBuildRequest BuildRequestFromUi()
        {
            return new AndroidBuild.TtAndroidBuildRequest
            {
                RepositoryRoot = mRepositoryRoot,
                Configuration = mConfiguration,
                CookFirst = mCookFirst,
                CookConfig = mCookConfig,
                CookBlacklist = mCookBlacklist,
            };
        }

        private unsafe void OnDrawPickDialog()
        {
            if (string.IsNullOrEmpty(mPendingPickKey))
                return;
            var fileDlg = TtEngine.Instance.EditorInstance.FileDialog.mFileDialog;
            if (fileDlg.DisplayDialog("AndroidBuildPickDir"))
            {
                if (fileDlg.IsOk())
                    mToolchainValues[mPendingPickKey] = fileDlg.GetFilePathName();
                fileDlg.CloseDialog();
                mPendingPickKey = null;
            }
        }

        public unsafe void OnDraw()
        {
            if (!Visible)
                return;
            OnDrawPickDialog();
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            var result = EGui.UIProxy.DockProxy.BeginMainForm(TtLocalization.Label("AndroidBuild", "###AndroidBuild"), this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                DockId = ImGuiAPI.GetWindowDockID();
                ImGuiAPI.Text(TtLocalization.Format("Repository root: {0}", mRepositoryRoot));

                if (ImGuiAPI.CollapsingHeader(TtLocalization.Tr("Toolchain"), ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                {
                    foreach (var key in mToolchainKeys)
                    {
                        var value = mToolchainValues[key];
                        ImGuiAPI.Text(key);
                        ImGuiAPI.SameLine(0, 6);
                        if (ImGuiAPI.InputText("##" + key, ref value))
                            mToolchainValues[key] = value;
                        ImGuiAPI.SameLine(0, 6);
                        ImGuiAPI.PushID(key);
                        if (EGui.UIProxy.CustomButton.ToolButton(TtLocalization.Tr("Pick"), in Vector2.Zero))
                        {
                            var fileDlg = TtEngine.Instance.EditorInstance.FileDialog.mFileDialog;
                            mPendingPickKey = key;
                            fileDlg.OpenModal("AndroidBuildPickDir", TtLocalization.Format("Pick {0} directory", key), "", mRepositoryRoot);
                        }
                        ImGuiAPI.PopID();
                    }
                    // 重载见 UvAnimEditor.cs:126：(string, ref int, int, int, ImGuiInputTextFlags_)。
                    var apiLevel = mApiLevel;
                    if (ImGuiAPI.InputInt("apiLevel", ref apiLevel, 1, 0, ImGuiInputTextFlags_.ImGuiInputTextFlags_None))
                        mApiLevel = apiLevel;

                    if (ImGuiAPI.TreeNode(TtLocalization.Tr("Installed SDK")))
                    {
                        var sdkRoot = AndroidBuild.TtAndroidRepository.ToFullPath(mRepositoryRoot, mToolchainValues["androidSdk"]);
                        var platforms = AndroidBuild.TtAndroidSdkInventory.GetPlatforms(Directory.Exists(sdkRoot) ? sdkRoot : string.Empty);
                        var buildTools = AndroidBuild.TtAndroidSdkInventory.GetBuildTools(Directory.Exists(sdkRoot) ? sdkRoot : string.Empty);
                        ImGuiAPI.Text(TtLocalization.Format("platforms: {0}", platforms.Count > 0 ? string.Join(", ", platforms) : TtLocalization.Tr("(not installed)")));
                        ImGuiAPI.Text(TtLocalization.Format("build-tools: {0}", buildTools.Count > 0 ? string.Join(", ", buildTools) : TtLocalization.Tr("(not installed)")));
                        var adb = AndroidBuild.TtAndroidSdkInventory.GetAdbPath(sdkRoot);
                        ImGuiAPI.Text(TtLocalization.Format("adb: {0}", adb != string.Empty ? adb : TtLocalization.Tr("(platform-tools/adb.exe not found)")));
                        ImGuiAPI.TreePop();
                    }
                }

                if (ImGuiAPI.CollapsingHeader(TtLocalization.Tr("Packaged Content (TitanContent.txt)"), ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                {
                    // 倒序遍历，删除后 continue，避免修改集合时跳行。
                    var patterns = mContentSpec.Patterns;
                    for (int i = patterns.Count - 1; i >= 0; i--)
                    {
                        var pattern = patterns[i];
                        ImGuiAPI.Text(pattern);
                        ImGuiAPI.SameLine(0, 6);
                        ImGuiAPI.PushID(pattern);
                        if (EGui.UIProxy.CustomButton.ToolButton(TtLocalization.Tr("Delete"), in Vector2.Zero))
                        {
                            mContentSpec.Remove(pattern);
                            ImGuiAPI.PopID();
                            continue;
                        }
                        ImGuiAPI.PopID();
                    }
                    var editor = mNewPattern;
                    if (ImGuiAPI.InputText("##newPattern", ref editor))
                        mNewPattern = editor;
                    ImGuiAPI.SameLine(0, 6);
                    if (EGui.UIProxy.CustomButton.ToolButton(TtLocalization.Tr("Add"), in Vector2.Zero))
                    {
                        var normalized = mContentSpec.Normalize(mNewPattern);
                        var error = AndroidBuild.TtAndroidContentSpec.Validate(normalized);
                        if (error != null)
                            mStatus = error;
                        else if (!mContentSpec.Add(normalized))
                            mStatus = TtLocalization.Format("Already exists or invalid: {0}", normalized);
                        else
                        {
                            mStatus = TtLocalization.Format("Added: {0} (save and rebuild the APK to take effect)", normalized);
                            mNewPattern = string.Empty;
                        }
                    }
                }

                if (ImGuiAPI.CollapsingHeader(TtLocalization.Tr("Build Options"), ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                {
                    if (ImGuiAPI.RadioButton("Debug##cfg", mConfiguration == "Debug"))
                        mConfiguration = "Debug";
                    ImGuiAPI.SameLine(0, 6);
                    if (ImGuiAPI.RadioButton("Release##cfg", mConfiguration == "Release"))
                        mConfiguration = "Release";

                    ImGuiAPI.Checkbox(TtLocalization.Tr("Cook assets before build"), ref mCookFirst);
                    if (mCookFirst)
                    {
                        var cfg = mCookConfig;
                        if (ImGuiAPI.InputText("CookCfg", ref cfg))
                            mCookConfig = cfg;
                        var blacklist = mCookBlacklist;
                        if (ImGuiAPI.InputText(TtLocalization.Tr("Blacklist (optional)"), ref blacklist))
                            mCookBlacklist = blacklist;
                    }
                }

                var running = mSession != null &&
                              (mSession.Phase == AndroidBuild.TtAndroidBuildPhase.Check ||
                               mSession.Phase == AndroidBuild.TtAndroidBuildPhase.Cook ||
                               mSession.Phase == AndroidBuild.TtAndroidBuildPhase.Build);

                if (EGui.UIProxy.CustomButton.ToolButton(TtLocalization.Tr("Save Config"), in Vector2.Zero))
                    SaveAll();
                ImGuiAPI.SameLine(0, 6);
                if (running)
                {
                    if (EGui.UIProxy.CustomButton.ToolButton(TtLocalization.Tr("Cancel Build"), in Vector2.Zero))
                    {
                        mSession.Cancel();
                        mStatus = mSession.LastError;
                    }
                }
                else
                {
                    if (EGui.UIProxy.CustomButton.ToolButton(TtLocalization.Tr("Start Build"), in Vector2.Zero))
                    {
                        SaveAll();
                        var request = BuildRequestFromUi();
                        var errors = request.Validate();
                        if (errors.Count > 0)
                        {
                            mStatus = string.Join("\n", errors.ToArray());
                        }
                        else
                        {
                            mSession = new AndroidBuild.TtAndroidBuildSession(new AndroidBuild.TtAndroidProcessLauncher());
                            mSession.Start(request);
                            mStatus = TtLocalization.Tr("Build started (phase: toolchain check)");
                        }
                    }
                }
                ImGuiAPI.SameLine(0, 6);
                if (EGui.UIProxy.CustomButton.ToolButton(TtLocalization.Tr("Check Toolchain"), in Vector2.Zero))
                {
                    SaveAll();
                    var request = BuildRequestFromUi();
                    mSession = new AndroidBuild.TtAndroidBuildSession(new AndroidBuild.TtAndroidProcessLauncher());
                    mSession.Start(request);
                    mStatus = TtLocalization.Tr("Toolchain check started");
                }

                if (mSession != null)
                {
                    if (mSession.IsRunning)
                        mLastRunHost = mSession.CurrentHost;
                    var phaseBefore = mSession.Phase;
                    mSession.Poll();
                    if (mSession.Phase == AndroidBuild.TtAndroidBuildPhase.Done)
                        mStatus = TtLocalization.Tr("Build finished");
                    else if (mSession.Phase == AndroidBuild.TtAndroidBuildPhase.Failed && phaseBefore != AndroidBuild.TtAndroidBuildPhase.Failed)
                    {
                        mStatus = mSession.LastError;
                        ReportBuildFailureToEngineLog(mLastRunHost);
                    }
                    mLastPhase = mSession.Phase;
                }
                ImGuiAPI.Text(TtLocalization.Format("Status: {0}", mStatus));
                if (mSession != null && mSession.Phase != AndroidBuild.TtAndroidBuildPhase.Idle)
                    DrawBuildSteps();

                if (mSession != null && !string.IsNullOrEmpty(mSession.ApkPath))
                {
                    ImGuiAPI.Text(TtLocalization.Format("APK: {0}", mSession.ApkPath));
                    ImGuiAPI.SameLine(0, 6);
                    if (EGui.UIProxy.CustomButton.ToolButton(TtLocalization.Tr("Open APK folder"), in Vector2.Zero))
                    {
                        var dir = Path.GetDirectoryName(mSession.ApkPath);
                        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", "\"" + dir + "\"") { UseShellExecute = true });
                    }
                }

                // adb 部署区：列设备 / 安装当前 APK。
                if (mToolchainValues.TryGetValue("androidSdk", out var sdkValue))
                {
                    var adb = AndroidBuild.TtAndroidSdkInventory.GetAdbPath(
                        AndroidBuild.TtAndroidRepository.ToFullPath(mRepositoryRoot, sdkValue));
                    if (adb == string.Empty)
                    {
                        ImGuiAPI.Text(TtLocalization.Tr("Deploy: adb not found (androidSdk/platform-tools/adb.exe)"));
                    }
                    else
                    {
                        var deploying = mDeployHost != null && !mDeployHost.HasExited;
                        if (deploying)
                        {
                            if (EGui.UIProxy.CustomButton.ToolButton(TtLocalization.Tr("Cancel Deploy"), in Vector2.Zero))
                            {
                                mDeployHost.Kill();
                                mDeployStatus = TtLocalization.Tr("Canceled.");
                            }
                        }
                        else
                        {
                            if (EGui.UIProxy.CustomButton.ToolButton(TtLocalization.Tr("List Devices"), in Vector2.Zero))
                            {
                                mDeployHost = (AndroidBuild.TtAndroidProcessHost)mDeployLauncher.Launch(
                                    AndroidBuild.TtAndroidDeployCommand.AdbDevices(adb), mRepositoryRoot);
                                mDeployStatus = TtLocalization.Tr("Listing devices\u2026");
                            }
                            ImGuiAPI.SameLine(0, 6);
                            var apkPath = mSession != null ? mSession.ApkPath : string.Empty;
                            var hasApk = File.Exists(apkPath);
                            if (EGui.UIProxy.CustomButton.ToolButton(TtLocalization.Tr("Deploy to Device"), in Vector2.Zero) && hasApk)
                            {
                                mDeployHost = (AndroidBuild.TtAndroidProcessHost)mDeployLauncher.Launch(
                                    AndroidBuild.TtAndroidDeployCommand.AdbInstall(adb, apkPath), mRepositoryRoot);
                                mDeployStatus = TtLocalization.Format("Installing {0} \u2026", apkPath);
                            }
                            if (!hasApk)
                                ImGuiAPI.SameLine(0, 6);
                        }
                        ImGuiAPI.SameLine(0, 6);
                        if (mDeployHost != null && mDeployHost.HasExited && mDeployStatus.EndsWith("…"))
                        {
                            mDeployStatus = mDeployHost.GetExitCode() == 0
                                ? TtLocalization.Tr("Deployed.")
                                : TtLocalization.Format("Deploy failed, exit code {0}.", mDeployHost.GetExitCode());
                            if (mDeployHost.GetExitCode() != 0 && mLastDeployPhase != AndroidBuild.TtAndroidBuildPhase.Failed)
                            {
                                Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Error, "AndroidBuild", "adb deploy: " + mDeployStatus);
                                foreach (var line in mDeployHost.GetOutput().Replace("\r\n", "\n").Split('\n'))
                                {
                                    var text = line.Trim();
                                    if (text.Length > 0)
                                        Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Error, "AndroidBuild", "  " + text);
                                }
                            }
                            mLastDeployPhase = mDeployHost.GetExitCode() == 0
                                ? AndroidBuild.TtAndroidBuildPhase.Done : AndroidBuild.TtAndroidBuildPhase.Failed;
                        }
                        ImGuiAPI.Text(TtLocalization.Format("Deploy: {0}", mDeployStatus));
                    }
                }

                var host = mSession != null ? mSession.CurrentHost : null;
                if (host == null && mDeployHost != null)
                    host = mDeployHost;
                if (host != null)
                {
                    if (ImGuiAPI.BeginChild("AndroidBuildLog", new Vector2(0, 260), ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_HorizontalScrollbar))
                    {
                        // 长日志只画尾部 2000 行，避免每帧全量重绘拖慢编辑器。
                        var lines = (host as AndroidBuild.TtAndroidProcessHost)?.GetLines();
                        if (lines != null)
                        {
                            var from = lines.Count > 2000 ? lines.Count - 2000 : 0;
                            for (var i = from; i < lines.Count; i++)
                                ImGuiAPI.Text(lines[i]);
                            if (lines.Count > 0)
                                ImGuiAPI.SetScrollHereY(1.0f);
                        }
                    }
                    ImGuiAPI.EndChild();
                }
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
    }
}
