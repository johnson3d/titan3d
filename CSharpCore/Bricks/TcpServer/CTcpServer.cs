using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace EngineNS.Bricks.TcpServer
{
    public class CTcpServer
    {
        HPSocketCS.TcpServer mServer;
        public HPSocketCS.TcpServer Server
        {
            get { return mServer; }
        }
        ConcurrentDictionary<IntPtr, CTcpConnect> mDictConnects = new ConcurrentDictionary<IntPtr, CTcpConnect>();
        public CTcpConnect GetTcpConnect(IntPtr connId)
        {//这个函数以后有可能要想办法优化
            lock (this)
            {
                CTcpConnect conn;
                if (mDictConnects.TryGetValue(connId, out conn) == false)
                    return null;
                return conn;
            }
        }
        public ConcurrentDictionary<IntPtr, CTcpConnect> Connects
        {
            get { return Connects; }
        }
        public delegate void FClientConnected(CTcpConnect pClient);
        public FClientConnected OnTcpConnected;
        public delegate void FOnReceiveData(CTcpConnect pClient, byte[] pData, int nLength, Int64 recvTime);
        public FOnReceiveData OnReceiveData;
        public delegate void FOnDisconnected(CTcpConnect pClient);
        public FOnDisconnected OnTcpDisConnected;
        public CTcpServer()
        {
            
        }
        ~CTcpServer()
        {
            Destroy();
        }
        internal void OnRcvPackage(CTcpConnect pClient, byte[] pData, int nLength, Int64 recvTime)
        {
            if (OnReceiveData != null)
            {
                OnReceiveData(pClient, pData, nLength, recvTime);
            }
            else
            {
                RemoteServices.RPCExecuter.Instance.ReceiveData(pClient, pData, nLength, recvTime);
            }
        }
        public void ProcessPackages()
        {
            try
            {
                using (var iter = mDictConnects.GetEnumerator())
                {
                    while (iter.MoveNext())
                    {
                        iter.Current.Value.Update();
                    }
                }
                RemoteServices.RPCExecuter.Instance.TryDoTimeout();
            }
            catch(Exception)
            {
            }
        }
        private HPSocketCS.HandleResult OnPrepareListen(HPSocketCS.IServer sender, IntPtr soListen)
        {
            // 监听事件到达了,一般没什么用吧?

            return HPSocketCS.HandleResult.Ok;
        }
        private HPSocketCS.HandleResult OnAccept(HPSocketCS.IServer sender, IntPtr connId, IntPtr pClient)
        {
            // 客户进入了
            // 获取客户端ip和端口
            string ip = string.Empty;
            ushort port = 0;
            if (mServer.GetRemoteAddress(connId, ref ip, ref port) == false)
            {
                return HPSocketCS.HandleResult.Error;
            }

            CTcpConnect conn;
            lock (this)
            {
                if (mDictConnects.TryGetValue(connId, out conn) == false)
                {
                    conn = new CTcpConnect(this, ip, port, connId);
                    mDictConnects[connId] = conn;
                }
            }

            if(OnTcpConnected!=null)
                OnTcpConnected(conn);

            return HPSocketCS.HandleResult.Ok;
        }
        private HPSocketCS.HandleResult OnClose(HPSocketCS.IServer sender, IntPtr connId, HPSocketCS.SocketOperation enOperation, int errorCode)
        {
            CTcpConnect conn = null;
            lock (this)
            {
                mDictConnects.TryRemove(connId, out conn);
                if (conn != null && OnTcpDisConnected != null)
                {
                    OnTcpDisConnected(conn);
                }
            }
            conn.OnDisconnect();
            return HPSocketCS.HandleResult.Ok;
        }
        private HPSocketCS.HandleResult OnSend(HPSocketCS.IServer sender, IntPtr connId, byte[] bytes)
        {
            // 服务器发数据了

            return HPSocketCS.HandleResult.Ok;
        }
        private HPSocketCS.HandleResult OnPointerDataReceive(HPSocketCS.IServer sender, IntPtr connId, IntPtr pData, int length)
        {
            CTcpConnect conn = GetTcpConnect(connId);
            if (conn != null)
            {
                conn.OnDataReceive(pData, length);
            }
            return HPSocketCS.HandleResult.Ok;
        }
        private HPSocketCS.HandleResult OnShutdown(HPSocketCS.IServer sender)
        {
            return HPSocketCS.HandleResult.Ok;
        }
        public bool Start(string ip, ushort port)
        {
            if (ip == "any")
                ip = "0.0.0.0";
            mServer = new HPSocketCS.TcpServer();
            mServer.OnPrepareListen += new HPSocketCS.ServerEvent.OnPrepareListenEventHandler(OnPrepareListen);
            mServer.OnAccept += new HPSocketCS.ServerEvent.OnAcceptEventHandler(OnAccept);
            //mServer.OnSend += new HPSocketCS.ServerEvent.OnSendEventHandler(OnSend);
            mServer.OnPointerDataReceive += new HPSocketCS.ServerEvent.OnPointerDataReceiveEventHandler(OnPointerDataReceive);
            mServer.OnClose += new HPSocketCS.ServerEvent.OnCloseEventHandler(OnClose);
            mServer.OnShutdown += new HPSocketCS.ServerEvent.OnShutdownEventHandler(OnShutdown);

            mServer.IpAddress = ip;
            mServer.Port = port;
            // 启动服务
            return mServer.Start();
        }
        public void Destroy()
        {
            if(mServer==null)
            {
                return;
            }
            foreach (var i in mDictConnects)
            {
                i.Value.OnDisconnect();
                if (OnTcpDisConnected != null)
                    OnTcpDisConnected(i.Value);
            }
            mDictConnects.Clear();
            mServer.OnPrepareListen -= new HPSocketCS.ServerEvent.OnPrepareListenEventHandler(OnPrepareListen);
            mServer.OnAccept -= new HPSocketCS.ServerEvent.OnAcceptEventHandler(OnAccept);
            //mServer.OnSend -= new HPSocketCS.ServerEvent.OnSendEventHandler(OnSend);
            mServer.OnPointerDataReceive -= new HPSocketCS.ServerEvent.OnPointerDataReceiveEventHandler(OnPointerDataReceive);
            mServer.OnClose -= new HPSocketCS.ServerEvent.OnCloseEventHandler(OnClose);
            mServer.OnShutdown -= new HPSocketCS.ServerEvent.OnShutdownEventHandler(OnShutdown);
            
            mServer.Destroy();
            mServer = null;
        }
    }
}
