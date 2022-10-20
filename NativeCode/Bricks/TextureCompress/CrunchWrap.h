#pragma once
#include "../../Base/IUnknown.h"
#include "../../Base/BlobObject.h"

NS_BEGIN

namespace TextureCompress
{
	enum TR_ENUM(SV_EnumNoFlags = true)
		ETextureFormat
	{
		DXT1 = 0,
		DXT3,
		DXT5,

		// Various DXT5 derivatives
		DXT5_CCxY,    // Luma-chroma
		DXT5_xGxR,    // Swizzled 2-component
		DXT5_xGBR,    // Swizzled 3-component
		DXT5_AGBR,    // Swizzled 4-component

		// ATI 3DC and X360 DXN
		DXN_XY,
		DXN_YX,

		// DXT5 alpha blocks only
		DXT5A,
		ETC1,

		DXT1a,

		Total,
		ForceDWORD = 0xFFFFFFFF
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FCubeImage
	{
		UINT* Image0 = nullptr;
		UINT* Image1 = nullptr;
		UINT* Image2 = nullptr;
		UINT* Image3 = nullptr;
		UINT* Image4 = nullptr;
		UINT* Image5 = nullptr;
	};

	class TR_CLASS()
		CrunchWrap : public VIUnknown
	{
	public:
		static bool CompressPixels(UINT numOfThreads, IBlobObject* result, UINT width, UINT height, const FCubeImage* pSrcImage, ETextureFormat fmt, bool genMips = true, bool srgb = true, float bitrate = 0.0f, int quality_level = -1);
	};
}

NS_END