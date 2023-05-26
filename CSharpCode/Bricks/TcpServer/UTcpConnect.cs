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
        public unsafe override void Dispose()
        {
            if (mCoreObject.GCHandle != (void*)0)
            {
                System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)mCoreObject.GCHandle).Free();
                mCoreObject.GCHandle = (void*)0;
            }
            base.Dispose();
        }
        public Bricks.Network.UNetPackageManager NetPackageManager = new Network.UNetPackageManager();
        Bricks.Network.RPC.PacketBuilder mPkgBuilder = new Bricks.Network.RPC.PacketBuilder();
        protected unsafe virtual void OnRcvData(byte* ptr, int size)
        {
            mPkgBuilder.ParsePackage(ptr, (uint)size, this);
        }
        public Bricks.Network.RPC.EAuthority Authority { get; set; } = Bricks.Network.RPC.EAuthority.Client;
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
        public object Tag { get; set; } = null;
        public UInt16 GetConnectId()
        {
            return (UInt16)mCoreObject.mConnId;
        }
        public unsafe void Send(void* ptr, uint size)
        {
            if (Connected == false)
                return;
            mCoreObject.Send((byte*)ptr, (int)size);
        }
        public void Send(in IO.AuxWriter<IO.UMemWriter> pkg)
        {
            unsafe 
            {
                mCoreObject.Send((byte*)pkg.CoreWriter.Writer.GetPointer(), (int)pkg.CoreWriter.Writer.Tell());
            }
        }
        public virtual void Tick()
        {
            NetPackageManager.Tick();
        }
    }
}
