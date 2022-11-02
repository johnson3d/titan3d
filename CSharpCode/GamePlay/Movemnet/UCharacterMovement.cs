using EngineNS.Bricks.PhysicsCore.SceneNode;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Movemnet
{
    public class UCharacterMovement : UMovement
    {

        protected override void UpdatePlacement(UWorld world, URenderPolicy policy)
        {
            var settedLinearVelocity = ConsumeSettedLinearVelocity();
            var settedAngularVelocity = ConsumeSettedAngularVelocity();
            var currentLinearVelocity = Vector3.Zero;
            var currentAngularVelocity = Vector3.Zero;
            if (EnableGravity)
            {
                GravityVelocity += GravityAcceleration * world.DeltaTimeSecond;
                if (GravityVelocity.Length() > MaxGravitySpeed)
                {
                    GravityVelocity = GravityAcceleration.NormalizeValue * MaxGravitySpeed;
                }
                currentLinearVelocity = settedLinearVelocity + GravityVelocity;
            }
            else
            {
                currentLinearVelocity = settedLinearVelocity;
            }
            currentAngularVelocity = settedAngularVelocity;

            DVector3 newPosition = DVector3.Zero;
            var phyControlNode = Parent.FindFirstChild<UPhyControllerNodeBase>() as UPhyControllerNodeBase;
            if (phyControlNode != null)
            {
                phyControlNode.TryMove(currentLinearVelocity.AsDVector() * world.DeltaTimeSecond, world.DeltaTimeSecond, out newPosition);
            }
            else
            {
                newPosition = Parent.Placement.Position + currentLinearVelocity * world.DeltaTimeSecond;
            }

            Parent.Placement.Position = newPosition;
            Parent.Placement.Quat = Parent.Placement.Quat * Quaternion.FromEuler(currentAngularVelocity * world.DeltaTimeSecond);
           
        }
    }
}
