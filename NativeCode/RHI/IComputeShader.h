#pragma once
#include "IShader.h"

NS_BEGIN

class TR_CLASS()
IComputeShader : public IShader
{
public:
	IComputeShader();
	~IComputeShader();
};

NS_END