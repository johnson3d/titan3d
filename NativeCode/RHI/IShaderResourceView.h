#pragma once
#include "IRenderResource.h"
#include "ITextureBase.h"

NS_BEGIN

class ITexture2D;
class IGpuBuffer;
class ICommandList;
class IRenderContext;
class GfxTextureStreaming;

struct TR_CLASS(SV_LayoutStruct = 8)
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

struct TR_CLASS(SV_LayoutStruct = 8)
TEX1D_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
};

struct TR_CLASS(SV_LayoutStruct = 8)
TEX1D_ARRAY_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
	UINT FirstArraySlice;
	UINT ArraySize;
};

struct TR_CLASS(SV_LayoutStruct = 8)
TEX2D_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
};

struct TR_CLASS(SV_LayoutStruct = 8)
TEX2D_ARRAY_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
	UINT FirstArraySlice;
	UINT ArraySize;
};

struct TR_CLASS(SV_LayoutStruct = 8)
TEX2DMS_SRV
{
	UINT UnusedField_NothingToDefine;
};

struct TR_CLASS(SV_LayoutStruct = 8)
TEX2DMS_ARRAY_SRV
{
	UINT FirstArraySlice;
	UINT ArraySize;
};

struct TR_CLASS(SV_LayoutStruct = 8)
TEX3D_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
};

struct TR_CLASS(SV_LayoutStruct = 8)
TEXCUBE_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
};

struct TR_CLASS(SV_LayoutStruct = 8)
TEXCUBE_ARRAY_SRV
{
	UINT MostDetailedMip;
	UINT MipLevels;
	UINT First2DArrayFace;
	UINT NumCubes;
};

struct TR_CLASS(SV_LayoutStruct = 8)
BUFFEREX_SRV
{
	UINT FirstElement;
	UINT NumElements;
	UINT Flags;
};

enum TR_ENUM()
	ESrvType
{
	ST_BufferSRV,
	ST_Texture1D,
	ST_Texture1DArray,
	ST_Texture2D,
	ST_Texture2DArray,
	ST_Texture2DMS,
	ST_Texture2DMSArray,
	ST_Texture3D,
	ST_Texture3DArray,
	ST_TextureCube,
	ST_TextureCubeArray,
	ST_BufferEx,
};

enum TR_ENUM()
	SRV_DIMENSION
{
	SRV_DIMENSION_UNKNOWN = 0,
	SRV_DIMENSION_BUFFER = 1,
	SRV_DIMENSION_TEXTURE1D = 2,
	SRV_DIMENSION_TEXTURE1DARRAY = 3,
	SRV_DIMENSION_TEXTURE2D = 4,
	SRV_DIMENSION_TEXTURE2DARRAY = 5,
	SRV_DIMENSION_TEXTURE2DMS = 6,
	SRV_DIMENSION_TEXTURE2DMSARRAY = 7,
	SRV_DIMENSION_TEXTURE3D = 8,
	SRV_DIMENSION_TEXTURECUBE = 9,
	SRV_DIMENSION_TEXTURECUBEARRAY = 10,
	SRV_DIMENSION_BUFFEREX = 11,
};

struct TR_CLASS(SV_LayoutStruct = 8, SV_Manual)
	IShaderResourceViewDesc
{
	void SetTexture2D()
	{
		memset(this, 0, sizeof(IShaderResourceViewDesc));
		Type = ST_Texture2D;
		mGpuBuffer = nullptr;
		Format = PXF_R8G8B8A8_UNORM;
		ViewDimension = SRV_DIMENSION_TEXTURE2D;
		Texture2D.MipLevels = 0;
	}
	void SetTexture2DArray()
	{
		memset(this, 0, sizeof(IShaderResourceViewDesc));
		Type = ST_Texture2DArray;
		mGpuBuffer = nullptr;
		Format = PXF_R8G8B8A8_UNORM;
		ViewDimension = SRV_DIMENSION_TEXTURE2DARRAY;
		Texture2DArray.MipLevels = 0;
	}
	void SetBuffer()
	{
		memset(this, 0, sizeof(IShaderResourceViewDesc));
		Type = ST_BufferSRV;
		mGpuBuffer = nullptr;
		Format = PXF_UNKNOWN;
		ViewDimension = SRV_DIMENSION_BUFFER;
		Buffer.FirstElement = 0;
		Buffer.NumElements = 0;
	}
	ESrvType Type;
	IGpuBuffer* mGpuBuffer;
	
	EPixelFormat Format;
	SRV_DIMENSION ViewDimension;
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

class TR_CLASS()
IShaderResourceView : public IRenderResource
{
public:
	IShaderResourceView();
	~IShaderResourceView();

	inline IGpuBuffer* GetGpuBuffer()
	{
		return mBuffer;
	}
	inline ITexture2D* TryGetTexture2D() {
		if (mSrvDesc.Type != ST_Texture2D)
			return nullptr;
		return mBuffer.As<ITexture2D>();
	}
	virtual IResourceState* GetResourceState() override{
		return &mResourceState;
	}

	virtual bool UpdateBuffer(IRenderContext* rc, const IGpuBuffer* buffer) = 0;

	virtual EPixelFormat GetFormat();
	
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
	virtual void* GetAPIObject() = 0;
public:
	IShaderResourceViewDesc	mSrvDesc;
	ITxPicDesc				mTxDesc;

	TR_MEMBER(SV_NoBind)
	AutoRef<IGpuBuffer>		mBuffer;
	TR_MEMBER(SV_NoBind)
	IResourceState			mResourceState;
};

NS_END