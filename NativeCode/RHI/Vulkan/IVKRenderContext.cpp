#include "IVKRenderSystem.h"
#include "IVKRenderContext.h"
#include "IVKCommandList.h"
#include "IVKRenderPipeline.h"
#include "IVKConstantBuffer.h"
#include "IVKVertexBuffer.h"
#include "IVKIndexBuffer.h"
#include "IVKRenderTargetView.h"
#include "IVKDepthStencilView.h"
#include "IVKVertexShader.h"
#include "IVKPixelShader.h"
#include "IVKInputLayout.h"
#include "IVKSwapChain.h"
#include "IVKFrameBuffers.h"
#include "IVKShaderProgram.h"
#include "IVKGeometryMesh.h"
#include "IVKPass.h"
#include "IVKConstantBuffer.h"
#include "IVKRasterizerState.h"
#include "IVKSamplerState.h"
#include "IVKDepthStencilState.h"
#include "IVKBlendState.h"
#include "IVKComputeShader.h"
#include "IVKTexture2D.h"
#include "IVKShaderResourceView.h"
#include "IVKGpuBuffer.h"
#include "IVKFence.h"
#include "IVKUnorderedAccessView.h"
#include "../ShaderReflector.h"
#include "..\IShader.h"
#include <set>
#include <limits>

#ifndef NOMINMAX
	#define NOMINMAX
#endif

//#include <spirv_cross/spirv_glsl.hpp>
#include <spirv_cross/spirv_cross_c.h>

#define new VNEW

using namespace EngineNS;

NS_BEGIN

