using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class InventoryAssemblyDesc : TtAssemblyDesc
        {
            public InventoryAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:Inventory AssemblyDesc Created");
            }
            ~InventoryAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:Inventory AssemblyDesc Destroyed");
            }
            public override string Name { get => "Inventory"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static InventoryAssemblyDesc AssmblyDesc = new InventoryAssemblyDesc();
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
        public static TtInventoryPlugin mPluginObject = new TtInventoryPlugin();
        public static EngineNS.Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public partial class TtInventoryPlugin : EngineNS.Bricks.AssemblyLoader.IPlugin
    {
        public void OnLoadedPlugin()
        {

        }
        public void OnUnloadPlugin()
        {

        }
    }
}