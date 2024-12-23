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
        public void SelectRole(int roleId)
		{
			var roleData = HeroManager.GetData("RoleId", roleId);
			var character = CurrentScene.FindFirstChild<TtCharacter>(null, true);
			EngineNS.TtEngine.Instance.TaskCollector.AddWaitTask(InitCharacter(character, roleData));

        }
		private async TtTask InitCharacter(TtNode parent, TtRoleData roleData)
		{
			var stateNode = new TtCharacterStateNode();
			var stateNodeData = new TtCharacterStateNode.TtCharacterStateNodeData();
			await stateNode.InitializeNode(parent.HostWorld, stateNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            stateNode.Parent = parent;

            var weaponNode = new TtWeaponNode();
            var weaponNodeData = new TtWeaponNode.TtWeaponNodeData();
			weaponNodeData.WeaponId = roleData.Weapon1;
            await weaponNode.InitializeNode(parent.HostWorld, weaponNodeData, EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
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