using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.TcpClient
{
    internal class CoreTcpClient : AuxCoreObject<CoreTcpClient.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }

        CTcpClient mHost;
        public CoreTcpClient(CTcpClient host)
        {
            mHost = host;
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("TCPClient");
        }
        ~CoreTcpClient()
        {
            Disconnect();
        }
        public bool mConnected = false;
        public bool Connect(string address, int port, int timeout)
        {
            if (mConnected)
                return false;

            var ok = SDK_TCPClient_Connect(CoreObject, address, port, timeout);
            if (ok == false)
                return false;

            PushBuffer.Reset();
            PopBuffer.Reset();

            mRcvThread = new System.Threading.Thread(ReceiveLoop);
            IsRun = true;
            mRcvThread.Name = "CTcpClientRcv";
            mRcvThread.Start();

            SendThreadStoped = false;
            mSndThread = new System.Threading.Thread(SendLoop);
            mSndThread.Name = "CTcpClientSnd";
            mSndThread.Start();

            mConnected = true;
            return true;
        }
        protected bool IsRun = false;
        protected System.Threading.Thread mRcvThread;
        protected System.Threading.Thread mSndThread;
        public void Disconnect(bool bySendThread = false)
        {
            if (mConnected == false)
                return;

            mConnected = false;
            IsRun = false;
            SDK_TCPClient_Disconnect(CoreObject);

            if (bySendThread == false)
            {
                while (SendThreadStoped == false)
                {
                    mSenderEvent.Set();
                }
            }

            CEngine.Instance.EventPoster.RunOn(() =>
            {
                if (mHost.OnDisconnected != null)
                {
                    mHost.OnDisconnected(mHost);
                }
                return null;
            }, Thread.Async.EAsyncTarget.Main);
        }
        public int PacketNumber;
        public int RecvPacketNumLimitter = 256;
        public int KickedWeakPkgNum;
        public int ProcPackNumber;
        public void Update()
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
                        if (mHost.OnReceiveData != null)
                            mHost.OnReceiveData(mHost, vBytes.PkgData, vBytes.Length, vBytes.RecvTime);
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
        private byte[] RecvBuffer = new byte[1024 * 1024];
        NetCore.PkgPoolManager mPkgPoolMgr = new NetCore.PkgPoolManager();
        protected NetCore.PacketBuilder mPkgBuilder = new NetCore.PacketBuilder();

        protected Queue<NetCore.RecvData> mRecvQueue = new Queue<NetCore.RecvData>();

        private void OnPacketOK(NetCore.RecvData rcvData)
        {
            //if (DiscardAllPacket)
            //    return;
            lock (mRecvQueue)
            {
                mRecvQueue.Enqueue(rcvData);
            }
        }
        private void ReceiveLoop()
        {
            while(IsRun)
            {
                unsafe
                {
                    int errCode = 0;
                    if (WaitData(&errCode))
                    {
                        fixed (byte* p = &RecvBuffer[0])
                        {
                            int rcvNum = RecvData(p, (UInt32)RecvBuffer.Length);
                            if(rcvNum<=0)
                            {
                                Disconnect();
                                break;
                            }
                            mPkgBuilder.ParsePackage(p, (UInt32)rcvNum, this.OnPacketOK, mPkgPoolMgr);
                        }
                    }
                    else
                    {
                        if(errCode < 0)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Socket", $"Select return {errCode}");
                        }
                    }
                }
            }
        }
        public unsafe int Send(byte* p, UInt32 size)
        {
            if (mConnected == false)
                return 0;
            return SDK_TCPClient_Send(CoreObject, p, size);
        }
        public unsafe bool WaitData(int* errCode)
        {
            return SDK_TCPClient_WaitData(CoreObject, errCode);
        }
        public unsafe int RecvData(byte* pBuffer, UInt32 bufferSize)
        {
            return SDK_TCPClient_RecvData(CoreObject, pBuffer, bufferSize);
        }

        internal class WaitSends
        {
            public const int BufferSize = 1024 * 1024;
            public byte[] Buffers = new byte[BufferSize];
            public int Size;
            public void Reset()
            {
                Size = 0;
            }
            public unsafe bool PushData(byte* p, int len)
            {
                if (len < 0)
                    return false;
                if (Size + len >= Buffers.Length)
                    return false;

                fixed (byte* tar = &Buffers[Size])
                {
                    CoreSDK.SDK_Memory_Copy(tar, p, (uint)len);
                }
                Size += len;
                return true;
            }
            public void Sender(CoreTcpClient client)
            {
                if (Size <= 0)
                {
                    Size = 0;
                    return;
                }
                unsafe
                {
                    int start = 0;
                    fixed (byte* ptr = &Buffers[0])
                    {
                        while (Size > 0)
                        {
                            if(client.mConnected==false)
                            {
                                Size = 0;
                                break;
                            }
                            int sendSize = client.Send(&ptr[start], (UInt32)Size);
                            if(sendSize==-1)
                            {
                                client.Disconnect(true);
                                Size = 0;
                                break;
                            }
                            Size -= sendSize;
                            start += sendSize;
                        }
                    }

                    System.Diagnostics.Debug.Assert(Size == 0);
                }
            }
        }
        private WaitSends PushBuffer = new WaitSends();
        private WaitSends PopBuffer = new WaitSends();
        private System.Threading.ManualResetEvent mSenderEvent = new System.Threading.ManualResetEvent(false);

        public unsafe void SendBuffer(byte* p, UInt32 size)
        {
            if (size >= WaitSends.BufferSize)
                return;

            lock (this)
            {
                if(PushBuffer.PushData(p, (int)size)==false)
                {
                    long t = Support.Time.GetTickCount();
                    while(PopBuffer.Size!=0)
                    {
                        var t1 = Support.Time.GetTickCount();
                        if(t1-t>50000)
                        {
                            PopBuffer.Reset();
                            System.Diagnostics.Debug.Assert(false);
                            break;
                        }
                        mSenderEvent.Set();
                    }
                    var saved = PopBuffer;
                    PopBuffer = PushBuffer;
                    PushBuffer = saved;
                    PushBuffer.PushData(p, (int)size);
                }
                mSenderEvent.Set();
            }
        }
        private bool SendThreadStoped = false;
        private void SendLoop()
        {
            while (IsRun)
            {
                mSenderEvent.WaitOne();

                PopBuffer.Sender(this);
                lock (this)
                {
                    var saved = PopBuffer;
                    PopBuffer = PushBuffer;
                    PushBuffer = saved;
                    if (PopBuffer.Size == 0 && PushBuffer.Size == 0)
                    {
                        mSenderEvent.Reset();
                    }
                }
            }
            SendThreadStoped = true;
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_TCPClient_Connect(NativePointer self, string address, int port, int timeout);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_TCPClient_Disconnect(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe int SDK_TCPClient_Send(NativePointer self, byte* p, UInt32 size);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_TCPClient_WaitData(NativePointer self, int* errCode);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe int SDK_TCPClient_RecvData(NativePointer self, byte* pBuffer, UInt32 bufferSize);
        #endregion
    }

    public class CTcpClient : NetCore.NetConnection
    {
        public CTcpClient()
        {
            mClient = new CoreTcpClient(this);
        }
        CoreTcpClient mClient;
        public delegate void FOnReceiveData(CTcpClient pClient, byte[] pData, int nLength, Int64 recvTime);
        public FOnReceiveData OnReceiveData;
        public delegate void FOnDisconnected(CTcpClient pClient);
        public FOnDisconnected OnDisconnected;
        public override bool Connected
        {
            get { return mClient.mConnected; }
        }
        public override async System.Threading.Tasks.Task<bool> Connect(string strHostIp, UInt16 nPort, int timeOutMillisecond = 3000)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            mPort = nPort;
            mIpAddress = strHostIp;
            return mClient.Connect(strHostIp, nPort, timeOutMillisecond);
        }
        public override void Update()
        {
            mClient.Update();
        }

        public override void SendBuffer(IntPtr ptr, int count)
        {
            unsafe
            {
                var pHeader = (NetCore.PkgHeader*)ptr;
                pHeader->PackageSize = (ushort)count;

                //mClient.Send((byte*)ptr, (UInt32)count);
                mClient.SendBuffer((byte*)ptr, (UInt32)count);
            }
        }

        public override void Disconnect()
        {
            mClient.Disconnect();
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
    }
}
