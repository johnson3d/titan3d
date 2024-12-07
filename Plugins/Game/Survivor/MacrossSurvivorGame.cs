using System;
using System.Collections.Generic;
using System.Text;

namespace Survivor
{
    [EngineNS.Macross.TtMacross]
    public class TtMacrossSurvivorGame : EngineNS.GamePlay.TtMacrossGame
    {
        [EngineNS.Rtti.Meta]
        public TtGameMode GameMode { get; } = new TtGameMode();
    }
}
