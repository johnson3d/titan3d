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

class ITextureBase : public IRenderResource
{
public:
	ITextureBase();
	~ITextureBase();
};

struct ITexture2DDesc
{
	ITexture2DDesc()
	{
		Width = 0;
		Height = 0;
		MipLevels = 1;
		ArraySize = 1;
		Format = PXF_R8G8B8A8_UNORM;
		BindFlags = BF_SHADER_RES;
		CPUAccess = 0;// CAS_WRITE | CAS_READ;
	}
	UINT			Width;
	UINT			Height;
	UINT			MipLevels;
	UINT			ArraySize;
	EPixelFormat	Format;
	UINT			BindFlags;
	UINT			CPUAccess;
	ImageInitData	InitData;

	VStringA ToString()
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

class ITexture2D : public ITextureBase
{
public:
	ITexture2DDesc		mDesc;
public:
	virtual vBOOL Map(ICommandList* cmd, int MipLevel, void** ppData, UINT* pRowPitch, UINT* pDepthPitch) = 0;
	virtual void Unmap(ICommandList* cmd, int MipLevel) = 0;
	void BuildImageBlob(IBlobObject* blob, void* pData, UINT RowPitch);
	virtual void UpdateMipData(ICommandList* cmd, UINT level, void* pData, UINT width, UINT height, UINT Pitch) = 0;
};

NS_END