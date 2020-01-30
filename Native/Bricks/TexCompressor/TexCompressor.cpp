#include "TexCompressor.h"
#include "../../3rd/Image.Shared/XImageBuffer.h"
#include "../../3rd/Image.Shared/XImageDecoder.h"
#include "../../3rd/Image.Shared/stb_dxt.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::TexCompressor, EngineNS::VIUnknown);

vBOOL TexCompressor::EncodePng2ETC(void* ptr, UINT size, Etc::Image::Format fmt, int miplevel,
				IBlobObject* blob)
{
	auto pDecoder = XImageDecoder::MatchDecoder("a.png");
	auto rawImage = new XImageBuffer();
	auto bLoad = pDecoder->LoadImageX(*rawImage, (const BYTE*)ptr, size);
	if (bLoad == false)
	{
		delete rawImage;
		return FALSE;
	}

	rawImage->ConvertToRGBA8();
	rawImage->FlipPixel4();

	auto floatImage = rawImage->ConvertToFloatImage();
	delete rawImage;

	unsigned int uiSourceWidth = floatImage->m_nWidth;
	unsigned int uiSourceHeight = floatImage->m_nHeight;

	int mMipMapLevel = miplevel;
	if(mMipMapLevel==0)
	{
		int dim = (uiSourceWidth < uiSourceHeight) ? uiSourceWidth : uiSourceHeight;
		int maxMips = 0;
		while (dim >= 1)
		{
			maxMips++;
			dim >>= 1;
		}
		if (mMipMapLevel == 0 || mMipMapLevel > maxMips)
		{
			mMipMapLevel = maxMips;
		}
	}

	Etc::RawImage *pMipmapImages = new Etc::RawImage[mMipMapLevel];
	Etc::ErrorMetric e_ErrMetric = Etc::ErrorMetric::RGBA;;
	float fEffort = ETCCOMP_DEFAULT_EFFORT_LEVEL;
	unsigned int uiJobs = 32;
	int iEncodingTime_ms;
	UINT mipFilterFlags = Etc::FILTER_WRAP_NONE;
#define MAX_JOBS 1024
	Etc::EncodeMipmaps(floatImage->m_pImageFloat32,
		uiSourceWidth, uiSourceHeight,
		fmt,
		e_ErrMetric,
		fEffort,
		uiJobs,
		MAX_JOBS,
		mMipMapLevel,
		mipFilterFlags,
		pMipmapImages,
		&iEncodingTime_ms);

	int SaveEtcFormat = (int)fmt;
	blob->PushData((BYTE*)(&SaveEtcFormat), sizeof(int));
	blob->PushData((BYTE*)(&mMipMapLevel), sizeof(int));

	for (int i = 0; i < mMipMapLevel; i++)
	{
		const auto& mipmap = pMipmapImages[i];
		
		blob->PushData((BYTE*)&mipmap.uiExtendedWidth, sizeof(int));
		blob->PushData((BYTE*)&mipmap.uiExtendedHeight, sizeof(int));
		blob->PushData((BYTE*)&mipmap.uiEncodingBitsBytes, sizeof(UINT));

		blob->PushData(mipmap.paucEncodingBits.get(), mipmap.uiEncodingBitsBytes);
	}

	delete[] pMipmapImages;
	delete floatImage;
	return TRUE;
}

vBOOL TexCompressor::EncodePng2DXT(void* ptr, UINT size, vBOOL bAlpha, EDxtCompressMode mode, OUT int* mipmap,
	IBlobObject* blob)
{
	auto pDecoder = XImageDecoder::MatchDecoder("a.png");
	std::shared_ptr<XImageBuffer> pImage = std::shared_ptr<XImageBuffer>(new XImageBuffer());
	auto bLoad = pDecoder->LoadImageX(*pImage, (const BYTE*)ptr, size);
	if (bLoad == false)
	{
		return FALSE;
	}

	pImage->ConvertToRGBA8();
	
	UINT32 width = pImage->m_nWidth;
	UINT32 height = pImage->m_nHeight;
	std::shared_ptr<XImageBuffer> pMipImage = pImage;
	std::vector<XImageBuffer*>	MipImagePtrArray;
	int widthMipCount;
	int heightMipCount;
	bool isPowerOfTwo;
	pImage->CalculateMipCount(&widthMipCount, &heightMipCount, mipmap, &isPowerOfTwo);

	for (int i = 0; i < *mipmap; i++)
	{
		if (isPowerOfTwo)
		{
			width = width / 2;
			height = height / 2;
			auto pTemp = pMipImage->BoxDownSampler(
				width,
				height,
				i + 1 < widthMipCount ? i + 1 : widthMipCount,
				i + 1 < heightMipCount ? i + 1 : heightMipCount);

			pMipImage = std::shared_ptr<XImageBuffer>(pTemp);
		}
		else
		{
			auto pTemp = pMipImage->DownSampler();
			pMipImage = std::shared_ptr<XImageBuffer>(pTemp);
		}

		//这里要把Image切割成4x4的块作为source传入，太麻烦了，后面有空再说吧
		//stb_compress_dxt_block(nullptr, pMipImage->m_pImage, bAlpha, (int)mode);
	}
	return TRUE;
}
NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI5(vBOOL, EngineNS, TexCompressor, EncodePng2ETC, void*, UINT, Etc::Image::Format, int, IBlobObject*);
}