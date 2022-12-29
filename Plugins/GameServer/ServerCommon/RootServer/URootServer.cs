using System;
using System.Collections.Generic;
using EngineNS.Bricks.Network.RPC;

namespace EngineNS.Plugins.RootServer
{
    [URpcClass(RunTarget = ERunTarget.Root, Executer = EExecuter.Root)]
    public partial class URootServer : ServerCommon.UServerBase
    {
        public URootServer()
        {
            CurrentTarget = ERunTarget.Root;
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
                case ERunTarget.Login:
                    return LoginServer?.Connect;
                case ERunTarget.Gate:
                    return GateServers[index]?.Connect;
            }

            return null;
        }
        public ServerCommon.UServerBase LoginServer { get; set; }
        public ServerCommon.UServerBase[] GateServers { get; } = new ServerCommon.UServerBase[UInt16.MaxValue];
        public ServerCommon.UServerBase[] LevelServers { get; } = new ServerCommon.UServerBase[UInt16.MaxValue];
        public ServerCommon.UServerBase FindGate(in Guid serverId)
		{
			foreach(var i in GateServers)
			{
				if (i == null)
					continue;
				if (i.ServerId == serverId)
					return i;
			}
			return null;
		}
        public ServerCommon.UServerBase FindLevel(in Guid serverId)
        {
            foreach (var i in LevelServers)
            {
                if (i == null)
                    continue;
                if (i.ServerId == serverId)
                    return i;
            }
            return null;
        }
        #region RPC
        [URpcMethod(Index = 100 + 0)]
        public bool RegLogin(string psw, Guid serverId, string ip, UInt16 port, UCallContext context)
        {
            if (psw != "TitanServer")
            {
				return false;
			}
            if (LoginServer != null)
                return false;
            LoginServer = new ServerCommon.UServerBase();
            LoginServer.Connect = context.NetConnect;
            LoginServer.ServerId = serverId;
			LoginServer.ListenPoint.Ip = ip;
			LoginServer.ListenPoint.Port = port;
            return true;
        }
        [URpcMethod(Index = 100 + 1)]
        public UInt16 RegGate(string psw, Guid serverId, string ip, UInt16 port, UCallContext context)
        {
            for (int i = 0; i < UInt16.MaxValue; i++)
            {
                if (GateServers[i] != null && GateServers[i].ServerId == serverId)
                {
                    GateServers[i].Connect = context.NetConnect;
                    return (UInt16)i;
                }
            }

            for (int i = 0; i < UInt16.MaxValue; i++)
            {
                if (GateServers[i] == null)
                {
                    var tmp = new ServerCommon.UServerBase();
                    tmp.Connect = context.NetConnect;
                    tmp.ServerId = serverId;
                    tmp.ListenPoint.Ip = ip;
                    tmp.ListenPoint.Port = port;
                    GateServers[i] = tmp;
                    return (UInt16)i;
                }
            }
            
            return UInt16.MaxValue;
        }
        [URpcMethod(Index = 100 + 2)]
        public UInt16 RegLevel(string psw, Guid serverId, string ip, UInt16 port, UCallContext context)
        {
            for (int i = 0; i < UInt16.MaxValue; i++)
            {
                if (LevelServers[i] != null && LevelServers[i].ServerId == serverId)
                {
                    GateServers[i].Connect = context.NetConnect;
                    return (UInt16)i;
                }
            }

            for (int i = 0; i < UInt16.MaxValue; i++)
            {
                if (LevelServers[i] == null)
                {
                    var tmp = new ServerCommon.UServerBase();
                    tmp.Connect = context.NetConnect;
                    tmp.ServerId = serverId;
                    tmp.ListenPoint.Ip = ip;
                    tmp.ListenPoint.Port = port;
                    LevelServers[i] = tmp;
                    return (UInt16)i;
                }
            }

            return UInt16.MaxValue;
        }
        [URpcMethod(Index = 100 + 3)]
		public async System.Threading.Tasks.Task<Bricks.Network.FNetworkPoint> SelectGateway(string user, Guid sessionId, UCallContext context)
		{
			ServerCommon.UServerBase slt = null;
			long Payload = long.MaxValue;
            foreach (var i in GateServers)
			{
				if (i == null)
					continue;
				var tmp = i.ClientManager.FindClient(sessionId);
                if (tmp == null)
                {
					slt = i;
					break;
				}
                if (i.Payload < Payload)
                {
                    Payload = i.Payload;
					slt = i;
                }
            }
			if (slt == null)
			{
				return null;
			}
			var ok = await GateServer.UGateServer_RpcCaller.WaitSession(sessionId, user, uint.MaxValue, ushort.MaxValue, slt.Connect);
			if (ok == false)
                return null;
            return slt.ListenPoint;
		}
        [URpcMethod(Index = 100 + 4)]
		public bool UpdatePayload(ERunTarget target, UInt16 index, long value, UCallContext context)
		{
            switch (target)
            {
                case ERunTarget.Login:
                    LoginServer.Payload = value;
					break;
                case ERunTarget.Gate:
                    GateServers[index].Payload = value;
					break;
            }
			return true;
        }
        [URpcMethod(Index = 100 + 5)]
        public bool RegClient(Guid gateId, Guid sessionId, string user, UCallContext context)
		{
			var server = FindGate(gateId);
			if (server == null)
				return false;

			var clt = server.ClientManager.RegClient<ServerCommon.UClient>(in sessionId);
            clt.UserName = user;
            return true;
		}
        #endregion
    }
}


#if TitanEngine_AutoGen
#region TitanEngine_AutoGen


namespace EngineNS.Plugins.RootServer
{
	partial class URootServer
	{
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_RegLogin = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			string psw;
			reader.Read(out psw);
			Guid serverId;
			reader.Read(out serverId);
			string ip;
			reader.Read(out ip);
			UInt16 port;
			reader.Read(out port);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Plugins.RootServer.URootServer)host).RegLogin(psw, serverId, ip, port, context);
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
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_RegGate = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			string psw;
			reader.Read(out psw);
			Guid serverId;
			reader.Read(out serverId);
			string ip;
			reader.Read(out ip);
			UInt16 port;
			reader.Read(out port);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Plugins.RootServer.URootServer)host).RegGate(psw, serverId, ip, port, context);
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
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_RegLevel = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			string psw;
			reader.Read(out psw);
			Guid serverId;
			reader.Read(out serverId);
			string ip;
			reader.Read(out ip);
			UInt16 port;
			reader.Read(out port);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Plugins.RootServer.URootServer)host).RegLevel(psw, serverId, ip, port, context);
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
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_SelectGateway = async (EngineNS.IO.AuxReader<UMemReader> reader, object host,  EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			string user;
			reader.Read(out user);
			Guid sessionId;
			reader.Read(out sessionId);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = await ((EngineNS.Plugins.RootServer.URootServer)host).SelectGateway(user, sessionId, context);
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
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_UpdatePayload = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			ERunTarget target;
			reader.Read(out target);
			UInt16 index;
			reader.Read(out index);
			long value;
			reader.Read(out value);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Plugins.RootServer.URootServer)host).UpdatePayload(target, index, value, context);
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
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_RegClient = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			Guid gateId;
			reader.Read(out gateId);
			Guid sessionId;
			reader.Read(out sessionId);
			string user;
			reader.Read(out user);
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Plugins.RootServer.URootServer)host).RegClient(gateId, sessionId, user, context);
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