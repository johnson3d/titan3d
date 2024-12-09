#include "NxGpuState.h"
#include "NxEffect.h"
#include "NxFrameBuffers.h"
#include "../Base/cityhash/city.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	const IGpuDrawState* FGpuPipelineManager::GetOrCreate(IGpuDevice* device, IRenderPass* rpass, IGraphicsEffect* effect, IGpuPipeline* pipeline, EPrimitiveType topology)
	{
		void* tmp[4] = { rpass, effect, pipeline , (void*)topology };
		UINT64 hash = CityHash64((const char*)tmp, sizeof(tmp));
		
		auto iter = GpuPipelineCache.find(hash);
		if (iter == GpuPipelineCache.end())
		{
			auto identifier = rpass->Identifier + effect->Identifier + pipeline->Identifier + "_";
			identifier += VStringA_FormatV("%d", topology);
			uint128 seed;
			seed.first = 0;
			seed.second = 0;
			auto cachedHash = CityHash128WithSeed(identifier.c_str(), identifier.length(), seed);
			//try load cachedHash

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
			else
			{
				MemStreamWriter ar;
				if (CoreSDK::mSaveMemStream != nullptr)
				{
					CoreSDK::mSaveMemStream(&ar, identifier.c_str(), "IGpuDrawState");
				}
			}
			tmp->KeyHash.first = cachedHash.first;
			tmp->KeyHash.second = cachedHash.second;
			GpuPipelineCache.insert(std::make_pair(hash, tmp));
			return tmp;
		}
		return iter->second;
	}
}

NS_END