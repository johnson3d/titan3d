using BCnEncoder.Shared;
using EngineNS;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using Inventory;
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
    public partial class TtCharacterStateNode : TtStateNode, Inventory.IHostActor
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
                    var gameMode = TtGameMode.GetSurvivorGameMode();
                    if (gameMode?.HpProgressUI == null || StateData?.RoleData == null || StateData.RoleData.Health <= 0)
                        return;

                    var percent = (float)value / (float)StateData.RoleData.Health;
                    percent = EngineNS.MathHelper.Clamp(percent, 0.0f, 1.0f);
                    gameMode.HpProgressUI.Percent = percent;
                }
            }
        }
        public TtCharacterStateNodeData StateData { get => NodeData as TtCharacterStateNodeData; }
        #region IHostActor
        public IDataFactory GetDataFactory()
        {
            return TtDatabase.Instance;
        }
        //��Ʒ����
        public Inventory.TtGoodsInventory GoodsInventory { get; } = new();
        //ֻ����Ʒ����
        public Inventory.TtGoodsUnlimitInventory ReadOnlyInventory { get; } = new();
        //���ܱ���
        public Inventory.TtSkillInventory SkillInventory { get; } = new();
        //������Ʒ���ͼ�걳��
        public Inventory.TtProxyInventory ProxyInventory { get; } = new();
        //���񱳰�
        public Inventory.TtMissionInventory MissionInventory { get; } = new();
        #endregion
        protected override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (await base.InitializeNode(world, data, bvType, placementType)==false)
                return false;

            GoodsInventory.Initialize(100);
            SkillInventory.Initialize(32);
            return true;
        }
        public override void BeAttacked(TtWeaponNode weaponNode)
        {
            if (IsDead || weaponNode?.WeaponData == null)
                return;

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
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            this.MissionInventory.Tick(this);
            this.SkillInventory.Tick(this);
            return base.OnTickLogic(args);
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
            if (IsDead || weaponNode?.WeaponData == null)
                return;

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
		public unsafe void macross_BeAttacked (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtWeaponNode weaponNode) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			BeAttacked(weaponNode);
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross