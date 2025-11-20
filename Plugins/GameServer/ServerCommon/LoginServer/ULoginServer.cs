using System;
using System.Collections.Generic;
using EngineNS.Bricks.Network.RPC;
using static EngineNS.Plugins.RootServer.URootServer;

namespace EngineNS.Plugins.LoginServer
{
    [TtRpcClass(RunTarget = ERunTarget.Login, Executer = EExecuter.Root)]
    public partial class ULoginServer : ServerCommon.UServerBase
    {
        protected Bricks.Network.TtNetPackageManager RootConnectPackages = new Bricks.Network.TtNetPackageManager();
        public Bricks.Network.TtTcpClient RootConnect { get; } = new Bricks.Network.TtTcpClient();
        public ULoginServer()
        {
            CurrentTarget = ERunTarget.Login;
        }
        public override IRpcHost GetExecuter(in FRouter router)
        {
            switch (router.Executer)
            {
                case EExecuter.Root:
                    return this;
            }
            return null;
        }
        public override Bricks.Network.INetConnect GetRunTargetConnect(ERunTarget target, uint index, Bricks.Network.INetConnect connect)
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

            var rpcArg = new Bricks.Network.RPC.FRpcCallArg();
            rpcArg.NetConnect = RootConnect;
            ret = await RootServer.URootServer_RpcCaller.RegLogin("TitanServer", ServerId, ip, port, rpcArg);
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
        [TtRpcMethod(Index = 100 + 0)]
        public async System.Threading.Tasks.Task<Bricks.Network.FLoginResultArgument> LoginAccount(string user, string psw, TtCallContext context)
        {
            var info = AccountManager.LoginAccount(user, psw);
            if (info == null)
                return null;

            var rpcArg = new Bricks.Network.RPC.FRpcCallArg();
            rpcArg.NetConnect = RootConnect;
            var result = new Bricks.Network.FLoginResultArgument();
            result.GatewayURL = await RootServer.URootServer_RpcCaller.SelectGateway(user, info.SessionId, rpcArg);
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
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_LoginAccount = async (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host,  EngineNS.Bricks.Network.RPC.TtCallContext context) =>
		{
			string user;
			reader.Read(out user);
			string psw;
			reader.Read(out psw);
			FReturnContext retContext;
			reader.Read(out retContext);
			var ret = await ((EngineNS.Plugins.LoginServer.ULoginServer)host).LoginAccount(user, psw, context);
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
	}
}
#endregion//TitanEngine_AutoGen
#endif//TitanEngine_AutoGen
#if TitanEngine_AutoGen_RPC
#region TitanEngine_AutoGen_RPC


namespace EngineNS.Plugins.LoginServer
{
	partial class ULoginServer
	{
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_LoginAccount = async (EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader> reader, object host,  EngineNS.Bricks.Network.RPC.TtCallContext context) =>
		{
			string user;
			reader.Read(out user);
			string psw;
			reader.Read(out psw);
			EngineNS.Bricks.Network.RPC.FReturnContext retContext;
			reader.Read(out retContext, false);
			var ret = await ((EngineNS.Plugins.LoginServer.ULoginServer)host).LoginAccount(user, psw, context);
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
		public async Thread.Async.TtTask<Bricks.Network.FLoginResultArgument> RPC_LoginAccount(string user, string psw, EngineNS.Bricks.Network.RPC.TtReturnContext retContext = null)
		{
			var rpcArg = new EngineNS.Bricks.Network.RPC.FRpcCallArg(retContext);
			rpcArg.ExeIndex = RpcExecuteIndex;
			rpcArg.NetConnect = GetRpcConnect(100 + 0);
			return await ULoginServer_RpcCaller.LoginAccount(user, psw, rpcArg);
		}
	}
}
#endregion//TitanEngine_AutoGen_RPC
#endif//TitanEngine_AutoGen_RPC