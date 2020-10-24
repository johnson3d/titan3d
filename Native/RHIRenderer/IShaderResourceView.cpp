#include "IShaderResourceView.h"
#include "ITextureBase.h"
#include "GfxTextureStreaming.h"
#include "IUnorderedAccessView.h"
#include "../3rd/Image.Shared/XImageBuffer.h"
#include "../3rd/Image.Shared/XImageDecoder.h"
#include "../Core/xnd/vfxxnd.h"

#define new VNEW

NS_BEGIN

IShaderResourceView::IShaderResourceView()
{
	
}

IShaderResourceView::~IShaderResourceView()
{
	mTexture2D.Clear();
}


EPixelFormat IShaderResourceView::GetTextureFormat()
{
	if (mTexture2D == nullptr)
	{
		return PXF_UNKNOWN;
	}

	return mTexture2D->mDesc.Format;
}

NS_END

using namespace EngineNS;

extern "C"
{
	////CSharpReturnAPI0(const char*, EngineNS, IShaderResourceView, GetName);
	////CSharpAPI1(EngineNS, IShaderResourceView, SetName, const char*);
	Cpp2CS1(EngineNS, IShaderResourceView, GetTxDesc);
	Cpp2CS0(EngineNS, IShaderResourceView, GetTextureFormat);
	Cpp2CS3(EngineNS, IShaderResourceView, Save2Memory);
	Cpp2CS5(EngineNS, IShaderResourceView, GetTexture2DData);
	Cpp2CS0(EngineNS, IShaderResourceView, RefreshResource);
	Cpp2CS0(EngineNS, IShaderResourceView, GetTexStreaming);

	VFX_API vBOOL SDK_ImageEncoder_SaveETC2(const char* file, XNDAttrib* attr, int mipLevel, vBOOL sRGB)
	{
		auto pDecoder = XImageDecoder::MatchDecoder(file);
		if (pDecoder == nullptr)
			return FALSE;
		ViseFile io;
		if (io.Open(file, VFile::modeRead) == FALSE)
		{
			return FALSE;
		}
		auto memLength = io.GetLength();
		auto pBuffer = new BYTE[memLength];
		io.Read(pBuffer, memLength);
		io.Close();

		XImageBuffer* image = new XImageBuffer();
		auto bLoad = pDecoder->LoadImageX(*image, pBuffer, memLength);
		if (bLoad == false)
			return FALSE;
		Safe_DeleteArray(pBuffer);

		int widthMipCount;
		int heightMipCount;
		int mipCount;
		bool isPowerOfTwo;
		image->CalculateMipCount(&widthMipCount, &heightMipCount, &mipCount, &isPowerOfTwo);
		if (mipLevel == 0)
		{
			mipLevel = mipCount;
		}
		else
		{
			mipLevel = vfxMIN(mipLevel, mipCount);
		}

		//int width = image->m_nWidth;
		//int height = image->m_nHeight;
		XImageBuffer* lvlImage = image;
		IShaderResourceView::ETCDesc desc;
		desc.sRGB = sRGB;
		desc.Mipmap = mipLevel;
		switch (image->m_nBitCount)
		{
		case 32:
			desc.Format = PXF_R8G8B8A8_UNORM;
			break;
		default:
			ASSERT(false);
			break;
		}
		attr->Write(desc);
		for (int i = 0; i < mipLevel; i++)
		{
			/*auto saved = lvlImage;
			XETC2Compressor compressor;
			int compressedImageSize = 0;
			if (!compressor.CalculateCompressedDataSize(lvlImage, &compressedImageSize))
			{
				Safe_Delete(image);
				return FALSE;
			}

			BYTE* data = new BYTE[compressedImageSize + 1];
			if (!compressor.CompressImage(lvlImage, data, compressedImageSize))
			{
				Safe_DeleteArray(data);
				Safe_Delete(image);
				return FALSE;
			}
			IShaderResourceView::ETCLayer layer;
			layer.Width = lvlImage->m_nWidth;
			layer.Height = lvlImage->m_nHeight;
			layer.Size = compressedImageSize;
			attr->Write(layer);
			attr->Write(data, compressedImageSize);
			Safe_DeleteArray(data);

			if (isPowerOfTwo)
			{
				width = width / 2;
				height = height / 2;
				lvlImage = image->BoxDownSampler(
					width,
					height,
					i + 1 < widthMipCount ? i + 1 : widthMipCount,
					i + 1 < heightMipCount ? i + 1 : heightMipCount);
			}
			else
			{
				lvlImage = lvlImage->DownSampler();
			}

			if (i > 0)
				Safe_Delete(saved);*/
		}
		if (lvlImage != image)
		{
			Safe_Delete(lvlImage);
		}
		Safe_Delete(image);

		return TRUE;
	}
	VFX_API vBOOL SDK_ImageEncoder_SaveETC2_PNG(BYTE* pBuffer, UINT length, XNDAttrib* attr, int mipLevel, vBOOL sRGB)
	{
		auto pDecoder = XImageDecoder::MatchDecoder("a.png");
		
		XImageBuffer* image = new XImageBuffer();
		auto bLoad = pDecoder->LoadImageX(*image, pBuffer, length);
		if (bLoad == false)
			return FALSE;

		int widthMipCount;
		int heightMipCount;
		int mipCount;
		bool isPowerOfTwo;
		image->CalculateMipCount(&widthMipCount, &heightMipCount, &mipCount, &isPowerOfTwo);
		if (mipLevel == 0)
		{
			mipLevel = mipCount;
		}
		else
		{
			mipLevel = vfxMIN(mipLevel, mipCount);
		}

		//int width = image->m_nWidth;
		//int height = image->m_nHeight;
		XImageBuffer* lvlImage = image;
		IShaderResourceView::ETCDesc desc;
		desc.sRGB = sRGB;
		desc.Mipmap = mipLevel;
		switch (image->m_nBitCount)
		{
		case 32:
			desc.Format = PXF_R8G8B8A8_UNORM;
			break;
		default:
			ASSERT(false);
			break;
		}
		attr->Write(desc);
		for (int i = 0; i < mipLevel; i++)
		{
			//auto saved = lvlImage;
			/*XETC2Compressor compressor;
			int compressedImageSize = 0;
			if (!compressor.CalculateCompressedDataSize(lvlImage, &compressedImageSize))
			{
				Safe_Delete(image);
				return FALSE;
			}

			BYTE* data = new BYTE[compressedImageSize + 1];
			if (!compressor.CompressImage(lvlImage, data, compressedImageSize))
			{
				Safe_DeleteArray(data);
				Safe_Delete(image);
				return FALSE;
			}
			IShaderResourceView::ETCLayer layer;
			layer.Width = lvlImage->m_nWidth;
			layer.Height = lvlImage->m_nHeight;
			layer.Size = compressedImageSize;
			attr->Write(layer);
			attr->Write(data, compressedImageSize);
			Safe_DeleteArray(data);

			if (isPowerOfTwo)
			{
				width = width / 2;
				height = height / 2;
				lvlImage = image->BoxDownSampler(
					width,
					height,
					i + 1 < widthMipCount ? i + 1 : widthMipCount,
					i + 1 < heightMipCount ? i + 1 : heightMipCount);
			}
			else
			{
				lvlImage = lvlImage->DownSampler();
			}

			if (i > 0)
				Safe_Delete(saved);*/
		}
		if (lvlImage != image)
		{
			Safe_Delete(lvlImage);
		}
		Safe_Delete(image);

		return TRUE;
	}
}
