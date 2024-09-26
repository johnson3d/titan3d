using EngineNS.GamePlay.Camera;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Movemnet
{
    [Bricks.CodeBuilder.ContextMenu("Movement", "Movement", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtMovement.TtMovementData), DefaultNamePrefix = "Movement")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtMovement : Scene.TtLightWeightNodeBase
    {
        public class TtMovementData : TtNodeData
        {

        }
        public Vector3 LinearVelocity { get; private set; }

        private Vector3 SettedLinearVelocity = Vector3.Zero;
        public void SetLinearVelocity(Vector3 linearVelocity)
        {
            SettedLinearVelocity = linearVelocity;
        }
        public Vector3 AngularVelocity { get; private set; }

        private Vector3 SettedAngularVelocity = Vector3.Zero;
        public void SetAngularVelocity(Vector3 angularVelocity)
        {
            SettedAngularVelocity = angularVelocity;
        }
        public bool EnableGravity { get; set; } = false;
        public Vector3 GravityAcceleration { get; set; } = Vector3.Down * 9.8f;
        protected Vector3 GravityVelocity = Vector3.Zero;
        public float MaxGravitySpeed = 10;

        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtMovement), nameof(TickLogic));
                return mScopeTick;
            }
        } 
        public override void TickLogic(TtNodeTickParameters args)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                DVector3 posBeforeMove = Parent.Placement.AbsTransform.Position;
                UpdatePlacement(args.World, args.Policy);
                DVector3 posAfterMove = Parent.Placement.AbsTransform.Position;
                LinearVelocity = (posAfterMove - posBeforeMove).ToSingleVector3() / args.World.DeltaTimeSecond;
                base.TickLogic(args);
            }   
        }

        protected Vector3 ConsumeSettedLinearVelocity()
        {
            var temp = SettedLinearVelocity;
            SettedLinearVelocity = Vector3.Zero;
            return temp;
        }
        protected Vector3 ConsumeSettedAngularVelocity()
        {
            var temp = SettedAngularVelocity;
            SettedAngularVelocity = Vector3.Zero;
            return temp;
        }
        protected virtual void UpdatePlacement(TtWorld world, TtRenderPolicy policy)
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
            Parent.Placement.Position += currentLinearVelocity * world.DeltaTimeSecond;
            Parent.Placement.Quat = Parent.Placement.Quat * Quaternion.FromEuler(new FRotator(currentAngularVelocity) * world.DeltaTimeSecond);
        }
    }
}
