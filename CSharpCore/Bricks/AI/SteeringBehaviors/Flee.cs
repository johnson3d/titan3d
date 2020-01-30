using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.SteeringBehaviors
{
    public class Flee : AIMovementBase
    {
        public override MovementTransform DoSteering()
        {
            return DoSteering(mTarget.Position);
        }
        internal override MovementTransform DoSteering(Vector3 target)
        {
            MovementTransform steering = new MovementTransform();
            var desireVel = -(target - mCharacter.Position).NormalizeValue * mCharacter.MaxVelocity;
            var force = desireVel - mCharacter.Velocity;
            var cos = Vector3.Dot(desireVel, mCharacter.Velocity);
            if (cos > 0.9f)
            {
                force = desireVel;
            }
            //steering.Force = force.NormalizeValue * Math.Min(force.Length(),60);
            if (IgnoreY)
                force.Y = 0;
            steering.Force = force;
            return steering;
        }
    }
}
