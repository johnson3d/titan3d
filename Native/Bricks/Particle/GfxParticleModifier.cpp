#include "GfxParticleModifier.h"

#define  new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxParticleModifier, EngineNS::GfxModifier);

GfxParticleModifier::~GfxParticleModifier(void)
{
	Cleanup();
}

void GfxParticleModifier::Cleanup()
{
	
}

GfxModifier* GfxParticleModifier::CloneModifier(IRenderContext* rc)
{
	auto result = new GfxParticleModifier();
	result->mName = mName;
	return result;
}

NS_END
