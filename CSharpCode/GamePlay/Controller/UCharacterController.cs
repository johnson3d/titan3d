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

namespace EngineNS.GamePlay.Controller
{
    public class UCharacterController : UNode, IController
    {
        public Scene.Actor.UActor ControlledCharacter { get; set; }
        public Camera.ICameraControlNode CameraControlNode { get; set; }
        public UMovement MovementNode { get; set; }

        public bool OrientCameraRoation = true;

        public override Thread.Async.TtTask<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
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
            VInput = -value.X;
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
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UCharacterController), nameof(TickLogic));
        public override void TickLogic(TtNodeTickParameters args)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                base.TickLogic(args);

                float PitchSpeed = 15, YawSpeed = 15;
                float yawDelta = Math.Min(YawDelta, 100) * 0.01f;
                float pitchDelta = Math.Min(PitchDelta, 100) * 0.01f;
                CameraControlNode.AddDelta(new FRotator(-pitchDelta * PitchSpeed * args.World.DeltaTimeSecond, yawDelta * YawSpeed * args.World.DeltaTimeSecond, 0));

                //MovementNode.AngularVelocity = new DVector3(0, YawDelta * 0.1f, 0);
                if (OrientCameraRoation)
                {
                    ControlledCharacter.Placement.Quat = Quaternion.GetQuaternion(Vector3.Forward, -new DVector3(CameraControlNode.Camera.Direction.X, 0, CameraControlNode.Camera.Direction.Z).ToSingleVector3());
                }
                PitchDelta = 0;
                YawDelta = 0;

                float speed = 3;
                Vector3 control = Vector3.Forward * VInput + Vector3.Left * HInput;
                MovementNode.SetLinearVelocity(ControlledCharacter.Placement.Quat * control * speed);
            }   
        }
    }
}
