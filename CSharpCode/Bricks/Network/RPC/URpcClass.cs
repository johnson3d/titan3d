using EngineNS.Support;
using System;
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
    public class TtRpcMethodAttribute : Attribute
    {
        public UInt16 Index;
        public EPkgTypes PkgFlags;
        public EAuthority Authority = EAuthority.Client;
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
        INetConnect GetRpcConnect();
        void OnRpcPropertyChanged(string propName, object v);
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
    public partial class TtRpcPropertyDataManager : IRpcHost
    {
        #region Interface
        static TtRpcClass smRpcClass = null;
        public TtRpcClass GetRpcClass()
        {
            if (smRpcClass == null)
                smRpcClass = new TtRpcClass(this.GetType());
            return smRpcClass;
        }
        public virtual ushort RpcExecuteIndex { get; set; } = 0;
        public virtual INetConnect GetRpcConnect()
        {
            return TtEngine.Instance.RpcModule.DefaultNetConnect;
        }
        public void OnRpcPropertyChanged(string propName, object v)
        {

        }
        #endregion

        [TtRpcProperty]
        public int TestSync1 { get; set; } = 1;
        public TtRpcPropertyDataManager()
        {
            RegisterHost(this);
        }
        public ConditionalWeakTable<IRpcHost, TtRpcPropertyData> PropertyDatas = new();
        public void RegisterHost(IRpcHost host)
        {
            var kls = host.GetRpcClass();
            var propData = new TtRpcPropertyData(kls);
            PropertyDatas.Add(host, propData);
        }
        public void UnregisterHost(IRpcHost host)
        {
            PropertyDatas.Remove(host);
        }
        private void CollectProperties(IO.IWriter writer)
        {
            List<object> propValues = new();
            TtBitset modifyProps = new();
            foreach (var i in PropertyDatas)
            {
                var host = i.Key;
                if (host == null)
                    continue;

                propValues.Clear();
                i.Value.CollectProperties(host, modifyProps, propValues, true);
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
                if (host == null || host.RpcExecuteIndex != rpcIndex)
                    bSet = false;
                if (PropertyDatas.TryGetValue(host, out var propData) == false)
                    continue;
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
                            prop.SetValue(host, v);
                            host.OnRpcPropertyChanged(propInfo.Name, v);
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
			rpcArg.NetConnect = GetRpcConnect();
			TtRpcPropertyDataManager_RpcCaller.SyncAllProperties(data, rpcArg);
		}
	}
}
#endregion//TitanEngine_AutoGen_RPC
#endif//TitanEngine_AutoGen_RPC