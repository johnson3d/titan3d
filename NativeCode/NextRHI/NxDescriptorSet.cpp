#include "NxDescriptorSet.h"
#include "NxGpuDevice.h"
#include "NxShader.h"

NS_BEGIN

namespace NxRHI
{
	void NxDesriptorPoolManager::Initialize(IGpuDevice* device)
	{
		FFenceDesc desc{};
		desc.InitValue = 0;
		mFrameFence = MakeWeakRef(device->CreateFence(&desc, "VKDescriptorSetFrame Fence"));
	}
	void NxDesriptorPoolManager::AllocFramePool()
	{
		TickRecycle();
		mCurrentFramePool = Pop();
		mCurrentFramePool->Reset();
	}
	void NxDesriptorPoolManager::FreePool(ICmdQueue* queue, IDescriptorPool* pool)
	{
		queue->IncreaseSignal(mFrameFence, EQueueType::QU_Default);
		pool->mFrameFenceValue = mFrameFence->GetExpectValue();
		mUsingFramePools.push_back(pool);
	}

	AutoRef<IDescriptorPool> NxDesriptorPoolManager::Pop()
	{
		if (mFreeFramePools.size() > 0)
		{
			auto ret = mFreeFramePools.top();
			mFreeFramePools.pop();
			return ret;
		}
		auto ptr = this->CreateDescriptorPool();
		//ptr->Initialize(mDeviceRef, this);
		auto ret = MakeWeakRef(ptr);
		return ret;
	}
	void NxDesriptorPoolManager::TickRecycle()
	{
		for (int i = 0; i < mUsingFramePools.size(); i++)
		{
			auto& cur = mUsingFramePools[i];
			if (cur->mFrameFenceValue <= mFrameFence->GetCompletedValue())
			{
				mFreeFramePools.push(cur);
				mUsingFramePools.erase(mUsingFramePools.begin() + i);
				i--;
			}
		}
	}
}

NS_END