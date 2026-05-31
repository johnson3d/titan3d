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
                        Camera.LookAtLH(pos, lookAt, up);
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
                            Camera.LookAtLH(pos, newLookAt, up);
                        }
                        else
                        {
                            var newPos = lookAt + dir;
                            Camera.LookAtLH(newPos, lookAt, up);
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
                            Camera.LookAtLH(in eye, in at, in EngineNS.Vector3.UnitY);
                        }
                        else
                        {
                            // 原行为: 仅在 step < |pos-lookAt| 时把相机往 lookAt 方向推一段,
                            // step 超过剩余距离时整段操作静默丢弃 -> 表现为"滚到逼近 lookAt 就再也滚不动"。
                            // 修复: step 仍小于剩余距离时按原方式只推相机; 一旦达到/超过剩余距离,
                            // 自动 fallback 成飞行模式 (同步把 lookAt 也往前推), 让滚轮可以
                            // 越过原 lookAt 点继续前进, 同时保持 |pos-lookAt| 不退化为 0。
                            var len = dir.Length();
                            dir.Normalize();
                            if (step < len)
                            {
                                var newPos = pos - dir * step;
                                Camera.LookAtLH(in newPos, in lookAt, in EngineNS.Vector3.UnitY);
                            }
                            else
                            {
                                var temp = dir * step;
                                var eye = pos - temp;
                                var at = lookAt - temp;
                                Camera.LookAtLH(in eye, in at, in EngineNS.Vector3.UnitY);
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
                            Camera.LookAtLH(in eye, in at, in up);
                        }
                        else
                        {
                            var delta = up * step;
                            var newPos = pos + delta;
                            var newLookAt = lookAt + delta;
                            Camera.LookAtLH(in newPos, in newLookAt, in up);
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
                            Camera.LookAtLH(in eye, in at, in up);
                        }
                        else
                        {
                            var newPos = pos - delta;
                            var newLookAt = lookAt - delta;
                            var up = Camera.mCoreObject.GetUp();
                            Camera.LookAtLH(in newPos, in newLookAt, in up);
                        }
                    }
                    break;
            }
        }
    }
}
