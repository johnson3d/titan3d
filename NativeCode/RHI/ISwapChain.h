#pragma once
#include "IRenderResource.h"

NS_BEGIN

class ITexture2D;

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
ISwapChainDesc
{
	ISwapChainDesc()
	{
		SetDefault();
	}
	TR_FUNCTION()
	void SetDefault()
	{
		Width = 0;
		Height = 0;
		Format = PXF_R8G8B8A8_UNORM;
		ColorSpace = COLOR_SPACE_SRGB_NONLINEAR;
		WindowHandle = nullptr;
	}
	UINT	Width;
	UINT	Height;
	EPixelFormat	Format;
	EColorSpace		ColorSpace;
	void*	WindowHandle;
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
ISwapChain : public IRenderResource
{
public:
	ISwapChain();
	~ISwapChain();

	TR_FUNCTION()
	virtual ITexture2D* GetTexture2D() = 0;
	TR_FUNCTION()
	virtual void BindCurrent() = 0;
	TR_FUNCTION()
	virtual void Present(UINT SyncInterval, UINT Flags) = 0;

	TR_FUNCTION()
	virtual void OnLost() = 0;
	TR_FUNCTION()
	virtual vBOOL OnRestore(const ISwapChainDesc* desc) = 0;
	TR_FUNCTION()
	inline void GetDesc(ISwapChainDesc* desc)
	{
		*desc = mDesc;
	}
public:
	ISwapChainDesc			mDesc;
};

NS_END