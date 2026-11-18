using System;
using System.Collections.Generic;
using System.IO;

namespace EngineNS.Editor.Forms
{
    /// <summary>
    /// UE 风格的 cook 进度浮窗: 顶部常驻小窗, 显示总体进度条、当前资产、阶段、已用时与取消按钮。
    /// 数据全部从 TtCookManager 只读取得 —— 本窗体不驱动状态机 (OnDraw 会被 DrawRootForms 按可见性跳过,
    /// 驱动留在 TickableManager 里), 关掉浮窗批次照常推进。
    /// </summary>
    public class TtCookProgressForm : IRootForm
    {
        public static TtCookProgressForm Instance;

        public bool Visible { get => mVisible; set => mVisible = value; }
        bool mVisible = true;
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; } = default;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        const string WindowName = "Cook Progress###TtCookProgressForm";
        /// <summary>批次跑完后浮窗停留多久自动隐藏 (毫秒)</summary>
        const long AutoHideMs = 6000;

        /// <summary>批次全部了结的时刻 (TickCount64); 0 = 未了结</summary>
        long mSettledTick;
        bool mPosSet;

        public TtCookProgressForm()
        {
            Instance = this;
        }

        public void Dispose() { }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        public void Cleanup() { }

        /// <summary>批次开始时由调用方显式唤起 (面板的 Cook 按钮、菜单项都走这里)</summary>
        public static void ShowProgress()
        {
            if (Instance == null)
                return;
            Instance.mVisible = true;
            Instance.mSettledTick = 0;
        }

        public unsafe void OnDraw()
        {
            var mgr = Cook.TtCookManager.Instance;

            // 第一次出现时停在主视口工作区顶部居中; 之后交给用户拖
            if (mPosSet == false)
            {
                var vp = ImGuiAPI.GetMainViewport();
                var pos = new EngineNS.Vector2(vp->WorkPos.X + vp->WorkSize.X * 0.5f, vp->WorkPos.Y + vp->WorkSize.Y * 0.06f);
                ImGuiAPI.SetNextWindowPos(in pos, ImGuiCond_.ImGuiCond_Always, new EngineNS.Vector2(0.5f, 0f));
                mPosSet = true;
            }
            // 宽度按字号缩放 (约 34 个字宽), 高 DPI/大字号下不挤字
            ImGuiAPI.SetNextWindowSize(new EngineNS.Vector2(ImGuiAPI.GetFontSize() * 34f, 0f), ImGuiCond_.ImGuiCond_Always);
            if (ImGuiAPI.Begin(WindowName, ref mVisible,
                ImGuiWindowFlags_.ImGuiWindowFlags_NoDocking | ImGuiWindowFlags_.ImGuiWindowFlags_NoCollapse) == false)
            {
                ImGuiAPI.End();
                TickHold(mgr);
                return;
            }

            var total = mgr.TotalCount;
            var settled = Math.Min(mgr.SettledCount, total);
            var fraction = total > 0 ? (float)settled / total : 0f;
            var running = FindRunning(mgr.Jobs);

            var title = running != null
                ? $"Cooking  {settled}/{total}"
                : (total > 0 && settled >= total ? "Cook finished" : (mgr.IsStopping ? "Stopping worker..." : "Cook idle"));
            ImGuiAPI.Text(title);

            ImGuiAPI.ProgressBar(fraction, new EngineNS.Vector2(-1f, 0f),
                total > 0 ? $"资产已结束 {settled}/{total}  ({(int)(fraction * 100)}%)" : "");

            if (running != null)
            {
                ImGuiAPI.PushTextWrapPos(0f);
                ImGuiAPI.TextUnformatted($"asset: {running.Asset}");
                ImGuiAPI.TextUnformatted($"stage: {(string.IsNullOrEmpty(running.Stage) ? (running.State == Cook.TtCookManager.ECookJobState.Running ? "running" : "queued in worker") : running.Stage)}");
                ImGuiAPI.PopTextWrapPos();
                ImGuiAPI.ProgressBar(Math.Max(0f, running.ProgressFraction), new EngineNS.Vector2(-1f, 0f),
                    running.ProgressFraction < 0 ? "当前工作步骤: 等待进度上报" : $"当前工作步骤: {running.ProgressFraction * 100:F1}%（非耗时估计）");
                ImGuiAPI.Text($"worker PID={mgr.ProcessId}   已用时 {running.DisplayElapsedSec:F1}s");
                if (running.State == Cook.TtCookManager.ECookJobState.Running)
                {
                    var now = Environment.TickCount64;
                    var messageAge = Math.Max(0, now - running.LastMessageTick) / 1000.0;
                    var stageAge = Math.Max(0, now - running.LastStageTick) / 1000.0;
                    ImGuiAPI.Text($"最近阶段更新: {stageAge:F1}s 前; worker 消息: {messageAge:F1}s 前");
                    if (messageAge >= 15)
                        ImGuiAPI.TextColored(new EngineNS.Vector4(1f, 0.85f, 0.3f, 1f), "暂未收到新消息; 当前步骤可能阻塞, 尚不能判定死锁");
                }
                if (running.CancelRequested)
                    ImGuiAPI.Text("已请求取消, 等待后台实际退出...");
            }
            else if (total > 0 && settled >= total)
            {
                if (mgr.FailedCount > 0)
                    ImGuiAPI.TextColored(new EngineNS.Vector4(1f, 0.4f, 0.2f, 1f), $"{mgr.FailedCount} failed, see Cook Manager");
                else if (mgr.CancelledCount > 0)
                    ImGuiAPI.TextColored(new EngineNS.Vector4(1f, 0.85f, 0.3f, 1f), $"{mgr.CancelledCount} cancelled");
                else
                    ImGuiAPI.TextColored(new EngineNS.Vector4(0.4f, 1f, 0.4f, 1f), "all succeeded");
            }

            if (running != null)
            {
                if (ImGuiAPI.Button("Cancel", new EngineNS.Vector2(0f, 0f)))
                    mgr.CancelAll();
                ImGuiAPI.SameLine(0f, 6f);
            }
            if (ImGuiAPI.Button("Details", new EngineNS.Vector2(0f, 0f)))
                TtCookManagerPanel.ShowPanel();

            ImGuiAPI.End();
            TickHold(mgr);
        }

