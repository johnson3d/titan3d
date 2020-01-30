#pragma once
#include "../ISamplerState.h"

NS_BEGIN

class INullSamplerState : public ISamplerState
{
public:
	INullSamplerState();
	~INullSamplerState();
};

NS_END