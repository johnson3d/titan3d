#include "GfxAnimationElement.h"

#define new VNEW
NS_BEGIN
RTTI_IMPL(EngineNS::GfxAnimationElementDesc, EngineNS::VIUnknown);
RTTI_IMPL(EngineNS::GfxAnimationElement, EngineNS::VIUnknown);
EngineNS::GfxAnimationElement::GfxAnimationElement(AnimationElementType type)
{
	mAnimationElementType = type;
}

GfxAnimationElement::~GfxAnimationElement()
{
}
NS_END

using namespace EngineNS;

extern "C"
{
	/////GfxAnimationElementDesc
	CSharpAPI1(EngineNS, GfxAnimationElementDesc, SetName, char*);
	CSharpReturnAPI0(const char*, EngineNS, GfxAnimationElementDesc, GetName);
	CSharpAPI1(EngineNS, GfxAnimationElementDesc, SetParent, char*);
	CSharpReturnAPI0(const char*, EngineNS, GfxAnimationElementDesc, GetParent);
	CSharpAPI1(EngineNS, GfxAnimationElementDesc, SetGrantParent, char*);
	CSharpReturnAPI0(const char*, EngineNS, GfxAnimationElementDesc, GetGrantParent);
	CSharpAPI1(EngineNS, GfxAnimationElementDesc, SetPath, char*);
	CSharpReturnAPI0(const char*, EngineNS, GfxAnimationElementDesc, GetPath);
	CSharpReturnAPI0(UINT, EngineNS, GfxAnimationElementDesc, GetNameHash);
	CSharpReturnAPI0(UINT, EngineNS, GfxAnimationElementDesc, GetParentHash);
	CSharpReturnAPI0(UINT, EngineNS, GfxAnimationElementDesc, GetGrantParentHash);
	////////////////////////////////////////////////////////
	//GfxAnimationElement
	CSharpAPI1(EngineNS, GfxAnimationElement, SetCurve, GfxICurve*);
	CSharpAPI1(EngineNS, GfxAnimationElement, SetAnimationElementType, AnimationElementType);
	CSharpAPI1(EngineNS, GfxAnimationElement, SetAnimationElementDesc, GfxAnimationElementDesc*);
	CSharpReturnAPI0(GfxICurve*, EngineNS, GfxAnimationElement, GetCurve);
	CSharpReturnAPI0(AnimationElementType, EngineNS, GfxAnimationElement, GetAnimationElementType);
	CSharpReturnAPI0(GfxAnimationElementDesc*, EngineNS, GfxAnimationElement, GetAnimationElementDesc);
}
