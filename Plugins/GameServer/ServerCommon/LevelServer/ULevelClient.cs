using System;
using System.Collections;
using System.Collections.Generic;
using EngineNS.Bricks.Network.RPC;
using EngineNS.GamePlay;
using EngineNS.Plugins.GateServer;
using EngineNS.Plugins.ServerCommon;
using EngineNS.Support;
using MathNet.Numerics.LinearAlgebra.Solvers;
using NPOI.SS.Formula.Functions;
using NPOI.SS.Formula.PTG;

namespace EngineNS.Plugins.LevelServer
{
    [TtRpcClass(RunTarget = ERunTarget.Level, Executer = EExecuter.Client)]
    public partial class ULevelClient : ServerCommon.UClient, IRpcHost, CSCommon.ISyncActor
    {
        #region IRpcHost
        static TtRpcClass smRpcClass = null;
        public TtRpcClass GetRpcClass()
        {
            if (smRpcClass == null)
                smRpcClass = new TtRpcClass(this.GetType());
            return smRpcClass;
        }
        public virtual uint RpcExecuteIndex { get; set; } = 0;
        public virtual Bricks.Network.INetConnect GetRpcConnect(UInt16 methodIndex)
        {
            return ClientConnect;
        }
        public void OnRpcPropertyChanged(string propName, object v, object old)
        {

        }
        public bool IgnoreUpdateProperties(uint RpcExecuteIndex)
        {
            return false;
        }
        public virtual TtRpcBroadCaster GetRpcBroadCaster(UInt16 methodIndex)
        {
            return TtRpcBroadCaster.Instance;
        }
        #endregion

        #region ISyncActor
        public uint SyncId { get; set; }
        #endregion

        public Bricks.Network.INetConnect ClientConnect { get; set; }
		public UInt16 IndexInGame { get; set; } = UInt16.MaxValue;//IndexInGate
        public TtPlacementBase Placement { get; } = new TtPlacement();

        public override void Tick()
		{
            //RobotClient.URobot_RpcCaller.UpdatePosition(Vector3.One, this.IndexInGame, ClientConnect);
		}

        #region RPC
        [TtRpcMethod(Index = 100 + 0)]
        public int GetHP(TtCallContext context)
        {
            return 5;
        }
        [TtRpcMethod(Index = 100 + 1)]
        public void UpdateAutoSyncData(IO.TtMemWriter data, TtCallContext context)
        {
            
        }
        #endregion
    }

    public class ULevelClientManager : UClientManager
    {

    }
    public class UMultiConnects : Bricks.Network.INetConnect
    {
        public Bricks.Network.RPC.EAuthority Authority { get; set; } = EAuthority.Client;
        public bool Connected
        {
            get => true;
            set
            {

            }
        }
        public object Tag { get; set; } = null;
        public UInt16 GetConnectId()
        {
            return 0;
        }
        public virtual unsafe void Send(in IO.AuxWriter<IO.TtMemWriter> pkg)
        {
            var enumerator = GetEnumerator();
            enumerator.Reset();
            if (enumerator.MoveNext())
            {
                var client = enumerator.Current as ULevelClient;
                if (client != null)
                {
                    var pRouter = (FRouter*)((byte*)pkg.Ptr + sizeof(FPkgHeader));
                    BeforeSend(pRouter, client);
                    client.ClientConnect.Send(in pkg);
                }
            }
        }
        public unsafe void Send(void* ptr, uint size)
        {
            System.Diagnostics.Debug.Assert(false);
        }
        public virtual IEnumerator GetEnumerator()
        {
            return null;
        }
        public unsafe virtual void BeforeSend(FRouter* pRouter, ULevelClient client)
        {
            pRouter->Index = client.ClientIndex;
        }
    }

    public class UClientAllConnects : UMultiConnects
    {
        public UClientManager ClientManager;
        public override IEnumerator GetEnumerator()
        {
            return ClientManager.Clients.GetEnumerator();
        }
    }
}
#if TitanEngine_AutoGen
#region TitanEngine_AutoGen


namespace EngineNS.Plugins.LevelServer
{
	partial class ULevelClient
	{
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_GetHP = (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host, EngineNS.Bricks.Network.RPC.TtCallContext context) =>
		{
			FReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Plugins.LevelServer.ULevelClient)host).GetHP(context);
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
		};
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_UpdateAutoSyncData = (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host, EngineNS.Bricks.Network.RPC.TtCallContext context) =>
		{
			IO.TtMemWriter data;
			reader.Read(out data);
			((EngineNS.Plugins.LevelServer.ULevelClient)host).UpdateAutoSyncData(data, context);
		};
	}
}
#endregion//TitanEngine_AutoGen
#endif//TitanEngine_AutoGen
#if TitanEngine_AutoGen_RPC
#region TitanEngine_AutoGen_RPC


namespace EngineNS.Plugins.LevelServer
{
	partial class ULevelClient
	{
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_GetHP = (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host, EngineNS.Bricks.Network.RPC.TtCallContext context) =>
		{
			EngineNS.Bricks.Network.RPC.FReturnContext retContext;
			reader.Read(out retContext, false);
			var ret = ((EngineNS.Plugins.LevelServer.ULevelClient)host).GetHP(context);
			using (var writer = EngineNS.IO.TtMemWriter.CreateInstance())
			{
				var pkg = new IO.AuxWriter<EngineNS.IO.TtMemWriter>(writer);
				var pkgHeader = new EngineNS.Bricks.Network.RPC.FPkgHeader();
				pkgHeader.SetHasReturn(true);
				pkg.Write(pkgHeader);
				pkg.Write(retContext, false);
				pkg.Write(ret);
				pkg.CoreWriter.SurePkgHeader();
				context.NetConnect?.Send(in pkg);
			}
		};
		public async Thread.Async.TtTask<int> RPC_GetHP(EngineNS.Bricks.Network.RPC.TtReturnContext retContext = null)
		{
			var rpcArg = new EngineNS.Bricks.Network.RPC.FRpcCallArg(retContext);
			rpcArg.ExeIndex = RpcExecuteIndex;
			rpcArg.NetConnect = GetRpcConnect(100 + 0);
			return await ULevelClient_RpcCaller.GetHP(rpcArg);
		}
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_UpdateAutoSyncData = (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host, EngineNS.Bricks.Network.RPC.TtCallContext context) =>
		{
			IO.TtMemWriter data;
			reader.Read(out data);
			((EngineNS.Plugins.LevelServer.ULevelClient)host).UpdateAutoSyncData(data, context);
			data.Dispose();
		};
		public void RPC_UpdateAutoSyncData(IO.TtMemWriter data, EngineNS.Bricks.Network.RPC.TtReturnContext retContext = null)
		{
			var rpcArg = new EngineNS.Bricks.Network.RPC.FRpcCallArg(retContext);
			rpcArg.ExeIndex = RpcExecuteIndex;
			rpcArg.NetConnect = GetRpcConnect(100 + 1);
			ULevelClient_RpcCaller.UpdateAutoSyncData(data, rpcArg);
		}
	}
}
#endregion//TitanEngine_AutoGen_RPC
#endif//TitanEngine_AutoGen_RPC