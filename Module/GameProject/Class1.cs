using System;
using System.Collections.Generic;
using EngineNS;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class GameAssemblyDesc : AssemblyDesc
        {
            public GameAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "GameAssemblyDesc Created");
            }
            ~GameAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "GameAssemblyDesc Destroyed");
            }
            public override string Name { get => "GameProject"; }
            public override string Service { get { return "Game"; } }
            public override bool IsGameModule { get { return true; } }
            public override string Platform { get { return "Windows"; } }

            public override object CreateInstance(RName name)
            {
                if (name.Name == "demo0.mcrs")
                {
                    return new GameProject.GAloneGame();
                }
                return null;
            }
        }
        static GameAssemblyDesc AssmblyDesc = new GameAssemblyDesc();
        public static AssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

