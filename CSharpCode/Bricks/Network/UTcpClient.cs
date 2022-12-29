using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Network
{
    public class UTcpClient : AuxPtrType<EngineNS.TcpClient>, INetConnect
    {
        public UTcpClient()
        {
            mCoreObject = EngineNS.TcpClient.CreateInstance();
            mRcvBuffer = Support.UNativeArray<byte>.CreateInstance();
        }
        ~UTcpClient()
        {
            mRcvBuffer.Dispose();
        }
        public RPC.EAuthority Authority { get; set; } = RPC.EAuthority.Client;
        public bool Connected { get; set; }
        public object Tag { get; set; } = null;
        public UInt16 ConnectId { get; private set; }
        public UInt16 GetConnectId()
        {
            return ConnectId;
        }
        public unsafe void Send(void* ptr, uint size)
        {
            if (Connected == false)
                return;
            mCoreObject.Send((sbyte*)ptr, size);
        }
        public void Send(in IO.AuxWriter<RPC.UMemWriter> pkg)
        {
            if (Connected == false)
                return;
            unsafe
            {
                mCoreObject.Send((sbyte*)pkg.CoreWriter.Writer.GetPointer(), (uint)pkg.CoreWriter.Writer.Tell());
            }
        }
        public void Disconnect()
        {
            if (Connected == false)
                return;
            Connected = false;
            mCoreObject.Disconnect();            
        }
        private System.Threading.Thread mRcvThread;
        public Support.UNativeArray<byte> mRcvBuffer;
        public async System.Threading.Tasks.Task<bool> Connect(string ip, UInt16 port, UNetPackageManager pkgManager = null, UInt16 connId = UInt16.MinValue, int timeOut = 2000)
        {
            if (pkgManager != null)
                mPkgBuilder.NetPackageManager = pkgManager;
            else
                mPkgBuilder.NetPackageManager = UEngine.Instance.RpcModule.NetPackageManager;
            ConnectId = connId;
            var ok = await UEngine.Instance.EventPoster.Post(() =>
            {
                return mCoreObject.Connect(ip, port, timeOut);
            }, Thread.Async.EAsyncTarget.TPools);

            Connected = ok != 0;

            const int BufferLimit = 1024 * 16;
            if (Connected)
            {
                unsafe
                {
                    mRcvBuffer.mCoreObject.SetSize(BufferLimit);
                }
                mRcvThread = new System.Threading.Thread(()=>
                {
                    while (true)
                    {
                        int error = 0;
                        var ret = mCoreObject.WaitData(ref error);
                        if (error == -1)
                        {
                            break;
                        }
                        else if(ret == 1)
                        {
                            unsafe
                            {
                                var ptr = (byte*)mRcvBuffer.UnsafeAddressAt(0).ToPointer();
                                int SizeOfRecv = mCoreObject.RecvData(ptr, BufferLimit);

                                if (SizeOfRecv > 0)
                                    OnRcvData(ptr, SizeOfRecv);
                            }
                            
                        }
                    }
                    Connected = false;
                });
                mRcvThread.Name = $"TcpClient";
                mRcvThread.Start();
            }
            return Connected;
        }
        RPC.PacketBuilder mPkgBuilder = new RPC.PacketBuilder();
        private unsafe void OnRcvData(byte* ptr, int size)
        {
            mPkgBuilder.ParsePackage(ptr, (uint)size, this);
        }
    }
}
