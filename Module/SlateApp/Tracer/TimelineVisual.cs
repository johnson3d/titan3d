using System;
using System.Collections.Generic;
using EngineNS;
using EngineNS.Profiler.Trace;

namespace Tracer
{
    /// <summary>
    /// 事件流时间线: 横轴是帧内时间, 纵轴每条 CPU 线程一条泳道(GPU 队列等 P1 接入)。
    ///
    /// 事件画成矩形, 纵向按 ParentId 链的深度内缩 —— 不用生成树结构,
    /// 因为事件在流里本来就是只 append 的数组, ParentId 恒小于自身下标。
    /// </summary>
    public class TtTimelineVisual : EngineNS.IRootForm
    {
        #region IRootForm
        public TtTimelineVisual()
        {
            TtEngine.RootFormManager.RegRootForm(this);
        }

        public async EngineNS.Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }

        public void Dispose()
        {
            TtEngine.RootFormManager.UnregRootForm(this);
        }

        bool mVisible = true;
        public bool Visible
        {
            get => mVisible;
            set => mVisible = value;
        }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        #endregion

        #region Lane model
        internal class TtLane
        {
            public string Name;
            public ETraceChannel Channel;
            public TtTraceEvent[] Events;
            public int Count;
            // 每条事件的嵌套深度, 用来做纵向内缩; 建 lane 时算一次
            public byte[] Depth;
            /// <summary>可绘制事件数: NameId&lt;0(未 End 被作废) 和 DurUS&lt;=0 的都不算</summary>
            public int VisibleCount;
            /// <summary>最深嵌套层数, 只用于深度过滤和名字缩进</summary>
            public int MaxDepth;
            /// <summary>可绘制事件按开始时间排好的显示顺序, 一行一个从; 建 lane 时算一次</summary>
            public int[] Order;
            /// <summary>子树是否收起(事件下标): 收起后后代整支不占行, 右边横条跟着少那几行</summary>
            public bool[] Collapsed;
            public int CollapsedCount;
            /// <summary>有没有子事件: 没子的事不画三角, 也没得折</summary>
            public bool[] HasChildren;
        }

        TtFrame mFrame;
        readonly List<TtLane> mLanes = new List<TtLane>();
        long mFrameOriginUS;   // 帧起点在 capture 时间轴上的位置
        long mFrameSpanUS;     // 帧内最大时间跨度, Fit 时用它
        int mBuiltFrameIndex = int.MinValue;

        int mSelectedLane = -1;
        int mSelectedEvent = -1;
        // 悬停命中: 不点也能先看清是什么, 且点击直接复用这次命中的结果
        int mHoverLane = -1;
        int mHoverEvent = -1;
        // 悬停在行首三角上: 这种点击是折叠, 不是选中
        bool mHoverOnArrow;

        // 深度过滤(0=不限): 只留嵌套深度小于它的行。事件名很长, 一层一行时行数一多就得靠它
        // 把无关的深层叶子裁掉, 不然名字栏自己要滚几十屏
        int mDepthLimit = 0;
        // Shift+滚轮: 行高倍率
        float mLayerScale = 1.0f;
        // 纵向滚动量(像素): 行数超出画布高度时才用得上
        float mScrollY;
        // 名字栏宽度: 事件名很长, 固定宽度必然截断, 所以做成可拖动 + 双击自适应
        float mNameColW = 480.0f;
        bool mColDragging;
        // 每行行高, 画布/命中/滚动三处要算同一个值
        const float MinRowH = 15.0f;
        const float MaxRowH = 34.0f;
        // 折叠三角固定在行首一列, 不随层级移动; 这个名字槽位要够放下三角 + 间隙
        const float ArrowSize = 16.0f;
        // 每层缩进步长: 三角不再承担层级提示, 层级全靠文字位置表达, 所以比原 10px 略大
        const float IndentStep = 14.0f;

        // 一个线程一个页签: 三条泳道堆在一个画布里时每条只剩几十像素,
        // 7 层嵌套压进 60px 根本看不出层次。一次只画一条, 它就能占满整个画布
        int mActiveLaneIndex = 0;
        // 帧切换后泳道顺序会变, 按名字恢复上次看的线程
        string mActiveLaneName;

        // 视口(帧内相对微秒)
        double mViewStartUS;
        double mViewEndUS;

        const float RowPadding = 2.0f;
        /// <summary>底部时间刻度尺占的高度</summary>
        const float RulerHeight = 20.0f;
        // 画布底下必须抠出这两行的位置: canvas 占满剩余空间的话, 详情行会被挤到窗口外
        // (详情两行 + 分隔线, 而且 Zoom 按钮会把行高撑到 ~26px)
        const float DetailHeight = 64.0f;
        const double MinViewUS = 4.0;

