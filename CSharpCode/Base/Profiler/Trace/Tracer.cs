using EngineNS.Bricks.CodeBuilder.MacrossNode;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Profiler.Trace
{
    [IO.TtConfig(Path = "tracer.jscfg")]
    public class TtTraceConfig : IO.IConfig
    {
        [Rtti.Meta("")]
        public string Ip { get; set; } = "127.0.0.1";
        [Rtti.Meta("")]
        public ushort Port { get; set; } = 23333;
    }
    [Flags]
    public enum ETraceChannel : uint
    {
        None = 0,
        Cpu = 1,
        Gpu = (1<<1),
        Rdg = (1<<2),
        Log = (1<<3),
    }
    [Bricks.Network.RPC.TtRpcClass(RunTarget = Bricks.Network.RPC.ERunTarget.Tracer, Executer = Bricks.Network.RPC.EExecuter.Tracer, CallerInClass = true)]
    public partial class TtTracer : Bricks.Network.RPC.AuxRpcHost<TtTracer>, IDisposable
    {
        public override Bricks.Network.INetConnect GetRpcConnect(UInt16 methodIndex)
        {
            return NetConnect;
        }
        ~TtTracer()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (NetConnect!=null)
            {
                NetConnect.Disconnect();
                NetConnect = null;
            }
            if (FileWriter!=null)
            {
                //FileWriter.Close();
                FileWriter = null;
            }
        }

        public async EngineNS.Thread.Async.TtTask<bool> ConnectTo(string ip=null, ushort port = ushort.MaxValue)
        {
            if (NetConnect!=null)
                NetConnect.Disconnect();
            else
                NetConnect = new Bricks.Network.TtTcpClient();
            if (ip==null)
            {
                ip = TtEngine.Instance.ConfigManager.GetConfig<TtTraceConfig>().Ip;
            }
            if (port== ushort.MaxValue)
            {
                port = TtEngine.Instance.ConfigManager.GetConfig<TtTraceConfig>().Port;
            }
            var ret = await NetConnect.Connect(ip, port, null);
            if (ret)
                Enabled = true;
            return ret;
        }

        public static void PushAction(TtAction action)
        {
            if (TtEngine.Instance==null)
                return;
            if (TtEngine.Instance.Tracer.Current==null)
            {
                return;
            }
            TtEngine.Instance.Tracer.Current.PushAction(TtEngine.Instance.Tracer, action);
        }
        public bool Enabled { get; set; } = false;
        public ETraceChannel Channels { get; set; } = ETraceChannel.Cpu;
        public List<TtFrame> Frames = new List<TtFrame>();
        public TtFrame Current;
        public TtFrame BeginFrame()
        {
            if (Enabled == false)
            {
                return null;
            }

            lock (this)
            {
                Current = new TtFrame();
                Current.InitChannels(Channels);
                Frames.Add(Current);
                return Current;
            }
        }
        public void EndFrame(TtFrame frame)
        {
            if (frame==null)
            {
                return;
            }
            TtTracer.PushAction(new TtCpuFrameProfilerAction());
            //send to server or write to file
            if (NetConnect!=null)
            {
                NetSendFrames();
            }
            else
            {
                WriteFrames();
            }
            Current = null;
        }
        public void PushFrame(TtFrame frame)
        {
            //if (Enabled == false)
            //{
            //    return;
            //}
            lock (this)
            {
                Frames.Add(frame);
            }
        }
        public Bricks.Network.TtTcpClient NetConnect = null;
        public IO.TtFileWriter FileWriter = null;
        public void WriteFrames()
        {
            while (Frames.Count>0)
            {
                TtFrame frame = null;
                lock (this)
                {
                    frame = Frames[0];
                    Frames.RemoveAt(0);
                }
                using (var writer = new IO.AuxWriter<IO.TtFileWriter>(FileWriter))
                {
                    writer.Write("StartFrame");
                    frame.Write(writer);
                    writer.Write("EndFrame");
                }
            }
        }
        public void NetSendFrames()
        {
            while (Frames.Count>0)
            {
                TtFrame frame = null;
                lock (this)
                {
                    frame = Frames[0];
                    Frames.RemoveAt(0);
                }
                using (var memWriter = IO.TtMemWriter.CreateInstance())
                {
                    using (var writer = new IO.AuxWriter<IO.TtMemWriter>(memWriter))
                    {
                        frame.Write(writer);
                        RPC_ReciveFrame(memWriter, null);
                    }
                }
            }
        }
        #region RPC
        [Bricks.Network.RPC.TtRpcMethod(Index = 0)]
        public virtual void ReciveFrame(IO.TtMemWriter frameData, Bricks.Network.RPC.TtCallContext context)
        {
            //using (var reader = IO.TtMemReader.CreateInstance(in frameData))
            //{
            //    using (var ar = new IO.AuxReader<IO.TtMemReader>(reader, this))
            //    {
            //        var frame = new TtFrame();
            //        //frame.Read
            //        //TtEngine.Instance.Tracer.PushFrame(frame);
            //        OnReciveFrame(frame);
            //        //TtEngine.Instance.GfxDevice.SlateApplication as 
            //    }
            //}
            System.Diagnostics.Debug.Assert(false);
        }
        #endregion
    }
    public class TtFrame
    {
        public List<TtChannel> Channels = new List<TtChannel>();
        public void InitChannels(ETraceChannel channel)
        {
            for (int i = 0; i<32; i++)
            {
                var flag = (ETraceChannel)(1 << i);
                if((channel & flag) != 0)
                {
                    var ttChannel = new TtChannel();
                    ttChannel.Channel = flag;
                    Channels.Add(ttChannel);
                }
            }
        }
        public TtChannel GetChannel(ETraceChannel channel)
        {
            foreach (var ch in Channels)
            {
                if (ch.Channel == channel)
                    return ch;
            }
            return null;
        }
        public void Write(IO.IWriter writer)
        {
            writer.Write(Channels.Count);
            foreach (var channel in Channels)
            {
                writer.Write(channel.Channel);
                channel.Write(writer);
            }
        }
        public void Read(IO.IReader reader)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i<count; i++)
            {
                ETraceChannel cn;
                reader.Read(out cn);
                var ttChannel = new TtChannel();
                ttChannel.Channel = cn;

                ttChannel.Read(reader);
                Channels.Add(ttChannel);
            }
        }

        public bool PushAction(TtTracer tracer, TtAction action)
        {
            if ((tracer.Channels & action.GetChannel()) == 0)
            {
                return false;
            }
            foreach (var ch in Channels)
            {
                if (ch.Channel == action.GetChannel())
                {
                    ch.Actions.Add(action);
                    return true;
                }
            }
            return false;
        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public Profiler.Trace.TtTracer Tracer { get; } = new Profiler.Trace.TtTracer();
    }
}

