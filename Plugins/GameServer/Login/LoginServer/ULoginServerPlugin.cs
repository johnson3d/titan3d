using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class ULoginServerAssemblyDesc : TtAssemblyDesc
        {
            public ULoginServerAssemblyDesc()
            {
                Profiler.Log.WriteLine<EngineNS.Profiler.TtCookGategory>(Profiler.ELogTag.Info, "Plugins:LoginServer AssemblyDesc Created");
            }
            ~ULoginServerAssemblyDesc()
            {
                Profiler.Log.WriteLine<EngineNS.Profiler.TtCookGategory>(Profiler.ELogTag.Info, "Plugins:LoginServer AssemblyDesc Destroyed");
            }
            public override string Name { get => "LoginServer"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static ULoginServerAssemblyDesc AssmblyDesc = new ULoginServerAssemblyDesc();
        public static TtAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

namespace EngineNS.Plugins.LoginServer
{
    public class TtPluginLoader
    {
        public static ULoginServerPlugin mPluginObject = new ULoginServerPlugin();
        public static Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public class ULoginServerPlugin : Bricks.AssemblyLoader.IPlugin
    {
        public void OnLoadedPlugin()
        {
            var server = new ULoginServer();
            TtEngine.Instance.RpcModule.RpcManager = server;
            Action action = async () =>
            {
                var np = server.SelectNetworkPoint();
                var ret = await server.StartServer(np.Ip, np.Port);
                Console.WriteLine($"LoginServer start:{ret}");
            };
            action();
        }
        public void OnUnloadPlugin()
        {

        }
    }
}
