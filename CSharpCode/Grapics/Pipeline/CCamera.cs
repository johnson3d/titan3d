using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class CCamera : AuxPtrType<ICamera>
    {
        public CCamera()
        {
            mCoreObject = ICamera.CreateInstance();
        }
        RHI.CConstantBuffer mPerCameraCBuffer;
        public RHI.CConstantBuffer PerCameraCBuffer
        {
            get
            {
                if (mPerCameraCBuffer == null)
                {
                    var effect = UEngine.Instance.GfxDevice.EffectManager.DummyEffect;
                    mPerCameraCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(effect.ShaderProgram, effect.CBPerCameraIndex);
                    mPerCameraCBuffer.mCoreObject.NativeSuper.SetDebugName($"Camera");
                    mCoreObject.BindConstBuffer(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, mPerCameraCBuffer.mCoreObject);
                }
                return mPerCameraCBuffer;
            }
        }
        public void AutoZoom(ref BoundingSphere sphere)
        {
            var dist = (sphere.Radius) / (float)Math.Sin((float)this.mCoreObject.mFov);
            var eye = sphere.Center - this.mCoreObject.GetDirection() * dist;
            var up = this.mCoreObject.GetUp();
            mCoreObject.LookAtLH(eye.AsDVector(), sphere.Center.AsDVector(), in up);
        }
        public void SetZRange(float zNear = 0.3f, float zFar = 1000.0f)
        {
            mCoreObject.PerspectiveFovLH(mCoreObject.mFov, mCoreObject.mWidth, mCoreObject.mHeight, zNear, zFar);
        }
        public CONTAIN_TYPE WhichContainTypeFast(GamePlay.UWorld world, in EngineNS.DBoundingBox dAabb, bool testInner)
        {
            unsafe
            {
                var frustum = mCoreObject.GetFrustum();

                BoundingBox aabb;
                DBoundingBox.OffsetToSingleBox(in world.mCameraOffset, in dAabb, out aabb);
                return frustum->whichContainTypeFast(in aabb, testInner ? 1 : 0);
            }
        }

        #region Fields
        public float Fov
        {
            get
            {
                return mCoreObject.mFov;
            }
        }
        public float ZNear
        {
            get
            {
                return mCoreObject.mZNear;
            }
        }
        public float ZFar
        {
            get
            {
                return mCoreObject.mZFar;
            }
        }
        public float Aspect
        {
            get
            {
                return mCoreObject.mAspect;
            }
        }
        public v3dxFrustum Frustum
        {
            get
            {
                return mCoreObject.mFrustum;
            }
            set
            {
                mCoreObject.mFrustum = value;
            }
        }
        public bool IsOrtho
        {
            get
            {
                return mCoreObject.mIsOrtho;
            }
        }
        public float Width
        {
            get
            {
                return mCoreObject.mWidth;
            }
        }
        public float Height
        {
            get
            {
                return mCoreObject.mHeight;
            }
        }
        public uint PositionId
        {
            get
            {
                return mCoreObject.mPositionId;
            }
            set
            {
                mCoreObject.mPositionId = value;
            }
        }
        public uint LookAtId
        {
            get
            {
                return mCoreObject.mLookAtId;
            }
            set
            {
                mCoreObject.mLookAtId = value;
            }
        }
        public uint DirectionId
        {
            get
            {
                return mCoreObject.mDirectionId;
            }
            set
            {
                mCoreObject.mDirectionId = value;
            }
        }
        public uint RightId
        {
            get
            {
                return mCoreObject.mRightId;
            }
            set
            {
                mCoreObject.mRightId = value;
            }
        }
        public uint UpId
        {
            get
            {
                return mCoreObject.mUpId;
            }
            set
            {
                mCoreObject.mUpId = value;
            }
        }
        public uint ViewMatrixId
        {
            get
            {
                return mCoreObject.mViewMatrixId;
            }
            set
            {
                mCoreObject.mViewMatrixId = value;
            }
        }
        public uint ViewInverseId
        {
            get
            {
                return mCoreObject.mViewInverseId;
            }
            set
            {
                mCoreObject.mViewInverseId = value;
            }
        }
        public uint ProjectionMatrixId
        {
            get
            {
                return mCoreObject.mProjectionMatrixId;
            }
            set
            {
                mCoreObject.mProjectionMatrixId = value;
            }
        }
        public uint ProjectionInverseId
        {
            get
            {
                return mCoreObject.mProjectionInverseId;
            }
            set
            {
                mCoreObject.mProjectionInverseId = value;
            }
        }
        public uint ViewProjectionId
        {
            get
            {
                return mCoreObject.mViewProjectionId;
            }
            set
            {
                mCoreObject.mViewProjectionId = value;
            }
        }
        public uint ViewProjectionInverseId
        {
            get
            {
                return mCoreObject.mViewProjectionInverseId;
            }
            set
            {
                mCoreObject.mViewProjectionInverseId = value;
            }
        }
        public uint ID_ZNear
        {
            get
            {
                return mCoreObject.mID_ZNear;
            }
            set
            {
                mCoreObject.mID_ZNear = value;
            }
        }
        public uint ID_ZFar
        {
            get
            {
                return mCoreObject.mID_ZFar;
            }
            set
            {
                mCoreObject.mID_ZFar = value;
            }
        }
        public uint CameraOffset
        {
            get
            {
                return mCoreObject.mCameraOffset;
            }
            set
            {
                mCoreObject.mCameraOffset = value;
            }
        }
        #endregion
        #region Function

        public void Cleanup()
        {
            mCoreObject.Cleanup();
        }
        public void BindConstBuffer(EngineNS.IRenderContext rc, EngineNS.IConstantBuffer cb)
        {
            mCoreObject.BindConstBuffer(rc, cb);
        }
        public void PerspectiveFovLH(float fov, float width, float height, float zMin, float zMax)
        {
            mCoreObject.PerspectiveFovLH(fov, width, height, zMin, zMax);
        }
        public void MakeOrtho(float w, float h, float zn, float zf)
        {
            mCoreObject.MakeOrtho(w, h, zn, zf);
        }
        public void DoOrthoProjectionForShadow(float w, float h, float znear, float zfar, float TexelOffsetNdcX, float TexelOffsetNdcY)
        {
            mCoreObject.DoOrthoProjectionForShadow(w, h, znear, zfar, TexelOffsetNdcX, TexelOffsetNdcY);
        }
        public void LookAtLH(in EngineNS.DVector3 eye, in EngineNS.DVector3 lookAt, in EngineNS.Vector3 up)
        {
            unsafe
            {
                fixed (EngineNS.DVector3* pinned_eye = &eye)
                fixed (EngineNS.DVector3* pinned_lookAt = &lookAt)
                fixed (EngineNS.Vector3* pinned_up = &up)
                {
                    mCoreObject.LookAtLH(pinned_eye, pinned_lookAt, pinned_up);
                }
            }
        }
        public int GetPickRay(ref EngineNS.Vector3 pvPickRay, float x, float y, float sw, float sh)
        {
            unsafe
            {
                fixed (EngineNS.Vector3* pinned_pvPickRay = &pvPickRay)
                {
                    return mCoreObject.GetPickRay(pinned_pvPickRay, x, y, sw, sh);
                }
            }
        }
        public v3dxFrustum GetFrustum()
        {
            unsafe
            {
                v3dxFrustum frustum;
                frustum = *mCoreObject.GetFrustum();
                return frustum;
            }
        }
        public EngineNS.DVector3 GetMatrixStartPosition()
        {
            return mCoreObject.GetMatrixStartPosition();
        }
        public void SetMatrixStartPosition(in EngineNS.DVector3 pos)
        {
            unsafe
            {
                fixed (EngineNS.DVector3* pinned_pos = &pos)
                {
                    mCoreObject.SetMatrixStartPosition(pinned_pos);
                }
            }
        }
        public EngineNS.DVector3 GetPosition()
        {
            return mCoreObject.GetPosition();
        }
        public EngineNS.Vector3 GetLocalPosition()
        {
            return mCoreObject.GetLocalPosition();
        }
        public EngineNS.DVector3 GetLookAt()
        {
            return mCoreObject.GetLookAt();
        }
        public EngineNS.Vector3 GetLocalLookAt()
        {
            return mCoreObject.GetLocalLookAt();
        }
        public EngineNS.Vector3 GetDirection()
        {
            return mCoreObject.GetDirection();
        }
        public EngineNS.Vector3 GetRight()
        {
            return mCoreObject.GetRight();
        }
        public EngineNS.Vector3 GetUp()
        {
            return mCoreObject.GetUp();
        }
        public EngineNS.Matrix GetViewMatrix()
        {
            return mCoreObject.GetViewMatrix();
        }
        public EngineNS.Matrix GetViewInverse()
        {
            return mCoreObject.GetViewInverse();
        }
        public EngineNS.Matrix GetProjectionMatrix()
        {
            return mCoreObject.GetProjectionMatrix();
        }
        public EngineNS.Matrix GetProjectionInverse()
        {
            return mCoreObject.GetProjectionInverse();
        }
        public EngineNS.Matrix GetViewProjection()
        {
            return mCoreObject.GetViewProjection();
        }
        public EngineNS.Matrix GetViewProjectionInverse()
        {
            return mCoreObject.GetViewProjectionInverse();
        }
        public EngineNS.Matrix GetToViewPortMatrix()
        {
            return mCoreObject.GetToViewPortMatrix();
        }
        public void UpdateConstBufferData(EngineNS.IRenderContext rc, int bImm)
        {
            mCoreObject.UpdateConstBufferData(rc, bImm);
        }
        #endregion
    }

    public enum ECameraAxis
    {
        Forward,
        Up,
        Right,
    }


    public interface ICameraController
    {
        CCamera Camera
        {
            get;
        }
        void ControlCamera(CCamera camera);
        void Rotate(ECameraAxis axis, float angle, bool rotLookAt = false);
        void Move(ECameraAxis axis, float step, bool moveWithLookAt = false);
    }
}
