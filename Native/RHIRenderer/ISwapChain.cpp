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
	CSharpReturnAPI0(ITexture2D*, EngineNS, ISwapChain, GetTexture2D);
	CSharpAPI0(EngineNS, ISwapChain, OnLost);
	CSharpReturnAPI1(vBOOL, EngineNS, ISwapChain, OnRestore, const ISwapChainDesc*);
	CSharpAPI1(EngineNS, ISwapChain, GetDesc, ISwapChainDesc*);
	CSharpAPI2(EngineNS, ISwapChain, Present, UINT, UINT);
}