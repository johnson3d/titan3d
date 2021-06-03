#pragma once
#include "IRenderResource.h"
#include "ITextureBase.h"

NS_BEGIN

class ITexture2D;
class IGpuBuffer;
class ICommandList;
class IRenderContext;
class GfxTextureStreaming;

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
IShaderResourceViewDesc
{
	IShaderResourceViewDesc()
	{
		mFormat = PXF_R8G8B8A8_UNORM;
		m_pTexture2D = nullptr;
	}

	EPixelFormat		mFormat;
	ITexture2D*			m_pTexture2D;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
ITxPicDesc
{
	/*ITxPicDesc()
	{
		SetDefault();
	}*/
	TR_FUNCTION()
	void SetDefault()
	{
		sRGB = 1;
		EtcFormat = ETCFormat::RGBA8;//rgba8
		MipLevel = 0;
		Width = 0;
		Height = 0;
	}
	int				sRGB;
	ETCFormat		EtcFormat;
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

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
EResourceDimension
{
	RESOURCE_DIMENSION_UNKNOWN = 0,
	RESOURCE_DIMENSION_BUFFER = 1,
	RESOURCE_DIMENSION_TEXTURE1D = 2,
	RESOURCE_DIMENSION_TEXTURE2D = 3,
	RESOURCE_DIMENSION_TEXTURE3D = 4
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8, SV_Manual)
BUFFER_SRV
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

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
TEX1D_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
TEX1D_ARRAY_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
	UINT FirstArraySlice;
	UINT ArraySize;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
TEX2D_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
TEX2D_ARRAY_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
	UINT FirstArraySlice;
	UINT ArraySize;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
TEX2DMS_SRV
{
	UINT UnusedField_NothingToDefine;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
TEX2DMS_ARRAY_SRV
{
	UINT FirstArraySlice;
	UINT ArraySize;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
TEX3D_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
TEXCUBE_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
TEXCUBE_ARRAY_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
	UINT First2DArrayFace;
	UINT NumCubes;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
BUFFEREX_SRV
{
	UINT FirstElement;
	UINT NumElements;
	UINT Flags;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8, SV_Manual)
ISRVDesc
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

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IShaderResourceView : public IRenderResource
{
public:
	IShaderResourceView();
	~IShaderResourceView();

	TR_FUNCTION()
	inline ITexture2D* GetTexture2D() 
	{
		return mTexture2D;
	}
	TR_FUNCTION()
	virtual IResourceState* GetResourceState() override{
		return &mResourceState;
	}

	TR_FUNCTION()
	virtual bool UpdateTexture2D(IRenderContext* rc, const ITexture2D* pTexture2D) = 0;

	TR_FUNCTION()
	void GetTxDesc(ITxPicDesc* desc)
	{
		*desc = mTxDesc;
	}

	TR_FUNCTION()
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
public:
	ISRVDesc			mSrvDesc;
	ITxPicDesc			mTxDesc;
	AutoRef<ITexture2D>		mTexture2D;
	AutoRef<IGpuBuffer>		mBuffer;
	IResourceState		mResourceState;
public:
	/*struct ETCDesc
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
	};*/
};

NS_END