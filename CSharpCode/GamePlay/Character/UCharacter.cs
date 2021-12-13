using EngineNS.GamePlay.Scene;
using EngineNS.GamePlay.Scene.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Character
{
    public partial class UCharacter : UActor
    {
        public partial class UCharacterData : UCharacter.UActorData
        {
        }

        public UCharacterData PlayerData
        {
            get
            {
                return NodeData as UCharacterData;
            }
        }
        partial void CreatePxCapsuleController(ref bool result, Scene.UScene scene, float radius, float height);
        public override async System.Threading.Tasks.Task<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
            {
                return false;
            }

            return true;
        }
        public override void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
        {
            base.OnGatherVisibleMeshes(rp);
        }
        protected override void OnParentSceneChanged(UScene prev, UScene cur)
        {
            if (cur != null)
            {
                bool ok = true;
                CreatePxCapsuleController(ref ok, cur, PlayerData.Radius, PlayerData.Height);
            }
        }
    }
}
