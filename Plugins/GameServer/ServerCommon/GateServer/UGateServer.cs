using System;
using System.Collections.Generic;
using EngineNS.Bricks.Network.RPC;
using EngineNS.Plugins.ServerCommon;

namespace EngineNS.Plugins.GateServer
{
    [URpcClass(RunTarget = ERunTarget.Gate, Executer = EExecuter.Root)]
    public partial class UGateServer : ServerCommon.UServerBase
    {
        public UGateServer()
        {
            CurrentTarget = ERunTarget.Gate;
        }
        public override object GetExecuter(in URouter router)
        {
            switch (router.Executer)
            {
                case EExecuter.Root:
                    return this;
            }
            return null;
        }
        public override Bricks.Network.INetConnect GetRunTargetConnect(ERunTarget target, UInt16 index, Bricks.Network.INetConnect connect)
        {
            switch (target)
            {
                case ERunTarget.Root:
                    return RootConnect;
				case ERunTarget.Level:
					{
                        var client = connect.Tag as UGateClient;
						if (client != null)
							return client.LevelConnect;
                    }
					break;
                case ERunTarget.Client:
                    {
						var client = this.ClientManager.GetClient<UGateClient>(index);
                        if (client != null)
                            return client.ClientConnect;
                    }
                    break;
            }

            return null;
        }
        public unsafe override bool OnRelay(Bricks.Network.RPC.URouter* pRouter, Bricks.Network.INetConnect connect)
        {
            var client = connect.Tag as UGateClient;
            if (client != null)
            {
                pRouter->Authority = connect.Authority;
                switch (pRouter->RunTarget)
                {
                    case ERunTarget.Root:
                        {
                            if (pRouter->Executer == EExecuter.Client)
                            {
                                if (client.IndexInRoot == UInt16.MaxValue)
                                {
                                    return false;
                                }
                                pRouter->Index = client.IndexInRoot;
                                return true;
                            }
                            else if (pRouter->Executer == EExecuter.Root)
                            {
                                //dangrous!!!
                                return true;
                            }
                        }
                        break;
                    case ERunTarget.Level:
                        {
                            if (pRouter->Executer == EExecuter.Client)
                            {
                                if (client.IndexInLevel == UInt16.MaxValue)
                                {
                                    return false;
                                }
                                pRouter->Index = client.IndexInLevel;
                                return true;
                            }
                        }
                        break;
                }
            }
            return false;
        }
        public override async System.Threading.Tasks.Task<bool> StartServer(string ip, UInt16 port)
        {
            var np = Bricks.Network.FNetworkPoint.FromString(TtEngine.Instance.Config.RootServerURL);
            var ret = await RootConnect.Connect(np.Ip, np.Port, RootConnectPackages);
            if (ret == false)
                return false;

            ret = await base.StartServer(ip, port);
            if (ret == false)
                return false;

            var index = await RootServer.URootServer_RpcCaller.RegGate("TitanServer", ServerId, ip, port, uint.MaxValue, ushort.MaxValue, RootConnect);
            if (index == UInt16.MaxValue)
                return false;
			IndexInRoot = index;
            return true;
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
        }
		#region Connect
		public UInt16 IndexInRoot { get; private set; } = UInt16.MaxValue;
        protected Bricks.Network.UNetPackageManager RootConnectPackages = new Bricks.Network.UNetPackageManager();
        public Bricks.Network.UTcpClient RootConnect { get; } = new Bricks.Network.UTcpClient();
        #endregion

        #region RPC
        [URpcMethod(Index = 100 + 0)]
        public bool WaitSession(Guid sessionId, string user, UCallContext context)
        {
			var client = ClientManager.FindClient(sessionId);
			if (client != null)
				return false;
            WaitSessions[user] = new KeyValuePair<Guid, System.DateTime>(sessionId, System.DateTime.UtcNow);
			//todo: wait session timeout
            return true;
        }
        Dictionary<string, KeyValuePair<Guid,System.DateTime>> WaitSessions = new Dictionary<string, KeyValuePair<Guid, System.DateTime>>();
        [URpcMethod(Index = 100 + 1)]
        public UInt16 RegClient(Guid sessionId, string user, UCallContext context)
        {
            KeyValuePair<Guid, System.DateTime> id;
            if (WaitSessions.TryGetValue(user, out id) == false)
            {
				return UInt16.MaxValue;// "User not find";
			}
            if (id.Key != sessionId)
                return UInt16.MaxValue;// "SessionId is invalid";
            WaitSessions.Remove(user);

			var client = ClientManager.RegClient<UGateClient>(in sessionId) as UGateClient;
			client.ClientConnect = context.NetConnect;
            client.LoginTime = System.DateTime.UtcNow;
			client.ClientConnect.Authority = EAuthority.Client;
			client.UserName = user;
			context.NetConnect.Tag = client;

			//todo: ULevelServer.RegClient
			return client.ClientIndex;// "OK";
        }
        [URpcMethod(Index = 100 + 2, Authority = EAuthority.Server)]
        public UInt16 ClientEnterLevel(Guid sessionId, string user, UInt16 indexInLevel, UCallContext context)
		{
            var client = ClientManager.FindClient(in sessionId) as UGateClient;
            if (client == null)
                return UInt16.MaxValue;

            client.LevelConnect = context.NetConnect;
			client.IndexInLevel = indexInLevel;

            return indexInLevel;
        }
        #endregion
    }
}
#if TitanEngine_AutoGen
#region TitanEngine_AutoGen


namespace EngineNS.Plugins.GateServer
{
	partial class UGateServer
	{
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_WaitSession = (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			Guid sessionId;
			reader.Read(out sessionId);
			string user;
			reader.Read(out user);
			FReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Plugins.GateServer.UGateServer)host).WaitSession(sessionId, user, context);
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
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_RegClient = (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			Guid sessionId;
			reader.Read(out sessionId);
			string user;
			reader.Read(out user);
			FReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Plugins.GateServer.UGateServer)host).RegClient(sessionId, user, context);
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
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_ClientEnterLevel = (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			Guid sessionId;
			reader.Read(out sessionId);
			string user;
			reader.Read(out user);
			UInt16 indexInLevel;
			reader.Read(out indexInLevel);
			FReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Plugins.GateServer.UGateServer)host).ClientEnterLevel(sessionId, user, indexInLevel, context);
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