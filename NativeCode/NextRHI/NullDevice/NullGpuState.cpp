#include "NullGpuState.h"
#include "NullGpuDevice.h"
#include "../NxEffect.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	NullSampler::NullSampler()
	{
		
	}
	NullSampler::~NullSampler()
	{
		
	}
	bool NullSampler::Init(NullGpuDevice* device, const FSamplerDesc& desc)
	{
		Desc = desc;

		return true;
	}

	NullGpuPipeline::NullGpuPipeline()
	{

	}
	NullGpuPipeline::~NullGpuPipeline()
	{
		
	}
	bool NullGpuPipeline::Init(NullGpuDevice* device, const FGpuPipelineDesc& desc)
	{
		Desc = desc;

		return true;
	}
}

NS_END