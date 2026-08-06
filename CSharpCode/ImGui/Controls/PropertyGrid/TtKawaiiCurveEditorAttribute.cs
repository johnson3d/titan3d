using EngineNS.Bricks.Animation.KawaiiPhysics;
using System;

namespace EngineNS.EGui.Controls.PropertyGrid
{
    /// <summary>
    /// PropertyGrid 自定义编辑器: 给 TtKawaiiCurve 属性画一个轻量可拖点的线性曲线控件。
    ///   - 左键拖动关键点; 双击空白加点; 右键点删点(至少保留 1 个点)。
    ///   - X 固定 [0,1](归一化链位置), Y 夹取到当前量程。
    ///   - 量程优先用曲线自带的 ViewYMin/ViewYMax(可序列化、美术可在控件下方直接改),
    ///     未指定时才用下面 YMin/YMax 这两个 attribute 默认值。
    ///   - 折线渲染, 与运行时 native FKawaiiCurve 的线性插值一致, 不做贝塞尔手柄。
    /// 放在 PropertyGrid 目录下, chain/cloth/rod 及后续任何 TtKawaiiCurve 字段都可复用。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class TtKawaiiCurveEditorAttribute : TtPGCustomValueEditorAttribute
    {
        /// <summary>量程下限默认值(曲线未自带量程时生效)。</summary>
        public float YMin = 0.0f;
        /// <summary>
        /// 量程上限默认值(曲线未自带量程时生效)。
        /// 注意: 曲线是乘子(最终值 = 基准标量 × 曲线值), 所以上限必须 > 1 才能"放大";
        /// 如果封顶在 1, 曲线只能把值往小拉(两个 0..1 相乘只会更小), 比如 base Stiffness=0.05
        /// 时永远做不到"根部紧跟动画"。
        /// </summary>
        public float YMax = 1.0f;
        /// <summary>控件高度(像素)。</summary>
        public float Height = 90.0f;

        // 命中拖拽状态: 记录正在拖的关键点下标, 跳帧保持。attribute 实例由 PGRenderer
        // 的 mDrawTargetDic 按 target 对象缓存(dirty 才重建), 所以字段能跳帧存活。
        int mDraggingKey = -1;
        
        // 本帧生效的量程(OnDraw 开头从曲线/attribute 解析一次, 供坐标换算复用)
        float mViewMin = 0.0f;
        float mViewMax = 1.0f;

        // 大编辑窗用真模态对话框(BeginPopupModal)。关键: OpenPopup/BeginPopupModal 必须在
        // PushID 之外、PG 单元格的自然 id 层调用, 且 popup id 自带唯一后缀(不靠 PushID),
        // 否则模态屏障不会装到根层 → 点击穿透到背景且被当非模态 popup 点外即关。
        // (参考可用模式: Grapics/Pipeline/Shader/Material.cs 的 ShaderCode 模态编辑器)
        bool mBigOpen = false;
        string mBigId;

        const float HandleRadius = 4.0f;
        const float HitRadius = 9.0f;   // 命中拖拽的宽容半径(比显示点大, 更好抓)
        const uint ColBg = 0xff1a1a1a;
        const uint ColGrid = 0x40ffffff;
        const uint ColNeutral = 0x80ffd24f;   // 中性线(乘子=1)标记色, 半透明琥珀
        const uint ColLine = 0xff4fc3f7;
        const uint ColKey = 0xffffffff;
        const uint ColKeyHot = 0xff00ffff;

        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            var curve = info.Value as TtKawaiiCurve;
            if (curve == null)
                return false;

            bool changed = false;
            bool readOnly = info.Readonly;

            // 每条曲线可自带量程; 未指定时用 attribute 默认值。本帧内统一用这对值,
            // 避免改量程后同一帧里渲染与命中用不同坐标系。
            curve.GetViewRange(YMin, YMax, out mViewMin, out mViewMax);

            ImGuiAPI.PushID(info.Name ?? "KawaiiCurve");

            var index = ImGuiAPI.TableGetColumnIndex();
            var width = ImGuiAPI.GetColumnWidth(index) - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X;
            if (width < 60)
                width = 60;

            // 内联画布: 模态打开时改为只展示, 避免与模态画布抢拖拽状态。
            var origin = ImGuiAPI.GetCursorScreenPos();
            var size = new Vector2(width, Height);
            DrawCanvas(curve, in origin, in size, "Inline", readOnly, !mBigOpen, ref changed);

            // 量程行 + 展开按钮(按钮在 PushID 内, 但 OpenPopup 延到 PushID 之外再发)
            bool wantOpen = false;
            if (!readOnly)
            {
                float btnW = 22.0f;
                DrawRangeRow(curve, width - btnW - 4, "Inline", ref changed);
                ImGuiAPI.SameLine(0, 4);
                var btnSz = new Vector2(btnW, 0);
                if (ImGuiAPI.Button("⤢##CurveBigOpen", in btnSz))
                    wantOpen = true;
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                    EGui.Controls.CtrlUtility.DrawHelper("Open large editor for precise editing");
            }

            ImGuiAPI.PopID();

            // 模态大窗: 必须在 PushID 之外、id 自带唯一后缀, 才能正确装上模态屏障。
            mBigId = $"Kawaii Curve: {info.Name}##KawaiiCurveBig_{info.Name}";
            if (wantOpen)
            {
                mBigOpen = true;
                ImGuiAPI.OpenPopup(mBigId, ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            if (mBigOpen)
                DrawBigEditor(curve, readOnly, ref changed);

            return changed;
        }

        // 画一块曲线画布(背景/网格/中性线/命中区/折线/关键点 + 拖拽⊙双击⊙右键交互)。
        // 内联与大窗共用; idSuffix 区分两个 InvisibleButton 的 id; interactive=false 时只画不改。
        void DrawCanvas(TtKawaiiCurve curve, in Vector2 origin, in Vector2 size, string idSuffix,
            bool readOnly, bool interactive, ref bool changed)
        {
            var drawList = ImGuiAPI.GetWindowDrawList();
            var rectMax = new Vector2(origin.X + size.X, origin.Y + size.Y);

            drawList.AddRectFilled(in origin, in rectMax, ColBg, 3.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
            drawList.AddRect(in origin, in rectMax, EGui.UIProxy.StyleConfig.Instance.PGItemBorderNormalColor,
                3.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);

            // 横向网格(1/4 分隔)
            for (int g = 1; g < 4; g++)
            {
                float gy = origin.Y + size.Y * g / 4.0f;
                var a = new Vector2(origin.X, gy);
                var b = new Vector2(rectMax.X, gy);
                drawList.AddLine(in a, in b, ColGrid, 1.0f);
            }

            // 中性线(乘子=1): 无论量程多少都按 value=1 定位
            if (1.0f >= mViewMin && 1.0f <= mViewMax)
            {
                var na = CurveToScreen(0.0f, 1.0f, in origin, in size);
                var nb = CurveToScreen(1.0f, 1.0f, in origin, in size);
                drawList.AddLine(in na, in nb, ColNeutral, 1.0f);
                var nTip = new Vector2(rectMax.X - 20, na.Y - 12);
                drawList.AddText(in nTip, ColNeutral, "x1", null);
            }

            // 命中区: 只在 interactive 时占交互; 不交互时也要叠一个以占布局空间
            var btnSize = size;
            if (interactive)
                ImGuiAPI.InvisibleButton("##CurveArea" + idSuffix, in btnSize,
                    ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft | ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonRight);
            else
                ImGuiAPI.Dummy(in btnSize);
            bool areaHovered = interactive && ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);
            var mouse = ImGuiAPI.GetMousePos();

            int keyCount = curve.KeyCount;
            if (keyCount >= 2)
            {
                for (int i = 0; i + 1 < keyCount; i++)
                {
                    var k0 = curve.GetKey(i);
                    var k1 = curve.GetKey(i + 1);
                    var p0 = CurveToScreen(k0.Time, k0.Value, in origin, in size);
                    var p1 = CurveToScreen(k1.Time, k1.Value, in origin, in size);
                    drawList.AddLine(in p0, in p1, ColLine, 2.0f);
                }
            }
            else if (keyCount == 1)
            {
                var k = curve.GetKey(0);
                var p0 = CurveToScreen(0, k.Value, in origin, in size);
                var p1 = CurveToScreen(1, k.Value, in origin, in size);
                drawList.AddLine(in p0, in p1, ColLine, 2.0f);
            }
            else
            {
                float v = Math.Clamp(1.0f, mViewMin, mViewMax);
                var p0 = CurveToScreen(0, v, in origin, in size);
                var p1 = CurveToScreen(1, v, in origin, in size);
                drawList.AddLine(in p0, in p1, ColGrid, 1.0f);
            }

            int hotKey = -1;
            for (int i = 0; i < keyCount; i++)
            {
                var k = curve.GetKey(i);
                var sp = CurveToScreen(k.Time, k.Value, in origin, in size);
                float dx = mouse.X - sp.X;
                float dy = mouse.Y - sp.Y;
                bool hot = interactive && (dx * dx + dy * dy) <= HitRadius * HitRadius;
                if (hot)
                    hotKey = i;
                drawList.AddCircleFilled(in sp, HandleRadius, (hot || (interactive && i == mDraggingKey)) ? ColKeyHot : ColKey, 12);
            }

            if (interactive && !readOnly)
            {
                bool leftDown = ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left);

                if (mDraggingKey < 0 && hotKey >= 0 &&
                    ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false))
                {
                    mDraggingKey = hotKey;
                }

                if (mDraggingKey >= 0)
                {
                    if (leftDown)
                    {
                        if (mDraggingKey < curve.KeyCount)
                        {
                            ScreenToCurve(in mouse, in origin, in size, out float t, out float v);
                            float lockedTime = curve.GetKey(mDraggingKey).Time;
                            curve.SetKey(mDraggingKey, lockedTime, v);
                            changed = true;
                        }
                    }
                    else
                    {
                        mDraggingKey = -1;
                    }
                }

                if (mDraggingKey < 0 && areaHovered && hotKey < 0 &&
                    ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    ScreenToCurve(in mouse, in origin, in size, out float t, out float v);
                    curve.AddKey(t, v);
                    changed = true;
                }

                // 右键删点。允许删到空: 空曲线才是"关闭"(Evaluate 返回 1.0), 剩一点不等于关闭。
                if (hotKey >= 0 &&
                    ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Right, false))
                {
                    curve.RemoveKey(hotKey);
                    mDraggingKey = -1;
                    changed = true;
                }
            }

