using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.TcpServer
{
    public class UDedicatedServer : UModule<UEngine>
    {
        public override async System.Threading.Tasks.Task<bool> Initialize(UEngine engine)
        {
            await base.Initialize(engine);

            return true;
        }
        public override void Cleanup(UEngine engine)
        {

        }
        public override void Tick(UEngine engine)
        {
            mServer?.Tick();
        }

        UTcpServer mServer;
        public bool StartServer(string ip, UInt16 port)
        {
            if (mServer != null)
                return false;
            mServer = new UTcpServer();
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
    public partial class UEngine
    {
        public static System.Type UDedicatedServerType = typeof(Bricks.TcpServer.UDedicatedServer);
        private Bricks.TcpServer.UDedicatedServer mDedicatedServer;
        public Bricks.TcpServer.UDedicatedServer DedicatedServer
        {
            get
            {
                if (mDedicatedServer == null)
                {
                    mDedicatedServer = Rtti.UTypeDescManager.CreateInstance(UDedicatedServerType) as Bricks.TcpServer.UDedicatedServer;
                }
                return mDedicatedServer;
            }
        }
    }
}
