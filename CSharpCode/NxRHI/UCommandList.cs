using System;
using System.Collections.Generic;
using System.Text;
using static EngineNS.NxRHI.URenderCmdQueue;

namespace EngineNS.NxRHI
{
    public class UCommandList : AuxPtrType<NxRHI.ICommandList>
    {
        public void BeginEvent(string info)
        {
            mCoreObject.BeginEvent(info);
        }
        public void EndEvent()
        {
            mCoreObject.EndEvent();
        }
        public bool BeginCommand()
        {
            return mCoreObject.BeginCommand();
        }
		public void EndCommand()
        {
            mCoreObject.EndCommand();
        }
        public void BeginPass(UFrameBuffers fb, in FRenderPassClears passClears, string name)
        {
            mCoreObject.BeginPass(fb.mCoreObject, in passClears, name);
            unsafe
            {
                mCoreObject.SetScissor(0, (NxRHI.FScissorRect*)IntPtr.Zero.ToPointer());
            }
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
        }
        public void SetShader(UShader shader)
        {
            mCoreObject.SetShader(shader.mCoreObject);
        }
        public void SetCBV(EShaderType type, FShaderBinder binder, UCbView buffer)
        {
            mCoreObject.SetCBV(type, binder, buffer.mCoreObject);
        }
        public void SetSrv(EShaderType type, FShaderBinder binder, USrView view)
        {
            mCoreObject.SetSrv(type, binder, view.mCoreObject);
        }
        public void SetUav(EShaderType type, FShaderBinder binder, UUaView view)
        {
            mCoreObject.SetUav(type, binder, view.mCoreObject);
        }
        public void SetSampler(EShaderType type, FShaderBinder binder, USampler sampler)
        {
            mCoreObject.SetSampler(type, binder, sampler.mCoreObject);
        }
        public void SetVertexBuffer(uint slot, UVbView buffer, uint Offset, uint Stride)
        {
            mCoreObject.SetVertexBuffer(slot, buffer.mCoreObject, Offset, Stride);
        }
        public void SetIndexBuffer(UIbView buffer, bool IsBit32 = false)
        {
            mCoreObject.SetIndexBuffer(buffer.mCoreObject, IsBit32);
        }
        public void SetInputLayout(UInputLayout layout)
        {
            mCoreObject.SetInputLayout(layout.mCoreObject);
        }
        
