using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class UCamera : AuxPtrType<ICamera>
    {
        public UCamera()
        {
            mCoreObject = ICamera.CreateInstance();
        }
        public string Name { get; set; }
        public GamePlay.UWorld.UVisParameter VisParameter;
        public TtGraphicsBuffers.TtTargetViewIdentifier TargetViewIdentifier = new TtGraphicsBuffers.TtTargetViewIdentifier();
        NxRHI.UCbView mPerCameraCBuffer;
        public NxRHI.UCbView PerCameraCBuffer
        {
            get
            {
                if (mPerCameraCBuffer == null)
                {
                    mPerCameraCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(UEngine.Instance.GfxDevice.CoreShaderBinder.CBufferCreator.cbPerCamera);
                    mPerCameraCBuffer.SetDebugName($"Camera");
                    //mCoreObject.BindConstBuffer(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, mPerCameraCBuffer.mCoreObject);
                }
                return mPerCameraCBuffer;
            }
        }
        public void AutoZoom(ref BoundingSphere sphere, bool bOptZRange = true)
        {
            var dist = (sphere.Radius) / (float)Math.Sin((float)this.mCoreObject.mFov);
            var eye = sphere.Center - this.mCoreObject.GetDirection() * dist;
            var up = this.mCoreObject.GetUp();
            if (bOptZRange && this.ZFar < dist)
            {
                SetZRange(this.ZNear, 2.0f * dist);
            }
            mCoreObject.LookAtLH(eye.AsDVector(), sphere.Center.AsDVector(), in up);
        }
        public float GetScaleWithFixSizeInScreen(in DVector3 position, float screenSize)
        {
            Vector3 dir = new Vector3(position - mCoreObject.GetPosition());
            var distance = Vector3.Dot(in dir, mCoreObject.GetDirection());
            var sizeInScreen = 0.5f * MathF.Tan(0.5f * mCoreObject.mFov) * distance;
            return sizeInScreen * screenSize / 400;
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
        public Vector2 JitterOffset
        {
            get
            {
                return mCoreObject.GetJitterOffset();
            }
            set
            {
                mCoreObject.SetJitterOffset(in value);
            }
        }
        #endregion
        #region Function
        public void Cleanup()
        {
            mCoreObject.Cleanup();
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
        public EngineNS.Matrix GetJitterViewProjection()
        {
            return mCoreObject.GetJitterViewProjection();
        }
        public EngineNS.Matrix GetJitterViewProjectionInverse()
        {
            return mCoreObject.GetJitterViewProjectionInverse();
        }
        public EngineNS.Matrix GetViewProjectionInverse()
        {
            return mCoreObject.GetViewProjectionInverse();
        }
        public EngineNS.Matrix GetToViewPortMatrix()
        {
            return mCoreObject.GetToViewPortMatrix();
        }
        public void UpdateConstBufferData(NxRHI.UGpuDevice rc)
        {
            if (PreFrameViewProjectionMatrix == null)
            {
                PreFrameViewProjectionMatrix = GetViewProjection();
            }
            if (JitterPreFrameViewProjectionMatrix == null)
            {
                JitterPreFrameViewProjectionMatrix = GetJitterViewProjection();
            }
            PerCameraCBuffer.SetMatrix(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerCamera.PreFrameViewPrjMtx, PreFrameViewProjectionMatrix.Value);
            PerCameraCBuffer.SetMatrix(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerCamera.JitterPreFrameViewPrjMtx, JitterPreFrameViewProjectionMatrix.Value);
            mCoreObject.UpdateConstBufferData(rc.mCoreObject, PerCameraCBuffer.mCoreObject, true, UEngine.Instance.GfxDevice.CbvUpdater.mCoreObject);
            PreFrameViewProjectionMatrix = GetViewProjection();
            JitterPreFrameViewProjectionMatrix = GetJitterViewProjection();
        }
        Matrix? PreFrameViewProjectionMatrix = null;
        Matrix? JitterPreFrameViewProjectionMatrix = null;
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
        UCamera Camera
        {
            get;
        }
        void ControlCamera(UCamera camera);
        void Rotate(ECameraAxis axis, float angle, bool rotLookAt = false);
        void Move(ECameraAxis axis, float step, bool moveWithLookAt = false);
    }
}
