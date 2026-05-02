using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.NxPhysics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Particle
{
    [Bricks.CodeBuilder.ContextMenu("NebulaNode ", "Graphics\\NebulaNode ", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtNebulaNode.TtNebulaNodeData), DefaultNamePrefix = "Nebula")]
    public class TtNebulaNode : GamePlay.Scene.TtMeshNode 
    {
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mNebulaParticle);
            base.Dispose();
        }
        public override bool IsNoTick 
        {
            get => HasStyle(ENodeStyles.NoTick);
            set => base.IsNoTick = value; 
        }
        public class TtNebulaNodeData : GamePlay.Scene.TtMeshNode.TtMeshNodeData
        {
            public TtNebulaNodeData()
            {
                this.MdfQueueType = Rtti.TtTypeDesc.TypeStr(typeof(TtParticleMdfQueue));
            }
            public TtNebulaParticle NebulaParticle = null;
            [Rtti.Meta("")]
            public RName NebulaName { get; set; }
        }
        [Category("Option")]
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
                    mNebulaParticle = await TtEngine.Instance.NebulaTemplateManager.GetParticle(value);
                };
                action();
            }
        }
        TtNebulaParticle mNebulaParticle;
        public TtNebulaParticle NebulaParticle { get=> mNebulaParticle; }
        protected override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            if (GetNodeData<TtNebulaNodeData>().NebulaParticle != null)
                mNebulaParticle = GetNodeData<TtNebulaNodeData>().NebulaParticle; 
            else
                mNebulaParticle = await TtEngine.Instance.NebulaTemplateManager.GetParticle(GetNodeData<TtNebulaNodeData>().NebulaName);

            this.UnsetStyle(ENodeStyles.ParallelTick);
            return ret;
        }
        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            if (mNebulaParticle == null)
                return;
            base.OnGatherVisibleMeshes(rp);

            foreach (var i in mNebulaParticle.Emitter.Values)
            {
                if (i.Mesh != null)
                {
                    rp.AddVisibleMesh(i.Mesh);
                }
            }
            rp.AddVisibleNode(this);
        }
        protected override void OnCameralOffsetChanged(TtWorld world)
        {
            foreach (var i in mNebulaParticle.Emitter.Values)
            {
                if (i.Mesh == null)
                    continue;
                i.Mesh.UpdateCameraOffset(world);
            }
        }
        public override Profiler.TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtNebulaNode>.Scope;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            //var particleNode = policy.FindNode("ParticleNode") as UParticleGraphNode;
            var particleNode = args.Policy.FindFirstNode<TtParticleGraphNode>();
            if (particleNode == null || particleNode.IsUsed == false || NebulaParticle == null)
                return true;

            NebulaParticle.Update(args.Policy, particleNode, TtEngine.Instance.ElapsedSecond, new Vector3(this.Placement.AbsTransform.Position));

            return true;
        }
    }
}
