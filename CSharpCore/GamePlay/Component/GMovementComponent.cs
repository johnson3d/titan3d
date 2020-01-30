using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.SceneGraph;
using EngineNS.Graphics;

namespace EngineNS.GamePlay.Component
{
    [Rtti.MetaClassAttribute]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GMovementComponentInitializer), "移动组件", "Movement Component")]
    public class GMovementComponent : GComponent, AI.SteeringBehaviors.IBoid
    {
        [Rtti.MetaClass]
        public class GMovementComponentInitializer : GComponentInitializer
        {

        }
        protected float mMaxVelocity = 2.0f;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float MaxVelocity
        {
            get => mMaxVelocity;
            set => mMaxVelocity = value;
        }
        protected Vector3 mVelocity = Vector3.Zero;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public Vector3 Velocity
        {
            get => mVelocity;
            set => mVelocity = value;
        }
        protected Vector3 mDesireVelcity = Vector3.Zero;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 DesireVelcity
        {
            get => mDesireVelcity;
            set => mDesireVelcity = value;
        }
        protected Vector3 mOrientation = -Vector3.UnitZ;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Orientation
        {
            get => mOrientation;
            set => mOrientation = value;
        }
        protected Vector3 mPosition = Vector3.Zero;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
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
        protected float mGravity = -9.8f;
        protected bool mIsInAir = true;
        protected bool mOrientThisRotation = true;
        public GMovementComponent()
        {
            this.Initializer = new GMovementComponentInitializer();
        }
        protected Vector3 delta = new Vector3();
        protected Vector3 mLastDirection;
        protected float mGravityVelocity = 0;
        protected Vector3 mTempVelcity = Vector3.Zero;

        public virtual void CalculateVelocity(GPlacementComponent placement, float dtSecond)
        {
            var phyCtrlCom = Host.GetComponent<Bricks.PhysicsCore.GPhyControllerComponent>();
            if (phyCtrlCom == null)
            {
                placement.Location += mVelocity * CEngine.Instance.EngineElapseTimeSecond;
            }
            if (mHasGravity /*&& mIsInAir*/ && phyCtrlCom != null)
                mGravityVelocity = mGravityVelocity + mGravity * CEngine.Instance.EngineElapseTimeSecond;
            mVelocity.Y += mGravityVelocity;
            delta = mVelocity * CEngine.Instance.EngineElapseTimeSecond;

            var oldLoc = placement.Location;
            placement?.TryMove(ref delta, 0.0001f, CEngine.Instance.EngineElapseTimeSecond);
            if (mOrientThisRotation)
            {
                if (mOrientation != Vector3.Zero)
                {
                    var rot = Quaternion.GetQuaternion(-Vector3.UnitZ, mOrientation);
                    if (placement.Rotation != rot)
                    {
                        //placement.Rotation = rot;

                    }
                }
            }

            if (phyCtrlCom != null)
            {
                switch (phyCtrlCom.CollisionFlags)
                {
                    case Bricks.PhysicsCore.PhyControllerCollisionFlag.eCOLLISION_None:
                        {
                            mIsInAir = true;
                        }
                        break;
                    case Bricks.PhysicsCore.PhyControllerCollisionFlag.eCOLLISION_DOWN:
                        {
                            mIsInAir = false;
                            mGravityVelocity = 0;
                            mVelocity.Y = 0;
                        }
                        break;
                }
            }

        }
        bool IsPlacementChanged = false;
        public override void Tick(GPlacementComponent placement)
        {
            IsPlacementChanged = false;
            base.Tick(placement);
            mTempVelcity = mVelocity;
            PredictFutureTrajectory(CEngine.Instance.EngineElapseTimeSecond);
            CalculateVelocity(placement, CEngine.Instance.EngineElapseTimeSecond);
            if (mPosition != placement.Location)
            {
                mPosition = placement.Location;
                IsPlacementChanged = true;
            }
            DebugTick(placement);
        }
        public override void OnAddedScene()
        {
            var nu = InitDebug();
        }
        #region PredictFutureTrajectory
        protected Vector3 mFutureVelocity = Vector3.Zero;
        protected Vector3 mFutureTempVelocity = Vector3.Zero;
        protected Vector3 mFutureDelta = Vector3.Zero;
        protected Vector3 mFutureLocation = Vector3.Zero;
        float mPredictTime = 5.0f;
        float mPredictionDelatTime = 1f / 30f;
        public void PredictFutureTrajectory(float dtSecond)
        {
            mFutureVelocity = mVelocity;
            mFutureLocation = mPosition;
            float totalTime = 0f;
            mFutureTrajectory.Clear();
            unsafe
            {
                while (totalTime < mPredictTime)
                {
                    //mFutureTrajectory.Enqueue(CalculatePredictionFuturePosition(mPredictionDelatTime));
                    var posTemp = CalculatePredictionFuturePosition(mPredictionDelatTime);
                    mFutureTrajectory.Enqueue(posTemp);
                    totalTime += mPredictionDelatTime;
                }
            }
        }
        public virtual Vector3 CalculatePredictionFuturePosition(float dtSecond)
        {
            return Vector3.Zero;
        }
        #endregion
        protected int TrackCount = 40;
        Support.NativeQueueForArray<Vector3> mPassTrajectory = new Support.NativeQueueForArray<Vector3>();
        Support.NativeQueueForArray<Vector3> mFutureTrajectory = new Support.NativeQueueForArray<Vector3>();
        public IntPtr FutureTrajectory
        {
            get
            {
                return mFutureTrajectory.GetBufferPtr();
            }
        }
        TrajectoryDebug mPassTrajectoryDebug = new TrajectoryDebug();
        TrajectoryDebug mFutureTrajectoryDebug = new TrajectoryDebug();

        bool mIsInitDebug = false;
        protected async System.Threading.Tasks.Task InitDebug()
        {
            await CreateVelcityDebug();
            await mPassTrajectoryDebug.Init(RName.GetRName("TitanDemo/greentest.instmtl"));
            await mFutureTrajectoryDebug.Init(RName.GetRName("TitanDemo/redtest.instmtl"));
            mPassTrajectoryDebug.Add2World(Host.Scene.World);
            mFutureTrajectoryDebug.Add2World(Host.Scene.World);
            mIsInitDebug = true;
        }

        Actor.GActor mVelcityDebugActor = null;
        Actor.GActor mDesireVelcityDebugActor = null;
        async System.Threading.Tasks.Task CreateVelcityDebug()
        {
            mVelcityDebugActor =await GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("TitanDemo/arrow.gms"));
            mVelcityDebugActor.SpecialName = "VelcityDebugActor";
            mVelcityDebugActor.Placement.Location = Host.Placement.Location + Vector3.UnitY;
            mVelcityDebugActor.Placement.Rotation = Host.Placement.Rotation;
            mVelcityDebugActor.Placement.Scale= Vector3.UnitXYZ;
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
                //mPassTrajectory.Enqueue(p);
                unsafe
                {
                    mPassTrajectory.Enqueue(p);
                }
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
                    mVelcityDebugActor.Placement.Scale = Vector3.UnitXYZ + Vector3.UnitZ * (v.Length()-1);
                }
                else
                {
                    mVelcityDebugActor.Placement.Scale = Vector3.Zero;
                }
            }
            if (mDesireVelcityDebugActor != null)
            {
                var v = mDesireVelcity;
                v = Math.Min(mDesireVelcity.Length(), mMaxVelocity) * mDesireVelcity.NormalizeValue;
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
            unsafe
            {
                mPassTrajectoryDebug.UnsafeSetVector3Points((Vector3*)mPassTrajectory.GetBufferPtr().ToPointer(), mPassTrajectory.Count);
                mFutureTrajectoryDebug.UnsafeSetVector3Points((Vector3*)mFutureTrajectory.GetBufferPtr().ToPointer(), mFutureTrajectory.Count);
            }
            mPassTrajectoryDebug.Tick();
            mFutureTrajectoryDebug.Tick();
        }
    }

    public class TrajectoryDebug
    {
        EngineNS.Bricks.GraphDrawer.GraphLines mGraphLines = new EngineNS.Bricks.GraphDrawer.GraphLines();
        EngineNS.Bricks.GraphDrawer.McMulLinesGen mLineGen = new Bricks.GraphDrawer.McMulLinesGen();
        public TrajectoryDebug()
        {

        }
        public async System.Threading.Tasks.Task Init(RName material)
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
                material);
            if(mtl==null)
            {
                return;
            }
            await mGraphLines.Init(mtl, 0);

            mGraphLines.UpdateGeomMesh(CEngine.Instance.RenderContext, 0);
            mGraphLines.GraphActor.Placement.Location = Vector3.Zero;

        }
        public void Add2World(GWorld world)
        {
            world.AddActor(mGraphLines.GraphActor);
            world.DefaultScene.AddDynamicActor(mGraphLines.GraphActor);
        }
        bool mIsDataChanged = false;
        public unsafe void UnsafeSetVector3Points(Vector3* ptr, int count)
        {
            if (count == 0)
                return;
            mLineGen.UnsafeSetVector3Points(ptr, count);
            mIsDataChanged = true;
        }
        public void SetVector3Points(Vector3[] data)
        {
            if (data.Length == 0)
                return;
            mLineGen.SetVector3Points(data);
            mIsDataChanged = true;
        }
        public void Tick()
        {
            if (mIsDataChanged)
            {
                mGraphLines.UpdateGeomMesh(EngineNS.CEngine.Instance.RenderContext, 0);
                mIsDataChanged = false;
            }
        }


    }
}
