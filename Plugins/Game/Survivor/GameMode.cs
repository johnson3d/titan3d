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
                mHpProgressUI = null;
                if (value != null)
                {
                    var mo = value.MacrossObject;
                    if (mo != null)
                    {
                        mHpProgressUI = EngineNS.Rtti.TtTypeDescManager.GetPropertyMember(mo, "ElementVar_4728652819903166736") as TtProgress;
                    }
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
        //��Ҫ���̣�������Ʒ��ʧ
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
            if (CurrentScene == null)
            {
                EngineNS.Profiler.Log.WriteLine<EngineNS.Profiler.TtGameplayGategory>(EngineNS.Profiler.ELogTag.Warning, "InitMonsterSpawner skipped: CurrentScene is null");
                return;
            }

            var nodeData = new TtMonsterSpawnerNode.TtMonsterSpawnerNodeData();
            var node = await TtNode.SpawnNode<TtMonsterSpawnerNode>(CurrentScene, null,
                nodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            if (node != null)
                node.Parent = CurrentScene;
        }
        [EngineNS.Rtti.Meta]
        public async TtTask<bool> InitControlledCharacter(EngineNS.GamePlay.Controller.TtCharacterController cc, int roleId)
        {
            var roleData = TtDatabase.Instance.GetHeroData(roleId);
            if (roleData == null)
            {
                EngineNS.Profiler.Log.WriteLine<EngineNS.Profiler.TtGameplayGategory>(EngineNS.Profiler.ELogTag.Warning, $"Hero({roleId}) not found");
                return false;
            }
            if (cc == null)
            {
                EngineNS.Profiler.Log.WriteLine<EngineNS.Profiler.TtGameplayGategory>(EngineNS.Profiler.ELogTag.Warning, "InitControlledCharacter skipped: controller is null");
                return false;
            }
            Player = cc.ControlledCharacter;
            if (Player == null)
            {
                EngineNS.Profiler.Log.WriteLine<EngineNS.Profiler.TtGameplayGategory>(EngineNS.Profiler.ELogTag.Warning, "InitControlledCharacter skipped: controlled character is null");
                return false;
            }
            await InitCharacter(cc, Player, roleData);
            return true;
        }

        private async TtTask InitCharacter(EngineNS.GamePlay.Controller.TtCharacterController cc, TtCharacter player, TtRoleData roleData)
        {
            if (player == null || roleData == null)
                return;

            var stateNodeData = new TtCharacterStateNode.TtCharacterStateNodeData();
            stateNodeData.RoleData = roleData;
            var stateNode = await TtNode.SpawnNode<TtCharacterStateNode>(player, null,
                stateNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            if (stateNode == null)
            {
                EngineNS.Profiler.Log.WriteLine<EngineNS.Profiler.TtGameplayGategory>(EngineNS.Profiler.ELogTag.Warning, "Create character state node failed");
                return;
            }

            stateNode.NodeName = "StateNode";
            stateNode.CurrentHP = roleData.Health;
            stateNode.OnDead = ()=>
            {
                var game = TtEngine.Instance.GameInstance.MacrossGame as TtMacrossSurvivorGame;
                game?.CountToTriggerPlayerDead();
            };

            var prefabNode = player.Parent;
            if (prefabNode == null)
            {
                EngineNS.Profiler.Log.WriteLine<EngineNS.Profiler.TtGameplayGategory>(EngineNS.Profiler.ELogTag.Warning, "Create character weapons skipped: player prefab is null");
                return;
            }

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
            var game = TtEngine.Instance.GameInstance?.MacrossGame as TtMacrossSurvivorGame;
            return game?.SurvivorGameMode;
        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace Survivor
{
	partial class TtGameMode
	{
		public unsafe void macross_LoadWeapons (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, RName name) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			LoadWeapons(name);
		}
		public unsafe void macross_LoadHeros (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, RName name) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			LoadHeros(name);
		}
		public unsafe void macross_LoadMonsters (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, RName name) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			LoadMonsters(name);
		}
		public async TtTask macross_InitMonsterSpawner (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			await InitMonsterSpawner();
		}
		public async TtTask<bool> macross_InitControlledCharacter (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, EngineNS.GamePlay.Controller.TtCharacterController cc, int roleId) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = await InitControlledCharacter(cc, roleId);
			return _return_value;
		}
		public static unsafe TtGameMode macross_GetSurvivorGameMode (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = GetSurvivorGameMode();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross