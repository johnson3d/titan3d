using System;
using System.Collections.Generic;
using EngineNS.Bricks.Network.RPC;
using EngineNS.Plugins.GateServer;

namespace EngineNS.Plugins.LevelServer
{
    [URpcClass(RunTarget = ERunTarget.Level, Executer = EExecuter.Root)]
    public partial class ULevelServer : ServerCommon.UServerBase
    {
        public ULevelServer()
        {
            CurrentTarget = ERunTarget.Level;
        }
        public override object GetExecuter(in URouter router)
        {
            switch (router.Executer)
            {
                case EExecuter.Root:
                    return this;
                case EExecuter.Client:
                    {
                        return ClientManager.GetClient(router.Index);
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
            TickClients();
        }
        private void TickClients()
        {
            foreach (var i in ClientManager.Clients)
            {
                if (i == null)
                    continue;
                i.Tick();
            }
        }
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
            return client.ClientIndex;
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
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_RegClient = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
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