        static readonly uint[] LaneEventColors =
        {
            0xFF4C7A34, // 绿
            0xFF2E6E9E, // 蓝
            0xFF8E5AA8, // 紫
            0xFF3E8B8B, // 青
            0xFF2F5FA8, // 靛
            0xFF6E7C3E, // 橄榄
            0xFF8B5A3E, // 棕
            0xFF4A4A8B, // 藏青
        };
        static readonly uint ColLaneBg = 0xFF1E1E1E;
        /// <summary>左侧名字栏底色</summary>
        static readonly uint ColNameBg = 0xFF181818;
        static readonly uint ColBorder = 0xFF3A3A3A;
        static readonly uint ColText = 0xFFC8C8C8;
        static readonly uint ColTextHi = 0xFFFFFFFF;
        static readonly uint ColTextDim = 0xFF808080;
        static readonly uint ColSelect = 0xFFFFFFFF;
        #endregion

        #region Frame selection
        /// <summary>由 TracerEditor 帧表格点选时调用</summary>
        public void SelectFrame(TtFrame frame)
        {
            mFrame = frame;
            mBuiltFrameIndex = int.MinValue;   // 强制重建
            mSelectedLane = -1;
            mSelectedEvent = -1;
            mHoverLane = -1;
            mHoverEvent = -1;
            mScrollY = 0;
        }

        public bool HasSelection => mFrame != null;
        /// <summary>当前选中的帧, 帧表格用它判断高亮行</summary>
        public TtFrame SelectedFrame => mFrame;

        void BuildLanes()
        {
            int key = mFrame == null ? int.MinValue : mFrame.FrameIndex;
            if (key == mBuiltFrameIndex && mLanes.Count > 0)
                return;
            mBuiltFrameIndex = key;

            mLanes.Clear();
            mSelectedLane = -1;
            mSelectedEvent = -1;
            mFrameSpanUS = 1;
            if (mFrame == null)
                return;

            // 事件 StartUS 相对 capture 起点, 换成帧内时间要减掉帧起点
            mFrameOriginUS = mFrame.StartTimeMicroSecond - mFrame.TraceBaseUS;
            long maxEnd = 0;

            foreach (var ch in mFrame.Channels)
            {
                if (ch.Channel != ETraceChannel.Cpu && ch.Channel != ETraceChannel.Gpu)
                    continue;
                foreach (var act in ch.Actions)
                {
                    var stream = act as TtEventStreamAction;
                    if (stream == null)
                        continue;
                    for (int i = 0; i < stream.Streams.Count; i++)
                    {
                        var data = stream.Streams[i];
                        if (data.Count == 0)
                            continue;
                        var lane = new TtLane
                        {
                            Name = data.Name,
                            Channel = data.Channel,
                            Events = data.Events,
                            Count = data.Count,
                            Depth = new byte[data.Count],
                            Collapsed = new bool[data.Count],
                            HasChildren = new bool[data.Count],
                        };
                        // 先定“哪些事件占行”, 再算缩进: 父可能被作废或零时长(不占行),
                        // 子不能为它白留一层, 否则画面上会凭空多出没父节点的缩进
                        var occupies = new bool[data.Count];
                        for (int k = 0; k < data.Count; k++)
                        {
                            ref var ev = ref data.Events[k];
                            occupies[k] = ev.NameId >= 0 && ev.DurUS > 0;
                        }
                        for (int k = 0; k < data.Count; k++)
                        {
                            if (occupies[k] == false)
                                continue;
                            ref var ev = ref data.Events[k];
                            lane.VisibleCount++;
                            // 只有真占到行的父才给三角: 收起来的必须是看得见的子树
                            int pid = ev.ParentId;
                            if (pid >= 0 && pid < data.Count && occupies[pid])
                                lane.HasChildren[pid] = true;
                            int depth = 0;
                            int p = ev.ParentId;
                            // 父一定先于子写入, 顺着链一路回退即可; 上限防止脏数据死循环
                            while (p >= 0 && p < k && depth < 64)
                            {
                                // 不占行的父不在缩进里计数, 子往上提一级
                                if (occupies[p])
                                    depth++;
                                p = data.Events[p].ParentId;
                            }
                            lane.Depth[k] = (byte)depth;
                            if (depth > lane.MaxDepth)
                                lane.MaxDepth = depth;
                            long end = (ev.StartUS - mFrameOriginUS) + Math.Max(0, ev.DurUS);
                            if (end > maxEnd)
                                maxEnd = end;
                        }
                        // 一行一个事件: 按开始时间排, 同时开始的父先于子(父该更长)
                        var order = new List<int>(lane.VisibleCount);
                        for (int k = 0; k < data.Count; k++)
                        {
                            ref var ev = ref data.Events[k];
                            if (ev.NameId < 0 || ev.DurUS <= 0)
                                continue;
                            order.Add(k);
                        }
                        order.Sort((a, b) =>
                        {
                            int c = data.Events[a].StartUS.CompareTo(data.Events[b].StartUS);
                            if (c != 0)
                                return c;
                            return data.Events[b].DurUS.CompareTo(data.Events[a].DurUS);
                        });
                        lane.Order = order.ToArray();
                        // 一条都画不出来就不占一行空白: 泳道数要和画面上的行数一致
                        if (lane.VisibleCount == 0)
                            continue;
                        mLanes.Add(lane);
                    }
                }
            }

            mFrameSpanUS = Math.Max(1, maxEnd);
            ApplyFit();
        }

