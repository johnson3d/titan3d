using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Controller
{
    public class EditorCameraController : Graphics.Pipeline.ICameraController
    {
        Graphics.Pipeline.TtCamera mCamera;
        public Graphics.Pipeline.TtCamera Camera
        {
            get => mCamera;
        }
        public void ControlCamera(Graphics.Pipeline.TtCamera camera)
        {
            mCamera = camera;
        }
        public void Rotate(Graphics.Pipeline.ECameraAxis axis, float angle, bool rotLookAt = false)
        {
            if (Camera == null)
                return;

            var pos = Camera.mCoreObject.GetPosition();
            var lookAt = Camera.mCoreObject.GetLookAt();

            switch (axis)
            {
                case Graphics.Pipeline.ECameraAxis.Forward:
                    {
                        var forward = Camera.mCoreObject.GetDirection();
                        var up = Camera.mCoreObject.GetUp();
                        var mat = EngineNS.Matrix.RotationAxis(forward, angle);
                        up = EngineNS.Vector3.TransformCoordinate(up, mat);
                        unsafe
                        {
                            Camera.mCoreObject.LookAtLH(&pos, &lookAt, &up);
                        }

                    }
                    break;
                case Graphics.Pipeline.ECameraAxis.Up:
                    {
                        var up = EngineNS.Vector3.Dot(Camera.mCoreObject.GetUp(), Vector3.UnitY) * Vector3.UnitY;
                        up.Normalize();
                        var mat = EngineNS.Matrix.RotationAxis(up, angle);
                        var dir = EngineNS.DVector3.TransformCoordinate((pos - lookAt), mat.AsDMatrix());
                        if (rotLookAt)
                        {
                            var newLookAt = pos - dir;
                            unsafe
                            {
                                Camera.mCoreObject.LookAtLH(&pos, &newLookAt, &up);
                            }
                        }
                        else
                        {
                            var newPos = lookAt + dir;
                            unsafe
                            {
                                Camera.mCoreObject.LookAtLH(&newPos, &lookAt, &up);
                            }
                        }
                    }
                    break;
                case Graphics.Pipeline.ECameraAxis.Right:
                    {
                        var right = Camera.mCoreObject.GetRight();
                        var mat = EngineNS.Matrix.RotationAxis(right, angle);
                        var dir = EngineNS.Vector3.TransformCoordinate((pos - lookAt).ToSingleVector3(), mat);
                        var up = EngineNS.Vector3.Cross(right, dir);
                        up.Normalize();
                        if (rotLookAt)
                        {
                            var newLookAt = pos - dir;
                            unsafe
                            {
                                Camera.mCoreObject.LookAtLH(&pos, &newLookAt, &up);
                            }
                        }
                        else
                        {
                            var newPos = lookAt + dir;
                            unsafe
                            {
                                Camera.mCoreObject.LookAtLH(&newPos, &lookAt, &up);
                            }
                        }
                    }
                    break;
            }
        }
        public unsafe void Move(Graphics.Pipeline.ECameraAxis axis, float step, bool moveWithLookAt = false)
        {
            if (Camera == null)
                return;

            var pos = Camera.mCoreObject.GetPosition();
            var lookAt = Camera.mCoreObject.GetLookAt();
            switch (axis)
            {
                case Graphics.Pipeline.ECameraAxis.Forward:
                    {
                        var dir = pos - lookAt;
                        if (moveWithLookAt)
                        {
                            dir.Normalize();
                            var temp = dir * step;
                            var eye = pos - temp;
                            var at = lookAt - temp;
                            Camera.mCoreObject.LookAtLH(in eye, in at, in EngineNS.Vector3.UnitY);
                        }
                        else
                        {
                            var len = dir.Length();
                            if (step < len)
                            {
                                dir.Normalize();
                                var newPos = pos - dir * step;
                                Camera.mCoreObject.LookAtLH(in newPos, in lookAt, in EngineNS.Vector3.UnitY);
                            }
                        }
                    }
                    break;
                case Graphics.Pipeline.ECameraAxis.Up:
                    {
                        var up = Camera.mCoreObject.GetUp();
                        up.Normalize();
                        if (moveWithLookAt)
                        {
                            var temp = up * step;
                            var eye = pos - temp;
                            var at = lookAt - temp;
                            Camera.mCoreObject.LookAtLH(in eye, in at, in up);
                        }
                        else
                        {
                            var delta = up * step;
                            var newPos = pos + delta;
                            var newLookAt = lookAt + delta;
                            Camera.mCoreObject.LookAtLH(in newPos, in newLookAt, in up);
                        }
                    }
                    break;
                case Graphics.Pipeline.ECameraAxis.Right:
                    {
                        var right = Camera.mCoreObject.GetRight();
                        right.Normalize();
                        var delta = right * step;
                        if (moveWithLookAt)
                        {
                            var eye = pos - delta;
                            var at = lookAt - delta;
                            var up = Camera.mCoreObject.GetUp();
                            Camera.mCoreObject.LookAtLH(in eye, in at, in up);
                        }
                        else
                        {
                            var newPos = pos - delta;
                            var newLookAt = lookAt - delta;
                            var up = Camera.mCoreObject.GetUp();
                            Camera.mCoreObject.LookAtLH(in newPos, in newLookAt, in up);
                        }
                    }
                    break;
            }
        }
    }
}
