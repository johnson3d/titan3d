#include "IEmitter.h"
#include "IEffector.h"

#define new VNEW

NS_BEGIN

IEmitter::~IEmitter()
{
	for (auto i : mEffectors)
	{
		i->Release();
	}
	mEffectors.clear();
}

void IEmitter::InitEmitter(UINT dataStide, UINT maxParticle)
{
	mPool.InitPool(dataStide, maxParticle);
}

void IEmitter::PushEffector(IEffector* effector)
{
	effector->AddRef();
	mEffectors.push_back(effector);
}

UINT IEmitter::Spawn(UINT num, UINT flags, float life)
{
	return mPool.Alloc(this, num, flags, life);
}

void IEmitter::UpdateLife(float elapsed)
{
	auto& alives = *mPool.GetCurAlives();
	for (auto i : alives)
	{
		auto particle = mPool.GetParticle(i);
		particle->Life -= elapsed;
		if (particle->IsDeath())
		{
			mPool.mChanged = true;
		}
	}
}

void IEmitter::Update(float elapsed)
{
	UpdateLife(elapsed);

	auto& alives = *mPool.GetCurAlives();
	for (auto i : mEffectors)
	{
		for (auto j : alives)
		{
			auto pParticle = mPool.GetParticle(j);
			if (pParticle->IsDeath() == false)
			{
				i->DoEffect(this, pParticle);
			}
		}
	}

	if (IsChanged())
	{
		for (auto i : *mPool.mBackendAlives)
		{
			UINT addr = i;
			auto pParticle = mPool.GetParticle(addr);
			OnInitParticle(pParticle);
		}
		Recycle();
	}
}

NS_END