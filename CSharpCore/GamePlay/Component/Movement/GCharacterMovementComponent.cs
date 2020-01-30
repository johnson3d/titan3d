using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Component.Movement
{
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public enum MoveType
    {
        Idle,
        Walk,
        Jog,
        Run,
    }
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public enum StanceType
    {
        Creep,
        Crouch,
        Stand,
        Jump,
        Grab,
        Fly,
    }
    [Rtti.MetaClassAttribute]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GCharacterMovementComponentInitializer), "第三人称移动组件", "Movement", "CharacterMovementComponent")]
    [Editor.Editor_ComponentClassIconAttribute("icon/charactermovementcomponent_64x.txpic", RName.enRNameType.Editor)]
    public class GCharacterMovementComponent : GDynamicsMovementComponent
    {
        #region Initializer
        [Rtti.MetaClass]
        public class GCharacterMovementComponentInitializer : GDynamicsMovementComponentInitializer
        {
            [Rtti.MetaData]
            public float MaxRuningSpeed { get; set; } = 4f;
            [Rtti.MetaData]
            public float MaxJogingSpeed { get; set; } = 2f;
            [Rtti.MetaData]
            public float MaxWalkingSpeed { get; set; } = 1f;
            [Rtti.MetaData]
            public float AccelerationTime { get; set; } = 0.5f;
            [Rtti.MetaData]
            public bool HasGravity { get; set; } = false;
            [Rtti.MetaData]
            public Vector3 Gravity { get; set; } = new Vector3(0, -9.8f, 0);
        }
        [Browsable(false)]
        public GCharacterMovementComponentInitializer CharacterMovementInitializer
        {
            get
            {
                return Initializer as GCharacterMovementComponentInitializer;
            }
        }
        #endregion Initializer
        Vector3 mDesireDirection = Vector3.Zero;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 DesireDirection
        {
            get => mDesireDirection;
            set
            {
                mDesireDirection = value.NormalizeValue;
                StartProcessMoveType(DesireMoveType, value, AccelerationTime);
            }
        }
        MoveType mDesireMoveType = MoveType.Jog;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public MoveType DesireMoveType
        {
            get => mDesireMoveType;
            set
            {
                mDesireMoveType = value;
                StartProcessMoveType(value, DesireDirection, AccelerationTime);
            }
        }
        #region MoveType
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float MaxRuningSpeed
        {
            get => CharacterMovementInitializer.MaxRuningSpeed;
            set => CharacterMovementInitializer.MaxRuningSpeed = value;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float MaxJogingSpeed
        {
            get => CharacterMovementInitializer.MaxJogingSpeed;
            set => CharacterMovementInitializer.MaxJogingSpeed = value;
        }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float MaxWalkingSpeed
        {
            get => CharacterMovementInitializer.MaxWalkingSpeed;
            set => CharacterMovementInitializer.MaxWalkingSpeed = value;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float AccelerationTime
        {
            get => CharacterMovementInitializer.AccelerationTime;
            set => CharacterMovementInitializer.AccelerationTime = value;
        }
        public bool HasGravity
        {
            get => CharacterMovementInitializer.HasGravity;
            set => CharacterMovementInitializer.HasGravity = value;
        }
        public Vector3 Gravity
        {
            get => CharacterMovementInitializer.Gravity;
            set => CharacterMovementInitializer.Gravity = value;
        }
        #endregion MoveType
        #region ProcessMoveTypeChange
        bool mProcessingMoveTypeChange = false;
        float mProcessingMoveTypeChangeTime = 0.0f;
        void StartProcessMoveType(MoveType targetType, Vector3 direction, float time)
        {
            var targetSpeed = 0.0f;
            switch (DesireMoveType)
            {
                case MoveType.Idle:
                    {
                        targetSpeed = 0;
                    }
                    break;
                case MoveType.Walk:
                    {
                        targetSpeed = MaxWalkingSpeed;
                    }
                    break;
                case MoveType.Jog:
                    {
                        targetSpeed = MaxJogingSpeed;
                    }
                    break;
                case MoveType.Run:
                    {
                        targetSpeed = MaxRuningSpeed;
                    }
                    break;
            }
            if (IsKinematic)
            {
                LinearVelocity = targetSpeed * direction;
            }
            else
            {
                LinearAcceleration = (targetSpeed * direction - LinearVelocity) / time;
                mProcessingMoveTypeChange = true;
                mProcessingMoveTypeChangeTime = 0;

            }
        }
        void ProcessingMoveTypeChange(GPlacementComponent placement)
        {
            if (mProcessingMoveTypeChange)
            {
                mProcessingMoveTypeChangeTime += CEngine.Instance.EngineElapseTimeSecond;
                if (mProcessingMoveTypeChangeTime >= AccelerationTime)
                {
                    Advance(AccelerationTime - mProcessingMoveTypeChangeTime + CEngine.Instance.EngineElapseTimeSecond, placement);
                    LinearAcceleration = Vector3.Zero;

                    mProcessingMoveTypeChange = false;
                }
            }
        }
        #endregion ProcessMoveTypeChange
        public override void VelocityCalculate(float elapseTimeSecond, GPlacementComponent placement)
        {
            if (!IsKinematic)
            {
                if (HasGravity)
                {
                    mLinearVelocity += (LinearAcceleration + Gravity) * elapseTimeSecond;
                }
                else
                {
                    mLinearVelocity += LinearAcceleration * elapseTimeSecond;
                }
                mAngularVelocity += AngularAcceleration * elapseTimeSecond;
            }
            else
            {

            }

        }
        public override void UpdateDisplacement(GPlacementComponent placement, float elapseTimeSecond, Vector3 linearDelta, Vector3 angularDelta)
        {
            var phyCtrlCom = Host.GetComponent<Bricks.PhysicsCore.GPhyControllerComponent>();
            if (phyCtrlCom == null)
                placement.Location += linearDelta;
            else
            {
                var oldLoc = placement.Location;
                phyCtrlCom.OnTryMove(placement, ref linearDelta, LinearMovePrecision, elapseTimeSecond);
                GetStateFromPhysicsController(phyCtrlCom, elapseTimeSecond);
                var realLoc = placement.Location;
                var tempV = (realLoc - oldLoc) / elapseTimeSecond;
                mLinearVelocity = (realLoc - oldLoc) / elapseTimeSecond;
            }

            mPosition = placement.Location;

            if (OrientedVelocity)
            {
                var v = mLinearVelocity;
                v.Y = 0;
                if (v.Length() > LinearMovePrecision)
                {
                    var rotation = Quaternion.GetQuaternion(-Vector3.UnitZ, v.NormalizeValue);
                    placement.Rotation = Quaternion.Slerp(placement.Rotation, rotation, 0.5f);
                }
            }
            else
            {
                var rotation = Quaternion.FromEuler(angularDelta);
                mEulerRotation = placement.Rotation.ToEuler() + angularDelta;
                mQuaternionRotation = Quaternion.FromEuler(mEulerRotation);
                placement.Rotation = mQuaternionRotation;
            }
        }
        public override void Tick(GPlacementComponent placement)
        {
            if (!IsKinematic)
                ProcessingMoveTypeChange(placement);
            base.Tick(placement);
        }
    }
}
