#pragma once
#include "IShader.h"

NS_BEGIN

class TR_CLASS()
IVertexShader : public IShader
{
public:
	IVertexShader();
	~IVertexShader();
};

NS_END