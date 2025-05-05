using System;
using System.Collections.Generic;
using EngineNS;
using EngineNS.Thread.Async;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Bricks.PhysicsCore.SceneNode;
using System.ComponentModel;

namespace Survivor
{
    public enum EMonsterSpawnType
    {
        Random,   //随机位置
        Direction //在角色的某个方向上
    }

    public class TtMonsterSpawnStrategy : EngineNS.IO.BaseSerializer
    {
        [EngineNS.Rtti.Meta]
        public EMonsterSpawnType SpawnType { get; set; } = EMonsterSpawnType.Random;
        [EngineNS.Rtti.Meta]
        public int MonsterId { get; set; } = 1;
        [EngineNS.Rtti.Meta]
        public int MonsterCount { get; set; } = 0;

        protected TtMonsterSpawnerNode SpawnerNode = null;
        public TtMonsterSpawnStrategy() { }
        public TtMonsterSpawnStrategy(EMonsterSpawnType spawnType, int monsterId, int monsterCount, TtMonsterSpawnerNode spawnerNode)
        {
            SpawnType = spawnType;
            MonsterId = monsterId;
            MonsterCount = monsterCount;
            SpawnerNode = spawnerNode;
        }
        public virtual void Tick(TtMonsterSpawnerNode spawner)
        {
        }

