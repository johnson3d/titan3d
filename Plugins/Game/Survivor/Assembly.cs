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
            public override string Name { get => "SurvivorGame"; }
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
    
