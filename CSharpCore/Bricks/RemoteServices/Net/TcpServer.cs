using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RemoteServices.Net
{
    public class TcpServer
    {
        public delegate void FOnTcpConnected(TcpConnect conn);
        public delegate void FOnReceivePackage(TcpConnect conn, NetCore.RecvData pkg);
        public delegate void FOnTcpDisConnected(TcpConnect conn);
        public FOnTcpConnected OnTcpConnected;
        public FOnReceivePackage OnReceivePackage;//这个除了做一些统计，不要设置，让RPC接管就好了
        public FOnTcpDisConnected OnTcpDisConnected;

        protected SuperSocket.SocketServerBase mSocketServer;
        protected TcpConnectPool mPool;
        public TcpConnectPool Pool
        {
            get { return mPool; }
        }
        public Dictionary<UInt16, TcpConnect> TcpConnects
        {
            get;
        } = new Dictionary<UInt16, TcpConnect>();
        public TcpConnect GetConnect(UInt16 index)
        {
            return mPool.Pools[index];
        }
        public bool Start(string ip, UInt16 port)
        {
            var desc = new SuperSocket.SocketServerBaseDesc();
            desc.Ip = ip;
            desc.Port = port;
            mPool = new TcpConnectPool((UInt16)desc.MaxConnect);

            mSocketServer = new SuperSocket.SocketServerBase(desc);
            mSocketServer.NewClientAccepted += NewClientAccepted;
            mSocketServer.CloseClientConnect += CloseClientConnect;
            return mSocketServer.Start();
        }
        private void NewClientAccepted(System.Net.Sockets.Socket client, SuperSocket.ISocketSession session)
        {
            var ass = session as SuperSocket.AsyncSocketSession;
            var tcpConn = mPool.AllocConnect();

            lock(TcpConnects)
            {
                TcpConnects[tcpConn.IndexInPool] = tcpConn;
            }
            tcpConn.Session = ass;
            tcpConn.Host = this;
            ass.ExtObject = tcpConn;

            ass.SetReceiveHandler(arg =>
            {
                unsafe
                {
                    fixed (byte* ptr = &arg.Buffer[arg.Offset])
                    {
                        tcpConn.OnReceiveData((IntPtr)ptr, arg.BytesTransferred);
                    }
                }
            });

            if (OnTcpConnected != null)
            {
                OnTcpConnected(tcpConn);
            }
        }
        public void ProcessConnectPackages(TcpConnect conn)
        {
            NetCore.RecvData pkg;
            lock (conn.Packages)
            {
                if (conn.Packages.Count == 0)
                    return;
                pkg = conn.Packages.Dequeue();
                RPCExecuter.Instance.ReceiveData(conn, pkg.PkgData, pkg.Length, pkg.RecvTime);
            }
            if (OnReceivePackage != null)
                OnReceivePackage(conn, pkg);
        }
        public void ProcessPackages()
        {
            lock (TcpConnects)
            {
                foreach (var i in TcpConnects)
                {
                    ProcessConnectPackages(i.Value);
                }
            }
            RPCExecuter.Instance.TryDoTimeout();
        }
        private void CloseClientConnect(SuperSocket.ISocketSession session)
        {
            var ass = session as SuperSocket.AsyncSocketSession;
            var conn = ass.ExtObject as TcpConnect;
            lock (TcpConnects)
            {
                TcpConnects.Remove(conn.IndexInPool);
            }
            if(OnTcpDisConnected!=null)
            {
                OnTcpDisConnected(conn);
            }
            conn.Session = null;
            conn.Host = null;
            mPool.FreeConnect(conn);
        }
    }
}
