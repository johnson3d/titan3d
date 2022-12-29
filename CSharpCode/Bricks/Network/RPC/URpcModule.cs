using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EngineNS.Bricks.Network.RPC;

namespace EngineNS.Bricks.Network.RPC
{
    [Flags]
    public enum EPkgTypes : byte
    {
        IsReturn = (1 << 0),//这个是系统用的flags，不允许写道Attribute上
        WeakPkg = (1 << 1),
    }
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
    public enum EExecuter : sbyte
    {
        Root,
        Client,
        Profiler,
    }
    public class URpcClassAttribute : Attribute
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
    public class URpcMethodAttribute : Attribute
	{	
		public UInt16 Index;
        public EPkgTypes PkgFlags;
        public EAuthority Authority = EAuthority.Client;
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct URouter
    {
        public ERunTarget RunTarget;
        public EExecuter Executer;
        public UInt16 Index;
		public EAuthority Authority;
        public override string ToString()
        {
            return $"Target = {RunTarget}, Executer = {Executer}, Index = {Index}, Authority = {Authority}";
        }
    }
    public struct UCallContext
    {
        public INetConnect NetConnect;
        public ERunTarget Caller;
        public ERunTarget Callee;
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct UReturnContext
    {
        public UInt32 Handle;
        public ERunTarget RunTarget;
        public byte Unused;
        public UInt16 Index;
    }
    public class UReturnAwaiter
    {
        public class UReturnAwaiterAllocator : UPooledObject<UReturnAwaiter>
        {
            protected override bool OnObjectRelease(UReturnAwaiter obj)
            {
                obj.Reset();
                return true;
            }
        }
        static UReturnAwaiterAllocator mAllocator = new UReturnAwaiterAllocator();
        private static UInt32 CurrentId = 0;
        public static UReturnAwaiter CreateInstance(uint timeOut)
        {
            //var result = new UReturnAwaiter();
            var result = mAllocator.QueryObjectSync();
            result.Context.Handle = System.Threading.Interlocked.Increment(ref CurrentId);
            result.Context.RunTarget = UEngine.Instance.RpcModule.RpcManager.CurrentTarget;
			result.BeginWaitTime = Support.Time.GetTickCount();
			result.Timeout = timeOut;
            UEngine.Instance.RpcModule.PushReturnAwaiter(result);
            return result;
        }
        public static void DisposeInstance(UReturnAwaiter awt)
        {
            mAllocator.ReleaseObject(awt);
        }
        public void Reset()
        {
            RetCallBack = null;
            Context.RunTarget = ERunTarget.None;
            Context.Index = 0;
            Context.Handle = 0;
        }
        public delegate void FReturnCallBack(ref IO.AuxReader<UMemReader> pkg, bool isTimeOut);
        public UReturnContext Context;
        public FReturnCallBack RetCallBack;
		public long BeginWaitTime;
        public uint Timeout;
    }
    public delegate void FCallMethod(IO.AuxReader<UMemReader> pkg, object host, UCallContext context);
    public interface IRpcHost
    {
        URpcClass GetRpcClass();
    }
    [Rtti.GenMetaClass(IsOverrideBitset = false)]
    [URpcClassAttribute(RunTarget = ERunTarget.None, Executer = EExecuter.Root, CallerInClass = true)]
    public partial class URpcManager : IRpcHost
    {
        public ERunTarget CurrentTarget { get; set; } = ERunTarget.Client;
        static URpcClass smRpcClass = null;
        public URpcClass GetRpcClass()
        {
            if (smRpcClass == null)
                smRpcClass = new URpcClass(this.GetType());
            return smRpcClass;
        }
        public virtual object GetExecuter(in URouter router)
        {
            switch (router.Executer)
            {
                case EExecuter.Profiler:
                    return RpcProfiler;
                case EExecuter.Root:
                    return this;
            }

            return null;
        }
        public virtual INetConnect GetRunTargetConnect(in URouter target, INetConnect connect)
		{
			return null;
		}
        public virtual INetConnect GetRunTargetConnect(in UReturnContext retCtx, INetConnect connect)
		{
            return null;
        }
		public unsafe virtual bool OnRelay(URouter* pRouter, INetConnect connect)
		{
			return true;
		}
        Profiler.URpcProfiler RpcProfiler = new Profiler.URpcProfiler();
        [Rtti.GenMeta()]
        private int mAutoGenProp0;

