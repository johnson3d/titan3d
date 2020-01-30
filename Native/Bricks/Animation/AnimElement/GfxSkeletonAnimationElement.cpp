#include "GfxSkeletonAnimationElement.h"

#define new VNEW
NS_BEGIN

RTTI_IMPL(EngineNS::GfxSkeletonAnimationElement, EngineNS::GfxAnimationElement);
EngineNS::GfxSkeletonAnimationElement::GfxSkeletonAnimationElement() :GfxAnimationElement(AET_Skeleton)
{

}

GfxSkeletonAnimationElement::~GfxSkeletonAnimationElement()
{
}

void EngineNS::GfxSkeletonAnimationElement::AddElement(GfxAnimationElement* element)
{
	{ mElements.insert(std::make_pair(element->GetAnimationElementDesc()->NameHash, element)); };
}

EngineNS::GfxAnimationElement* EngineNS::GfxSkeletonAnimationElement::GetElement(UINT index)
{
	auto it = mElements.begin();
	int i = 0;
	for (i = 0; it != mElements.end(); it++, i++)
	{
		if (i == index)
			return (*it).second;

	}
	return  NULL;
}
NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI0(UINT, EngineNS, GfxSkeletonAnimationElement, GetElementCount);
	CSharpReturnAPI1(GfxAnimationElement*, EngineNS, GfxSkeletonAnimationElement, GetElement, UINT);
}