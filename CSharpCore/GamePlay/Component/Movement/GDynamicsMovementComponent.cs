using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.GamePlay.Component.Movement
{
    [Rtti.MetaClassAttribute]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GDynamicsMovementComponentInitializer), "动力学移动组件", "Movement", "DynamicsMovementComponent")]
    public class GDynamicsMovementComponent : GComponent
    {
        #region Initialilzer
        [Rtti.MetaClass]
        public class GDynamicsMovementComponentInitializer : GComponentInitializer
        {
            [Rtti.MetaData]
            public bool IsKinematic { get; set; } = false;
            [Rtti.MetaData]
            public bool OrientedVelocity { get; set; } = false;
            [Rtti.MetaData]
            public bool IsCCD { get; set; } = false;
            [Rtti.MetaData]
            public float CCDAdvanceDistance { get; set; } = 0.01f;
            [Rtti.MetaData]
            protected float mLinearMovePrecision { get; set; } = 0.0001f;
            [Rtti.MetaData]
            public float LinearMovePrecision
            {
                get => mLinearMovePrecision;
                set
                {
                    mLinearMovePrecision = value;
                }
            }
            protected float mAngularMovePrecision = 0.0001f;
            [Rtti.MetaData]
            public float AngularMovePrecision
            {
                get => mAngularMovePrecision;
                set
                {
                    mAngularMovePrecision = value;
                }
            }
            [Rtti.MetaData]
            public Vector3 MaxLinearVelocity { get; set; } = Vector3.Zero;
            [Rtti.MetaData]
            public Vector3 MaxAngularVelocity { get; set; } = Vector3.Zero;
            [Rtti.MetaData]
            public Vector3 MaxLinearAcceleration { get; set; } = Vector3.Zero;
            [Rtti.MetaData]
            public Vector3 MaxAngularAcceleration { get; set; } = Vector3.Zero;
        }
        [Browsable(false)]
        public GDynamicsMovementComponentInitializer MovementInitializer
        {
            get
            {
                return Initializer as GDynamicsMovementComponentInitializer;
            }
        }
        #endregion Initialilzer
        #region properties
        protected Vector3 mPosition = Vector3.Zero;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Category("Common")]
        public Vector3 Position
        {
            get => mPosition;
            set
            {
                mPosition = value;
            }
        }
        protected Vector3 mEulerRotation = Vector3.Zero;
        /// <summary>
        /// 角度
        /// </summary>
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Category("Common")]
        public Vector3 EulerRotation
        {
            get => mEulerRotation;
            set
            {
                mEulerRotation = value;
            }
        }
        protected Quaternion mQuaternionRotation = Quaternion.Identity;
        [Browsable(false)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Quaternion QuaternionRotation
        {
            get => mQuaternionRotation;
            set
            {
                mQuaternionRotation = value;
            }
        }
        protected Vector3 mLinearVelocity = Vector3.Zero;
        [Category("Common")]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 LinearVelocity
        {
            get => mLinearVelocity;
            set
            {
                if (MaxLinearVelocity != Vector3.Zero)
                    mLinearVelocity = Vector3.Minimize(value, MaxLinearVelocity);
                else
                    mLinearVelocity = value;
            }
        }

        protected Vector3 mAngularVelocity = Vector3.Zero;
        [Category("Common")]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 AngularVelocity
        {
            get => mAngularVelocity;
            set
            {
                if (MaxAngularVelocity != Vector3.Zero)
                    mAngularVelocity = Vector3.Minimize(value, MaxAngularVelocity);
                else
                    mAngularVelocity = value;
            }
        }
        protected Vector3 mLinearAcceleration = new Vector3(0, 0, 0);
        [Category("Common")]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 LinearAcceleration
        {
            get => mLinearAcceleration;
            set
            {
                if (MaxLinearAcceleration != Vector3.Zero)
                    mLinearAcceleration = Vector3.Minimize(value, MaxLinearAcceleration);
                else
                    mLinearAcceleration = value;
                if (mLinearAcceleration.Length() < LinearMovePrecision)
                {
                    mLinearAcceleration = Vector3.Zero;
                }
            }
        }
        protected Vector3 mAngularAcceleration = Vector3.Zero;
        [Category("Common")]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 AngularAcceleration
        {
            get => mAngularAcceleration;
            set
            {
                if (MaxAngularAcceleration != Vector3.Zero)
                    mAngularAcceleration = Vector3.Minimize(value, MaxAngularAcceleration);
                else
                    mAngularAcceleration = value;
                if (mAngularAcceleration.Length() < AngularMovePrecision)
                {
                    mAngularAcceleration = Vector3.Zero;
                }
            }
        }
        [Category("Common")]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool OrientedVelocity
        {
            get => MovementInitializer.OrientedVelocity;
            set => MovementInitializer.OrientedVelocity = value;
        }
        [Category("Common")]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsKinematic
        {
            get => MovementInitializer.IsKinematic;
            set => MovementInitializer.IsKinematic = value;
        }
        [Category("Common")]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsCCD
        {
            get => MovementInitializer.IsCCD;
            set => MovementInitializer.IsCCD = value;
        }
        [Category("Common")]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float CCDAdvanceDistance
        {
            get => MovementInitializer.CCDAdvanceDistance;
            set => MovementInitializer.CCDAdvanceDistance = value;
        }
        [Category("Common")]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public bool IsMoving
        {
            get
            {
                if (IsKinematic)
                {
                    if (mLinearVelocity.Length() < LinearMovePrecision)
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    if (mLinearVelocity.Length() < LinearMovePrecision && mLinearAcceleration.Length() < LinearMovePrecision)
                    {
                        return false;
                    }
                    return true;
                }
            }
        }
        [Category("Common")]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public bool IsRotating
        {
            get
            {
                if (IsKinematic)
                {
                    if (mAngularVelocity.Length() < AngularMovePrecision)
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    if (mAngularVelocity.Length() < AngularMovePrecision && mAngularAcceleration.Length() < AngularMovePrecision)
                    {
                        return false;
                    }
                    return true;
                }
            }
        }
        #endregion properties
        #region limit
        [Category("Limit")]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float LinearMovePrecision
        {
            get => MovementInitializer.LinearMovePrecision;
            set
            {
                MovementInitializer.LinearMovePrecision = value;
            }
        }
        [Category("Limit")]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float AngularMovePrecision
        {
            get => MovementInitializer.AngularMovePrecision;
            set
            {
                MovementInitializer.AngularMovePrecision = value;
            }
        }
        [Category("Limit")]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 MaxLinearVelocity
        {
            get => MovementInitializer.MaxLinearVelocity;
            set => MovementInitializer.MaxLinearVelocity = value;
        }
        [Category("Limit")]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 MaxAngularVelocity
        {
            get => MovementInitializer.MaxAngularVelocity;
            set => MovementInitializer.MaxAngularVelocity = value;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Category("Limit")]
        public Vector3 MaxLinearAcceleration
        {
            get => MovementInitializer.MaxLinearAcceleration;
            set => MovementInitializer.MaxLinearAcceleration = value;
        }
        [Category("Limit")]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 MaxAngularAcceleration
        {
            get => MovementInitializer.MaxAngularAcceleration;
            set => MovementInitializer.MaxAngularAcceleration = value;
        }

        #endregion limit
        #region McComponent
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(McDynamicsMovement))]
        [Category("Common")]
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
        private McDynamicsMovement McMovementGetter
        {
            get
            {
                return mMcCompGetter?.CastGet<McDynamicsMovement>(OnlyForGame);
            }
        }
        #endregion McComponent
        public GDynamicsMovementComponent()
        {
            OnlyForGame = true;
            this.Initializer = new GDynamicsMovementComponentInitializer();
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);
            return true;
        }
        public override void Tick(GPlacementComponent placement)
        {
            var t = EngineNS.CEngine.Instance.EngineElapseTimeSecond;
            if (IsCCD)
            {
                //以现有速度估算 也可以加入加速度估算
                var count = (mLinearVelocity * t).Length() / CCDAdvanceDistance;
                if (count != 0)
                {
                    var intCount = (int)count;
                    var perT = (int)(t * 1000) / intCount;
                    var leftT = (int)(t * 1000) % intCount;
                    for (int i = 0; i < intCount; ++i)
                    {
                        Advance(perT * 0.001f, placement);
                    }
                    Advance(leftT * 0.001f, placement);
                }
                else
                {
                    Advance(t, placement);
                }
            }
            else
            {
                Advance(t, placement);
            }
        }
        public virtual void VelocityCalculate(float elapseTimeSecond, GPlacementComponent placement)
        {
            if (!IsKinematic)
            {
                mLinearVelocity += LinearAcceleration * elapseTimeSecond;
                mAngularVelocity += AngularAcceleration * elapseTimeSecond;
            }
            else
            {

            }
        }
        public void Advance(float elapseTimeSecond, GPlacementComponent placement)
        {
            VelocityCalculate(elapseTimeSecond, placement);
            if (mLinearVelocity.Length() < LinearMovePrecision)
            {
                mLinearVelocity = Vector3.Zero;
            }
            if (mAngularVelocity.Length() < AngularMovePrecision)
            {
                mAngularVelocity = Vector3.Zero;
            }
            if (MaxLinearVelocity != Vector3.Zero)
                mLinearVelocity = Vector3.Minimize(mLinearVelocity, MaxLinearVelocity);
            if (MaxAngularVelocity != Vector3.Zero)
                mAngularVelocity = Vector3.Minimize(mAngularVelocity, MaxAngularVelocity);

            var linearDelta = mLinearVelocity * elapseTimeSecond;
            var angularDelta = mAngularVelocity * elapseTimeSecond;
            UpdateDisplacement(placement, elapseTimeSecond, linearDelta, angularDelta);
            McMovementGetter?.OnAdvance(placement, linearDelta, angularDelta, elapseTimeSecond);
            if (IsMoving && !lastStateIsMoving)
            {
                McMovementGetter?.OnMoving(true, placement, linearDelta, LinearMovePrecision, elapseTimeSecond);
            }
            if (!IsMoving && lastStateIsMoving)
            {
                McMovementGetter?.OnMoving(false, placement, linearDelta, LinearMovePrecision, elapseTimeSecond);
            }
            lastStateIsMoving = IsMoving;

            if (IsRotating && !lastStateIsRotating)
            {
                McMovementGetter?.OnRotating(true, placement, angularDelta, AngularMovePrecision, elapseTimeSecond);
            }
            if (!IsRotating && lastStateIsRotating)
            {
                McMovementGetter?.OnRotating(true, placement, angularDelta, AngularMovePrecision, elapseTimeSecond);
            }
            lastStateIsRotating = IsRotating;
        }
        bool mIsInAir;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public bool IsInAir
        {
            get => mIsInAir;
        }

        public void GetStateFromPhysicsController(Bricks.PhysicsCore.GPhyControllerComponent phyCtrlCom, float dtSecond)
        {
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
                        }
                        break;
                }
            }
        }
        bool lastStateIsMoving = false;
        bool lastStateIsRotating = false;
        public virtual void UpdateDisplacement(GPlacementComponent placement, float elapseTimeSecond, Vector3 linearDelta, Vector3 angularDelta)
        {
            var phyCtrlCom = Host.GetComponent<Bricks.PhysicsCore.GPhyControllerComponent>();
            if (phyCtrlCom == null)
                placement.Location += linearDelta;
            else
            {
                var oldLoc = placement.Location;
                phyCtrlCom.OnTryMove(placement, ref linearDelta, LinearMovePrecision, elapseTimeSecond);
                GetStateFromPhysicsController(phyCtrlCom, elapseTimeSecond);
            }
            mPosition = placement.Location;

            var v = mLinearVelocity;
            v.Y = 0;
            if (v.Length() > LinearMovePrecision)
            {
                var rotation = Quaternion.GetQuaternion(-Vector3.UnitZ, v.NormalizeValue);
                placement.Rotation = rotation;
            }
            else
            {
                var rotation = Quaternion.FromEuler(angularDelta);
                mEulerRotation = placement.Rotation.ToEuler() + angularDelta;
                mQuaternionRotation = Quaternion.FromEuler(mEulerRotation);
                placement.Rotation = mQuaternionRotation;
            }
        }
    }
    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    [Editor.Editor_MacrossClassIconAttribute("icon/mcmoving_64.txpic", RName.enRNameType.Editor)]
    public class McDynamicsMovement : McComponent
    {
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnAdvance(GPlacementComponent placement, Vector3 linearDelta, Vector3 angularDelta, float elapsedTime)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnMoving(bool isMoving, GPlacementComponent placement, Vector3 delta, float minDist, float elapsedTime)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnRotating(bool isRotating, GPlacementComponent placement, Vector3 delta, float minDist, float elapsedTime)
        {

        }
    }
}
