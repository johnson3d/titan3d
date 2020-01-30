#pragma once
#include "IShader.h"

NS_BEGIN

class IComputeShader : public IShader
{
public:
	IComputeShader();
	~IComputeShader();
};

NS_END