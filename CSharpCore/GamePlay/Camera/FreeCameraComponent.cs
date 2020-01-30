using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.Component;

namespace EngineNS.GamePlay.Camera
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GamePlay.Component.GComponent.GComponentInitializer), "自由摄像机组件", "Camera", "FreeCameraComponent")]
    public class FreeCameraComponent : CameraComponent
    {
        public override void UpdateCamera(GPlacementComponent placement)
        {
            var lookAt = Camera.CameraData.LookAt;
            var curForward = Camera.CameraData.Direction;
            var mat = EngineNS.Matrix.RotationAxis(DesireDirection, mYawPitchRoll.Length());
            var forward = EngineNS.Vector3.TransformNormal(curForward, mat);
            var rotation = Quaternion.GetQuaternion(-Vector3.UnitZ, forward);
            mPlacementComponent.Rotation = rotation;
            var up = Vector3.TransformNormal(Vector3.UnitY, Matrix.RotationQuaternion(rotation));
            lookAt = DesirePosition + DesireDirection;
            Camera.LookAtLH(DesirePosition, lookAt, Vector3.UnitY);
            //if (Vector3.Dot(lookAt - Location, Vector3.UnitY) > 0)
            //{
            //}
            //else
            //{
            //    Camera.LookAtLH(Location, lookAt, -Vector3.UnitY);
            //}
        }
        public override void PreCalculateCameraData(GPlacementComponent placement)
        {
            if (mYawPitchRoll == Vector3.Zero)
                return;
            var curTarget2Camera = -Camera.CameraData.Direction;
            var yMat = EngineNS.Matrix.RotationAxis(Camera.CameraData.Up, mYawPitchRoll.Y);
            var xMat = EngineNS.Matrix.RotationAxis(Camera.CameraData.Right, mYawPitchRoll.X);
            var mat = yMat * xMat;
            var target2Camera = EngineNS.Vector3.TransformNormal(curTarget2Camera, mat);
            target2Camera.Normalize();
            DesireDirection = -target2Camera;
            //DesirePosition = Location;
            base.PreCalculateCameraData(placement);
        }
    }
}
