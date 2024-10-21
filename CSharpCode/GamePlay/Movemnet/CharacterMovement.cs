using EngineNS.Bricks.PhysicsCore.SceneNode;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Movemnet
{
    [Bricks.CodeBuilder.ContextMenu("CharacterMovement", "CharacterMovement", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtCharacterMovement.TtCharacterMovementData), DefaultNamePrefix = "CharacterMovement")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtCharacterMovement : TtMovement
    {
        public class TtCharacterMovementData : TtMovementData
        {

        }

        protected override void UpdatePlacement(TtWorld world, TtRenderPolicy policy)
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
            var phyControlNode = Parent.FindFirstChild<TtPhyControllerNodeBase>() as TtPhyControllerNodeBase;
            if (phyControlNode != null)
            {
                phyControlNode.TryMove(currentLinearVelocity.AsDVector() * world.DeltaTimeSecond, world.DeltaTimeSecond, out newPosition);
            }
            else
            {
                newPosition = Parent.Placement.Position + currentLinearVelocity * world.DeltaTimeSecond;
            }

            Parent.Placement.Position = newPosition;
            Parent.Placement.Quat = Parent.Placement.Quat * Quaternion.FromEuler(new FRotator(currentAngularVelocity) * world.DeltaTimeSecond);
           
        }
    }
}
