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

        protected override TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            return base.InitializeNode(world, data, bvType, placementType);
        }
        public override EngineNS.Profiler.TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtMonsterController>.Scope;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            if(Player != null && MonsterNode != null && !MonsterNode.StateNode.IsDead)
            {
                float distance = (float)DVector3.Distance(Player.Placement.AbsTransform.Position,
                                            MonsterNode.MonseterPlacement.AbsTransform.Position);
                if (distance > MonsterNode.MonsterData.AttackRange)
                {
                    //move
                    var dir = Player.Placement.AbsTransform.Position -
                                                MonsterNode.MonseterPlacement.AbsTransform.Position;
                    dir.Normalize();
                    dir.Y = 0;
                    var pos = MonsterNode.MonseterPlacement.Position + dir * MonsterNode.MonsterData.Speed * args.World.DeltaTimeSecond;
                    var simpleMovement = MonsterNode.MonsterPrefab.FindFirstChild<TtSimpleMovement>(null, true) as TtSimpleMovement;
                    simpleMovement.SetDesiredPosition(pos.ToSingleVector3());
                    MonsterNode.MonseterPlacement.Quat = Quaternion.RotationFrowTwoVector(Vector3.Forward, -dir.ToSingleVector3());
                }
                else
                {
                    //attack
                }
            }
            
            return base.OnTickLogic(args);
        }

    }
}
