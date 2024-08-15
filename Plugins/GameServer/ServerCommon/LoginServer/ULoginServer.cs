using System;
using System.Collections.Generic;
using EngineNS.Bricks.Network.RPC;
using static EngineNS.Plugins.RootServer.URootServer;

namespace EngineNS.Plugins.LoginServer
{
    [URpcClass(RunTarget = ERunTarget.Login, Executer = EExecuter.Root)]
    public partial class ULoginServer : ServerCommon.UServerBase
    {
        protected Bricks.Network.UNetPackageManager RootConnectPackages = new Bricks.Network.UNetPackageManager();
        public Bricks.Network.UTcpClient RootConnect { get; } = new Bricks.Network.UTcpClient();
        public ULoginServer()
        {
            CurrentTarget = ERunTarget.Login;
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
            }

            return null;
        }
        public override Bricks.Network.FNetworkPoint SelectNetworkPoint()
        {
            //base.SelectNetworkPoint();
            var result = new Bricks.Network.FNetworkPoint();
            result.Ip = "127.0.0.1";
            result.Port = 2334;
            return result;
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

            ret = await RootServer.URootServer_RpcCaller.RegLogin("TitanServer", ServerId, ip, port, uint.MaxValue, ushort.MaxValue, RootConnect);
            if (ret == false)
                return false;
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
        public UAccountManager AccountManager { get; } = new UAccountManager();
        #region RPC
        [URpcMethod(Index = 100 + 0)]
        public async System.Threading.Tasks.Task<Bricks.Network.FLoginResultArgument> LoginAccount(string user, string psw, UCallContext context)
        {
            var info = AccountManager.LoginAccount(user, psw);
            if (info == null)
                return null;

            var result = new Bricks.Network.FLoginResultArgument();
            result.GatewayURL = await RootServer.URootServer_RpcCaller.SelectGateway(user, info.SessionId, uint.MaxValue, ushort.MaxValue, RootConnect);
            if (result.GatewayURL == null)
            {
                return null;
            }
            result.Sessiond = info.SessionId;
            return result;
        }
        #endregion
    }
}
#if TitanEngine_AutoGen
#region TitanEngine_AutoGen


namespace EngineNS.Plugins.LoginServer
{
	partial class ULoginServer
	{
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_LoginAccount = async (EngineNS.IO.AuxReader<EngineNS.IO.UMemReader> reader, object host,  EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			string user;
			reader.Read(out user);
			string psw;
			reader.Read(out psw);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = await ((EngineNS.Plugins.LoginServer.ULoginServer)host).LoginAccount(user, psw, context);
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