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
            /// <summary>写这个属性的 Section 是不是 RestoreState</summary>
            public bool WantsRestore;
        }

        Dictionary<FAnimatedPropertyKey, FSaved> mSaved = new Dictionary<FAnimatedPropertyKey, FSaved>();

        public int Count { get => mSaved.Count; }

        /// <summary>
        /// 第一次见到这个 (目标, 属性) 时把当前值存下来; 已经存过的不再覆盖 ——
        /// 否则第二帧存的就是第一帧被序列改过的值, 恢复就失效了。
        ///
        /// wantsRestore 来自写这个属性的 Section 的 CompletionMode。同一个属性被多个
        /// Section 写、而它们的模式又不一样时 KeepState 优先: 只要有一个写它的 Section
        /// 想保持末值, 就不该在另一个 Section 结束时把它改回原值。这个降级是单向的,
        /// 一旦降成"不恢复"就不会再升回去 —— 粒度偏粗 (方向与 UE 的 OverrideKeepState
        /// 一致), 换来的是不必按 Section 分桶记账。
        /// </summary>
        public void CaptureIfFirst(object target, ISequencePropertyAccessor accessor, bool wantsRestore)
        {
            if (target == null || accessor == null)
                return;
            var key = new FAnimatedPropertyKey(target, accessor.PropertyId);
            FSaved exist;
            if (mSaved.TryGetValue(key, out exist))
            {
                if (exist.WantsRestore && wantsRestore == false)
                {
                    exist.WantsRestore = false;
                    mSaved[key] = exist;
                }
                return;
            }
            var value = accessor.Read(target);
            if (value == null)
                return;
            mSaved.Add(key, new FSaved()
            {
                Target = target,
                Accessor = accessor,
                Value = value,
                WantsRestore = wantsRestore,
            });
        }
        /// <summary>
        /// 收集所有"结束时要恢复"的记录。给 TtSequenceEvalTable.Flush 用: 它拿到这批 key
        /// 之后逐个看本帧有没有人写, 没人写的就地恢复。
        ///
        /// 拆成"先收集再逐条恢复"而不是让 store 直接去遍历中间表, 是为了保持依赖单向:
        /// 中间表认识 store, store 不认识中间表。
        /// </summary>
        public void CollectRestorable(List<FAnimatedPropertyKey> result)
        {
            if (result == null)
                return;
            foreach (var i in mSaved)
            {
                if (i.Value.WantsRestore)
                    result.Add(i.Key);
            }
        }
        /// <summary>
        /// 把一条记录写回原值并丢掉它, 没有这条记录时什么都不做。
        ///
        /// 恢复之后连记录一起丢掉是安全的: 播放头再次进入那个 Section 时会重新 CaptureIfFirst,
        /// 而此刻属性上的值已经是原值, 于是新存下来的还是同一个值。留着记录反而会让
        /// 每一帧都重复写一次原值。
        /// </summary>
        public bool RestoreOne(FAnimatedPropertyKey key)
        {
            FSaved saved;
            if (mSaved.TryGetValue(key, out saved) == false)
                return false;
            saved.Accessor.Write(saved.Target, saved.Value);
            mSaved.Remove(key);
            return true;
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
