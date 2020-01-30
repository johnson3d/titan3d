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
	CSharpReturnAPI0(const char*, EngineNS, GfxModifier, GetName);
	CSharpAPI1(EngineNS, GfxModifier, SetName, const char*);
	CSharpReturnAPI0(const char*, EngineNS, GfxModifier, GetShaderModuleName);
	CSharpAPI1(EngineNS, GfxModifier, SetShaderModuleName, const char*);
	CSharpAPI3(EngineNS, GfxModifier, TickLogic, IRenderContext*, GfxMesh*, vTimeTick);
	CSharpAPI3(EngineNS, GfxModifier, TickSync, IRenderContext*, GfxMesh*, vTimeTick);
	CSharpAPI1(EngineNS, GfxModifier, Save2Xnd, XNDNode*);
	CSharpReturnAPI1(vBOOL, EngineNS, GfxModifier, LoadXnd, XNDNode*);
	CSharpReturnAPI1(GfxModifier*, EngineNS, GfxModifier, CloneModifier, IRenderContext*);
}