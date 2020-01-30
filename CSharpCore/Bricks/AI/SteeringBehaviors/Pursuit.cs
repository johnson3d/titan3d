using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.SteeringBehaviors
{
    public class Pursuit : AIMovementBase
    {
        Arrive mSeek = new Arrive();
        public override IBoid Character
        {
            get { return mCharacter; }
            set { mCharacter = value; mSeek.Character = value; }
        }
        public override IBoid Target
        {
            get { return mTarget; }
            set { mTarget = value; mSeek.Target = value; }
        }
        public float PredictTime = 3.0f;
        public override MovementTransform DoSteering()
        {
            PredictTime = (mTarget.Position - mCharacter.Position).Length() / mCharacter.MaxVelocity;
            return DoSteering(mTarget.Position + mTarget.Velocity * PredictTime);
        }
        internal override MovementTransform DoSteering(Vector3 target)
        {
            return mSeek.DoSteering(target);
        }
    }
}
