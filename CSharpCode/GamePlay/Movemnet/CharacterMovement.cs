using EngineNS.Bricks.PhysicsCore.SceneNode;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Movemnet
{
    [Bricks.CodeBuilder.ContextMenu("CharacterMovement", "Gameplay\\CharacterMovement", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtCharacterMovement.TtCharacterMovementData), DefaultNamePrefix = "CharacterMovement")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtCharacterMovement : TtMovement
    {
        [Rtti.Meta("")]
        public class TtCharacterMovementData : TtMovementData
        {

        }

        protected override void UpdatePlacement(TtWorld world, TtRenderPolicy policy)
        {
            if (!world.IsGameWorld)
                return;

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

            // RootMotion位移与速度位移合并后交给胶囊, 保证碰撞阻挡同样生效
            FTransform rootMotionDelta;
            DVector3 rootMotionTranslation = DVector3.Zero;
            if (ConsumeRootMotion(out rootMotionDelta))
            {
                rootMotionTranslation = ApplyRootMotionRotationAndGetTranslation(in rootMotionDelta);
            }

            DVector3 newPosition = DVector3.Zero;
            var displacement = currentLinearVelocity.AsDVector() * world.DeltaTimeSecond + rootMotionTranslation;
            var phyControlNode = Parent.FindFirstChild<TtPhyControllerNodeBase>() as TtPhyControllerNodeBase;
            if (phyControlNode != null)
            {
                phyControlNode.TryMove(displacement, world.DeltaTimeSecond, out newPosition);
            }
            else
            {
                newPosition = Parent.Placement.Position + displacement;
            }

            Parent.Placement.Position = newPosition;
            Parent.Placement.Quat = Parent.Placement.Quat * Quaternion.FromEuler(new FRotator(currentAngularVelocity) * world.DeltaTimeSecond);
           
        }
    }
}
