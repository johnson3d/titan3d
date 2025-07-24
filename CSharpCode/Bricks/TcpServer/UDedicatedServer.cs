using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.TcpServer
{
    public class TtDedicatedServer : TtModule<TtEngine>
    {
        public override async System.Threading.Tasks.Task<bool> Initialize(TtEngine engine)
        {
            await base.Initialize(engine);

            return true;
        }
        public override void Cleanup(TtEngine engine)
        {

        }
        public override void TickModule(TtEngine engine)
        {
            mServer?.Tick();
        }

        TtTcpServer mServer;
        public bool StartServer(string ip, UInt16 port)
        {
            if (mServer != null)
                return false;
            mServer = new TtTcpServer();
            return mServer.StartServer(ip, port);
        }
        public void StopServer()
        {
            if (mServer == null)
                return;
            mServer.StopServer();
            mServer = null;
        }
    }
}

namespace EngineNS
{
    public partial class TtEngine
    {
        public static System.Type UDedicatedServerType = typeof(Bricks.TcpServer.TtDedicatedServer);
        private Bricks.TcpServer.TtDedicatedServer mDedicatedServer;
        public Bricks.TcpServer.TtDedicatedServer DedicatedServer
        {
            get
            {
                if (mDedicatedServer == null)
                {
                    mDedicatedServer = Rtti.TtTypeDescManager.CreateInstance(UDedicatedServerType) as Bricks.TcpServer.TtDedicatedServer;
                }
                return mDedicatedServer;
            }
        }
    }
}
