using System;
using System.Collections.Generic;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    /// <summary>
    /// 地形编辑的通道。两个通道共用同一套拾取 / 圆环 / 半径与软度,
    /// 写入对象不同 (高度图 vs 材质 ID 图)。
    /// </summary>
    public enum ETerrainEditChannel
    {
        Height,
        Material,
    }

    /// <summary>
    /// 地形笔刷的视口交互模式 (高度 / 材质双通道)。注册到某个 viewport 类型后会自动出现在
    /// InteractiveMode 下拉里 (SceneEditor 把这个下拉画在自己的工具栏上)。
    ///
    /// 与基类的分工: 左键落笔期间完全不走 base.OnEvent, 从而同时屏蔽掉
    /// TtAxis 的拖动响应和抬手时的 hitproxy 选中; 其余事件 (Alt+左键转视角、
    /// 中键平移、滚轮推进、WASD) 原样交回基类, 编辑地形时相机操作照常可用。
    /// </summary>
    public class TtTerrainEditorInteractiveMode : EGui.Slate.TtWorldViewportInteractiveMode
    {
        /// <summary>
        /// ray marching 的步数上限。朝天的射线会被地形 AABB 直接剔掉, 但沿地表方向的
        /// 掠射线可能在盒内跑很长距离, 需要这个上限兜底 (超了就自动放大步长)。
        /// </summary>
        const int MaxMarchSteps = 4096;
        /// <summary>
        /// 命中区间的二分细化次数。步长是一个 texel, 8 次二分足够细到亚 texel。
        /// </summary>
        const int RefineIterations = 8;
        const int RingSegments = 48;

        /// <summary>
        /// 与 TtAxis.HistoryHost 同名同语义: 由宿主编辑器在初始化时赋上自己的历史栈。
        /// 为 null 时地形编辑仍然可用, 只是不进 undo (例如 MCP / 无宿主的视口)。
        /// </summary>
        public Editor.Infrastructure.TtEditorHistory HistoryHost;

        FTerrainBrushParam mBrush = FTerrainBrushParam.Default;
        /// <summary>
        /// Raise/Lower 的 Strength 是世界单位的高度增量, Flatten/Smooth 的是 0~1 的混合权重,
        /// 两者量级完全不同。分开存, 切换工具时不会把对方的值带过去变成荒谬的默认值。
        /// </summary>
        float mStrengthWorld = 0.5f;
        float mStrengthBlend = 0.5f;

        readonly TtTerrainHeightDragRecorder mRecorder = new TtTerrainHeightDragRecorder();
        readonly TtTerrainMaterialIdDragRecorder mMaterialRecorder = new TtTerrainMaterialIdDragRecorder();

        /// <summary>
        /// 当前编辑通道。Radius / Falloff 两通道共用 mBrush 里的那两个值 (同一个视口圆环,
        /// 手感一致, 也少两个滑条), 其余参数各自独立。
        /// </summary>
        ETerrainEditChannel mChannel = ETerrainEditChannel.Height;
        /// <summary>
        /// 材质通道要刷的 ID = UTerrainMaterialIdManager.MaterialIdArray 的下标。
        /// </summary>
        byte mPaintMaterialId = 0;
        /// <summary>
        /// 本次进入模式以来 BeginEditSession 过的 level, 离开模式时要逐个 EndEditSession
        /// 把高度纹理的编码基准收回到真实范围 (会话期间是拉开 headroom 的低精度编码)。
        /// </summary>
        readonly List<UTerrainLevelData> mSessionLevels = new List<UTerrainLevelData>();

        bool mIsPainting = false;
        bool mHasHover = false;
        DVector3 mHoverPos = DVector3.Zero;
        /// <summary>
        /// 本笔是否反向: 高度通道下是 Raise↔Lower 互换, 材质通道下是“本笔是擦除”。
        /// 在 MOUSEBUTTONDOWN 时锁定, 不跟随拖动中的 Shift 抵起 —— 否则一笔里会出现
        /// 一半抬一半压 (或一半刷一半擦) 的鬼影。
        /// </summary>
        bool mInvertStroke = false;

        #region ModeLifetime
        public override void OnEnterMode()
        {
            base.OnEnterMode();
            mIsPainting = false;
            mHasHover = false;
            mInvertStroke = false;
            mRecorder.Reset();
            mMaterialRecorder.Reset();
        }
        public override void OnLeaveMode()
        {
            // 先把还没封口的那一段拖动变成历史记录 (两个通道的记录器都会封), 再收会话 ——
            // 顺序反了的话 EndEditSession 里的整层重建会把 after 快照的取值时机搞乱。
            CommitCommand();
            mIsPainting = false;
            mHasHover = false;

            for (int i = 0; i < mSessionLevels.Count; i++)
            {
                mSessionLevels[i].EndEditSession();
            }
            mSessionLevels.Clear();

            base.OnLeaveMode();
        }
        #endregion

        #region Event
        public override bool OnEvent(in Bricks.Input.Event e)
        {
            var worldViewport = WorldViewport;
            if (worldViewport == null)
                return true;

            bool altDown = TtEngine.Instance.InputSystem.IsKeyDown(Bricks.Input.Keycode.KEY_LALT);
            bool isLeftButton = e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT;

            if (e.Type == Bricks.Input.EventType.MOUSEBUTTONDOWN)
            {
                if (isLeftButton && altDown == false && CanStartPaint(worldViewport, e.MouseMotion.X, e.MouseMotion.Y))
                {
                    var mousePt = new Vector2(e.MouseMotion.X, e.MouseMotion.Y);
                    if (TryPickTerrain(in mousePt, out var terrain, out var hitPos))
                    {
                        mIsPainting = true;
                        mInvertStroke = IsShiftDown();
                        mHasHover = true;
                        mHoverPos = hitPos;
                        Stroke(terrain, in hitPos);
                        return true;
                    }
                }
            }
            else if (e.Type == Bricks.Input.EventType.MOUSEMOTION)
            {
                if (mIsPainting && isLeftButton)
                {
                    var mousePt = new Vector2(e.MouseMotion.X, e.MouseMotion.Y);
                    if (TryPickTerrain(in mousePt, out var terrain, out var hitPos))
                    {
                        mHasHover = true;
                        mHoverPos = hitPos;
                        Stroke(terrain, in hitPos);
                    }
                    return true;
                }
            }
            else if (e.Type == Bricks.Input.EventType.MOUSEBUTTONUP)
            {
                if (mIsPainting && isLeftButton)
                {
                    EndStroke();
                    return true;
                }
            }

            return base.OnEvent(in e);
        }
        /// <summary>
        /// 落笔前的准入检查, 判据与基类里 hitproxy 选中的那组完全一致 ——
        /// 鼠标在视口内、没在操作 gizmo、没压在视口上层的按钮/面板上。
        /// </summary>
        static bool CanStartPaint(EGui.Slate.TtWorldViewportSlate worldViewport, float mouseX, float mouseY)
        {
            if (worldViewport.IsMouseIn == false || worldViewport.UIOperated)
                return false;
            if (worldViewport.Axis != null && worldViewport.Axis.CurrentAxisType != GamePlay.TtAxis.enAxisType.Null)
                return false;
            var viewportPoint = new Vector2(mouseX, mouseY) + worldViewport.ViewportPos;
            return worldViewport.PointInOverlappedArea(in viewportPoint) == false;
        }
        #endregion

        #region Picking
        TtTerrainNode FindTerrain()
        {
            var scene = WorldViewport?.World?.Root;
            if (scene == null)
                return null;
            return scene.FindFirstChild<TtTerrainNode>(null, true);
        }
        /// <summary>
        /// 鼠标位置 (视口内坐标, 与 Bricks.Input.Event.MouseMotion 同一套) 打到地形上。
        /// </summary>
        bool TryPickTerrain(in Vector2 mousePt, out TtTerrainNode terrain, out DVector3 hitPos)
        {
            terrain = null;
            hitPos = DVector3.Zero;

            var worldViewport = WorldViewport;
            if (worldViewport == null)
                return false;
            var camera = worldViewport.CameraController?.Camera;
            if (camera == null)
                return false;
            var node = FindTerrain();
            if (node == null)
                return false;

            var start = camera.GetPosition();
            var dir = Vector3.Zero;
            var msPt = worldViewport.Window2Viewport(mousePt);
            camera.GetPickRay(ref dir, msPt.X, msPt.Y, worldViewport.ClientSize.X, worldViewport.ClientSize.Y);
            dir.Normalize();

            if (MarchTerrain(node, in start, in dir, out hitPos) == false)
                return false;
            terrain = node;
            return true;
        }
        /// <summary>
        /// 沿射线步进求与地形表面的交点。
        ///
        /// 不用物理 raycast 是因为: 编辑器态的权威高度源是 SourceHeightMap,
        /// physx heightfield 是它的 short 量化派生物, 而且未必已经 cook 出来。
        /// </summary>
        bool MarchTerrain(TtTerrainNode node, in DVector3 start, in Vector3 dir, out DVector3 hitPos)
        {
            hitPos = DVector3.Zero;
            if (node.Levels == null || node.LevelSize <= 0.0f)
                return false;

            var origin = node.Placement.AbsTransform.Position;
            double sizeX = (double)node.NumOfLevelX * node.LevelSize;
            double sizeZ = (double)node.NumOfLevelZ * node.LevelSize;
            GetLoadedHeightRange(node, out var loY, out var hiY);

            var boxMin = new DVector3(origin.X, origin.Y + loY, origin.Z);
            var boxMax = new DVector3(origin.X + sizeX, origin.Y + hiY, origin.Z + sizeZ);
            if (IntersectRayBox(in start, in dir, in boxMin, in boxMax, out var tMin, out var tMax) == false)
                return false;

            double step = node.GridSize > 0.0f ? node.GridSize : 1.0;
            double span = tMax - tMin;
            if (span <= 0.0)
                return false;
            if (span / step > MaxMarchSteps)
                step = span / MaxMarchSteps;

            var dirD = dir.AsDVector();
            bool hasPrev = false;
            double prevT = tMin;
            for (double t = tMin; ; t += step)
            {
                if (t > tMax)
                    t = tMax;

                var p = start + dirD * t;
                double h = node.GetEditAltitudeAtWorld(in p);
                if (h == double.MinValue)
                {
                    // 这一段没有已加载 (或不可编辑) 的 level, 不能与前一个有效样本配成区间。
                    hasPrev = false;
                }
                else if (p.Y - h <= 0.0)
                {
                    if (hasPrev == false)
                    {
                        // 相机就在地表以下, 没有符号翻转可用, 直接取当前点。
                        hitPos = p;
                        return true;
                    }
                    hitPos = RefineHit(node, in start, in dirD, prevT, t);
                    return true;
                }
                else
                {
                    hasPrev = true;
                    prevT = t;
                }

                if (t >= tMax)
                    break;
            }
            return false;
        }
        /// <summary>
        /// 在 [tAbove, tBelow] 区间上二分细化命中点。
        /// </summary>
        static DVector3 RefineHit(TtTerrainNode node, in DVector3 start, in DVector3 dir, double tAbove, double tBelow)
        {
            for (int i = 0; i < RefineIterations; i++)
            {
                double mid = (tAbove + tBelow) * 0.5;
                var p = start + dir * mid;
                double h = node.GetEditAltitudeAtWorld(in p);
                if (h == double.MinValue)
                    break;
                if (p.Y - h > 0.0)
                    tAbove = mid;
                else
                    tBelow = mid;
            }
            return start + dir * ((tAbove + tBelow) * 0.5);
        }
        /// <summary>
        /// 取已加载 level 的高度范围作 Y 方向的 slab。没有任何 level 加载出来时给一个
        /// 兜底的大范围, 让射线照常步进 (步进时的无效样本会自己被跳过)。
        /// </summary>
        static void GetLoadedHeightRange(TtTerrainNode node, out double loY, out double hiY)
        {
            float lo = float.MaxValue;
            float hi = float.MinValue;
            var levels = node.ActiveLevels;
            if (levels != null)
            {
                for (int i = 0; i < levels.GetLength(0); i++)
                {
                    for (int j = 0; j < levels.GetLength(1); j++)
                    {
                        var data = levels[i, j]?.LevelData;
                        if (data == null)
                            continue;
                        if (data.HeightMapMinHeight < lo)
                            lo = data.HeightMapMinHeight;
                        if (data.HeightMapMaxHeight > hi)
                            hi = data.HeightMapMaxHeight;
                    }
                }
            }
            if (lo > hi)
            {
                loY = -10000.0;
                hiY = 10000.0;
                return;
            }
            // 编辑会话期间 min/max 是拉开 headroom 的, 这里再留一点余量,
            // 防止贴着地表的掠射线被 slab 剔掉。
            loY = lo - 1.0;
            hiY = hi + 1.0;
        }
        static bool IntersectRayBox(in DVector3 start, in Vector3 dir, in DVector3 boxMin, in DVector3 boxMax, out double tMin, out double tMax)
        {
            tMin = 0.0;
            tMax = double.MaxValue;
            for (int axis = 0; axis < 3; axis++)
            {
                double o = axis == 0 ? start.X : (axis == 1 ? start.Y : start.Z);
                double d = axis == 0 ? dir.X : (axis == 1 ? dir.Y : dir.Z);
                double lo = axis == 0 ? boxMin.X : (axis == 1 ? boxMin.Y : boxMin.Z);
                double hi = axis == 0 ? boxMax.X : (axis == 1 ? boxMax.Y : boxMax.Z);
                if (Math.Abs(d) < 1e-8)
                {
                    if (o < lo || o > hi)
                        return false;
                    continue;
                }
                double t1 = (lo - o) / d;
                double t2 = (hi - o) / d;
                if (t1 > t2)
                {
                    double tmp = t1;
                    t1 = t2;
                    t2 = tmp;
                }
                if (t1 > tMin)
                    tMin = t1;
                if (t2 < tMax)
                    tMax = t2;
                if (tMin > tMax)
                    return false;
            }
            return true;
        }
        #endregion

        #region Stroke
        static bool IsShiftDown()
        {
            var input = TtEngine.Instance.InputSystem;
            return input.IsKeyDown(Bricks.Input.Keycode.KEY_LSHIFT) || input.IsKeyDown(Bricks.Input.Keycode.KEY_RSHIFT);
        }
        /// <summary>
        /// 本笔是不是“反向”。没落笔时取实时键盘状态, 落笔中用锁定值,
        /// 这样圆环颜色能提前预告方向。
        /// </summary>
        bool IsInvertStroke()
        {
            return mIsPainting ? mInvertStroke : IsShiftDown();
        }
        /// <summary>
        /// Strength 滑条只给正值 (负的 Raise 强度读起来很反直觉), 压下去靠 Shift 反向
        /// —— 这是地形/油漆类笔刷的通行约定。Flatten/Smooth 本身无"方向"可言, 不反。
        /// </summary>
        ETerrainBrushTool GetEffectiveTool()
        {
            bool invert = IsInvertStroke();
            if (invert == false)
                return mBrush.Tool;
            if (mBrush.Tool == ETerrainBrushTool.Raise)
                return ETerrainBrushTool.Lower;
            if (mBrush.Tool == ETerrainBrushTool.Lower)
                return ETerrainBrushTool.Raise;
            return mBrush.Tool;
        }
        FTerrainBrushParam MakeBrushParam()
        {
            var param = mBrush;
            param.Tool = GetEffectiveTool();
            param.Strength = IsBlendTool(param.Tool) ? mStrengthBlend : mStrengthWorld;
            return param;
        }
        /// <summary>
        /// Flatten/Smooth 的 Strength 是"向目标值插值的权重", 语义与 Raise/Lower 的
        /// "高度增量"完全不同, UI 与默认值都要按这个分流。
        /// </summary>
        internal static bool IsBlendTool(ETerrainBrushTool tool)
        {
            return tool == ETerrainBrushTool.Flatten || tool == ETerrainBrushTool.Smooth;
        }
        FTerrainMaterialBrushParam MakeMaterialParam()
        {
            var param = FTerrainMaterialBrushParam.Default;
            param.MaterialId = mPaintMaterialId;
            // 两通道共用半径与软度: 视口里就一个圆环, 分开存只会让它跟不上手感。
            param.Radius = mBrush.Radius;
            param.Falloff = mBrush.Falloff;
            param.bErase = IsInvertStroke();
            return param;
        }
        void Stroke(TtTerrainNode terrain, in DVector3 worldPos)
        {
            if (mChannel == ETerrainEditChannel.Material)
            {
                StrokeMaterial(terrain, in worldPos);
                return;
            }
            StrokeHeight(terrain, in worldPos);
        }
        void StrokeHeight(TtTerrainNode terrain, in DVector3 worldPos)
        {
            var levelData = terrain.GetLevelDataAtWorld(in worldPos, out var localX, out var localZ);
            if (levelData == null || levelData.IsEditable == false)
                return;

            if (levelData.IsInEditSession == false)
            {
                levelData.BeginEditSession();
            }
            if (mSessionLevels.Contains(levelData) == false)
            {
                mSessionLevels.Add(levelData);
            }

            var param = MakeBrushParam();
            var strokeRect = levelData.CalcBrushRect(localX, localZ, in param);
            if (strokeRect.IsValid == false)
                return;

            if (mRecorder.PreStroke(levelData, in strokeRect))
            {
                // 拖过了 level 边界: 一条命令只描述一个 level 的一块矩形,
                // 先把上一段封成命令再从新 level 重新开始记录。
                CommitCommand();
                mRecorder.PreStroke(levelData, in strokeRect);
            }

            levelData.ApplyHeightBrush(localX, localZ, in param);
            // 拖动过程中不重 cook physx heightfield (整层 cook 是 1024² 量级, 每帧做不起),
            // 只做 GPU 局部上传; 物理留到抬手时补一次。
            levelData.FlushDirty(false);
        }
        /// <summary>
        /// 材质通道不调 BeginEditSession (ID 图是 R8 原值, 没有 min/max 编码基准可冻结),
        /// 也不进 mSessionLevels; FlushMaterialIdDirty 只做局部上传 + RVT 标脏, 没有法线重算
        /// 也没有物理重建, 每帧调没问题。
        /// </summary>
        void StrokeMaterial(TtTerrainNode terrain, in DVector3 worldPos)
        {
            var levelData = terrain.GetLevelDataAtWorld(in worldPos, out var localX, out var localZ);
            if (levelData == null || levelData.IsMaterialIdEditable == false)
                return;

            var param = MakeMaterialParam();
            var strokeRect = levelData.CalcMaterialBrushRect(localX, localZ, in param);
            if (strokeRect.IsValid == false)
                return;

            if (mMaterialRecorder.PreStroke(levelData, in strokeRect))
            {
                // 拖过了 level 边界: 一条命令只描述一个 level 的一块矩形。
                CommitCommand();
                mMaterialRecorder.PreStroke(levelData, in strokeRect);
            }

            levelData.ApplyMaterialBrush(localX, localZ, in param);
            levelData.FlushMaterialIdDirty();
        }
        void EndStroke()
        {
            if (mChannel == ETerrainEditChannel.Material)
            {
                // 材质通道没有抬手补物理这一步 —— ID 图不参与碰撞。
                CommitCommand();
                mIsPainting = false;
                return;
            }

            var levelData = mRecorder.LevelData;
            // 先封口再清 mIsPainting: GetEffectiveTool 靠 mIsPainting 判定该用锁定值还是
            // 实时键盘状态, 顺序反了命令名会跟着抬手瞬间的 Shift 状态乱跳。
            CommitCommand();
            mIsPainting = false;
            // 抬手补物理。此时脏区已经被拖动中的 flush 清空了, 但 FlushDirty(true)
            // 在脏区为空时仍会走到物理重建, 正是这里需要的。
            levelData?.FlushDirty(true);
        }
        /// <summary>
        /// 两个通道的记录器都封一次口 —— 没在记录的那个 BuildCommand 会返回 null。
        /// 离开模式 / 切通道 / 跨 level 都靠这个统一收尾。
        /// </summary>
        void CommitCommand()
        {
            // 名字用实际生效的工具, 否则 Shift 压下去的一笔在 History 里依旧写着 Raise。
            PushCommand(mRecorder.BuildCommand($"Terrain {GetEffectiveTool()}"));
            PushCommand(mMaterialRecorder.BuildCommand(MakeMaterialCommandName()));
        }
        void PushCommand(Editor.Infrastructure.TtEditorCommand cmd)
        {
            if (cmd == null || HistoryHost == null)
                return;
            // 数据已经在外部改完了, 只入栈记录; 一次拖动就是一步, 立刻封口不参与合并。
            HistoryHost.PushCommand(cmd);
            cmd.Seal();
        }
        string MakeMaterialCommandName()
        {
            if (IsInvertStroke())
                return "Terrain Erase Material";
            return $"Terrain Paint {GetMaterialDisplayName(mPaintMaterialId)}";
        }
        /// <summary>
        /// 材质列表可能根本没配 (地形没有 MatIdMapping 节点), 拿不到名字时退回下标。
        /// </summary>
        string GetMaterialDisplayName(int index)
        {
            var list = FindTerrain()?.TerrainMaterialIdManager?.MaterialIdArray;
            if (list == null || index < 0 || index >= list.Count)
                return index.ToString();
            var tex = list[index].TexDiffuse;
            return tex != null ? tex.PureName : index.ToString();
        }
        #endregion

        #region UI
        /// <summary>
        /// 视口里只画贴合地形的笔刷圆环。参数 UI 走宿主编辑器的停靠面板
        /// (见 TtSceneEditor.DrawTerrainBrush), 挤在视口左上角既放不下标签也容易误点。
        /// </summary>
        public override Vector2 OnDrawViewportUI(in Vector2 startDrawPos)
        {
            if (WorldViewport == null)
                return Vector2.Zero;

            UpdateHoverForPreview();
            DrawBrushRing();
            return Vector2.Zero;
        }
        /// <summary>
        /// 笔刷参数 UI。由宿主编辑器在自己的停靠面板里调用, 面板宽度不确定,
        /// 所以控件宽度全用负值 (= 右边界回退这么多像素留给标签)。
        /// </summary>
        public void DrawBrushParams()
        {
            const float LabelWidth = -80.0f;

            ImGuiAPI.SetNextItemWidth(LabelWidth);
            if (ImGuiAPI.BeginCombo("Channel", mChannel.ToString(), ImGuiComboFlags_.ImGuiComboFlags_None))
            {
                var channels = Enum.GetValues<ETerrainEditChannel>();
                for (int i = 0; i < channels.Length; i++)
                {
                    var channel = channels[i];
                    if (ImGuiAPI.Selectable(channel.ToString(), channel == mChannel, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero)
                        && channel != mChannel)
                    {
                        // 切通道等于抬手: 半段拖动必须先封口, 否则这段编辑会永久进不了 undo 栈。
                        CommitCommand();
                        mChannel = channel;
                    }
                }
                ImGuiAPI.EndCombo();
            }
            ImGuiAPI.Separator();

            if (mChannel == ETerrainEditChannel.Material)
            {
                DrawMaterialBrushParams(LabelWidth);
                return;
            }
            DrawHeightBrushParams(LabelWidth);
        }
        void DrawHeightBrushParams(float LabelWidth)
        {
            ImGuiAPI.SetNextItemWidth(LabelWidth);
            if (ImGuiAPI.BeginCombo("Tool", mBrush.Tool.ToString(), ImGuiComboFlags_.ImGuiComboFlags_None))
            {
                var tools = Enum.GetValues<ETerrainBrushTool>();
                for (int i = 0; i < tools.Length; i++)
                {
                    var tool = tools[i];
                    if (ImGuiAPI.Selectable(tool.ToString(), tool == mBrush.Tool, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                    {
                        mBrush.Tool = tool;
                    }
                }
                ImGuiAPI.EndCombo();
            }

            ImGuiAPI.SetNextItemWidth(LabelWidth);
            ImGuiAPI.SliderFloat("Radius", ref mBrush.Radius, 1.0f, 200.0f, "%.1f", ImGuiSliderFlags_.ImGuiSliderFlags_None);

            ImGuiAPI.SetNextItemWidth(LabelWidth);
            if (IsBlendTool(mBrush.Tool))
            {
                ImGuiAPI.SliderFloat("Strength", ref mStrengthBlend, 0.0f, 1.0f, "%.3f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                    ImGuiAPI.SetTooltip($"{mBrush.Tool} 的 Strength 是 0~1 的混合权重");
            }
            else
            {
                ImGuiAPI.SliderFloat("Strength", ref mStrengthWorld, 0.0f, 10.0f, "%.3f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                    ImGuiAPI.SetTooltip($"{mBrush.Tool} 的 Strength 是每次落笔的高度增量(米)");
            }

            ImGuiAPI.SetNextItemWidth(LabelWidth);
            ImGuiAPI.SliderFloat("Falloff", ref mBrush.Falloff, 0.0f, 1.0f, "%.2f", ImGuiSliderFlags_.ImGuiSliderFlags_None);

            if (mBrush.Tool == ETerrainBrushTool.Flatten)
            {
                ImGuiAPI.SetNextItemWidth(-130.0f);
                ImGuiAPI.SliderFloat("Target", ref mBrush.TargetHeight, -500.0f, 500.0f, "%.2f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                ImGuiAPI.SameLine(0, -1);
                if (ImGuiAPI.Button("Pick", in Vector2.Zero))
                {
                    PickTargetHeightFromHover();
                }
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                    ImGuiAPI.SetTooltip("取鼠标当前所指位置的高度填进 Target");
            }

            if (mBrush.Tool == ETerrainBrushTool.Raise || mBrush.Tool == ETerrainBrushTool.Lower)
            {
                var effective = GetEffectiveTool();
                if (effective != mBrush.Tool)
                    ImGuiAPI.Text($"Shift 反向中 -> {effective}");
                else
                    ImGuiAPI.Text("按住 Shift 反向落笔");
            }

            if (mHasHover)
                ImGuiAPI.Text($"Cursor: ({mHoverPos.X:F1}, {mHoverPos.Y:F1}, {mHoverPos.Z:F1})");
            else
                ImGuiAPI.Text("Cursor: --");
        }
        /// <summary>
        /// 材质通道的参数 UI。没有 Tool 下拉 (只有“刷”一个动作, 擦除走 Shift), 也没有
        /// Strength (ID 是整数下标, 不存在“刷多少”这回事)。
        /// </summary>
        void DrawMaterialBrushParams(float LabelWidth)
        {
            var list = FindTerrain()?.TerrainMaterialIdManager?.MaterialIdArray;
            if (list == null || list.Count == 0)
            {
                ImGuiAPI.Text("地形没有 MatIdMapping 节点或材质列表为空");
            }
            else
            {
                ImGuiAPI.Text("Material");
                for (int i = 0; i < list.Count; i++)
                {
                    var tex = list[i].TexDiffuse;
                    var text = $"[{i}] {(tex != null ? tex.PureName : "<none>")}";
                    if (ImGuiAPI.Selectable(text, i == mPaintMaterialId, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                    {
                        mPaintMaterialId = (byte)i;
                    }
                }
                ImGuiAPI.Separator();
            }

            ImGuiAPI.SetNextItemWidth(LabelWidth);
            ImGuiAPI.SliderFloat("Radius", ref mBrush.Radius, 1.0f, 200.0f, "%.1f", ImGuiSliderFlags_.ImGuiSliderFlags_None);

            ImGuiAPI.SetNextItemWidth(LabelWidth);
            ImGuiAPI.SliderFloat("Falloff", ref mBrush.Falloff, 0.0f, 1.0f, "%.2f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                ImGuiAPI.SetTooltip("材质是整数 ID, 无法插值; Falloff 越大, 边缘的散点抖动带越宽");

            if (IsInvertStroke())
                ImGuiAPI.Text("Shift 擦除中 -> 回基底材质");
            else
                ImGuiAPI.Text("按住 Shift 擦除回基底");

            if (mHasHover)
                ImGuiAPI.Text($"Cursor: ({mHoverPos.X:F1}, {mHoverPos.Y:F1}, {mHoverPos.Z:F1})");
            else
                ImGuiAPI.Text("Cursor: --");
        }
        /// <summary>
        /// TargetHeight 是 level 内相对高度 (与 SourceHeightMap 同基准), 手填很难对上,
        /// 给个从鼠标当前位置取值的按钮。
        /// </summary>
        void PickTargetHeightFromHover()
        {
            if (mHasHover == false)
                return;
            var node = FindTerrain();
            if (node == null)
                return;
            var levelData = node.GetLevelDataAtWorld(in mHoverPos, out var localX, out var localZ);
            if (levelData == null)
                return;
            float local = levelData.GetSourceAltitude(localX, localZ, node.GridSize);
            if (local == float.MinValue)
                return;
            mBrush.TargetHeight = local;
        }
        /// <summary>
        /// 落笔时 mHoverPos 由事件流更新; 没落笔时这里每帧补一次, 圆环才能跟着鼠标走。
        /// </summary>
        void UpdateHoverForPreview()
        {
            if (mIsPainting)
                return;

            var worldViewport = WorldViewport;
            if (worldViewport == null || worldViewport.IsMouseIn == false)
            {
                mHasHover = false;
                return;
            }

            var mouse = TtEngine.Instance.InputSystem.Mouse;
            var mousePt = new Vector2(mouse.EventMouseX, mouse.EventMouseY) - worldViewport.ViewportPos;
            mHasHover = TryPickTerrain(in mousePt, out _, out mHoverPos);
        }
        /// <summary>
        /// 沿笔刷圆周采样, 每点取地形高度得到世界坐标再投影到屏幕, 画出贴合起伏的圆环。
        /// </summary>
        void DrawBrushRing()
        {
            if (mHasHover == false)
                return;
            var worldViewport = WorldViewport;
            var camera = worldViewport?.CameraController?.Camera;
            if (camera == null)
                return;
            var node = FindTerrain();
            if (node == null)
                return;

            var coreCamera = camera.mCoreObject;
            var cameraOffset = coreCamera.GetMatrixStartPosition();
            var c2vMat = coreCamera.GetToViewPortMatrix();
            var camPos = camera.GetPosition();
            var camDir = coreCamera.GetDirection();

            var winPos = ImGuiAPI.GetWindowPos();
            var vpMin = ImGuiAPI.GetWindowContentRegionMin();
            var drawOffset = new Vector2(winPos.X + vpMin.X, winPos.Y + vpMin.Y);

            var cmdlst = ImGuiAPI.GetWindowDrawList();
            uint color = GetRingColor();

            bool hasFirst = false;
            var firstPt = Vector2.Zero;
            var prevPt = Vector2.Zero;
            bool hasPrev = false;

            for (int i = 0; i < RingSegments; i++)
            {
                double angle = Math.PI * 2.0 * i / RingSegments;
                var p = mHoverPos;
                p.X += Math.Cos(angle) * mBrush.Radius;
                p.Z += Math.Sin(angle) * mBrush.Radius;

                double h = node.GetEditAltitudeAtWorld(in p);
                if (h != double.MinValue)
                    p.Y = h;

                // 相机背后的点投影出来是镜像的假位置, 必须断开而不是硬连线。
                var toPt = p - camPos;
                if (toPt.X * camDir.X + toPt.Y * camDir.Y + toPt.Z * camDir.Z <= 0.0)
                {
                    hasPrev = false;
                    continue;
                }

                DVector3.TransformCoordinate(in p, in cameraOffset, in c2vMat, out var screenPos);
                var pt = new Vector2((float)screenPos.X, (float)screenPos.Y) + drawOffset;

                if (hasFirst == false)
                {
                    hasFirst = true;
                    firstPt = pt;
                }
                if (hasPrev)
                {
                    cmdlst.AddLine(in prevPt, pt, color, 1.5f);
                }
                prevPt = pt;
                hasPrev = true;
            }
            if (hasFirst && hasPrev)
            {
                cmdlst.AddLine(in prevPt, firstPt, color, 1.5f);
            }
        }
        /// <summary>
        /// 不用看面板就知道落笔会发生什么: 高度通道黄 = 抬 / 青 = 压,
        /// 材质通道洋红 = 刷 / 白 = 擦除。未落笔时跟随实时 Shift 预告。
        /// </summary>
        uint GetRingColor()
        {
            if (mChannel == ETerrainEditChannel.Material)
            {
                return IsInvertStroke()
                    ? (uint)Color4b.White.ToR8G8B8A8()
                    : (uint)Color4b.Magenta.ToR8G8B8A8();
            }
            return GetEffectiveTool() == ETerrainBrushTool.Lower
                ? (uint)Color4b.Cyan.ToR8G8B8A8()
                : (uint)Color4b.Yellow.ToR8G8B8A8();
        }
        #endregion
    }
}
