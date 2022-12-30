using System;
using System.Collections.Generic;
using EngineNS;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class GameAssemblyDesc : UAssemblyDesc
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
                #region MacrossGenerated Start
                if (name == RName.GetRName("utest/puppet/testgameplay.macross", EngineNS.RName.ERNameType.Game))
                {
                    return new NS_utest.puppet.testgameplay();
                }
                if (name == RName.GetRName("utest/pgc/testprog.macross", EngineNS.RName.ERNameType.Game))
                {
                    return new NS_utest.pgc.testprog();
                }
                if (name == RName.GetRName("utest/pgc/terrain_bz_height.macross", EngineNS.RName.ERNameType.Game))
                {
                    return new NS_utest.pgc.terrain_bz_height();
                }
                if (name == RName.GetRName("utest/pgc/terraingen.macross", EngineNS.RName.ERNameType.Game))
                {
                    return new NS_utest.pgc.terraingen();
                }
                if (name == RName.GetRName("utest/test_game01.macross", EngineNS.RName.ERNameType.Game))
                {
                    return new NS_utest.test_game01();
                }
                #endregion MacrossGenerated End
                return null;
            }
        }
        static GameAssemblyDesc AssmblyDesc = new GameAssemblyDesc();
        public static UAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

