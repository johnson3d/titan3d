#include "GfxCamera.h"
#include "Scene/GfxSceneView.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxCamera, EngineNS::VIUnknown);

GfxCamera::GfxCamera()
{
	mIsOrtho = false;
	mOrthoWidth = 0;
	mOrthoHeight = 0;
	mPositionId = -1;
	mLookAtId = -1;
	mDirectionId = -1;
	mRightId = -1;
	mUpId = -1;
	mViewMatrixId = -1;
	mViewInverseId = -1;
	mProjectionMatrixId = -1;
	mID_ZFar = -1;
	
	mLogicData = new CameraData();
	mRenderData = new CameraData();

	mFov = V_PI / 2.0f;
	mZNear = 0.1f;
	mZFar = 500.0f;
	PerspectiveFovLH(mFov, 800.0f, 600.0f, mZNear, mZFar);
	v3dxVector3 eye(0, 10, -10);
	v3dxVector3 lookAt(0, 0, 0);
	v3dxVector3 up(0, 11, 0);
	LookAtLH(&eye, &lookAt, &up, TRUE);
}

GfxCamera::~GfxCamera()
{
	Cleanup();
	Safe_Delete(mLogicData);
	Safe_Delete(mRenderData);
}

void GfxCamera::Cleanup()
{
	if(mCBuffer!=nullptr)
		mCBuffer->Cleanup();
	if (mViewTarget != nullptr)
		mViewTarget->Cleanup();
}

void GfxCamera::BindConstBuffer(IConstantBuffer* cb)
{
	mCBuffer.StrongRef(cb);

	if (mCBuffer != nullptr)
	{
		mPositionId = mCBuffer->FindVar("CameraPosition");
		mLookAtId = mCBuffer->FindVar("CameraLookAt");
		mDirectionId = mCBuffer->FindVar("CameraDirection");
		mRightId = mCBuffer->FindVar("CameraRight");
		mUpId = mCBuffer->FindVar("CameraUp");
		mViewMatrixId = mCBuffer->FindVar("CameraViewMatrix");
		mViewInverseId = mCBuffer->FindVar("CameraViewInverse");
		mViewProjectionId = mCBuffer->FindVar("ViewPrjMtx");
		mViewProjectionInverseId = mCBuffer->FindVar("ViewPrjInvMtx");
		mProjectionMatrixId = mCBuffer->FindVar("PrjMtx");
		mProjectionInverseId = mCBuffer->FindVar("PrjInvMtx");

		mID_ZFar = mCBuffer->FindVar("gZFar");
	}
}

void GfxCamera::UpdateConstBufferData(IRenderContext* rc, vBOOL bImm)
{
	v3dxMatrix4 tempM;
	
	memcpy(mRenderData, mLogicData, sizeof(CameraData));
	if (mCBuffer != nullptr)
	{
		mCBuffer->SetVarValuePtr(mPositionId, &mLogicData->mPosition, sizeof(mLogicData->mPosition), 0);
		mCBuffer->SetVarValuePtr(mLookAtId, &mLogicData->mLookAt, sizeof(mLogicData->mLookAt), 0);
		mCBuffer->SetVarValuePtr(mDirectionId, &mLogicData->mDirection, sizeof(mLogicData->mDirection), 0);
		mCBuffer->SetVarValuePtr(mRightId, &mLogicData->mRight, sizeof(mLogicData->mRight), 0);
		mCBuffer->SetVarValuePtr(mUpId, &mLogicData->mUp, sizeof(mLogicData->mUp), 0);
		v3dxTransposeMatrix4(&tempM, &mLogicData->mViewMatrix);
		mCBuffer->SetVarValuePtr(mViewMatrixId, &tempM, sizeof(tempM), 0);
		v3dxTransposeMatrix4(&tempM, &mLogicData->mViewInverse);
		mCBuffer->SetVarValuePtr(mViewInverseId, &tempM, sizeof(tempM), 0);
		v3dxTransposeMatrix4(&tempM, &mLogicData->mViewProjection);
		mCBuffer->SetVarValuePtr(mViewProjectionId, &tempM, sizeof(tempM), 0);
		v3dxTransposeMatrix4(&tempM, &mLogicData->mViewProjectionInverse);
		mCBuffer->SetVarValuePtr(mViewProjectionInverseId, &tempM, sizeof(tempM), 0);

		v3dxTransposeMatrix4(&tempM, &mLogicData->mProjectionMatrix);
		mCBuffer->SetVarValuePtr(mProjectionMatrixId, &tempM, sizeof(tempM), 0);
		v3dxTransposeMatrix4(&tempM, &mLogicData->mProjectionInverse);
		mCBuffer->SetVarValuePtr(mProjectionInverseId, &tempM, sizeof(tempM), 0);

		/*if (mZFar > 500.0f)
		{
			mZFar = 500.0f;
		}*/
		mCBuffer->SetVarValuePtr(mID_ZFar, &mZFar, sizeof(mZFar), 0);

		mCBuffer->UpdateDrawPass(rc->GetImmCommandList(), bImm);
	}
}