bool ShaderReflector::ReflectSpirV(const EngineNS::IShaderDesc* desc)
{
	auto reflector = ((EngineNS::IShaderDesc*)desc)->GetReflector();
	reflector->Reset();

	/*spirv_cross::CompilerGLSL glsl(desc->SpirV);
	spirv_cross::ShaderResources resources = glsl.get_shader_resources();
	for (auto& resource : resources.sampled_images)
	{
		unsigned set = glsl.get_decoration(resource.id, spv::DecorationDescriptorSet);
		unsigned binding = glsl.get_decoration(resource.id, spv::DecorationBinding);

	}
	for (auto& resource : resources.uniform_buffers)
	{
		unsigned set = glsl.get_decoration(resource.id, spv::DecorationDescriptorSet);
		unsigned binding = glsl.get_decoration(resource.id, spv::DecorationBinding);
	}*/

	spvc_context context = NULL;
	spvc_parsed_ir ir = NULL;
	spvc_compiler compiler_glsl = NULL;
	spvc_compiler_options options = NULL;
	spvc_resources resources = NULL;
	const spvc_reflected_resource* list = NULL;
	const char* result = NULL;
	size_t count;
	size_t i;

	const SpvId* spirv = &desc->SpirV[0];
	size_t word_count = desc->SpirV.size();

	// Create context.
	spvc_context_create(&context);

	// Set debug callback.
	//spvc_context_set_error_callback(context, error_callback, userdata);

	// Parse the SPIR-V.
	spvc_context_parse_spirv(context, spirv, word_count, &ir);

	// Hand it off to a compiler instance and give it ownership of the IR.
	spvc_context_create_compiler(context, SPVC_BACKEND_GLSL, ir, SPVC_CAPTURE_MODE_TAKE_OWNERSHIP, &compiler_glsl);

	// Do some basic reflection.
	spvc_compiler_create_shader_resources(compiler_glsl, &resources);
	spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_STAGE_INPUT, &list, &count);
	for (i = 0; i < count; i++)
	{
		auto spv_type = spvc_compiler_get_type_handle(compiler_glsl, list[i].base_type_id);
		auto name = spvc_compiler_get_name(compiler_glsl, list[i].id);
		auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
		auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationLocation);
		const spvc_reflected_builtin_resource* pInputs;
		size_t inputCount = 0;
		spvc_resources_get_builtin_resource_list_for_type(resources, SPVC_BUILTIN_RESOURCE_TYPE_STAGE_INPUT, &pInputs, &inputCount);

		/*for (int j = 0; j < inputCount; j++)
		{
			if (spvc_compiler_has_active_builtin(compiler_glsl, pInputs[j].builtin, SpvStorageClass::SpvStorageClassInput))
			{
				int xxx = 0;
			}
			else
			{
				int xxx = 0;
			}
		}*/
	}
	spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_UNIFORM_BUFFER, &list, &count);
	for (i = 0; i < count; i++)
	{
		auto spv_type = spvc_compiler_get_type_handle(compiler_glsl, list[i].base_type_id);
		std::string name = spvc_compiler_get_name(compiler_glsl, list[i].id);
		auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
		auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);
		size_t sz;
		spvc_compiler_get_declared_struct_size(compiler_glsl, spv_type, &sz);

		EngineNS::IConstantBufferDesc tmp;
		tmp.CBufferLayout = MakeWeakRef(new IConstantBufferLayout());
		tmp.Name = name;
		tmp.BindCount = 1;
		tmp.DescriptorSet = descriptorSet;
		tmp.Size = (UINT)sz;
		switch (desc->GetShaderType())
		{
		case EShaderType::EST_VertexShader:
			tmp.VSBindPoint = binding;
			break;
		case EShaderType::EST_PixelShader:
			tmp.PSBindPoint = binding;
			break;
		case EShaderType::EST_ComputeShader:
			tmp.CSBindPoint = binding;
			break;
		default:
			break;
		}

		UINT NumOfMember = spvc_type_get_num_member_types(spv_type);
		for (UINT idx = 0; idx < NumOfMember; ++idx)
		{
			ConstantVarDesc v;
			auto memberName = spvc_compiler_get_member_name(compiler_glsl, list[i].base_type_id, idx);
			size_t varSize;
			spvc_compiler_get_declared_struct_member_size(compiler_glsl, spv_type, idx, &varSize);
			spvc_compiler_type_struct_member_offset(compiler_glsl, spv_type, idx, &v.Offset);
			v.Name = memberName;
			v.Size = (UINT)varSize;

			UINT varStride = 0;
			auto ok = spvc_compiler_type_struct_member_array_stride(compiler_glsl, spv_type, idx, &varStride);
			if (SPVC_SUCCESS == ok)
				v.Elements = (UINT)(varSize / varStride);
			else
				v.Elements = 1;
			auto member_id = spvc_type_get_member_type(spv_type, idx);
			auto member_type = spvc_compiler_get_type_handle(compiler_glsl, member_id);
			auto baseType = spvc_type_get_basetype(member_type);
			auto vectorSize = spvc_type_get_columns(member_type);
			auto cols = spvc_type_get_vector_size(member_type);
			v.Type = EShaderVarType::SVT_Unknown;
			switch (baseType)
			{
			case SPVC_BASETYPE_UNKNOWN:
				break;
			case SPVC_BASETYPE_VOID:
				break;
			case SPVC_BASETYPE_BOOLEAN:
				break;
			case SPVC_BASETYPE_INT8:
				break;
			case SPVC_BASETYPE_UINT8:
				break;
			case SPVC_BASETYPE_INT16:
				break;
			case SPVC_BASETYPE_UINT16:
				break;
			case SPVC_BASETYPE_INT32:
				if (vectorSize == 4 && cols == 4)
					v.Type = EShaderVarType::SVT_Matrix4x4;
				else if (vectorSize == 3 && cols == 3)
					v.Type = EShaderVarType::SVT_Matrix3x3;
				else if (vectorSize == 1)
				{
					if (cols == 1)
						v.Type = EShaderVarType::SVT_Int1;
					else if (cols == 2)
						v.Type = EShaderVarType::SVT_Int2;
					else if (cols == 3)
						v.Type = EShaderVarType::SVT_Int3;
					else if (cols == 4)
						v.Type = EShaderVarType::SVT_Int4;
				}
				break;
			case SPVC_BASETYPE_UINT32:
				break;
			case SPVC_BASETYPE_INT64:
				break;
			case SPVC_BASETYPE_UINT64:
				break;
			case SPVC_BASETYPE_ATOMIC_COUNTER:
				break;
			case SPVC_BASETYPE_FP16:
				break;
			case SPVC_BASETYPE_FP32:
				if (vectorSize == 4 && cols == 4)
					v.Type = EShaderVarType::SVT_Matrix4x4;
				else if (vectorSize == 3 && cols == 3)
					v.Type = EShaderVarType::SVT_Matrix3x3;
				else if (vectorSize == 1)
				{
					if (cols == 1)
						v.Type = EShaderVarType::SVT_Float1;
					else if (cols == 2)
						v.Type = EShaderVarType::SVT_Float2;
					else if (cols == 3)
						v.Type = EShaderVarType::SVT_Float3;
					else if (cols == 4)
						v.Type = EShaderVarType::SVT_Float4;
				}
				break;
			case SPVC_BASETYPE_FP64:
				break;
			case SPVC_BASETYPE_STRUCT:
				break;
			case SPVC_BASETYPE_IMAGE:
				break;
			case SPVC_BASETYPE_SAMPLED_IMAGE:
				v.Type = EShaderVarType::SVT_Texture;
				break;
			case SPVC_BASETYPE_SAMPLER:
				v.Type = EShaderVarType::SVT_Sampler;
				break;
			case SPVC_BASETYPE_ACCELERATION_STRUCTURE:
				break;
			case SPVC_BASETYPE_INT_MAX:
				break;
			default:
				break;
			}
			tmp.CBufferLayout->Vars.push_back(v);
		}
		reflector->mCBDescArray.push_back(tmp);
	}

	spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_STORAGE_BUFFER, &list, &count);
	for (i = 0; i < count; i++)
	{
		std::string decl_block_name = spvc_compiler_get_remapped_declared_block_name(compiler_glsl, list[i].id);
		auto name = spvc_compiler_get_name(compiler_glsl, list[i].id);
		auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
		auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);
		/*const SpvDecoration* decoration[10];
		size_t num = 0;
		spvc_compiler_get_buffer_block_decorations(compiler_glsl, list[i].id, decoration, &num);*/
		auto constant = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationNonWritable);

		if (decl_block_name.rfind("type.RWStructuredBuffer.") == 0||
			decl_block_name.rfind("type.RWByteAddressBuffer") == 0)
		{
			UavBindInfo tmp;
			tmp.Name = name;
			tmp.BindCount = 1;
			tmp.DescriptorSet = descriptorSet;
			switch (desc->GetShaderType())
			{
			case EShaderType::EST_VertexShader:
				tmp.VSBindPoint = binding;
				break;
			case EShaderType::EST_PixelShader:
				tmp.PSBindPoint = binding;
				break;
			case EShaderType::EST_ComputeShader:
				tmp.CSBindPoint = binding;
				break;
			default:
				break;
			}
			reflector->mUavBindArray.push_back(tmp);
		}
		else if (decl_block_name.rfind("type.StructuredBuffer.") == 0)
		{
			ShaderRViewBindInfo tmp;
			tmp.Name = name;
			tmp.BindCount = 1;
			tmp.DescriptorSet = descriptorSet;
			tmp.BufferType = EGpuBufferType::GBT_UavBuffer;
			switch (desc->GetShaderType())
			{
			case EShaderType::EST_VertexShader:
				tmp.VSBindPoint = binding;
				break;
			case EShaderType::EST_PixelShader:
				tmp.PSBindPoint = binding;
				break;
			case EShaderType::EST_ComputeShader:
				tmp.CSBindPoint = binding;
				break;
			default:
				break;
			}

			reflector->mSrvBindArray.push_back(tmp);
		}
		else
		{
			ASSERT(false);
		}
	}
	spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_STORAGE_IMAGE, &list, &count);
	for (i = 0; i < count; i++)
	{
		std::string decl_block_name = spvc_compiler_get_remapped_declared_block_name(compiler_glsl, list[i].id);
		auto name = spvc_compiler_get_name(compiler_glsl, list[i].id);
		auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
		auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);

		ShaderRViewBindInfo tmp;
		tmp.Name = name;
		tmp.BindCount = 1;
		tmp.DescriptorSet = descriptorSet;
		tmp.BufferType = EGpuBufferType::GBT_TBufferBuffer;
		switch (desc->GetShaderType())
		{
		case EShaderType::EST_VertexShader:
			tmp.VSBindPoint = binding;
			break;
		case EShaderType::EST_PixelShader:
			tmp.PSBindPoint = binding;
			break;
		case EShaderType::EST_ComputeShader:
			tmp.CSBindPoint = binding;
			break;
		default:
			break;
		}

		reflector->mSrvBindArray.push_back(tmp);
	}
	spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_STORAGE_IMAGE, &list, &count);
	for (i = 0; i < count; i++)
	{
		std::string decl_block_name = spvc_compiler_get_remapped_declared_block_name(compiler_glsl, list[i].id);
		std::string name = spvc_compiler_get_name(compiler_glsl, list[i].id);
		auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
		auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);
		auto constant = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationConstant);
		auto spv_type = spvc_compiler_get_type_handle(compiler_glsl, list[i].base_type_id);
		SpvAccessQualifier access = spvc_type_get_image_access_qualifier(spv_type);
		UavBindInfo tmp;
		tmp.Name = name;
		tmp.BindCount = 1;
		tmp.DescriptorSet = descriptorSet;
		switch (desc->GetShaderType())
		{
		case EShaderType::EST_VertexShader:
			tmp.VSBindPoint = binding;
			break;
		case EShaderType::EST_PixelShader:
			tmp.PSBindPoint = binding;
			break;
		case EShaderType::EST_ComputeShader:
			tmp.CSBindPoint = binding;
			break;
		default:
			break;
		}
		reflector->mUavBindArray.push_back(tmp);
	}
	spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_SAMPLED_IMAGE, &list, &count);
	for (i = 0; i < count; i++)
	{
		auto name = spvc_compiler_get_name(compiler_glsl, list[i].id);
		auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
		auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);

		ShaderRViewBindInfo tmp;
		SamplerBindInfo tmp2;
		tmp.Name = name;
		tmp.BindCount = 1;
		tmp.DescriptorSet = descriptorSet;
		tmp2.Name = name;
		tmp2.BindCount = 1;
		tmp2.DescriptorSet = descriptorSet;
		switch (desc->GetShaderType())
		{
		case EShaderType::EST_VertexShader:
			tmp.VSBindPoint = binding;
			tmp2.VSBindPoint = binding;
			break;
		case EShaderType::EST_PixelShader:
			tmp.PSBindPoint = binding;
			tmp2.PSBindPoint = binding;
			break;
		case EShaderType::EST_ComputeShader:
			tmp.CSBindPoint = binding;
			tmp2.CSBindPoint = binding;
			break;
		default:
			break;
		}
		reflector->mSrvBindArray.push_back(tmp);
		reflector->mSamplerBindArray.push_back(tmp2);
	}
	spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_SEPARATE_IMAGE, &list, &count);
	for (i = 0; i < count; i++)
	{
		std::string name = spvc_compiler_get_name(compiler_glsl, list[i].id);
		auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
		auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);

		ShaderRViewBindInfo tmp;
		tmp.Name = name;
		tmp.BindCount = 1;
		tmp.DescriptorSet = descriptorSet;
		switch (desc->GetShaderType())
		{
		case EShaderType::EST_VertexShader:
			tmp.VSBindPoint = binding;
			break;
		case EShaderType::EST_PixelShader:
			tmp.PSBindPoint = binding;
			break;
		case EShaderType::EST_ComputeShader:
			tmp.CSBindPoint = binding;
			break;
		default:
			break;
		}
		reflector->mSrvBindArray.push_back(tmp);
	}
	spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_SEPARATE_SAMPLERS, &list, &count);
	for (i = 0; i < count; i++)
	{
		auto name = spvc_compiler_get_name(compiler_glsl, list[i].id);
		auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
		auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);

		SamplerBindInfo tmp;
		tmp.Name = name;
		tmp.BindCount = 1;
		tmp.DescriptorSet = descriptorSet;
		switch (desc->GetShaderType())
		{
		case EShaderType::EST_VertexShader:
			tmp.VSBindPoint = binding;
			break;
		case EShaderType::EST_PixelShader:
			tmp.PSBindPoint = binding;
			break;
		case EShaderType::EST_ComputeShader:
			tmp.CSBindPoint = binding;
			break;
		default:
			break;
		}
		reflector->mSamplerBindArray.push_back(tmp);
	}

	spvc_context_destroy(context);

	return true;
}

