using System;
using System.Collections.Generic;
using EngineNS.Bricks.Network.RPC;
using EngineNS.Plugins.GateServer;

namespace EngineNS.Plugins.LevelServer
{
    [URpcClass(RunTarget = ERunTarget.Level, Executer = EExecuter.Root)]
    public partial class ULevelServer : ServerCommon.UServerBase
    {
        static ULevelServer mInstance = null;
        public static ULevelServer Instance { get => mInstance; }
        public UClientAllConnects ClientAllConnects = null;
        public ULevelServer()
        {
            CurrentTarget = ERunTarget.Level;
            System.Diagnostics.Debug.Assert(mInstance == null);
            mInstance = this;
            ClientAllConnects = new UClientAllConnects();
            ClientAllConnects.ClientManager = ClientManager;
        }
        public override object GetExecuter(in URouter router)
        {
            switch (router.Executer)
            {
                case EExecuter.Root:
                    return this;
                case EExecuter.Client:
                    {
                        return ClientManager.GetClient<ULevelClient>(router.Index);
                    }
            }
            return null;
        }
        public override Bricks.Network.INetConnect GetRunTargetConnect(ERunTarget target, UInt16 index, Bricks.Network.INetConnect connect)
        {
            switch (target)
            {
                case ERunTarget.Root:
                    return RootConnect;
            }

            return null;
        }
        public override Bricks.Network.FNetworkPoint SelectNetworkPoint()
        {
            //base.SelectNetworkPoint();
            var result = new Bricks.Network.FNetworkPoint();
            result.Ip = "127.0.0.1";
            result.Port = 2335;
            return result;
        }
        public override async System.Threading.Tasks.Task<bool> StartServer(string ip, UInt16 port)
        {
            ClientManager = new ULevelClientManager();
            ClientAllConnects.ClientManager = ClientManager;

            var ret = await base.StartServer(ip, port);
            if (ret == false)
                return false;

            base.TcpServer.OnConnectAction = (action, connect) =>
            {//level up authority for debug
                if (action == "OnAccept")
                {
                    connect.Authority = UEngine.Instance.Config.DefaultAuthority;
                }
            };

            var np = Bricks.Network.FNetworkPoint.FromString(UEngine.Instance.Config.RootServerURL);
            ret = await RootConnect.Connect(np.Ip, np.Port, RootConnectPackages);
            if (ret == false)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "RPC", $"LevelServer connect root failed:{np}");
                return true;
            }
            else
            {
                IndexInRoot = await RootServer.URootServer_RpcCaller.RegLevel("TitanServer", ServerId, ip, port, uint.MaxValue, ushort.MaxValue, RootConnect);
                if (IndexInRoot == UInt16.MaxValue)
                    return false;
                return true;
            }
        }
        public override void StopServer()
        {
            base.StopServer();
            RootConnect.Disconnect();
        }
        public override void Tick()
        {
            RootConnectPackages.Tick();
            base.Tick();

            LevelManager.Tick();
        }
        public ULevelManager LevelManager { get; } = new ULevelManager();

        #region Connect
        public UInt16 IndexInRoot { get; private set; } = UInt16.MaxValue;
        protected Bricks.Network.UNetPackageManager RootConnectPackages = new Bricks.Network.UNetPackageManager();
        public Bricks.Network.UTcpClient RootConnect { get; } = new Bricks.Network.UTcpClient();
        #endregion

        #region RPC
        [URpcMethod(Index = 100 + 0, Authority = EAuthority.Gateway)]
        public UInt16 RegClient(Guid sessionId, string user, UInt16 indexInGate, UCallContext context)
        {
            var client = ClientManager.RegClient<ULevelClient>(in sessionId) as ULevelClient;
            client.SessionId = sessionId;
            client.UserName = user;
            client.ClientConnect = context.NetConnect;
            client.IndexInGame = indexInGate;
            client.AutoSyncData.IsGhostSyncObject = true;
            return client.ClientIndex;
        }
        [URpcMethod(Index = 100 + 1, Authority = EAuthority.Server)]
        public bool RegLevel(Guid id, RName name, UCallContext context)
        {
            var level = new ULevel();
            level.AssetName = name;
            return LevelManager.RegLevel(in id, level);
        }
        [URpcMethod(Index = 100 + 2, Authority = EAuthority.Server)]
        public uint TryClientEnterLevel(UInt16 clientIndex, Guid levelId, UCallContext context)
        {
            var clt = ClientManager.GetClient<ULevelClient>(clientIndex);
            if (clt == null)
                return uint.MaxValue;
            var level = LevelManager.FindLevel(in levelId);
            if (level == null)
                return uint.MaxValue;
            return level.EnterActor(clt, CSCommon.ESyncIdType.Dynamic);
        }
        [URpcMethod(Index = 100 + 3, Authority = EAuthority.Server)]
        public bool TryClientLeaveLevel(UInt16 clientIndex, Guid levelId, UCallContext context)
        {
            var clt = ClientManager.GetClient<ULevelClient>(clientIndex);
            if (clt == null)
                return false;
            var level = LevelManager.FindLevel(in levelId);
            if (level == null)
                return false;
            return level.LeaveActor(clt, CSCommon.ESyncIdType.Dynamic);
        }
        #endregion
    }
}
#if TitanEngine_AutoGen
#region TitanEngine_AutoGen


namespace EngineNS.Plugins.LevelServer
{
	partial class ULevelServer
	{
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_RegClient = (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			Guid sessionId;
			reader.Read(out sessionId);
			string user;
			reader.Read(out user);
			UInt16 indexInGate;
			reader.Read(out indexInGate);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Plugins.LevelServer.ULevelServer)host).RegClient(sessionId, user, indexInGate, context);
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
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_RegLevel = (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			Guid id;
			reader.Read(out id);
			RName name;
			reader.Read(out name);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Plugins.LevelServer.ULevelServer)host).RegLevel(id, name, context);
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
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_TryClientEnterLevel = (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			UInt16 clientIndex;
			reader.Read(out clientIndex);
			Guid levelId;
			reader.Read(out levelId);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Plugins.LevelServer.ULevelServer)host).TryClientEnterLevel(clientIndex, levelId, context);
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
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_TryClientLeaveLevel = (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			UInt16 clientIndex;
			reader.Read(out clientIndex);
			Guid levelId;
			reader.Read(out levelId);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Plugins.LevelServer.ULevelServer)host).TryClientLeaveLevel(clientIndex, levelId, context);
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
	}
}
#endregion//TitanEngine_AutoGen
#endif//TitanEngine_AutoGen