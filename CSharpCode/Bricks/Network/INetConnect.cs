using System;
using System.Collections.Generic;
using EngineNS.Bricks.Network.RPC;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.Network
{
    public class FNetworkPoint : IO.BaseSerializer
    {
        public static FNetworkPoint FromString(string str)
        {
            FNetworkPoint result = new FNetworkPoint();
            try
            {
                var segs = str.Split(':');
                result.Ip = segs[0];
                result.Port = System.Convert.ToUInt16(segs[1]);
                return result;
            }
            catch
            {
                return new FNetworkPoint();
            }
        }
        [Rtti.Meta]
        public string Ip { get; set; }
        [Rtti.Meta]
        public UInt16 Port { get; set; }
        public override string ToString()
        {
            return $"{Ip}:{Port}";
        }
    }

    public class FLoginResultArgument : IO.BaseSerializer
    {
        [Rtti.Meta]
        public Bricks.Network.FNetworkPoint GatewayURL { get; set; }
        [Rtti.Meta]
        public Guid Sessiond { get; set; }
    }

    public unsafe interface INetConnect
    {
        RPC.EAuthority Authority { get; set; }
        bool Connected { get; set; }
        object Tag { get; set; }
        UInt16 GetConnectId();
        void Send(void* ptr, uint size);
        void Send(in IO.AuxWriter<IO.UMemWriter> pkg);
    }
    public class UFakeNetConnect : INetConnect
    {
        public RPC.EAuthority Authority { get; set; } = EAuthority.Client;
        public bool Connected 
        { 
            get => true; 
            set
            {

            }
        }
        public object Tag { get; set; } = null;
        public UInt16 GetConnectId()
        {
            return 0;
        }
        public void Send(in IO.AuxWriter<IO.UMemWriter> pkg)
        {
            unsafe
            {
                //UEngine.Instance.RpcModule.NetPackageManager.PushPackage(pkg.CoreWriter.Writer.GetPointer(), (uint)pkg.CoreWriter.Writer.Tell(), this);
                Send(pkg.CoreWriter.Writer.GetPointer(), (uint)pkg.CoreWriter.Writer.Tell());
            }
        }
        public unsafe void Send(void* ptr, uint size)
        {
            UEngine.Instance.RpcModule.NetPackageManager.PushPackage(ptr, size, this);
        }
    }
}
