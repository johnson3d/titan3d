using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.SteeringBehaviors
{

    public class KinematicSeek : AIMovementBase
    {
        public override MovementTransform DoSteering()
        {
            MovementTransform steering = new MovementTransform();
            if (mTarget == null)
            {
                return steering;
            }
            steering.Force = (mTarget.Position - mCharacter.Position);
            steering.Force.Normalize();
            steering.Force *= mCharacter.MaxVelocity;
            return steering;
        }
        internal override MovementTransform DoSteering(Vector3 target)
        {
            MovementTransform steering = new MovementTransform();
            steering.Force = (target - mCharacter.Position);
            steering.Force.Normalize();
            steering.Force *= mCharacter.MaxVelocity;
            return steering;
        }
    }
}
