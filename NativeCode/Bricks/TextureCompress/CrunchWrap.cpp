#include "CrunchWrap.h"
#include "../../../3rd/native/crunch/inc/crnlib.h"
#include "../../../3rd/native/crunch/inc/dds_defs.h"
#include "../../../3rd/native/crunch/inc/crn_decomp.h"
#include "../../Base/IO/MemStream.h"
#include <xutility>

#pragma comment(lib, "crnlibD_x64_VC9.lib")

#define new VNEW

extern "C" const void* __stdcall __std_find_trivial_2(const void* _First, const void* _Last, uint16_t _Val) noexcept
{
	const uint16_t* pCur = (const uint16_t*)_First;
	const uint16_t* pEnd = (const uint16_t*)_Last;
	while (pCur < pEnd)
	{
		if (*pCur == _Val)
			return pCur;
		pCur++;
	}
	return nullptr;
}

NS_BEGIN

namespace TextureCompress
{
	crn_format ToCrunchFomat(ETextureFormat fmt)
	{
		switch (fmt)
		{
		case EngineNS::TextureCompress::DXT1:
		case EngineNS::TextureCompress::DXT1a:
			return crn_format::cCRNFmtDXT1;
		case EngineNS::TextureCompress::DXT3:
			return crn_format::cCRNFmtDXT3;
		case EngineNS::TextureCompress::DXT5:
			return crn_format::cCRNFmtDXT5;
		case EngineNS::TextureCompress::DXT5_CCxY:
			return crn_format::cCRNFmtDXT5_CCxY;
		case EngineNS::TextureCompress::DXT5_xGxR:
			return crn_format::cCRNFmtDXT5_xGxR;
		case EngineNS::TextureCompress::DXT5_xGBR:
			return crn_format::cCRNFmtDXT5_xGBR;
		case EngineNS::TextureCompress::DXT5_AGBR:
			return crn_format::cCRNFmtDXT5_AGBR;
		case EngineNS::TextureCompress::DXN_XY:
			return crn_format::cCRNFmtDXN_XY;
		case EngineNS::TextureCompress::DXN_YX:
			return crn_format::cCRNFmtDXN_YX;
		case EngineNS::TextureCompress::DXT5A:
			return crn_format::cCRNFmtDXT5A;
		case EngineNS::TextureCompress::ETC1:
			return crn_format::cCRNFmtETC1;
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
	bool CrunchWrap::CompressPixels(UINT numOfThreads, IBlobObject* result, UINT width, UINT height, const FCubeImage* pSrcImage, ETextureFormat fmt, bool genMips, bool srgb, float bitrate, int quality_level)
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
		if (fmt == DXT1a)
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
		for (int i = 0; i < face; i++)
		{
			for (int j = 0; j < dds_desc.dwMipMapCount; j++)
			{
				const crn_uint32 width = std::max(1U, dds_desc.dwWidth >> j);
				const crn_uint32 height = std::max(1U, dds_desc.dwHeight >> j);
				const crn_uint32 blocks_x = std::max(1U, (width + 3) >> 2);
				const crn_uint32 blocks_y = std::max(1U, (height + 3) >> 2);
				const crn_uint32 row_pitch = blocks_x * crnd::crnd_get_bytes_per_dxt_block(comp_params.m_format);
				const crn_uint32 total_face_size = row_pitch * blocks_y;
			}
		}

		result->PushData(&actual_quality_level, sizeof(UINT));
		result->PushData(&actual_bitrate, sizeof(float));
		result->PushData(&output_file_size, sizeof(UINT));
		result->PushData(pOutput_file_data, output_file_size);
		
		crn_free_block(pOutput_file_data);

		return true;
	}
}

NS_END