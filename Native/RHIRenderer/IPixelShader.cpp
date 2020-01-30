#include "IPixelShader.h"

#define new VNEW

NS_BEGIN

IPixelShader::IPixelShader()
{
}


IPixelShader::~IPixelShader()
{
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI0(IShaderDesc*, EngineNS, IPixelShader, GetDesc);
}