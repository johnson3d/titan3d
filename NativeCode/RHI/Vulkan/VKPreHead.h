#pragma once

#include <vulkan/vulkan.h>
#include "../PreHead.h"

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
        return VK_FORMAT_R16_UNORM;
    case EngineNS::PXF_R16_SNORM:
        return VK_FORMAT_R16_SNORM;
    case EngineNS::PXF_R32_UINT:
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
    default:
        ASSERT(false);
        break;
	}
    return VK_FORMAT_UNDEFINED;
}

class IVKRenderContext;
uint32_t VK_FindMemoryType(IVKRenderContext* rc, uint32_t typeFilter, VkMemoryPropertyFlags properties);
bool VK_CreateBuffer(IVKRenderContext* rc, VkDeviceSize size, VkBufferUsageFlags usage, VkMemoryPropertyFlags properties, VkBuffer& buffer, VkDeviceMemory& bufferMemory);
void VK_CopyBuffer(IVKRenderContext* rc, VkBuffer srcBuffer, VkBuffer dstBuffer, VkDeviceSize size);

NS_END