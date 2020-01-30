using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.SceneGraph;
using EngineNS.Graphics;

namespace EngineNS.GamePlay.Component
{
    public class MoveSolver
    {

    }
  
    [Rtti.MetaClassAttribute]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    [Editor.Editor_ComponentClassIconAttribute("icon/charactermovementcomponent_64x.txpic", RName.enRNameType.Editor)]
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GMovementComponentInitializer), "基础移动组件", "Movement", "MovementComponent")]
    public class GMovementComponent : GComponent, Bricks.AI.SteeringBehaviors.IBoid
    {
        [Rtti.MetaClass]
        public class GMovementComponentInitializer : GComponentInitializer
        {
            [Rtti.MetaData]
            public float Friction { get; set; } = 1.0f;
            [Rtti.MetaData]
            public float MovePrecision { get; set; } = 0.0001f;
            [Rtti.MetaData]
            public bool OrientThisRotation
            {
                get; set;
            } = true;
            [Rtti.MetaData]
            public bool ProcessOrientation
            {
                get; set;
            } = true;
        }
        protected float mFriction = 1.0f;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Friction
        {
            get => mFriction;
            set
            {
                mFriction = value;
                var init = Initializer as GMovementComponentInitializer;
                init.Friction = value;
            }
        }
        protected Bricks.AI.SteeringBehaviors.EnvironmentContext mEnvironmentContext;
        [Browsable(false)]
        public Bricks.AI.SteeringBehaviors.EnvironmentContext EnvironmentContext
        {
            get => mEnvironmentContext;
            set => mEnvironmentContext = value;
        }
        protected Vector3 mControllerInput = Vector3.Zero;
        [Browsable(false)]
        public Vector3 ControllerInput
        {
            get => mControllerInput;
            set { mControllerInput = value; HandleInputData(); }
        }

        protected float mMaxVelocity = 5.0f;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float MaxVelocity
        {
            get => mMaxVelocity;
            set => mMaxVelocity = value;
        }
        protected Vector3 mVelocity = Vector3.Zero;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Browsable(false)]
        public Vector3 Velocity
        {
            get => mVelocity;
            set => mVelocity = value;
        }
        protected Vector3 mDesireDirection = Vector3.Zero;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public virtual Vector3 DesireDirection
        {
            get => mDesireDirection;
            set => mDesireDirection = value;
        }

        protected Vector3 mOrientation = -Vector3.UnitZ;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public Vector3 Orientation
        {
            get => mOrientation;
            set => mOrientation = value;
        }
        protected Vector3 mDesireOrientation = -Vector3.UnitZ;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public Vector3 DesireOrientation
        {
            get => mDesireOrientation;
            set => mDesireOrientation = value;
        }
        protected Vector3 mPosition = Vector3.Zero;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public Vector3 Position
        {
            get => mPosition;
            set => mPosition = value;
        }
        protected float mMass = 1;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Mass
        {
            get => mMass;
            set => mMass = value;
        }
        protected bool mHasGravity = true;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool HasGravity
        {
            get => mHasGravity;
            set => mHasGravity = value;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Gravity { get; set; } = -9.8f;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsInAir { get; set; } = true;
        public bool OrientThisRotation
        {
            get => MovementComponentInitializer.OrientThisRotation;
            set => MovementComponentInitializer.OrientThisRotation = value;
        }
        protected bool mProcessOrientation = true;
        public bool ProcessOrientation
        {
            get => MovementComponentInitializer.ProcessOrientation;
            set => MovementComponentInitializer.ProcessOrientation = value;
        }
        protected float mMovePrecision = 0.0001f;

        public GMovementComponentInitializer MovementComponentInitializer
        {
            get=> Initializer as GMovementComponentInitializer;
        }
        public float MovePrecision
        {
            get => mMovePrecision;
            set
            {
                mMovePrecision = value;
                var init = Initializer as GMovementComponentInitializer;
                init.MovePrecision = value;
            }
        }
        public GMovementComponent()
        {
            OnlyForGame = false;
            this.Initializer = new GMovementComponentInitializer();
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);
            var initializer = v as GMovementComponentInitializer;
            Friction = initializer.Friction;
            MovePrecision = initializer.MovePrecision;
            return true;
        }
        public virtual void HandleInputData()
        {
            mDesireDirection = mControllerInput;
        }
        bool IsPlacementChanged = false;
        bool mIsDebug = false;
        private bool IsMoveStopped()
        {
            if (mDesireDirection == Vector3.Zero && IsPlacementChanged == false && mVelocity == Vector3.Zero)
                return true;
            return false;
        }
        public override void Tick(GPlacementComponent placement)
        {
            IsPlacementChanged = false;
            base.Tick(placement);
            CalculateMovement(placement, CEngine.Instance.EngineElapseTimeSecond);
            if (mPosition != placement.Location)
            {
                mPosition = placement.Location;
                IsPlacementChanged = true;
            }
            PredictFutureTrajectory(CEngine.Instance.EngineElapseTimeSecond);
            if (!mIsDebug)
                return;
            DebugTick(placement);
        }
        public virtual void Update(GPlacementComponent placement)
        {

        }
        public override void OnAddedScene()
        {
            if (Host.Scene.World == null)
                return;
            mEnvironmentContext.World = Host.Scene.World;
            if (!mIsDebug)
                return;
            Action action = async () =>
            {
                await InitDebug();
            };
            action.Invoke();
        }
        public void CalculateMovement(GPlacementComponent placement, float dtSecond)
        {
            var delta = ProcessingDisplacement(placement, dtSecond);
            var oldLoc = placement.Location;
            var phyCtrlCom = Host.GetComponent<Bricks.PhysicsCore.GPhyControllerComponent>();
            if (phyCtrlCom == null)
                OnTryMove(placement, ref delta, mMovePrecision, dtSecond);
            else
            {
                phyCtrlCom.OnTryMove(placement, ref delta, mMovePrecision, dtSecond);
                GetStateFromPhysicsController(phyCtrlCom, dtSecond);
            }
            var realLoc = placement.Location;
            mVelocity = (realLoc - oldLoc) / dtSecond;
            Update(placement);
            ProcessingOrientation(placement, dtSecond);

            //McMovementGetter?.OnMoving(!IsMoveStopped(), placement, ref delta, 0.0001f, dtSecond);
            if (!IsMoveStopped() && !lastStateIsMoving)
            {
                McMovementGetter?.OnMoving(true, placement, ref delta, 0.0001f, dtSecond);
            }
            if (IsMoveStopped() && lastStateIsMoving)
            {
                McMovementGetter?.OnMoving(false, placement, ref delta, 0.0001f, dtSecond);
            }
            lastStateIsMoving = IsMoveStopped();
        }
        bool lastStateIsMoving = false;
        public override bool OnTryMove(GPlacementComponent placement, ref Vector3 delta, float minDist, float elapsedTime)
        {
            placement.Location += delta;
            return true;
        }


        #region CalculateMovement
        protected Vector3 delta = new Vector3();
        protected float mGravityVelocity = 0;
        protected Vector3 mTempVelcity = Vector3.Zero;
        public virtual Vector3 ProcessingDisplacement(GPlacementComponent placement, float dtSecond)
        {
            mTempVelcity = mDesireDirection * mMaxVelocity;

            var phyCtrlCom = Host.GetComponent<Bricks.PhysicsCore.GPhyControllerComponent>();
            if (mHasGravity /*&& mIsInAir*/ && phyCtrlCom != null)
                mGravityVelocity = mGravityVelocity + Gravity * dtSecond;
            mTempVelcity.Y += mGravityVelocity;
            delta = mTempVelcity * dtSecond;
            return delta;
        }
        public virtual void ProcessingOrientation(GPlacementComponent placement, float dtSecond)
        {
            if (OrientThisRotation)
            {

                if (mProcessOrientation)
                {
                    if (mOrientation != Vector3.Zero)
                        mOrientation = Vector3.Slerp(mOrientation, mDesireOrientation, 0.8f);
                    else
                        mOrientation = mDesireOrientation;
                }
                if (mOrientation != Vector3.Zero)
                {

                    var rot = Quaternion.GetQuaternion(-Vector3.UnitZ, mOrientation);
                    if (placement.Rotation != rot)
                    {
                        placement.Rotation = rot;
                    }
                }
            }
        }
        public void GetStateFromPhysicsController(Bricks.PhysicsCore.GPhyControllerComponent phyCtrlCom, float dtSecond)
        {
            if (phyCtrlCom != null)
            {
                switch (phyCtrlCom.CollisionFlags)
                {
                    case Bricks.PhysicsCore.PhyControllerCollisionFlag.eCOLLISION_None:
                        {
                            IsInAir = true;
                        }
                        break;
                    case Bricks.PhysicsCore.PhyControllerCollisionFlag.eCOLLISION_DOWN:
                        {
                            IsInAir = false;
                            mGravityVelocity = 0;
                            mVelocity.Y = 0;
                        }
                        break;
                }
            }
        }
        #endregion

        #region PredictFutureTrajectory
        protected Vector3 mFutureVelocity = Vector3.Zero;
        protected Vector3 mFutureTempVelocity = Vector3.Zero;
        protected Vector3 mFutureDelta = Vector3.Zero;
        protected Vector3 mFutureLocation = Vector3.Zero;
        float mPredictTime = 1.0f;
        float mPredictionDelatTime = 1f / 20f;
        public void PredictFutureTrajectory(float dtSecond)
        {
            mFutureVelocity = mVelocity;
            mFutureLocation = mPosition;
            float totalTime = 0f;
            mFutureTrajectory.Clear();
            mFutureTrajectory.Enqueue(mPosition + Vector3.UnitY * 0.02f);
            while (totalTime < mPredictTime)
            {
                mFutureTrajectory.Enqueue(CalculatePredictionFuturePosition(mPredictionDelatTime));
                totalTime += mPredictionDelatTime;
            }
        }
        public virtual Vector3 CalculatePredictionFuturePosition(float dtSecond)
        {
            return Vector3.Zero;
        }
        #endregion

        #region Debug
        protected int TrackCount = 40;
        Support.NativeQueueForArray<Vector3> mPassTrajectory = new Support.NativeQueueForArray<Vector3>();
        Support.NativeQueueForArray<Vector3> mFutureTrajectory = new Support.NativeQueueForArray<Vector3>();
        [Browsable(false)]
        public Support.NativeQueueForArray<Vector3> FutureTrajectory
        {
            get => mFutureTrajectory;
        }
        TrajectoryDebug mPassTrajectoryDebug = new TrajectoryDebug();
        TrajectoryDebug mFutureTrajectoryDebug = new TrajectoryDebug();

        bool mIsInitDebug = false;
        protected async System.Threading.Tasks.Task InitDebug()
        {
            await CreateVelcityDebug();
            await mPassTrajectoryDebug.Init(RName.GetRName("TitanDemo/greentest.instmtl"), RName.GetRName("TitanDemo/Character/test_girl/material/greenwireframe.instmtl"));
            await mFutureTrajectoryDebug.Init(RName.GetRName("TitanDemo/redtest.instmtl"), RName.GetRName("TitanDemo/Character/test_girl/material/redwireframe.instmtl"));
            mPassTrajectoryDebug.Add2World(Host.Scene.World);
            mFutureTrajectoryDebug.Add2World(Host.Scene.World);
            mIsInitDebug = true;
        }

        Actor.GActor mVelcityDebugActor = null;
        Actor.GActor mDesireVelcityDebugActor = null;
        async System.Threading.Tasks.Task CreateVelcityDebug()
        {
            mVelcityDebugActor = await GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("TitanDemo/arrow.gms"));
            mVelcityDebugActor.SpecialName = "VelcityDebugActor";
            mVelcityDebugActor.Placement.Location = Host.Placement.Location + Vector3.UnitY;
            mVelcityDebugActor.Placement.Rotation = Host.Placement.Rotation;
            mVelcityDebugActor.Placement.Scale = Vector3.UnitXYZ;
            Host.Scene.World.AddActor(mVelcityDebugActor);
            Host.Scene.World.DefaultScene.AddActor(mVelcityDebugActor);

            mDesireVelcityDebugActor = await GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("TitanDemo/redarrow.gms"));
            mDesireVelcityDebugActor.SpecialName = "DesireVelcityDebugActor";
            mDesireVelcityDebugActor.Placement.Location = Host.Placement.Location + Vector3.UnitY;
            mDesireVelcityDebugActor.Placement.Scale = Vector3.UnitXYZ;
            Host.Scene.World.AddActor(mDesireVelcityDebugActor);
            Host.Scene.World.DefaultScene.AddActor(mDesireVelcityDebugActor);
        }
        void DebugTick(GPlacementComponent placement)
        {
            if (IsPlacementChanged)
            {
                if (!mIsInitDebug)
                    return;
                if (mPassTrajectory.Count > TrackCount)
                {
                    mPassTrajectory.Dequeue();
                }
                var p = mPosition + Vector3.UnitY * 0.02f;
                mPassTrajectory.Enqueue(p);
                if (mVelcityDebugActor != null)
                    mVelcityDebugActor.Placement.Location = mPosition + Vector3.UnitY;
                if (mDesireVelcityDebugActor != null)
                    mDesireVelcityDebugActor.Placement.Location = mPosition + Vector3.UnitY;
            }

            if (mVelcityDebugActor != null)
            {
                var v = mVelocity;
                v.Y = 0;

                if (v != Vector3.Zero)
                {
                    var rot = Quaternion.GetQuaternion(Vector3.UnitZ, v);
                    mVelcityDebugActor.Placement.Rotation = rot;
                    mVelcityDebugActor.Placement.Scale = Vector3.UnitXYZ + Vector3.UnitZ * (v.Length() - 1);
                }
                else
                {
                    mVelcityDebugActor.Placement.Scale = Vector3.Zero;
                }
            }
            if (mDesireVelcityDebugActor != null)
            {
                var v = mDesireDirection;
                v = Math.Min(mDesireDirection.Length(), mMaxVelocity) * mDesireDirection.NormalizeValue;
                v.Y = 0;
                if (v != Vector3.Zero)
                {
                    var rot = Quaternion.GetQuaternion(Vector3.UnitZ, v);
                    mDesireVelcityDebugActor.Placement.Rotation = rot;
                    mDesireVelcityDebugActor.Placement.Scale = Vector3.UnitXYZ + Vector3.UnitZ * (v.Length() - 1);
                }
                else
                {
                    mDesireVelcityDebugActor.Placement.Scale = Vector3.Zero;
                }
            }
            mPassTrajectoryDebug.SetVector3Points(mPassTrajectory);
            mFutureTrajectoryDebug.SetVector3Points(mFutureTrajectory);
            mPassTrajectoryDebug.Tick(placement);
            mFutureTrajectoryDebug.Tick(placement);
        }
        #endregion

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(McMovement))]
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
        private McMovement McMovementGetter
        {
            get
            {
                return mMcCompGetter?.CastGet<McMovement>(OnlyForGame);
            }
        }
    }
    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    [Editor.Editor_MacrossClassIconAttribute("icon/mcmoving_64.txpic", RName.enRNameType.Editor)]
    public class McMovement : McComponent
    {
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnMoving(bool isMoving, GPlacementComponent placement, ref Vector3 delta, float minDist, float elapsedTime)
        {

        }
    }

    public class TrajectoryDebug
    {
        GWorld mWorld = null;
        EngineNS.Bricks.GraphDrawer.GraphLines mGraphLines = new EngineNS.Bricks.GraphDrawer.GraphLines();
        EngineNS.Bricks.GraphDrawer.McMulLinesGen mLineGen = new Bricks.GraphDrawer.McMulLinesGen();
        Queue<Actor.GActor> mCachedPoints = new Queue<Actor.GActor>();
        List<Actor.GActor> mPoints = new List<Actor.GActor>();
        bool isInit = false;
        public TrajectoryDebug()
        {

        }
        public async System.Threading.Tasks.Task Init(RName lineMaterial, RName sphereMaterial)
        {
            var start = new Vector3(0, 0, 0);
            mGraphLines = new Bricks.GraphDrawer.GraphLines();
            mGraphLines.LinesGen = mLineGen;
            mGraphLines.LinesGen.Interval = 0;
            mGraphLines.LinesGen.Segement = 0.1f;
            mGraphLines.LinesGen.Start = start;
            mLineGen.SetVector3Points(new Vector3[] { start, start + Vector3.UnitX * 0.1f });
            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(
            CEngine.Instance.RenderContext,
            lineMaterial);
            await mGraphLines.Init(mtl, 0);

            mGraphLines.UpdateGeomMesh(CEngine.Instance.RenderContext, 0);
            mGraphLines.GraphActor.Placement.Location = Vector3.Zero;

            for (int i = 0; i < 50; ++i)
            {
                var acotr = await Actor.GActor.NewMeshActorAsync(RName.GetRName("TitanDemo/wirframemesh.gms"));
                acotr.Placement.Scale *= 0.02f;
                var mesh = acotr.GetComponent<GMeshComponent>();
                await mesh.SetMaterialInstance(CEngine.Instance.RenderContext, 0, sphereMaterial, null);
                mCachedPoints.Enqueue(acotr);
            }
            isInit = true;
        }
        public void Add2World(GWorld world)
        {
            world.AddActor(mGraphLines.GraphActor);
            world.DefaultScene.AddDynamicActor(mGraphLines.GraphActor);
            mWorld = world;
            var it = mCachedPoints.GetEnumerator();
            while (it.MoveNext())
            {
                AddPointActor2World(it.Current);
            }
            it.Dispose();
        }
        public void Add2World(GWorld world, Actor.GActor actor)
        {
            world.AddActor(actor);
            world.DefaultScene.AddDynamicActor(actor);
        }
        void AddPointActor2World(Actor.GActor actor)
        {
            Add2World(mWorld, actor);
        }
        void RemovePointActor2World(Actor.GActor actor)
        {

        }
        bool mIsDataChanged = false;
        Support.NativeQueueForArray<Vector3> Vector3Points;
        public void SetVector3Points(Support.NativeQueueForArray<Vector3> data)
        {
            if (!isInit)
                return;
            if (data.Count == 0)
                return;
            if (data.Count > mPoints.Count)
            {
                var count = data.Count - mPoints.Count;
                for (int i = 0; i < count; ++i)
                {
                    mPoints.Add(mCachedPoints.Dequeue());
                }
            }
            else if (data.Count < mPoints.Count)
            {
                var count = mPoints.Count - data.Count;
                for (int i = 0; i < count; ++i)
                {
                    var actor = mPoints[mPoints.Count - 1];
                    mPoints.Remove(actor);
                    mCachedPoints.Enqueue(actor);
                }
            }
            unsafe
            {
                mLineGen.UnsafeSetVector3Points((Vector3*)data.GetBufferPtr(), data.Count);
            }
            Vector3Points = data;
            mIsDataChanged = true;
        }
        public void Tick(GPlacementComponent placement)
        {
            if (mIsDataChanged)
            {
                mGraphLines.UpdateGeomMesh(EngineNS.CEngine.Instance.RenderContext, 0);
                var count = Math.Min(mPoints.Count, Vector3Points.Count);
                for (int i = 0; i < count; ++i)
                {
                    mPoints[i].Placement.Location = Vector3Points[i];
                }
                mIsDataChanged = false;
            }
            var it = mCachedPoints.GetEnumerator();
            while (it.MoveNext())
            {
                it.Current.Placement.Location = placement.Location;
            }
            it.Dispose();
        }


    }
}
