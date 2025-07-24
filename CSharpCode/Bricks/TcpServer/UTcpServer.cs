using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.TcpServer
{
    public class TtTcpServer : AuxPtrType<EngineNS.TcpServer>
    {
        #region Natvie Callback
        static EngineNS.TcpServer.FDelegate_FOnTcpConnectAccept OnTcpConnectAccept = OnTcpConnectAcceptImpl;
        static EngineNS.TcpServer.FDelegate_FOnTcpConnectClosed OnTcpConnectClosed = OnTcpConnectClosedImpl;
        static EngineNS.TcpServer.FDelegate_FOnTcpServerListen OnTcpServerListen = OnTcpServerListenImpl;
        static EngineNS.TcpServer.FDelegate_FOnTcpServerListen OnTcpServerShutdown = OnTcpServerShutdownImpl;

        static unsafe void OnTcpConnectAcceptImpl(EngineNS.TcpServer arg0, EngineNS.TcpConnect arg1)
        {
            if (arg0.GCHandle == (void*)0)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)arg0.GCHandle);
            var server = gcHandle.Target as TtTcpServer;

            var connect = server.CreateTcpConnect(arg1);
            server.OnConnectAccept(connect);
        }
        static unsafe void OnTcpConnectClosedImpl(EngineNS.TcpServer arg0, EngineNS.TcpConnect arg1, int arg2)
        {
            if (arg0.GCHandle == (void*)0 || arg1.GCHandle == (void*)0)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)arg0.GCHandle);
            var server = gcHandle.Target as TtTcpServer;
            var gcHandle1 = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)arg1.GCHandle);
            var connect = gcHandle1.Target as TtTcpConnect;
            server.OnConnectClosed(connect);
            connect.Dispose();
        }
        static unsafe void OnTcpServerListenImpl(EngineNS.TcpServer arg0)
        {
            if (arg0.GCHandle == (void*)0)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)arg0.GCHandle);

            var server = gcHandle.Target as TtTcpServer;
            server.OnListen();
        }
        static unsafe void OnTcpServerShutdownImpl(EngineNS.TcpServer arg0)
        {
            if (arg0.GCHandle == (void*)0)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)arg0.GCHandle);

            var server = gcHandle.Target as TtTcpServer;
            server.OnShutdown();
            server.Dispose();
        }
        static TtTcpServer()
        {
            EngineNS.TcpConnect.SetOnTcpConnectRcvData(TtTcpConnect.OnTcpConnectRcvData);
            EngineNS.TcpServer.SetOnTcpConnectAccept(OnTcpConnectAccept);
            EngineNS.TcpServer.SetOnTcpConnectClosed(OnTcpConnectClosed);
            EngineNS.TcpServer.SetOnTcpServerListen(OnTcpServerListen);
            EngineNS.TcpServer.SetOnTcpServerShutdown(OnTcpServerShutdown);
        }
        #endregion
        public TtTcpServer()
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
        public Dictionary<ulong, TtTcpConnect> mTcpConnects = new Dictionary<ulong, TtTcpConnect>();
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
        public delegate void FOnConnectAction(string action, TtTcpConnect conn);
        public FOnConnectAction OnConnectAction = null;
        protected virtual TtTcpConnect CreateTcpConnect(EngineNS.TcpConnect conn)
        {
            var result = new TtTcpConnect(conn);
            if (OnConnectAction != null)
                OnConnectAction("OnCreate", result);
            return result;
        }
        protected virtual void OnConnectAccept(TtTcpConnect connect)
        {
            lock (mTcpConnects)
            {
                mTcpConnects[connect.mCoreObject.mConnId] = connect;
                if (OnConnectAction != null)
                    OnConnectAction("OnAccept", connect);
            }
        }
        protected virtual void OnConnectClosed(TtTcpConnect connect)
        {
            lock (mTcpConnects)
            {
                mTcpConnects.Remove(connect.mCoreObject.mConnId);
                if (OnConnectAction != null)
                    OnConnectAction("OnClosed", connect);
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
