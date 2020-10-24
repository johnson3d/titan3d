#include "GfxModifier.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxModifier, EngineNS::VIUnknown);

GfxModifier::GfxModifier()
{
}

GfxModifier::~GfxModifier()
{
}

void GfxModifier::Cleanup()
{

}

Hash64 GfxModifier::GetHash64()
{
	auto hashStr = VStringA_FormatV("Modifier:%s", mName.c_str());
	return HashHelper::CalcHash64(hashStr.c_str(), (int)hashStr.length());
}

void GfxModifier::SetName(const char* name) 
{
	mName = name;
}

bool GfxModifier::Init(GfxModifierDesc* desc)
{
	return true;
}

void GfxModifier::Save2Xnd(XNDNode* node)
{

}

vBOOL GfxModifier::LoadXnd(XNDNode* node)
{
	return TRUE;
}

GfxModifier* GfxModifier::CloneModifier(IRenderContext* rc)
{
	return nullptr;
}

NS_END

using namespace EngineNS;
extern "C"
{
	Cpp2CS0(EngineNS, GfxModifier, GetName);
	Cpp2CS1(EngineNS, GfxModifier, SetName);
	Cpp2CS0(EngineNS, GfxModifier, GetShaderModuleName);
	Cpp2CS1(EngineNS, GfxModifier, SetShaderModuleName);
	Cpp2CS3(EngineNS, GfxModifier, TickLogic);
	Cpp2CS3(EngineNS, GfxModifier, TickSync);
	Cpp2CS1(EngineNS, GfxModifier, Save2Xnd);
	Cpp2CS1(EngineNS, GfxModifier, LoadXnd);
	Cpp2CS1(EngineNS, GfxModifier, CloneModifier);
}