        /// <summary>这一行是否要显示: 深度过滤 + 祖先折叠后行数会变, 名字栏和横条必须数同一边</summary>
        bool RowVisible(TtLane lane, int evIdx)
        {
            if (mDepthLimit > 0 && lane.Depth[evIdx] >= mDepthLimit)
                return false;
            return HasCollapsedAncestor(lane, evIdx) == false;
        }

        /// <summary>沿父链看有没有被收起的祖先; 全展开时直接短路, 不吃链上往返</summary>
        bool HasCollapsedAncestor(TtLane lane, int evIdx)
        {
            if (lane.CollapsedCount == 0)
                return false;
            int p = lane.Events[evIdx].ParentId;
            int guard = 0;
            while (p >= 0 && p < lane.Count && guard++ < 64)
            {
                if (lane.Collapsed[p])
                    return true;
                p = lane.Events[p].ParentId;
            }
            return false;
        }

        void SetCollapsed(TtLane lane, int k, bool collapsed)
        {
            if (lane.HasChildren[k] == false || lane.Collapsed[k] == collapsed)
                return;
            lane.Collapsed[k] = collapsed;
            lane.CollapsedCount += collapsed ? 1 : -1;
        }

        void ToggleCollapse(TtLane lane, int k)
        {
            SetCollapsed(lane, k, lane.Collapsed[k] == false);
            ClearHiddenSelection();
        }

        /// <summary>折叠后选中行可能已经看不见了, 一起清掉免得详情栏显示一条画面上没有的事件</summary>
        void ClearHiddenSelection()
        {
            if (mSelectedLane < 0 || mSelectedLane >= mLanes.Count || mSelectedEvent < 0)
                return;
            if (HasCollapsedAncestor(mLanes[mSelectedLane], mSelectedEvent))
            {
                mSelectedLane = -1;
                mSelectedEvent = -1;
            }
        }

        /// <summary>收起: 只折根节点, 让画面剩主线几行(折中间层会连兄弟一起藏, 分不清收没收)</summary>
        void CollapseAllRoots()
        {
            if (mActiveLaneIndex >= mLanes.Count)
                return;
            var lane = mLanes[mActiveLaneIndex];
            for (int k = 0; k < lane.Count; k++)
            {
                if (lane.Depth[k] == 0)
                    SetCollapsed(lane, k, true);
            }
            ClearHiddenSelection();
        }

        void ExpandAll()
        {
            for (int i = 0; i < mLanes.Count; i++)
            {
                var lane = mLanes[i];
                for (int k = 0; k < lane.Count; k++)
                    SetCollapsed(lane, k, false);
            }
        }

        /// <summary>深度过滤/折叠后实际要画的行数</summary>
        int RowCount(TtLane lane)
        {
            if (lane.Order == null)
                return 0;
            if (mDepthLimit <= 0 && lane.CollapsedCount == 0)
                return lane.Order.Length;
            int n = 0;
            for (int i = 0; i < lane.Order.Length; i++)
            {
                if (RowVisible(lane, lane.Order[i]))
                    n++;
            }
            return n;
        }

        /// <summary>每行高度: 行少时撑到好看, 行多时压到 MinRowH 再靠滚动
        /// (而不是像以前那样固定 9px 把字叠成一片)</summary>
        float ComputeRowH(int rows, float availH)
        {
            if (rows <= 0)
                return MinRowH;
            float h = (availH - RowPadding * 2) / rows;
            h *= mLayerScale;
            if (h > MaxRowH)
                h = MaxRowH;
            if (h < MinRowH)
                h = MinRowH;
            return h;
        }

        /// <summary>泳道顺序逐帧可能变, 页签选中项按线程名保持</summary>
        void EnsureActiveLane()
        {
            if (mLanes.Count == 0)
            {
                mActiveLaneIndex = 0;
                return;
            }
            if (string.IsNullOrEmpty(mActiveLaneName) == false)
            {
                for (int i = 0; i < mLanes.Count; i++)
                {
                    if (mLanes[i].Name == mActiveLaneName)
                    {
                        mActiveLaneIndex = i;
                        return;
                    }
                }
            }
            if (mActiveLaneIndex >= mLanes.Count)
                mActiveLaneIndex = 0;
            mActiveLaneName = mLanes[mActiveLaneIndex].Name;
        }

        void ApplyFit()
        {
            mViewStartUS = 0;
            mViewEndUS = mFrameSpanUS;
        }
        #endregion

