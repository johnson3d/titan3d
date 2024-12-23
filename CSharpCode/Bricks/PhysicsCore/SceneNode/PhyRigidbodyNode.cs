using EngineNS.Bricks.CodeBuilder;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using System.ComponentModel;

namespace EngineNS.Bricks.PhysicsCore.SceneNode
{

    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    [Bricks.CodeBuilder.ContextMenu("Rigidbody", "Collision\\Rigidbody", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPhyRigidbodyNode.TtPhyRigidbodyNodeData), DefaultNamePrefix = "Rigidbody")]
    //only contians one shape
    public class TtPhyRigidbodyNode : GamePlay.Scene.TtLightWeightNodeBase
    {
        [Rtti.Meta]
        public class TtPhyRigidbodyNodeData : GamePlay.Scene.TtNodeData
        {
            [Rtti.Meta]
            public EPhyActorType PhyActorType { get; set; } = EPhyActorType.PAT_Static;
            [Rtti.Meta]
            public float Mass { get; set; } = 1;
            [Rtti.Meta]
            public EPhyActorFlag ActorFlag { get; set; } = EPhyActorFlag.PAF_eVISUALIZATION;
            [Rtti.Meta]
            public PhyFilterData QueryFilterData { get; set; }
            [Rtti.Meta]
            public PhyFilterData SimulationFilterData { get; set; }
            [Rtti.Meta]
            public RName EventMacross { get; set; }
        }
        public TtPhyRigidbodyNodeData RigidbodyNodeData
        {
            get => NodeData as TtPhyRigidbodyNodeData;
        }
        public Bricks.PhysicsCore.TtPhyActor PhyActor { get; set; }
        [Category("Option")]
        public EPhyActorType PhyActorType 
        { 
            get => RigidbodyNodeData.PhyActorType;
            set
            {
                var oldValue = RigidbodyNodeData.PhyActorType;
                RigidbodyNodeData.PhyActorType = value;
                if(oldValue != value)
                {
                    OnActorTypeChange(value);
                }
            }
        }
        [Category("Option")]
        public float Mass { get => RigidbodyNodeData.Mass; set => RigidbodyNodeData.Mass = value; }
        [Category("Option")]
        public PhyFilterData QueryFilterData { get => RigidbodyNodeData.QueryFilterData; set => RigidbodyNodeData.QueryFilterData = value; }
        [Category("Option")]
        public PhyFilterData SimulationFilterData { get => RigidbodyNodeData.SimulationFilterData; set => RigidbodyNodeData.SimulationFilterData = value; }

        Macross.TtMacrossGetter<TtPhyEventMacrossBase> mMacrossGetter = null;
        public Macross.TtMacrossGetter<TtPhyEventMacrossBase> MacrossGetter
        {
            get
            {
                if (mMacrossGetter != null)
                {
                    return mMacrossGetter;
                }
                else
                {
                    if (EventMacross != null && !RName.IsEmpty(EventMacross))
                    {
                        var macrossGetter = Macross.TtMacrossGetter<TtPhyEventMacrossBase>.NewInstance();
                        macrossGetter.Name = EventMacross;
                        if (macrossGetter.Get() != null)
                        {
                            mMacrossGetter = macrossGetter;
                            return mMacrossGetter;
                        }
                    }
                    return null;
                }
            }
        }
        [RName.PGMacrossRName<TtPhyEventMacrossBase>(FilterExts = TtMacross.AssetExt)]
        [Category("Option")]
        public RName EventMacross
        {
            get
            {
                if (NodeData is TtPhyRigidbodyNodeData data)
                {
                    return data.EventMacross;
                }
                return null;
            }
            set
            {
                if (mMacrossGetter == null)
                {
                    mMacrossGetter = Macross.TtMacrossGetter<TtPhyEventMacrossBase>.NewInstance();
                }
                if (NodeData is TtPhyRigidbodyNodeData data)
                {
                    data.EventMacross = value;
                    if (value == null)
                    {
                        mMacrossGetter.Name = null;
                        return;
                    }
                }

                mMacrossGetter = Macross.TtMacrossGetter<TtPhyEventMacrossBase>.NewInstance();
                mMacrossGetter.Name = value;
                if (mMacrossGetter.Get() != null)
                {
                }
            }
        }
        public override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var baseResult = await base.InitializeNode(world, data, bvType, placementType);
            if (!baseResult)
                return false;

            CreatePhyActor();

            return true;
        }
        public virtual void OnActorTypeChange(EPhyActorType newType)
        {
            PhyActor.RemoveFromScene(ParentScene.PxSceneMB.PxScene);
            CreatePhyActor();
            if(Parent != null)
            {
                PhyActor.TagNode = Parent;
            }
            if(ParentScene != null && ParentScene.PxSceneMB.PxScene != null)
            {
                PhyActor.AddToScene(ParentScene.PxSceneMB.PxScene);
            }
        }
        public virtual TtPhyShape CreatePhyShape()
        {
            return null;
        }

        public void CreatePhyActor()
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            var transform = Placement.AbsTransform;
            PhyActor = pc.CreateActor(RigidbodyNodeData.PhyActorType, in transform.mPosition, in transform.mQuat);
            PhyActor.RigidBodyNode = this;
            PhyActor.mCoreObject.SetActorFlag(EPhyActorFlag.PAF_eVISUALIZATION, true);
            
            PhyActor.mCoreObject.SetMass(RigidbodyNodeData.Mass);
            PhyActor.mCoreObject.SetMinCCDAdvanceCoefficient(0);       
        }

        public override TtTask OnNodeLoaded(TtNode parent)
        {
            return base.OnNodeLoaded(parent);
        }

        protected override void OnParentChanged(TtNode prev, TtNode cur)
        {
            base.OnParentChanged(prev, cur);
            PhyActor.TagNode = Parent;
            Parent.UpdateAbsTransform();
            OnAbsTransformChanged();
            PhyActor.AddToScene(ParentScene.PxSceneMB.PxScene);
        }
        protected override void OnAbsTransformChanged()
        {
            var pxScene = ParentScene?.PxSceneMB.PxScene;
            if (pxScene != null)
            {
                if (PhyActor != null && pxScene.IsPxFetchingPose == false)
                {
                    ref var transform = ref Placement.AbsTransform;
                    PhyActor.SetPose2Physics(in transform.mPosition, in transform.mQuat, true);
                }
            }
            base.OnAbsTransformChanged();
        }
        protected override void OnParentSceneChanged(TtScene prev, TtScene cur)
        {
            base.OnParentSceneChanged(prev, cur);
            //TODO: Remove from Prev-Scene if exist
            PhyActor.AddToScene(ParentScene.PxSceneMB.PxScene);
        }

        public void OnContact(TtNode selfNode, TtNode otherNode)
        {
            MacrossGetter?.Get().OnContact(selfNode, otherNode);
        }
        public void OnBeginTrigger(TtNode selfNode, TtNode otherNode)
        {
            MacrossGetter?.Get().OnBeginTrigger(selfNode, otherNode);
        }
        public void OnEndTrigger(TtNode selfNode, TtNode otherNode)
        {
            MacrossGetter?.Get().OnEndTrigger(selfNode, otherNode);
        }

    }
}
