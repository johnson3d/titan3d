using System;
using System.Collections.Generic;
using EngineNS;
using EngineNS.GamePlay.Character;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using NPOI.Util;

namespace Survivor
{
    [EngineNS.Rtti.Meta]
    public partial class TtGameMode
    {
        [EngineNS.Rtti.Meta]
		public EngineNS.GamePlay.Scene.TtScene CurrentScene { get; set; }
        [EngineNS.Rtti.Meta]
        public TtWeaponManager WeaponManager { get; } = new TtWeaponManager();
        [EngineNS.Rtti.Meta]
        public TtHeroManager HeroManager { get; } = new TtHeroManager();
        [EngineNS.Rtti.Meta]
        public TtMonsterManager MonsterManager { get; } = new TtMonsterManager();
		public TtCharacter Player = null;
        [EngineNS.Rtti.Meta]
        public void LoadWeapons(
			[RName.PGRName(FilterExts = EngineNS.Bricks.DataSet.TtDataSet.AssetExt)]
			RName name)
        {
            WeaponManager.LoadDataSet(name);
        }
        [EngineNS.Rtti.Meta]
        public void LoadHeros(
            [RName.PGRName(FilterExts = EngineNS.Bricks.DataSet.TtDataSet.AssetExt)]
            RName name)
        {
            HeroManager.LoadDataSet(name);
        }
        [EngineNS.Rtti.Meta]
        public void LoadMonsters(
		[RName.PGRName(FilterExts = EngineNS.Bricks.DataSet.TtDataSet.AssetExt)]
            RName name)
        {
            MonsterManager.LoadDataSet(name);
        }
        [EngineNS.Rtti.Meta]
        public void CreateMonster(int roleId)
        {
			var monsterData = new TtMonsterData();
            EngineNS.TtEngine.Instance.TaskCollector.AddWaitTask(InitMonster(monsterData));

        }
        private async TtTask InitMonster(TtMonsterData monsterData)
        {
            var monsterNode = new TtMonsterNode();
            var monsterNodeData = new TtMonsterNode.TtMonsterNodeData();
            //monsterNodeData.MonsterId = monsterData.Weapon1;
            await monsterNode.InitializeNode(CurrentScene.HostWorld, monsterNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            monsterNode.MonsterData = monsterData;
            monsterNode.Parent = CurrentScene;

            var stateNode = new TtMonsterStateNode();
            var stateNodeData = new TtMonsterStateNode.TtMonsterStateNodeData();
            await stateNode.InitializeNode(CurrentScene.HostWorld, stateNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            stateNode.Parent = monsterNode;
			RName monsterName = EngineNS.RName.GetRName("survivor/monsters/barghest/prefab_barghest.prefab", EngineNS.RName.ERNameType.Game);
            var monsterPrefab = EngineNS.TtEngine.Instance.GameInstance.PrefabPoolManager.CreatePrefab(monsterName);
			monsterPrefab.Parent = monsterNode;
        }
        [EngineNS.Rtti.Meta]
        public void CreateMonsterSpawner()
        {
            EngineNS.TtEngine.Instance.TaskCollector.AddWaitTask(InitMonsterSpawner());

        }
        private async TtTask InitMonsterSpawner()
        {
            var node = new TtMonsterSpawnerNode();
            var nodeData = new TtMonsterSpawnerNode.TtMonsterSpawnerNodeData();
            await node.InitializeNode(CurrentScene.HostWorld, nodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            node.Parent = CurrentScene;
        }
        [EngineNS.Rtti.Meta]
        public void SelectRole(int roleId)
		{
			var roleData = HeroManager.GetData("RoleId", roleId);
			var character = CurrentScene.FindFirstChild<TtCharacter>(null, true);
			Player = character as TtCharacter;
            EngineNS.TtEngine.Instance.TaskCollector.AddWaitTask(InitCharacter(character, roleData));
        }

        private async TtTask InitCharacter(TtNode parent, TtRoleData roleData)
		{
			var stateNode = new TtCharacterStateNode();
			var stateNodeData = new TtCharacterStateNode.TtCharacterStateNodeData();
            stateNodeData.RoleData = roleData;
            stateNodeData.CurrentHP = roleData.Health;

			await stateNode.InitializeNode(parent.HostWorld, stateNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            stateNode.Parent = parent;

            var weaponNode = new TtWeaponNode();
            var weaponNodeData = new TtWeaponNode.TtWeaponNodeData();
			weaponNodeData.WeaponId = roleData.Weapon1;
            await weaponNode.InitializeNode(parent.HostWorld, weaponNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
			weaponNode.RoleData = roleData;
            weaponNode.Parent = parent;
        }
        [EngineNS.Rtti.Meta]
		public static TtGameMode GetSurvivorGameMode()
		{
			var game = TtEngine.Instance.GameInstance.MacrossGame as TtMacrossSurvivorGame;
			return game.GameMode;
		}
    }
}

#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace Survivor
{
	partial class TtGameMode
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_LoadWeapons_2037383663 = new EngineNS.Macross.TtMacrossBreak("Survivor.TtGameMode->void LoadWeapons(RName name)");
		public unsafe void macross_LoadWeapons (string nodeName, RName name) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":name", name);
				}
			}
			LoadWeapons(name);
			macross_break_LoadWeapons_2037383663.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_LoadHeros_2037383663 = new EngineNS.Macross.TtMacrossBreak("Survivor.TtGameMode->void LoadHeros(RName name)");
		public unsafe void macross_LoadHeros (string nodeName, RName name) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":name", name);
				}
			}
			LoadHeros(name);
			macross_break_LoadHeros_2037383663.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_LoadMonsters_2037383663 = new EngineNS.Macross.TtMacrossBreak("Survivor.TtGameMode->void LoadMonsters(RName name)");
		public unsafe void macross_LoadMonsters (string nodeName, RName name) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":name", name);
				}
			}
			LoadMonsters(name);
			macross_break_LoadMonsters_2037383663.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_CreateMonster_770898579 = new EngineNS.Macross.TtMacrossBreak("Survivor.TtGameMode->void CreateMonster(int roleId)");
		public unsafe void macross_CreateMonster (string nodeName, int roleId) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":roleId", roleId);
				}
			}
			CreateMonster(roleId);
			macross_break_CreateMonster_770898579.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_CreateMonsterSpawner_2609910045 = new EngineNS.Macross.TtMacrossBreak("Survivor.TtGameMode->void CreateMonsterSpawner()");
		public unsafe void macross_CreateMonsterSpawner (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			CreateMonsterSpawner();
			macross_break_CreateMonsterSpawner_2609910045.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_SelectRole_770898579 = new EngineNS.Macross.TtMacrossBreak("Survivor.TtGameMode->void SelectRole(int roleId)");
		public unsafe void macross_SelectRole (string nodeName, int roleId) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":roleId", roleId);
				}
			}
			SelectRole(roleId);
			macross_break_SelectRole_770898579.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_GetSurvivorGameMode_3323264318 = new EngineNS.Macross.TtMacrossBreak("Survivor.TtGameMode->static TtGameMode GetSurvivorGameMode()");
		public static unsafe TtGameMode macross_GetSurvivorGameMode (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = GetSurvivorGameMode();
			macross_break_GetSurvivorGameMode_3323264318.TryBreak();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross