#include "XImageBuffer.h"
#include "IImageDecoder.h"
#include "../../../NativeCode/Base/debug/vfxdebug.h"

#if !defined(EXCLUDE_DECODER_SUPPORT)

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
#if !defined(EXCLUDE_EXR_SUPPORT)
#include "XImageDecoder_Exr.h"
#endif


NS_BEGIN

XImageIO::XImageIO()
{}
XImageIO::~XImageIO()
{}

IImageDecoder::~IImageDecoder()
{
}

bool IImageDecoder::SaveImage(const XImageBuffer & buffer, XImageIO * pIO, XImageEncodeParames * ep)
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
#if !defined(EXCLUDE_EXR_SUPPORT)
static XIExrDecoder		g_ExrDecoder;
#endif

//返回的Decoder不需要删除
IImageDecoder * IImageDecoder::MatchDecoder(const char * pszFileName)
{
	if (pszFileName == NULL)
		return NULL;

	static IImageDecoder * decoders[] =
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
#if !defined(EXCLUDE_EXR_SUPPORT)
		&g_ExrDecoder,
#endif
	};

	size_t fileNameLength = strlen(pszFileName);

	char * lowerName = (char *)_vfxMemoryNew(fileNameLength + 1, __FILE__, __LINE__);

	for (size_t i = 0; i < fileNameLength; ++i)
		lowerName[i] = tolower(pszFileName[i]);

	lowerName[fileNameLength] = '\0';

	for (size_t i = 0; i < sizeof(decoders)/sizeof(IImageDecoder*); ++i)
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

NS_END

#endif