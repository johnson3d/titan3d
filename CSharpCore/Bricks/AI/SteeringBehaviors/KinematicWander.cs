using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.SteeringBehaviors
{
    public class KinematicWander : AIMovementBase
    {
        public float Radius { get; set; } = 10;
        public float Distance { get; set; } = 15;
        public float Jitter { get; set; } = 10;
        Vector3 mTargetPos = Vector3.Zero;
        public override MovementTransform DoSteering()
        {
            MovementTransform steering = new MovementTransform();
            if (mTarget == null)
            {
                return steering;
            }
            mTargetPos = new Vector3(((MathHelper.RandomRange(0, 2) - 1) * Jitter), 0, ((MathHelper.RandomRange(0, 2) - 1) * Jitter));
            mTargetPos *= Radius;
            mTargetPos += (-Vector3.UnitZ * Distance);
            var rot = Quaternion.GetQuaternion(-Vector3.UnitZ, mCharacter.Orientation);
            var pos = Vector3.TransformCoordinate(mTargetPos, Matrix.RotationQuaternion(rot));
            var dir = pos - mCharacter.Position;
            dir.Normalize();
            dir.Y = 0;
            steering.Force = dir * mCharacter.MaxVelocity;
            return steering;
        }
    }
}
