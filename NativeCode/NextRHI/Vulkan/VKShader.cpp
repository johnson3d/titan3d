#include "VKShader.h"
#include "VKGpuDevice.h"
#include "VKBuffer.h"
#include "VKGpuState.h"
#include "../NxRHIDefine.h"
#include "../../Bricks/CrossShaderCompiler/IShaderConductor.h"
#include "../../../3rd/native/spirv-cross/spirv_cross_c.h"
//#include <spirv_cross/spirv_cross_c.h>

#if defined(HasModule_GpuDump)
#include "../../Bricks/GpuDump/NvAftermath.h"
#endif

#define new VNEW

NS_BEGIN
namespace NxRHI
{
	template<>
	struct AuxGpuResourceDestroyer<VkShaderModule>
	{
		static void Destroy(VkShaderModule obj, IGpuDevice* device1)
		{
			auto device = (VKGpuDevice*)device1;
			vkDestroyShaderModule(device->mDevice, obj, device->GetVkAllocCallBacks());
		}
	};
	template<>
	struct AuxGpuResourceDestroyer<VkDescriptorSetLayout>
	{
		static void Destroy(VkDescriptorSetLayout obj, IGpuDevice* device1)
		{
			auto device = (VKGpuDevice*)device1;
			vkDestroyDescriptorSetLayout(device->mDevice, obj, device->GetVkAllocCallBacks());
		}
	};
	MemAlloc::FPage<VkDescriptorSet>* VKDescriptorSetCreator::CreatePage(UINT pageSize)
	{
		std::vector<VkDescriptorPoolSize> psz;
		if (NumOfUbo > 0)
		{
			VkDescriptorPoolSize tmp{};
			tmp.type = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
			tmp.descriptorCount = NumOfUbo * pageSize;
			psz.push_back(tmp);
		}

		if (NumOfSampler > 0)
		{
			VkDescriptorPoolSize tmp{};
			tmp.type = VK_DESCRIPTOR_TYPE_SAMPLER;
			tmp.descriptorCount = NumOfSampler * pageSize;
			psz.push_back(tmp);
		}

		if (NumOfSsbo > 0)
		{
			VkDescriptorPoolSize tmp{};
			tmp.type = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
			tmp.descriptorCount = NumOfSsbo * pageSize;
			psz.push_back(tmp);
		}

		if (NumOfImage > 0)
		{
			VkDescriptorPoolSize tmp{};
			tmp.type = VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
			tmp.descriptorCount = NumOfImage * pageSize;
			psz.push_back(tmp);
		}

		if (NumOfStorageImage > 0)
		{
			VkDescriptorPoolSize tmp{};
			tmp.type = VK_DESCRIPTOR_TYPE_STORAGE_IMAGE;
			tmp.descriptorCount = NumOfStorageImage * pageSize;
			psz.push_back(tmp);
		}

		VkDescriptorPoolCreateInfo poolInfo = {};
		poolInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO;
		poolInfo.poolSizeCount = (UINT)psz.size();
		if (poolInfo.poolSizeCount > 0)
			poolInfo.pPoolSizes = &psz[0];
		poolInfo.maxSets = pageSize;

		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return nullptr;
		VkDescriptorPool descPool;
		if (vkCreateDescriptorPool(device->mDevice, &poolInfo, device->GetVkAllocCallBacks(), &descPool) != VK_SUCCESS)
		{
			return nullptr;
		}

		auto result = new VKDescriptorSetPage();
		result->mDescriptorPool = descPool;
		return result;
	}
	MemAlloc::FPagedObject<VkDescriptorSet>* VKDescriptorSetCreator::CreatePagedObject(MemAlloc::FPage<VkDescriptorSet>* page, UINT index)
	{
		auto device = mDeviceRef.GetPtr();

		VkDescriptorSetAllocateInfo allocInfo{};
		allocInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO;
		allocInfo.descriptorPool = ((VKDescriptorSetPage*)page)->mDescriptorPool;
		allocInfo.descriptorSetCount = 1;
		allocInfo.pSetLayouts = &Shader->mLayout;

		VkDescriptorSet ds;
		if (vkAllocateDescriptorSets(device->mDevice, &allocInfo, &ds) != VK_SUCCESS)
		{
			return nullptr;
		}

		auto result = new VKDescriptorSetPagedObject();
		result->RealObject = ds;

		return result;
	}
	void VKDescriptorSetCreator::OnFree(MemAlloc::FPagedObject<VkDescriptorSet>* obj)
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		for (auto& i : Shader->mLayoutBindings)
		{
			VkWriteDescriptorSet descriptorWrite = {};
			VkDescriptorImageInfo tmp{};
			VkDescriptorBufferInfo tmpStructureBuffer{};
			descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			descriptorWrite.dstSet = obj->RealObject;

			descriptorWrite.dstBinding = i.binding;
			descriptorWrite.dstArrayElement = 0;
			descriptorWrite.descriptorCount = 1;

			switch (i.descriptorType)
			{
				case VkDescriptorType::VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER:
				{
					tmpStructureBuffer.buffer = device->mNullUBO->mBuffer;
					tmpStructureBuffer.range = VK_WHOLE_SIZE;
					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
					descriptorWrite.pBufferInfo = &tmpStructureBuffer;
					vkUpdateDescriptorSets(device->mDevice, 1, &descriptorWrite, 0, nullptr);
					break;
				}
				case VkDescriptorType::VK_DESCRIPTOR_TYPE_STORAGE_BUFFER:
				{
					tmpStructureBuffer.buffer = device->mNullSSBO->mBuffer;
					tmpStructureBuffer.range = VK_WHOLE_SIZE;
					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
					descriptorWrite.pBufferInfo = &tmpStructureBuffer;
					vkUpdateDescriptorSets(device->mDevice, 1, &descriptorWrite, 0, nullptr);
					break;
				}
				case VkDescriptorType::VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE:
				{
					tmp.imageLayout = ((VKTexture*)device->mNullSampledImage->GetBuffer())->GetImageLayout();
					tmp.imageView = (VkImageView)device->mNullSampledImage->GetHWBuffer();
					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
					descriptorWrite.pImageInfo = &tmp;
					vkUpdateDescriptorSets(device->mDevice, 1, &descriptorWrite, 0, nullptr);
					break;
				}
				case VkDescriptorType::VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER:
				{
					tmp.imageLayout = VK_IMAGE_LAYOUT_ATTACHMENT_OPTIMAL;
					tmp.imageView = (VkImageView)device->mNullSampledImage->GetHWBuffer();
					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER;
					descriptorWrite.pImageInfo = &tmp;
					vkUpdateDescriptorSets(device->mDevice, 1, &descriptorWrite, 0, nullptr);
					break;
				}
				case VkDescriptorType::VK_DESCRIPTOR_TYPE_SAMPLER:
				{
					tmp.sampler = (VkSampler)device->mNullSampler->GetHWBuffer();
					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLER;
					descriptorWrite.pImageInfo = &tmp;
					vkUpdateDescriptorSets(device->mDevice, 1, &descriptorWrite, 0, nullptr);
					break;
				}
				default:
					break;
			}
		}
	}
	void VKDescriptorSetCreator::FinalCleanup(MemAlloc::FPage<VkDescriptorSet>* page)
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		auto pPage = (VKDescriptorSetPage*)page;
		vkDestroyDescriptorPool(device->mDevice, pPage->mDescriptorPool, device->GetVkAllocCallBacks());
		pPage->mDescriptorPool = nullptr;
	}

	static void FillRangeVK(std::vector<VkDescriptorSetLayoutBinding>* pOutRanges, FShaderBinder* pBinder, VkDescriptorType type, VkShaderStageFlagBits shaderStage)
	{
		if (pBinder == nullptr)
			return;
		VkDescriptorSetLayoutBinding binding{};
		binding.binding = pBinder->Slot;
		binding.descriptorCount = 1;
		binding.descriptorType = type;
		binding.pImmutableSamplers = nullptr;
		binding.stageFlags = shaderStage;
		pOutRanges->push_back(binding);
	}

	bool VKShader::CompileShader(FShaderCompiler* compiler, FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader)
	{
		desc->FunctionName = entry;
		return IShaderConductor::GetInstance()->CompileShader(compiler, desc, shader, entry, type, sm, defines, bDebugShader, sl, bDebugShader);
	}

	VKShader::VKShader()
	{
		
	}
	VKShader::~VKShader()
	{
		mDescriptorSetAllocator.FinalCleanup();
		mDescriptorSetAllocator.Creator.Shader = nullptr;

		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;

		device->DelayDestroy(mShader);
		mShader = nullptr;

		if (mLayout != nullptr)
		{
			device->DelayDestroy(mLayout);
			mLayout = nullptr;
		}
	}
	bool VKShader::Init(VKGpuDevice* device, FShaderDesc* desc)
	{
		Desc = desc;
		if (Desc->SpirV.size() == 0)
			return false;

		//Reflect(desc);
		Reflector = desc->SpirvReflector;

		VkShaderModuleCreateInfo createInfo{};
		createInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
		createInfo.codeSize = desc->SpirV.size();
		createInfo.pCode = reinterpret_cast<const uint32_t*>(&desc->SpirV[0]);

		if (vkCreateShaderModule(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mShader) != VK_SUCCESS)
		{
			return false;
		}

#if defined(HasModule_GpuDump)
		GpuDump::NvAftermath::RegSpirv(&desc->SpirV[0], (UINT)createInfo.codeSize);
#endif

		VkShaderStageFlagBits shaderStage = (VkShaderStageFlagBits)0;
		switch (Desc->Type)
		{
			case EShaderType::SDT_VertexShader:
				shaderStage = (VkShaderStageFlagBits)(VK_SHADER_STAGE_VERTEX_BIT | VK_SHADER_STAGE_FRAGMENT_BIT);
				break;
			case EShaderType::SDT_PixelShader:
				shaderStage = (VkShaderStageFlagBits)(VK_SHADER_STAGE_FRAGMENT_BIT | VK_SHADER_STAGE_VERTEX_BIT);
				break;
			case EShaderType::SDT_ComputeShader:
				shaderStage = VK_SHADER_STAGE_COMPUTE_BIT;
				break;
			default:
				break;
		} 
		
		for (auto& i : Reflector->CBuffers)
		{
			auto pBinder = (FShaderBinder*)i;
			FillRangeVK(&mLayoutBindings, pBinder, VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, shaderStage);
		}

		for (auto& i : Reflector->Srvs)
		{
			auto pBinder = (FShaderBinder*)i;
			if (pBinder->IsStructuredBuffer)
			{
				FillRangeVK(&mLayoutBindings, pBinder, VK_DESCRIPTOR_TYPE_STORAGE_BUFFER, shaderStage);
			}
			else
			{
				FillRangeVK(&mLayoutBindings, pBinder, VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE, shaderStage);
			}
		}

		for (auto& i : Reflector->Uavs)
		{
			auto pBinder = (FShaderBinder*)i;
			if (pBinder->IsStructuredBuffer)
			{
				FillRangeVK(&mLayoutBindings, pBinder, VK_DESCRIPTOR_TYPE_STORAGE_BUFFER, shaderStage);
			}
			else
			{
				FillRangeVK(&mLayoutBindings, pBinder, VK_DESCRIPTOR_TYPE_STORAGE_IMAGE, shaderStage);
			}
		}

		for (auto& i : Reflector->Samplers)
		{
			auto pBinder = (FShaderBinder*)i;
			FillRangeVK(&mLayoutBindings, pBinder, VK_DESCRIPTOR_TYPE_SAMPLER, shaderStage);
		}

		VkDescriptorSetLayoutCreateInfo layoutInfo{};
		layoutInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
		layoutInfo.bindingCount = (UINT)mLayoutBindings.size();
		if (layoutInfo.bindingCount != 0)
		{
			layoutInfo.pBindings = &mLayoutBindings[0];
			if (vkCreateDescriptorSetLayout(device->mDevice, &layoutInfo, device->GetVkAllocCallBacks(), &mLayout) != VK_SUCCESS)
			{
				ASSERT(false);
				return false;
			}
		}
		else
		{
			layoutInfo.pBindings = nullptr;
			if (vkCreateDescriptorSetLayout(device->mDevice, &layoutInfo, device->GetVkAllocCallBacks(), &mLayout) != VK_SUCCESS)
			{
				ASSERT(false);
				return false;
			}
		}

		if (mLayout != nullptr)
		{
			mDescriptorSetAllocator.Creator.NumOfUbo = 0;
			for (const auto& i : mLayoutBindings)
			{
				if (i.descriptorType == VkDescriptorType::VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER)
				{
					mDescriptorSetAllocator.Creator.NumOfUbo++;
				}
			}
			mDescriptorSetAllocator.Creator.NumOfSsbo = 0;
			for (const auto& i : mLayoutBindings)
			{
				if (i.descriptorType == VkDescriptorType::VK_DESCRIPTOR_TYPE_STORAGE_BUFFER)
				{
					mDescriptorSetAllocator.Creator.NumOfSsbo++;
				}
			}
			mDescriptorSetAllocator.Creator.NumOfSampler = 0;
			for (const auto& i : mLayoutBindings)
			{
				if (i.descriptorType == VkDescriptorType::VK_DESCRIPTOR_TYPE_SAMPLER)
				{
					mDescriptorSetAllocator.Creator.NumOfSampler++;
				}
			}
			mDescriptorSetAllocator.Creator.NumOfImage = 0;
			for (const auto& i : mLayoutBindings)
			{
				if (i.descriptorType == VkDescriptorType::VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE)
				{
					mDescriptorSetAllocator.Creator.NumOfImage++;
				}
			}
			mDescriptorSetAllocator.Creator.NumOfStorageImage = 0;
			for (const auto& i : mLayoutBindings)
			{
				if (i.descriptorType == VkDescriptorType::VK_DESCRIPTOR_TYPE_STORAGE_IMAGE)
				{
					mDescriptorSetAllocator.Creator.NumOfStorageImage++;
				}
			}
		}

		mDescriptorSetAllocator.Creator.mDeviceRef.FromObject(device);
		mDescriptorSetAllocator.Creator.Shader = this;
		/*auto obj = testAllocator.Alloc();
		testAllocator.Free(obj);*/

		return true;
	}
	
	bool VKShader::Reflect(FShaderDesc* desc)
	{
		desc->SpirvReflector = MakeWeakRef(new IShaderReflector());

		auto Reflector = desc->SpirvReflector;

		spvc_context context = NULL;
		spvc_parsed_ir ir = NULL;
		spvc_compiler compiler_glsl = NULL;
		spvc_compiler_options options = NULL;
		spvc_resources resources = NULL;
		const spvc_reflected_resource* list = NULL;
		const char* result = NULL;
		size_t count;
		size_t i;

		const SpvId* spirv = (const SpvId*)& desc->SpirV[0];
		ASSERT(desc->SpirV.size() % sizeof(SpvId) == 0)
		size_t word_count = desc->SpirV.size() / sizeof(SpvId);

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
			auto type = GetStreamTypeByVKBinding(binding);
			if (type != VST_Number)
				desc->InputStreams.push_back(type);

			/*const spvc_reflected_builtin_resource* pInputs;
			size_t inputCount = 0;
			spvc_resources_get_builtin_resource_list_for_type(resources, SPVC_BUILTIN_RESOURCE_TYPE_STAGE_INPUT, &pInputs, &inputCount);
			for (int j = 0; j < inputCount; j++)
			{
				auto type = GetStreamType(pInputs[i].resource.id);
				if (type != VST_Number)
					desc->InputStreams.push_back(type);
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

			auto binder = MakeWeakRef(new FShaderBinder());
			binder->Name = name;
			binder->Space = descriptorSet;
			binder->Size = (UINT)sz;
			binder->Slot = binding;
			binder->Type = EShaderBindType::SBT_CBuffer;
			
			UINT NumOfMember = spvc_type_get_num_member_types(spv_type);
			for (UINT idx = 0; idx < NumOfMember; ++idx)
			{
				auto v = MakeWeakRef(new FShaderVarDesc());
				auto memberName = spvc_compiler_get_member_name(compiler_glsl, list[i].base_type_id, idx);
				size_t varSize;
				spvc_compiler_get_declared_struct_member_size(compiler_glsl, spv_type, idx, &varSize);
				spvc_compiler_type_struct_member_offset(compiler_glsl, spv_type, idx, (UINT*)&v->Offset);
				v->Name = memberName;
				v->Size = (UINT)varSize;

				UINT varStride = 0;
				auto ok = spvc_compiler_type_struct_member_array_stride(compiler_glsl, spv_type, idx, &varStride);
				if (SPVC_SUCCESS == ok)
					v->Elements = (USHORT)(varSize / varStride);//spvc_type_get_vector_size
				else
					v->Elements = 1;
				auto member_id = spvc_type_get_member_type(spv_type, idx);
				auto member_type = spvc_compiler_get_type_handle(compiler_glsl, member_id);
				auto baseType = spvc_type_get_basetype(member_type);
				auto vectorSize = spvc_type_get_columns(member_type);
				//ASSERT(v->Elements == vectorSize);
				auto cols = spvc_type_get_vector_size(member_type);
				v->Type = EShaderVarType::SVT_Unknown;
				v->Columns = (USHORT)cols;
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
					v->Type = EShaderVarType::SVT_Int;
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
					v->Type = EShaderVarType::SVT_Float;
					break;
				case SPVC_BASETYPE_FP64:
					break;
				case SPVC_BASETYPE_STRUCT:
					break;
				case SPVC_BASETYPE_IMAGE:
					v->Type = EShaderVarType::SVT_Texture;
					break;
				case SPVC_BASETYPE_SAMPLED_IMAGE:
					v->Type = EShaderVarType::SVT_Texture;
					break;
				case SPVC_BASETYPE_SAMPLER:
					v->Type = EShaderVarType::SVT_Sampler;
					break;
				case SPVC_BASETYPE_ACCELERATION_STRUCTURE:
					break;
				case SPVC_BASETYPE_INT_MAX:
					break;
				default:
					break;
				}
				binder->Fields.push_back(v);
			}
			Reflector->CBuffers.push_back(binder);
		}

		spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_STORAGE_BUFFER, &list, &count);
		for (i = 0; i < count; i++)
		{//UAV buffer:rwstructuredbuffer
			auto spv_type = spvc_compiler_get_type_handle(compiler_glsl, list[i].base_type_id);
			std::string decl_block_name = spvc_compiler_get_remapped_declared_block_name(compiler_glsl, list[i].id);
			auto name = spvc_compiler_get_name(compiler_glsl, list[i].id);
			auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
			auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);
			/*const SpvDecoration* decoration[10];
			size_t num = 0;
			spvc_compiler_get_buffer_block_decorations(compiler_glsl, list[i].id, decoration, &num);*/
			auto constant = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationNonWritable);

			auto binder = MakeWeakRef(new FShaderBinder());
			binder->Name = name;
			binder->Space = descriptorSet;
			binder->Size = 0;
			binder->Slot = binding;
			binder->IsStructuredBuffer = TRUE;

			if (decl_block_name.rfind("type.RWStructuredBuffer") == 0 ||
				decl_block_name.rfind("type.RWByteAddressBuffer") == 0)
			{
				binder->Type = EShaderBindType::SBT_UAV;
				Reflector->Uavs.push_back(binder);
			}
			else if (decl_block_name.rfind("type.StructuredBuffer") == 0 || 
				decl_block_name.rfind("type.ByteAddressBuffer") == 0)
			{
				binder->Type = EShaderBindType::SBT_SRV;
				Reflector->Srvs.push_back(binder);
			}
			else
			{
				ASSERT(false);
			}

			UINT NumOfMember = spvc_type_get_num_member_types(spv_type);
			for (UINT idx = 0; idx < NumOfMember; ++idx)
			{
				auto v = MakeWeakRef(new FShaderVarDesc());
				auto memberName = spvc_compiler_get_member_name(compiler_glsl, list[i].base_type_id, idx);
				size_t varSize;
				spvc_compiler_get_declared_struct_member_size(compiler_glsl, spv_type, idx, &varSize);
				spvc_compiler_type_struct_member_offset(compiler_glsl, spv_type, idx, (UINT*)&v->Offset);
				v->Name = memberName;
				v->Size = (UINT)varSize;

				UINT varStride = 0;
				auto ok = spvc_compiler_type_struct_member_array_stride(compiler_glsl, spv_type, idx, &varStride);
				if (SPVC_SUCCESS == ok)
					v->Elements = (USHORT)(varSize / varStride);//spvc_type_get_vector_size
				else
					v->Elements = 1;
				auto member_id = spvc_type_get_member_type(spv_type, idx);
				auto member_type = spvc_compiler_get_type_handle(compiler_glsl, member_id);
				auto baseType = spvc_type_get_basetype(member_type);
				auto vectorSize = spvc_type_get_columns(member_type);
				auto cols = spvc_type_get_vector_size(member_type);
				v->Type = EShaderVarType::SVT_Unknown;
				v->Columns = (USHORT)vectorSize;
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
					v->Type = EShaderVarType::SVT_Int;
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
					v->Type = EShaderVarType::SVT_Float;
					break;
				case SPVC_BASETYPE_FP64:
					break;
				case SPVC_BASETYPE_STRUCT:
					break;
				case SPVC_BASETYPE_IMAGE:
					v->Type = EShaderVarType::SVT_Texture;
					break;
				case SPVC_BASETYPE_SAMPLED_IMAGE:
					v->Type = EShaderVarType::SVT_Texture;
					break;
				case SPVC_BASETYPE_SAMPLER:
					v->Type = EShaderVarType::SVT_Sampler;
					break;
				case SPVC_BASETYPE_ACCELERATION_STRUCTURE:
					break;
				case SPVC_BASETYPE_INT_MAX:
					break;
				default:
					break;
				}
				binder->Fields.push_back(v);
			}
		}
		spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_STORAGE_IMAGE, &list, &count);
		for (i = 0; i < count; i++)
		{//UAV texture:rwtexture
			std::string decl_block_name = spvc_compiler_get_remapped_declared_block_name(compiler_glsl, list[i].id);
			std::string name = spvc_compiler_get_name(compiler_glsl, list[i].id);
			auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
			auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);
			auto constant = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationConstant);
			auto spv_type = spvc_compiler_get_type_handle(compiler_glsl, list[i].base_type_id);
			SpvAccessQualifier access = spvc_type_get_image_access_qualifier(spv_type);

			auto binder = MakeWeakRef(new FShaderBinder());
			
			binder->Name = name;
			binder->Space = descriptorSet;
			binder->Size = 0;
			binder->Slot = binding;
			binder->Type = EShaderBindType::SBT_UAV;
			binder->IsStructuredBuffer = FALSE;

			Reflector->Uavs.push_back(binder);
		}
		spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_SAMPLED_IMAGE, &list, &count);
		for (i = 0; i < count; i++)
		{//For GL:combine sampler&texture
			auto name = spvc_compiler_get_name(compiler_glsl, list[i].id);
			auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
			auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);

			{
				auto binder = MakeWeakRef(new FShaderBinder());
				binder->Name = name;
				binder->Space = descriptorSet;
				binder->Size = 0;
				binder->Slot = binding;
				binder->Type = EShaderBindType::SBT_SRV;
				binder->IsStructuredBuffer = FALSE;
				Reflector->Srvs.push_back(binder);
			}
			{
				auto binder = MakeWeakRef(new FShaderBinder());
				binder->Name = name;
				binder->Space = descriptorSet;
				binder->Size = 0;
				binder->Slot = binding;
				binder->Type = EShaderBindType::SBT_Sampler;
				Reflector->Samplers.push_back(binder);
			}
		}
		spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_SEPARATE_IMAGE, &list, &count);
		for (i = 0; i < count; i++)
		{//srv:texture 
			std::string name = spvc_compiler_get_name(compiler_glsl, list[i].id);
			auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
			auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);

			auto binder = MakeWeakRef(new FShaderBinder());
			binder->Name = name;
			binder->Space = descriptorSet;
			binder->Size = 0;
			binder->Slot = binding;
			binder->Type = EShaderBindType::SBT_SRV;
			binder->IsStructuredBuffer = FALSE;
			Reflector->Srvs.push_back(binder);
		}
		spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_SEPARATE_SAMPLERS, &list, &count);
		for (i = 0; i < count; i++)
		{//samplers
			auto name = spvc_compiler_get_name(compiler_glsl, list[i].id);
			auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
			auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);

			auto binder = MakeWeakRef(new FShaderBinder());
			binder->Name = name;
			binder->Space = descriptorSet;
			binder->Size = 0;
			binder->Slot = binding;
			binder->Type = EShaderBindType::SBT_Sampler;
			binder->IsStructuredBuffer = FALSE;
			Reflector->Samplers.push_back(binder);
		}

		spvc_context_destroy(context);

		return true;
	}
}
NS_END