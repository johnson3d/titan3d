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
    public class GAloneNPC : EngineNS.GamePlay.Scene.Actor.TtActor
    {
    }
    [TtNode(NodeDataType = typeof(GAlonePlayerData), DefaultNamePrefix = "AlonePlayer ")]
    public class GAlonePlayer : EngineNS.GamePlay.Character.TtCharacter
    {
        public class GAlonePlayerData : TtCharacterData
        {

        }
        public GAlonePlayer()
        {

        }
        public override async EngineNS.Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;
           
            return true;
        }
    }
}
