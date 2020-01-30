#include "GfxParticle.h"

#define  new VNEW

NS_BEGIN

GfxParticleState::GfxParticleState()
{
	Host = nullptr;
	mExtData = nullptr;
}

void GfxParticleState::Reset()
{	
	mPose.reset(true);
	mStartPose.reset(true);
	mExtData = nullptr;
}

float GfxParticleState::GetLifeProgress()
{
	return Host->GetLifeProgress();
}

//////////////////////////////////////////////////////////////////////////

GfxParticlePose::GfxParticlePose()
	: mPosition(v3dxVector3::ZERO)
	, mScale(v3dxVector3::UNIT_SCALE)
	, mVelocity(v3dxVector3::ZERO)
	, mAcceleration(v3dxVector3::ZERO)
{
	mColor.x = 1;
	mColor.y = 1;
	mColor.z = 1;
	mColor.w = 1;
	//mRotation.x = 0;
	//mRotation.y = 1;
	//mRotation.z = 0;
	//mRotation.w = 0;
	mAxis.setValue(0, 1, 0);
	mAngle = 0;
	mRotation = v3dxQuaternion::IDENTITY;
}
GfxParticlePose::GfxParticlePose(const v3dxVector3& pos, const v3dxVector3& velocity, const v3dxVector3& accel,
	const v3dUInt32_4& clr, const v3dxVector3& scale, const v3dxQuaternion& rotation)
{
	mPosition = pos;
	mVelocity = velocity;
	mColor = clr;
	mScale = scale;
	mRotation = rotation;
}
void GfxParticlePose::reset(bool isTrans)
{
	mPosition = v3dxVector3::ZERO;
	mVelocity = v3dxVector3::ZERO;
	mAcceleration = v3dxVector3::ZERO;
	mScale = v3dxVector3::UNIT_SCALE;
	mColor = v3dUInt32_4::Zero;
	//mRotation.x = 0;
	//mRotation.y = 1;
	//mRotation.z = 0;
	mRotation = v3dxQuaternion::IDENTITY;
}

GfxParticlePool::GfxParticlePool()
{
	mFreePoint = nullptr;
	RemainNum = AliveNum = 0;
}

vBOOL GfxParticlePool::Init(int maxNum, int state)
{
	RemainNum = maxNum;
	mParticles.resize(maxNum);
	mStates.resize(maxNum*state);
	AliveNum = 0;
	//int stateMax = maxNum * state;

	for (int i = 0; i < maxNum; i++)
	{
		auto p = &mParticles[i];
		p->mIndex = i;
		if (i == maxNum - 1)
		{
			p->mNextParticle = nullptr;
		}
		else
		{
			p->mNextParticle = &mParticles[i + 1];
		}
		p->mStates.resize(state);
		p->mStateArray = &p->mStates[0];
		for (int j = 0; j < state; j++)
		{
			p->mStates[j] = &mStates[i*state + j];
			p->mStates[j]->Host = p;
		}
	}

	mFreePoint = &mParticles[0];
	return TRUE;
}

GfxParticle* GfxParticlePool::AllocParticle()
{
	if (mFreePoint == nullptr)
		return nullptr;
	auto p = mFreePoint;
	mFreePoint = mFreePoint->mNextParticle;
	AliveNum++;
	RemainNum--;
	p->Reset();
	return p;
}

void GfxParticlePool::FreeParticle(GfxParticle* p)
{
	p->mNextParticle = mFreePoint;
	mFreePoint = p;
	AliveNum--;
	RemainNum++;
}

NS_END

extern "C"
{
	VFX_API int Inner_TestCall(int a, float b)
	{
		int r = 0;
		r += a;
		r += (int)b;
		return r;
	}
};