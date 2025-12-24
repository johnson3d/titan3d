using EngineNS.Bricks.PhysicsCore.SceneNode;
using EngineNS.GamePlay.Camera;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Movemnet
{
    [Bricks.CodeBuilder.ContextMenu("SimpleMovement", "Gameplay\\SimpleMovement", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtSimpleMovement.TtSimpleMovementData), DefaultNamePrefix = "SimpleMovement")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtSimpleMovement : Scene.TtLightWeightNodeBase
    {
        [Rtti.Meta("")]
        public class TtSimpleMovementData : TtNodeData
        {
            [Rtti.Meta("")]
            public bool EnableGravity { get; set; } = false;
            [Rtti.Meta("")]
            public Vector3 GravityAcceleration { get; set; } = Vector3.Down * 9.8f;
        }
        public TtSimpleMovementData MovementData { get=> NodeData as TtSimpleMovementData; }
        public float Speed { get; set; } = 3;
        public Vector3 LinearVelocity { get; private set; }
        [Category("Option")]
        public Vector3 DesiredPosition { get; set; } = Vector3.Zero;
        public void SetDesiredPosition(Vector3 position)
        {
            DesiredPosition = position;
        }
        
        [Category("Option")]
        public bool EnableGravity { get=>MovementData.EnableGravity; set=> MovementData.EnableGravity = value; }
        [Category("Option")]
        public Vector3 GravityAcceleration { get => MovementData.GravityAcceleration; set => MovementData.GravityAcceleration = value; }
        protected Vector3 GravityVelocity = Vector3.Zero;
        public float MaxGravitySpeed = 10;

        public override Profiler.TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtSimpleMovement>.Scope;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            DVector3 posBeforeMove = Parent.Placement.AbsTransform.Position;
            UpdatePlacement(args.World, args.Policy);
            DVector3 posAfterMove = Parent.Placement.AbsTransform.Position;
            LinearVelocity = (posAfterMove - posBeforeMove).ToSingleVector3() / args.World.DeltaTimeSecond;
            return true;
        }

        protected virtual void UpdatePlacement(TtWorld world, TtRenderPolicy policy)
        {
            if (!world.IsGameWorld)
                return;

            var distance = DesiredPosition - Parent.Placement.Position.ToSingleVector3();
            var gravityVelocity = Vector3.Zero;
            
            if (EnableGravity)
            {
                GravityVelocity += GravityAcceleration * world.DeltaTimeSecond;
                if (GravityVelocity.Length() > MaxGravitySpeed)
                {
                    GravityVelocity = GravityAcceleration.NormalizeValue * MaxGravitySpeed;
                }
                gravityVelocity = GravityVelocity;
            }

            DVector3 newPosition = DVector3.Zero;
            var phyControlNode = Parent.FindFirstChild<TtPhyControllerNodeBase>() as TtPhyControllerNodeBase;
            if (phyControlNode != null)
            {
                phyControlNode.TryMove((distance + gravityVelocity * world.DeltaTimeSecond).AsDVector(), world.DeltaTimeSecond, out newPosition);
            }
            else
            {
                newPosition = Parent.Placement.Position + (distance + gravityVelocity * world.DeltaTimeSecond).AsDVector();
            }

            Parent.Placement.Position = newPosition;
            //Parent.Placement.Quat = Parent.Placement.Quat * Quaternion.FromEuler(new FRotator(currentAngularVelocity) * world.DeltaTimeSecond);
        }
    }
}
