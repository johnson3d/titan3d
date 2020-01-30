#include "GfxInstancingModifier.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxInstancingModifier, EngineNS::GfxModifier);

GfxInstancingModifier::GfxInstancingModifier()
{
	
}

GfxInstancingModifier::~GfxInstancingModifier()
{

}

bool GfxInstancingModifier::Init(GfxModifierDesc* desc)
{
	return true;
}

void GfxInstancingModifier::Save2Xnd(XNDNode* node)
{
	
}

vBOOL GfxInstancingModifier::LoadXnd(XNDNode* node)
{
	
	return TRUE;
}

GfxModifier* GfxInstancingModifier::CloneModifier(IRenderContext* rc)
{
	auto result = new GfxInstancingModifier();
	result->mName = mName;
	return result;
}

NS_END

using namespace EngineNS;

extern "C"
{
	
}