void GfxCamera::UpdateFrustum()
{
	if (mIsOrtho)
	{
		UpdateFrustumOrtho();
		return;
	}
	/*
	6-----7
	|\   /|
	| 2-3 |
	| |+| |
	| 0-1 |
	|/   \|
	4-----5
	*/
	v3dxVector3 vec[8];
	if (false)
	{
		const v3dVector3_t vecFrustum[8] =
		{
			{ -1.0f, -1.0f,  0.0f }, // xyz
		{ 1.0f, -1.0f,  0.0f }, // Xyz
		{ -1.0f,  1.0f,  0.0f }, // xYz
		{ 1.0f,  1.0f,  0.0f }, // XYz
		{ -1.0f, -1.0f,  1.0f }, // xyZ
		{ 1.0f, -1.0f,  1.0f }, // XyZ
		{ -1.0f,  1.0f,  1.0f }, // xYZ
		{ 1.0f,  1.0f,  1.0f }, // XYZ
		};

		for (INT i = 0; i < 8; i++)
			v3dxVec3TransformCoord(&vec[i], &vecFrustum[i], &mLogicData->mViewProjectionInverse);
	}
	else
	{
		float tanTheta = tanf(mFov*0.5f);
		float fHF, fWF, fHN, fWN;
		fHF = mZFar * tanTheta;
		fWF = fHF * mAspect;
		fHN = mZNear * tanTheta;
		fWN = fHN * mAspect;

		v3dxVector3 vecFrustum[8];
		vecFrustum[2].setValue(-fWN, fHN, mZNear);
		//	avStart[ENUM_FRUSTUMCN_LEFTTOP] += m_vCameraPt;
		vecFrustum[3].setValue(fWN, fHN, mZNear);
		//	avStart[ENUM_FRUSTUMCN_RIGHTTOP] += m_vCameraPt;
		vecFrustum[1].setValue(fWN, -fHN, mZNear);
		//	avStart[ENUM_FRUSTUMCN_RIGHTBOTTOM] += m_vCameraPt;
		vecFrustum[0].setValue(-fWN, -fHN, mZNear);
		//	avStart[ENUM_FRUSTUMCN_LEFTBOTTOM] += m_vCameraPt;
		vecFrustum[4 + 2].setValue(-fWF, fHF, mZFar);
		//	avEnd[ENUM_FRUSTUMCN_LEFTTOP] += m_vCameraPt;
		vecFrustum[4 + 3].setValue(fWF, fHF, mZFar);
		//	avEnd[ENUM_FRUSTUMCN_RIGHTTOP] += m_vCameraPt;
		vecFrustum[4 + 1].setValue(fWF, -fHF, mZFar);
		//	avEnd[ENUM_FRUSTUMCN_RIGHTBOTTOM] += m_vCameraPt;
		vecFrustum[4 + 0].setValue(-fWF, -fHF, mZFar);
		//	avEnd[ENUM_FRUSTUMCN_LEFTBOTTOM] += m_vCameraPt;

		for (INT i = 0; i < 8; i++)
			v3dxVec3TransformCoord(&vec[i], &vecFrustum[i], &mLogicData->mViewInverse);
	}

	mFrustum.m_vTipPt = mLogicData->mPosition;
	mFrustum.buildFrustum(vec);
}

