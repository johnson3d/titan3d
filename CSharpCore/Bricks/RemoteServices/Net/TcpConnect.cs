using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RemoteServices.Net
{
    public class TcpConnect : NetCore.NetConnection
    {
        public TcpServer Host
        {
            get;
            set;
        }
        public UInt16 IndexInPool
        {
            get;
            internal set;
        }
        internal bool IsFree;
        public SuperSocket.AsyncSocketSession Session
        {
            get;
            internal set;
        }
        public override bool Connected
        {
            get
            {
                return Session!=null;
            }
        }
        protected NetCore.PacketBuilder PkgBuilder = new NetCore.PacketBuilder();
        protected NetCore.PkgPoolManager PkgPoolMgr = new NetCore.PkgPoolManager();
        public Queue<NetCore.RecvData> Packages
        {
            get;
        } = new Queue<NetCore.RecvData>();
        public override ushort Port => throw new NotImplementedException();

        public override string IpAddress => throw new NotImplementedException();

        public void OnReceiveData(IntPtr pData, int length)
        {
            unsafe
            {
                PkgBuilder.ParsePackage((byte*)pData.ToPointer(), (UInt32)length, this.OnPacketOK, PkgPoolMgr);
            }
        }
        private void OnPacketOK(NetCore.RecvData rcv)
        {
            lock (Packages)
            {
                Packages.Enqueue(rcv);
            }
        }
        public override void SendBuffer(IntPtr ptr, int count)
        {
            #warning 这里性能问题严重，需要以后处理
            //这里性能问题严重，需要以后处理
            byte[] temp = new byte[count];
            unsafe
            {
                var pHeader = (NetCore.PkgHeader*)ptr;
                pHeader->PackageSize = (ushort)count;
                fixed (byte* dest = &temp[0])
                {
                    CoreSDK.SDK_Memory_Copy(dest, (void*)ptr, (UInt32)count);
                }
            }
            Session.Send(temp, 0, count);
        }

        public override void Update()
        {
            
        }
    }

    public class TcpConnectPool
    {
        internal TcpConnect[] Pools;
        private Queue<TcpConnect> FreeObjects = new Queue<TcpConnect>();
        public TcpConnectPool(UInt16 num = 8192)
        {
            Pools = new TcpConnect[num];
            for (UInt16 i = 0; i < num; i++)
            {
                Pools[i] = new TcpConnect();
                Pools[i].IndexInPool = i;
                Pools[i].IsFree = true;
                FreeObjects.Enqueue(Pools[i]);
            }
        }
        public TcpConnect AllocConnect()
        {
            lock (FreeObjects)
            {
                if (FreeObjects.Count == 0)
                    return null;
                var r = FreeObjects.Dequeue();
                r.IsFree = false;
                return r;
            }
        }
        public void FreeConnect(TcpConnect conn)
        {
            lock(FreeObjects)
            {
                if (conn.IsFree == true)
                    return;
                conn.IsFree = true;
                FreeObjects.Enqueue(conn);
            }
        }
    }
}
