#pragma once
#include "IShader.h"

NS_BEGIN

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IPixelShader : public IShader
{
public:
	IPixelShader();
	~IPixelShader();
};

NS_END