void GfxCamera::UpdateFrustumOrtho()
{
	/*
	6-----7
	|\   /|
	| 2-3 |
	| |+| |
	| 0-1 |
	|/   \|
	4-----5
	*/
	float fH, fW;
	fH = mOrthoHeight /2;
	fW = mOrthoWidth / 2;
	v3dxVector3 vecFrustum[8];
	vecFrustum[2].setValue(-fW, fH, mZNear);
	//	avStart[ENUM_FRUSTUMCN_LEFTTOP] += m_vCameraPt;
	vecFrustum[3].setValue(fW, fH, mZNear);
	//	avStart[ENUM_FRUSTUMCN_RIGHTTOP] += m_vCameraPt;
	vecFrustum[1].setValue(fW, -fH, mZNear);
	//	avStart[ENUM_FRUSTUMCN_RIGHTBOTTOM] += m_vCameraPt;
	vecFrustum[0].setValue(-fW, -fH, mZNear);
	//	avStart[ENUM_FRUSTUMCN_LEFTBOTTOM] += m_vCameraPt;
	vecFrustum[4 + 2].setValue(-fW, fH, mZFar);
	//	avEnd[ENUM_FRUSTUMCN_LEFTTOP] += m_vCameraPt;
	vecFrustum[4 + 3].setValue(fW, fH, mZFar);
	//	avEnd[ENUM_FRUSTUMCN_RIGHTTOP] += m_vCameraPt;
	vecFrustum[4 + 1].setValue(fW, -fH, mZFar);
	//	avEnd[ENUM_FRUSTUMCN_RIGHTBOTTOM] += m_vCameraPt;
	vecFrustum[4 + 0].setValue(-fW, -fH, mZFar);
	//	avEnd[ENUM_FRUSTUMCN_LEFTBOTTOM] += m_vCameraPt;

	v3dxVector3 vec[8];
	for (INT i = 0; i < 8; i++)
		v3dxVec3TransformCoord(&vec[i], &vecFrustum[i], &mLogicData->mViewInverse);

	mFrustum.m_vTipPt = mLogicData->mPosition;
	mFrustum.buildFrustum(vec);
}

//void ClampZfar(float znear, float zfar)
//{
//	if (zfar > 500.0f)
//	{
//		zfar = 500.0f;
//	}
//	if (zfar <= znear)
//	{
//		zfar = znear + 1.0f;
//	}
//}

void GfxCamera::PerspectiveFovLH(float fov, float width, float height, float zMin, float zMax)
{
	mIsOrtho = false;
	mZNear = zMin;
	mZFar = zMax;

	//ClampZfar(mZNear, mZFar);

	mFov = fov;
	mAspect = width / height;

	v3dxMatrix4Perspective(&mLogicData->mProjectionMatrix, fov, mAspect, zMin, zMax);
	v3dxMatrix4Inverse(&mLogicData->mProjectionInverse, &mLogicData->mProjectionMatrix, NULL);

	mLogicData->mViewProjection = mLogicData->mViewMatrix * mLogicData->mProjectionMatrix;
	v3dxMatrix4Inverse(&mLogicData->mViewProjectionInverse, &mLogicData->mViewProjection, NULL);

	mLogicData->mViewPortOffsetMatrix.scaleMatrix(width * 0.5f, height * -0.5f, 1.0f);
	mLogicData->mViewPortOffsetMatrix.setTrans(width * 0.5f, height * 0.5f, 0.0f);
	
	v3dxMatrix4Mul(&mLogicData->mToViewPortMatrix, &mLogicData->mViewProjection, &mLogicData->mViewPortOffsetMatrix);
	//UpdateConstBufferData();
	UpdateFrustum();
	
	mLogicData->mProjectionMatrix = mLogicData->mProjectionMatrix;
	mLogicData->mProjectionInverse = mLogicData->mProjectionInverse;
	mLogicData->mViewPortOffsetMatrix = mLogicData->mViewPortOffsetMatrix;
	mLogicData->mToViewPortMatrix = mLogicData->mToViewPortMatrix;
}

void GfxCamera::MakeOrtho(float w, float h, float zn, float zf)
{
	mIsOrtho = true;

	mOrthoWidth = w;
	mOrthoHeight = h;
	mZNear = zn;
	mZFar = zf;
	
	//ClampZfar(mZNear, mZFar);

	v3dxMatrix4Ortho(&mLogicData->mProjectionMatrix, w, h, zn, zf);
	v3dxMatrix4Inverse(&mLogicData->mProjectionInverse, &mLogicData->mProjectionMatrix, NULL);

	mLogicData->mViewProjection = mLogicData->mViewMatrix * mLogicData->mProjectionMatrix;
	v3dxMatrix4Inverse(&mLogicData->mViewProjectionInverse, &mLogicData->mViewProjection, NULL);

	mLogicData->mViewPortOffsetMatrix.scaleMatrix(w * 0.5f, h * -0.5f, 1.0f);
	mLogicData->mViewPortOffsetMatrix.setTrans(w * 0.5f, h * 0.5f, 0.0f);

	v3dxMatrix4Mul(&mLogicData->mToViewPortMatrix, &mLogicData->mViewProjection, &mLogicData->mViewPortOffsetMatrix);
	//UpdateConstBufferData();
	UpdateFrustum();
	
	mLogicData->mProjectionMatrix = mLogicData->mProjectionMatrix;
	mLogicData->mProjectionInverse = mLogicData->mProjectionInverse;
	mLogicData->mViewPortOffsetMatrix = mLogicData->mViewPortOffsetMatrix;
	mLogicData->mToViewPortMatrix = mLogicData->mToViewPortMatrix;
}

