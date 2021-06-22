using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Network
{
    public class UTcpClient : AuxPtrType<EngineNS.TcpClient>, INetConnect
    {
        public bool Connected { get; set; }
        public UInt16 ConnectId { get; private set; }
        public UInt16 GetConnectId()
        {
            return ConnectId;
        }
        public void Send(ref IO.AuxWriter<RPC.UMemWriter> pkg)
        {
            //mCoreObject.Send(pkg.CoreWriter.Writer);
        }
        public void Disconnect()
        {
            if (Connected == false)
                return;
            mCoreObject.Disconnect();
        }
        private System.Threading.Thread mRcvThread;
        public Support.UNativeArray<byte> mRcvBuffer = new Support.UNativeArray<byte>();
        public async System.Threading.Tasks.Task<bool> Connect(string ip, UInt16 port, int timeOut = 2000)
        {
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
                    mRcvBuffer.mCoreObject.SetCapacity(BufferLimit);
                }
                mRcvThread = new System.Threading.Thread(()=>
                {
                    while (true)
                    {
                        int error = 0;
                        if (mCoreObject.WaitData(ref error) != 0)
                        {
                            unsafe
                            {
                                var ptr = (byte*)mRcvBuffer.UnsafeAddressAt(0).ToPointer();
                                int SizeOfRecv = mCoreObject.RecvData(ptr, BufferLimit);
                                OnRcvData(ptr, SizeOfRecv);
                            }
                        }
                        else
                        {
                            Connected = false;
                            break;
                        }
                    }
                });
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
