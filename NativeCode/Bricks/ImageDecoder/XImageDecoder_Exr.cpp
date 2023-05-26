#if !defined(EXCLUDE_EXR_SUPPORT)

#include <string.h>
#include <algorithm>
#include "XImageBuffer.h"
#include "IImageDecoder.h"
#include "XImageDecoder_Exr.h"
#define TINYEXR_IMPLEMENTATION
#include "../../../3rd/native/zlib/zlib.h"
#include "../../../3rd/native/Image.Shared/tinyexr/tinyexr.h"

#ifndef PLATFORM_WIN
extern void OutputDebugStringA(const char* lpOutputString);
#endif

NS_BEGIN

XIExrDecoder::~XIExrDecoder()
{

}

bool XIExrDecoder::CheckFileExtention(const char* pszFileName, bool bSaveImage)
{
	if (pszFileName == NULL)
		return false;
	size_t nLength = strlen(pszFileName);
	return (nLength >= 4 && strncmp(pszFileName + nLength - 4, ".exr", 4) == 0);
}

bool XIExrDecoder::CheckFileContent(const uint8_t* pMemory, size_t nLength)
{
	if (pMemory == NULL || nLength < 4) return false;

	EXRVersion version;
	int ret = ParseEXRVersionFromMemory(&version, pMemory, nLength);

	if (ret != 0)
	{
		::OutputDebugStringA("invalid exr file verison\n");
		return false;
	}

	return true;
}
bool XIExrDecoder::LoadImageX(XImageBuffer& image, const char* pszFileName)
{
	try
	{
		float* out;
		int width;
		int height;
		const char* err = nullptr;
		if (LoadEXR(&out, &width, &height, pszFileName, &err) != TINYEXR_SUCCESS)
			return false;

		//if (image.Create(width, height, XIPF_R32G32B32A32F))
		//{
		//	SaveEXRImageToMemory()
		//	return true;
		//}

		return false;
	}
	catch (...)
	{
		image.Cleanup();
		return false;
	}
}

bool XIExrDecoder::LoadImageX(XImageBuffer& image, const uint8_t* pMemory, size_t nLength)
{


	return true;
}

bool XIExrDecoder::SaveImage(const XImageBuffer& xib, XImageIO* pIO, XImageEncodeParames* ep)
{
	return true;
}

void XIExrDecoder::GetFileExtention(const char** pszExt)
{
	*pszExt = ".exr";
}

XDecoderType XIExrDecoder::GetDecoderType() const
{
	return XDecoderType::EXRDecoder;
}

NS_END

#endif


