#pragma once
#include "IGpuBuffer.h"

NS_BEGIN

struct TR_CLASS(SV_LayoutStruct = 8)
IBufferUAV
{
	UINT FirstElement;
	UINT NumElements;
	UINT Flags;
};

struct TR_CLASS(SV_LayoutStruct = 8)
ITex1DUAV
{
	UINT MipSlice;
};

struct TR_CLASS(SV_LayoutStruct = 8)
ITex1DArrayUAV
{
	UINT MipSlice;
	UINT FirstArraySlice;
	UINT ArraySize;
};

struct TR_CLASS(SV_LayoutStruct = 8)
ITex2DUAV
{
	UINT MipSlice;
};

struct TR_CLASS(SV_LayoutStruct = 8)
ITex2DArrayUAV
{
	UINT MipSlice;
	UINT FirstArraySlice;
	UINT ArraySize;
};

struct TR_CLASS(SV_LayoutStruct = 8)
ITex3DUAV
{
	UINT MipSlice;
	UINT FirstWSlice;
	UINT WSize;
};

enum TR_ENUM()
EDimensionUAV
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

enum TR_ENUM()
	EUAVBufferFlag
{
	UAV_FLAG_RAW = 0x1,
	UAV_FLAG_APPEND = 0x2,
	UAV_FLAG_COUNTER = 0x4
};

struct TR_CLASS(SV_LayoutStruct = 8, SV_Manual)
IUnorderedAccessViewDesc
{
	void SetBuffer()
	{
		memset(this, 0, sizeof(IUnorderedAccessViewDesc));
		Format = EPixelFormat::PXF_UNKNOWN;
		ViewDimension = EDimensionUAV::UAV_DIMENSION_BUFFER;
		//MiscFlags = (UInt32)EResourceMiscFlag.BUFFER_STRUCTURED;
		Buffer.FirstElement = 0;
		//Buffer.NumElements = descBuf.ByteWidth / descBuf.StructureByteStride;
	}
	void SetTexture2D()
	{
		memset(this, 0, sizeof(IUnorderedAccessViewDesc));
		Format = EPixelFormat::PXF_UNKNOWN;
		ViewDimension = EDimensionUAV::UAV_DIMENSION_TEXTURE2D;
		Texture2D.MipSlice = 0;
	}
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

class TR_CLASS()
IUnorderedAccessView : public IRenderResource
{
public:
	IUnorderedAccessView();
	~IUnorderedAccessView();

public:
	IUnorderedAccessViewDesc		mDesc;
};

NS_END