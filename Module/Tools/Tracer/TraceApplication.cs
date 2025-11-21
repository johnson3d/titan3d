using EngineNS;
using EngineNS.Bricks.Network.RPC;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tracer.TtCpuProfilerVisual;

namespace Tracer
{
    public class TtMyTracer : EngineNS.Profiler.Trace.TtTracer
    {
        public override void ReciveFrame(EngineNS.IO.TtMemWriter frameData, EngineNS.Bricks.Network.RPC.TtCallContext context)
        {
            using (var reader = EngineNS.IO.TtMemReader.CreateInstance(in frameData))
            {
                using (var ar = new EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader>(reader, this))
                {
                    var frame = new EngineNS.Profiler.Trace.TtFrame();
                    frame.Read(ar);
                    this.PushFrame(frame);
                    var cpu = frame.GetChannel(EngineNS.Profiler.Trace.ETraceChannel.Cpu);
                    if (cpu!=null)
                    {
                        var app = TtEngine.Instance.GfxDevice.SlateApplication as TtTraceApplication;
                        foreach (var a in cpu.Actions)
                        {
                            if (a.GetType() == typeof(EngineNS.Profiler.Trace.TtCpuFrameProfilerAction))
                            {
                                //todo: select frame by ui
                                app.CpuProfilerVisual.FrameProfiler = a as EngineNS.Profiler.Trace.TtCpuFrameProfilerAction;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
    [TtRpcClass(RunTarget = ERunTarget.None, Executer = EExecuter.Root, CallerInClass = true)]
    public partial class TtTraceRpcManager : EngineNS.Bricks.Network.RPC.TtRpcManager
    {
        #region IRpcHost
        static TtRpcClass smRpcClass = null;
        public override TtRpcClass GetRpcClass()
        {
            if (smRpcClass==null)
                smRpcClass = new TtRpcClass(typeof(TtTraceRpcManager));
            return smRpcClass;
        }
        public override IRpcHost GetExecuter(in FRouter router)
        {
            if (router.Executer == EExecuter.Tracer)
            {
                return Tracer;
            }
            return base.GetExecuter(router);
        }
        #endregion

        public TtMyTracer Tracer { get; set; } = new TtMyTracer();
        public TtTraceRpcManager()
        {
            this.CurrentTarget = ERunTarget.Tracer;
        }
    }
    public partial class TtTraceApplication : TtSlateApplication, ITickable
    {
        #region ITickable
        public int GetTickOrder()
        {
            return 0;
        }
        #region Tick
        public void TickLogic(float ellapse)
        {
            //WorldViewportSlate.TickLogic(ellapse);
            TcpServer?.Tick();
        }
        public void TickRender(float ellapse)
        {
            //WorldViewportSlate.TickRender(ellapse);
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {
            //WorldViewportSlate.TickSync(ellapse);

            //OnDrawSlate();
        }
        #endregion
        #endregion

        public EngineNS.Bricks.TcpServer.TtTcpServer TcpServer { get; private set; } = new EngineNS.Bricks.TcpServer.TtTcpServer();
        public TtCpuProfilerVisual CpuProfilerVisual = null;
        public override async EngineNS.Thread.Async.TtTask<bool> InitializeApplication(EngineNS.NxRHI.TtGpuDevice rc, RName rpName)
        {
            await base.InitializeApplication(rc, rpName);
            TcpServer.StartServer("0.0.0.0", TtEngine.Instance.ConfigManager.GetConfig<EngineNS.Profiler.Trace.TtTraceConfig>().Port);

            CpuProfilerVisual = new TtCpuProfilerVisual();
            await CpuProfilerVisual.Initialize();
            CpuProfilerVisual.Visible = true;
            TtEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public override void Cleanup()
        {
            TcpServer.StopServer();
            TtEngine.Instance.TickableManager.RemoveTickable(this);
            EngineNS.CoreSDK.DisposeObject(ref CpuProfilerVisual);
            
            base.Cleanup();
        }
    }
}
