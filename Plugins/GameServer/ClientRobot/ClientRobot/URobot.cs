using Microsoft.Build.Framework.Profiler;
using System;
using System.Collections.Generic;
using EngineNS.Bricks.Network.RPC;
using EngineNS;

namespace RobotClient
{
    [URpcClass(RunTarget = ERunTarget.Client, Executer = EExecuter.Root)]
    partial class URobot : EngineNS.Bricks.Network.RPC.URpcManager, EngineNS.ITickable
    {
        public URobot()
        {
            this.CurrentTarget = ERunTarget.Client;
        }
        #region tickable
        public void TickLogic(int ellapse)
        {
            RootConnectPackages.Tick();
        }
        public void TickRender(int ellapse)
        {

        }
        public void TickSync(int ellapse)
        {

        }
        #endregion
        #region Connect
        public UInt16 IndexInGate { get; set; } = UInt16.MaxValue;
        public UInt16 IndexInLevel { get; set; } = UInt16.MaxValue;
        protected EngineNS.Bricks.Network.UNetPackageManager RootConnectPackages = new EngineNS.Bricks.Network.UNetPackageManager();
        public EngineNS.Bricks.Network.UTcpClient RootConnect { get; } = new EngineNS.Bricks.Network.UTcpClient();
        #endregion
        public virtual async System.Threading.Tasks.Task<bool> Initialize()
        {
            EngineNS.UEngine.Instance.RpcModule.RpcManager = this;
            Guid sessionId = Guid.NewGuid();
            EngineNS.UEngine.Instance.RpcModule.DefaultNetConnect = RootConnect;
            var ret = await RootConnect.Connect("127.0.0.1", 2334, RootConnectPackages);
            if (ret)
            {
                var lnk = await EngineNS.Plugins.LoginServer.ULoginServer_RpcCaller.LoginAccount("User0", "god");
                if (EngineNS.Bricks.Network.RPC.URpcAwaiter.IsTimeout)
                {

                }
                RootConnect.Disconnect();
                if (lnk == null)
                {
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "RPC", "Gateway is invalid");
                }
                var np = lnk.GatewayURL;
                ret = await RootConnect.Connect(np.Ip, np.Port, RootConnectPackages);
                sessionId = lnk.Sessiond;

                IndexInGate = await EngineNS.Plugins.GateServer.UGateServer_RpcCaller.RegClient(sessionId, "User0");
                UEngine.Instance.RpcModule.DefaultExeIndex = IndexInGate;

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
                if (EngineNS.Bricks.Network.RPC.URpcAwaiter.IsTimeout)
                    return false;
                UEngine.Instance.RpcModule.DefaultExeIndex = IndexInLevel;
            }

            var hp = await EngineNS.Plugins.LevelServer.ULevelClient_RpcCaller.GetHP(5000);
            if (EngineNS.Bricks.Network.RPC.URpcAwaiter.IsTimeout)
                return false;
            if (hp != 5)
                return false;
            return true;
        }

        #region RPC
        [URpcMethod(Index = 100 + 0)]
        public void UpdatePosition(Vector3 pos, UCallContext context)
        {

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
		public static EngineNS.Bricks.Network.RPC.FCallMethod rpc_UpdatePosition = (EngineNS.IO.AuxReader<UMemReader> reader, object host, EngineNS.Bricks.Network.RPC.UCallContext context) =>
		{
			Vector3 pos;
			reader.Read(out pos);
			((RobotClient.URobot)host).UpdatePosition(pos, context);
		};
	}
}
#endregion//TitanEngine_AutoGen
#endif//TitanEngine_AutoGen