#pragma once
#include "../../NextRHI/NxRHI.h"
#include "../../Math/v3dxFrustum.h"
#include "../../Math/v3dxDVector3.h"

NS_BEGIN

class TR_CLASS()
	ICamera : public IWeakRefObject
{
public:
	struct CameraData
	{
		CameraData()
		{
			mMatrixStartPosition = v3dxDVector3(0, 0, 0);
		}
		v3dxDVector3			mMatrixStartPosition;
		v3dxDVector3			mPosition;
		v3dxDVector3			mLookAt;
		v3dxVector3				mDirection;
		v3dxVector3				mRight;
		v3dxVector3				mUp;
		v3dxMatrix4				mViewMatrix;
		v3dxMatrix4				mViewInverse;
		
		v3dxMatrix4				mProjectionMatrix;
		v3dxMatrix4				mProjectionInverse;
		v3dxMatrix4				mViewProjection;
		v3dxMatrix4				mViewProjectionInverse;

		// 上一帧 view-projection 矩阵, 用于 motion vector 计算和 TAA reproject.
		// 关键: 必须在 UpdateConstBufferData (一帧一次) 里维护, 不能在 PerspectiveFovLH /
		// LookAtLH 里维护 (一帧可能被调多次, 上一帧值会被自己反复覆盖).
		// 矩阵本身是纯 view-projection, 不含 jitter; jitter 由 cbPerCamera.JitterOffset /
		// PreJitterOffset 在 shader 端单独处理.
		v3dxMatrix4				mPreFrameViewProjection;
		// 标记上一帧矩阵是否已被初始化过 (首帧 = false 时, 用当前帧矩阵兜底, 避免 motion 跳变).
		bool					mHasValidPreFrameMatrix = false;

		v3dxMatrix4				mViewPortOffsetMatrix;
		v3dxMatrix4				mToViewPortMatrix;

		v3dxVector3 GetLocalPosition() {
			v3dxVector3 result;
			result.X = (float)(mPosition.X - mMatrixStartPosition.X);
			result.Y = (float)(mPosition.Y - mMatrixStartPosition.Y);
			result.Z = (float)(mPosition.Z - mMatrixStartPosition.Z);
			return result;
		}
		v3dxVector3 GetLocalLookAt() {
			v3dxVector3 result;
			result.X = (float)(mLookAt.X - mMatrixStartPosition.X);
			result.Y = (float)(mLookAt.Y - mMatrixStartPosition.Y);
			result.Z = (float)(mLookAt.Z - mMatrixStartPosition.Z);
			return result;
		}
	};
public:
	ENGINE_RTTI(ICamera);

	ICamera();
	~ICamera();

	virtual void Cleanup() override;

	void PerspectiveFovLH(float fov, float width, float height, float zMin, float zMax);
	void MakeOrtho(float w, float h, float zn, float zf);
	void DoOrthoProjectionForShadow(float w, float h, float znear, float zfar, float TexelOffsetNdcX, float TexelOffsetNdcY);
	void LookAtLH(const v3dxDVector3* eye, const v3dxDVector3* lookAt, const v3dxVector3* up);

	bool GetPickRay(v3dxVector3* pvPickRay, float x, float y, float sw, float sh);
	bool GetPickRayInViewSpace(v3dxVector3* pvPickRay, float x, float y, float sw, float sh);
	v3dxVector2 GetJitterOffset() const {
		return mJitterOffset;
	}
	// 设置本帧 jitter offset (Halton 原始值 [0,1)). 不再触发投影矩阵重算 -- 矩阵保持
	// 纯 view-projection, jitter 仅以 cbPerCamera.JitterOffset 的形式参与 shader 计算.
	void SetJitterOffset(const v3dxVector2& v) {
		mJitterOffset = v;
	}
	v3dxVector2 GetJitterUV() const{
		v3dxVector2 jitterUV;
		jitterUV.X = (mJitterOffset.X - 0.5f) / mWidth;
		jitterUV.Y = (mJitterOffset.Y - 0.5f) / mHeight;
		return jitterUV;
	}
	// 上一帧的 jitter UV (与 GetJitterUV() 同口径, 由 UpdateConstBufferData 在一帧一次
	// 推进缓存后再写入 cb. 首帧用本帧值兜底).
	v3dxVector2 GetPreJitterUV() const{
		v3dxVector2 jitterUV;
		jitterUV.X = (mPreJitterOffset.X - 0.5f) / mWidth;
		jitterUV.Y = (mPreJitterOffset.Y - 0.5f) / mHeight;
		return jitterUV;
	}
	
	v3dxFrustum* GetFrustum() {
		return &mFrustum;
	}

	v3dxDVector3 GetMatrixStartPosition() const {
		return mLogicData->mMatrixStartPosition;
	}
	void SetMatrixStartPosition(const v3dxDVector3* pos){
		mLogicData->mMatrixStartPosition = *pos;
	}
	v3dxDVector3 GetPosition() const{
		return mLogicData->mPosition;
	}
	v3dxVector3 GetLocalPosition() const {
		return mLogicData->GetLocalPosition();
	}
	v3dxDVector3 GetLookAt() const{
		return mLogicData->mLookAt;
	}
	v3dxVector3 GetLocalLookAt() const {
		return mLogicData->GetLocalLookAt();
	}
	v3dxVector3 GetDirection() const {
		return mLogicData->mDirection;
	}
	v3dxVector3 GetRight() const {
		return mLogicData->mRight;
	}
	v3dxVector3 GetUp() const {
		return mLogicData->mUp;
	}
	v3dxMatrix4 GetViewMatrix() const {
		return mLogicData->mViewMatrix;
	}
	v3dxMatrix4 GetViewInverse() const {
		return mLogicData->mViewInverse;
	}
	v3dxMatrix4 GetProjectionMatrix() const {
		return mLogicData->mProjectionMatrix;
	}
	v3dxMatrix4 GetProjectionInverse() const {
		return mLogicData->mProjectionInverse;
	}
	v3dxMatrix4 GetViewProjection() const {
		return mLogicData->mViewProjection;
	}
	v3dxMatrix4 GetViewProjectionInverse() const {
		return mLogicData->mViewProjectionInverse;
	}
	v3dxMatrix4 GetToViewPortMatrix() const {
		return mLogicData->mToViewPortMatrix;
	}
	v3dxMatrix4 GetViewPortOffsetMatrix() const {
		return mLogicData->mViewPortOffsetMatrix;
	}
	void UpdateConstBufferData(EngineNS::NxRHI::IGpuDevice* device, EngineNS::NxRHI::ICbView* buffer, bool bFlush, EngineNS::NxRHI::FCbvUpdater* pUpdater);
	inline bool GetReverseZ() const {
		return mIsReverseZ;
	}
	void SetReverseZ(bool bReverseZ);

	void CopyDataTo(ICamera* target);
protected:
	void UpdateFrustum();
	void UpdateFrustumOrtho();
protected:
	VSLLock					mLocker;
	bool					mIsReverseZ = false;
	float					mFov;
	float					mZNear;
	float					mZFar;
	float					mAspect;

	v3dxFrustum				mFrustum;
	bool					mIsOrtho;
	float					mWidth;
	float					mHeight;

	// Halton 序列原始值, 取值范围 [0,1), 由 C# 端 TtAntiAliasingNode.TickSyncTAA 写入.
	// 真正用于 shader 端的 jitter UV 是 GetJitterUV(): (mJitterOffset - 0.5) / size,
	// 即 NDC 半像素偏移 (UV 空间), 范围约 [-0.5/size, 0.5/size).
	// 不要在外部把 mJitterOffset 当 NDC 偏移直接写, 否则会偏一倍.
	// 矩阵不再含 jitter, 因此修改 mJitterOffset 也不会触发投影重算.
	v3dxVector2				mJitterOffset;
	// 上一帧的 mJitterOffset, 由 UpdateConstBufferData 在一帧一次 (cb 写完后) 推进.
	// 用于在 shader 端 reproject history 时反 jitter.
	v3dxVector2				mPreJitterOffset;
public:
	CameraData*				mLogicData = nullptr;

	/*const NxRHI::FShaderVarDesc* mPositionId = nullptr;
	const NxRHI::FShaderVarDesc* mLookAtId = nullptr;
	const NxRHI::FShaderVarDesc* mDirectionId = nullptr;
	const NxRHI::FShaderVarDesc* mRightId = nullptr;
	const NxRHI::FShaderVarDesc* mUpId = nullptr;
	const NxRHI::FShaderVarDesc* mViewMatrixId = nullptr;
	const NxRHI::FShaderVarDesc* mViewInverseId = nullptr;
	const NxRHI::FShaderVarDesc* mProjectionMatrixId = nullptr;
	const NxRHI::FShaderVarDesc* mProjectionInverseId = nullptr;
	const NxRHI::FShaderVarDesc* mViewProjectionId = nullptr;
	const NxRHI::FShaderVarDesc* mViewProjectionInverseId = nullptr;
	const NxRHI::FShaderVarDesc* mID_ZNear = nullptr;
	const NxRHI::FShaderVarDesc* mID_ZFar = nullptr;
	const NxRHI::FShaderVarDesc* mCameraOffset = nullptr;*/
};

NS_END