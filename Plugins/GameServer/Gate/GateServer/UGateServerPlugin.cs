﻿using EngineNS.Plugins.RootServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class UGateServerAssemblyDesc : TtAssemblyDesc
        {
            public UGateServerAssemblyDesc()
            {
                Profiler.Log.WriteLine<EngineNS.Profiler.TtCookGategory>(Profiler.ELogTag.Info, "Plugins:GateServer AssemblyDesc Created");
            }
            ~UGateServerAssemblyDesc()
            {
                Profiler.Log.WriteLine<EngineNS.Profiler.TtCookGategory>(Profiler.ELogTag.Info, "Plugins:GateServer AssemblyDesc Destroyed");
            }
            public override string Name { get => "GateServer"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static UGateServerAssemblyDesc AssmblyDesc = new UGateServerAssemblyDesc();
        public static TtAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}


namespace EngineNS.Plugins.GateServer
{
    public class TtPluginLoader
    {
        public static UGateServerPlugin mPluginObject = new UGateServerPlugin();
        public static Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public class UGateServerPlugin : Bricks.AssemblyLoader.IPlugin
    {
        public void OnLoadedPlugin()
        {
            var server = new UGateServer();
            TtEngine.Instance.RpcModule.RpcManager = server;
            Action action = async () =>
            {
                var np = server.SelectNetworkPoint();
                var ret = await server.StartServer(np.Ip, np.Port);
                Console.WriteLine($"GateServer start: {ret}");
            };
            action();
        }
        public void OnUnloadPlugin()
        {
            var server = TtEngine.Instance.RpcModule.RpcManager as UGateServer;
            server?.StopServer();
        }
    }
}