bool IVKDescriptorPool::InitPool(IVKRenderContext* rc, UINT setCount, UINT ubCount, UINT texCount, UINT samplerCount, UINT sbCount, UINT stbCount)
{
	mPoolSize[0].type = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
	mPoolSize[0].descriptorCount = ubCount;

	mPoolSize[1].type = VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
	mPoolSize[1].descriptorCount = texCount;

	mPoolSize[2].type = VK_DESCRIPTOR_TYPE_SAMPLER;
	mPoolSize[2].descriptorCount = samplerCount;

	mPoolSize[3].type = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
	mPoolSize[3].descriptorCount = sbCount;

	mPoolSize[4].type = VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER;
	mPoolSize[4].descriptorCount = stbCount;

	mNumSet = setCount;

	VkDescriptorPoolCreateInfo poolInfo = {};
	poolInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO;
	poolInfo.poolSizeCount = 5;
	poolInfo.pPoolSizes = mPoolSize;
	poolInfo.maxSets = setCount;

	if (vkCreateDescriptorPool(rc->mLogicalDevice, &poolInfo, rc->GetVkAllocCallBacks(), &mDescriptorPool) != VK_SUCCESS)
	{
		return false;
	}
	return true;
}

VkDescriptorPool IVKDescriptorPool::AllocDescriptor(const FDescriptorAlloc& arg)
{
	VAutoVSLLock locker(mLocker);
	if (mNumSet == 0 ||
		mPoolSize[0].descriptorCount < arg.UniformNum ||
		mPoolSize[1].descriptorCount < arg.TextureNum ||
		mPoolSize[2].descriptorCount < arg.SamplerNum ||
		mPoolSize[3].descriptorCount < arg.StorageNum ||
		mPoolSize[4].descriptorCount < arg.StorageTexelNum)
	{
		ASSERT(false);
		return nullptr;
	}
	mPoolSize[0].descriptorCount -= arg.UniformNum;
	mPoolSize[1].descriptorCount -= arg.TextureNum;
	mPoolSize[2].descriptorCount -= arg.SamplerNum;
	mPoolSize[3].descriptorCount -= arg.StorageNum;
	mPoolSize[5].descriptorCount -= arg.StorageTexelNum;
	mNumSet--;
	return mDescriptorPool;
}

