using EngineNS.GamePlay.Controller;
using EngineNS.GamePlay.Scene;
using EngineNS.GamePlay.Scene.Actor;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.GamePlay.Player
{
    public partial class UPlayer : UNode, IEventProcessor
    {
        public partial class UPlayerData : UNodeData
        {
            [Rtti.Meta]
            public UCharacterController CharacterController { get; set; } = null; //there maybe many controllers cause of one player can control many characters, for now assume only one
        }
        
        public UPlayerData PlayerData
        {
            get
            {
                return NodeData as UPlayerData;
            }
        }
        public override async Thread.Async.TtTask<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
            {
                return false;
            }
            UEngine.Instance.EventProcessorManager.RegProcessor(this);
            return true;
        }
        public override void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
        {
            base.OnGatherVisibleMeshes(rp);
        }
        protected override void OnParentSceneChanged(UScene prev, UScene cur)
        {

        }

        public override void TickLogic(TtNodeTickParameters args)
        {
            base.TickLogic(args);
            PlayerData.CharacterController?.TickLogic(args);
        }

        public unsafe bool OnEvent(in Bricks.Input.Event e)
        {

            return true;
        }
    }
}
