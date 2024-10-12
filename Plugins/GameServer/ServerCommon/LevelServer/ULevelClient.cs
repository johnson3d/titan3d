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
    [URpcClass(RunTarget = ERunTarget.Level, Executer = EExecuter.Client)]
    public partial class ULevelClient : ServerCommon.UClient, IRpcHost, CSCommon.ISyncActor
    {
        #region IRpcHost
        static URpcClass smRpcClass = null;
        public URpcClass GetRpcClass()
        {
            if (smRpcClass == null)
                smRpcClass = new URpcClass(this.GetType());
            return smRpcClass;
        }
        #endregion

        #region ISyncActor
        public uint SyncId { get; set; }
        #endregion

        public Bricks.Network.INetConnect ClientConnect { get; set; }
		public UInt16 IndexInGame { get; set; } = UInt16.MaxValue;//IndexInGate
		public CSCommon.UClientAutoSyncData AutoSyncData { get; } = new CSCommon.UClientAutoSyncData();
        public TtPlacementBase Placement { get; } = new TtPlacement();

        public override void Tick()
		{
            //RobotClient.URobot_RpcCaller.UpdatePosition(Vector3.One, this.IndexInGame, ClientConnect);
		}

        #region RPC
        [URpcMethod(Index = 100 + 0)]
        public int GetHP(UCallContext context)
        {
            return 5;
        }
        [URpcMethod(Index = 100 + 1)]
        public void UpdateAutoSyncData(IO.UMemWriter data, UCallContext context)
        {
            using (var reader = IO.UMemReader.CreateInstance(in data))
            {
                var ar = new IO.AuxReader<IO.UMemReader>(reader, null);
                Bricks.Network.AutoSync.FSyncHelper.SyncValues(AutoSyncData, ar);
            }
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
        public virtual unsafe void Send(in IO.AuxWriter<IO.UMemWriter> pkg)
        {
            var enumerator = GetEnumerator();
            enumerator.Reset();
            if (enumerator.MoveNext())
            {
                var client = enumerator.Current as ULevelClient;
                if (client != null)
                {
                    var pRouter = (URouter*)((byte*)pkg.Ptr + sizeof(FPkgHeader));
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
        public unsafe virtual void BeforeSend(URouter* pRouter, ULevelClient client)
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
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_GetHP = (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			FReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Plugins.LevelServer.ULevelClient)host).GetHP(context);
			using (var writer = EngineNS.IO.UMemWriter.CreateInstance())
			{
				var pkg = new IO.AuxWriter<EngineNS.IO.UMemWriter>(writer);
				var pkgHeader = new FPkgHeader();
				pkgHeader.SetHasReturn(true);
				pkg.Write(pkgHeader);
				pkg.Write(retContext);
				pkg.Write(ret);
				pkg.CoreWriter.SurePkgHeader();
				context.NetConnect?.Send(in pkg);
			}
		};
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_UpdateAutoSyncData = (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			IO.UMemWriter data;
			reader.Read(out data);
			((EngineNS.Plugins.LevelServer.ULevelClient)host).UpdateAutoSyncData(data, context);
		};
	}
}
#endregion//TitanEngine_AutoGen
#endif//TitanEngine_AutoGen