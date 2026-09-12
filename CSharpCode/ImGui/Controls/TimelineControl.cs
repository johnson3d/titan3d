using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Controls
{
    public enum ETimelineItemType
    {
        /// <summary>
        /// 有长度的条(Montage段落)
        /// </summary>
        Bar,
        /// <summary>
        /// 无长度的打点(瞬时通知、Section起点)
        /// </summary>
        Marker,
        /// <summary>
        /// 有长度的区间(持续通知), 与Bar的区别只在于绘制样式
        /// </summary>
        Range,
    }

    public class TtTimelineItem
    {
        /// <summary>
        /// 本帧内唯一的标识, 控件用它记录选中与拖拽状态
        /// </summary>
        public string Id;
        public string Label;
        public int TrackIndex = 0;
        public float Begin = 0.0f;
        /// <summary>
        /// Marker类型忽略该值
        /// </summary>
        public float End = 0.0f;
        public ETimelineItemType Type = ETimelineItemType.Bar;
        public uint Color = 0xFF7F7F7F;
        public bool AllowMove = true;
        public bool AllowResize = false;
        /// <summary>
        /// 缩放时范围必须始终盖住的区间 (秒), NaN 表示不约束。只作用于 ResizeBegin/
        /// ResizeEnd, 整体拖动不受限 —— 那时区间里的东西跟着一起平移。
        ///
        /// 调用方拿它声明“这个条里装着东西, 别把它们露到范围外面”。Sequencer 传的是
        /// 这一段最早/最晚的关键帧: 起点被拖到第一个关键帧右边的话, 那个关键帧就既
        /// 不驱动任何东西 (段落在那个时刻还没生效) 又照旧画在轨道上。
        /// </summary>
        public float MustCoverBegin = float.NaN;
        public float MustCoverEnd = float.NaN;
        /// <summary>
        /// 调用方挂自己的数据对象, 控件不解释
        /// </summary>
        public object UserData;
    }

    public class TtTimelineTrack
    {
        public string Label;
        public float Height = 22.0f;
    }

    /// <summary>
    /// 最小可用的时间轴控件: 时间标尺 + 播放头 + 多轨道条/打点的选中与拖拽。
    /// 供Montage编辑器编辑Slot段落/Section/Notify, 也供AnimationClip编辑器编辑Notify。
    ///
    /// 用法: 调用方每帧填Tracks/Items, 调OnDraw, 然后读取本帧的交互结果字段。
    /// 控件本身不弹Popup(Popup必须与OpenPopup处于同一PushID作用域, 交由调用方处理)。
    /// </summary>
    public class TtTimelineControl
    {
        public float Duration { get; set; } = 1.0f;
        public float PlayPosition { get; set; } = 0.0f;
        /// <summary>
        /// 视图左边界(秒)
        /// </summary>
        public float ViewStart { get; set; } = 0.0f;
        /// <summary>
        /// 视图跨度(秒), 小于0表示自适应整条时间轴
        /// </summary>
        public float ViewDuration { get; set; } = -1.0f;
        /// <summary>
        /// 拖拽吸附间隔(秒), 0表示不吸附
        /// </summary>
        public float SnapInterval { get; set; } = 0.0f;
        public float LabelWidth { get; set; } = 96.0f;
        public float HeaderHeight { get; set; } = 20.0f;
        public string SelectedItemId { get; set; } = null;

        public List<TtTimelineTrack> Tracks { get; } = new List<TtTimelineTrack>();
        public List<TtTimelineItem> Items { get; } = new List<TtTimelineItem>();

        #region 本帧交互结果
        public bool PlayPositionChanged { get; private set; } = false;
        /// <summary>
        /// 本帧被拖动或缩放的项(时间已被写回该对象)
        /// </summary>
        public TtTimelineItem ChangedItem { get; private set; } = null;
        public TtTimelineItem ClickedItem { get; private set; } = null;
        /// <summary>
        /// 本帧是否刚结束一次项的拖拽 (松手那一帧为 true)。
        ///
        /// 判“拖拽结束”必须用它, 不要用 "ChangedItem == null": 那个字段的含义是
        /// “本帧值发生了变化”, 鼠标按住不动的帧它就是 null。拿它当结束信号会把一次
        /// 拖拽切成 N 段 (N = 拖拽过程中鼠标停顿的帧数), 撤销一次拖拽得按 N 次 Ctrl+Z。
        /// </summary>
        public bool ItemDragFinished { get; private set; } = false;
        public TtTimelineItem DoubleClickedItem { get; private set; } = null;
        public TtTimelineItem RightClickedItem { get; private set; } = null;
        /// <summary>
        /// 在轨道空白处右键时的轨道索引, 没有则为-1
        /// </summary>
        public int RightClickedTrack { get; private set; } = -1;
        public float RightClickedTime { get; private set; } = 0.0f;
        #endregion 本帧交互结果

        enum EDragMode
        {
            None,
            Move,
            ResizeBegin,
            ResizeEnd,
            PlayHead,
        }
        EDragMode mDragMode = EDragMode.None;
        string mDragItemId = null;
        float mDragTimeOffset = 0.0f;

        const float EdgeGrabPixels = 4.0f;
        const float MarkerHalfWidth = 5.0f;
        /// <summary>播放头竖线的可抓半宽 (像素)。线宽 2px, 给宽裕一点免得要像素级对准。</summary>
        const float PlayHeadGrabPixels = 5.0f;
        /// <summary>播放头顶部手柄的尺寸。光杆自己看不出可拖, 给个手柄做 affordance。</summary>
        const float PlayHeadHandleHalfWidth = 5.0f;
        const float PlayHeadHandleHeight = 8.0f;

        static uint ColorBackground { get => UIProxy.StyleConfig.Instance.PanelBackground; }
        static uint ColorGrid { get => UIProxy.StyleConfig.Instance.GridColor; }
        static uint ColorText { get => UIProxy.StyleConfig.Instance.TextColor; }
        const uint ColorPlayHead = 0xFF3030FF;
        /// <summary>悬停可抓 / 正在拖拽时的播放头颜色</summary>
        const uint ColorPlayHeadHot = 0xFF9090FF;
        const uint ColorSelected = 0xFF00FFFF;
        const uint ColorTrackBgOdd = 0x20FFFFFF;

        float EffectiveViewDuration
        {
            get
            {
                float view = ViewDuration > 0.0f ? ViewDuration : Duration;
                return view > 1e-4f ? view : 1.0f;
            }
        }

        float TimeToScreenX(float time, float areaX0, float areaWidth)
        {
            return areaX0 + (time - ViewStart) / EffectiveViewDuration * areaWidth;
        }
        float ScreenXToTime(float screenX, float areaX0, float areaWidth)
        {
            if (areaWidth <= 0.0f)
                return ViewStart;
            return ViewStart + (screenX - areaX0) / areaWidth * EffectiveViewDuration;
        }
        float Snap(float time)
        {
            if (SnapInterval <= 0.0f)
                return time;
            return (float)Math.Round(time / SnapInterval) * SnapInterval;
        }
        float ClampTime(float time)
        {
            return MathHelper.Clamp(time, 0.0f, Math.Max(Duration, 0.0f));
        }

        /// <summary>
        /// 绘制时间轴。返回true表示本帧有数据被修改(时间或播放头)。
        /// </summary>
        public unsafe bool OnDraw(in Vector2 size)
        {
            PlayPositionChanged = false;
            ChangedItem = null;
            ClickedItem = null;
            DoubleClickedItem = null;
            RightClickedItem = null;
            RightClickedTrack = -1;
            ItemDragFinished = false;

            var origin = ImGuiAPI.GetCursorScreenPos();
            var canvasSize = size;
            if (canvasSize.X <= 0.0f)
                canvasSize.X = ImGuiAPI.GetContentRegionAvail().X;
            float tracksHeight = 0.0f;
            for (int i = 0; i < Tracks.Count; ++i)
                tracksHeight += Tracks[i].Height;
            if (canvasSize.Y <= 0.0f)
                canvasSize.Y = HeaderHeight + tracksHeight + 4.0f;

            var canvasEnd = origin + canvasSize;
            float areaX0 = origin.X + LabelWidth;
            float areaWidth = Math.Max(1.0f, canvasEnd.X - areaX0);

            var drawList = ImGuiAPI.GetWindowDrawList();
            drawList.AddRectFilled(in origin, in canvasEnd, ColorBackground, 3.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
            drawList.AddRect(in origin, in canvasEnd, ColorGrid, 3.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);

            ImGuiAPI.InvisibleButton("##TimelineCanvas", in canvasSize,
                ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft | ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonRight);
            bool canvasHovered = ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);
            var mouse = ImGuiAPI.GetMousePos();

            DrawRuler(drawList, in origin, in canvasEnd, areaX0, areaWidth);
            bool changed = DrawTracksAndItems(drawList, in origin, in canvasEnd, areaX0, areaWidth, canvasHovered, in mouse);

            // 播放头: 竖线贯穿全部轨道
            float playX = TimeToScreenX(PlayPosition, areaX0, areaWidth);
            // 可抓判定必须在 DrawTracksAndItems 之后算: mDragMode 非 None 说明本帧被某个 item 抢先
            // 命中了, 那时不该报可抓。
            bool playHeadGrabbable = canvasHovered && mDragMode == EDragMode.None &&
                IsOverPlayHeadGrabZone(in mouse, in origin, in canvasEnd, areaX0, areaWidth);
            bool playHeadHot = mDragMode == EDragMode.PlayHead || playHeadGrabbable;
            if (playHeadHot)
                ImGuiAPI.SetMouseCursor(ImGuiMouseCursor_.ImGuiMouseCursor_ResizeEW);
            if (playX >= areaX0 && playX <= canvasEnd.X)
            {
                var headColor = playHeadHot ? ColorPlayHeadHot : ColorPlayHead;
                var p0 = new Vector2(playX, origin.Y);
                var p1 = new Vector2(playX, canvasEnd.Y);
                drawList.AddLine(in p0, in p1, headColor, playHeadHot ? 3.0f : 2.0f);
                var handleMin = new Vector2(playX - PlayHeadHandleHalfWidth, origin.Y);
                var handleMax = new Vector2(playX + PlayHeadHandleHalfWidth, origin.Y + PlayHeadHandleHeight);
                drawList.AddRectFilled(in handleMin, in handleMax, headColor, 2.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
            }

            changed |= UpdatePlayHeadDrag(canvasHovered, in mouse, in origin, in canvasEnd, areaX0, areaWidth);
            return changed;
        }

        void DrawRuler(ImDrawList drawList, in Vector2 origin, in Vector2 canvasEnd, float areaX0, float areaWidth)
        {
            var headerEnd = new Vector2(canvasEnd.X, origin.Y + HeaderHeight);
            drawList.AddRectFilled(in origin, in headerEnd, ColorTrackBgOdd, 0.0f, ImDrawFlags_.ImDrawFlags_None);

            // 刻度间隔取1/2/5的10倍数序列, 保证屏幕上间距不小于60像素
            float viewDuration = EffectiveViewDuration;
            float roughStep = viewDuration * 60.0f / areaWidth;
            float step = 0.01f;
            while (step < roughStep)
            {
                if (step * 2.0f >= roughStep) { step *= 2.0f; break; }
                if (step * 5.0f >= roughStep) { step *= 5.0f; break; }
                step *= 10.0f;
            }

            float first = (float)Math.Ceiling(ViewStart / step) * step;
            for (float t = first; t <= ViewStart + viewDuration; t += step)
            {
                float x = TimeToScreenX(t, areaX0, areaWidth);
                if (x < areaX0 || x > canvasEnd.X)
                    continue;
                var p0 = new Vector2(x, origin.Y + HeaderHeight * 0.5f);
                var p1 = new Vector2(x, canvasEnd.Y);
                drawList.AddLine(in p0, in p1, ColorGrid, 1.0f);
                var textPos = new Vector2(x + 2, origin.Y + 2);
                drawList.AddText(in textPos, ColorText, t.ToString("0.##") + "s", null);
            }

            var sepA = new Vector2(areaX0, origin.Y);
            var sepB = new Vector2(areaX0, canvasEnd.Y);
            drawList.AddLine(in sepA, in sepB, ColorGrid, 1.0f);
        }

        bool DrawTracksAndItems(ImDrawList drawList, in Vector2 origin, in Vector2 canvasEnd, float areaX0, float areaWidth,
            bool canvasHovered, in Vector2 mouse)
        {
            bool changed = false;
            float rowY = origin.Y + HeaderHeight;
            bool leftClicked = ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false);
            bool leftDoubleClicked = ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            bool rightClicked = ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Right, false);
            bool leftDown = ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left);

            TtTimelineItem hitItem = null;
            EDragMode hitMode = EDragMode.None;

            for (int trackIndex = 0; trackIndex < Tracks.Count; ++trackIndex)
            {
                var track = Tracks[trackIndex];
                float rowHeight = track.Height;
                var rowMin = new Vector2(origin.X, rowY);
                var rowMax = new Vector2(canvasEnd.X, rowY + rowHeight);
                if ((trackIndex & 1) == 1)
                    drawList.AddRectFilled(in rowMin, in rowMax, ColorTrackBgOdd, 0.0f, ImDrawFlags_.ImDrawFlags_None);

                if (!string.IsNullOrEmpty(track.Label))
                {
                    // 标签必须裁在行头区里: 文字宽度由数据定 (绑定名 + 轨道名很容易超过
                    // LabelWidth), 溢出去就压在时间轴的刻度和条子上, 拼成一堆看不出来的字。
                    var labelClipMin = new Vector2(origin.X, rowY);
                    var labelClipMax = new Vector2(areaX0 - 2.0f, rowY + rowHeight);
                    drawList.PushClipRect(in labelClipMin, in labelClipMax, true);
                    var labelPos = new Vector2(origin.X + 4, rowY + 3);
                    drawList.AddText(in labelPos, ColorText, track.Label, null);
                    drawList.PopClipRect();
                }

                // 项一律裁在时间轴区内: 起点在 ViewStart 之前的条会一直画到行头标签上去。
                var itemClipMin = new Vector2(areaX0, rowY);
                var itemClipMax = new Vector2(canvasEnd.X, rowY + rowHeight);
                drawList.PushClipRect(in itemClipMin, in itemClipMax, true);
                for (int i = 0; i < Items.Count; ++i)
                {
                    var item = Items[i];
                    if (item.TrackIndex != trackIndex)
                        continue;

                    bool selected = SelectedItemId != null && SelectedItemId == item.Id;
                    Vector2 itemMin;
                    Vector2 itemMax;
                    if (item.Type == ETimelineItemType.Marker)
                    {
                        float x = TimeToScreenX(item.Begin, areaX0, areaWidth);
                        itemMin = new Vector2(x - MarkerHalfWidth, rowY + 2);
                        itemMax = new Vector2(x + MarkerHalfWidth, rowY + rowHeight - 2);
                        drawList.AddRectFilled(in itemMin, in itemMax, item.Color, 2.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
                    }
                    else
                    {
                        float x0 = TimeToScreenX(item.Begin, areaX0, areaWidth);
                        float x1 = TimeToScreenX(Math.Max(item.End, item.Begin), areaX0, areaWidth);
                        if (x1 - x0 < 2.0f)
                            x1 = x0 + 2.0f;
                        itemMin = new Vector2(x0, rowY + 2);
                        itemMax = new Vector2(x1, rowY + rowHeight - 2);
                        drawList.AddRectFilled(in itemMin, in itemMax, item.Color, 2.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
                        if (!string.IsNullOrEmpty(item.Label))
                        {
                            var labelPos = new Vector2(x0 + 3, rowY + 3);
                            drawList.AddText(in labelPos, ColorText, item.Label, null);
                        }
                    }
                    if (selected)
                        drawList.AddRect(in itemMin, in itemMax, ColorSelected, 2.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 2.0f);

                    if (!canvasHovered || mDragMode != EDragMode.None)
                        continue;
                    // 抓取区不能伸到行头区里去: 起点落在 ViewStart 上的条 (Sequencer 里就是
                    // StartTick=0 的 Section) 左边缘正好压在 areaX0 这条分隔线上, ±EdgeGrabPixels
                    // 跨过去之后, 点行头标签就会启动 ResizeBegin —— 用户想选轨道, 结果把条子
                    // 的起点拖走了。
                    if (mouse.X < areaX0)
                        continue;
                    if (mouse.X < itemMin.X - EdgeGrabPixels || mouse.X > itemMax.X + EdgeGrabPixels ||
                        mouse.Y < itemMin.Y || mouse.Y > itemMax.Y)
                        continue;

                    hitItem = item;
                    hitMode = EDragMode.Move;
                    if (item.AllowResize && item.Type != ETimelineItemType.Marker)
                    {
                        if (Math.Abs(mouse.X - itemMin.X) <= EdgeGrabPixels)
                            hitMode = EDragMode.ResizeBegin;
                        else if (Math.Abs(mouse.X - itemMax.X) <= EdgeGrabPixels)
                            hitMode = EDragMode.ResizeEnd;
                    }
                }
                drawList.PopClipRect();

                // 轨道空白处右键: 交给调用方弹新增菜单
                if (canvasHovered && rightClicked && hitItem == null &&
                    mouse.Y >= rowMin.Y && mouse.Y <= rowMax.Y && mouse.X >= areaX0)
                {
                    RightClickedTrack = trackIndex;
                    RightClickedTime = ClampTime(ScreenXToTime(mouse.X, areaX0, areaWidth));
                }

                rowY += rowHeight;
            }

            if (hitItem != null)
            {
                if (leftDoubleClicked)
                {
                    DoubleClickedItem = hitItem;
                    SelectedItemId = hitItem.Id;
                }
                else if (leftClicked)
                {
                    ClickedItem = hitItem;
                    SelectedItemId = hitItem.Id;
                    if (hitItem.AllowMove || hitMode != EDragMode.Move)
                    {
                        mDragMode = hitMode;
                        mDragItemId = hitItem.Id;
                        mDragTimeOffset = ScreenXToTime(ImGuiAPI.GetMousePos().X, areaX0, areaWidth) - hitItem.Begin;
                    }
                }
                if (rightClicked)
                {
                    RightClickedItem = hitItem;
                    SelectedItemId = hitItem.Id;
                }
            }

            // 拖拽推进: 用IsMouseDown续拖, 松手结束
            if (mDragMode != EDragMode.None && mDragMode != EDragMode.PlayHead)
            {
                if (!leftDown)
                {
                    mDragMode = EDragMode.None;
                    mDragItemId = null;
                    ItemDragFinished = true;
                }
                else
                {
                    var item = FindItem(mDragItemId);
                    if (item == null)
                    {
                        // 拖拽中项没了 (调用方重建了数据): 也得报一次结束, 否则调用方那边
                        // 攒着的 undo 快照永远等不到收尾。
                        mDragMode = EDragMode.None;
                        mDragItemId = null;
                        ItemDragFinished = true;
                    }
                    else
                    {
                        float mouseTime = ClampTime(ScreenXToTime(mouse.X, areaX0, areaWidth));
                        switch (mDragMode)
                        {
                            case EDragMode.Move:
                                {
                                    float length = item.Type == ETimelineItemType.Marker ? 0.0f : Math.Max(0.0f, item.End - item.Begin);
                                    float newBegin = ClampTime(Snap(mouseTime - mDragTimeOffset));
                                    if (newBegin + length > Duration)
                                        newBegin = Math.Max(0.0f, Duration - length);
                                    if (newBegin != item.Begin)
                                    {
                                        item.Begin = newBegin;
                                        item.End = item.Type == ETimelineItemType.Marker ? newBegin : newBegin + length;
                                        ChangedItem = item;
                                        changed = true;
                                    }
                                }
                                break;
                            case EDragMode.ResizeBegin:
                                {
                                    float newBegin = Math.Min(Snap(mouseTime), item.End);
                                    // 缩放不能把 MustCover 区间露到范围外面去。不夹的话, 条子里那些
                                    // 东西就变成了孤儿: 既不生效又还画在轨道上, 看着像“前面一段怎么
                                    // 拖都不动, 到起点突然跳一下”。
                                    if (float.IsNaN(item.MustCoverBegin) == false && newBegin > item.MustCoverBegin)
                                        newBegin = item.MustCoverBegin;
                                    if (newBegin != item.Begin)
                                    {
                                        item.Begin = newBegin;
                                        ChangedItem = item;
                                        changed = true;
                                    }
                                }
                                break;
                            case EDragMode.ResizeEnd:
                                {
                                    float newEnd = Math.Max(Snap(mouseTime), item.Begin);
                                    if (float.IsNaN(item.MustCoverEnd) == false && newEnd < item.MustCoverEnd)
                                        newEnd = item.MustCoverEnd;
                                    if (newEnd != item.End)
                                    {
                                        item.End = newEnd;
                                        ChangedItem = item;
                                        changed = true;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            return changed;
        }

        /// <summary>
        /// 鼠标是否落在能启动播放头拖拽的区域: 标尺行任意处, 或竖线本身附近。
        /// 竖线贯穿整个画布高度, 就得在整个高度上都能抓住它, 否则用户按住线往旁边拖却没反应。
        /// </summary>
        bool IsOverPlayHeadGrabZone(in Vector2 mouse, in Vector2 origin, in Vector2 canvasEnd, float areaX0, float areaWidth)
        {
            // 上下界都要判: 只判 mouse.X >= areaX0 的话, 标尺右侧空白 (乃至画布之外) 的点击也会
            // 启动拖拽, 而 ClampTime 会把它夹成 Duration —— 表现为播放头莫名跳到末尾。
            if (mouse.X < areaX0 || mouse.X > areaX0 + areaWidth)
                return false;
            if (mouse.Y >= origin.Y && mouse.Y <= origin.Y + HeaderHeight)
                return true;
            if (mouse.Y < origin.Y || mouse.Y > canvasEnd.Y)
                return false;
            return Math.Abs(mouse.X - TimeToScreenX(PlayPosition, areaX0, areaWidth)) <= PlayHeadGrabPixels;
        }

        bool UpdatePlayHeadDrag(bool canvasHovered, in Vector2 mouse, in Vector2 origin, in Vector2 canvasEnd, float areaX0, float areaWidth)
        {
            bool leftDown = ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left);

            if (mDragMode == EDragMode.None && canvasHovered &&
                IsOverPlayHeadGrabZone(in mouse, in origin, in canvasEnd, areaX0, areaWidth) &&
                ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false))
            {
                mDragMode = EDragMode.PlayHead;
            }
            if (mDragMode != EDragMode.PlayHead)
                return false;
            if (!leftDown)
            {
                mDragMode = EDragMode.None;
                return false;
            }

            float newPosition = ClampTime(ScreenXToTime(mouse.X, areaX0, areaWidth));
            if (newPosition == PlayPosition)
                return false;

            PlayPosition = newPosition;
            PlayPositionChanged = true;
            return true;
        }

        TtTimelineItem FindItem(string id)
        {
            if (id == null)
                return null;
            for (int i = 0; i < Items.Count; ++i)
            {
                if (Items[i].Id == id)
                    return Items[i];
            }
            return null;
        }
    }
}
