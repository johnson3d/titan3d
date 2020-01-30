#pragma once
#include "../IPixelShader.h"

NS_BEGIN

class INullPixelShader : public IPixelShader
{
public:
	INullPixelShader();
	~INullPixelShader();
};

NS_END