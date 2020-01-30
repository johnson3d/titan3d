#include "GfxParticleSystem.h"
#include "../../Graphics/GfxCamera.h"
#include "../../Core/vfxSampCounter.h"

#define  new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxParticleSystem, EngineNS::VIUnknown);

GfxParticleSystem::~GfxParticleSystem(void)
{
	Cleanup();
}

void GfxParticleSystem::Cleanup()
{
	for (auto i : mSubStates)
	{
		Safe_Release(i);
	}
	mSubStates.clear();
}

vBOOL GfxParticleSystem::InitParticlePool(IRenderContext* rc, int maxNum, int state)
{
	mParticleLiveTime = new v3dScalarVariable();
	
	mPools.Init(maxNum, state);
	mSubStates.resize(state);
	for (int i = 0; i < state; i++)
	{
		mSubStates[i] = new GfxParticleSubState();
	}

	if (rc != nullptr)
	{
		IVertexBufferDesc vbDesc;
		vbDesc.CPUAccess = CAS_WRITE;
		vbDesc.ByteWidth = mIsTrail == false ? maxNum * sizeof(v3dxVector3) : sizeof(v3dxVector3);
		vbDesc.Stride = sizeof(v3dxVector3);
		mPosVB = rc->CreateVertexBuffer(&vbDesc);
		vbDesc.ByteWidth = mIsTrail == false ? maxNum * sizeof(v3dxQuaternion) : sizeof(v3dxQuaternion);
		vbDesc.Stride = sizeof(v3dxQuaternion);
		mScaleVB = rc->CreateVertexBuffer(&vbDesc);
		vbDesc.ByteWidth = mIsTrail == false ? maxNum * sizeof(v3dxQuaternion) : sizeof(v3dxQuaternion);
		vbDesc.Stride = sizeof(v3dxQuaternion);
		mRotateVB = rc->CreateVertexBuffer(&vbDesc);
		vbDesc.ByteWidth = maxNum * sizeof(v3dxQuaternion);
		vbDesc.Stride = sizeof(v3dxQuaternion);
		mColorVB = rc->CreateVertexBuffer(&vbDesc);
	}
	return TRUE;
}

void GfxParticleSystem::Simulate(float elaspedTime)
{
	mCurLiveTime += elaspedTime;
	mDeathParticles.clear();
	for (auto i = mParticles.begin(); i != mParticles.end(); )
	{
		auto p = *i;
		if (p->Update(elaspedTime) ==FALSE)
		{
			i = mParticles.erase(i);
			mPools.FreeParticle(p);
			mDeathParticles.push_back(p);
		}
		else
		{
			i++;
		}
	}
}

void GfxParticleSystem::ClearParticles()
{
	mDeathParticles.clear();
	for (auto i = mParticles.begin(); i != mParticles.end(); )
	{
		auto p = *i;
		i = mParticles.erase(i);
		mPools.FreeParticle(p);
	}
}



int GfxParticleSystem::FireParticles(int num)
{
	if (mPools.GetRemainNum() < num)
	{
		num = mPools.GetRemainNum();
	}

	for (int i = 0; i < num; i++)
	{
		auto p = mPools.AllocParticle();
		if (p == nullptr)
			return i;

		p->mLife = mParticleLiveTime->getRandomValue();
		for (size_t j = 0; j < mSubStates.size(); j++)
		{
			if (i == 0)
			{
				mSubStates[j]->mNewBorns.resize(num);
			}
			GfxParticleState* ps = p->mStates[j];
			ps->Reset();
			mSubStates[j]->SetShapeData(ps);
			mSubStates[j]->mNewBorns[i] = ps;


		}

		mParticles.push_back(p);
	}

	mPrevFireTime = mCurLiveTime;
	return num;
}

