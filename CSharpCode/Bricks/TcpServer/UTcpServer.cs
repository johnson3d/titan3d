using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.TcpServer
{
    public class UTcpServer : AuxPtrType<EngineNS.TcpServer>
    {
        #region Natvie Callback
        static EngineNS.TcpServer.FDelegate_FOnTcpConnectAccept OnTcpConnectAccept = OnTcpConnectAcceptImpl;
        static EngineNS.TcpServer.FDelegate_FOnTcpConnectClosed OnTcpConnectClosed = OnTcpConnectClosedImpl;
        static EngineNS.TcpServer.FDelegate_FOnTcpServerListen OnTcpServerListen = OnTcpServerListenImpl;
        static EngineNS.TcpServer.FDelegate_FOnTcpServerShutdown OnTcpServerShutdown = OnTcpServerShutdownImpl;

        static unsafe void OnTcpConnectAcceptImpl(EngineNS.TcpServer arg0, EngineNS.TcpConnect arg1)
        {
            if (arg0.GCHandle == (void*)0)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)arg0.GCHandle);
            var server = gcHandle.Target as UTcpServer;

            var connect = server.CreateTcpConnect(arg1);
            server.OnConnectAccept(connect);
        }
        static unsafe void OnTcpConnectClosedImpl(EngineNS.TcpServer arg0, EngineNS.TcpConnect arg1, int arg2)
        {
            if (arg0.GCHandle == (void*)0 || arg1.GCHandle == (void*)0)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)arg0.GCHandle);
            var server = gcHandle.Target as UTcpServer;
            var gcHandle1 = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)arg1.GCHandle);
            var connect = gcHandle1.Target as UTcpConnect;
            server.OnConnectClosed(connect);
            connect.Dispose();
        }
        static unsafe void OnTcpServerListenImpl(EngineNS.TcpServer arg0)
        {
            if (arg0.GCHandle == (void*)0)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)arg0.GCHandle);

            var server = gcHandle.Target as UTcpServer;
            server.OnListen();
        }
        static unsafe void OnTcpServerShutdownImpl(EngineNS.TcpServer arg0)
        {
            if (arg0.GCHandle == (void*)0)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)arg0.GCHandle);

            var server = gcHandle.Target as UTcpServer;
            server.OnShutdown();
            server.Dispose();
        }
        static UTcpServer()
        {
            EngineNS.TcpConnect.SetOnTcpConnectRcvData(UTcpConnect.OnTcpConnectRcvData);
            EngineNS.TcpServer.SetOnTcpConnectAccept(OnTcpConnectAccept);
            EngineNS.TcpServer.SetOnTcpConnectClosed(OnTcpConnectClosed);
            EngineNS.TcpServer.SetOnTcpServerListen(OnTcpServerListen);
            EngineNS.TcpServer.SetOnTcpServerShutdown(OnTcpServerShutdown);
        }
        #endregion
        public UTcpServer()
        {
            unsafe
            {
                mCoreObject = EngineNS.TcpServer.CreateInstance();
                mCoreObject.GCHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(this)).ToPointer();
            }
        }
        public unsafe override void Dispose()
        {
            if (mCoreObject.GCHandle != (void*)0)
            {
                System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)mCoreObject.GCHandle).Free();
                mCoreObject.GCHandle = (void*)0;
            }
            base.Dispose();
        }
        public bool IsListened { get; protected set; }
        public Dictionary<ulong, UTcpConnect> mTcpConnects = new Dictionary<ulong, UTcpConnect>();
        public bool StartServer(string ip, UInt16 port)
        {
            return mCoreObject.StartServer(ip, port);
        }
        public void StopServer()
        {
            mCoreObject.StopServer();
        }
        protected virtual void OnListen()
        {
            IsListened = true;
        }
        protected virtual void OnShutdown()
        {
            IsListened = false;
            lock (mTcpConnects)
            {
                mTcpConnects.Clear();
            }
        }
        protected virtual UTcpConnect CreateTcpConnect(EngineNS.TcpConnect conn)
        {
            var result = new UTcpConnect(conn);            
            return result;
        }
        protected virtual void OnConnectAccept(UTcpConnect connect)
        {
            lock (mTcpConnects)
            {
                mTcpConnects[connect.mCoreObject.mConnId] = connect;
            }
        }
        protected virtual void OnConnectClosed(UTcpConnect connect)
        {
            lock (mTcpConnects)
            {
                mTcpConnects.Remove(connect.mCoreObject.mConnId);
            }
        }
        public void Tick()
        {
            lock (mTcpConnects)
            {
                foreach (var i in mTcpConnects)
                {
                    i.Value.Tick();
                }
            }
        }
    }
}
