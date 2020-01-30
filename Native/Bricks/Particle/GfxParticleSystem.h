#pragma once
#include "GfxParticle.h"
#include "../../Graphics/Mesh/GfxModifier.h"
#include "GfxParticleEmitterShape.h"
#include "GfxParticleSubState.h"

NS_BEGIN

class GfxCamera;

class GfxParticleSystem : public VIUnknown
{
public:
	RTTI_DEF(GfxParticleSystem, 0xa501427c5b987942, true);

	GfxParticleSystem()
	{
		mLiveTime = FLT_MAX;
		mCurLiveTime = 0;
		mPrevFireTime = 0;
		mFireInterval = 1.0f;
		mFireCountPerTime = 1;
		mIsTrail = false;
	}
	virtual ~GfxParticleSystem(void);
	virtual void Cleanup() override;
public:
	enum BILLBOARD_TYPE
	{
		BILLBOARD_DISABLE = 0,
		BILLBOARD_FREE,
		BILLBOARD_LOCKY_EYE,
		BILLBOARD_LOCKY_PARALLEL,//ƽ�������
		BILLBOARD_LOCKVELOCITY,
	};
	enum CoordinateSpace
	{
		CSPACE_WORLD,
		CSPACE_LOCAL,
		CSPACE_LOCALWITHDIRECTION,
		CSPACE_WORLDWITHDIRECTION,
	};
	
	void SetDirectionCamera(GfxCamera* eye);

	vBOOL InitParticlePool(IRenderContext* rc, int maxNum = 32, int state = 1);
	GfxParticleSubState* GetSubState(int index){
		return mSubStates[index];
	}

	void Simulate(float elaspedTime);
	void ClearParticles();
	int FireParticles(int num);
	void GetParticlePool(GfxParticle** pParticles, int* num, int* stride)
	{
		*stride = sizeof(GfxParticle);
		*num = (int)mPools.GeParticles().size();
		if (*num == 0)
		{
			*pParticles = nullptr;
		}
		else
		{
			*pParticles = &mPools.GeParticles()[0];
		}
	}
	void GetParticles(GfxParticle*** ppParticles, int* num)
	{
		*num = (int)mParticles.size();
		if (*num == 0)
		{
			*ppParticles = nullptr;
		}
		else
		{
			*ppParticles = &mParticles[0];
		}
	}
	void GetDeathParticles(GfxParticle*** ppParticles, int* num)
	{
		*num = (int)mDeathParticles.size();
		if (*num == 0)
		{
			*ppParticles = nullptr;
		}
		else
		{
			*ppParticles = &mDeathParticles[0];
		}
	}

	inline int GetParticleNum() const {
		return (int)mParticles.size();
	}
	void Flush2VB(ICommandList* cmd, vBOOL bImm);

	IVertexBuffer* GetPosVB() {
		return mPosVB;
	}
	IVertexBuffer* GetScaleVB() {
		return mScaleVB;
	}
	IVertexBuffer* GetRotateVB() {
		return mRotateVB;
	}
	IVertexBuffer* GetColorVB() {
		return mColorVB;
	}

	void Face2(GfxParticlePose* pose, BILLBOARD_TYPE type, const GfxCamera* camera, CoordinateSpace coord, const v3dxMatrix4* worldMatrix, vBOOL bind, vBOOL isBillboard, const v3dxVector3* prepos);
protected:
	AutoRef<v3dScalarVariable>				mParticleLiveTime;
	//AutoRef<GfxCamera>						mDirectionCamera;
	GfxParticlePool							mPools;
	std::vector<GfxParticleSubState*>		mSubStates;
	std::vector<GfxParticle*>				mParticles;
	std::vector<GfxParticle*>				mDeathParticles;
	
	AutoRef<IVertexBuffer>		mPosVB;
	AutoRef<IVertexBuffer>		mScaleVB;
	AutoRef<IVertexBuffer>		mRotateVB;
	AutoRef<IVertexBuffer>		mColorVB;
public:
	GfxParticle* AllocParticle();
	void FreeParticle(GfxParticle* p);

	VDef_ReadWrite(float, LiveTime, m);
	VDef_ReadWrite(float, PrevFireTime, m);
	VDef_ReadWrite(float, FireInterval, m);
	VDef_ReadWrite(float, CurLiveTime, m);
	VDef_ReadWrite(int, FireCountPerTime, m);
	VDef_ReadWrite(bool, IsTrail, m);

protected:
	float				mLiveTime;
	float				mPrevFireTime;
	float				mFireInterval;
	float				mCurLiveTime;
	int					mFireCountPerTime;

	bool				mIsTrail;
};

NS_END