using EngineNS.Plugins.RootServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class URpcCallerAssemblyDesc : UAssemblyDesc
        {
            public URpcCallerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:RpcCaller AssemblyDesc Created");
            }
            ~URpcCallerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:RpcCaller AssemblyDesc Destroyed");
            }
            public override string Name { get => "RpcCaller"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static URpcCallerAssemblyDesc AssmblyDesc = new URpcCallerAssemblyDesc();
        public static UAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}


namespace EngineNS.Plugins.RpcCaller
{
    public class UPluginLoader
    {
        public static URpcCallerPlugin mPluginObject = new URpcCallerPlugin();
        public static Bricks.AssemblyLoader.UPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public class URpcCallerPlugin : Bricks.AssemblyLoader.UPlugin
    {
        public override void OnLoadedPlugin()
        {
            
        }
        public override void OnUnloadPlugin()
        {
            
        }
    }
}
