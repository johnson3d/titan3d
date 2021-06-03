#pragma once
#include "IRenderResource.h"

NS_BEGIN

class ICommandList;

class IPixelBuffer : public IRenderResource
{
public:
	virtual void* MapBuffer(ICommandList* cmdList) = 0;
	virtual void UnmapBuffer(ICommandList* cmdList) = 0;
};

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_EnumNoFlags)
ETCFormat
{
	UNKNOWN,
	//
	ETC1,
	//
	// ETC2 formats
	RGB8,
	SRGB8,
	RGBA8,
	SRGBA8,
	R11,
	SIGNED_R11,
	RG11,
	SIGNED_RG11,
	RGB8A1,
	SRGB8A1,
	//
	FORMATS,
	//
	DEFAULT = SRGB8
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
ITextureBase : public IRenderResource
{
public:
	ITextureBase();
	~ITextureBase();
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
ITexture2DDesc
{
	TR_FUNCTION()
	void SetDefault()
	{
		Width = 0;
		Height = 0;
		MipLevels = 1;
		ArraySize = 1;
		Format = PXF_R8G8B8A8_UNORM;
		BindFlags = BF_SHADER_RES;
		CPUAccess = 0;// CAS_WRITE | CAS_READ;
		InitData = nullptr;
	}
	UINT			Width;
	UINT			Height;
	UINT			MipLevels;
	UINT			ArraySize;
	EPixelFormat	Format;
	UINT			BindFlags;
	UINT			CPUAccess;
	
	ImageInitData*	InitData;

	TR_DISCARD()
	std::string ToString()
	{
		return VStringA_FormatV("Format = %d; Width = %d; Height = %d; MipLevels = %d; BindFlags = %d; CPUAccess = %d;",
			Format, Width, Height, MipLevels, BindFlags, CPUAccess);
	}
};

struct ITexture2DDescWithLayersData : public ITexture2DDesc
{
	struct ImageLayer
	{
		UINT				Width;
		UINT				Height;
		std::vector<BYTE>	mImageData;
	};
	std::vector<ImageLayer>	Layers;
	void CreateLayerBuffers()
	{
		UINT width = Width;
		UINT height = Height;
		auto pixelWidth = GetPixelByteWidth(Format);
		for (UINT i = 0; i < MipLevels; i++)
		{
			ImageLayer layer;
			layer.Width = width;
			layer.Height = height;
			width = width / 2;
			height = height / 2;
			layer.mImageData.resize(pixelWidth * layer.Width * layer.Height);
			Layers.push_back(layer);
		}
	}
};

struct TR_CLASS(SV_LayoutStruct = 8)
PixelDesc
{
	int Width;
	int Height;
	int Stride;
	EPixelFormat Format;
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
ITexture2D : public ITextureBase
{
public:
	TR_MEMBER()
	ITexture2DDesc		mDesc;
public:
	TR_FUNCTION()
	virtual vBOOL Map(ICommandList* cmd, int MipLevel, void** ppData, UINT* pRowPitch, UINT* pDepthPitch) = 0;
	TR_FUNCTION()
	virtual void Unmap(ICommandList* cmd, int MipLevel) = 0;
	TR_FUNCTION()
	void BuildImageBlob(IBlobObject* blob, void* pData, UINT RowPitch);
	TR_FUNCTION()
	virtual void UpdateMipData(ICommandList* cmd, UINT level, void* pData, UINT width, UINT height, UINT Pitch) = 0;
};

NS_END