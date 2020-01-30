#include "ITextureBase.h"
#include "ICommandList.h"
#include "IRenderSystem.h"

#include "../../../3rd/Image.Shared/XImageDecoder.h"
#include "../../../3rd/Image.Shared/XImageBuffer.h"

#define new VNEW

NS_BEGIN

ITextureBase::ITextureBase()
{
}


ITextureBase::~ITextureBase()
{
}

struct PixelDesc
{
	int Width;
	int Height;
	int Stride;
	EPixelFormat Format;
};

void ITexture2D::BuildImageBlob(IBlobObject* blob, void* pData, UINT RowPitch)
{
	int nStride = (mDesc.Width * 32 + 7) / 8;
	int nPixelBufferSize = nStride * mDesc.Height;
	if (blob->GetSize() != sizeof(PixelDesc) + nPixelBufferSize)
	{
		blob->ReSize(sizeof(PixelDesc) + nPixelBufferSize);
	}
	PixelDesc* pOutDesc = (PixelDesc*)blob->GetData();
	pOutDesc->Width = mDesc.Width;
	pOutDesc->Height = mDesc.Height;
	pOutDesc->Stride = nStride;
	pOutDesc->Format = PXF_R8G8B8A8_UNORM;

	XImageBuffer image;
	image.m_pImage = (uint8_t*)(pOutDesc + 1);
	image.m_nWidth = mDesc.Width;
	image.m_nHeight = mDesc.Height;
	image.m_nBitCount = 32;
	image.m_nStride = nStride;
	BYTE* row = (BYTE*)pData;
	BYTE* dst = image.m_pImage;
	UINT copySize = mDesc.Width * 4;
	if (mDesc.Format == PXF_R8G8B8A8_UNORM)
	{
		for (UINT i = 0; i < mDesc.Height; i++)
		{
			memcpy(dst, row, copySize);
			/*for (UINT j = 0; j < mDesc.Width; j++)
			{
				if (mDesc.Format == PXF_R8G8B8A8_UNORM)
				{
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

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI5(vBOOL, EngineNS, ITexture2D, Map, ICommandList*, int, void**, UINT*, UINT*);
	CSharpAPI2(EngineNS, ITexture2D, Unmap, ICommandList*, int);
	CSharpAPI3(EngineNS, ITexture2D, BuildImageBlob, IBlobObject*, void*, UINT);
	CSharpAPI6(EngineNS, ITexture2D, UpdateMipData, ICommandList*, UINT, void*, UINT, UINT, UINT);
}