using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Particle
{
    public class UNebulaNode<FParticle, FParticleSystem, TEmitter, TMdfQueueType> : GamePlay.Scene.UMeshNode 
        where FParticle : unmanaged
        where FParticleSystem : unmanaged
        where TEmitter : UEmitter<FParticle, FParticleSystem>, new()
    {
        public abstract class UNebulaNodeData : GamePlay.Scene.UMeshNode.UMeshNodeData
        {
            public UNebulaNodeData()
            {
                this.MdfQueueType = Rtti.UTypeDesc.TypeStr(typeof(TMdfQueueType));
            }
            [Rtti.Meta]
            public RName NebulaName { get; set; }
        }
        public RName NebulaName 
        { 
            get
            {
                return GetNodeData<UNebulaNodeData>().NebulaName;
            }
            set
            {
                GetNodeData<UNebulaNodeData>().NebulaName = value;
                System.Action action = async () =>
                {
                    mNebulaParticle = await UEngine.Instance.NebulaTemplateManager.GetParticle(value);
                };
                action();
            }
        }
        UNebulaParticle mNebulaParticle;
        public UNebulaParticle NebulaParticle { get=> mNebulaParticle; }
        public override async Task<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            mNebulaParticle = await UEngine.Instance.NebulaTemplateManager.GetParticle(GetNodeData<UNebulaNodeData>().NebulaName);
            return ret;
        }
        public override void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
        {
            base.OnGatherVisibleMeshes(rp);

            if (mNebulaParticle == null)
                return;

            foreach (var i in mNebulaParticle.Emitter.Values)
            {
                if (i.Mesh != null)
                {
                    if (rp.World.CameralOffsetSerialId != CameralOffsetSerialId)
                    {
                        CameralOffsetSerialId = rp.World.CameralOffsetSerialId;
                        i.Mesh.UpdateCameraOffset(rp.World);
                    }

                    rp.VisibleMeshes.Add(i.Mesh);
                }
            }
            if (rp.VisibleNodes != null)
            {
                rp.VisibleNodes.Add(this);
            }
        }
        public override bool OnTickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy)
        {
            //var particleNode = policy.FindNode("ParticleNode") as UParticleGraphNode;
            var particleNode = policy.FindFirstNode<UParticleGraphNode>();
            if (particleNode == null)
                return true;

            NebulaParticle.Update(particleNode, UEngine.Instance.ElapsedSecond);

            return true;
        }
    }
}
