using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Controls
{
    public class ULogWatcher : IRootForm
    {
        public ULogWatcher()
        {
            TtEngine.RootFormManager.RegRootForm(this);
            Profiler.Log.OnReportLog += OnReportLog;

            UpdateCategoryFilters();
            Visible = true;
        }
        ~ULogWatcher()
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

                #region Filters
                EngineNS.EGui.UIProxy.CheckBox.DrawCheckBox("Log", ref mIsReportLog);
                ImGuiAPI.SameLine(0, -1);
                if (ImGuiAPI.Button("Clear"))
                {
                    mLogInfos.Clear();
                    mNewLogs.Clear();
                }
                bool check = (TagFilters & Profiler.ELogTag.Info) != 0;
                ImGuiAPI.SameLine(0, -1);
                EngineNS.EGui.UIProxy.CheckBox.DrawCheckBox("Info", ref check);
                if (check)
                {
                    TagFilters |= Profiler.ELogTag.Info;
                }
                else
                {
                    TagFilters &= (~Profiler.ELogTag.Info);
                }
                check = (TagFilters & Profiler.ELogTag.Warning) != 0;
                ImGuiAPI.SameLine(0, -1);
                EngineNS.EGui.UIProxy.CheckBox.DrawCheckBox("Warning", ref check);
                if (check)
                {
                    TagFilters |= Profiler.ELogTag.Warning;
                }
                else
                {
                    TagFilters &= (~Profiler.ELogTag.Warning);
                }
                check = (TagFilters & Profiler.ELogTag.Error) != 0;
                ImGuiAPI.SameLine(0, -1);
                EngineNS.EGui.UIProxy.CheckBox.DrawCheckBox("Error", ref check);
                if (check)
                {
                    TagFilters |= Profiler.ELogTag.Error;
                }
                else
                {
                    TagFilters &= (~Profiler.ELogTag.Error);
                }
                check = (TagFilters & Profiler.ELogTag.Fatal) != 0;
                ImGuiAPI.SameLine(0, -1);
                EngineNS.EGui.UIProxy.CheckBox.DrawCheckBox("Fatal", ref check);
                if (check)
                {
                    TagFilters |= Profiler.ELogTag.Fatal;
                }
                else
                {
                    TagFilters &= (~Profiler.ELogTag.Fatal);
                }
                ImGuiAPI.SameLine(0, 25);
                ImGuiAPI.Text("Category:");
                ImGuiAPI.SameLine(0, -1);
                if (ImGuiAPI.InputText("##CategoryFilter", ref CategoryFilterText))
                {
                    UpdateCategoryFilters();
                }
                #endregion

                #region Command
                ImGuiAPI.Text("Cmd:");
                ImGuiAPI.SameLine(0, -1);
                if (ImGuiAPI.InputText("##Command", ref CommandText))
                {
                    
                }
                ImGuiAPI.SameLine(0, -1);
                if (ImGuiAPI.Button("OK"))
                {
                    var pos = CommandText.IndexOf(' ');
                    if (pos < 0)
                    {
                        pos = CommandText.Length;
                    }
                    var cmdName = CommandText.Substring(0, pos);
                    var cmd = TtCommandManager.Instance.TryGetCommand(cmdName);
                    if (cmd != null)
                    {
                        if (CommandText.Length > pos)
                        {
                            var args = CommandText.Substring(pos + 1);
                            cmd.Execute(args);
                        }
                        else
                        {
                            cmd.Execute(null);
                        }
                    }
                    else
                    {
                        Profiler.Log.WriteLine<Profiler.TtDebugLogCategory>(Profiler.ELogTag.Info, "Commands", $"Command {cmdName} is not found");
                    }
                    CommandText = "";
                }
                #endregion

                if (ImGuiAPI.BeginChild("LogContent", in Vector2.MinusOne, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    UpdateNewLogs();
                    
                    if (ImGuiAPI.BeginTable("Logs", 5, ImGuiTableFlags_.ImGuiTableFlags_Resizable, in Vector2.Zero, 0.0f))
                    {
                        ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_Headers, 0);
                        ImGuiAPI.TableSetColumnIndex(0);
                        ImGuiAPI.Text("Tag");
                        ImGuiAPI.TableSetColumnIndex(1);
                        ImGuiAPI.Text("Category");
                        ImGuiAPI.TableSetColumnIndex(2);
                        ImGuiAPI.Text("Content");
                        ImGuiAPI.TableSetColumnIndex(3);
                        ImGuiAPI.Text("Source");
                        ImGuiAPI.TableSetColumnIndex(4);
                        ImGuiAPI.Text("Line");
                        foreach (var i in mLogInfos)
                        {
                            if ((TagFilters & i.Tag) == 0)
                                continue;

                            if (IsCategory(i.Category) == false)
                                continue;

                            var clr = Vector4.One;
                            switch (i.Tag)
                            {
                                case Profiler.ELogTag.Info:
                                    clr = UCoreStyles.Instance.LogInfoColor.ToColor4Float();
                                    break;
                                case Profiler.ELogTag.Warning:
                                    clr = UCoreStyles.Instance.LogWarningColor.ToColor4Float();
                                    break;
                                case Profiler.ELogTag.Error:
                                    clr = UCoreStyles.Instance.LogErrorColor.ToColor4Float();
                                    break;
                                case Profiler.ELogTag.Fatal:
                                    clr = UCoreStyles.Instance.LogFatalColor.ToColor4Float();
                                    break;
                            }

                            ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_None, 0);
                            ImGuiAPI.TableSetColumnIndex(0);
                            ImGuiAPI.TextColored(in clr, i.Tag.ToString());
                            ImGuiAPI.TableSetColumnIndex(1);
                            ImGuiAPI.Text(i.Category);
                            ImGuiAPI.TableSetColumnIndex(2);
                            ImGuiAPI.Text(i.LogText);
                            ImGuiAPI.TableSetColumnIndex(3);
                            ImGuiAPI.Text(i.SourceFile);
                            ImGuiAPI.TableSetColumnIndex(4);
                            ImGuiAPI.Text(i.SourceLine.ToString());
                        }
                        ImGuiAPI.EndTable();
                    }
                }
                ImGuiAPI.EndChild();
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
        private struct FLogInfo
        {
            public Profiler.ELogTag Tag;
            public string Category;
            public string MemberName;
            public string SourceFile;
            public int SourceLine;
            public string LogText;
        }
        Queue<FLogInfo> mLogInfos = new Queue<FLogInfo>();
        List<FLogInfo> mNewLogs = new List<FLogInfo>();
        public int MaxLogs { get; set; } = 1024;
        public Profiler.ELogTag TagFilters { get; set; } = Profiler.ELogTag.All;
        private string[] CategoryFilters;
        public string CategoryFilterText;
        public string CommandText;
        private bool IsCategory(string cate)
        {
            if (CategoryFilters == null)
                return true;

            foreach (var i in CategoryFilters)
            {
                if (cate.Contains(i))
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
                    while (total >= 0 && mLogInfos.Count > 0)
                    {
                        mLogInfos.Dequeue();
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

                mNewLogs.Add(tmp);
            }
            if (mNewLogs.Count >= 20)
            {
                UpdateNewLogs();
            }
        }
    }
}
