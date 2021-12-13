#pragma once

#include "../PreHead.h"

#ifdef PLATFORM_WIN
#define VK_USE_PLATFORM_WIN32_KHR
#endif

#include <vulkan/vulkan.h>

NS_BEGIN

inline VkColorSpaceKHR ColorSpace2VKFormat(EColorSpace clrSpace)
{
    switch (clrSpace)
    {
    case EngineNS::COLOR_SPACE_SRGB_NONLINEAR:
        return VK_COLOR_SPACE_SRGB_NONLINEAR_KHR;
    case EngineNS::COLOR_SPACE_EXTENDED_SRGB_LINEAR:
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
        return VK_FORMAT_D24_UNORM_S8_UINT;
    case EngineNS::PXF_D32_FLOAT:
        return VK_FORMAT_D32_SFLOAT;
    case EngineNS::PXF_D32_FLOAT_S8X24_UINT:
        return VK_FORMAT_D32_SFLOAT_S8_UINT;
    case EngineNS::PXF_D16_UNORM:
        return VK_FORMAT_D16_UNORM;
    case EngineNS::PXF_B8G8R8A8_UNORM:
        return VK_FORMAT_B8G8R8A8_UNORM;
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

inline EPixelFormat VkFormat2Format(VkFormat fmt)
{
    switch (fmt)
    {
    case VK_FORMAT_UNDEFINED:
        return EngineNS::PXF_UNKNOWN;
    case VK_FORMAT_R4G4_UNORM_PACK8:
        break;
    case VK_FORMAT_R4G4B4A4_UNORM_PACK16:
        break;
    case VK_FORMAT_B4G4R4A4_UNORM_PACK16:
        break;
    case VK_FORMAT_R5G6B5_UNORM_PACK16:
        break;
    case VK_FORMAT_B5G6R5_UNORM_PACK16:
        return EngineNS::PXF_B5G6R5_UNORM;
    case VK_FORMAT_R5G5B5A1_UNORM_PACK16:
        break;
    case VK_FORMAT_B5G5R5A1_UNORM_PACK16:
        break;
    case VK_FORMAT_A1R5G5B5_UNORM_PACK16:
        break;
    case VK_FORMAT_R8_UNORM:
        break;
    case VK_FORMAT_R8_SNORM:
        break;
    case VK_FORMAT_R8_USCALED:
        break;
    case VK_FORMAT_R8_SSCALED:
        break;
    case VK_FORMAT_R8_UINT:
        break;
    case VK_FORMAT_R8_SINT:
        break;
    case VK_FORMAT_R8_SRGB:
        break;
    case VK_FORMAT_R8G8_UNORM:
        break;
    case VK_FORMAT_R8G8_SNORM:
        break;
    case VK_FORMAT_R8G8_USCALED:
        break;
    case VK_FORMAT_R8G8_SSCALED:
        break;
    case VK_FORMAT_R8G8_UINT:
        break;
    case VK_FORMAT_R8G8_SINT:
        break;
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
        break;
    case VK_FORMAT_R8G8B8A8_SNORM:
        break;
    case VK_FORMAT_R8G8B8A8_USCALED:
        break;
    case VK_FORMAT_R8G8B8A8_SSCALED:
        break;
    case VK_FORMAT_R8G8B8A8_UINT:
        break;
    case VK_FORMAT_R8G8B8A8_SINT:
        break;
    case VK_FORMAT_R8G8B8A8_SRGB:
        break;
    case VK_FORMAT_B8G8R8A8_UNORM:
        break;
    case VK_FORMAT_B8G8R8A8_SNORM:
        break;
    case VK_FORMAT_B8G8R8A8_USCALED:
        break;
    case VK_FORMAT_B8G8R8A8_SSCALED:
        break;
    case VK_FORMAT_B8G8R8A8_UINT:
        break;
    case VK_FORMAT_B8G8R8A8_SINT:
        break;
    case VK_FORMAT_B8G8R8A8_SRGB:
        break;
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
        break;
    case VK_FORMAT_A2R10G10B10_SNORM_PACK32:
        break;
    case VK_FORMAT_A2R10G10B10_USCALED_PACK32:
        break;
    case VK_FORMAT_A2R10G10B10_SSCALED_PACK32:
        break;
    case VK_FORMAT_A2R10G10B10_UINT_PACK32:
        break;
    case VK_FORMAT_A2R10G10B10_SINT_PACK32:
        break;
    case VK_FORMAT_A2B10G10R10_UNORM_PACK32:
        break;
    case VK_FORMAT_A2B10G10R10_SNORM_PACK32:
        break;
    case VK_FORMAT_A2B10G10R10_USCALED_PACK32:
        break;
    case VK_FORMAT_A2B10G10R10_SSCALED_PACK32:
        break;
    case VK_FORMAT_A2B10G10R10_UINT_PACK32:
        break;
    case VK_FORMAT_A2B10G10R10_SINT_PACK32:
        break;
    case VK_FORMAT_R16_UNORM:
        break;
    case VK_FORMAT_R16_SNORM:
        break;
    case VK_FORMAT_R16_USCALED:
        break;
    case VK_FORMAT_R16_SSCALED:
        break;
    case VK_FORMAT_R16_UINT:
        break;
    case VK_FORMAT_R16_SINT:
        break;
    case VK_FORMAT_R16_SFLOAT:
        break;
    case VK_FORMAT_R16G16_UNORM:
        break;
    case VK_FORMAT_R16G16_SNORM:
        break;
    case VK_FORMAT_R16G16_USCALED:
        break;
    case VK_FORMAT_R16G16_SSCALED:
        break;
    case VK_FORMAT_R16G16_UINT:
        break;
    case VK_FORMAT_R16G16_SINT:
        break;
    case VK_FORMAT_R16G16_SFLOAT:
        break;
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
        break;
    case VK_FORMAT_R16G16B16A16_SNORM:
        break;
    case VK_FORMAT_R16G16B16A16_USCALED:
        break;
    case VK_FORMAT_R16G16B16A16_SSCALED:
        break;
    case VK_FORMAT_R16G16B16A16_UINT:
        break;
    case VK_FORMAT_R16G16B16A16_SINT:
        break;
    case VK_FORMAT_R16G16B16A16_SFLOAT:
        break;
    case VK_FORMAT_R32_UINT:
        break;
    case VK_FORMAT_R32_SINT:
        break;
    case VK_FORMAT_R32_SFLOAT:
        break;
    case VK_FORMAT_R32G32_UINT:
        break;
    case VK_FORMAT_R32G32_SINT:
        break;
    case VK_FORMAT_R32G32_SFLOAT:
        break;
    case VK_FORMAT_R32G32B32_UINT:
        break;
    case VK_FORMAT_R32G32B32_SINT:
        break;
    case VK_FORMAT_R32G32B32_SFLOAT:
        break;
    case VK_FORMAT_R32G32B32A32_UINT:
        break;
    case VK_FORMAT_R32G32B32A32_SINT:
        break;
    case VK_FORMAT_R32G32B32A32_SFLOAT:
        break;
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
        break;
    case VK_FORMAT_X8_D24_UNORM_PACK32:
        break;
    case VK_FORMAT_D32_SFLOAT:
        break;
    case VK_FORMAT_S8_UINT:
        break;
    case VK_FORMAT_D16_UNORM_S8_UINT:
        break;
    case VK_FORMAT_D24_UNORM_S8_UINT:
        break;
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

inline VkPolygonMode FillMode2VKFillMode(EFillMode mode)
{
    switch (mode)
    {
    case EngineNS::FMD_WIREFRAME:
        return VkPolygonMode::VK_POLYGON_MODE_LINE;
    case EngineNS::FMD_SOLID:
        return VkPolygonMode::VK_POLYGON_MODE_FILL;
    default:
        return VkPolygonMode::VK_POLYGON_MODE_FILL;
    }
}

inline VkCullModeFlagBits CullMode2VKCullMode(ECullMode mode)
{
    switch (mode)
    {
    case EngineNS::CMD_NONE:
        return VkCullModeFlagBits::VK_CULL_MODE_NONE;
    case EngineNS::CMD_FRONT:
        return VkCullModeFlagBits::VK_CULL_MODE_FRONT_BIT;
    case EngineNS::CMD_BACK:
        return VkCullModeFlagBits::VK_CULL_MODE_BACK_BIT;
    default:
        return VkCullModeFlagBits::VK_CULL_MODE_BACK_BIT;
    }
}

inline VkBlendOp BlendOp2VKBlendOp(EBlendOp op)
{
    switch (op)
    {
    case EngineNS::BLDOP_ADD:
        return VkBlendOp::VK_BLEND_OP_ADD;
    case EngineNS::BLDOP_SUBTRACT:
        return VkBlendOp::VK_BLEND_OP_SUBTRACT;
    case EngineNS::BLDOP_REV_SUBTRACT:
        return VkBlendOp::VK_BLEND_OP_REVERSE_SUBTRACT;
    case EngineNS::BLDOP_MIN:
        return VkBlendOp::VK_BLEND_OP_MIN;
    case EngineNS::BLDOP_MAX:
        return VkBlendOp::VK_BLEND_OP_MAX;
    default:
        return VkBlendOp::VK_BLEND_OP_ADD;
    }
}

inline VkBlendFactor BlendFactor2VKBlendFactor(EBlend mode)
{
    switch (mode)
    {
    case EngineNS::BLD_ZERO:
        return VkBlendFactor::VK_BLEND_FACTOR_ZERO;
    case EngineNS::BLD_ONE:
        return VkBlendFactor::VK_BLEND_FACTOR_ONE;
    case EngineNS::BLD_SRC_COLOR:
        return VkBlendFactor::VK_BLEND_FACTOR_SRC_COLOR;
    case EngineNS::BLD_INV_SRC_COLOR:
        return VkBlendFactor::VK_BLEND_FACTOR_ONE_MINUS_SRC_COLOR;
    case EngineNS::BLD_SRC_ALPHA:
        return VkBlendFactor::VK_BLEND_FACTOR_SRC_ALPHA;
    case EngineNS::BLD_INV_SRC_ALPHA:
        return VkBlendFactor::VK_BLEND_FACTOR_ONE_MINUS_SRC_ALPHA;
    case EngineNS::BLD_DEST_ALPHA:
        return VkBlendFactor::VK_BLEND_FACTOR_DST_COLOR;
    case EngineNS::BLD_INV_DEST_ALPHA:
        return VkBlendFactor::VK_BLEND_FACTOR_ONE_MINUS_DST_ALPHA;
    case EngineNS::BLD_DEST_COLOR:
        return VkBlendFactor::VK_BLEND_FACTOR_DST_COLOR;
    case EngineNS::BLD_INV_DEST_COLOR:
        return VkBlendFactor::VK_BLEND_FACTOR_ONE_MINUS_DST_COLOR;
    case EngineNS::BLD_SRC_ALPHA_SAT:
        return VkBlendFactor::VK_BLEND_FACTOR_SRC_ALPHA_SATURATE;
    case EngineNS::BLD_BLEND_FACTOR:
        return VkBlendFactor::VK_BLEND_FACTOR_CONSTANT_ALPHA;
    case EngineNS::BLD_INV_BLEND_FACTOR:
        return VkBlendFactor::VK_BLEND_FACTOR_ONE_MINUS_CONSTANT_ALPHA;
    case EngineNS::BLD_SRC1_COLOR:
        return VkBlendFactor::VK_BLEND_FACTOR_SRC1_COLOR;
    case EngineNS::BLD_INV_SRC1_COLOR:
        return VkBlendFactor::VK_BLEND_FACTOR_ONE_MINUS_SRC1_COLOR;
    case EngineNS::BLD_SRC1_ALPHA:
        return VkBlendFactor::VK_BLEND_FACTOR_SRC1_ALPHA;
    case EngineNS::BLD_INV_SRC1_ALPHA:
        return VkBlendFactor::VK_BLEND_FACTOR_ONE_MINUS_SRC1_ALPHA;
    default:
        return VkBlendFactor::VK_BLEND_FACTOR_ONE;
    }
}

inline VkCompareOp CompareOp2VKCompareOp(EComparisionMode mode)
{
    switch (mode)
    {
    case EngineNS::CMP_NEVER:
        return VkCompareOp::VK_COMPARE_OP_NEVER;
    case EngineNS::CMP_LESS:
        return VkCompareOp::VK_COMPARE_OP_LESS;
    case EngineNS::CMP_EQUAL:
        return VkCompareOp::VK_COMPARE_OP_EQUAL;
    case EngineNS::CMP_LESS_EQUAL:
        return VkCompareOp::VK_COMPARE_OP_LESS_OR_EQUAL;
    case EngineNS::CMP_GREATER:
        return VkCompareOp::VK_COMPARE_OP_GREATER;
    case EngineNS::CMP_NOT_EQUAL:
        return VkCompareOp::VK_COMPARE_OP_NOT_EQUAL;
    case EngineNS::CMP_GREATER_EQUAL:
        return VkCompareOp::VK_COMPARE_OP_GREATER_OR_EQUAL;
    case EngineNS::CMP_ALWAYS:
        return VkCompareOp::VK_COMPARE_OP_ALWAYS;
    default:
        return VkCompareOp::VK_COMPARE_OP_ALWAYS;
    }
}

inline VkStencilOp StencilOp2VKStencilOp(EStencilOp op)
{
    switch (op)
    {
    case EngineNS::STOP_KEEP:
        return VkStencilOp::VK_STENCIL_OP_KEEP;
    case EngineNS::STOP_ZERO:
        return VkStencilOp::VK_STENCIL_OP_ZERO;
    case EngineNS::STOP_REPLACE:
        return VkStencilOp::VK_STENCIL_OP_REPLACE;
    case EngineNS::STOP_INCR_SAT:
        return VkStencilOp::VK_STENCIL_OP_INCREMENT_AND_CLAMP;
    case EngineNS::STOP_DECR_SAT:
        return VkStencilOp::VK_STENCIL_OP_DECREMENT_AND_CLAMP;
    case EngineNS::STOP_INVERT:
        return VkStencilOp::VK_STENCIL_OP_INVERT;
    case EngineNS::STOP_INCR:
        return VkStencilOp::VK_STENCIL_OP_INCREMENT_AND_WRAP;
    case EngineNS::STOP_DECR:
        return VkStencilOp::VK_STENCIL_OP_DECREMENT_AND_WRAP;
    default:
        return VkStencilOp::VK_STENCIL_OP_KEEP;
    }
}

inline VkSamplerAddressMode AddressMode2VKAddressMode(EAddressMode mode)
{
    switch (mode)
    {
    case EngineNS::ADM_WRAP:
        return VkSamplerAddressMode::VK_SAMPLER_ADDRESS_MODE_REPEAT;
    case EngineNS::ADM_MIRROR:
        return VkSamplerAddressMode::VK_SAMPLER_ADDRESS_MODE_MIRRORED_REPEAT;
    case EngineNS::ADM_CLAMP:
        return VkSamplerAddressMode::VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_EDGE;
    case EngineNS::ADM_BORDER:
        return VkSamplerAddressMode::VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_BORDER;
    case EngineNS::ADM_MIRROR_ONCE:
        return VkSamplerAddressMode::VK_SAMPLER_ADDRESS_MODE_MIRROR_CLAMP_TO_EDGE;
    default:
        return VkSamplerAddressMode::VK_SAMPLER_ADDRESS_MODE_REPEAT;
    }
}

inline VkAttachmentLoadOp FrameBufferLoadAction2VK(FrameBufferLoadAction action)
{
    switch (action)
    {
    case EngineNS::LoadActionDontCare:
        return VkAttachmentLoadOp::VK_ATTACHMENT_LOAD_OP_DONT_CARE;
    case EngineNS::LoadActionLoad:
        return VkAttachmentLoadOp::VK_ATTACHMENT_LOAD_OP_LOAD;
    case EngineNS::LoadActionClear:
        return VkAttachmentLoadOp::VK_ATTACHMENT_LOAD_OP_CLEAR;
    default:
        return VkAttachmentLoadOp::VK_ATTACHMENT_LOAD_OP_DONT_CARE;
    }
}

inline VkAttachmentStoreOp FrameBufferStoreAction2VK(FrameBufferStoreAction action)
{
    switch (action)
    {
    case EngineNS::StoreActionDontCare:
        return VkAttachmentStoreOp::VK_ATTACHMENT_STORE_OP_DONT_CARE;
    case EngineNS::StoreActionStore:
        return VkAttachmentStoreOp::VK_ATTACHMENT_STORE_OP_STORE;
    case EngineNS::StoreActionMultisampleResolve:
        return VkAttachmentStoreOp::VK_ATTACHMENT_STORE_OP_MAX_ENUM;
    case EngineNS::StoreActionStoreAndMultisampleResolve:
        return VkAttachmentStoreOp::VK_ATTACHMENT_STORE_OP_MAX_ENUM;
    case EngineNS::StoreActionUnknown:
        return VkAttachmentStoreOp::VK_ATTACHMENT_STORE_OP_MAX_ENUM;
    default:
        return VkAttachmentStoreOp::VK_ATTACHMENT_STORE_OP_MAX_ENUM;
    }
}

inline VkPrimitiveTopology PrimitiveTopology2VK(EPrimitiveType type, UINT NumPrimitives, UINT& indexCount)
{
    switch (type)
    {
    case EngineNS::EPT_PointList:
        indexCount = NumPrimitives * 2;
        return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_POINT_LIST;
    case EngineNS::EPT_LineList:
        indexCount = NumPrimitives + 1;
        return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_LINE_LIST;
    case EngineNS::EPT_LineStrip:
        indexCount = NumPrimitives + 2;
        return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_LINE_STRIP;
    case EngineNS::EPT_TriangleList:
        indexCount = NumPrimitives * 3;
        return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
    case EngineNS::EPT_TriangleStrip:
        indexCount = NumPrimitives + 2;
        return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP;
    case EngineNS::EPT_TriangleFan:
        indexCount = NumPrimitives + 2;
        return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_TRIANGLE_FAN;
    default:
        ASSERT(false);
        return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_POINT_LIST;
    }
}

class IVKRenderContext;
uint32_t VK_FindMemoryType(IVKRenderContext* rc, uint32_t typeFilter, VkMemoryPropertyFlags properties);

struct VKGpuMemory;
struct VKGpuMemoryType;
struct VkMemoryBatch
{
    VKGpuMemoryType*            MemoryType;
    VkDeviceMemory              Memory;
    std::vector<VKGpuMemory>    Cells;
};

struct VKGpuMemoryType
{
    VKGpuMemoryType()
    {
        CellSize = 0;
        CellCount = 0;
        FreePoint = nullptr;
    }
    VKGpuMemory*        FreePoint;
    bool Init(IVKRenderContext* rc, UINT typeIndex, UINT64 cellSize);    
    const UINT64        GpuMemoryBlockSize = 1024 * 1024;
    bool IsHugeBlock() const {
        return CellSize > GpuMemoryBlockSize;
    }
    VKGpuMemory* Alloc(IVKRenderContext* rc);
    std::atomic<UINT>	MemCount;
    UINT                TypeIndex;
    UINT64              CellSize;
    UINT                CellCount;

    std::vector<VkMemoryBatch*>     MemoryBatches;
};

struct VKGpuMemory
{
    VkMemoryBatch*      HostBatch;
    VKGpuMemory*        Next;
    UINT64              Offset;
    UINT64              Size;
    VKGpuMemory()
    {
        HostBatch = 0;
        Next = nullptr;
        Offset = 0;
    }
    inline VkDeviceMemory GetDeviceMemory() {
        return HostBatch->Memory;
    }
};

VKGpuMemory* VK_AllocGpuMemory(IVKRenderContext* rc, const VkMemoryAllocateInfo* info);
void VK_FreeGpuMemory(IVKRenderContext* rc, VKGpuMemory* memory);

bool VK_CreateBuffer(IVKRenderContext* rc, VkDeviceSize size, VkBufferUsageFlags usage, VkMemoryPropertyFlags properties, VkBuffer& buffer, VKGpuMemory*& bufferMemory);
void VK_CopyBuffer(IVKRenderContext* rc, VkBuffer srcBuffer, VkBuffer dstBuffer, VkDeviceSize size, VkCommandBuffer commandBuffer);
void VK_CopyBufferToImage(IVKRenderContext* rc, VkBuffer buffer, VkImage image, UINT mipLevel, uint32_t width, uint32_t height, VkCommandBuffer commandBuffer);
VkCommandBuffer VK_BeginSingleTimeCommands(IVKRenderContext* rc);
void VK_EndSingleTimeCommands(IVKRenderContext* rc, VkCommandBuffer commandBuffer);

typedef void (FVkExecute)(IVKRenderContext* rc);
void PostVkExecute(const std::function<FVkExecute>& exec, int delayFrame = 3);
void VkExecuteAll(IVKRenderContext* rc, bool force);

NS_END