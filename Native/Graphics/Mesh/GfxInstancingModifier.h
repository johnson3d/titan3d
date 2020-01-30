#pragma once
#include "GfxModifier.h"
#include "../GfxPreHead.h"

NS_BEGIN

class GfxInstancingModifier : public GfxModifier
{
public:
	RTTI_DEF(GfxInstancingModifier, 0x9ea394665c427b23, true);
	GfxInstancingModifier();
	~GfxInstancingModifier();

	virtual bool Init(GfxModifierDesc* desc) override;

	virtual void Save2Xnd(XNDNode* node) override;
	virtual vBOOL LoadXnd(XNDNode* node) override;

	virtual GfxModifier* CloneModifier(IRenderContext* rc) override;
};


NS_END