using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.TcpServer
{
    public class UTcpConnect : AuxPtrType<EngineNS.TcpConnect>, Bricks.Network.INetConnect
    {
        internal static unsafe EngineNS.TcpConnect.FDelegate_FOnTcpConnectRcvData OnTcpConnectRcvData = OnTcpConnectRcvDataImpl;
        private static unsafe void OnTcpConnectRcvDataImpl(EngineNS.TcpConnect arg0, byte* arg1, int arg2)
        {
            if (arg0.GCHandle == (void*)0)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)arg0.GCHandle);

            var connect = gcHandle.Target as UTcpConnect;
            connect.OnRcvData(arg1, arg2);
        }
        public unsafe UTcpConnect(EngineNS.TcpConnect coreObject)
        {
            mCoreObject = coreObject;
            mCoreObject.NativeSuper.AddRef();
            mCoreObject.GCHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(this)).ToPointer();

            mPkgBuilder.NetPackageManager = NetPackageManager;
        }
        public Bricks.Network.UNetPackageManager NetPackageManager = new Network.UNetPackageManager();
        Bricks.Network.RPC.PacketBuilder mPkgBuilder = new Bricks.Network.RPC.PacketBuilder();
        protected unsafe virtual void OnRcvData(byte* ptr, int size)
        {
            mPkgBuilder.ParsePackage(ptr, (uint)size, this);
        }

        public bool Connected 
        {
            get
            {
                return mCoreObject.mConnected;
            }
            set
            {

            }
        }
        public UInt16 GetConnectId()
        {
            return (UInt16)mCoreObject.mConnId;
        }
        public void Send(ref IO.AuxWriter<Bricks.Network.RPC.UMemWriter> pkg)
        {
            unsafe 
            {
                mCoreObject.Send((byte*)pkg.CoreWriter.Writer.GetDataPointer(), (int)pkg.CoreWriter.Writer.Tell());
            }
        }
        public virtual void Tick()
        {
            NetPackageManager.Tick();
        }
    }
}
