#pragma once
#include "../IDepthStencilView.h"

NS_BEGIN

class INullDepthStencilView : public IDepthStencilView
{
public:
	INullDepthStencilView();
	~INullDepthStencilView();
};

NS_END