using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.SteeringBehaviors
{
    public class KinematicPathFollowing : AIMovementBase
    {
        public Vector3[] Path { get; set; }
        public int PathPoint;
        public bool IsArrive { get; set; } = false;
        public int CurrentIndex { get; set; } = 0;
        KinematicArrive mArrive = new KinematicArrive();
        KinematicSeek mSeek = new KinematicSeek();
        public float Radius { get; set; } = 1;
        public float ArriveDistance
        {
            get => mArrive.Radius;
            set => mArrive.Radius = value;
        }
        public float TimeToTarget
        {
            get => mArrive.TimeToTarget;
            set => mArrive.TimeToTarget = value;
        }
        public override MovementTransform DoSteering()
        {
            MovementTransform steering = new MovementTransform();
            if (Path == null || PathPoint == 0)
            {
                return steering;
            }
            if(CurrentIndex == PathPoint - 1)
            {
                if ((mCharacter.Position - Path[CurrentIndex]).Length() <= ArriveDistance)
                {
                        IsArrive = true;
                        return steering;
                }
            }
            else if ((mCharacter.Position - Path[CurrentIndex]).Length() < Radius)
            {
                CurrentIndex++;

                if (CurrentIndex >= PathPoint)
                {
                    CurrentIndex = PathPoint - 1;
                    IsArrive = true;
                    return steering;
                }
            }
            IsArrive = false;
            if (CurrentIndex == PathPoint - 1)
            {
                mArrive.Character = Character;
                return mArrive.DoSteering(Path[CurrentIndex]);
            }
            else
            {
                mSeek.Character = Character;
                return mSeek.DoSteering(Path[CurrentIndex]);
            }
        }
        internal override MovementTransform DoSteering(Vector3 target)
        {
            return default(MovementTransform);
        }
    }
}
