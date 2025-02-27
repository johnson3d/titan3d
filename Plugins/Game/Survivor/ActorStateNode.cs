using BCnEncoder.Shared;
using EngineNS;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.Text;
using static Survivor.TtCharacterStateNode;

namespace Survivor
{
    [EngineNS.Rtti.Meta]
    public partial class TtStateNode : EngineNS.GamePlay.Scene.TtSceneActorNode
    {
        public class TtStateNodeData : TtNodeData
        {
            [EngineNS.Rtti.Meta]
            public float CurrentHP { get; set; } = 0;
        }
        public virtual float CurrentHP 
        {
            get => GetNodeData<TtStateNodeData>().CurrentHP;
            set
            {
                GetNodeData<TtStateNodeData>().CurrentHP = value;
            }
        }
        public Action OnDead;
        [EngineNS.Rtti.Meta]
        public bool IsDead { get; set; } = false;
        [EngineNS.Rtti.Meta]
        public virtual void BeAttacked(TtWeaponNode weaponNode)
        {
        }
        
    }
    public partial class TtCharacterStateNode : TtStateNode
    {
        public class TtCharacterStateNodeData : TtStateNodeData
        {
            [EngineNS.Rtti.Meta]
            public TtRoleData RoleData { get; set; } = null;
            
        }
        public override float CurrentHP
        {
            get => GetNodeData<TtStateNodeData>().CurrentHP;
            set
            {
                var data = GetNodeData<TtStateNodeData>();
                if (data.CurrentHP != value)
                {
                    data.CurrentHP = value;
                    if (TtGameMode.GetSurvivorGameMode().HpProgressUI == null)
                        return;
                    var percent = (float)value / (float)StateData.RoleData.Health;
                    percent = EngineNS.MathHelper.Clamp(percent, 0.0f, 1.0f);
                    TtGameMode.GetSurvivorGameMode().HpProgressUI.Percent = percent;
                }
            }
        }
        public TtCharacterStateNodeData StateData { get => NodeData as TtCharacterStateNodeData; }
        public TtItemInventory Bag0;
        protected override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            return await base.InitializeNode(world, data, bvType, placementType);
        }
        public override void BeAttacked(TtWeaponNode weaponNode)
        {
            var hp = this.CurrentHP - weaponNode.WeaponData.Damage;
            if (hp <= 0 && !IsDead)
            {
                IsDead = true;
                if (OnDead != null)
                {
                    OnDead.Invoke();
                }
            }

            this.CurrentHP = MathF.Max(hp, 0);
            return;
        }
    }

    public partial class TtMonsterStateNode : TtStateNode
    {
        public class TtMonsterStateNodeData: TtStateNodeData
        {
            [EngineNS.Rtti.Meta]
            public TtMonsterData MonsterData { get; set; } = null;
        }
        public TtMonsterStateNodeData StateData { get => NodeData as TtMonsterStateNodeData; }
        public override void BeAttacked(TtWeaponNode weaponNode)
        {
            var hp = this.CurrentHP - weaponNode.WeaponData.Damage;
            if (hp <= 0 && !IsDead)
            {
                IsDead = true;
                if(OnDead != null)
                {
                    OnDead.Invoke();
                }
            }

            this.CurrentHP = MathF.Max(hp, 0);
            return;
        }
    }
}
#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace Survivor
{
	partial class TtStateNode
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_BeAttacked_2499766893 = new EngineNS.Macross.TtMacrossBreak("Survivor.TtStateNode->void BeAttacked(TtWeaponNode weaponNode)");
		public unsafe void macross_BeAttacked (string nodeName, TtWeaponNode weaponNode) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":weaponNode", weaponNode);
				}
			}
			BeAttacked(weaponNode);
			macross_break_BeAttacked_2499766893.TryBreak();
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross