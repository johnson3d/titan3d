﻿using EngineNS.GamePlay.Controller;
using EngineNS.GamePlay.Scene;
using EngineNS.GamePlay.Scene.Actor;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.GamePlay.Player
{
    public partial class TtPlayer : TtNode, IEventProcessor
    {
        public partial class TtPlayerData : TtNodeData
        {
            [Rtti.Meta]
            public TtCharacterController CharacterController { get; set; } = null; //there maybe many controllers cause of one player can control many characters, for now assume only one
        }
        
        public TtPlayerData PlayerData
        {
            get
            {
                return NodeData as TtPlayerData;
            }
        }
        public override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
            {
                return false;
            }
            TtEngine.Instance.EventProcessorManager.RegProcessor(this);
            return true;
        }
        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            base.OnGatherVisibleMeshes(rp);
        }
        protected override void OnParentSceneChanged(TtScene prev, TtScene cur)
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
