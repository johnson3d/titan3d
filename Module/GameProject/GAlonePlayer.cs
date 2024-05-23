using EngineNS;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Character;
using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameProject
{
    public class GAloneNPC : EngineNS.GamePlay.Scene.Actor.UActor
    {
    }
    public class GAlonePlayer : EngineNS.GamePlay.Character.UCharacter
    {
        public GAlonePlayer()
        {

        }
        public override async EngineNS.Thread.Async.TtTask<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;
           
            return true;
        }
    }
}
