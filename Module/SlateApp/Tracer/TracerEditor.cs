using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EngineNS;
using EngineNS.EGui;
using EngineNS.EGui.Controls;

namespace Tracer
{
    public class TtTracerEditor : IRootForm
    {
        public TtTracerEditor()
        {
            TtEngine.RootFormManager.RegRootForm(this);

            Visible = true;
        }
        ~TtTracerEditor()
        {
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
        public bool Visible { get; set; }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            var size = new Vector2(800, 600);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EngineNS.EGui.UIProxy.DockProxy.BeginMainForm("TracerEditor", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                DockId = ImGuiAPI.GetWindowDockID();
                DrawFrames();
            }
            EngineNS.EGui.UIProxy.DockProxy.EndMainForm(result);
        }

        #region Frame table
        sealed class TtFrameStat
        {
            public int EventCount;
            public long GpuUs;
            public int LaneCount;
        }
        // 每帧的事件统计算一次就缓存: 一帧可能几万条事件, 每帧重算会拽垮 UI。
        // 键用弱引用: 帧被环形缓存淘汰后统计也跟着回收, 不会把整帧数据钉在堆上
        readonly System.Runtime.CompilerServices.ConditionalWeakTable<
            EngineNS.Profiler.Trace.TtFrame, TtFrameStat> mStats =
            new System.Runtime.CompilerServices.ConditionalWeakTable<
                EngineNS.Profiler.Trace.TtFrame, TtFrameStat>();

        TtFrameStat GetStat(EngineNS.Profiler.Trace.TtFrame frame)
        {
            return mStats.GetValue(frame, ComputeStat);
        }

        static TtFrameStat ComputeStat(EngineNS.Profiler.Trace.TtFrame frame)
        {
            var stat = new TtFrameStat();
            foreach (var ch in frame.Channels)
            {
                foreach (var act in ch.Actions)
                {
                    var stream = act as EngineNS.Profiler.Trace.TtEventStreamAction;
                    if (stream == null)
                        continue;
                    for (int i = 0; i < stream.Streams.Count; i++)
                    {
                        var data = stream.Streams[i];
                        // 只数画得出来的: 未 End 被封帧作废的事件(NameId<0) 在 Timeline 上并不存在,
                        // 两处口径不一致会让人以为丢了数据
                        int drawable = 0;
                        for (int k = 0; k < data.Count; k++)
                        {
                            var ev = data.Events[k];
                            if (ev.NameId < 0 || ev.DurUS <= 0)
                                continue;
                            drawable++;
                            if (data.Channel == EngineNS.Profiler.Trace.ETraceChannel.Gpu)
                                stat.GpuUs += ev.DurUS;
                        }
                        if (drawable == 0)
                            continue;
                        stat.LaneCount++;
                        if (data.Channel != EngineNS.Profiler.Trace.ETraceChannel.Gpu)
                            stat.EventCount += drawable;
                    }
                }
            }
            return stat;
        }

        // 采集抓满自停后, 表头那一大堆行都是之后的快照帧;
        // 不主动滚过去, 用户会以为一条事件都没采到
        EngineNS.Profiler.Trace.TtFrame mScrollToFrame = null;

        static int CountTraceFrames(TtTraceApplication app, int count)
        {
            int n = 0;
            for (int i = 0; i < count; i++)
            {
                if (TtTraceApplication.HasEventStream(app.GetFrame(i)))
                    n++;
            }
            return n;
        }

        // 录制由引擎侧完成(对齐 UE: trace writer 在引擎进程里, 边跑边写文件),
        // Tracer 只负责列出 cache/tracer 下的文件并加载回放
        string mSelectedRecordFile;
        string mRecordStatus;
        string[] mRecordFiles = Array.Empty<string>();

        void RefreshRecordFiles()
        {
            try
            {
                string dir = EngineNS.Profiler.Trace.TtTraceFile.GetRecordDir();
                if (System.IO.Directory.Exists(dir) == false)
                {
                    mRecordFiles = Array.Empty<string>();
                    return;
                }
                var files = System.IO.Directory.GetFiles(dir, "*" + EngineNS.Profiler.Trace.TtTraceFile.Extension);
                Array.Sort(files);
                Array.Reverse(files);   // 最新的排最前面
                mRecordFiles = files;
            }
            catch
            {
                mRecordFiles = Array.Empty<string>();
            }
        }

