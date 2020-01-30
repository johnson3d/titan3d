#include "IRasterizerState.h"

#define new VNEW

NS_BEGIN

IRasterizerState::IRasterizerState()
{
}


IRasterizerState::~IRasterizerState()
{
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpAPI1(EngineNS, IRasterizerState, GetDesc, IRasterizerStateDesc*);
}