VkDescriptorPool IVKDescriptorPool::FreeDescriptor(const FDescriptorAlloc& arg)
{
	VAutoVSLLock locker(mLocker);
	mPoolSize[0].descriptorCount += arg.UniformNum;
	mPoolSize[1].descriptorCount += arg.TextureNum;
	mPoolSize[2].descriptorCount += arg.SamplerNum;
	mPoolSize[3].descriptorCount += arg.StorageNum;
	mPoolSize[5].descriptorCount += arg.StorageTexelNum;
	mNumSet++;
	return mDescriptorPool;
}

void* IVKRenderContext::vkAllocationFunction(
	void* pUserData,
	size_t                                      size,
	size_t                                      alignment,
	VkSystemAllocationScope                     allocationScope)
{
	void* p1;
	if ((p1 = (void*)malloc(size + alignment + sizeof(size_t))) == NULL)
		return nullptr;

	size_t addr = (size_t)p1 + alignment + sizeof(size_t);
	auto p2 = (void*)(addr - (addr % alignment));

	*((size_t*)p2 - 1) = (size_t)p1;
	return p2;
}

void IVKRenderContext::vkFreeFunction(
	void* pUserData,
	void* pMemory)
{
	free((void*)(*((size_t*)pMemory - 1)));
}

void IVKRenderContext::vkInternalAllocationNotification(
	void* pUserData,
	size_t                                      size,
	VkInternalAllocationType                    allocationType,
	VkSystemAllocationScope                     allocationScope)
{

}

void IVKRenderContext::vkInternalFreeNotification(
	void* pUserData,
	size_t                                      size,
	VkInternalAllocationType                    allocationType,
	VkSystemAllocationScope                     allocationScope)
{

}

void* IVKRenderContext::vkReallocationFunction(
	void* pUserData,
	void* pOriginal,
	size_t                                      size,
	size_t                                      alignment,
	VkSystemAllocationScope                     allocationScope)
{
	vkFreeFunction(pUserData, pOriginal);
	return vkAllocationFunction(pUserData, size, alignment, allocationScope);
}

