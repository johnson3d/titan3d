using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EngineNS.Bricks.RemoteServices
{
    public abstract class RPCProcessor
    {
        private readonly static Dictionary<System.Type, RPCProcessor> Processors = new Dictionary<Type, RPCProcessor>();
        private readonly static Dictionary<System.Type, RPCProcessor> Arguments = new Dictionary<Type, RPCProcessor>();
        private readonly static Dictionary<System.Type, RPCProcessor> Returns = new Dictionary<Type, RPCProcessor>();
        //public static RPCProcessor InitProcessor(System.Type type, System.Type argType, System.Type returnType)
        //{
        //    RPCProcessor proc;
        //    if (Processors.TryGetValue(type, out proc))
        //        return proc;
        //    proc = System.Activator.CreateInstance(type) as RPCProcessor;
        //    Processors[type] = proc;
        //    Arguments[argType] = proc;
        //    if (returnType != null)
        //    {
        //        Returns[returnType] = proc;
        //    }
        //    return proc;
        //}
        public static RPCProcessor InitProcessor(System.Type type)
        {
            RPCProcessor proc;
            if (Processors.TryGetValue(type, out proc))
                return proc;
            proc = System.Activator.CreateInstance(type) as RPCProcessor;
            Processors[type] = proc;
            Arguments[proc.ArgumentType] = proc;
            if (proc.ReturnType != null)
            {
                Returns[proc.ReturnType] = proc;
            }
            return proc;
        }
        public static RPCProcessor GetProcessor(System.Type argType)
        {
            RPCProcessor proc;
            if (Processors.TryGetValue(argType, out proc))
                return proc;
            return null;
        }
        public static RPCProcessor GetProcessorByArgument(System.Type argType)
        {
            RPCProcessor proc;
            if (Arguments.TryGetValue(argType, out proc))
                return proc;
            return null;
        }
        public static RPCProcessor GetProcessorByReturn(System.Type returnType)
        {
            RPCProcessor proc;
            if (Returns.TryGetValue(returnType, out proc))
                return proc;
            return null;
        }

        #region Data
        public static NetCore.NetConnection DefaultConnection = null;
        public System.Type ArgumentType;
        public System.Type ReturnType;
        public System.Reflection.MethodInfo Method;
        public UInt16 RPCIndex = RPCExecuter.MaxRPC;
        public UInt32 MethodHash;
        public RPCCallAttribute CallAttr;
        public IRouter Rounter;
        #endregion

        public void SetMethod(System.Reflection.MethodInfo mtd, UInt16 index, System.Type argType)
        {
            ArgumentType = argType;
            RPCIndex = index;
            Method = mtd;
            var HashString = GetRPCCode(mtd);
            MethodHash = UniHash.APHash(HashString);
        }
        
        public virtual object Execute(object host, ref NetCore.PkgReader pkg, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            var parameter = CreateArgument();

            IO.Serializer.SerializerHelper.ReadObject(parameter, pkg);

            var args = new object[] { pkg.GetUserFlags(), parameter, serialId, connect, routeInfo };
            return Method.Invoke(host, args);
        }

        #region Send Pkg
        public delegate void FRPCTypeReturnCallBack<T>(T retValue, bool isTimeOut) where T : IReturnValue;
        public virtual object ReadReturn(ref NetCore.PkgReader pkg)
        {
            return null;
        }
        public virtual unsafe void UnsafeReadReturn(ref NetCore.PkgReader pkg, void* arg)
        {
            
        }

        private static bool CheckReturnType(Type paramType, Type retType)
        {
            if (paramType.BaseType.IsGenericType == false)
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
            return;
        }
        //protected async Task<R> C2S_AwaitCall<R,T>(T arg, long timeOut, ERouteTarget route = ERouteTarget.Hall) 
        //    where R : struct, IReturnValue
        //    where T : struct, IArgument
        //{
        //    AssertReturnType(this.GetType(), typeof(R));
        //    if (route == ERouteTarget.Gate)
        //        route = ERouteTarget.Self;

        //    RPCExecuter.RPCWait waiter = null;
        //    if (timeOut == 0)
        //        waiter = WaitDoCallImpl<T>(ref arg, 0, null, route, RPCExecuter.Instance.C2SConnect);
        //    else
        //        waiter = WaitDoCallImpl<T>(ref arg, timeOut, RPCAwaiter.NullCB, route, RPCExecuter.Instance.C2SConnect);

        //    var result = await RPCAwaiter.RPCWaitReturn<R>(waiter);
        //    return result;
        //}
        //protected RPCExecuter.RPCWait C2S_Call<R, T>(ref T arg, long timeOut, ERouteTarget route = ERouteTarget.Hall, FRPCTypeReturnCallBack<R> cb = null) 
        //    where R : struct, IReturnValue
        //    where T : struct, IArgument
        //{
        //    AssertReturnType(this.GetType(), typeof(R));
        //    if (route == ERouteTarget.Gate)
        //        route = ERouteTarget.Self;
        //    if (cb == null)
        //        return WaitDoCallImpl<T>(ref arg, 0, null, route, RPCExecuter.Instance.C2SConnect);
        //    RPCExecuter.FRPCReturnCallBack fn = (PkgReader data, bool isTimeOut) =>
        //    {
        //        if (isTimeOut)
        //        {
        //            cb(default(R), true);
        //            return;
        //        }
        //        R retValue = new R();
        //        IO.Serializer.SerializerHelper.ReadObject(retValue, data);
        //        cb(retValue, false);
        //    };
        //    return WaitDoCallImpl<T>(ref arg, timeOut, fn, route, RPCExecuter.Instance.C2SConnect);
        //}

        //protected RPCExecuter.RPCWait S2C_Call<T>(ref T arg, RPCRouter router, ERouteTarget sender = ERouteTarget.Unknown)
        //    where T : struct, IArgument
        //{
        //    if (sender == ERouteTarget.Unknown)
        //    {
        //        //sender = RPCExecuter.Instance.AppTarget;
        //        sender = ERouteTarget.Self;
        //    }

        //    switch (sender)
        //    {
        //        case ERouteTarget.Gate:
        //            return WaitDoCallImpl<T>(ref arg, 0, null, ERouteTarget.Self, router.C2GConnect);
        //        case ERouteTarget.Hall:
        //            return WaitDoCallImpl<T>(ref arg, 0, null, ERouteTarget.Client, router.ToGConnect, router);
        //        case ERouteTarget.Keep:
        //            return WaitDoCallImpl<T>(ref arg, 0, null, ERouteTarget.Client, router.ToGConnect, router);
        //        case ERouteTarget.Data:
        //            return WaitDoCallImpl<T>(ref arg, 0, null, ERouteTarget.Client, router.ToGConnect, router);
        //        case ERouteTarget.Reg:
        //            return WaitDoCallImpl<T>(ref arg, 0, null, ERouteTarget.Client, router.ToGConnect, router);
        //        default:
        //            return null;
        //    }
        //}

        //protected async Task<R> S2S_AwaitCall<R, T>(T arg, long timeOut, ERouteTarget target = ERouteTarget.Data)
        //    where R : struct, IReturnValue
        //    where T : struct, IArgument
        //{
        //    AssertReturnType(this.GetType(), typeof(R));

        //    RPCExecuter.RPCWait writer = null;
        //    if (timeOut == 0)
        //        writer = WaitDoCallImpl<T>(ref arg, timeOut, null, ERouteTarget.Self, RPCExecuter.Instance.GetServerConnect(target));
        //    else
        //        writer = WaitDoCallImpl<T>(ref arg, timeOut, RPCAwaiter.NullCB, ERouteTarget.Self, RPCExecuter.Instance.GetServerConnect(target));

        //    var result = await RPCAwaiter.RPCWaitReturn<R>(writer);
        //    return result;
        //}

        //protected RPCExecuter.RPCWait S2S_Call<R,T>(ref T arg, long timeOut, ERouteTarget target = ERouteTarget.Data, FRPCTypeReturnCallBack<R> cb = null)
        //    where R : struct, IReturnValue
        //    where T : struct, IArgument
        //{
        //    AssertReturnType(this.GetType(), typeof(R));
        //    if (cb == null)
        //    {
        //        return WaitDoCallImpl<T>(ref arg, timeOut, null, ERouteTarget.Self, RPCExecuter.Instance.GetServerConnect(target));
        //    }

        //    RPCExecuter.FRPCReturnCallBack fn = (PkgReader data, bool isTimeOut) =>
        //    {
        //        if (isTimeOut)
        //        {
        //            cb(default(R), true);
        //            return;
        //        }
        //        R retValue = new R();
        //        IO.Serializer.SerializerHelper.ReadObject(retValue, data);
        //        cb(retValue, false);
        //    };
        //    return WaitDoCallImpl<T>(ref arg, timeOut, fn, ERouteTarget.Self, RPCExecuter.Instance.GetServerConnect(target));
        //}
        #endregion

        #region Pkg Implement
        protected RPCExecuter.RPCWait WritePkgHeader<T>(ref NetCore.PkgWriter pkg, ref T arg, long timeOut, RPCExecuter.FRPCReturnCallBack cb,
            NetCore.ERouteTarget route, NetCore.NetConnection conn, NetCore.RPCRouter router) where T : IArgument
        {
            var Index = RPCIndex;
            
            if (Index >= RPCExecuter.MaxRPC)
            {
                System.Diagnostics.Debug.WriteLine($"RPC Index is invalid:{this.GetType().FullName}");
                return null;
            }
            pkg.Write((byte)route);
            if (router != null)
            {
                router.RouteInfo.Save(ref pkg, route);
            }

            RPCExecuter.RPCWait waiter = null;
            if (cb != null)
            {
                waiter = RPCExecuter.Instance.AddCalBack(cb);
                pkg.Waiter = waiter;
                if (timeOut < 0)
                    timeOut = long.MaxValue;
                waiter.Timout = timeOut;
                Index |= RPCExecuter.WaitFlag;
                pkg.SetHasReturn(true);
                pkg.Write(Index);
                pkg.Write(waiter.SerialId);
            }
            else
            {
                pkg.SetHasReturn(false);
                pkg.Write(Index);
            }
            return waiter;
        }
        protected RPCExecuter.RPCWait WritePkgHeader_Hash<T>(ref NetCore.PkgWriter pkg, ref T arg, long timeOut, RPCExecuter.FRPCReturnCallBack cb,
            NetCore.ERouteTarget route, NetCore.NetConnection conn, NetCore.RPCRouter router)
            where T : IArgument
        {
            var hash = MethodHash;
            pkg.SetHashIndex(true);
            pkg.Write((byte)route);
            if (router != null)
            {
                router.RouteInfo.Save(ref pkg, route);
            }
            RPCExecuter.RPCWait waiter = null;
            if (cb != null)
            {
                pkg.SetHasReturn(true);
                waiter = RPCExecuter.Instance.AddCalBack(cb);
                pkg.Waiter = waiter;
                if (timeOut < 0)
                    timeOut = long.MaxValue;
                waiter.Timout = timeOut;
                pkg.Write(hash);
                pkg.Write(waiter.SerialId);
            }
            else
            {
                pkg.SetHasReturn(false);
                pkg.Write(hash);
            }
            return waiter;
        }
        #endregion

        #region override
        //得到这个RPC被哪个对象作为this调用
        public virtual object GetHostObject(ref NetCore.RPCRouter.RouteData routeInfo)
        {
            if (Rounter != null)
                return Rounter.GetHostObjectImpl(ref routeInfo);
            else
                return DefaultRoute.HostObject;
        }
        public virtual IArgument CreateArgument()
        {
            return System.Activator.CreateInstance(ArgumentType) as IArgument;
        }
        //返回这个用户的权限
        public virtual NetCore.RPCExecuteLimitLevel GetAuthority(object host)
        {
            if (CallAttr == null)
                return NetCore.RPCExecuteLimitLevel.Player;
            return (NetCore.RPCExecuteLimitLevel)CallAttr.LimitLevel;
        }
        #endregion

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
            foreach (var i in desc.Members)
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
    }
}