#if TitanEngine_AutoGen_RPC
#region TitanEngine_AutoGen_RPC
#pragma warning disable 105


namespace EngineNS.Profiler.Trace
{
	public partial class TtTracer_RpcCaller
	{
		public static void ReciveFrame(IO.TtMemWriter frameData, in EngineNS.Bricks.Network.RPC.FRpcCallArg rpcArg)
		{
			var ExeIndex = rpcArg.ExeIndex;
			var NetConnect = rpcArg.NetConnect;
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = TtEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = TtEngine.Instance.RpcModule.DefaultNetConnect;
			}
			using (var writer = EngineNS.IO.TtMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<EngineNS.IO.TtMemWriter>(writer);
				var router = new EngineNS.Bricks.Network.RPC.FRouter();
				router.RunTarget = Bricks.Network.RPC.ERunTarget.Tracer;
				router.Executer = Bricks.Network.RPC.EExecuter.Tracer;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new EngineNS.Bricks.Network.RPC.FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router, false);
				UInt16 methodIndex = 0;
				pkg.Write(methodIndex);
				pkg.Write(frameData);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
		}
	}
}


namespace EngineNS.Profiler.Trace
{
	partial class TtTracer
	{
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_ReciveFrame = (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host, EngineNS.Bricks.Network.RPC.TtCallContext context) =>
		{
			IO.TtMemWriter frameData;
			reader.Read(out frameData);
			((EngineNS.Profiler.Trace.TtTracer)host).ReciveFrame(frameData, context);
			frameData.Dispose();
		};
		public void RPC_ReciveFrame(IO.TtMemWriter frameData, EngineNS.Bricks.Network.RPC.TtReturnContext retContext = null)
		{
			var rpcArg = new EngineNS.Bricks.Network.RPC.FRpcCallArg(retContext);
			rpcArg.ExeIndex = RpcExecuteIndex;
			rpcArg.NetConnect = GetRpcConnect(0);
			TtTracer_RpcCaller.ReciveFrame(frameData, rpcArg);
		}
	}
}
#endregion//TitanEngine_AutoGen_RPC
#endif//TitanEngine_AutoGen_RPC