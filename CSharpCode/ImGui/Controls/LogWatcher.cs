using EngineNS.DesignMacross;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Controls
{
    public class TtLogWatcher : IRootForm
    {
        public TtLogWatcher()
        {
            TtEngine.RootFormManager.RegRootForm(this);
            Profiler.Log.OnReportLog += OnReportLog;

            UpdateCategoryFilters();
            Visible = true;
        }
        ~TtLogWatcher()
        {
            Profiler.Log.OnReportLog -= OnReportLog;
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        public void Dispose() { }
        public bool Visible { get; set; }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public bool mIsReportLog = true;
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            var size = new Vector2(800, 600);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm("LogWatcher", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                DockId = ImGuiAPI.GetWindowDockID();
                UpdateNewLogs();
                DrawToolbar();
                DrawCommandLine();

                if (ImGuiAPI.BeginChild("LogContent", in Vector2.MinusOne, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    if (ImGuiAPI.BeginTabBar("LogWatcherTabs", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
                    {
                        if (ImGuiAPI.BeginTabItem("Logs", null, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                        {
                            DrawLogTable();
                            ImGuiAPI.EndTabItem();
                        }
                        if (ImGuiAPI.BeginTabItem("Overview", null, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                        {
                            DrawOverview();
                            ImGuiAPI.EndTabItem();
                        }
                        ImGuiAPI.EndTabBar();
                    }
                }
                ImGuiAPI.EndChild();
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
        private void DrawToolbar()
        {
            EngineNS.EGui.UIProxy.CheckBox.DrawCheckBox("Log", ref mIsReportLog);
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.Button("Clear"))
            {
                lock (mNewLogs)
                {
                    mLogInfos.Clear();
                    mNewLogs.Clear();
                }
            }

            DrawTagFilter("Info", Profiler.ELogTag.Info);
            DrawTagFilter("Warning", Profiler.ELogTag.Warning);
            DrawTagFilter("Error", Profiler.ELogTag.Error);
            DrawTagFilter("Fatal", Profiler.ELogTag.Fatal);

            ImGuiAPI.SameLine(0, 25);
            ImGuiAPI.Text("Category:");
            ImGuiAPI.SameLine(0, -1);
            ImGuiAPI.SetNextItemWidth(220);
            if (ImGuiAPI.InputText("##CategoryFilter", ref CategoryFilterText))
            {
                UpdateCategoryFilters();
            }
            ImGuiAPI.SameLine(0, 12);
            ImGuiAPI.TextDisabled($"{mLogInfos.Count}/{MaxLogs}");
        }

        private void DrawTagFilter(string label, Profiler.ELogTag tag)
        {
            bool check = (TagFilters & tag) != 0;
            ImGuiAPI.SameLine(0, -1);
            if (EngineNS.EGui.UIProxy.CheckBox.DrawCheckBox(label, ref check))
            {
                if (check)
                    TagFilters |= tag;
                else
                    TagFilters &= ~tag;
            }
        }

        private void DrawCommandLine()
        {
            ImGuiAPI.Text("Cmd:");
            ImGuiAPI.SameLine(0, -1);
            ImGuiAPI.SetNextItemWidth(-64);
            ImGuiAPI.InputText("##Command", ref CommandText);
            if (ImGuiAPI.IsItemActive())
            {
                if (ImGuiAPI.IsKeyDown(ImGuiKey.ImGuiKey_UpArrow))
                    SelectCommandHistory(1);
                if (ImGuiAPI.IsKeyDown(ImGuiKey.ImGuiKey_DownArrow))
                    SelectCommandHistory(-1);
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.Button("OK"))
                ExecuteCommand();
        }

        private void SelectCommandHistory(int offset)
        {
            if (CommandHistory.Count == 0)
                return;
            CurSelectHistory += offset;
            if (CurSelectHistory >= CommandHistory.Count)
                CurSelectHistory = 0;
            if (CurSelectHistory < 0)
                CurSelectHistory = CommandHistory.Count - 1;
            CommandText = CommandHistory[CurSelectHistory];
        }

        private void ExecuteCommand()
        {
            if (string.IsNullOrWhiteSpace(CommandText))
                return;

            var pos = CommandText.IndexOf(' ');
            if (pos < 0)
                pos = CommandText.Length;
            var cmdName = CommandText.Substring(0, pos);
            var cmd = TtCommandManager.Instance.TryGetCommand(cmdName);
            if (cmd != null)
            {
                if (CommandText.Length > pos)
                    cmd.Execute(CommandText.Substring(pos + 1));
                else
                    cmd.Execute(null);
            }
            else
            {
                Profiler.Log.WriteLine<Profiler.TtDebugLogCategory>(Profiler.ELogTag.Info, "Commands", $"Command {cmdName} is not found");
            }

            CommandHistory.Remove(CommandText);
            CommandHistory.Add(CommandText);
            if (CommandHistory.Count > 20)
                CommandHistory.RemoveRange(0, CommandHistory.Count - 20);
            CurSelectHistory = CommandHistory.Count - 1;
            CommandText = "";
        }

        private unsafe void DrawLogTable()
        {
            var tableFlags = ImGuiTableFlags_.ImGuiTableFlags_Resizable |
                ImGuiTableFlags_.ImGuiTableFlags_Reorderable |
                ImGuiTableFlags_.ImGuiTableFlags_RowBg |
                ImGuiTableFlags_.ImGuiTableFlags_BordersInnerV |
                ImGuiTableFlags_.ImGuiTableFlags_ScrollY;
            if (ImGuiAPI.BeginTable("Logs", 5, tableFlags, in Vector2.Zero, 0.0f))
            {
                ImGuiAPI.TableSetupScrollFreeze(0, 1);
                ImGuiAPI.TableSetupColumn("Tag", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 82, 0);
                ImGuiAPI.TableSetupColumn("Category", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 140, 0);
                ImGuiAPI.TableSetupColumn("Content", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthStretch, 0, 0);
                ImGuiAPI.TableSetupColumn("Source", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 220, 0);
                ImGuiAPI.TableSetupColumn("Line", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 60, 0);
                ImGuiAPI.TableHeadersRow();

                int rowIndex = 0;
                foreach (var i in mLogInfos)
                {
                    if (!PassesFilters(i))
                        continue;

                    ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_None, 0);
                    ImGuiAPI.TableSetColumnIndex(0);
                    var clr = GetTagColor(i.Tag);
                    ImGuiAPI.TextColored(in clr, i.Tag.ToString());
                    ImGuiAPI.TableSetColumnIndex(1);
                    ImGuiAPI.Text(i.Category ?? "");
                    ImGuiAPI.TableSetColumnIndex(2);
                    ImGuiAPI.Text(i.LogText ?? "");
                    DrawLogContextMenu(i, rowIndex);
                    ImGuiAPI.TableSetColumnIndex(3);
                    ImGuiAPI.Text(i.SourceFile ?? "");
                    ImGuiAPI.TableSetColumnIndex(4);
                    ImGuiAPI.Text(i.SourceLine.ToString());
                    rowIndex++;
                }
                ImGuiAPI.EndTable();
            }
        }

        private void DrawLogContextMenu(FLogInfo info, int rowIndex)
        {
            UIProxy.StyleConfig.Instance.PushPopupStyle();
            if (ImGuiAPI.BeginPopupContextItem($"LogRowContext_{rowIndex}", ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                if (ImGuiAPI.MenuItem("Copy Message", null, false, true))
                    ImGuiAPI.SetClipboardText(info.LogText ?? "");
                if (ImGuiAPI.MenuItem("Copy Source", null, false, true))
                    ImGuiAPI.SetClipboardText($"{info.SourceFile}:{info.SourceLine}");
                if (!string.IsNullOrEmpty(info.SourceFile) && ImGuiAPI.MenuItem("Goto Source", null, false, true))
                {
                    var plugin = EngineNS.Bricks.DevIDE.TtDevIDEPlugin.FindDevIDEPlugin();
                    plugin?.OpenFileAtLine(info.SourceFile, info.SourceLine);
                }
                ImGuiAPI.EndPopup();
            }
            UIProxy.StyleConfig.Instance.PopPopupStyle();
        }

        private unsafe void DrawOverview()
        {
            var sampleCount = BuildTrendData();
            if (sampleCount == 0)
            {
                ImGuiAPI.TextDisabled("No logs match the current filters.");
                return;
            }

            var plotSize = new Vector2(-1, 180);
            if (ImPlotAPI.IsAvailable)
            {
                if (ImPlotAPI.BeginPlot("Log Trend", in plotSize, ImPlotFlags_.ImPlotFlags_NoTitle))
                {
                    ImPlotAPI.SetupAxes("seconds", "count",
                        ImPlotAxisFlags_.ImPlotAxisFlags_NoMenus | ImPlotAxisFlags_.ImPlotAxisFlags_NoHighlight,
                        ImPlotAxisFlags_.ImPlotAxisFlags_AutoFit | ImPlotAxisFlags_.ImPlotAxisFlags_NoMenus | ImPlotAxisFlags_.ImPlotAxisFlags_NoHighlight);
                    ImPlotAPI.PlotLine("Info", mTrendXs, mTrendInfo, sampleCount);
                    ImPlotAPI.PlotLine("Warning", mTrendXs, mTrendWarning, sampleCount);
                    ImPlotAPI.PlotLine("Error", mTrendXs, mTrendError, sampleCount);
                    ImPlotAPI.PlotLine("Fatal", mTrendXs, mTrendFatal, sampleCount);
                    ImPlotAPI.PlotLine("Total", mTrendXs, mTrendTotal, sampleCount, ImPlotLineFlags_.ImPlotLineFlags_Shaded);
                    ImPlotAPI.EndPlot();
                }
            }
            else
            {
                fixed (float* total = mTrendTotal)
                {
                    ImGuiAPI.PlotLines("Total logs / sec", total, sampleCount, 0, "ImPlot native binding unavailable", 0, GetTrendMax(), plotSize, sizeof(float));
                }
            }

            ImGuiAPI.Spacing();
            DrawCategoryOverview();
        }

        private void DrawCategoryOverview()
        {
            var categoryCount = BuildCategoryData();
            if (categoryCount == 0)
                return;

            var plotSize = new Vector2(-1, 160);
            if (ImPlotAPI.IsAvailable && ImPlotAPI.BeginPlot("Top Categories", in plotSize, ImPlotFlags_.ImPlotFlags_NoTitle))
            {
                ImPlotAPI.SetupAxes("category", "count",
                    ImPlotAxisFlags_.ImPlotAxisFlags_NoTickLabels | ImPlotAxisFlags_.ImPlotAxisFlags_NoMenus | ImPlotAxisFlags_.ImPlotAxisFlags_NoHighlight,
                    ImPlotAxisFlags_.ImPlotAxisFlags_AutoFit | ImPlotAxisFlags_.ImPlotAxisFlags_NoMenus | ImPlotAxisFlags_.ImPlotAxisFlags_NoHighlight);
                ImPlotAPI.PlotBars("Logs", mCategoryXs, mCategoryYs, categoryCount, 0.55);
                ImPlotAPI.EndPlot();
            }

            if (ImGuiAPI.BeginTable("TopCategoryLegend", 3, ImGuiTableFlags_.ImGuiTableFlags_RowBg | ImGuiTableFlags_.ImGuiTableFlags_BordersInnerV, in Vector2.Zero, 0.0f))
            {
                ImGuiAPI.TableSetupColumn("#", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 36, 0);
                ImGuiAPI.TableSetupColumn("Category", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthStretch, 0, 0);
                ImGuiAPI.TableSetupColumn("Count", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 80, 0);
                ImGuiAPI.TableHeadersRow();
                for (int i = 0; i < categoryCount; i++)
                {
                    ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_None, 0);
                    ImGuiAPI.TableSetColumnIndex(0);
                    ImGuiAPI.Text(i.ToString());
                    ImGuiAPI.TableSetColumnIndex(1);
                    ImGuiAPI.Text(mCategoryLabels[i]);
                    ImGuiAPI.TableSetColumnIndex(2);
                    ImGuiAPI.Text(mCategoryYs[i].ToString("0"));
                }
                ImGuiAPI.EndTable();
            }
        }

        private bool PassesFilters(FLogInfo info)
        {
            if ((TagFilters & info.Tag) == 0)
                return false;
            return IsCategory(info.Category);
        }

        private Vector4 GetTagColor(Profiler.ELogTag tag)
        {
            switch (tag)
            {
                case Profiler.ELogTag.Info:
                    return UCoreStyles.Instance.LogInfoColor.ToColor4Float();
                case Profiler.ELogTag.Warning:
                    return UCoreStyles.Instance.LogWarningColor.ToColor4Float();
                case Profiler.ELogTag.Error:
                    return UCoreStyles.Instance.LogErrorColor.ToColor4Float();
                case Profiler.ELogTag.Fatal:
                    return UCoreStyles.Instance.LogFatalColor.ToColor4Float();
                default:
                    return Vector4.One;
            }
        }

        private struct FLogInfo
        {
            public Profiler.ELogTag Tag;
            public string Category;
            public string MemberName;
            public string SourceFile;
            public int SourceLine;
            public string LogText;
            public double TimeSeconds;
        }
        Queue<FLogInfo> mLogInfos = new Queue<FLogInfo>();
        List<FLogInfo> mNewLogs = new List<FLogInfo>();
        readonly DateTime mStartTime = DateTime.UtcNow;
        readonly float[] mTrendXs = new float[60];
        readonly float[] mTrendInfo = new float[60];
        readonly float[] mTrendWarning = new float[60];
        readonly float[] mTrendError = new float[60];
        readonly float[] mTrendFatal = new float[60];
        readonly float[] mTrendTotal = new float[60];
        readonly float[] mCategoryXs = new float[8];
        readonly float[] mCategoryYs = new float[8];
        readonly List<string> mCategoryLabels = new List<string>(8);
        public int MaxLogs { get; set; } = 1024;
        public Profiler.ELogTag TagFilters { get; set; } = Profiler.ELogTag.All;
        private string[] CategoryFilters;
        public string CategoryFilterText;
        public string CommandText;
        public List<string> CommandHistory = new List<string>();
        public int CurSelectHistory = 0;
        private int BuildTrendData()
        {
            Array.Clear(mTrendInfo, 0, mTrendInfo.Length);
            Array.Clear(mTrendWarning, 0, mTrendWarning.Length);
            Array.Clear(mTrendError, 0, mTrendError.Length);
            Array.Clear(mTrendFatal, 0, mTrendFatal.Length);
            Array.Clear(mTrendTotal, 0, mTrendTotal.Length);

            var now = (float)(DateTime.UtcNow - mStartTime).TotalSeconds;
            int matched = 0;
            for (int i = 0; i < mTrendXs.Length; i++)
                mTrendXs[i] = i - (mTrendXs.Length - 1);

            foreach (var log in mLogInfos)
            {
                if (!PassesFilters(log))
                    continue;
                var age = (int)Math.Floor(now - log.TimeSeconds);
                if (age < 0 || age >= mTrendXs.Length)
                    continue;

                var index = mTrendXs.Length - 1 - age;
                switch (log.Tag)
                {
                    case Profiler.ELogTag.Info:
                        mTrendInfo[index]++;
                        break;
                    case Profiler.ELogTag.Warning:
                        mTrendWarning[index]++;
                        break;
                    case Profiler.ELogTag.Error:
                        mTrendError[index]++;
                        break;
                    case Profiler.ELogTag.Fatal:
                        mTrendFatal[index]++;
                        break;
                }
                mTrendTotal[index]++;
                matched++;
            }
            return matched > 0 ? mTrendXs.Length : 0;
        }

        private float GetTrendMax()
        {
            float max = 1;
            for (int i = 0; i < mTrendTotal.Length; i++)
            {
                if (mTrendTotal[i] > max)
                    max = mTrendTotal[i];
            }
            return max;
        }

        private int BuildCategoryData()
        {
            var categoryCounts = new Dictionary<string, int>();
            foreach (var log in mLogInfos)
            {
                if (!PassesFilters(log))
                    continue;
                var category = string.IsNullOrEmpty(log.Category) ? "Default" : log.Category;
                if (categoryCounts.TryGetValue(category, out var count))
                    categoryCounts[category] = count + 1;
                else
                    categoryCounts.Add(category, 1);
            }

            var sorted = new List<KeyValuePair<string, int>>(categoryCounts);
            sorted.Sort((left, right) => right.Value.CompareTo(left.Value));
            mCategoryLabels.Clear();
            var resultCount = Math.Min(mCategoryXs.Length, sorted.Count);
            for (int i = 0; i < resultCount; i++)
            {
                mCategoryXs[i] = i;
                mCategoryYs[i] = sorted[i].Value;
                mCategoryLabels.Add(sorted[i].Key);
            }
            return resultCount;
        }

        private bool IsCategory(string cate)
        {
            if (CategoryFilters == null)
                return true;

            if (string.IsNullOrEmpty(cate))
                cate = "";
            foreach (var i in CategoryFilters)
            {
                if (string.IsNullOrEmpty(i))
                    continue;
                if (cate.Contains(i, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        private void UpdateCategoryFilters()
        {
            if (string.IsNullOrEmpty(CategoryFilterText))
            {
                CategoryFilters = null;
                return;
            }
            CategoryFilters = CategoryFilterText.Split(',');
        }
        private void UpdateNewLogs()
        {
            lock (mNewLogs)
            {
                foreach (var i in mNewLogs)
                {
                    mLogInfos.Enqueue(i);
                }
                mNewLogs.Clear();

                int total = mNewLogs.Count + mLogInfos.Count;
                if (total > MaxLogs)
                {
                    total = total - MaxLogs;
                    while (total > 0 && mLogInfos.Count > 0)
                    {
                        mLogInfos.Dequeue();
                        total--;
                    }
                }
            }
        }
        public void OnReportLog(Profiler.ELogTag tag, string category, string memberName, string sourceFilePath, int sourceLineNumber, string info)
        {
            if (mIsReportLog == false)
                return;
            lock (mNewLogs)
            {
                FLogInfo tmp;
                tmp.Tag = tag;
                tmp.Category = category;
                tmp.MemberName = memberName;
                tmp.SourceFile = sourceFilePath;
                tmp.SourceLine = sourceLineNumber;
                tmp.LogText = info;
                tmp.TimeSeconds = (DateTime.UtcNow - mStartTime).TotalSeconds;

                mNewLogs.Add(tmp);
            }
            if (mNewLogs.Count >= 20)
            {
                UpdateNewLogs();
            }
        }
    }
}
