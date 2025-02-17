using System;
using System.Collections.Generic;
using EngineNS;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Character;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using EngineNS.UI;
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

        public void Tick(TtGameInstance host, float elapsedMillisecond)
        {
            TtUIManager.UIKeyName keyName = new TtUIManager.UIKeyName();
            TtEngine.Instance.UIManager.GetUI(keyName);
        }
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
        public async TtTask InitMonsterSpawner() 
        { 
            var nodeData = new TtMonsterSpawnerNode.TtMonsterSpawnerNodeData();
            var node = await TtNode.SpawnNode<TtMonsterSpawnerNode>(CurrentScene, null,
                nodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            node.Parent = CurrentScene;
        }
        [EngineNS.Rtti.Meta]
        public async TtTask SelectRole(int roleId)
        {
            var roleData = HeroManager.GetData("RoleId", roleId);
            if (roleData == null)
                return;
            var character = CurrentScene.FindFirstChild<TtCharacter>(null, true);
            Player = character as TtCharacter;
            if (Player == null)
                return;
            await InitCharacter(character, roleData);
        }

        private async TtTask InitCharacter(TtNode parent, TtRoleData roleData)
        {
            var stateNodeData = new TtCharacterStateNode.TtCharacterStateNodeData();
            stateNodeData.RoleData = roleData;
            stateNodeData.CurrentHP = roleData.Health;
            var stateNode = await TtNode.SpawnNode<TtCharacterStateNode>(parent, null,
                stateNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            stateNode.NodeName = "StateNode";
            stateNode.OnDead = ()=>
            {
                var game = TtEngine.Instance.GameInstance.MacrossGame as TtMacrossSurvivorGame;
                game.CountToTriggerPlayerDead();
            };

            {
                var weaponNodeData = new TtWeaponNode.TtWeaponNodeData();
                weaponNodeData.WeaponId = roleData.Weapon1;
                var weaponNode = await TtNode.SpawnNode<TtWeaponNode>(parent.Parent, null,
                    weaponNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
                weaponNode.RoleData = roleData;
            }

            {
                var weaponNodeData = new TtWeaponNode.TtWeaponNodeData();
                weaponNodeData.WeaponId = 1002;
                var weaponNode = await TtNode.SpawnNode<TtWeaponNode>(parent.Parent, null,
                    weaponNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
                weaponNode.RoleData = roleData;
            }
            
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
		private static EngineNS.Macross.TtMacrossBreak macross_break_InitMonsterSpawner_2609910045 = new EngineNS.Macross.TtMacrossBreak("Survivor.TtGameMode->TtTask InitMonsterSpawner()");
		public async TtTask macross_InitMonsterSpawner (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			await InitMonsterSpawner();
			macross_break_InitMonsterSpawner_2609910045.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_SelectRole_770898579 = new EngineNS.Macross.TtMacrossBreak("Survivor.TtGameMode->TtTask SelectRole(int roleId)");
		public async TtTask macross_SelectRole (string nodeName, int roleId) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":roleId", roleId);
				}
			}
			await SelectRole(roleId);
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