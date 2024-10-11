using Microsoft.Build.Framework.Profiler;
using System;
using System.Collections.Generic;
using EngineNS.Bricks.Network.RPC;
using EngineNS;
using EngineNS.Plugins.ClientCommon;

namespace RobotClient
{
    [URpcClass(RunTarget = ERunTarget.Client, Executer = EExecuter.Root)]
    partial class URobot : EngineNS.Bricks.Network.RPC.TtRpcManager, EngineNS.ITickable
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public URobot()
        {
            this.CurrentTarget = ERunTarget.Client;
        }
        #region tickable
        public void TickLogic(float ellapse)
        {
            RootConnectPackages.Tick();
            if (Initialized == false)
                return;
            
            Tick();
        }
        public void TickRender(float ellapse)
        {

        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {

        }
        #endregion
        #region Connect
        public UInt16 IndexInGate { get; set; } = UInt16.MaxValue;
        public UInt16 IndexInLevel { get; set; } = UInt16.MaxValue;
        public uint SyncIdInLevel { get; set; } = uint.MaxValue;
        protected EngineNS.Bricks.Network.UNetPackageManager RootConnectPackages = new EngineNS.Bricks.Network.UNetPackageManager();
        public EngineNS.Bricks.Network.UTcpClient RootConnect { get; } = new EngineNS.Bricks.Network.UTcpClient();
        #endregion
        public bool Initialized = false;
        public virtual async System.Threading.Tasks.Task<bool> Initialize()
        {
            EngineNS.TtEngine.Instance.RpcModule.RpcManager = this;
            Guid sessionId = Guid.NewGuid();
            EngineNS.TtEngine.Instance.RpcModule.DefaultNetConnect = RootConnect;
            RootConnect.ReturnContext = new TtReturnContext();
            var ret = await RootConnect.Connect("127.0.0.1", 2334, RootConnectPackages);
            if (ret)
            {
                var lnk = await EngineNS.Plugins.LoginServer.ULoginServer_RpcCaller.LoginAccount("User0", "god");
                if (RootConnect.ReturnContext.IsTimeout)
                {

                }
                RootConnect.Disconnect();
                if (lnk == null)
                {
                    EngineNS.Profiler.Log.WriteLine<EngineNS.Profiler.TtNetCategory>(EngineNS.Profiler.ELogTag.Error, "Gateway is invalid");
                }
                var np = lnk.GatewayURL;
                ret = await RootConnect.Connect(np.Ip, np.Port, RootConnectPackages);
                sessionId = lnk.Sessiond;

                IndexInGate = await EngineNS.Plugins.GateServer.UGateServer_RpcCaller.RegClient(sessionId, "User0");
                TtEngine.Instance.RpcModule.DefaultExeIndex = IndexInGate;

                var ok = await EngineNS.Plugins.RootServer.URootServer_RpcCaller.UpdatePayload(EngineNS.Bricks.Network.RPC.ERunTarget.Gate, 0, 1);
                if (ok)
                {

                }
            }
            else
            {
                RootConnect.Disconnect();
                ret = await RootConnect.Connect("127.0.0.1", 2335, RootConnectPackages);
                if (ret == false)
                {

                }
                IndexInLevel = await EngineNS.Plugins.LevelServer.ULevelServer_RpcCaller.RegClient(sessionId, "User0", UInt16.MaxValue, 5000);
                if (RootConnect.ReturnContext.IsTimeout)
                    return false;
                TtEngine.Instance.RpcModule.DefaultExeIndex = IndexInLevel;

                var sceneId = Guid.NewGuid();
                ret = await EngineNS.Plugins.LevelServer.ULevelServer_RpcCaller.RegLevel(sceneId, RName.GetRName("test0.scene"));
                if (ret == false)
                {

                }
                SyncIdInLevel = await EngineNS.Plugins.LevelServer.ULevelServer_RpcCaller.TryClientEnterLevel(IndexInLevel, sceneId);
                if (SyncIdInLevel == uint.MaxValue)
                {

                }
            }

            var hp = await EngineNS.Plugins.LevelServer.ULevelClient_RpcCaller.GetHP(5000);
            if (RootConnect.ReturnContext.IsTimeout)
                return false;
            if (hp != 5)
                return false;

            AutoSyncData.A = 8;

            Initialized = true;
            return true;
        }
        public void Tick()
        {
            UpdateAutoSyncData2Server();
        }
        public EngineNS.Plugins.CSCommon.USyncActorManager<UGhostActor> GhostActorManager { get; } = new EngineNS.Plugins.CSCommon.USyncActorManager<UGhostActor>();
        public EngineNS.Plugins.CSCommon.UClientAutoSyncData AutoSyncData { get; } = new EngineNS.Plugins.CSCommon.UClientAutoSyncData();

        private void UpdateAutoSyncData2Server()
        {//call by tick per second
            if (AutoSyncData.IsDirty == false)
                return;
            using (var writer = EngineNS.IO.UMemWriter.CreateInstance())
            {
                var ar = new EngineNS.IO.AuxWriter<EngineNS.IO.UMemWriter>(writer);
                EngineNS.Bricks.Network.AutoSync.FSyncHelper.BuildModify(AutoSyncData, ar);

                EngineNS.Plugins.LevelServer.ULevelClient_RpcCaller.UpdateAutoSyncData(writer);
            }
        }

        #region RPC
        [URpcMethod(Index = 100 + 0)]
        public void UpdatePosition(Vector3 pos, UCallContext context)
        {

        }
        [URpcMethod(Index = 100 + 1)]
        public void GhostsAutoSync(EngineNS.IO.UMemWriter data, UCallContext context)
        {
            using (var reader = EngineNS.IO.UMemReader.CreateInstance(in data))
            {
                var ar = new EngineNS.IO.AuxReader<EngineNS.IO.UMemReader>(reader, null);
                UInt16 num = 0;
                ar.Read(out num);
                for (byte i = 0; i < num; i++)
                {
                    uint syncId;
                    ar.Read(out syncId);
                    var actor = GhostActorManager.FindSyncActor(syncId);
                    if (actor == null)
                    {
                        actor = new UGhostActor();
                        actor.SyncId = syncId;
                        GhostActorManager.RegSyncActor(actor);
                    }

                    if (syncId != SyncIdInLevel)
                    {
                        EngineNS.Bricks.Network.AutoSync.FSyncHelper.SyncValues(actor.AutoSyncData, ar, true);
                    }
                    else
                    {
                        EngineNS.Bricks.Network.AutoSync.FSyncHelper.SyncValues(AutoSyncData, ar, false);
                    }
                }
            }
        }
        [URpcMethod(Index = 100 + 2)]
        public void GhostsUpdatePlacement(EngineNS.IO.UMemWriter data, UCallContext context)
        {
            using (var reader = EngineNS.IO.UMemReader.CreateInstance(in data))
            {
                var ar = new EngineNS.IO.AuxReader<EngineNS.IO.UMemReader>(reader, null);
                UInt16 num = 0;
                ar.Read(out num);
                for (byte i = 0; i < num; i++)
                {
                    uint syncId;
                    ar.Read(out syncId);
                    var actor = GhostActorManager.FindSyncActor(syncId);
                    if (actor == null)
                    {
                        actor = new UGhostActor();
                        actor.SyncId = syncId;
                        GhostActorManager.RegSyncActor(actor);
                    }
                    FTransform transform;
                    ar.Read(out transform);
                    if (syncId != SyncIdInLevel)
                        actor.Placement.TransformData = transform;
                }
            }
        }
        #endregion
    }
}

#if TitanEngine_AutoGen
#region TitanEngine_AutoGen


namespace RobotClient
{
	partial class URobot
	{
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_UpdatePosition = (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			Vector3 pos;
			reader.Read(out pos);
			((RobotClient.URobot)host).UpdatePosition(pos, context);
		};
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_GhostsAutoSync = (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			EngineNS.IO.UMemWriter data;
			reader.Read(out data);
			((RobotClient.URobot)host).GhostsAutoSync(data, context);
		};
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_GhostsUpdatePlacement = (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			EngineNS.IO.UMemWriter data;
			reader.Read(out data);
			((RobotClient.URobot)host).GhostsUpdatePlacement(data, context);
		};
	}
}
#endregion//TitanEngine_AutoGen
#endif//TitanEngine_AutoGen