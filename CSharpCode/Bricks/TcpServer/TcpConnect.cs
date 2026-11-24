using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.TcpServer
{
    public class TtTcpConnect : AuxPtrType<EngineNS.TcpConnect>, Bricks.Network.INetConnect
    {
        internal static unsafe EngineNS.TcpConnect.FDelegate_FOnTcpConnectRcvData OnTcpConnectRcvData = OnTcpConnectRcvDataImpl;
        private static unsafe void OnTcpConnectRcvDataImpl(EngineNS.TcpConnect arg0, byte* arg1, int arg2)
        {
            if (arg0.GCHandle == (void*)0)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)arg0.GCHandle);

            var connect = gcHandle.Target as TtTcpConnect;
            connect.OnRcvData(arg1, arg2);
        }
        public unsafe TtTcpConnect(EngineNS.TcpConnect coreObject)
        {
            mCoreObject = coreObject;
            mCoreObject.NativeSuper.NativeSuper.AddRef();
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
        public Bricks.Network.TtNetPackageManager NetPackageManager = new Network.TtNetPackageManager();
        Bricks.Network.RPC.PacketBuilder mPkgBuilder = new Bricks.Network.RPC.PacketBuilder();
        protected unsafe virtual void OnRcvData(byte* ptr, int size)
        {
            // 这是原生接收线程回调进来的托管帧, 异常没有托管调用根: 不在这里兜住,
            // CLR 会直接终止整个进程(退出码 0xE0434352, 而且 stderr 可能什么都不留)
            try
            {
                mPkgBuilder.ParsePackage(ptr, (uint)size, this);
            }
            catch (System.Exception e)
            {
                System.Console.Error.WriteLine($"[Net] package parse failed({size} bytes): {e}");
                mPkgBuilder.ResetBroken();
            }
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
        public void Send(in IO.AuxWriter<IO.TtMemWriter> pkg)
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