void GfxCamera::DoOrthoProjectionForShadow(float w, float h, float znear, float zfar, float TexelOffsetNdcX, float TexelOffsetNdcY)
{
	mZNear = znear;
	mZFar = zfar;
	
	//ClampZfar(mZNear, mZFar);

	v3dxMatrix4OrthoForDirLightShadow(&mLogicData->mProjectionMatrix, w, h, znear, zfar, TexelOffsetNdcX,TexelOffsetNdcY);
	v3dxMatrix4Inverse(&mLogicData->mProjectionInverse, &mLogicData->mProjectionMatrix, NULL);

	mLogicData->mViewProjection = mLogicData->mViewMatrix * mLogicData->mProjectionMatrix;
	v3dxMatrix4Inverse(&mLogicData->mViewProjectionInverse, &mLogicData->mViewProjection, NULL);

	//UpdateConstBufferData();
	UpdateFrustum();
	
	mLogicData->mProjectionMatrix = mLogicData->mProjectionMatrix;
	mLogicData->mProjectionInverse = mLogicData->mProjectionInverse;
	mLogicData->mViewPortOffsetMatrix = mLogicData->mViewPortOffsetMatrix;
	mLogicData->mToViewPortMatrix = mLogicData->mToViewPortMatrix;
}

void GfxCamera::ExecuteLookAtLH()
{
	if (mCameraLookAt != nullptr)
	{
		mCameraLookAt();
		mCameraLookAt = nullptr;
	}
}

void GfxCamera::LookAtLH(const v3dxVector3* eye, const v3dxVector3* lookAt, const v3dxVector3* up, vBOOL imm)
{
	if (imm!=FALSE)
	{
		mLogicData->mPosition = *eye;
		mLogicData->mLookAt = *lookAt;
		mLogicData->mDirection = mLogicData->mLookAt - mLogicData->mPosition;

		v3dxVec3Cross(&mLogicData->mRight, up, &mLogicData->mDirection);
		v3dxVec3Cross(&mLogicData->mUp, &mLogicData->mDirection, &mLogicData->mRight);

		mLogicData->mDirection.normalize();
		mLogicData->mRight.normalize();
		mLogicData->mUp.normalize();

		v3dxMatrixLookAtLH(&mLogicData->mViewMatrix, eye, lookAt, up);
		v3dxMatrix4Inverse(&mLogicData->mViewInverse, &mLogicData->mViewMatrix, NULL);

		mLogicData->mViewProjection = mLogicData->mViewMatrix * mLogicData->mProjectionMatrix;
		v3dxMatrix4Inverse(&mLogicData->mViewProjectionInverse, &mLogicData->mViewProjection, NULL);

		v3dxMatrix4Mul(&mLogicData->mToViewPortMatrix, &mLogicData->mViewProjection, &mLogicData->mViewPortOffsetMatrix);
		//UpdateConstBufferData();
		UpdateFrustum();
	}
	else
	{
		v3dxVector3 ueye = *eye;
		v3dxVector3 ulookAt = *lookAt;
		v3dxVector3 uup = *up;
		mCameraLookAt = [=]()->void {
			mLogicData->mPosition = ueye;
			mLogicData->mLookAt = ulookAt;
			mLogicData->mDirection = mLogicData->mLookAt - mLogicData->mPosition;

			v3dxVec3Cross(&mLogicData->mRight, &uup, &mLogicData->mDirection);
			v3dxVec3Cross(&mLogicData->mUp, &mLogicData->mDirection, &mLogicData->mRight);

			mLogicData->mDirection.normalize();
			mLogicData->mRight.normalize();
			mLogicData->mUp.normalize();

			v3dxMatrixLookAtLH(&mLogicData->mViewMatrix, eye, lookAt, up);
			v3dxMatrix4Inverse(&mLogicData->mViewInverse, &mLogicData->mViewMatrix, NULL);

			mLogicData->mViewProjection = mLogicData->mViewMatrix * mLogicData->mProjectionMatrix;
			v3dxMatrix4Inverse(&mLogicData->mViewProjectionInverse, &mLogicData->mViewProjection, NULL);

			v3dxMatrix4Mul(&mLogicData->mToViewPortMatrix, &mLogicData->mViewProjection, &mLogicData->mViewPortOffsetMatrix);
			//UpdateConstBufferData();
			UpdateFrustum();
		};
	}
}

