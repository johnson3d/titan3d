#pragma once
#include "GfxPreHead.h"

NS_BEGIN

class GfxSceneView;

class GfxCamera : public VIUnknown
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
	RTTI_DEF(GfxCamera, 0x7ef43b215b18857a, true);
	GfxCamera();
	~GfxCamera();

	virtual void Cleanup() override;
	void BindConstBuffer(IConstantBuffer* cb);

	void PerspectiveFovLH(float fov, float width, float height, float zMin, float zMax);
	void MakeOrtho(float w, float h, float zn, float zf);
	void DoOrthoProjectionForShadow(float w, float h, float znear, float zfar, float TexelOffsetNdcX, float TexelOffsetNdcY);
	void LookAtLH(const v3dxVector3* eye, const v3dxVector3* lookAt, const v3dxVector3* up, vBOOL imm);
	void ExecuteLookAtLH();
	vBOOL GetPickRay(v3dxVector3* pvPickRay, float x, float y, float sw, float sh);
	
	v3dxFrustum* GetFrustum() {
		return &mFrustum;
	}

	VDef_ReadOnly(Fov);
	VDef_ReadOnly(ZNear);
	VDef_ReadOnly(ZFar);
	VDef_ReadOnly(LogicData);
	VDef_ReadOnly(RenderData);

	v3dxVector3 GetPosition() const{
		return mLogicData->mPosition;
	}
	v3dxVector3 GetLookAt() const{
		return mLogicData->mLookAt;
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
	void UpdateConstBufferData(IRenderContext* rc, vBOOL bImm);
protected:
	void UpdateFrustum();
	void UpdateFrustumOrtho();
protected:
	AutoRef<IConstantBuffer>	mCBuffer;
	AutoRef<GfxSceneView>	mViewTarget;
	float					mFov;
	float					mZNear;
	float					mZFar;
	float					mAspect;

	v3dxFrustum				mFrustum;
	bool					mIsOrtho;
	float					mOrthoWidth;
	float					mOrthoHeight;

	typedef void (FCameraLookAt)();
	std::function<FCameraLookAt>	mCameraLookAt;
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