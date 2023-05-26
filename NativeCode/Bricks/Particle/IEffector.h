#pragma once
#include "IParticleState.h"

NS_BEGIN

class TR_CLASS(SV_CSImplement=true)
	IEffector : public IWeakReference
{
public:
	TR_FUNCTION(SV_CSImplement = true)
	virtual void DoEffect(IEmitter* emitter, IBaseParticleState* particle) = 0;
};

NS_END



