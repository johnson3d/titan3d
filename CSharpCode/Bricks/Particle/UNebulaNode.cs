using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public class UNebulaNode<FParticle, FParticleSystem, TEmitter, TMdfQueueType> : GamePlay.Scene.UMeshNode 
        where FParticle : unmanaged
        where FParticleSystem : unmanaged
        where TEmitter : UEmitter<FParticle, FParticleSystem>, new()
    {
        public class UNebulaNodeData : GamePlay.Scene.UMeshNode.UMeshNodeData
        {
            public UNebulaNodeData()
            {
                this.MdfQueueType = Rtti.UTypeDesc.TypeStr(typeof(TMdfQueueType));
            }
        }
        public UNebulaParticle NebulaParticle { get; } = new UNebulaParticle();
        
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
