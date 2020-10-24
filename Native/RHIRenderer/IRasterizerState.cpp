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
	Cpp2CS1(EngineNS, IRasterizerState, GetDesc);
}