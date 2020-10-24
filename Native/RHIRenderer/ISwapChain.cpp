#include "ISwapChain.h"

#define new VNEW

NS_BEGIN

ISwapChain::ISwapChain()
{
}


ISwapChain::~ISwapChain()
{
}

NS_END

using namespace EngineNS;

extern "C"
{
	Cpp2CS0(EngineNS, ISwapChain, GetTexture2D);
	Cpp2CS0(EngineNS, ISwapChain, OnLost);
	Cpp2CS1(EngineNS, ISwapChain, OnRestore);
	Cpp2CS1(EngineNS, ISwapChain, GetDesc);
	Cpp2CS2(EngineNS, ISwapChain, Present);
}