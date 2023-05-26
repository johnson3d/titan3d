#pragma once
#include "../NxEffect.h"
#include "VKPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class VKGraphicsEffect : public IGraphicsEffect
	{
	public:
		VKGraphicsEffect();
		~VKGraphicsEffect();
		virtual void BuildState(IGpuDevice* device) override;
		virtual void Commit(ICommandList* cmdlist, IGraphicDraw* drawcall) override;
	public:
		TWeakRefHandle<VKGpuDevice>		mDeviceRef;
		
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

		VkPipelineLayout				mPipelineLayout = nullptr;
		VkPipeline						mComputePipeline = nullptr;
	};
}

NS_END
