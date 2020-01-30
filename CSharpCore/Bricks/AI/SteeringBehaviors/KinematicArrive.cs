using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.SteeringBehaviors
{
    public class KinematicArrive : AIMovementBase
    {
        public float Radius { get; set; } = 0.1f;
        public float TimeToTarget { get; set; } = 0.25f;
        public override MovementTransform DoSteering()
        {
            MovementTransform steering = new MovementTransform();
            if (mTarget == null)
            {
                return steering;
            }
            steering.Force = (mTarget.Position - mCharacter.Position);
            if (steering.Force.Length() < Radius)
                return default(MovementTransform);
            steering.Force /= TimeToTarget;
            if (steering.Force.Length()>Character.MaxVelocity)
            {
                steering.Force.Normalize();
                steering.Force *= Character.MaxVelocity;

            }
            return steering;
        }
        internal override MovementTransform DoSteering(Vector3 target)
        {
            MovementTransform steering = new MovementTransform();
            steering.Force = (target - mCharacter.Position);
            if (steering.Force.Length() < Radius)
                return default(MovementTransform);
            steering.Force /= TimeToTarget;
            if (steering.Force.Length() > Character.MaxVelocity)
            {
                steering.Force.Normalize();
                steering.Force *= Character.MaxVelocity;

            }
            return steering;
        }
    }
}
