#pragma once
#include "../../RHI/RHI.h"
#include "../../Math/v3dxFrustum.h"
#include "../../Math/v3dxFrustum.h"

NS_BEGIN

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
ICamera : public VIUnknown
{
public:
	struct CameraData
	{
		v3dxVector3				mPosition;
		v3dxVector3				mLookAt;
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
	};
public:
	RTTI_DEF(ICamera, 0x7ef43b215b18857a, true);

	TR_CONSTRUCTOR()
	ICamera();
	~ICamera();

	virtual void Cleanup() override;

	TR_FUNCTION()
	void BindConstBuffer(IRenderContext* rc, IConstantBuffer* cb);

	TR_FUNCTION()
	void PerspectiveFovLH(float fov, float width, float height, float zMin, float zMax);
	TR_FUNCTION()
	void MakeOrtho(float w, float h, float zn, float zf);
	TR_FUNCTION()
	void DoOrthoProjectionForShadow(float w, float h, float znear, float zfar, float TexelOffsetNdcX, float TexelOffsetNdcY);
	TR_FUNCTION()
	void LookAtLH(const v3dxVector3* eye, const v3dxVector3* lookAt, const v3dxVector3* up);

	TR_FUNCTION()
	vBOOL GetPickRay(v3dxVector3* pvPickRay, float x, float y, float sw, float sh);
	
	TR_FUNCTION()
	v3dxFrustum* GetFrustum() {
		return &mFrustum;
	}

	TR_FUNCTION(SV_ReturnConverter = v3dVector3_t)
	v3dxVector3 GetPosition() const{
		return mLogicData->mPosition;
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector3_t)
	v3dxVector3 GetLookAt() const{
		return mLogicData->mLookAt;
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector3_t)
	v3dxVector3 GetDirection() const {
		return mLogicData->mDirection;
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector3_t)
	v3dxVector3 GetRight() const {
		return mLogicData->mRight;
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector3_t)
	v3dxVector3 GetUp() const {
		return mLogicData->mUp;
	}
	TR_FUNCTION(SV_ReturnConverter = v3dMatrix4_t)
	v3dxMatrix4 GetViewMatrix() const {
		return mLogicData->mViewMatrix;
	}
	TR_FUNCTION(SV_ReturnConverter = v3dMatrix4_t)
	v3dxMatrix4 GetViewInverse() const {
		return mLogicData->mViewInverse;
	}
	TR_FUNCTION(SV_ReturnConverter = v3dMatrix4_t)
	v3dxMatrix4 GetProjectionMatrix() const {
		return mLogicData->mProjectionMatrix;
	}
	TR_FUNCTION(SV_ReturnConverter = v3dMatrix4_t)
	v3dxMatrix4 GetProjectionInverse() const {
		return mLogicData->mProjectionInverse;
	}
	TR_FUNCTION(SV_ReturnConverter = v3dMatrix4_t)
	v3dxMatrix4 GetViewProjection() const {
		return mLogicData->mViewProjection;
	}
	TR_FUNCTION(SV_ReturnConverter = v3dMatrix4_t)
	v3dxMatrix4 GetViewProjectionInverse() const {
		return mLogicData->mViewProjectionInverse;
	}
	TR_FUNCTION(SV_ReturnConverter = v3dMatrix4_t)
	v3dxMatrix4 GetToViewPortMatrix() const {
		return mLogicData->mToViewPortMatrix;
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
	float					mOrthoWidth;
	float					mOrthoHeight;

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
	UINT					mID_ZFar;

	CameraData*				mLogicData;
	CameraData*				mRenderData;
};

NS_END