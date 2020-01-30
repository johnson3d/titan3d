using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;

namespace EngineNS.Bricks.PhysicsCore.CollisionComponent
{
    public enum CollisionLayer
    {
        None = 0,
        Static = 1 << 0,
        Dynamic = 1 << 1,
        Actor = 1 << 2,
        Camera = 1 << 3,
        Vehicle = 1 << 4,
        AirWall = 1 << 5,
        UI = 1 << 6,
        Water = 1 << 7,
        Custom1 = 1 << 8,
        Custom2 = 1 << 9,
        Custom3 = 1 << 10,
        Custom4 = 1 << 11,
    }
    [IO.Serializer.EnumSizeAttribute(typeof(IO.Serializer.Int32Enum))]
    public enum CollisionLayers
    {
        None = 0,
        Static = 1 << 0,
        Dynamic = 1 << 1,
        Actor = 1 << 2,
        Camera = 1 << 3,
        Vehicle = 1 << 4,
        AirWall = 1 << 5,
        UI = 1 << 6,
        Water = 1 << 7,
        Custom1 = 1 << 8,
        Custom2 = 1 << 9,
        Custom3 = 1 << 10,
        Custom4 = 1 << 11,
    }
    public enum CollisionResponse
    {
        Ignore,
        Overlap,
        Block,
    }
    public interface IPhysicsEventCallback
    {
        //void OnHit(CPhyActor otherActor);
        //void OnCursorIn();
        //void OnCursorOut();
        void OnTriggerIn(CPhyActor otherActor);
        void OnTriggerOut(CPhyActor otherActor);
        void OnTriggerIn(CPhyController otherController);
        void OnTriggerOut(CPhyController otherController);
    }
   
    [Rtti.MetaClass]
    public class GPhysicsCollisionComponentInitializer : GComponent.GComponentInitializer
    {
        private EPhyActorType mActorType = EPhyActorType.PAT_Static;

        [Rtti.MetaData]
        public bool IsEnableNavgation
        {
            get;
            set;
        } = false;

        [Rtti.MetaData]
        public EPhyActorType ActorType
        {
            get => mActorType;
            set
            {
                mActorType = value;
            }
        }
        Byte mAreaType = 0;
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Byte AreaType
        {
            get
            {
                return mAreaType;
            }
            set
            {
                if (value > 0 && value < 16)
                    mAreaType = value;
            }
        }

