#if !defined(EXCLUDE_WEBP_SUPPORT)

//#include <string.h>
//#include <algorithm>
#include "XImageBuffer.h"
#include "XImageDecoder.h"
#include "XImageDecoder_Webp.h"

#include "libwebp-0.4.2/src/webp/decode.h"
#include "libwebp-0.4.2/src/webp/encode.h"

NS_BEGIN

static XImageFormat vpic_LoadWDP_Memory(XImageBuffer & image, const uint8_t * pStart, intptr_t nSize)
{
	XImageFormat eFormat = XIF_MAX;

	do
	{
		WebPDecoderConfig config;
		if (WebPInitDecoderConfig(&config) == 0) break;
		if (WebPGetFeatures(static_cast<const uint8_t*>(pStart), nSize, &config.input) != VP8_STATUS_OK) break;
		if (config.input.width == 0 || config.input.height == 0) break;
		if (config.input.has_alpha)
		{
			if (!image.Create(config.input.width, config.input.height, 32))
				break;
			eFormat = XIF_A8R8G8B8;
			config.output.colorspace = MODE_RGBA;
		}
		else
		{
			if (!image.Create(config.input.width, config.input.height, 24))
				break;
			eFormat = XIF_X8R8G8B8;
			config.output.colorspace = MODE_RGB;
		}

		config.output.u.RGBA.rgba = static_cast<uint8_t*>(image.m_pImage);
		config.output.u.RGBA.stride = image.m_nStride;
		config.output.u.RGBA.size = image.m_nStride * image.m_nHeight;
		config.output.is_external_memory = 1;
		if (WebPDecode(static_cast<const uint8_t*>(pStart), nSize, &config) != VP8_STATUS_OK)
		{
			eFormat = XIF_MAX;
			break;
		}

		image.Flip();
	} while (0);

	return eFormat;
}

XIWebpDecoder::~XIWebpDecoder()
{

}

const char * c_pWEBPExtention3 = ".wbp";
const char * c_pWEBPExtention4 = ".webp";

bool XIWebpDecoder::CheckFileExtention(const char * pszFileName, bool bSaveImage)
{
	if (pszFileName == NULL)
		return false;
	size_t nLength = strlen(pszFileName);
	if (nLength < 3)
		return false;
	if (strncmp(pszFileName + nLength - 4, c_pWEBPExtention3, 4) == 0)
		return true;
	if (nLength < 4)
		return false;
	return strncmp(pszFileName + nLength - 5, c_pWEBPExtention4, 5) == 0;
}

bool XIWebpDecoder::CheckFileContent(const uint8_t * pMemory, size_t nLength)
{
	if (pMemory == NULL || nLength <= 12)
		return false;

	return memcmp(pMemory, "RIFF", 4) == 0 && memcmp(pMemory + 8, "WEBP", 4) == 0;
}

bool XIWebpDecoder::LoadImageX(XImageBuffer & xib, const uint8_t * pMemory, size_t nLength)
{
	if (pMemory == NULL)
		return false;
	XImageFormat eFormat = vpic_LoadWDP_Memory(xib, pMemory, nLength);
	if (XIF_MAX != eFormat)
	{
		return true;
	}
	return false;
}

void XIWebpDecoder::GetFileExtention(const char ** pszExt)
{
	pszExt = c_pWEBPExtention3;
}

NS_END

#endif
