using System;
using System.Collections.Generic;
using EngineNS;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Character;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using EngineNS.UI;
using EngineNS.UI.Controls;
using NPOI.Util;

namespace Survivor
{
    [EngineNS.Rtti.Meta]
    public partial class TtGameMode : TtGameModeBase
    {
        EngineNS.UI.Controls.TtUIElement mBattleUI;
        [EngineNS.Rtti.Meta(Flags = EngineNS.Rtti.MetaAttribute.EMetaFlags.NoSerializable)]
        public EngineNS.UI.Controls.TtUIElement BattleUI
        {
            get => mBattleUI;
            set
            {
                mBattleUI = value;
                if (value != null)
                {
                    var mo = value.MacrossObject;
                    if (mo != null)
                    {
                        mHpProgressUI = EngineNS.Rtti.TtTypeDescManager.GetPropertyMember(mo, "ElementVar_4728652819903166736") as TtProgress;
                    }
                }
                else
                {
                    mHpProgressUI = null;
                }
            }
        }
        [EngineNS.Rtti.Meta(Flags = EngineNS.Rtti.MetaAttribute.EMetaFlags.NoSerializable)]
        public TtCharacterStateNode CharStateNode
        {
            get
            {
                return CharacterController?.ControlledCharacter?.FindFirstChild<TtCharacterStateNode>();
            }
        }
        public TtCharacter Player = null;
        public TtProgress mHpProgressUI = null;
        public TtProgress HpProgressUI
        {
            get
            {
                return mHpProgressUI;
            }
        }
        //需要存盘，避免物品丢失
        public Inventory.TtItem SwapItem = null;
        public void ClickItem(Inventory.TtInventory targetInventory, short index)
        {
            SwapItem = targetInventory.SwapItem(SwapItem, index);
        }

        public override void Tick(TtGameInstance host, float elapsedMillisecond)
        {
            base.Tick(host, elapsedMillisecond);
            TtUIManager.UIKeyName keyName = new TtUIManager.UIKeyName();
            TtEngine.Instance.UIManager.GetUI(keyName);
            if (CharStateNode?.CurrentHP == 0)
            {

            }
        }
        [EngineNS.Rtti.Meta]
        public void LoadWeapons(
            [RName.PGRName(FilterExts = EngineNS.Bricks.DataSet.TtDataSet.AssetExt)]
            RName name)
        {
            TtDatabase.Instance.LoadWeapons(name);
        }
        [EngineNS.Rtti.Meta]
        public void LoadHeros(
            [RName.PGRName(FilterExts = EngineNS.Bricks.DataSet.TtDataSet.AssetExt)]
            RName name)
        {
            TtDatabase.Instance.LoadHeros(name);
        }
        [EngineNS.Rtti.Meta]
        public void LoadMonsters(
        [RName.PGRName(FilterExts = EngineNS.Bricks.DataSet.TtDataSet.AssetExt)]
            RName name)
        {
            TtDatabase.Instance.LoadMonsters(name);
        }
        public void LoadItems(
        [RName.PGRName(FilterExts = EngineNS.Bricks.DataSet.TtDataSet.AssetExt)]
            RName name)
        {
            TtDatabase.Instance.LoadItems(name);
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
        public async TtTask<bool> InitControlledCharacter(EngineNS.GamePlay.Controller.TtCharacterController cc, int roleId)
        {
            var roleData = TtDatabase.Instance.GetHeroData(roleId);
            if (roleData == null)
                return false;
            Player = cc.ControlledCharacter;
            if (Player == null)
                return false;
            await InitCharacter(cc, Player, roleData);
            return true;
        }

        private async TtTask InitCharacter(EngineNS.GamePlay.Controller.TtCharacterController cc, TtCharacter player, TtRoleData roleData)
        {
            var stateNodeData = new TtCharacterStateNode.TtCharacterStateNodeData();
            stateNodeData.RoleData = roleData;
            var stateNode = await TtNode.SpawnNode<TtCharacterStateNode>(player, null,
                stateNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            stateNode.NodeName = "StateNode";
            stateNode.CurrentHP = roleData.Health;
            stateNode.OnDead = ()=>
            {
                var game = TtEngine.Instance.GameInstance.MacrossGame as TtMacrossSurvivorGame;
                game.CountToTriggerPlayerDead();
            };

            var prefabNode = player.Parent;
            {
                var weaponNodeData = new TtWeaponNode.TtWeaponNodeData();
                weaponNodeData.WeaponId = roleData.Weapon1;
                var weaponNode = await TtNode.SpawnNode<TtWeaponNode>(prefabNode, null,
                    weaponNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
                weaponNode.RoleData = roleData;
            }

            {
                var weaponNodeData = new TtWeaponNode.TtWeaponNodeData();
                weaponNodeData.WeaponId = 1002;
                var weaponNode = await TtNode.SpawnNode<TtWeaponNode>(prefabNode, null,
                    weaponNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
                weaponNode.RoleData = roleData;
            }
            
        }
        [EngineNS.Rtti.Meta]
        public static TtGameMode GetSurvivorGameMode()
        {
            var game = TtEngine.Instance.GameInstance.MacrossGame as TtMacrossSurvivorGame;
            return game.SurvivorGameMode;
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
		private static EngineNS.Macross.TtMacrossBreak macross_break_InitControlledCharacter_1038341169 = new EngineNS.Macross.TtMacrossBreak("Survivor.TtGameMode->TtTask<bool> InitControlledCharacter(EngineNS.GamePlay.Controller.TtCharacterController cc, int roleId)");
		public async TtTask<bool> macross_InitControlledCharacter (string nodeName, EngineNS.GamePlay.Controller.TtCharacterController cc, int roleId) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":cc", cc);
					stackframe.SetWatchVariable(nodeName + ":roleId", roleId);
				}
			}
			var _return_value = await InitControlledCharacter(cc, roleId);
			macross_break_InitControlledCharacter_1038341169.TryBreak();
			return _return_value;
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