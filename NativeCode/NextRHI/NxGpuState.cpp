#include "NxGpuState.h"
#include "NxEffect.h"
#include "NxFrameBuffers.h"
#include "../Base/cityhash/city.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	const IGpuDrawState* FGpuPipelineManager::GetOrCreate(IGpuDevice* device, IRenderPass* rpass, IShaderEffect* effect, IGpuPipeline* pipeline, EPrimitiveType topology)
	{
		void* tmp[4] = { rpass, effect, pipeline , (void*)topology };
		UINT64 hash = CityHash64((const char*)tmp, sizeof(tmp));
		auto iter = GpuPipelineCache.find(hash);
		if (iter == GpuPipelineCache.end())
		{
			auto tmp = MakeWeakRef(device->CreateGpuDrawState());
			tmp->Pipeline = pipeline;
			tmp->ShaderEffect = effect;
			tmp->TopologyType = topology;
			tmp->RenderPass = rpass;
			if (tmp->BuildState(device) == false)
			{
				ASSERT(false);
				return nullptr;
			}
			GpuPipelineCache.insert(std::make_pair(hash, tmp));
			return tmp;
		}
		return iter->second;
	}
}

NS_END