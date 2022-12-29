using EngineNS.Plugins.RootServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class URootServerAssemblyDesc : UAssemblyDesc
        {
            public URootServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:RootServer AssemblyDesc Created");
            }
            ~URootServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:RootServer AssemblyDesc Destroyed");
            }
            public override string Name { get => "RootServer"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static URootServerAssemblyDesc AssmblyDesc = new URootServerAssemblyDesc();
        public static UAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}


namespace EngineNS.Plugins.RootServer
{
    public class UPluginLoader
    {
        public static URootServerPlugin mPluginObject = new URootServerPlugin();
        public static Bricks.AssemblyLoader.UPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public class URootServerPlugin : Bricks.AssemblyLoader.UPlugin
    {
        public override void OnLoadedPlugin()
        {
            var server = new URootServer();
            UEngine.Instance.RpcModule.RpcManager = server;
            Action action = async () =>
            {
                var np = Bricks.Network.FNetworkPoint.FromString(UEngine.Instance.Config.RootServerURL);
                var ret = await server.StartServer(np.Ip, np.Port);
                Console.WriteLine($"RootServer start;{ret}");
            };
            action();
        }
        public override void OnUnloadPlugin()
        {
            var server = UEngine.Instance.RpcModule.RpcManager as URootServer;
            server?.StopServer();
        }
    }
}
