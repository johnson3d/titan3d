#pragma once

#include "../NxGpuDevice.h"
#include "../NxRHIDefine.h"

#ifdef PLATFORM_WIN
#define VK_USE_PLATFORM_WIN32_KHR
#endif

#include <vulkan/vulkan.h>

#pragma warning(push)
#pragma warning(disable:4005)
#include <d3d12.h>
#include <d3d12Shader.h>
#include <d3dcompiler.h>
#include <dxgi1_4.h>
#pragma warning(pop)

NS_BEGIN

namespace NxRHI
{
	class VKGpuDevice;
	inline VkColorSpaceKHR ColorSpace2VKFormat(EColorSpace clrSpace)
	{
		switch (clrSpace)
		{
		case EColorSpace::SRGB_NONLINEAR:
			return VK_COLOR_SPACE_SRGB_NONLINEAR_KHR;
		case EColorSpace::EXTENDED_SRGB_LINEAR:
			return VK_COLOR_SPACE_EXTENDED_SRGB_LINEAR_EXT;
		default:
			ASSERT(false)
				return VK_COLOR_SPACE_MAX_ENUM_KHR;
		}
	}
	inline VkFormat Format2VKFormat(EPixelFormat fmt)
	{
		switch (fmt)
		{
		case EngineNS::PXF_UNKNOWN:
			return VK_FORMAT_UNDEFINED;
		case EngineNS::PXF_R16_FLOAT:
			return VK_FORMAT_R16_SFLOAT;
		case EngineNS::PXF_R16_UINT:
			return VK_FORMAT_R16_UINT;
		case EngineNS::PXF_R16_SINT:
			return VK_FORMAT_R16_SINT;
		case EngineNS::PXF_R16_UNORM:
		case EngineNS::PXF_R16_TYPELESS:
			return VK_FORMAT_R16_UNORM;
		case EngineNS::PXF_R16_SNORM:
			return VK_FORMAT_R16_SNORM;
		case EngineNS::PXF_R32_UINT:
		case EngineNS::PXF_R32_TYPELESS:
			return VK_FORMAT_R32_UINT;
		case EngineNS::PXF_R32_SINT:
			return VK_FORMAT_R32_SINT;
		case EngineNS::PXF_R32_FLOAT:
			return VK_FORMAT_R32_SFLOAT;
		case EngineNS::PXF_R8G8B8A8_SINT:
			return VK_FORMAT_R8G8B8A8_SINT;
		case EngineNS::PXF_R8G8B8A8_UINT:
			return VK_FORMAT_R8G8B8A8_UINT;
		case EngineNS::PXF_R8G8B8A8_UNORM:
			return VK_FORMAT_R8G8B8A8_UNORM;
		case EngineNS::PXF_R8G8B8A8_SNORM:
			return VK_FORMAT_R8G8B8A8_SNORM;
        case EngineNS::PXF_R8G8B8A8_UNORM_SRGB:
            return VK_FORMAT_R8G8B8A8_SRGB;
		case EngineNS::PXF_R16G16_TYPELESS:
		case EngineNS::PXF_R16G16_UINT:
			return VK_FORMAT_R16G16_UINT;
		case EngineNS::PXF_R16G16_SINT:
			return VK_FORMAT_R16G16_SINT;
		case EngineNS::PXF_R16G16_FLOAT:
			return VK_FORMAT_R16G16_SFLOAT;
		case EngineNS::PXF_R16G16_UNORM:
			return VK_FORMAT_R16G16_UNORM;
		case EngineNS::PXF_R16G16_SNORM:
			return VK_FORMAT_R16G16_SNORM;
		case EngineNS::PXF_R16G16B16A16_UINT:
			return VK_FORMAT_R16G16B16A16_UINT;
		case EngineNS::PXF_R16G16B16A16_SINT:
			return VK_FORMAT_R16G16B16A16_SINT;
		case EngineNS::PXF_R16G16B16A16_FLOAT:
			return VK_FORMAT_R16G16B16A16_SFLOAT;
		case EngineNS::PXF_R16G16B16A16_UNORM:
			return VK_FORMAT_R16G16B16A16_UNORM;
		case EngineNS::PXF_R16G16B16A16_SNORM:
			return VK_FORMAT_R16G16B16A16_SNORM;
		case EngineNS::PXF_R32G32B32A32_UINT:
			return VK_FORMAT_R32G32B32A32_UINT;
		case EngineNS::PXF_R32G32B32A32_SINT:
			return VK_FORMAT_R32G32B32A32_SINT;
		case EngineNS::PXF_R32G32B32A32_FLOAT:
			return VK_FORMAT_R32G32B32A32_SFLOAT;
		case EngineNS::PXF_R32G32B32_UINT:
			return VK_FORMAT_R32G32B32_UINT;
		case EngineNS::PXF_R32G32B32_SINT:
			return VK_FORMAT_R32G32B32_SINT;
		case EngineNS::PXF_R32G32B32_FLOAT:
			return VK_FORMAT_R32G32B32_SFLOAT;
		case EngineNS::PXF_R32G32_UINT:
			return VK_FORMAT_R32G32_UINT;
		case EngineNS::PXF_R32G32_SINT:
			return VK_FORMAT_R32G32_SINT;
		case EngineNS::PXF_R32G32_FLOAT:
			return VK_FORMAT_R32G32_SFLOAT;
		case EngineNS::PXF_R24_UNORM_X8_TYPELESS:
		case EngineNS::PXF_R24G8_TYPELESS:
		case EngineNS::PXF_X24_TYPELESS_G8_UINT:
		case EngineNS::PXF_D24_UNORM_S8_UINT:
			//return VK_FORMAT_D24_UNORM_S8_UINT;
            return VK_FORMAT_X8_D24_UNORM_PACK32;
		case EngineNS::PXF_D32_FLOAT:
			return VK_FORMAT_D32_SFLOAT;
		case EngineNS::PXF_D32_FLOAT_S8X24_UINT:
			return VK_FORMAT_D32_SFLOAT_S8_UINT;
		case EngineNS::PXF_D16_UNORM:
			return VK_FORMAT_D16_UNORM;
		case EngineNS::PXF_B8G8R8A8_UNORM:
			return VK_FORMAT_B8G8R8A8_UNORM;
        case EngineNS::PXF_B8G8R8A8_UNORM_SRGB:
            return VK_FORMAT_B8G8R8A8_SRGB;
        case EngineNS::PXF_R11G11B10_FLOAT:
			ASSERT(false);
			break;
		case EngineNS::PXF_R8G8_UNORM:
			return VK_FORMAT_R8G8_UNORM;
		case EngineNS::PXF_R8_UNORM:
			return VK_FORMAT_R8_UNORM;
		case EngineNS::PXF_R10G10B10A2_UNORM:
			return VK_FORMAT_A2R10G10B10_UNORM_PACK32;
		case EngineNS::PXF_R10G10B10A2_UINT:
			return VK_FORMAT_A2R10G10B10_UINT_PACK32;
		case EngineNS::PXF_R10G10B10A2_TYPELESS:
			return VK_FORMAT_A2R10G10B10_UINT_PACK32;
		default:
			ASSERT(false);
			break;
		}
		return VK_FORMAT_UNDEFINED;
	}
    inline EPixelFormat VKFormat2Format(VkFormat fmt)
    {
        switch (fmt)
        {
        case VK_FORMAT_UNDEFINED:
            return EPixelFormat::PXF_UNKNOWN;
        case VK_FORMAT_R4G4_UNORM_PACK8:
            break;
        case VK_FORMAT_R4G4B4A4_UNORM_PACK16:
            break;
        case VK_FORMAT_B4G4R4A4_UNORM_PACK16:
            return EPixelFormat::PXF_B4G4R4A4_UNORM;
        case VK_FORMAT_R5G6B5_UNORM_PACK16:
            break;
        case VK_FORMAT_B5G6R5_UNORM_PACK16:
            return EPixelFormat::PXF_B5G6R5_UNORM;
        case VK_FORMAT_R5G5B5A1_UNORM_PACK16:
            break;
        case VK_FORMAT_B5G5R5A1_UNORM_PACK16:
            break;
        case VK_FORMAT_A1R5G5B5_UNORM_PACK16:
            break;
        case VK_FORMAT_R8_UNORM:
            return EPixelFormat::PXF_R8_UNORM;
        case VK_FORMAT_R8_SNORM:
            return EPixelFormat::PXF_R8_SNORM;
        case VK_FORMAT_R8_USCALED:
            break;
        case VK_FORMAT_R8_SSCALED:
            break;
        case VK_FORMAT_R8_UINT:
            return EPixelFormat::PXF_R8_UINT;
        case VK_FORMAT_R8_SINT:
            return EPixelFormat::PXF_R8_SINT;
        case VK_FORMAT_R8_SRGB:
            break;
        case VK_FORMAT_R8G8_UNORM:
            return EPixelFormat::PXF_R8G8_UNORM;
        case VK_FORMAT_R8G8_SNORM:
            return EPixelFormat::PXF_R8G8_SNORM;
        case VK_FORMAT_R8G8_USCALED:
            break;
        case VK_FORMAT_R8G8_SSCALED:
            break;
        case VK_FORMAT_R8G8_UINT:
            return EPixelFormat::PXF_R8G8_UINT;
        case VK_FORMAT_R8G8_SINT:
            return EPixelFormat::PXF_R8G8_SINT;
        case VK_FORMAT_R8G8_SRGB:
            break;
        case VK_FORMAT_R8G8B8_UNORM:
            break;
        case VK_FORMAT_R8G8B8_SNORM:
            break;
        case VK_FORMAT_R8G8B8_USCALED:
            break;
        case VK_FORMAT_R8G8B8_SSCALED:
            break;
        case VK_FORMAT_R8G8B8_UINT:
            break;
        case VK_FORMAT_R8G8B8_SINT:
            break;
        case VK_FORMAT_R8G8B8_SRGB:
            break;
        case VK_FORMAT_B8G8R8_UNORM:
            break;
        case VK_FORMAT_B8G8R8_SNORM:
            break;
        case VK_FORMAT_B8G8R8_USCALED:
            break;
        case VK_FORMAT_B8G8R8_SSCALED:
            break;
        case VK_FORMAT_B8G8R8_UINT:
            break;
        case VK_FORMAT_B8G8R8_SINT:
            break;
        case VK_FORMAT_B8G8R8_SRGB:
            break;
        case VK_FORMAT_R8G8B8A8_UNORM:
            return EPixelFormat::PXF_R8G8B8A8_UNORM;
        case VK_FORMAT_R8G8B8A8_SNORM:
            return EPixelFormat::PXF_R8G8B8A8_SNORM;
        case VK_FORMAT_R8G8B8A8_USCALED:
            break;
        case VK_FORMAT_R8G8B8A8_SSCALED:
            break;
        case VK_FORMAT_R8G8B8A8_UINT:
            return EPixelFormat::PXF_R8G8B8A8_UINT;
        case VK_FORMAT_R8G8B8A8_SINT:
            return EPixelFormat::PXF_R8G8B8A8_SINT;
        case VK_FORMAT_R8G8B8A8_SRGB:
            return EPixelFormat::PXF_R8G8B8A8_UNORM_SRGB;
        case VK_FORMAT_B8G8R8A8_UNORM:
            return EPixelFormat::PXF_B8G8R8A8_UNORM;
        case VK_FORMAT_B8G8R8A8_SNORM:
            return EPixelFormat::PXF_B8G8R8A8_SNORM;
        case VK_FORMAT_B8G8R8A8_USCALED:
            break;
        case VK_FORMAT_B8G8R8A8_SSCALED:
            break;
        case VK_FORMAT_B8G8R8A8_UINT:
            return EPixelFormat::PXF_B8G8R8A8_UINT;
            break;
        case VK_FORMAT_B8G8R8A8_SINT:
            return EPixelFormat::PXF_B8G8R8A8_SINT;
        case VK_FORMAT_B8G8R8A8_SRGB:
            return EPixelFormat::PXF_B8G8R8A8_UNORM_SRGB;
        case VK_FORMAT_A8B8G8R8_UNORM_PACK32:
            break;
        case VK_FORMAT_A8B8G8R8_SNORM_PACK32:
            break;
        case VK_FORMAT_A8B8G8R8_USCALED_PACK32:
            break;
        case VK_FORMAT_A8B8G8R8_SSCALED_PACK32:
            break;
        case VK_FORMAT_A8B8G8R8_UINT_PACK32:
            break;
        case VK_FORMAT_A8B8G8R8_SINT_PACK32:
            break;
        case VK_FORMAT_A8B8G8R8_SRGB_PACK32:
            break;
        case VK_FORMAT_A2R10G10B10_UNORM_PACK32:
            return EPixelFormat::PXF_R10G10B10A2_UNORM;
        case VK_FORMAT_A2R10G10B10_SNORM_PACK32:
            return EPixelFormat::PXF_R10G10B10A2_SNORM;
        case VK_FORMAT_A2R10G10B10_USCALED_PACK32:
            break;
        case VK_FORMAT_A2R10G10B10_SSCALED_PACK32:
            break;
        case VK_FORMAT_A2R10G10B10_UINT_PACK32:
            return EPixelFormat::PXF_R10G10B10A2_UINT;
        case VK_FORMAT_A2R10G10B10_SINT_PACK32:
            return EPixelFormat::PXF_R10G10B10A2_SINT;
        case VK_FORMAT_A2B10G10R10_UNORM_PACK32:
            return EPixelFormat::PXF_B10G10R10A2_UNORM;
        case VK_FORMAT_A2B10G10R10_SNORM_PACK32:
            return EPixelFormat::PXF_B10G10R10A2_SNORM;
        case VK_FORMAT_A2B10G10R10_USCALED_PACK32:
            break;
        case VK_FORMAT_A2B10G10R10_SSCALED_PACK32:
            break;
        case VK_FORMAT_A2B10G10R10_UINT_PACK32:
            return EPixelFormat::PXF_B10G10R10A2_UINT;
        case VK_FORMAT_A2B10G10R10_SINT_PACK32:
            return EPixelFormat::PXF_B10G10R10A2_SINT;
        case VK_FORMAT_R16_UNORM:
            return EPixelFormat::PXF_R16_UNORM;
        case VK_FORMAT_R16_SNORM:
            return EPixelFormat::PXF_R16_SNORM;
        case VK_FORMAT_R16_USCALED:
            break;
        case VK_FORMAT_R16_SSCALED:
            break;
        case VK_FORMAT_R16_UINT:
            return EPixelFormat::PXF_R16_UINT;
        case VK_FORMAT_R16_SINT:
            return EPixelFormat::PXF_R16_SINT;
        case VK_FORMAT_R16_SFLOAT:
            return EPixelFormat::PXF_R16_FLOAT;
        case VK_FORMAT_R16G16_UNORM:
            return EPixelFormat::PXF_R16G16_UNORM;
        case VK_FORMAT_R16G16_SNORM:
            return EPixelFormat::PXF_R16G16_SNORM;
        case VK_FORMAT_R16G16_USCALED:
            break;
        case VK_FORMAT_R16G16_SSCALED:
            break;
        case VK_FORMAT_R16G16_UINT:
            return EPixelFormat::PXF_R16G16_UINT;
        case VK_FORMAT_R16G16_SINT:
            return EPixelFormat::PXF_R16G16_SINT;
        case VK_FORMAT_R16G16_SFLOAT:
            return EPixelFormat::PXF_R16G16_FLOAT;
        case VK_FORMAT_R16G16B16_UNORM:
            break;
        case VK_FORMAT_R16G16B16_SNORM:
            break;
        case VK_FORMAT_R16G16B16_USCALED:
            break;
        case VK_FORMAT_R16G16B16_SSCALED:
            break;
        case VK_FORMAT_R16G16B16_UINT:
            break;
        case VK_FORMAT_R16G16B16_SINT:
            break;
        case VK_FORMAT_R16G16B16_SFLOAT:
            break;
        case VK_FORMAT_R16G16B16A16_UNORM:
            return EPixelFormat::PXF_R16G16B16A16_UNORM;
        case VK_FORMAT_R16G16B16A16_SNORM:
            return EPixelFormat::PXF_R16G16B16A16_SNORM;
        case VK_FORMAT_R16G16B16A16_USCALED:
            break;
        case VK_FORMAT_R16G16B16A16_SSCALED:
            break;
        case VK_FORMAT_R16G16B16A16_UINT:
            return EPixelFormat::PXF_R16G16B16A16_UINT;
        case VK_FORMAT_R16G16B16A16_SINT:
            return EPixelFormat::PXF_R16G16B16A16_SINT;
        case VK_FORMAT_R16G16B16A16_SFLOAT:
            return EPixelFormat::PXF_R16G16B16A16_FLOAT;
        case VK_FORMAT_R32_UINT:
            return EPixelFormat::PXF_R32_UINT;
        case VK_FORMAT_R32_SINT:
            return EPixelFormat::PXF_R32_SINT;
        case VK_FORMAT_R32_SFLOAT:
            return EPixelFormat::PXF_R32_FLOAT;
        case VK_FORMAT_R32G32_UINT:
            return EPixelFormat::PXF_R32G32_UINT;
        case VK_FORMAT_R32G32_SINT:
            return EPixelFormat::PXF_R32G32_SINT;
        case VK_FORMAT_R32G32_SFLOAT:
            return EPixelFormat::PXF_R32G32_FLOAT;
        case VK_FORMAT_R32G32B32_UINT:
            return EPixelFormat::PXF_R32G32B32_UINT;
        case VK_FORMAT_R32G32B32_SINT:
            return EPixelFormat::PXF_R32G32B32_SINT;
        case VK_FORMAT_R32G32B32_SFLOAT:
            return EPixelFormat::PXF_R32G32B32_FLOAT;
        case VK_FORMAT_R32G32B32A32_UINT:
            return EPixelFormat::PXF_R32G32B32A32_UINT;
        case VK_FORMAT_R32G32B32A32_SINT:
            return EPixelFormat::PXF_R32G32B32A32_SINT;
        case VK_FORMAT_R32G32B32A32_SFLOAT:
            return EPixelFormat::PXF_R32G32B32A32_FLOAT;
        case VK_FORMAT_R64_UINT:
            break;
        case VK_FORMAT_R64_SINT:
            break;
        case VK_FORMAT_R64_SFLOAT:
            break;
        case VK_FORMAT_R64G64_UINT:
            break;
        case VK_FORMAT_R64G64_SINT:
            break;
        case VK_FORMAT_R64G64_SFLOAT:
            break;
        case VK_FORMAT_R64G64B64_UINT:
            break;
        case VK_FORMAT_R64G64B64_SINT:
            break;
        case VK_FORMAT_R64G64B64_SFLOAT:
            break;
        case VK_FORMAT_R64G64B64A64_UINT:
            break;
        case VK_FORMAT_R64G64B64A64_SINT:
            break;
        case VK_FORMAT_R64G64B64A64_SFLOAT:
            break;
        case VK_FORMAT_B10G11R11_UFLOAT_PACK32:
            break;
        case VK_FORMAT_E5B9G9R9_UFLOAT_PACK32:
            break;
        case VK_FORMAT_D16_UNORM:
            return EPixelFormat::PXF_D16_UNORM;
        case VK_FORMAT_X8_D24_UNORM_PACK32:
            return EPixelFormat::PXF_D24_UNORM_S8_UINT;
        case VK_FORMAT_D32_SFLOAT:
            return EPixelFormat::PXF_D32_FLOAT;
        case VK_FORMAT_S8_UINT:
            break;
        case VK_FORMAT_D16_UNORM_S8_UINT:
            break;
        case VK_FORMAT_D24_UNORM_S8_UINT:
            return EPixelFormat::PXF_D24_UNORM_S8_UINT;
        case VK_FORMAT_D32_SFLOAT_S8_UINT:
            break;
        case VK_FORMAT_BC1_RGB_UNORM_BLOCK:
            break;
        case VK_FORMAT_BC1_RGB_SRGB_BLOCK:
            break;
        case VK_FORMAT_BC1_RGBA_UNORM_BLOCK:
            break;
        case VK_FORMAT_BC1_RGBA_SRGB_BLOCK:
            break;
        case VK_FORMAT_BC2_UNORM_BLOCK:
            break;
        case VK_FORMAT_BC2_SRGB_BLOCK:
            break;
        case VK_FORMAT_BC3_UNORM_BLOCK:
            break;
        case VK_FORMAT_BC3_SRGB_BLOCK:
            break;
        case VK_FORMAT_BC4_UNORM_BLOCK:
            break;
        case VK_FORMAT_BC4_SNORM_BLOCK:
            break;
        case VK_FORMAT_BC5_UNORM_BLOCK:
            break;
        case VK_FORMAT_BC5_SNORM_BLOCK:
            break;
        case VK_FORMAT_BC6H_UFLOAT_BLOCK:
            break;
        case VK_FORMAT_BC6H_SFLOAT_BLOCK:
            break;
        case VK_FORMAT_BC7_UNORM_BLOCK:
            break;
        case VK_FORMAT_BC7_SRGB_BLOCK:
            break;
        case VK_FORMAT_ETC2_R8G8B8_UNORM_BLOCK:
            break;
        case VK_FORMAT_ETC2_R8G8B8_SRGB_BLOCK:
            break;
        case VK_FORMAT_ETC2_R8G8B8A1_UNORM_BLOCK:
            break;
        case VK_FORMAT_ETC2_R8G8B8A1_SRGB_BLOCK:
            break;
        case VK_FORMAT_ETC2_R8G8B8A8_UNORM_BLOCK:
            break;
        case VK_FORMAT_ETC2_R8G8B8A8_SRGB_BLOCK:
            break;
        case VK_FORMAT_EAC_R11_UNORM_BLOCK:
            break;
        case VK_FORMAT_EAC_R11_SNORM_BLOCK:
            break;
        case VK_FORMAT_EAC_R11G11_UNORM_BLOCK:
            break;
        case VK_FORMAT_EAC_R11G11_SNORM_BLOCK:
            break;
        case VK_FORMAT_ASTC_4x4_UNORM_BLOCK:
            break;
        case VK_FORMAT_ASTC_4x4_SRGB_BLOCK:
            break;
        case VK_FORMAT_ASTC_5x4_UNORM_BLOCK:
            break;
        case VK_FORMAT_ASTC_5x4_SRGB_BLOCK:
            break;
        case VK_FORMAT_ASTC_5x5_UNORM_BLOCK:
            break;
        case VK_FORMAT_ASTC_5x5_SRGB_BLOCK:
            break;
        case VK_FORMAT_ASTC_6x5_UNORM_BLOCK:
            break;
        case VK_FORMAT_ASTC_6x5_SRGB_BLOCK:
            break;
        case VK_FORMAT_ASTC_6x6_UNORM_BLOCK:
            break;
        case VK_FORMAT_ASTC_6x6_SRGB_BLOCK:
            break;
        case VK_FORMAT_ASTC_8x5_UNORM_BLOCK:
            break;
        case VK_FORMAT_ASTC_8x5_SRGB_BLOCK:
            break;
        case VK_FORMAT_ASTC_8x6_UNORM_BLOCK:
            break;
        case VK_FORMAT_ASTC_8x6_SRGB_BLOCK:
            break;
        case VK_FORMAT_ASTC_8x8_UNORM_BLOCK:
            break;
        case VK_FORMAT_ASTC_8x8_SRGB_BLOCK:
            break;
        case VK_FORMAT_ASTC_10x5_UNORM_BLOCK:
            break;
        case VK_FORMAT_ASTC_10x5_SRGB_BLOCK:
            break;
        case VK_FORMAT_ASTC_10x6_UNORM_BLOCK:
            break;
        case VK_FORMAT_ASTC_10x6_SRGB_BLOCK:
            break;
        case VK_FORMAT_ASTC_10x8_UNORM_BLOCK:
            break;
        case VK_FORMAT_ASTC_10x8_SRGB_BLOCK:
            break;
        case VK_FORMAT_ASTC_10x10_UNORM_BLOCK:
            break;
        case VK_FORMAT_ASTC_10x10_SRGB_BLOCK:
            break;
        case VK_FORMAT_ASTC_12x10_UNORM_BLOCK:
            break;
        case VK_FORMAT_ASTC_12x10_SRGB_BLOCK:
            break;
        case VK_FORMAT_ASTC_12x12_UNORM_BLOCK:
            break;
        case VK_FORMAT_ASTC_12x12_SRGB_BLOCK:
            break;
        case VK_FORMAT_G8B8G8R8_422_UNORM:
            break;
        case VK_FORMAT_B8G8R8G8_422_UNORM:
            break;
        case VK_FORMAT_G8_B8_R8_3PLANE_420_UNORM:
            break;
        case VK_FORMAT_G8_B8R8_2PLANE_420_UNORM:
            break;
        case VK_FORMAT_G8_B8_R8_3PLANE_422_UNORM:
            break;
        case VK_FORMAT_G8_B8R8_2PLANE_422_UNORM:
            break;
        case VK_FORMAT_G8_B8_R8_3PLANE_444_UNORM:
            break;
        case VK_FORMAT_R10X6_UNORM_PACK16:
            break;
        case VK_FORMAT_R10X6G10X6_UNORM_2PACK16:
            break;
        case VK_FORMAT_R10X6G10X6B10X6A10X6_UNORM_4PACK16:
            break;
        case VK_FORMAT_G10X6B10X6G10X6R10X6_422_UNORM_4PACK16:
            break;
        case VK_FORMAT_B10X6G10X6R10X6G10X6_422_UNORM_4PACK16:
            break;
        case VK_FORMAT_G10X6_B10X6_R10X6_3PLANE_420_UNORM_3PACK16:
            break;
        case VK_FORMAT_G10X6_B10X6R10X6_2PLANE_420_UNORM_3PACK16:
            break;
        case VK_FORMAT_G10X6_B10X6_R10X6_3PLANE_422_UNORM_3PACK16:
            break;
        case VK_FORMAT_G10X6_B10X6R10X6_2PLANE_422_UNORM_3PACK16:
            break;
        case VK_FORMAT_G10X6_B10X6_R10X6_3PLANE_444_UNORM_3PACK16:
            break;
        case VK_FORMAT_R12X4_UNORM_PACK16:
            break;
        case VK_FORMAT_R12X4G12X4_UNORM_2PACK16:
            break;
        case VK_FORMAT_R12X4G12X4B12X4A12X4_UNORM_4PACK16:
            break;
        case VK_FORMAT_G12X4B12X4G12X4R12X4_422_UNORM_4PACK16:
            break;
        case VK_FORMAT_B12X4G12X4R12X4G12X4_422_UNORM_4PACK16:
            break;
        case VK_FORMAT_G12X4_B12X4_R12X4_3PLANE_420_UNORM_3PACK16:
            break;
        case VK_FORMAT_G12X4_B12X4R12X4_2PLANE_420_UNORM_3PACK16:
            break;
        case VK_FORMAT_G12X4_B12X4_R12X4_3PLANE_422_UNORM_3PACK16:
            break;
        case VK_FORMAT_G12X4_B12X4R12X4_2PLANE_422_UNORM_3PACK16:
            break;
        case VK_FORMAT_G12X4_B12X4_R12X4_3PLANE_444_UNORM_3PACK16:
            break;
        case VK_FORMAT_G16B16G16R16_422_UNORM:
            break;
        case VK_FORMAT_B16G16R16G16_422_UNORM:
            break;
        case VK_FORMAT_G16_B16_R16_3PLANE_420_UNORM:
            break;
        case VK_FORMAT_G16_B16R16_2PLANE_420_UNORM:
            break;
        case VK_FORMAT_G16_B16_R16_3PLANE_422_UNORM:
            break;
        case VK_FORMAT_G16_B16R16_2PLANE_422_UNORM:
            break;
        case VK_FORMAT_G16_B16_R16_3PLANE_444_UNORM:
            break;
        case VK_FORMAT_PVRTC1_2BPP_UNORM_BLOCK_IMG:
            break;
        case VK_FORMAT_PVRTC1_4BPP_UNORM_BLOCK_IMG:
            break;
        case VK_FORMAT_PVRTC2_2BPP_UNORM_BLOCK_IMG:
            break;
        case VK_FORMAT_PVRTC2_4BPP_UNORM_BLOCK_IMG:
            break;
        case VK_FORMAT_PVRTC1_2BPP_SRGB_BLOCK_IMG:
            break;
        case VK_FORMAT_PVRTC1_4BPP_SRGB_BLOCK_IMG:
            break;
        case VK_FORMAT_PVRTC2_2BPP_SRGB_BLOCK_IMG:
            break;
        case VK_FORMAT_PVRTC2_4BPP_SRGB_BLOCK_IMG:
            break;
        case VK_FORMAT_ASTC_4x4_SFLOAT_BLOCK_EXT:
            break;
        case VK_FORMAT_ASTC_5x4_SFLOAT_BLOCK_EXT:
            break;
        case VK_FORMAT_ASTC_5x5_SFLOAT_BLOCK_EXT:
            break;
        case VK_FORMAT_ASTC_6x5_SFLOAT_BLOCK_EXT:
            break;
        case VK_FORMAT_ASTC_6x6_SFLOAT_BLOCK_EXT:
            break;
        case VK_FORMAT_ASTC_8x5_SFLOAT_BLOCK_EXT:
            break;
        case VK_FORMAT_ASTC_8x6_SFLOAT_BLOCK_EXT:
            break;
        case VK_FORMAT_ASTC_8x8_SFLOAT_BLOCK_EXT:
            break;
        case VK_FORMAT_ASTC_10x5_SFLOAT_BLOCK_EXT:
            break;
        case VK_FORMAT_ASTC_10x6_SFLOAT_BLOCK_EXT:
            break;
        case VK_FORMAT_ASTC_10x8_SFLOAT_BLOCK_EXT:
            break;
        case VK_FORMAT_ASTC_10x10_SFLOAT_BLOCK_EXT:
            break;
        case VK_FORMAT_ASTC_12x10_SFLOAT_BLOCK_EXT:
            break;
        case VK_FORMAT_ASTC_12x12_SFLOAT_BLOCK_EXT:
            break;
        case VK_FORMAT_G8_B8R8_2PLANE_444_UNORM_EXT:
            break;
        case VK_FORMAT_G10X6_B10X6R10X6_2PLANE_444_UNORM_3PACK16_EXT:
            break;
        case VK_FORMAT_G12X4_B12X4R12X4_2PLANE_444_UNORM_3PACK16_EXT:
            break;
        case VK_FORMAT_G16_B16R16_2PLANE_444_UNORM_EXT:
            break;
        case VK_FORMAT_A4R4G4B4_UNORM_PACK16_EXT:
            break;
        case VK_FORMAT_A4B4G4R4_UNORM_PACK16_EXT:
            break;
        case VK_FORMAT_MAX_ENUM:
            break;
        default:
            break;
        }

        return EngineNS::PXF_UNKNOWN;
    }

