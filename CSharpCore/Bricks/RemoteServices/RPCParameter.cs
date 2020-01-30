using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EngineNS.Bricks.RemoteServices
{
    public enum ERouteTarget : byte
    {
        Unknown,
        Self,
        Routed,
        Client,
        Hall,
        Keep,
        Data,
        Reg,
        Log,
        Path,
        Gate,

        ReturnFlag = 0x80,
        ReturnMask = 0x7F,
    }
    
    public abstract class RPCParameter : RPCSerialize
    {
        public abstract RPCExec GetMethodBinder();
        public abstract void SystemSetMethodBinder(RPCExec exec);
        public System.Reflection.MethodInfo GetRPCMethod()
        {
            var exec = RPCExecManager.Insance.GetExecByIndex(GetMethodBinder().Index);
            if (exec == null)
                return null;
            return exec.Method;
        }

        #region CodeGen
        public static string GetRPCCode(System.Reflection.MethodInfo method)
        {
            var parameters = method.GetParameters();
            string hashStr = method.DeclaringType.FullName + "->" + method.ReturnType.FullName + " " + method.Name + "(";
            foreach (var i in parameters)
            {
                hashStr += i.ParameterType + " " + i.Name + ", ";
            }
            hashStr += ")";
            return hashStr;
        }
        public static string GetPkgReadCode(Type type)
        {
            var clasName = type.FullName.Replace('+', '.');
            string code = $"    public static void ReadPackage(Net.RPC.RPCParameter parameter, Net.RPC.PkgReader pkg)\r\n";
            code += "   {\r\n";
            code += $"      {clasName} arg = ({clasName})parameter;\r\n";
            var desc = IO.Serializer.TypeDescGenerator.Instance.GetTypeDesc(type);
            foreach(var i in desc.Members)
            {
                code += "       " + i.PropInfo.MemberType.FullName + " tmp_" + i.PropInfo.Name + ";\r\n";
                code += $"      pkg.Read(out tmp_{i.PropInfo.Name});\r\n";
                code += $"      arg.{i.PropInfo.Name} = tmp_{i.PropInfo.Name};\r\n";
            }
            code += "   }\r\n";
            return code;
        }
        public static string GetPkgWriteCode(Type type)
        {
            var clasName = type.FullName.Replace('+', '.');
            string code = $"    public static void WritePackage(Net.RPC.RPCParameter parameter, Net.RPC.PkgWriter pkg)\r\n";
            code += "   {\r\n";
            code += $"      {clasName} arg = ({clasName})parameter;\r\n";
            var desc = IO.Serializer.TypeDescGenerator.Instance.GetTypeDesc(type);
            foreach (var i in desc.Members)
            {
                code += $"      pkg.Write(arg.{i.PropInfo.Name});\r\n";
            }
            code += "   }\r\n";
            return code;
        }
        #endregion

        #region Send Pkg
        public delegate void FRPCTypeReturnCallBack<T>(T retValue, bool isTimeOut);
        
        private static bool CheckReturnType(Type paramType, Type retType)
        {
            if(paramType.BaseType.IsGenericType==false)
            {
                return false;
            }
            if (paramType.BaseType.GenericTypeArguments.Length == 3)
            {
                if (paramType.BaseType.GenericTypeArguments[2] != retType)
                    return false;
            }
            
            return true;
        }
        private static void AssertReturnType(Type paramType, Type retType)
        {
            var check = CheckReturnType(paramType, retType);
            if(check==false)
            {
                System.Diagnostics.Debugger.Break();
            }
        }
        public async Task<T> C2S_AwaitCall<T>(long timeOut, ERouteTarget route = ERouteTarget.Hall) where T : RPCReturnValue, new()
        {
            AssertReturnType(this.GetType(), typeof(T));
            if (route == ERouteTarget.Gate)
                route = ERouteTarget.Self;

            RPCExecManager.RPCWait waiter = null;
            if (timeOut==0)
                waiter = WaitDoCallImpl(0, null, route, RPCExecManager.Insance.C2SConnect); 
            else
                waiter = WaitDoCallImpl(timeOut, RPCAwaiter.NullCB, route, RPCExecManager.Insance.C2SConnect);

            var result = await RPCAwaiter.RPCWaitReturn<T>(waiter);
            return result;
        }
        public RPCExecManager.RPCWait C2S_Call<T>(long timeOut, ERouteTarget route = ERouteTarget.Hall, FRPCTypeReturnCallBack<T> cb = null) where T : RPCReturnValue, new()
        {
            AssertReturnType(this.GetType(), typeof(T));
            if (route == ERouteTarget.Gate)
                route = ERouteTarget.Self;
            if(cb==null)
                return WaitDoCallImpl(0, null, route, RPCExecManager.Insance.C2SConnect);
            RPCExecManager.FRPCReturnCallBack fn = (PkgReader data, bool isTimeOut) =>
            {
                if (isTimeOut)
                {
                    cb(null, true);
                    return;
                }
                T retValue = new T();
                retValue.ReadObject(data);
                cb(retValue, false);
            };
            return WaitDoCallImpl(timeOut, fn, route, RPCExecManager.Insance.C2SConnect);
        }
        
        public RPCExecManager.RPCWait S2C_Call(RPCRouter router, ERouteTarget sender=ERouteTarget.Unknown)
        {
            if (sender == ERouteTarget.Unknown)
                sender = RPCExecManager.Insance.AppTarget;

            switch (sender)
            {
                case ERouteTarget.Gate:
                    return WaitDoCallImpl(0, null, ERouteTarget.Self, router.C2GConnect);
                case ERouteTarget.Hall:
                    return WaitDoCallImpl(0, null, ERouteTarget.Client, router.ToGConnect, router);
                case ERouteTarget.Keep:
                    return WaitDoCallImpl(0, null, ERouteTarget.Client, router.ToGConnect, router);
                case ERouteTarget.Data:
                    return WaitDoCallImpl(0, null, ERouteTarget.Client, router.ToGConnect, router);
                case ERouteTarget.Reg:
                    return WaitDoCallImpl(0, null, ERouteTarget.Client, router.ToGConnect, router);
                default:
                    return null;
            }
        }

        public async Task<T> S2S_AwaitCall<T>(long timeOut, ERouteTarget target = ERouteTarget.Data) where T : RPCReturnValue, new()
        {
            AssertReturnType(this.GetType(), typeof(T));

            RPCExecManager.RPCWait writer = null;
            if (timeOut == 0)
                writer = WaitDoCallImpl(timeOut, null, ERouteTarget.Self, RPCExecManager.Insance.GetServerConnect(target));
            else
                writer = WaitDoCallImpl(timeOut, RPCAwaiter.NullCB, ERouteTarget.Self, RPCExecManager.Insance.GetServerConnect(target));

            var result = await RPCAwaiter.RPCWaitReturn<T>(writer);
            return result;
        }
        public RPCExecManager.RPCWait S2S_Call<T>(long timeOut, ERouteTarget target = ERouteTarget.Data, FRPCTypeReturnCallBack<T> cb = null) where T : RPCReturnValue, new()
        {
            AssertReturnType(this.GetType(), typeof(T));
            if (cb==null)
            {
                return WaitDoCallImpl(timeOut, null, ERouteTarget.Self, RPCExecManager.Insance.GetServerConnect(target));
            }

            RPCExecManager.FRPCReturnCallBack fn = (PkgReader data, bool isTimeOut) =>
            {
                if (isTimeOut)
                {
                    cb(null, true);
                    return;
                }
                T retValue = new T();
                retValue.ReadObject(data);
                cb(retValue, false);
            };
            return WaitDoCallImpl(timeOut, fn, ERouteTarget.Self, RPCExecManager.Insance.GetServerConnect(target));
        }

        public RPCExecManager.RPCWait WaitDoCall<T>(long timeOut, FRPCTypeReturnCallBack<T> cb, 
                    ERouteTarget route = ERouteTarget.Self, 
                    Net.NetConnection conn = null, 
                    RPCRouter router = null) where T : RPCReturnValue, new()
        {
            AssertReturnType(this.GetType(), typeof(T));
            if (cb==null)
                return WaitDoCallImpl(timeOut, null, route, conn, router);
            RPCExecManager.FRPCReturnCallBack fn = (PkgReader data, bool isTimeOut) =>
            {
                if (isTimeOut)
                {
                    cb(null, true);
                    return;
                }
                T retValue = new T();
                retValue.ReadObject(data);
                cb(retValue, false);
            };
            return WaitDoCallImpl(timeOut, fn, route, conn, router);
        }

        public async Task<T> AwaitCall<T>(long timeOut, ERouteTarget route = ERouteTarget.Self,
                    Net.NetConnection conn = null,
                    RPCRouter router = null) where T : RPCReturnValue, new()
        {
            AssertReturnType(this.GetType(), typeof(T));
            RPCExecManager.RPCWait writer = null;
            if (timeOut == 0)
                writer = WaitDoCallImpl(timeOut, null, route, conn, router);
            else
                writer = WaitDoCallImpl(timeOut, RPCAwaiter.NullCB, route, conn, router);

            var result = await RPCAwaiter.RPCWaitReturn<T>(writer);
            if(result==null)
            {
                System.Diagnostics.Debug.WriteLine($"RPC Time Out: {this.GetType().FullName}");
                return null;
            }
            return result;
        }

        public RPCExecManager.RPCWait WaitHashCall<T>(long timeOut, FRPCTypeReturnCallBack<T> cb,
                    ERouteTarget route = ERouteTarget.Self,
                    Net.NetConnection conn = null,
                    RPCRouter router = null) where T : RPCReturnValue, new()
        {
            AssertReturnType(this.GetType(), typeof(T));
            if (cb == null)
                return WaitDoCallImpl_Hash(timeOut, null, route, conn, router);
            RPCExecManager.FRPCReturnCallBack fn = (PkgReader data, bool isTimeOut) =>
            {
                if (isTimeOut)
                {
                    cb(null, true);
                    return;
                }
                T retValue = new T();
                retValue.ReadObject(data);
                cb(retValue, false);
            };
            return WaitDoCallImpl_Hash(timeOut, fn, route, conn, router);
        }
        public async Task<T> AwaitHashCall<T>(long timeOut, ERouteTarget route = ERouteTarget.Self,
                   Net.NetConnection conn = null,
                   RPCRouter router = null) where T : RPCReturnValue, new()
        {
            AssertReturnType(this.GetType(), typeof(T));
            RPCExecManager.RPCWait pkg = null;
            if (timeOut == 0)
                pkg = WaitDoCallImpl_Hash(timeOut, null, route, conn, router);
            else
                pkg = WaitDoCallImpl_Hash(timeOut, RPCAwaiter.NullCB, route, conn, router);

            var result = await RPCAwaiter.RPCWaitReturn<T>(pkg);
            if (result == null)
            {
                System.Diagnostics.Debug.WriteLine($"RPC Time Out: {this.GetType().FullName}");
                return null;
            }
            return result;
        }

        private RPCExecManager.RPCWait WaitDoCallImpl(long timeOut, RPCExecManager.FRPCReturnCallBack cb, ERouteTarget route = ERouteTarget.Self, Net.NetConnection conn = null, RPCRouter router=null)
        {
            var Index = this.GetMethodBinder().Index;
            var pkg = new PkgWriter(this.GetPkgSize());
            if (Index >= RPCExecManager.MaxRPC)
            {
                System.Diagnostics.Debug.WriteLine($"RPC Index is invalid:{this.GetType().FullName}");
                return null;
            }
            pkg.Write((byte)route);
            if (router!=null)
            {
                router.RouteInfo.Save(pkg, route);
            }

            RPCExecManager.RPCWait waiter = null;
            if (cb != null)
            {
                waiter = RPCExecManager.Insance.AddCalBack(cb);
                this.SerialId = waiter.SerialId;
                pkg.Waiter = waiter;
                if (timeOut < 0)
                    timeOut = long.MaxValue;
                waiter.Timout = timeOut;
                Index |= RPCExecManager.WaitFlag;
                pkg.SetHasReturn(true);
                pkg.Write(Index);
                pkg.Write(this.SerialId);
            }
            else
            {
                pkg.SetHasReturn(false);
                pkg.Write(Index);
            }
            WriteObject(pkg);

            if (conn != null)
                pkg.SendBuffer(conn);

            return waiter;
        }

        private RPCExecManager.RPCWait WaitDoCallImpl_Hash(long timeOut, RPCExecManager.FRPCReturnCallBack cb, ERouteTarget route = ERouteTarget.Self, Net.NetConnection conn = null, RPCRouter router = null)
        {
            var hash = this.GetMethodBinder().MethordHash;
            var pkg = new PkgWriter(this.GetPkgSize());
            pkg.SetHashIndex(true);
            pkg.Write((byte)route);
            if (router != null)
            {
                router.RouteInfo.Save(pkg, route);
            }
            RPCExecManager.RPCWait waiter = null;
            if (cb != null)
            {
                pkg.SetHasReturn(true);
                waiter = RPCExecManager.Insance.AddCalBack(cb);
                this.SerialId = waiter.SerialId;
                pkg.Waiter = waiter;
                if (timeOut < 0)
                    timeOut = long.MaxValue;
                waiter.Timout = timeOut;
                pkg.Write(hash);
                pkg.Write(this.SerialId);
            }
            else
            {
                pkg.SetHasReturn(false);
                pkg.Write(hash);
            }
            WriteObject(pkg);

            if (conn != null)
                pkg.SendBuffer(conn);

            pkg.Dispose();

            return waiter;
        }

        #endregion

        #region override
        //得到这个RPC被哪个对象作为this调用
        public virtual object GetHostObject()
        {
            return null;
        }
        public virtual string GetHostObjectName()
        {
            return "";
        }
        //返回这个用户的权限
        public virtual RPCExecuteLimitLevel GetAuthority(object host)
        {
            return (RPCExecuteLimitLevel)this.RouteInfo.Authority;
        }

        public virtual object DoExecute(RPCExec exec, object host)
        {
            try
            {
                return exec.Execute(host, this);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return null;
            }
        }
        #endregion

        public UInt16 SerialId = UInt16.MaxValue;
        public RPCRouter.RouteData RouteInfo;
        public Net.NetConnection Connect;
        public PkgReader ExtraReader;
        public RPCRouter GetRouter()
        {
            if (Connect == null)
                return null;
            return Connect.Router as RPCRouter;
        }
    }
    
    public class IRouter
    {
        public virtual object GetHostObjectImpl(RPCParameter arg)
        {
            return null;
        }
        public virtual string GetHostObjectName(RPCParameter arg)
        {
            return "";
        }
        public virtual object DoExecute(RPCParameter arg, RPCExec exec, object host)
        {
            try
            {
                return exec.Execute(host, arg);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return null;
            }
        }
    }

    #region Used Router
    public partial class DefaultRoute : IRouter
    {
        public static object HostObject;
        public override object GetHostObjectImpl(RPCParameter arg)
        {
            return HostObject;
        }
    }

    public partial class ClientRoute : IRouter
    {

    }
    
    public partial class ClientRoleRoute : IRouter
    {
        
    }

    public partial class GateRoute : IRouter
    {

    }

    public partial class HallRoute : IRouter
    {

    }

    public partial class HPlayerRoute : IRouter
    {

    }

    public partial class RegRoute : IRouter
    {

    }

    public partial class LogRoute : IRouter
    {

    }

    public partial class PathRoute : IRouter
    {

    }

    public partial class KeepRoute : IRouter
    {

    }

    public partial class DataRoute : IRouter
    {

    }
    #endregion
}
