using System;
using System.Collections.Generic;
using EngineNS.Bricks.Network.RPC;

namespace EngineNS.Bricks.Network
{
    public class UNetPackageManager
    {
        public List<IO.UMemWriter> RcvPacakages = new List<IO.UMemWriter>();
        public List<IO.UMemWriter> PushList = new List<IO.UMemWriter>();
        public unsafe void PushPackage(void* ptr, uint size, INetConnect connect)
        {
            using (var reader = IO.UMemReader.CreateInstance((byte*)ptr, size))
            {
                var pkg = new IO.AuxReader<IO.UMemReader>(reader, null);
                var pkgHeader = new RPC.FPkgHeader();
                pkg.Read(out pkgHeader);
                if (pkgHeader.IsHasReturn())
                {
                    UReturnContext retContext;
                    pkg.Read(out retContext);
                    if (retContext.RunTarget != TtEngine.Instance.RpcModule.RpcManager.CurrentTarget)
                    {
                        var conn = TtEngine.Instance.RpcModule.RpcManager.GetRunTargetConnect(in retContext, connect);
                        if (conn != null)
                        {
                            conn.Send(ptr, size);
                        }
                        else
                        {
                            Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Warning, $"Relay failed {retContext}");
                        }
                        return;
                    }
                }
                else
                {
                    URouter router1 = new URouter();
                    pkg.Read(out router1);
                    if (router1.RunTarget != ERunTarget.None && router1.RunTarget != TtEngine.Instance.RpcModule.RpcManager.CurrentTarget)
                    {
                        var pRouterAddr = (URouter*)((byte*)ptr + sizeof(RPC.FPkgHeader));
                        if (TtEngine.Instance.RpcModule.RpcManager.OnRelay(pRouterAddr, connect))
                        {
                            var conn = TtEngine.Instance.RpcModule.RpcManager.GetRunTargetConnect(in *pRouterAddr, connect);
                            if (conn != null)
                            {
                                conn.Send(ptr, size);
                            }
                            else
                            {
                                Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Warning, $"Relay failed {router1.RunTarget}");
                            }
                        }
                        else
                        {
                            Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Warning, $"OnRelay return false");
                        }
                        return;
                    }
                }
            }
            EngineNS.IO.UMemWriter tmp = EngineNS.IO.UMemWriter.CreateInstance();
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
                using (var reader = EngineNS.IO.UMemReader.CreateInstance((byte*)i.Writer.GetPointer(), i.Writer.Tell()))
                {
                    var pkg = new IO.AuxReader<EngineNS.IO.UMemReader>(reader, null);
                    var pkgHeader = new RPC.FPkgHeader();
                    pkg.Read(out pkgHeader);
                    if (pkgHeader.IsHasReturn())
                    {
                        UReturnContext retContext;
                        pkg.Read(out retContext);
                        TtEngine.Instance.RpcModule.RemoteReturn(retContext.Handle, ref pkg);
                    }
                    else
                    {
                        URouter router1 = new URouter();
                        pkg.Read(out router1);
                        if (router1.RunTarget != ERunTarget.None && router1.RunTarget != TtEngine.Instance.RpcModule.RpcManager.CurrentTarget)
                        {
                            Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Warning, $"{router1.RunTarget} != ERunTarget.None");
                            continue;
                        }
                        UInt16 methodIndex1 = 0;
                        pkg.Read(out methodIndex1);

                        var exe = TtEngine.Instance.RpcModule.RpcManager?.GetExecuter(in router1) as IRpcHost;
                        if (exe != null)
                        {
                            var fun = exe.GetRpcClass().GetCallee(methodIndex1);
                            var conn = i.Tag as INetConnect;
                            if (fun.Method != null && fun.Attribute.Authority <= conn.Authority && fun.Attribute.Authority <= router1.Authority)
                            {
                                UCallContext context = new UCallContext();
                                context.NetConnect = conn;
                                context.Callee = TtEngine.Instance.RpcModule.RpcManager.CurrentTarget;
                                fun.Method(pkg, exe, context);
                            }
                            else
                            {
                                if (fun.Method == null)
                                    Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Warning, $"{exe.GetType().FullName}.{methodIndex1} is null");
                                else if (fun.Attribute.Authority > conn.Authority)
                                    Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Warning, $"{exe.GetType().FullName}.{fun.Name}:{fun.Attribute.Authority} > {conn.Authority} || {fun.Attribute.Authority} > {router1.Authority}");
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