void GfxParticleSystem::Flush2VB(ICommandList* cmd, vBOOL bImm)
{
	auto size = (UINT)mParticles.size();
	if (size == 0)
		return;

	std::vector<v3dVector3_t> pos;
	std::vector<v3dVector4_t> scale;
	std::vector<v3dVector4_t> quat;
	std::vector<v3dUInt32_4> clr;
	

	AUTO_SAMP("Native.GfxParticleSystem.UpdateVB");
	if (mIsTrail == false)
	{
		pos.resize(size);
		scale.resize(size);
		quat.resize(size);
		clr.resize(size);

		for (size_t i = 0; i < size; i++)
		{
			if (i >= mParticles.size())
				break;
			auto p = mParticles[i];
			pos[i] = p->mFinalPose.mPosition;
			scale[i].x = p->mFinalPose.mScale.x;
			scale[i].y = p->mFinalPose.mScale.y;
			scale[i].z = p->mFinalPose.mScale.z;
			scale[i].w = 0;
			quat[i] = p->mFinalPose.mRotation;
			clr[i] = p->mFinalPose.mColor;
		}

		mPosVB->UpdateGPUBuffData(cmd, &pos[0], sizeof(v3dxVector3)*size);
		mScaleVB->UpdateGPUBuffData(cmd, &scale[0], sizeof(v3dxQuaternion)*size);
		mRotateVB->UpdateGPUBuffData(cmd, &quat[0], sizeof(v3dxQuaternion)*size);
		mColorVB->UpdateGPUBuffData(cmd, &clr[0], sizeof(v3dxColor4)*size);
	}
	else
	{
		pos.resize(1);
		scale.resize(1);
		quat.resize(1);
		clr.resize(1);

		pos[0].x = 0;
		pos[0].y = 0;
		pos[0].z = 0;

		scale[0].x = 1;
		scale[0].y = 1;
		scale[0].z = 1;

		quat[0].x = 0;
		quat[0].y = 0;
		quat[0].z = 0;
		quat[0].w = 1;

		clr[0] = v3dUInt32_4::GetVar(0,0,0,0);

		mPosVB->UpdateGPUBuffData(cmd, &pos[0], sizeof(v3dxVector3));
		mScaleVB->UpdateGPUBuffData(cmd, &scale[0], sizeof(v3dxQuaternion));
		mRotateVB->UpdateGPUBuffData(cmd, &quat[0], sizeof(v3dxQuaternion));
		mColorVB->UpdateGPUBuffData(cmd, &clr[0], sizeof(v3dxColor4));

	}
	
}

