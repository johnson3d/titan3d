#include "IComputeShader.h"

#define new VNEW

NS_BEGIN

IComputeShader::IComputeShader()
{
}


IComputeShader::~IComputeShader()
{
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI0(IShaderDesc*, EngineNS, IComputeShader, GetDesc);
}