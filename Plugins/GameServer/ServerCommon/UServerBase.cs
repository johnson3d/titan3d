using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Plugins.ServerCommon
{
    public class UServerBase : Bricks.Network.RPC.TtRpcManager, ITickable
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public Bricks.Network.INetConnect Connect { get; set; } = null;
        public Guid ServerId { get; set; } = Guid.NewGuid();
        public Bricks.Network.FNetworkPoint ListenPoint { get; } = new Bricks.Network.FNetworkPoint();
        public Bricks.TcpServer.UTcpServer TcpServer { get; protected set; } = null;
        public long Payload { get; set; } = 0;
        public ServerCommon.UClientManager ClientManager { get; set; } = new ServerCommon.UClientManager();

        public virtual async System.Threading.Tasks.Task<bool> StartServer(string ip, UInt16 port)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            TtEngine.Instance.TickableManager.AddTickable(this);
            TcpServer = new Bricks.TcpServer.UTcpServer();
            return TcpServer.StartServer(ip, port);
        }
        public virtual void StopServer()
        {
            TcpServer.StopServer();
            TtEngine.Instance.TickableManager.RemoveTickable(this);
        }
        public virtual void Tick()
        {
            TcpServer?.Tick();
        }
        public virtual Bricks.Network.INetConnect GetRunTargetConnect(Bricks.Network.RPC.ERunTarget target, UInt16 index, Bricks.Network.INetConnect connect)
        {
            return null;
        }
        public override Bricks.Network.INetConnect GetRunTargetConnect(in Bricks.Network.RPC.URouter target, Bricks.Network.INetConnect connect)
        {
            return GetRunTargetConnect(target.RunTarget, target.Index, connect);
        }
        public override Bricks.Network.INetConnect GetRunTargetConnect(in Bricks.Network.RPC.FReturnContext target, Bricks.Network.INetConnect connect)
        {
            return GetRunTargetConnect(target.RunTarget, target.Index, connect);
        }
        #region tickable
        public void TickLogic(float ellapse)
        {
            Tick();
        }
        public void TickRender(float ellapse)
        {

        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {

        }
        #endregion
        public virtual Bricks.Network.FNetworkPoint SelectNetworkPoint()
        {
            var result = new Bricks.Network.FNetworkPoint();
            result.Ip = "127.0.0.1";
            //result.Port
            var ipProps = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
            var points = ipProps.GetActiveTcpListeners();
            for (int i = 1000; i < 30000; i++)
            {
                bool inUsed = false;
                foreach(var j in points)
                {
                    if (j.Port == i)
                    {
                        inUsed = true;
                        break;
                    }
                }
                if (inUsed == false)
                {
                    result.Port = (UInt16)i;
                    break;
                }
            }
            return result;
        }
    }
}
