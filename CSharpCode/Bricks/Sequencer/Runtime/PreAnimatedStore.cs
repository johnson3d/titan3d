using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer
{
    /// <summary>
    /// 原值快照。序列第一次写某个 (目标, 属性) 之前把当时的值记下来, 停止播放时按记录
    /// 恢复。对标 UE 的 PreAnimatedState。
    ///
    /// 为什么必须有这一层: 在编辑器里拖播放头就是在真改场景节点的 Transform。没有快照,
    /// 关掉序列编辑器后场景就永久停在播放头那一帧的姿态上, 而且用户不会意识到自己的
    /// 场景已经被改坏了。
    ///
    /// 只记"序列真的写过"的属性, 所以恢复不会碰用户手工改的其他属性。
    /// </summary>
    public class TtPreAnimatedStore
    {
        struct FSaved
        {
            public object Target;
            public ISequencePropertyAccessor Accessor;
            public object Value;
        }

        Dictionary<FAnimatedPropertyKey, FSaved> mSaved = new Dictionary<FAnimatedPropertyKey, FSaved>();

        public int Count { get => mSaved.Count; }

        /// <summary>
        /// 第一次见到这个 (目标, 属性) 时把当前值存下来; 已经存过的不再覆盖 ——
        /// 否则第二帧存的就是第一帧被序列改过的值, 恢复就失效了。
        /// </summary>
        public void CaptureIfFirst(object target, ISequencePropertyAccessor accessor)
        {
            if (target == null || accessor == null)
                return;
            var key = new FAnimatedPropertyKey(target, accessor.PropertyId);
            if (mSaved.ContainsKey(key))
                return;
            var value = accessor.Read(target);
            if (value == null)
                return;
            mSaved.Add(key, new FSaved()
            {
                Target = target,
                Accessor = accessor,
                Value = value,
            });
        }
        /// <summary>把记下来的原值全部写回, 并清空记录</summary>
        public void RestoreAll()
        {
            foreach (var i in mSaved)
                i.Value.Accessor.Write(i.Value.Target, i.Value.Value);
            mSaved.Clear();
        }
        /// <summary>
        /// 丢掉某个目标的所有记录而不恢复。目标已经被销毁时用 —— 往销毁掉的对象上写值
        /// 没意义, 还可能踩到已释放的原生资源。
        /// </summary>
        public void Forget(object target)
        {
            if (target == null)
                return;
            List<FAnimatedPropertyKey> removes = null;
            foreach (var i in mSaved)
            {
                if (ReferenceEquals(i.Value.Target, target) == false)
                    continue;
                if (removes == null)
                    removes = new List<FAnimatedPropertyKey>();
                removes.Add(i.Key);
            }
            if (removes == null)
                return;
            for (int i = 0; i < removes.Count; ++i)
                mSaved.Remove(removes[i]);
        }
        public void Clear()
        {
            mSaved.Clear();
        }
    }
}
