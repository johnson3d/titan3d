using System.Collections.Generic;
using EngineNS.EGui.Slate;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;

namespace EngineNS.Editor.Forms
{
    // SceneEditor 的默认交互模式。
    // 继承自 TtWorldViewportInteractiveMode 以复用基础的相机控制
    // (WASD 移动、Alt+左键 / 中键 / X1 旋转和平移、滚轮缩放)
    // 以及左键单击 hitproxy 选中的派发 (走 TtSceneEditorViewport.OnHitproxySelected)。
    //
    // 在此基础上额外提供:
    //   - Ctrl + 左键按下 Move 轴: 把当前选中的所有节点克隆一份, 选择切换到克隆出的
    //     新节点, 由 Axis 直接拖动这些复制品 (类似 Unity / Unreal 的 Ctrl+Drag 复制)
    //
    // 后续 SceneEditor 专属的交互行为 (例如 Gizmo 高级操作、Place 资产到地面命中点、
    // 多视口同步等) 都应该在这里扩展, 或者新增并发的 mode 类型并通过
    // InteractiveModeManager.RegisterMode 注册, 而不是再去改基类。
    public class TtSceneEditorInteractiveMode : TtWorldViewportInteractiveMode
    {
        protected TtSceneEditor.TtSceneEditorViewport SceneEditorViewport
            => Viewport as TtSceneEditor.TtSceneEditorViewport;

        // 标记本次按下 -> 抬起 期间是否已经触发过克隆, 避免在拖动过程中反复克隆。
        bool mCloneTriggeredInThisDrag = false;
        bool mOpenSplineContextMenu = false;
        TtBezierSplineNode mSplineContextNode;
        int mSplineContextPointIndex = -1;
        TtBezierSplineNode.ESplineElementKind mSplineContextElementKind;
        bool mSplineContextIsLeftTangent;
        bool mConsumeSplineDoubleClickButtonUp = false;
        TtBezierSplineNode.TtSplinePoint mSplineDoubleClickCandidate;
        Vector2 mSplineDoubleClickStart;
        const float SplineDoubleClickMaxDistance = 12.0f;

