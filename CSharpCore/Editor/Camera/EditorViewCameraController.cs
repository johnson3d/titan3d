using System;
using System.Collections.Generic;
using System.Text;

namespace Editor.Camera
{
    public class EditorViewCameraController : EngineNS.GamePlay.Camera.CameraController
    {
        public EngineNS.Graphics.CGfxCamera Camera
        {
            get;
            set;
        }
        public void BeforeFrame()
        {

        }
        public void Rotate(EngineNS.GamePlay.Camera.eCameraAxis axis, float angle, bool rotLookAt = false)
        {
            if (Camera == null)
                return;

            var pos = Camera.CameraData.Position;
            var lookAt = Camera.CameraData.LookAt;

            switch(axis)
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
                        var up = EngineNS.Vector3.Dot(Camera.CameraData.Up, EngineNS.Vector3.UnitY) * EngineNS.Vector3.UnitY;
                        up.Normalize();
                        var mat = EngineNS.Matrix.RotationAxis(up, angle);
                        var dir = EngineNS.Vector3.TransformCoordinate((pos - lookAt), mat);
                        if(rotLookAt)
                        {
                            var newLookAt = pos - dir;
                            Camera.LookAtLH(pos, newLookAt, up);
                        }
                        else
                        {
                            var newPos = lookAt + dir;
                            Camera.LookAtLH(newPos, lookAt, up);
                        }
                    }
                    break;
                case EngineNS.GamePlay.Camera.eCameraAxis.Right:
                    {
                        var right = Camera.CameraData.Right;
                        var mat = EngineNS.Matrix.RotationAxis(right, angle);
                        var dir = EngineNS.Vector3.TransformCoordinate((pos - lookAt), mat);
                        var up = EngineNS.Vector3.Cross(right, dir);
                        up.Normalize();
                        if(rotLookAt)
                        {
                            var newLookAt = pos - dir;
                            Camera.LookAtLH(pos, newLookAt, up);
                        }
                        else
                        {
                            var newPos = lookAt + dir;
                            Camera.LookAtLH(newPos, lookAt, up);
                        }
                    }
                    break;
            }
        }
        public void Move(EngineNS.GamePlay.Camera.eCameraAxis axis, float step, bool moveWithLookAt = false)
        {
            if (Camera == null)
                return;

            var pos = Camera.CameraData.Position;
            var lookAt = Camera.CameraData.LookAt;
            switch(axis)
            {
                case EngineNS.GamePlay.Camera.eCameraAxis.Forward:
                    {
                        var dir = pos - lookAt;
                        if (moveWithLookAt)
                        {
                            dir.Normalize();
                            var temp = dir * step;
                            Camera.LookAtLH(pos - temp, lookAt - temp, EngineNS.Vector3.UnitY);
                        }
                        else
                        {
                            var len = dir.Length();
                            if (step < len)
                            {
                                dir.Normalize();
                                var newPos = pos - dir * step;
                                Camera.LookAtLH(newPos, lookAt, EngineNS.Vector3.UnitY);
                            }
                        }
                    }
                    break;
                case EngineNS.GamePlay.Camera.eCameraAxis.Up:
                    {
                        var up = Camera.CameraData.Up;
                        up.Normalize();
                        if (moveWithLookAt)
                        {
                            var temp = up * step;
                            Camera.LookAtLH(pos - temp, lookAt - temp, up);
                        }
                        else
                        {
                            var delta = up * step;
                            var newPos = pos + delta;
                            var newLookAt = lookAt + delta;
                            Camera.LookAtLH(newPos, newLookAt, up);
                        }
                    }
                    break;
                case EngineNS.GamePlay.Camera.eCameraAxis.Right:
                    {
                        var right = Camera.CameraData.Right;
                        right.Normalize();
                        var delta = right * step;
                        if(moveWithLookAt)
                        {
                            Camera.LookAtLH(pos - delta, lookAt - delta, Camera.CameraData.Up);
                        }
                        else
                        {
                            var newPos = pos - delta;
                            var newLookAt = lookAt - delta;
                            Camera.LookAtLH(newPos, newLookAt, Camera.CameraData.Up);
                        }
                    }
                    break;
            }
        }
    }
}
