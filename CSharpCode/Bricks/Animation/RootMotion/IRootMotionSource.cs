using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.RootMotion
{
    /// <summary>
    /// RootMotion的提供方。动画播放节点实现本接口, 由Movement消费其位移。
    /// </summary>
    public interface IRootMotionSource
    {
        /// <summary>
        /// 本提供方当前的RootMotion过滤模式
        /// </summary>
        ERootMotionMode RootMotionMode { get; set; }
        /// <summary>
        /// 取出待消费的位移增量(Actor空间), 取出后内部清零。没有待消费位移时返回false。
        /// </summary>
        bool ConsumeRootMotion(out FTransform delta);
    }

    /// <summary>
    /// RootMotion的收集与消费缓冲。播放节点每帧Submit, Movement每帧Consume。
    ///
    /// 两边的先后靠 TtNode.GetTickOrder() 保证: 动画播放节点取 ETickOrder.Animation,
    /// TtMovement 取 ETickOrder.Movement, World.TickLogic 会按此对同步Tick的节点升序排序。
    /// 下面的帧号检测是个哨兵: 若哪天排序被破坏(例如新的消费方忘了 override GetTickOrder,
    /// 或者消费方被标上了 ENodeStyles.ParallelTick —— 并行组虽然排在同步组之后,
    /// 但组内无序, 且如果提交方也在并行组就彻底没保证了), 会警告一次并保持原行为。
    /// </summary>
    public class TtRootMotionAccumulator
    {
        FRootMotionData mPending = FRootMotionData.Empty;
        int mSubmitFrame = -1;
        bool mStaleWarned = false;

        public bool HasPending { get => mPending.HasRootMotion; }
        public FRootMotionData Pending { get => mPending; }

        /// <summary>
        /// 提交本帧的RootMotion。同一帧多次提交按顺序复合(例如Montage与基础动画分别提交)。
        /// </summary>
        public void Submit(in FRootMotionData data)
        {
            if (!data.HasRootMotion)
                return;

            int frame = TtEngine.Instance.FrameCount;
            if (mPending.HasRootMotion && mSubmitFrame == frame)
            {
                mPending.Delta = TtRootMotionUtil.Combine(mPending.Delta, data.Delta);
                mPending.FromMontage |= data.FromMontage;
            }
            else
            {
                mPending = data;
            }
            mSubmitFrame = frame;
        }

        public void Reset()
        {
            mPending = FRootMotionData.Empty;
            mSubmitFrame = -1;
        }

        /// <summary>
        /// 取出待消费位移并清零
        /// </summary>
        public bool Consume(out FTransform delta)
        {
            if (!mPending.HasRootMotion)
            {
                delta = FTransform.Identity;
                return false;
            }

            int frame = TtEngine.Instance.FrameCount;
            if (mSubmitFrame != frame && !mStaleWarned)
            {
                mStaleWarned = true;
                Profiler.Log.WriteLine<TtAnimationCategory>(Profiler.ELogTag.Warning,
                    "RootMotion被延迟一帧消费: 消费方的Tick早于动画节点, 请检查两边的 TtNode.GetTickOrder() 是否满足 动画 < 消费方, 以及两边是否都没有 ENodeStyles.ParallelTick 标记");
            }

            delta = mPending.Delta;
            mPending = FRootMotionData.Empty;
            return true;
        }
    }
}
