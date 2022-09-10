#pragma once
#include "../NxGpuState.h"
#include "NullPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class NullGpuDevice;
	class NullSampler : public ISampler
	{
	public:
		NullSampler();
		~NullSampler();
		bool Init(NullGpuDevice* device, const FSamplerDesc& desc);
	public:
	};

	class NullGpuPipeline : public IGpuPipeline
	{
	public:
		NullGpuPipeline();
		~NullGpuPipeline();
		bool Init(NullGpuDevice* device, const FGpuPipelineDesc& desc);
	public:
	};
}

NS_END