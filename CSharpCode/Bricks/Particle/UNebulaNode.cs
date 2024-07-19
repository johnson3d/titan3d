using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Particle
{
    [Bricks.CodeBuilder.ContextMenu("NebulaNode ", "NebulaNode ", UNode.EditorKeyword)]
    [UNode(NodeDataType = typeof(TtNebulaNode.TtNebulaNodeData), DefaultNamePrefix = "Nebula")]
    public class TtNebulaNode : GamePlay.Scene.UMeshNode 
    {
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mNebulaParticle);
            base.Dispose();
        }
        public class TtNebulaNodeData : GamePlay.Scene.UMeshNode.UMeshNodeData
        {
            public TtNebulaNodeData()
            {
                this.MdfQueueType = Rtti.UTypeDesc.TypeStr(typeof(TtParticleMdfQueue));
            }
            [Rtti.Meta]
            public RName NebulaName { get; set; }
        }
        [RName.PGRName(FilterExts = TtNebulaParticle.AssetExt)]
        public RName NebulaName 
        { 
            get
            {
                return GetNodeData<TtNebulaNodeData>().NebulaName;
            }
            set
            {
                GetNodeData<TtNebulaNodeData>().NebulaName = value;
                System.Action action = async () =>
                {
                    mNebulaParticle = await UEngine.Instance.NebulaTemplateManager.GetParticle(value);
                };
                action();
            }
        }
        TtNebulaParticle mNebulaParticle;
        public void UnsafeSetNebula(TtNebulaParticle nebula)
        {
            mNebulaParticle = nebula;
        }
        public TtNebulaParticle NebulaParticle { get=> mNebulaParticle; }
        public override async Thread.Async.TtTask<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            mNebulaParticle = await UEngine.Instance.NebulaTemplateManager.GetParticle(GetNodeData<TtNebulaNodeData>().NebulaName);
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

                    rp.AddVisibleMesh(i.Mesh);
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