void GfxParticleSystem::Face2(GfxParticlePose* pose, BILLBOARD_TYPE type, const GfxCamera* eye, CoordinateSpace coord, const v3dxMatrix4* worldMatrix, vBOOL bind, vBOOL isBillboard, const v3dxVector3* prepos)
{
	switch (type)
	{
		case BILLBOARD_FREE:
		case BILLBOARD_LOCKVELOCITY:
		{
			if (eye != NULL)
			{
				v3dxVector3 posePos = v3dxVector3::ZERO;			// ��������λ��
				v3dxVector3 tempScale = v3dxVector3::UNIT_SCALE;
				worldMatrix->ExtractionScale(tempScale);
				tempScale = tempScale * pose->mScale;

				switch (coord)
				{
					case CSPACE_WORLD:
					case CSPACE_WORLDWITHDIRECTION:
					{
						posePos = pose->mPosition;
					}
					break;
					case CSPACE_LOCAL:
					{
						v3dxVector3 tempTrans = v3dxVector3::ZERO;
						worldMatrix->ExtractionTrans(tempTrans);
						posePos = pose->mPosition * tempScale + tempTrans;
					}
					break;
					case CSPACE_LOCALWITHDIRECTION:
					{
						v3dxVec3TransformCoord(&posePos, &pose->mPosition, worldMatrix);
					}
					break;
				}
				pose->mScale = tempScale;
				pose->mPosition = posePos;

				//v3dxVector3 eyeDir = posePos - pos;
				
				if (bind)
				{
					posePos = posePos * (*worldMatrix);
				}

				v3dxMatrix4 camMat, camInvMat;
				auto pos = eye->GetPosition();
				v3dxVector3 up;
				if (type == BILLBOARD_LOCKVELOCITY)
				{
					up = pose->mPosition - *prepos;
				}
				else
				{
					up = eye->GetUp();
				}
				
				//v3dxMatrix4 worldrotmat = *worldMatrix;
				//worldrotmat.m41 =
				//	worldrotmat.m42 =
				//	worldrotmat.m43 = 0.0f;
			
				if (isBillboard)
				{
					auto lookat = eye->GetLookAt();
					v3dxMatrixLookAtLH(&camMat, &lookat, &pos, &up);
				}
				else
				{
					auto lookat = eye->GetLookAt();
					v3dxMatrixLookAtLH(&camMat, &pos, &lookat, &up);
				}
				
				camMat = (*worldMatrix) * camMat;
				v3dxMatrix4Inverse(&camInvMat, &camMat, NULL);
				camInvMat.m41 =
					camInvMat.m42 =
					camInvMat.m43 = 0.0f;

				v3dxQuaternion quatMat;
				quatMat.fromRotationMatrix(camInvMat);

				if (isBillboard)
				{
					v3dxQuaternion quatEx = v3dxQuaternion(eye->GetDirection(), pose->mAngle); //v3dxVector3::UNIT_Z
					pose->mRotation = quatMat * quatEx;// *worldOppQuat;
				}
				else
				{
					//v3dxVector3 eyeDir = pos - posePos;
					//eyeDir.normalize();
					v3dxQuaternion quatEx = v3dxQuaternion(eye->GetDirection(), pose->mAngle);
					pose->mRotation = quatEx * quatMat;// *worldOppQuat;
				}
			
			}
		}
		break;

		case  BILLBOARD_LOCKY_EYE:
		case  BILLBOARD_LOCKY_PARALLEL:
		{
			if (eye != NULL)
			{
				v3dxVector3 posePos = v3dxVector3::ZERO;
				v3dxVector3 tempScale = v3dxVector3::UNIT_SCALE;
				worldMatrix->ExtractionScale(tempScale);
				tempScale = tempScale * pose->mScale;

				switch (coord)
				{
					case CSPACE_WORLD:
					case CSPACE_WORLDWITHDIRECTION:
					{
						posePos = pose->mPosition;
					}
					break;
					case CSPACE_LOCAL:
					{
						v3dxVector3 tempTrans = v3dxVector3::ZERO;
						worldMatrix->ExtractionTrans(tempTrans);
						posePos = pose->mPosition * tempScale + tempTrans;
					}
					break;
					case CSPACE_LOCALWITHDIRECTION:
					{
						v3dxVec3TransformCoord(&posePos, &pose->mPosition, worldMatrix);
					}
					break;
				}
				pose->mScale = tempScale;
				pose->mPosition = posePos;

				//v3dxVector3 particleDir;
				v3dxQuaternion rotation;
				worldMatrix->ExtractionRotation(rotation);
				//rotation = rotation * pose->mRotation;
				//v3dxVec3TransformCoord(&particleDir, &v3dxVector3::UNIT_Y, &rotation);
				v3dxVector3 zDir, xDir;
				v3dxVec3TransformCoord(&zDir, &v3dxVector3::UNIT_Z, &rotation);
				v3dxVec3TransformCoord(&xDir, &v3dxVector3::UNIT_X, &rotation);
				v3dxVector3 eyeDir;
				if (type == BILLBOARD_LOCKY_EYE)
				{
					eyeDir = eye->GetPosition() - posePos;
				}
				else
				{
					eyeDir = eye->GetPosition() - eye->GetLookAt();
				}
				
				v3dxVector3 eyeDirNormal;
				v3dxVec3Normalize(&eyeDirNormal, &eyeDir);
				auto xLen = v3dxVec3Dot(&eyeDirNormal, &xDir);
				auto zLen = v3dxVec3Dot(&eyeDirNormal, &zDir);
				auto tempVec = xLen * xDir + zLen * zDir;
				tempVec.normalize();

				if (Math::Abs(tempVec.x - zDir.x) < EPSILON && Math::Abs(tempVec.y - zDir.y) < EPSILON)
				{
					if (tempVec.z * zDir.z > 0)
					{
						pose->mRotation = v3dxQuaternion::IDENTITY;
					}
					else
					{
						pose->mRotation = v3dxQuaternion(v3dxVector3::UNIT_Y, 3.14f);
					}
				}
				else
				{
					v3dxVector3 rotDir;
					v3dxVec3Cross(&rotDir, &zDir, &tempVec);
					auto angle = acosf(v3dxVec3Dot(&zDir, &tempVec));
					v3dxQuaternion extRot;
					
					v3dxQuaternionRotationAxis(&extRot, &rotDir, angle);
					pose->mRotation = extRot;
				}

				if (Math::Abs(pose->mAngle) > EPSILON)
				{
					v3dxQuaternion extRot = v3dxQuaternion(v3dxVector3::UNIT_Z, pose->mAngle);
					pose->mRotation = extRot * pose->mRotation;
				}
			}
		}
		break;
		
		default:
		{
			v3dxVector3 posePos = v3dxVector3::ZERO;
			v3dxVector3 tempScale = v3dxVector3::UNIT_SCALE;

			switch (coord)
			{
				case CSPACE_WORLD:
				case CSPACE_WORLDWITHDIRECTION:
				{
					worldMatrix->ExtractionScale(tempScale);
					tempScale = tempScale * pose->mScale;
					posePos = pose->mPosition;
					pose->mRotation = pose->mRotation;//v3dxQuaternion(dir, pose.mRotation.w);
				}
				break;
				case CSPACE_LOCAL:
				{
					worldMatrix->ExtractionScale(tempScale);
					tempScale = tempScale * pose->mScale;
					v3dxVector3 tempTrans = v3dxVector3::ZERO;
					worldMatrix->ExtractionTrans(tempTrans);
					posePos = pose->mPosition * tempScale + tempTrans;
					pose->mRotation = pose->mRotation;//v3dxQuaternion(dir, pose.mRotation.w);
				}
				break;
				case CSPACE_LOCALWITHDIRECTION:
				{
					v3dxVec3TransformCoord(&posePos, &pose->mPosition, worldMatrix);
					v3dxVector3 tempTrans;
					v3dxQuaternion tempRot;
					worldMatrix->Decompose(tempScale, tempTrans, tempRot);
					tempScale = tempScale * pose->mScale;
					v3dxQuaternionMultiply(&pose->mRotation, &(pose->mRotation), &tempRot);
					//quat[mRenderNumber] = pose.mRotation;//v3dxQuaternion(dir, pose.mRotation.w);
				}
				break;
			}
			//pose.mScale
			pose->mScale = tempScale;
			pose->mPosition = posePos;

		}
		break;
	}
}
NS_END