IVKRenderContext::IVKRenderContext()
{
	memset(&mDeviceFeatures, 0, sizeof(mDeviceFeatures));
	//mDeviceFeatures.shaderFloat64 = VK_TRUE;
	mDeviceFeatures.shaderInt16 = VK_TRUE;
	mDeviceFeatures.samplerAnisotropy = VK_TRUE;
	mDeviceFeatures.depthClamp = VK_TRUE;
	mDeviceFeatures.fillModeNonSolid = VK_TRUE;
	mDeviceFeatures.shaderUniformBufferArrayDynamicIndexing = VK_TRUE;
	mDeviceFeatures.shaderSampledImageArrayDynamicIndexing = VK_TRUE;
	mDeviceFeatures.shaderStorageBufferArrayDynamicIndexing = VK_TRUE;
	mDeviceFeatures.shaderStorageImageArrayDynamicIndexing = VK_TRUE;

	mPhysicalDevice = nullptr;
	mLogicalDevice = nullptr;
	mGraphicsQueue = nullptr;
	mGraphicsQueueForSingleTime = nullptr;
	mPresentQueue = nullptr;
	mCommandPool = nullptr;
	mCommandPoolForSingleTime = nullptr;

	memset(&mAllocCallback, 0, sizeof(mAllocCallback));
	mAllocCallback.pfnAllocation = &IVKRenderContext::vkAllocationFunction;
	mAllocCallback.pfnFree = &IVKRenderContext::vkFreeFunction;
	mAllocCallback.pfnInternalAllocation = &IVKRenderContext::vkInternalAllocationNotification;
	mAllocCallback.pfnInternalFree = &IVKRenderContext::vkInternalFreeNotification;
	mAllocCallback.pfnReallocation = &IVKRenderContext::vkReallocationFunction;

	mPipelineManager.WeakRef(new IVKRenderPipelineManager(this));
}

IVKRenderContext::~IVKRenderContext()
{
	VkExecuteAll(this, true);
	mPipelineManager->Cleanup();
	if (mGraphicsQueue != nullptr)
	{
		mGraphicsQueue = nullptr;
	}
	if (mGraphicsQueueForSingleTime != nullptr)
	{
		mGraphicsQueueForSingleTime = nullptr;
	}
	if (mPresentQueue != nullptr)
	{
		mPresentQueue = nullptr;
	}
	if (mCommandPool != nullptr)
	{
		vkDestroyCommandPool(mLogicalDevice, mCommandPool, nullptr);
		mCommandPool = nullptr;
	}
	if (mCommandPoolForSingleTime != nullptr)
	{
		vkDestroyCommandPool(mLogicalDevice, mCommandPoolForSingleTime, nullptr);
		mCommandPoolForSingleTime = nullptr;
	}
	if (mLogicalDevice != nullptr)
	{
		vkDestroyDevice(mLogicalDevice, nullptr);
		mLogicalDevice = nullptr;
	}
	if (mPhysicalDevice != nullptr)
	{
		mPhysicalDevice = nullptr;
	}
}

void IVKRenderContext::InitDescLayout(VkDevice device)
{
	VkDescriptorSetLayoutBinding uboLayoutBinding[MaxCB];
	for (int i = 0; i < MaxCB; i++)
	{
		uboLayoutBinding[i].binding = i;
		uboLayoutBinding[i].descriptorCount = 1;
		uboLayoutBinding[i].descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
		uboLayoutBinding[i].pImmutableSamplers = nullptr;
		uboLayoutBinding[i].stageFlags = VK_SHADER_STAGE_VERTEX_BIT;
	}
	VkDescriptorSetLayoutCreateInfo layoutInfo = {};
	layoutInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
	layoutInfo.bindingCount = MaxCB;
	layoutInfo.pBindings = uboLayoutBinding;
	if (vkCreateDescriptorSetLayout(device, &layoutInfo, nullptr, &mCBDescSetLayout) != VK_SUCCESS)
	{
		return;
	}
}

