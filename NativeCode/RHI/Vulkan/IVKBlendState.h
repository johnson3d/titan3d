#pragma once

#include "../IBlendState.h"

NS_BEGIN

class IVKRenderContext;

class IVKBlendState : public IBlendState
{
public:
	IVKBlendState();
	~IVKBlendState();
};

NS_END