        [URpcMethod(Index = 0)]
        public int TestBaseRpc1(float arg, UCallContext context)
        {
            //AutoGenProp0 = 1;
            //AutoGenProp0 = 2;
            return (int)arg + 2;
        }
    }

	public class URpcModule : UModule<UEngine>
	{
		public INetConnect DefaultNetConnect = new UFakeNetConnect();
        public UInt16 DefaultExeIndex = UInt16.MaxValue;
        //public UNetConnetProvider ConnectProvider { get; set; } = null;
        public URpcManager RpcManager;
		public Dictionary<UInt32, UReturnAwaiter> ReturnAwaiters = new Dictionary<uint, UReturnAwaiter>();
		public UNetPackageManager NetPackageManager = new UNetPackageManager();
		public void PushReturnAwaiter(UReturnAwaiter awaiter)
		{
			lock (ReturnAwaiters)
			{
				ReturnAwaiters[awaiter.Context.Handle] = awaiter;
			}
		}
		public void RemoteReturn(UInt32 handle, ref IO.AuxReader<UMemReader> pkg)
		{
			UReturnAwaiter awaiter = null;
			lock (ReturnAwaiters)
			{
				if (ReturnAwaiters.TryGetValue(handle, out awaiter))
				{
					ReturnAwaiters.Remove(handle);
				}
				else
				{
					return;
				}
			}
			awaiter?.RetCallBack(ref pkg, false);

			UReturnAwaiter.DisposeInstance(awaiter);
		}
		public override async System.Threading.Tasks.Task<bool> Initialize(UEngine host)
		{
			await Thread.AsyncDummyClass.DummyFunc();
			var type = Rtti.UTypeDesc.TypeOf(host.Config.RpcRootType);
			RpcManager = Rtti.UTypeDescManager.CreateInstance(type) as URpcManager;
			if (RpcManager == null)
			{
				RpcManager = new URpcManager();
			}
			return true;
		}
        public unsafe override void Tick(UEngine host)
        {
            NetPackageManager.Tick();
			var now = Support.Time.GetTickCount();
			var nullPkg = new IO.AuxReader<UMemReader>();
            lock (ReturnAwaiters)
			{
                foreach (var i in ReturnAwaiters)
                {
					var awaiter = i.Value;
                    if ((uint)(now - awaiter.BeginWaitTime) > awaiter.Timeout)
                    {
                        ReturnAwaiters.Remove(i.Key);
                        awaiter.RetCallBack(ref nullPkg, true);
                        UReturnAwaiter.DisposeInstance(awaiter);
                        break;
                    }
                }
            }   
        }
    }
}

namespace EngineNS
{
    partial class UEngine
    {
        public Bricks.Network.RPC.URpcModule RpcModule { get; } = new Bricks.Network.RPC.URpcModule();
    }
}

namespace EngineNS.UTest
{
    using Bricks.Network;
    using Bricks.Network.RPC;
    using EngineNS.Rtti;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    //using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Reflection;

    [UTest.UTest]
    [Rtti.GenMetaClass()]
    [URpcClassAttribute(RunTarget = ERunTarget.None, Executer = EExecuter.Root, CallerInClass = true)]
    public partial class UTest_Rpc : Bricks.Network.RPC.URpcManager
    {
        [Rtti.GenMeta()]
        private int mAutoGenProp2;
        [Rtti.GenMeta()]
        private int mAutoGenProp1;
        //partial void OnPropertyPreChanged(string name, int index, ref EngineNS.Support.UAnyPointer info)
        //{
        //    switch (index)
        //    {
        //        case 0:
        //            info.Value.SetI32(mAutoGenProp1);
        //            break;
        //    }
        //}
        //partial void OnPropertyChanged(string name, int index, ref EngineNS.Support.UAnyPointer info)
        //{
        //    if (name == "AutoGenProp1")
        //    {
        //        if(Name2Index["AutoGenProp1"] != index)
        //        {
        //            return;
        //        }
        //        if (Index2Name[index] != "AutoGenProp1")
        //        {
        //            return;
        //        }
        //        if (Bitset.IsSet((uint)index) == false)
        //        {
        //            return;
        //        }
        //        return;
        //    }
        //}

