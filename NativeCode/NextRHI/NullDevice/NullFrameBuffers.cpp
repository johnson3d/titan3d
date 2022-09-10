#include "NullFrameBuffers.h"
#include "NullBuffer.h"
#include "NullGpuDevice.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	NullFrameBuffers::NullFrameBuffers()
	{

	}
	NullFrameBuffers::~NullFrameBuffers()
	{
	}
	void NullFrameBuffers::FlushModify()
	{

	}

	NullSwapChain::NullSwapChain()
	{
	}

	NullSwapChain::~NullSwapChain()
	{
	}
	bool NullSwapChain::Init(NullGpuDevice* device, const FSwapChainDesc& desc)
	{
		Desc = desc;
		
		return Resize(device, Desc.Width, Desc.Height);
	}
	bool NullSwapChain::Resize(IGpuDevice* device1, UINT w, UINT h)
	{
		Desc.Width = w;
		Desc.Height = h;
		return true;
	}
	ITexture* NullSwapChain::GetBackBuffer(UINT index)
	{
		return nullptr;
	}
	IRenderTargetView* NullSwapChain::GetBackRTV(UINT index)
	{
		return nullptr;
	}
	UINT NullSwapChain::GetCurrentBackBuffer()
	{
		return 0;
	}
	void NullSwapChain::Present(IGpuDevice* device, UINT SyncInterval, UINT Flags)
	{
	}
}

NS_END