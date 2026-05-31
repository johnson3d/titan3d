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

        public override bool OnEvent(in Bricks.Input.Event e)
        {
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

            return base.OnEvent(in e);
        }

        public override void TickOnFocus()
        {
            base.TickOnFocus();

            var keyboards = TtEngine.Instance.InputSystem;
            if (keyboards.IsKeyPressed(Bricks.Input.Keycode.KEY_DELETE))
            {
                DeleteSelectedNodes();
            }
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
            foreach (var node in selected)
            {
                if (node == null || node == world?.Root)
                    continue;
                node.DeleteFromScene();
            }
            selected.Clear();
            host.NodeInspector.Target = null;
            SceneEditorViewport?.Axis?.SetSelectedNodes(selected);
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
            if (host == null || host.mWorldOutliner == null)
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
