#include "IVKSamplerState.h"
#include "IVKRenderContext.h"

#define new VNEW

NS_BEGIN

IVKSamplerState::IVKSamplerState()
{
}


IVKSamplerState::~IVKSamplerState()
{
}

bool IVKSamplerState::Init(IVKRenderContext* rc, const ISamplerStateDesc* desc)
{
	VkSamplerCreateInfo samplerInfo;
	memset(&samplerInfo, 0, sizeof(samplerInfo));

	samplerInfo.sType = VK_STRUCTURE_TYPE_SAMPLER_CREATE_INFO;
	samplerInfo.anisotropyEnable = FALSE;
	samplerInfo.compareEnable = VK_FALSE;
	switch (desc->Filter)
	{
	case EngineNS::SPF_MIN_MAG_MIP_POINT:
		samplerInfo.magFilter = VK_FILTER_NEAREST;
		samplerInfo.minFilter = VK_FILTER_NEAREST;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
		break;
	case EngineNS::SPF_MIN_MAG_POINT_MIP_LINEAR:
		samplerInfo.magFilter = VK_FILTER_LINEAR;
		samplerInfo.minFilter = VK_FILTER_LINEAR;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
		break;
	case EngineNS::SPF_MIN_POINT_MAG_LINEAR_MIP_POINT:
		samplerInfo.magFilter = VK_FILTER_LINEAR;
		samplerInfo.minFilter = VK_FILTER_NEAREST;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
		break;
	case EngineNS::SPF_MIN_POINT_MAG_MIP_LINEAR:
		samplerInfo.magFilter = VK_FILTER_LINEAR;
		samplerInfo.minFilter = VK_FILTER_NEAREST;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
		break;
	case EngineNS::SPF_MIN_LINEAR_MAG_MIP_POINT:
		samplerInfo.magFilter = VK_FILTER_NEAREST;
		samplerInfo.minFilter = VK_FILTER_LINEAR;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
		break;
	case EngineNS::SPF_MIN_LINEAR_MAG_POINT_MIP_LINEAR:
		samplerInfo.magFilter = VK_FILTER_NEAREST;
		samplerInfo.minFilter = VK_FILTER_LINEAR;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
		break;
	case EngineNS::SPF_MIN_MAG_LINEAR_MIP_POINT:
		samplerInfo.magFilter = VK_FILTER_LINEAR;
		samplerInfo.minFilter = VK_FILTER_LINEAR;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
		break;
	case EngineNS::SPF_MIN_MAG_MIP_LINEAR:
		samplerInfo.magFilter = VK_FILTER_LINEAR;
		samplerInfo.minFilter = VK_FILTER_LINEAR;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
		break;
	case EngineNS::SPF_ANISOTROPIC:
		/*samplerInfo.magFilter = VK_FILTER_NEAREST;
		samplerInfo.minFilter = VK_FILTER_LINEAR;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;*/
		samplerInfo.anisotropyEnable = VK_TRUE;
		break;
	case EngineNS::SPF_COMPARISON_MIN_MAG_MIP_POINT:
		samplerInfo.magFilter = VK_FILTER_NEAREST;
		samplerInfo.minFilter = VK_FILTER_NEAREST;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
		samplerInfo.compareEnable = VK_TRUE;
		break;
	case EngineNS::SPF_COMPARISON_MIN_MAG_POINT_MIP_LINEAR:
		samplerInfo.magFilter = VK_FILTER_NEAREST;
		samplerInfo.minFilter = VK_FILTER_NEAREST;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
		samplerInfo.compareEnable = VK_TRUE;
		break;
	case EngineNS::SPF_COMPARISON_MIN_POINT_MAG_LINEAR_MIP_POINT:
		samplerInfo.magFilter = VK_FILTER_LINEAR;
		samplerInfo.minFilter = VK_FILTER_NEAREST;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
		samplerInfo.compareEnable = VK_TRUE;
		break;
	case EngineNS::SPF_COMPARISON_MIN_POINT_MAG_MIP_LINEAR:
		samplerInfo.magFilter = VK_FILTER_LINEAR;
		samplerInfo.minFilter = VK_FILTER_NEAREST;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
		samplerInfo.compareEnable = VK_TRUE;
		break;
	case EngineNS::SPF_COMPARISON_MIN_LINEAR_MAG_MIP_POINT:
		samplerInfo.magFilter = VK_FILTER_NEAREST;
		samplerInfo.minFilter = VK_FILTER_LINEAR;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
		samplerInfo.compareEnable = VK_TRUE;
		break;
	case EngineNS::SPF_COMPARISON_MIN_LINEAR_MAG_POINT_MIP_LINEAR:
		samplerInfo.magFilter = VK_FILTER_NEAREST;
		samplerInfo.minFilter = VK_FILTER_LINEAR;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
		samplerInfo.compareEnable = VK_TRUE;
		break;
	case EngineNS::SPF_COMPARISON_MIN_MAG_LINEAR_MIP_POINT:
		samplerInfo.magFilter = VK_FILTER_LINEAR;
		samplerInfo.minFilter = VK_FILTER_LINEAR;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
		samplerInfo.compareEnable = VK_TRUE;
		break;
	case EngineNS::SPF_COMPARISON_MIN_MAG_MIP_LINEAR:
		samplerInfo.magFilter = VK_FILTER_LINEAR;
		samplerInfo.minFilter = VK_FILTER_LINEAR;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
		samplerInfo.compareEnable = VK_TRUE;
		break;
	case EngineNS::SPF_COMPARISON_ANISOTROPIC:
		samplerInfo.compareEnable = VK_TRUE;
		samplerInfo.anisotropyEnable = VK_TRUE;
		break;
	case EngineNS::SPF_MINIMUM_MIN_MAG_MIP_POINT:
		samplerInfo.magFilter = VK_FILTER_NEAREST;
		samplerInfo.minFilter = VK_FILTER_NEAREST;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
		break;
	case EngineNS::SPF_MINIMUM_MIN_MAG_POINT_MIP_LINEAR:
		break;
	case EngineNS::SPF_MINIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT:
		break;
	case EngineNS::SPF_MINIMUM_MIN_POINT_MAG_MIP_LINEAR:
		break;
	case EngineNS::SPF_MINIMUM_MIN_LINEAR_MAG_MIP_POINT:
		break;
	case EngineNS::SPF_MINIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR:
		break;
	case EngineNS::SPF_MINIMUM_MIN_MAG_LINEAR_MIP_POINT:
		break;
	case EngineNS::SPF_MINIMUM_MIN_MAG_MIP_LINEAR:
		break;
	case EngineNS::SPF_MINIMUM_ANISOTROPIC:
		break;
	case EngineNS::SPF_MAXIMUM_MIN_MAG_MIP_POINT:
		break;
	case EngineNS::SPF_MAXIMUM_MIN_MAG_POINT_MIP_LINEAR:
		break;
	case EngineNS::SPF_MAXIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT:
		break;
	case EngineNS::SPF_MAXIMUM_MIN_POINT_MAG_MIP_LINEAR:
		break;
	case EngineNS::SPF_MAXIMUM_MIN_LINEAR_MAG_MIP_POINT:
		break;
	case EngineNS::SPF_MAXIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR:
		break;
	case EngineNS::SPF_MAXIMUM_MIN_MAG_LINEAR_MIP_POINT:
		break;
	case EngineNS::SPF_MAXIMUM_MIN_MAG_MIP_LINEAR:
		break;
	case EngineNS::SPF_MAXIMUM_ANISOTROPIC:
		break;
	default:
		break;
	}
	
	
	samplerInfo.addressModeU = AddressMode2VKAddressMode(desc->AddressU);
	samplerInfo.addressModeV = AddressMode2VKAddressMode(desc->AddressV);
	samplerInfo.addressModeW = AddressMode2VKAddressMode(desc->AddressW);
	
	samplerInfo.maxAnisotropy = (float)desc->MaxAnisotropy;// properties.limits.maxSamplerAnisotropy;
	samplerInfo.borderColor = VK_BORDER_COLOR_INT_OPAQUE_BLACK;//desc->BorderColor
	samplerInfo.unnormalizedCoordinates = VK_FALSE;	
	samplerInfo.compareOp = CompareOp2VKCompareOp(desc->CmpMode);
	
	if (vkCreateSampler(rc->mLogicalDevice, &samplerInfo, nullptr, &TextureSampler) != VK_SUCCESS)
	{
		return false;
	}
	return true;
}

NS_END