        void DrawRecordToolbar(TtTraceApplication app)
        {
            ImGuiAPI.TextColored(new Vector4(0.75f, 0.75f, 0.75f, 1.0f), "录制文件:");
            ImGuiAPI.SameLine(0, 8);
            if (ImGuiAPI.Button("加载最新"))
            {
                RefreshRecordFiles();
                if (mRecordFiles.Length == 0)
                {
                    mRecordStatus = "cache/tracer 下没有 .ttrace 文件";
                }
                else
                {
                    mSelectedRecordFile = mRecordFiles[0];
                    ApplyRecordFile(app);
                }
            }
            ImGuiAPI.SameLine(0, 8);
            var preview = string.IsNullOrEmpty(mSelectedRecordFile) ? "选择 .ttrace" : System.IO.Path.GetFileName(mSelectedRecordFile);
            if (ImGuiAPI.BeginCombo("##RecordFile", preview, ImGuiComboFlags_.ImGuiComboFlags_None))
            {
                // 每帧扫目录太浪费, 打开下拉的这一刻刷新一次就够
                RefreshRecordFiles();
                if (mRecordFiles.Length == 0)
                    ImGuiAPI.TextColored(new Vector4(0.7f, 0.7f, 0.4f, 1.0f), "cache/tracer 下没有 .ttrace");
                for (int i = 0; i < mRecordFiles.Length; i++)
                {
                    // 下拉项用完整文件名(带时间戳), 同名的话用户没法区分
                    var name = System.IO.Path.GetFileName(mRecordFiles[i]);
                    if (ImGuiAPI.Selectable(name, mSelectedRecordFile == mRecordFiles[i], ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                    {
                        mSelectedRecordFile = mRecordFiles[i];
                        ApplyRecordFile(app);
                    }
                }
                ImGuiAPI.EndCombo();
            }
            if (string.IsNullOrEmpty(mRecordStatus) == false)
            {
                ImGuiAPI.SameLine(0, 8);
                ImGuiAPI.TextColored(new Vector4(0.6f, 0.8f, 0.6f, 1.0f), mRecordStatus);
            }
        }

        void ApplyRecordFile(TtTraceApplication app)
        {
            if (string.IsNullOrEmpty(mSelectedRecordFile))
                return;
            int n = app.LoadRecording(mSelectedRecordFile);
            if (n > 0)
            {
                // 加载后帧区间跟文件长度完全不同, 可视窗口必须回到全览
                mViewSpan = 0.0f;
                mViewBegin = 0.0f;
                mP99Count = -1;
                mRecordStatus = $"已加载 {System.IO.Path.GetFileName(mSelectedRecordFile)}  ({n} 帧)";
            }
            else
            {
                mRecordStatus = "加载失败: " + app.LastRecordError;
            }
        }

        void DrawFrames()
        {
            var app = TtTraceApplication.Instance;
            if (app == null)
            {
                ImGuiAPI.Text("TtTraceApplication 未初始化");
                return;
            }

            int count = app.FrameCount;
            ImGuiAPI.Text($"Frames {count}");
            ImGuiAPI.SameLine(0, 12);
            int traceFrames = CountTraceFrames(app, count);
            ImGuiAPI.TextColored(new Vector4(0.5f, 0.9f, 0.5f, 1.0f), $"trace {traceFrames}");
            ImGuiAPI.SameLine(0, 12);
            if (ImGuiAPI.Button("JumpToTrace"))
            {
                // 新帧在下标大的一端，从后往前找就是“最近一次采集”
                for (int i = count - 1; i >= 0; i--)
                {
                    var f = app.GetFrame(i);
                    if (TtTraceApplication.HasEventStream(f))
                    {
                        mScrollToFrame = f;
                        app.TimelineVisual?.SelectFrame(f);
                        break;
                    }
                }
            }

            ImGuiAPI.SameLine(0, 12);
            ImGuiAPI.TextColored(new Vector4(0.6f, 0.6f, 0.6f, 1.0f), "点柱子选帧 / 拖拽平移 / 滚轮缩放 / 双击全览");

            // 录制单独一行: 和 Frames/trace/JumpToTrace 挤一行会被挤出窗口, 用户根本看不到按钮
            DrawRecordToolbar(app);
            ImGuiAPI.SameLine(0, 12);
            ImGuiAPI.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1.0f), "录制在引擎侧: 工具 → 开始录制跟踪");

            ImGuiAPI.SameLine(0, 16);
            if (ImGuiAPI.Button("全览"))
            {
                mViewSpan = 0.0f;
                mViewBegin = 0.0f;
            }
            ImGuiAPI.SameLine(0, 16);
            if (ImGuiAPI.Button(mShowTable ? "隐藏表格" : "显示表格"))
                mShowTable = !mShowTable;

            DrawFrameGraph(app, count);
            DrawFrameTable(app, count);
        }

