#pragma once
#include "../NxShader.h"
#include "VKPreHead.h"
#include "../../Base/allocator/PagedAllocator.h"

NS_BEGIN

namespace NxRHI
{
	class VKGpuDevice;
	class VKShader;
	
	class VKDescriptorSetHolder : public IGpuResource
	{
	public:
		~VKDescriptorSetHolder()
		{
			DescriptorSet->Free();
			UsedResources.clear();
		}
		AutoRef<MemAlloc::FPagedObject<VkDescriptorSet>> DescriptorSet;
		std::vector<AutoRef<IGpuResource>>		UsedResources;
		void UseResource(IGpuResource* resource)
		{
			if (resource != nullptr)
			{
				UsedResources.push_back(resource);
			}
		}
	};
	
	struct VKDescriptorSetCreator
	{
		struct VKDescriptorSetPage : public MemAlloc::FPage<VkDescriptorSet>
		{
			VkDescriptorPool				mDescriptorPool = (VkDescriptorPool)nullptr;
		};

		UINT		NumOfUbo = 0;
		UINT		NumOfSsbo = 0;
		UINT		NumOfSampler = 0;
		UINT		NumOfImage = 0;
		UINT		NumOfStorageImage = 0;

		using ObjectType = VkDescriptorSet;
		using PagedObjectType = MemAlloc::FPagedObject<ObjectType>;
		using PageType = VKDescriptorSetPage;// MemAlloc::FPage<ObjectType>;
		using AllocatorType = MemAlloc::FAllocatorBase<ObjectType>;

		TWeakRefHandle<VKGpuDevice>		mDeviceRef;
		VKShader*						Shader = nullptr;

		UINT GetPageSize() const {
			return PageSize;
		}
		UINT PageSize = 128;

		PageType* CreatePage(UINT pageSize);
		PagedObjectType* CreatePagedObject(PageType* page, UINT index);
		void OnAlloc(AllocatorType* pAllocator, PagedObjectType* obj);
		void OnFree(AllocatorType* pAllocator, PagedObjectType* obj);
		void FinalCleanup(MemAlloc::FPage<ObjectType>* page);
	};
	class FDescriptorSetAllocator : public MemAlloc::FPagedObjectAllocator<VkDescriptorSet, VKDescriptorSetCreator, true>
	{
	public:
		VKDescriptorSetHolder* AllocDecriptorSet()
		{
			auto result = new VKDescriptorSetHolder();
			result->DescriptorSet = this->Alloc();
			return result;
		}
	};

	class VKShader : public IShader
	{
	public:
		static bool CompileShader(FShaderCompiler* compiler, FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader, const char* extHlslVersion, const char* dxcArgs, IBlobObject* output, bool asModule);

		VKShader();
		~VKShader();
		bool Init(VKGpuDevice* device, FShaderDesc* desc);
		static bool Reflect(FShaderDesc* desc);
	public:
		TWeakRefHandle<VKGpuDevice>	mDeviceRef;
		VkShaderModule		mShader{};
		VNameString			mFunctionName;
		std::vector<VkDescriptorSetLayoutBinding>	mLayoutBindings;
		VkDescriptorSetLayout				mLayout = (VkDescriptorSetLayout)nullptr;
		FDescriptorSetAllocator				mDescriptorSetAllocator;
	};
}

NS_END