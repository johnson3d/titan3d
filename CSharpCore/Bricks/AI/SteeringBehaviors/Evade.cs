using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.SteeringBehaviors
{
    public class Evade :AIMovementBase
    {
        Flee mFlee = new Flee();
        public override IBoid Character
        {
            get { return mCharacter; }
            set { mCharacter = value; mFlee.Character = value; }
        }
        public override IBoid Target
        {
            get { return mTarget; }
            set { mTarget = value; mFlee.Target = value; }
        }
        public float PredictTime = 3.0f;
        public override MovementTransform DoSteering()
        {
            PredictTime = (mTarget.Position - mCharacter.Position).Length() / mCharacter.MaxVelocity;
            return DoSteering(mTarget.Position + mTarget.Velocity * PredictTime);
        }
        internal override MovementTransform DoSteering(Vector3 target)
        {
            return mFlee.DoSteering(target);
        }
    }
}
