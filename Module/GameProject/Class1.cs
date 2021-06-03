using System;
using System.Collections.Generic;

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
                    return new GameProject.Demo0Game();
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


namespace GameProject
{
    public class Demo0Game : EngineNS.GamePlay.IGameBase
    {
        ~Demo0Game()
        {

        }
        public override async System.Threading.Tasks.Task<bool> BeginPlay(EngineNS.GamePlay.UGameBase host)
        {
            await base.BeginPlay(host);
            return true;
        }
        public override void Tick(EngineNS.GamePlay.UGameBase host, int elapsedMillisecond)
        {
            base.Tick(host, elapsedMillisecond);
        }
        public override void BeginDestroy(EngineNS.GamePlay.UGameBase host)
        {
            base.BeginDestroy(host);
        }
    }
}
