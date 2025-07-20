#pragma once
#include "../NxEffect.h"
#include "VKPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class VKGraphicsEffect;
	class VKComputeEffect;
	class VKDescriptorSetLayoutBuilder
	{
	public:
		static VkDescriptorSetLayout Build(VKGpuDevice* device, VKGraphicsEffect* effect, std::vector<VkDescriptorSetLayoutBinding>& bindings);
		static VkDescriptorSetLayout Build(VKGpuDevice* device, VKComputeEffect* effect, std::vector<VkDescriptorSetLayoutBinding>& bindings);
		static VkDescriptorType GetDescriptorType(const FShaderBinder* binder);
	};
	class VKGraphicsEffect : public IGraphicsEffect
	{
	public:
		VKGraphicsEffect();
		~VKGraphicsEffect();
		virtual void BuildState(IGpuDevice* device) override;
		virtual void Commit(ICommandList* cmdlist, IGraphicDraw* drawcall) override;
	public:
		TWeakRefHandle<VKGpuDevice>		mDeviceRef;
		
		std::vector<VkDescriptorSetLayoutBinding> mBindings;
		VkDescriptorSetLayout			mLayout = nullptr;
		VkPipelineLayout				mPipelineLayout = nullptr;
	};

	class VKComputeEffect : public IComputeEffect
	{
	public:
		VKComputeEffect();
		~VKComputeEffect();
		virtual void BuildState(IGpuDevice* device) override;
		virtual void Commit(ICommandList* cmdlist) override;
	public:
		TWeakRefHandle<VKGpuDevice>		mDeviceRef;

		std::vector<VkDescriptorSetLayoutBinding> mBindings;
		VkDescriptorSetLayout			mLayout = nullptr;
		VkPipelineLayout				mPipelineLayout = nullptr;
		VkPipeline						mComputePipeline = nullptr;
	};
}

NS_END
