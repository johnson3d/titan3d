using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Movemnet
{
    public class UMovement : Scene.ULightWeightNodeBase
    {
        public DVector3 LinearVelocity { get; set; }
        public DVector3 AngularVelocity { get; set; }

        public override void TickLogic(UWorld world, URenderPolicy policy)
        {
            UpdatePlacement(world,policy);
            base.TickLogic(world, policy);
        }
        protected void UpdatePlacement(UWorld world, URenderPolicy policy)
        {
            Parent.Placement.Position += LinearVelocity * world.DeltaTimeSecond;
            Parent.Placement.Quat = Parent.Placement.Quat * Quaternion.FromEuler(AngularVelocity.ToSingleVector3()* world.DeltaTimeSecond);
            LinearVelocity = DVector3.Zero;
            AngularVelocity = DVector3.Zero;
        }
    }
}
