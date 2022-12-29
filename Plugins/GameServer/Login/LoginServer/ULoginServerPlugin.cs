using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class ULoginServerAssemblyDesc : UAssemblyDesc
        {
            public ULoginServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:LoginServer AssemblyDesc Created");
            }
            ~ULoginServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:LoginServer AssemblyDesc Destroyed");
            }
            public override string Name { get => "LoginServer"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static ULoginServerAssemblyDesc AssmblyDesc = new ULoginServerAssemblyDesc();
        public static UAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

namespace EngineNS.Plugins.LoginServer
{
    public class UPluginLoader
    {
        public static ULoginServerPlugin mPluginObject = new ULoginServerPlugin();
        public static Bricks.AssemblyLoader.UPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public class ULoginServerPlugin : Bricks.AssemblyLoader.UPlugin
    {
        public override void OnLoadedPlugin()
        {
            var server = new ULoginServer();
            UEngine.Instance.RpcModule.RpcManager = server;
            Action action = async () =>
            {
                var np = server.SelectNetworkPoint();
                var ret = await server.StartServer(np.Ip, np.Port);
                Console.WriteLine($"LoginServer start:{ret}");
            };
            action();
        }
        public override void OnUnloadPlugin()
        {

        }
    }
}
