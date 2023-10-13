using EngineNS.EGui;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EngineNS.Editor.Forms
{
    public class UCpuProfiler : IRootForm
    {
        public UCpuProfiler()
        {
            UEngine.RootFormManager.RegRootForm(this);
        }

        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }

        public void Dispose() { }
        bool mVisible = true;
        public bool Visible 
        {
            get => mVisible; 
            set => mVisible = value;
        }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        Task<Profiler.URpcProfiler.RpcProfilerData> mRpcProfilerData;
        Task<Profiler.URpcProfiler.RpcProfilerThreads> mRpcProfilerThreads;
        List<string> ProfilerThreadNames = new List<string>();
        List<Profiler.URpcProfiler.RpcProfilerData.ScopeInfo> Scopes = new List<Profiler.URpcProfiler.RpcProfilerData.ScopeInfo>();
        void SetCopes(List<Profiler.URpcProfiler.RpcProfilerData.ScopeInfo> src)
        {
            if (mSortMode != ESortMode.None)
            {
                for (int j = 0; j < Scopes.Count; j++)
                {
                    bool find = false;
                    foreach (var i in src)
                    {
                        if (Scopes[j].ShowName == i.ShowName)
                        {
                            find = true;
                            break;
                        }
                    }
                    if (find == false)
                    {
                        Scopes.RemoveAt(j);
                        j--;
                    }
                }
                foreach (var i in src)
                {
                    bool find = false;
                    for (int j = 0; j < Scopes.Count; j++)
                    {
                        if (Scopes[j].ShowName == i.ShowName)
                        {
                            Scopes[j] = i;
                            find = true;
                            break;
                        }
                    }
                    if (find == false)
                    {
                        Scopes.Add(i);
                    }
                }
            }
            else
            {
                Scopes = src;
            }
            //SortScopes();
        }
        private int FindScope(string name)
        {
            for (int i = 0; i < Scopes.Count; i++)
            {
                if (Scopes[i].ShowName == name)
                    return i;
            }
            return -1;
        }
        enum ESortMode
        {
            None = 0,
            ByName,
            ByTime,
        }
        ESortMode mSortMode = ESortMode.None;
        void SortScopes()
        {
            switch (mSortMode)
            {
                case ESortMode.ByName:
                    {
                        Scopes.Sort((x, y) =>
                        {
                            return x.ShowName.CompareTo(y.ShowName);
                        });
                    }
                    break;
                case ESortMode.ByTime:
                    {
                        Scopes.Sort((x, y) =>
                        {
                            return y.AvgTime.CompareTo(x.AvgTime);
                        });
                    }
                    break;
            }

            
        }
        string mFilter;
        bool mFilterFocusd;
        string CurrentThreadName = null;
        string CurrentName = null;
        [ThreadStatic]
        private static Profiler.TimeScope ScopeOnDraw = Profiler.TimeScopeManager.GetTimeScope(typeof(UCpuProfiler), nameof(OnDraw));
        public void OnDraw()
        {
            using (new Profiler.TimeScopeHelper(ScopeOnDraw))
            {
                OnDrawImpl();
            }
        }
        private unsafe void OnDrawImpl()
        {
            if (mRpcProfilerThreads == null || mRpcProfilerThreads.IsCompleted)
            {
                if (UEngine.Instance.RpcModule.RpcManager != null)
                {
                    if (mRpcProfilerThreads != null)
                        ProfilerThreadNames = mRpcProfilerThreads.Result.ThreadNames;
                    mRpcProfilerThreads = Profiler.URpcProfiler_RpcCaller.GetProfilerThreads(0);
                }
            }
                
            if (Visible == false)
                return;
            //ImGuiAPI.SetNextWindowDockID(DockId, DockCond);

            var size = new Vector2(800, 600);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);
            if (EGui.UIProxy.DockProxy.BeginMainForm("CpuProfiler", this, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                var cmdlst = ImGuiAPI.GetWindowDrawList();
                var stats = UEngine.Instance.GfxDevice.RenderCmdQueue.GetStat();
                ImGuiAPI.Text($"CmdList = {stats.NumOfCmdlist};Drawcall = {stats.NumOfDrawcall};Primitive = {stats.NumOfPrimitive};");
                EGui.UIProxy.SearchBarProxy.OnDraw(ref mFilterFocusd, cmdlst, "filter", ref mFilter, ImGuiAPI.GetWindowContentRegionWidth());
                DockId = ImGuiAPI.GetWindowDockID();
                if (ImGuiAPI.BeginTabBar("CPU", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
                {
                    foreach (var i in ProfilerThreadNames)
                    {
                        if (ImGuiAPI.BeginTabItem(i, null, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                        {
                            if (i != CurrentThreadName)
                            {
                                CurrentThreadName = i;
                                CurrentName = null;
                                Scopes.Clear();
                            }
                            
                            if (mRpcProfilerData == null || mRpcProfilerData.IsCompleted)
                            {
                                if (mRpcProfilerData != null)
                                    SetCopes(mRpcProfilerData.Result.Scopes);
                                mRpcProfilerData = Profiler.URpcProfiler_RpcCaller.GetProfilerData(i);
                            }
                            if (ImGuiAPI.BeginChild("TimeScope", in Vector2.MinusOne, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                            {
                                if (ImGuiAPI.BeginTable("TimeScope", 5, ImGuiTableFlags_.ImGuiTableFlags_Resizable | ImGuiTableFlags_.ImGuiTableFlags_ScrollY, in Vector2.Zero, 0.0f))
                                {
                                    var startY = ImGuiAPI.GetItemRectMax().Y;
                                    ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_Headers, 0);
                                    ImGuiAPI.TableSetColumnIndex(0);
                                    ImGuiAPI.Text("Name");
                                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                                    {
                                        mSortMode = ESortMode.ByName;
                                        SortScopes();
                                        CurrentName = null;
                                    }
                                    ImGuiAPI.TableSetColumnIndex(1);
                                    ImGuiAPI.Text("AvgTime");
                                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                                    {
                                        mSortMode = ESortMode.ByTime;
                                        SortScopes();
                                        CurrentName = null;
                                    }
                                    ImGuiAPI.TableSetColumnIndex(2);
                                    ImGuiAPI.Text("AvgHit");
                                    ImGuiAPI.TableSetColumnIndex(3);
                                    ImGuiAPI.Text("MaxTime");
                                    ImGuiAPI.TableSetColumnIndex(4);
                                    ImGuiAPI.Text("Parent");

                                    //ImGuiAPI.TableSetupColumn("Name", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_DefaultSort | ImGuiTableColumnFlags_.ImGuiTableColumnFlags_NoHide, 0.0f, 0);
                                    //if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                                    //{
                                    //    mSortMode = ESortMode.ByName;
                                    //    SortScopes();
                                    //    CurrentName = null;
                                    //}
                                    //ImGuiAPI.TableSetupColumn("AvgTime", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_DefaultSort | ImGuiTableColumnFlags_.ImGuiTableColumnFlags_NoHide, 0.0f, 1);
                                    //if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                                    //{
                                    //    mSortMode = ESortMode.ByTime;
                                    //    SortScopes();
                                    //    CurrentName = null;
                                    //}
                                    //ImGuiAPI.TableSetupColumn("AvgHit", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_DefaultSort | ImGuiTableColumnFlags_.ImGuiTableColumnFlags_NoHide, 0.0f, 2);
                                    //ImGuiAPI.TableSetupColumn("MaxTime", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_DefaultSort | ImGuiTableColumnFlags_.ImGuiTableColumnFlags_NoHide, 0.0f, 3);
                                    //ImGuiAPI.TableSetupColumn("Parent", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_DefaultSort | ImGuiTableColumnFlags_.ImGuiTableColumnFlags_NoHide, 0.0f, 4);
                                    //ImGuiAPI.TableSetupScrollFreeze(1, 1);
                                    //ImGuiAPI.TableHeadersRow();

                                    //ImGuiTableSortSpecs* sorts_specs = ImGuiAPI.TableGetSortSpecs();
                                    //if (sorts_specs != IntPtr.Zero.ToPointer() && sorts_specs->SpecsDirty)
                                    //{

                                    //}
                                    //bool sorts_specs_using_name = (ImGuiAPI.TableGetColumnFlags(0) & (int)ImGuiTableColumnFlags_.ImGuiTableColumnFlags_IsSorted) != 0;
                                    //if (sorts_specs_using_name)
                                    //{
                                    //    mSortMode = ESortMode.ByName;
                                    //    SortScopes();
                                    //    CurrentName = null;
                                    //}

                                    foreach (var j in Scopes)
                                    {
                                        if (string.IsNullOrEmpty(mFilter) == false && j.ShowName.Contains(mFilter) == false)
                                            continue;

                                        ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_None, 0);

                                        ImGuiAPI.TableSetColumnIndex(0);
                                        ImGuiAPI.Text(j.ShowName);
                                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                                        {
                                            CurrentName = j.ShowName;
                                        }
                                        if (j.ShowName == CurrentName)
                                        {
                                            var start = ImGuiAPI.GetItemRectMin();
                                            if (startY <= start.Y)
                                            {
                                                var textSize = ImGuiAPI.CalcTextSize(j.ShowName, false, 0);
                                                var end = new Vector2(start.X + ImGuiAPI.GetWindowContentRegionWidth(), start.Y + textSize.Y);
                                                cmdlst.AddRectFilled(in start, in end, 0x80808080, 1, ImDrawFlags_.ImDrawFlags_None);
                                            }
                                        }

                                        ImGuiAPI.TableSetColumnIndex(1);
                                        ImGuiAPI.Text(j.AvgTime.ToString());
                                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                                        {
                                            PopItemMenu(i, j, "AvgTime");
                                        }
                                        ImGuiAPI.TableSetColumnIndex(2);
                                        ImGuiAPI.Text(j.AvgHit.ToString());
                                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                                        {
                                            PopItemMenu(i, j, "AvgHit");
                                        }
                                        ImGuiAPI.TableSetColumnIndex(3);
                                        ImGuiAPI.Text(j.MaxTime.ToString());
                                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                                        {
                                            PopItemMenu(i, j, "MaxTime");
                                        }
                                        ImGuiAPI.TableSetColumnIndex(4);
                                        Vector4 clr = new Vector4(0.5f, 0.69f, 0.93f, 1);
                                        if (j.Parent != "null")
                                        {
                                            ImGuiAPI.TextColored(in clr, j.Parent);
                                            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                                            {
                                                ImGuiAPI.SetTooltip(j.Parent);
                                            }
                                            var min = ImGuiAPI.GetItemRectMin();
                                            var max = ImGuiAPI.GetItemRectMax();
                                            min.Y = max.Y;
                                            var cmdlist = ImGuiAPI.GetWindowDrawList();
                                            cmdlist.AddLine(in min, in max, EGui.UIProxy.StyleConfig.Instance.LinkStringColor, 1);
                                            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                                            {
                                                CurrentName = j.Parent;
                                            }
                                        }
                                        else
                                            ImGuiAPI.TextColored(in clr, "null");
                                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                                        {
                                            PopItemMenu(i, j, "Parent");
                                        }
                                    }
                                    ImGuiAPI.EndTable();
                                }
                                
                                ImGuiAPI.EndChild();
                            }
                            ImGuiAPI.EndTabItem();
                        }
                    }
                    ImGuiAPI.EndTabBar();
                }

                if (OnDrawMenu != null)
                    OnDrawMenu();
            }
            EGui.UIProxy.DockProxy.EndMainForm();
        }
        bool mMenuShow = false;
        private unsafe void PopItemMenu(string watchingThread, Profiler.URpcProfiler.RpcProfilerData.ScopeInfo scope, string column)
        {
            switch (column)
            {
                case "AvgTime":
                    {
                        OnDrawMenu = null;
                    }
                    break;
                case "AvgHit":
                    {
                        OnDrawMenu = null;
                    }
                    break;
                case "MaxTime":
                    {
                        OnDrawMenu = () =>
                        {
                            if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
                            {
                                mMenuShow = true;
                                if (ImGuiAPI.MenuItem($"Reset", null, false, true))
                                {
                                    var arg = new Profiler.URpcProfiler.ResetMaxTimeArg();
                                    arg.ThreadName = watchingThread;
                                    arg.ScopeName = scope.ShowName;
                                    Profiler.URpcProfiler_RpcCaller.ResetMaxTime(arg);
                                    OnDrawMenu = null;
                                }
                                ImGuiAPI.EndPopup();
                            }
                            else
                            {
                                if (mMenuShow)
                                {
                                    OnDrawMenu = null;
                                }
                                mMenuShow = false;
                            }
                        };
                    }
                    break;
                case "Parent":
                    {
                        OnDrawMenu = null;
                    }
                    break;
                default:
                    {
                        OnDrawMenu = null;
                    }
                    break;
            }
        }
        System.Action OnDrawMenu = null;
    }
}
