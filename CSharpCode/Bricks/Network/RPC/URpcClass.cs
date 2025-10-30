using EngineNS.Support;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.Bricks.Network.RPC
{
    public enum ERunTarget : sbyte
    {
        Return = -1,
        None = 0,
        Client,
        Root,
        Login,
        Data,
        Log,
        Gate,
        Level,
    }
    public enum EExecuter : byte
    {
        Root,
        Client,
        Profiler,
        PropertyData,
    }
    public class TtRpcClassAttribute : Attribute
    {
        public ERunTarget RunTarget;
        public EExecuter Executer;
        public bool CallerInClass = false;
    }
    public enum EAuthority : byte
    {
        Client = 0,
        Gateway,
        Server,
        God = byte.MaxValue,
    }
    public class TtRpcBroadCaster
    {
        public virtual IEnumerator GetEnumerator(IRpcHost sender, System.Type type)
        {
            FRouter router = new FRouter();
            router.Executer = sender.GetRpcClass().Executer;
            TtEngine.Instance.RpcModule.RpcManager.GetExecuter(in router);
            return null;
        }
        public static TtRpcBroadCaster Instance { get; } = new TtRpcBroadCaster();
    }
    public class TtRpcMethodAttribute : Attribute
    {
        public UInt16 Index;
        public EPkgTypes PkgFlags;
        public EAuthority Authority = EAuthority.Client;
        public bool IsBroadCaster = false;
    }
    public class TtRpcPropertyAttribute : Attribute
    {
        public UInt16 Index;
    }
    public class TtRpcClass
    {
        public ERunTarget RunTarget;
        public EExecuter Executer;
        public struct FRpcMethodInfo
        {
            public string Name;
            public FCallMethod Method;
            public TtRpcMethodAttribute Attribute;
            public bool IsBroadCaster;
        }
        public FRpcMethodInfo[] Methods = new FRpcMethodInfo[UInt16.MaxValue];

        public struct FRpcPropertyInfo
        {
            public string Name;
            public TtRpcPropertyAttribute Attribute;
        }
        public FRpcPropertyInfo[] Properties = null;// new FRpcPropertyInfo[UInt16.MaxValue];

        public TtRpcClass(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(TtRpcClassAttribute), false);
            if (attrs.Length == 0)
                throw new TtException("");

            var kls = attrs[0] as TtRpcClassAttribute;
            RunTarget = kls.RunTarget;
            Executer = kls.Executer;

            var methods = type.GetMethods();
            foreach (var i in methods)
            {
                attrs = i.GetCustomAttributes(typeof(TtRpcMethodAttribute), true);
                if (attrs.Length == 0)
                    continue;

                if (CheckDefine(i) == false)
                {
                    throw new TtException("");
                }

                var dlgt = GetFieldInherit(type, $"rpc_{i.Name}", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (dlgt == null)
                    throw new TtException("");

                var fun = dlgt.GetValue(null) as FCallMethod;
                if (fun == null)
                    throw new TtException("");

                var mtd = attrs[0] as TtRpcMethodAttribute;
                if (Methods[mtd.Index].Method != null)
                    throw new TtException("");
                Methods[mtd.Index].Name = i.Name;
                Methods[mtd.Index].Method = fun;
                Methods[mtd.Index].Attribute = mtd;
                Methods[mtd.Index].IsBroadCaster = mtd.IsBroadCaster;
            }

            var props = type.GetProperties();
            int MaxPropIndex = -1;
            foreach (var i in props)
            {
                attrs = i.GetCustomAttributes(typeof(TtRpcPropertyAttribute), false);
                if (attrs.Length == 0)
                    continue;

                var mtd = attrs[0] as TtRpcPropertyAttribute;
                if(mtd.Index > MaxPropIndex)
                    MaxPropIndex = mtd.Index;
            }
            Properties = new FRpcPropertyInfo[MaxPropIndex + 1];
            foreach (var i in props)
            {
                attrs = i.GetCustomAttributes(typeof(TtRpcPropertyAttribute), false);
                if (attrs.Length == 0)
                    continue;

                var mtd = attrs[0] as TtRpcPropertyAttribute;
                if (Properties[mtd.Index].Attribute != null)
                    throw new TtException("");
                Properties[mtd.Index].Name = i.Name;
                Properties[mtd.Index].Attribute = mtd;
            }
        }
        static System.Reflection.FieldInfo GetFieldInherit(System.Type type, string name, System.Reflection.BindingFlags flags)
        {
            var dlgt = type.GetField(name, flags);
            if (dlgt != null)
            {
                return dlgt;
            }
            if (type.BaseType == null)
                return null;
            return GetFieldInherit(type.BaseType, name, flags);
        }
        private bool CheckDefine(System.Reflection.MethodInfo mtd)
        {
            System.Type realRetType = mtd.ReturnType;
            if (mtd.ReturnType.IsGenericType && (mtd.ReturnType.FullName.StartsWith("System.Threading.Tasks.Task") ||
                mtd.ReturnType.FullName.StartsWith("EngineNS.Thread.Async.TtTask")))
            {
                realRetType = mtd.ReturnType.GetGenericArguments()[0];
            }
            if (realRetType != typeof(void) &&
                realRetType != typeof(string) &&
                realRetType != typeof(RName) &&
                realRetType.IsValueType == false &&
                realRetType.Name != "ISerializer" &&
                realRetType.GetInterface("ISerializer") == null)
            {
                Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Error, $"{mtd.Name} return type is invalid");
                return false;
            }
                
            var parms = mtd.GetParameters();
            //if (parms.Length != 2)
            //    return false;
            
            for (int i = 0; i < parms.Length - 1; i++)
            {
                if (parms[i].ParameterType != typeof(string) &&
                    parms[i].ParameterType != typeof(RName) &&
                    parms[i].ParameterType != typeof(Rtti.TtTypeDesc) &&
                    parms[i].ParameterType.IsValueType == false &&
                    parms[i].ParameterType.Name != "ISerializer" &&
                    parms[i].ParameterType.GetInterface("ISerializer") == null)
                {
                    Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Error, $"{mtd.Name} parameter{i} type is invalid");
                    return false;
                }
                    
            }
            if (parms[parms.Length - 1].ParameterType != typeof(TtCallContext))
                return false;
            return true;
        }
        public FRpcMethodInfo GetCallee(UInt16 index)
        {
            return Methods[index];
        }
    }
    public interface IRpcHost
    {
        TtRpcClass GetRpcClass();
        ushort RpcExecuteIndex { get; set; }
        bool IgnoreUpdateProperties(ushort RpcExecuteIndex);//exclude some special case
        INetConnect GetRpcConnect(UInt16 methodIndex);
        TtRpcBroadCaster GetRpcBroadCaster();
        void OnRpcPropertyChanged(string propName, object v, object old);
    }
    public class AuxRpcHost<T> : IRpcHost
    {
        static TtRpcClass smRpcClass = new TtRpcClass(typeof(T));
        public virtual TtRpcClass GetRpcClass()
        {
            return smRpcClass;
        }
        public virtual ushort RpcExecuteIndex { get; set; } = 0;
        public virtual INetConnect GetRpcConnect(UInt16 methodIndex)
        {
            return TtEngine.Instance.RpcModule.DefaultNetConnect;
        }
        public virtual bool IgnoreUpdateProperties(ushort RpcExecuteIndex)
        {
            return false;
        }
        public virtual void OnRpcPropertyChanged(string propName, object v, object old)
        {
        }
        public virtual TtRpcBroadCaster GetRpcBroadCaster()
        {
            return TtRpcBroadCaster.Instance;
        }
    }
    public class TtRpcPropertyData
    {
        public TtRpcClass HostClass;
        public object[] PropertyValues;
        public TtRpcPropertyData(TtRpcClass kls)
        {
            HostClass = kls;
            PropertyValues = new object[kls.Properties.Length];
        }
        public void CollectProperties(IRpcHost host, TtBitset modifyProps, List<object> propValues, bool saveState = true)
        {
            if (host.GetRpcClass()!=HostClass)
                return;

            modifyProps.SetBitCount((uint)HostClass.Properties.Length);
            var type = host.GetType();
            for (int i = 0; i < HostClass.Properties.Length; i++)
            {
                if (HostClass.Properties[i].Attribute == null)
                    continue;
                var prop = type.GetProperty(HostClass.Properties[i].Name);
                if (prop == null)
                    throw new TtException("");
                var val = prop.GetValue(host);
                if (IsSameValue(PropertyValues[i], val)==false)
                {
                    modifyProps.SetBit((uint)i);
                    propValues.Add(val);
                    if (saveState)
                        PropertyValues[i] = val;
                }
            }
        }
        public static bool IsSameValue(object v1, object v2)
        {
            if (v1 == null && v2 == null)
                return true;
            if (v1 == null || v2 == null)
                return false;
            return v1.Equals(v2);
        }
    }
    [TtRpcClassAttribute(RunTarget = ERunTarget.None, Executer = EExecuter.PropertyData, CallerInClass = true)]
    public partial class TtRpcPropertyDataManager : AuxRpcHost<TtRpcPropertyDataManager>
    {
        #region Interface
        public override INetConnect GetRpcConnect(UInt16 methodIndex)
        {
            return TtEngine.Instance.RpcModule.DefaultNetConnect;
        }
        #endregion

        [TtRpcProperty]
        public int TestSync1 { get; set; } = 1;
        public TtRpcPropertyDataManager()
        {
            RegisterHost(this);
        }
        public struct TtPropKey
        {
            public TtPropKey(IRpcHost host)
            {
                ExecuterType = host.GetRpcClass().Executer;
                ExecuteIndex = host.RpcExecuteIndex;
            }
            public TtPropKey(EExecuter type, ushort index)
            {
                ExecuterType = type;
                ExecuteIndex = index;
            }
            public EExecuter ExecuterType;
            public ushort ExecuteIndex;
            public override int GetHashCode()
            {
                return (int)(ExecuteIndex + (byte)ExecuterType);
            }
        }
        public class TtPropValue
        {
            public TtRpcPropertyData PropertyData;
            public WeakReference<IRpcHost> Host;
        }
        class TtPropKeyComparer : IEqualityComparer<TtPropKey>
        {
            public bool Equals(TtPropKey x, TtPropKey y)
            {
                return x.ExecuterType == y.ExecuterType && x.ExecuteIndex == y.ExecuteIndex;
            }

            public int GetHashCode(TtPropKey obj)
            {
                return obj.GetHashCode();
            }
        }
        public Dictionary<TtPropKey, TtPropValue> PropertyDatas = new(new TtPropKeyComparer());
        public void RegisterHost(IRpcHost host)
        {
            var kls = host.GetRpcClass();
            var propData = new TtRpcPropertyData(kls);
            var v = new TtPropValue();
            v.PropertyData = propData;
            v.Host = new WeakReference<IRpcHost>(host);
            PropertyDatas.Add(new TtPropKey(host), v);
        }
        public IRpcHost FindHost(EExecuter executerType, ushort executeIndex)
        {
            if (PropertyDatas.TryGetValue(new TtPropKey(executerType, executeIndex), out var result))
            {
                if (result.Host.TryGetTarget(out var t))
                {
                    return t;
                }
                PropertyDatas.Remove(new TtPropKey(executerType, executeIndex));
            }
            return null;
        }
        public IRpcHost UnregisterHost(IRpcHost host)
        {
            if (PropertyDatas.TryGetValue(new TtPropKey(host), out var result))
            {
                if (result.Host.TryGetTarget(out var t))
                {
                    return t;
                }
                PropertyDatas.Remove(new TtPropKey(host));
            }
            return null;
        }
        public IRpcHost UnregisterHost(EExecuter executerType, ushort executeIndex)
        {
            if (PropertyDatas.TryGetValue(new TtPropKey(executerType, executeIndex), out var result))
            {
                if (result.Host.TryGetTarget(out var t))
                {
                    return t;
                }
                PropertyDatas.Remove(new TtPropKey(executerType, executeIndex));
            }
            return null;
        }
        private void CollectProperties(IO.IWriter writer)
        {
            List<object> propValues = new();
            TtBitset modifyProps = new();
            List<TtPropKey> rmvKeys = new();
            foreach (var i in PropertyDatas)
            {
                IRpcHost host;
                if (false == i.Value.Host.TryGetTarget(out host))
                {
                    rmvKeys.Add(i.Key);
                    continue;
                }

                propValues.Clear();
                i.Value.PropertyData.CollectProperties(host, modifyProps, propValues, true);
                if (modifyProps.IsAnySet()==false)
                    continue;

                sbyte executer = (sbyte)host.GetRpcClass().Executer;
                writer.Write(executer);
                writer.Write(host.RpcExecuteIndex);
                writer.Write(modifyProps);
                var type = host.GetType();
                //write val
                int idx = 0;
                for (uint j = 0; j < modifyProps.BitCount; j++)
                {
                    if (modifyProps.IsSet(j))
                    {
                        var propInfo = host.GetRpcClass().Properties[j];
                        var prop = type.GetProperty(propInfo.Name);
                        if (prop == null)
                            throw new TtException("");
                        writer.WriteWithType(prop.PropertyType, propValues[idx++]);
                    }
                }
            }
            writer.Write(byte.MaxValue); //结束标志

            foreach(var i in rmvKeys)
            {
                PropertyDatas.Remove(i);
            }
        }
        public void Tick()
        {
            using (var writer = IO.TtMemWriter.CreateInstance())
            {
                using (var ar = new IO.AuxWriter<IO.TtMemWriter>(writer))
                {
                    CollectProperties(ar);
                    RPC_SyncAllProperties(writer);
                }
            }
            //InitProperties(EExecuter.PropertyData, 0).AddWaitTask();
        }
        private void UpdateProperties(IO.IReader reader)
        {
            while (true)
            {
                byte executer = 0;
                reader.Read(out executer);
                if (executer == byte.MaxValue)
                    break;
                UInt16 rpcIndex = 0;
                reader.Read(out rpcIndex);
                TtBitset modifyProps = new();
                reader.Read(ref modifyProps);
                bool bSet = true;
                FRouter router = new FRouter()
                {
                    Executer = (EExecuter)executer,
                    Index = rpcIndex,
                };
                var host = TtEngine.Instance.RpcModule.RpcManager.GetExecuter(in router);
                if (host == null || host.RpcExecuteIndex != rpcIndex || host.IgnoreUpdateProperties(rpcIndex))
                {
                    bSet = false;
                }
                var type = host.GetType();
                for (uint i = 0; i < modifyProps.BitCount; i++)
                {
                    if (modifyProps.IsSet(i))
                    {
                        var propInfo = host.GetRpcClass().Properties[i];
                        var prop = type.GetProperty(propInfo.Name);
                        if (prop == null)
                            throw new TtException("");
                        object v = null;
                        //read val
                        v = reader.ReadWithType(prop.PropertyType);
                        if (bSet)
                        {
                            var old = prop.GetValue(host);
                            if (TtRpcPropertyData.IsSameValue(old, v))
                                continue;
                            prop.SetValue(host, v);
                            host.OnRpcPropertyChanged(propInfo.Name, v, old);
                        }
                    }
                }
            }
        }
        [TtRpcMethod(Index = 0)]
        public void SyncAllProperties(EngineNS.IO.TtMemWriter data, TtCallContext context)
        {
            using (var reader = IO.TtMemReader.CreateInstance(in data))
            {
                using (var ar = new IO.AuxReader<IO.TtMemReader>(reader, this))
                {
                    UpdateProperties(ar);
                }
            }
        }
        // Callback when response RPC_CreateRpcHost
        public delegate void FOnCreateRpcHost(IRpcHost host, string info);
        public FOnCreateRpcHost OnCreateRpcHost;
        [TtRpcMethod(Index = 1, IsBroadCaster = true)]
        public void CreateRpcHost(Rtti.TtTypeDesc type, ushort executeIndex, string info, TtCallContext context)
        {
            var rpcClass = type.GetCustomAttribute(typeof(TtRpcClassAttribute), false) as TtRpcClassAttribute;
            if (rpcClass!=null)
                return;
            
            var result = FindHost(rpcClass.Executer, executeIndex);
            if (result!=null)
            {
                if (result.GetType()==type.SystemType)
                    return;
                else
                    UnregisterHost(rpcClass.Executer, executeIndex);
            }
            result = Rtti.TtTypeDescManager.CreateInstance(type) as IRpcHost;
            if (result==null)
                return;
            result.RpcExecuteIndex = executeIndex;
            if (OnCreateRpcHost!=null)
            {
                OnCreateRpcHost(result, info);
            }
            context.NoBroadCast = false;
        }
        public delegate void FOnRemoveRpcHost(IRpcHost host, string info);
        public FOnRemoveRpcHost OnRemoveRpcHost;
        [TtRpcMethod(Index = 2)]
        public void RemoveRpcHost(EExecuter executerType, ushort executeIndex, string info, TtCallContext context)
        {
            var host = UnregisterHost(executerType, executeIndex);
            if (OnCreateRpcHost!=null)
            {
                OnRemoveRpcHost(host, info);
            }
        }
        [TtRpcMethod(Index = 3)]
        public EngineNS.IO.TtMemWriter QueryProperties(EExecuter executerType, ushort executeIndex, TtCallContext context)
        {
            var host = FindHost(executerType, executeIndex);
            var result = IO.TtMemWriter.CreateInstance();
            using (var ar = new IO.AuxWriter<IO.TtMemWriter>(result))
            {
                if (host==null)
                {
                    ar.Write((short)0);
                    return result;
                }
                else
                {
                    var HostClass = host.GetRpcClass();
                    ar.Write((short)HostClass.Properties.Length);
                    var type = host.GetType();
                    for (int i = 0; i < HostClass.Properties.Length; i++)
                    {
                        if (HostClass.Properties[i].Attribute == null)
                            continue;
                        var prop = type.GetProperty(HostClass.Properties[i].Name);
                        if (prop == null)
                            throw new TtException("");
                        var val = prop.GetValue(host);
                        ar.WriteWithType(prop.PropertyType, val);
                    }
                    return result;
                }
            }
        }
        public async Thread.Async.TtTask InitProperties(EExecuter executerType, ushort executeIndex)
        {
            var host = FindHost(executerType, executeIndex);
            if (host == null)
                return;
            using (var writer = await this.RPC_QueryProperties(executerType, executeIndex))
            {
                using (var reader = IO.TtMemReader.CreateInstance(in writer))
                {
                    using (var ar = new IO.AuxReader<IO.TtMemReader>(reader, this))
                    {
                        short propCount = 0;
                        ar.Read(out propCount);

                        var type = host.GetType();
                        var props = host.GetRpcClass().Properties;
                        for (int i = 0; i < propCount; i++)
                        {
                            var propInfo = props[i];
                            var prop = type.GetProperty(propInfo.Name);
                            if (prop == null)
                                throw new TtException("");
                            object v = null;
                            //read val
                            v = ar.ReadWithType(prop.PropertyType);
                            prop.SetValue(host, v);
                        }
                    }
                }   
            }
        }
    }
}
#if TitanEngine_AutoGen_RPC
#region TitanEngine_AutoGen_RPC
#pragma warning disable 105


