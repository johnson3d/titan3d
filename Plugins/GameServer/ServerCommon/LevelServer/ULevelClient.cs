using System;
using System.Collections.Generic;
using EngineNS.Bricks.Network.RPC;
using EngineNS.Plugins.GateServer;
using EngineNS.Plugins.ServerCommon;

namespace EngineNS.Plugins.LevelServer
{
    [URpcClass(RunTarget = ERunTarget.Level, Executer = EExecuter.Client)]
    public partial class ULevelClient : ServerCommon.UClient, IRpcHost
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

        public Bricks.Network.INetConnect ClientConnect { get; set; }
		public UInt16 IndexInGame { get; set; } = UInt16.MaxValue;//IndexInGate

        public override void Tick()
		{
            RobotClient.URobot_RpcCaller.UpdatePosition(Vector3.One, this.IndexInGame, ClientConnect);
		}

        #region RPC
        [URpcMethod(Index = 100 + 0)]
        public int GetHP(UCallContext context)
        {
            return 5;
        }
        #endregion
    }
}
#if TitanEngine_AutoGen
#region TitanEngine_AutoGen


namespace EngineNS.Plugins.LevelServer
{
	partial class ULevelClient
	{
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_GetHP = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			UReturnContext retContext;
			reader.Read(out retContext);
			var ret = ((EngineNS.Plugins.LevelServer.ULevelClient)host).GetHP(context);
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