using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class TtDataCopyerAssemblyDesc : TtAssemblyDesc
        {
            public TtDataCopyerAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:DataCopyer AssemblyDesc Created");
            }
            ~TtDataCopyerAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:DataCopyer AssemblyDesc Destroyed");
            }
            public override string Name { get => "DataCopyer"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static TtDataCopyerAssemblyDesc AssmblyDesc = new TtDataCopyerAssemblyDesc();
        public static TtAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

namespace EngineNS.Plugins.DataCopyer
{
    public class TtPluginLoader
    {
        public static TtDataCopyerPlugin mPluginObject = new TtDataCopyerPlugin();
        public static Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public partial class TtDataCopyerPlugin : Bricks.DataCopyer.TtDataCopyer
    {
        internal Hash160 VersionHash = Hash160.Emtpy;
        public override Hash160 GetVersionHash()
        {
            return VersionHash;
        }
    }
}