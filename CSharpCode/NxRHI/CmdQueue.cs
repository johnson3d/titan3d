using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public delegate void FRenderCmd(ICommandList ImCmdlist, ref FRCmdInfo info);
    public struct FRCmdInfo
    {
        public FRenderCmd Cmd;
        public EQueueType QueueType;
        public string Name;
        public object Tag;
    }

    public class TtRCmdQueue
    {
        public Queue<FRCmdInfo> Cmds = new Queue<FRCmdInfo>();
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
        public UQueueStat QueueStats = new UQueueStat();

        public void QueueCmd(FRenderCmd cmd, string name, object tag = null, NxRHI.EQueueType qType = EQueueType.QU_Default)
        {
            var info = new FRCmdInfo();
            info.Name = name;
            info.QueueType = qType;
            info.Cmd = cmd;
            info.Tag = tag;
            Cmds.Enqueue(info);
        }
        public void QueueCmdlist(UCommandList cmd, string name = null, EQueueType qType = EQueueType.QU_Default)
        {
            System.Diagnostics.Debug.Assert(cmd.mCoreObject.IsRecording() == false);
            var info = new FRCmdInfo();
            info.QueueType = qType;
            info.Name = name;
            info.Tag = cmd;
            info.Cmd = static (NxRHI.ICommandList im_cmd, ref FRCmdInfo info) =>
            {
                UEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList(info.Tag as UCommandList, info.QueueType);
            };
            Cmds.Enqueue(info);

            QueueStats.NumOfDrawcall += cmd.mCoreObject.GetDrawcallNumber();
            QueueStats.NumOfCmdlist++;
            QueueStats.NumOfPrimitive += cmd.mCoreObject.GetPrimitiveNumber();
        }

        public void Execute(ICommandList ImCmdlist)
        {
            var curCmds = Cmds;
            while (curCmds.Count > 0)
            {
                try
                {
                    NxRHI.FRCmdInfo cmd;

                    cmd = curCmds.Peek();
                    curCmds.Dequeue();

                    cmd.Cmd(ImCmdlist, ref cmd);
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
            }
        }
    }
    public class TtRenderSwapQueue
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public TtRCmdQueue.UQueueStat GetStat()
        {
            return RenderCmds[1].QueueStats;
        }
        public readonly TtRCmdQueue[] RenderCmds = new TtRCmdQueue[2];
        public TtRenderSwapQueue()
        {
            RenderCmds[0] = new TtRCmdQueue();
            RenderCmds[1] = new TtRCmdQueue();
        }
        public void Reset()
        {
            lock (RenderCmds)
            {
                TickRender(0);
                TickSync(0);
                TickRender(0);
                TickSync(0);

                UEngine.Instance.GfxDevice.RenderContext.GpuQueue.Flush();
                var count = RenderCmds[0].Cmds.Count + RenderCmds[1].Cmds.Count;
                System.Diagnostics.Debug.Assert(count == 0);
            }
        }
        public void QueueCmd(FRenderCmd cmd, string name, object tag = null, NxRHI.EQueueType qType = EQueueType.QU_Default)
        {
            lock (RenderCmds)
            {
                if (UEngine.Instance.Config.MultiRenderMode == EMultiRenderMode.None)
                {
                    var info = new FRCmdInfo();
                    info.Name = name;
                    info.QueueType = qType;
                    info.Cmd = cmd;
                    info.Tag = tag;
                    using (var tCmd = new FTransientCmd(EQueueType.QU_Default, "TickRender"))
                    {
                        cmd(tCmd.CmdList, ref info);
                    }
                }
                else
                {
                    RenderCmds[0].QueueCmd(cmd, name, tag, qType);
                }
            }   
        }
        public void QueueCmdlist(UCommandList cmd, string name = null, EQueueType qType = EQueueType.QU_Default)
        {
            System.Diagnostics.Debug.Assert(cmd.mCoreObject.IsRecording() == false);
            lock (RenderCmds)
            {
                if (UEngine.Instance.Config.MultiRenderMode == EMultiRenderMode.None)
                {
                    UEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList(cmd, qType);
                }
                else
                {
                    RenderCmds[0].QueueCmdlist(cmd, name, qType);
                }
            }
        }
        public void TickLogic(float ellapse)
        {

        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeRenderTick = Profiler.TimeScopeManager.GetTimeScope(typeof(TtRenderSwapQueue), nameof(TickRender));
        public void TickRender(float ellapse)
        {
            var cmdQueue = UEngine.Instance.GfxDevice.RenderContext.GpuQueue;

            using (new Profiler.TimeScopeHelper(ScopeRenderTick))
            {
                using (var cmd = new FTransientCmd(EQueueType.QU_Default, "TickRender"))
                {
                    TickRenderImpl(cmd.CmdList);
                }
            }
        }
        private void TickRenderImpl(ICommandList ImCmdlist)
        {
            if (UEngine.Instance.Config.MultiRenderMode == EMultiRenderMode.Queue)
            {
                var curCmds = RenderCmds[0].Cmds;
                while (true)
                {
                    if (curCmds.Count > 0)
                    {
                        try
                        {
                            FRCmdInfo cmd;
                            lock (RenderCmds)
                            {
                                cmd = curCmds.Peek();
                                curCmds.Dequeue();
                            }
                            cmd.Cmd(ImCmdlist, ref cmd);
                            if (cmd.Name == "#TickLogicEnd#")
                            {
                                System.Diagnostics.Debug.Assert(curCmds.Count == 0);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Profiler.Log.WriteException(ex);
                        }
                    }
                }
            }
            else if (UEngine.Instance.Config.MultiRenderMode == EMultiRenderMode.QueueNextFrame)
            {
                RenderCmds[1].Execute(ImCmdlist);
            }
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeSyncTick = Profiler.TimeScopeManager.GetTimeScope(typeof(TtRenderSwapQueue), nameof(TickRender));
        public void TickSync(float ellapse)
        {
            using (new Profiler.TimeScopeHelper(ScopeRenderTick))
            {
                var cmdQueue = UEngine.Instance.GfxDevice.RenderContext.GpuQueue;

                Swap(ref RenderCmds[0], ref RenderCmds[1]);
                RenderCmds[0].QueueStats.Reset();
            }
        }
        public void Swap<T>(ref T l, ref T r)
        {
            var save = l;
            l = r;
            r = save;
        }
        public bool CaptureRenderDocFrame = false;
        public bool BeginFrameCapture()
        {
            if (CaptureRenderDocFrame == false)
            {
                return false;
            }
            if (IRenderDocTool.GetInstance().IsFrameCapturing())
            {
                return false;
            }
            IRenderDocTool.GetInstance().StartFrameCapture();
            return true;
        }
        public string EndFrameCapture(string tagName = null, bool openRenderDoc = true)
        {
            if (CaptureRenderDocFrame == false)
            {
                return null;
            }
            //System.Diagnostics.Debug.Assert(IRenderDocTool.GetInstance().IsFrameCapturing());
            CaptureRenderDocFrame = false;
            IRenderDocTool.GetInstance().EndFrameCapture();

            ulong timeStamp = 0;
            var idx = IRenderDocTool.GetInstance().GetNumCaptures() - 1;
            var file = IRenderDocTool.GetInstance().GetCapture(idx, ref timeStamp);
            if (!string.IsNullOrEmpty(file) && IO.TtFileManager.FileExists(file))
            {
                var extName = (tagName != null) ? "_" + tagName : "";
                var tarFile = IO.TtFileManager.GetPureName(file) + $"{extName}.rdc";
                var absTarFile = UEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.RenderDoc) + tarFile;
                try
                {
                    System.IO.File.Move(file, absTarFile, true);
                }
                catch (System.Exception e)
                {
                    if (openRenderDoc)
                    {
                        IRenderDocTool.GetInstance().OpenFile(file);
                    }
                    Profiler.Log.WriteException(e);
                }
                finally
                {
                    if (openRenderDoc)
                    {
                        IRenderDocTool.GetInstance().OpenFile(absTarFile);
                    }
                }
                return tarFile;
            }
            return null;
        }
    }
}
