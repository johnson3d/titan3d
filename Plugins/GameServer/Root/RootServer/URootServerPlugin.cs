using EngineNS.Plugins.RootServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class URootServerAssemblyDesc : TtAssemblyDesc
        {
            public URootServerAssemblyDesc()
            {
                Profiler.Log.WriteLine<EngineNS.Profiler.TtCookGategory>(Profiler.ELogTag.Info, "Plugins:RootServer AssemblyDesc Created");
            }
            ~URootServerAssemblyDesc()
            {
                Profiler.Log.WriteLine<EngineNS.Profiler.TtCookGategory>(Profiler.ELogTag.Info, "Plugins:RootServer AssemblyDesc Destroyed");
            }
            public override string Name { get => "RootServer"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static URootServerAssemblyDesc AssmblyDesc = new URootServerAssemblyDesc();
        public static TtAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}


namespace EngineNS.Plugins.RootServer
{
    public class TtPluginLoader
    {
        public static URootServerPlugin mPluginObject = new URootServerPlugin();
        public static Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public class URootServerPlugin : Bricks.AssemblyLoader.IPlugin
    {
        public void OnLoadedPlugin()
        {
            var server = new URootServer();
            TtEngine.Instance.RpcModule.RpcManager = server;
            Action action = async () =>
            {
                var np = Bricks.Network.FNetworkPoint.FromString(TtEngine.Instance.Config.RootServerURL);
                var ret = await server.StartServer(np.Ip, np.Port);
                Console.WriteLine($"RootServer start;{ret}");
            };
            action();
        }
        public void OnUnloadPlugin()
        {
            var server = TtEngine.Instance.RpcModule.RpcManager as URootServer;
            server?.StopServer();
        }
    }
}
