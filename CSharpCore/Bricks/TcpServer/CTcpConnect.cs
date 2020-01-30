using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.TcpServer
{
    public class CTcpConnect : NetCore.NetConnection
    {
        private CTcpServer mHostServer;
        private NetCore.PacketBuilder mPkgBuilder = new NetCore.PacketBuilder();
        private NetCore.PkgPoolManager mPkgPoolMgr = new NetCore.PkgPoolManager();
        private Queue<NetCore.RecvData> mRecvQueue = new Queue<NetCore.RecvData>();
        internal CTcpConnect(CTcpServer server, string ip, UInt16 port, IntPtr cid)
        {
            mHostServer = server;
            mPort = port;
            mIpAddress = ip;
            mConnectId = cid;
            mConnected = true;
        }
        internal void OnDataReceive(IntPtr p, int size)
        {
            unsafe
            {
                mPkgBuilder.ParsePackage((byte*)p, (UInt32)size, this.OnPacketOK, mPkgPoolMgr);
            }
        }
        internal void OnDisconnect()
        {
            mConnected = false;
        }
        private void OnPacketOK(NetCore.RecvData rcvData)
        {
            //if (DiscardAllPacket)
            //    return;
            lock (mRecvQueue)
            {
                mRecvQueue.Enqueue(rcvData);
            }
        }
        IntPtr mConnectId;
        public IntPtr ConnectId
        {
            get { return mConnectId; }
        }
        System.UInt16 mPort;
        public override System.UInt16 Port
        {
            get
            {
                return mPort;
            }
        }

        string mIpAddress;
        public override string IpAddress
        {
            get { return mIpAddress; }
        }
        public override async System.Threading.Tasks.Task<bool> Connect(string strHostIp, UInt16 nPort, int timeOutMillisecond = 3000)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return false;
        }
        public override void Disconnect()
        {
            mHostServer?.Server?.Disconnect(mConnectId);
            OnDisconnect();
        }
        public int PacketNumber;
        public int RecvPacketNumLimitter = 256;
        public int KickedWeakPkgNum;
        public int ProcPackNumber;
        public override void Update()
        {
            try
            {
                NetCore.RecvData vBytes = null;
                PacketNumber = mRecvQueue.Count;
                int WeakPkgNum = 0;
                int nProcNumber = 0;
                bool DiscardWeak = false;
                if (PacketNumber > RecvPacketNumLimitter)
                    DiscardWeak = true;

                lock (mRecvQueue)
                {
                    for (int i = 0; i < PacketNumber; i++)
                    {
                        if (mRecvQueue.Count == 0)
                            break;
                        vBytes = mRecvQueue.Dequeue();
                        if (vBytes == null)
                            continue;
                        if (DiscardWeak)
                        {
                            unsafe
                            {
                                fixed (byte* pPkg = &vBytes.PkgData[0])
                                {
                                    if (((NetCore.PkgHeader*)pPkg)->IsWeakPkg())
                                    {
                                        WeakPkgNum++;
                                        continue;
                                    }
                                }
                            }
                        }

                        nProcNumber++;
                        mHostServer.OnRcvPackage(this, vBytes.PkgData, vBytes.Length, vBytes.RecvTime);
                        vBytes.Dispose();
                    }
                }

                KickedWeakPkgNum = WeakPkgNum;
                ProcPackNumber = nProcNumber;
            }
            catch (System.Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
        }
        public override void SendBuffer(IntPtr ptr, int count)
        {
            unsafe
            {
                var pHeader = (NetCore.PkgHeader*)ptr;
                pHeader->PackageSize = (ushort)count;

                //mClient.Send((byte*)ptr, (UInt32)count);
                mHostServer?.Server?.Send(mConnectId, ptr, count);
            }
        }
        bool mConnected;
        public override bool Connected
        {
            get { return mConnected; }
        }
    }
}
