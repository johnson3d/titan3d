using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.Component;

namespace EngineNS.GamePlay.Camera
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GamePlay.Component.GComponent.GComponentInitializer), "摄像机组件", "Camera Compoent")]
    public class ThirdPartCameraComponent : GamePlay.Component.GComponent
    {
        public Graphics.CGfxCamera Camera
        {
            get;
            protected set;
        }
        public Vector3 Offset;
        public float LookDist;
        public float HookeFactor = 3.0f;
        private Vector3 EyeTarget;
        private float EyeMoveSpeed;
        private Vector3 GetLookAt()
        {
            return Host.Placement.Location + Offset;
        }
        //private float EyeMoveAccel;
        public void SetCamera(Graphics.CGfxCamera camera, float dist, Vector3 offset)
        {
            Camera = camera;
            LookDist = dist;
            Offset = offset;

            if (camera!=null || Host==null)
            {
                EyeTarget = GetLookAt() - camera.Direction * dist;
                Camera.LookAtLH(EyeTarget, GetLookAt(), Vector3.UnitY);
            }
        }
        public override void OnAdded()
        {
            base.OnAdded();

            if (Camera != null)
            {
                EyeTarget = GetLookAt() - Camera.Direction * LookDist;
                Camera.LookAtLH(EyeTarget, GetLookAt(), Vector3.UnitY);
            }
        }
        public override void OnUpdateDrawMatrix(ref Matrix drawMatrix)
        {   
            if (Camera == null)
                return;
            EyeTarget = GetLookAt() - Camera.Direction * LookDist;

            var delta = EyeTarget - Camera.Position;
            if (delta.Length() <= 0.1f)
            {
                EyeMoveSpeed = 0;
            }
            else
            {
                EyeMoveSpeed = HookeLaw(HookeFactor, delta.Length());
            }
        }
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
            var dir = EyeTarget - Camera.Position;
            dir.Normalize();
            float moveDist = EyeMoveSpeed * t;// + 0.5f * EyeMoveSpeed * t * t;
            var eye = Camera.Position + dir * moveDist;
            Camera.LookAtLH(eye, GetLookAt(), Vector3.UnitY);

            //EyeMoveSpeed = EyeMoveSpeed + t * EyeMoveAccel;
            var delta = EyeTarget - Camera.Position;
            if (delta.Length() <= 0.1f)
            {
                EyeMoveSpeed = 0;
            }
            else
            {
                //EyeMoveAccel = HookeLaw(HookeFactor, delta.Length());
                EyeMoveSpeed = HookeLaw(HookeFactor, delta.Length());
            }


            dir = Camera.Direction;
            dir.Y = 0;
            dir.Normalize();
            placement.Rotation = Quaternion.GetQuaternion(Vector3.UnitZ, dir);
        }
        private float HookeLaw(float k, float delta)
        {
            return k * delta;
        }
    }
}
