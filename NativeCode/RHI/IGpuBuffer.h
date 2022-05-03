#pragma once
#include "IRenderResource.h"

NS_BEGIN

enum TR_ENUM()
	EGpuUsage
{
	USAGE_DEFAULT = 0,
	USAGE_IMMUTABLE = 1,
	USAGE_DYNAMIC = 2,
	USAGE_STAGING = 3
};

enum TR_ENUM() 
EBindFlag
{
	BIND_VERTEX_BUFFER = 0x1L,
	BIND_INDEX_BUFFER = 0x2L,
	BIND_CONSTANT_BUFFER = 0x4L,
	BIND_SHADER_RESOURCE = 0x8L,
	BIND_STREAM_OUTPUT = 0x10L,
	BIND_RENDER_TARGET = 0x20L,
	BIND_DEPTH_STENCIL = 0x40L,
	BIND_UNORDERED_ACCESS = 0x80L,
	BIND_DECODER = 0x200L,
	BIND_VIDEO_ENCODER = 0x400L
};

enum TR_ENUM()
	EResourceMiscFlag
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

struct TR_CLASS(SV_LayoutStruct = 8)
IGpuBufferDesc
{
	IGpuBufferDesc()
	{
		SetDefault();
	}
	void SetDefault()
	{
		Type = GBT_UavBuffer;
		Usage = USAGE_DEFAULT;
		ByteWidth = 0;
		BindFlags = 0;
		CPUAccessFlags = 0;
		MiscFlags = 0;
		StructureByteStride = 0;
	}
	EGpuBufferType Type;
	EGpuUsage Usage;
	UINT ByteWidth;
	UINT BindFlags;
	UINT CPUAccessFlags;
	UINT MiscFlags;
	UINT StructureByteStride;
};

struct TR_CLASS(SV_LayoutStruct = 8)
IMappedSubResource
{
	void *pData;
	UINT RowPitch;
	UINT DepthPitch;
};

enum TR_ENUM()
EGpuMAP
{
	MAP_READ = 1,
	MAP_WRITE = 2,
	MAP_READ_WRITE = 3,
	MAP_WRITE_DISCARD = 4,
	MAP_WRITE_NO_OVERWRITE = 5
};

class ICommandList;
class TR_CLASS()
IGpuBuffer : public IRenderResource
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
	virtual vBOOL UpdateBufferData(ICommandList* cmd, UINT offset, void* data, UINT size)
	{
		return TRUE;
	}

	virtual IResourceState* GetResourceState() override {
		return &mResourceState;
	}

	virtual void* GetHWBuffer() const = 0;

	IGpuBufferDesc			mBufferDesc;
	TR_MEMBER(SV_NoBind)
	IResourceState			mResourceState;
};

NS_END