        #region Draw
        public void Draw(EPrimitiveType topology, uint BaseVertex, uint DrawCount, uint Instance = 1)
        {
            mCoreObject.Draw(topology, BaseVertex, DrawCount, Instance);
        }
        public void DrawIndexed(EPrimitiveType topology, uint BaseVertex, uint StartIndex, uint DrawCount, uint Instance = 1)
        {
            mCoreObject.DrawIndexed(topology, BaseVertex, StartIndex, DrawCount, Instance);
        }
        public void IndirectDrawIndexed(EPrimitiveType topology, UBuffer indirectArg, uint indirectArgOffset = 0)
        {
            mCoreObject.IndirectDrawIndexed(topology, indirectArg.mCoreObject, indirectArgOffset);
        }
        public void Dispatch(uint x, uint y, uint z)
        {
            mCoreObject.Dispatch(x, y, z);
        }
        public void CopyBufferRegion(UBuffer target, ulong DstOffset, UBuffer src, ulong SrcOffset, ulong Size)
        {
            mCoreObject.CopyBufferRegion(target.mCoreObject, DstOffset, src.mCoreObject, SrcOffset, Size);
        }
        public void CopyTextureRegion(UTexture target, uint tarSubRes, uint DstX, uint DstY, uint DstZ, UTexture src, uint srcSubRes, in EngineNS.NxRHI.FSubresourceBox box)
        {
            mCoreObject.CopyTextureRegion(target.mCoreObject, tarSubRes, DstX, DstY, DstZ, src.mCoreObject, srcSubRes, in box);
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
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UCommandList), nameof(FlushDraws));
        public void FlushDraws()
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                mCoreObject.FlushDraws();
            }   
        }
        public void ResetGpuDraws()
        {
            mCoreObject.ResetGpuDraws();
        }
        public void PushGpuDraw(IGpuDraw draw)
        {
            mCoreObject.PushGpuDraw(draw);
        }
        public void PushGpuDraw(IGraphicDraw draw)
        {
            mCoreObject.PushGpuDraw(draw.NativeSuper);
        }
        public void PushGpuDraw(IComputeDraw draw)
        {
            mCoreObject.PushGpuDraw(draw.NativeSuper);
        }
        public void PushGpuDraw(ICopyDraw draw)
        {
            mCoreObject.PushGpuDraw(draw.NativeSuper);
        }
        //[ThreadStatic]
        //private static Profiler.TimeScope ScopePushGpuDraw = Profiler.TimeScopeManager.GetTimeScope(typeof(UCommandList), nameof(PushGpuDraw));
        public void PushGpuDraw(UGraphicDraw draw)
        {
            PushGpuDraw(draw.mCoreObject);
        }
        public void PushGpuDraw(UComputeDraw draw)
        {
            PushGpuDraw(draw.mCoreObject);
        }
        public void PushGpuDraw(UCopyDraw draw)
        {
            PushGpuDraw(draw.mCoreObject);
        }
        #endregion
    }

    public class URenderCmdQueue : ITickable
    {
        public delegate void FRenderCmd(ICommandList ImCmdlist, string name);
        public struct FRCmdInfo
        {
            public FRenderCmd Cmd;
            public string Name;
        }
        public class UQueueStat
        {
            public uint NumOfCmdlist;
            public uint NumOfDrawcall;
            public uint NumOfPrimitive;
            public void Reset()
            {
                NumOfCmdlist = 0;
                NumOfDrawcall = 0;
                NumOfPrimitive = 0;
            }
        }
        public UQueueStat GetStat()
        {
            return QueueStats[1];
        }
        public UQueueStat[] QueueStats = new UQueueStat[2];
        public readonly Queue<FRCmdInfo>[] RenderCmds = new Queue<FRCmdInfo>[2];
        public readonly Queue<FRCmdInfo>[] RenderPreCmds = new Queue<FRCmdInfo>[2];
        public readonly Queue<FRCmdInfo>[] RenderPostCmds = new Queue<FRCmdInfo>[2];
        public URenderCmdQueue()
        {
            QueueStats[0] = new UQueueStat();
            QueueStats[1] = new UQueueStat();

            RenderCmds[0] = new Queue<FRCmdInfo>();
            RenderCmds[1] = new Queue<FRCmdInfo>();

            RenderPreCmds[0] = new Queue<FRCmdInfo>();
            RenderPreCmds[1] = new Queue<FRCmdInfo>();

            RenderPostCmds[0] = new Queue<FRCmdInfo>();
            RenderPostCmds[1] = new Queue<FRCmdInfo>();
        }
        public void Reset()
        {
            //RenderCmds[0]?.Clear();
            //RenderCmds[1]?.Clear();
            //RenderPreCmds[0]?.Clear();
            //RenderPreCmds[1]?.Clear();
            //RenderPostCmds[0]?.Clear();
            //RenderPostCmds[1]?.Clear();
            //UEngine.Instance.GfxDevice.RenderContext.CmdQueue.Flush();
            //RenderCmds[0]?.Clear();
            //RenderCmds[1]?.Clear();
            //RenderPreCmds[0]?.Clear();
            //RenderPreCmds[1]?.Clear();
            //RenderPostCmds[0]?.Clear();
            //RenderPostCmds[1]?.Clear();

            TickRender(0);
            TickSync(0);
            TickRender(0);
            TickSync(0);

            UEngine.Instance.GfxDevice.RenderContext.CmdQueue.Flush();
            var count = RenderCmds[0].Count + RenderCmds[1].Count + RenderPreCmds[0].Count + RenderPreCmds[1].Count + RenderPostCmds[0].Count + RenderPostCmds[0].Count;
            System.Diagnostics.Debug.Assert(count == 0);
        }
        public void QueueCmd(FRenderCmd cmd, string name)
        {
            lock (RenderCmds)
            {
                var info = new FRCmdInfo();
                info.Name = name;
                info.Cmd = cmd;
                RenderCmds[0].Enqueue(info);
            }
        }
        public void QueuePreCmd(FRenderCmd cmd, string name)
        {
            lock (RenderPreCmds)
            {
                var info = new FRCmdInfo();
                info.Name = name;
                info.Cmd = cmd;
                RenderPreCmds[0].Enqueue(info);
            }
        }
        public void QueuePostCmd(FRenderCmd cmd, string name)
        {
            lock (RenderPostCmds)
            {
                var info = new FRCmdInfo();
                info.Name = name;
                info.Cmd = cmd;
                RenderPostCmds[0].Enqueue(info);
            }
        }
        public void QueueCmdlist(UCommandList cmd, string name = null)
        {
            lock (RenderCmds)
            {
                var info = new FRCmdInfo();
                info.Name = name;
                info.Cmd = (im_cmd, name) =>
                {
                    UEngine.Instance.GfxDevice.RenderContext.CmdQueue.ExecuteCommandList(cmd);
                };
                RenderCmds[0].Enqueue(info);
                QueueStats[0].NumOfDrawcall += cmd.mCoreObject.GetDrawcallNumber();
                QueueStats[0].NumOfCmdlist++;
                QueueStats[0].NumOfPrimitive += cmd.mCoreObject.GetPrimitiveNumber();
            }
        }
        public void TickLogic(int ellapse)
        {

        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeRenderTick = Profiler.TimeScopeManager.GetTimeScope(typeof(URenderCmdQueue), nameof(TickRender));
        public void TickRender(int ellapse)
        {
            var cmdQueue = UEngine.Instance.GfxDevice.RenderContext.CmdQueue;
            
            using (new Profiler.TimeScopeHelper(ScopeRenderTick))
            {
                var im_cmd = cmdQueue.GetIdleCmdlist(EQueueCmdlist.QCL_Read);
                TickAways(im_cmd);
                cmdQueue.ReleaseIdleCmdlist(im_cmd, EQueueCmdlist.QCL_Read);
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeSyncTick = Profiler.TimeScopeManager.GetTimeScope(typeof(URenderCmdQueue), nameof(TickRender));
        public void TickSync(int ellapse)
        {
            using (new Profiler.TimeScopeHelper(ScopeRenderTick))
            {
                var cmdQueue = UEngine.Instance.GfxDevice.RenderContext.CmdQueue;
                var im_cmd = cmdQueue.GetIdleCmdlist(EQueueCmdlist.QCL_FramePost);
                im_cmd.EndCommand();
                cmdQueue.mCoreObject.ExecuteCommandList(im_cmd);
                im_cmd.BeginCommand();
                cmdQueue.ReleaseIdleCmdlist(im_cmd, EQueueCmdlist.QCL_FramePost);

                //UEngine.Instance.EventPoster.TickPostTickSyncEvents();
                //UEngine.Instance.GfxDevice.RenderContext.TickPostEvents();

                Swap(ref RenderCmds[0], ref RenderCmds[1]);
                Swap(ref RenderPreCmds[0], ref RenderPreCmds[1]);
                Swap(ref RenderPostCmds[0], ref RenderPostCmds[1]);
                Swap(ref QueueStats[0], ref QueueStats[1]);
                QueueStats[0].Reset();
                //{
                //    var save = RenderCmds[0];
                //    RenderCmds[0] = RenderCmds[1];
                //    RenderCmds[1] = save;
                //}
                //{
                //    var save = RenderPreCmds[0];
                //    RenderPreCmds[0] = RenderPreCmds[1];
                //    RenderPreCmds[1] = save;
                //}
                //{
                //    var save = RenderPostCmds[0];
                //    RenderPostCmds[0] = RenderPostCmds[1];
                //    RenderPostCmds[1] = save;
                //}
            }
        }
        public void Swap<T>(ref T l, ref T r)
        {
            var save = l;
            l = r;
            r = save;
        }
        public void TickAways(ICommandList ImCmdlist)
        {
            var curCmds = RenderPreCmds[1];
            while (curCmds.Count > 0)
            {
                try
                {
                    FRCmdInfo cmd;
                    lock (RenderPreCmds)
                    {
                        cmd = curCmds.Peek();
                        curCmds.Dequeue();
                    }
                    cmd.Cmd(ImCmdlist, cmd.Name);
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
            }

            curCmds = RenderCmds[1];
            while (curCmds.Count > 0)
            {
                try
                {
                    FRCmdInfo cmd;
                    lock (RenderCmds)
                    {
                        cmd = curCmds.Peek();
                        curCmds.Dequeue();
                    }
                    cmd.Cmd(ImCmdlist, cmd.Name);
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
            }

            curCmds = RenderPostCmds[1];
            while (curCmds.Count > 0)
            {
                try
                {
                    FRCmdInfo cmd;
                    lock (RenderPostCmds)
                    {
                        cmd = curCmds.Peek();
                        curCmds.Dequeue();
                    }
                    cmd.Cmd(ImCmdlist, cmd.Name);
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
            }
        }
    }
}