vBOOL GfxCamera::GetPickRay(v3dxVector3* pvPickRay, float x, float y, float sw, float sh)
{
	if (sw == -1)
	{
		if (mViewTarget!=nullptr)
			sw = mViewTarget->GetViewport()->Width;
	}
	if (sh == -1)
	{
		if (mViewTarget != nullptr)
			sh = mViewTarget->GetViewport()->Height;
	}
	if (x<0 || x>sw || y<0 || y>sh)
		return FALSE;
	v3dVector3_t v;
	v.x = (((2.0f * x) / sw) - 1) / mLogicData->mProjectionMatrix.m11;
	v.y = -(((2.0f * y) / sh) - 1) / mLogicData->mProjectionMatrix.m22;
	v.z = 1.0f;
	//v3dxVector3 v = v3dxVector3( (((2.0f*x)/sw)-1), -(((2.0f*y)/sh)-1), 1.0f) * m_InvProjTM;

	//ת����Ļ�ռ�ļ�ȡ�ߵ�3D�ռ�
	v3dxMatrix4& finaMatrix = mLogicData->mViewInverse;
	pvPickRay->x = v.x*finaMatrix.m11 + v.y*finaMatrix.m21 + v.z*finaMatrix.m31;
	pvPickRay->y = v.x*finaMatrix.m12 + v.y*finaMatrix.m22 + v.z*finaMatrix.m32;
	pvPickRay->z = v.x*finaMatrix.m13 + v.y*finaMatrix.m23 + v.z*finaMatrix.m33;
	pvPickRay->normalize();
	return TRUE;
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpAPI1(EngineNS, GfxCamera, BindConstBuffer, IConstantBuffer*);
	CSharpAPI5(EngineNS, GfxCamera, PerspectiveFovLH, float, float, float, float, float);
	CSharpAPI4(EngineNS, GfxCamera, MakeOrtho, float, float, float, float);
	CSharpAPI6(EngineNS, GfxCamera, DoOrthoProjectionForShadow, float, float, float, float, float, float);
	CSharpAPI4(EngineNS, GfxCamera, LookAtLH, const v3dxVector3*, const v3dxVector3*, const v3dxVector3*, vBOOL);
	CSharpAPI0(EngineNS, GfxCamera, ExecuteLookAtLH);
	CSharpReturnAPI5(vBOOL, EngineNS, GfxCamera, GetPickRay, v3dxVector3*, float, float, float, float);
	CSharpAPI2(EngineNS, GfxCamera, UpdateConstBufferData, IRenderContext*, vBOOL);

	CSharpReturnAPI0(v3dxFrustum*, EngineNS, GfxCamera, GetFrustum);

	CSharpReturnAPI0(float, EngineNS, GfxCamera, GetFov);
	CSharpReturnAPI0(float, EngineNS, GfxCamera, GetZNear);
	CSharpReturnAPI0(float, EngineNS, GfxCamera, GetZFar);
	CSharpReturnAPI0(v3dVector3_t, EngineNS, GfxCamera, GetPosition);
	CSharpReturnAPI0(v3dVector3_t, EngineNS, GfxCamera, GetLookAt);
	CSharpReturnAPI0(v3dVector3_t, EngineNS, GfxCamera, GetDirection);
	CSharpReturnAPI0(v3dVector3_t, EngineNS, GfxCamera, GetRight);
	CSharpReturnAPI0(v3dVector3_t, EngineNS, GfxCamera, GetUp);
	CSharpReturnAPI0(v3dMatrix4_t, EngineNS, GfxCamera, GetViewMatrix);
	CSharpReturnAPI0(v3dMatrix4_t, EngineNS, GfxCamera, GetViewInverse);
	CSharpReturnAPI0(v3dMatrix4_t, EngineNS, GfxCamera, GetProjectionMatrix);
	CSharpReturnAPI0(v3dMatrix4_t, EngineNS, GfxCamera, GetProjectionInverse);
	CSharpReturnAPI0(v3dMatrix4_t, EngineNS, GfxCamera, GetViewProjection);
	CSharpReturnAPI0(v3dMatrix4_t, EngineNS, GfxCamera, GetViewProjectionInverse);
	CSharpReturnAPI0(v3dMatrix4_t, EngineNS, GfxCamera, GetToViewPortMatrix);

	CSharpReturnAPI0(GfxCamera::CameraData*, EngineNS, GfxCamera, GetLogicData);
	CSharpReturnAPI0(GfxCamera::CameraData*, EngineNS, GfxCamera, GetRenderData);
}