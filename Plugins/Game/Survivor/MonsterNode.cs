using EngineNS;
using EngineNS.Bricks.PhysicsCore.SceneNode;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using EngineNS.NxPhysics;
using EngineNS.Thread.Async;
using MathNet.Numerics.Random;
using System;
using System.Collections.Generic;
using System.Text;
using static Survivor.TtMonsterController;
using static Survivor.TtWeaponNode;

namespace Survivor
{
    //无论角色的武器还是怪物的近远程攻击都算做武器攻击
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
        public TtMonsterData MonsterData { get; set; } = null;

        public override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);

            return true;
        }
        public override EngineNS.Profiler.TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtMonsterNode>.Scope;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            if(StateNode.IsDead)
            {
                Parent = null;
            }
            return base.OnTickLogic(args);
        }
    }
    public enum EMonsterSpawnType
    {
        Random,   //随机位置
        Direction //在角色的某个方向上
    }

    public class TtMonsterSpawnStrategy
    {
        public EMonsterSpawnType SpawnType { get; set; } = EMonsterSpawnType.Random;
        public int MonsterId { get; set; } = 0;
        public int MonsterCount { get; set; } = 0;

        protected TtMonsterSpawnerNode SpawnerNode = null;
        public TtMonsterSpawnStrategy(EMonsterSpawnType spawnType, int monsterId, int monsterCount, TtMonsterSpawnerNode spawnerNode)
        {
            SpawnType = spawnType;
            MonsterId = monsterId;
            MonsterCount = monsterCount;
            SpawnerNode = spawnerNode;
        }
        public virtual void Tick(TtWorld world)
        {
        }

        public virtual void CreateMonster(int monsterId, FTransform transform, TtWorld world)
        {
			var monsterData = new TtMonsterData();
            EngineNS.TtEngine.Instance.TaskCollector.AddWaitTask(InitMonster(monsterData, transform, world));

        }
        private async TtTask InitMonster(TtMonsterData monsterData, FTransform transform, TtWorld world)
        {
            var monsterNode = new TtMonsterNode();
            var monsterNodeData = new TtMonsterNode.TtMonsterNodeData();
            //monsterNodeData.MonsterId = monsterData.Weapon1;
            await monsterNode.InitializeNode(world, monsterNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            monsterNode.MonsterData = monsterData;
            monsterNode.Parent = world.Root.Children[0];

            var stateNode = new TtMonsterStateNode();
            var stateNodeData = new TtMonsterStateNode.TtMonsterStateNodeData();
            stateNodeData.CurrentHP = monsterData.Health;
            stateNodeData.MonsterData = monsterData;
            await stateNode.InitializeNode(world, stateNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            stateNode.Parent = monsterNode;
            monsterNode.StateNode = stateNode;

			RName monsterName = EngineNS.RName.GetRName("survivor/monsters/barghest/prefab_barghest.prefab", EngineNS.RName.ERNameType.Game);
            var monsterPrefab = EngineNS.TtEngine.Instance.GameInstance.PrefabPoolManager.CreatePrefab(monsterName);
            var node = monsterPrefab.Placement.HostNode;
            monsterPrefab.Placement.SetTransform(transform);
            monsterPrefab.Placement.HostNode = monsterPrefab;
            monsterPrefab.Parent = monsterNode;
            monsterNode.MonsterPrefab = monsterPrefab;
            var controlNode = monsterPrefab.FindFirstChild<TtPhyControllerNodeBase>(null, true) as TtPhyControllerNodeBase;
            controlNode.SetFootPosition(transform.Position.ToSingleVector3());

            var monsterCtroller = new TtMonsterController();
            var monsterCtrollerData = new TtMonsterControllerData();
            await monsterCtroller.InitializeNode(world, monsterCtrollerData, EBoundVolumeType.Box, typeof(TtPlacement));
            var macrossGame = EngineNS.TtEngine.Instance.GameInstance.MacrossGame as TtMacrossSurvivorGame;
            monsterCtroller.Player = macrossGame.GameMode.Player;
            monsterCtroller.MonsterNode = monsterNode;
            monsterCtroller.Parent = monsterNode;
            monsterNode.Controller = monsterCtroller;

            var weaponNode = new TtWeaponNode();
            var weaponNodeData = new TtWeaponNodeData();
            weaponNodeData.WeaponType = "Melee";
            await weaponNode.InitializeNode(world, weaponNodeData, EBoundVolumeType.None, typeof(TtPlacement));
            weaponNode.Parent = monsterNode;
        }
    }
    public class TtMonsterSpawnStrategy_AtTime : TtMonsterSpawnStrategy
    {
        public float SpawnTime { get; set; } = 0;
        public TtMonsterSpawnStrategy_AtTime(EMonsterSpawnType spawnType, int monsterId, int monsterCount, TtMonsterSpawnerNode spawnerNode, float spawnTime)
            : base(spawnType, monsterId, monsterCount, spawnerNode)
        {
            SpawnTime = spawnTime;
        }
        float mLastTime = 0;
        public override void Tick(TtWorld world)
        {
            if(world.TimeSecond > SpawnTime && mLastTime < SpawnTime )
            {
                FTransform transform = FTransform.Identity;
                CreateMonster(1, transform, world);
            }
            mLastTime = world.TimeSecond;
        }
    }
    public class TtMonsterSpawnStrategy_CoolDown : TtMonsterSpawnStrategy
    {
        public float CoolDown = 1;
        public TtMonsterSpawnStrategy_CoolDown(EMonsterSpawnType spawnType, int monsterId, int monsterCount, TtMonsterSpawnerNode spawnerNode, float coolDown)
            : base(spawnType, monsterId, monsterCount, spawnerNode)
        {
            CoolDown = coolDown;
        }
        float mAccumulateTime = 0;
        public override void Tick(TtWorld world)
        {
            if (mAccumulateTime > CoolDown)
            {
                for (int i = 0; i < MonsterCount; i++) 
                {
                    var random = new Random();
                    
                    Vector3 location = Vector3.Zero;
                    location.x = random.Next(-10, 10);
                    location.z = random.Next(-10, 10);
                    location.y = 0;
                    FTransform transform = FTransform.Identity;
                    transform.Position = location.AsDVector();
                    CreateMonster(MonsterId, transform, world);
                }
                
                mAccumulateTime = 0;
            }
            mAccumulateTime += world.DeltaTimeSecond;
        }
    }
    public class TtMonsterSpawnerNode : EngineNS.GamePlay.Scene.TtSceneActorNode
    {
        public class TtMonsterSpawnerNodeData : EngineNS.GamePlay.Scene.TtNodeData
        {
            [EngineNS.Rtti.Meta]
            public List<TtMonsterSpawnStrategy> SpawnStrategies { get; set; } = new();
        }
        public TtMonsterSpawnerNodeData MonsterSpawnerNodeData { get => NodeData as TtMonsterSpawnerNodeData; }
        public List<TtMonsterSpawnStrategy> SpawnStrategies { get => MonsterSpawnerNodeData.SpawnStrategies; }

        public override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);
            TtMonsterSpawnStrategy strategy = new TtMonsterSpawnStrategy_CoolDown(EMonsterSpawnType.Random, 1, 1, this, 2);
            SpawnStrategies.Add(strategy);
            return true;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            foreach (var strategy in SpawnStrategies)
            {
                strategy.Tick(args.World);
            }
            return base.OnTickLogic(args);
        }
    }
}
