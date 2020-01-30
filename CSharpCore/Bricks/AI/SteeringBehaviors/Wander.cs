using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.SteeringBehaviors
{
    public class Wander : AIMovementBase
    {
        public float Radius { get; set; } = 3f;
        public float Distance { get; set; } = 4f;
        Vector3 mTargetPos = Vector3.Zero;
        public override MovementTransform DoSteering()
        {
            return DoSteering(mTarget.Position);
        }
        internal override MovementTransform DoSteering(Vector3 target)
        {
            MovementTransform steering = new MovementTransform();
            if (mTarget == null)
            {
                return steering;
            }
            if(IgnoreY)
                mTargetPos = new Vector3(GetRandomFloat(), 0, GetRandomFloat());
            else
                mTargetPos = new Vector3(GetRandomFloat(), GetRandomFloat(), GetRandomFloat());
            mTargetPos *= Radius;
            mTargetPos += (-Vector3.UnitZ * Distance);
            var rot = Quaternion.GetQuaternion(-Vector3.UnitZ, mCharacter.Velocity);
            var dir = Vector3.TransformNormal(mTargetPos, Matrix.RotationQuaternion(rot));
            var force = dir - mCharacter.Velocity;
            if (IgnoreY)
                force.Y = 0;
            force.Normalize();
            steering.Force = force * mCharacter.MaxVelocity;
            return steering;
        }
        float GetRandomFloat()
        {
            return MathHelper.RandomDouble() * 2.0f - 1.0f;
        }
    }
}
