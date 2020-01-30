using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using EngineNS.GamePlay.Component;
using EngineNS.Graphics;

namespace EngineNS.GamePlay.Camera
{
    public class GCamera
    {
        #region CameraProperties
        float mFov = 90.0f * MathHelper.Deg2Rad;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Fov
        {
            get
            {
                return mFov;
            }
            set
            {
                mFov = value;
                PerspectiveFovLH();
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Aspect
        {
            get
            {
                return Width / Height;
            }
        }
        float mZNear = 0.1f;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float ZNear
        {
            get
            {
                return mZNear;
            }
            set
            {
                if (mZNear < 0)
                    return;
                mZNear = value;
                PerspectiveFovLH();
            }
        }
        float mZFar = 200.0f;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float ZFar
        {
            get
            {
                return mZFar;
            }
            set
            {
                if (mZFar < 0)
                    return;
                mZFar = value;
                PerspectiveFovLH();
            }
        }
        float mWidth = -1;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Width
        {
            get
            {
                return mWidth;
            }
            set
            {
                mWidth = value;
                PerspectiveFovLH();
            }
        }
        float mHeight = -1;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Height
        {
            get
            {
                return mHeight;
            }
            set
            {
                mHeight = value;
                PerspectiveFovLH();
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public Vector3 Direction
        {
            get
            {
                if (Camera != null)
                    return Camera.CameraData.Direction;
                return Vector3.UnitX;
            }
        }
        Vector3 mUp;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Up
        {
            get
            {
                return mUp;
            }
            set
            {
                if (mUp == value)
                    return;
                mUp = value;
                LookAtLH();
            }
        }
        Vector3 mPosition;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Position
        {
            get
            {
                return mPosition;
            }
            set
            {
                if (mPosition == value)
                    return;
                mPosition = value;
                LookAtLH();
            }
        }
        Quaternion mRotation = Quaternion.Identity;
        public Quaternion Rotation
        {
            get
            {
                return mRotation;
            }
            set
            {
                if (mRotation == value)
                    return;
                mRotation = value;
                LookAtLH();
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 EulerAngle
        {
            get
            {
                return Rotation.ToEuler() * MathHelper.Rad2Deg;
            }
            set
            {
                Rotation = Quaternion.FromEuler(value * MathHelper.Deg2Rad);
            }
        }
        CGfxCamera mCamera = null;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Browsable(false)]
        public CGfxCamera Camera
        {
            get
            {
                return mCamera;
            }
        }
        public string DebugName { get; set; } = "GCamera";
        #endregion CameraProperties
        public GCamera()
        {

        }
        public bool Init(CRenderContext rc, bool autoFlush)
        {
            mCamera = new CGfxCamera();
            Camera.DebugName = "GCameraComponent";
            if (!Camera.Init(rc, autoFlush))
                return false;
            PerspectiveFovLH();
            var eye = new EngineNS.Vector3();
            eye.SetValue(0.0f, 0.0f, -3.6f);
            var at = new EngineNS.Vector3();
            at.SetValue(0.0f, 0.0f, 0.0f);
            var up = new EngineNS.Vector3();
            up.SetValue(0.0f, 1.0f, 0.0f);
            Camera.LookAtLH(eye, at, up);
            return true;
        }
        void PerspectiveFovLH()
        {
            if (Width <= 0)
                mWidth = Camera.CameraWidth;
            if (Height <= 0)
                mHeight = Camera.CameraHeight;
            Camera.PerspectiveFovLH(Fov, mWidth, mHeight, ZNear, ZFar);
        }
        void LookAtLH()
        {
            var up = EngineNS.Vector3.Dot(Camera.CameraData.Up, EngineNS.Vector3.UnitY) * EngineNS.Vector3.UnitY;
            up.Normalize();
            Vector3 lookAt = Vector3.Zero;
            lookAt = Rotation * (-Vector3.UnitZ) + Position;
            Camera.LookAtLH(Position, lookAt, up);
        }
        void LookAtLH(Vector3 position, Quaternion rotation)
        {
            mPosition = position;
            mRotation = rotation;
            LookAtLH();
        }
    }
}
