using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene.Actor
{
    public partial class UActor : TtSceneActorNode
    {
        public partial class UActorData : TtNodeData
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
        public UCenterData CenterData { get; } = new UCenterData();

        public override async Thread.Async.TtTask<bool> InitializeNode(UWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
            {
                return false;
            }

            return true;
        }
        partial void CreatePxCapsuleActor(ref bool result, Scene.TtScene scene, float radius, float height);
        protected override void OnParentSceneChanged(TtScene prev, TtScene cur)
        {
            if (cur != null)
            {
                //bool ok = true;
                //CreatePxCapsuleActor(ref ok, cur, ActorData.Radius, ActorData.Height);
            }
        }
    }
}
