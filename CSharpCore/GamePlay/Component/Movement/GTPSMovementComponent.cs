using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Component.Movement
{
    [Rtti.MetaClass]
    public class GTPSMovementComponentInitializer : GDynamicMovementComponent.GDynamicMovementComponentInitializer
    {

    }
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public enum TPSStanceType
    {
        Creep,
        Crouch,
        Stand,
        Jump,
        Grab,
        Fly,
    }
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public enum TPSMoveType
    {
        Idle,
        Walk,
    }
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public enum TPSArmType
    {
        DisArm,
        Releax,
        Aim,
    }
    [Rtti.MetaClassAttribute]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class GTPSMovementComponent : GDynamicMovementComponent
    {
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public TPSStanceType StanceType { get; set; } = TPSStanceType.Stand;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public TPSMoveType MoveType { get; set; } = TPSMoveType.Idle;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public TPSArmType ArmType { get; set; } = TPSArmType.Releax;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float MaxSpeed_Creep { get; set; } = 0.3f;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float MaxSpeed_Crouch { get; set; } = 1.0f;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float MaxSpeed_Walk { get; set; } = 2f;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float MaxSpeed_Jog { get; set; } = 2.5f;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float MaxSpeed_Run { get; set; } = 3.5f;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public Vector3 LookAtDir { get; set; } = -Vector3.UnitZ;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public Vector3 MovementDir { get; set; } = -Vector3.UnitZ;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public float MovementSpeed { get; set; } = 0;
        public float BrakeForce { get; set; } = 200;
        public GTPSMovementComponent()
        {

        }

        public override void Update(GPlacementComponent placement)
        {
            base.Update(placement);
            var lookAtDir2D = LookAtDir;
            lookAtDir2D.Y = 0;
            var velocity = mVelocity;
            velocity.Y = 0;
            velocity.Normalize();
            if (mVelocity.LengthSquared() == 0)
            {
                MovementDir = (placement.Rotation * -Vector3.UnitZ).NormalizeValue;
                MovementSpeed = MathHelper.FloatLerp(MovementSpeed, mVelocity.Length(), 0.5f);
                if (MovementSpeed > 0 && MovementSpeed < 0.1f)
                {
                    MovementSpeed = 0;
                }
            }
            else
            {
                var orignDir = (placement.Rotation * -Vector3.UnitZ).NormalizeValue;
                var sign = Vector3.Dot(orignDir, velocity);
                if (MathHelper.Abs(sign) <= MathHelper.Epsilon)
                    sign = 1;
                else
                {
                    sign = sign / MathHelper.Abs(sign);
                }
                MovementSpeed = MathHelper.FloatLerp(MovementSpeed, sign * mVelocity.Length(), 0.2f);
                if (MathHelper.Abs(MovementSpeed) > 0 && MathHelper.Abs(MovementSpeed) < 0.1f)
                {
                    MovementSpeed = sign * 0.1f;
                }
                MovementDir = mVelocity.NormalizeValue;
            }
        }
        public override void ProcessingOrientation(GPlacementComponent placement, float dtSecond)
        {
            var camera = Host.GetComponentRecursion<Camera.CameraComponent>();
            LookAtDir = camera.CameraDirection;
            if (OrientThisRotation)
            {
                if (mProcessOrientation)
                {
                    if (mOrientation != Vector3.Zero)
                    {
                        var lookAtDir2D = LookAtDir;
                        lookAtDir2D.Y = 0;
                        mOrientation = Vector3.Slerp(mOrientation, lookAtDir2D, 0.3f);
                    }
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
    }
}
