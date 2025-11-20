using EngineNS.Bricks.CodeBuilder.MacrossNode;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Profiler.Trace
{
    [Flags]
    public enum ETraceChannel : uint
    {
        None = 0,
        CPU = 1,
        GPU = (1<<1),
        RDG = (1<<2),
    }
    public class TtTracer
    {
        public static void PushAction(TtAction action)
        {
            if (TtEngine.Instance==null)
                return;
            TtEngine.Instance.Tracer.Current?.PushAction(action);
        }
        public bool Enabled { get; set; } = false;
        public ETraceChannel Channels { get; set; } = ETraceChannel.CPU;
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
                //using(var writer = new IO.AuxWriter<IO.TtFileWriter>(FileWriter))
                //{
                //    writer.Write("StartFrame");
                //    frame.Write(writer);
                //    writer.Write("EndFrame");
                //}
            }
        }
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
        public void Write(IO.IWriter writer)
        {
            writer.Write(Channels.Count);
            foreach (var channel in Channels)
            {
                writer.Write(channel.Channel);
                channel.Write(writer);
            }
        }

        public bool PushAction(TtAction action)
        {
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