        void DrawFrameTable(TtTraceApplication app, int count)
        {
            if (mShowTable == false || count <= 0)
                return;

            var tblFlags = ImGuiTableFlags_.ImGuiTableFlags_Resizable |
                           ImGuiTableFlags_.ImGuiTableFlags_RowBg |
                           ImGuiTableFlags_.ImGuiTableFlags_ScrollY;
            if (ImGuiAPI.BeginTable("Frames", 5, tblFlags, in Vector2.Zero, 0.0f) == false)
                return;

            ImGuiAPI.TableSetupColumn("Frame", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 90, 0);
            ImGuiAPI.TableSetupColumn("CPU ms", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 90, 0);
            ImGuiAPI.TableSetupColumn("GPU ms", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 90, 0);
            ImGuiAPI.TableSetupColumn("Events", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 120, 0);
            ImGuiAPI.TableSetupColumn("Flags", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthStretch, 0, 0);
            ImGuiAPI.TableHeadersRow();

            // 新帧在上
            for (int i = count - 1; i >= 0; i--)
            {
                var frame = app.GetFrame(i);
                if (frame == null)
                    continue;
                var stat = GetStat(frame);
                bool selected = app.TimelineVisual != null && app.TimelineVisual.SelectedFrame == frame;

                ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_None, 0);
                ImGuiAPI.TableSetColumnIndex(0);
                if (ReferenceEquals(frame, mScrollToFrame))
                {
                    ImGuiAPI.SetScrollHereY(0.5f);
                    mScrollToFrame = null;
                }
                var label = $"#{frame.FrameIndex}";
                if (ImGuiAPI.Selectable(label, selected, ImGuiSelectableFlags_.ImGuiSelectableFlags_SpanAllColumns, in Vector2.Zero))
                {
                    app.TimelineVisual?.SelectFrame(frame);
                }

                ImGuiAPI.TableSetColumnIndex(1);
                ImGuiAPI.Text($"{frame.DurationMicroSecond / 1000.0f:F3}");

                ImGuiAPI.TableSetColumnIndex(2);
                ImGuiAPI.Text($"{stat.GpuUs / 1000.0f:F3}");

                ImGuiAPI.TableSetColumnIndex(3);
                ImGuiAPI.Text($"{stat.EventCount} / {stat.LaneCount} lanes");

                ImGuiAPI.TableSetColumnIndex(4);
                if (stat.EventCount == 0 && stat.GpuUs == 0)
                    ImGuiAPI.TextColored(new Vector4(0.7f, 0.7f, 0.4f, 1.0f), "no-trace");
                else
                    ImGuiAPI.Text("ok");
            }
            ImGuiAPI.EndTable();
        }
        #endregion

        #region Frame graph
        // 对齐 UE Insights 的 Frames track: 一帧一根柱, 左旧右新, 高度是耗时。
        // 表格里一行一行读 CPU ms 看不出"哪几帧卡、卡在哪一段", 柱状图能一眼定位,
        // 而且 no-trace 帧的颜色能直接区分“没采到”和“采到但很快”
        const float MinGraphHeight = 160.0f;
        /// <summary>表格展开时图占剩余高度的比例, 收起时就占满整个面板</summary>
        const float GraphShareWithTable = 0.55f;
        /// <summary>鼠标移动超过这个距离才当拖拽平移, 否则当点击选帧</summary>
        const float PanThreshold = 4.0f;

