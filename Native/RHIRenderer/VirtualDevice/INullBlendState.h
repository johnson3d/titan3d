#pragma once

#include "../IBlendState.h"

NS_BEGIN

class INullRenderContext;

class INullBlendState : public IBlendState
{
public:
	INullBlendState();
	~INullBlendState();

	bool Init(const IBlendStateDesc* desc);
};

NS_END