using System;
using System.Collections.Generic;

namespace EngineNS.NxRHI
{
    // 帧内产生的临时 GPU 对象(计时采样/统计/一次性资源), 由 TtGpuFrameContainer 统一收集,
    // 等这一帧的 fence 真的完成后再回调 OnGpuFinished 做结算
    public interface IGpuFrameObject
    {
        void OnGpuFinished();
        // 容器被异常丢弃(上一帧没走 EndFrame 就被打断)时调: 必须归还资源, 但不得参与统计
        void OnGpuDiscarded();
    }
    public class TtGpuFrameContainer
    {
        // 录制可能发生在并行 Tick 的多个节点线程上, 收集列表必须自锁
        readonly object mLocker = new object();
        readonly List<IGpuFrameObject> mFrameObjects = new List<IGpuFrameObject>();
        readonly List<IGpuFrameObject> mFiringList = new List<IGpuFrameObject>();
        // 这一帧在 GPU 上收尾的 fence 值, 由 Manager.EndFrame 填入
        public UInt64 WaitValue { get; internal set; } = 0;
        public UInt64 FrameIndex { get; internal set; } = 0;
        public int Count { get { return mFrameObjects.Count; } }
        public void Push(IGpuFrameObject obj)
        {
            if (obj == null)
                return;
            lock (mLocker)
            {
                mFrameObjects.Add(obj);
            }
        }
        public void Reset()
        {
            mFrameObjects.Clear();
            mFiringList.Clear();
            WaitValue = 0;
        }
        public void ProcessOnGpuFinished()
        {
            // 先换到临时列表再遍历: 回调里如果又 Push 了对象, 既不会破坏遍历也不会被这次清掉
            lock (mLocker)
            {
                mFiringList.AddRange(mFrameObjects);
                mFrameObjects.Clear();
            }
            for (int i = 0; i < mFiringList.Count; i++)
            {
                mFiringList[i].OnGpuFinished();
            }
            mFiringList.Clear();
        }
        // 这个容器的 fence 永远不会到了(上一帧没 EndFrame 就被丢): 对象必须当场归还, 否则池会被吃干
        public void Discard()
        {
            lock (mLocker)
            {
                mFiringList.AddRange(mFrameObjects);
                mFrameObjects.Clear();
            }
            for (int i = 0; i < mFiringList.Count; i++)
            {
                mFiringList[i].OnGpuDiscarded();
            }
            mFiringList.Clear();
        }
    }
    public class TtGpuFrameContainerManager
    {
        readonly Queue<TtGpuFrameContainer> mGpuFramePool = new Queue<TtGpuFrameContainer>();
        readonly List<TtGpuFrameContainer> mPendingFrames = new List<TtGpuFrameContainer>();
        readonly object mLocker = new object();
        TtGpuFrameContainer mCurrGpuFrame;
        UInt64 mFrameIndex = 0;
        int mDroppedFrames = 0;
        public UInt64 FrameIndex { get { return mFrameIndex; } }
        // 累计因为上一帧没 EndFrame 而丢弃的容器数, 正常情况应恒为 0
        public int DroppedFrames { get { lock (mLocker) { return mDroppedFrames; } } }
        public TtGpuFrameContainer Current
        {
            get
            {
                lock (mLocker)
                {
                    // 兜底: 万一某条路径漏了 BeginFrame, 也不能让 BeginPass 拿到 null 容器
                    if (mCurrGpuFrame == null)
                        mCurrGpuFrame = AcquireContainer();
                    return mCurrGpuFrame;
                }
            }
        }
        TtGpuFrameContainer AcquireContainer()
        {
            lock (mLocker)
            {
                TtGpuFrameContainer result;
                if (mGpuFramePool.Count > 0)
                    result = mGpuFramePool.Dequeue();
                else
                    result = new TtGpuFrameContainer();
                mFrameIndex++;
                result.Reset();
                result.FrameIndex = mFrameIndex;
                return result;
            }
        }
        public void BeginFrame()
        {
            TtGpuFrameContainer stale;
            lock (mLocker)
            {
                // 上一帧没走 EndFrame 就被打断(窗口丢失/异常/跳过渲染): 那个容器的 fence 永远不会到,
                // 里面的采样必须当场归还, 否则每漏一帧就永久占着一批实例
                stale = mCurrGpuFrame;
                mCurrGpuFrame = AcquireContainer();
            }
            if (stale != null)
            {
                stale.Discard();
                lock (mLocker)
                {
                    mGpuFramePool.Enqueue(stale);
                    mDroppedFrames++;
                }
            }
        }
        // waitValue 必须是这一帧提交的最后一次 IncreaseSignal 之后的 fence 期望值,
        // 由 TtGpuDevice.TickPostEvents 在 mCoreObject.TickPostEvents() 之后取
        public void EndFrame(UInt64 waitValue)
        {
            TtGpuFrameContainer frame;
            lock (mLocker)
            {
                frame = mCurrGpuFrame;
                mCurrGpuFrame = null;
                if (frame != null)
                {
                    frame.WaitValue = waitValue;
                    mPendingFrames.Add(frame);
                }
            }
            Tick();
        }
        public void Tick()
        {
            var rc = TtEngine.Instance?.GfxDevice?.RenderContext;
            if (rc == null)
                return;
            // frame fence 只在 QU_Default 上 signal, 所以这里只保证默认队列的提交确实完成了
            var completed = rc.mCoreObject.GetFrameFence().GetCompletedValue();
            List<TtGpuFrameContainer> ready = null;
            lock (mLocker)
            {
                // 必须倒序: 正序 RemoveAt 会跳过紧邻的下一个元素
                for (int i = mPendingFrames.Count - 1; i >= 0; i--)
                {
                    var frame = mPendingFrames[i];
                    if (frame.WaitValue == 0 || frame.WaitValue > completed)
                        continue;
                    mPendingFrames.RemoveAt(i);
                    // 用局部表而不是字段: ProfilerModule.TickModule 与 TickPostEvents 可能
                    // 各调一次 Tick, 共享字段会让同一容器被两个线程重复结算
                    if (ready == null)
                        ready = new List<TtGpuFrameContainer>();
                    ready.Add(frame);
                }
            }
            if (ready == null)
                return;
            // 结算放在锁外: 回调要碰各 scope 自己的锁和池子
            for (int i = 0; i < ready.Count; i++)
            {
                var frame = ready[i];
                frame.ProcessOnGpuFinished();
                lock (mLocker)
                {
                    mGpuFramePool.Enqueue(frame);
                }
            }
        }
        public void Dispose()
        {
            lock (mLocker)
            {
                mPendingFrames.Clear();
                mGpuFramePool.Clear();
                mCurrGpuFrame = null;
            }
        }
    }
}
