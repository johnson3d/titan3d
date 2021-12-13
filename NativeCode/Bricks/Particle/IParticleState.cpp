#include "IParticleState.h"
#include "IEmitter.h"

#define new VNEW

NS_BEGIN

bool IParticlePool::InitPool(UINT dataStride, UINT maxNum)
{
	mDataStride = dataStride;
	mParticleArray.resize((size_t)maxNum * mDataStride);
	mFreeParticles.resize(maxNum);
	for (size_t i = 0; i < mFreeParticles.size(); i++)
	{
		mFreeParticles[i] = (UINT)i;
	}
	mLivedParticles[0].clear();
	mLivedParticles[1].clear();
	mCurAlives = &mLivedParticles[0];
	mBackendAlives = &mLivedParticles[1];
	mChanged = false;

	return true;
}
UINT IParticlePool::Alloc(IEmitter* pEmitter, UINT num, UINT flags, float life)
{
	if (mFreeParticles.size() < num)
	{
		num = (UINT)mFreeParticles.size();
		if (num == 0)
			return 0;
	}
	ASSERT(life > 0);
	for (UINT i = 0; i < num; i++)
	{
		UINT addr = mFreeParticles[i];
		auto pParticle = (IBaseParticleState*)GetParticle(addr);
		pParticle->Flags = flags;
		ASSERT(pParticle->Life <= 0);
		pParticle->Life = life;
	}
	mChanged = true;
	mBackendAlives->insert(mBackendAlives->begin(), mFreeParticles.begin(), mFreeParticles.begin() + num);
	mFreeParticles.erase(mFreeParticles.begin(), mFreeParticles.begin() + num);

	return num;
}
void IParticlePool::Recycle(IEmitter* pEmitter)
{
	std::vector<UINT>& prev = *mCurAlives;
	std::vector<UINT>& cur = *mBackendAlives;
	for (size_t i = 0; i < prev.size(); i++)
	{
		UINT addr = prev[i];
		if (GetParticle(addr)->IsDeath())
		{
			mFreeParticles.push_back(addr);
		}
		else
		{
			cur.push_back(addr);
		}
	}
	prev.clear();
	mCurAlives = &cur;
	mBackendAlives = &prev;
	mChanged = false;
}

NS_END