bool IVKRenderContext::Init(const IRenderContextDesc* desc, IVKRenderSystem* pSys, VkPhysicalDevice device, VkSurfaceKHR surface)
{
	mRenderSystem.FromObject(pSys);
	mPhysicalDevice = device;
	auto family = pSys->FindQueueFamilies(device, surface);

	std::vector<VkDeviceQueueCreateInfo> queueCreateInfos;
	std::set<uint32_t> uniqueQueueFamilies = { family.graphicsFamily, family.presentFamily };

	float queuePriority = 1.0f;
	for (uint32_t queueFamily : uniqueQueueFamilies)
	{
		VkDeviceQueueCreateInfo queueCreateInfo = {};
		queueCreateInfo.sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
		queueCreateInfo.queueFamilyIndex = queueFamily;
		queueCreateInfo.queueCount = 1;
		queueCreateInfo.pQueuePriorities = &queuePriority;
		queueCreateInfos.push_back(queueCreateInfo);
	}

	uint32_t extensionCount;
	vkEnumerateDeviceExtensionProperties(device, nullptr, &extensionCount, nullptr);
	std::vector<VkExtensionProperties> availableExtensions(extensionCount);
	vkEnumerateDeviceExtensionProperties(device, nullptr, &extensionCount, availableExtensions.data());

	std::vector<const char*>	mDeviceExtensions;
	for (auto& i : availableExtensions)
	{
		mDeviceExtensions.push_back(i.extensionName);
	}
	mDeviceExtensions.clear();
	mDeviceExtensions.push_back(VK_KHR_SWAPCHAIN_EXTENSION_NAME);
	mDeviceExtensions.push_back(VK_KHR_16BIT_STORAGE_EXTENSION_NAME);
	mDeviceExtensions.push_back(VK_KHR_SHADER_FLOAT16_INT8_EXTENSION_NAME);
	//mDeviceExtensions.push_back(VK_EXT_ROBUSTNESS_2_EXTENSION_NAME);
	
	//mDeviceExtensions.push_back(VK_AMD_GPU_SHADER_HALF_FLOAT_EXTENSION_NAME);
	VkPhysicalDeviceInheritedViewportScissorFeaturesNV dynScissorFeatures{};
	dynScissorFeatures.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_INHERITED_VIEWPORT_SCISSOR_FEATURES_NV;
	
	/*VkPhysicalDeviceRobustness2FeaturesEXT robustFeatures{};
	robustFeatures.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_ROBUSTNESS_2_PROPERTIES_EXT;
	robustFeatures.pNext = &dynScissorFeatures;*/

	/*VkPhysicalDeviceShaderFloat16Int8Features f16i8Features{};
	f16i8Features.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_SHADER_FLOAT16_INT8_FEATURES_KHR;
	f16i8Features.pNext = &robustFeatures;*/
	
	VkPhysicalDeviceFeatures2 features2{};
	features2.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_FEATURES_2_KHR;
	features2.pNext = &dynScissorFeatures;// &f16i8Features;
	vkGetPhysicalDeviceFeatures2(device, &features2);

	vkGetPhysicalDeviceProperties(device, &mDeviceProperties);
	//f16i8Features.shaderFloat16 = VK_TRUE;
	//robustFeatures.nullDescriptor = VK_TRUE;
	//robustFeatures.robustBufferAccess2 = VK_TRUE;
	//robustFeatures.robustImageAccess2 = VK_TRUE;
	//robustFeatures.pNext = &devfeatures11;

	VkPhysicalDeviceVulkan11Features devfeatures11{};
	devfeatures11.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_VULKAN_1_1_FEATURES;
	devfeatures11.pNext = nullptr;//&dynScissorFeatures;
	devfeatures11.multiview = VK_TRUE;
	//devfeatures11.storageInputOutput16 = VK_TRUE;
	devfeatures11.uniformAndStorageBuffer16BitAccess = VK_TRUE;
	//devfeatures11.storagePushConstant16 = VK_TRUE;	
	
	VkPhysicalDeviceVulkan12Features devfeatures12{};
	devfeatures12.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_VULKAN_1_2_FEATURES;
	//devfeatures12.shaderFloat16 = VK_TRUE;
	//devfeatures12.shaderInt8 = VK_TRUE;
	//devfeatures12.pNext = &devfeatures11;

	VkDeviceCreateInfo createInfo = {};
	createInfo.sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;
	createInfo.pNext = &devfeatures12;

	createInfo.queueCreateInfoCount = static_cast<uint32_t>(queueCreateInfos.size());
	createInfo.pQueueCreateInfos = queueCreateInfos.data();

	createInfo.pEnabledFeatures = &mDeviceFeatures;

	createInfo.enabledExtensionCount = static_cast<uint32_t>(mDeviceExtensions.size());
	createInfo.ppEnabledExtensionNames = mDeviceExtensions.data();

	if (desc->CreateDebugLayer) 
	{
		createInfo.enabledLayerCount = static_cast<uint32_t>(pSys->mValidationLayers.size());
		createInfo.ppEnabledLayerNames = pSys->mValidationLayers.data();
	}
	else 
	{
		createInfo.enabledLayerCount = 0;
	}

	if (vkCreateDevice(device, &createInfo, GetVkAllocCallBacks(), &mLogicalDevice) != VK_SUCCESS) 
	{
		ASSERT(false);
		return false;
	}

	vkGetDeviceQueue(mLogicalDevice, family.graphicsFamily, 0, &mGraphicsQueue);
	vkGetDeviceQueue(mLogicalDevice, family.graphicsFamily, 0, &mGraphicsQueueForSingleTime);
	vkGetDeviceQueue(mLogicalDevice, family.presentFamily, 0, &mPresentQueue);

	VkCommandPoolCreateInfo poolInfo = {};
	poolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
	poolInfo.queueFamilyIndex = family.graphicsFamily;
	poolInfo.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;

	if (vkCreateCommandPool(mLogicalDevice, &poolInfo, GetVkAllocCallBacks(), &mCommandPool) != VK_SUCCESS) 
	{
		return false;
	}

	if (vkCreateCommandPool(mLogicalDevice, &poolInfo, GetVkAllocCallBacks(), &mCommandPoolForSingleTime) != VK_SUCCESS)
	{
		return false;
	}

	mDescriptorPool.WeakRef(new IVKDescriptorPool());
	mDescriptorPool->InitPool(this, 1024 * 16, MaxCB * 1024, MaxTexOrSamplerSlot * 1024, MaxTexOrSamplerSlot * 1024, MaxTexOrSamplerSlot * 512, MaxTexOrSamplerSlot * 512);

	mImmCmdList.WeakRef(new IVKCommandList());
	ICommandListDesc cmdDesc{};
	mImmCmdList->Init(this, &cmdDesc);

	ITexture2DDesc txDesc;
	txDesc.SetDefault();
	txDesc.Width = 1;
	txDesc.Height = 1;
	auto pTexture = this->CreateTexture2D(&txDesc);
	IShaderResourceViewDesc srvDesc;
	srvDesc.SetTexture2D();
	srvDesc.mGpuBuffer = pTexture;
	mNullRsv.WeakRef((IVKShaderResourceView*)this->CreateShaderResourceView(&srvDesc));
	pTexture->Release();

	IConstantBufferDesc cbDesc;
	cbDesc.CBufferLayout = MakeWeakRef(new IConstantBufferLayout());
	cbDesc.SetDefault();
	cbDesc.Size = 1;
	mNullCBuffer.WeakRef((IVKConstantBuffer*)this->CreateConstantBuffer(&cbDesc));

	ISamplerStateDesc smpDesc;
	smpDesc.SetDefault();
	mNullSampler.WeakRef((IVKSamplerState*)this->CreateSamplerState(&smpDesc));

	return true;
}