namespace EngineNS.Bricks.Network.RPC
{
	public partial class TtRpcPropertyDataManager_RpcCaller
	{
		public static void SyncAllProperties(EngineNS.IO.TtMemWriter data, in EngineNS.Bricks.Network.RPC.FRpcCallArg rpcArg)
		{
			var ExeIndex = rpcArg.ExeIndex;
			var NetConnect = rpcArg.NetConnect;
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = TtEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = TtEngine.Instance.RpcModule.DefaultNetConnect;
			}
			using (var writer = EngineNS.IO.TtMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<EngineNS.IO.TtMemWriter>(writer);
				FRouter router = new FRouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.PropertyData;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router);
				UInt16 methodIndex = 0;
				pkg.Write(methodIndex);
				pkg.Write(data);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
		}
		public static void CreateRpcHost(Rtti.TtTypeDesc type, ushort executeIndex, string info, in EngineNS.Bricks.Network.RPC.FRpcCallArg rpcArg)
		{
			var ExeIndex = rpcArg.ExeIndex;
			var NetConnect = rpcArg.NetConnect;
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = TtEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = TtEngine.Instance.RpcModule.DefaultNetConnect;
			}
			using (var writer = EngineNS.IO.TtMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<EngineNS.IO.TtMemWriter>(writer);
				FRouter router = new FRouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.PropertyData;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router);
				UInt16 methodIndex = 1;
				pkg.Write(methodIndex);
				pkg.Write(type);
				pkg.Write(executeIndex);
				pkg.Write(info);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
		}
		public static void RemoveRpcHost(EExecuter executerType, ushort executeIndex, string info, in EngineNS.Bricks.Network.RPC.FRpcCallArg rpcArg)
		{
			var ExeIndex = rpcArg.ExeIndex;
			var NetConnect = rpcArg.NetConnect;
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = TtEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = TtEngine.Instance.RpcModule.DefaultNetConnect;
			}
			using (var writer = EngineNS.IO.TtMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<EngineNS.IO.TtMemWriter>(writer);
				FRouter router = new FRouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.PropertyData;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router);
				UInt16 methodIndex = 2;
				pkg.Write(methodIndex);
				pkg.Write(executerType);
				pkg.Write(executeIndex);
				pkg.Write(info);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
		}
		public static async Thread.Async.TtTask<EngineNS.IO.TtMemWriter> QueryProperties(EExecuter executerType, ushort executeIndex, EngineNS.Bricks.Network.RPC.FRpcCallArg rpcArg)
		{
			var ExeIndex = rpcArg.ExeIndex;
			var NetConnect = rpcArg.NetConnect;
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = TtEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = TtEngine.Instance.RpcModule.DefaultNetConnect;
			}
			var retContext = TtReturnAwaiter<EngineNS.IO.TtMemWriter>.CreateInstance(rpcArg.Timeout, rpcArg.ReturnContext);
			if (NetConnect != null)
			{
				retContext.Context.Index = ExeIndex;
			}
			using (var writer = EngineNS.IO.TtMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<EngineNS.IO.TtMemWriter>(writer);
				FRouter router = new FRouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.PropertyData;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router);
				UInt16 methodIndex = 3;
				pkg.Write(methodIndex);
				pkg.Write(executerType);
				pkg.Write(executeIndex);
				pkg.Write(retContext.Context);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
			return await TtRpcAwaiter.AwaitReturn_MemWriter(retContext);
		}
	}
}


