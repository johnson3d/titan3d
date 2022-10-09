using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Movemnet
{
    public class UMovement : Scene.ULightWeightNodeBase
    {
        public Vector3 LinearVelocity { protected get; set; }
        public Vector3 AngularVelocity { protected get; set; }
        public bool EnableGravity { get; set; } = false;
        public Vector3 GravityAcceleration { get; set; } = Vector3.Down * 9.8f;
        protected Vector3 GravityVelocity = Vector3.Zero;
        public float MaxGravitySpeed = 10;

        public Vector3 CurrentLinearVelocity { get; protected set; }
        public Vector3 CurrentAngularVelocity { get; protected set; }

        public override void TickLogic(UWorld world, URenderPolicy policy)
        {
            UpdatePlacement(world,policy);
            base.TickLogic(world, policy);
        }
        protected void UpdatePlacement(UWorld world, URenderPolicy policy)
        {
            if (EnableGravity)
            {
                GravityVelocity += GravityAcceleration * world.DeltaTimeSecond;
                if (GravityVelocity.Length() > MaxGravitySpeed)
                {
                    GravityVelocity = GravityAcceleration.NormalizeValue * MaxGravitySpeed;
                }
                CurrentLinearVelocity = LinearVelocity + GravityVelocity;
            }
            else
            {
                CurrentLinearVelocity = LinearVelocity;
            }
            CurrentAngularVelocity = AngularVelocity;
            Parent.Placement.Position += CurrentLinearVelocity * world.DeltaTimeSecond;
            Parent.Placement.Quat = Parent.Placement.Quat * Quaternion.FromEuler(CurrentLinearVelocity * world.DeltaTimeSecond);
            LinearVelocity = Vector3.Zero;
            AngularVelocity = Vector3.Zero;
        }
    }
}
