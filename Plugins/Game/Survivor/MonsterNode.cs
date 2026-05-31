using EngineNS;
using EngineNS.Bricks.PhysicsCore.SceneNode;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using EngineNS.NxPhysics;
using EngineNS.Thread.Async;

namespace Survivor
{
    //无论角色的武器还是怪物的近远程攻击都算做武器攻击
    [EngineNS.Rtti.Meta]
    public class TtMonsterNode : EngineNS.GamePlay.Scene.TtSceneActorNode
    {
        public class TtMonsterNodeData : EngineNS.GamePlay.Scene.TtNodeData
        {
            [EngineNS.Rtti.Meta]
            public int MonsterId = 0;
        }
        public TtMonsterNodeData MonsterNodeData { get => NodeData as TtMonsterNodeData; }
        public TtMonsterStateNode StateNode { get; set; } = null;
        public TtMonsterController Controller { get; set; } = null;
        public TtPrefabNode MonsterPrefab { get; set; } = null;
        public TtPlacementBase MonseterPlacement { get => MonsterPrefab?.Placement; }
        public TtMonsterData MonsterData { get; set; } = null;

        protected override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);

            return true;
        }
        public override EngineNS.Profiler.TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtMonsterNode>.Scope;
        }
        float TimeToRemove = 2;
        float AccTimeToRemove = 0;
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            if (StateNode != null && StateNode.IsDead)
            {
                if (AccTimeToRemove > TimeToRemove)
                {
                    OnDead();
                    AccTimeToRemove = 0;
                }
                else
                {
                    AccTimeToRemove += args.World.DeltaTimeSecond;
                }
            }
            return base.OnTickLogic(args);
        }
        public void OnDead()
        {
            RemoveFromWorld();

            MonsterPrefab.Parent = null;
            MonsterPrefab.IsCollide = false;

            //EngineNS.TtEngine.Instance.GameInstance.PrefabPoolManager.ReleasePrefab(MonsterPrefab);
            MonsterPrefab = null;
            Controller.MonsterNode = null;
            Controller.Player = null;
        }
    }
}