        [URpcMethod(Index = 100 + 0, PkgFlags = EPkgTypes.WeakPkg)]
        public int TestRpc1(float arg, UCallContext context)
        {
            //AutoGenProp1 = 1;
            //AutoGenProp1 = 2;
            return (int)arg + 2;
        }
        [URpcMethod(Index = 100 + 1)]
        public void TestRpc2(string arg, UCallContext context)
        {
            Console.WriteLine(arg);
        }
        [URpcMethod(Index = 100 + 2)]
        public IO.ISerializer TestRpc3(int arg, UCallContext context)
        {
            return null;
        }
        [URpcMethod(Index = 100 + 3)]
        public string TestRpc4(string arg, UCallContext context)
        {
            return arg.ToString();
        }
        [URpcMethod(Index = 100 + 4)]
        public async Task<Vector3> TestRpc5(Vector3 arg, UCallContext context)
        {
            var ret3 = await UTest_Rpc_RpcCaller.TestRpc4("10.1");
            var result = arg * 2;
            result.X += System.Convert.ToSingle(ret3);
            return result;
        }
        public class TestRPCArgument : IO.BaseSerializer
        {
            [Rtti.Meta]
            public int AA { get; set; } = 100 + 3;
            public override void OnWriteMember(IO.IWriter ar, IO.ISerializer obj, UMetaVersion metaVersion)
            {
                ar.Write(AA);
            }
            public override void OnReadMember(IO.IReader ar, IO.ISerializer obj, Rtti.UMetaVersion metaVersion)
            {
                AA = ar.Read<int>();
            }
        }
        [URpcMethod(Index = 100 + 5)]
        public EngineNS.UTest.UTest_Rpc.TestRPCArgument TestRpc6(EngineNS.UTest.UTest_Rpc.TestRPCArgument arg, UCallContext context)
        {
            arg.AA += 5;
            return arg;
        }
        public struct TestUnmanagedStruct
        {
            public int A;
            public Vector3 B;
        }
        [URpcMethod(Index = 100 + 6)]
        public int TestRpc7(EngineNS.UTest.UTest_Rpc.TestUnmanagedStruct arg, UCallContext context)
        {
            arg.A += 15;
            return arg.A;
        }
        public void UnitTestEntrance()
        {
            Action action = async () =>
            {
                INetConnect pConnect = UEngine.Instance.RpcModule.DefaultNetConnect;
                UTcpClient tcpClient = new UTcpClient();
                var ok = await tcpClient.Connect("127.0.0.1", 5555);
                if (ok)
                {
                    pConnect = tcpClient;
                }
                else
                {
                    tcpClient = null;
                }
                var ret = await UTest_Rpc_RpcCaller.TestRpc1(2.0f, uint.MaxValue, ushort.MaxValue, pConnect);
                if (ret != 4)
                {
                    return;
                }
                UTest_Rpc_RpcCaller.TestRpc2("", ushort.MaxValue, pConnect);
                var ret3 = await UTest_Rpc_RpcCaller.TestRpc3(1, uint.MaxValue, ushort.MaxValue, pConnect);
                var ret4 = await UTest_Rpc_RpcCaller.TestRpc4("2", uint.MaxValue, ushort.MaxValue, pConnect);
                if (ret4 != "2")
                {
                    return;
                }
                var ret5 = await UTest_Rpc_RpcCaller.TestRpc5(Vector3.One, uint.MaxValue, ushort.MaxValue, pConnect);
                var ret6 = await UTest_Rpc_RpcCaller.TestRpc6(new TestRPCArgument() { AA = 8 }, uint.MaxValue, ushort.MaxValue, pConnect);
                var ret7 = await UTest_Rpc_RpcCaller.TestRpc7(new TestUnmanagedStruct() { A = 8 }, uint.MaxValue, ushort.MaxValue, pConnect);

                var base_ret7 = await URpcManager_RpcCaller.TestBaseRpc1(5.0f, uint.MaxValue, ushort.MaxValue, pConnect);

                tcpClient?.Disconnect();
            };
            action();
        }
    }
}


#if TitanEngine_AutoGen
#region TitanEngine_AutoGen
#pragma warning disable 105


namespace EngineNS.Bricks.Network.RPC
{
	public partial class URpcManager_RpcCaller
	{
		public static async System.Threading.Tasks.Task<int> TestBaseRpc1(float arg, uint Timeout = uint.MaxValue, UInt16 ExeIndex = UInt16.MaxValue, EngineNS.Bricks.Network.INetConnect NetConnect = null)
		{
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = UEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = UEngine.Instance.RpcModule.DefaultNetConnect;
			}
			var retContext = UReturnAwaiter.CreateInstance(Timeout);
			if (NetConnect != null)
			{
				retContext.Context.Index = ExeIndex;
			}
			using (var writer = UMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<UMemWriter>(writer);
				URouter router = new URouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.Root;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router);
				UInt16 methodIndex = 0;
				pkg.Write(methodIndex);
				pkg.Write(arg);
				pkg.Write(retContext.Context);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
			return await URpcAwaiter.AwaitReturn<int>(retContext);
		}
	}
}


namespace EngineNS.Bricks.Network.RPC
{
	partial class URpcManager
	{
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_TestBaseRpc1 = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			float arg;
			reader.Read(out arg);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Bricks.Network.RPC.URpcManager)host).TestBaseRpc1(arg, context);
			using (var writer = UMemWriter.CreateInstance())
			{
				var pkg = new IO.AuxWriter<UMemWriter>(writer);
				var pkgHeader = new FPkgHeader();
				pkgHeader.SetHasReturn(true);
				pkg.Write(pkgHeader);
				pkg.Write(retContext);
				pkg.Write(ret);
				pkg.CoreWriter.SurePkgHeader();
				context.NetConnect?.Send(in pkg);
			}
		};
	}
}
#pragma warning disable 105


