using EngineNS.ECS;
using EngineNS.UnitTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.NxRHI
{
    partial struct ICmdRecorder
    {
        public void PushGpuDraw(TtGraphicDraw draw)
        {
            PushGpuDraw(draw.mCoreObject.NativeSuper);
        }
        public void PushGpuDraw(TtComputeDraw draw)
        {
            PushGpuDraw(draw.mCoreObject.NativeSuper);
        }
        public void PushGpuDraw(TtRayTracingDraw draw)
        {
            PushGpuDraw(draw.mCoreObject.NativeSuper);
        }
        public void PushGpuDraw(TtCopyDraw draw)
        {
            PushGpuDraw(draw.mCoreObject.NativeSuper);
        }
        public void PushGpuDraw(TtActionDraw draw)
        {
            PushGpuDraw(draw.mCoreObject.NativeSuper);
        }
        public void PushGpuDraw(IActionDraw draw)
        {
            PushGpuDraw(draw);
        }
    }
    public class TtCmdRecorder : AuxPtrType<NxRHI.ICmdRecorder>
    {
        public TtCmdRecorder(NxRHI.ECmdRecorderType types = ECmdRecorderType.CRT_Graphics)
        {
            mCoreObject = ICmdRecorder.CreateInstance(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, types);
        }
        public uint DrawcallNumber
        {
            get => mCoreObject.GetDrawcallNumber();
        }
        public void SortGraphicDrawWithEffect()
        {
            mCoreObject.SortGraphicDrawWithEffect();
        }
        public IGpuDraw GetDrawcall(uint index)
        {
            return mCoreObject.GetDrawcall(index);
        }
        public void PushGpuDraw(TtGraphicDraw draw)
        {
            mCoreObject.PushGpuDraw(draw.mCoreObject);
        }
        public void PushGpuDraw(TtComputeDraw draw)
        {
            mCoreObject.PushGpuDraw(draw.mCoreObject);
        }
        public void PushGpuDraw(TtRayTracingDraw draw)
        {
            mCoreObject.PushGpuDraw(draw.mCoreObject);
        }
        public void PushGpuDraw(TtCopyDraw draw)
        {
            mCoreObject.PushGpuDraw(draw.mCoreObject);
        }
        public void PushGpuDraw(TtActionDraw draw)
        {
            mCoreObject.PushGpuDraw(draw.mCoreObject.NativeSuper);
        }
        public void PushGpuDraw(IActionDraw draw)
        {
            mCoreObject.PushGpuDraw(draw);
        }
        public void ResetGpuDraws()
        {
            mCoreObject.ResetGpuDraws();
        }
    }

    public enum ECommandListState
    {
        None,
        Recording,
        FinishRecord,
        Committed,
    }
    public class TtCommandList : AuxPtrType<NxRHI.ICommandList>
    {
        static public TtCommandList GetCmdList()
        {
            return TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
        }
        ECommandListState mCommandListState = ECommandListState.None;
        public ECommandListState CommandListState
        {
            get => mCommandListState;
            internal set => mCommandListState = value;
        }
        public override void Dispose()
        {
            CurrentGpuScope = null;
            base.Dispose();
        }
        public string DebugName
        {
            get => mCoreObject.GetDebugName();
        }
        public void BeginEvent(string info, uint color = 0)
        {
            mCoreObject.BeginEvent(info, color);
        }
        public void EndEvent()
        {
            mCoreObject.EndEvent();
        }
        public ICmdRecorder BeginCommand()
        {
            mCommandListState = ECommandListState.Recording;
            return mCoreObject.BeginCommand();
        }
		public void EndCommand()
        {
            mCoreObject.EndCommand();
            mCommandListState = ECommandListState.FinishRecord;
        }
        private NxRHI.TtGpuFrameScope CurrentGpuScope;
        public unsafe bool BeginPass(TtFrameBuffers fb, in FRenderPassClears passClears, string name)
        {
            var scope = TtEngine.Instance.ProfilerModule.GpuTimeScopeManager.GetGpuScope(name);
            CurrentGpuScope = scope == null ? null : scope.AcquireForRecord();
            if (CurrentGpuScope != null)
                CurrentGpuScope.BeginRecord(this);

            var cp = passClears;
            if (TtEngine.Instance.GfxDevice.Config.IsReverseZ)
                cp.m_DepthClearValue = 0;
            else
                cp.m_DepthClearValue = 1;
            var ret = mCoreObject.BeginPass(fb.mCoreObject, in cp, name);
            if (ret == false && CurrentGpuScope != null)
            {
                // pass 没真正开起来就不会有 EndPass, 这条记录立刻归还, 避免悬挂占用
                CurrentGpuScope.AbortRecord();
                CurrentGpuScope = null;
            }
            return ret;
        }
        public unsafe void SetViewport(uint Num, FViewPort* pViewports)
        {
            mCoreObject.SetViewport(Num, pViewports);
        }
        public unsafe void SetViewport(in FViewPort pViewports)
        {
            mCoreObject.SetViewport(in pViewports);
        }
        public unsafe void SetScissor(uint Num, FScissorRect* pScissor)
        {
            mCoreObject.SetScissor(Num, pScissor);
        }
        public unsafe void SetScissor(in FScissorRect pScissor)
        {
            mCoreObject.SetScissor(pScissor);
        }
        public void EndPass()
        {
            mCoreObject.EndPass();
            if (CurrentGpuScope != null)
            {
                CurrentGpuScope.EndRecord(this, TtEngine.Instance.ProfilerModule.GpuFrameContainerManager.Current);
            }
            CurrentGpuScope = null;
        }
        // 命令列表被回收/复用时调用: BeginPass 后漏了 EndPass 的采样要当场归还,
        // 否则那个实例会被永久扣在 IsInUse 上, 池迟早被吃干
        internal void AbandonCurrentGpuScope()
        {
            var scope = CurrentGpuScope;
            CurrentGpuScope = null;
            if (scope != null)
                scope.Abandon();
        }
        public void InheritPass(TtCommandList cmdlist)
        {
            mCoreObject.InheritPass(cmdlist.mCoreObject);
        }
        public void SetShader(TtShader shader)
        {
            mCoreObject.SetShader(shader.mCoreObject);
        }
        public void SetCBV(EShaderType type, FShaderBinder binder, TtCbView buffer)
        {
            mCoreObject.SetCBV(type, binder, buffer.mCoreObject);
        }
        public void SetSrv(EShaderType type, FShaderBinder binder, TtSrView view)
        {
            mCoreObject.SetSrv(type, binder, view.mCoreObject);
        }
        public void SetUav(EShaderType type, FShaderBinder binder, TtUaView view)
        {
            mCoreObject.SetUav(type, binder, view.mCoreObject);
        }
        public void SetSampler(EShaderType type, FShaderBinder binder, TtSampler sampler)
        {
            mCoreObject.SetSampler(type, binder, sampler.mCoreObject);
        }
        public void SetVertexBuffer(uint slot, TtVbView buffer, uint Offset, uint Stride)
        {
            mCoreObject.SetVertexBuffer(slot, buffer.mCoreObject, Offset, Stride);
        }
        public void SetIndexBuffer(TtIbView buffer, bool IsBit32 = false)
        {
            mCoreObject.SetIndexBuffer(buffer.mCoreObject, IsBit32);
        }
        public void SetInputLayout(TtInputLayout layout)
        {
            mCoreObject.SetInputLayout(layout.mCoreObject);
        }
        
        #region Draw
        public void Draw(EPrimitiveType topology, uint BaseVertex, uint DrawCount, uint Instance = 1)
        {
            mCoreObject.Draw(topology, BaseVertex, DrawCount, Instance);
        }
        public void IndirectDraw(EPrimitiveType topology, TtBuffer indirectArg, uint indirectArgOffset = 0, TtBuffer countBuffer = null)
        {
            mCoreObject.IndirectDrawIndexed(topology, indirectArg.mCoreObject, indirectArgOffset, countBuffer.mCoreObject);
        }
        public void DrawIndexed(EPrimitiveType topology, uint BaseVertex, uint StartIndex, uint DrawCount, uint Instance = 1)
        {
            mCoreObject.DrawIndexed(topology, BaseVertex, StartIndex, DrawCount, Instance);
        }
        public void IndirectDrawIndexed(EPrimitiveType topology, TtBuffer indirectArg, uint indirectArgOffset = 0, TtBuffer countBuffer = null)
        {
            mCoreObject.IndirectDrawIndexed(topology, indirectArg.mCoreObject, indirectArgOffset, countBuffer.mCoreObject);
        }
        public void Dispatch(uint x, uint y, uint z)
        {
            mCoreObject.Dispatch(x, y, z);
        }
        public void IndirectDispatch(TtBuffer indirectArg, uint indirectArgOffset)
        {
            mCoreObject.IndirectDispatch(indirectArg.mCoreObject, indirectArgOffset);
        }
        public void CopyBufferRegion(TtBuffer target, ulong DstOffset, TtBuffer src, ulong SrcOffset, ulong Size)
        {
            mCoreObject.CopyBufferRegion(target.mCoreObject, DstOffset, src.mCoreObject, SrcOffset, Size);
        }
        public void CopyTextureRegion(TtTexture target, uint tarSubRes, uint DstX, uint DstY, uint DstZ, TtTexture src, uint srcSubRes, in EngineNS.NxRHI.FSubresourceBox box)
        {
            mCoreObject.CopyTextureRegion(target.mCoreObject, tarSubRes, DstX, DstY, DstZ, src.mCoreObject, srcSubRes, in box);
        }
        public unsafe void CopyTexture(TtTexture target, uint tarSubRes, TtTexture src, uint srcSubRes)
        {
            mCoreObject.CopyTextureRegion(target.mCoreObject, tarSubRes, 0, 0, 0, src.mCoreObject, srcSubRes, (NxRHI.FSubresourceBox*)IntPtr.Zero.ToPointer());
        }
        public unsafe void WriteBufferUINT32(uint Count, EngineNS.NxRHI.FBufferWriter* BufferWriters)
        {
            mCoreObject.WriteBufferUINT32(Count, BufferWriters);
        }
        #endregion

        #region PushDrawcall
        public uint DrawcallNumber
        {
            get
            {
                return mCoreObject.GetDrawcallNumber();
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtCommandList), nameof(FlushDraws));
                return mScopeTick;
            }
        }
        public void FlushDraws()
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                mCoreObject.FlushDraws();
            }   
        }
        //public void ResetGpuDraws()
        //{
        //    mCoreObject.ResetGpuDraws();
        //}
        //public void PushGpuDraw(IGpuDraw draw)
        //{
        //    mCoreObject.PushGpuDraw(draw);
        //}
        //public void PushGpuDraw(IGraphicDraw draw)
        //{
        //    mCoreObject.PushGpuDraw(draw.NativeSuper);
        //}
        //public void PushGpuDraw(IComputeDraw draw)
        //{
        //    mCoreObject.PushGpuDraw(draw.NativeSuper);
        //}
        //public void PushGpuDraw(ICopyDraw draw)
        //{
        //    mCoreObject.PushGpuDraw(draw.NativeSuper);
        //}
        public void DirectGpuDraw(TtGraphicDraw draw)
        {
            mCoreObject.DirectGpuDraw(draw.mCoreObject.NativeSuper);
        }
        public void AppendDraws(TtCmdRecorder pCmdRecorder)
        {
            mCoreObject.AppendDraws(pCmdRecorder.mCoreObject);
        }
        public void PushGpuDraw(TtGraphicDraw draw)
        {
            mCoreObject.GetCmdRecorder().PushGpuDraw(draw);
        }
        public void PushGpuDraw(TtComputeDraw draw)
        {
            mCoreObject.GetCmdRecorder().PushGpuDraw(draw);
        }
        public void PushGpuDraw(TtRayTracingDraw draw)
        {
            mCoreObject.GetCmdRecorder().PushGpuDraw(draw);
        }
        public void PushGpuDraw(TtCopyDraw draw)
        {
            mCoreObject.PushGpuDraw(draw.mCoreObject);
        }
        public void PushGpuDraw(TtActionDraw draw)
        {
            mCoreObject.GetCmdRecorder().PushGpuDraw(draw);
        }
        public void PushGpuDraw(IActionDraw draw)
        {
            mCoreObject.GetCmdRecorder().PushGpuDraw(draw);
        }
        public unsafe void PushAction(IActionDraw.FDelegate_OnActionDraw action, void* arg,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            var draw = TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.CreateActionDraw(filePath, lineNumber);
            draw.OnActionDraw = action;
            draw.Arg = arg;
            mCoreObject.PushGpuDraw(draw);
            draw.NativeSuper.NativeSuper.Release();
        }
        #endregion
    }

    public struct TtCmdListScope : IDisposable
    {
        TtCommandList mCmdList;
        public TtCmdListScope(TtCommandList cmdlist, string name)
        {
            if (name == null)
            {
                var stackTrace = new StackTrace();
                var method = stackTrace.GetFrame(1)?.GetMethod();
                if (method != null)
                {
                    name = method.DeclaringType.FullName + "." + method.Name;
                }
            }
            mCmdList = cmdlist;
            mCmdList.BeginCommand();
            mCmdList.BeginEvent(name);
        }
        public void Dispose()
        {
            mCmdList.EndEvent();
            mCmdList.EndCommand();
            
            mCmdList = null;
        }
    }

    // 持久对象: 按 pass 名字唯一, 只做统计与 frame scope 池管理;
    // 真正的 GPU 采样在 TtGpuFrameScope(每帧每次记录一份)里做
    public class TtGpuScope : IDisposable
    {
        public string Name;
        // 最近一次结算完成的那一帧里, 同名 pass 多条记录的合计
        public ulong TimeUS { get; private set; }
        // EMA 平均, 单帧值噪声大, 面板按这个排序(样本是单条记录, Rec>1 时与 TimeUS 不同量级)
        public double AvgTimeUS { get; private set; }
        // 最近 PeakWindow 个样本的最大值
        public ulong PeakTimeUS { get; private set; }
        public uint SampleCount { get; private set; }
        public uint RecordCount { get; private set; }
        // 最近一次被命中的时刻(Stopwatch 单调毫秒): 场景切走/节点关了之后这个 pass 不再录制,
        // 由 TtGpuTimeScopeManager.TickExpire 把它从表里摘掉, 面板不留陈旧读数。
        // 不能用 TtEngine.ElapsedSecond: 那是每帧重算的增量, 两帧相减永远是个极小值
        public long LastHitTickMS { get; internal set; }
        public TtGpuScope Parent;
        [ThreadStatic]
        public static TtGpuScope CurrentScope = null;
    
        // 采样实例的分配/归还全交给 TtGpuFrameScopePool, scope 自己不再持有列表
        internal TtGpuFrameScopePool Pool;
        readonly object mLocker = new object();
        ulong mFrameIndex = 0;
        // TimeUS 属于哪个录制帧必须单独记: 结算比录制晚 1~2 帧才到,
        // 拿它跟 mFrameIndex 比会永远不相等, TimeUS 就再也写不回去了
        ulong mTimeUSFrameIndex = 0;
        // 峰值用滑动窗口而不是永久最大, 否则跑几分钟后就永久卡在偶发抖动上
        const int PeakWindow = 60;
        readonly ulong[] mRecentPeaks = new ulong[PeakWindow];
        int mPeakCursor = 0;
        int mPeakCount = 0;
    
        public TtGpuFrameScope AcquireForRecord([CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            var pool = Pool;
            if (pool == null)
                return null;
            LastHitTickMS = Support.TtTime.GetTickCount();
            var frameIndex = TtEngine.Instance?.ProfilerModule?.GpuFrameContainerManager?.FrameIndex ?? 0;
            lock (mLocker)
            {
                if (mFrameIndex != frameIndex)
                {
                    // 这里只清本帧计数器, 绝不能把 IsInUse 归位——
                    // 上一帧还没过 fence 的采样还在容器里, 提前复用会覆盖它的 readback
                    mFrameIndex = frameIndex;
                    RecordCount = 0;
                }
                var result = pool.Acquire(this, filePath, lineNumber);
                if (result != null)
                    RecordCount++;
                return result;
            }
        }
        internal void OnFrameScopeFinished(TtGpuFrameScope scope, ulong timeUS)
        {
            lock (mLocker)
            {
                // 按“这条采样属于哪一帧”归属: 同一帧的多个同名记录求和,
                // 遇到新一帧的首个样本则重开合计
                if (scope.RecordFrameIndex != mTimeUSFrameIndex)
                {
                    mTimeUSFrameIndex = scope.RecordFrameIndex;
                    TimeUS = timeUS;
                }
                else
                {
                    TimeUS += timeUS;
                }
                if (SampleCount == 0)
                    AvgTimeUS = timeUS;
                else
                    AvgTimeUS = AvgTimeUS * 0.9 + (double)timeUS * 0.1;
                mRecentPeaks[mPeakCursor] = timeUS;
                mPeakCursor = (mPeakCursor + 1) % PeakWindow;
                if (mPeakCount < PeakWindow)
                    mPeakCount++;
                ulong peak = 0;
                for (int i = 0; i < mPeakCount; i++)
                {
                    if (mRecentPeaks[i] > peak)
                        peak = mRecentPeaks[i];
                }
                PeakTimeUS = peak;
                SampleCount++;
                // IsInUse 由 pool 归还时复位; scope 被淘汰后在途样本也只会写回垃圾对象
            }
        }
        public void Dispose()
        {
            // 实例所有权在 TtGpuFrameScopePool, 这里一个都不能 Dispose:
            // 同一个实例可能正被别的 scope 借走
            Pool = null;
        }
    }
    
    // 一次 pass 记录一份: 各自持有一份原生 query heap + readback buffer,
    // 所以同帧同名多次记录、以及多帧 in-flight 都不会互相覆盖
    public class TtGpuFrameScope : AuxPtrType<NxRHI.IGpuScope>, IGpuFrameObject
    {
        public TtGpuScope Owner;
        public bool IsInUse = false;
        public ulong RecordFrameIndex = 0;
        internal TtGpuFrameScopePool Pool;
        ulong mBeginFenceValue = 0;
        bool mPushedCurrent = false;
        // 事件流里这条 pass 的占位; 结算回来再回填时长
        EngineNS.Profiler.Trace.TtTraceSession.TtTraceGpuHandle mTraceHandle;
    
        // 名字不在这里设: 由 pool 借出时决定, 复用不重设
        public static TtGpuFrameScope Create(TtGpuFrameScopePool pool, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            var rc = TtEngine.Instance?.GfxDevice?.RenderContext;
            if (rc == null)
                return null;
            var result = new TtGpuFrameScope();
            result.mCoreObject = rc.mCoreObject.CreateGpuScope(filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            result.Pool = pool;
            return result;
        }
        void ReleaseSelf()
        {
            var pool = Pool;
            if (pool != null)
            {
                pool.Release(this);
                return;
            }
            IsInUse = false;
            Owner = null;
        }
        ulong GetFrameFenceExpect()
        {
            return TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetFrameFence().GetExpectValue();
        }
        public void BeginRecord(TtCommandList cmdlist)
        {
            RecordFrameIndex = TtEngine.Instance.ProfilerModule.GpuFrameContainerManager.FrameIndex;
            mBeginFenceValue = GetFrameFenceExpect();
            var owner = Owner;
            // 占位必须按提交顺序: 封帧时 GPU 事件的时间轴就是按这个顺序累加出来的
            mTraceHandle = EngineNS.Profiler.Trace.TtTraceSession.Instance.BeginGpuEvent(owner != null ? owner.Name : "GPU Pass");
            mCoreObject.Begin(cmdlist.mCoreObject);
            if (owner != null)
            {
                owner.Parent = TtGpuScope.CurrentScope;
                TtGpuScope.CurrentScope = owner;
                mPushedCurrent = true;
            }
        }
        public void EndRecord(TtCommandList cmdlist, TtGpuFrameContainer container)
        {
            mCoreObject.End(cmdlist.mCoreObject);
            PopCurrent();
            // Begin/End 的 fence 期望值必须相等: 不等就说明这条记录跨帧了,
            // 差值会变成几百毫秒的假值, 直接丢弃不参与统计
            if (GetFrameFenceExpect() != mBeginFenceValue || container == null)
            {
                TraceCancel();
                ReleaseSelf();
                return;
            }
            container.Push(this);
        }
        void TraceFinish(int durUS, ulong beginTicks, ulong endTicks, ulong freq)
        {
            if (mTraceHandle.Valid == false)
                return;
            EngineNS.Profiler.Trace.TtTraceSession.Instance.EndGpuEvent(in mTraceHandle, durUS, beginTicks, endTicks, freq);
            mTraceHandle = default;
        }
        // 采样不参与统计时事件也要作废, 否则 Timeline 上会留一条永远不结束的条
        void TraceCancel()
        {
            if (mTraceHandle.Valid == false)
                return;
            EngineNS.Profiler.Trace.TtTraceSession.Instance.CancelGpuEvent(in mTraceHandle);
            mTraceHandle = default;
        }
        public void AbortRecord()
        {
            TraceCancel();
            PopCurrent();
            ReleaseSelf();
        }
        void PopCurrent()
        {
            if (mPushedCurrent == false)
                return;
            mPushedCurrent = false;
            var owner = Owner;
            if (owner != null && TtGpuScope.CurrentScope == owner)
                TtGpuScope.CurrentScope = owner.Parent;
        }
        public void OnGpuFinished()
        {
            var owner = Owner;
            var rc = TtEngine.Instance?.GfxDevice?.RenderContext;
            // 关闭期可能已经释放了设备或原生 scope, 这里不能再当可用对象用
            if (owner == null || rc == null || mCoreObject.IsValidPointer == false)
            {
                TraceCancel();
                ReleaseSelf();
                return;
            }
            var delta = mCoreObject.GetDeltaTime();
            var freq = rc.mCoreObject.GetCmdQueue().mDefaultQueueFrequence;
            var timeUS = freq == 0 ? 0 : (delta * 1000000) / freq;
            // 时间戳一起送过去: 事件流用它把 pass 放到真实位置, 没有时间戳时自然退回 delta 路径
            TraceFinish(timeUS > int.MaxValue ? int.MaxValue : (int)timeUS,
                mCoreObject.GetBeginTime(), mCoreObject.GetEndTime(), freq);
            owner.OnFrameScopeFinished(this, timeUS);
            ReleaseSelf();
        }
        // 容器被异常丢弃: 只归还实例, 不参与统计
        public void OnGpuDiscarded()
        {
            TraceCancel();
            ReleaseSelf();
        }
        // 命令列表回收时清理悬挂的 pass 采样(不碰 CurrentScope, 避免跨线程栈污染)
        internal void Abandon()
        {
            TraceCancel();
            ReleaseSelf();
        }
    }
    
    // 全局采样实例池: 所有 TtGpuScope 共用, 谁要谁借, GPU 结算完自动归还。
    // 高水位只有一处, 所以 shrink 只需一个策略; scope 名字被淘汰时也不会留下永久占用
    public class TtGpuFrameScopePool : IDisposable
    {
        readonly Stack<TtGpuFrameScope> mFree = new Stack<TtGpuFrameScope>();
        readonly object mLocker = new object();
        int mInFlight = 0;
        // 本帧观察到的在途峰值(每借一个刷新一次), 帧末推进滚动窗口
        int mFramePeak = 0;
        // 生命周期峰值, 只用于诊断: 与最近峰值差很多就说明曾经有过尖峰
        int mPeakInFlight = 0;
        // 池容量目标: 最近一段时间的在途峰值 + 余量。超过它就回收, 不足就靠扩容顶上
        int mCapacity = 0;
        // 滚动窗口: 最近 PeakWindowFrames 帧的每帧在途峰值, 容量取它们的最大值
        const int PeakWindowFrames = 180;
        readonly int[] mRecentFramePeaks = new int[PeakWindowFrames];
        int mPeakCursor = 0;
        int mPeakCount = 0;
        // 容量下限: 空空如也时也留这么多, 避免个别帧视口数抖动就反复创建
        const int MinCapacity = 32;
        // native 名字只给抓帧辨识用(面板读的是托管 TtGpuScope.Name),
        // 关掉后连新建那一次的 SetName 也省掉
        public bool EnableNativeName { get; set; } = true;
        public int InFlight { get { lock (mLocker) { return mInFlight; } } }
        public int PeakInFlight { get { lock (mLocker) { return mPeakInFlight; } } }
        public int FreeCount { get { lock (mLocker) { return mFree.Count; } } }
        public int TotalCount { get { lock (mLocker) { return mFree.Count + mInFlight; } } }
        public int Capacity { get { lock (mLocker) { return mCapacity; } } }
        public TtGpuFrameScope Acquire(TtGpuScope owner, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            lock (mLocker)
            {
                TtGpuFrameScope result;
                if (mFree.Count > 0)
                {
                    // 复用时不重设 native 名字: 每帧上千次 SetName(含宽字符串转换 + SetPrivateData) 纯浪费,
                    // 代价只是抓帧里 heap 名字会停在它第一次被用时的那个 pass
                    result = mFree.Pop();
                }
                else
                {
                    // 没有硬上限: 需求涨了就扩容。采样不允许因为"池不够"静默丢结果,
                    // 容量控制交给 TickShrink 在需求回落时回收
                    result = TtGpuFrameScope.Create(this, filePath, lineNumber);
                    if (result == null)
                        return null;
                    if (EnableNativeName)
                        result.mCoreObject.SetName(owner.Name);
                }
                result.Owner = owner;
                result.IsInUse = true;
                mInFlight++;
                if (mInFlight > mFramePeak)
                    mFramePeak = mInFlight;
                if (mInFlight > mPeakInFlight)
                    mPeakInFlight = mInFlight;
                return result;
            }
        }
        public void Release(TtGpuFrameScope scope)
        {
            if (scope == null)
                return;
            scope.IsInUse = false;
            scope.Owner = null;
            lock (mLocker)
            {
                mInFlight--;
                mFree.Push(scope);
            }
        }
        // 每帧一次。容量 = 最近 PeakWindowFrames 帧的在途峰值 + 余量, 多出来的空闲实例回收掉。
        // 不设硬上限: 需求上涨就扩容, 这里只负责在需求回落后把显存收回来
        public int TickShrink()
        {
            lock (mLocker)
            {
                mRecentFramePeaks[mPeakCursor] = mFramePeak;
                mPeakCursor = (mPeakCursor + 1) % PeakWindowFrames;
                if (mPeakCount < PeakWindowFrames)
                    mPeakCount++;
                mFramePeak = mInFlight;

                int recentPeak = 0;
                for (int i = 0; i < mPeakCount; i++)
                {
                    if (mRecentFramePeaks[i] > recentPeak)
                        recentPeak = mRecentFramePeaks[i];
                }
                if (recentPeak < MinCapacity)
                    recentPeak = MinCapacity;
                // 留 1/8 + MinCapacity 的余量: 刚好贴着峰值的话, 下一帧稍抖一下又要重新 Create
                mCapacity = recentPeak + Math.Max(recentPeak / 8, MinCapacity);

                int total = mFree.Count + mInFlight;
                if (total <= mCapacity)
                    return 0;
                // 只回收空闲的; 在途的那部分要等 GPU 结算, 下一帧还会再评估一次
                int toRemove = Math.Min(total - mCapacity, mFree.Count);
                int removed = 0;
                while (removed < toRemove)
                {
                    mFree.Pop().Dispose();
                    removed++;
                }
                return removed;
            }
        }
        public void Dispose()
        {
            lock (mLocker)
            {
                while (mFree.Count > 0)
                {
                    mFree.Pop().Dispose();
                }
                mInFlight = 0;
                mPeakInFlight = 0;
                mCapacity = 0;
            }
        }
    }
    
    public struct TtGpuScopeHelper : IDisposable//Waiting for C#8 ,ref struct -> Dispose
    {
        public TtGpuFrameScope mFrameScope;
        public TtCommandList mCmdList;
        public TtGpuScopeHelper(TtGpuScope t, TtCommandList cmdlist)
        {
            mFrameScope = null;
            mCmdList = cmdlist;
            if (t == null)
                return;
            mFrameScope = t.AcquireForRecord();
            if (mFrameScope == null)
                return;
            mFrameScope.BeginRecord(cmdlist);
        }
        public void Dispose()
        {
            if (mFrameScope == null)
                return;
            mFrameScope.EndRecord(mCmdList, TtEngine.Instance.ProfilerModule.GpuFrameContainerManager.Current);
            mFrameScope = null;
            mCmdList = null;
        }
    }
    
    public class TtGpuTimeScopeManager : IDisposable
    {
        public bool IsGpuProfiling { get; set; } = true;
        public Dictionary<string, TtGpuScope> Scopes { get; } = new Dictionary<string, TtGpuScope>();
        // 所有 scope 的采样实例都从这一个池里借, 生命周期不跟 scope 绑
        public TtGpuFrameScopePool Pool { get; } = new TtGpuFrameScopePool();
        readonly object mLocker = new object();
        public TtGpuScope GetGpuScope(string name)
        {
            if (IsGpuProfiling == false || name == null)
                return null;
            lock (mLocker)
            {
                TtGpuScope scope;
                if (Scopes.TryGetValue(name, out scope))
                    return scope;
                scope = new TtGpuScope();
                scope.Name = name;
                scope.Pool = Pool;
                scope.LastHitTickMS = Support.TtTime.GetTickCount();
                Scopes.Add(name, scope);
                return scope;
            }
        }
        public int ScopeCount { get { lock (mLocker) { return Scopes.Count; } } }
        // 面板快照: 直接遍历 Scopes 会与 TickExpire 的 Remove 撞车(字典遍历中改元素)
        public void SnapshotScopes(List<TtGpuScope> outList)
        {
            outList.Clear();
            lock (mLocker)
            {
                foreach (var i in Scopes)
                    outList.Add(i.Value);
            }
        }
        // 超过这么久没被命中就从表里摘掉(毫秒, 0=不摘): 场景切换后旧 pass 不再渲染,
        // 面板不该继续挂着它们的陈旧读数。阈值由 EditorConfig.GpuScopeExpireSeconds 驱动
        public int ExpireMS { get; set; } = 5000;
        public int TickExpire(long nowTickMS)
        {
            if (ExpireMS <= 0 || Scopes.Count == 0)
                return 0;
            List<TtGpuScope> dead = null;
            lock (mLocker)
            {
                // 先收集再删: 字典遍历中不能改
                foreach (var i in Scopes)
                {
                    if (nowTickMS - i.Value.LastHitTickMS < ExpireMS)
                        continue;
                    if (dead == null)
                        dead = new List<TtGpuScope>();
                    dead.Add(i.Value);
                }
                if (dead != null)
                {
                    for (int i = 0; i < dead.Count; i++)
                        Scopes.Remove(dead[i].Name);
                }
            }
            if (dead == null)
                return 0;
            // 锁外释放: 在途采样可能还反引着 owner, 但它们只会写进一个已无人读取的统计对象
            for (int i = 0; i < dead.Count; i++)
                dead[i].Dispose();
            return dead.Count;
        }
        public void Dispose()
        {
            lock (mLocker)
            {
                foreach (var i in Scopes)
                {
                    i.Value.Dispose();
                }
                Scopes.Clear();
            }
            // 实例的真正释放在池这边, 取消干净
            Pool.Dispose();
        }
    }

    public class TtGpuEventScope : IDisposable
    {
        TtCommandList mCmdList;
        public unsafe TtGpuEventScope(TtCommandList cmdlist, VNameString name)
        {
            mCmdList = cmdlist;
            mCmdList.PushAction(static (EngineNS.NxRHI.ICommandList cmd, void* arg1) =>
            {
                var str = new VNameString();
                str.m_Index = (int)(arg1);
                cmd.BeginEvent(str, 0);
            }, (void*)name.Index);
        }
        public unsafe void Dispose()
        {
            mCmdList.PushAction(static (EngineNS.NxRHI.ICommandList cmd, void* arg1) =>
            {
                cmd.EndEvent();
            }, IntPtr.Zero.ToPointer());
            mCmdList = null;
        }
    }

    public class TtCmdListManager : IDisposable
    {
        public void Dispose()
        {
            while (mUsingCmdlists.Count > 0)
            {
                Tick();
            }
            foreach (var i in mIdleCmdlists)
            {
                i.Dispose();
            }
            mIdleCmdlists.Clear();
        }
        public Stack<TtCommandList> mIdleCmdlists = new Stack<TtCommandList>();
        public List<TtCommandList> mUsingCmdlists = new List<TtCommandList>();
        public TtCommandList GetCmdList()
        {
            lock (this)
            {
                if (mIdleCmdlists.Count == 0)
                {
                    var t = TtEngine.Instance.GfxDevice.RenderContext.CreateCommandList();
                    mIdleCmdlists.Push(t);
                }
                var result = mIdleCmdlists.Pop();
                // 上一次使用留下的悬挂采样不能带到这次
                result.AbandonCurrentGpuScope();
                mUsingCmdlists.Add(result);
                return result;
            }
        }
        public void Tick()
        {
            lock (this)
            {
                for (int i = mUsingCmdlists.Count - 1; i >= 0; i--)
                {
                    var fence = mUsingCmdlists[i].mCoreObject.GetCommitFence();
                    if (mUsingCmdlists[i].CommandListState == ECommandListState.Committed && fence.GetExpectValue() <= fence.GetCompletedValue())
                    {
                        mUsingCmdlists[i].AbandonCurrentGpuScope();
                        mUsingCmdlists[i].CommandListState = ECommandListState.None;
                        mIdleCmdlists.Push(mUsingCmdlists[i]);
                        mUsingCmdlists.RemoveAt(i);
                    }
                }
            }
        }
    }

    partial class TtGpuDevice
    {
        public TtCmdListManager CmdListManager { get; } = new TtCmdListManager();
    }
}
