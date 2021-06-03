#pragma once
#include "IShader.h"

NS_BEGIN

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IComputeShader : public IShader
{
public:
	IComputeShader();
	~IComputeShader();
};

NS_END