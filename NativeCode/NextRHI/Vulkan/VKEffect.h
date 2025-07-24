#pragma once
#include "../NxEffect.h"
#include "VKPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class VKLayoutWrapper : public IGpuResource
	{
	public:
		VKGpuDevice* mDeviceRef = nullptr;
		std::vector<VkDescriptorSetLayoutBinding> mBindings;
		VkDescriptorSetLayout mLayout = nullptr;

		static VkDescriptorType GetDescriptorType(const FShaderBinder* binder);
		static VkShaderStageFlagBits GetShaderStage(const FShaderBinder* binder);
		static void BuildBindings(std::vector<VkDescriptorSetLayoutBinding>& bindings, const std::vector<AutoRef<const FShaderBinder>>& binders);
		static void BuildBindings(std::vector<VkDescriptorSetLayoutBinding>& bindings, const FShaderBinder* pBinder);
;
		~VKLayoutWrapper();
		bool Initialize(VKGpuDevice* device, std::vector<VkDescriptorSetLayoutBinding>& bindings);
	};

	class VKGraphicsEffect : public IGraphicsEffect
	{
	public:
		VKGraphicsEffect();
		~VKGraphicsEffect();
		virtual void BuildState(IGpuDevice* device) override;
		virtual void Commit(ICommandList* cmdlist, IGraphicDraw* drawcall) override;

		void PushLayout(AutoRef<VKLayoutWrapper>& tmp, const FShaderBinder* pBinder)
		{
			((FShaderBinder*)pBinder)->DescriptorIndex = (UINT)mLayouts.size();
			mLayouts.push_back(tmp);
		}
	public:
		TWeakRefHandle<VKGpuDevice>		mDeviceRef;
		
		std::vector<AutoRef<VKLayoutWrapper>> mLayouts;
		VkPipelineLayout				mPipelineLayout = nullptr;
	};

	class VKComputeEffect : public IComputeEffect
	{
	public:
		VKComputeEffect();
		~VKComputeEffect();
		virtual void BuildState(IGpuDevice* device) override;
		virtual void Commit(ICommandList* cmdlist) override;

		void PushLayout(AutoRef<VKLayoutWrapper>& tmp, const FShaderBinder* pBinder)
		{
			((FShaderBinder*)pBinder)->DescriptorIndex = (UINT)mLayouts.size();
			mLayouts.push_back(tmp);
		}
	public:
		TWeakRefHandle<VKGpuDevice>		mDeviceRef;

		std::vector<AutoRef<VKLayoutWrapper>> mLayouts;
		
		VkPipelineLayout				mPipelineLayout = nullptr;
		VkPipeline						mComputePipeline = nullptr;
	};
}

NS_END
