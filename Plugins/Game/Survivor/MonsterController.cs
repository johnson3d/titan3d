using EngineNS;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Character;
using EngineNS.GamePlay.Controller;
using EngineNS.GamePlay.Movemnet;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.Text;

namespace Survivor
{
    public class TtMonsterController : TtAIController
    {
        public class TtMonsterControllerData : TtNodeData
        {

        }
        public TtCharacter Player { get; set; } = null;
        public TtMonsterNode MonsterNode { get; set; } = null;

        public override TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            return base.InitializeNode(world, data, bvType, placementType);
        }
        
        public override bool OnTickLogic(TtWorld world, TtRenderPolicy policy)
        {
            if(Player != null && MonsterNode != null)
            {
                float distance = Vector3.Distance(Player.Placement.AbsTransform.Position.ToSingleVector3(),
                                            MonsterNode.MonsterPrefab.Placement.AbsTransform.Position.ToSingleVector3());
                if (distance > MonsterNode.MonsterData.AttackRange)
                {
                    //move
                    var dir = Player.Placement.AbsTransform.Position.ToSingleVector3() -
                                                MonsterNode.MonsterPrefab.Placement.AbsTransform.Position.ToSingleVector3();
                    dir.Normalize();
                    
                    var pos = MonsterNode.MonsterPrefab.Placement.Position + dir * MonsterNode.MonsterData.Speed * world.DeltaTimeSecond;
                    var simpleMovement = MonsterNode.MonsterPrefab.FindFirstChild<TtSimpleMovement>(null, true) as TtSimpleMovement;
                    simpleMovement.SetDesiredPosition(pos.ToSingleVector3());
                    MonsterNode.MonsterPrefab.Placement.Quat = Quaternion.RotationFrowTwoVector(Vector3.Forward, -dir);
                }
                else
                {
                    //attack
                }
            }
            
            return base.OnTickLogic(world, policy);
        }

    }
}
