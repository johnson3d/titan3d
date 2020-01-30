#pragma once
#include "GfxParticle.h"
#include "GfxParticleEmitterShape.h"

NS_BEGIN

class GfxParticleEmitterShape;

class GfxParticleSubState : public VIUnknown
{
public:
	GfxParticleSubState()
	{
		//mPosition.x = 0.0f;
		//mPosition.y = 0.0f;
		//mPosition.z = 0.0f;
		mPosition = v3dxVector3::ZERO;
		mDirection = v3dxVector3::ZERO;
	}

	void SetShapeType(GfxParticleEmitterShape* shape);
	GfxParticleEmitterShape* GetShapeType() const{
		return mShape;
	}

	void SetShapeData(GfxParticleState* sub);
	void GetNewBorns(GfxParticleState*** ppState, int* num)
	{
		*num = (int)mNewBorns.size();
		if (*num == 0)
		{
			*ppState = nullptr;
		}
		else
		{
			(*ppState) = &mNewBorns[0];
		}
	}
	void PushNewBorns()
	{
		mParticles.insert(mParticles.begin(), mNewBorns.begin(), mNewBorns.end());
		mNewBorns.clear();
	}
	void GetParticles(GfxParticleState*** ppState, int* num)
	{
		*num = (int)mParticles.size();
		if (*num == 0)
		{
			*ppState = nullptr;
		}
		else
		{
			*ppState = &mParticles[0];
		}
	}
	void Simulate(float elaspedTime);
	int FireParticles(int num);

	VDef_ReadWrite(v3dxVector3, Position, m);
	VDef_ReadWrite(v3dxVector3, Direction, m);
public:
	AutoRef<GfxParticleEmitterShape>	mShape;
	std::vector<GfxParticleState*>		mParticles;
	std::vector<GfxParticleState*>		mNewBorns;

	v3dxVector3			mPosition;
	v3dxVector3			mDirection;
};

NS_END