    inline UINT InputLayoutSemanticToVK(const VNameString& sematic, UINT index)
    {
        if (sematic == "POSITION")
        {
            if (index == 0)
                return 0;
        }
        else if (sematic == "NORMAL")
        {
            if (index == 0)
                return 1;
        }
        else if (sematic == "COLOR")
        {
            if (index == 0)
                return 3;
        }
        else if (sematic == "TEXCOORD")
        {
            switch (index)
            {
            case 0:
                return 2;
            case 1:
                return 4;
            case 2:
                return 5;
            case 3:
                return 6;
            case 4:
                return 7;
            case 5:
                return 8;
            case 6:
                return 9;
            case 7:
                return 10;
            case 8:
                return 11;
            case 9:
                return 12;
            case 10:
                return 13;
            case 11:
                return 14;
            case 12:
                return 15;
            default:
                break;
            }
        }
        else if (sematic == "SV_VertexID")
        {
            if (index == 0)
                return 16;
        }
        else if (sematic == "SV_InstanceID")
        {
            if (index == 0)
                return 17;
        }
        return 0xFFFFFFFF;
    }
    static EVertexStreamType GetStreamTypeByVKBinding(UINT inputSlot)
    {
        switch (inputSlot)
        {
        case 0:
            return VST_Position;
        case 1:
            return VST_Normal;
        case 2:
            return VST_Tangent;
        case 3:
            return VST_Color;
        case 4:
            return VST_UV;
        case 5:
            return VST_LightMap;
        case 6:
            return VST_SkinIndex;
        case 7:
            return VST_SkinWeight;
        case 8:
            return VST_TerrainIndex;
        case 9:
            return VST_TerrainGradient;
        case 10:
            return VST_InstPos;
        case 11:
            return VST_InstQuat;
        case 12:
            return VST_InstScale;
        case 13:
            return VST_F4_1;
        case 14:
            return VST_F4_2;
        case 15:
            return VST_F4_3;
        default:
            break;
        }

        return VST_Number;
    }
	