        public virtual void CreateMonster(int monsterId, FTransform transform, TtMonsterSpawnerNode spawner)
        {
            var monsterData = TtDatabase.Instance.GetMonsterData(monsterId);
            InitMonster(monsterData, transform, spawner).AddWaitTask();
            //InitMonster(monsterData, transform, world).WaitCompletedAndDispose();
        }
        private async TtTask InitMonster(TtMonsterData monsterData, FTransform transform, TtMonsterSpawnerNode spawner)
        {
            var monsterNodeData = new TtMonsterNode.TtMonsterNodeData();
            //monsterNodeData.MonsterId = monsterData.Weapon1;
            await TtNode.SpawnNode<TtMonsterNode>(spawner, async (monsterNode) =>
            {
                monsterNode.MonsterData = monsterData;

                var stateNodeData = new TtMonsterStateNode.TtMonsterStateNodeData();
                stateNodeData.MonsterData = monsterData;
                var stateNode = await TtNode.SpawnNode<TtMonsterStateNode>(monsterNode, null,
                    stateNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
                stateNode.CurrentHP = monsterData.Health;
                monsterNode.StateNode = stateNode;

                RName monsterName = RName.ParseFrom(monsterData.Prefab);
                var monsterPrefab = EngineNS.TtEngine.Instance.GameInstance.PrefabPoolManager.CreatePrefab(monsterName, false);
                monsterPrefab.IsCollide = true;
                var node = monsterPrefab.Placement.HostNode;
                monsterPrefab.Placement.SetTransform(transform);
                monsterPrefab.Placement.HostNode = monsterPrefab;
                monsterPrefab.Parent = monsterNode;
                monsterNode.MonsterPrefab = monsterPrefab;
                var controlNode = monsterPrefab.FindFirstChild<TtPhyControllerNodeBase>(null, true);
                controlNode.SetFootPosition(transform.Position.ToSingleVector3());

                var monsterCtrollerData = new TtMonsterController.TtMonsterControllerData();
                var monsterCtroller = await TtNode.SpawnNode<TtMonsterController>(monsterNode, null,
                    monsterCtrollerData, EBoundVolumeType.Box, typeof(TtPlacement));
                monsterCtroller.Player = TtGameMode.GetSurvivorGameMode().Player;
                monsterCtroller.MonsterNode = monsterNode;
                monsterCtroller.Parent = monsterNode;
                monsterNode.Controller = monsterCtroller;

                var weaponNodeData = new TtWeaponNode.TtWeaponNodeData();
                weaponNodeData.WeaponType = "Melee";
                var weaponNode = await TtNode.SpawnNode<TtWeaponNode>(monsterNode, null,
                    weaponNodeData, EBoundVolumeType.None, typeof(TtPlacement));
                weaponNode.WeaponData.Damage = monsterData.Damage;
                weaponNode.WeaponData.AttackRange = monsterData.AttackRange;
            }, monsterNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
        }
    }
    public class TtMonsterSpawnStrategy_AtTime : TtMonsterSpawnStrategy
    {
        [EngineNS.Rtti.Meta]
        public float SpawnTime { get; set; } = 0;
        public TtMonsterSpawnStrategy_AtTime() { }
        public TtMonsterSpawnStrategy_AtTime(EMonsterSpawnType spawnType, int monsterId, int monsterCount, TtMonsterSpawnerNode spawnerNode, float spawnTime)
            : base(spawnType, monsterId, monsterCount, spawnerNode)
        {
            SpawnTime = spawnTime;
        }
        float mLastTime = 0;
        public override void Tick(TtMonsterSpawnerNode spawner)
        {
            var world = spawner.GetWorld();
            if (world.TimeSecond > SpawnTime && mLastTime < SpawnTime)
            {
                FTransform transform = FTransform.Identity;
                CreateMonster(1, transform, spawner);
            }
            mLastTime = world.TimeSecond;
        }
    }
    public class TtMonsterSpawnStrategy_CoolDown : TtMonsterSpawnStrategy
    {
        [EngineNS.Rtti.Meta]
        public float CoolDown { get; set; } = 1;
        public TtMonsterSpawnStrategy_CoolDown() { }
        public TtMonsterSpawnStrategy_CoolDown(EMonsterSpawnType spawnType, int monsterId, int monsterCount, TtMonsterSpawnerNode spawnerNode, float coolDown)
            : base(spawnType, monsterId, monsterCount, spawnerNode)
        {
            CoolDown = coolDown;
        }
        float mAccumulateTime = 0;
        public override void Tick(TtMonsterSpawnerNode spawner)
        {
            var world = spawner.GetWorld();
            if (TtEngine.Instance.PlayMode == EPlayMode.Editor || world.IsGameWorld == false)
                return;
            if (mAccumulateTime > CoolDown)
            {
                var player = TtGameMode.GetSurvivorGameMode().Player;
                if (player != null)
                {
                    var playerLocation = player.Placement.AbsTransform.Position;
                    for (int i = 0; i < MonsterCount; i++)
                    {
                        var random = new Random();
                        Vector3 location = Vector3.Zero;
                        const int randomMin = -15;
                        const int randomMax = 15;
                        location.x = random.Next(randomMin + (int)playerLocation.X, randomMax + (int)playerLocation.X);
                        location.z = random.Next(randomMin + (int)playerLocation.Z, randomMax + (int)playerLocation.Z);
                        location.y = 0;
                        FTransform transform = FTransform.Identity;
                        transform.Position = location.AsDVector();
                        CreateMonster(MonsterId, transform, spawner);
                    }
                }
                mAccumulateTime = 0;
            }
            mAccumulateTime += world.DeltaTimeSecond;
        }
    }
    [EngineNS.Bricks.CodeBuilder.ContextMenu("MonsterSpawner", "Game\\Survivor\\MonsterSpawner", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtMonsterSpawnerNode.TtMonsterSpawnerNodeData), DefaultNamePrefix = "MonsterSpawner")]
    public class TtMonsterSpawnerNode : EngineNS.GamePlay.Scene.TtLightWeightNodeBase
    {
        public class TtMonsterSpawnerNodeData : EngineNS.GamePlay.Scene.TtNodeData
        {
            [EngineNS.Rtti.Meta]
            public List<TtMonsterSpawnStrategy> SpawnStrategies { get; set; } = new();
        }
        public TtMonsterSpawnerNodeData MonsterSpawnerNodeData { get => NodeData as TtMonsterSpawnerNodeData; }
        [Category("Survivor")]
        public List<TtMonsterSpawnStrategy> SpawnStrategies { get => MonsterSpawnerNodeData.SpawnStrategies; }

        protected override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);
            if (SpawnStrategies.Count == 0)
            {
                TtMonsterSpawnStrategy_CoolDown strategy = new(EMonsterSpawnType.Random, 20001, 2, this, 2);
                SpawnStrategies.Add(strategy);
                TtMonsterSpawnStrategy_CoolDown strategy1 = new(EMonsterSpawnType.Random, 20002, 1, this, 30);
                SpawnStrategies.Add(strategy1);
            }
            return true;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            foreach (var strategy in SpawnStrategies)
            {
                strategy.Tick(this);
            }
            return base.OnTickLogic(args);
        }
    }
}
