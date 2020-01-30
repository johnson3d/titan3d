#pragma once
#include "GfxParticle.h"
#include "../../Graphics/Mesh/GfxModifier.h"
#include "GfxParticleEmitterShape.h"
#include "GfxParticleSubState.h"

NS_BEGIN
class GfxParticleModifier : public GfxModifier
{
public:
	RTTI_DEF(GfxParticleModifier, 0x30191957532fd2a1, true);

	GfxParticleModifier()
	{
	}
	virtual ~GfxParticleModifier(void);
	virtual void Cleanup() override;
	virtual GfxModifier* CloneModifier(IRenderContext* rc) override;
public:
	
};

NS_END