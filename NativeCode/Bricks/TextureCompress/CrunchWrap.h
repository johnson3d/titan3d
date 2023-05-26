#pragma once
#include "../../Base/IUnknown.h"
#include "../../Base/BlobObject.h"
#include "../../NextRHI/NxRHIDefine.h"

NS_BEGIN

namespace TextureCompress
{
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
		CrunchWrap : public IWeakReference
	{
	public:
		static bool CompressPixels(UINT numOfThreads, IBlobObject* result, UINT width, UINT height, const FCubeImage* pSrcImage, EngineNS::NxRHI::ETextureCompressFormat fmt, bool genMips = true, bool srgb = true, float bitrate = 0.0f, int quality_level = -1);
		static void* DecompressDxt(const void* pDDS_file_data, UINT dds_file_size, UINT** ppImages, NxRHI::FPictureDesc* tex_desc);
		static void FreeDecpressDxtContext(void* p);
	};
}

NS_END