	struct VKGpuHeap : public IGpuHeap
	{
		VkDeviceMemory              Memory;
		virtual UINT64 GetGPUVirtualAddress() override
		{
			return 0;
		}
		virtual void* GetHWBuffer() override{
			return Memory;
		}
	};
	struct FVKDefaultGpuMemory : public FGpuMemory
	{
		~FVKDefaultGpuMemory();
		VKGpuHeap* GetVKGpuHeap() {
			return (VKGpuHeap*)GpuHeap;
		}
		virtual void FreeMemory();
	};

	class VKGpuDefaultMemAllocator : public IGpuMemAllocator
	{
	public:
		UINT							mMemTypeIndex = -1;
		AutoRef<FGpuMemory> Alloc(IGpuDevice* device, UINT typeIndex, UINT64 size);
		virtual AutoRef<FGpuMemory> Alloc(IGpuDevice* device, UINT64 size);
		virtual void Free(FGpuMemory* memory);
	};

	class VKGpuPooledMemAllocator : public IGpuPooledMemAllocator
	{
	public:
		UINT64							mBatchPoolSize = 64 * 1024 * 1024;
		UINT							mMemTypeIndex = -1;
		virtual UINT GetBatchCount(UINT64 size) {
			return (UINT)(mBatchPoolSize / size);
		}
		virtual IGpuHeap* CreateGpuHeap(IGpuDevice* device, UINT64 size, UINT count) override;
	};
	class VKGpuLinearMemAllocator : public IGpuLinearMemAllocator
	{
	public:
		virtual IGpuHeap* CreateGpuHeap(IGpuDevice* device, UINT64 size) override;
	public:
		UINT							mAlignment = 1;
		UINT							mMemTypeIndex = -1;
	};
}

NS_END