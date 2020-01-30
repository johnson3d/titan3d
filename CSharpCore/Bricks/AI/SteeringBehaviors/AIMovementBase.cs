using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.SteeringBehaviors
{
    public interface IBoid //"bird-oid object"
    {
        float Friction { get; set; }
        Vector3 Velocity { get; set; }
        float MaxVelocity { get; set; }
        Vector3 Position { get; set; }
        float Mass { get; set; }
        Vector3 Orientation { get; set; }
        EnvironmentContext EnvironmentContext { get; set; }
    }
    public struct EnvironmentContext
    {
        public GamePlay.GWorld World;
    }
    public struct MovementTransform
    {
        public Vector3 Force;
        public Vector3 Orientation;
        public float Angular;

        public static MovementTransform operator *(MovementTransform value, float scale)
        {
            MovementTransform result;
            result.Force = value.Force * scale;
            result.Orientation = value.Orientation * scale;
            result.Angular = value.Angular * scale;
            return result;
        }
        public static MovementTransform operator +(MovementTransform left, MovementTransform right)
        {
            MovementTransform result;
            result.Force = left.Force + right.Force;
            result.Orientation = left.Orientation + right.Orientation;
            result.Angular = left.Angular + right.Angular;
            return result;
        }
        //public static MovementTransform operator -(MovementTransform left, MovementTransform right)
        //{
        //    MovementTransform result;
        //    result.Force = left.Force - right.Force;
        //    result.Orientation = left.Orientation - right.Orientation;
        //    result.Angular = left.Angular - right.Angular;
        //    return result;
        //}
    }
    public abstract class AIMovementBase
    {
        //public Vector3 position;
        //public float orientation;
        //public Vector3 velocity;
        //public float rotation;

        public bool IgnoreY = true;
        protected IBoid mCharacter;
        public virtual IBoid Character
        {
            get => mCharacter;
            set => mCharacter = value;
        }
        protected IBoid mTarget;
        public virtual IBoid Target
        {
            get => mTarget;
            set => mTarget = value;
        }
        protected float mMaxSpeed;
        public abstract MovementTransform DoSteering();
        internal virtual MovementTransform DoSteering(Vector3 target) { return default(MovementTransform); }
    }
}
