using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        Data,
        Gate,
        Hall,
        Log,
    }
    public enum EExecuter : sbyte
    {
        Root,
        Player,
        Profiler,
    }
    public class URpcClassAttribute : Attribute
    {
        public ERunTarget RunTarget;
        public EExecuter Executer;
    }
    public class URpcMethodAttribute : Attribute
    {
        public UInt16 Index;
        public EPkgTypes PkgFlags;
        public int Authority = 0;
        public bool ReturnISerializer;
        public bool ArgISerializer;
    }
    public struct URouter
    {
        public ERunTarget RunTarget;
        public EExecuter Executer;
        public UInt16 Index;
    }
    public struct UCallContext
    {
        public INetConnect NetConnect;
        public ERunTarget Caller;
        public ERunTarget Callee;
    }
    public struct UReturnContext
    {
        public UInt32 Handle;
        public UInt16 ConnectId;
        public UInt16 Unused;
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
        public static UReturnAwaiter CreateInstance()
        {
            //var result = new UReturnAwaiter();
            var result = mAllocator.QueryObjectSync();
            result.Context.Handle = System.Threading.Interlocked.Increment(ref CurrentId);
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
            Context.ConnectId = UInt16.MaxValue;
            Context.Handle = 0;
        }
        public delegate void FReturnCallBack(ref IO.AuxReader<UMemReader> pkg, bool isTimeOut);
        public UReturnContext Context;
        public FReturnCallBack RetCallBack;
    }
    public delegate void FCallMethod(IO.AuxReader<UMemReader> pkg, object host, UCallContext context);
    public interface IRpcHost
    {
        URpcClass GetRpcClass();
    }
    [Rtti.GenMetaClass(IsOverrideBitset = false)]
    [URpcClassAttribute(RunTarget = ERunTarget.None, Executer = EExecuter.Root)]
    public partial class URpcManager : IRpcHost
    {
        public ERunTarget CurrentTarget = ERunTarget.Client;
        static URpcClass smRpcClass = null;
        public URpcClass GetRpcClass()
        {
            if (smRpcClass == null)
                smRpcClass = new URpcClass(this.GetType());
            return smRpcClass;
        }
        public virtual object GetExecuter(ref URouter router)
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

        Profiler.URpcProfiler RpcProfiler = new Profiler.URpcProfiler();
        [Rtti.GenMeta()]
        private int mAutoGenProp0;

        const int RpcIndexStart = 0;

        [URpcMethod(Index = RpcIndexStart + 0)]
        public int TestBaseRpc1(float arg, UCallContext context)
        {
            AutoGenProp0 = 1;
            AutoGenProp0 = 2;
            return (int)arg + 2;
        }
    }

    public class URpcModule : UModule<UEngine>
    {
        public INetConnect DefaultNetConnect = new UFakeNetConnect();
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
    [URpcClassAttribute(RunTarget = ERunTarget.None, Executer = EExecuter.Root)]
    public partial class UTest_Rpc : Bricks.Network.RPC.URpcManager
    {
        [Rtti.GenMeta()]
        private int mAutoGenProp2;
        [Rtti.GenMeta()]
        private int mAutoGenProp1;
        partial void OnPropertyPreChanged(string name, int index, ref EngineNS.Support.UAnyPointer info)
        {
            switch (index)
            {
                case 0:
                    info.Value.SetI32(mAutoGenProp1);
                    break;
            }
        }
        partial void OnPropertyChanged(string name, int index, ref EngineNS.Support.UAnyPointer info)
        {
            if (name == "AutoGenProp1")
            {
                if(Name2Index["AutoGenProp1"] != index)
                {
                    return;
                }
                if (Index2Name[index] != "AutoGenProp1")
                {
                    return;
                }
                if (Bitset.IsSet((uint)index) == false)
                {
                    return;
                }
                return;
            }
        }

        const int RpcIndexStart = 100;
        [URpcMethod(Index = RpcIndexStart + 0, PkgFlags = EPkgTypes.WeakPkg)]
        public int TestRpc1(float arg, UCallContext context)
        {
            AutoGenProp1 = 1;
            AutoGenProp1 = 2;
            return (int)arg + 2;
        }
        [URpcMethod(Index = RpcIndexStart + 1)]
        public void TestRpc2(string arg, UCallContext context)
        {
            Console.WriteLine(arg);
        }
        [URpcMethod(Index = RpcIndexStart + 2, ReturnISerializer = true)]
        public IO.ISerializer TestRpc3(int arg, UCallContext context)
        {
            return null;
        }
        [URpcMethod(Index = RpcIndexStart + 3)]
        public string TestRpc4(string arg, UCallContext context)
        {
            return arg.ToString();
        }
        [URpcMethod(Index = RpcIndexStart + 4)]
        public async Task<Vector3> TestRpc5(Vector3 arg, UCallContext context)
        {
            var ret3 = await UTest_Rpc.TestRpc4("10.1");
            var result = arg * 2;
            result.X += System.Convert.ToSingle(ret3);
            return result;
        }
        public class TestRPCArgument : IO.BaseSerializer
        {
            [Rtti.Meta]
            public int AA { get; set; } = RpcIndexStart + 3;
            public override void OnWriteMember(IO.IWriter ar, IO.ISerializer obj, UMetaVersion metaVersion)
            {
                ar.Write(AA);
            }
            public override void OnReadMember(IO.IReader ar, IO.ISerializer obj, Rtti.UMetaVersion metaVersion)
            {
                int t_AA;
                ar.Read(out t_AA);
                AA = t_AA;
            }
        }
        [URpcMethod(Index = RpcIndexStart + 5, ReturnISerializer = true, ArgISerializer = true)]
        public TestRPCArgument TestRpc6(TestRPCArgument arg, UCallContext context)
        {
            arg.AA += 5;
            return arg;
        }
        public struct TestUnmanagedStruct
        {
            public int A;
            public Vector3 B;
        }
        [URpcMethod(Index = RpcIndexStart + 6)]
        public int TestRpc7(TestUnmanagedStruct arg, UCallContext context)
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
                var ok = await tcpClient.Connect("127.0.0.1", 5555, 1, 1000);
                if (ok)
                {
                    pConnect = tcpClient;
                }
                else
                {
                    tcpClient = null;
                }
                var ret = await UTest_Rpc.TestRpc1(2.0f, 0, pConnect);
                if (ret != 4)
                {
                    return;
                }
                UTest_Rpc.TestRpc2("", 0, pConnect);
                var ret3 = await UTest_Rpc.TestRpc3(1, 0, pConnect);
                var ret4 = await UTest_Rpc.TestRpc4("2", 0, pConnect);
                if (ret4 != "2")
                {
                    return;
                }
                var ret5 = await UTest_Rpc.TestRpc5(Vector3.One, 0, pConnect);
                var ret6 = await UTest_Rpc.TestRpc6(new TestRPCArgument() { AA = 8 }, 0, pConnect);
                var ret7 = await UTest_Rpc.TestRpc7(new TestUnmanagedStruct() { A = 8 }, 0, pConnect);

                var base_ret7 = await URpcManager.TestBaseRpc1(5.0f, 0, pConnect);

                tcpClient?.Disconnect();
            };
            action();
        }
    }
}
