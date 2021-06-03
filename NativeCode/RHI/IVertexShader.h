#pragma once
#include "IShader.h"

NS_BEGIN

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IVertexShader : public IShader
{
public:
	IVertexShader();
	~IVertexShader();
};

NS_END