        #region Draw
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;
            ImGuiAPI.SetNextWindowSize(new Vector2(900, 520), DockCond);
            var result = EngineNS.EGui.UIProxy.DockProxy.BeginMainForm("Timeline", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                DockId = ImGuiAPI.GetWindowDockID();
                BuildLanes();
                DrawToolbar();
                DrawCanvas();
                DrawDetail();
            }
            EngineNS.EGui.UIProxy.DockProxy.EndMainForm(result);
        }

        void DrawToolbar()
        {
            if (mFrame == null)
            {
                ImGuiAPI.Text("在 TracerEditor 里点选一帧查看事件流");
                return;
            }
            ImGuiAPI.Text($"Frame {mFrame.FrameIndex}  CPU {mFrame.DurationMicroSecond / 1000.0f:F3} ms  事件 {CountEvents()} / {mLanes.Count} 泳道");
            ImGuiAPI.SameLine(0, 12);
            if (ImGuiAPI.Button("Fit"))
            {
                ApplyFit();
            }
            // 树折叠: 收起只留根节点看主线, 右边横条同步少行
            ImGuiAPI.SameLine(0, 12);
            if (ImGuiAPI.Button("收起树"))
                CollapseAllRoots();
            ImGuiAPI.SameLine(0, 4);
            if (ImGuiAPI.Button("展开树"))
                ExpandAll();
            // 深度过滤: 一个事件一行时行数就是叶子数, 想看主线就只留前几层
            ImGuiAPI.SameLine(0, 12);
            ImGuiAPI.Text("深度");
            ImGuiAPI.SameLine(0, 4);
            if (ImGuiAPI.Button(mDepthLimit == 1 ? "[1]" : "1"))
                mDepthLimit = 1;
            ImGuiAPI.SameLine(0, 4);
            if (ImGuiAPI.Button(mDepthLimit == 4 ? "[4]" : "4"))
                mDepthLimit = 4;
            ImGuiAPI.SameLine(0, 4);
            if (ImGuiAPI.Button(mDepthLimit == 0 ? "[all]" : "all"))
                mDepthLimit = 0;
            ImGuiAPI.SameLine(0, 12);
            ImGuiAPI.TextColored(new Vector4(0.6f, 0.6f, 0.6f, 1.0f), "滚轮缩放 / Shift+滚轮调行高 / 右键拖动 / 拖分割线调名字栏宽");

            // 第二行: 一个线程一个页签，选中的那条独占整个画布
            EnsureActiveLane();
            ImGuiAPI.Text("线程");
            for (int i = 0; i < mLanes.Count; i++)
            {
                ImGuiAPI.SameLine(0, 4);
                bool active = i == mActiveLaneIndex;
                var label = (active ? "● " : "○ ") + mLanes[i].Name + " " + mLanes[i].VisibleCount;
                if (ImGuiAPI.Button(label))
                {
                    mActiveLaneIndex = i;
                    mActiveLaneName = mLanes[i].Name;
                    mSelectedLane = -1;
                    mSelectedEvent = -1;
                    mScrollY = 0;
                }
            }
        }

        int CountEvents()
        {
            int n = 0;
            for (int i = 0; i < mLanes.Count; i++)
                n += mLanes[i].VisibleCount;
            return n;
        }

        void DrawCanvas()
        {
            var avail = ImGuiAPI.GetContentRegionAvail();
            // dock 布局下 GetContentRegionAvail 会返回整个 dock 节点的高度(实测 1610 而窗口只有 ~700),
            // 不夹住的话画布会把后面的详情行整块顶到窗口外 —— 表现就是父链那一行永远看不见
            var crMin = ImGuiAPI.GetWindowContentRegionMin();
            var crMax = ImGuiAPI.GetWindowContentRegionMax();
            float contentH = crMax.Y - crMin.Y;
            if (contentH > 0 && avail.Y > contentH)
                avail.Y = contentH;
            avail.Y -= DetailHeight;
            if (avail.X < 32 || avail.Y < 32)
                return;

            var origin = ImGuiAPI.GetCursorScreenPos();
            var rectMax = new Vector2(origin.X + avail.X, origin.Y + avail.Y);
            var cmdlst = ImGuiAPI.GetWindowDrawList();
            cmdlst.AddRectFilled(in origin, in rectMax, 0xFF101010, 0, ImDrawFlags_.ImDrawFlags_None);

            // 先占交互区, 之后画的都在它上面
            ImGuiAPI.InvisibleButton("##TimelineCanvas", in avail,
                ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft |
                ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonRight |
                ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonMiddle);
            bool hovered = ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);

            var mouse = ImGuiAPI.GetMousePos();
            // 左侧名字栏: 一个事件一行地对齐到右边横条, 横条里不再写字
            float x0 = origin.X + mNameColW;
            float x1 = rectMax.X - 8;
            if (x1 - x0 < 8)
                x1 = x0 + 8;

            HandleInteraction(hovered, in origin, in rectMax, in mouse, x0, x1);

            double span = Math.Max(MinViewUS, mViewEndUS - mViewStartUS);
            double scale = (x1 - x0) / span;

            DrawLanes(cmdlst, in origin, in rectMax, in mouse, x0, x1, scale);
            DrawTimeRuler(cmdlst, x0, x1, rectMax.Y, scale);
            // 名字栏分割线: 拖它能改宽度, 双击自适应最长名字
            uint splitCol = mColDragging ? ColSelect : ColBorder;
            cmdlst.AddLine(new Vector2(x0, origin.Y), new Vector2(x0, rectMax.Y), splitCol, mColDragging ? 2.0f : 1.0f);
        }

        void HandleInteraction(bool hovered, in Vector2 origin, in Vector2 rectMax, in Vector2 mouse, float x0, float x1)
        {
            if (mFrame == null)
                return;

            // 名字栏宽度: 拖分割线改, 双击按最长名字自适应
            bool onSplit = hovered && Math.Abs(mouse.X - x0) <= 4.0f;
            if (onSplit)
                ImGuiAPI.SetMouseCursor(ImGuiMouseCursor_.ImGuiMouseCursor_ResizeEW);
            if (onSplit && ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false))
            {
                if (ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left) &&
                    mActiveLaneIndex < mLanes.Count)
                    mNameColW = AutoFitNameCol(mLanes[mActiveLaneIndex]);
                mColDragging = true;
            }
            if (mColDragging)
            {
                mNameColW = mouse.X - origin.X;
                float maxW = (rectMax.X - origin.X) * 0.85f;
                if (mNameColW < 120.0f)
                    mNameColW = 120.0f;
                if (mNameColW > maxW)
                    mNameColW = maxW;
                if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) == false)
                    mColDragging = false;
                // 拖动中和刚松手那一帧都不要顺便把横条选上
                return;
            }

            // 滚轮: 普通滚轮管纵向滚动(行数压不进画布时), 否则管时间缩放; Shift 管行高
            if (hovered && mLanes.Count > 0)
            {
                var io = ImGuiAPI.GetIO();
                float wheel = io.MouseWheel;
                if (wheel != 0.0f)
                {
                    var lane = mLanes[mActiveLaneIndex];
                    float viewH = (rectMax.Y - RulerHeight) - (origin.Y + 4);
                    int rows = RowCount(lane);
                    float rowH = ComputeRowH(rows, viewH);
                    if (io.KeyShift)
                    {
                        // 行高: 每行像素不够分时把行拉高
                        mLayerScale *= wheel > 0 ? 1.15f : 0.87f;
                        if (mLayerScale < 0.6f)
                            mLayerScale = 0.6f;
                        if (mLayerScale > 6.0f)
                            mLayerScale = 6.0f;
                    }
                    else if (rows * rowH > viewH - RowPadding * 2)
                    {
                        // 行数本来就装不下, 滚轮优先纵向滚动, 否则缩放后一样看不见
                        mScrollY -= wheel * rowH * 2.0f;
                        ClampScroll(lane, viewH, rowH);
                    }
                    else if (mouse.X >= x0)
                    {
                        double span = Math.Max(MinViewUS, mViewEndUS - mViewStartUS);
                        double anchorT = mViewStartUS + (mouse.X - x0) / Math.Max(1.0, x1 - x0) * span;
                        double factor = wheel > 0 ? 0.8 : 1.25;
                        double newSpan = Math.Max(MinViewUS, span * factor);
                        double maxSpan = Math.Max(MinViewUS, mFrameSpanUS * 4.0);
                        if (newSpan > maxSpan)
                            newSpan = maxSpan;
                        double ratio = (anchorT - mViewStartUS) / span;
                        mViewStartUS = anchorT - newSpan * ratio;
                        mViewEndUS = mViewStartUS + newSpan;
                        if (mViewStartUS < -mFrameSpanUS)
                            mViewStartUS = -mFrameSpanUS;
                        if (mViewEndUS > mFrameSpanUS * 1.5)
                            mViewEndUS = mFrameSpanUS * 1.5;
                    }
                }
            }

            // 平移: 右键或中键拖动
            bool dragging = ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right) ||
                            ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Middle);
            if (dragging && hovered == false && mDragging == false)
                return;
            if (dragging)
            {
                if (mDragging)
                {
                    double span = Math.Max(MinViewUS, mViewEndUS - mViewStartUS);
                    double scale = (x1 - x0) / span;
                    if (scale > 0)
                    {
                        double deltaT = (mLastMouse.X - mouse.X) / scale;
                        mViewStartUS += deltaT;
                        mViewEndUS += deltaT;
                    }
                }
                mDragging = true;
                mLastMouse = mouse;
            }
            else
            {
                mDragging = false;
            }

            // 命中: 每帧算一次悬停, 点击直接复用。行号和名字栏完全对齐,
            // 所以在左边名字上点也行 —— 横条窄得只剩 1px 时这是唯一能稳选的入口
            mHoverLane = -1;
            mHoverEvent = -1;
            mHoverOnArrow = false;
            if (hovered && mActiveLaneIndex < mLanes.Count)
            {
                var lane = mLanes[mActiveLaneIndex];
                float laneTop = origin.Y + 4;
                float laneBottom = rectMax.Y - RulerHeight;
                float viewH = laneBottom - laneTop;
                int rows = RowCount(lane);
                float rowH = ComputeRowH(rows, viewH);
                double spanNow = Math.Max(MinViewUS, mViewEndUS - mViewStartUS);
                double scaleNow = (x1 - x0) / spanNow;
                float yRel = mouse.Y - laneTop - RowPadding + mScrollY;
                int row = yRel < 0 ? -1 : (int)(yRel / rowH);
                if (row >= 0 && row < rows && scaleNow > 0)
                {
                    int k = EventOfRow(lane, row);
                    if (k >= 0)
                    {
                        ref var ev = ref lane.Events[k];
                        bool hit = mouse.X < x0;   // 名字栏: 整行都能点
                        // 行首三角独立判定: 点在它上面是折叠, 不是选中
                        if (hit && lane.HasChildren[k])
                        {
                            float arrowX = origin.X + 3;
                            mHoverOnArrow = mouse.X >= arrowX - 1 && mouse.X <= arrowX + ArrowSize;
                        }
                        if (hit == false)
                        {
                            long startUS = ev.StartUS - mFrameOriginUS;
                            float ex0 = x0 + (float)((startUS - mViewStartUS) * scaleNow);
                            float ex1 = x0 + (float)((startUS + ev.DurUS - mViewStartUS) * scaleNow);
                            if (ex1 - ex0 < 3.0f)
                                ex1 = ex0 + 3.0f;
                            hit = mouse.X >= ex0 && mouse.X <= ex1;
                        }
                        if (hit)
                        {
                            mHoverLane = mActiveLaneIndex;
                            mHoverEvent = k;
                        }
                    }
                }
            }

            // 名字栏放不下时全名只能靠 tooltip
            if (hovered && mHoverEvent >= 0 && mHoverLane >= 0 && mHoverLane < mLanes.Count)
            {
                var lane = mLanes[mHoverLane];
                ref var ev = ref lane.Events[mHoverEvent];
                var foldHint = lane.HasChildren[mHoverEvent] ? "\n双击/点三角折叠子树" : "";
                ImGuiAPI.SetTooltip($"{TtTraceNameTable.GetName(ev.NameId)}\n深度 {lane.Depth[mHoverEvent]}  Start {ev.StartUS - mFrameOriginUS}us  Dur {ev.DurUS}us{foldHint}");
            }

            if (hovered && ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false))
            {
                bool canFold = mHoverEvent >= 0 && mActiveLaneIndex < mLanes.Count &&
                    mLanes[mActiveLaneIndex].HasChildren[mHoverEvent];
                bool dbl = ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
                if (canFold && (mHoverOnArrow || dbl))
                {
                    ToggleCollapse(mLanes[mActiveLaneIndex], mHoverEvent);
                    mHoverOnArrow = false;
                }
                else
                {
                    mSelectedLane = mHoverLane;
                    mSelectedEvent = mHoverEvent;
                }
            }
        }

        Vector2 mLastMouse;
        bool mDragging = false;

        /// <summary>滚动量的合法区间: 内容不超过画布时恒为 0</summary>
        void ClampScroll(TtLane lane, float viewH, float rowH)
        {
            float maxScroll = RowCount(lane) * rowH - (viewH - RowPadding * 2);
            if (maxScroll < 0)
                maxScroll = 0;
            if (mScrollY > maxScroll)
                mScrollY = maxScroll;
            if (mScrollY < 0)
                mScrollY = 0;
        }

        /// <summary>双击分割线: 按当前泳道最长的名字定宽(等宽近似 7px/字)</summary>
        float AutoFitNameCol(TtLane lane)
        {
            int maxLen = 0;
            if (lane.Order != null)
            {
                for (int i = 0; i < lane.Order.Length; i++)
                {
                    int k = lane.Order[i];
                    if (RowVisible(lane, k) == false)
                        continue;
                    var name = TtTraceNameTable.GetName(lane.Events[k].NameId);
                    if (name != null && name.Length > maxLen)
                        maxLen = name.Length;
                }
            }
            float w = 10.0f + ArrowSize + Math.Min(lane.MaxDepth, 10) * IndentStep + maxLen * 7.5f;
            if (w < 180.0f)
                w = 180.0f;
            if (w > 900.0f)
                w = 900.0f;
            return w;
        }

        /// <summary>第 row 个可见行对应的事件下标; -1 表示越界</summary>
        int EventOfRow(TtLane lane, int row)
        {
            var order = lane.Order;
            if (order == null || row < 0)
                return -1;
            int n = 0;
            for (int i = 0; i < order.Length; i++)
            {
                int k = order[i];
                if (RowVisible(lane, k) == false)
                    continue;
                if (n == row)
                    return k;
                n++;
            }
            return -1;
        }

        /// <summary>名字栏放不下就截断加省略号: 全名靠悬停 tooltip 看</summary>
        static string FitText(string text, float maxW)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            if (maxW <= 0)
                return "";
            // 等宽近似: 默认字号下英文字符约 7.5px 宽
            int maxChars = (int)(maxW / 7.5f);
            if (maxChars >= text.Length)
                return text;
            if (maxChars <= 1)
                return "…";
            return text.Substring(0, maxChars - 1) + "…";
        }

        void DrawLanes(ImDrawList cmdlst, in Vector2 origin, in Vector2 rectMax, in Vector2 mouse,
            float x0, float x1, double scale)
        {
            if (mLanes.Count == 0)
            {
                var hintPos = new Vector2(origin.X + 10, origin.Y + 8);
                cmdlst.AddText(in hintPos, ColTextDim, "该帧没有事件流数据(未开启 Capture ?)", null);
                return;
            }

            var lane = mLanes[mActiveLaneIndex];
            float laneTop = origin.Y + 4;
            float laneBottom = rectMax.Y - RulerHeight;
            if (laneBottom <= laneTop)
                return;
            float viewH = laneBottom - laneTop;
            int rows = RowCount(lane);
            float rowH = ComputeRowH(rows, viewH);
            ClampScroll(lane, viewH, rowH);

            cmdlst.AddRectFilled(new Vector2(origin.X, laneTop), new Vector2(rectMax.X, laneBottom),
                ColLaneBg, 0, ImDrawFlags_.ImDrawFlags_None);
            // 名字栏底色: 和时间轴分开, 一眼能看出左边是标签不是事件
            cmdlst.AddRectFilled(new Vector2(origin.X, laneTop), new Vector2(origin.X + mNameColW, laneBottom),
                ColNameBg, 0, ImDrawFlags_.ImDrawFlags_None);

            // 第一遍: 左边名字。一个事件一行, 深度做缩进, 和右边横条共用同一个 rowH / 滚动量
            cmdlst.PushClipRect(new Vector2(origin.X + 2, laneTop), new Vector2(origin.X + mNameColW - 6, laneBottom), true);
            int row = 0;
            for (int i = 0; i < lane.Order.Length; i++)
            {
                int k = lane.Order[i];
                if (RowVisible(lane, k) == false)
                    continue;
                ref var ev = ref lane.Events[k];
                float ry0 = laneTop + RowPadding + row * rowH - mScrollY;
                float ry1 = ry0 + rowH;
                row++;
                if (ry1 < laneTop || ry0 > laneBottom)
                    continue;

                bool selected = (k == mSelectedEvent && mActiveLaneIndex == mSelectedLane);
                bool hoveredEv = selected == false && k == mHoverEvent && mActiveLaneIndex == mHoverLane;
                // 行分隔线: 左边名字和右边横条靠它对齐
                cmdlst.AddLine(new Vector2(origin.X, ry1), new Vector2(rectMax.X, ry1), 0xFF202020, 1.0f);

                float indent = Math.Min((int)lane.Depth[k], 10) * IndentStep;
                // 三角固定一列: 跟着层级右移的话会和文字缩进叠成两层视觉缩进
                float arrowX = origin.X + 3;
                float ay = ry0 + rowH * 0.5f;
                if (lane.HasChildren[k])
                {
                    if (lane.Collapsed[k])
                    {
                        // 收起: 三角朝右
                        cmdlst.AddTriangleFilled(new Vector2(arrowX + 3, ay - 5), new Vector2(arrowX + 3, ay + 5),
                            new Vector2(arrowX + 12, ay), ColText);
                    }
                    else
                    {
                        // 展开: 三角朝下
                        cmdlst.AddTriangleFilled(new Vector2(arrowX + 2, ay - 4), new Vector2(arrowX + 13, ay - 4),
                            new Vector2(arrowX + 7.5f, ay + 5), ColText);
                    }
                }
                else
                {
                    // 叶子行: 缩进还在(层级是真实的), 但没子事件就没有三角; 给个淡点占位,
                    // 否则看上去像“凭空多缩进了一级”
                    cmdlst.AddCircleFilled(new Vector2(arrowX + 8, ay), 1.5f, 0xFF4A4A4A, 8);
                }
                float nameX = origin.X + 3 + ArrowSize + indent;
                var name = FitText(TtTraceNameTable.GetName(ev.NameId), mNameColW - (nameX - origin.X) - 10.0f);
                var namePos = new Vector2(nameX, ry0 + (rowH - 15.0f) * 0.5f);
                cmdlst.AddText(in namePos, selected ? 0xFFFFFFFF : (hoveredEv ? ColTextHi : ColText), name, null);
            }
            cmdlst.PopClipRect();

            // 第二遍: 右边横条。X 是真实时间, Y 是行号 —— 横条里不再写字
            cmdlst.PushClipRect(new Vector2(x0, laneTop), new Vector2(rectMax.X, laneBottom), true);
            row = 0;
            for (int i = 0; i < lane.Order.Length; i++)
            {
                int k = lane.Order[i];
                if (RowVisible(lane, k) == false)
                    continue;
                ref var ev = ref lane.Events[k];
                float ry0 = laneTop + RowPadding + row * rowH - mScrollY;
                float ry1 = ry0 + rowH;
                row++;
                if (ry1 < laneTop || ry0 > laneBottom)
                    continue;

                long startUS = ev.StartUS - mFrameOriginUS;
                float ex0 = x0 + (float)((startUS - mViewStartUS) * scale);
                float ex1 = x0 + (float)((startUS + ev.DurUS - mViewStartUS) * scale);
                if (ex1 < x0 || ex0 > rectMax.X)
                    continue;
                // 真干活的那些事件在整帧视图下只有零点几像素, 至少给它 2px 不然根本看不见
                if (ex1 - ex0 < 2.0f)
                    ex1 = ex0 + 2.0f;

                bool selected = (k == mSelectedEvent && mActiveLaneIndex == mSelectedLane);
                bool hoveredEv = selected == false && k == mHoverEvent && mActiveLaneIndex == mHoverLane;
                uint color = selected ? ColSelect : LaneEventColors[Math.Abs(ev.NameId) % LaneEventColors.Length];
                var barMin = new Vector2(ex0, ry0 + 2.0f);
                var barMax = new Vector2(ex1, ry1 - 2.0f);
                cmdlst.AddRectFilled(in barMin, in barMax, color, 1.0f, ImDrawFlags_.ImDrawFlags_None);
                if (hoveredEv)
                    cmdlst.AddRect(in barMin, in barMax, ColSelect, 1.0f, ImDrawFlags_.ImDrawFlags_None, 1.0f);
            }
            cmdlst.PopClipRect();
        }

        void DrawTimeRuler(ImDrawList cmdlst, float x0, float x1, float rectBottom, double scale)
        {
            double span = Math.Max(MinViewUS, mViewEndUS - mViewStartUS);
            // 每 80 像素一个刻度, 再取整到 1/2/5 x 10^n
            double rawStep = span * 80.0 / Math.Max(1.0, x1 - x0);
            double step = NiceStep(rawStep);
            if (step <= 0)
                return;

            double t = Math.Ceiling(mViewStartUS / step) * step;
            for (; t <= mViewEndUS; t += step)
            {
                float x = x0 + (float)((t - mViewStartUS) * scale);
                cmdlst.AddLine(new Vector2(x, rectBottom - 14), new Vector2(x, rectBottom - 6), ColBorder, 1.0f);
                var pos = new Vector2(x + 2, rectBottom - 14);
                cmdlst.AddText(in pos, ColTextDim, $"{t:F0}us", null);
            }
        }

        static double NiceStep(double raw)
        {
            if (raw <= 0)
                return 0;
            double exp = Math.Floor(Math.Log10(raw));
            double baseVal = Math.Pow(10, exp);
            double n = raw / baseVal;
            if (n <= 1.0)
                return baseVal;
            if (n <= 2.0)
                return 2 * baseVal;
            if (n <= 5.0)
                return 5 * baseVal;
            return 10 * baseVal;
        }

        void DrawDetail()
        {
            ImGuiAPI.Separator();
            // 选中的优先, 没选中就显示鼠标悬停的那条 —— 不点也能先看清是什么
            bool pinned = mSelectedLane >= 0;
            int laneIdx = pinned ? mSelectedLane : mHoverLane;
            int evIdx = pinned ? mSelectedEvent : mHoverEvent;
            if (laneIdx < 0 || laneIdx >= mLanes.Count ||
                evIdx < 0 || evIdx >= mLanes[laneIdx].Count)
            {
                ImGuiAPI.Text("未选中事件   (悬停预览, 左键固定)");
                return;
            }

            var lane = mLanes[laneIdx];
            ref var ev = ref lane.Events[evIdx];
            var prefix = pinned ? "" : "[悬停] ";
            ImGuiAPI.Text($"{prefix}{TtTraceNameTable.GetName(ev.NameId)}   Start {ev.StartUS - mFrameOriginUS}us   Dur {ev.DurUS}us   Lane {lane.Name}");
            ImGuiAPI.SameLine(0, 12);
            // 整帧视图下 Tick.Sleep 这类等待能占 85% 宽度, 真正干活的几十个事件全挤在左边几像素里,
            // 不缩放根本分不开 —— 所以给一个一键缩到该事件的入口
            if (ImGuiAPI.Button("Zoom"))
                ZoomToEvent(lane, evIdx);
            // 父链单独占一行: 跟上面同一行的话稍深一点的嵌套就被裁掉了
            ImGuiAPI.TextColored(new Vector4(0.6f, 0.7f, 0.9f, 1.0f), "父链: " + BuildParentChain(lane, evIdx));
        }

        void ZoomToEvent(TtLane lane, int k)
        {
            ref var ev = ref lane.Events[k];
            long start = ev.StartUS - mFrameOriginUS;
            long dur = Math.Max(1, ev.DurUS);
            double pad = Math.Max(dur * 0.05, 1.0);
            mViewStartUS = start - pad;
            mViewEndUS = start + dur + pad;
        }

        string BuildParentChain(TtLane lane, int index)
        {
            var chain = new List<string>();
            int p = lane.Events[index].ParentId;
            int guard = 0;
            while (p >= 0 && p < lane.Count && guard++ < 64)
            {
                chain.Add(TtTraceNameTable.GetName(lane.Events[p].NameId));
                p = lane.Events[p].ParentId;
            }
            if (chain.Count == 0)
                return "(root)";
            chain.Reverse();
            return string.Join(" > ", chain);
        }
        #endregion
    }
}
