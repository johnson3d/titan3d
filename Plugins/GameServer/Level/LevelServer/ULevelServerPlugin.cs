using EngineNS.Plugins.RootServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class ULevelServerAssemblyDesc : UAssemblyDesc
        {
            public ULevelServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:LevelServer AssemblyDesc Created");
            }
            ~ULevelServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:LevelServer AssemblyDesc Destroyed");
            }
            public override string Name { get => "LevelServer"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static ULevelServerAssemblyDesc AssmblyDesc = new ULevelServerAssemblyDesc();
        public static UAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}


namespace EngineNS.Plugins.LevelServer
{
    public class UPluginLoader
    {
        public static ULevelServerPlugin mPluginObject = new ULevelServerPlugin();
        public static Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public class ULevelServerPlugin : Bricks.AssemblyLoader.IPlugin
    {
        public void OnLoadedPlugin()
        {
            var server = new ULevelServer();
            UEngine.Instance.RpcModule.RpcManager = server;
            Action action = async () =>
            {
                var np = server.SelectNetworkPoint();
                var ret = await server.StartServer(np.Ip, np.Port);
                Console.WriteLine($"LevelServer start:{ret}");
            };
            action();
        }
        public void OnUnloadPlugin()
        {
            var server = UEngine.Instance.RpcModule.RpcManager as ULevelServer;
            server?.StopServer();
        }
    }
}
