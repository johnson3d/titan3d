#include "CrunchWrap.h"
#include "../../../3rd/native/crunch/inc/crnlib.h"
#include "../../../3rd/native/crunch/inc/dds_defs.h"
#include "../../../3rd/native/crunch/inc/crn_decomp.h"
#include "../../Base/IO/MemStream.h"
#include <xutility>

#if defined(_DEBUG)
#pragma comment(lib, "crnlibD_x64_VC9.lib")
#else 
#pragma comment(lib, "crnlib_x64_VC9.lib")//crnlib_DLL_x64_VC9.lib
#endif

#define new VNEW

//extern "C" const void* __stdcall __std_find_trivial_2(const void* _First, const void* _Last, uint16_t _Val) noexcept
//{
//	const uint16_t* pCur = (const uint16_t*)_First;
//	const uint16_t* pEnd = (const uint16_t*)_Last;
//	while (pCur < pEnd)
//	{
//		if (*pCur == _Val)
//			return pCur;
//		pCur++;
//	}
//	return nullptr;
//}

NS_BEGIN

namespace TextureCompress
{
	crn_format ToCrunchFomat(NxRHI::ETextureCompressFormat fmt)
	{
		switch (fmt)
		{
		case NxRHI::ETextureCompressFormat::TCF_Dxt1:
		case NxRHI::ETextureCompressFormat::TCF_Dxt1a:
			return crn_format::cCRNFmtDXT1;
		case NxRHI::ETextureCompressFormat::TCF_Dxt3:
			return crn_format::cCRNFmtDXT3;
		case NxRHI::ETextureCompressFormat::TCF_Dxt5:
			return crn_format::cCRNFmtDXT5;
		default:
			return crn_format::cCRNFmtInvalid;
		}
	}
	static crn_bool progress_callback_func(crn_uint32 phase_index, crn_uint32 total_phases, crn_uint32 subphase_index, crn_uint32 total_subphases, void* pUser_data_ptr)
	{
		int percentage_complete = (int)(.5f + (phase_index + float(subphase_index) / total_subphases) * 100.0f) / total_phases;
		//printf("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\bProcessing: %u%%", std::min(100, std::max(0, percentage_complete)));
		return true;
	}
	EPixelFormat ExtFormatToPixelFormat(NxRHI::ETextureCompressFormat fmt, bool srgb)
	{
		switch (fmt)
		{
		case NxRHI::ETextureCompressFormat::TCF_Dxt1:
		case NxRHI::ETextureCompressFormat::TCF_Dxt1a:
			if(srgb)
				return PXF_BC1_UNORM_SRGB;
			else
				return PXF_BC1_UNORM;
		case NxRHI::ETextureCompressFormat::TCF_Dxt3:
			if (srgb)
				return PXF_BC2_UNORM_SRGB;
			else
				return PXF_BC2_UNORM;
		case NxRHI::ETextureCompressFormat::TCF_Dxt5:
			if (srgb)
				return PXF_BC3_UNORM_SRGB;
			else
				return PXF_BC3_UNORM;
		default:
			break;
		}
		return PXF_UNKNOWN;
	}
	bool CrunchWrap::CompressPixels(UINT numOfThreads, IBlobObject* result, UINT width, UINT height, const FCubeImage* pSrcImage, NxRHI::ETextureCompressFormat fmt, bool genMips, bool srgb, float bitrate, int quality_level)
	{
		bool use_adaptive_block_sizes = true;
		crn_comp_params comp_params{};
		comp_params.m_width = width;
		comp_params.m_height = height;
		comp_params.m_file_type = cCRNFileTypeDDS;
		comp_params.m_target_bitrate = bitrate;
		comp_params.m_quality_level = quality_level;

		comp_params.m_pImages[0][0] = pSrcImage->Image0;
		comp_params.m_pImages[1][0] = pSrcImage->Image1;
		comp_params.m_pImages[2][0] = pSrcImage->Image2;
		comp_params.m_pImages[3][0] = pSrcImage->Image3;
		comp_params.m_pImages[4][0] = pSrcImage->Image4;
		comp_params.m_pImages[5][0] = pSrcImage->Image5;

		comp_params.m_format = ToCrunchFomat(fmt);
		if (fmt == NxRHI::ETextureCompressFormat::TCF_Dxt1a)
		{
			comp_params.set_flag(cCRNCompFlagDXT1AForTransparency, true);
		}
		comp_params.set_flag(cCRNCompFlagPerceptual, srgb);
		comp_params.set_flag(cCRNCompFlagHierarchical, use_adaptive_block_sizes);
		
		/*SYSTEM_INFO g_system_info;
		GetSystemInfo(&g_system_info);
		numOfThreads = std::max<int>(0, (int)g_system_info.dwNumberOfProcessors - 1);*/

		comp_params.m_num_helper_threads = numOfThreads;
		comp_params.m_pProgress_func = progress_callback_func;

		crn_mipmap_params mip_params;
		mip_params.m_gamma_filtering = srgb;
		mip_params.m_min_mip_size = 8;
		mip_params.m_mode = genMips ? cCRNMipModeGenerateMips : cCRNMipModeNoMips;

		UINT actual_quality_level;
		float actual_bitrate;
		UINT output_file_size;
		void* pOutput_file_data = crn_compress(comp_params, mip_params, output_file_size, &actual_quality_level, &actual_bitrate);
		if (pOutput_file_data == nullptr)
			return false;

		/*auto fp = fopen("d:/test.dds", "wb+");
		fwrite(pOutput_file_data, 1, output_file_size, fp);
		fclose(fp);*/

		MemStreamReader ar;
		ar.ProxyPointer((BYTE*)pOutput_file_data, output_file_size);

		UINT signature = 0;
		ar.Read(signature);
		ASSERT(crnlib::cDDSFileSignature == signature);
		crnlib::DDSURFACEDESC2 dds_desc;
		ar.Read(dds_desc);

		if (dds_desc.ddsCaps.dwCaps & crnlib::DDSCAPS_MIPMAP)
		{

		}
		if (dds_desc.ddsCaps.dwCaps & crnlib::DDSCAPS_COMPLEX)
		{

		}
		int face = 1;
		if (dds_desc.ddsCaps.dwCaps2 & crnlib::DDSCAPS2_CUBEMAP)
		{
			face = 6;
			if (dds_desc.ddsCaps.dwCaps2 & crnlib::DDSCAPS2_CUBEMAP_POSITIVEX)
			{
				
			}
		}
		
		NxRHI::FPictureDesc picDesc{};
		picDesc.Width = dds_desc.dwWidth;
		picDesc.Height = dds_desc.dwHeight;
		picDesc.MipLevel = dds_desc.dwMipMapCount;
		picDesc.Format = ExtFormatToPixelFormat(fmt, srgb);
		picDesc.CompressFormat = fmt;
		picDesc.CubeFaces = face;		
		picDesc.sRGB = srgb;

		{
			for (int j = 0; j < (int)dds_desc.dwMipMapCount; j++)
			{
				const UINT width = std::max(1U, dds_desc.dwWidth >> j);
				const UINT height = std::max(1U, dds_desc.dwHeight >> j);
				if (width < 4 || height < 4)
				{
					dds_desc.dwMipMapCount = j;
					picDesc.MipLevel = j;
					break;
				}
				if (width % 4 != 0 || height % 4 != 0)
				{
					dds_desc.dwMipMapCount = j;
					picDesc.MipLevel = j;
					break;
				}
			}
		}

		result->PushData(&picDesc, sizeof(picDesc));
		
		for (int i = 0; i < face; i++)
		{
			for (int j = 0; j < (int)dds_desc.dwMipMapCount; j++)
			{
				const UINT width = std::max(1U, dds_desc.dwWidth >> j);
				const UINT height = std::max(1U, dds_desc.dwHeight >> j);
				const UINT blocks_x = std::max(1U, (width + 3) >> 2);
				const UINT blocks_y = std::max(1U, (height + 3) >> 2);
				const UINT row_pitch = blocks_x * crnd::crnd_get_bytes_per_dxt_block(comp_params.m_format);
				const UINT total_face_size = row_pitch * blocks_y;
				ASSERT(width >= 4 && height >= 4);
				result->PushData(&width, sizeof(UINT));
				result->PushData(&height, sizeof(UINT));
				result->PushData(&row_pitch, sizeof(UINT));

				auto pPixels = new BYTE[total_face_size];
				ar.Read(pPixels, total_face_size);
				result->PushData(&total_face_size, sizeof(UINT));
				result->PushData(pPixels, total_face_size);
				delete[] pPixels;
			}
		}
		
		crn_free_block(pOutput_file_data);

		return true;
	}
	EPixelFormat CrnPixelFormatTo(crnlib::pixel_format fmt)
	{
		switch (fmt)
		{
		case crnlib::PIXEL_FMT_INVALID:
			break;
		case crnlib::PIXEL_FMT_DXT1:
			return EPixelFormat::PXF_BC1_UNORM;
		case crnlib::PIXEL_FMT_DXT2:
			return EPixelFormat::PXF_BC2_UNORM;
		case crnlib::PIXEL_FMT_DXT3:
			return EPixelFormat::PXF_BC2_UNORM;
		case crnlib::PIXEL_FMT_DXT4:
			return EPixelFormat::PXF_BC3_UNORM;
		case crnlib::PIXEL_FMT_DXT5:
			return EPixelFormat::PXF_BC3_UNORM;
		case crnlib::PIXEL_FMT_3DC:
			break;
		case crnlib::PIXEL_FMT_DXN:
			break;
		case crnlib::PIXEL_FMT_DXT5A:
			return EPixelFormat::PXF_BC3_UNORM;
		case crnlib::PIXEL_FMT_DXT5_CCxY:
			break;
		case crnlib::PIXEL_FMT_DXT5_xGxR:
			break;
		case crnlib::PIXEL_FMT_DXT5_xGBR:
			break;
		case crnlib::PIXEL_FMT_DXT5_AGBR:
			break;
		case crnlib::PIXEL_FMT_DXT1A:
			return EPixelFormat::PXF_BC1_UNORM;
		case crnlib::PIXEL_FMT_ETC1:
			break;
		case crnlib::PIXEL_FMT_R8G8B8:
			break;
		case crnlib::PIXEL_FMT_L8:
			break;
		case crnlib::PIXEL_FMT_A8:
			break;
		case crnlib::PIXEL_FMT_A8L8:
			break;
		case crnlib::PIXEL_FMT_A8R8G8B8:
			break;
		default:
			break;
		}return EPixelFormat::PXF_R8G8B8A8_UNORM;
	}
	void* CrunchWrap::DecompressDxt(const void* pDDS_file_data, UINT dds_file_size, UINT** ppImages, NxRHI::FPictureDesc* tex_desc)
	{
		crn_texture_desc ddsDesc{};

		auto conjtext = crn_decompress_dds_to_images_withcontext(pDDS_file_data, dds_file_size, ppImages, ddsDesc);
		if (conjtext == nullptr)
		{
			return nullptr;
		}

		tex_desc->SetDefault();
		tex_desc->MipLevel = ddsDesc.m_levels;
		tex_desc->CubeFaces = ddsDesc.m_faces;
		tex_desc->Width = ddsDesc.m_width;
		tex_desc->Height = ddsDesc.m_height;

		tex_desc->Format = CrnPixelFormatTo((crnlib::pixel_format)ddsDesc.m_fmt_fourcc);

		return conjtext;
	}
	void CrunchWrap::FreeDecpressDxtContext(void* p)
	{
		free_crn_decompress_context(p);
	}
}

NS_END