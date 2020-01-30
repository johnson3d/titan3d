#include "IDepthStencilState.h"

#define new VNEW

NS_BEGIN

StructImpl(StencilOpDesc);
StructImpl(IDepthStencilStateDesc);

IDepthStencilState::IDepthStencilState()
{
}


IDepthStencilState::~IDepthStencilState()
{
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpAPI1(EngineNS, IDepthStencilState, GetDesc, IDepthStencilStateDesc*);
}