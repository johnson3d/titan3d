using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.SteeringBehaviors
{
    public class Arrive : AIMovementBase
    {
        Seek mSeek = new Seek();
        public override IBoid Character
        {
            get { return mCharacter; }
            set { mCharacter= value; mSeek.Character = value; }
        }
        public override IBoid Target
        {
            get { return mTarget; }
            set { mTarget = value; mSeek.Target = value; }
        }
        public float Radius { get; set; } = 1;
        public float TimeToTarget { get; set; } = 0.25f;
        public override MovementTransform DoSteering()
        {
            return DoSteering(mTarget.Position);
        }
        internal override MovementTransform DoSteering(Vector3 target)
        {
            var dis = (target - mCharacter.Position).Length();
            if (dis < Radius)
            {
                return default(MovementTransform);
            }
            else
                return mSeek.DoSteering(target);
        }
    }
}
