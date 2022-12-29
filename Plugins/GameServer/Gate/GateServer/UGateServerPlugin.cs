using EngineNS.Plugins.RootServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class UGateServerAssemblyDesc : UAssemblyDesc
        {
            public UGateServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:GateServer AssemblyDesc Created");
            }
            ~UGateServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:GateServer AssemblyDesc Destroyed");
            }
            public override string Name { get => "GateServer"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static UGateServerAssemblyDesc AssmblyDesc = new UGateServerAssemblyDesc();
        public static UAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}


namespace EngineNS.Plugins.GateServer
{
    public class UPluginLoader
    {
        public static UGateServerPlugin mPluginObject = new UGateServerPlugin();
        public static Bricks.AssemblyLoader.UPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public class UGateServerPlugin : Bricks.AssemblyLoader.UPlugin
    {
        public override void OnLoadedPlugin()
        {
            var server = new UGateServer();
            UEngine.Instance.RpcModule.RpcManager = server;
            Action action = async () =>
            {
                var np = server.SelectNetworkPoint();
                var ret = await server.StartServer(np.Ip, np.Port);
                Console.WriteLine($"GateServer start: {ret}");
            };
            action();
        }
        public override void OnUnloadPlugin()
        {
            var server = UEngine.Instance.RpcModule.RpcManager as UGateServer;
            server?.StopServer();
        }
    }
}
