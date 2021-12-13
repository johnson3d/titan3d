#pragma once
#include "ITextureBase.h"

NS_BEGIN

enum TR_ENUM()
RTV_DIMENSION
{
	RTV_DIMENSION_UNKNOWN = 0,
	RTV_DIMENSION_BUFFER = 1,
	RTV_DIMENSION_TEXTURE1D = 2,
	RTV_DIMENSION_TEXTURE1DARRAY = 3,
	RTV_DIMENSION_TEXTURE2D = 4,
	RTV_DIMENSION_TEXTURE2DARRAY = 5,
	RTV_DIMENSION_TEXTURE2DMS = 6,
	RTV_DIMENSION_TEXTURE2DMSARRAY = 7,
	RTV_DIMENSION_TEXTURE3D = 8
};

struct TR_CLASS(SV_LayoutStruct = 8)
	BUFFER_RTV
{
	union
	{
		UINT FirstElement;
		UINT ElementOffset;
	};
	union
	{
		UINT NumElements;
		UINT ElementWidth;
	};
};

struct TR_CLASS(SV_LayoutStruct = 8)
	TEX1D_RTV
{
	UINT MipSlice;
};

struct TR_CLASS(SV_LayoutStruct = 8)
	TEX1D_ARRAY_RTV
{
	UINT MipSlice;
	UINT FirstArraySlice;
	UINT ArraySize;
};

struct TR_CLASS(SV_LayoutStruct = 8)
	TEX2D_RTV
{
	UINT MipSlice;
};

struct TR_CLASS(SV_LayoutStruct = 8)
	TEX2D_ARRAY_RTV
{
	UINT MipSlice;
	UINT FirstArraySlice;
	UINT ArraySize;
};

enum TR_ENUM()
	ERtvType
{
	RTV_BufferSRV,
	RTV_Texture1D,
	RTV_Texture1DArray,
	RTV_Texture2D,
	RTV_Texture2DArray,
	RTV_Texture2DMS,
	RTV_Texture2DMSArray,
	RTV_Texture3D,
};

class ITexture2D;
struct TR_CLASS(SV_LayoutStruct = 8)
IRenderTargetViewDesc
{
	IRenderTargetViewDesc()
	{
		SetTexture2D();
	}
	void SetTexture2D()
	{
		memset(this, 0, sizeof(IRenderTargetViewDesc));
		Type = RTV_Texture2D;
		mGpuBuffer = nullptr;
		mCanBeSampled = TRUE;
		Width = 0;
		Height = 0;
		Format = PXF_R8G8B8A8_UNORM;
		ViewDimension = RTV_DIMENSION_TEXTURE2D;
	}
	ERtvType					Type;
	IGpuBuffer*					mGpuBuffer;
	vBOOL						mCanBeSampled;
	UINT						Width;
	UINT						Height;

	EPixelFormat				Format;
	RTV_DIMENSION				ViewDimension;

	union
	{
		BUFFER_RTV Buffer;
		TEX1D_RTV Texture1D;
		TEX1D_ARRAY_RTV Texture1DArray;
		TEX2D_RTV Texture2D;
		TEX2D_ARRAY_RTV Texture2DArray;
		//D3D11_TEX2DMS_RTV Texture2DMS;
		//D3D11_TEX2DMS_ARRAY_RTV Texture2DMSArray;
		//D3D11_TEX3D_RTV Texture3D;
	};
};

class TR_CLASS()
IRenderTargetView : public IRenderResource
{
public:
	IRenderTargetViewDesc	Desc;
	AutoRef<IGpuBuffer>		RefGpuBuffer;
public:
	IRenderTargetView();
	~IRenderTargetView();

	IGpuBuffer* GetGpuBuffer()
	{
		return RefGpuBuffer;
	}
};

NS_END