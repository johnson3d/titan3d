using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer
{
    /// <summary>
    /// 把序列里的绑定解析成场景里真实的节点。
    ///
    /// 解析走两路: 先按目标节点 Guid (TargetNodeId), 拿不到再回退到名字路径。名字路径
    /// 经不起改名与挪父节点, 而轻量节点 (TtLightWeightNodeBase 系) 又没有 Guid —— 两路都
    /// 得留着。
    ///
    /// 解析结果按 BindingId 缓存: 求值每帧都要拿目标, 每帧都去遍历场景树找名字太贵。
    /// 缓存的代价是场景结构变了 (节点改名、挪父、删除) 之后必须显式 Invalidate, 否则
    /// 会一直指着旧节点甚至已销毁的节点。编辑器改动场景结构的地方要负责调它。
    /// </summary>
    public class TtSequenceBindingResolver
    {
        Dictionary<Guid, GamePlay.Scene.TtNode> mCache = new Dictionary<Guid, GamePlay.Scene.TtNode>();

        /// <summary>解析的起点场景, 换场景时必须重设并清缓存</summary>
        public GamePlay.Scene.TtScene Scene { get; private set; }

        public void SetScene(GamePlay.Scene.TtScene scene)
        {
            if (ReferenceEquals(Scene, scene))
                return;
            Scene = scene;
            mCache.Clear();
        }
        public void Invalidate()
        {
            mCache.Clear();
        }
        public void Invalidate(in Guid bindingId)
        {
            mCache.Remove(bindingId);
        }

        /// <summary>
        /// 解析一个绑定。解析不到返回 null —— 这不是错误: 序列可能引用了当前场景里不存在的
        /// 节点 (还没加载、被删了、复用到别的场景), 播放器碰到 null 就跳过这个绑定。
        /// </summary>
        public GamePlay.Scene.TtNode Resolve(Asset.TtSequenceBinding binding)
        {
            if (binding == null || Scene == null)
                return null;

            GamePlay.Scene.TtNode cached;
            if (mCache.TryGetValue(binding.BindingId, out cached))
                return cached;

            var result = ResolveInternal(binding);
            // null 也进缓存: 否则每帧都要为解析不到的绑定重新遍历一遍场景树。代价是节点
            // 后来才加载进来时需要外部 Invalidate 才能生效。
            mCache[binding.BindingId] = result;
            return result;
        }
        GamePlay.Scene.TtNode ResolveInternal(Asset.TtSequenceBinding binding)
        {
            // Guid 优先。名字路径经不起三件事: 改名、挪父节点、出现同名兄弟 (最后这条
            // 最麻烦 —— FindChildByExactName 返回第一个匹配项, 会静默绑到错节点且界面上
            // 看不出异常)。Guid 三种都不受影响。
            if (binding.TargetNodeId != Guid.Empty)
            {
                var byId = Scene.NodeId == binding.TargetNodeId ? Scene : Scene.FindNode(binding.TargetNodeId, true);
                if (byId != null)
                    return byId;
                // 拿不到不能就此返回 null: 节点可能是被删了重建的 (同名同位置但新 Guid),
                // 这种情况名字路径反而是对的 —— 继续往下回退。
            }

            var byPath = ResolveByPath(binding);
            // 名字路径找到了, 但绑定上的 Guid 缺失或已失效 —— 把当前 Guid 补上, 下次就走
            // Guid 路径。这一步是为了早期资产自愈, 不需要用户手动重建绑定。
            //
            // 注意这里改了资产数据但**不标脏**: 解析只能拿到绑定, 拿不到持有它的资产
            // 与编辑器。补写的是一个不改变语义的推导值 (就是当前已经解到的那个节点),
            // 丢了只是下次再自愈一次, 不值得为此把未保存标记推给用户。
            if (byPath != null && byPath.NodeId != Guid.Empty && byPath.NodeId != binding.TargetNodeId)
                binding.TargetNodeId = byPath.NodeId;
            return byPath;
        }
        /// <summary>
        /// 按 (参照节点 Guid + 名字路径) 解析。它是 Guid 失效时的回退手段, 不能删:
        /// TtLightWeightNodeBase 系 (相机 / Movement / SpringArm / AnimPlayNode) 不 override NodeId,
        /// 它们的 TargetNodeId 永远是 Empty, 只有这条路能解到。
        /// </summary>
        GamePlay.Scene.TtNode ResolveByPath(Asset.TtSequenceBinding binding)
        {
            GamePlay.Scene.TtNode parent;
            if (binding.ParentNodeId == Guid.Empty)
            {
                parent = Scene;
            }
            else
            {
                parent = Scene.NodeId == binding.ParentNodeId ? Scene : Scene.FindNode(binding.ParentNodeId, true);
                if (parent == null)
                    return null;
            }

            if (string.IsNullOrEmpty(binding.RelativePath))
                return parent;

            var segments = binding.RelativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var current = parent;
            for (int i = 0; i < segments.Length; ++i)
            {
                current = FindChildByExactName(current, segments[i]);
                if (current == null)
                    return null;
            }
            return current;
        }
        /// <summary>
        /// 按名字精确匹配直接子节点。这里不用 TtNode.FindFirstChild: 它是 Contains 模糊匹配
        /// 且可以递归, 拿来做路径解析会匹配到名字含子串的兄弟节点或者更深层的节点。
        /// </summary>
        static GamePlay.Scene.TtNode FindChildByExactName(GamePlay.Scene.TtNode parent, string name)
        {
            if (parent == null)
                return null;
            for (int i = 0; i < parent.Children.Count; ++i)
            {
                if (parent.Children[i].NodeName == name)
                    return parent.Children[i];
            }
            return null;
        }

        /// <summary>
        /// 由一个场景节点反推出该写进绑定的 (参照节点 Guid, 相对路径)。编辑器把节点拖进
        /// 序列时用。referenceNode 传 null 表示以场景根为参照。
        /// 目标不在参照节点的子树里时返回 false。
        ///
        /// 注意这里只管路径那一路。调用方还得自己把 target.NodeId 写进 TtSequenceBinding
        /// 的 TargetNodeId —— 否则解析只能走名字回退, 节点改名就断绑。
        /// </summary>
        public static bool MakeBindingPath(GamePlay.Scene.TtNode target, GamePlay.Scene.TtNode referenceNode, out Guid parentNodeId, out string relativePath)
        {
            parentNodeId = Guid.Empty;
            relativePath = "";
            if (target == null)
                return false;

            var stopAt = referenceNode;
            if (stopAt != null)
                parentNodeId = stopAt.NodeId;

            var names = new List<string>();
            var current = target;
            while (current != null)
            {
                if (stopAt != null && ReferenceEquals(current, stopAt))
                {
                    names.Reverse();
                    relativePath = string.Join('/', names);
                    return true;
                }
                if (stopAt == null && current.Parent == null)
                {
                    // 走到根了, 根自己不进路径
                    names.Reverse();
                    relativePath = string.Join('/', names);
                    return true;
                }
                names.Add(current.NodeName);
                current = current.Parent;
            }
            return false;
        }
    }
}