            if (areaHovered)
            {
                var tip = "L-drag move / dbl-click add / R-click del (empty = off)";
                var tPos = new Vector2(origin.X + 4, origin.Y + 2);
                drawList.AddText(in tPos, ColGrid, tip, null);
            }
        }

        // 量程编辑行(Y 上下限), 内联与大窗共用。
        void DrawRangeRow(TtKawaiiCurve curve, float availWidth, string idSuffix, ref bool changed)
        {
            float editMin = mViewMin;
            float editMax = mViewMax;
            float boxWidth = (availWidth - 30) * 0.5f;
            if (boxWidth < 40)
                boxWidth = 40;

            ImGuiAPI.Text("Y");
            ImGuiAPI.SameLine(0, 4);
            ImGuiAPI.SetNextItemWidth(boxWidth);
            bool minChanged = ImGuiAPI.DragFloat("##CurveYMin" + idSuffix, ref editMin, 0.01f, -1000.0f, 1000.0f, "%.3f",
                ImGuiSliderFlags_.ImGuiSliderFlags_None);
            ImGuiAPI.SameLine(0, 4);
            ImGuiAPI.SetNextItemWidth(boxWidth);
            bool maxChanged = ImGuiAPI.DragFloat("##CurveYMax" + idSuffix, ref editMax, 0.01f, -1000.0f, 1000.0f, "%.3f",
                ImGuiSliderFlags_.ImGuiSliderFlags_None);

            if (minChanged || maxChanged)
            {
                if (editMax > editMin)
                {
                    curve.SetViewRange(editMin, editMax);
                    mViewMin = editMin;
                    mViewMax = editMax;
                    changed = true;
                }
            }
            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                EGui.Controls.CtrlUtility.DrawHelper("Y range of this curve (curve value is a multiplier; >1 amplifies)");
        }

        // 可缩放的大编辑窗: 大画布 + 量程行 + 关键点数值表(精确输入)。
        // 用真模态对话框 BeginPopupModal: 它是带标题栏的完整窗口(不带 popup 的
        // AlwaysAutoResize, 所以不会像普通 popup 那样被内容反馈擑大), 模态期间屏蔽
        // 背景交互, 开/关语义清晰; 比从 PG 单元格里 Begin 一个非模态浮窗更安全。
        unsafe void DrawBigEditor(TtKawaiiCurve curve, bool readOnly, ref bool changed)
        {
            var initSize = new Vector2(560, 460);
            ImGuiAPI.SetNextWindowSize(in initSize, ImGuiCond_.ImGuiCond_Appearing);

            // BeginPopupModal 与 BeginPopup 一样: 仅当返回 true 时才配 EndPopup。
            // ref mBigOpen: 标题栏 X 关闭会把它置 false。
            if (ImGuiAPI.BeginPopupModal(mBigId, ref mBigOpen, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                var avail = ImGuiAPI.GetContentRegionAvail();
                // 底部留给量程行 + 关键点表, 上方大画布。模态窗非自动扩, avail 稳定。
                float canvasH = avail.Y - 210;
                if (canvasH < 120)
                    canvasH = 120;
                float canvasW = avail.X;
                if (canvasW < 120)
                    canvasW = 120;
                var canvasOrigin = ImGuiAPI.GetCursorScreenPos();
                var canvasSize = new Vector2(canvasW, canvasH);
                DrawCanvas(curve, in canvasOrigin, in canvasSize, "Big", readOnly, true, ref changed);

                ImGuiAPI.Separator();
                if (!readOnly)
                    DrawRangeRow(curve, canvasW, "Big", ref changed);

                ImGuiAPI.Separator();
                ImGuiAPI.Text("Keys (Time / Value)");
                int keyCount = curve.KeyCount;
                int removeIndex = -1;
                for (int i = 0; i < keyCount; i++)
                {
                    var k = curve.GetKey(i);
                    float t = k.Time;
                    float v = k.Value;
                    ImGuiAPI.PushID("key" + i);
                    ImGuiAPI.SetNextItemWidth(140);
                    bool tChanged = ImGuiAPI.DragFloat("##T", ref t, 0.002f, 0.0f, 1.0f, "t %.3f",
                        ImGuiSliderFlags_.ImGuiSliderFlags_None);
                    ImGuiAPI.SameLine(0, 6);
                    ImGuiAPI.SetNextItemWidth(140);
                    bool vChanged = ImGuiAPI.DragFloat("##V", ref v, 0.01f, mViewMin, mViewMax, "v %.3f",
                        ImGuiSliderFlags_.ImGuiSliderFlags_None);
                    ImGuiAPI.SameLine(0, 6);
                    if (!readOnly && ImGuiAPI.SmallButton("X"))
                        removeIndex = i;
                    ImGuiAPI.PopID();

                    if (!readOnly && (tChanged || vChanged))
                    {
                        // SetKey 会重排, 数值表下一帧按新顺序重建, 索引变化可接受
                        curve.SetKey(i, t, v);
                        changed = true;
                    }
                }
                if (removeIndex >= 0)
                {
                    curve.RemoveKey(removeIndex);
                    mDraggingKey = -1;
                    changed = true;
                }

                if (!readOnly)
                {
                    var addSz = new Vector2(0, 0);
                    if (ImGuiAPI.Button("Add Key", in addSz))
                    {
                        // 默认加在链中、中性值 1.0
                        curve.AddKey(0.5f, Math.Clamp(1.0f, mViewMin, mViewMax));
                        changed = true;
                    }
                    ImGuiAPI.SameLine(0, 8);
                    if (ImGuiAPI.Button("Clear (off)", in addSz))
                    {
                        while (curve.KeyCount > 0)
                            curve.RemoveKey(0);
                        mDraggingKey = -1;
                        changed = true;
                    }
                    ImGuiAPI.SameLine(0, 8);
                    if (ImGuiAPI.Button("Close", in addSz))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        mBigOpen = false;
                    }
                }

                ImGuiAPI.EndPopup();
            }
        }

        Vector2 CurveToScreen(float time, float value, in Vector2 origin, in Vector2 size)
        {
            float x = origin.X + Math.Clamp(time, 0.0f, 1.0f) * size.X;
            float norm = (mViewMax > mViewMin) ? (value - mViewMin) / (mViewMax - mViewMin) : 0.0f;
            norm = Math.Clamp(norm, 0.0f, 1.0f);
            float y = origin.Y + (1.0f - norm) * size.Y;   // Y 轴向上
            return new Vector2(x, y);
        }

        void ScreenToCurve(in Vector2 screen, in Vector2 origin, in Vector2 size, out float time, out float value)
        {
            time = size.X > 1e-3f ? (screen.X - origin.X) / size.X : 0.0f;
            time = Math.Clamp(time, 0.0f, 1.0f);
            float norm = size.Y > 1e-3f ? 1.0f - (screen.Y - origin.Y) / size.Y : 0.0f;
            norm = Math.Clamp(norm, 0.0f, 1.0f);
            value = mViewMin + norm * (mViewMax - mViewMin);
        }
    }
}
