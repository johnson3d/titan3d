using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RemoteServices
{
    public partial class RPCExecManager
    {
        public object Execute(PkgReader pkg, Net.NetConnection connect, ref RPCRouter.RouteData routeInfo)
        {
            UInt16 rpcIndex = 0;
            UInt32 rpcHash = 0;
            bool hasReturn = false;
            bool isHashIndex = pkg.IsHashIndex();
            if (isHashIndex)
            {
                hasReturn = pkg.IsHasReturn();
                pkg.Read(out rpcHash);
            }
            else
            {
                pkg.Read(out rpcIndex);
                hasReturn = (rpcIndex & WaitFlag) != 0 ? true : false;
            }
            if (hasReturn != pkg.IsHasReturn())
            {
                System.Diagnostics.Debug.Assert(false);
            }
            UInt16 serialId = 0;
            if (hasReturn)
            {
                pkg.Read(out serialId);
                rpcIndex = (UInt16)(rpcIndex & MaxRPC);
            }
            RPCExec exec = null;
            if (isHashIndex)
            {
                exec = GetExecByHash(rpcHash);
            }
            else
            {
                exec = GetExecByIndex(rpcIndex);
            }
            if (exec == null)
            {
                System.Diagnostics.Debug.WriteLine($"RPC GetExecByIndex is null:{rpcIndex}");
                return null;
            }
            var parameter = exec.CreateParameter(pkg);
            parameter.SerialId = serialId;
            parameter.RouteInfo = routeInfo;
            parameter.Connect = connect;
            parameter.ExtraReader = pkg;
            object host = parameter.GetHostObject();
            if (host == null)
            {
                exec.DestroyParameter(parameter);
                System.Diagnostics.Debug.WriteLine($"RPC HostObject is null:{parameter.GetType().FullName}");
                return null;
            }
            var authority = parameter.GetAuthority(host);
            if (authority < exec.CallAttr.LimitLevel)
            {//超越权限
                exec.DestroyParameter(parameter);
                System.Diagnostics.Debug.WriteLine($"Over Authority[{authority}<{exec.CallAttr.LimitLevel.ToString()}]:{parameter.GetHostObjectName()}=>{parameter.GetRPCMethod().Name}");
                return null;
            }

            try
            {
                return parameter.DoExecute(exec, host);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return null;
            }
            finally
            {
                exec.DestroyParameter(parameter);
            }
        }

        public IPackageReceiver PkgRcver
        {
            get;
            set;
        }

        #region Connect

        public Net.NetConnection C2SConnect
        {
            get;
            set;
        }

        public Net.NetConnection GetServerConnect(ERouteTarget target)
        {
            return PkgRcver.GetServerConnect(target);
        }
        private Net.NetConnection RouterTargetConnect(ERouteTarget target, RPCRouter router)
        {
            switch (target)
            {
                case ERouteTarget.Reg:
                case ERouteTarget.Log:
                case ERouteTarget.Path:
                case ERouteTarget.Data:
                case ERouteTarget.Keep:
                    return PkgRcver.GetServerConnect(target);
                case ERouteTarget.Hall:
                    return router.ToHConnect;
                case ERouteTarget.Gate:
                    return router.ToGConnect;
                default:
                    break;
            }
            return null;
        }
        #endregion

        #region NetEvent
        public void ReceiveData(Net.NetConnection sender, byte[] pData, int nLength, Int64 recvTime)
        {
            unsafe
            {
                if (nLength < sizeof(RPCHeader))
                {
                    return;
                }
            }

            var router = sender.Router as RPCRouter;
            if (router == null)
            {
                System.Diagnostics.Debug.WriteLine($"connect bind a invalid data");
                return;
            }
            var pkg = new PkgReader(pData, nLength, recvTime);
            if (PkgRcver != null)
            {
                PkgRcver.OnReceivePackage(sender, pkg);
            }

            byte route = 0;
            pkg.Read(out route);
            var isReturn = (route & (byte)ERouteTarget.ReturnFlag) != 0 ? true : false;
            route = (byte)(route & (byte)ERouteTarget.ReturnMask);
            ERouteTarget target = (ERouteTarget)route;
            switch (target)
            {
                case ERouteTarget.Self:
                    {
                        if (isReturn)
                        {
                            UInt16 serialId = 0;
                            pkg.Read(out serialId);
                            DoCallBack(serialId, pkg);
                        }
                        else
                        {
                            Execute(pkg, sender, ref router.RouteInfo);
                        }
                    }
                    break;
                case ERouteTarget.Routed:
                    {
                        var routeInfo = new RPCRouter.RouteData();
                        routeInfo.Load(pkg);
                        Execute(pkg, sender, ref routeInfo);
                    }
                    break;
                case ERouteTarget.Client:
                    {//只有GateServer才有转发到客户端的需求
#if Server
                        var routePkg = new PkgWriter();
                        var routeInfo = new RPCRouter.RouteData();
                        routeInfo.Load(pkg);
                        if (Vise3D.Server.IGateServer.Instance != null)
                        {
                            if (isReturn)
                            {
                                route = (byte)((byte)ERouteTarget.Self | (byte)ERouteTarget.ReturnFlag);
                                routePkg.Write(route);
                                UInt16 seriaId;
                                pkg.Read(out seriaId);
                                routePkg.Write(seriaId);
                            }
                            else
                            {
                                route = (byte)ERouteTarget.Self;
                                routePkg.Write(route);
                            }
                            
                            routePkg.AppendPkg(pkg, pkg.GetPosition());
                            routePkg.SendBuffer(Vise3D.Server.IGateServer.Instance.GetClientConnect(routeInfo.RouteSlot));
                        }
#endif
                    }
                    break;
                case ERouteTarget.Hall:
                    {
                        var routePkg = new PkgWriter(nLength+64);
                        routePkg.Write((byte)ERouteTarget.Routed);
                        router.RouteInfo.Save(routePkg, ERouteTarget.Hall);
                        routePkg.SetFlags(pkg.GetFlags());
                        routePkg.AppendPkg(pkg, pkg.GetPosition());
                        routePkg.SendBuffer(RouterTargetConnect(target, router));
                        routePkg.Dispose();
                    }
                    break;
                case ERouteTarget.Data:
                case ERouteTarget.Keep:
                case ERouteTarget.Reg:
                case ERouteTarget.Path:
                case ERouteTarget.Log:
                    {
                        var routePkg = new PkgWriter(nLength+64);
                        routePkg.Write((byte)ERouteTarget.Routed);
                        router.RouteInfo.Save(routePkg, target);
                        routePkg.SetFlags(pkg.GetFlags());
                        routePkg.AppendPkg(pkg, pkg.GetPosition());
                        routePkg.SendBuffer(GetServerConnect(target));
                        routePkg.Dispose();
                    }
                    break;
            }

            pkg.Dispose();
        }
        #endregion
    }

    public class FactoryObjectAttribute : System.Attribute
    {

    }
    public class FactoryObjectManager
    {
        public static FactoryObjectManager Instance = new FactoryObjectManager();
        public Dictionary<System.Type, System.Type> Factorys
        {
            get;
        } = new Dictionary<Type, Type>();

        private static Type GetRootAttributeType(Type t, Type root)
        {
            var attrs = t.GetCustomAttributes(typeof(FactoryObjectAttribute), false);
            if (attrs != null && attrs.Length > 0)
            {
                root = t;
            }

            if (t.BaseType == null)
                return root;
            else
                return GetRootAttributeType(t.BaseType, root);
        }
        public void BuildFactory(System.Reflection.Assembly[] assems)
        {
            if(assems==null)
            {
                assems = AppDomain.CurrentDomain.GetAssemblies();
            }
            for (var i = 0; i < assems.Length; i++)
            {
                try
                {
                    var assm = assems[i];
                    var types = assm.GetTypes();
                    foreach (var t in types)
                    {
                        var attrs = t.GetCustomAttributes(typeof(FactoryObjectAttribute), true);
                        if (attrs == null || attrs.Length == 0)
                            continue;
                        var root = GetRootAttributeType(t, t);
                        var att = attrs[0] as FactoryObjectAttribute;
                        System.Type curType = null;
                        if (Factorys.TryGetValue(root, out curType) == false)
                        {
                            Factorys.Add(root, t);
                        }
                        else
                        {
                            if (t.IsSubclassOf(curType))
                                Factorys[root] = t;
                        }
                    }
                }
                catch(System.Exception ex)
                {
                    EngineNS.Profiler.Log.WriteException(ex);
                }
            }
        }

        public T CreateObject<T>() where T : class
        {
            return CreateObject(typeof(T)) as T;
        }
        public object CreateObject(Type baseType)
        {
            Type t;
            if (Factorys.TryGetValue(baseType, out t))
            {
                return System.Activator.CreateInstance(t);
            }
            System.Diagnostics.Debug.WriteLine($"Class {baseType.FullName} isn't a Factory Type");
            return null;
        }
    }
}