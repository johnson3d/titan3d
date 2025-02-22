using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class SurvivorGameAssemblyDesc : TtAssemblyDesc
        {
            public SurvivorGameAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:SurvivorGame AssemblyDesc Created");
            }
            ~SurvivorGameAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:SurvivorGame AssemblyDesc Destroyed");
            }
            public override string Name { get => "Survivor"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static SurvivorGameAssemblyDesc AssmblyDesc = new SurvivorGameAssemblyDesc();
        public static TtAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

namespace Survivor
{
    [EngineNS.Bricks.AssemblyLoader.TtPlugin]
    public class TtPluginLoader
    {
        public static TtSurvivorGamePlugin mPluginObject = new TtSurvivorGamePlugin();
        public static EngineNS.Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public partial class TtSurvivorGamePlugin : EngineNS.Bricks.AssemblyLoader.IPlugin
    {
        public void OnLoadedPlugin()
        {

        }
        public void OnUnloadPlugin()
        {

        }
    }
}