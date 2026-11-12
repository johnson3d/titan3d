using System;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    /// <summary>
    /// TerrainNode Details 面板上 "Levels" 按钮弹出的 level 网格视图:
    /// 用 ImGui table 展示 NumOfLevelZ x NumOfLevelX 的全部 level (行=Z, 列=X),
    /// cell 前缀标注状态, 左键点击或右键菜单 Goto 把场景视口相机拉到该 level。
    ///
    /// cell 状态前缀:
    ///   '*' 活动 level (ActiveLevels 里已挂载, 周围会被流式加载)
    ///   '~' 正在 VGDag bake
    ///   '+' LevelData 激活可用
    ///   'L' LevelData 异步加载中
    ///   'W' LevelData 等待销毁 (延迟卸载队列中倒计时, 仍可捞回来直接复用)
    ///   'D' LevelData 正在销毁 (Dispose 中)
    ///   '-' 已彻底销毁 (LevelData == null, 不在任何队列)
    ///
    /// 纯只读视图 + 相机导航 (仅右键菜单 Goto), 不写地形数据, 因此不参与 Undo / 序列化。
    /// 框色: 红 = 活动中心 (ActiveCenterLevelIndex), 绿 = 活动窗口内其余被流式激活的 level。
    /// </summary>
    public static class TtTerrainLevelGridEditor
    {
        const float CellWidth = 72.0f;
        const float GridHeight = 320.0f;

        public static void Draw(TtTerrainNode node)
        {
            if (node == null)
                return;

            // 行列数优先取真实数组维度: 参数改了但地形还没重建时 NumOfLevelX/Z 会先变,
            // 用它们遍历会越界。地形未建立时回退到配置值 (只显示空网格)。
            int countZ = node.Levels?.GetLength(0) ?? node.NumOfLevelZ;
            int countX = node.Levels?.GetLength(1) ?? node.NumOfLevelX;
            if (countX <= 0 || countZ <= 0)
            {
                ImGuiAPI.TextDisabled("Terrain has no level.");
                return;
            }

            var origin = node.Placement.AbsTransform.mPosition;
            ImGuiAPI.Text($"Levels {countX} x {countZ}, LevelSize = {node.LevelSize:0.##}");
            ImGuiAPI.TextDisabled($"Node pos = ({origin.X:0.##}, {origin.Y:0.##}, {origin.Z:0.##})");
            // 图例太长会把弹窗顶宽, 缩成短提示, 悬停时用 tooltip 展开完整图例
            ImGuiAPI.TextDisabled("Goto via right-click menu.  hover here for legend");
            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
            {
                ImGuiAPI.SetTooltip(
                    "Cells:  * active   ~ baking\n" +
                    "        + loaded   L loading\n" +
                    "        W wait-destroy   D destroying   - empty\n" +
                    "Frames: red = active center\n" +
                    "        green = streamed active");
            }

            // 状态读数: 相机位置 / 相机所在 level 索引 / 活动中心, 用于诊断 goto 行为
            // (goto 目标不是活动中心时, 下帧 SetActiveCenter 会把整个活动窗口拉过去, 红框应跟着跳)
            var center = node.ActiveCenterLevelIndex;
            var slate = node.GetWorld()?.ViewportSlate as EGui.Slate.TtWorldViewportSlate;
            var cam = slate?.CameraController?.Camera;
            if (cam != null)
            {
                var eyePos = cam.GetPosition();
                var camLevel = node.GetLevelIndex(in eyePos);
                ImGuiAPI.Text($"Eye=({eyePos.X:0.#}, {eyePos.Y:0.#}, {eyePos.Z:0.#})  EyeLevel=({camLevel.X},{camLevel.Y})  Center=({center.X},{center.Y})");
                if (camLevel != center)
                    ImGuiAPI.TextColored(new Vector4(1.0f, 0.6f, 0.2f, 1.0f), "Eye level != active center, streaming window will jump next tick");
            }
            else
            {
                ImGuiAPI.Text($"Center=({center.X},{center.Y}) (no viewport camera)");
            }

            var tableFlags = ImGuiTableFlags_.ImGuiTableFlags_RowBg |
                             ImGuiTableFlags_.ImGuiTableFlags_Borders |
                             ImGuiTableFlags_.ImGuiTableFlags_ScrollX |
                             ImGuiTableFlags_.ImGuiTableFlags_ScrollY;
            var tableSize = new Vector2(CellWidth * Math.Min(countX, 12) + 72.0f, GridHeight);
            if (!ImGuiAPI.BeginTable("TerrainLevelGrid", countX + 1, tableFlags, in tableSize, 0.0f))
                return;

            // 冻结首行(X 轴索引)与首列(Z 轴索引), 大范围 level 滚动时仍能定位
            ImGuiAPI.TableSetupScrollFreeze(1, 1);
            ImGuiAPI.TableSetupColumn("Z\\X", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 56.0f, 0);
            for (int x = 0; x < countX; x++)
                ImGuiAPI.TableSetupColumn(x.ToString(), ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, CellWidth, 0);
            ImGuiAPI.TableHeadersRow();

            for (int z = 0; z < countZ; z++)
            {
                ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_None, 0);
                ImGuiAPI.TableSetColumnIndex(0);
                ImGuiAPI.TextDisabled(z.ToString());

                for (int x = 0; x < countX; x++)
                {
                    ImGuiAPI.TableSetColumnIndex(x + 1);
                    DrawCell(node, x, z);
                }
            }
            ImGuiAPI.EndTable();
        }

        static void DrawCell(TtTerrainNode node, int levelX, int levelZ)
        {
            var level = node.Levels?[levelZ, levelX];
            // ActiveLevels 是以活动中心为原点的窗口数组, 下标不是全局 level 索引, 只能按引用找
            bool active = IsLevelActive(node, level);
            bool baking = level?.IsBakeRunning == true;

            var state = node.GetLevelDataState(level);
            var stateMark = state switch
            {
                ELevelDataState.Loaded => "+",
                ELevelDataState.Loading => "L",
                ELevelDataState.PendingDestroy => "W",
                ELevelDataState.Destroying => "D",
                _ => "-",
            };
            var mark = active ? "*" : (baking ? "~" : stateMark);
            var label = $"{mark}{levelX},{levelZ}";
            var size = Vector2.Zero;

            // 只保留右键菜单 Goto: 单击太容易误触, 一滑就把相机拉走并触发整个活动窗口迁移
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_NavHighlight, 0u);
            ImGuiAPI.Selectable(label, false, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in size);
            ImGuiAPI.PopStyleColor(1);

            // 活动中心 level 红框; 同一活动窗口内其余被流式激活的 level 绿框
            var centerIdx = node.ActiveCenterLevelIndex;
            var min = ImGuiAPI.GetItemRectMin();
            var max = ImGuiAPI.GetItemRectMax();
            var drawList = ImGuiAPI.GetWindowDrawList();
            if (levelX == centerIdx.X && levelZ == centerIdx.Y)
            {
                drawList.AddRect(min, max, 0xff0000ff, 0.0f, ImDrawFlags_.ImDrawFlags_None, 2.0f);
            }
            else if (active)
            {
                drawList.AddRect(min, max, 0xff00ff00, 0.0f, ImDrawFlags_.ImDrawFlags_None, 1.5f);
            }

            DrawCellMenu(node, levelX, levelZ);
        }

        static bool IsLevelActive(TtTerrainNode node, TtTerrainLevel level)
        {
            if (level == null || node.ActiveLevels == null)
                return false;
            foreach (var lv in node.ActiveLevels)
            {
                if (lv == level)
                    return true;
            }
            return false;
        }

        static void DrawCellMenu(TtTerrainNode node, int levelX, int levelZ)
        {
            EGui.UIProxy.StyleConfig.Instance.PushPopupStyle();
            if (ImGuiAPI.BeginPopupContextItem($"TerrainLevelCell_{levelX}_{levelZ}", ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                if (ImGuiAPI.MenuItem("Goto", null, false, true))
                {
                    GotoLevel(node, levelX, levelZ);
                }
                ImGuiAPI.EndPopup();
            }
            EGui.UIProxy.StyleConfig.Instance.PopPopupStyle();
        }

        /// <summary>goto 后的俯角: 从目标 level 前上方看下去, 兼顾"整块入画"与"能看出高低"</summary>
        const double GotoPitchDegree = 60.0;

        /// <summary>
        /// 把场景视口相机拉到指定 level 的上方俯视, 让整个 level 入画。
        ///
        /// 定位完全基于 level 自身的几何:
        ///   1. level 的节点局部范围取自 Level.StartPosition + LevelSize (与 CreateLevel 一致),
        ///      再经节点 AbsTransform 变换到世界 —— 节点有缩放/旋转时也正确
        ///      (旧实现直接 mPosition + 索引*LevelSize, 一旦节点带缩放就会偏)。
        ///   2. level 已构建 patch 时, 用这些 patch 的 AABB union 作为真实世界包围盒
        ///      (带地表高度), 保证俯视高度贴合起伏, 而不是一刀切节点原点。
        ///   3. level 还没流式加载时, 先用已知层的高度中值兜底, 并主动请求加载,
        ///      避免相机落在地面以下或贴着一片空白。
        ///
        /// 返回 false 表示节点/level 无效, 或当前节点没有可用的场景视口相机。
        /// </summary>
        public static bool GotoLevel(TtTerrainNode node, int levelX, int levelZ)
        {
            if (node == null || node.Levels == null)
                return false;
            if (levelZ < 0 || levelZ >= node.Levels.GetLength(0) ||
                levelX < 0 || levelX >= node.Levels.GetLength(1))
                return false;
            var level = node.Levels[levelZ, levelX];
            if (level == null)
                return false;

            var world = node.GetWorld();
            var slate = world?.ViewportSlate as EGui.Slate.TtWorldViewportSlate;
            var camera = slate?.CameraController?.Camera;
            if (camera == null)
            {
                Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Warning, "TerrainLevelGrid",
                    $"GotoLevel({levelX},{levelZ}): 没有可用的场景视口相机, 请在 SceneEditor 里操作");
                return false;
            }

            // goto 要保证目标 level 有真实几何, 否则 AABB 只能靠估算, 相机可能落到地下或对着一片空。
            // 按 LevelData 状态分别处理 (见 TtTerrainNode.EnsureLevelDataReady):
            //   等待销毁 → 从卸载队列捞回来直接复用, 不重建
            //   正在销毁 → 阻塞等 Dispose 结束, 再按已销毁重新加载
            //   已彻底销毁 → 直接重新加载
            var stateBefore = node.GetLevelDataState(level);
            node.EnsureLevelDataReady(level);
            var stateAfter = node.GetLevelDataState(level);

            var box = GetLevelWorldBox(node, level);
            var center = box.GetCenter();
            var halfSide = Math.Max(box.GetMaxSide() * 0.5, 1.0);

            Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Info, "TerrainLevelGrid",
                $"GotoLevel({levelX},{levelZ}): state={stateBefore}->{stateAfter}, loaded={level.LevelData != null}, centerIdx=({node.ActiveCenterLevelIndex.X},{node.ActiveCenterLevelIndex.Y}), " +
                $"boxMin=({box.Minimum.X:0.#},{box.Minimum.Z:0.#}) boxMax=({box.Maximum.X:0.#},{box.Maximum.Z:0.#}), dist={halfSide / Math.Max(Math.Abs(Math.Sin(camera.Fov)), 0.05):0.#}");

            // 距离按垂直 fov 反推: 与 TtCamera.AutoZoom 的半径/sin(fov) 语义保持一致
            var sinFov = Math.Abs(Math.Sin(camera.Fov));
            var dist = halfSide / Math.Max(sinFov, 0.05);

            var pitch = GotoPitchDegree * Math.PI / 180.0;
            var eye = new DVector3(center.X,
                center.Y + dist * Math.Sin(pitch),
                center.Z - dist * Math.Cos(pitch));

            var zNear = camera.ZNear;
            if (!float.IsFinite(zNear) || zNear <= 0.0f)
                zNear = 0.3f;
            var zFar = (float)Math.Min(dist + halfSide * 4.0, float.MaxValue);
            if (!float.IsFinite(zFar) || zFar <= zNear * 2.0f)
                zFar = zNear * 100.0f;
            if (!float.IsFinite(camera.ZFar) || camera.ZFar < zFar)
                camera.SetZRange(zNear, zFar);

            // SetMatrixStartPosition 只改双精度拆分基准、不重算 view; 而 LookAtLH 用"调用当时"的基准
            // 算 localEye/localLookAt (native ICamera::LookAtLH)。若只调 LookAtLH, 下一帧
            // SetActiveCenter 把 CameraOffset/startPosition 切到新位置后, view 仍是旧基准算的,
            // 与地形的世界平移(新基准)差一个基准位移 -> 地形整体跑出视锥, goto 后画面一片空。
            // 所以 goto 必须在这里同步切基准(CameraOffset + startPosition)并按新基准重算 view。
            world.SetCameraOffset(in eye, camera);
            camera.LookAtLH(in eye, in center, Vector3.Up);
            // 打断可能还在飞的 AutoZoom 动画, 否则下一帧会被旧目标点拉回去
            TtEngine.Instance.TickableManager.RemoveTickable(camera);
            return true;
        }

        /// <summary>
        /// 一个 level 的世界包围盒。
        /// 已构建 patch 时 union 它们的 AABB (真实几何, 带地表起伏高度);
        /// 否则用 level 的节点局部范围经节点变换到世界, Y 用已知层高度中值兜底。
        /// </summary>
        static DBoundingBox GetLevelWorldBox(TtTerrainNode node, TtTerrainLevel level)
        {
            var box = DBoundingBox.EmptyBox();
            var data = level.LevelData;
            if (data?.TiledPatch != null)
            {
                foreach (var patch in data.TiledPatch)
                {
                    if (patch == null)
                        continue;
                    box.Merge(in patch.AABB);
                }
            }
            if (box.IsEmpty() == false)
                return box;

            // 未加载: StartPosition 是节点局部坐标 (CreateLevel 只填了 X/Z), 加节点变换
            var start = level.StartPosition;
            var levelSize = (double)node.LevelSize;
            var localBox = new DBoundingBox(in start, new DVector3(start.X + levelSize, start.Y, start.Z + levelSize));
            var transform = node.Placement.AbsTransform;
            DBoundingBox.Transform(in localBox, in transform, out var worldBox);

            var estimateY = GetKnownSurfaceHeight(node, out var hasEstimate);
            if (hasEstimate)
            {
                worldBox.Minimum.Y = Math.Min(worldBox.Minimum.Y, estimateY - 1.0);
                worldBox.Maximum.Y = Math.Max(worldBox.Maximum.Y, estimateY + 1.0);
            }
            if (worldBox.Maximum.Y - worldBox.Minimum.Y < 1.0)
                worldBox.Maximum.Y = worldBox.Minimum.Y + 1.0;
            return worldBox;
        }

        /// <summary>已加载 level 的高度范围中值 (世界 Y), 用作尚未加载 level 的高度估计</summary>
        static double GetKnownSurfaceHeight(TtTerrainNode node, out bool found)
        {
            found = false;
            if (node.Levels == null)
                return 0.0;

            var nodeY = node.Placement.AbsTransform.mPosition.Y;
            double sum = 0.0;
            int count = 0;
            foreach (var lv in node.Levels)
            {
                var data = lv?.LevelData;
                if (data == null)
                    continue;
                sum += (data.HeightMapMinHeight + data.HeightMapMaxHeight) * 0.5 + nodeY;
                count++;
            }
            if (count == 0)
                return 0.0;
            found = true;
            return sum / count;
        }
    }
}
