#pragma once
#include "IRenderResource.h"

NS_BEGIN

struct IBufferUAV
{
	UINT FirstElement;
	UINT NumElements;
	UINT Flags;
};

struct ITex1DUAV
{
	UINT MipSlice;
};

struct ITex1DArrayUAV
{
	UINT MipSlice;
	UINT FirstArraySlice;
	UINT ArraySize;
};

struct ITex2DUAV
{
	UINT MipSlice;
};

struct ITex2DArrayUAV
{
	UINT MipSlice;
	UINT FirstArraySlice;
	UINT ArraySize;
};

struct ITex3DUAV
{
	UINT MipSlice;
	UINT FirstWSlice;
	UINT WSize;
};

enum EDimensionUAV
{
	UAV_DIMENSION_UNKNOWN = 0,
	UAV_DIMENSION_BUFFER = 1,
	UAV_DIMENSION_TEXTURE1D = 2,
	UAV_DIMENSION_TEXTURE1DARRAY = 3,
	UAV_DIMENSION_TEXTURE2D = 4,
	UAV_DIMENSION_TEXTURE2DARRAY = 5,
	UAV_DIMENSION_TEXTURE3D = 8,
	UAV_DIMENSION_BUFFEREX = 11,
};

struct IUnorderedAccessViewDesc
{
	EPixelFormat Format;
	EDimensionUAV ViewDimension;
	union
	{
		IBufferUAV Buffer;
		ITex1DUAV Texture1D;
		ITex1DArrayUAV Texture1DArray;
		ITex2DUAV Texture2D;
		ITex2DArrayUAV Texture2DArray;
		ITex3DUAV Texture3D;
	};
};

enum EGpuUsage
{
	USAGE_DEFAULT = 0,
	USAGE_IMMUTABLE = 1,
	USAGE_DYNAMIC = 2,
	USAGE_STAGING = 3
};

enum EResourceMiscFlag
{
	GENERATE_MIPS = 0x1,
	SHARED = 0x2,
	TEXTURECUBE = 0x4,
	DRAWINDIRECT_ARGS = 0x10,
	BUFFER_ALLOW_RAW_VIEWS = 0x20,
	BUFFER_STRUCTURED = 0x40,
	RESOURCE_CLAMP = 0x80,
	SHARED_KEYEDMUTEX = 0x100,
	GDI_COMPATIBLE = 0x200,
	SHARED_NTHANDLE = 0x800,
	RESTRICTED_CONTENT = 0x1000,
	RESTRICT_SHARED_RESOURCE = 0x2000,
	RESTRICT_SHARED_RESOURCE_DRIVER = 0x4000,
	GUARDED = 0x8000,
	TILE_POOL = 0x20000,
	TILED = 0x40000,
	HW_PROTECTED = 0x80000
};

struct IGpuBufferDesc
{
	UINT ByteWidth;
	EGpuUsage Usage;
	UINT BindFlags;
	UINT CPUAccessFlags;
	UINT MiscFlags;
	UINT StructureByteStride;
};

struct IMappedSubResource
{
	void *pData;
	UINT RowPitch;
	UINT DepthPitch;
};

enum EGpuMAP
{
	MAP_READ = 1,
	MAP_WRITE = 2,
	MAP_READ_WRITE = 3,
	MAP_WRITE_DISCARD = 4,
	MAP_WRITE_NO_OVERWRITE = 5
};

class ICommandList;
class IGpuBuffer : public IRenderResource
{
public:
	virtual vBOOL GetBufferData(IRenderContext* rc, IBlobObject* blob)
	{
		return FALSE;
	}
	virtual vBOOL Map(IRenderContext* rc, 
		UINT Subresource,
		EGpuMAP MapType,
		UINT MapFlags, 
		IMappedSubResource* mapRes)
	{
		return FALSE;
	}
	virtual void Unmap(IRenderContext* rc, UINT Subresource)
	{
		
	}

	virtual vBOOL UpdateBufferData(ICommandList* cmd, void* data, UINT size)
	{
		return TRUE;
	}

	virtual IResourceState* GetResourceState() override {
		return &mResourceState;
	}

	IGpuBufferDesc			mDesc;
	IResourceState			mResourceState;
};

class IUnorderedAccessView : public IRenderResource
{
public:
	IUnorderedAccessView();
	~IUnorderedAccessView();
};

NS_END