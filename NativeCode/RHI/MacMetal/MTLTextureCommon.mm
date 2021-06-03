#include "MTLTextureCommon.h"

namespace EngineNS 
{
	static const MTLPixelFormat MtlPixelFormatArray[] =
	{
		MTLPixelFormatInvalid,
		MTLPixelFormatR16Float,
		MTLPixelFormatR16Uint,
		MTLPixelFormatR16Sint,
		MTLPixelFormatR16Unorm,
		MTLPixelFormatR16Snorm,

		MTLPixelFormatR32Uint,
		MTLPixelFormatR32Sint,
		MTLPixelFormatR32Float,

		MTLPixelFormatRGBA8Sint,
		MTLPixelFormatRGBA8Uint,
		MTLPixelFormatRGBA8Unorm,
		MTLPixelFormatRGBA8Snorm,

		MTLPixelFormatRG16Uint,
		MTLPixelFormatRG16Sint,
		MTLPixelFormatRG16Float,
		MTLPixelFormatRG16Unorm,
		MTLPixelFormatRG16Snorm,

		MTLPixelFormatRGBA16Uint,
		MTLPixelFormatRGBA16Sint,
		MTLPixelFormatRGBA16Float,
		MTLPixelFormatRGBA16Unorm,
		MTLPixelFormatRGBA16Snorm,

		MTLPixelFormatRGBA32Uint,
		MTLPixelFormatRGBA32Sint,
		MTLPixelFormatRGBA32Float,

		MTLPixelFormatInvalid,
		MTLPixelFormatInvalid,
		MTLPixelFormatInvalid,

		MTLPixelFormatRG32Uint,
		MTLPixelFormatRG32Sint,
		MTLPixelFormatRG32Float,

		MTLPixelFormatDepth32Float,
		MTLPixelFormatDepth32Float,
		MTLPixelFormatDepth32Float,
		MTLPixelFormatDepth32Float,

		MTLPixelFormatBGRA8Unorm,
		MTLPixelFormatRG11B10Float,
		MTLPixelFormatRG8Unorm,
		MTLPixelFormatR8Unorm,
		
	};

	MTLPixelFormat TranslatePixelFormat_RHI2Mtl(EPixelFormat pixel_format)
	{
		UINT32 idx = (UINT32)pixel_format;
		return MtlPixelFormatArray[idx];
	}

}