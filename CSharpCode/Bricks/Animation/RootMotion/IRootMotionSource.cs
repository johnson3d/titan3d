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
    /// 注意: 场景节点的Tick是深度优先且按Children顺序, 若Movement节点排在
    /// MeshNode(动画节点挂在其下)之前, 消费到的会是上一帧提交的位移, 即有一帧延迟。
    /// 这里用帧号检测该情况并只警告一次, 不改变行为(一帧延迟对表现无实质影响)。
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
                    "RootMotion被延迟一帧消费: Movement节点的Tick早于动画节点, 建议把动画所在的MeshNode排在Movement之前");
            }

            delta = mPending.Delta;
            mPending = FRootMotionData.Empty;
            return true;
        }
    }
}