        // 可视区间(帧下标, 浮点便于亚帧平滑): Span<=0 表示全览。
        // 滚轮以鼠标位置为锚点缩放, 左键拖拽平移, 双击回全览
        float mViewBegin;
        float mViewSpan;
        bool mShowTable;
        bool mPanPending;
        bool mPanning;
        Vector2 mPanStartMouse;
        float mPanStartBegin;
        // Y 轴上下限缓存: 帧集合不变时没必要每帧重排一遍
        int mP99Count = -1;
        float mP99LimitMs = 20.0f;
        /// <summary>
        /// CPU 柱的 Y 轴下限: 帧耗时都在 16ms 上下时从 0 起画, 整块面板会变成一片贴满的柱墙。
        /// GPU 柱不能共用这条基线: pass 之和通常只有CPU的一小半, 共用以它们会全部退化成 1px,
        /// 所以 GPU 有自己的 [mGpuFloorMs, mGpuLimitMs] 刻度, 图例上标出范围
        /// </summary>
        float mFloorMs = 0.0f;
        float mGpuFloorMs = 0.0f;
        float mGpuLimitMs = 20.0f;
        bool mHasGpu;

        static readonly uint ColCpuBar = 0xFF4E9A46;          // 采到事件流的帧
        static readonly uint ColCpuBarNoTrace = 0xFF3A4A36;   // 采集窗口外的帧
        static readonly uint ColCpuBarHover = 0xFF88D97A;
        static readonly uint ColCpuBarSel = 0xFF9BE08F;
        static readonly uint ColGpuBar = 0xFF3D7EBF;
        static readonly uint ColGpuBarSel = 0xFF9CCBF5;
        static readonly uint ColBudgetLine = 0xFF7A4A4A;
        /// <summary>超出 Y 轴上限被裁的柱头颜色</summary>
        static readonly uint ColBarClipped = 0xFFE0B040;
        static readonly uint ColGraphBg = 0xFF101010;

        void DrawFrameGraph(TtTraceApplication app, int count)
        {
            if (count <= 0)
                return;
            var avail = ImGuiAPI.GetContentRegionAvail();
            // dock 下 avail.Y 会返回整个 dock 节点的高度, 不夹住图会被画到窗口外(Timeline 同款坑)
            var crMin = ImGuiAPI.GetWindowContentRegionMin();
            var crMax = ImGuiAPI.GetWindowContentRegionMax();
            float contentH = crMax.Y - crMin.Y;
            if (contentH > 0 && avail.Y > contentH)
                avail.Y = contentH;
            float graphH = mShowTable ? avail.Y * GraphShareWithTable : avail.Y;
            if (graphH < MinGraphHeight)
                graphH = MinGraphHeight;
            float width = avail.X;
            if (width < 80.0f)
                return;

            var size = new Vector2(width, graphH);
            var origin = ImGuiAPI.GetCursorScreenPos();
            var rectMax = new Vector2(origin.X + width, graphH + origin.Y);
            var cmdlst = ImGuiAPI.GetWindowDrawList();
            cmdlst.AddRectFilled(in origin, in rectMax, ColGraphBg, 0, ImDrawFlags_.ImDrawFlags_None);

            ImGuiAPI.InvisibleButton("##FrameGraph", in size,
                ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft |
                ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonRight);
            bool hovered = ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);
            var mouse = ImGuiAPI.GetMousePos();

            float x0 = origin.X + 46.0f;        // 左侧让出 Y 轴刻度
            float x1 = rectMax.X - 8.0f;
            float yTop = origin.Y + 18.0f;      // 顶部让出峰值/图例
            float yBottom = rectMax.Y - 22.0f;  // 底部让出帧号刻度
            if (x1 - x0 < 16.0f || yBottom - yTop < 16.0f)
                return;

            ClampFrameView(count);
            float viewSpan = mViewSpan > 0.0f ? mViewSpan : count;
            float slotW = (x1 - x0) / viewSpan;

