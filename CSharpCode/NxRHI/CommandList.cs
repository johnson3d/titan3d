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
        private NxRHI.TtGpuScope CurrentGpuScope;
        public unsafe bool BeginPass(TtFrameBuffers fb, in FRenderPassClears passClears, string name)
        {
            CurrentGpuScope = TtEngine.Instance.ProfilerModule.GpuTimeScopeManager.GetGpuScope(name);
            if (CurrentGpuScope != null)
                CurrentGpuScope.Begin(this);

            var cp = passClears;
            if (TtEngine.Instance.GfxDevice.Config.IsReverseZ)
                cp.m_DepthClearValue = 0;
            else
                cp.m_DepthClearValue = 1;
            var ret = mCoreObject.BeginPass(fb.mCoreObject, in cp, name);
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
                CurrentGpuScope.End(this);
            CurrentGpuScope = null;
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
        public unsafe void PushAction(IActionDraw.FDelegate_OnActionDraw action, void* arg)
        {
            var draw = TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.CreateActionDraw();
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

    public class TtGpuScope : AuxPtrType<NxRHI.IGpuScope>
    {
        ulong ScopeFrameValue = 0;
        public ulong TimeUS { get; private set; }
        public string Name
        {
            get => mCoreObject.GetName();
            set
            {
                mCoreObject.SetName(value);
            }
        }
        public bool IsFinished()
        {
            return mCoreObject.IsFinished();
        }
        public ulong GetDeltaTime()
        {
            return mCoreObject.GetDeltaTime();
        }
        public TtGpuScope Parent;
        [ThreadStatic]
        public static TtGpuScope CurrentScope = null;
        public void Begin(TtCommandList cmdlist)
        {
            if (ScopeFrameValue == 0)
            {
                mCoreObject.Begin(cmdlist.mCoreObject);
                Parent = CurrentScope;
                CurrentScope = this;
            }
        }
        public void End(TtCommandList cmdlist)
        {
            if (ScopeFrameValue == 0)
            {
                mCoreObject.End(cmdlist.mCoreObject);
                ScopeFrameValue = TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetFrameFence().GetExpectValue();
            }
        }
        public void Update()
        {
            if (ScopeFrameValue != 0)
            {
                var completed = TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetFrameFence().GetCompletedValue();
                if (completed >= ScopeFrameValue)
                {
                    var delta = GetDeltaTime();

                    var freq = TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetCmdQueue().mDefaultQueueFrequence;
                    TimeUS = (delta * 1000000) / freq;

                    ScopeFrameValue = 0;
                }
            }
        }
    }

    public struct TtGpuScopeHelper : IDisposable//Waiting for C#8 ,ref struct -> Dispose
    {
        public TtGpuScope mTime;
        public TtCommandList mCmdList;
        public TtGpuScopeHelper(TtGpuScope t, TtCommandList cmdlist)
        {
            mTime = t;
            mCmdList = cmdlist;
            if (t == null)
                return;
            mTime.Begin(cmdlist);
        }
        public void Dispose()
        {
            if (mTime == null)
                return;
            mTime.End(mCmdList);
            mTime = null;
            mCmdList = null;
        }
    }

    public class TtGpuTimeScopeManager : IDisposable
    {
        public bool IsGpuProfiling { get; set; } = true;
        public Dictionary<string, TtGpuScope> Scopes { get; } = new Dictionary<string, TtGpuScope>();
        public TtGpuScope GetGpuScope(string name)
        {
            if (IsGpuProfiling == false)
                return null;
            TtGpuScope scope;
            if (Scopes.TryGetValue(name, out scope))
                return scope;
            scope = TtEngine.Instance.GfxDevice.RenderContext.CreateGpuScope();
            if (scope == null)
                return null;
            scope.Name = name;
            Scopes.Add(name, scope);
            return scope;
        }
        public void UpdateSync()
        {
            foreach(var i in Scopes)
            {
                i.Value.Update();
            }
        }
        public void Dispose()
        {
            foreach (var i in Scopes)
            {
                i.Value.Dispose();
            }
            Scopes.Clear();
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