namespace EngineNS.UTest
{
	public partial class UTest_Rpc_RpcCaller
	{
		public static async System.Threading.Tasks.Task<int> TestRpc1(float arg, uint Timeout = uint.MaxValue, UInt16 ExeIndex = UInt16.MaxValue, EngineNS.Bricks.Network.INetConnect NetConnect = null)
		{
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = UEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = UEngine.Instance.RpcModule.DefaultNetConnect;
			}
			var retContext = UReturnAwaiter.CreateInstance(Timeout);
			if (NetConnect != null)
			{
				retContext.Context.Index = ExeIndex;
			}
			using (var writer = UMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<UMemWriter>(writer);
				URouter router = new URouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.Root;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new FPkgHeader();
				pkgHeader.PKGFlags = (byte)EPkgTypes.WeakPkg;
				pkg.Write(pkgHeader);
				pkg.Write(router);
				UInt16 methodIndex = 100 + 0;
				pkg.Write(methodIndex);
				pkg.Write(arg);
				pkg.Write(retContext.Context);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
			return await URpcAwaiter.AwaitReturn<int>(retContext);
		}
		public static void TestRpc2(string arg, UInt16 ExeIndex = UInt16.MaxValue, EngineNS.Bricks.Network.INetConnect NetConnect = null)
		{
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = UEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = UEngine.Instance.RpcModule.DefaultNetConnect;
			}
			using (var writer = UMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<UMemWriter>(writer);
				URouter router = new URouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.Root;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router);
				UInt16 methodIndex = 100 + 1;
				pkg.Write(methodIndex);
				pkg.Write(arg);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
		}
		public static async System.Threading.Tasks.Task<IO.ISerializer> TestRpc3(int arg, uint Timeout = uint.MaxValue, UInt16 ExeIndex = UInt16.MaxValue, EngineNS.Bricks.Network.INetConnect NetConnect = null)
		{
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = UEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = UEngine.Instance.RpcModule.DefaultNetConnect;
			}
			var retContext = UReturnAwaiter.CreateInstance(Timeout);
			if (NetConnect != null)
			{
				retContext.Context.Index = ExeIndex;
			}
			using (var writer = UMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<UMemWriter>(writer);
				URouter router = new URouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.Root;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router);
				UInt16 methodIndex = 100 + 2;
				pkg.Write(methodIndex);
				pkg.Write(arg);
				pkg.Write(retContext.Context);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
			return await URpcAwaiter.AwaitReturn<IO.ISerializer>(retContext);
		}
		public static async System.Threading.Tasks.Task<string> TestRpc4(string arg, uint Timeout = uint.MaxValue, UInt16 ExeIndex = UInt16.MaxValue, EngineNS.Bricks.Network.INetConnect NetConnect = null)
		{
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = UEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = UEngine.Instance.RpcModule.DefaultNetConnect;
			}
			var retContext = UReturnAwaiter.CreateInstance(Timeout);
			if (NetConnect != null)
			{
				retContext.Context.Index = ExeIndex;
			}
			using (var writer = UMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<UMemWriter>(writer);
				URouter router = new URouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.Root;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router);
				UInt16 methodIndex = 100 + 3;
				pkg.Write(methodIndex);
				pkg.Write(arg);
				pkg.Write(retContext.Context);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
			return await URpcAwaiter.AwaitReturn_String(retContext);
		}
		public static async System.Threading.Tasks.Task<Vector3> TestRpc5(Vector3 arg, uint Timeout = uint.MaxValue, UInt16 ExeIndex = UInt16.MaxValue, EngineNS.Bricks.Network.INetConnect NetConnect = null)
		{
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = UEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = UEngine.Instance.RpcModule.DefaultNetConnect;
			}
			var retContext = UReturnAwaiter.CreateInstance(Timeout);
			if (NetConnect != null)
			{
				retContext.Context.Index = ExeIndex;
			}
			using (var writer = UMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<UMemWriter>(writer);
				URouter router = new URouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.Root;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router);
				UInt16 methodIndex = 100 + 4;
				pkg.Write(methodIndex);
				pkg.Write(arg);
				pkg.Write(retContext.Context);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
			return await URpcAwaiter.AwaitReturn<Vector3>(retContext);
		}
		public static async System.Threading.Tasks.Task<EngineNS.UTest.UTest_Rpc.TestRPCArgument> TestRpc6(EngineNS.UTest.UTest_Rpc.TestRPCArgument arg, uint Timeout = uint.MaxValue, UInt16 ExeIndex = UInt16.MaxValue, EngineNS.Bricks.Network.INetConnect NetConnect = null)
		{
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = UEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = UEngine.Instance.RpcModule.DefaultNetConnect;
			}
			var retContext = UReturnAwaiter.CreateInstance(Timeout);
			if (NetConnect != null)
			{
				retContext.Context.Index = ExeIndex;
			}
			using (var writer = UMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<UMemWriter>(writer);
				URouter router = new URouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.Root;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router);
				UInt16 methodIndex = 100 + 5;
				pkg.Write(methodIndex);
				pkg.Write(arg);
				pkg.Write(retContext.Context);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
			return await URpcAwaiter.AwaitReturn<EngineNS.UTest.UTest_Rpc.TestRPCArgument>(retContext);
		}
		public static async System.Threading.Tasks.Task<int> TestRpc7(EngineNS.UTest.UTest_Rpc.TestUnmanagedStruct arg, uint Timeout = uint.MaxValue, UInt16 ExeIndex = UInt16.MaxValue, EngineNS.Bricks.Network.INetConnect NetConnect = null)
		{
			if (ExeIndex == UInt16.MaxValue)
			{
				ExeIndex = UEngine.Instance.RpcModule.DefaultExeIndex;
			}
			if (NetConnect == null)
			{
				NetConnect = UEngine.Instance.RpcModule.DefaultNetConnect;
			}
			var retContext = UReturnAwaiter.CreateInstance(Timeout);
			if (NetConnect != null)
			{
				retContext.Context.Index = ExeIndex;
			}
			using (var writer = UMemWriter.CreateInstance())
			{
				var pkg = new EngineNS.IO.AuxWriter<UMemWriter>(writer);
				URouter router = new URouter();
				router.RunTarget = ERunTarget.None;
				router.Executer = EExecuter.Root;
				router.Index = ExeIndex;
				router.Authority = EngineNS.Bricks.Network.RPC.EAuthority.God;
				var pkgHeader = new FPkgHeader();
				pkg.Write(pkgHeader);
				pkg.Write(router);
				UInt16 methodIndex = 100 + 6;
				pkg.Write(methodIndex);
				pkg.Write(arg);
				pkg.Write(retContext.Context);
				pkg.CoreWriter.SurePkgHeader();
				NetConnect?.Send(in pkg);
			}
			return await URpcAwaiter.AwaitReturn<int>(retContext);
		}
	}
}


