using EngineNS.Macross;
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

    public class TtRCmdQueue : IDisposable
    {
        public void Dispose()
        {
            QueueStats.Reset();
            Cmds.Clear();
        }
        #region RenderDoc Capture
        public bool CaptureRenderDocFrame = false;
        public int RemainingCaptureFrames = 0;
        /// <summary>
        /// 抓完是否顺手用 RenderDoc UI 打开。交互式抓帧 (编辑器 Cap 菜单) 要 true;
        /// 自动化抓帧 (MCP 工具) 置 false, 否则每抓一帧弹一个窗口。
        /// </summary>
        public bool OpenRenderDocAfterCapture = true;
        /// <summary>
        /// EndFrameCapture 传给文件名的后缀。抓帧是跨帧异步的, 发起方设好这里,
        /// 主循环收帧时才用得上。
        /// </summary>
        public string CaptureTagName = null;
        /// <summary>
        /// 最近一次抓帧落盘的文件名 (相对 Cache/RenderDoc 目录)。
        /// 主循环里 EndFrameCapture 的返回值本来是被丢弃的, 存在这里好让发起方拿到结果。
        /// </summary>
        public string LastCaptureFile = null;
        uint mCaptureStartCount;
        bool mCaptureStarted;

        void CancelRenderDocCapture(string reason)
        {
            CaptureRenderDocFrame = false;
            RemainingCaptureFrames = 0;
            mCaptureStarted = false;
            LastCaptureFile = null;
            Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Error, $"RenderDoc 抓帧失败：{reason}");
        }

        public bool BeginFrameCapture()
        {
            if (CaptureRenderDocFrame == false)
            {
                return false;
            }
#if PWindow
            if (IRenderDocTool.GetInstance().IsFrameCapturing())
            {
                return false;
            }
            LastCaptureFile = null;
            mCaptureStartCount = IRenderDocTool.GetInstance().GetNumCaptures();
            IRenderDocTool.GetInstance().StartFrameCapture();
            if (!IRenderDocTool.GetInstance().IsFrameCapturing())
            {
                CancelRenderDocCapture("未能开始抓帧，请检查 RenderDoc API 与 Vulkan capture layer 是否使用同一份 DLL，以及 layer 是否被禁用。");
                return false;
            }
            mCaptureStarted = true;
            return true;
#else
            CancelRenderDocCapture("当前平台不支持此 RenderDoc 抓帧入口。");
            return false;
#endif
        }
        int CaptureId = 0;
        public string EndFrameCapture(string tagName = null, bool openRenderDoc = true)
        {
            if (CaptureRenderDocFrame == false || !mCaptureStarted)
            {
                return null;
            }
            //System.Diagnostics.Debug.Assert(IRenderDocTool.GetInstance().IsFrameCapturing());
            CaptureRenderDocFrame = false;
            mCaptureStarted = false;
#if PWindow
            if (IRenderDocTool.GetInstance().EndFrameCapture() == 0)
            {
                CancelRenderDocCapture("EndFrameCapture 返回失败，未生成新的 rdc。");
                return null;
            }
            var captureCount = IRenderDocTool.GetInstance().GetNumCaptures();
            if (captureCount == 0 || captureCount <= mCaptureStartCount)
            {
                CancelRenderDocCapture("RenderDoc 未报告新增抓帧，拒绝读取旧文件。");
                return null;
            }

            ulong timeStamp = 0;
            var idx = captureCount - 1;
            var file = IRenderDocTool.GetInstance().GetCapture(idx, ref timeStamp);
            if (!string.IsNullOrEmpty(file) && IO.TtFileManager.FileExists(file))
            {
                var extName = (tagName != null) ? "_" + tagName : "";
                var tarFile = IO.TtFileManager.GetPureName(file) + $"_{CaptureId++}_{extName}.rdc";
                var absTarFile = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.RenderDoc) + tarFile;
                try
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(absTarFile));
                    System.IO.File.Move(file, absTarFile, true);
                }
                catch (System.Exception e)
                {
                    Profiler.Log.WriteException(e);
                    CancelRenderDocCapture($"rdc 已生成在 {file}，但移动到缓存目录失败。");
                    return null;
                }
                LastCaptureFile = tarFile;
                if (RemainingCaptureFrames > 0)
                {
                    RemainingCaptureFrames--;
                    CaptureRenderDocFrame = true;
                }
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Info, $"RenderDoc 抓帧已保存：{absTarFile}");
                if (openRenderDoc)
                {
                    IRenderDocTool.GetInstance().OpenFile(absTarFile);
                }
                return tarFile;
            }
            CancelRenderDocCapture("RenderDoc 报告的抓帧文件不存在。");
#endif
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
        public void Flush(bool bFlushGPU)
        {
            lock (Cmds)
            {
                FlushExecute(bFlushGPU);
                TickSync(0);
            }
        }
        public void TickSync(float elapsedTime)
        {
            QueueStats.Reset();
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
            var spinWait = new System.Threading.SpinWait();
            while (true)
            {
                int nFrameEnd = 0;
                while (Cmds.Count > 0)
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
                            nFrameEnd++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                }
                if (nFrameEnd > 0 || TtMacrossDebugger.Instance.CurrrentBreak != null)
                {
                    break;
                }
                spinWait.SpinOnce();
            }
        }
        public void QueueCmd(FRenderCmd cmd, string name, object tag = null, NxRHI.EQueueType qType = EQueueType.QU_Default, ERCmdType type = ERCmdType.Cmd, bool bImm = false)
        {
            var info = new FRCmdInfo();
            info.CmdType = type;
            info.Name = name;
            info.QueueType = qType;
            info.Cmd = cmd;
            info.Tag = tag;
            lock (Cmds)
            {
                ProcCmd(ref info, bImm);
            }
        }
        public void QueueCmdlist(TtCommandList cmd, string name = null, EQueueType qType = EQueueType.QU_Default, bool bImm = false)
        {
            System.Diagnostics.Debug.Assert(cmd.mCoreObject.IsRecording() == false);

            //GetDrawcallNumber内部调用Recorder的时候，如果ProcCmd先执行完了，就会导致Recorder被销毁了，所以这里先把数据统计了
            QueueStats.NumOfDrawcall += cmd.mCoreObject.GetDrawcallNumber();
            QueueStats.NumOfCmdlist++;
            QueueStats.NumOfPrimitive += cmd.mCoreObject.GetPrimitiveNumber();

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
                ProcCmd(ref info, bImm);
            }
        }
        public TtFence QueueFence(TtFence fence = null, string name = null, EQueueType qType = EQueueType.QU_Default, bool bImm = false)
        {
            if (fence == null)
            {
                var fcDesc = new FFenceDesc();
                fcDesc.InitValue = 0;
                fence = TtEngine.Instance.GfxDevice.RenderContext.CreateFence(in fcDesc, "QueueFence." + name);
            }
            var info = new FRCmdInfo();
            info.CmdType = ERCmdType.Cmdlist;
            info.QueueType = qType;
            info.Name = name;
            info.Tag = fence;
            info.Cmd = static (TtRCmdQueue queue, ref FRCmdInfo info) =>
            {
                TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.IncreaseSignal(info.Tag as TtFence, info.QueueType);
            };
            lock (Cmds)
            {
                ProcCmd(ref info, bImm);
            }
            return fence;
        }
        private void ProcCmd(ref FRCmdInfo info, bool bImm)
        {
            if (bImm == false && TtEngine.Instance.Config.MultiRenderMode == EMultiRenderMode.Queue)
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
        public void FlushExecute(bool bFlushGPU)
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

            if (bFlushGPU)
            {
                TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.Flush();
            }
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
