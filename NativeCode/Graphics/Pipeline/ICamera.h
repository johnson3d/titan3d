#pragma once
#include "../../RHI/RHI.h"
#include "../../Math/v3dxFrustum.h"
#include "../../Math/v3dxDVector3.h"

NS_BEGIN

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
ICamera : public VIUnknown
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
		v3dxMatrix4				mViewPortOffsetMatrix;
		v3dxMatrix4				mToViewPortMatrix;

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

	TR_CONSTRUCTOR()
	ICamera();
	~ICamera();

	virtual void Cleanup() override;

	TR_FUNCTION()
	void BindConstBuffer(IRenderContext* rc, IConstantBuffer* cb);

	TR_FUNCTION(SV_SupressGC)
	void PerspectiveFovLH(float fov, float width, float height, float zMin, float zMax);
	TR_FUNCTION(SV_SupressGC)
	void MakeOrtho(float w, float h, float zn, float zf);
	TR_FUNCTION(SV_SupressGC)
	void DoOrthoProjectionForShadow(float w, float h, float znear, float zfar, float TexelOffsetNdcX, float TexelOffsetNdcY);
	TR_FUNCTION(SV_SupressGC)
	void LookAtLH(const v3dxDVector3* eye, const v3dxDVector3* lookAt, const v3dxVector3* up);

	TR_FUNCTION(SV_SupressGC)
	vBOOL GetPickRay(v3dxVector3* pvPickRay, float x, float y, float sw, float sh);
	
	TR_FUNCTION(SV_SupressGC)
	v3dxFrustum* GetFrustum() {
		return &mFrustum;
	}

	TR_FUNCTION(SV_SupressGC)
		v3dxDVector3 GetMatrixStartPosition() const {
		return mLogicData->mMatrixStartPosition;
	}
	TR_FUNCTION(SV_SupressGC)
		void SetMatrixStartPosition(const v3dxDVector3* pos){
		mLogicData->mMatrixStartPosition = *pos;
	}
	TR_FUNCTION(SV_SupressGC)
		v3dxDVector3 GetPosition() const{
		return mLogicData->mPosition;
	}
	TR_FUNCTION(SV_SupressGC)
		v3dxVector3 GetLocalPosition() const {
		return mLogicData->GetLocalPosition();
	}
	TR_FUNCTION(SV_SupressGC)
		v3dxDVector3 GetLookAt() const{
		return mLogicData->mLookAt;
	}
	TR_FUNCTION(SV_SupressGC)
		v3dxVector3 GetLocalLookAt() const {
		return mLogicData->GetLocalLookAt();
	}
	TR_FUNCTION(SV_SupressGC)
	v3dxVector3 GetDirection() const {
		return mLogicData->mDirection;
	}
	TR_FUNCTION(SV_SupressGC)
	v3dxVector3 GetRight() const {
		return mLogicData->mRight;
	}
	TR_FUNCTION(SV_SupressGC)
	v3dxVector3 GetUp() const {
		return mLogicData->mUp;
	}
	TR_FUNCTION(SV_SupressGC)
	v3dxMatrix4 GetViewMatrix() const {
		return mLogicData->mViewMatrix;
	}
	TR_FUNCTION(SV_SupressGC)
	v3dxMatrix4 GetViewInverse() const {
		return mLogicData->mViewInverse;
	}
	TR_FUNCTION(SV_SupressGC)
	v3dxMatrix4 GetProjectionMatrix() const {
		return mLogicData->mProjectionMatrix;
	}
	TR_FUNCTION(SV_SupressGC)
	v3dxMatrix4 GetProjectionInverse() const {
		return mLogicData->mProjectionInverse;
	}
	TR_FUNCTION(SV_SupressGC)
	v3dxMatrix4 GetViewProjection() const {
		return mLogicData->mViewProjection;
	}
	TR_FUNCTION(SV_SupressGC)
	v3dxMatrix4 GetViewProjectionInverse() const {
		return mLogicData->mViewProjectionInverse;
	}
	TR_FUNCTION(SV_SupressGC)
	v3dxMatrix4 GetToViewPortMatrix() const {
		return mLogicData->mToViewPortMatrix;
	}
	TR_FUNCTION(SV_SupressGC)
	v3dxMatrix4 GetViewPortOffsetMatrix() const {
		return mLogicData->mViewPortOffsetMatrix;
	}
	TR_FUNCTION()
	void UpdateConstBufferData(IRenderContext* rc, vBOOL bImm);
protected:
	void UpdateFrustum();
	void UpdateFrustumOrtho();
protected:
	AutoRef<IConstantBuffer>	mCBuffer;
	TR_MEMBER(SV_ReadOnly)
	float					mFov;
	TR_MEMBER(SV_ReadOnly)
	float					mZNear;
	TR_MEMBER(SV_ReadOnly)
	float					mZFar;
	TR_MEMBER(SV_ReadOnly)
	float					mAspect;

	v3dxFrustum				mFrustum;
	TR_MEMBER(SV_ReadOnly)
	bool					mIsOrtho;
	TR_MEMBER(SV_ReadOnly)
	float					mWidth;
	TR_MEMBER(SV_ReadOnly)
	float					mHeight;

public:
	
	UINT					mPositionId;
	UINT					mLookAtId;
	UINT					mDirectionId;
	UINT					mRightId;
	UINT					mUpId;
	UINT					mViewMatrixId;
	UINT					mViewInverseId;
	UINT					mProjectionMatrixId;
	UINT					mProjectionInverseId;
	UINT					mViewProjectionId;
	UINT					mViewProjectionInverseId;
	UINT					mID_ZNear;
	UINT					mID_ZFar;
	UINT					mCameraOffset;

	CameraData*				mLogicData;
	CameraData*				mRenderData;
};

NS_END