        [Rtti.MetaData]
        public virtual EPhysShapeType ShapeType
        {
            get;
            set;
        } = EPhysShapeType.PST_Unknown;
        bool mIsTrigger = false;
        [Rtti.MetaData]
        public bool IsTrigger
        {
            get => mIsTrigger;
            set
            {
                mIsTrigger = value;
            }
        }
        [Rtti.MetaData]
        public CollisionLayers SelfCollisionLayer { get; set; } = CollisionLayers.Static;
        [Rtti.MetaData]
        public CollisionLayers BlockCollisionLayers { get; set; } = CollisionLayers.Static | CollisionLayers.Dynamic | CollisionLayers.Camera | CollisionLayers.Actor;
        [Rtti.MetaData]
        [Editor.Editor_PackData()]
        public RName PhyMtlName
        {
            get;
            set;
        } = new RName("Physics/PhyMtl/default.phymtl", 0);
        [Rtti.MetaData]
        public GPlacementComponent.GPlacementComponentInitializer PlacementInitializer { get; set; }
        [Rtti.MetaData]
        public string SocketName { get; set; }
        [Rtti.MetaData]
        public bool DisableGravity { get; set; } = false;
        public bool Iskinematic { get; set; } = false;
    }
    //单Shape刚体的基类
    public class GPhysicsCollisionComponent : GamePlay.Component.GComponent, IPhysicsEventCallback, IPlaceable, Bricks.Animation.Skeleton.ISocketable
    {
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(McCollisionComponent))]
        public override RName ComponentMacross
        {
            get
            {
                return base.ComponentMacross;
            }
            set
            {
                base.ComponentMacross = value;
            }
        }
        [Browsable(false)]
        public GPhysicsCollisionComponentInitializer CollisionInitializer
        {
            get
            {
                var v = Initializer as GPhysicsCollisionComponentInitializer;
                if (v != null)
                    return v;
                return null;
            }
        }

        [DisplayName("是否作为寻路信息")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsEnableNavgation
        {
            get
            {
                if (CollisionInitializer != null)
                {
                    return CollisionInitializer.IsEnableNavgation;
                }
                return false;
            }
            set
            {
                if (CollisionInitializer != null)
                {
                    CollisionInitializer.IsEnableNavgation = value;
                }
            }
        }
        CollisionLayers mSelfLayer;
        public CollisionLayers SelfCollisionLayer
        {
            get => mSelfLayer;
            set
            {
                mSelfLayer = value;
                CollisionInitializer.SelfCollisionLayer = value;
                if (HostShape != null)
                {
                    PhyFilterData data = new PhyFilterData();
                    data.word0 = (uint)value;
                    data.word1 = (uint)BlockCollisionLayers;
                    SetQueryFilterData(HostShape, data);
                }
            }
        }

        CollisionLayers mBlockLayers;
        [EngineNS.Editor.Editor_FlagsEnumSetter]
        public CollisionLayers BlockCollisionLayers
        {
            get => mBlockLayers;
            set
            {
                mBlockLayers = value;
                CollisionInitializer.BlockCollisionLayers = value;
                if (HostShape != null)
                {
                    PhyFilterData data = new PhyFilterData();
                    data.word0 = (uint)SelfCollisionLayer;
                    data.word1 = (uint)value;
                    SetQueryFilterData(HostShape, data);
                }
            }
        }
        [Browsable(false)]
        protected IPlaceable HostPlaceable
        {
            get
            {
                if (HostContainer is IPlaceable)
                {
                    return HostContainer as IPlaceable;
                }
                else
                {
                    return Host as IPlaceable;
                }
            }
        }
        
        [Browsable(false)]
        public CPhyActor RigidBody { get; set; } = null;
        [Browsable(false)]
        public CPhyShape HostShape { get; set; } = null;
        private EPhyActorType mActorType = EPhyActorType.PAT_Static;
        public EPhyActorType ActorType
        {
            get => mActorType;
            set
            {
                mActorType = value;
                CollisionInitializer.ActorType = value;
                RecreatePhyActor();
            }
        }
        [Browsable(false)]
        public virtual EPhysShapeType ShapeType
        {
            get;
            set;
        } = EPhysShapeType.PST_Unknown;
        bool mIsTrigger = false;
        public bool IsTrigger
        {
            get => mIsTrigger;
            set
            {
                mIsTrigger = value;
                CollisionInitializer.IsTrigger = value;
                if (HostShape != null)
                {
                    SetIsTriggerOrNot(HostShape, value);
                }
            }
        }
        Byte mAreaType = 0;
        public Byte AreaType
        {
            get
            {
                return mAreaType;
            }
            set
            {
                if (value > 0 && value < 16)
                {
                    mAreaType = value;
                    CollisionInitializer.AreaType = value;
                }
            }
        }
        protected void SetIsTriggerOrNot(CPhyShape shape, bool isTrigger)
        {
            if (isTrigger)
            {
                shape.SetFlag(EPhysShapeFlag.PST_SIMULATION_SHAPE, false);
                shape.SetFlag(EPhysShapeFlag.PST_TRIGGER_SHAPE, true);
            }
            else
            {
                shape.SetFlag(EPhysShapeFlag.PST_TRIGGER_SHAPE, false);
                shape.SetFlag(EPhysShapeFlag.PST_SIMULATION_SHAPE, true);
            }
        }
        protected void SetQueryFilterData(CPhyShape shape, PhyFilterData data)
        {
            shape.SetQueryFilterData(ref data);
        }
        RName mPhyMtlName = new RName("Physics/PhyMtl/default.phymtl", 0);
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.PhyMaterial)]
        public RName PhyMtlName
        {
            get => mPhyMtlName;
            set
            {
                mPhyMtlName = value;
                CollisionInitializer.PhyMtlName = value;
            }
        }
        protected bool mDisableGravity = false;
        public bool DisableGravity
        {
            get => mDisableGravity;
            set
            {
                if (mDisableGravity == value)
                    return;
                mDisableGravity = value;
                CollisionInitializer.DisableGravity = value;
                RigidBody.SetActorFlag(EPhyActorFlag.eDISABLE_GRAVITY, value);
            }
        }
        protected bool mIskinematic = false;
        public bool Iskinematic
        {
            get => mIskinematic;
            set
            {
                if (mIskinematic == value)
                    return;
                mIskinematic = value;
                CollisionInitializer.Iskinematic = value;
                RigidBody.SetRigidBodyFlag(EPhyRigidBodyFlag.eKINEMATIC, value);
            }
        }
        GPlacementComponent mPlacement = new GPlacementComponent();
        public GPlacementComponent Placement
        {
            get => mPlacement;
            set
            {
                mPlacement = value;
            }
        }

        public GPhysicsCollisionComponent()
        {
            Placement.PropertyChanged += InnerPlacement_PropertyChanged;
        }

        ~GPhysicsCollisionComponent()
        {
            Placement.PropertyChanged -= InnerPlacement_PropertyChanged;
        }
        protected virtual void RefreshShape()
        {

        }
        protected virtual void SetShapeData(CPhyShape shape)
        {
            SetIsTriggerOrNot(shape, IsTrigger);
            PhyFilterData data = new PhyFilterData();
            data.word0 = (uint)SelfCollisionLayer;
            data.word1 = (uint)BlockCollisionLayers;
            SetQueryFilterData(shape, data);
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);
            var init = v as GPhysicsCollisionComponentInitializer;
            if (init.PlacementInitializer == null)
                init.PlacementInitializer = new GPlacementComponent.GPlacementComponentInitializer();
            await Placement.SetInitializer(rc, host, null, init.PlacementInitializer);
            Placement.PlaceableHost = this;
            mActorType = init.ActorType;
            mPhyMtlName = init.PhyMtlName;
            mIsTrigger = init.IsTrigger;
            mAreaType = init.AreaType;
            mDisableGravity = init.DisableGravity;
            SelfCollisionLayer = init.SelfCollisionLayer;
            BlockCollisionLayers = init.BlockCollisionLayers;

            HostPlaceable.Placement.PropertyChanged += OutterPlacement_PropertyChanged;
            //Placement.HostContainer = this;
            return true;
        }



        protected CPhyActor CreateCPhyActor(GActor host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            var init = v as GPhysicsCollisionComponentInitializer;
            PhyTransform phyTrans;
            phyTrans = EngineNS.Bricks.PhysicsCore.PhyTransform.CreateTransform(HostPlaceable.Placement.Location, HostPlaceable.Placement.Rotation);
            var phyActor = CEngine.Instance.PhyContext.CreateActor(init.ActorType, ref phyTrans);
            if (mDisableGravity)
                phyActor.SetActorFlag(EPhyActorFlag.eDISABLE_GRAVITY, true);
            if (Iskinematic)
                RigidBody.SetRigidBodyFlag(EPhyRigidBodyFlag.eKINEMATIC, true);
            phyActor.HostActor = host;
            RigidBody = phyActor;
            RigidBody.HostComponent = this;
            return phyActor;
        }
        protected virtual void RecreatePhyActor()
        {
            OnRemove();
            CreateCPhyActor(Host, HostContainer, Initializer);
            RefreshShape();
            OnAdded();
        }
        //内部placement缩放变化，通知更新shape
        private void InnerPlacement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Scale")
            {
                OnInnerScaleChanged();
            }
        }
        private void OutterPlacement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Scale")
            {
                OnInnerScaleChanged();
            }
        }
        protected void OnInnerScaleChanged()
        {
            RefreshShape();
        }
        #region IPlaceable
        Vector3 worldScale = Vector3.UnitXYZ;
        public void OnPlacementChanged(GamePlay.Component.GPlacementComponent placement)
        {
            if (placement == Placement)
            {
                //内部的placement回调 更新shape相对位置
                var relativePose = EngineNS.Bricks.PhysicsCore.PhyTransform.CreateTransform(Placement.Location * HostPlaceable.Placement.Scale, Placement.Rotation);
                HostShape.SetLocalPose(ref relativePose);
            }
            else
            {
                //hostActor位置改变，更新phyactor的位置
                var mat = HostPlaceable.Placement.WorldMatrix * SocketMatrix;
                Vector3 loc, scale;
                Quaternion rotate;
                mat.Decompose(out scale, out rotate, out loc);
                if (scale != worldScale)
                {
                    worldScale = scale;
                    OnInnerScaleChanged();
                }
                if (IsPlacementSetting)
                    return;
                RigidBody.SetPose2PhysicsFrom(loc, rotate);
            }
        }
        public void OnPlacementChangedUninfluencePhysics(GPlacementComponent placement)
        {
            //useless
        }
        #endregion
        #region add Remove Scene
        public override void OnAdded()
        {
            var sg = Host.Scene;
            if (sg != null)
            {
                RigidBody.AddToScene(sg.PhyScene);

            }
            if (HostPlaceable.Placement != null)
            {
                var mat = HostPlaceable.Placement.WorldMatrix * SocketMatrix;
                var trans = PhyTransform.CreateTransformFromMatrix(ref mat);
                RigidBody.SetPose2Physics(ref trans);
            }
            Host.mComponentFlags |= EngineNS.GamePlay.Actor.GActor.EComponentFlags.HasInstancing;
            base.OnAdded();
        }
        public override void OnAddedScene()
        {
            OnAdded();
        }

        public override void OnRemove()
        {
            RigidBody?.AddToScene(null);
            Host.mComponentFlags &= (~EngineNS.GamePlay.Actor.GActor.EComponentFlags.HasInstancing);
            base.OnRemove();
        }

        public override void OnRemoveScene()
        {
            OnRemove();
        }
        #endregion

        #region Transform Synchronization
        //bool TransformSynchronizationFromPhysics = false;
        bool SyncTransform = true;
        private bool IsPlacementSetting = false;
        public override void Tick(GPlacementComponent placement)
        {
            base.Tick(placement);
            if (!SyncTransform)
                return;
            if (RigidBody.PhyType == EPhyActorType.PAT_Dynamic)
            {
                IsPlacementSetting = true;
                HostPlaceable.Placement.Location = RigidBody.Position;
                HostPlaceable.Placement.Rotation = RigidBody.Rotation;
                Matrix drawMatrix;
                Matrix.Transformation(HostPlaceable.Placement.mPlacementData.mScale, HostPlaceable.Placement.mPlacementData.mRotation, HostPlaceable.Placement.mPlacementData.mLocation, out drawMatrix);
                HostPlaceable.Placement.DrawTransform = drawMatrix;
                //HostPlaceable.OnPlacementChangedUninfluencePhysics(placement);
                IsPlacementSetting = false;

            }
            if (DebugActor != null)
            {
                var shapeTrans = HostShape.GetLocalPose();
                mDebugActor.Placement.Rotation = shapeTrans.Q * RigidBody.Rotation;
                mDebugActor.Placement.Location = RigidBody.Rotation * shapeTrans.P + RigidBody.Position;
            }
        }

        #endregion

        #region IPhysicsEventCallback
        public void OnTriggerIn(CPhyActor otherActor)
        {
            CEngine.Instance.EventPoster.RunOn(() =>
            {
                mMcCompGetter?.CastGet<McCollisionComponent>()?.OnTriggerIn(this, otherActor.HostActor, otherActor.HostComponent as GComponent);
                return null;
            }, Thread.Async.EAsyncTarget.Logic);
        }
        public void OnTriggerOut(CPhyActor otherActor)
        {
            CEngine.Instance.EventPoster.RunOn(() =>
            {
                mMcCompGetter?.CastGet<McCollisionComponent>()?.OnTriggerOut(this, otherActor.HostActor, otherActor.HostComponent as GComponent);
                return null;
            }, Thread.Async.EAsyncTarget.Logic);
        }
        public void OnTriggerIn(CPhyController otherController)
        {
            CEngine.Instance.EventPoster.RunOn(() =>
            {
                mMcCompGetter?.CastGet<McCollisionComponent>()?.OnTriggerIn(this, otherController.HostComponent.Host, otherController.HostComponent as GComponent);
                return null;
            }, Thread.Async.EAsyncTarget.Logic);
        }
        public void OnTriggerOut(CPhyController otherController)
        {
            CEngine.Instance.EventPoster.RunOn(() =>
            {
                mMcCompGetter?.CastGet<McCollisionComponent>()?.OnTriggerOut(this, otherController.HostComponent.Host, otherController.HostComponent as GComponent);
                return null;
            }, Thread.Async.EAsyncTarget.Logic);
        }
        #endregion

        #region ISocketable
        string mSocketName = "";
        [Editor.Editor_SocketSelect]
        public string SocketName
        {
            get => mSocketName;
            set
            {
                if (value == "None" || string.IsNullOrEmpty(value))
                {
                    mSocketName = "";
                    if (ParentSocket != null)
                    {
                        ParentSocket.OnMeshSpaceMatrixChange -= OnCharacterSpaceMatrixChanged;
                        ParentSocket = null;
                        ((Bricks.Animation.Skeleton.GSocketPlacement)Placement).SocketCharacterSpaceMatrix = Matrix.Identity;
                        OnCharacterSpaceMatrixChanged(Matrix.Identity);
                    }
                }
                else if (mHostContainer is GMeshComponent)
                {
                    var hostContainer = mHostContainer as GMeshComponent;
                    var skin = hostContainer.SceneMesh.MdfQueue.FindModifier<Graphics.Mesh.CGfxSkinModifier>();
                    if (skin == null)
                        return;
                    var bone = skin.AnimationPose.FindBonePose(value);
                    if (bone == null)
                        return;
                    if (ParentSocket != null)
                        ParentSocket.OnMeshSpaceMatrixChange -= OnCharacterSpaceMatrixChanged;
                    ParentSocket = bone;
                    ParentSocket.OnMeshSpaceMatrixChange += OnCharacterSpaceMatrixChanged;
                    mSocketName = value;
                    OnCharacterSpaceMatrixChanged(ParentSocket.MeshSpaceMatrix);
                }
                var init = this.Initializer as GPhysicsCollisionComponentInitializer;
                if (init == null)
                    init.SocketName = value;
            }
        }
        protected Matrix SocketMatrix = Matrix.Identity;
        private void OnCharacterSpaceMatrixChanged(Matrix matrix)
        {
            if (HostPlaceable == null)
                return;
            SocketMatrix = matrix;
            var mat = SocketMatrix * HostPlaceable.Placement.WorldMatrix;
            Vector3 loc, scale;
            Quaternion rotate;
            mat.Decompose(out scale, out rotate, out loc);
            RigidBody.SetPose2PhysicsFrom(loc, rotate);
        }

        [Browsable(false)]
        public Bricks.Animation.Skeleton.IScocket ParentSocket { get; set; } = null;

        void KeepParentSocketValid()
        {
            if (ParentSocket != null)
            {
                var hostContainer = mHostContainer as GMeshComponent;
                var skin = hostContainer.SceneMesh.MdfQueue.FindModifier<Graphics.Mesh.CGfxSkinModifier>();
                if (skin == null)
                    return;
                var socket = ParentSocket as Bricks.Animation.Pose.CGfxBonePose;
                var bone = skin.AnimationPose.FindBonePose(socket.ReferenceBone.BoneDesc.NameHash);
                if (bone != socket)
                {
                    ParentSocket.OnMeshSpaceMatrixChange -= OnCharacterSpaceMatrixChanged;
                    ParentSocket = bone;
                    ParentSocket.OnMeshSpaceMatrixChange += OnCharacterSpaceMatrixChanged;
                }
            }
        }

        #endregion
        #region visual Debug
        protected GamePlay.Actor.GActor mDebugActor;
        public GamePlay.Actor.GActor DebugActor
        {
            get
            {
                return mDebugActor;
            }
        }

        protected EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new Thread.Async.TaskLoader.WaitContext();
        public async System.Threading.Tasks.Task AwaitLoadDebugActor()
        {
            await EngineNS.Thread.Async.TaskLoader.Awaitload(WaitContext);
        }

        public async System.Threading.Tasks.Task SetMaterial()
        {
            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(
                              CEngine.Instance.RenderContext,
                              RName.GetRName("editor/icon/icon_3D/material/physical_model.instmtl"));//rotator
            await AwaitLoadDebugActor();
            await mDebugActor.GetComponent<GamePlay.Component.GMeshComponent>().SetMaterialInstanceAsync(CEngine.Instance.RenderContext, 0, mtl, null);
        }

        protected bool mReady = false;
        public virtual void _OnEditorCommitVisual(CCommandList cmd, Graphics.CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            if (CEngine.PhysicsDebug == false)
            {
                if (IsEnableNavgation == false || CEngine.NavtionDebug == false)
                    return;
            }
        }
        public override void OnEditorCommitVisual(CCommandList cmd, Graphics.CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            if (CEngine.PhysicsDebug == false)
            {
                if (IsEnableNavgation == false || CEngine.NavtionDebug == false)
                    return;
            }
            _OnEditorCommitVisual(cmd, camera, param);
            if (mDebugActor != null)
            {
                foreach (var comp in mDebugActor.Components)
                {
                    var meshComp = comp as EngineNS.GamePlay.Component.GVisualComponent;
                    if (meshComp != null)
                        meshComp.CommitVisual(cmd, camera, param);
                }
            }
        }
        public async System.Threading.Tasks.Task CreateDefaultBox()
        {
            mDebugActor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("editor/basemesh/box_center.gms"));
            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
        }

        public async System.Threading.Tasks.Task CreateDefaultSphere()
        {
            mDebugActor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("editor/basemesh/sphere.gms"));
            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
        }
        public async System.Threading.Tasks.Task CreateDefaultCapsule()
        {
            mDebugActor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("editor/basemesh/capsule.gms"));
            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
        }
        #endregion
        //public virtual async Task<GamePlay.Actor.GActor> CreateActor(Editor.PlantableItemCreateActorParam param)
        //{
        //    var rc = EngineNS.CEngine.Instance.RenderContext;

        //    await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        //    var actor = new EngineNS.GamePlay.Actor.GActor();
        //    actor.ActorId = Guid.NewGuid();
        //    var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
        //    actor.Placement = placement;
        //    placement.Location = param.Location;

        //    var init = new GPhysicsComponentInitializer();
        //    init.SpecialName = "PhysicsData";
        //    await SetInitializer(rc, actor, actor, init);

        //    actor.AddComponent(this);
        //    return actor;
        //}
    }
}
