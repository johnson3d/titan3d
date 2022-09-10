#include "IParticleState.h"
#include "IEmitter.h"
#include <tuple>

#define new VNEW

NS_BEGIN


void IParticleSystemAttribute::BuildAttributes(UINT AlignSize)
{	
	UINT packOffset = 0;
	UINT offsetTmp = 0;
	for (size_t i = 0; i < NamedAttributes.size(); i++)
	{
		UINT size = 0;
		size = 0;// (UINT)NxRHI::FShaderVarDesc::GetShaderVarTypeSize(NamedAttributes[i].Type)* NamedAttributes[i].Columns;

		if (size >= 16)
		{
			NamedAttributes[i].Offset = packOffset;
			if (size % AlignSize == 0)
			{
				packOffset += size;
			}
			else
			{
				packOffset += (size / AlignSize + 1) * AlignSize;
			}
			offsetTmp = 0;
		}
		else
		{
			NamedAttributes[i].Offset = packOffset + offsetTmp;
			if (offsetTmp + size == AlignSize)
			{
				packOffset += AlignSize;
				offsetTmp = 0;
			}
			else if (offsetTmp + size < AlignSize)
			{
				offsetTmp += size;
			}
			else
			{
				packOffset += AlignSize;
				NamedAttributes[i].Offset = packOffset;
				offsetTmp = 0;				
			}
		}
	}
}

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