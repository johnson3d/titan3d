using System;
using System.Collections.Generic;
using EngineNS.Bricks.Network.RPC;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.Network
{
    public interface INetConnect
    {
        bool Connected { get; set; }
        UInt16 GetConnectId();
        void Send(ref IO.AuxWriter<RPC.UMemWriter> pkg);
    }
    public class UFakeNetConnect : INetConnect
    {
        public bool Connected 
        { 
            get => true; 
            set
            {

            }
        }
        public UInt16 GetConnectId()
        {
            return 0;
        }
        public void Send(ref IO.AuxWriter<RPC.UMemWriter> pkg)
        {
            unsafe
            {
                UEngine.Instance.RpcModule.NetPackageManager.PushPackage(pkg.CoreWriter.Writer.GetPointer(), (uint)pkg.CoreWriter.Writer.Tell(), this);
            }
        }
    }
    public class UNetPackageManager
    {
        public List<RPC.UMemWriter> RcvPacakages = new List<RPC.UMemWriter>();
        public List<RPC.UMemWriter> PushList = new List<RPC.UMemWriter>();
        public unsafe void PushPackage(void* ptr, uint size, INetConnect connect)
        {
            RPC.UMemWriter tmp = RPC.UMemWriter.CreateInstance();
            tmp.WritePtr(ptr, (int)size);
            tmp.Tag = connect;

            lock (PushList)
            {
                PushList.Add(tmp);
            }
        }
        public unsafe void Tick()
        {
            lock (PushList)
            {
                if (PushList.Count > 0)
                    RcvPacakages.AddRange(PushList);

                PushList.Clear();
            }

            foreach (var i in RcvPacakages)
            {
                using (var reader = UMemReader.CreateInstance((byte*)i.Writer.GetPointer(), i.Writer.Tell()))
                {
                    var pkg = new IO.AuxReader<UMemReader>(reader, null);
                    var pkgHeader = new RPC.FPkgHeader();
                    pkg.Read(out pkgHeader);
                    if (pkgHeader.IsHasReturn())
                    {
                        UReturnContext retContext;
                        pkg.Read(out retContext);
                        UEngine.Instance.RpcModule.RemoteReturn(retContext.Handle, ref pkg);
                    }
                    else
                    {
                        URouter router1 = new URouter();
                        pkg.Read(out router1);
                        UInt16 methodIndex1 = 0;
                        pkg.Read(out methodIndex1);

                        var exe = UEngine.Instance.RpcModule.RpcManager?.GetExecuter(ref router1) as IRpcHost;
                        var fun = exe?.GetRpcClass().GetCallee(methodIndex1);
                        if (fun != null)
                        {
                            UCallContext context = new UCallContext();
                            context.NetConnect = i.Tag as INetConnect;
                            context.Callee = UEngine.Instance.RpcModule.RpcManager.CurrentTarget;
                            fun(pkg, exe, context);
                        }
                    }
                }
                i.Dispose();
            }
            RcvPacakages.Clear();
        }
    }
}
