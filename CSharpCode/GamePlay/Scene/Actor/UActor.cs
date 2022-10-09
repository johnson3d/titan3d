using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene.Actor
{
    public partial class UActor : UNode
    {
        public partial class UActorData : UNodeData
        {
            [Rtti.Meta]
            public float Radius { get; set; } = 1.0f;
            [Rtti.Meta]
            public float Height { get; set; } = 1.85f;
        }
        public UActorData ActorData
        {
            get
            {
                return NodeData as UActorData;
            }
        }

        public override async System.Threading.Tasks.Task<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
            {
                return false;
            }

            return true;
        }
        partial void CreatePxCapsuleActor(ref bool result, Scene.UScene scene, float radius, float height);
        protected override void OnParentSceneChanged(UScene prev, UScene cur)
        {
            if (cur != null)
            {
                bool ok = true;
                //CreatePxCapsuleActor(ref ok, cur, ActorData.Radius, ActorData.Height);
            }
        }
    }
}
