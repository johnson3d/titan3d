using EngineNS.Bricks.Input.Control;
using EngineNS.Bricks.Input.Device.Keyboard;
using EngineNS.Bricks.Input.Device.Mouse;
using EngineNS.Bricks.Input.InputMapping.Action;
using EngineNS.Bricks.Input.InputMapping.Binding;
using EngineNS.Bricks.PhysicsCore;
using EngineNS.GamePlay.Character;
using EngineNS.GamePlay.Movemnet;
using EngineNS.GamePlay.Scene;
using EngineNS.GamePlay.Scene.Actor;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using static EngineNS.GamePlay.Movemnet.TtMovement;

namespace EngineNS.GamePlay.Controller
{
    //[Bricks.CodeBuilder.ContextMenu("Movement", "Movement", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtCharacterController.TtCharacterControllerNodeData), DefaultNamePrefix = "CharacterController")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtCharacterController : TtNode, IController
    {
        public class TtCharacterControllerNodeData : TtNodeData
        {
            [Rtti.Meta]
            public bool OrientCameraRoation = true;
            [Rtti.Meta]
            public bool OrientToMovmement = false;
        }
        public Scene.Actor.TtActor ControlledCharacter { get; set; }
        public Camera.ICameraControlNode CameraControlNode { get; set; }
        public TtMovement MovementNode { get; set; }
        public TtCharacterControllerNodeData CharacterControllerNodeData { get => NodeData as TtCharacterControllerNodeData; }

        public bool OrientCameraRoation { get => CharacterControllerNodeData.OrientCameraRoation; set => CharacterControllerNodeData.OrientCameraRoation = value; }
        public bool OrientToMovmement { get => CharacterControllerNodeData.OrientToMovmement; set => CharacterControllerNodeData.OrientToMovmement = value; }

        public override Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            UAxis2DAction axis2D = IAction.Create<UAxis2DAction>(new UAxis2DAction.UAxis2DActionData());
            var upControl = IControl.Create<UKey>(new UKey.UKeyData() { Keycode = Bricks.Input.Keycode.KEY_w });
            var downControl = IControl.Create<UKey>(new UKey.UKeyData() { Keycode = Bricks.Input.Keycode.KEY_s });
            var leftControl = IControl.Create<UKey>(new UKey.UKeyData() { Keycode = Bricks.Input.Keycode.KEY_a });
            var rightControl = IControl.Create<UKey>(new UKey.UKeyData() { Keycode = Bricks.Input.Keycode.KEY_d });
            axis2D.Binding = IBinding.Create<UAxis2DBinding>(new UAxis2DBinding.UAxis2DBindingData()
            { UpControl = upControl, DownControl = downControl, LeftControl = leftControl, RightControl = rightControl });
            axis2D.OnValueUpdate += Axis2D_OnValueUpdate; ;

            UValueAction2D mouseMotion = IAction.Create<UValueAction2D>(new UValueAction2D.UValueAction2DData());
            mouseMotion.Binding = IBinding.Create<UValue2DBinding>(new UValue2DBinding.UValue2DBindingData() { ValueControl = IControl.Create<UMouseMotion>(new UMouseMotion.UMouseMotionData())});
            mouseMotion.OnValueUpdate += OnMouseMotion_OnValueUpdate;
            return base.InitializeNode(world, data, bvType, placementType);
        }

        private void Axis2D_OnValueUpdate(UAxis2DAction sender, Vector2 value)
        {
            VInput = value.X;
            HInput = value.Y;
        }

        Vector2 mPreMousePt = Vector2.One * float.MaxValue;
        float PitchDelta = 0;
        float YawDelta = 0;
        private void OnMouseMotion_OnValueUpdate(UValueAction2D sender, Vector2 value)
        {
            if (mPreMousePt != value)
            {
                PitchDelta = value.Y;
                YawDelta = value.X;
                mPreMousePt = value;
            }
        }

        float HInput = 0;
        float VInput = 0;
        public override Profiler.TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtCharacterController>.Scope;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            base.OnTickLogic(args);

            float PitchSpeed = 15, YawSpeed = 15;
            float yawDelta = Math.Min(YawDelta, 100) * 0.01f;
            float pitchDelta = Math.Min(PitchDelta, 100) * 0.01f;
            CameraControlNode.AddDelta(new FRotator(yawDelta * YawSpeed * args.World.DeltaTimeSecond, -pitchDelta * PitchSpeed * args.World.DeltaTimeSecond, 0));

            //MovementNode.AngularVelocity = new DVector3(0, YawDelta * 0.1f, 0);

            PitchDelta = 0;
            YawDelta = 0;

            Vector3 control = Vector3.Forward * VInput + Vector3.Right * HInput;
            if (OrientCameraRoation)
            {
                ControlledCharacter.Placement.Quat = Quaternion.GetQuaternion(Vector3.Forward, new DVector3(CameraControlNode.Camera.Direction.X, 0, CameraControlNode.Camera.Direction.Z).ToSingleVector3());
                MovementNode.SetLinearVelocity(ControlledCharacter.Placement.Quat * control * MovementNode.Speed);
            }
            if (OrientToMovmement)
            {
                MovementNode.SetLinearVelocity(control * MovementNode.Speed);
                var linearVelDir = MovementNode.LinearVelocity.NormalizeValue;
                linearVelDir.y = 0;
                if (linearVelDir.LengthSquared() == 0)
                {
                    linearVelDir = Quaternion.RotateVector3(ControlledCharacter.Placement.Quat, Vector3.Forward);
                }
                ControlledCharacter.Placement.Quat = Quaternion.GetQuaternionUp(Vector3.Forward, linearVelDir);
            }
            return true;
        }
    }
}
