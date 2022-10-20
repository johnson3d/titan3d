#include "VKGpuState.h"
#include "VKGpuDevice.h"
#include "VKFrameBuffers.h"
#include "VKEffect.h"
#include "VKInputAssembly.h"
#include "VKShader.h"
#include "../NxEffect.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	inline VkCompareOp CompareOp2VKCompareOp(EComparisionMode mode)
	{
		switch (mode)
		{
		case EComparisionMode::CMP_NEVER:
			return VkCompareOp::VK_COMPARE_OP_NEVER;
		case EComparisionMode::CMP_LESS:
			return VkCompareOp::VK_COMPARE_OP_LESS;
		case EComparisionMode::CMP_EQUAL:
			return VkCompareOp::VK_COMPARE_OP_EQUAL;
		case EComparisionMode::CMP_LESS_EQUAL:
			return VkCompareOp::VK_COMPARE_OP_LESS_OR_EQUAL;
		case EComparisionMode::CMP_GREATER:
			return VkCompareOp::VK_COMPARE_OP_GREATER;
		case EComparisionMode::CMP_NOT_EQUAL:
			return VkCompareOp::VK_COMPARE_OP_NOT_EQUAL;
		case EComparisionMode::CMP_GREATER_EQUAL:
			return VkCompareOp::VK_COMPARE_OP_GREATER_OR_EQUAL;
		case EComparisionMode::CMP_ALWAYS:
			return VkCompareOp::VK_COMPARE_OP_ALWAYS;
		default:
			return VkCompareOp::VK_COMPARE_OP_ALWAYS;
		}
	}
	inline VkSamplerAddressMode AddressMode2VKAddressMode(EAddressMode mode)
	{
		switch (mode)
		{
		case EAddressMode::ADM_WRAP:
			return VkSamplerAddressMode::VK_SAMPLER_ADDRESS_MODE_REPEAT;
		case EAddressMode::ADM_MIRROR:
			return VkSamplerAddressMode::VK_SAMPLER_ADDRESS_MODE_MIRRORED_REPEAT;
		case EAddressMode::ADM_CLAMP:
			return VkSamplerAddressMode::VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_EDGE;
		case EAddressMode::ADM_BORDER:
			return VkSamplerAddressMode::VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_BORDER;
		case EAddressMode::ADM_MIRROR_ONCE:
			return VkSamplerAddressMode::VK_SAMPLER_ADDRESS_MODE_MIRROR_CLAMP_TO_EDGE;
		default:
			return VkSamplerAddressMode::VK_SAMPLER_ADDRESS_MODE_REPEAT;
		}
	}
	VKSampler::VKSampler()
	{
	}
	VKSampler::~VKSampler()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		if (mView != nullptr)
		{
			vkDestroySampler(device->mDevice, mView, device->GetVkAllocCallBacks());
			mView = nullptr;
		}
	}
	bool VKSampler::Init(VKGpuDevice* device, const FSamplerDesc& desc)
	{
		Desc = desc;
		
		VkSamplerCreateInfo samplerInfo;
		memset(&samplerInfo, 0, sizeof(samplerInfo));

		samplerInfo.sType = VK_STRUCTURE_TYPE_SAMPLER_CREATE_INFO;
		samplerInfo.anisotropyEnable = FALSE;
		samplerInfo.compareEnable = VK_FALSE;
		switch (desc.Filter)
		{
			case ESamplerFilter::SPF_MIN_MAG_MIP_POINT:
				samplerInfo.magFilter = VK_FILTER_NEAREST;
				samplerInfo.minFilter = VK_FILTER_NEAREST;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
				break;
			case ESamplerFilter::SPF_MIN_MAG_POINT_MIP_LINEAR:
				samplerInfo.magFilter = VK_FILTER_LINEAR;
				samplerInfo.minFilter = VK_FILTER_LINEAR;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
				break;
			case ESamplerFilter::SPF_MIN_POINT_MAG_LINEAR_MIP_POINT:
				samplerInfo.magFilter = VK_FILTER_LINEAR;
				samplerInfo.minFilter = VK_FILTER_NEAREST;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
				break;
			case ESamplerFilter::SPF_MIN_POINT_MAG_MIP_LINEAR:
				samplerInfo.magFilter = VK_FILTER_LINEAR;
				samplerInfo.minFilter = VK_FILTER_NEAREST;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
				break;
			case ESamplerFilter::SPF_MIN_LINEAR_MAG_MIP_POINT:
				samplerInfo.magFilter = VK_FILTER_NEAREST;
				samplerInfo.minFilter = VK_FILTER_LINEAR;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
				break;
			case ESamplerFilter::SPF_MIN_LINEAR_MAG_POINT_MIP_LINEAR:
				samplerInfo.magFilter = VK_FILTER_NEAREST;
				samplerInfo.minFilter = VK_FILTER_LINEAR;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
				break;
			case ESamplerFilter::SPF_MIN_MAG_LINEAR_MIP_POINT:
				samplerInfo.magFilter = VK_FILTER_LINEAR;
				samplerInfo.minFilter = VK_FILTER_LINEAR;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
				break;
			case ESamplerFilter::SPF_MIN_MAG_MIP_LINEAR:
				samplerInfo.magFilter = VK_FILTER_LINEAR;
				samplerInfo.minFilter = VK_FILTER_LINEAR;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
				break;
			case ESamplerFilter::SPF_ANISOTROPIC:
				/*samplerInfo.magFilter = VK_FILTER_NEAREST;
				samplerInfo.minFilter = VK_FILTER_LINEAR;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;*/
				samplerInfo.anisotropyEnable = VK_TRUE;
				break;
			case ESamplerFilter::SPF_COMPARISON_MIN_MAG_MIP_POINT:
				samplerInfo.magFilter = VK_FILTER_NEAREST;
				samplerInfo.minFilter = VK_FILTER_NEAREST;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
				samplerInfo.compareEnable = VK_TRUE;
				break;
			case ESamplerFilter::SPF_COMPARISON_MIN_MAG_POINT_MIP_LINEAR:
				samplerInfo.magFilter = VK_FILTER_NEAREST;
				samplerInfo.minFilter = VK_FILTER_NEAREST;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
				samplerInfo.compareEnable = VK_TRUE;
				break;
			case ESamplerFilter::SPF_COMPARISON_MIN_POINT_MAG_LINEAR_MIP_POINT:
				samplerInfo.magFilter = VK_FILTER_LINEAR;
				samplerInfo.minFilter = VK_FILTER_NEAREST;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
				samplerInfo.compareEnable = VK_TRUE;
				break;
			case ESamplerFilter::SPF_COMPARISON_MIN_POINT_MAG_MIP_LINEAR:
				samplerInfo.magFilter = VK_FILTER_LINEAR;
				samplerInfo.minFilter = VK_FILTER_NEAREST;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
				samplerInfo.compareEnable = VK_TRUE;
				break;
			case ESamplerFilter::SPF_COMPARISON_MIN_LINEAR_MAG_MIP_POINT:
				samplerInfo.magFilter = VK_FILTER_NEAREST;
				samplerInfo.minFilter = VK_FILTER_LINEAR;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
				samplerInfo.compareEnable = VK_TRUE;
				break;
			case ESamplerFilter::SPF_COMPARISON_MIN_LINEAR_MAG_POINT_MIP_LINEAR:
				samplerInfo.magFilter = VK_FILTER_NEAREST;
				samplerInfo.minFilter = VK_FILTER_LINEAR;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
				samplerInfo.compareEnable = VK_TRUE;
				break;
			case ESamplerFilter::SPF_COMPARISON_MIN_MAG_LINEAR_MIP_POINT:
				samplerInfo.magFilter = VK_FILTER_LINEAR;
				samplerInfo.minFilter = VK_FILTER_LINEAR;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
				samplerInfo.compareEnable = VK_TRUE;
				break;
			case ESamplerFilter::SPF_COMPARISON_MIN_MAG_MIP_LINEAR:
				samplerInfo.magFilter = VK_FILTER_LINEAR;
				samplerInfo.minFilter = VK_FILTER_LINEAR;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
				samplerInfo.compareEnable = VK_TRUE;
				break;
			case ESamplerFilter::SPF_COMPARISON_ANISOTROPIC:
				samplerInfo.compareEnable = VK_TRUE;
				samplerInfo.anisotropyEnable = VK_TRUE;
				break;
			case ESamplerFilter::SPF_MINIMUM_MIN_MAG_MIP_POINT:
				samplerInfo.magFilter = VK_FILTER_NEAREST;
				samplerInfo.minFilter = VK_FILTER_NEAREST;
				samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
				break;
			case ESamplerFilter::SPF_MINIMUM_MIN_MAG_POINT_MIP_LINEAR:
				break;
			case ESamplerFilter::SPF_MINIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT:
				break;
			case ESamplerFilter::SPF_MINIMUM_MIN_POINT_MAG_MIP_LINEAR:
				break;
			case ESamplerFilter::SPF_MINIMUM_MIN_LINEAR_MAG_MIP_POINT:
				break;
			case ESamplerFilter::SPF_MINIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR:
				break;
			case ESamplerFilter::SPF_MINIMUM_MIN_MAG_LINEAR_MIP_POINT:
				break;
			case ESamplerFilter::SPF_MINIMUM_MIN_MAG_MIP_LINEAR:
				break;
			case ESamplerFilter::SPF_MINIMUM_ANISOTROPIC:
				break;
			case ESamplerFilter::SPF_MAXIMUM_MIN_MAG_MIP_POINT:
				break;
			case ESamplerFilter::SPF_MAXIMUM_MIN_MAG_POINT_MIP_LINEAR:
				break;
			case ESamplerFilter::SPF_MAXIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT:
				break;
			case ESamplerFilter::SPF_MAXIMUM_MIN_POINT_MAG_MIP_LINEAR:
				break;
			case ESamplerFilter::SPF_MAXIMUM_MIN_LINEAR_MAG_MIP_POINT:
				break;
			case ESamplerFilter::SPF_MAXIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR:
				break;
			case ESamplerFilter::SPF_MAXIMUM_MIN_MAG_LINEAR_MIP_POINT:
				break;
			case ESamplerFilter::SPF_MAXIMUM_MIN_MAG_MIP_LINEAR:
				break;
			case ESamplerFilter::SPF_MAXIMUM_ANISOTROPIC:
				break;
			default:
				break;
		}

		samplerInfo.addressModeU = AddressMode2VKAddressMode(desc.AddressU);
		samplerInfo.addressModeV = AddressMode2VKAddressMode(desc.AddressV);
		samplerInfo.addressModeW = AddressMode2VKAddressMode(desc.AddressW);

		samplerInfo.maxAnisotropy = (float)desc.MaxAnisotropy;// properties.limits.maxSamplerAnisotropy;
		samplerInfo.borderColor = VK_BORDER_COLOR_INT_OPAQUE_BLACK;//desc->BorderColor
		samplerInfo.unnormalizedCoordinates = VK_FALSE;
		samplerInfo.compareOp = CompareOp2VKCompareOp(desc.CmpMode);

		if (vkCreateSampler(device->mDevice, &samplerInfo, nullptr, &mView) != VK_SUCCESS)
		{
			return false;
		}
		return true;
	}

	VKGpuPipeline::VKGpuPipeline()
	{

	}
	VKGpuPipeline::~VKGpuPipeline()
	{
	}
	inline VkPolygonMode FillMode2VKFillMode(EFillMode mode)
	{
		switch (mode)
		{
		case EFillMode::FMD_WIREFRAME:
			return VkPolygonMode::VK_POLYGON_MODE_LINE;
		case EFillMode::FMD_SOLID:
			return VkPolygonMode::VK_POLYGON_MODE_FILL;
		default:
			return VkPolygonMode::VK_POLYGON_MODE_FILL;
		}
	}
	inline VkCullModeFlagBits CullMode2VKCullMode(ECullMode mode)
	{
		switch (mode)
		{
		case ECullMode::CMD_NONE:
			return VkCullModeFlagBits::VK_CULL_MODE_NONE;
		case ECullMode::CMD_FRONT:
			return VkCullModeFlagBits::VK_CULL_MODE_FRONT_BIT;
		case ECullMode::CMD_BACK:
			return VkCullModeFlagBits::VK_CULL_MODE_BACK_BIT;
		default:
			return VkCullModeFlagBits::VK_CULL_MODE_BACK_BIT;
		}
	}
	inline VkBlendOp BlendOp2VKBlendOp(EBlendOp op)
	{
		switch (op)
		{
		case EBlendOp::BLDOP_ADD:
			return VkBlendOp::VK_BLEND_OP_ADD;
		case EBlendOp::BLDOP_SUBTRACT:
			return VkBlendOp::VK_BLEND_OP_SUBTRACT;
		case EBlendOp::BLDOP_REV_SUBTRACT:
			return VkBlendOp::VK_BLEND_OP_REVERSE_SUBTRACT;
		case EBlendOp::BLDOP_MIN:
			return VkBlendOp::VK_BLEND_OP_MIN;
		case EBlendOp::BLDOP_MAX:
			return VkBlendOp::VK_BLEND_OP_MAX;
		default:
			return VkBlendOp::VK_BLEND_OP_ADD;
		}
	}
	inline VkBlendFactor BlendFactor2VKBlendFactor(EBlend mode)
	{
		switch (mode)
		{
		case EBlend::BLD_ZERO:
			return VkBlendFactor::VK_BLEND_FACTOR_ZERO;
		case EBlend::BLD_ONE:
			return VkBlendFactor::VK_BLEND_FACTOR_ONE;
		case EBlend::BLD_SRC_COLOR:
			return VkBlendFactor::VK_BLEND_FACTOR_SRC_COLOR;
		case EBlend::BLD_INV_SRC_COLOR:
			return VkBlendFactor::VK_BLEND_FACTOR_ONE_MINUS_SRC_COLOR;
		case EBlend::BLD_SRC_ALPHA:
			return VkBlendFactor::VK_BLEND_FACTOR_SRC_ALPHA;
		case EBlend::BLD_INV_SRC_ALPHA:
			return VkBlendFactor::VK_BLEND_FACTOR_ONE_MINUS_SRC_ALPHA;
		case EBlend::BLD_DEST_ALPHA:
			return VkBlendFactor::VK_BLEND_FACTOR_DST_COLOR;
		case EBlend::BLD_INV_DEST_ALPHA:
			return VkBlendFactor::VK_BLEND_FACTOR_ONE_MINUS_DST_ALPHA;
		case EBlend::BLD_DEST_COLOR:
			return VkBlendFactor::VK_BLEND_FACTOR_DST_COLOR;
		case EBlend::BLD_INV_DEST_COLOR:
			return VkBlendFactor::VK_BLEND_FACTOR_ONE_MINUS_DST_COLOR;
		case EBlend::BLD_SRC_ALPHA_SAT:
			return VkBlendFactor::VK_BLEND_FACTOR_SRC_ALPHA_SATURATE;
		case EBlend::BLD_BLEND_FACTOR:
			return VkBlendFactor::VK_BLEND_FACTOR_CONSTANT_ALPHA;
		case EBlend::BLD_INV_BLEND_FACTOR:
			return VkBlendFactor::VK_BLEND_FACTOR_ONE_MINUS_CONSTANT_ALPHA;
		case EBlend::BLD_SRC1_COLOR:
			return VkBlendFactor::VK_BLEND_FACTOR_SRC1_COLOR;
		case EBlend::BLD_INV_SRC1_COLOR:
			return VkBlendFactor::VK_BLEND_FACTOR_ONE_MINUS_SRC1_COLOR;
		case EBlend::BLD_SRC1_ALPHA:
			return VkBlendFactor::VK_BLEND_FACTOR_SRC1_ALPHA;
		case EBlend::BLD_INV_SRC1_ALPHA:
			return VkBlendFactor::VK_BLEND_FACTOR_ONE_MINUS_SRC1_ALPHA;
		default:
			return VkBlendFactor::VK_BLEND_FACTOR_ONE;
		}
	}
	inline VkStencilOp StencilOp2VKStencilOp(EStencilOp op)
	{
		switch (op)
		{
		case EStencilOp::STOP_KEEP:
			return VkStencilOp::VK_STENCIL_OP_KEEP;
		case EStencilOp::STOP_ZERO:
			return VkStencilOp::VK_STENCIL_OP_ZERO;
		case EStencilOp::STOP_REPLACE:
			return VkStencilOp::VK_STENCIL_OP_REPLACE;
		case EStencilOp::STOP_INCR_SAT:
			return VkStencilOp::VK_STENCIL_OP_INCREMENT_AND_CLAMP;
		case EStencilOp::STOP_DECR_SAT:
			return VkStencilOp::VK_STENCIL_OP_DECREMENT_AND_CLAMP;
		case EStencilOp::STOP_INVERT:
			return VkStencilOp::VK_STENCIL_OP_INVERT;
		case EStencilOp::STOP_INCR:
			return VkStencilOp::VK_STENCIL_OP_INCREMENT_AND_WRAP;
		case EStencilOp::STOP_DECR:
			return VkStencilOp::VK_STENCIL_OP_DECREMENT_AND_WRAP;
		default:
			return VkStencilOp::VK_STENCIL_OP_KEEP;
		}
	}
	bool VKGpuPipeline::Init(VKGpuDevice* device, const FGpuPipelineDesc& desc)
	{
		Desc = desc;
		
		mRasterState.sType = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO;
		//mRasterState.rasterizerDiscardEnable = VK_FALSE;
		mRasterState.polygonMode = FillMode2VKFillMode(desc.Rasterizer.FillMode);
		mRasterState.lineWidth = 1.0f;
		mRasterState.cullMode = CullMode2VKCullMode(desc.Rasterizer.CullMode);
		mRasterState.frontFace = desc.Rasterizer.FrontCounterClockwise ? VK_FRONT_FACE_COUNTER_CLOCKWISE : VK_FRONT_FACE_CLOCKWISE;

		mRasterState.depthClampEnable = desc.Rasterizer.DepthBiasClamp == 0 ? FALSE : TRUE;
		mRasterState.depthBiasEnable = desc.Rasterizer.DepthBias;
		mRasterState.depthBiasClamp = desc.Rasterizer.DepthBiasClamp;
		mRasterState.depthBiasSlopeFactor = desc.Rasterizer.SlopeScaledDepthBias;
		mRasterState.rasterizerDiscardEnable = VK_FALSE;

		for (int i = 0; i < 8; i++)
		{
			mColorBlendAttachment[i].colorWriteMask = desc.Blend.RenderTarget[i].RenderTargetWriteMask;//VK_COLOR_COMPONENT_R_BIT | VK_COLOR_COMPONENT_G_BIT | VK_COLOR_COMPONENT_B_BIT | VK_COLOR_COMPONENT_A_BIT;
			mColorBlendAttachment[i].blendEnable = desc.Blend.RenderTarget[i].BlendEnable;

			mColorBlendAttachment[i].colorBlendOp = BlendOp2VKBlendOp(desc.Blend.RenderTarget[i].BlendOp);
			mColorBlendAttachment[i].srcColorBlendFactor = BlendFactor2VKBlendFactor(desc.Blend.RenderTarget[i].SrcBlend);
			mColorBlendAttachment[i].dstColorBlendFactor = BlendFactor2VKBlendFactor(desc.Blend.RenderTarget[i].DestBlend);

			mColorBlendAttachment[i].alphaBlendOp = BlendOp2VKBlendOp(desc.Blend.RenderTarget[i].BlendOpAlpha);
			mColorBlendAttachment[i].srcAlphaBlendFactor = BlendFactor2VKBlendFactor(desc.Blend.RenderTarget[i].SrcBlendAlpha);
			mColorBlendAttachment[i].dstAlphaBlendFactor = BlendFactor2VKBlendFactor(desc.Blend.RenderTarget[i].DestBlendAlpha);
		}

		mBlendState.sType = VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO;
		mBlendState.logicOpEnable = VK_FALSE;
		mBlendState.logicOp = VK_LOGIC_OP_COPY;
		mBlendState.attachmentCount = 0;// desc->NumOfRT;
		mBlendState.pAttachments = mColorBlendAttachment;
		mBlendState.blendConstants[0] = 0.0f;//blendfactor...
		mBlendState.blendConstants[1] = 0.0f;
		mBlendState.blendConstants[2] = 0.0f;
		mBlendState.blendConstants[3] = 0.0f;

		mDepthStencilState.sType = VK_STRUCTURE_TYPE_PIPELINE_DEPTH_STENCIL_STATE_CREATE_INFO;
		mDepthStencilState.flags = 0;
		mDepthStencilState.depthTestEnable = desc.DepthStencil.DepthEnable;
		mDepthStencilState.depthWriteEnable = (desc.DepthStencil.DepthWriteMask == DSWM_ZERO) ? FALSE : TRUE;
		/*if (mDepthStencilState.depthTestEnable == FALSE)
			mDepthStencilState.depthCompareOp = VkCompareOp::VK_COMPARE_OP_ALWAYS;
		else*/
			mDepthStencilState.depthCompareOp = CompareOp2VKCompareOp(desc.DepthStencil.DepthFunc);
		mDepthStencilState.depthBoundsTestEnable = VK_FALSE;
		mDepthStencilState.stencilTestEnable = desc.DepthStencil.StencilEnable;
		mDepthStencilState.front.failOp = StencilOp2VKStencilOp(desc.DepthStencil.FrontFace.StencilFailOp);
		mDepthStencilState.front.passOp = StencilOp2VKStencilOp(desc.DepthStencil.FrontFace.StencilPassOp);
		mDepthStencilState.front.depthFailOp = StencilOp2VKStencilOp(desc.DepthStencil.FrontFace.StencilDepthFailOp);
		mDepthStencilState.front.compareOp = CompareOp2VKCompareOp(desc.DepthStencil.FrontFace.StencilFunc);

		mDepthStencilState.back.failOp = StencilOp2VKStencilOp(desc.DepthStencil.BackFace.StencilFailOp);
		mDepthStencilState.back.passOp = StencilOp2VKStencilOp(desc.DepthStencil.BackFace.StencilPassOp);
		mDepthStencilState.back.depthFailOp = StencilOp2VKStencilOp(desc.DepthStencil.BackFace.StencilDepthFailOp);
		mDepthStencilState.back.compareOp = CompareOp2VKCompareOp(desc.DepthStencil.BackFace.StencilFunc);

		return true;
	}
	inline VkPrimitiveTopology PrimitiveTopology2VK(EPrimitiveType type, UINT NumPrimitives, UINT& indexCount)
	{
		switch (type)
		{
		case EPrimitiveType::EPT_PointList:
			indexCount = NumPrimitives * 2;
			return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_POINT_LIST;
		case EPrimitiveType::EPT_LineList:
			indexCount = NumPrimitives + 1;
			return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_LINE_LIST;
		case EPrimitiveType::EPT_LineStrip:
			indexCount = NumPrimitives + 2;
			return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_LINE_STRIP;
		case EPrimitiveType::EPT_TriangleList:
			indexCount = NumPrimitives * 3;
			return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
		case EPrimitiveType::EPT_TriangleStrip:
			indexCount = NumPrimitives + 2;
			return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP;
		case EPrimitiveType::EPT_TriangleFan:
			indexCount = NumPrimitives + 2;
			return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_TRIANGLE_FAN;
		default:
			ASSERT(false);
			return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_POINT_LIST;
		}
	}
	bool VKGpuDrawState::BuildState(IGpuDevice* device1)
	{
		auto device = (VKGpuDevice*)device1;
		auto effect = ShaderEffect.UnsafeConvertTo<VKGraphicsEffect>();

		VkPipelineShaderStageCreateInfo shaderStages[2]{};
		shaderStages[0].sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
		shaderStages[0].stage = VK_SHADER_STAGE_VERTEX_BIT;
		shaderStages[0].module = ((VKShader*)effect->GetVS())->mShader;
		shaderStages[0].pName = effect->mVertexShader->Desc->FunctionName.c_str();
		shaderStages[1].sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
		shaderStages[1].stage = VK_SHADER_STAGE_FRAGMENT_BIT;
		shaderStages[1].module = ((VKShader*)effect->GetPS())->mShader;
		shaderStages[1].pName = effect->mPixelShader->Desc->FunctionName.c_str();
		

		VkPipelineMultisampleStateCreateInfo multisampling{};
		multisampling.sType = VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO;
		multisampling.sampleShadingEnable = VK_FALSE;
		multisampling.rasterizationSamples = VK_SAMPLE_COUNT_1_BIT;

		VkPipelineInputAssemblyStateCreateInfo inputAssembly{};
		inputAssembly.sType = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO;
		UINT indexCount;
		inputAssembly.topology = PrimitiveTopology2VK(this->TopologyType , 0, indexCount);
		inputAssembly.primitiveRestartEnable = VK_FALSE;

		VkPipelineViewportStateCreateInfo viewportState{};
		viewportState.sType = VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO;

		/*VkRect2D scissor{};
		scissor.offset = { (int)vp->x, (int)(vp->y + vp->height) };
		scissor.extent = { (UINT)vp->width, (UINT)(-vp->height) };*/

		viewportState.viewportCount = 1;
		viewportState.pViewports = nullptr;// vp;
		viewportState.scissorCount = 1;
		viewportState.pScissors = nullptr;

		//viewportState.flags = 0;
		/*viewportState.viewportCount = 0;
		viewportState.pViewports = nullptr;
		viewportState.scissorCount = 0;
		viewportState.pScissors = nullptr;*/

		//vkCmdSetPrimitiveTopologyEXT()
		//vkCmdSetViewport()
		//vkCmdSetScissor()

		VkDynamicState dynVPState[2];
		dynVPState[0] = VK_DYNAMIC_STATE_VIEWPORT;
		dynVPState[1] = VK_DYNAMIC_STATE_SCISSOR;

		VkPipelineDynamicStateCreateInfo dynStateInfo{};
		dynStateInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_DYNAMIC_STATE_CREATE_INFO;
		dynStateInfo.dynamicStateCount = 2;
		dynStateInfo.pDynamicStates = dynVPState;

		VkGraphicsPipelineCreateInfo pipelineInfo{};
		pipelineInfo.sType = VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO;
		pipelineInfo.flags = 0;
		pipelineInfo.stageCount = 2;
		pipelineInfo.pStages = shaderStages;

		pipelineInfo.pInputAssemblyState = &inputAssembly;
		pipelineInfo.pViewportState = &viewportState;
		pipelineInfo.pMultisampleState = &multisampling;
		pipelineInfo.pDynamicState = &dynStateInfo;

		auto vkPipeline = this->Pipeline.UnsafeConvertTo<VKGpuPipeline>();
		pipelineInfo.pRasterizationState = &vkPipeline->mRasterState;
		
		/*VkPipelineDepthStencilStateCreateInfo depthStencilState = {};
		depthStencilState.sType = VK_STRUCTURE_TYPE_PIPELINE_DEPTH_STENCIL_STATE_CREATE_INFO;
		depthStencilState.depthTestEnable = VK_TRUE;
		depthStencilState.depthWriteEnable = VK_TRUE;
		depthStencilState.depthCompareOp = VK_COMPARE_OP_LESS_OR_EQUAL;
		depthStencilState.depthBoundsTestEnable = VK_FALSE;
		depthStencilState.back.failOp = VK_STENCIL_OP_KEEP;
		depthStencilState.back.passOp = VK_STENCIL_OP_KEEP;
		depthStencilState.back.compareOp = VK_COMPARE_OP_ALWAYS;
		depthStencilState.stencilTestEnable = VK_FALSE;
		depthStencilState.front = depthStencilState.back;*/

		pipelineInfo.pDepthStencilState = &vkPipeline->mDepthStencilState;
		auto bldState = vkPipeline->mBlendState;
		bldState.attachmentCount = RenderPass->Desc.NumOfMRT;
		pipelineInfo.pColorBlendState = &bldState;

		pipelineInfo.pVertexInputState = &effect->mInputLayout.UnsafeConvertTo<VKInputLayout>()->mInfo;

		pipelineInfo.layout = effect->mPipelineLayout;
		pipelineInfo.renderPass = this->RenderPass.UnsafeConvertTo<VKRenderPass>()->mRenderPass;
		pipelineInfo.subpass = 0;
		pipelineInfo.basePipelineHandle = VK_NULL_HANDLE;

		if (vkCreateGraphicsPipelines(device->mDevice, VK_NULL_HANDLE, 1, &pipelineInfo,
			device->GetVkAllocCallBacks(), &mGraphicsPipeline) != VK_SUCCESS)
		{
			return false;
		}

		//VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_DEBUG_REPORT_OBJECT_TYPE_PIPELINE_EXT , mGraphicsPipeline, "PipeLine1");

		return true;
	}
}

NS_END