using namespace EngineNS;

typedef void(WINAPI *FOnCallDelegate)(int num, int a, float b);

extern "C"
{
	CSharpReturnAPI3(vBOOL, EngineNS, GfxParticleSystem, InitParticlePool, IRenderContext*, int, int);
	CSharpReturnAPI1(GfxParticleSubState*, EngineNS, GfxParticleSystem, GetSubState, int);
	CSharpAPI1(EngineNS, GfxParticleSystem, Simulate, float);
	CSharpReturnAPI1(int, EngineNS, GfxParticleSystem, FireParticles, int);

	CSharpAPI3(EngineNS, GfxParticleSystem, GetParticlePool, GfxParticle**, int*, int*);
	CSharpAPI2(EngineNS, GfxParticleSystem, GetParticles, GfxParticle***, int*);
	CSharpAPI2(EngineNS, GfxParticleSystem, GetDeathParticles, GfxParticle***, int*);
	
	CSharpReturnAPI0(IVertexBuffer*, EngineNS, GfxParticleSystem, GetPosVB);
	CSharpReturnAPI0(IVertexBuffer*, EngineNS, GfxParticleSystem, GetScaleVB);
	CSharpReturnAPI0(IVertexBuffer*, EngineNS, GfxParticleSystem, GetRotateVB);
	CSharpReturnAPI0(IVertexBuffer*, EngineNS, GfxParticleSystem, GetColorVB);
	CSharpAPI2(EngineNS, GfxParticleSystem, Flush2VB, ICommandList*, vBOOL);
	CSharpReturnAPI0(int, EngineNS, GfxParticleSystem, GetParticleNum);

	CSharpReturnAPI0(float, EngineNS, GfxParticleSystem, GetLiveTime);
	CSharpAPI1(EngineNS, GfxParticleSystem, SetLiveTime, float);
	CSharpReturnAPI0(float, EngineNS, GfxParticleSystem, GetCurLiveTime);
	CSharpAPI1(EngineNS, GfxParticleSystem, SetCurLiveTime, float);
	CSharpReturnAPI0(float, EngineNS, GfxParticleSystem, GetPrevFireTime);
	CSharpAPI1(EngineNS, GfxParticleSystem, SetPrevFireTime, float);
	CSharpReturnAPI0(float, EngineNS, GfxParticleSystem, GetFireInterval);
	CSharpAPI1(EngineNS, GfxParticleSystem, SetFireInterval, float);
	CSharpReturnAPI0(int, EngineNS, GfxParticleSystem, GetFireCountPerTime);
	CSharpAPI1(EngineNS, GfxParticleSystem, SetFireCountPerTime, int);

	CSharpReturnAPI0(bool, EngineNS, GfxParticleSystem, GetIsTrail);
	CSharpAPI1(EngineNS, GfxParticleSystem, SetIsTrail, bool);

	CSharpAPI0(EngineNS, GfxParticleSystem, ClearParticles);

	CSharpAPI8(EngineNS, GfxParticleSystem, Face2, GfxParticlePose*, GfxParticleSystem::BILLBOARD_TYPE, const GfxCamera*, GfxParticleSystem::CoordinateSpace, const v3dxMatrix4*, vBOOL, vBOOL, const v3dxVector3*);
	
	VFX_API void SDK_TestCall(int num, int a, float b);
	
	VFX_API void SDK_TestCallDelegate(FOnCallDelegate func, int num, int repeat, int a, float b)
	{
		for (int i = 0; i < num; i++)
		{
			func(repeat, a, b);
		}
	}
	VFX_API void SDK_TestPureCpp(int num, int repeat, int a, float b)
	{
		for (int i = 0; i < repeat; i++)
		{
			SDK_TestCall(num, a, b);
		}
	}
};