ICommandList* IVKRenderContext::GetImmCommandList()
{
	if (mImmCmdList->mIsRecording == false)
	{
		mImmCmdList->BeginCommand();
	}
	return mImmCmdList;
}

void IVKRenderContext::FlushImmContext()
{
	if (mImmCmdList->mIsRecording)
	{
		mImmCmdList->EndCommand();
		mImmCmdList->Commit(this);
	}
	VkExecuteAll(this, false);
}


ISwapChain* IVKRenderContext::CreateSwapChain(const ISwapChainDesc* desc)
{
	auto swapchain = new IVKSwapChain();
	if (swapchain->Init(this, desc) == false)
	{
		swapchain->Release();
		return nullptr;
	}
	return swapchain;
}

ICommandList* IVKRenderContext::CreateCommandList(const ICommandListDesc* desc)
{
	auto cmd_list = new IVKCommandList();
	if (cmd_list->Init(this, desc) == false)
	{
		cmd_list->Release();
		return nullptr;
	}
	return cmd_list;
}


IDrawCall* IVKRenderContext::CreateDrawCall()
{
	auto pass = new IVKDrawCall();
	if (pass->Init(this, nullptr) == false)
	{
		pass->Release();
		return nullptr;
	}
	return pass;
}

IComputeDrawcall* IVKRenderContext::CreateComputeDrawcall()
{
	auto pass = new IVKComputeDrawcall();
	if (pass->Init(this) == false)
	{
		pass->Release();
		return nullptr;
	}
	return pass;
}

ICopyDrawcall* IVKRenderContext::CreateCopyDrawcall()
{
	auto pass = new IVKCopyDrawcall();
	/*if (pass->Init(this) == false)
	{
		pass->Release();
		return nullptr;
	}*/
	return pass;
}

IRenderPipeline* IVKRenderContext::CreateRenderPipeline(const IRenderPipelineDesc* desc)
{
	auto rpl = new IVKRenderPipeline();
	if (rpl->Init(this, desc) == false)
	{
		rpl->Release();
		return nullptr;
	}
	rpl->BindRenderPass(desc->RenderPass);
	rpl->BindBlendState(desc->Blend);
	rpl->BindDepthStencilState(desc->DepthStencil);
	rpl->BindRasterizerState(desc->Rasterizer);
	rpl->BindGpuProgram(desc->GpuProgram);
	return rpl;
}

IVertexBuffer* IVKRenderContext::CreateVertexBuffer(const IVertexBufferDesc* desc)
{
	auto result = new IVKVertexBuffer();
	if (false == result->Init(this, desc))
	{
		result->Release();
		return nullptr;
	}
	return result;
}

IIndexBuffer* IVKRenderContext::CreateIndexBuffer(const IIndexBufferDesc* desc)
{
	auto result = new IVKIndexBuffer();
	if (false == result->Init(this, desc))
	{
		result->Release();
		return nullptr;
	}
	return result;
}

IIndexBuffer* IVKRenderContext::CreateIndexBufferFromBuffer(const IIndexBufferDesc* desc, const IGpuBuffer* pBuffer)
{
	auto result = new IVKIndexBuffer();
	if (false == result->Init(this, (IVKGpuBuffer*)pBuffer))
	{
		result->Release();
		return nullptr;
	}
	return result;
}

IVertexBuffer* IVKRenderContext::CreateVertexBufferFromBuffer(const IVertexBufferDesc* desc, const IGpuBuffer* pBuffer)
{
	auto result = new IVKVertexBuffer();
	if (false == result->Init(this, (IVKGpuBuffer*)pBuffer))
	{
		result->Release();
		return nullptr;
	}
	return result;
}

IGeometryMesh* IVKRenderContext::CreateGeometryMesh()
{
	return new IVKGeometryMesh();
}

IRenderPass* IVKRenderContext::CreateRenderPass(const IRenderPassDesc* desc)
{
	auto result = new IVKRenderPass();
	if (false == result->Init(this, desc))
	{
		result->Release();
		return nullptr;
	}
	return result;
}

IFrameBuffers* IVKRenderContext::CreateFrameBuffers(const IFrameBuffersDesc* desc)
{
	auto rt = new IVKFrameBuffers();
	if (false == rt->Init(this, desc))
	{
		rt->Release();
		return nullptr;
	}
	return rt;
}