        public override bool OnEvent(in Bricks.Input.Event e)
        {
            // 第一次单击控制点后 Gizmo 会出现在控制球上，第二击重新读 HitProxy 时可能
            // 命中 Gizmo 而不是控制点。这里使用第一击已经激活的控制点完成双击，并在
            // Axis 收到第二次 MOUSEBUTTONDOWN 前截断事件，避免误启动 Gizmo 拖动。
            if (TryHandleSplineElementDoubleClick(in e))
                return true;

            // Ctrl + 左键按下 + 当前 hover 在 Move 轴上 + 有选中节点 -> 触发"复制并拖动"。
            // 必须在 base.OnEvent 之前执行: base.OnEvent 会把按下事件转给 TtAxis 启动拖动,
            // 我们要在那之前把选择列表替换成克隆出的新节点, 这样 Axis 拖的就是复制品。
            TryStartClonedDrag(in e);

            // MOUSEBUTTONUP 时清掉一次性触发标志, 让下一次按下能重新触发克隆。
            if (e.Type == Bricks.Input.EventType.MOUSEBUTTONUP &&
                e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
            {
                mCloneTriggeredInThisDrag = false;
            }
            else if (e.Type == Bricks.Input.EventType.MOUSEBUTTONUP &&
                e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_RIGHT)
            {
                var viewport = SceneEditorViewport;
                var host = viewport?.HostEditor;
                var active = host?.ActiveSplineElement;
                var node = active?.OwnerNode ?? host?.GetSelectedSplineNode();
                if (viewport != null && viewport.IsMouseIn && node?.Spline != null)
                {
                    // Popup 打开后，菜单点击仍会经过视口输入链，当前选择可能先发生变化。
                    // 在右键抬起时固定操作目标，菜单回调不再依赖易变的 ActiveSplineElement。
                    mSplineContextNode = node;
                    mSplineContextPointIndex = active?.OwnerNode == node ? active.PointIndex :
                        Math.Max(0, node.Spline.PointCount - 1);
                    mSplineContextElementKind = active?.OwnerNode == node ? active.ElementKind :
                        TtBezierSplineNode.ESplineElementKind.Anchor;
                    mSplineContextIsLeftTangent = active?.OwnerNode == node && active.IsLeftTangent;
                    mOpenSplineContextMenu = true;
                }
            }

            return base.OnEvent(in e);
        }

        bool TryHandleSplineElementDoubleClick(in Bricks.Input.Event e)
        {
            if (e.Type == Bricks.Input.EventType.MOUSEBUTTONUP &&
                e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT &&
                mConsumeSplineDoubleClickButtonUp)
            {
                mConsumeSplineDoubleClickButtonUp = false;
                return true;
            }

            if (e.Type != Bricks.Input.EventType.MOUSEBUTTONDOWN)
                return false;

            if (e.MouseButton.Button != (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
            {
                // 右键打开菜单或其它鼠标操作会中断当前左键双击序列。否则菜单项的左键
                // 点击可能继承 SDL 的 Clicks 计数，被误当成控制点第二击而提前吞掉。
                mSplineDoubleClickCandidate = null;
                return false;
            }

            var viewport = SceneEditorViewport;
            if (viewport == null)
            {
                mSplineDoubleClickCandidate = null;
                return false;
            }
            var mousePoint = new Vector2(e.MouseButton.X, e.MouseButton.Y);
            var viewportPoint = mousePoint + viewport.ViewportPos;
            if (viewport.Axis?.IsTransforming == true ||
                viewport.UIOperated || !viewport.IsMouseIn ||
                viewport.PointInOverlappedArea(in viewportPoint))
            {
                mSplineDoubleClickCandidate = null;
                return false;
            }

            if (e.MouseButton.Clicks < 2)
            {
                // 只把 HitProxy 确实命中控制球的首次按下记为候选。第一次抬起后 Gizmo
                // 可能覆盖控制球，所以第二击不能再依赖 HitProxy，但必须匹配该候选。
                mSplineDoubleClickCandidate = GetSplineElementAtMouse(viewport, in mousePoint);
                mSplineDoubleClickStart = mousePoint;
                return false;
            }

            var candidate = mSplineDoubleClickCandidate;
            mSplineDoubleClickCandidate = null;
            var active = viewport.HostEditor?.ActiveSplineElement;
            var delta = mousePoint - mSplineDoubleClickStart;
            if (candidate?.OwnerNode == null || !object.ReferenceEquals(candidate, active) ||
                delta.LengthSquared() > SplineDoubleClickMaxDistance * SplineDoubleClickMaxDistance)
            {
                return false;
            }

            mConsumeSplineDoubleClickButtonUp = true;
            viewport.OnHitproxyDoubleClick(candidate);
            return true;
        }

        static TtBezierSplineNode.TtSplinePoint GetSplineElementAtMouse(
            TtSceneEditor.TtSceneEditorViewport viewport, in Vector2 mousePoint)
        {
            var policy = viewport.RenderPolicy as Graphics.Pipeline.TtRenderPolicy;
            if (policy == null)
                return null;
            var position = viewport.Window2Viewport(mousePoint);
            if (position.X < 0 || position.Y < 0)
                return null;
            return policy.GetHitproxy((uint)position.X, (uint)position.Y) as
                TtBezierSplineNode.TtSplinePoint;
        }

        public override void TickOnFocus()
        {
            base.TickOnFocus();

            var host = SceneEditorViewport?.HostEditor;
            host?.SyncSplineControlFromAxis();

            var keyboards = TtEngine.Instance.InputSystem;
            if (keyboards.IsKeyPressed(Bricks.Input.Keycode.KEY_DELETE))
            {
                // 有样条子元素处于活动状态时，Delete 只处理锚点，绝不向下删除所属节点。
                if (host?.ActiveSplineElement != null)
                    host.DeleteSelectedSplinePoint();
                else
                    DeleteSelectedNodes();
            }
        }

        public override Vector2 OnDrawViewportUI(in Vector2 startDrawPos)
        {
            var host = SceneEditorViewport?.HostEditor;
            if (mOpenSplineContextMenu)
            {
                ImGuiAPI.OpenPopup("##SplineControlContext", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                mOpenSplineContextMenu = false;
            }

            EGui.UIProxy.StyleConfig.Instance.PushPopupStyle();
            if (ImGuiAPI.BeginPopup("##SplineControlContext", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                var splineNode = mSplineContextNode;
                var spline = splineNode?.Spline;
                var pointIndex = mSplineContextPointIndex;
                var validPoint = spline != null && pointIndex >= 0 && pointIndex < spline.PointCount;
                bool actionExecuted = false;
                if (ImGuiAPI.MenuItem(TtLocalization.Tr("Insert Point After"), null, false, validPoint))
                {
                    host?.InsertSplinePointAfter(splineNode, pointIndex,
                        mSplineContextElementKind, mSplineContextIsLeftTangent);
                    actionExecuted = true;
                }
                if (!actionExecuted && ImGuiAPI.MenuItem(TtLocalization.Tr("Append Point"), null, false,
                    spline != null && !spline.IsClosed))
                {
                    host?.AppendSplinePoint(splineNode, pointIndex,
                        mSplineContextElementKind, mSplineContextIsLeftTangent);
                    actionExecuted = true;
                }
                var canDelete = validPoint &&
                    mSplineContextElementKind == TtBezierSplineNode.ESplineElementKind.Anchor &&
                    spline.PointCount > (spline.IsClosed ? 3 : 2);
                if (!actionExecuted && ImGuiAPI.MenuItem(TtLocalization.Tr("Delete Point"), null, false, canDelete))
                {
                    host?.DeleteSplinePoint(splineNode, pointIndex);
                    actionExecuted = true;
                }
                var canToggleClosed = spline != null && (spline.IsClosed || spline.PointCount >= 3);
                var toggleLabel = spline?.IsClosed == true ? "Open Spline" : "Close Spline";
                if (!actionExecuted && ImGuiAPI.MenuItem(TtLocalization.Tr(toggleLabel), null, false, canToggleClosed))
                {
                    host?.ToggleSplineClosed(splineNode, pointIndex);
                    actionExecuted = true;
                }
                if (actionExecuted)
                {
                    mSplineContextNode = null;
                    mSplineContextPointIndex = -1;
                }
                ImGuiAPI.EndPopup();
            }
            EGui.UIProxy.StyleConfig.Instance.PopPopupStyle();
            return base.OnDrawViewportUI(in startDrawPos);
        }

        public override void OnLeaveMode()
        {
            mSplineContextNode = null;
            mSplineContextPointIndex = -1;
            mConsumeSplineDoubleClickButtonUp = false;
            mSplineDoubleClickCandidate = null;
            SceneEditorViewport?.HostEditor?.ClearSplineElementSelection(true);
            base.OnLeaveMode();
        }

        void DeleteSelectedNodes()
        {
            var host = SceneEditorViewport?.HostEditor;
            if (host?.mWorldOutliner == null)
                return;
            var selected = host.mWorldOutliner.SelectedNodes;
            if (selected == null || selected.Count == 0)
                return;

            var world = SceneEditorViewport?.World;
            // 开门时整批删除封为一条可撤销事务; history为null时行为与旧流程一致
            var history = host.EditorHistory;
            history?.BeginTransaction(selected.Count == 1 ? $"Delete Node {selected[0]?.NodeName}" : $"Delete {selected.Count} Nodes");
            foreach (var node in selected)
            {
                if (node == null || node == world?.Root)
                    continue;
                TtSceneEditor.DeleteNodeWithHistory(history, node);
            }
            history?.EndTransaction();
            selected.Clear();
            host.NodeInspector.Target = null;
            SceneEditorViewport?.Axis?.SetSelectedNodes(selected);

            // 清除渲染层描边/高亮
            var policy = host.RenderPolicy as Graphics.Pipeline.TtRenderPolicy;
            if (policy?.PickedProxiableManager != null)
                policy.PickedProxiableManager.ClearSelected();
        }

        // 检查是否符合 Ctrl+左键按 Move 轴的复制条件; 符合则同步克隆所有选中节点,
        // 并把宿主编辑器的 outliner 选择列表替换为克隆出的新节点。
        // 不符合条件 / 克隆失败时静默返回, 不改变任何状态, 让 base.OnEvent 走原有逻辑。
        void TryStartClonedDrag(in Bricks.Input.Event e)
        {
            if (mCloneTriggeredInThisDrag)
                return;

            if (e.Type != Bricks.Input.EventType.MOUSEBUTTONDOWN)
                return;
            if (e.MouseButton.Button != (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                return;

            var input = TtEngine.Instance.InputSystem;
            if (input == null || !input.IsCtrlKeyDown())
                return;

            var viewport = SceneEditorViewport;
            if (viewport == null || viewport.Axis == null)
                return;

            // 仅在 Move 类轴 hover 时触发 (Rot / Scale 的 Ctrl+拖通常没有"复制"语义)。
            var axisType = viewport.Axis.CurrentAxisType;
            if (axisType < TtAxis.enAxisType.Move_Start || axisType > TtAxis.enAxisType.Move_End)
                return;

            var host = viewport.HostEditor;
            if (host == null || host.mWorldOutliner == null || host.ActiveSplineElement != null)
                return;
            var selected = host.mWorldOutliner.SelectedNodes;
            if (selected == null || selected.Count == 0)
                return;

            // 把当前选中节点的快照拿出来; 后续要替换 selected 列表, 不能在迭代它的同时改它。
            var sourceNodes = new List<TtNode>(selected);

            var world = viewport.World;
            var clones = new List<TtNode>(sourceNodes.Count);
            for (int i = 0; i < sourceNodes.Count; i++)
            {
                var src = sourceNodes[i];
                if (src == null)
                    continue;
                var cloneTask = src.CloneNode(world, null);
                // 编辑器单次操作, 同步等待克隆完成, 让选择列表立即可用,
                // base.OnEvent 转交给 TtAxis 后, Axis 拖的就是新克隆的节点。
                var cloned = cloneTask.GetResultUntilCompleted();
                if (cloned == null)
                    continue;

                // 挂到原节点的同一 parent 下, 保持场景层级一致;
                // CloneNode 已经把 Placement 的 Position/Quat/Scale 复制过来了。
                var parent = src.Parent ?? host.Scene;
                cloned.Parent = parent;

                // CloneNode 通过 DataCopyer.DataCopy 把整份 NodeData 复制过来,
                // 包括 Name 字段, 所以新节点和源节点同名 (例如都叫 "red"), 在 outliner
                // 里很难区分; 这里给克隆节点的 NodeName 追加 "_Copy" / "_Copy1" 等后缀,
                // 并在同 parent 下确保唯一。
                cloned.NodeName = MakeUniqueNodeName(parent, src.NodeName);

                // CloneNode 只复制了数据, 没触发 HitproxyType setter,
                // 新节点的 HitProxy 字段为 null, 既没有自己的 ProxyId, 也无法被 hitproxy
                // 拾取 (会让原节点也跟着拾取异常)。
                // 通过对 HitproxyType 自赋值, 触发 Node_Editor.cs 里的 OnHipproxyTypeChanged:
                //   - Root        -> UnmapProxy + MapProxy 分配一个新 ProxyId
                //   - FollowParent -> 共享 Parent.HitProxy
                //   - None        -> 不注册
                // 与 Node.cs:1526 OnSceneLoaded 里的同一手法一致。
                cloned.HitproxyType = cloned.HitproxyType;

                clones.Add(cloned);
            }

            if (clones.Count == 0)
                return;

            // 克隆操作记录为一条可撤销事务(undo时整批移除克隆体)
            var cloneHistory = host.EditorHistory;
            if (cloneHistory != null)
            {
                cloneHistory.BeginTransaction(clones.Count == 1 ? $"Clone Node {clones[0].NodeName}" : $"Clone {clones.Count} Nodes");
                foreach (var c in clones)
                {
                    TtSceneEditor.PushNodeCreateCommand(cloneHistory, c, "Clone Node");
                }
                cloneHistory.EndTransaction();
            }

            // 把原来的节点 Selected 标记清掉, 选中切换到新克隆的节点。
            for (int i = 0; i < selected.Count; i++)
            {
                if (selected[i] != null)
                    selected[i].Selected = false;
            }
            selected.Clear();
            for (int i = 0; i < clones.Count; i++)
            {
                clones[i].Selected = true;
                selected.Add(clones[i]);
            }
            host.NodeInspector.Target = selected;

            mCloneTriggeredInThisDrag = true;
        }

        // 在 parent 下为 baseName 找一个不重名的 NodeName: 优先 baseName + "_Copy",
        // 还重则继续 _Copy1, _Copy2, ... 直到找到空位。
        // baseName 为 null/空 时退化为 "Node_Copy"。
        static string MakeUniqueNodeName(TtNode parent, string baseName)
        {
            if (string.IsNullOrEmpty(baseName))
                baseName = "Node";

            // 如果原名已经是 "xxx_CopyN" 形式, 复制时基于 "xxx" 继续编号, 避免连续多次
            // 复制出现 "red_Copy_Copy_Copy" 这种名字。
            var stem = StripCopySuffix(baseName);

            var candidate = stem + "_Copy";
            int suffix = 1;
            while (NodeNameExistsUnderParent(parent, candidate))
            {
                candidate = stem + "_Copy" + suffix;
                suffix++;
                if (suffix > 10000) // 安全上限, 正常场景永远走不到
                    break;
            }
            return candidate;
        }

        static string StripCopySuffix(string name)
        {
            // 命中 "_Copy" 或 "_Copy{数字}" 时, 把后缀截掉, 保留前面的 stem。
            const string copyTag = "_Copy";
            int idx = name.LastIndexOf(copyTag);
            if (idx < 0)
                return name;
            var tail = name.Substring(idx + copyTag.Length);
            if (tail.Length == 0)
                return name.Substring(0, idx);
            for (int i = 0; i < tail.Length; i++)
            {
                if (tail[i] < '0' || tail[i] > '9')
                    return name; // _Copy 后跟非数字, 不是我们生成的后缀, 保留原名
            }
            return name.Substring(0, idx);
        }

        static bool NodeNameExistsUnderParent(TtNode parent, string name)
        {
            if (parent == null)
                return false;
            var children = parent.Children;
            if (children == null)
                return false;
            for (int i = 0; i < children.Count; i++)
            {
                var c = children[i];
                if (c != null && c.NodeName == name)
                    return true;
            }
            return false;
        }
    }
}
