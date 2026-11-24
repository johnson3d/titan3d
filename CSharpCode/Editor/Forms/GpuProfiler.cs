using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Forms
{
    public class TtGpuProfiler : IRootForm
    {
        public TtGpuProfiler()
        {
            TtEngine.RootFormManager.RegRootForm(this);
        }

        public async Thread.Async.TtTask<bool> Initialize()
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
        public void OnDraw()
        {
            OnDrawImpl();
        }
        string mFilter;
        bool mFilterFocusd;

        // 每帧从 manager 快照一次, 之后所有绘制都只读这份: 直接遍历 Scopes 会与 TickExpire 的 Remove 撞车
        readonly List<NxRHI.TtGpuScope> mAllScopes = new List<NxRHI.TtGpuScope>();
        readonly HashSet<NxRHI.TtGpuScope> mAllScopeSet = new HashSet<NxRHI.TtGpuScope>();
        // ByList 的排序结果
        readonly List<NxRHI.TtGpuScope> mSortCache = new List<NxRHI.TtGpuScope>();

        #region ByTree entries
        // 树节点载体: Scope 为 null 表示"名字前缀分组"的虚拟节点。
        // entry 按 scope 对象/分组名跨帧复用, 每帧只清空 Children 重建层级, 避免每帧堆分配
        sealed class TtTreeEntry
        {
            public string Id;                 // ImGui 稳定 ID, 不能随 Rec 之类的显示文本变化
            public string Name;
            public NxRHI.TtGpuScope Scope;    // null = 分组
            public readonly List<TtTreeEntry> Children = new List<TtTreeEntry>();
            public double AvgUS;
            public ulong LastUS;
            public ulong PeakUS;
            public uint Rec;
            public bool IsGroup => Scope == null;
        }
        readonly Dictionary<NxRHI.TtGpuScope, TtTreeEntry> mScopeEntries = new Dictionary<NxRHI.TtGpuScope, TtTreeEntry>();
        readonly Dictionary<string, TtTreeEntry> mGroupEntries = new Dictionary<string, TtTreeEntry>();
        readonly List<TtTreeEntry> mRootEntries = new List<TtTreeEntry>();
        readonly HashSet<TtTreeEntry> mVisibleEntries = new HashSet<TtTreeEntry>();
        readonly List<NxRHI.TtGpuScope> mStaleScopes = new List<NxRHI.TtGpuScope>();
        const int MaxTreeDepth = 32;
        #endregion

        private unsafe void OnDrawImpl()
        {
            if (Visible == false)
                return;

            var size = new Vector2(800, 600);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm(TtLocalization.Label("GpuProfiler", "###GpuProfiler"), this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                var cmdlst = ImGuiAPI.GetWindowDrawList();
                var stats = TtEngine.Instance.GfxDevice.RenderQueue.QueueStats;
                ImGuiAPI.Text(TtLocalization.Format("CmdList = {0};Drawcall = {1};Primitive = {2};", stats.NumOfCmdlist, stats.NumOfDrawcall, stats.NumOfPrimitive));
                // 采样实例池状态: capacity 是最近一段时间峰值定的目标容量, 需求涨了会直接扩容不丢采样;
                // dropped>0 说明有帧没走 EndFrame(窗口丢失/异常路径), 那些采样已被丢弃
                var gpuPool = TtEngine.Instance.ProfilerModule?.GpuTimeScopeManager?.Pool;
                if (gpuPool != null)
                {
                    var gpuFrames = TtEngine.Instance.ProfilerModule.GpuFrameContainerManager;
                    // scopes 是统计表里的活跃项数: 关场景后它会跟着超时摘除往下掉, 是验证过期机制最直接的读数
                    var gpuMgr = TtEngine.Instance.ProfilerModule.GpuTimeScopeManager;
                    ImGuiAPI.Text(TtLocalization.Format("GpuScopePool: total={0};in-flight={1};free={2};capacity={3};peak={4};dropped={5};scopes={6};expire={7}ms",
                        gpuPool.TotalCount, gpuPool.InFlight, gpuPool.FreeCount, gpuPool.Capacity, gpuPool.PeakInFlight, gpuFrames.DroppedFrames,
                        gpuMgr.ScopeCount, gpuMgr.ExpireMS));
                }
                EGui.UIProxy.SearchBarProxy.OnDraw(ref mFilterFocusd, cmdlst, "filter", ref mFilter, ImGuiAPI.GetWindowContentRegionWidth());
                DockId = ImGuiAPI.GetWindowDockID();
                RefreshScopes();

                EGui.UIProxy.StyleConfig.Instance.PushPanelChildStyle(EGui.UIProxy.StyleConfig.Instance.SecondPanelBackground);
                if (ImGuiAPI.BeginChild("TimeScope", in Vector2.MinusOne, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    if (ImGuiAPI.BeginTabBar("GpuShowMode", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
                    {
                        if (ImGuiAPI.BeginTabItem("ByList", null, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                        {
                            DrawByList();
                            ImGuiAPI.EndTabItem();
                        }
                        if (ImGuiAPI.BeginTabItem("ByTree", null, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                        {
                            DrawByTree();
                            ImGuiAPI.EndTabItem();
                        }
                        ImGuiAPI.EndTabBar();
                    }
                }
                ImGuiAPI.EndChild();
                EGui.UIProxy.StyleConfig.Instance.PopPanelChildStyle();

                if (OnDrawMenu != null)
                    OnDrawMenu();
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }

        void RefreshScopes()
        {
            var mgr = TtEngine.Instance.ProfilerModule?.GpuTimeScopeManager;
            mAllScopeSet.Clear();
            if (mgr == null)
            {
                mAllScopes.Clear();
                return;
            }
            mgr.SnapshotScopes(mAllScopes);
            foreach (var i in mAllScopes)
                mAllScopeSet.Add(i);
        }

        private static bool MatchFilter(NxRHI.TtGpuScope scope, string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return true;
            return scope.Name != null && scope.Name.Contains(filter);
        }

        #region ByList
        private unsafe void DrawByList()
        {
            // 单帧值噪声大, 按 EMA 平均降序排; 谁真的贵一眼就看得到
            mSortCache.Clear();
            foreach (var j in mAllScopes)
            {
                if (MatchFilter(j, mFilter) == false)
                    continue;
                mSortCache.Add(j);
            }
            mSortCache.Sort((x, y) => y.AvgTimeUS.CompareTo(x.AvgTimeUS));

            //GpuScope 名字都是 EngineNS.NxRHI... / Pass 名这种长串, 原来用 WidthStretch 列,
            //面板一窄就被压到只剩一两个字符, 看上去全是一条条碎条; 改成固定列宽 + 横向滚动 + tooltip
            var tableFlags = ImGuiTableFlags_.ImGuiTableFlags_Resizable |
                ImGuiTableFlags_.ImGuiTableFlags_ScrollY |
                ImGuiTableFlags_.ImGuiTableFlags_ScrollX |
                ImGuiTableFlags_.ImGuiTableFlags_RowBg |
                ImGuiTableFlags_.ImGuiTableFlags_BordersInnerV |
                ImGuiTableFlags_.ImGuiTableFlags_BordersOuterH |
                ImGuiTableFlags_.ImGuiTableFlags_SizingFixedFit;
            if (ImGuiAPI.BeginTable("GpuScopeTable", 6, tableFlags, in Vector2.Zero, 0.0f))
            {
                ImGuiAPI.TableSetupScrollFreeze(0, 1);
                ImGuiAPI.TableSetupColumn("Name", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 420, 0);
                ImGuiAPI.TableSetupColumn("AvgUS", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 90, 0);
                ImGuiAPI.TableSetupColumn("LastUS", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 90, 0);
                ImGuiAPI.TableSetupColumn("PeakUS", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 90, 0);
                ImGuiAPI.TableSetupColumn("Rec", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 50, 0);
                ImGuiAPI.TableSetupColumn("Parent", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 420, 0);
                DrawHeader(6);

                foreach (var j in mSortCache)
                {
                    var scopeName = j.Name;
                    ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_None, 0);

                    ImGuiAPI.TableSetColumnIndex(0);
                    // Rec > 1: 同一帧里这个 pass 被记录了多次(多 viewport / 预览窗)
                    if (j.RecordCount > 1)
                        ImGuiAPI.Text(TtLocalization.Format("{0} (x{1})", scopeName, j.RecordCount));
                    else
                        ImGuiAPI.Text(scopeName);
                    if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                    {
                        ImGuiAPI.SetTooltip(scopeName);
                    }

                    DrawScopeValueColumns(j.SampleCount == 0, j.AvgTimeUS, j.TimeUS, j.PeakTimeUS, j.RecordCount);

                    ImGuiAPI.TableSetColumnIndex(5);
                    var parent = j.Parent;
                    ImGuiAPI.Text(parent == null ? "-" : parent.Name);
                }
                ImGuiAPI.EndTable();
            }
        }
        #endregion

        #region ByTree
        private unsafe void DrawByTree()
        {
            BuildTree();

            var tableFlags = ImGuiTableFlags_.ImGuiTableFlags_Resizable |
                ImGuiTableFlags_.ImGuiTableFlags_ScrollY |
                ImGuiTableFlags_.ImGuiTableFlags_RowBg |
                ImGuiTableFlags_.ImGuiTableFlags_BordersInnerV |
                ImGuiTableFlags_.ImGuiTableFlags_BordersOuterH |
                ImGuiTableFlags_.ImGuiTableFlags_SizingFixedFit;
            if (ImGuiAPI.BeginTable("GpuScopeTreeTable", 5, tableFlags, in Vector2.Zero, 0.0f))
            {
                ImGuiAPI.TableSetupScrollFreeze(0, 1);
                ImGuiAPI.TableSetupColumn("Name", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 480, 0);
                ImGuiAPI.TableSetupColumn("AvgUS", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 90, 0);
                ImGuiAPI.TableSetupColumn("LastUS", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 90, 0);
                ImGuiAPI.TableSetupColumn("PeakUS", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 90, 0);
                ImGuiAPI.TableSetupColumn("Rec", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 50, 0);
                DrawHeader(5);

                DrawTreeRows(mRootEntries, 0);
                ImGuiAPI.EndTable();
            }
        }

        private unsafe void DrawTreeRows(List<TtTreeEntry> nodes, int depth)
        {
            if (nodes == null || nodes.Count == 0 || depth > MaxTreeDepth)
                return;

            foreach (var e in nodes)
            {
                int numChild = e.Children.Count;

                ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_None, 0);
                ImGuiAPI.TableSetColumnIndex(0);

                var flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow |
                    ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;
                if (numChild == 0)
                    flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;

                // "显示文本##ID": 显示串里的 (xN)/(N项) 会变, ID 部分固定, 否则展开状态会跟着丢
                string display;
                if (e.IsGroup)
                    display = TtLocalization.Format("{0} ({1})", e.Name, numChild);
                else if (e.Rec > 1)
                    display = TtLocalization.Format("{0} (x{1})", e.Name, e.Rec);
                else
                    display = e.Name;
                bool open = ImGuiAPI.TreeNodeEx(display + "##" + e.Id, flags);
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                {
                    ImGuiAPI.SetTooltip(e.IsGroup ? TtLocalization.Format("{0} (grouped by name prefix)", e.Name) : e.Name);
                }

                DrawScopeValueColumns(e.Rec == 0, e.AvgUS, e.LastUS, e.PeakUS, e.Rec);

                // ImGui 的语义是"TreeNodeEx 返回 true 才 push 了 ID/缩进", 折叠状态下不需要 TreePop;
                // leaf 也是恒展开, 同样要 pop —— 与 TtTreeNodeDrawer.DrawTree 的写法保持一致。
                // 写成"numChild>0 就 pop"会同时产生 Missing TreePop(leaf 少弹)和 Mismatching PushID/PopID(折叠多弹)
                if (open)
                {
                    // 子行必须在 TreePop 之前画, 此时缩进还在栈上; 它们的 row 会插在本行之后
                    if (numChild > 0)
                        DrawTreeRows(e.Children, depth + 1);
                    ImGuiAPI.TreePop();
                }
            }
        }

        private unsafe void DrawHeader(int numColumns)
        {
            ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_Headers, 0);
            ImGuiAPI.TableSetColumnIndex(0);
            ImGuiAPI.Text(TtLocalization.Tr("Name"));
            ImGuiAPI.TableSetColumnIndex(1);
            ImGuiAPI.Text(TtLocalization.Tr("AvgUS"));
            ImGuiAPI.TableSetColumnIndex(2);
            ImGuiAPI.Text(TtLocalization.Tr("LastUS"));
            ImGuiAPI.TableSetColumnIndex(3);
            ImGuiAPI.Text(TtLocalization.Tr("PeakUS"));
            ImGuiAPI.TableSetColumnIndex(4);
            ImGuiAPI.Text(TtLocalization.Tr("Rec"));
            if (numColumns > 5)
            {
                ImGuiAPI.TableSetColumnIndex(5);
                ImGuiAPI.Text(TtLocalization.Tr("Parent"));
            }
        }

        // AvgUS / LastUS / PeakUS / Rec 四列, ByList 与 ByTree 共用; NotSampled 时显示 - 而不是 0
        private static unsafe void DrawScopeValueColumns(bool notSampled, double avgUS, ulong lastUS, ulong peakUS, uint rec)
        {
            ImGuiAPI.TableSetColumnIndex(1);
            ImGuiAPI.Text(notSampled ? "-" : avgUS.ToString("F0"));

            ImGuiAPI.TableSetColumnIndex(2);
            ImGuiAPI.Text(notSampled ? "-" : lastUS.ToString());

            ImGuiAPI.TableSetColumnIndex(3);
            ImGuiAPI.Text(peakUS == 0 ? "-" : peakUS.ToString());

            ImGuiAPI.TableSetColumnIndex(4);
            ImGuiAPI.Text(rec.ToString());
        }

        private void BuildTree()
        {
            // 1. 清空上一帧的层级, entry 对象本身复用
            mRootEntries.Clear();
            mVisibleEntries.Clear();
            foreach (var i in mScopeEntries.Values)
                i.Children.Clear();
            foreach (var i in mGroupEntries.Values)
                i.Children.Clear();

            // 2. 回收已经过期删除的 scope 的 entry
            mStaleScopes.Clear();
            foreach (var i in mScopeEntries)
            {
                if (mAllScopeSet.Contains(i.Key) == false)
                    mStaleScopes.Add(i.Key);
            }
            foreach (var i in mStaleScopes)
                mScopeEntries.Remove(i);

            // 3. filter 命中集合(带祖先补全, 否则层级被截断, 看起来还是拍平的)
            bool filtering = string.IsNullOrEmpty(mFilter) == false;
            var picked = new List<NxRHI.TtGpuScope>();
            foreach (var j in mAllScopes)
            {
                if (MatchFilter(j, mFilter))
                    picked.Add(j);
            }
            if (filtering)
            {
                int idx = 0;
                while (idx < picked.Count)
                {
                    var p = picked[idx].Parent;
                    int guard = 0;
                    while (p != null && guard++ < MaxTreeDepth)
                    {
                        if (picked.Contains(p) == false)
                            picked.Add(p);
                        p = p.Parent;
                    }
                    idx++;
                }
            }

            // 4. 建叶子 entry 并填值
            foreach (var j in picked)
            {
                TtTreeEntry entry;
                if (mScopeEntries.TryGetValue(j, out entry) == false)
                {
                    entry = new TtTreeEntry();
                    entry.Scope = j;
                    entry.Name = j.Name;
                    entry.Id = "S" + j.Name;
                    mScopeEntries.Add(j, entry);
                }
                entry.AvgUS = j.AvgTimeUS;
                entry.LastUS = j.TimeUS;
                entry.PeakUS = j.PeakTimeUS;
                entry.Rec = j.RecordCount;
                mVisibleEntries.Add(entry);
            }

            // 5. 按真实 BeginPass 嵌套挂第一次; 挂不上的(无父/父已过期/会成环)当根
            var orphans = new List<TtTreeEntry>();
            foreach (var e in mVisibleEntries)
            {
                var parent = e.Scope.Parent;
                TtTreeEntry parentEntry = null;
                if (parent != null)
                    mScopeEntries.TryGetValue(parent, out parentEntry);
                if (parentEntry == null || ReferenceEquals(parentEntry, e) || IsAncestorEntry(e, parentEntry))
                {
                    orphans.Add(e);
                    continue;
                }
                parentEntry.Children.Add(e);
            }

            // 6. BeginPass 实际很少嵌套(面板 Parent 列基本是 -), 所以对孤儿再按名字前缀兜底分组:
            //    BloomNode:Up0 / BloomNode:Down0 -> BloomNode 一组。真实层级优先, 分组只在无父时生效
            var ungrouped = new List<TtTreeEntry>();
            foreach (var e in orphans)
            {
                var prefix = GetNamePrefix(e.Name);
                if (prefix == null)
                {
                    ungrouped.Add(e);
                    continue;
                }
                TtTreeEntry group;
                if (mGroupEntries.TryGetValue(prefix, out group) == false)
                {
                    group = new TtTreeEntry();
                    group.Scope = null;
                    group.Name = prefix;
                    group.Id = "G" + prefix;
                    mGroupEntries.Add(prefix, group);
                }
                group.Children.Add(e);
            }

            // 7. 分组至少要 2 个子项才有意义(否则只是凭空多一层), 不足的把子项提升为根
            foreach (var g in mGroupEntries.Values)
            {
                if (g.Children.Count >= 2)
                {
                    Accumulate(g);
                    mRootEntries.Add(g);
                    continue;
                }
                foreach (var c in g.Children)
                    mRootEntries.Add(c);
            }
            foreach (var e in ungrouped)
                mRootEntries.Add(e);

            // 8. 每层按 EMA 平均降序
            SortByAvg(mRootEntries);
            foreach (var i in mScopeEntries.Values)
                SortByAvg(i.Children);
            foreach (var i in mGroupEntries.Values)
                SortByAvg(i.Children);
        }

        private static void SortByAvg(List<TtTreeEntry> list)
        {
            if (list.Count < 2)
                return;
            list.Sort((x, y) => y.AvgUS.CompareTo(x.AvgUS));
        }

        // 分组的时间列: Avg/Last/Rec 求和, Peak 取最大(峰值相加没有物理意义)
        private static void Accumulate(TtTreeEntry group)
        {
            double avg = 0;
            ulong last = 0;
            ulong peak = 0;
            uint rec = 0;
            foreach (var c in group.Children)
            {
                if (c.IsGroup)
                    Accumulate(c);
                avg += c.AvgUS;
                last += c.LastUS;
                if (c.PeakUS > peak)
                    peak = c.PeakUS;
                rec += c.Rec;
            }
            group.AvgUS = avg;
            group.LastUS = last;
            group.PeakUS = peak;
            group.Rec = rec;
        }

        // 名字分隔符: BeginPass 实际都是平铺调用(每个渲染节点自己拿 cmdlist), 没有嵌套关系可用,
        // 所以只能按名字里最后一个分隔符归组: BloomNode:Up0 -> BloomNode, RL_Background -> RL
        private static readonly char[] NameSeparators = new char[] { ':', '_' };
        private static string GetNamePrefix(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            int idx = name.LastIndexOfAny(NameSeparators);
            if (idx <= 0 || idx == name.Length - 1)
                return null;
            return name.Substring(0, idx);
        }

        // ancestor 是不是 entry 的祖先: 带步数保护防止脏数据成环(帧1 A在B内, 帧2 B在A内 就互为父)
        private static bool IsAncestorEntry(TtTreeEntry ancestor, TtTreeEntry node)
        {
            if (node.Scope == null)
                return false;
            int guard = 0;
            var p = node.Scope.Parent;
            while (p != null && guard++ < MaxTreeDepth)
            {
                if (ReferenceEquals(p, ancestor.Scope))
                    return true;
                p = p.Parent;
            }
            return false;
        }
        #endregion

        protected bool mMenuShow = false;
        private unsafe void PopItemMenu(string watchingThread, Profiler.TtRpcProfiler.RpcProfilerData.ScopeInfo scope, string column)
        {

        }
        System.Action OnDrawMenu = null;
    }
}
