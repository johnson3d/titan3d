using EngineNS.BehaviorTree.Composite;
using EngineNS.GamePlay;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class TtCamera : AuxPtrType<ICamera>, ITickable
    {
        public TtCamera()
        {
            mCoreObject = ICamera.CreateInstance();
            mCoreObject.SetReverseZ(TtEngine.Instance.GfxDevice.Config.IsReverseZ);
        }
        public string Name { get; set; }
        public TtGraphicsBuffers.TtTargetViewIdentifier TargetViewIdentifier = new TtGraphicsBuffers.TtTargetViewIdentifier();
        NxRHI.TtCbView mPerCameraCBuffer;
        public NxRHI.TtCbView PerCameraCBuffer
        {
            get
            {
                if (mPerCameraCBuffer == null)
                {
                    mPerCameraCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(NxRHI.TtShader.TtCommonShaderResourceIndexer.Instance.cbPerCamera);
                    mPerCameraCBuffer.SetDebugName($"Camera");
                    //mCoreObject.BindConstBuffer(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, mPerCameraCBuffer.mCoreObject);
                }
                return mPerCameraCBuffer;
            }
        }
        public void AutoZoom(in DBoundingBox aabb, float zoomTimeInSecond = 0.0f, bool bOptZRange = true)
        {
            var ct = aabb.GetCenter();
            var sphere = new DBoundingSphere(ct, (float)aabb.GetMaxSide());
            AutoZoom(in sphere, zoomTimeInSecond, bOptZRange);
        }
        FRotator mEuler;
        public FRotator Euler
        {
            get => mEuler;
        }
        public Quaternion YawFaceToCameraDirection
        {
            get
            {
                return Quaternion.RotationAxis(Vector3.Up, Euler.Yaw);
            }
        }
        public Quaternion FreeFaceToCameraDirection
        {
            get
            {
                var euler = Euler;
                euler.Roll = 0;
                return Quaternion.FromEuler(euler);
            }
        }
        public Quaternion GetYawFaceToCamera(DVector3 objPos)
        {
            var camPos = GetPosition();
            var dir = new Vector3((float)(camPos.X - objPos.X), 0, (float)(camPos.Z - objPos.Z));
            var yawAngle = (float)Math.Atan2(dir.X, dir.Z);
            return Quaternion.RotationAxis(Vector3.Up, yawAngle);
        }
        public Quaternion GetFreeFaceToCamera(DVector3 objPos)
        {
            var camPos = GetPosition();
            var dir = new Vector3((float)(camPos.X - objPos.X), (float)(camPos.Y - objPos.Y), (float)(camPos.Z - objPos.Z));
            var length = dir.Length();
            if (length < MathHelper.Epsilon)
                return Quaternion.Identity;
            dir /= length;
            var euler = new FRotator();
            euler.Yaw = (float)Math.Atan2(dir.X, dir.Z);
            euler.Pitch = -(float)Math.Asin(dir.Y);
            euler.Roll = 0;
            return Quaternion.FromEuler(in euler);
        }
        DVector3 TargetEye;
        DVector3 TargetLookAt;
        Vector3 TargetUp;
        DVector3 TargetEyeMoveSpeed;
        DVector3 TargetLookAtMoveSpeed;
        Vector3 TargetUpMoveSpeed;
        float mZoomTime = 0;
        static bool IsFinite(in DVector3 value)
        {
            return double.IsFinite(value.X) && double.IsFinite(value.Y) && double.IsFinite(value.Z);
        }
        static bool IsFinite(in Vector3 value)
        {
            return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
        }
        static bool TryNormalize(ref Vector3 value)
        {
            if (!IsFinite(in value))
                return false;

            var maxComponent = Math.Max(Math.Abs(value.X), Math.Max(Math.Abs(value.Y), Math.Abs(value.Z)));
            if (!float.IsFinite(maxComponent) || maxComponent <= MathHelper.Epsilon)
                return false;

            value /= maxComponent;
            var length = value.Normalize();
            return float.IsFinite(length) && length > MathHelper.Epsilon && IsFinite(in value);
        }
        public void AutoZoom(in DBoundingSphere sphere, float zoomTimeInSecond = 0.0f, bool bOptZRange = true)
        {
            if (!IsFinite(in sphere.Center) || !double.IsFinite(sphere.Radius) || sphere.Radius <= MathHelper.Epsilon)
                return;

            var fov = mCoreObject.mFov;
            if (!float.IsFinite(fov))
                return;
            var sinFov = Math.Abs(Math.Sin(fov));
            if (!double.IsFinite(sinFov) || sinFov <= MathHelper.Epsilon)
                return;

            var dist = sphere.Radius / sinFov;
            if (!double.IsFinite(dist) || dist <= MathHelper.Epsilon)
                return;

            var direction = mCoreObject.GetDirection();
            if (!TryNormalize(ref direction))
                direction = Vector3.Forward;
            var eye = sphere.Center - direction.AsDVector() * dist;
            if (!IsFinite(in eye))
                return;

            var up = mCoreObject.GetUp();
            if (!TryNormalize(ref up))
                up = Vector3.Up;

            if (bOptZRange && (!float.IsFinite(ZFar) || ZFar < dist))
            {
                var nextZFar = dist * 2.0;
                if (double.IsFinite(nextZFar) && nextZFar <= float.MaxValue)
                {
                    var nextZNear = float.IsFinite(ZNear) && ZNear > MathHelper.Epsilon ? ZNear : 0.3f;
                    SetZRange(nextZNear, (float)nextZFar);
                }
            }

            var useAnimation = float.IsFinite(zoomTimeInSecond) && zoomTimeInSecond > MathHelper.Epsilon;
            if (!useAnimation)
            {
                LookAtLH(eye, sphere.Center, in up);
                TtEngine.Instance.TickableManager.RemoveTickable(this);
                return;
            }

            var currentEye = mCoreObject.GetPosition();
            var currentLookAt = mCoreObject.GetLookAt();
            var eyeMoveSpeed = (eye - currentEye) / zoomTimeInSecond;
            var lookAtMoveSpeed = (sphere.Center - currentLookAt) / zoomTimeInSecond;
            if (!IsFinite(in currentEye) || !IsFinite(in currentLookAt) ||
                !IsFinite(in eyeMoveSpeed) || !IsFinite(in lookAtMoveSpeed))
            {
                LookAtLH(eye, sphere.Center, in up);
                TtEngine.Instance.TickableManager.RemoveTickable(this);
                return;
            }

            mZoomTime = zoomTimeInSecond;
            TargetEye = eye;
            TargetLookAt = sphere.Center;
            TargetUp = up;
            TargetEyeMoveSpeed = eyeMoveSpeed;
            TargetLookAtMoveSpeed = lookAtMoveSpeed;
            TargetUpMoveSpeed = Vector3.Zero;
            TtEngine.Instance.TickableManager.AddTickable(this);
        }
        public float GetScaleWithFixSizeInScreen(in DVector3 position, float screenSize, float divValue = -1)
        {
            Vector3 dir = new Vector3(position - mCoreObject.GetPosition());
            var distance = Vector3.Dot(in dir, mCoreObject.GetDirection());
            var sizeInScreen = 0.5f * MathF.Tan(0.5f * mCoreObject.mFov) * distance;
            if (divValue < 0)
            {
                divValue = mCoreObject.mWidth;
            }
            return sizeInScreen * screenSize / divValue;
        }
        public void SetZRange(float zNear = 0.3f, float zFar = 1000.0f)
        {
            if (!float.IsFinite(zNear) || !float.IsFinite(zFar) ||
                zNear <= MathHelper.Epsilon || zFar <= zNear + MathHelper.Epsilon ||
                !float.IsFinite(mCoreObject.mFov) || !float.IsFinite(mCoreObject.mWidth) ||
                !float.IsFinite(mCoreObject.mHeight) || mCoreObject.mWidth <= MathHelper.Epsilon ||
                mCoreObject.mHeight <= MathHelper.Epsilon)
                return;

            mCoreObject.PerspectiveFovLH(mCoreObject.mFov, mCoreObject.mWidth, mCoreObject.mHeight, zNear, zFar);
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeWhichContainTypeFast;
        private static Profiler.TimeScope ScopeWhichContainTypeFast
        {
            get
            {
                if (mScopeWhichContainTypeFast == null)
                    mScopeWhichContainTypeFast = new Profiler.TimeScope(typeof(TtCamera), nameof(WhichContainTypeFast));
                return mScopeWhichContainTypeFast;
            }
        }
        
        public unsafe CONTAIN_TYPE WhichContainTypeFast(GamePlay.TtWorld world, in EngineNS.DBoundingBox dAabb, bool testInner)
        {
            using (new Profiler.TimeScopeHelper(ScopeWhichContainTypeFast))
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
            if (!IsFinite(in eye) || !IsFinite(in lookAt))
                return;

            var delta = lookAt - eye;
            if (!IsFinite(in delta))
                return;
            var maxDelta = Math.Max(Math.Abs(delta.X), Math.Max(Math.Abs(delta.Y), Math.Abs(delta.Z)));
            if (!double.IsFinite(maxDelta) || maxDelta <= double.Epsilon)
                return;

            var direction = new Vector3(
                (float)(delta.X / maxDelta),
                (float)(delta.Y / maxDelta),
                (float)(delta.Z / maxDelta));
            if (!TryNormalize(ref direction))
                return;

            var safeUp = up;
            if (!TryNormalize(ref safeUp))
                safeUp = Vector3.Up;
            if (Math.Abs(Vector3.Dot(in direction, in safeUp)) > 0.999f)
                safeUp = Math.Abs(direction.Y) < 0.999f ? Vector3.Up : Vector3.Right;

            unsafe
            {
                var pinned_up = &safeUp;
                fixed (EngineNS.DVector3* pinned_eye = &eye)
                fixed (EngineNS.DVector3* pinned_lookAt = &lookAt)
                {
                    mCoreObject.LookAtLH(pinned_eye, pinned_lookAt, pinned_up);
                }
                var quat = Quaternion.RotationMatrix(GetViewMatrix());
                var euler = quat.ToEuler();
                if (float.IsFinite(euler.Yaw) && float.IsFinite(euler.Pitch) && float.IsFinite(euler.Roll))
                    mEuler = euler;
            }
        }
        public bool GetPickRay(ref EngineNS.Vector3 pvPickRay, float x, float y, float sw, float sh)
        {
            unsafe
            {
                fixed (EngineNS.Vector3* pinned_pvPickRay = &pvPickRay)
                {
                    return mCoreObject.GetPickRay(pinned_pvPickRay, x, y, sw, sh);
                }
            }
        }
        public bool GetPickRayInViewSpace(ref EngineNS.Vector3 pvPickRay, float x, float y, float sw, float sh)
        {
            unsafe
            {
                fixed (EngineNS.Vector3* pinned_pvPickRay = &pvPickRay)
                {
                    return mCoreObject.GetPickRayInViewSpace(pinned_pvPickRay, x, y, sw, sh);
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
        public void UpdateConstBufferData(NxRHI.TtGpuDevice rc, NxRHI.TtCbView.EUpdateMode mode = NxRHI.TtCbView.EUpdateMode.Auto)
        {
            mCoreObject.UpdateConstBufferData(rc.mCoreObject, PerCameraCBuffer.mCoreObject, true, mode == NxRHI.TtCbView.EUpdateMode.Immediately ? new NxRHI.FCbvUpdater() : TtEngine.Instance.GfxDevice.CbvUpdater.mCoreObject);
        }

        public void TickLogic(float ellapse)
        {
            if(mZoomTime >= 0)
            {
                var ellapseSecond = ellapse * 0.001f;
                var eyePos = mCoreObject.GetPosition();
                var eyeLookAt = mCoreObject.GetLookAt();
                var eyeUp = mCoreObject.GetUp();
                var deltaEye = TargetEye - eyePos;
                var deltaLookAt = TargetLookAt - eyeLookAt;
                var deltaUp = TargetUp - eyeUp;
                var speedEye = TargetEyeMoveSpeed * ellapseSecond;
                var speedLookAt = TargetLookAtMoveSpeed * ellapseSecond;
                var speedUp = TargetUpMoveSpeed * ellapseSecond;

                eyePos += speedEye;
                var deltaE = (TargetEye - eyePos) * deltaEye;
                if (deltaE.X <= 0 &&
                    deltaE.Y <= 0 &&
                    deltaE.Z <= 0)
                {
                    eyePos = TargetEye;
                }
                eyeLookAt += speedLookAt;
                var deltaL = (TargetLookAt - eyeLookAt) * deltaLookAt;
                if(deltaL.X <= 0 &&
                   deltaL.Y <= 0 &&
                   deltaL.Z <= 0)
                {
                    eyeLookAt = TargetLookAt;
                }
                eyeUp += speedUp;
                var deltaU = (TargetUp - eyeUp) * deltaUp;
                if(deltaU.X <= 0 &&
                   deltaU.Y <= 0 &&
                   deltaU.Z <= 0)
                {
                    eyeUp = TargetUp;
                }

                LookAtLH(eyePos, eyeLookAt, in eyeUp);
                mZoomTime -= ellapseSecond;
                if ((deltaE.X <= 0 && deltaE.Y <= 0 && deltaE.Z <= 0 &&
                     deltaL.X <= 0 && deltaL.Y <= 0 && deltaL.Z <= 0 &&
                     deltaU.X <= 0 && deltaU.Y <= 0 && deltaU.Z <= 0) ||
                     mZoomTime <= 0)
                {
                    mZoomTime = 0;
                    LookAtLH(TargetEye, TargetLookAt, in TargetUp);
                    TtEngine.Instance.TickableManager.RemoveTickable(this);
                }
            }
        }

        public void TickRender(float ellapse)
        {
        }

        public void TickBeginFrame(float ellapse)
        {
        }

        public void TickSync(float ellapse)
        {
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
        TtCamera Camera
        {
            get;
        }
        void ControlCamera(TtCamera camera);
        void Rotate(ECameraAxis axis, float angle, bool rotLookAt = false);
        void Move(ECameraAxis axis, float step, bool moveWithLookAt = false);
    }
}