            // 滚轮缩放(锚住鼠标下的帧) / 双击回全览 / 左键拖拽平移
            if (hovered)
            {
                float wheel = ImGuiAPI.GetIO().MouseWheel;
                if (wheel != 0.0f && mouse.X >= x0 && mouse.X <= x1)
                {
                    float anchor = mViewBegin + (mouse.X - x0) / slotW;
                    float newSpan = viewSpan * (wheel > 0.0f ? 0.85f : 1.18f);
                    if (newSpan < 4.0f)
                        newSpan = 4.0f;
                    mViewSpan = newSpan >= count ? 0.0f : newSpan;   // 撑到全览就别留小数
                    if (mViewSpan > 0.0f)
                    {
                        // 缩放前后同一个帧落在同一个像素上, 否则每滚一格图就跑
                        float ns = (x1 - x0) / mViewSpan;
                        mViewBegin = anchor - (mouse.X - x0) / ns;
                    }
                    ClampFrameView(count);
                }
                if (ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    mViewSpan = 0.0f;
                    mViewBegin = 0.0f;
                }
            }
            if (hovered && ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false))
            {
                mPanPending = true;
                mPanning = false;
                mPanStartMouse = mouse;
                mPanStartBegin = mViewBegin;
            }
            if (mPanPending)
            {
                if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    float dx = mouse.X - mPanStartMouse.X;
                    if (mPanning == false && Math.Abs(dx) > PanThreshold)
                        mPanning = true;
                    if (mPanning && mViewSpan > 0.0f)
                    {
                        mViewBegin = mPanStartBegin - dx / slotW;
                        ClampFrameView(count);
                    }
                }
                else
                {
                    // 松手: 没拖动过就当点击选帧
                    if (mPanning == false && mouse.X >= x0 && mouse.X <= x1)
                    {
                        int pick = (int)Math.Floor(mViewBegin + (mouse.X - x0) / slotW);
                        if (pick >= 0 && pick < count)
                        {
                            var pf = app.GetFrame(pick);
                            if (pf != null)
                                app.TimelineVisual?.SelectFrame(pf);
                        }
                    }
                    mPanPending = false;
                    mPanning = false;
                }
            }
            ClampFrameView(count);
            viewSpan = mViewSpan > 0.0f ? mViewSpan : count;
            slotW = (x1 - x0) / viewSpan;

            // Y 轴上下限都用分位数而不是极值: 上限取 P99 避开首帧初始化/偶发大卡顿把正常帧压成一条线,
            // 下限取 P1 避开 0 基线把面板下部 80% 全填成柱子(16ms 的帧只需要看得出差异)。
            // CPU/GPU 各算一套刻度: 共用一套时 GPU 普遍低于 CPU 基线, 会整列退化成 1px。
            // 超出上限的柱在顶上标黄头, 低于下限的柱留 1px 头, 都不会被静默隐藏
            if (mP99Count != count)
            {
                var cpuSamples = new System.Collections.Generic.List<float>(count);
                var gpuSamples = new System.Collections.Generic.List<float>(count);
                for (int i = 0; i < count; i++)
                {
                    var f = app.GetFrame(i);
                    if (f == null)
                        continue;
                    cpuSamples.Add(f.DurationMicroSecond / 1000.0f);
                    float gpu = GetStat(f).GpuUs / 1000.0f;
                    if (gpu > 0.01f)
                        gpuSamples.Add(gpu);
                }
                mP99LimitMs = CalcAxisLimit(cpuSamples, 20.0f);
                mFloorMs = CalcAxisFloor(cpuSamples, mP99LimitMs);
                mHasGpu = gpuSamples.Count > 0;
                if (mHasGpu)
                {
                    mGpuLimitMs = CalcAxisLimit(gpuSamples, 1.0f);
                    mGpuFloorMs = CalcAxisFloor(gpuSamples, mGpuLimitMs);
                }
                else
                {
                    mGpuLimitMs = 20.0f;
                    mGpuFloorMs = 0.0f;
                }
                mP99Count = count;
            }
            float limitMs = mP99LimitMs;
            float floorMs = mFloorMs;
            float bandMs = limitMs - floorMs;
            if (bandMs < 1.0f)
                bandMs = 1.0f;
            float scale = (yBottom - yTop) / bandMs;
            float gpuBandMs = mGpuLimitMs - mGpuFloorMs;
            if (gpuBandMs < 0.5f)
                gpuBandMs = 0.5f;
            float gpuScale = (yBottom - yTop) / gpuBandMs;

            // 60fps 预算线, 超过它再画 33.3ms 那条
            DrawGraphLine(cmdlst, x0, x1, yTop, yBottom, scale, floorMs, 16.67f, "16.7ms", ColBudgetLine);
            if (33.3f > floorMs && 33.3f < limitMs)
                DrawGraphLine(cmdlst, x0, x1, yTop, yBottom, scale, floorMs, 33.3f, "33.3ms", ColBudgetLine);
            // 基线不是 0 必须在轴上写清楚, 否则柱高会被读成绝对耗时
            var zeroPos = new Vector2(origin.X + 4.0f, yBottom - 7.0f);
            cmdlst.AddText(in zeroPos, 0xFF606060, $"基线{floorMs:F1}ms", null);
            var topPos = new Vector2(origin.X + 4.0f, yTop - 7.0f);
            cmdlst.AddText(in topPos, 0xFF606060, $"{limitMs:F0}ms", null);
            // GPU 是独立刻度: 不写出来两根柱的高度就没法比较
            if (mHasGpu)
            {
                var gpuAxisPos = new Vector2(origin.X + 4.0f, yBottom - 21.0f);
                cmdlst.AddText(in gpuAxisPos, ColGpuBar, $"GPU {mGpuFloorMs:F1}~{mGpuLimitMs:F0}ms", null);
            }

            // 顶部说明 + 图例
            var titlePos = new Vector2(x0, origin.Y + 2.0f);
            cmdlst.AddText(in titlePos, 0xFFB0B0B0, $"帧耗时  {count} 帧  可视 #{Math.Round(mViewBegin)}~#{Math.Round(mViewBegin + viewSpan) - 1}", null);
            float legendX = x0 + 340.0f;
            if (legendX < x1 - 120.0f)
            {
                cmdlst.AddRectFilled(new Vector2(legendX, origin.Y + 7.0f), new Vector2(legendX + 9.0f, origin.Y + 14.0f), ColCpuBar, 0, ImDrawFlags_.ImDrawFlags_None);
                var l1 = new Vector2(legendX + 13.0f, origin.Y + 3.0f);
                cmdlst.AddText(in l1, 0xFF909090, "CPU", null);
                cmdlst.AddRectFilled(new Vector2(legendX + 48.0f, origin.Y + 7.0f), new Vector2(legendX + 57.0f, origin.Y + 14.0f), ColGpuBar, 0, ImDrawFlags_.ImDrawFlags_None);
                var l2 = new Vector2(legendX + 61.0f, origin.Y + 3.0f);
                // 图例宽度是硬排的, 这里不能加字; GPU 的实际刻度写在左下角同色标签上
                cmdlst.AddText(in l2, 0xFF909090, "GPU", null);
                cmdlst.AddRectFilled(new Vector2(legendX + 96.0f, origin.Y + 7.0f), new Vector2(legendX + 105.0f, origin.Y + 14.0f), ColCpuBarNoTrace, 0, ImDrawFlags_.ImDrawFlags_None);
                var l3 = new Vector2(legendX + 109.0f, origin.Y + 3.0f);
                cmdlst.AddText(in l3, 0xFF909090, "no-trace", null);
            }

            float barW = slotW * 0.42f;
            if (barW < 1.0f)
                barW = 1.0f;
            if (barW > 14.0f)
                barW = 14.0f;
            // 帧窄到两根柱会粘在一起时只画 CPU, 否则看着像一根错色的柱
            bool drawGpu = slotW >= 4.0f;

            int hoverIndex = -1;
            if (hovered && mouse.X >= x0 && mouse.X <= x1)
            {
                hoverIndex = (int)Math.Floor(mViewBegin + (mouse.X - x0) / slotW);
                if (hoverIndex < 0)
                    hoverIndex = 0;
                if (hoverIndex >= count)
                    hoverIndex = count - 1;
            }

            var selectedFrame = app.TimelineVisual?.SelectedFrame;
            int firstVisible = (int)Math.Floor(mViewBegin);
            int lastVisible = (int)Math.Ceiling(mViewBegin + viewSpan);
            if (firstVisible < 0)
                firstVisible = 0;
            if (lastVisible > count)
                lastVisible = count;
            for (int i = firstVisible; i < lastVisible; i++)
            {
                var frame = app.GetFrame(i);
                if (frame == null)
                    continue;
                float bx = x0 + (i - mViewBegin) * slotW;
                if (bx + barW * 2.0f < x0 || bx > x1)
                    continue;

                var stat = GetStat(frame);
                float cpuMs = frame.DurationMicroSecond / 1000.0f;
                float gpuMs = stat.GpuUs / 1000.0f;
                bool hasTrace = stat.EventCount > 0 || stat.GpuUs > 0;
                bool selected = ReferenceEquals(frame, selectedFrame);
                bool isHover = i == hoverIndex;

                uint cpuCol = selected ? ColCpuBarSel : (hasTrace ? (isHover ? ColCpuBarHover : ColCpuBar) : ColCpuBarNoTrace);
                bool clipped = cpuMs > limitMs;
                float ch = (cpuMs - floorMs) * scale;
                if (ch > yBottom - yTop)
                    ch = yBottom - yTop;
                if (ch < 1.0f)
                    ch = 1.0f;
                var cMin = new Vector2(bx, yBottom - ch);
                var cMax = new Vector2(bx + barW, yBottom);
                cmdlst.AddRectFilled(in cMin, in cMax, cpuCol, 0, ImDrawFlags_.ImDrawFlags_None);
                if (clipped)
                {
                    // 被裁的柱头标黄, 否则用户会以为这一帧真的只有那么高
                    var kMin = new Vector2(bx, yBottom - ch);
                    var kMax = new Vector2(bx + barW, yBottom - ch + 3.0f);
                    cmdlst.AddRectFilled(in kMin, in kMax, ColBarClipped, 0, ImDrawFlags_.ImDrawFlags_None);
                }

                if (drawGpu && gpuMs > 0.01f)
                {
                    // GPU 用自己的刻度, 否则 pass 之和普遍低于 CPU 基线时会全部退化成 1px
                    float gh = (gpuMs - mGpuFloorMs) * gpuScale;
                    if (gh > yBottom - yTop)
                        gh = yBottom - yTop;
                    if (gh < 1.0f)
                        gh = 1.0f;
                    float gx = bx + barW + 0.5f;
                    var gMin = new Vector2(gx, yBottom - gh);
                    var gMax = new Vector2(gx + barW, yBottom);
                    cmdlst.AddRectFilled(in gMin, in gMax, selected ? ColGpuBarSel : ColGpuBar, 0, ImDrawFlags_.ImDrawFlags_None);
                }

                if (selected)
                {
                    // 高亮框要盖住两根柱里更高的那根, 否则 GPU 冲到顶时会被截在框外
                    float selH = mHasGpu && gpuMs > 0.01f ? Math.Max(ch, (gpuMs - mGpuFloorMs) * gpuScale) : ch;
                    if (selH > yBottom - yTop)
                        selH = yBottom - yTop;
                    float selW = drawGpu ? barW * 2.0f + 0.5f : barW;
                    var sMin = new Vector2(bx - 1.0f, yBottom - selH - 1.0f);
                    var sMax = new Vector2(bx + selW + 1.0f, yBottom + 1.0f);
                    cmdlst.AddRect(in sMin, in sMax, 0xFFFFFFFF, 0, ImDrawFlags_.ImDrawFlags_None, 1.0f);
                }
            }

            // 帧号刻度: 按像素间距挑一个整齐的步长, 缩放后自然变密/变稀
            int rough = Math.Max(1, (int)Math.Ceiling(96.0f / slotW));
            int step = rough <= 10 ? 10 : (rough <= 50 ? 50 : (rough <= 100 ? 100 : (rough <= 500 ? 500 : 1000)));
            if (step > count)
                step = Math.Max(1, count);
            int firstTick = (int)(Math.Ceiling(mViewBegin / step) * step);
            for (int t = firstTick; t <= mViewBegin + viewSpan; t += step)
            {
                if (t < 0 || t >= count)
                    continue;
                float tx = x0 + (t - mViewBegin) * slotW;
                cmdlst.AddLine(new Vector2(tx, yBottom), new Vector2(tx, yBottom + 4.0f), 0xFF404040, 1.0f);
                var tf = app.GetFrame(t);
                if (tf == null)
                    continue;
                var label = $"#{tf.FrameIndex}";
                float lx = tx + 3.0f;
                if (lx + label.Length * 7.5f > x1)
                    lx = tx - label.Length * 7.5f - 3.0f;
                var lp = new Vector2(lx, yBottom + 5.0f);
                cmdlst.AddText(in lp, 0xFF808080, label, null);
            }

            if (hoverIndex >= 0)
            {
                var frame = app.GetFrame(hoverIndex);
                if (frame != null)
                {
                    var stat = GetStat(frame);
                    var flag = (stat.EventCount == 0 && stat.GpuUs == 0) ? "no-trace" : "ok";
                    ImGuiAPI.SetTooltip($"#{frame.FrameIndex}  CPU {frame.DurationMicroSecond / 1000.0f:F3} ms  GPU {stat.GpuUs / 1000.0f:F3} ms\n事件 {stat.EventCount} / {stat.LaneCount} 泳道  {flag}");
                }
            }
        }

        /// <summary>把可视区间夹回 [0, count]; 区间撑满就归一成“全览”(Span=0)</summary>
        void ClampFrameView(int count)
        {
            if (count <= 4 || mViewSpan <= 0.0f || mViewSpan >= count)
            {
                mViewSpan = 0.0f;
                mViewBegin = 0.0f;
                return;
            }
            if (mViewSpan < 4.0f)
                mViewSpan = 4.0f;
            float maxBegin = count - mViewSpan;
            if (mViewBegin > maxBegin)
                mViewBegin = maxBegin;
            if (mViewBegin < 0.0f)
                mViewBegin = 0.0f;
        }

        /// <summary>
        /// 刻度上限取 P99 而不是最大值: 首帧初始化/偶发大卡顿不该把正常帧压成一条线。
        /// minLimit 是“宁可留白也不能把柱拉满”的底线
        /// </summary>
        static float CalcAxisLimit(System.Collections.Generic.List<float> samples, float minLimit)
        {
            if (samples.Count == 0)
                return minLimit;
            samples.Sort();
            int idx = (int)(samples.Count * 0.99f);
            if (idx >= samples.Count)
                idx = samples.Count - 1;
            return Math.Max(minLimit, samples[idx] * 1.15f);
        }

        /// <summary>
        /// 刻度下限取 P1 × 0.95: 数据集中在 16ms 附近时从 0 起画会整块面板变成柱墙。
        /// 再夹一次上限, 保证柱高至少留四分之一高度, 否则很平的数据就成了横着的一条线
        /// </summary>
        static float CalcAxisFloor(System.Collections.Generic.List<float> samples, float limitMs)
        {
            if (samples.Count == 0)
                return 0.0f;
            samples.Sort();
            int idx = (int)(samples.Count * 0.01f);
            if (idx >= samples.Count)
                idx = samples.Count - 1;
            float floor = Math.Max(0.0f, samples[idx] * 0.95f);
            float cap = limitMs * 0.75f;
            return floor > cap ? cap : floor;
        }

        static void DrawGraphLine(ImDrawList cmdlst, float x0, float x1, float yTop, float yBottom, float scale, float floorMs, float ms, string label, uint col)
        {
            float y = yBottom - (ms - floorMs) * scale;
            if (y < yTop)
                return;   // 基线抬高后预算线可能已经在上限之外, 不画到标题上去
            cmdlst.AddLine(new Vector2(x0, y), new Vector2(x1, y), col, 1.0f);
            var pos = new Vector2(x0 + 4.0f, y - 13.0f);
            cmdlst.AddText(in pos, col, label, null);
        }
        #endregion
    }
}
