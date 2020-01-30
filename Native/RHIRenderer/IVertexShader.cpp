#include "IVertexShader.h"

#define new VNEW

NS_BEGIN

IVertexShader::IVertexShader()
{
}


IVertexShader::~IVertexShader()
{
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI0(IShaderDesc*, EngineNS, IVertexShader, GetDesc);
}