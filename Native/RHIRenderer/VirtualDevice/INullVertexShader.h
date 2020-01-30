#pragma once
#include "../IVertexShader.h"

NS_BEGIN

class INullVertexShader : public IVertexShader
{
public:
	INullVertexShader();
	~INullVertexShader();
};

NS_END