namespace EngineNS.Bricks.Network.RPC
{
	partial class TtRpcPropertyDataManager
	{
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_SyncAllProperties = (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host, EngineNS.Bricks.Network.RPC.TtCallContext context) =>
		{
			EngineNS.IO.TtMemWriter data;
			reader.Read(out data);
			((EngineNS.Bricks.Network.RPC.TtRpcPropertyDataManager)host).SyncAllProperties(data, context);
			data.Dispose();
		};
		public void RPC_SyncAllProperties(EngineNS.IO.TtMemWriter data, EngineNS.Bricks.Network.RPC.TtReturnContext retContext = null)
		{
			var rpcArg = new EngineNS.Bricks.Network.RPC.FRpcCallArg(retContext);
			rpcArg.ExeIndex = RpcExecuteIndex;
			rpcArg.NetConnect = GetRpcConnect(0);
			TtRpcPropertyDataManager_RpcCaller.SyncAllProperties(data, rpcArg);
		}
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_CreateRpcHost = (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host, EngineNS.Bricks.Network.RPC.TtCallContext context) =>
		{
			Rtti.TtTypeDesc type;
			reader.Read(out type);
			ushort executeIndex;
			reader.Read(out executeIndex);
			string info;
			reader.Read(out info);
			((EngineNS.Bricks.Network.RPC.TtRpcPropertyDataManager)host).CreateRpcHost(type, executeIndex, info, context);
			if (context.NoBroadCast)
			{
				return;
			}
			var broadCaster = (host as IRpcHost)?.GetRpcBroadCaster();
			if (broadCaster != null)
			{
				var t_iter = broadCaster.GetEnumerator(host as IRpcHost, typeof(EngineNS.Bricks.Network.RPC.TtRpcPropertyDataManager));
				if (t_iter!=null)
				{
					while (t_iter.MoveNext())
					{
						var t_sendTarget = t_iter.Current as EngineNS.Bricks.Network.RPC.TtRpcPropertyDataManager;
						if (t_sendTarget!=null)
						{
							t_sendTarget.RPC_CreateRpcHost(type, executeIndex, info, null);
						}
					}
				}
			}
		};
		public void RPC_CreateRpcHost(Rtti.TtTypeDesc type, ushort executeIndex, string info, EngineNS.Bricks.Network.RPC.TtReturnContext retContext = null)
		{
			var rpcArg = new EngineNS.Bricks.Network.RPC.FRpcCallArg(retContext);
			rpcArg.ExeIndex = RpcExecuteIndex;
			rpcArg.NetConnect = GetRpcConnect(1);
			TtRpcPropertyDataManager_RpcCaller.CreateRpcHost(type, executeIndex, info, rpcArg);
		}
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_RemoveRpcHost = (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host, EngineNS.Bricks.Network.RPC.TtCallContext context) =>
		{
			EExecuter executerType;
			reader.Read(out executerType);
			ushort executeIndex;
			reader.Read(out executeIndex);
			string info;
			reader.Read(out info);
			((EngineNS.Bricks.Network.RPC.TtRpcPropertyDataManager)host).RemoveRpcHost(executerType, executeIndex, info, context);
		};
		public void RPC_RemoveRpcHost(EExecuter executerType, ushort executeIndex, string info, EngineNS.Bricks.Network.RPC.TtReturnContext retContext = null)
		{
			var rpcArg = new EngineNS.Bricks.Network.RPC.FRpcCallArg(retContext);
			rpcArg.ExeIndex = RpcExecuteIndex;
			rpcArg.NetConnect = GetRpcConnect(2);
			TtRpcPropertyDataManager_RpcCaller.RemoveRpcHost(executerType, executeIndex, info, rpcArg);
		}
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_QueryProperties = (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host, EngineNS.Bricks.Network.RPC.TtCallContext context) =>
		{
			EExecuter executerType;
			reader.Read(out executerType);
			ushort executeIndex;
			reader.Read(out executeIndex);
			FReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Bricks.Network.RPC.TtRpcPropertyDataManager)host).QueryProperties(executerType, executeIndex, context);
			using (var writer = EngineNS.IO.TtMemWriter.CreateInstance())
			{
				var pkg = new IO.AuxWriter<EngineNS.IO.TtMemWriter>(writer);
				var pkgHeader = new FPkgHeader();
				pkgHeader.SetHasReturn(true);
				pkg.Write(pkgHeader);
				pkg.Write(retContext);
				pkg.Write(ret);
				pkg.CoreWriter.SurePkgHeader();
				context.NetConnect?.Send(in pkg);
			}
			ret.Dispose();
		};
		public async Thread.Async.TtTask<EngineNS.IO.TtMemWriter> RPC_QueryProperties(EExecuter executerType, ushort executeIndex, EngineNS.Bricks.Network.RPC.TtReturnContext retContext = null)
		{
			var rpcArg = new EngineNS.Bricks.Network.RPC.FRpcCallArg(retContext);
			rpcArg.ExeIndex = RpcExecuteIndex;
			rpcArg.NetConnect = GetRpcConnect(3);
			return await TtRpcPropertyDataManager_RpcCaller.QueryProperties(executerType, executeIndex, rpcArg);
		}
	}
}
#endregion//TitanEngine_AutoGen_RPC
#endif//TitanEngine_AutoGen_RPC