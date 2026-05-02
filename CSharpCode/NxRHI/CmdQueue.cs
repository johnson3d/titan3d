using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public enum ERCmdType : int
    {
        Cmd,
        Cmdlist,
        FrameEnd,
    }
    public delegate void FRenderCmd(TtRCmdQueue queue, ref FRCmdInfo info);
    public struct FRCmdInfo
    {
        public ERCmdType CmdType;
        public FRenderCmd Cmd;
        public EQueueType QueueType;
        public string Name;
        public object Tag;
    }

    public class TtRCmdQueue
    {
        #region RenderDoc Capture
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
        int CaptureId = 0;
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
                var tarFile = IO.TtFileManager.GetPureName(file) + $"_{CaptureId++}_{extName}.rdc";
                var absTarFile = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.RenderDoc) + tarFile;
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
        #endregion
        public Queue<FRCmdInfo> Cmds = new Queue<FRCmdInfo>();
        public class TtQueueStat
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
        public TtQueueStat QueueStats = new TtQueueStat();
        public void Flush()
        {
            lock (Cmds)
            {
                using (var tsCmd = new NxRHI.FTransientCmd(NxRHI.EQueueType.QU_Default, "TtRCmdQueue.Reset"))
                {
                    FlushExecute(tsCmd.CmdList);
                }
                TickSync(0);

                TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.Flush();
                //System.Diagnostics.Debug.Assert(Cmds.Count == 0);
            }
        }
        public void TickSync(float elapsedTime)
        {

        }
        public void TickLogic(float elapsedTime)
        {

        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeRenderTick;
        private static Profiler.TimeScope ScopeRenderTick
        {
            get
            {
                if (mScopeRenderTick == null)
                    mScopeRenderTick = new Profiler.TimeScope(typeof(TtRCmdQueue), nameof(TickRender));
                return mScopeRenderTick;
            }
        }
        public void TickRender(float elapsedTime)
        {
            var cmdQueue = TtEngine.Instance.GfxDevice.RenderContext.GpuQueue;

            using (new Profiler.TimeScopeHelper(ScopeRenderTick))
            {
                TickRenderImpl();
            }
        }
        private void TickRenderImpl()
        {
            while (true)
            {
                //todo:这里可以考虑，多个cmdlist合并一起，一次执行TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandLists
                if (Cmds.Count > 0)
                {
                    try
                    {
                        FRCmdInfo cmd;
                        lock (Cmds)
                        {
                            cmd = Cmds.Peek();
                            Cmds.Dequeue();
                        }
                        cmd.Cmd(this, ref cmd);
                        if (cmd.CmdType == ERCmdType.FrameEnd)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                }
            }
            //if (TtEngine.Instance.Config.MultiRenderMode == EMultiRenderMode.Queue)
            //{
                
            //}
        }
        public void QueueCmd(FRenderCmd cmd, string name, object tag = null, NxRHI.EQueueType qType = EQueueType.QU_Default, ERCmdType type = ERCmdType.Cmd)
        {
            var info = new FRCmdInfo();
            info.CmdType = type;
            info.Name = name;
            info.QueueType = qType;
            info.Cmd = cmd;
            info.Tag = tag;
            lock (Cmds)
            {
                ProcCmd(ref info);
            }
        }
        public void QueueCmdlist(TtCommandList cmd, string name = null, EQueueType qType = EQueueType.QU_Default)
        {
            System.Diagnostics.Debug.Assert(cmd.mCoreObject.IsRecording() == false);
            var info = new FRCmdInfo();
            info.CmdType = ERCmdType.Cmdlist;
            info.QueueType = qType;
            info.Name = name;
            info.Tag = cmd;
            info.Cmd = static (TtRCmdQueue queue, ref FRCmdInfo info) =>
            {
                TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList(info.Tag as TtCommandList, info.QueueType);
                //TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.mCoreObject.Flush(info.QueueType);
            };
            lock (Cmds)
            {
                ProcCmd(ref info);
                TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.mCoreObject.Flush(info.QueueType);
            }
            QueueStats.NumOfDrawcall += cmd.mCoreObject.GetDrawcallNumber();
            QueueStats.NumOfCmdlist++;
            QueueStats.NumOfPrimitive += cmd.mCoreObject.GetPrimitiveNumber();
        }
        private void ProcCmd(ref FRCmdInfo info)
        {
            if (TtEngine.Instance.Config.MultiRenderMode == EMultiRenderMode.Queue)
            {
                Cmds.Enqueue(info);
            }
            else
            {
                if (info.CmdType == ERCmdType.FrameEnd)
                {
                    Cmds.Enqueue(info);
                }
                else
                {
                    info.Cmd(this, ref info);
                }
            }
        }
        public void FlushExecute(ICommandList ImCmdlist)
        {
            bool bFindFrameEnd = false;
            NxRHI.FRCmdInfo endCmd = new();
            while (Cmds.Count > 0)
            {
                try
                {
                    NxRHI.FRCmdInfo cmd;
                    lock (Cmds)
                    {
                        cmd = Cmds.Peek();
                        if (cmd.CmdType == ERCmdType.FrameEnd)
                        {
                            bFindFrameEnd = true;
                            endCmd = cmd;
                        }
                        Cmds.Dequeue();
                    }

                    cmd.Cmd(this, ref cmd);
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
            }

            TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.Flush();
            if (bFindFrameEnd)
            {
                lock (Cmds)
                {
                    Cmds.Enqueue(endCmd);
                }
            }
        }
    }
}
