using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene.Actor
{
    public partial class TtActor : TtSceneActorNode
    {
        public partial class TtActorData : TtNodeData
        {

        }
        public TtActorData ActorData
        {
            get
            {
                return NodeData as TtActorData;
            }
        }
        public UCenterData CenterData { get; } = new UCenterData();

        public override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
            {
                return false;
            }

            return true;
        }
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
