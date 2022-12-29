using EngineNS.Plugins.RootServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class UGameServerAssemblyDesc : UAssemblyDesc
        {
            public UGameServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:GameServer AssemblyDesc Created");
            }
            ~UGameServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:GameServer AssemblyDesc Destroyed");
            }
            public override string Name { get => "GameServer"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static UGameServerAssemblyDesc AssmblyDesc = new UGameServerAssemblyDesc();
        public static UAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}


namespace EngineNS.Plugins.GameServer
{
    public class UPluginLoader
    {
        public static UGameServerPlugin? mPluginObject = new UGameServerPlugin();
        public static Bricks.AssemblyLoader.UPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public class UGameServerPlugin : Bricks.AssemblyLoader.UPlugin
    {
        public override void OnLoadedPlugin()
        {
            
        }
        public override void OnUnloadPlugin()
        {
        }
    }
}
