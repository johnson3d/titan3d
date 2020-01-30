#pragma once
#include "IRenderResource.h"

NS_BEGIN

class ITexture2D;
class IGpuBuffer;
class ICommandList;
class IRenderContext;
class GfxTextureStreaming;
struct IShaderResourceViewDesc
{
	IShaderResourceViewDesc()
	{
		mFormat = PXF_R8G8B8A8_UNORM;
		m_pTexture2D = nullptr;
	}

	EPixelFormat		mFormat;
	ITexture2D*			m_pTexture2D;
};

struct ITxPicDesc
{
	ITxPicDesc()
	{
		sRGB = 1;
		EtcFormat = 4;//rgba8
		MipLevel = 0;
		Width = 0;
		Height = 0;
	}
	int				sRGB;
	int				EtcFormat;
	int				MipLevel;
	int				Width;
	int				Height;
};

//struct ITex2DDesc
//{
//	UINT				mWidth;
//	UINT				mHeight;
//	UINT				mMipLevels;
//	EPixelFormat	mFormat;
//};

enum EResourceDimension
{
	RESOURCE_DIMENSION_UNKNOWN = 0,
	RESOURCE_DIMENSION_BUFFER = 1,
	RESOURCE_DIMENSION_TEXTURE1D = 2,
	RESOURCE_DIMENSION_TEXTURE2D = 3,
	RESOURCE_DIMENSION_TEXTURE3D = 4
};

struct BUFFER_SRV
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

struct TEX1D_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
};

struct TEX1D_ARRAY_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
	UINT FirstArraySlice;
	UINT ArraySize;
};

struct TEX2D_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
};

struct TEX2D_ARRAY_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
	UINT FirstArraySlice;
	UINT ArraySize;
};

struct TEX2DMS_SRV
{
	UINT UnusedField_NothingToDefine;
};

struct TEX2DMS_ARRAY_SRV
{
	UINT FirstArraySlice;
	UINT ArraySize;
};

struct TEX3D_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
};

struct TEXCUBE_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
};

struct TEXCUBE_ARRAY_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
	UINT First2DArrayFace;
	UINT NumCubes;
};

struct BUFFEREX_SRV
{
	UINT FirstElement;
	UINT NumElements;
	UINT Flags;
};

struct ISRVDesc
{
	EPixelFormat Format;
	EResourceDimension ViewDimension;
	union
	{
		BUFFER_SRV Buffer;
		TEX1D_SRV Texture1D;
		TEX1D_ARRAY_SRV Texture1DArray;
		TEX2D_SRV Texture2D;
		TEX2D_ARRAY_SRV Texture2DArray;
		TEX2DMS_SRV Texture2DMS;
		TEX2DMS_ARRAY_SRV Texture2DMSArray;
		TEX3D_SRV Texture3D;
		TEXCUBE_SRV TextureCube;
		TEXCUBE_ARRAY_SRV TextureCubeArray;
		BUFFEREX_SRV BufferEx;
	};
};

class IShaderResourceView : public IRenderResource
{
public:
	IShaderResourceView();
	~IShaderResourceView();

	inline ITexture2D* GetTexture2D() 
	{
		return mTexture2D;
	}
	virtual IResourceState* GetResourceState() override{
		return &mResourceState;
	}

	void GetTxDesc(ITxPicDesc* desc)
	{
		*desc = mTxDesc;
	}

	virtual EPixelFormat GetTextureFormat();
	
	virtual vBOOL Save2Memory(IRenderContext* rc, IBlobObject* data, int Type)
	{
		return FALSE;
	}
	virtual vBOOL GetTexture2DData(IRenderContext* rc, IBlobObject* data, int level, int RectWidth, int RectHeight)
	{
		//ASSERT(false);
		return FALSE;
	}
	virtual void RefreshResource()
	{

	}
	GfxTextureStreaming* GetTexStreaming() {
		return mTexStreaming;
	}
public:
	ISRVDesc			mSrvDesc;
	//RName				mName;
	ITxPicDesc			mTxDesc;
	AutoRef<ITexture2D>		mTexture2D;
	AutoRef<IGpuBuffer>		mBuffer;
	IResourceState		mResourceState;

	AutoRef<GfxTextureStreaming> mTexStreaming;
public:
	struct ETCDesc
	{
		EPixelFormat	Format;
		vBOOL	sRGB;
		int		Mipmap;
	};
	struct ETCLayer
	{
		int		Width;
		int		Height;
		UINT	Size;
	};
	struct ETCMipBuffers : public VIUnknown
	{
		std::vector<ETCLayer>			Layers;
		std::vector<std::vector<BYTE>>	Pixels;
	};
};

NS_END