using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer
{
    /// <summary>
    /// Sequencer 模块。持有全局属性访问器注册表, 并驱动所有活跃播放器。
    ///
    /// GetOrder 返回 3, 排在动画模块之后: 序列写的是场景节点的 Transform, 而动画/物理
    /// 可能也在写同一批节点。序列作为"导演层"应该最后落笔, 否则同一帧里它的结果会被
    /// 后跑的模块覆盖, 表现成拖播放头时节点抽搐。
    /// </summary>
    public partial class TtSequencerModule : TtModule<TtEngine>
    {
        List<WeakReference<TtSequencePlayer>> mPlayers = new List<WeakReference<TtSequencePlayer>>();

        /// <summary>
        /// 全局属性访问器注册表。哪些属性能被序列驱动是一份白名单, 不把所有 public 属性
        /// 自动暴露成可 K 帧: 那样里面大量属性写进去要么无效要么直接崩, 而这种错误只会在
        /// 运行时才暴露。
        ///
        /// 通用属性轨 (TtPropertyTrack) 走的 TtReflectedPropertyAccessor 用了反射, 但没有
        /// 放宽这条边界 —— 它要求属性有 [Rtti.Meta]、有公开 get/set、值类型有适配器, 而且
        /// 得由用户显式建一条轨道, 判据集中在 TtReflectedPropertyAccessor.CanAnimate。
        /// </summary>
        public TtSequencePropertyRegistry PropertyRegistry { get; } = new TtSequencePropertyRegistry();

        public override int GetOrder()
        {
            return 3;
        }
        public override async Thread.Async.TtTask<bool> Initialize(TtEngine host)
        {
            TtNodeTransformAccessors.RegisterAll(PropertyRegistry);
            // 值适配器是静态注册表 (理由见 TtSequenceValueAdapters 的注释), 不挂在模块上,
            // 但仍然在这里注册, 保证"引擎起来了适配器就齐了"这个时序跟访问器一致。
            TtBuiltinValueAdapters.RegisterAll();
            await Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        /// <summary>
        /// 注册播放器以便自动推进。用弱引用持有 —— 编辑器窗口关闭时不一定记得注销,
        /// 强引用会把整条序列和它引用的场景节点一起吊住。
        /// </summary>
        public void RegisterPlayer(TtSequencePlayer player)
        {
            if (player == null)
                return;
            for (int i = 0; i < mPlayers.Count; ++i)
            {
                TtSequencePlayer exist;
                if (mPlayers[i].TryGetTarget(out exist) && ReferenceEquals(exist, player))
                    return;
            }
            mPlayers.Add(new WeakReference<TtSequencePlayer>(player));
        }
        public void UnregisterPlayer(TtSequencePlayer player)
        {
            if (player == null)
                return;
            for (int i = 0; i < mPlayers.Count; ++i)
            {
                TtSequencePlayer exist;
                if (mPlayers[i].TryGetTarget(out exist) && ReferenceEquals(exist, player))
                {
                    mPlayers.RemoveAt(i);
                    return;
                }
            }
        }
        public override void TickLogic(TtEngine host)
        {
            if (mPlayers.Count == 0)
                return;

            var elapse = host.ElapsedSecond;
            for (int i = mPlayers.Count - 1; i >= 0; --i)
            {
                TtSequencePlayer player;
                if (mPlayers[i].TryGetTarget(out player) == false)
                {
                    mPlayers.RemoveAt(i);
                    continue;
                }
                player.Update(elapse);
            }
        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public EngineNS.Sequencer.TtSequencerModule SequencerModule { get; } = new Sequencer.TtSequencerModule();
    }
}
