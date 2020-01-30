#pragma once
#include "../IRasterizerState.h"

NS_BEGIN

class INullRasterizerState : public IRasterizerState
{
public:
	INullRasterizerState();
	~INullRasterizerState();
};

NS_END