namespace EngineNS.UTest
{
	partial class UTest_Rpc
	{
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_TestRpc1 = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			float arg;
			reader.Read(out arg);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.UTest.UTest_Rpc)host).TestRpc1(arg, context);
			using (var writer = UMemWriter.CreateInstance())
			{
				var pkg = new IO.AuxWriter<UMemWriter>(writer);
				var pkgHeader = new FPkgHeader();
				pkgHeader.SetHasReturn(true);
				pkg.Write(pkgHeader);
				pkg.Write(retContext);
				pkg.Write(ret);
				pkg.CoreWriter.SurePkgHeader();
				context.NetConnect?.Send(in pkg);
			}
		};
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_TestRpc2 = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			string arg;
			reader.Read(out arg);
			((EngineNS.UTest.UTest_Rpc)host).TestRpc2(arg, context);
		};
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_TestRpc3 = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			int arg;
			reader.Read(out arg);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.UTest.UTest_Rpc)host).TestRpc3(arg, context);
			using (var writer = UMemWriter.CreateInstance())
			{
				var pkg = new IO.AuxWriter<UMemWriter>(writer);
				var pkgHeader = new FPkgHeader();
				pkgHeader.SetHasReturn(true);
				pkg.Write(pkgHeader);
				pkg.Write(retContext);
				pkg.Write(ret);
				pkg.CoreWriter.SurePkgHeader();
				context.NetConnect?.Send(in pkg);
			}
		};
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_TestRpc4 = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			string arg;
			reader.Read(out arg);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.UTest.UTest_Rpc)host).TestRpc4(arg, context);
			using (var writer = UMemWriter.CreateInstance())
			{
				var pkg = new IO.AuxWriter<UMemWriter>(writer);
				var pkgHeader = new FPkgHeader();
				pkgHeader.SetHasReturn(true);
				pkg.Write(pkgHeader);
				pkg.Write(retContext);
				pkg.Write(ret);
				pkg.CoreWriter.SurePkgHeader();
				context.NetConnect?.Send(in pkg);
			}
		};
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_TestRpc5 = async (EngineNS.IO.AuxReader<UMemReader> reader, object host,  EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			Vector3 arg;
			reader.Read(out arg);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = await ((EngineNS.UTest.UTest_Rpc)host).TestRpc5(arg, context);
			using (var writer = UMemWriter.CreateInstance())
			{
				var pkg = new IO.AuxWriter<UMemWriter>(writer);
				var pkgHeader = new FPkgHeader();
				pkgHeader.SetHasReturn(true);
				pkg.Write(pkgHeader);
				pkg.Write(retContext);
				pkg.Write(ret);
				pkg.CoreWriter.SurePkgHeader();
				context.NetConnect?.Send(in pkg);
			}
		};
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_TestRpc6 = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			EngineNS.UTest.UTest_Rpc.TestRPCArgument arg;
			reader.Read(out arg);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.UTest.UTest_Rpc)host).TestRpc6(arg, context);
			using (var writer = UMemWriter.CreateInstance())
			{
				var pkg = new IO.AuxWriter<UMemWriter>(writer);
				var pkgHeader = new FPkgHeader();
				pkgHeader.SetHasReturn(true);
				pkg.Write(pkgHeader);
				pkg.Write(retContext);
				pkg.Write(ret);
				pkg.CoreWriter.SurePkgHeader();
				context.NetConnect?.Send(in pkg);
			}
		};
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_TestRpc7 = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			EngineNS.UTest.UTest_Rpc.TestUnmanagedStruct arg;
			reader.Read(out arg);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.UTest.UTest_Rpc)host).TestRpc7(arg, context);
			using (var writer = UMemWriter.CreateInstance())
			{
				var pkg = new IO.AuxWriter<UMemWriter>(writer);
				var pkgHeader = new FPkgHeader();
				pkgHeader.SetHasReturn(true);
				pkg.Write(pkgHeader);
				pkg.Write(retContext);
				pkg.Write(ret);
				pkg.CoreWriter.SurePkgHeader();
				context.NetConnect?.Send(in pkg);
			}
		};
	}
}
#endregion//TitanEngine_AutoGen
#endif//TitanEngine_AutoGen