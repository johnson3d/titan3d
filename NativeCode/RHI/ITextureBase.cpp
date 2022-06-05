#include "ITextureBase.h"
#include "ICommandList.h"
#include "IRenderSystem.h"
#include "../../Base/float16/float16.h"

#include "../../../3rd/native/Image.Shared/XImageDecoder.h"
#include "../../../3rd/native/Image.Shared/XImageBuffer.h"

#define new VNEW

NS_BEGIN

ITextureBase::ITextureBase()
{
}


ITextureBase::~ITextureBase()
{
}

void ITexture2D::BuildImageBlob(IBlobObject* blob, void* pData, UINT RowPitch)
{
	int nStride = (mTextureDesc.Width * 32 + 7) / 8;
	int nPixelBufferSize = nStride * mTextureDesc.Height;
	if (blob->GetSize() != sizeof(PixelDesc) + nPixelBufferSize)
	{
		blob->ReSize(sizeof(PixelDesc) + nPixelBufferSize);
	}
	PixelDesc* pOutDesc = (PixelDesc*)blob->GetData();
	pOutDesc->Width = mTextureDesc.Width;
	pOutDesc->Height = mTextureDesc.Height;
	pOutDesc->Stride = nStride;
	pOutDesc->Format = PXF_R8G8B8A8_UNORM;

	XImageBuffer image;
	image.m_pImage = (uint8_t*)(pOutDesc + 1);
	image.m_nWidth = mTextureDesc.Width;
	image.m_nHeight = mTextureDesc.Height;
	image.m_nBitCount = 32;
	image.m_nStride = nStride;
	BYTE* row = (BYTE*)pData;
	BYTE* dst = image.m_pImage;
	UINT copySize = mTextureDesc.Width * 4;
	if (mTextureDesc.Format == PXF_R8G8B8A8_UNORM)
	{
		for (UINT i = 0; i < mTextureDesc.Height; i++)
		{
			memcpy(dst, row, copySize);
			/*for (UINT j = 0; j < mDesc.Width; j++)
			{
				if (mDesc.Format == PXF_R8G8B8A8_UNORM)
				{
					auto color = *(DWORD*)(&row[j * 4]);
					if (color != 0)
					{
						dst[j * 4] = color;
					}
					dst[j * 4] = row[j * 4];
					dst[j * 4 + 1] = row[j * 4 + 1];
					dst[j * 4 + 2] = row[j * 4 + 2];
					dst[j * 4 + 3] = row[j * 4 + 3];
				}
				else
				{
					dst[j] = 0;
				}
			}*/
			row += RowPitch;
			dst += image.m_nStride;
		}
	}
	else if (mTextureDesc.Format == PXF_B8G8R8A8_UNORM)
	{
		for (UINT i = 0; i < mTextureDesc.Height; i++)
		{
			for (UINT j = 0; j < mTextureDesc.Width; j++)
			{
				auto color = *(DWORD*)(&row[j * 4]);
				dst[j * 4 + 3] = row[j * 4];
				dst[j * 4 + 2] = row[j * 4 + 1];
				dst[j * 4 + 1] = row[j * 4 + 2];
				dst[j * 4 + 0] = row[j * 4 + 3];
			}
			row += RowPitch;
			dst += image.m_nStride;
		}
	}
	else if (mTextureDesc.Format == PXF_R10G10B10A2_UNORM)
	{
		for (UINT i = 0; i < mTextureDesc.Height; i++)
		{
			for (UINT j = 0; j < mTextureDesc.Width; j++)
			{
				auto color = *(DWORD*)(&row[j * 4]);
				DWORD channel = (color & 0x3FF);
				dst[j * 4] = (BYTE)(channel * 255 / 1024);

				channel = ((color >> 10) & 0x3FF);
				dst[j * 4 + 1] = (BYTE)(channel * 255 / 1024);
				
				channel = ((color >> 20) & 0x3FF);
				dst[j * 4 + 2] = (BYTE)(channel * 255 / 1024);

				channel = ((color >> 30) & 0x3);
				dst[j * 4 + 3] = (BYTE)(channel * 255 / 4);
			}
			row += RowPitch;
			dst += image.m_nStride;
		}
	}
	else if (mTextureDesc.Format == PXF_R16G16B16A16_FLOAT)
	{
		for (UINT i = 0; i < mTextureDesc.Height; i++)
		{
			auto color = (float16*)row;
			for (UINT j = 0; j < mTextureDesc.Width; j++)
			{
				auto f1 = (float)color[j * 4];
				auto f2 = (float)color[j * 4 + 1];
				auto f3 = (float)color[j * 4 + 2];
				auto f4 = (float)color[j * 4 + 3];
				if(f1>1.0f)
					dst[j * 4] = 255;
				else
					dst[j * 4] = (BYTE)(f1 * 255.0f);
				if (f2 > 1.0f)
					dst[j * 4] = 255;
				else
					dst[j * 4 + 1] = (BYTE)(f2 * 255.0f);
				if (f3> 1.0f)
					dst[j * 4] = 255;
				else
					dst[j * 4 + 2] = (BYTE)(f3 * 255.0f);
				if (f4 > 1.0f)
					dst[j * 4] = 255;
				else
					dst[j * 4 + 3] = (BYTE)(f4 * 255.0f);
			}
			row += RowPitch;
			dst += image.m_nStride;
		}
	}

	switch (IRenderSystem::Instance->mRHIType)
	{
		case RHT_OGL:
		{
			image.FlipPixel4();
		}
		break;
		default:
		{
			break;
		}
	}

	image.m_pImage = nullptr;
	/*blob->PushData((BYTE*)&saveDesc, sizeof(PixelDesc));
	blob->PushData(image.m_pImage, saveDesc.Height * saveDesc.Stride);*/
}

NS_END
