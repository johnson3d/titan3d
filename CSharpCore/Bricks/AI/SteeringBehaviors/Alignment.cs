using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.SteeringBehaviors
{
    public class Alignment : AIMovementBase
    {
        public float NeighborDistance = 2;
        public override MovementTransform DoSteering()
        {
            Vector3 temp = Vector3.Zero;
            int neighborCount = 0;
            var it = mCharacter.EnvironmentContext.World.Actors.GetEnumerator();
            while (it.MoveNext())
            {
                var actor = it.Current.Value;
                var boid = actor.GetComponent<GamePlay.Component.GMovementComponent>() as IBoid;
                if (boid == null || boid == mCharacter || boid == mTarget)
                    continue;
                if ((boid.Position - mCharacter.Position).Length() < NeighborDistance)
                {

                    temp += boid.Velocity;
                    neighborCount++;
                }
            }
            it.Dispose();
            MovementTransform movementTransform = new MovementTransform();
            if (neighborCount > 0 && temp != Vector3.Zero)
            {
                //temp /= neighborCount;
                if (IgnoreY)
                    temp.Y = 0;
                temp.Normalize();
                movementTransform.Orientation = temp;
            }
            else
            {
                movementTransform.Orientation = mCharacter.Velocity.NormalizeValue;
            }
            return movementTransform;
        }
    }
}
