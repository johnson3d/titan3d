#pragma once
#ifndef __v3dParticle_h__
#define __v3dParticle_h__
#include "../../Graphics/GfxPreHead.h"
#include "../../Math/IntegerType.h"

#pragma pack(push,4)

NS_BEGIN

enum CoordinateSpace
{
	CSPACE_WORLD,
	CSPACE_LOCAL,
	CSPACE_LOCALWITHDIRECTION,
	CSPACE_WORLDWITHDIRECTION,
};

// 粒子姿态信息
struct GfxParticlePose
{
	GfxParticlePose();
	GfxParticlePose(const v3dxVector3& pos, const v3dxVector3& velocity, const v3dxVector3& accel,
		const v3dUInt32_4& clr, const v3dxVector3& scale, const v3dxQuaternion& rotation);
	void reset(bool isTrans = true);
	friend GfxParticlePose operator +(const GfxParticlePose& base, const GfxParticlePose& trans) {
		v3dxVector3 pos = base.mPosition + trans.mPosition + trans.mVelocity;
		v3dxVector3 vel = base.mVelocity + trans.mVelocity;
		v3dxVector3 acc = base.mAcceleration + trans.mAcceleration;
		v3dUInt32_4 clr;
		clr.x = base.mColor.x + trans.mColor.x;
		clr.y = base.mColor.y + trans.mColor.y;
		clr.z = base.mColor.z + trans.mColor.z;
		clr.w = base.mColor.w + trans.mColor.w;
		v3dxVector3 scale = base.mScale * trans.mScale;
		v3dxQuaternion rot = base.mRotation * trans.mRotation;
		return GfxParticlePose(pos, vel, acc, clr, scale, rot);
	}
	float getSpeed() {
		return mVelocity.getLength();
	}
	v3dxVector3 getDirection() {
		return mVelocity.getNormal();
	}

	//直接使用的属性
	v3dxVector3 mPosition;
	v3dxVector3 mScale;
	v3dxQuaternion mRotation;
	v3dUInt32_4 mColor;

	//用来计算的中间属性
	v3dxVector3 mVelocity;
	v3dxVector3 mAcceleration;
	v3dxVector3 mAxis;
	float		mAngle;
};

struct GfxParticle;
// 粒子生命体
struct GfxParticleState
{
	GfxParticleState();

	void Reset();
	inline void Update(float elapsedTime)
	{
		mPose.mPosition += mPose.mVelocity * elapsedTime;
		mPose.mVelocity += mPose.mAcceleration * elapsedTime;
	}
	float GetLifeProgress();
	GfxParticle*	Host;
	void*			mExtData;// 附加状态
	GfxParticlePose mStartPose;	// 起始姿态
	GfxParticlePose mPose;	// 当前姿态
	GfxParticlePose mPrevPose;// 叠加变换姿态
};

struct GfxParticle
{
	GfxParticle()
	{
		mLife = 0;
		mLifeTick = 0;
		mExtData = 0;
		mExtData2 = 0;
		mFlags = 0;
	}
	int				mIndex;
	float			mLife;			// 生存期
	float			mLifeTick;		// 当前年龄
	DWORD			mFlags;//附加1
	void*			mExtData;//附加2
	void*			mExtData2;//附加3
	
	GfxParticlePose		mFinalPose;
	GfxParticleState**	mStateArray;
	GfxParticle*	mNextParticle;
	std::vector<GfxParticleState*> mStates;

	vBOOL IsAlive() const{
		return mLifeTick < mLife;
	}
	bool Update(float elapsedTime)
	{
		if (IsAlive()==FALSE)
		{
			return false;
		}
		mLifeTick += elapsedTime;
		return true;
	}
	float GetLifeProgress()
	{
		if (mLifeTick > mLife)
			return 1;
		else if (mLifeTick < 0)
			return 0;
		return mLifeTick / mLife;
	}
	void Reset()
	{
		mLife = mLifeTick = 0;
		mLifeTick = 0;
	}
};

class GfxParticlePool
{
public:
	GfxParticlePool();
	vBOOL Init(int maxNum = 32, int state = 1);
	GfxParticle* AllocParticle();
	void FreeParticle(GfxParticle* p);
	inline int GetAliveNum() const{
		return AliveNum;
	}
	inline int GetRemainNum() const {
		return RemainNum;
	}
	std::vector<GfxParticle>& GeParticles() {
		return mParticles;
	}
protected:
	int									RemainNum;
	int									AliveNum;
	GfxParticle*						mFreePoint;
	std::vector<GfxParticle>			mParticles;
	std::vector<GfxParticleState>		mStates;
};

NS_END

#pragma pack(pop)

#endif