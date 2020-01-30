#pragma once
#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "../AnimCurve/GfxICurve.h"
#include "GfxAnimationElement.h"
NS_BEGIN

class GfxSkeletonAnimationElement :public GfxAnimationElement
{
	RTTI_DEF(GfxSkeletonAnimationElement, 0x933803dc5d1eedd9, true);
public:
	GfxSkeletonAnimationElement();
	~GfxSkeletonAnimationElement();  
public:
	void AddElement(GfxAnimationElement* element);
	UINT GetElementCount() { return (UINT)mElements.size(); }
	GfxAnimationElement* GetElement(UINT index);
protected:
	std::map<UINT,GfxAnimationElement*> mElements;
};

NS_END