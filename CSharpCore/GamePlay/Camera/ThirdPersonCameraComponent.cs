using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.Component;

namespace EngineNS.GamePlay.Camera
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GamePlay.Component.GComponent.GComponentInitializer), "第三人称摄像机组件", "Camera", "ThirdPersonCameraComponent")]
    public class ThirdPersonCameraComponent : GamePlay.Component.GComponent , GamePlay.Camera.CameraController
    {
        public Graphics.CGfxCamera Camera
        {
            get;
            set;
        }
        public Vector3 Offset;
        public float LookDist;
        public float HookeFactor = 9.0f;
        public float LimitMoveSpeed = 10.0f;
        private Vector3 EyeTarget;
        private float EyeMoveSpeed;
        private Vector3 GetLookAt()
        {
            return Host.Placement.Location + Offset;
        }
        public void SetCamera(Graphics.CGfxCamera camera, float dist, Vector3 offset)
        {
            Camera = camera;
            LookDist = dist;
            Offset = offset;

            if (camera!=null || Host==null)
            {
                EyeTarget = GetLookAt() - camera.CameraData.Direction * dist;
                Camera.LookAtLH(EyeTarget, GetLookAt(), Vector3.UnitY);
            }
        }
        public override void OnAdded()
        {
            if (Camera != null)
            {
                EyeTarget = GetLookAt() - Camera.CameraData.Direction * LookDist;
                Camera.LookAtLH(EyeTarget, GetLookAt(), Vector3.UnitY);
            }
            base.OnAdded();
        }
        private Vector3 KeepDirection;
        public override void OnUpdateDrawMatrix(ref Matrix drawMatrix)
        {   
            if (Camera == null)
                return;
            //KeepDirection = Camera.Direction;
            EyeTarget = GetLookAt() - KeepDirection * LookDist;

            var delta = EyeTarget - Camera.CameraData.Position;
            if (delta.Length() <= 0.1f)
            {
                EyeMoveSpeed = 0;
                PrevMoveDir = Vector3.Zero;
            }
            else
            {
                if (Vector3.Dot(PrevMoveDir, delta) < 0.0f)
                {
                    EyeMoveSpeed = 0;
                    PrevMoveDir = Vector3.Zero;
                    return;
                }

                PrevMoveDir = delta;
                PrevMoveDir.Normalize();
                EyeMoveSpeed = HookeLaw(HookeFactor, delta.Length());
            }
        }
        private Vector3 PrevMoveDir;
        public override void Tick(GPlacementComponent placement)
        {
            base.Tick(placement);

            if (Camera == null)
                return;

            if (EyeMoveSpeed == 0.0f)
            {
                return;
            }

            float t = CEngine.Instance.EngineElapseTimeSecond;
            var dir = EyeTarget - Camera.CameraData.Position;
            dir.Normalize();
            if(Vector3.Dot(PrevMoveDir, dir)<0.0f)
            {
                EyeMoveSpeed = 0;
                PrevMoveDir = Vector3.Zero;
                return;
            }
            float moveDist = EyeMoveSpeed * t;
            var eye = Camera.CameraData.Position+ dir* moveDist;
            Camera.LookAtLH(eye, GetLookAt(), Vector3.UnitY);

            var delta = EyeTarget - Camera.CameraData.Position;
            if (delta.Length() <= 0.1f)
            {
                EyeMoveSpeed = 0;
            }
            else
            {
                EyeMoveSpeed = HookeLaw(HookeFactor, delta.Length());
            }


            dir = Camera.CameraData.Direction;
            dir.Y = 0;
            dir.Normalize();
            placement.Rotation = Quaternion.GetQuaternion(-Vector3.UnitZ, dir);
        }
        private float HookeLaw(float k, float delta)
        {
            float result = k * delta;
            return Math.Max(result, LimitMoveSpeed);
        }

        public virtual void Rotate(GamePlay.Camera.eCameraAxis axis, float angle, bool rotLookAt = false)
        {
            if (Camera == null)
                return;

            var pos = Camera.CameraData.Position;
            var lookAt = Camera.CameraData.LookAt;

            switch (axis)
            {
                case EngineNS.GamePlay.Camera.eCameraAxis.Forward:
                    {
                        var forward = Camera.CameraData.Direction;
                        var up = Camera.CameraData.Up;
                        var mat = EngineNS.Matrix.RotationAxis(forward, angle);
                        up = EngineNS.Vector3.TransformCoordinate(up, mat);
                        Camera.LookAtLH(pos, lookAt, up);

                    }
                    break;
                case EngineNS.GamePlay.Camera.eCameraAxis.Up:
                    {
                        var up = EngineNS.Vector3.UnitY;
                        var mat = EngineNS.Matrix.RotationAxis(up, angle);
                        var dir = EngineNS.Vector3.TransformCoordinate((pos - lookAt), mat);
                        var newPos = lookAt + dir;
                        Camera.LookAtLH(newPos, lookAt, up);
                    }
                    break;
                case EngineNS.GamePlay.Camera.eCameraAxis.Right:
                    {
                        var right = Camera.CameraData.Right;
                        var mat = EngineNS.Matrix.RotationAxis(right, angle);
                        var dir = EngineNS.Vector3.TransformCoordinate((pos - lookAt), mat);
                        var newPos = lookAt + dir;
                        Camera.LookAtLH(newPos, lookAt, Camera.CameraData.Up);
                    }
                    break;
            }
            KeepDirection = Camera.CameraData.Direction;
        }
        public virtual void Move(GamePlay.Camera.eCameraAxis axis, float step, bool moveWithLookAt = false)
        {
            //if (Camera == null)
            //    return;

            //var pos = Camera.Position;
            //var lookAt = Camera.LookAt;
            //switch(axis)
            //{
            //    case EngineNS.GamePlay.Camera.eCameraAxis.Forward:
            //        {
            //            var dir = pos - lookAt;
            //            var len = dir.Length();
            //            if(step < len)
            //            {
            //                dir.Normalize();
            //                var newPos = pos - dir * step;
            //                Camera.LookAtLH(newPos, lookAt, EngineNS.Vector3.UnitY);
            //            }
            //        }
            //        break;
            //    case EngineNS.GamePlay.Camera.eCameraAxis.Up:
            //        {
            //            var up = Camera.Up;
            //            up.Normalize();
            //            var delta = up * step;
            //            var newPos = pos + delta;
            //            var newLookAt = lookAt + delta;
            //            Camera.LookAtLH(newPos, newLookAt, up);
            //        }
            //        break;
            //    case EngineNS.GamePlay.Camera.eCameraAxis.Right:
            //        {
            //            var right = Camera.Right;
            //            right.Normalize();
            //            var delta = right * step;
            //            var newPos = pos - delta;
            //            var newLookAt = lookAt - delta;
            //            Camera.LookAtLH(newPos, newLookAt, Camera.Up);
            //        }
            //        break;
            //}
        }
    }
}
