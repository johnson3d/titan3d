#pragma once
#include "../IRenderTargetView.h"

NS_BEGIN

class INullRenderTargetView : public IRenderTargetView
{
public:
	INullRenderTargetView();
	~INullRenderTargetView();
};

NS_END