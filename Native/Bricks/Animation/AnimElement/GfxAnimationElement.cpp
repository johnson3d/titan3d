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
	Cpp2CS1(EngineNS, GfxAnimationElementDesc, SetName);
	Cpp2CS0(EngineNS, GfxAnimationElementDesc, GetName);
	Cpp2CS1(EngineNS, GfxAnimationElementDesc, SetParent);
	Cpp2CS0(EngineNS, GfxAnimationElementDesc, GetParent);
	Cpp2CS1(EngineNS, GfxAnimationElementDesc, SetGrantParent);
	Cpp2CS0(EngineNS, GfxAnimationElementDesc, GetGrantParent);
	Cpp2CS1(EngineNS, GfxAnimationElementDesc, SetPath);
	Cpp2CS0(EngineNS, GfxAnimationElementDesc, GetPath);
	Cpp2CS0(EngineNS, GfxAnimationElementDesc, GetNameHash);
	Cpp2CS0(EngineNS, GfxAnimationElementDesc, GetParentHash);
	Cpp2CS0(EngineNS, GfxAnimationElementDesc, GetGrantParentHash);
	////////////////////////////////////////////////////////
	//GfxAnimationElement
	Cpp2CS1(EngineNS, GfxAnimationElement, SetCurve);
	Cpp2CS1(EngineNS, GfxAnimationElement, SetAnimationElementType);
	Cpp2CS1(EngineNS, GfxAnimationElement, SetAnimationElementDesc);
	Cpp2CS0(EngineNS, GfxAnimationElement, GetCurve);
	Cpp2CS0(EngineNS, GfxAnimationElement, GetAnimationElementType);
	Cpp2CS0(EngineNS, GfxAnimationElement, GetAnimationElementDesc);
}
