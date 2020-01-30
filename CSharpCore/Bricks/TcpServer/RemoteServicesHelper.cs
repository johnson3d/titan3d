using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RemoteServices
{
    public partial class RemoteServicesHelper
    {
#if PWindow
        TcpServer.CTcpServer mServer = null;
#else
        Net.TcpServer mServer = null;
#endif
        partial void Cleanup_Server()
        {
#if PWindow
            mServer?.Destroy();
#else
            //mServer;
#endif
            StopClient();
        }
        partial void Tick_Server()
        {
            if (mServer != null)
            {
                mServer.ProcessPackages();
            }
        }
        public async System.Threading.Tasks.Task InitServer(SuperSocket.SocketServerBaseDesc serverDesc)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            try
            {
#if PWindow
                mServer = new TcpServer.CTcpServer();
#else
                mServer = new Net.TcpServer();
#endif

                mServer.OnTcpConnected = (conn) =>
                {
                    conn.Router = new NetCore.RPCRouter();
                };
                mServer.OnTcpDisConnected = (conn) =>
                {
                    conn.Router = null;
                };
                var ret = mServer.Start(serverDesc.Ip, serverDesc.Port);
                if (ret == false)
                    return;
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Server", $"Profiler Server {serverDesc.Ip}:{serverDesc.Port} init failed");
                Profiler.Log.WriteException(ex);
            }
        }
    }
}
