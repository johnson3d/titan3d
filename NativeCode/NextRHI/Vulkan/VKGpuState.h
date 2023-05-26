#pragma once
#include "../NxGpuState.h"
#include "VKPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class VKGpuDevice;
	class VKSampler : public ISampler
	{
	public:
		VKSampler();
		~VKSampler();
		bool Init(VKGpuDevice* device, const FSamplerDesc& desc);
		virtual void* GetHWBuffer() override {
			return mView;
		}
	public:
		TWeakRefHandle<VKGpuDevice>	mDeviceRef;
		VkSampler		mView = nullptr;
	};

	class VKGpuPipeline : public IGpuPipeline
	{
	public:
		VKGpuPipeline();
		~VKGpuPipeline();
		bool Init(VKGpuDevice* device, const FGpuPipelineDesc& desc);
	public:
		VkPipelineRasterizationStateCreateInfo		mRasterState{};
		VkPipelineColorBlendStateCreateInfo			mBlendState{};
		VkPipelineDepthStencilStateCreateInfo		mDepthStencilState{};

		VkPipelineColorBlendAttachmentState			mColorBlendAttachment[8];
	};

	class VKGpuDrawState : public IGpuDrawState
	{
	public:
		virtual bool BuildState(IGpuDevice* device) override;
	public:
		VkPipeline						mGraphicsPipeline = nullptr;
	};
}

NS_END