#include <string.h>
#include <algorithm>
#include "XImageBuffer.h"

#include "../../Core/debug/vfxdebug.h"

#if !defined(EXCLUDE_DECODER_SUPPORT)

#include "XImageDecoder.h"

#define EXCLUDE_WEBP_SUPPORT

#if !defined(EXCLUDE_PNG_SUPPORT)
#include "XImageDecoder_Png.h"
#endif
#if !defined(EXCLUDE_JPG_SUPPORT)
#include "XImageDecoder_Jpg.h"
#endif
#if !defined(EXCLUDE_WEBP_SUPPORT)
#include "XImageDecoder_Webp.h"
#endif


XImageIO::XImageIO()
{}
XImageIO::~XImageIO()
{}

XImageDecoder::~XImageDecoder()
{
}

bool XImageDecoder::SaveImage(const XImageBuffer & buffer, XImageIO * pIO, XImageEncodeParames * ep)
{
	return false;
}

#if !defined(EXCLUDE_PNG_SUPPORT)
static XIPngDecoder		g_PngDecoder;
#endif
#if !defined(EXCLUDE_JPG_SUPPORT)
static XIJpgDecoder		g_JpgDecoder;
#endif
#if !defined(EXCLUDE_WEBP_SUPPORT)
static XIWebpDecoder	g_WebpDecoder;
#endif

//返回的Decoder不需要删除
XImageDecoder * XImageDecoder::MatchDecoder(const char * pszFileName)
{
	if (pszFileName == NULL)
		return NULL;

	static XImageDecoder * decoders[] =
	{
#if !defined(EXCLUDE_PNG_SUPPORT)
		&g_PngDecoder,
#endif
#if !defined(EXCLUDE_JPG_SUPPORT)
		&g_JpgDecoder,
#endif
#if !defined(EXCLUDE_WEBP_SUPPORT)
		&g_WebpDecoder,
#endif
	};

	size_t fileNameLength = strlen(pszFileName);

	char * lowerName = (char *)_vfxMemoryNew(fileNameLength + 1, __FILE__, __LINE__);

	for (size_t i = 0; i < fileNameLength; ++i)
		lowerName[i] = tolower(pszFileName[i]);

	lowerName[fileNameLength] = '\0';

	for (size_t i = 0; i < sizeof(decoders)/sizeof(XImageDecoder*); ++i)
	{
		if (pszFileName != NULL && decoders[i]->CheckFileExtention(lowerName, false))
		{
			_vfxMemoryDelete(lowerName, __FILE__, __LINE__);

			return decoders[i];
		}
	}

	_vfxMemoryDelete(lowerName, __FILE__, __LINE__);

	return NULL;
}

#endif