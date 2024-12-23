using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.Text;

namespace Survivor
{
    public class TtStateNode : EngineNS.GamePlay.Scene.TtSceneActorNode
    {
        public class TtStateNodeData : TtNodeData
        {

        }
        public virtual void OnAttacted(TtWeaponNode weaponNode)
        {
        }
    }
    public class TtCharacterStateNode : TtStateNode
    {
        public class TtCharacterStateNodeData : TtStateNodeData
        {

        }
        public TtRoleData RoleData { get; set; } = null;
        public override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            return await base.InitializeNode(world, data, bvType, placementType);
        }
        public override void OnAttacted(TtWeaponNode weaponNode)
        {
            
        }
    }

    public class TtMonsterStateNode : TtStateNode
    {
        public class TtMonsterStateNodeData: TtStateNodeData
        {

        }
        public override void OnAttacted(TtWeaponNode weaponNode)
        {

        }
    }
}
