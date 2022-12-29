using System;
using System.Collections.Generic;
using EngineNS.Bricks.Network.RPC;

namespace EngineNS.Bricks.Network
{
    public class UNetPackageManager
    {
        public List<RPC.UMemWriter> RcvPacakages = new List<RPC.UMemWriter>();
        public List<RPC.UMemWriter> PushList = new List<RPC.UMemWriter>();
        public unsafe void PushPackage(void* ptr, uint size, INetConnect connect)
        {
            using (var reader = UMemReader.CreateInstance((byte*)ptr, size))
            {
                var pkg = new IO.AuxReader<UMemReader>(reader, null);
                var pkgHeader = new RPC.FPkgHeader();
                pkg.Read(out pkgHeader);
                if (pkgHeader.IsHasReturn())
                {
                    UReturnContext retContext;
                    pkg.Read(out retContext);
                    if (retContext.RunTarget != UEngine.Instance.RpcModule.RpcManager.CurrentTarget)
                    {
                        var conn = UEngine.Instance.RpcModule.RpcManager.GetRunTargetConnect(in retContext, connect);
                        if (conn != null)
                        {
                            conn.Send(ptr, size);
                        }
                        else
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "RPC", $"Relay failed {retContext}");
                        }
                        return;
                    }
                }
                else
                {
                    URouter router1 = new URouter();
                    pkg.Read(out router1);
                    if (router1.RunTarget != ERunTarget.None && router1.RunTarget != UEngine.Instance.RpcModule.RpcManager.CurrentTarget)
                    {
                        var pRouterAddr = (URouter*)((byte*)ptr + sizeof(RPC.FPkgHeader));
                        if (UEngine.Instance.RpcModule.RpcManager.OnRelay(pRouterAddr, connect))
                        {
                            var conn = UEngine.Instance.RpcModule.RpcManager.GetRunTargetConnect(in *pRouterAddr, connect);
                            if (conn != null)
                            {
                                conn.Send(ptr, size);
                            }
                            else
                            {
                                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "RPC", $"Relay failed {router1.RunTarget}");
                            }
                        }
                        else
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "RPC", $"OnRelay return false");
                        }
                        return;
                    }
                }
            }
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
                        if (router1.RunTarget != ERunTarget.None && router1.RunTarget != UEngine.Instance.RpcModule.RpcManager.CurrentTarget)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "RPC", $"{router1.RunTarget} != ERunTarget.None");
                            continue;
                        }
                        UInt16 methodIndex1 = 0;
                        pkg.Read(out methodIndex1);

                        var exe = UEngine.Instance.RpcModule.RpcManager?.GetExecuter(in router1) as IRpcHost;
                        if (exe != null)
                        {
                            var fun = exe.GetRpcClass().GetCallee(methodIndex1);
                            var conn = i.Tag as INetConnect;
                            if (fun.Method != null && fun.Attribute.Authority <= conn.Authority && fun.Attribute.Authority <= router1.Authority)
                            {
                                UCallContext context = new UCallContext();
                                context.NetConnect = conn;
                                context.Callee = UEngine.Instance.RpcModule.RpcManager.CurrentTarget;
                                fun.Method(pkg, exe, context);
                            }
                            else
                            {
                                if (fun.Method == null)
                                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "RPC", $"{exe.GetType().FullName}.{methodIndex1} is null");
                                else if (fun.Attribute.Authority > conn.Authority)
                                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "RPC", $"{exe.GetType().FullName}.{fun.Name}:{fun.Attribute.Authority} > {conn.Authority} || {fun.Attribute.Authority} > {router1.Authority}");
                            }
                        }
                    }
                }
                i.Dispose();
            }
            RcvPacakages.Clear();
        }
    }
}
