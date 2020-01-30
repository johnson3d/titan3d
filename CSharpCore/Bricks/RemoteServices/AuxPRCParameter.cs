using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RemoteServices
{
    public class NullReturn : RPCReturnValue
    {
        public override int GetPkgSize()
        {
            return 4;
        }
    }

    internal class ParamVirtualImpl<K, V> where V : IRouter, new()
    {
        V Router = new V();
        RPCExec MethodBinder = new RPCExec();
        public RPCExec GetMethodBinder()
        {
            return MethodBinder;
        }
        public void SystemSetMethodBinder(RPCExec exec)
        {
            MethodBinder = exec;
        }
        public object GetHostObject(RPCParameter arg)
        {
            var result = Router.GetHostObjectImpl(arg);
            if (result != null)
                return result;
            return null;
        }
        public string GetHostObjectName(RPCParameter arg)
        {
            return Router.GetHostObjectName(arg);
        }
        public object DoExecute(RPCParameter arg, RPCExec exec, object host)
        {
            return Router.DoExecute(arg, exec, host);
        }
    }

    internal class ReturnImpl<R> where R : RPCReturnValue, new()
    {
        public static void DoReturn(RPCParameter arg, R retValue, RPCRouter router)
        {
            var pkg = new PkgWriter(arg.GetPkgSize());
            byte route = (byte)ERouteTarget.Client | (byte)ERouteTarget.ReturnFlag;
            pkg.Write((byte)route);
            router.RouteInfo.Save(pkg, ERouteTarget.Self);
            pkg.Write(arg.SerialId);
            retValue.WriteObject(pkg);
            pkg.SendBuffer(arg.Connect);
            pkg.Dispose();
        }
        public static void DoReturnClient(RPCParameter arg, R retValue)
        {
            var pkg = new PkgWriter(arg.GetPkgSize());
            byte route = (byte)ERouteTarget.Client | (byte)ERouteTarget.ReturnFlag;
            pkg.Write((byte)route);
            arg.RouteInfo.Save(pkg, ERouteTarget.Self);
            pkg.Write(arg.SerialId);
            retValue.WriteObject(pkg);
            pkg.SendBuffer(arg.Connect);
            pkg.Dispose();
        }
        public static void DoReturn(RPCParameter arg, R retValue)
        {
            var pkg = new PkgWriter(arg.GetPkgSize());
            byte route = (byte)ERouteTarget.Self | (byte)ERouteTarget.ReturnFlag;
            pkg.Write((byte)route);
            pkg.Write(arg.SerialId);
            retValue.WriteObject(pkg);
            pkg.SendBuffer(arg.Connect);
            pkg.Dispose();
        }
    }

    //泛型参数K，虽然里面没有做任何用处，但是他可以让每一个RPCAuxParameter<K,V>变成一个新的类型，
    //从而smIndex是为每个类型都生成了代码
    public abstract class RPCAuxParameter<K, V> : RPCParameter where V : IRouter, new()
    {
        static ParamVirtualImpl<K, V> ParamImpl = new ParamVirtualImpl<K, V>();
        public override RPCExec GetMethodBinder()
        {
            return ParamImpl.GetMethodBinder();
        }
        public override void SystemSetMethodBinder(RPCExec exec)
        {
            ParamImpl.SystemSetMethodBinder(exec);
        }
        public override object GetHostObject()
        {
            return ParamImpl.GetHostObject(this);
        }
        public override string GetHostObjectName()
        {
            return ParamImpl.GetHostObjectName(this);
        }
        public override object DoExecute(RPCExec exec, object host)
        {
            return ParamImpl.DoExecute(this, exec, host);
        }
    }

    public abstract class RPCAuxParameterReturn<K, V, R> : RPCAuxParameter<K, V> where V : IRouter, new() where R : RPCReturnValue, new()
    {
        public R ReturnValue = new R();
        
        public void DoReturn(RPCRouter router)
        {
            ReturnImpl<R>.DoReturn(this, ReturnValue);
        }
        public void DoReturnClient()
        {
            ReturnImpl<R>.DoReturnClient(this, ReturnValue);
        }
        public void DoReturn()
        {
            ReturnImpl<R>.DoReturn(this, ReturnValue);
        }

        public async System.Threading.Tasks.Task<R> C2S_AwaitCallReturn(long timeOut, ERouteTarget route = ERouteTarget.Hall)
        {
            return await C2S_AwaitCall<R>(timeOut, route);
        }
        public async System.Threading.Tasks.Task<R> S2S_AwaitCallReturn(long timeOut, ERouteTarget target = ERouteTarget.Data)
        {
            return await S2S_AwaitCall<R>(timeOut, target);
        }
        public async System.Threading.Tasks.Task<R> AwaitCallReturn(long timeOut, ERouteTarget route = ERouteTarget.Self,
                    Net.NetConnection conn = null,
                    RPCRouter router = null)
        {
            return await AwaitCall<R>(timeOut, route, conn, router);
        }
        public async System.Threading.Tasks.Task<R> AwaitHashCallReturn(long timeOut, ERouteTarget route = ERouteTarget.Self,
                   Net.NetConnection conn = null,
                   RPCRouter router = null)
        {
            return await AwaitHashCall<R>(timeOut, route, conn, router);
        }
    }

    #region S2C SingleId RPC
    public abstract class RPCSingleIdParameter : RPCParameter
    {
        public UInt32 SingleId
        {
            get;
            set;
        }
    }
    
    public abstract class RPCAuxParameterSingleId<K, V> : RPCSingleIdParameter where V : IRouter, new()
    {
        static ParamVirtualImpl<K, V> ParamImpl = new ParamVirtualImpl<K, V>();
        public override RPCExec GetMethodBinder()
        {
            return ParamImpl.GetMethodBinder();
        }
        public override void SystemSetMethodBinder(RPCExec exec)
        {
            ParamImpl.SystemSetMethodBinder(exec);
        }
        public override object GetHostObject()
        {
            return ParamImpl.GetHostObject(this);
        }
        public override string GetHostObjectName()
        {
            return ParamImpl.GetHostObjectName(this);
        }
        public override object DoExecute(RPCExec exec, object host)
        {
            return ParamImpl.DoExecute(this, exec, host);
        }
    }

    public abstract class RPCAuxParameterSingleIdReturn<K, V, R> : RPCAuxParameterSingleId<K, V> where V : IRouter, new() where R : RPCReturnValue, new()
    {
        public R ReturnValue = new R();

        public void DoReturn(RPCRouter router)
        {
            ReturnImpl<R>.DoReturn(this, ReturnValue);
        }
        public void DoReturnClient()
        {
            ReturnImpl<R>.DoReturnClient(this, ReturnValue);
        }
        public void DoReturn()
        {
            ReturnImpl<R>.DoReturn(this, ReturnValue);
        }

        public async System.Threading.Tasks.Task<R> C2S_AwaitCallReturn(long timeOut, ERouteTarget route = ERouteTarget.Hall)
        {
            return await C2S_AwaitCall<R>(timeOut, route);
        }
        public async System.Threading.Tasks.Task<R> S2S_AwaitCallReturn(long timeOut, ERouteTarget target = ERouteTarget.Data)
        {
            return await S2S_AwaitCall<R>(timeOut, target);
        }
        public async System.Threading.Tasks.Task<R> AwaitCallReturn(long timeOut, ERouteTarget route = ERouteTarget.Self,
                    Net.NetConnection conn = null,
                    RPCRouter router = null)
        {
            return await AwaitCall<R>(timeOut, route, conn, router);
        }
        public async System.Threading.Tasks.Task<R> AwaitHashCallReturn(long timeOut, ERouteTarget route = ERouteTarget.Self,
                   Net.NetConnection conn = null,
                   RPCRouter router = null)
        {
            return await AwaitHashCall<R>(timeOut, route, conn, router);
        }
    }
    #endregion
}
