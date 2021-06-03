#pragma once
#include "../IDepthStencilState.h"

NS_BEGIN

class INullDepthStencilState : public IDepthStencilState
{
public:
	INullDepthStencilState();
	~INullDepthStencilState();

	bool Init(const IDepthStencilStateDesc* desc);
};

NS_END