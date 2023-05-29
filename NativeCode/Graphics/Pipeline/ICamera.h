#pragma once
#include "../../NextRHI/NxRHI.h"
#include "../../Math/v3dxFrustum.h"
#include "../../Math/v3dxDVector3.h"

NS_BEGIN

class TR_CLASS()
	ICamera : public IWeakReference
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
		v3dxMatrix4				mJitterProjectionMatrix;
		v3dxMatrix4				mJitterProjectionInverse;
		v3dxMatrix4				mJitterViewProjection;
		v3dxMatrix4				mJitterViewProjectionInverse;

		v3dxMatrix4				mViewPortOffsetMatrix;
		v3dxMatrix4				mToViewPortMatrix;
		v3dxMatrix4				mJitterToViewPortMatrix;

		v3dxVector3 GetLocalPosition() {
			v3dxVector3 result;
			result.x = (float)(mPosition.x - mMatrixStartPosition.x);
			result.y = (float)(mPosition.y - mMatrixStartPosition.y);
			result.z = (float)(mPosition.z - mMatrixStartPosition.z);
			return result;
		}
		v3dxVector3 GetLocalLookAt() {
			v3dxVector3 result;
			result.x = (float)(mLookAt.x - mMatrixStartPosition.x);
			result.y = (float)(mLookAt.y - mMatrixStartPosition.y);
			result.z = (float)(mLookAt.z - mMatrixStartPosition.z);
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

	vBOOL GetPickRay(v3dxVector3* pvPickRay, float x, float y, float sw, float sh);
	v3dxVector2 GetJitterOffset() const {
		return mJitterOffset;
	}
	void SetJitterOffset(const v3dxVector2& v) {
		mJitterOffset = v;
		PerspectiveFovLH(mFov, mWidth, mHeight, mZNear, mZFar);
	}
	v3dxVector2 GetJitterUV() const{
		v3dxVector2 jitterUV;
		jitterUV.x = (mJitterOffset.x - 0.5f) / mWidth;
		jitterUV.y = (mJitterOffset.y - 0.5f) / mHeight;
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
	v3dxMatrix4 GetJitterProjectionMatrix() const {
		return mLogicData->mJitterProjectionMatrix;
	}
	v3dxMatrix4 GetViewProjection() const {
		return mLogicData->mViewProjection;
	}
	v3dxMatrix4 GetViewProjectionInverse() const {
		return mLogicData->mViewProjectionInverse;
	}
	v3dxMatrix4 GetJitterViewProjection() const {
		return mLogicData->mJitterViewProjection;
	}
	v3dxMatrix4 GetJitterViewProjectionInverse() const {
		return mLogicData->mJitterViewProjectionInverse;
	}
	v3dxMatrix4 GetToViewPortMatrix() const {
		return mLogicData->mToViewPortMatrix;
	}
	v3dxMatrix4 GetViewPortOffsetMatrix() const {
		return mLogicData->mViewPortOffsetMatrix;
	}
	void UpdateConstBufferData(EngineNS::NxRHI::IGpuDevice* device, EngineNS::NxRHI::ICbView* buffer);
protected:
	void UpdateFrustum();
	void UpdateFrustumOrtho();
protected:
	VSLLock					mLocker;
	float					mFov;
	float					mZNear;
	float					mZFar;
	float					mAspect;

	v3dxFrustum				mFrustum;
	bool					mIsOrtho;
	float					mWidth;
	float					mHeight;

	v3dxVector2				mJitterOffset;//0-1
public:
	CameraData*				mLogicData = nullptr;
	CameraData*				mRenderData = nullptr;

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