IRenderTargetView* IVKRenderContext::CreateRenderTargetView(const IRenderTargetViewDesc* desc)
{
	auto rt = new IVKRenderTargetView();
	if (false == rt->Init(this, desc))
	{
		rt->Release();
		return nullptr;
	}
	return rt;
}

IDepthStencilView* IVKRenderContext::CreateDepthRenderTargetView(const IDepthStencilViewDesc* desc)
{
	auto drt = new IVKDepthStencilView();
	if (false == drt->Init(this, desc))
	{
		drt->Release();
		return nullptr;
	}
	return drt;
}

ITexture2D* IVKRenderContext::CreateTexture2D(const ITexture2DDesc* desc)
{
	auto result = new IVKTexture2D();
	if (false == result->Init(this, desc))
	{
		result->Release();
		return nullptr;
	}
	return result;
}
IShaderResourceView* IVKRenderContext::CreateShaderResourceView(const IShaderResourceViewDesc* desc)
{
	auto result = new IVKShaderResourceView();
	if (false == result->Init(this, desc))
	{
		result->Release();
		return nullptr;
	}
	return result;
}
//IShaderResourceView* IVKRenderContext::CreateShaderResourceViewFromBuffer(IGpuBuffer* pBuffer, const ISRVDesc* desc)
//{
//	return nullptr;
//}
IGpuBuffer* IVKRenderContext::CreateGpuBuffer(const IGpuBufferDesc* desc, void* pInitData)
{
	auto result = new IVKGpuBuffer();
	if (false == result->Init(this, desc))
	{
		result->Release();
		return nullptr;
	}
	return result;
}
IUnorderedAccessView* IVKRenderContext::CreateUnorderedAccessView(IGpuBuffer* pBuffer, const IUnorderedAccessViewDesc* desc)
{
	auto result = new IVKUnorderedAccessView();
	if (false == result->Init(this, (IVKGpuBuffer*)pBuffer, desc))
	{
		result->Release();
		return nullptr;
	}
	return result;
}
IShaderResourceView* IVKRenderContext::LoadShaderResourceView(const char* file)
{
	return nullptr;
}
ISamplerState* IVKRenderContext::CreateSamplerState(const ISamplerStateDesc* desc)
{
	auto result = new IVKSamplerState();
	if (false == result->Init(this, desc))
	{
		result->Release();
		return nullptr;
	}
	return result;
}
IRasterizerState* IVKRenderContext::CreateRasterizerState(const IRasterizerStateDesc* desc)
{
	auto result = new IVKRasterizerState();
	if (false == result->Init(this, desc))
	{
		result->Release();
		return nullptr;
	}
	return result;
}
IDepthStencilState* IVKRenderContext::CreateDepthStencilState(const IDepthStencilStateDesc* desc)
{
	auto result = new IVKDepthStencilState();
	if (false == result->Init(this, desc))
	{
		result->Release();
		return nullptr;
	}
	return result;
}
IBlendState* IVKRenderContext::CreateBlendState(const IBlendStateDesc* desc)
{
	auto result = new IVKBlendState();
	if (false == result->Init(this, desc))
	{
		result->Release();
		return nullptr;
	}
	return result;
}
//shader
IShaderProgram* IVKRenderContext::CreateShaderProgram(const IShaderProgramDesc* desc)
{
	auto program = new IVKShaderProgram();
	if (program->Init(this, desc) == false)
	{
		program->Release();
		return nullptr;
	}
	return program;
}

IVertexShader* IVKRenderContext::CreateVertexShader(const IShaderDesc* desc)
{
	auto vs = new IVKVertexShader();
	if (false == vs->Init(this, desc))
	{
		vs->Release();
		return nullptr;
	}
	return vs;
}

IPixelShader* IVKRenderContext::CreatePixelShader(const IShaderDesc* desc)
{
	auto ps = new IVKPixelShader();
	if (false == ps->Init(this, desc))
	{
		ps->Release();
		return nullptr;
	}
	return ps;
}

IComputeShader* IVKRenderContext::CreateComputeShader(const IShaderDesc* desc)
{
	auto result = new IVKComputeShader();
	if (false == result->Init(this, desc))
	{
		result->Release();
		return nullptr;
	}
	return result;
}

IInputLayout* IVKRenderContext::CreateInputLayout(const IInputLayoutDesc* desc)
{
	auto layout = new IVKInputLayout();
	if (false == layout->Init(this, desc))
	{
		layout->Release();
		return nullptr;
	}
	return layout;
}

IConstantBuffer* IVKRenderContext::CreateConstantBuffer(const IConstantBufferDesc* desc)
{
	auto cb = new IVKConstantBuffer();
	if (cb->Init(this, desc) == false)
	{
		cb->Release();
		return nullptr;
	}
	return cb;
}

IFence* IVKRenderContext::CreateFence()
{
	auto cb = new IVKFence();
	if (cb->Init(this) == false)
	{
		cb->Release();
		return nullptr;
	}
	return cb;
}

ISemaphore* IVKRenderContext::CreateGpuSemaphore()
{
	auto cb = new IVKSemaphore();
	if (cb->Init(this) == false)
	{
		cb->Release();
		return nullptr;
	}
	return cb;
}

NS_END