        /// <summary>优先取真正在跑的; 没有就取第一个已发给 worker 的</summary>
        static Cook.TtCookManager.FJobEntry FindRunning(IReadOnlyList<Cook.TtCookManager.FJobEntry> jobs)
        {
            Cook.TtCookManager.FJobEntry dispatched = null;
            foreach (var i in jobs)
            {
                if (i.State == Cook.TtCookManager.ECookJobState.Running)
                    return i;
                if (dispatched == null && i.State == Cook.TtCookManager.ECookJobState.Dispatched)
                    dispatched = i;
            }
            return dispatched;
        }

        void TickHold(Cook.TtCookManager mgr)
        {
            // 全了结后不立刻消失, 留几秒让人看清结果; 期间重新提交会打断计时
            if (mgr.TotalCount > 0 && mgr.SettledCount >= mgr.TotalCount)
            {
                var now = Environment.TickCount64;
                if (mSettledTick == 0)
                    mSettledTick = now;
                else if (now - mSettledTick > AutoHideMs)
                    mVisible = false;
            }
            else
            {
                mSettledTick = 0;
            }
        }
    }

    /// <summary>
    /// Cook 管理面板: 扫资产、勾选提交、看每个 job 的终态与 worker 日志尾部。
    /// 同样只读状态 + 发指令, 不驱动状态机。
    /// </summary>
    public class TtCookManagerPanel : IRootForm
    {
        public static TtCookManagerPanel Instance;

        public bool Visible { get => mVisible; set => mVisible = value; }
        bool mVisible;
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; } = default;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        const string WindowName = "Cook Manager###TtCookManagerPanel";
        /// <summary>一次扫描最多列多少条, 防止大仓库把面板拖死</summary>
        public const int MaxScanAssets = 2000;

        readonly List<string> mScanned = new List<string>();
        readonly HashSet<string> mChecked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string mScanDir = "";
        string mStatus = "";
        string[] mLog = Array.Empty<string>();
        long mLogRefreshTick;
        /// <summary>日志快照刷新间隔: GetLogSnapshot 每次拷整个缓冲 (最多 8000 行), 不能每帧拷</summary>
        const long LogRefreshMs = 250;

        public TtCookManagerPanel()
        {
            Instance = this;
        }

        public void Dispose() { }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        public void Cleanup() { }

        public static void ShowPanel()
        {
            if (Instance == null)
                return;
            Instance.mVisible = true;
        }

        public unsafe void OnDraw()
        {
            var mgr = Cook.TtCookManager.Instance;
            if (ImGuiAPI.Begin(WindowName, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None) == false)
            {
                ImGuiAPI.End();
                return;
            }

            ImGuiAPI.Text($"worker: {(mgr.IsWorkerRunning ? "running pid=" + mgr.ProcessId : "stopped")}{(mgr.IsStopping ? " (stopping)" : "")}");
            ImGuiAPI.Text($"batch: {mgr.SettledCount}/{mgr.TotalCount} settled, {mgr.FailedCount} failed, {mgr.CancelledCount} cancelled");
            ImGuiAPI.ProgressBar(mgr.TotalCount > 0 ? (float)mgr.SettledCount / mgr.TotalCount : 0f,
                new EngineNS.Vector2(0f, 0f), "");
            ImGuiAPI.Separator();

            DrawScan(mgr);
            DrawJobs(mgr);
            DrawLog(mgr);

            ImGuiAPI.Separator();
            // 宽度一律传 0 让 ImGui 按文字自适应: 写死像素在大字号/高 DPI 下会截字
            if (ImGuiAPI.Button("Cook Selected", new EngineNS.Vector2(0f, 0f)))
            {
                var added = mgr.SubmitBatch(mChecked, "Scene", false);
                if (added > 0)
                    mStatus = $"submitted {added}";
                else if (mgr.IsStopping)
                    mStatus = "worker is stopping, retry after it exits";
                else
                    mStatus = mChecked.Count == 0 ? "nothing checked" : "nothing new (already in batch)";
                if (added > 0)
                    TtCookProgressForm.ShowProgress();
            }
            ImGuiAPI.SameLine(0f, 6f);
            if (ImGuiAPI.Button("Clear Batch", new EngineNS.Vector2(0f, 0f)))
            {
                mStatus = mgr.ResetBatch() ? "batch cleared" : "batch still running, cancel it first";
            }
            ImGuiAPI.SameLine(0f, 6f);
            if (ImGuiAPI.Button("Cancel All", new EngineNS.Vector2(0f, 0f)))
                mgr.CancelAll();
            ImGuiAPI.SameLine(0f, 6f);
            if (ImGuiAPI.Button("Stop Worker", new EngineNS.Vector2(0f, 0f)))
                mgr.RequestWorkerShutdown();
            if (string.IsNullOrEmpty(mStatus) == false)
                ImGuiAPI.Text(mStatus);

            ImGuiAPI.End();
        }

        void DrawScan(Cook.TtCookManager mgr)
        {
            // worker 起来前没有 hello.Root, 而 worker 又要提交之后才起 —— 不能拿它当前置条件;
            // 编辑器与 worker 共用同一个 content 根, 先用编辑器自己的, worker 报上来后以 worker 为准
            var root = mgr.WorkerRootPath;
            if (string.IsNullOrEmpty(root))
                root = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game);
            if (string.IsNullOrEmpty(root))
            {
                ImGuiAPI.Text("(game root unknown)");
                return;
            }
            ImGuiAPI.Text($"game root: {root}");
            if (string.IsNullOrEmpty(mScanDir))
                ImGuiAPI.Text($"(not scanned yet; lists *{SceneMetaExt}, capped at {MaxScanAssets})");
            if (ImGuiAPI.Button("Scan Scenes", new EngineNS.Vector2(0f, 0f)))
                Scan(root);
            ImGuiAPI.SameLine(0f, 6f);
            if (ImGuiAPI.Button("Check All", new EngineNS.Vector2(0f, 0f)))
            {
                foreach (var i in mScanned)
                    mChecked.Add(i);
            }
            ImGuiAPI.SameLine(0f, 6f);
            if (ImGuiAPI.Button("Uncheck All", new EngineNS.Vector2(0f, 0f)))
                mChecked.Clear();

            // 场景很多时列表会把 jobs/log 挤出窗口, 可收起; 标题带勾选数, 收起也看得到选了多少
            if (ImGuiAPI.CollapsingHeader($"scenes ({mChecked.Count}/{mScanned.Count} checked)###cookScenes", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen) == false)
                return;
            for (var i = 0; i < mScanned.Count; i++)
            {
                var url = mScanned[i];
                var check = mChecked.Contains(url);
                if (ImGuiAPI.Checkbox(url + $"##ck{i}", ref check))
                {
                    if (check)
                        mChecked.Add(url);
                    else
                        mChecked.Remove(url);
                }
            }
        }

        /// <summary>
        /// 扫 root 下的场景 meta 转成资产 URL (相对 root、正斜杠、去 .ameta 后缀)。
        /// 只列场景: 提交时 AssetType 固定为 Scene, 把贴图/材质也列出来只会批量报错。
        /// 只做目录遍历, 不进引擎资产系统, 因此不会与 worker 的加载态打架。
        /// </summary>
        const string SceneMetaExt = ".scene.ameta";
        void Scan(string root)
        {
            mScanned.Clear();
            mChecked.Clear();
            mScanDir = root;
            try
            {
                if (Directory.Exists(root) == false)
                {
                    mStatus = "root missing: " + root;
                    return;
                }
                var full = Path.GetFullPath(root);
                foreach (var file in Directory.EnumerateFiles(full, "*" + SceneMetaExt, SearchOption.AllDirectories))
                {
                    if (mScanned.Count >= MaxScanAssets)
                        break;
                    var rel = Path.GetRelativePath(full, file);
                    var url = rel.Replace('\\', '/');
                    url = url.Substring(0, url.Length - ".ameta".Length);
                    mScanned.Add(url);
                }
                mStatus = $"scanned {mScanned.Count}";
            }
            catch (Exception ex)
            {
                mStatus = "scan failed: " + ex.Message;
            }
        }

        void DrawJobs(Cook.TtCookManager mgr)
        {
            if (mgr.Jobs.Count == 0)
                return;
            ImGuiAPI.Separator();
            ImGuiAPI.Text("jobs");
            for (var i = 0; i < mgr.Jobs.Count; i++)
            {
                var job = mgr.Jobs[i];
                var mark = StateMark(job.State);
                ImGuiAPI.Text($"{mark} #{job.Id} {job.Asset}");
                if (job.State == Cook.TtCookManager.ECookJobState.Running)
                {
                    ImGuiAPI.PushTextWrapPos(0f);
                    ImGuiAPI.TextUnformatted($"     {job.Stage} | {job.DisplayElapsedSec:F1}s");
                    ImGuiAPI.PopTextWrapPos();
                }
                if (string.IsNullOrEmpty(job.Error) == false)
                    ImGuiAPI.TextColored(new EngineNS.Vector4(1f, 0.4f, 0.4f, 1f), "     " + job.Error);
            }
        }

        static string StateMark(Cook.TtCookManager.ECookJobState state)
        {
            switch (state)
            {
                case Cook.TtCookManager.ECookJobState.Done: return "[ok]";
                case Cook.TtCookManager.ECookJobState.Failed: return "[X]";
                case Cook.TtCookManager.ECookJobState.Cancelled: return "[--]";
                case Cook.TtCookManager.ECookJobState.Running: return "[>>]";
                case Cook.TtCookManager.ECookJobState.Dispatched: return "[..]";
                case Cook.TtCookManager.ECookJobState.Crash: return "[!]";
                default: return "[  ]";
            }
        }

        void DrawLog(Cook.TtCookManager mgr)
        {
            ImGuiAPI.Separator();
            ImGuiAPI.Text("manager notices (last lines)");
            var notices = mgr.Notices;
            for (var i = Math.Max(0, notices.Count - 12); i < notices.Count; i++)
                ImGuiAPI.Text(notices[i]);
            ImGuiAPI.Separator();
            if (ImGuiAPI.CollapsingHeader("worker log (last lines)###cookWorkerLog", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen) == false)
                return;
            var host = mgr.WorkerHost;
            var now = Environment.TickCount64;
            if (host != null && now - mLogRefreshTick >= LogRefreshMs)
            {
                // 按时间刷新而不是按行数: 缓冲到上限后行数不再变, 按行数判断会让日志冻住
                mLogRefreshTick = now;
                var snap = host.GetLogSnapshot();
                var start = snap.Length > 60 ? snap.Length - 60 : 0;
                mLog = new string[snap.Length - start];
                Array.Copy(snap, start, mLog, 0, mLog.Length);
            }
            foreach (var line in mLog)
                ImGuiAPI.Text(line);
        }
    }
}
