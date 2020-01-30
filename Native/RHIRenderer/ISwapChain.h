#pragma once
#include "IRenderResource.h"

NS_BEGIN

class ITexture2D;

struct ISwapChainDesc
{
	ISwapChainDesc()
	{
		Width = 0;
		Height = 0;
		Format = PXF_R8G8B8A8_UINT;
		ColorSpace = COLOR_SPACE_SRGB_NONLINEAR;
		WindowHandle = nullptr;
	}
	UINT	Width;
	UINT	Height;
	EPixelFormat	Format;
	EColorSpace		ColorSpace;
	void*	WindowHandle;
};

class ISwapChain : public IRenderResource
{
public:
	ISwapChain();
	~ISwapChain();

	virtual ITexture2D* GetTexture2D() = 0;
	virtual void BindCurrent() = 0;
	virtual void Present(UINT SyncInterval, UINT Flags) = 0;

	virtual void OnLost() = 0;
	virtual vBOOL OnRestore(const ISwapChainDesc* desc) = 0;
	inline void GetDesc(ISwapChainDesc* desc)
	{
		*desc = mDesc;
	}
public:
	ISwapChainDesc			mDesc;
};

NS_END