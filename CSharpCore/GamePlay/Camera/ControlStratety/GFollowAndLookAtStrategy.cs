using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.Component;

namespace EngineNS.GamePlay.Camera.ControlStratety
{
    public class GFollowAndLookAtStrategy : GCameraControlStrategy
    {
        public IPlaceable LookAtObject { get; set; } = null;
        public IPlaceable HostCameraComponent { get; set; } = null;
        public Vector3 LookAtOffset { get; set; } = Vector3.Zero;
        public Vector3 Position { get; set; } = Vector3.Zero;
        public override void OnPerform(GCamera camera)
        {
            var currentDir = camera.Direction;
            var position = LookAtObject.Placement.Location;
            var rotation = HostCameraComponent.Placement.Rotation;
            var euler = rotation.ToEuler();
            euler.X = 0;
            euler.Z = 0;
            var onlyYRotation = Quaternion.FromEuler(euler);

            var nextLookAt = Vector3.TransformCoordinate(LookAtOffset, Matrix.RotationQuaternion(onlyYRotation)) + position;
            var nextDir = rotation * (-Vector3.UnitZ);
            camera.Position = nextLookAt + nextDir * -HostCameraComponent.Placement.Location.Length();
            camera.Rotation = rotation;
        }
    }
}
